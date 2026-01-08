using Godot;
using System.Collections.Generic;

/// <summary>
/// Main ragdoll controller that replaces StickClone when spawned from explosions.
/// Orchestrates the creation and management of individual limbs, joints, and physics simulation.
/// Provides comprehensive control over ragdoll behavior and lifecycle management.
/// </summary>
public partial class RagdollStickClone : Node2D
{
    [Signal] public delegate void RagdollCreatedEventHandler();
    [Signal] public delegate void RagdollDestroyedEventHandler();
    [Signal] public delegate void RagdollDespawnedEventHandler();

    [ExportGroup("Ragdoll Physics Settings")]
    [Export] private float _jointStiffness = 0.5f;
    [Export] private float _limbMass = 1.0f;
    [Export] private float _linearDamping = 3.0f;
    [Export] private float _angularDamping = 5.0f;
    [Export] private float _explosionForceMultiplier = 2.0f;
    [Export] private float _lifetime = 8.0f;

    [ExportGroup("Ragdoll Visuals")]
    [Export] private NodePath _limbContainerPath;
    [Export] private NodePath _connectorNodePath;

    [ExportGroup("Ragdoll Prefab References")]
    [Export] private PackedScene _headScene;
    [Export] private PackedScene _torsoScene;
    [Export] private PackedScene _armScene;
    [Export] private PackedScene _legScene;

    private Node2D? _limbContainer;
    private RagdollLimbConnector? _connector;
    private RagdollLimb? _head;
    private RagdollLimb? _torso;
    private RagdollLimb? _armLeft;
    private RagdollLimb? _armRight;
    private RagdollLimb? _legLeft;
    private RagdollLimb? _legRight;

    private Dictionary<RagdollLimb.LimbType, RagdollLimb> _limbs = new Dictionary<RagdollLimb.LimbType, RagdollLimb>();
    private Timer? _lifetimeTimer;
    private bool _isInitialized = false;
    private string _faceCustomization = "";

    // Physics tuning presets for different game feel
    public static class PhysicsPresets
    {
        /// <summary>Realistic physics with moderate damping</summary>
        public const float RealisticJointStiffness = 0.3f;
        public const float RealisticLimbMass = 1.2f;
        public const float RealisticLinearDamping = 4.0f;
        public const float RealisticAngularDamping = 6.0f;

        /// <summary>Cartoon-style bouncy physics</summary>
        public const float CartoonJointStiffness = 0.7f;
        public const float CartoonLimbMass = 0.8f;
        public const float CartoonLinearDamping = 2.0f;
        public const float CartoonAngularDamping = 3.0f;

        /// <summary>Extra bouncy for comedic effect</summary>
        public const float BouncyJointStiffness = 0.9f;
        public const float BouncyLimbMass = 0.5f;
        public const float BouncyLinearDamping = 1.0f;
        public const float BouncyAngularDamping = 2.0f;
    }

    public override void _Ready()
    {
        InitializeRagdoll();
    }

    /// <summary>
    /// Initializes the ragdoll system with all limbs and joints
    /// This is called automatically when the scene is ready
    /// </summary>
    private void InitializeRagdoll()
    {
        if (_isInitialized) return;

        // Get container references
        _limbContainer = GetNodeOrNull<Node2D>(_limbContainerPath);
        if (_limbContainer == null)
        {
            _limbContainer = new Node2D { Name = "LimbContainer" };
            AddChild(_limbContainer);
        }

        _connector = GetNodeOrNull<RagdollLimbConnector>(_connectorNodePath);
        if (_connector == null)
        {
            _connector = new RagdollLimbConnector { Name = "LimbConnector" };
            AddChild(_connector);
        }

        CreateLimbs();
        ConnectLimbs();
        SetupLifetime();
        ConnectSignals();

        _isInitialized = true;
        GD.Print("Ragdoll system initialized successfully");
        EmitSignal(SignalName.RagdollCreated);
    }

