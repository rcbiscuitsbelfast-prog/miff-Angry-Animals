using Godot;

/// <summary>
/// Breakable obstacle that uses material properties for damage calculation.
/// Supports different material types with varying hardness and visual feedback.
/// Integrates with the existing DestructibleProp system for compatibility.
/// </summary>
public partial class BreakableObstacle : DestructibleProp
{
    /// <summary>
    /// Current material properties defining hardness and visual appearance.
    /// </summary>
    [ExportCategory("Material System")]
    [Export] public MaterialProperties Material { get; set; }

    /// <summary>
    /// Number of hits taken by this obstacle.
    /// Resets when material is changed.
    /// </summary>
    [Export] public int CurrentHitsTaken { get; private set; }

    /// <summary>
    /// Whether this obstacle should use procedural material assignment.
    /// If true, material will be assigned during level generation.
    /// </summary>
    [Export] public bool UseProceduralMaterial = true;

    /// <summary>
    /// Optional: Override the default material with a specific type.
    /// Only used if UseProceduralMaterial is false.
    /// </summary>
    [Export] public MaterialType OverrideMaterialType = MaterialType.Wood;

    // Private fields for damage feedback
    private Sprite2D _sprite;
    private Tween _damageTween;
    private Color _originalColor;
    private Vector2 _originalScale;

    public override void _Ready()
    {
        // Initialize base DestructibleProp functionality
        base._Ready();

        // Get sprite reference for visual effects
        _sprite = GetNodeOrNull<Sprite2D>("Sprite2D");
        if (_sprite == null)
        {
            // Try to create a basic sprite if none exists
            _sprite = new Sprite2D();
            AddChild(_sprite);
        }

        // Store original visual properties
        if (_sprite != null)
        {
            _originalColor = _sprite.Modulate;
            _originalScale = _sprite.Scale;
        }

        // Set up material based on configuration
        SetupMaterial();
    }

    /// <summary>
    /// Sets up the material properties for this obstacle.
    /// Called during initialization and when material is changed.
    /// </summary>
    private void SetupMaterial()
    {
        // Use override material if not using procedural
        if (!UseProceduralMaterial)
        {
            Material = MaterialProperties.GetMaterialProperties(OverrideMaterialType);
        }
        else if (Material.Hardness == 0) // Default/uninitialized
        {
            // Set default material
            Material = MaterialProperties.Wood;
        }

        // Update MaxHp based on material hardness
        MaxHp = Material.HitsToDestroy * 10; // Scale HP to material durability
        CurrentHp = MaxHp;
        CurrentHitsTaken = 0;

        // Apply material visuals
        ApplyMaterialVisuals();

        // Update visuals to reflect new material
        UpdateVisuals();
    }

    /// <summary>
    /// Applies material-based visual properties to the obstacle.
    /// Sets colors, scales, and opacity based on material type.
    /// </summary>
    private void ApplyMaterialVisuals()
    {
        if (_sprite == null) return;

        // Apply base material color with opacity modifier
        var visualColor = Material.BaseColor;
        visualColor.A = Material.VisualModifier.Y; // Use Y component for opacity
        
        // Apply the material color
        _sprite.Modulate = visualColor;

        // Apply scale modifier (X component)
        _sprite.Scale = Material.VisualModifier;

        GD.Print($"Applied material {Material.Material} to obstacle at {GlobalPosition}: Color={Material.BaseColor}, Hardness={Material.Hardness}, HitsToDestroy={Material.HitsToDestroy}");
    }

    /// <summary>
    /// Processes damage taking into account material hardness.
    /// Overrides the base Hit method to add material-specific logic.
    /// </summary>
    /// <param name="damage">Amount of damage to apply.</param>
    public override void Hit(int damage)
    {
        if (CurrentHp <= 0) return;

        // Increment hit counter
        CurrentHitsTaken++;

        // Calculate damage percentage for visual feedback
        float damagePercentage = GetDamagePercentage();

        // Emit debug information
        GD.Print($"Obstacle hit! Material: {Material.Material}, Hit #{CurrentHitsTaken}/{Material.HitsToDestroy}, " +
                $"HP: {CurrentHp}/{MaxHp}, Damage: {damagePercentage:P0}");

        // Apply damage to base HP system
        base.Hit(damage);

        // Play material-specific damage feedback
        PlayDamageFeedback(damagePercentage);
    }

