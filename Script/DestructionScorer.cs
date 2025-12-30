using Godot;

namespace AngryAnimals.Scoring;

/// <summary>
/// Handles destruction scoring and score popups for destructible props.
/// This is separate from the global RageSystem (rage bar + combo signals).
/// </summary>
public partial class DestructionScorer : Node
{
    /// <summary>
    /// The scene used to instantiate score popups.
    /// </summary>
    [Export] public PackedScene? ScorePopupScene;

    private int _currentDestructionScore;
    private int _comboMultiplier = 1;
    private float _comboTimer;

    private const float COMBO_WINDOW = 2.0f;

    public override void _Ready()
    {
        if (SignalManager.Instance != null)
        {
            SignalManager.Instance.OnPropDamaged += HandlePropDamaged;
            SignalManager.Instance.OnPropDestroyed += HandlePropDestroyed;
        }
    }

    public override void _Process(double delta)
    {
        if (_comboTimer <= 0)
            return;

        _comboTimer -= (float)delta;
        if (_comboTimer <= 0)
            ResetCombo();
    }

    public override void _ExitTree()
    {
        if (SignalManager.Instance != null)
        {
            SignalManager.Instance.OnPropDamaged -= HandlePropDamaged;
            SignalManager.Instance.OnPropDestroyed -= HandlePropDestroyed;
        }
    }

    private void HandlePropDamaged(Node prop, int damage)
    {
        SpawnPopup(prop, damage);
    }

    private void HandlePropDestroyed(Node prop, int scoreValue)
    {
        _comboMultiplier++;
        _comboTimer = COMBO_WINDOW;

        int points = scoreValue * _comboMultiplier;
        _currentDestructionScore += points;

        SpawnPopup(prop, points);
        SignalManager.EmitOnDestructionScoreUpdated(_currentDestructionScore);
    }

    private void SpawnPopup(Node prop, int value)
    {
        if (ScorePopupScene == null || prop is not Node2D prop2D)
            return;

        var popup = ScorePopupScene.Instantiate<ScorePopup>();
        popup.GlobalPosition = prop2D.GlobalPosition;
        popup.Setup(value);

        GetTree().CurrentScene?.AddChild(popup);
    }

    private void ResetCombo()
    {
        _comboMultiplier = 1;
    }
}
