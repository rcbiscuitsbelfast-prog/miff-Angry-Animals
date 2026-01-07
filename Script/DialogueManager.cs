using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;

/// <summary>
/// Queue-based multi-speaker dialogue UI for cutscenes and gameplay.
/// Designed to be used as an AutoLoad.
/// </summary>
public partial class DialogueManager : CanvasLayer
{
    public static DialogueManager Instance { get; private set; } = null!;

    [ExportGroup("Layout")]
    [Export] public Vector2 PanelSize { get; set; } = new Vector2(900, 160);
    [Export] public Vector2 PanelOffset { get; set; } = new Vector2(0, -40);

    [ExportGroup("Timing")]
    [Export] public float FadeSeconds { get; set; } = 0.3f;

    [ExportGroup("Portrait")]
    [Export] public Vector2 PortraitSize { get; set; } = new Vector2(128, 128);

    [ExportGroup("Input")]
    [Export] public bool ClickToAdvance { get; set; } = true;

    private readonly Queue<DialogueEntry> _queue = new();
    private bool _isPlaying;
    private bool _advanceRequested;

    private Panel? _panel;
    private TextureRect? _portrait;
    private Label? _speakerLabel;
    private RichTextLabel? _textLabel;

    public override void _Ready()
    {
        Instance = this;
        ProcessMode = ProcessModeEnum.Always;

        EnsureUi();
        HideImmediate();
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (!ClickToAdvance)
            return;

        if (!_isPlaying)
            return;

        if (@event is InputEventMouseButton mb && mb.Pressed)
            _advanceRequested = true;
        else if (@event.IsActionPressed("ui_accept"))
            _advanceRequested = true;
    }

    public void EnqueueDialogue(DialogueEntry entry)
    {
        _queue.Enqueue(entry);
        if (!_isPlaying)
            _ = PlayQueueAsync();
    }

    public void EnqueueDialogue(string speaker, string text, float duration = 2.0f, string portraitVariant = "")
    {
        EnqueueDialogue(new DialogueEntry
        {
            Speaker = speaker,
            Text = text,
            Duration = duration,
            PortraitVariant = portraitVariant
        });
    }

    public void Clear()
    {
        _queue.Clear();
        _advanceRequested = false;
        _isPlaying = false;
        HideImmediate();
    }

    private async Task PlayQueueAsync()
    {
        _isPlaying = true;

        while (_queue.Count > 0)
        {
            var entry = _queue.Dequeue();
            await ShowEntryAsync(entry);
        }

        await FadeOutAsync();
        _isPlaying = false;
    }

    private async Task ShowEntryAsync(DialogueEntry entry)
    {
        EnsureUi();

        if (_speakerLabel != null)
            _speakerLabel.Text = string.IsNullOrWhiteSpace(entry.Speaker) ? string.Empty : $"{entry.Speaker}:";

        if (_textLabel != null)
            _textLabel.Text = entry.Text ?? string.Empty;

        if (_portrait != null)
        {
            _portrait.Texture = ResolvePortraitTexture(entry.PortraitVariant);
            _portrait.Visible = _portrait.Texture != null;
        }

        _advanceRequested = false;
        await FadeInAsync();

        float duration = Mathf.Max(0.1f, entry.Duration);
        float t = 0f;

        while (t < duration && !_advanceRequested)
        {
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
            t += (float)GetProcessDeltaTime();
        }

        await FadeOutAsync();
    }

    private Texture2D? ResolvePortraitTexture(string portraitVariant)
    {
        if (string.IsNullOrWhiteSpace(portraitVariant))
            return null;

        var path = $"res://Assets/Portraits/{portraitVariant}.png";
        if (!ResourceLoader.Exists(path))
            return null;

        return ResourceLoader.Load<Texture2D>(path);
    }