    /// <summary>
    /// Creates all individual ragdoll limbs from scenes or dynamically
    /// Each limb is configured with proper physics properties and collision detection
    /// </summary>
    private void CreateLimbs()
    {
        // Create torso (central anchor point)
        _torso = CreateLimb(RagdollLimb.LimbType.Torso, _torsoScene);
        if (_torso == null)
        {
            GD.PushError("Failed to create torso limb");
            return;
        }

        _limbs[RagdollLimb.LimbType.Torso] = _torso;

        // Create head
        _head = CreateLimb(RagdollLimb.LimbType.Head, _headScene);
        if (_head != null)
        {
            _limbs[RagdollLimb.LimbType.Head] = _head;
            // Position head above torso
            _head.GlobalPosition = _torso.GlobalPosition + new Vector2(0, -40);
        }

        // Create arms
        _armLeft = CreateLimb(RagdollLimb.LimbType.ArmLeft, _armScene);
        if (_armLeft != null)
        {
            _limbs[RagdollLimb.LimbType.ArmLeft] = _armLeft;
            _armLeft.GlobalPosition = _torso.GlobalPosition + new Vector2(-30, -10);
        }

        _armRight = CreateLimb(RagdollLimb.LimbType.ArmRight, _armScene);
        if (_armRight != null)
        {
            _limbs[RagdollLimb.LimbType.ArmRight] = _armRight;
            _armRight.GlobalPosition = _torso.GlobalPosition + new Vector2(30, -10);
        }

        // Create legs
        _legLeft = CreateLimb(RagdollLimb.LimbType.LegLeft, _legScene);
        if (_legLeft != null)
        {
            _limbs[RagdollLimb.LimbType.LegLeft] = _legLeft;
            _legLeft.GlobalPosition = _torso.GlobalPosition + new Vector2(-15, 40);
        }

        _legRight = CreateLimb(RagdollLimb.LimbType.LegRight, _legScene);
        if (_legRight != null)
        {
            _limbs[RagdollLimb.LimbType.LegRight] = _legRight;
            _legRight.GlobalPosition = _torso.GlobalPosition + new Vector2(15, 40);
        }

        // Apply physics properties to all limbs
        ApplyPhysicsProperties();

        GD.Print($"Created { _limbs.Count} ragdoll limbs");
    }

    /// <summary>
    /// Creates a single ragdoll limb from a packed scene or dynamically
    /// </summary>
    /// <param name="limbType">Type of limb to create</param>
    /// <param name="prefab">Optional prefab scene to instantiate</param>
    /// <returns>The created RagdollLimb, or null if creation failed</returns>
    private RagdollLimb? CreateLimb(RagdollLimb.LimbType limbType, PackedScene? prefab)
    {
        RagdollLimb? limb = null;

        if (prefab != null)
        {
            limb = prefab.Instantiate<RagdollLimb>();
        }
        else
        {
            // Create limb dynamically if no prefab provided
            limb = new RagdollLimb();
        }

        if (limb != null)
        {
            limb.Name = limbType.ToString();
            limb.SetLimbType(limbType);
            _limbContainer?.AddChild(limb);
            
            GD.Print($"Created limb: {limbType}");
        }

        return limb;
    }

    /// <summary>
    /// Connects all limbs with PinJoint2D constraints to form a complete ragdoll
    /// Creates a realistic skeleton structure with proper joint placement
    /// </summary>
    private void ConnectLimbs()
    {
        if (_connector == null) return;

        // Connect head to torso
        if (_head != null && _torso != null)
        {
            _connector.ConnectLimbs(_head, _torso, _jointStiffness * 0.8f);
        }

        // Connect arms to torso
        if (_armLeft != null && _torso != null)
        {
            _connector.ConnectLimbs(_armLeft, _torso, _jointStiffness);
        }
        if (_armRight != null && _torso != null)
        {
            _connector.ConnectLimbs(_armRight, _torso, _jointStiffness);
        }

        // Connect legs to torso
        if (_legLeft != null && _torso != null)
        {
            _connector.ConnectLimbs(_legLeft, _torso, _jointStiffness * 1.2f); // Legs are slightly more rigid
        }
        if (_legRight != null && _torso != null)
        {
            _connector.ConnectLimbs(_legRight, _torso, _jointStiffness * 1.2f);
        }

        // Connect opposite limbs for stability (arms and legs to each other)
        if (_armLeft != null && _armRight != null)
        {
            _connector.ConnectLimbs(_armLeft, _armRight, _jointStiffness * 0.3f);
        }
        if (_legLeft != null && _legRight != null)
        {
            _connector.ConnectLimbs(_legLeft, _legRight, _jointStiffness * 0.4f);
        }

        GD.Print("All limbs connected with joints");
    }

