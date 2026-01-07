using Godot;

/// <summary>
/// Handles visual damage feedback effects for breakable obstacles.
/// Provides color flashes, scale bounces, and particle effects based on material properties.
/// Designed to work with the material hardness system for consistent visual feedback.
/// </summary>
public partial class DamageIndicator : Node
{
    /// <summary>
    /// Node reference to the sprite that will display damage effects.
    /// </summary>
    [Export] public Node2D TargetSprite { get; set; }

    /// <summary>
    /// Duration of the damage flash effect in seconds.
    /// </summary>
    [Export] public float FlashDuration = 0.2f;

    /// <summary>
    /// Duration of the scale bounce effect in seconds.
    /// </summary>
    [Export] public float BounceDuration = 0.15f;

    /// <summary>
    /// Maximum bounce scale (1.0 = no bounce, 1.2 = 20% larger).
    /// </summary>
    [Export] public float MaxBounceScale = 1.2f;

    // Private fields
    private Tween _damageTween;
    private Color _originalColor;
    private Vector2 _originalScale;

    public override void _Ready()
    {
        if (TargetSprite == null)
        {
            // Try to find a Sprite2D child if no target specified
            TargetSprite = GetNodeOrNull<Sprite2D>("Sprite2D");
        }

        // Store original properties if we have a target
        if (TargetSprite != null && TargetSprite is Sprite2D sprite)
        {
            _originalColor = sprite.Modulate;
            _originalScale = sprite.Scale;
        }
    }

    /// <summary>
    /// Triggers a damage flash effect on the target sprite.
    /// </summary>
    /// <param name="intensity">Damage intensity from 0.0 to 1.0.</param>
    /// <param name="materialColor">Base material color for reference.</param>
    /// <param name="hardness">Material hardness for feedback adjustment.</param>
    public void TriggerDamageFlash(float intensity, Color materialColor, int hardness)
    {
        if (TargetSprite == null) return;

        // Calculate flash intensity based on material hardness
        // Harder materials have less dramatic flash effects
        float hardnessModifier = 1.0f - ((hardness - 1) * 0.1f);
        float finalIntensity = Mathf.Clamp(intensity * hardnessModifier, 0.1f, 1.0f);

        // Create or reset the damage tween
        if (_damageTween != null && _damageTween.IsRunning())
        {
            _damageTween.Kill();
        }

        _damageTween = CreateTween();

        // Store original properties
        if (TargetSprite is Sprite2D sprite)
        {
            _originalColor = sprite.Modulate;
            _originalScale = sprite.Scale;
        }

        // Calculate flash color (whiter flash for higher intensity)
        var flashColor = materialColor.Lerp(Color.White, finalIntensity);
        
        // Apply flash duration scaling
        float actualFlashDuration = FlashDuration * (1.0f - finalIntensity * 0.3f);

        // Tween to flash color
        _damageTween.TweenProperty(TargetSprite, "modulate", flashColor, actualFlashDuration * 0.3f);
        
        // Tween back to original color
        _damageTween.TweenProperty(TargetSprite, "modulate", _originalColor, actualFlashDuration * 0.7f);
    }

    /// <summary>
    /// Triggers a scale bounce effect on the target sprite.
    /// </summary>
    /// <param name="intensity">Damage intensity from 0.0 to 1.0.</param>
    /// <param name="hardness">Material hardness for bounce adjustment.</param>
    public void TriggerBounceEffect(float intensity, int hardness)
    {
        if (TargetSprite == null) return;

        // Harder materials bounce less
        float hardnessModifier = 1.0f - ((hardness - 1) * 0.15f);
        float finalIntensity = Mathf.Clamp(intensity * hardnessModifier, 0.05f, 1.0f);

        // Calculate target scale
        float bounceAmount = (MaxBounceScale - 1.0f) * finalIntensity;
        Vector2 targetScale = _originalScale * (1.0f + bounceAmount);

        // Calculate bounce duration (less intense = faster bounce)
        float actualBounceDuration = BounceDuration * (1.0f - finalIntensity * 0.4f);

        // Create or reset the damage tween
        if (_damageTween != null && _damageTween.IsRunning())
        {
            _damageTween.Kill();
        }

        _damageTween = CreateTween();

        // Scale up quickly
        _damageTween.TweenProperty(TargetSprite, "scale", targetScale, actualBounceDuration * 0.3f);
        
        // Scale back to original
        _damageTween.TweenProperty(TargetSprite, "scale", _originalScale, actualBounceDuration * 0.7f);
    }

