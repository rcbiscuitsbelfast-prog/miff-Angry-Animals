using Godot;
using System.Collections.Generic;

/// <summary>
/// Orchestrates the spawning of ragdolls in response to explosions.
/// Listens for explosion events from projectiles and creates ragdolls at the appropriate time and position.
/// Handles the transformation from StickClone to RagdollStickClone and manages ragdoll pooling for performance.
/// </summary>
public partial class RagdollSpawner : Node
{
    [Signal] public delegate void RagdollSpawnedEventHandler(RagdollStickClone ragdoll);
    [Signal] public delegate void ExplosionDetectedEventHandler(Vector2 epicenter, float force, float radius);

    [ExportGroup("Ragdoll Spawning")]
    [Export] private PackedScene _ragdollScene;
    [Export] private float _spawnDelay = 0.1f; // Delay between explosion and ragdoll spawn
    [Export] private int _maxSimultaneousRagdolls = 3;
    [Export] private bool _enableRagdollPooling = true;

    [ExportGroup("Explosion Detection")]
    [Export] private float _explosionRadius = 100.0f;
    [Export] private float _explosionForce = 500.0f;
    [Export] private LayerMask _stickCloneLayer = 1; // Layer 1 for StickClone

    [ExportGroup("Ragdoll Physics Defaults")]
    [Export] private string _defaultPhysicsPreset = "realistic"; // realistic, cartoon, bouncy

    private readonly List<RagdollStickClone> _activeRagdolls = new List<RagdollStickClone>();
    private readonly Queue<RagdollStickClone> _ragdollPool = new Queue<RagdollStickClone>();
    private int _totalSpawned = 0;

    // Explosion tracking for detecting StickClones in blast radius
    private readonly Dictionary<Vector2, float> _recentExplosions = new Dictionary<Vector2, float>();
    private float _explosionCleanupTime = 0.5f;

    public override void _Ready()
    {
        InitializeSpawner();
        ConnectSignals();
        
        GD.Print("RagdollSpawner initialized and listening for explosions");
    }

    public override void _Process(double delta)
    {
        CleanupExpiredExplosions(delta);
        CleanupOffscreenRagdolls();
    }

    /// <summary>
    /// Initializes the ragdoll spawner with default settings
    /// </summary>
    private void InitializeSpawner()
    {
        // Create default ragdoll scene if none provided
        if (_ragdollScene == null)
        {
            CreateDefaultRagdollScene();
        }

        // Initialize object pool if enabled
        if (_enableRagdollPooling)
        {
            PrewarmPool();
        }
    }

    /// <summary>
    /// Connects to signals from projectiles, StickClones, and other game systems
    /// </summary>
    private void ConnectSignals()
    {
        // Connect to SignalManager for explosion events
        if (SignalManager.Instance != null)
        {
            SignalManager.Instance.OnAnimalDied += OnAnimalDied;
        }

        // Connect to GameFeelManager for explosion effects
        if (GameFeelManager.Instance != null)
        {
            // Listen for any existing explosion signals
            // Note: These would need to be added to the existing systems
        }

        // Connect to any existing projectile systems
        ConnectProjectileSignals();
    }

    /// <summary>
    /// Connects signals from projectile-related systems to detect explosions
    /// This connects to the actual explosion signals added to the existing systems
    /// </summary>
    private void ConnectProjectileSignals()
    {
        // Connect to all Projectiles in the scene for explosion detection
        var projectiles = GetTree().GetNodesInGroup("projectiles");
        foreach (var projectile in projectiles)
        {
            if (projectile is Projectile proj)
            {
                proj.ExplosionOccurred += OnProjectileExplosionOccurred;
            }
        }

        // Listen for new projectiles being added to the scene
        GetTree().NodeAdded += OnNodeAddedToScene;

        GD.Print("Connected to projectile explosion detection");
    }

    /// <summary>
    /// Called when a new node is added to the scene
    /// We use this to connect explosion signals to newly spawned projectiles
    /// </summary>
    /// <param name="node">The node that was added</param>
    private void OnNodeAddedToScene(Node node)
    {
        if (node is Projectile projectile)
        {
            projectile.ExplosionOccurred += OnProjectileExplosionOccurred;
            GD.Print($"Connected to new projectile: {projectile.Name}");
        }
    }

