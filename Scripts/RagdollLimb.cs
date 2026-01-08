using Godot;

/// <summary>
/// Represents a single limb (arm, leg, torso, head) in the ragdoll system as a physics body.
/// Each limb has its own RigidBody2D physics and can be independently manipulated by explosions.
/// </summary>
public partial class RagdollLimb : RigidBody2D
{
    /// <summary>
    /// Enumeration of different limb types that can be part of a ragdoll
    /// </summary>
    public enum LimbType
    {
        Head,
        Torso,
        ArmLeft,
        ArmRight,
        LegLeft,
        LegRight
    }

    [Signal] public delegate void LimbImpactedEventHandler(float impactForce);
    [Signal] public delegate void LimbSleepingEventHandler();

    [ExportGroup("Ragdoll Limb Settings")]
    [Export] private float _defaultMass = 1.0f;
    [Export] private float _defaultLinearDamping = 3.0f;
    [Export] private float _defaultAngularDamping = 5.0f;
    [Export] private NodePath _spritePath;
    [Export] private NodePath _collisionShapePath;
    [Export] private bool _allowSelfCollision = false;

    private Sprite2D? _sprite;
    private CollisionShape2D? _collisionShape;
    private LimbType _limbType = LimbType.Torso;
    private bool _hasImpacted = false;
    private float _impactThreshold = 50.0f;

    public override void _Ready()
    {
        InitializeLimb();
        ConnectSignals();
    }

    /// <summary>
    /// Initializes the ragdoll limb with default physics settings
    /// </summary>
    private void InitializeLimb()
    {
        // Get sprite and collision shape references
        _sprite = GetNodeOrNull<Sprite2D>(_spritePath);
        _collisionShape = GetNodeOrNull<CollisionShape2D>(_collisionShapePath);

        // Set up physics material for better collision response
        var physicsMaterial = new PhysicsMaterial();
        physicsMaterial.Friction = 0.8f;
        physicsMaterial.Bounce = 0.1f;
        PhysicsMaterialOverride = physicsMaterial;

        // Configure default physics properties
        Mass = _defaultMass;
        LinearDamp = _defaultLinearDamping;
        AngularDamp = _defaultAngularDamping;

        // Set collision layers - ragdoll limbs on layer 3, collide with environment (layer 2)
        CollisionLayer = 1 << 2; // Layer 3 (bit 2)
        CollisionMask = 1 << 1;   // Environment layer 2 (bit 1)

        // Configure auto-sleep for performance
        CanSleep = true;
        SleepingStateChanged += OnSleepingStateChanged;

        GD.Print($"Ragdoll limb initialized: {Name} at position {GlobalPosition}");
    }

    /// <summary>
    /// Connects signals for physics interaction feedback
    /// </summary>
    private void ConnectSignals()
    {
        BodyEntered += OnBodyEntered;
    }

