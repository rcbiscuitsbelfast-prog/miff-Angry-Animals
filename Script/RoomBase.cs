using Godot;

/// <summary>
/// RoomBase represents a single room level in the traversal flow.
/// Manages the slingshot phase (projectile destruction) and the traversal phase (StickClone navigation).
/// Coordinates between the slingshot system and the character movement phase.
/// </summary>
public partial class RoomBase : Node2D
{
    /// <summary>
    /// Phases of a room's gameplay.
    /// </summary>
    public enum RoomPhase { SLINGSHOT, TRAVERSAL, COMPLETED }

    // Node references
    [Export] Node2D _slinghotRoot; // Reference to slingshot setup (will be replaced with actual slingshot nodes)
    [Export] Marker2D _stickCloneSpawn; // Spawn position marker for StickClone
    [Export] ExitDoor _exitDoor; // Exit door that triggers room completion
    [Export] PackedScene _stickCloneScene = GD.Load<PackedScene>("res://Scenes/StickClone/StickClone.tscn");

    private RoomPhase _currentPhase = RoomPhase.SLINGSHOT;
    private StickClone _stickClone;
    private int _roomNumber = 1;
    private string _faceCustomization = ""; // Path to face texture for StickClone

    // Signals for room completion and progression
    [Signal]
    public delegate void OnRoomCompletedEventHandler(int roomNumber);

    [Signal]
    public delegate void OnTraversalStartedEventHandler();

    [Signal]
    public delegate void OnRoomFailedEventHandler();

    public override void _Ready()
    {
        // Start in slingshot phase
        _currentPhase = RoomPhase.SLINGSHOT;

        // Find ExitDoor if not assigned
        if (_exitDoor == null)
        {
            var door = GetNodeOrNull<ExitDoor>("ExitDoor");
            if (door != null)
            {
                _exitDoor = door;
            }
        }

        // Connect to relevant signals
        ConnectSignals();
    }

    public override void _Process(double delta)
    {
        // Override allows VentEscape to use _PhysicsProcess for timer updates
    }

    /// <summary>
    /// Connects room-relevant signals for phase transitions.
    /// </summary>
    private void ConnectSignals()
    {
        // Listen for when all projectiles are destroyed (end of slingshot phase)
        SignalManager.Instance.Connect(SignalManager.SignalName.OnLevelCompleted, Callable.From(OnSlinghotPhaseComplete));
    }

    /// <summary>
    /// Called when the slingshot phase is complete (all targets destroyed).
    /// Transitions to the traversal phase.
    /// </summary>
    private void OnSlinghotPhaseComplete()
    {
        if (_currentPhase == RoomPhase.SLINGSHOT)
        {
            TransitionToTraversalPhase();
        }
    }

    /// <summary>
    /// Transitions from slingshot phase to traversal phase.
    /// Spawns StickClone with face customization and sets up exit door.
    /// </summary>
    private void TransitionToTraversalPhase()
    {
        _currentPhase = RoomPhase.TRAVERSAL;

        // Spawn StickClone
        SpawnStickClone();

        // Emit signal that traversal has started
        EmitSignal(SignalName.OnTraversalStarted);
    }

    /// <summary>
    /// Spawns StickClone at the designated spawn position.
    /// Applies face customization if available.
    /// </summary>
    private void SpawnStickClone()
    {
        if (_stickCloneScene == null)
        {
            GD.PrintErr("StickClone scene not assigned to RoomBase");
            return;
        }

        _stickClone = _stickCloneScene.Instantiate<StickClone>();

        if (_stickCloneSpawn != null)
        {
            _stickClone.GlobalPosition = _stickCloneSpawn.GlobalPosition;
        }

        // Apply face customization if available
        if (!string.IsNullOrEmpty(_faceCustomization))
        {
            _stickClone.SetFaceCustomization(_faceCustomization);
        }

        // Connect StickClone signals
        _stickClone.Connect(StickClone.SignalName.OnReachedExit, Callable.From(OnStickCloneReachedExit));
        _stickClone.Connect(StickClone.SignalName.OnDied, Callable.From(OnStickCloneDied));

        AddChild(_stickClone);
    }

    /// <summary>
    /// Called when StickClone reaches the exit door.
    /// Completes the room.
    /// </summary>
    private void OnStickCloneReachedExit()
    {
        _currentPhase = RoomPhase.COMPLETED;
        EmitSignal(SignalName.OnRoomCompleted, _roomNumber);
    }

    /// <summary>
    /// Called when StickClone dies during traversal.
    /// Resets the traversal phase or fails the room.
    /// </summary>
    private void OnStickCloneDied()
    {
        EmitSignal(SignalName.OnRoomFailed);
        // Could reset to slingshot phase or handle retry logic here
    }

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
}
