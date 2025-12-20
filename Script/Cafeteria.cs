using Godot;

/// <summary>
/// Cafeteria is a specific room implementation.
/// Extends RoomBase with Cafeteria-specific logic and setup.
/// </summary>
public partial class Cafeteria : RoomBase
{
    [Export] int _targetScore = 10; // Score needed to unlock exit

    private ExitDoor _exitDoor;

    public override void _Ready()
    {
        base._Ready();

        // Initialize Cafeteria-specific setup
        InitializeCafeteria();
    }

    /// <summary>
    /// Initializes Cafeteria-specific elements.
    /// </summary>
    private void InitializeCafeteria()
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

        // Connect scoring signals to check if exit should unlock
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
}