    /// <summary>
    /// Plays visual and audio feedback when the obstacle takes damage.
    /// Feedback intensity varies based on material hardness and damage percentage.
    /// </summary>
    /// <param name="damagePercentage">Percentage of total damage taken (0.0-1.0).</param>
    private void PlayDamageFeedback(float damagePercentage)
    {
        if (_sprite == null) return;

        // Calculate feedback intensity (harder materials = less intense feedback)
        float hardnessModifier = 1.0f - ((Material.Hardness - 1) * 0.15f);
        float intensity = Mathf.Clamp(damagePercentage * hardnessModifier, 0.1f, 1.0f);

        // Flash effect (tint to white and back)
        var originalColor = _sprite.Modulate;
        var flashColor = Color.White;
        flashColor.A = originalColor.A;

        // Create or reset damage tween
        if (_damageTween != null && _damageTween.IsRunning())
        {
            _damageTween.Kill();
        }

        _damageTween = CreateTween();

        // Flash white, then back to original color
        _damageTween.TweenProperty(_sprite, "modulate", flashColor, 0.05f);
        _damageTween.TweenProperty(_sprite, "modulate", originalColor, 0.15f);

        // Scale bounce effect (smaller bounce for harder materials)
        float bounceAmount = 0.05f * intensity;
        Vector2 targetScale = _originalScale * (1.0f + bounceAmount);

        _damageTween.Parallel().TweenProperty(_sprite, "scale", targetScale, 0.05f);
        _damageTween.Parallel().TweenProperty(_sprite, "scale", _originalScale, 0.15f);

        // Spawn damage particles if available
        SpawnDamageParticles(intensity);
    }

    /// <summary>
    /// Spawns damage particles based on material type and damage intensity.
    /// Placeholder implementation using simple colored particles.
    /// </summary>
    /// <param name="intensity">Damage intensity for particle count and velocity.</param>
    private void SpawnDamageParticles(float intensity)
    {
        // This is a placeholder implementation
        // In a full implementation, you would instantiate particle effects here
        // For now, we'll just log that particles should spawn
        GD.Print($"Spawning damage particles for {Material.Material} material, intensity: {intensity:F2}");
    }

    /// <summary>
    /// Calculates and returns the current damage percentage (0.0-1.0).
    /// Used for visual feedback and damage indication.
    /// </summary>
    /// <returns>Damage percentage as a float from 0.0 (no damage) to 1.0 (destroyed).</returns>
    public float GetDamagePercentage()
    {
        if (MaxHp <= 0) return 1.0f;
        return 1.0f - ((float)CurrentHp / MaxHp);
    }

    /// <summary>
    /// Gets the remaining hits needed to destroy this obstacle.
    /// Useful for UI display and game balance.
    /// </summary>
    /// <returns>Number of hits remaining until destruction.</returns>
    public int GetHitsRemaining()
    {
        return Mathf.Max(0, Material.HitsToDestroy - CurrentHitsTaken);
    }

    /// <summary>
    /// Sets a new material for this obstacle.
    /// Updates visuals and resets hit counters.
    /// </summary>
    /// <param name="newMaterial">The new material properties to apply.</param>
    public void SetMaterial(MaterialProperties newMaterial)
    {
        Material = newMaterial;
        SetupMaterial();
    }

    /// <summary>
    /// Sets a new material type for this obstacle.
    /// Looks up the appropriate MaterialProperties for the given type.
    /// </summary>
    /// <param name="materialType">The new material type to apply.</param>
    public void SetMaterial(MaterialType materialType)
    {
        Material = MaterialProperties.GetMaterialProperties(materialType);
        SetupMaterial();
    }

    /// <summary>
    /// Overrides the base Die method to add material-specific destruction effects.
    /// </summary>
    protected override void Die()
    {
        GD.Print($"Obstacle destroyed! Material: {Material.Material}, Total hits taken: {CurrentHitsTaken}");

        // Emit material-specific destruction signal
        // This allows other systems to react to material destruction
        // (e.g., for achievements, sound effects, etc.)

        // Call base destruction logic
        base.Die();
    }
}