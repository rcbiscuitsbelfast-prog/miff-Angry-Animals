using Godot;

/// <summary>
/// Base class for all room/level scenes.
/// Handles the flow between slingshot phase and traversal phase,
/// manages room completion and exit door unlocking.
/// </summary>
public partial class RoomBase : Node2D
{
    [Signal] public delegate void SlingshotPhaseStartedEventHandler();
    [Signal] public delegate void TraversalPhaseStartedEventHandler();
    [Signal] public delegate void RoomTargetReachedEventHandler();
    [Signal] public delegate void ExitDoorUnlockedEventHandler();

    [Export] private NodePath _slingshotPath;
    [Export] private NodePath _exitDoorPath;
    [Export] private NodePath _projectilesLoaderPath;
    [Export] private int _targetScore = 3;
    [Export] private bool _isBonusRoom = false;
    [Export] private NodePath _nextRoomPath; // For bonus room transitions
    [Export] private Marker2D _stickCloneSpawn; // Spawn position marker for StickClone

    private Slingshot? _slingshot;
    private Node2D? _exitDoor;
    private ProjectilesLoader? _projectilesLoader;
    private Node2D? _nextRoomMarker;
    private StickClone? _stickClone;

    private enum RoomPhase { SLINGSHOT, TRAVERSAL, COMPLETE }
    private RoomPhase _currentPhase = RoomPhase.SLINGSHOT;

    private int _destructionScore = 0;
    private bool _exitUnlocked = false;
    private int _roomNumber = 1;
    private string _faceCustomization = ""; // Path to face texture for StickClone

    public override void _Ready()
    {
        InitializeRoom();
        ConnectSignals();
    }

    private void InitializeRoom()
    {
        _slingshot = GetNodeOrNull<Slingshot>(_slingshotPath);
        _exitDoor = GetNodeOrNull<Node2D>(_exitDoorPath);
        _projectilesLoader = GetNodeOrNull<ProjectilesLoader>(_projectilesLoaderPath);
        _nextRoomMarker = GetNodeOrNull<Node2D>(_nextRoomPath);

        if (_exitDoor != null)
        {
            _exitDoor.SetProcess(false);
            // TODO: Set exit door visual state to locked
        }

        // Set up target score from GameManager if available
        var currentRoomIndex = GameManager.Instance?.CurrentRoomIndex ?? 0;
        if (currentRoomIndex >= 0 && currentRoomIndex < GameManager.Instance.Rooms.Length)
        {
            _targetScore = GameManager.Instance.Rooms[currentRoomIndex].TargetScore;
            _roomNumber = currentRoomIndex + 1;
        }
    }

    private void ConnectSignals()
    {
        // Connect to SignalManager for game events
        SignalManager.Instance.OnDestructionScoreUpdated += OnDestructionScoreUpdated;
        SignalManager.Instance.OnCupDestroyed += OnCupDestroyed;
        SignalManager.Instance.OnPropDestroyed += OnPropDestroyed;
        SignalManager.Instance.OnAnimalDied += OnAnimalDied;
        SignalManager.Instance.OnLevelCompleted += OnSlinghotPhaseComplete;

        // Connect to projectiles loader for phase transitions
        if (_projectilesLoader != null)
        {
            _projectilesLoader.ProjectileLaunched += OnProjectileLaunched;
            _projectilesLoader.AllProjectilesUsed += OnAllProjectilesUsed;
        }

        // Connect to slingshot for launch events
        if (_slingshot != null)
        {
            _slingshot.ProjectileLaunched += OnSlingshotProjectileLaunched;
        }
    }

    public override void _ExitTree()
    {
        // Disconnect signals to prevent memory leaks
        if (SignalManager.Instance != null)
        {
            SignalManager.Instance.OnDestructionScoreUpdated -= OnDestructionScoreUpdated;
            SignalManager.Instance.OnCupDestroyed -= OnCupDestroyed;
            SignalManager.Instance.OnPropDestroyed -= OnPropDestroyed;
            SignalManager.Instance.OnAnimalDied -= OnAnimalDied;
            SignalManager.Instance.OnLevelCompleted -= OnSlinghotPhaseComplete;
        }

        if (_projectilesLoader != null)
        {
            _projectilesLoader.ProjectileLaunched -= OnProjectileLaunched;
            _projectilesLoader.AllProjectilesUsed -= OnAllProjectilesUsed;
        }

        if (_slingshot != null)
        {
            _slingshot.ProjectileLaunched -= OnSlingshotProjectileLaunched;
        }
    }

    private void OnDestructionScoreUpdated(int score)
    {
        _destructionScore = score;
        
        // Check if target score is reached
        if (_destructionScore >= _targetScore && !_exitUnlocked)
        {
            UnlockExitDoor();
        }
    }

    private void OnCupDestroyed()
    {
        // Cup destruction contributes to score
        GD.Print("Cup destroyed in room");
    }

    private void OnPropDestroyed(Node prop, int scoreValue)
    {
        // Prop destruction contributes to score
        GD.Print($"Prop destroyed with score value: {scoreValue}");
    }

    private void OnAnimalDied()
    {
        // Animal death triggers traversal phase if projectiles are available
        if (_currentPhase == RoomPhase.SLINGSHOT && _projectilesLoader != null)
        {
            // Check if more projectiles are available
            if (_projectilesLoader.HasMoreProjectiles)
            {
                StartTraversalPhase();
            }
            else
            {
                // No more projectiles, check if level should complete
                MaybeCompleteRoom();
            }
        }
    }

    private void OnProjectileLaunched(Projectile projectile)
    {
        // Projectile launched from projectiles loader
    }

