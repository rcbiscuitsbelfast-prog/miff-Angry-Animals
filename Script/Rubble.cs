using Godot;

public partial class Rubble : RigidBody2D
{
    [Export] public float FadeDelay = 2.0f;
    [Export] public float FadeDuration = 1.0f;

    public override void _Ready()
    {
        AddToGroup("walkable_rubble");
        
        // Random rotation and impulse for better feel
        Rotation = GD.Randf() * Mathf.Tau;
        ApplyTorqueImpulse(GD.RandfRange(-10, 10));
        
        GetTree().CreateTimer(FadeDelay).Timeout += StartFade;
    }

    private void StartFade()
    {
        Tween tween = CreateTween();
        tween.TweenProperty(this, "modulate:a", 0.0f, FadeDuration);
        tween.TweenCallback(Callable.From(QueueFree));
    }
}
