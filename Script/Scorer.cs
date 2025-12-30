using Godot;

/// <summary>
/// Tracks the player's performance within a level.
/// Counts destroyed cups, number of attempts, and updates the ScoreManager when conditions are met.
/// </summary>
public partial class Scorer : Node
{
    [Export] public int TargetScore = 1000;

    private int _totalCups;
    private int _cupsDestroyed;
    private int _attempt = 0;
    private int _currentDestructionScore = 0;


    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        // Listens for game events
        if (SignalManager.Instance != null)
        {
            SignalManager.Instance.OnCupDestroyed += OnCupDestroyed;
            SignalManager.Instance.OnAttemptMade += OnAttemptMade;
            SignalManager.Instance.OnDestructionScoreUpdated += OnDestructionScoreUpdated;
        }

        // Count all cups currently in the level (grouped by cup.GROUP_NAME);
        _totalCups = GetTree().GetNodesInGroup(Cup.GROUP_NAME).Count;
    }

    public override void _ExitTree()
    {
        if (SignalManager.Instance != null)
        {
            SignalManager.Instance.OnCupDestroyed -= OnCupDestroyed;
            SignalManager.Instance.OnAttemptMade -= OnAttemptMade;
            SignalManager.Instance.OnDestructionScoreUpdated -= OnDestructionScoreUpdated;
        }
    }


    /// <summary>
    /// Called when a cup is destroyed.
    /// Kept for legacy support or specific cup tracking.
    /// </summary>
    private void OnCupDestroyed()
    {
        _cupsDestroyed++;
        CheckLevelCompletion();
    }

    private void OnDestructionScoreUpdated(int score)
    {
        _currentDestructionScore = score;
        CheckLevelCompletion();
    }
    
    private void CheckLevelCompletion()
    {
        // Condition: Reach target score OR destroy all cups (legacy)?
        // Ticket says: "when total destruction meets the target the exit unlocks"
        // It doesn't explicitly say "destroy all cups" is no longer valid, but usually points replace simple count.
        // But if I want to support existing levels that might rely on cups...
        // Let's assume TargetScore is the new way. 
        // If TargetScore is 0 (default?), maybe fallback to cups?
        // But I exported TargetScore = 1000.
        
        // Let's rely on TargetScore primarily if updated.
        // But wait, the existing cups might not have score values set up if they aren't converted yet.
        // I need to update Cup.cs too.
        
        if (_currentDestructionScore >= TargetScore)
        {
            SignalManager.EmitOnLevelCompleted();
            ScoreManager.SetLevelScore(ScoreManager.GetLevel(), _attempt);
        }
    }


    /// <summary>
    /// Called whenever the player makes an attempt.
    /// Increments attempt counter and updates the score display.
    /// </summary>
    private void OnAttemptMade()
    {
        _attempt++;
        SignalManager.EmitOnScoreUpdated(_attempt);
    }
}
