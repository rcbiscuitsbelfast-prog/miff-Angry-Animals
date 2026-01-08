using System;
using System.Threading.Tasks;
using Godot;

/// <summary>
/// Centralized transition manager for handling all screen fades, crossfades, and UI animations.
/// Provides consistent, configurable transition effects across the game.
/// Integrates with GameSettingsManager for inspector-tweakable timing and colors.
/// </summary>
public partial class TransitionManager : CanvasLayer
{
    public static TransitionManager Instance { get; private set; } = null!;

    [Signal] public delegate void FadeOutStartedEventHandler();
    [Signal] public delegate void FadeInStartedEventHandler();
    [Signal] public delegate void TransitionCompletedEventHandler(string transitionType);

    [Header("🎬 Transition Configuration")]
    [Tooltip("ColorRect node for fade effects (auto-created if null).")]
    [Export] public ColorRect? FadeRect { get; private set; }
    
    [Tooltip("Layer order for fade effects (higher = on top).")]
    [Export] public int FadeLayer { get; set; } = 100;

    private ColorRect? _activeFadeRect;
    private bool _isTransitioning = false;
    private GameSettingsManager? _settings;

    public override void _Ready()
    {
        Instance = this;
        ProcessMode = ProcessModeEnum.Always;
        
        _settings = GameSettingsManager.Instance;
        
        // Auto-create fade rect if not assigned
        if (FadeRect == null)
        {
            CreateFadeRect();
        }
        
        GD.Print("TransitionManager initialized");
    }

    private void CreateFadeRect()
    {
        _activeFadeRect = new ColorRect();
        _activeFadeRect.Name = "TransitionFadeRect";
        _activeFadeRect.AnchorLeft = 0;
        _activeFadeRect.AnchorTop = 0;
        _activeFadeRect.AnchorRight = 1;
        _activeFadeRect.AnchorBottom = 1;
        _activeFadeRect.OffsetLeft = 0;
        _activeFadeRect.OffsetTop = 0;
        _activeFadeRect.OffsetRight = 0;
        _activeFadeRect.OffsetBottom = 0;
        _activeFadeRect.Color = Colors.Black;
        _activeFadeRect.Modulate = new Color(1, 1, 1, 0); // Transparent
        _activeFadeRect.ZIndex = FadeLayer;
        _activeFadeRect.Visible = false;
        
        AddChild(_activeFadeRect);
        FadeRect = _activeFadeRect;
    }

    #region Fade Effects

    /// <summary>
    /// Fades to a solid color (usually black) over the specified duration.
    /// </summary>
    public async Task FadeOut(float? duration = null, Color? color = null)
    {
        if (_isTransitioning || FadeRect == null)
            return;

        _isTransitioning = true;
        EmitSignal(SignalName.FadeOutStarted);

        float fadeDuration = duration ?? _settings?.LevelCompleteFadeDuration ?? 1.0f;
        Color fadeColor = color ?? _settings?.LevelCompleteFadeColor ?? Colors.Black;

        FadeRect.Visible = true;
        FadeRect.Color = fadeColor;
        FadeRect.Modulate = new Color(1, 1, 1, 0); // Start transparent

        // Create tween for fade animation
        var tween = CreateTween();
        if (tween != null)
        {
            tween.TweenProperty(FadeRect, "modulate:a", 1.0, fadeDuration);
            await ToSignal(tween, Tween.SignalName.Finished);
        }

        GD.Print($"FadeOut completed: {fadeDuration}s");
        _isTransitioning = false;
    }

    /// <summary>
    /// Fades from solid color to transparent over the specified duration.
    /// </summary>
    public async Task FadeIn(float? duration = null, Color? color = null)
    {
        if (_isTransitioning || FadeRect == null)
            return;

        _isTransitioning = true;
        EmitSignal(SignalName.FadeInStarted);

        float fadeDuration = duration ?? _settings?.LevelCompleteFadeDuration ?? 1.0f;
        Color fadeColor = color ?? _settings?.LevelCompleteFadeColor ?? Colors.Black;

        FadeRect.Visible = true;
        FadeRect.Color = fadeColor;
        FadeRect.Modulate = new Color(1, 1, 1, 1); // Start opaque

        // Create tween for fade animation
        var tween = CreateTween();
        if (tween != null)
        {
            tween.TweenProperty(FadeRect, "modulate:a", 0.0, fadeDuration);
            await ToSignal(tween, Tween.SignalName.Finished);
            
            FadeRect.Visible = false; // Hide when fully transparent
        }

        GD.Print($"FadeIn completed: {fadeDuration}s");
        _isTransitioning = false;
    }

