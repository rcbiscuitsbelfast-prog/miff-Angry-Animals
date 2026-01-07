using Godot;

/// <summary>
/// Debris chunks that can be walked on after destruction.
/// Now uses object pooling for performance optimization.
/// </summary>
public partial class Rubble : RigidBody2D, IPoolable
{
    [Export] public float FadeDelay = 2.0f;
    [Export] public float FadeDuration = 1.0f;

    // Object pooling support
    private ObjectPool _pool;
    private bool _isPooled = false;
    private Tween _fadeTween;

    public override void _Ready()
    {
        // Try to get object pool reference
        _pool = GetNode<ObjectPool>("/root/ObjectPool");
        _isPooled = _pool != null;

        AddToGroup("walkable_rubble");

        // Random rotation and impulse for better feel
        Rotation = GD.Randf() * Mathf.Tau;
        ApplyTorqueImpulse(GD.RandfRange(-10, 10));

        GetTree().CreateTimer(FadeDelay).Timeout += StartFade;
    }

    private void StartFade()
    {
        if (_isPooled)
        {
            MarkForPooling();
        }
        else
        {
            _fadeTween = CreateTween();
            _fadeTween.TweenProperty(this, "modulate:a", 0.0f, FadeDuration);
            _fadeTween.TweenCallback(Callable.From(QueueFree));
        }
    }

    public void ResetForPool()
    {
        Modulate = Colors.White;
        LinearVelocity = Vector2.Zero;
        AngularVelocity = 0f;
        Rotation = GD.Randf() * Mathf.Tau;
        Freeze = true;
    }

    public void MarkForPooling()
    {
        SetMeta("can_be_pooled", true);
    }
}
