using System.Threading.Tasks;
using Godot;

/// <summary>
/// A short, skippable meme interlude shown between X6 and X7 levels.
/// Variants are lightweight tween-based animations.
/// </summary>
public partial class SixToSevenMinigame : Control
{
    [Signal] public delegate void FinishedEventHandler();

    [Export] public float MinDurationSeconds { get; set; } = 3.0f;
    [Export] public float MaxDurationSeconds { get; set; } = 5.0f;

    private Label? _label;
    private ColorRect? _bg;

    public override void _Ready()
    {
        ProcessMode = ProcessModeEnum.Always;
        MouseFilter = MouseFilterEnum.Stop;

        EnsureUi();
        _ = RunAsync();
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event.IsActionPressed("ui_cancel"))
            EmitSignal(SignalName.Finished);
        else if (@event is InputEventMouseButton mb && mb.Pressed)
            EmitSignal(SignalName.Finished);
        else if (@event is InputEventScreenTouch touch && touch.Pressed)
            EmitSignal(SignalName.Finished);
    }

    private void EnsureUi()
    {
        AnchorLeft = 0;
        AnchorTop = 0;
        AnchorRight = 1;
        AnchorBottom = 1;

        _bg = new ColorRect
        {
            Name = "Background",
            Color = new Color(0, 0, 0, 0.85f),
            MouseFilter = MouseFilterEnum.Stop
        };
        _bg.SetAnchorsPreset(LayoutPreset.FullRect);
        AddChild(_bg);

        _label = new Label
        {
            Name = "MemeLabel",
            Text = "PERFECT!!",
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
        _label.SetAnchorsPreset(LayoutPreset.FullRect);
        _label.AddThemeFontSizeOverride("font_size", 96);
        _label.Modulate = Colors.White;
        AddChild(_label);
    }

    private async Task RunAsync()
    {
        float duration = (float)GD.RandRange(MinDurationSeconds, MaxDurationSeconds);

        var variant = (MemeVariant)GD.RandRange(0, (int)MemeVariant.Count - 1);
        await PlayVariantAsync(variant, duration);

        EmitSignal(SignalName.Finished);
    }

    private async Task PlayVariantAsync(MemeVariant variant, float duration)
    {
        if (_label == null)
            return;

        _label.Rotation = 0;
        _label.Scale = Vector2.One;
        _label.Position = Vector2.Zero;

        switch (variant)
        {
            case MemeVariant.YeetSpin:
                _label.Text = "YEET SPIN";
                await SpinAsync(duration);
                break;
            case MemeVariant.PerfectText:
                _label.Text = "PERFECT??";
                await BounceAsync(duration);
                break;
            case MemeVariant.MemeFlash:
                await FlashAsync(duration);
                break;
            case MemeVariant.ComboCounter:
                await CounterAsync(duration);
                break;
            case MemeVariant.VictoryDance:
                _label.Text = "VICTORY DANCE";
                await WiggleAsync(duration);
                break;
            case MemeVariant.AchievementUnlocked:
                _label.Text = "ACHIEVEMENT UNLOCKED";
                await PopAsync(duration);
                break;
            case MemeVariant.BrainExpansion:
                await BrainAsync(duration);
                break;
        }
    }

    private Task Delay(float seconds)
    {
        return ToSignal(GetTree().CreateTimer(seconds, true, false, false), SceneTreeTimer.SignalName.Timeout);
    }

    private async Task SpinAsync(float duration)
    {
        var tween = CreateTween();
        tween.SetPauseMode(Tween.TweenPauseMode.Process);
        tween.TweenProperty(_label, "rotation", Mathf.Pi * 8, duration);
        await Delay(duration);
    }

    private async Task BounceAsync(float duration)
    {
        var tween = CreateTween();
        tween.SetPauseMode(Tween.TweenPauseMode.Process);
        tween.TweenProperty(_label, "scale", Vector2.One * 1.35f, 0.25f).SetTrans(Tween.TransitionType.Bounce);
        tween.TweenProperty(_label, "scale", Vector2.One, 0.25f).SetTrans(Tween.TransitionType.Bounce);
        tween.SetLoops((int)Mathf.Max(1, duration / 0.5f));
        await Delay(duration);
    }

    private async Task FlashAsync(float duration)
    {
        string[] frames = { "SUCCESS KID", "DRAKE", "DISTRACTED BF", "EXPANDING BRAIN", "YEET FACE" };

        float t = 0f;
        int idx = 0;
        while (t < duration)
        {
            _label!.Text = frames[idx % frames.Length];
            _label.Modulate = new Color(GD.Randf(), GD.Randf(), GD.Randf(), 1f);
            idx++;
            await Delay(0.25f);
            t += 0.25f;
        }

        _label.Modulate = Colors.White;
    }

    private async Task CounterAsync(float duration)
    {
        float t = 0f;
        int n = 0;
        while (t < duration)
        {
            _label!.Text = $"COMBO x{n}";
            _label.Scale = Vector2.One * (1f + (n % 5) * 0.05f);
            n += GD.RandRange(1, 3);
            await Delay(0.2f);
            t += 0.2f;
        }

        _label.Scale = Vector2.One;
    }

    private async Task WiggleAsync(float duration)
    {
        var tween = CreateTween();
        tween.SetPauseMode(Tween.TweenPauseMode.Process);
        tween.TweenProperty(_label, "rotation", 0.2f, 0.1f);
        tween.TweenProperty(_label, "rotation", -0.2f, 0.1f);
        tween.SetLoops((int)Mathf.Max(1, duration / 0.2f));
        await Delay(duration);
        _label!.Rotation = 0;
    }

    private async Task PopAsync(float duration)
    {
        _label!.Scale = Vector2.Zero;
        var tween = CreateTween();
        tween.SetPauseMode(Tween.TweenPauseMode.Process);
        tween.TweenProperty(_label, "scale", Vector2.One, 0.35f).SetTrans(Tween.TransitionType.Bounce);
        await Delay(duration);
    }

    private async Task BrainAsync(float duration)
    {
        string[] stages = { "BRAIN", "BIG BRAIN", "GALAXY BRAIN", "YEET BRAIN", "EARTH YEETER" };
        float step = Mathf.Max(0.3f, duration / stages.Length);

        for (int i = 0; i < stages.Length; i++)
        {
            _label!.Text = stages[i];
            _label.Modulate = new Color(0.7f + i * 0.05f, 0.7f, 1f, 1f);
            _label.Scale = Vector2.One * (1f + i * 0.12f);
            await Delay(step);
        }

        _label.Modulate = Colors.White;
        _label.Scale = Vector2.One;
    }

    private enum MemeVariant
    {
        YeetSpin,
        PerfectText,
        MemeFlash,
        ComboCounter,
        VictoryDance,
        AchievementUnlocked,
        BrainExpansion,

        Count
    }
}