    /// <summary>
    /// Complete fade out then fade in sequence.
    /// </summary>
    public async Task FadeOutIn(float? duration = null, Color? color = null)
    {
        await FadeOut(duration, color);
        await FadeIn(duration, color);
    }

    /// <summary>
    /// Fades to a color, waits, then fades back to transparent.
    /// Useful for attention-grabbing effects.
    /// </summary>
    public async Task Flash(Color? color = null, float fadeDuration = 0.3f, float holdDuration = 0.1f)
    {
        await FadeOut(fadeDuration, color);
        
        if (holdDuration > 0)
        {
            await Task.Delay((int)(holdDuration * 1000));
        }
        
        await FadeIn(fadeDuration, color);
    }

    #endregion

    #region Scene Transitions

    /// <summary>
    /// Crossfades from current scene to target scene using fade effects.
    /// </summary>
    public async Task CrossFadeToScene(string scenePath, float? duration = null)
    {
        if (_isTransitioning)
            return;

        _isTransitioning = true;
        GD.Print($"Crossfading to scene: {scenePath}");

        // Fade out
        await FadeOut(duration);
        
        // Change scene while screen is black
        var tree = GetTree();
        var error = tree.ChangeSceneToFile(scenePath);
        if (error != Error.Ok)
        {
            GD.PushError($"Failed to change scene to {scenePath}: {error}");
            _isTransitioning = false;
            return;
        }
        
        // Wait for scene to load
        await Task.Yield();
        await Task.Delay(100); // Small delay for scene load
        
        // Fade in
        await FadeIn(duration);
        
        EmitSignal(SignalName.TransitionCompleted, "cross_fade");
        _isTransitioning = false;
    }

    /// <summary>
    /// Changes scene with a quick flash effect.
    /// </summary>
    public async Task FlashToScene(string scenePath, Color? color = null)
    {
        await Flash(color);
        
        var error = GetTree().ChangeSceneToFile(scenePath);
        if (error != Error.Ok)
        {
            GD.PushError($"Failed to change scene to {scenePath}: {error}");
        }
    }

    #endregion

    #region UI Transitions

    /// <summary>
    /// Animates a control panel in with scaling and fade effects.
    /// </summary>
    public async Task AnimatePanelIn(Control panel, float? duration = null)
    {
        if (panel == null)
            return;

        float animDuration = duration ?? _settings?.MenuTransitionSpeed ?? 0.3f;
        
        panel.Visible = true;
        panel.Modulate = new Color(1, 1, 1, 0);
        panel.Scale = new Vector2(0.8f, 0.8f);

        var tween = CreateTween();
        if (tween != null)
        {
            tween.TweenProperty(panel, "modulate:a", 1.0, animDuration);
            tween.TweenProperty(panel, "scale", Vector2.One, animDuration).SetTrans(Tween.TransitionType.BackOut);
            await ToSignal(tween, Tween.SignalName.Finished);
        }
    }

    /// <summary>
    /// Animates a control panel out with scaling and fade effects.
    /// </summary>
    public async Task AnimatePanelOut(Control panel, float? duration = null)
    {
        if (panel == null)
            return;

        float animDuration = duration ?? _settings?.MenuTransitionSpeed ?? 0.3f;

        var tween = CreateTween();
        if (tween != null)
        {
            tween.TweenProperty(panel, "modulate:a", 0.0, animDuration);
            tween.TweenProperty(panel, "scale", new Vector2(0.8f, 0.8f), animDuration).SetTrans(Tween.TransitionType.BackIn);
            await ToSignal(tween, Tween.SignalName.Finished);
            
            panel.Visible = false;
        }
    }

    /// <summary>
    /// Bounces a UI element to get attention.
    /// </summary>
    public async Task Bounce(Control element, float intensity = 1.3f, float duration = 0.6f)
    {
        if (element == null)
            return;

        var tween = CreateTween();
        if (tween != null)
        {
            Vector2 originalScale = element.Scale;
            tween.TweenProperty(element, "scale", originalScale * intensity, duration * 0.3f).SetTrans(Tween.TransitionType.Bounce);
            tween.TweenProperty(element, "scale", originalScale, duration * 0.7f).SetTrans(Tween.TransitionType.Bounce);
            await ToSignal(tween, Tween.SignalName.Finished);
        }
    }

