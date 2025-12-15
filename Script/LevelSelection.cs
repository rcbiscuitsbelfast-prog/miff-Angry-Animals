using Godot;

/// <summary>
/// Handles the level selection screen.
/// Displays available levels with their unlock states and best scores.
/// Allows players to select a level to play.
/// </summary>
public partial class LevelSelection : CanvasLayer
{
	[Export] Container _levelButtonContainer;
	[Export] Button _backButton;
	[Export] Label _titleLabel;

	private PackedScene _levelButtonScene;
	private const int TOTAL_LEVELS = 10;

	public override void _Ready()
	{
		if (_levelButtonContainer == null) GD.PrintErr("LevelButtonContainer not assigned");
		if (_backButton == null) GD.PrintErr("BackButton not assigned");

		if (_backButton != null)
			_backButton.Pressed += OnBackPressed;

		PopulateLevels();
	}

	private void PopulateLevels()
	{
		if (_levelButtonContainer == null) return;

		for (int i = 1; i <= TOTAL_LEVELS; i++)
		{
			CreateLevelButton(i);
		}
	}

	private void CreateLevelButton(int levelNumber)
	{
		if (_levelButtonContainer == null) return;

		var button = new Button
		{
			Text = $"Level {levelNumber}",
			CustomMinimumSize = new Vector2(100, 100),
			Disabled = !PlayerProfile.IsLevelUnlocked(levelNumber)
		};

		int bestScore = ScoreManager.GetLevelBestScore(levelNumber);
		if (bestScore > 0)
		{
			button.Text += $"\n({bestScore})";
		}

		int level = levelNumber;
		button.Pressed += () => OnLevelSelected(level);

		_levelButtonContainer.AddChild(button);
	}

	private void OnLevelSelected(int levelNumber)
	{
		ScoreManager.SetLevel(levelNumber);
		GameManager.LoadLevel(levelNumber);
	}

	private void OnBackPressed()
	{
		GameManager.LoadMainMenu();
	}

	public override void _ExitTree()
	{
		if (_backButton != null)
			_backButton.Pressed -= OnBackPressed;
	}
}
