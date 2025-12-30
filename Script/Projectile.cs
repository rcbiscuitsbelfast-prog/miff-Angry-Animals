using Godot;

/// <summary>
/// Base class for all projectiles launched from the slingshot.
/// Handles physics interactions, collision detection, sound effects, and lifecycle management.
/// Supports impact feedback and cross-platform audio.
/// </summary>
public partial class Projectile : RigidBody2D
{
    [Signal] public delegate void AlmostStoppedEventHandler();

    [Export] private AudioStreamPlayer2D? _kickWoodSound;
    [Export] private VisibleOnScreenNotifier2D _onScreenNotifier;
    [Export] private bool _enableImpactEffects = true;
    [Export] private float _impactSoundThreshold = 50f;

    private const float STOPPED_THRESHOLD = 0.1f;
    private bool _hasBeenLaunched = false;
    private bool _almostStoppedEmitted = false;
    private int _lastCollisionCount = 0;
    private Vector2 _lastVelocity;
    private bool _hasCollided = false;

    public override void _Ready()
    {
        ConnectSignals();
        _lastVelocity = LinearVelocity;
    }

    public override void _PhysicsProcess(double delta)
    {
        CheckIfAlmostStopped();
        CheckForImpact();
    }

    private void ConnectSignals()
    {
        if (_onScreenNotifier != null)
        {
            _onScreenNotifier.ScreenExited += OnScreenExited;
        }

        SleepingStateChanged += OnSleepingStateChanged;
    }

    /// <summary>
    /// Launches the projectile with the given impulse.
    /// </summary>
    /// <param name="impulse">The physics impulse vector to apply.</param>
    public void Launch(Vector2 impulse)
    {
        _hasBeenLaunched = true;
        Freeze = false;
        ApplyCentralImpulse(impulse);
        _lastVelocity = impulse;

        // Play launch sound
        PlayLaunchSound();
    }

    private void PlayLaunchSound()
    {
        var audioManager = GetNodeOrNull<AudioManager>("/root/AudioManager");
        audioManager?.PlaySlingshotSoundSfx();
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

    /// <summary>
    /// Detects significant impacts for audio/visual feedback.
    /// </summary>
    private void CheckForImpact()
    {
        if (!_enableImpactEffects || !_hasBeenLaunched) return;

        // Detect sudden velocity changes (impacts)
        float velocityChange = (LinearVelocity - _lastVelocity).Length();
        if (velocityChange > _impactSoundThreshold && _lastCollisionCount == 0)
        {
            // Just experienced a significant impact
            PlayImpactSound();
        }

        _lastVelocity = LinearVelocity;
    }

    private void PlayImpactSound()
    {
        if (_kickWoodSound != null && !_kickWoodSound.Playing)
        {
            _kickWoodSound.Play();
        }
        else
        {
            // Fallback to AudioManager
            var audioManager = GetNodeOrNull<AudioManager>("/root/AudioManager");
            audioManager?.PlayDestructionSoundSfx();
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
        QueueFree();
    }

    /// <summary>
    /// Enables or disables impact effects.
    /// </summary>
    public void SetImpactEffectsEnabled(bool enabled) => _enableImpactEffects = enabled;
}
