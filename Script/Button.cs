using Godot;

/// <summary>
/// Level selection button in the menu.
/// Displays the level number and best score, and loads the corresponding sceme when pressed.
/// </summary>
public partial class Button : TextureButton
{
	/// <summary>
	/// The level number the button represents.
	/// </summary>
	[Export] int LevelNumber { get; set; }


	[Export] Label _levelLabel;
	[Export] Label _scoreLabel;


	private static readonly Vector2 HOVER_SCALE = new Vector2(1.2f, 1.2f);
	private static readonly Vector2 NORMAL_SCALE = new Vector2(1, 1);


	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		if (_levelLabel != null)
			_levelLabel.Text = $"{LevelNumber}";

		if (_scoreLabel != null)
			_scoreLabel.Text = ScoreManager.GetLevelBestScore(LevelNumber).ToString("D4");

		// Connecting button events.
		MouseEntered += OnMouseEntered;
		MouseExited += OnMouseExited;
		Pressed += OnButtonPressed;
	}


	/// <summary>
	/// Hanldes the button press event.
	/// Sets the current level and loads the level scene.
	/// </summary>
	private void OnButtonPressed()
	{
		ScoreManager.SetLevel(LevelNumber);
		GetTree().ChangeSceneToFile($"res://Scenes/Level/Level{LevelNumber}.tscn");
	}


	/// <summary>
	/// Enlarges the button when the mouse hovers over it.
	/// </summary>
	private void OnMouseEntered() => Scale = HOVER_SCALE;


	/// <summary>
	/// Resets the button scale when the mouse stops hovering.
	/// </summary>
	private void OnMouseExited() => Scale = NORMAL_SCALE;
}