    /// <summary>
    /// Applies consistent physics properties to all limbs based on current settings
    /// Called when physics parameters are changed during gameplay
    /// </summary>
    private void ApplyPhysicsProperties()
    {
        foreach (var limb in _limbs.Values)
        {
            if (limb != null)
            {
                limb.SetPhysicsProperties(_limbMass, _linearDamping, _angularDamping);
            }
        }
    }

    /// <summary>
    /// Sets up the lifetime timer that will automatically despawn the ragdoll
    /// </summary>
    private void SetupLifetime()
    {
        _lifetimeTimer = new Timer();
        _lifetimeTimer.WaitTime = _lifetime;
        _lifetimeTimer.OneShot = true;
        _lifetimeTimer.Timeout += OnLifetimeExpired;
        AddChild(_lifetimeTimer);
        _lifetimeTimer.Start();

        GD.Print($"Ragdoll lifetime set to {_lifetime} seconds");
    }

    /// <summary>
    /// Connects signals from all limbs for impact feedback and lifecycle management
    /// </summary>
    private void ConnectSignals()
    {
        foreach (var limb in _limbs.Values)
        {
            if (limb != null)
            {
                limb.LimbImpacted += OnLimbImpacted;
            }
        }
    }

    /// <summary>
    /// Main initialization method called when spawning ragdoll from explosion
    /// Transfers face customization from the original StickClone
    /// </summary>
    /// <param name="spawnPosition">World position where ragdoll should appear</param>
    /// <param name="faceCustomization">Face customization data from StickClone</param>
    public void Initialize(Vector2 spawnPosition, string faceCustomization = "")
    {
        GlobalPosition = spawnPosition;
        _faceCustomization = faceCustomization;

        // Apply face customization to head limb
        if (_head != null && !string.IsNullOrEmpty(faceCustomization))
        {
            ApplyFaceCustomization(faceCustomization);
        }

        // Activate the ragdoll physics
        ActivateRagdoll();

        GD.Print($"Ragdoll initialized at {spawnPosition} with face: {faceCustomization}");
    }

    /// <summary>
    /// Transfers face customization from StickClone to ragdoll head
    /// Handles hats, glasses, emotions, and custom faces
    /// </summary>
    /// <param name="customization">Face customization string from StickClone</param>
    private void ApplyFaceCustomization(string customization)
    {
        if (_head == null) return;

        // Parse customization string (format: hat,glasses,emotion)
        var parts = customization.Split(',');
        if (parts.Length >= 3)
        {
            string hat = parts[0];
            string glasses = parts[1];
            string emotion = parts[2];

            GD.Print($"Applying face customization: Hat={hat}, Glasses={glasses}, Emotion={emotion}");

            // TODO: Apply actual sprite changes to head sprite
            // This would involve loading the appropriate sprites and applying them to _head
        }
    }

