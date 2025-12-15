using Godot;

/// <summary>
/// Handles the user interface of the game.
/// Displays the current level, attempts made by the player, and shows the Game Over panel when the level is completed.
/// </summary>
public partial class Ui : MarginContainer
{
	[Export] Label _levelLabel;
	[Export] Label _attemptLabel;
	[Export] BoxContainer _gameOverVB;

	private bool _levelCompleted = false;

	public override void _Ready()
	{
		if (_gameOverVB != null)
			_gameOverVB.Hide();

		if (_levelLabel != null)
			_levelLabel.Text = $"Level: {ScoreManager.GetLevel()}";

		if (SignalManager.Instance != null)
		{
			SignalManager.Instance.OnScoreUpdated += OnUpdateAttemptsLabel;
			SignalManager.Instance.OnLevelCompleted += OnLevelFinished;
		}
	}

	public override void _Process(double delta)
	{
		if (_levelCompleted && Input.IsActionJustPressed("level_completed"))
		{
			GameManager.LoadLevelCompleted();
		}
	}

	public override void _ExitTree()
	{
		if (SignalManager.Instance != null)
		{
			SignalManager.Instance.OnScoreUpdated -= OnUpdateAttemptsLabel;
			SignalManager.Instance.OnLevelCompleted -= OnLevelFinished;
		}
	}

	/// <summary>
	/// Updates the attempts label whenever the player makes a new attempt.
	/// </summary>
	/// <param name="attempts">The total number of attempts made by the player.</param>
	private void OnUpdateAttemptsLabel(int attempts)
	{
		if (_attemptLabel != null)
			_attemptLabel.Text = $"Attempts: {attempts}";
	}

	/// <summary>
	/// Displays the Game Over panel when the level is completed.
	/// </summary>
	private void OnLevelFinished()
	{
		if (_gameOverVB != null)
			_gameOverVB.Show();

		_levelCompleted = true;
	}
}
