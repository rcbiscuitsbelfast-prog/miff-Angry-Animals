using Godot;

/// <summary>
/// Provides haptic feedback (vibration) for mobile devices.
/// Automatically detects platform and uses appropriate API.
/// Safe to call on any platform - does nothing if haptics unavailable.
/// </summary>
public partial class HapticFeedbackManager : Node
{
    public static HapticFeedbackManager Instance { get; private set; } = null!;

    [ExportGroup("General Settings", "General")]
    [Export] private bool _enableHaptics = true;
    [Export] private float _globalIntensity = 1.0f;

    [ExportGroup("Impact Feedback", "Impact")]
    [Export] private float _lightImpactDuration = 0.03f;
    [Export] private float _mediumImpactDuration = 0.05f;
    [Export] private float _heavyImpactDuration = 0.1f;

    [ExportGroup("UI Feedback", "UI")]
    [Export] private float _buttonTapDuration = 0.02f;
    [Export] private float _selectionChangeDuration = 0.015f;

    [ExportGroup("Success/Failure Feedback", "Result")]
    [Export] private float _successDuration = 0.15f;
    [Export] private float _failureDuration = 0.1f;

    public override void _Ready()
    {
        Instance = this;
    }

    /// <summary>
    /// Light impact feedback (for small collisions, taps)
    /// </summary>
    public void LightImpact()
    {
        if (!_enableHaptics) return;
        Vibrate(_lightImpactDuration * _globalIntensity);
    }

    /// <summary>
    /// Medium impact feedback (for standard collisions, projectile hits)
    /// </summary>
    public void MediumImpact()
    {
        if (!_enableHaptics) return;
        Vibrate(_mediumImpactDuration * _globalIntensity);
    }

    /// <summary>
    /// Heavy impact feedback (for explosions, boss hits, major events)
    /// </summary>
    public void HeavyImpact()
    {
        if (!_enableHaptics) return;
        Vibrate(_heavyImpactDuration * _globalIntensity);
    }

    /// <summary>
    /// Feedback for button taps
    /// </summary>
    public void ButtonTap()
    {
        if (!_enableHaptics) return;
        Vibrate(_buttonTapDuration * _globalIntensity);
    }

    /// <summary>
    /// Feedback for selection changes (scrolling, toggles)
    /// </summary>
    public void SelectionChange()
    {
        if (!_enableHaptics) return;
        Vibrate(_selectionChangeDuration * _globalIntensity);
    }

    /// <summary>
    /// Success feedback (level complete, achievements)
    /// </summary>
    public void Success()
    {
        if (!_enableHaptics) return;

        // Double vibration for success
        Vibrate(_successDuration * _globalIntensity);
        CreateTimer(0.05, () => Vibrate(_successDuration * _globalIntensity));
    }

    /// <summary>
    /// Failure feedback (level failed, errors)
    /// </summary>
    public void Failure()
    {
        if (!_enableHaptics) return;
        Vibrate(_failureDuration * _globalIntensity);
    }

    /// <summary>
    /// Pattern for level completion (three pulses)
    /// </summary>
    public void LevelComplete()
    {
        if (!_enableHaptics) return;

        Vibrate(0.05f * _globalIntensity);
        CreateTimer(0.1, () => Vibrate(0.05f * _globalIntensity));
        CreateTimer(0.2, () => Vibrate(0.1f * _globalIntensity));
    }

    /// <summary>
    /// Pattern for door unlock (two quick pulses)
    /// </summary>
    public void DoorUnlock()
    {
        if (!_enableHaptics) return;

        Vibrate(0.03f * _globalIntensity);
        CreateTimer(0.05, () => Vibrate(0.04f * _globalIntensity));
    }

    /// <summary>
    /// Pattern for projectile launch (ascending)
    /// </summary>
    public void ProjectileLaunch()
    {
        if (!_enableHaptics) return;

        Vibrate(0.02f * _globalIntensity);
        CreateTimer(0.03, () => Vibrate(0.03f * _globalIntensity));
        CreateTimer(0.06, () => Vibrate(0.04f * _globalIntensity));
    }

    private void Vibrate(float duration)
    {
        if (OS.GetName() == "Android")
        {
            AndroidVibrate(duration);
        }
        else if (OS.GetName() == "iOS")
        {
            iOSVibrate(duration);
        }
    }

    private void AndroidVibrate(float duration)
    {
        // Use Godot's built-in vibration support on Android
        int durationMs = (int)(duration * 1000);
        if (durationMs > 0)
        {
            GD.Print($"[Haptics] Android vibrate for {durationMs}ms");
            // Godot will call the native Android vibration API
            Input.VibrateHandheld(durationMs);
        }
    }

    private void iOSVibrate(float duration)
    {
        // iOS uses specific vibration patterns via UINotificationFeedbackGenerator
        // Godot's Input.VibrateHandshake() works on iOS
        int durationMs = (int)(duration * 1000);
        if (durationMs > 0)
        {
            GD.Print($"[Haptics] iOS vibrate for {durationMs}ms");
            Input.VibrateHandheld(durationMs);
        }
    }

    private void CreateTimer(float delay, System.Action callback)
    {
        var timer = new Timer
        {
            WaitTime = delay,
            OneShot = true,
            Autostart = true
        };
        timer.Timeout += () =>
        {
            callback?.Invoke();
            timer.QueueFree();
        };
        AddChild(timer);
    }
}
