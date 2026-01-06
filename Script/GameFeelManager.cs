using Godot;
using System;
using System.Threading.Tasks;

/// <summary>
/// Central manager for all game feel polish including screen shake,
/// particle effects, animations, and audio feedback on game events.
/// AUTOMATED: This is an autoload singleton - do not create instances manually.
/// </summary>
public partial class GameFeelManager : Node
{
    public static GameFeelManager Instance { get; private set; } = null!;

    [ExportGroup("General Settings", "General")]
    [Export] private bool _enableScreenShake = true;
    [Export] private bool _enableParticles = true;
    [Export] private bool _enableSlowMotion = false;

    [ExportGroup("Slingshot Feel", "Slingshot")]
    [Export] private float _slingshotChargeDuration = 0.3f;
    [Export] private bool _showTrajectory = true;

    [ExportGroup("Impact Feel", "Impact")]
    [Export] private float _heavyImpactThreshold = 500.0f;
    [Export] private bool _slowMotionOnImpact = false;
    [Export] private float _slowMotionDuration = 0.2f;
    [Export] private float _slowMotionScale = 0.3f;

    private float _originalTimeScale = 1.0f;

    public override void _Ready()
    {
        Instance = this;
        ConnectGameEvents();
    }

    private void ConnectGameEvents()
    {
        if (SignalManager.Instance == null)
            return;

        // Connect to game events for automatic feedback
        SignalManager.Instance.OnCupDestroyed += OnCupDestroyed;
        SignalManager.Instance.OnPropDestroyed += OnPropDestroyed;
        SignalManager.Instance.OnAttemptMade += OnAttemptMade;
    }

    public override void _ExitTree()
    {
        if (SignalManager.Instance == null)
            return;

        SignalManager.Instance.OnCupDestroyed -= OnCupDestroyed;
        SignalManager.Instance.OnPropDestroyed -= OnPropDestroyed;
        SignalManager.Instance.OnAttemptMade -= OnAttemptMade;
    }

    #region Slingshot Feedback

    /// <summary>
    /// Called when slingshot is being dragged
    /// </summary>
    public void OnSlingshotDrag(float tension)
    {
        // Optional: Add slight camera shake based on tension
        if (_enableScreenShake && tension > 0.8f)
        {
            EffectsManager.Instance?.ShakeScreen(1.0f, 0.1f);
        }
    }

    /// <summary>
    /// Called when projectile is launched
    /// </summary>
    public void OnProjectileLaunched()
    {
        // Audio is handled by AudioManager
        // Camera recoil
        if (_enableScreenShake)
        {
            EffectsManager.Instance?.ShakeScreen(3.0f, 0.2f);
        }
    }

    /// <summary>
    /// Called when projectile is launched with slingshot type-specific effects
    /// </summary>
    public void OnSlingshotLaunched(SlingshotType type, Vector2 position)
    {
        if (!_enableParticles) return;

        switch (type)
        {
            case SlingshotType.Catapult:
                // Classic confetti burst
                EffectsManager.Instance?.SpawnConfetti(position);
                EffectsManager.Instance?.SpawnDust(position + new Vector2(0, 20));
                break;

            case SlingshotType.GiantHand:
                // Sparkle explosion
                EffectsManager.Instance?.SpawnSparkle(position);
                EffectsManager.Instance?.SpawnSparkle(position + new Vector2(30, 0));
                EffectsManager.Instance?.SpawnSparkle(position + new Vector2(-30, 0));
                EffectsManager.Instance?.SpawnSparkle(position + new Vector2(0, 30));
                break;

            case SlingshotType.Trebuchet:
                // Dust cloud with some particles
                EffectsManager.Instance?.SpawnDust(position);
                EffectsManager.Instance?.SpawnDust(position + new Vector2(20, 10));
                EffectsManager.Instance?.SpawnDust(position + new Vector2(-20, 10));
                break;

            case SlingshotType.Spring:
                // Bouncy particles - mix of sparkles and dust
                EffectsManager.Instance?.SpawnSparkle(position);
                EffectsManager.Instance?.SpawnDust(position + new Vector2(0, 15));
                break;
        }
    }

    #endregion

    #region Impact Feedback

    /// <summary>
    /// Called on projectile impact with any object
    /// </summary>
    public void OnImpact(Node2D impactNode, float impactForce)
    {
        // Particle effects
        if (_enableParticles)
        {
            EffectsManager.Instance?.SpawnDust(impactNode.GlobalPosition);
        }

        // Screen shake based on impact force
        if (_enableScreenShake)
        {
            float shakeIntensity = Mathf.Clamp(impactForce / 100.0f, 0, 15.0f);
            float shakeDuration = Mathf.Clamp(impactForce / 1000.0f, 0.1f, 0.5f);

            if (shakeIntensity > 2.0f)
            {
                EffectsManager.Instance?.ShakeScreen(shakeIntensity, shakeDuration);
            }
        }

        // Haptic feedback
        if (HapticFeedbackManager.Instance != null)
        {
            if (impactForce > 800)
                HapticFeedbackManager.Instance.HeavyImpact();
            else if (impactForce > 400)
                HapticFeedbackManager.Instance.MediumImpact();
            else
                HapticFeedbackManager.Instance.LightImpact();
        }

        // Slow motion on heavy impacts
        if (_slowMotionOnImpact && impactForce > _heavyImpactThreshold)
        {
            TriggerSlowMotion();
        }
    }

    /// <summary>
    /// Called on heavy collision
    /// </summary>
    public void OnHeavyCollision(Node2D collider)
    {
        if (_enableParticles)
        {
            EffectsManager.Instance?.SpawnExplosion(collider.GlobalPosition);
        }

        if (_enableScreenShake)
        {
            EffectsManager.Instance?.ShakeScreenIntense();
        }
    }

