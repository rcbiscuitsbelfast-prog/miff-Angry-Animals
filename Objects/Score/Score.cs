using Godot;

/// <summary>
/// Displays the current score (attempts) during gameplay.
/// Updates automatically when the score changes.
/// </summary>
public partial class Score : Label
{
	[Export] string _scorePrefix = "Attempts: ";

	public override void _Ready()
	{
		SignalManager.Instance.OnScoreUpdated += OnScoreUpdated;

		Text = _scorePrefix + "0";
	}

	private void OnScoreUpdated(int score)
	{
		Text = _scorePrefix + score;
	}

	public override void _ExitTree()
	{
		SignalManager.Instance.OnScoreUpdated -= OnScoreUpdated;
	}
}