    /// <summary>
    /// Applies explosion force to all ragdoll limbs realistically
    /// Closer limbs receive more force, creating natural flailing motion
    /// </summary>
    /// <param name="epicenter">World position of explosion center</param>
    /// <param name="force">Base force of explosion</param>
    /// <param name="radius">Radius of explosion effect</param>
    public void ApplyExplosionForce(Vector2 epicenter, float force, float radius)
    {
        foreach (var limb in _limbs.Values)
        {
            if (limb != null)
            {
                // Calculate distance from explosion
                float distance = limb.GlobalPosition.DistanceTo(epicenter);
                
                // Apply force falloff based on distance
                float falloff = Mathf.Clamp(1.0f - (distance / radius), 0.0f, 1.0f);
                float adjustedForce = force * falloff * _explosionForceMultiplier;

                // Calculate direction from explosion to limb
                Vector2 direction = (limb.GlobalPosition - epicenter).Normalized();
                
                // Add some randomness for more realistic flailing
                direction += new Vector2(
                    (float)GD.RandRange(-0.3, 0.3),
                    (float)GD.RandRange(-0.3, 0.3)
                );
                direction = direction.Normalized();

                // Apply impulse in world space
                limb.ApplyLocalImpulse(direction * adjustedForce);

                GD.Print($"Applied explosion force {adjustedForce} to limb {limb.GetLimbType()} at distance {distance}");
            }
        }

        // Spawn explosion effects
        if (EffectsManager.Instance != null)
        {
            EffectsManager.Instance.SpawnExplosion(epicenter);
            EffectsManager.Instance.ShakeScreenIntense();
        }

        // Play explosion sound
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayImpactVocalSfx();
        }
    }

    /// <summary>
    /// Adjusts physics damping to control how quickly the ragdoll settles
    /// Higher values = faster settling, lower values = more bouncy/slow settling
    /// </summary>
    /// <param name="damping">Damping level (1.0 = normal, 0.5 = bouncy, 2.0 = heavy)</param>
    public void SetDampingLevel(float damping)
    {
        damping = Mathf.Clamp(damping, 0.1f, 10.0f);
        
        _linearDamping = damping * 3.0f;
        _angularDamping = damping * 5.0f;

        ApplyPhysicsProperties();

        GD.Print($"Ragdoll damping set to {damping} (Linear: {_linearDamping}, Angular: {_angularDamping})");
    }

    /// <summary>
    /// Activates the ragdoll physics system and starts simulation
    /// Called after initialization to begin ragdoll behavior
    /// </summary>
    public void ActivateRagdoll()
    {
        if (!_isInitialized)
        {
            InitializeRagdoll();
        }

        // Ensure all limbs are active
        foreach (var limb in _limbs.Values)
        {
            if (limb != null)
            {
                limb.Freeze = false;
                limb.Monitoring = true;
                limb.Monitorable = true;
            }
        }

        GD.Print("Ragdoll physics activated");
    }

    /// <summary>
    /// Applies a physics preset for quick tuning of ragdoll behavior
    /// </summary>
    /// <param name="preset">Preset type to apply</param>
    public void ApplyPhysicsPreset(string preset)
    {
        switch (preset.ToLower())
        {
            case "realistic":
                _jointStiffness = PhysicsPresets.RealisticJointStiffness;
                _limbMass = PhysicsPresets.RealisticLimbMass;
                _linearDamping = PhysicsPresets.RealisticLinearDamping;
                _angularDamping = PhysicsPresets.RealisticAngularDamping;
                break;

            case "cartoon":
                _jointStiffness = PhysicsPresets.CartoonJointStiffness;
                _limbMass = PhysicsPresets.CartoonLimbMass;
                _linearDamping = PhysicsPresets.CartoonLinearDamping;
                _angularDamping = PhysicsPresets.CartoonAngularDamping;
                break;

            case "bouncy":
                _jointStiffness = PhysicsPresets.BouncyJointStiffness;
                _limbMass = PhysicsPresets.BouncyLimbMass;
                _linearDamping = PhysicsPresets.BouncyLinearDamping;
                _angularDamping = PhysicsPresets.BouncyAngularDamping;
                break;

            default:
                GD.PushWarning($"Unknown physics preset: {preset}");
                return;
        }

        ApplyPhysicsProperties();
        ConnectLimbs(); // Reconnect with new stiffness

        GD.Print($"Applied physics preset: {preset}");
    }

    /// <summary>
    /// Gets a specific limb by type
    /// </summary>
    /// <param name="limbType">Type of limb to retrieve</param>
    /// <returns>The RagdollLimb of specified type, or null if not found</returns>
    public RagdollLimb? GetLimb(RagdollLimb.LimbType limbType)
    {
        return _limbs.ContainsKey(limbType) ? _limbs[limbType] : null;
    }

    /// <summary>
    /// Gets all limbs in the ragdoll
    /// </summary>
    /// <returns>Array of all RagdollLimb instances</returns>
    public RagdollLimb[] GetAllLimbs()
    {
        var limbList = new List<RagdollLimb>();
        foreach (var limb in _limbs.Values)
        {
            if (limb != null)
            {
                limbList.Add(limb);
            }
        }
        return limbList.ToArray();
    }

    /// <summary>
    /// Gets the total number of limbs in this ragdoll
    /// </summary>
    /// <returns>Number of active limbs</returns>
    public int GetLimbCount()
    {
        return _limbs.Count;
    }

    /// <summary>
    /// Checks if the ragdoll has settled and stopped moving significantly
    /// Used for optimization and cleanup decisions
    /// </summary>
    /// <returns>True if ragdoll has settled</returns>
    public bool HasSettled()
    {
        float totalVelocity = 0.0f;
        int activeLimbs = 0;

        foreach (var limb in _limbs.Values)
        {
            if (limb != null && !limb.Sleeping)
            {
                totalVelocity += limb.GetVelocity().Length();
                activeLimbs++;
            }
        }

        // Consider settled if average velocity is very low
        return activeLimbs > 0 && (totalVelocity / activeLimbs) < 5.0f;
    }

    /// <summary>
    /// Called when any limb experiences an impact
    /// </summary>
    /// <param name="impactForce">Force of the impact</param>
    private void OnLimbImpacted(float impactForce)
    {
        GD.Print($"Ragdoll limb impacted with force: {impactForce}");

        // Additional effects can be triggered here
        // For example: screen shake, particle effects, sound variations, etc.
    }

    /// <summary>
    /// Called when the ragdoll's lifetime expires
    /// Automatically despawns the ragdoll to prevent memory issues
    /// </summary>
    private void OnLifetimeExpired()
    {
        GD.Print("Ragdoll lifetime expired, despawning...");
        DespawnRagdoll();
    }

    /// <summary>
    /// Despawns the ragdoll with visual and audio feedback
    /// Called automatically when lifetime expires or manually for cleanup
    /// </summary>
    public void DespawnRagdoll()
    {
        EmitSignal(SignalName.RagdollDespawned);

        // Spawn some particles for a satisfying "fade out" effect
        if (EffectsManager.Instance != null)
        {
            EffectsManager.Instance.SpawnDust(GlobalPosition);
        }

        // Queue free with slight delay for effect
        CallDeferred(MethodName.QueueFree);
    }

    /// <summary>
    /// Force destroys the ragdoll immediately (for emergency cleanup)
    /// </summary>
    public void ForceDestroy()
    {
        GD.Print("Force destroying ragdoll");
        EmitSignal(SignalName.RagdollDestroyed);
        QueueFree();
    }

    /// <summary>
    /// Resets the ragdoll to initial state for object pooling
    /// </summary>
    public void ResetForPool()
    {
        // Reset all limbs to initial positions and states
        foreach (var limb in _limbs.Values)
        {
            if (limb != null)
            {
                limb.ResetImpactState();
                limb.LinearVelocity = Vector2.Zero;
                limb.AngularVelocity = 0f;
                limb.Rotation = 0f;
            }
        }

        // Reset timing
        if (_lifetimeTimer != null && _lifetimeTimer.IsStopped())
        {
            _lifetimeTimer.Start();
        }

        // Reset physics settings to defaults
        ApplyPhysicsProperties();

        GD.Print("Ragdoll reset for pooling");
    }

    public override void _ExitTree()
    {
        // Cleanup
        if (_lifetimeTimer != null)
        {
            _lifetimeTimer.Timeout -= OnLifetimeExpired;
        }

        // Disconnect limb signals
        foreach (var limb in _limbs.Values)
        {
            if (limb != null)
            {
                limb.LimbImpacted -= OnLimbImpacted;
            }
        }

        // Disconnect all joints
        _connector?.DisconnectAllJoints();

        GD.Print("Ragdoll cleanup completed");
    }
}