    private void OnSlingshotProjectileLaunched(Projectile projectile)
    {
        // Projectile launched from slingshot - start monitoring for death
        projectile.AlmostStopped += () => OnProjectileAlmostStopped(projectile);
    }

    private void OnProjectileAlmostStopped(Projectile projectile)
    {
        // This will trigger the traversal phase
        if (_currentPhase == RoomPhase.SLINGSHOT)
        {
            StartTraversalPhase();
        }
    }

    private void OnAllProjectilesUsed()
    {
        // All projectiles used, check if room should complete
        MaybeCompleteRoom();
    }

    private void StartTraversalPhase()
    {
        if (_currentPhase != RoomPhase.SLINGSHOT)
            return;

        GD.Print("Starting traversal phase");
        _currentPhase = RoomPhase.TRAVERSAL;
        EmitSignal(SignalName.TraversalPhaseStarted);

        // Spawn StickClone with player's face customization
        SpawnStickClone();
    }

    private void SpawnStickClone()
    {
        // Get face customization from PlayerProfile
        var hat = PlayerProfile.GetHats()[PlayerProfile.Instance.SelectedHatIndex];
        var glasses = PlayerProfile.GetGlasses()[PlayerProfile.Instance.SelectedGlassesIndex];
        var emotion = PlayerProfile.GetEmotions()[PlayerProfile.Instance.SelectedEmotionIndex];

        // Find a spawn position for the StickClone
        var spawnPosition = FindStickCloneSpawnPosition();
        if (spawnPosition == Vector2.Zero)
        {
            GD.PushWarning("Could not find spawn position for StickClone");
            return;
        }

        // Create StickClone instance
        var stickCloneScene = ResourceLoader.Load<PackedScene>("res://Scenes/Characters/StickClone.tscn");
        if (stickCloneScene != null)
        {
            var stickClone = stickCloneScene.Instantiate<StickClone>();
            stickClone.GlobalPosition = spawnPosition;
            AddChild(stickClone);
            
            GD.Print($"Spawning StickClone at {spawnPosition} with: Hat={hat}, Glasses={glasses}, Emotion={emotion}");
        }
        else
        {
            GD.PushWarning("StickClone scene not found: res://Scenes/Characters/StickClone.tscn");
        }
    }

    private void UnlockExitDoor()
    {
        _exitUnlocked = true;
        
        if (_exitDoor != null)
        {
            // TODO: Set exit door visual state to unlocked
            _exitDoor.SetProcess(true);
        }

        GD.Print($"Exit door unlocked! Score: {_destructionScore}/{_targetScore}");
        EmitSignal(SignalName.ExitDoorUnlocked);
    }

    private void MaybeCompleteRoom()
    {
        // Room completes when:
        // 1. Exit door is unlocked AND
        // 2. All projectiles are used OR player reaches exit
        if (_exitUnlocked)
        {
            CompleteRoom();
        }
    }

    private void CompleteRoom()
    {
        if (_currentPhase == RoomPhase.COMPLETE)
            return;

        GD.Print("Room completed!");
        _currentPhase = RoomPhase.COMPLETE;
        EmitSignal(SignalName.RoomTargetReached);

        // Handle bonus room transitions
        if (_isBonusRoom && _nextRoomMarker != null)
        {
            HandleBonusRoomTransition();
        }
        else
        {
            // Standard room completion
            SignalManager.EmitOnLevelCompleted();
        }
    }

    private void HandleBonusRoomTransition()
    {
        // Handle special transitions like Cafeteria -> VentEscape
        GD.Print("Bonus room completed, handling transition...");
        
        // TODO: Implement specific bonus room logic
        // For now, just complete the room
        SignalManager.EmitOnLevelCompleted();
    }

    /// <summary>
    /// Called when player reaches the exit door.
    /// </summary>
    public void OnExitReached()
    {
        GD.Print("Player reached exit door");
        MaybeCompleteRoom();
    }

    /// <summary>
    /// Gets the current destruction score.
    /// </summary>
    public int GetDestructionScore() => _destructionScore;

    /// <summary>
    /// Gets the target score for this room.
    /// </summary>
    public int GetTargetScore() => _targetScore;

    /// <summary>
    /// Checks if the exit door is unlocked.
    /// </summary>
    public bool IsExitUnlocked() => _exitUnlocked;

    /// <summary>
    /// Sets the room number for this room.
    /// </summary>
    public void SetRoomNumber(int roomNumber) => _roomNumber = roomNumber;

    /// <summary>
    /// Sets the face customization for StickClone.
    /// </summary>
    public void SetFaceCustomization(string faceResourcePath) => _faceCustomization = faceResourcePath;

    /// <summary>
    /// Gets the current phase of the room.
    /// </summary>
    public RoomPhase GetCurrentPhase() => _currentPhase;

    /// <summary>
    /// Gets the StickClone instance if spawned.
    /// </summary>
    public StickClone GetStickClone() => _stickClone;

    /// <summary>
    /// Finds a suitable spawn position for StickClone.
    /// </summary>
    /// <returns>Spawn position or Vector2.Zero if not found</returns>
    private Vector2 FindStickCloneSpawnPosition()
    {
        // Look for a spawn marker
        var spawnMarker = GetNodeOrNull<Node2D>("StickCloneSpawn");
        if (spawnMarker != null)
        {
            return spawnMarker.GlobalPosition;
        }

        // Fallback to slingshot position
        if (_slingshot != null)
        {
            return _slingshot.GlobalPosition + new Vector2(100, 0);
        }

        // Ultimate fallback to origin
        return Vector2.Zero;
    }
}