    #endregion

    #region Destruction Feedback

    private void OnCupDestroyed()
    {
        // Cup destruction is a key event
        if (_enableParticles)
        {
            // Find where to spawn particles - would need cup position
            // For now, screen center as fallback
            EffectsManager.Instance?.SpawnExplosion(GetViewport().GetVisibleRect().Center);
        }

        if (_enableScreenShake)
        {
            EffectsManager.Instance?.ShakeScreen(8.0f, 0.3f);
        }
    }

    private void OnPropDestroyed(Node prop, int scoreValue)
    {
        if (_enableParticles && prop is Node2D node2d)
        {
            EffectsManager.Instance?.SpawnSparkle(node2d.GlobalPosition);
        }

        if (_enableScreenShake && scoreValue > 10)
        {
            EffectsManager.Instance?.ShakeScreen(5.0f, 0.25f);
        }
    }

    private void OnAttemptMade()
    {
        // Slight camera nudge on attempt
        if (_enableScreenShake)
        {
            EffectsManager.Instance?.ShakeScreen(2.0f, 0.15f);
        }
    }

    #endregion

    #region Level Completion Feedback

    /// <summary>
    /// Celebratory effects for level completion
    /// </summary>
    public void OnLevelComplete(int starsEarned)
    {
        // Confetti explosion
        if (_enableParticles)
        {
            var center = GetViewport().GetVisibleRect().Center;
            EffectsManager.Instance?.SpawnConfetti(center);

            // Additional confetti bursts at edges
            var rect = GetViewport().GetVisibleRect();
            EffectsManager.Instance?.SpawnConfetti(rect.Position + new Vector2(rect.Size.X * 0.2f, rect.Size.Y * 0.3f));
            EffectsManager.Instance?.SpawnConfetti(rect.Position + new Vector2(rect.Size.X * 0.8f, rect.Size.Y * 0.3f));
        }

        // Intense screen shake
        if (_enableScreenShake)
        {
            EffectsManager.Instance?.ShakeScreenIntense();
        }

        // Haptic feedback
        HapticFeedbackManager.Instance?.LevelComplete();
    }

    /// <summary>
    /// Effects for star rating display
    /// </summary>
    public async Task PlayStarRevealSequenceAsync(int starsEarned)
    {
        for (int i = 0; i < starsEarned; i++)
        {
            // Delay between stars
            await ToSignal(GetTree().CreateTimer(0.3), SceneTreeTimer.SignalName.Timeout);

            // Particle burst at star position
            if (_enableParticles)
            {
                var rect = GetViewport().GetVisibleRect();
                float starX = rect.Position.X + rect.Size.X * 0.4f + (i * rect.Size.X * 0.2f);
                var starPos = new Vector2(starX, rect.Position.Y + rect.Size.Y * 0.35f);
                EffectsManager.Instance?.SpawnSparkle(starPos);
            }

            if (_enableScreenShake)
            {
                EffectsManager.Instance?.ShakeScreen(3.0f, 0.2f);
            }
        }
    }

    #endregion

    #region Failure Feedback

    /// <summary>
    /// Effects for level failure
    /// </summary>
    public void OnLevelFailed()
    {
        // Subtle screen shake
        if (_enableScreenShake)
        {
            EffectsManager.Instance?.ShakeScreen(4.0f, 0.3f);
        }
    }

    #endregion

    #region Slow Motion

    private void TriggerSlowMotion()
    {
        if (!_enableSlowMotion)
            return;

        _originalTimeScale = Engine.TimeScale;
        Engine.TimeScale = _slowMotionScale;

        // Create tween to restore time scale
        var tween = CreateTween();
        if (tween != null)
        {
            tween.TweenInterval(_slowMotionDuration);
            tween.TweenCallback(Callable.From(() => RestoreTimeScale()));
        }
    }

    private void RestoreTimeScale()
    {
        Engine.TimeScale = _originalTimeScale;
    }

    #endregion

    #region UI Feedback

    /// <summary>
    /// Feedback for button interactions
    /// </summary>
    public void OnButtonPress(Control button)
    {
        if (_enableScreenShake)
        {
            EffectsManager.Instance?.ShakeScreen(1.0f, 0.1f);
        }

        HapticFeedbackManager.Instance?.ButtonTap();
    }

    /// <summary>
    /// Feedback for score popup
    /// </summary>
    public void OnScorePopup(Vector2 position)
    {
        if (_enableParticles)
        {
            EffectsManager.Instance?.SpawnSparkle(position);
        }
    }

    #endregion

    #region Door Unlock Feedback

    /// <summary>
    /// Effects when exit door unlocks
    /// </summary>
    public void OnDoorUnlocked(Vector2 doorPosition)
    {
        if (_enableParticles)
        {
            EffectsManager.Instance?.SpawnSparkle(doorPosition);
        }

        if (_enableScreenShake)
        {
            EffectsManager.Instance?.ShakeScreen(5.0f, 0.3f);
        }
    }

    #endregion

    #region StickClone Feedback

    /// <summary>
    /// Effects when StickClone spawns
    /// </summary>
    public void OnStickCloneSpawn(Vector2 position)
    {
        if (_enableParticles)
        {
            EffectsManager.Instance?.SpawnDust(position);
        }

        if (_enableScreenShake)
        {
            EffectsManager.Instance?.ShakeScreen(2.0f, 0.2f);
        }
    }

    /// <summary>
    /// Effects when StickClone takes damage
    /// </summary>
    public void OnStickCloneDamaged(Vector2 position)
    {
        if (_enableParticles)
        {
            EffectsManager.Instance?.SpawnSparkle(position);
        }
    }

    #endregion
}
