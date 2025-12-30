using Godot;

namespace AngryAnimals.UI;

/// <summary>
/// Level selection button in the menu.
/// Displays the level number and best score, and loads the corresponding scene when pressed.
/// </summary>
public partial class Button : TextureButton
{
    /// <summary>
    /// The level number the button represents.
    /// </summary>
    [Export] public int LevelNumber { get; set; }

    [Export] private Label _levelLabel = null!;
    [Export] private Label _scoreLabel = null!;

    private static readonly Vector2 HOVER_SCALE = new(1.2f, 1.2f);
    private static readonly Vector2 NORMAL_SCALE = new(1, 1);

    public override void _Ready()
    {
        _levelLabel.Text = $"{LevelNumber}";
        _scoreLabel.Text = ScoreManager.GetLevelBestScore(LevelNumber).ToString("D4");

        MouseEntered += OnMouseEntered;
        MouseExited += OnMouseExited;
        Pressed += OnButtonPressed;
    }

    public override void _ExitTree()
    {
        MouseEntered -= OnMouseEntered;
        MouseExited -= OnMouseExited;
        Pressed -= OnButtonPressed;
    }

    private void OnButtonPressed()
    {
        GameManager.StartRoomByLevelNumber(LevelNumber);
    }

    private void OnMouseEntered() => Scale = HOVER_SCALE;

    private void OnMouseExited() => Scale = NORMAL_SCALE;
}
