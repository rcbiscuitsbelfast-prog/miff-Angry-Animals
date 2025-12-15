using Godot;

/// <summary>
/// Handles the main menu screen with animated background and navigation buttons.
/// Provides entry point to level selection and game settings.
/// </summary>
public partial class MainMenu : CanvasLayer
{
	[Export] Button _playButton;
	[Export] Button _quitButton;
	[Export] Control _titleContainer;
	[Export] Label _titleLabel;

	private Vector2 _originalTitlePosition;
	private float _titleBounceAmount = 20f;
	private float _titleBounceSpeed = 2f;
	private float _bounceTimer = 0f;

	public override void _Ready()
	{
		if (_playButton == null) GD.PrintErr("PlayButton not assigned in MainMenu");
		if (_quitButton == null) GD.PrintErr("QuitButton not assigned in MainMenu");
		if (_titleContainer == null) GD.PrintErr("TitleContainer not assigned in MainMenu");

		if (_titleContainer != null)
			_originalTitlePosition = _titleContainer.Position;

		if (_playButton != null)
			_playButton.Pressed += OnPlayButtonPressed;

		if (_quitButton != null)
			_quitButton.Pressed += OnQuitButtonPressed;

		AnimateTitleIn();
	}

	public override void _Process(double delta)
	{
		if (_titleContainer != null)
		{
			_bounceTimer += (float)delta * _titleBounceSpeed;
			float bounceOffset = Mathf.Sin(_bounceTimer) * _titleBounceAmount;
			_titleContainer.Position = _originalTitlePosition + Vector2.Up * bounceOffset;
		}
	}

	private void AnimateTitleIn()
	{
		if (_titleContainer == null) return;

		var tween = CreateTween();
		tween.SetTrans(Tween.TransitionType.Elastic);
		tween.SetEase(Tween.EaseType.Out);
		tween.TweenProperty(_titleContainer, "scale", Vector2.One, 0.8);
	}

	private void OnPlayButtonPressed()
	{
		GameManager.LoadLevelSelection();
	}

	private void OnQuitButtonPressed()
	{
		GetTree().Quit();
	}

	public override void _ExitTree()
	{
		if (_playButton != null)
			_playButton.Pressed -= OnPlayButtonPressed;

		if (_quitButton != null)
			_quitButton.Pressed -= OnQuitButtonPressed;
	}
}