    /// <summary>
    /// Applies an impulse force to this limb in local space (relative to limb orientation)
    /// This is the main method used by explosions to push ragdoll limbs realistically
    /// </summary>
    /// <param name="localForce">Force vector in local coordinates</param>
    public void ApplyLocalImpulse(Vector2 localForce)
    {
        // Convert local force to world coordinates
        Vector2 worldForce = localForce.Rotated(Rotation);
        ApplyCentralImpulse(worldForce);

        // Add some random rotation for more realistic flailing
        float randomSpin = (float)GD.RandRange(-50.0, 50.0);
        ApplyTorqueImpulse(randomSpin);

        GD.Print($"Applied impulse {worldForce} to limb {Name}");

        // Trigger impact effects for significant forces
        if (worldForce.Length() > _impactThreshold)
        {
            EmitSignal(SignalName.LimbImpacted, worldForce.Length());
            _hasImpacted = true;

            // Play impact sound
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayImpactVocalSfx();
            }

            // Screen shake for major impacts
            if (EffectsManager.Instance != null)
            {
                EffectsManager.Instance.ShakeScreenIntense();
            }
        }
    }

    /// <summary>
    /// Sets the visual sprite for this limb, inheriting from StickClone customization
    /// </summary>
    /// <param name="sprite">Sprite2D node to use for visual representation</param>
    public void SetSprite(Sprite2D sprite)
    {
        if (_sprite != null)
        {
            _sprite.Texture = sprite.Texture;
            _sprite.RegionEnabled = sprite.RegionEnabled;
            _sprite.RegionRect = sprite.RegionRect;
            _sprite.Modulate = sprite.Modulate;
            _sprite.FlipH = sprite.FlipH;
            _sprite.FlipV = sprite.FlipV;
            _sprite.Scale = sprite.Scale;
            _sprite.Rotation = sprite.Rotation;
            _sprite.ZIndex = sprite.ZIndex;

            GD.Print($"Applied sprite to limb {Name}: {sprite.Texture?.GetPath()}");
        }
    }

    /// <summary>
    /// Sets the physics properties for this limb to control how it behaves
    /// </summary>
    /// <param name="mass">Weight of the limb (0.5-2.0 for realistic feel)</param>
    /// <param name="linearDamping">Air resistance (1.0-10.0, higher = faster settling)</param>
    /// <param name="angularDamping">Spin resistance (1.0-10.0, higher = faster stopping)</param>
    public void SetPhysicsProperties(float mass, float linearDamping, float angularDamping)
    {
        Mass = Mathf.Clamp(mass, 0.1f, 5.0f);
        LinearDamp = Mathf.Clamp(linearDamping, 0.0f, 20.0f);
        AngularDamp = Mathf.Clamp(angularDamping, 0.0f, 20.0f);

        GD.Print($"Set physics for limb {Name}: Mass={Mass}, LinearDamp={LinearDamp}, AngularDamp={AngularDamp}");
    }

    /// <summary>
    /// Gets the limb type for this ragdoll part
    /// </summary>
    /// <returns>The limb type enumeration</returns>
    public LimbType GetLimbType()
    {
        return _limbType;
    }

    /// <summary>
    /// Sets the limb type for this ragdoll part
    /// </summary>
    /// <param name="type">The limb type to set</param>
    public void SetLimbType(LimbType type)
    {
        _limbType = type;
        Name = type.ToString();
    }

    /// <summary>
    /// Sets custom collision behavior - whether this limb can collide with other limbs
    /// </summary>
    /// <param name="allowSelfCollision">True to allow limb-to-limb collisions</param>
    public void SetSelfCollision(bool allowSelfCollision)
    {
        _allowSelfCollision = allowSelfCollision;
        
        if (!allowSelfCollision)
        {
            // Remove collision with other ragdoll limbs (layer 3)
            CollisionMask &= ~(1 << 2); // Remove environment layer 2 bit
            CollisionMask |= (1 << 1);   // Add back environment layer 2 bit
        }
    }

    /// <summary>
    /// Gets whether this limb has experienced a significant impact
    /// </summary>
    /// <returns>True if limb has been impacted</returns>
    public bool HasImpacted()
    {
        return _hasImpacted;
    }

    /// <summary>
    /// Resets the impact state for reuse (for object pooling)
    /// </summary>
    public void ResetImpactState()
    {
        _hasImpacted = false;
    }

    /// <summary>
    /// Called when this limb collides with another body
    /// </summary>
    /// <param name="body">The body this limb collided with</param>
    private void OnBodyEntered(Node body)
    {
        // Only trigger effects for significant collisions
        if (LinearVelocity.Length() > 30.0f)
        {
            float impactForce = LinearVelocity.Length();
            EmitSignal(SignalName.LimbImpacted, impactForce);

            // Play collision sound
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayImpactVocalSfx();
            }
        }
    }

    /// <summary>
    /// Called when the limb enters or exits sleeping state
    /// </summary>
    private void OnSleepingStateChanged()
    {
        if (Sleeping)
        {
            GD.Print($"Limb {Name} has settled and gone to sleep");
            EmitSignal(SignalName.LimbSleeping);
        }
    }

    /// <summary>
    /// Gets the current velocity of this limb
    /// </summary>
    /// <returns>Current linear velocity vector</returns>
    public Vector2 GetVelocity()
    {
        return LinearVelocity;
    }

    /// <summary>
    /// Sets the impact threshold for triggering impact effects
    /// </summary>
    /// <param name="threshold">Force threshold below which impacts are ignored</param>
    public void SetImpactThreshold(float threshold)
    {
        _impactThreshold = Mathf.Max(0.0f, threshold);
    }

    public override void _ExitTree()
    {
        // Clean up signal connections
        BodyEntered -= OnBodyEntered;
        SleepingStateChanged -= OnSleepingStateChanged;
    }
}