    /// <summary>
    /// Called when a projectile explodes
    /// This is the main entry point for ragdoll spawning
    /// </summary>
    /// <param name="epicenter">Position of the explosion</param>
    /// <param name="force">Force of the explosion</param>
    /// <param name="radius">Radius of the explosion effect</param>
    private void OnProjectileExplosionOccurred(Vector2 epicenter, float force, float radius)
    {
        GD.Print($"Projectile explosion detected at {epicenter} with force {force} and radius {radius}");
        SpawnRagdollFromExplosion(epicenter);
    }

    /// <summary>
    /// Called when an animal/projectile dies (currently used as explosion proxy)
    /// This creates a timing reference for when ragdolls should spawn
    /// </summary>
    private void OnAnimalDied()
    {
        // For now, we'll use projectile death as an explosion event
        // In the future, this will be replaced with actual explosion detection
        
        Vector2 deathPosition = Vector2.Zero; // This would come from the actual projectile
        
        // Check for StickClones in the explosion area
        DetectAndSpawnNearbyRagdolls(deathPosition);
    }

    /// <summary>
    /// Main entry point for spawning ragdolls from explosion events
    /// Called by projectiles when they explode, or by other explosion systems
    /// </summary>
    /// <param name="epicenter">World position of explosion center</param>
    /// <param name="projectile">The projectile that caused the explosion (optional)</param>
    public void SpawnRagdollFromExplosion(Vector2 epicenter, Projectile? projectile = null)
    {
        EmitSignal(SignalName.ExplosionDetected, epicenter, _explosionForce, _explosionRadius);

        // Record explosion for cleanup
        _recentExplosions[epicenter] = Time.GetTicksMsec() / 1000.0f;

        GD.Print($"Explosion detected at {epicenter}, spawning ragdolls...");

        // Check if we've reached the maximum ragdoll limit
        if (_activeRagdolls.Count >= _maxSimultaneousRagdolls)
        {
            GD.Print($"Max ragdoll limit reached ({_maxSimultaneousRagdolls}), skipping explosion");
            return;
        }

        // Use timer to spawn after a short delay (lets explosion effects play first)
        var timer = new Timer();
        timer.WaitTime = _spawnDelay;
        timer.OneShot = true;
        timer.Timeout += () => SpawnRagdollNearPosition(epicenter, projectile);
        AddChild(timer);
        timer.Start();
    }

    /// <summary>
    /// Spawns a single ragdoll at the specified position
    /// Used for testing and simple explosion scenarios
    /// </summary>
    /// <param name="position">Position to spawn ragdoll</param>
    /// <param name="projectile">Source projectile for physics data</param>
    private void SpawnRagdollNearPosition(Vector2 position, Projectile? projectile = null)
    {
        // Create ragdoll at the explosion position
        var ragdoll = CreateRagdollInstance();
        if (ragdoll == null)
        {
            GD.PushError("Failed to create ragdoll instance");
            return;
        }

        // Initialize the ragdoll
        ragdoll.Initialize(position, "none,none,surprised");
        ragdoll.ApplyPhysicsPreset(_defaultPhysicsPreset);

        // Apply explosion force
        float explosionForce = projectile != null ? projectile.LinearVelocity.Length() : _explosionForce;
        float explosionRadius = projectile != null ? projectile.GetExplosionRadius() : _explosionRadius;

        ragdoll.ApplyExplosionForce(position, explosionForce, explosionRadius);

        // Connect ragdoll lifecycle signals
        ragdoll.RagdollDespawned += () => OnRagdollDespawned(ragdoll);
        ragdoll.RagdollDestroyed += () => OnRagdollDestroyed(ragdoll);

        // Add to active list
        _activeRagdolls.Add(ragdoll);
        GetTree().CurrentScene.AddChild(ragdoll);

        _totalSpawned++;
        EmitSignal(SignalName.RagdollSpawned, ragdoll);

        GD.Print($"Ragdoll spawned at {position} (Total spawned: {_totalSpawned})");
    }

