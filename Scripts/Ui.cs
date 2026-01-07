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

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		// Initializes UI labels.
		_gameOverVB.Hide();
		_levelLabel.Text = $"Level: {ScoreManager.GetLevel()}";

		// Connects relevant signals.
		SignalManager.Instance.OnScoreUpdated += OnUpdateAttemptsLabel;
		SignalManager.Instance.OnLevelCompleted += OnLevelFinished;
	}

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        // Checks if the Game Over panel is visible and listens for the restar input.
        if (_gameOverVB.Visible && Input.IsActionJustPressed("level_completed"))
        {
			GameManager.LoadMain();
        }
    }

	// Called when the node is removed from the scene tree.
	public override void _ExitTree()
	{
		// Disconects signals to prevent memory leaks.
		SignalManager.Instance.OnScoreUpdated -= OnUpdateAttemptsLabel;
        SignalManager.Instance.OnLevelCompleted -= OnLevelFinished;
    }


	/// <summary>
	/// Updates the attempts label whenever the player makes a new attempt.
	/// </summary>
	/// <param name="attempts">The total number of attempts made by the player.</param>
	private void OnUpdateAttemptsLabel(int attempts) => _attemptLabel.Text = $"Attempts: {attempts}";


	/// <summary>
	/// Displays the Game Over panel when the level is completed.
	/// </summary>
    private void OnLevelFinished() => _gameOverVB.Show();
}