    /// <summary>
    /// Slides a UI element in from the specified direction.
    /// </summary>
    public async Task SlideIn(Control element, Vector2 direction, float? duration = null)
    {
        if (element == null)
            return;

        float animDuration = duration ?? _settings?.MenuTransitionSpeed ?? 0.3f;
        
        Vector2 startPos = element.Position + (direction * 100);
        Vector2 endPos = element.Position;
        
        element.Position = startPos;
        element.Visible = true;

        var tween = CreateTween();
        if (tween != null)
        {
            tween.TweenProperty(element, "position", endPos, animDuration).SetTrans(Tween.TransitionType.CubicOut);
            await ToSignal(tween, Tween.SignalName.Finished);
        }
    }

    #endregion

    #region Screen Shake

    /// <summary>
    /// Creates a screen shake effect on the specified node.
    /// </summary>
    public void ShakeScreen(Node2D target, float intensity = 10.0f, float duration = 0.5f)
    {
        if (target == null)
            return;

        // Get intensity from settings if available
        float shakeIntensity = intensity;
        if (_settings != null)
        {
            shakeIntensity *= _settings.ScreenShakeIntensity;
        }

        var originalPosition = target.Position;
        var shakeTween = CreateTween();
        
        if (shakeTween != null)
        {
            // Random shake pattern
            float elapsed = 0;
            while (elapsed < duration)
            {
                float timeStep = 0.1f;
                Vector2 randomOffset = new Vector2(
                    (float)(RandomNumberGenerator.RandfRange(-shakeIntensity, shakeIntensity)),
                    (float)(RandomNumberGenerator.RandfRange(-shakeIntensity, shakeIntensity))
                );
                
                shakeTween.TweenProperty(target, "position", originalPosition + randomOffset, timeStep);
                elapsed += timeStep;
            }
            
            shakeTween.TweenProperty(target, "position", originalPosition, 0.1f);
        }
    }

    /// <summary>
    /// Quick shake effect for impacts and collisions.
    /// </summary>
    public void QuickShake(Node2D target, float intensity = 5.0f)
    {
        ShakeScreen(target, intensity, 0.2f);
    }

    #endregion

    #region Utility Methods

    /// <summary>
    /// Checks if any transition is currently in progress.
    /// </summary>
    public bool IsTransitioning()
    {
        return _isTransitioning;
    }

    /// <summary>
    /// Cancels all active transitions (use with caution).
    /// </summary>
    public void CancelAllTransitions()
    {
        // Stop all tweens
        var tweens = GetTree().GetProcessTweens();
        foreach (var tween in tweens)
        {
            tween.Kill();
        }
        
        _isTransitioning = false;
        
        if (FadeRect != null)
        {
            FadeRect.Visible = false;
            FadeRect.Modulate = new Color(1, 1, 1, 0);
        }
        
        GD.Print("All transitions cancelled");
    }

    /// <summary>
    /// Sets the fade color and updates settings.
    /// </summary>
    public void SetFadeColor(Color color)
    {
        if (_settings != null)
        {
            _settings.LevelCompleteFadeColor = color;
        }
        
        if (FadeRect != null)
        {
            FadeRect.Color = color;
        }
    }

    /// <summary>
    /// Sets the default fade duration and updates settings.
    /// </summary>
    public void SetFadeDuration(float duration)
    {
        if (_settings != null)
        {
            _settings.LevelCompleteFadeDuration = duration;
        }
    }

    #endregion

    #region Preset Transitions

    /// <summary>
    /// Standard level complete transition with fade and scene change.
    /// </summary>
    public async Task LevelCompleteTransition(string nextScenePath = "")
    {
        float duration = _settings?.LevelCompleteFadeDuration ?? 1.0f;
        
        await FadeOut(duration);
        
        if (!string.IsNullOrEmpty(nextScenePath))
        {
            var error = GetTree().ChangeSceneToFile(nextScenePath);
            if (error != Error.Ok)
            {
                GD.PushError($"Failed to change scene to {nextScenePath}: {error}");
            }
        }
        
        await Task.Delay(200); // Brief pause at black screen
        
        await FadeIn(duration);
        
        EmitSignal(SignalName.TransitionCompleted, "level_complete");
    }

    /// <summary>
    /// Quick menu transition for buttons and UI interactions.
    /// </summary>
    public async Task MenuTransition()
    {
        float duration = _settings?.MenuTransitionSpeed ?? 0.3f;
        await Flash(_settings?.LevelCompleteFadeColor ?? Colors.Black, duration * 0.5f, duration * 0.1f);
    }

    /// <summary>
    /// Gentle fade for pause/resume transitions.
    /// </summary>
    public async Task PauseTransition(bool pause)
    {
        float duration = _settings?.MenuTransitionSpeed ?? 0.3f;
        Color color = new Color(0, 0, 0, pause ? 0.5f : 0.0f);
        
        if (pause)
            await FadeOut(duration, color);
        else
            await FadeIn(duration, color);
    }

    #endregion
}
