using Godot;

/// <summary>
/// Handles the level completed screen displayed after winning a level.
/// Shows the final score, stars/rating, and navigation buttons.
/// </summary>
public partial class LevelCompleted : CanvasLayer
{
	[Export] Label _completedLabel;
	[Export] Label _scoreLabel;
	[Export] Button _nextLevelButton;
	[Export] Button _retryButton;
	[Export] Button _levelSelectButton;
	[Export] Control _panel;

	private int _currentLevel;
	private int _finalScore;

	public override void _Ready()
	{
		if (_nextLevelButton != null)
			_nextLevelButton.Pressed += OnNextLevelPressed;

		if (_retryButton != null)
			_retryButton.Pressed += OnRetryPressed;

		if (_levelSelectButton != null)
			_levelSelectButton.Pressed += OnLevelSelectPressed;

		if (_panel != null)
			_panel.Modulate = new Color(1, 1, 1, 0);

		if (SignalManager.Instance != null)
			SignalManager.Instance.OnLevelCompleted += OnLevelComplete;
	}

	private void OnLevelComplete()
	{
		_currentLevel = ScoreManager.GetLevel();
		_finalScore = ScoreManager.GetLevelBestScore(_currentLevel);

		if (_completedLabel != null)
			_completedLabel.Text = "Level Complete!";

		if (_scoreLabel != null)
			_scoreLabel.Text = $"Score: {_finalScore} attempts";

		AnimatePanel();
	}

	private void AnimatePanel()
	{
		if (_panel == null) return;

		var tween = CreateTween();
		tween.SetTrans(Tween.TransitionType.Quad);
		tween.SetEase(Tween.EaseType.Out);
		tween.TweenProperty(_panel, "modulate", new Color(1, 1, 1, 1), 0.5);
	}

	private void OnNextLevelPressed()
	{
		int nextLevel = _currentLevel + 1;
		if (PlayerProfile.IsLevelUnlocked(nextLevel))
		{
			PlayerProfile.SetUnlockedLevels(nextLevel);
			ScoreManager.SetLevel(nextLevel);
			GameManager.LoadLevel(nextLevel);
		}
		else
		{
			GameManager.LoadLevelSelection();
		}
	}

	private void OnRetryPressed()
	{
		GameManager.LoadLevel(_currentLevel);
	}

	private void OnLevelSelectPressed()
	{
		GameManager.LoadLevelSelection();
	}

	public override void _ExitTree()
	{
		if (_nextLevelButton != null)
			_nextLevelButton.Pressed -= OnNextLevelPressed;

		if (_retryButton != null)
			_retryButton.Pressed -= OnRetryPressed;

		if (_levelSelectButton != null)
			_levelSelectButton.Pressed -= OnLevelSelectPressed;

		if (SignalManager.Instance != null)
			SignalManager.Instance.OnLevelCompleted -= OnLevelComplete;
	}
}
