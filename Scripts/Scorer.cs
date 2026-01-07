using Godot;

/// <summary>
/// Tracks player's performance within a level.
/// Counts destroyed cups, number of attempts, and updates ScoreManager when conditions are met.
/// Now also tracks enemy destruction.
/// </summary>
public partial class Scorer : Node
{
    [Export] public int TargetScore = 1000;
    [Export] public int EnemyPoints = 100; // Points awarded per enemy destroyed

	private int _totalCups;
	private int _cupsDestroyed;
	private int _attempt = 0;
    private int _currentDestructionScore = 0;


	// Called when node enters the scene tree for the first time.
	public override void _Ready()
	{
		// Listens for when a cup is destroyed.
		SignalManager.Instance.Connect(SignalManager.SignalName.OnCupDestroyed, Callable.From(OnCupDestroyed));

		//Listen for when the player makes an attempt.
		SignalManager.Instance.Connect(SignalManager.SignalName.OnAttemptMade, Callable.From(OnAttemptMade));

        // Listen for destruction score updates
        SignalManager.Instance.Connect(SignalManager.SignalName.OnDestructionScoreUpdated, Callable.From<int>(OnDestructionScoreUpdated));

		// Count all cups currently in the level (grouped by cup.GROUP_NAME);
		_totalCups = GetTree().GetNodesInGroup(Cup.GROUP_NAME).Count;
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
        // Ticket says: "when total destruction meets the target exit unlocks"
        // It doesn't explicitly say "destroy all cups" is no longer valid, but usually points replace simple count.
        // But if I want to support existing levels that might rely on cups...
        // Let's assume TargetScore is the new way.
        // If TargetScore is 0 (default?), maybe fallback to cups?
        // But I exported TargetScore = 1000.

        // Let's rely on TargetScore primarily if updated.
        // But wait, existing cups might not have score values set up if they aren't converted yet.
        // I need to update Cup.cs too.

        if (_currentDestructionScore >= TargetScore)
        {
			SignalManager.EmitOnLevelCompleted();
			ScoreManager.SetLevelScore(ScoreManager.GetLevel(), _attempt);
        }
    }


	/// <summary>
	/// Called whenever the player makes an attempt.
	/// Increments attempt counter and updates score display.
	/// </summary>
	private void OnAttemptMade()
	{
		_attempt++;
		SignalManager.EmitOnScoreUpdated(_attempt);
	}

    /// <summary>
    /// Adds score when an enemy is destroyed.
    /// </summary>
    public void AddScore(int points, Vector2 position)
    {
        ScoreManager.AddScore(points);
        CheckLevelCompletion();

        // Show score popup at enemy position
        var popup = GD.Load<PackedScene>("res://Scenes/ScorePopup.tscn").Instantiate<ScorePopup>();
        GetParent().AddChild(popup);
        popup.GlobalPosition = position;
        popup.ShowScore(points);
    }
}
