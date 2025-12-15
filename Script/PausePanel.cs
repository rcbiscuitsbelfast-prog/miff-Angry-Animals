using Godot;

/// <summary>
/// Handles the pause menu overlay displayed when the player pauses the game.
/// Provides options to resume, retry, or return to level selection.
/// </summary>
public partial class PausePanel : CanvasLayer
{
	[Export] Control _panelContainer;
	[Export] Button _resumeButton;
	[Export] Button _retryButton;
	[Export] Button _levelSelectButton;
	[Export] Label _pauseLabel;

	private bool _isPaused = false;

	public override void _Ready()
	{
		if (_panelContainer != null)
			_panelContainer.Visible = false;

		if (_resumeButton != null)
			_resumeButton.Pressed += OnResumePressed;

		if (_retryButton != null)
			_retryButton.Pressed += OnRetryPressed;

		if (_levelSelectButton != null)
			_levelSelectButton.Pressed += OnLevelSelectPressed;
	}

	public override void _Process(double delta)
	{
		if (Input.IsActionJustPressed("pause"))
		{
			TogglePause();
		}
	}

	private void TogglePause()
	{
		_isPaused = !_isPaused;

		if (_panelContainer != null)
			_panelContainer.Visible = _isPaused;

		GetTree().Paused = _isPaused;
	}

	private void OnResumePressed()
	{
		TogglePause();
	}

	private void OnRetryPressed()
	{
		GetTree().Paused = false;
		int currentLevel = ScoreManager.GetLevel();
		GameManager.LoadLevel(currentLevel);
	}

	private void OnLevelSelectPressed()
	{
		GetTree().Paused = false;
		GameManager.LoadLevelSelection();
	}

	public override void _ExitTree()
	{
		if (_resumeButton != null)
			_resumeButton.Pressed -= OnResumePressed;

		if (_retryButton != null)
			_retryButton.Pressed -= OnRetryPressed;

		if (_levelSelectButton != null)
			_levelSelectButton.Pressed -= OnLevelSelectPressed;

		GetTree().Paused = false;
	}
}