    /// <summary>
    /// Creates a new ragdoll instance from the pool or instantiates a new one
    /// </summary>
    /// <returns>New or recycled RagdollStickClone instance</returns>
    private RagdollStickClone? CreateRagdollInstance()
    {
        RagdollStickClone? ragdoll = null;

        if (_enableRagdollPooling && _ragdollPool.Count > 0)
        {
            // Reuse pooled instance
            ragdoll = _ragdollPool.Dequeue();
            ragdoll.ResetForPool(); // This method would need to be added to RagdollStickClone
            GD.Print("Reused ragdoll from pool");
        }
        else
        {
            // Create new instance
            if (_ragdollScene != null)
            {
                ragdoll = _ragdollScene.Instantiate<RagdollStickClone>();
            }
            else
            {
                // Fallback to direct instantiation
                ragdoll = new RagdollStickClone();
            }
        }

        return ragdoll;
    }

    /// <summary>
    /// Returns a ragdoll to the pool for reuse (for object pooling optimization)
    /// </summary>
    /// <param name="ragdoll">Ragdoll to pool</param>
    private void PoolRagdoll(RagdollStickClone ragdoll)
    {
        if (!_enableRagdollPooling || ragdoll == null) return;

        // Reset ragdoll to initial state
        ragdoll.ResetForPool();
        
        // Add to pool (with size limit)
        if (_ragdollPool.Count < 5) // Max pool size
        {
            _ragdollPool.Enqueue(ragdoll);
            ragdoll.Visible = false;
            ragdoll.SetProcess(false);
            GD.Print("Ragdoll returned to pool");
        }
        else
        {
            // Pool full, actually destroy
            ragdoll.QueueFree();
            GD.Print("Pool full, destroyed ragdoll");
        }
    }

    /// <summary>
    /// Transfers face customization data from StickClone to ragdoll system
    /// Handles hats, glasses, emotions, and custom faces
    /// </summary>
    /// <param name="source">Source StickClone to copy customization from</param>
    /// <returns>String representation of face customization</returns>
    private string TransferFaceCustomization(StickClone source)
    {
        if (source == null) return "";

        var customization = source.GetFaceCustomization();
        string customizationString = $"{customization["hat"]},{customization["glasses"]},{customization["emotion"]}";

        GD.Print($"Transferred face customization: {customizationString}");

        return customizationString;
    }

    /// <summary>
    /// Pre-warms the ragdoll pool for better performance
    /// Creates a few ragdoll instances in advance
    /// </summary>
    private void PrewarmPool()
    {
        for (int i = 0; i < 2; i++) // Create 2 pre-warmed instances
        {
            var ragdoll = CreateRagdollInstance();
            if (ragdoll != null)
            {
                PoolRagdoll(ragdoll);
            }
        }
        GD.Print("Ragdoll pool pre-warmed");
    }

    /// <summary>
    /// Detects StickClones in the explosion area and spawns appropriate ragdolls
    /// Currently a placeholder that will be enhanced with actual explosion detection
    /// </summary>
    /// <param name="explosionPosition">Position of explosion</param>
    private void DetectAndSpawnNearbyRagdolls(Vector2 explosionPosition)
    {
        // This is a simplified implementation
        // In the full version, this would:
        // 1. Query physics space for StickClones in explosion radius
        // 2. Calculate explosion force based on distance
        // 3. Apply appropriate damage/physics effects
        // 4. Spawn ragdolls with correct force and direction

        GD.Print($"Detecting ragdolls near explosion at {explosionPosition}");
    }

    /// <summary>
    /// Removes old explosion records to prevent memory leaks
    /// </summary>
    /// <param name="delta">Time since last frame</param>
    private void CleanupExpiredExplosions(double delta)
    {
        var currentTime = Time.GetTicksMsec() / 1000.0f;
        var toRemove = new List<Vector2>();

        foreach (var kvp in _recentExplosions)
        {
            if (currentTime - kvp.Value > _explosionCleanupTime)
            {
                toRemove.Add(kvp.Key);
            }
        }

        foreach (var position in toRemove)
        {
            _recentExplosions.Remove(position);
        }
    }

