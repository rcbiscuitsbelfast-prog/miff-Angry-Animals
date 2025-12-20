using Godot;

/// <summary>
/// VentEscape is a bonus room implementation.
/// A special room that becomes available after completing certain levels.
/// Features enhanced challenges and unique mechanics.
/// </summary>
public partial class VentEscape : RoomBase
{
    [Export] int _targetScore = 15; // Higher target for bonus room
    [Export] float _timeLimit = 60.0f; // Time limit for bonus challenge

    private ExitDoor _exitDoor;
    private float _timeRemaining;
    private bool _isTimeRunning = false;
    private Label _timerLabel;

    public override void _Ready()
    {
        base._Ready();

        // Initialize VentEscape-specific setup
        InitializeVentEscape();
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        // Update timer
        if (_isTimeRunning)
        {
            _timeRemaining -= (float)delta;

            if (_timerLabel != null)
            {
                _timerLabel.Text = $"Time: {_timeRemaining:F1}s";
            }

            if (_timeRemaining <= 0)
            {
                OnTimeExpired();
            }
        }
    }

    /// <summary>
    /// Initializes VentEscape-specific elements.
    /// </summary>
    private void InitializeVentEscape()
    {
        // Find the exit door in the scene (check base class assignment first)
        if (_exitDoor == null)
        {
            _exitDoor = GetNodeOrNull<ExitDoor>("ExitDoor");
        }

        if (_exitDoor != null)
        {
            // Set the target score for the exit door
            _exitDoor.SetTargetScore(_targetScore);
        }

        // Find timer label if it exists
        var timerPath = GetNodeOrNull<Node>("UI/TimerLabel");
        if (timerPath is Label label)
        {
            _timerLabel = label;
        }

        // Start the timer
        _timeRemaining = _timeLimit;
        _isTimeRunning = true;

        // Connect scoring signals
        SignalManager.Instance.Connect(SignalManager.SignalName.OnCupDestroyed, Callable.From(CheckExitUnlock));
    }

    /// <summary>
    /// Checks if enough targets have been destroyed to unlock the exit door.
    /// </summary>
    private void CheckExitUnlock()
    {
        if (_exitDoor != null && !_exitDoor.IsUnlocked())
        {
            // Count remaining cups (destroyed ones are removed from the scene)
            int cupsRemaining = GetTree().GetNodesInGroup(Cup.GROUP_NAME).Count;

            // If all cups are destroyed or target score is met, unlock the door
            if (cupsRemaining == 0 || cupsRemaining <= _targetScore)
            {
                _exitDoor.SetUnlocked(true);
            }
        }
    }

    /// <summary>
    /// Called when time runs out in the bonus room.
    /// Fails the room if StickClone hasn't reached the exit.
    /// </summary>
    private void OnTimeExpired()
    {
        _isTimeRunning = false;
        GD.Print("Time expired in VentEscape!");

        if (_exitDoor != null && !_exitDoor.IsUnlocked())
        {
            // Time out - room fails
            EmitSignal(SignalName.OnRoomFailed);
        }
    }

    /// <summary>
    /// Gets the bonus room difficulty multiplier.
    /// </summary>
    public float GetDifficultyMultiplier() => 1.5f;
}
