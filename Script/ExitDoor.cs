using Godot;

/// <summary>
/// ExitDoor represents the goal marker in a room's traversal phase.
/// When StickClone reaches this door, the room is marked as completed.
/// </summary>
public partial class ExitDoor : Area2D
{
    [Export] AnimationPlayer _unlockAnimation;
    [Export] Label _targetScoreLabel;

    private int _targetScore = 0;
    private bool _isUnlocked = false;

    public const string GROUP_NAME = "exit_door";

    public override void _Ready()
    {
        // Add to group for easy reference
        AddToGroup(GROUP_NAME);

        // Connect body entry signal
        BodyEntered += OnBodyEntered;

        // Find AnimationPlayer if not assigned
        if (_unlockAnimation == null)
        {
            _unlockAnimation = GetNodeOrNull<AnimationPlayer>("AnimationPlayer");
        }

        // Find target score label if not assigned
        if (_targetScoreLabel == null)
        {
            _targetScoreLabel = GetNodeOrNull<Label>("TargetScoreLabel");
        }

        // Initialize UI if target score exists
        if (_targetScoreLabel != null && _targetScore > 0)
        {
            _targetScoreLabel.Text = $"Target: {_targetScore}";
        }
    }

    /// <summary>
    /// Called when a body enters the exit door area.
    /// Checks if it's StickClone and triggers completion.
    /// </summary>
    private void OnBodyEntered(Node2D body)
    {
        if (body is StickClone stickClone && _isUnlocked)
        {
            stickClone.TriggerExitDoor();
        }
    }

    /// <summary>
    /// Sets whether the door is unlocked (can be passed).
    /// </summary>
    public void SetUnlocked(bool unlocked)
    {
        _isUnlocked = unlocked;

        if (unlocked && _unlockAnimation != null)
        {
            _unlockAnimation.Play("unlock");
        }
    }

    /// <summary>
    /// Sets the target score needed to unlock this door.
    /// </summary>
    public void SetTargetScore(int score)
    {
        _targetScore = score;
        if (_targetScoreLabel != null)
        {
            _targetScoreLabel.Text = $"Target: {_targetScore}";
        }
    }

    /// <summary>
    /// Checks if the exit door is unlocked.
    /// </summary>
    public bool IsUnlocked() => _isUnlocked;

    /// <summary>
    /// Gets the target score for this door.
    /// </summary>
    public int GetTargetScore() => _targetScore;
}