    /// <summary>
    /// Cleans up ragdolls that have moved off-screen to prevent memory leaks
    /// </summary>
    private void CleanupOffscreenRagdolls()
    {
        var camera = GetViewport().GetCamera2D();
        if (camera == null) return;

        var viewportSize = GetViewport().GetVisibleRect().Size;
        var cameraPosition = camera.GlobalPosition;
        var halfScreen = viewportSize / 2;

        var offscreenRagdolls = new List<RagdollStickClone>();

        foreach (var ragdoll in _activeRagdolls)
        {
            if (ragdoll == null) continue;

            var position = ragdoll.GlobalPosition;
            var bounds = new Rect2(cameraPosition - halfScreen - Vector2.One * 100, viewportSize + Vector2.One * 200);

            if (!bounds.HasPoint(position))
            {
                offscreenRagdolls.Add(ragdoll);
            }
        }

        foreach (var ragdoll in offscreenRagdolls)
        {
            OnRagdollDespawned(ragdoll);
        }
    }

    /// <summary>
    /// Called when a ragdoll naturally despawns (lifetime expired)
    /// </summary>
    /// <param name="ragdoll">The ragdoll that despawned</param>
    private void OnRagdollDespawned(RagdollStickClone ragdoll)
    {
        if (_activeRagdolls.Contains(ragdoll))
        {
            _activeRagdolls.Remove(ragdoll);
            
            if (_enableRagdollPooling)
            {
                PoolRagdoll(ragdoll);
            }
            else
            {
                ragdoll.QueueFree();
            }

            GD.Print($"Ragdoll despawned, active count: {_activeRagdolls.Count}");
        }
    }

    /// <summary>
    /// Called when a ragdoll is force destroyed
    /// </summary>
    /// <param name="ragdoll">The ragdoll that was destroyed</param>
    private void OnRagdollDestroyed(RagdollStickClone ragdoll)
    {
        if (_activeRagdolls.Contains(ragdoll))
        {
            _activeRagdolls.Remove(ragdoll);
            GD.Print($"Ragdoll destroyed, active count: {_activeRagdolls.Count}");
        }
    }

    /// <summary>
    /// Gets statistics about ragdoll spawning and pooling
    /// </summary>
    /// <returns>Dictionary with ragdoll statistics</returns>
    public Dictionary<string, int> GetRagdollStats()
    {
        return new Dictionary<string, int>
        {
            { "ActiveRagdolls", _activeRagdolls.Count },
            { "PooledRagdolls", _ragdollPool.Count },
            { "TotalSpawned", _totalSpawned }
        };
    }

    /// <summary>
    /// Force destroys all active ragdolls (for level cleanup)
    /// </summary>
    public void ClearAllRagdolls()
    {
        foreach (var ragdoll in _activeRagdolls)
        {
            if (ragdoll != null)
            {
                ragdoll.ForceDestroy();
            }
        }
        _activeRagdolls.Clear();
        
        // Clear pool too
        while (_ragdollPool.Count > 0)
        {
            var ragdoll = _ragdollPool.Dequeue();
            if (ragdoll != null)
            {
                ragdoll.QueueFree();
            }
        }

        GD.Print("All ragdolls cleared");
    }

    /// <summary>
    /// Sets the maximum number of simultaneous ragdolls allowed
    /// </summary>
    /// <param name="maxRagdolls">Maximum simultaneous ragdolls (1-10 recommended)</param>
    public void SetMaxRagdolls(int maxRagdolls)
    {
        _maxSimultaneousRagdolls = Mathf.Clamp(maxRagdolls, 1, 10);
        GD.Print($"Max simultaneous ragdolls set to {_maxSimultaneousRagdolls}");
    }

    /// <summary>
    /// Creates a default ragdoll scene if none is provided in the inspector
    /// </summary>
    private void CreateDefaultRagdollScene()
    {
        // This would create a minimal ragdoll scene programmatically
        // For now, we'll just log that this needs to be set up
        GD.PushWarning("No ragdoll scene provided, please assign RagdollStickClone.tscn in the inspector");
    }
}