    /// <summary>
    /// Triggers combined damage feedback (flash + bounce) based on material properties.
    /// This is the main method to call for complete visual feedback.
    /// </summary>
    /// <param name="damagePercentage">Percentage of damage taken (0.0-1.0).</param>
    /// <param name="materialProperties">Material properties for feedback calculation.</param>
    public void TriggerDamageFeedback(float damagePercentage, MaterialProperties materialProperties)
    {
        if (TargetSprite == null) return;

        // Calculate damage intensity
        float intensity = Mathf.Clamp(damagePercentage, 0.0f, 1.0f);

        // Get material-specific feedback
        var materialColor = materialProperties.BaseColor;
        int hardness = materialProperties.Hardness;

        // Trigger both effects
        TriggerDamageFlash(intensity, materialColor, hardness);
        TriggerBounceEffect(intensity, hardness);

        // Log for debugging
        GD.Print($"Damage feedback triggered: Material={materialProperties.Material}, " +
                $"Intensity={intensity:P0}, Hardness={hardness}");
    }

    /// <summary>
    /// Creates and spawns damage particles based on material type.
    /// Placeholder implementation that can be extended with actual particle effects.
    /// </summary>
    /// <param name="materialProperties">Material properties for particle customization.</param>
    /// <param name="intensity">Damage intensity for particle count and velocity.</param>
    /// <param name="impactPosition">Position where the damage occurred.</param>
    public void SpawnDamageParticles(MaterialProperties materialProperties, float intensity, Vector2 impactPosition)
    {
        // Placeholder particle system
        // In a full implementation, this would:
        // 1. Load appropriate particle effect based on material type
        // 2. Set particle colors from material color
        // 3. Configure particle count based on intensity
        // 4. Set particle velocity based on material hardness
        // 5. Spawn the particles at the impact position

        // For now, we'll log the expected behavior
        Color particleColor = materialProperties.BaseColor;
        int particleCount = Mathf.RoundToInt(intensity * 10); // Scale 1-10 particles based on intensity
        float particleSpeed = 50.0f * (2.0f - materialProperties.Hardness * 0.3f); // Slower for harder materials

        GD.Print($"Spawning {particleCount} damage particles for {materialProperties.Material}: " +
                $"Color={particleColor}, Speed={particleSpeed}, Position={impactPosition}");

        // TODO: Implement actual particle spawning
        // This would require particle effect scenes and proper instantiation
        /*
        var particleScene = GetParticleSceneForMaterial(materialProperties.Material);
        var particles = particleScene.Instantiate<CPUParticles2D>();
        particles.GlobalPosition = impactPosition;
        
        // Configure particles
        particles.Modulate = particleColor;
        particles.Amount = particleCount;
        particles.SpeedScale = particleSpeed / 50.0f; // Normalize speed
        
        GetTree().CurrentScene.AddChild(particles);
        */
    }

    /// <summary>
    /// Gets the appropriate particle effect scene for a material type.
    /// Helper method for future particle system implementation.
    /// </summary>
    /// <param name="materialType">The material type to get particles for.</param>
    /// <returns>PackedScene containing the particle effect, or null if not found.</returns>
    private PackedScene GetParticleSceneForMaterial(MaterialType materialType)
    {
        // This would be expanded to return actual particle effect scenes
        // For now, return null to indicate placeholder behavior
        return null;
    }

    /// <summary>
    /// Updates the target sprite reference and stores original properties.
    /// Call this if the sprite reference changes during gameplay.
    /// </param>
    /// <param name="newTarget">The new sprite to apply effects to.</param>
    public void UpdateTargetSprite(Node2D newTarget)
    {
        TargetSprite = newTarget;
        
        if (TargetSprite != null && TargetSprite is Sprite2D sprite)
        {
            _originalColor = sprite.Modulate;
            _originalScale = sprite.Scale;
        }
    }
}