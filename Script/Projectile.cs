using Godot;

/// <summary>
/// Base class for all projectiles launched from the slingshot.
/// Handles physics interactions, collision detection, sound effects, and lifecycle management.
/// Now uses object pooling for performance optimization.
/// </summary>
public partial class Projectile : RigidBody2D, IPoolable
{
    [Signal] public delegate void AlmostStoppedEventHandler();

    [Export] private AudioStreamPlayer2D _kickWoodSound;
    [Export] private VisibleOnScreenNotifier2D _onScreenNotifier;

    private const float STOPPED_THRESHOLD = 0.1f;
    private bool _hasBeenLaunched = false;
    private bool _almostStoppedEmitted = false;
    private int _lastCollisionCount = 0;

    // Object pooling support
    private ObjectPool _pool;
    private bool _isPooled = false;
    
    public override void _Ready()
    {
        // Try to get object pool reference
        _pool = GetNode<ObjectPool>("/root/ObjectPool");
        _isPooled = _pool != null;

        ConnectSignals();
    }
    
    public override void _PhysicsProcess(double delta)
    {
        CheckIfAlmostStopped();
    }
    
    private void ConnectSignals()
    {
        if (_onScreenNotifier != null)
        {
            _onScreenNotifier.ScreenExited += OnScreenExited;
        }
        
        SleepingStateChanged += OnSleepingStateChanged;
    }
    
    public void Launch(Vector2 impulse)
    {
        _hasBeenLaunched = true;
        Freeze = false;
        ApplyCentralImpulse(impulse);
    }
    
    private void CheckIfAlmostStopped()
    {
        if (!_hasBeenLaunched || _almostStoppedEmitted) return;
        
        if (LinearVelocity.Length() < STOPPED_THRESHOLD)
        {
            _almostStoppedEmitted = true;
            EmitSignal(SignalName.AlmostStopped);
        }
    }
    
    public override void _IntegrateForces(PhysicsDirectBodyState2D state)
    {
        if (_hasBeenLaunched && _kickWoodSound != null)
        {
            int contactCount = state.GetContactCount();
            if (_lastCollisionCount == 0 && contactCount > 0 && !_kickWoodSound.Playing)
            {
                _kickWoodSound.Play();

                // Add game feel impact feedback
                if (GameFeelManager.Instance != null)
                {
                    float impactForce = LinearVelocity.Length();
                    GameFeelManager.Instance.OnImpact(this, impactForce);
                }
            }
            _lastCollisionCount = contactCount;
        }
    }
    
    private void OnSleepingStateChanged()
    {
        if (Sleeping && _hasBeenLaunched)
        {
            foreach (Node2D body in GetCollidingBodies())
            {
                if (body is Cup cup)
                {
                    cup.Die();
                }
            }
            
            CallDeferred(MethodName.Die);
        }
    }
    
    private void OnScreenExited()
    {
        if (_hasBeenLaunched)
        {
            Die();
        }
    }
    
    private void Die()
    {
        SignalManager.EmitOnAnimalDied();

        // Use object pooling if available
        if (_isPooled)
        {
            MarkForPooling();
        }
        else
        {
            QueueFree();
        }
    }

    public void ResetForPool()
    {
        _hasBeenLaunched = false;
        _almostStoppedEmitted = false;
        _lastCollisionCount = 0;
        LinearVelocity = Vector2.Zero;
        AngularVelocity = 0f;
        Rotation = 0f;
        Freeze = true;
    }

    public void MarkForPooling()
    {
        SetMeta("can_be_pooled", true);
    }
}
