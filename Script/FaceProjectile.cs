using Godot;

/// <summary>
/// A projectile with a face sprite, representing an animal character.
/// Extends the base Projectile class with visual representation.
/// </summary>
public partial class FaceProjectile : Projectile
{
    [Export] private Sprite2D _faceSprite;
    private ExpressionManager _expressionManager;
    private Vector2 _lastVelocity;
    private ExpressionType _lastExpression = ExpressionType.Neutral;
    private bool _hasTriggeredLaunchBubble = false;

    public override void _Ready()
    {
        base._Ready();

        _expressionManager = new ExpressionManager();
        AddChild(_expressionManager);

        // If we have a face sprite, center the expressions on it
        if (_faceSprite != null)
        {
            _expressionManager.Position = _faceSprite.Position;
        }

        LoadFaceImage();
    }

    private void LoadFaceImage()
    {
        string path = PlayerProfile.Instance.FaceImagePath;
        if (string.IsNullOrEmpty(path)) return;

        if (FileAccess.FileExists(path))
        {
            var image = new Image();
            if (image.Load(path) == Error.Ok)
            {
                var texture = ImageTexture.CreateFromImage(image);
                if (_faceSprite != null)
                {
                    _faceSprite.Texture = texture;
                }
            }
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        UpdateExpressions((float)delta);
    }

    private void UpdateExpressions(float delta)
    {
        if (_expressionManager == null) return;

        float speed = LinearVelocity.Length();
        float acceleration = (LinearVelocity - _lastVelocity).Length() / delta;
        _lastVelocity = LinearVelocity;

        ExpressionType newExpression = _lastExpression;

        // Logic for expression triggers
        if (speed > 1000f)
        {
            newExpression = ExpressionType.Scared;
        }
        else if (speed < 50f && speed > 5f)
        {
            newExpression = ExpressionType.Bored;
        }
        else if (acceleration > 5000f) // Impact or Launch
        {
            if (speed < 500f)
                newExpression = ExpressionType.Dizzy;
            else
                newExpression = ExpressionType.Determined;
        }

        // Random during flight
        if (speed > 100f && GD.Randf() < 0.01f)
        {
            newExpression = (ExpressionType)GD.RandRange(0, 13);
        }

        // Apply expression change and trigger feedback
        if (newExpression != _lastExpression)
        {
            _expressionManager.SetExpression(newExpression, 1.0f);

            // Play expression vocal
            AudioManager.PlayExpressionVocalSfx(newExpression);

            // Spawn speech bubble on expression change
            if (SpeechBubbleManager.Instance != null)
            {
                SpeechBubbleManager.Instance.OnExpressionChanged(newExpression);
            }

            _lastExpression = newExpression;
        }
    }

    public override void _ExitTree()
    {
        base._ExitTree();

        // Clear speech bubble tracking
        if (SpeechBubbleManager.Instance != null)
        {
            SpeechBubbleManager.Instance.SetCurrentProjectile(null);
        }
    }

    public void OnImpact()
    {
        _expressionManager?.SetExpression(ExpressionType.Dizzy, 2.0f);
    }

    public void OnSuccess()
    {
        _expressionManager?.SetExpression(ExpressionType.Happy, 3.0f);
    }
}