    private void EnsureUi()
    {
        if (_panel != null)
            return;

        _panel = new Panel
        {
            Name = "DialoguePanel",
            Size = PanelSize,
            CustomMinimumSize = PanelSize,
            ProcessMode = ProcessModeEnum.Always
        };

        AddChild(_panel);

        _panel.AnchorLeft = 0.5f;
        _panel.AnchorRight = 0.5f;
        _panel.AnchorTop = 1f;
        _panel.AnchorBottom = 1f;
        _panel.OffsetLeft = -PanelSize.X / 2f + PanelOffset.X;
        _panel.OffsetRight = PanelSize.X / 2f + PanelOffset.X;
        _panel.OffsetTop = -PanelSize.Y + PanelOffset.Y;
        _panel.OffsetBottom = PanelOffset.Y;

        var style = new StyleBoxFlat
        {
            BgColor = new Color(0f, 0f, 0f, 0.75f),
            BorderColor = new Color(1f, 1f, 1f, 0.2f),
            BorderWidthBottom = 2,
            BorderWidthLeft = 2,
            BorderWidthRight = 2,
            BorderWidthTop = 2,
            CornerRadiusBottomLeft = 12,
            CornerRadiusBottomRight = 12,
            CornerRadiusTopLeft = 12,
            CornerRadiusTopRight = 12,
            ContentMarginLeft = 14,
            ContentMarginRight = 14,
            ContentMarginTop = 12,
            ContentMarginBottom = 12
        };
        _panel.AddThemeStyleboxOverride("panel", style);

        var row = new HBoxContainer
        {
            Name = "Row",
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
            SizeFlagsVertical = Control.SizeFlags.ExpandFill
        };
        _panel.AddChild(row);

        _portrait = new TextureRect
        {
            Name = "Portrait",
            CustomMinimumSize = PortraitSize,
            StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
            ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
            SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter
        };
        row.AddChild(_portrait);

        var textCol = new VBoxContainer
        {
            Name = "TextCol",
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
            SizeFlagsVertical = Control.SizeFlags.ExpandFill
        };
        row.AddChild(textCol);

        _speakerLabel = new Label
        {
            Name = "Speaker",
            Text = "",
            Modulate = Colors.Gold
        };
        _speakerLabel.AddThemeFontSizeOverride("font_size", 24);
        textCol.AddChild(_speakerLabel);

        _textLabel = new RichTextLabel
        {
            Name = "Text",
            BbcodeEnabled = false,
            FitContent = true,
            ScrollActive = false,
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
            SizeFlagsVertical = Control.SizeFlags.ExpandFill
        };
        _textLabel.AddThemeFontSizeOverride("normal_font_size", 24);
        textCol.AddChild(_textLabel);
    }

    private void HideImmediate()
    {
        if (_panel != null)
        {
            _panel.Visible = false;
            _panel.Modulate = new Color(1, 1, 1, 0);
        }
    }

    private Task FadeInAsync()
    {
        if (_panel == null)
            return Task.CompletedTask;

        _panel.Visible = true;
        var tcs = new TaskCompletionSource<bool>();

        var tween = CreateTween();
        tween.SetPauseMode(Tween.TweenPauseMode.Process);
        tween.TweenProperty(_panel, "modulate", new Color(1, 1, 1, 1), FadeSeconds).From(new Color(1, 1, 1, 0));
        tween.Finished += () => tcs.TrySetResult(true);

        return tcs.Task;
    }

    private Task FadeOutAsync()
    {
        if (_panel == null)
            return Task.CompletedTask;

        var tcs = new TaskCompletionSource<bool>();

        var tween = CreateTween();
        tween.SetPauseMode(Tween.TweenPauseMode.Process);
        tween.TweenProperty(_panel, "modulate", new Color(1, 1, 1, 0), FadeSeconds).From(_panel.Modulate);
        tween.Finished += () =>
        {
            if (_panel != null)
                _panel.Visible = false;
            tcs.TrySetResult(true);
        };

        return tcs.Task;
    }
}

/// <summary>
/// Dialogue data structure used by DialogueManager.
/// Marked as GlobalClass so it can be created/edited in the Inspector.
/// </summary>
[GlobalClass]
public partial class DialogueEntry : Resource
{
    [Export] public string Speaker { get; set; } = string.Empty;
    [Export(PropertyHint.MultilineText)] public string Text { get; set; } = string.Empty;
    [Export] public float Duration { get; set; } = 2.0f;
    [Export] public string PortraitVariant { get; set; } = string.Empty;
}
