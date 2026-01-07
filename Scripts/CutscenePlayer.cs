using System;
using System.Threading.Tasks;
using Godot;

/// <summary>
/// Scene-based cutscene player.
/// Loads a separate cutscene scene (.tscn) and plays it while the game is paused.
/// Designed to be used via StoryEventTrigger.
/// </summary>
public partial class CutscenePlayer : CanvasLayer
{
    [Export] public NodePath ContainerPath { get; set; } = NodePath("CutsceneContainer");
    [Export] public NodePath FadeRectPath { get; set; } = NodePath("FadeRect");

    private Node? _container;
    private ColorRect? _fadeRect;

    private bool _isSkipping;
    private bool _hasFinished;
    private int _returnRoomIndex = -1;

    public bool IsSkipping => _isSkipping;

    public override void _Ready()
    {
        ProcessMode = ProcessModeEnum.Always;

        _container = GetNodeOrNull<Node>(ContainerPath);
        _fadeRect = GetNodeOrNull<ColorRect>(FadeRectPath);

        _ = PlayQueuedCutsceneAsync();
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event.IsActionPressed("ui_cancel"))
            Skip();
        else if (@event is InputEventMouseButton mb && mb.Pressed)
            Skip();
        else if (@event is InputEventScreenTouch touch && touch.Pressed)
            Skip();
    }

    private async Task PlayQueuedCutsceneAsync()
    {
        if (StoryEventTrigger.Instance == null)
        {
            GD.PushWarning("CutscenePlayer: StoryEventTrigger not available.");
            GameManager.LoadMain();
            return;
        }

        var cutscenePath = StoryEventTrigger.Instance.PendingCutsceneScenePath;
        var roomIndex = StoryEventTrigger.Instance.PendingReturnRoomIndex;
        _returnRoomIndex = roomIndex;

        if (string.IsNullOrWhiteSpace(cutscenePath))
        {
            GD.PushWarning("CutscenePlayer: No pending cutscene path.");
            GameManager.StartRoom(roomIndex);
            return;
        }

        // Pause the game world while keeping this CanvasLayer processing.
        GetTree().Paused = true;

        await FadeToAsync(0f, 0f);
        await FadeToAsync(1f, 0.15f);

        var packed = ResourceLoader.Load<PackedScene>(cutscenePath);
        if (packed == null)
        {
            GD.PushWarning($"CutscenePlayer: Could not load cutscene: {cutscenePath}");
            await FinishAsync(roomIndex);
            return;
        }

        var cutsceneRoot = packed.Instantiate<Node>();
        cutsceneRoot.ProcessMode = ProcessModeEnum.Always;

        _container?.AddChild(cutsceneRoot);

        if (cutsceneRoot is ICutscene cutscene)
        {
            try
            {
                await cutscene.PlayAsync(this);
            }
            catch (Exception ex)
            {
                GD.PushWarning($"CutscenePlayer: cutscene threw: {ex.Message}");
            }
        }
        else
        {
            // Fallback: play an AnimationPlayer named "AnimationPlayer" if present.
            var animPlayer = cutsceneRoot.GetNodeOrNull<AnimationPlayer>("AnimationPlayer");
            if (animPlayer != null)
            {
                animPlayer.ProcessMode = ProcessModeEnum.Always;
                var animName = animPlayer.HasAnimation("Cutscene") ? "Cutscene" : animPlayer.GetAnimationList().Count > 0 ? animPlayer.GetAnimationList()[0] : "";

                if (!string.IsNullOrWhiteSpace(animName))
                {
                    animPlayer.Play(animName);
                    await ToSignal(animPlayer, AnimationPlayer.SignalName.AnimationFinished);
                }
                else
                {
                    await ToSignal(GetTree().CreateTimer(2.0f, true, false, false), SceneTreeTimer.SignalName.Timeout);
                }
            }
            else
            {
                await ToSignal(GetTree().CreateTimer(2.0f, true, false, false), SceneTreeTimer.SignalName.Timeout);
            }
        }

        await FinishAsync(roomIndex);
    }

    public void Skip()
    {
        if (_isSkipping)
            return;

        _isSkipping = true;

        // Clear any queued dialogue immediately.
        DialogueManager.Instance?.Clear();

        // Stop any cutscene nodes.
        if (_container != null)
        {
            foreach (Node child in _container.GetChildren())
                child.QueueFree();
        }

        if (!_hasFinished)
            CallDeferred(nameof(DeferredFinish));
    }

    private async void DeferredFinish()
    {
        if (_hasFinished)
            return;

        await FinishAsync(_returnRoomIndex);
    }

    public async Task FinishAsync(int roomIndex)
    {
        if (_hasFinished)
            return;

        _hasFinished = true;

        await FadeToAsync(0f, 0.15f);

        // Resume gameplay.
        GetTree().Paused = false;

        StoryEventTrigger.Instance?.ClearPending();

        // Start the intended room without re-triggering cutscenes.
        GameManager.Instance?.StartRoomInternalFromCutscene(roomIndex);

        QueueFree();
    }

    public Task FadeToAsync(float alpha, float seconds)
    {
        if (_fadeRect == null)
            return Task.CompletedTask;

        var tcs = new TaskCompletionSource<bool>();

        var from = _fadeRect.Color;
        var to = new Color(from.R, from.G, from.B, alpha);

        var tween = CreateTween();
        tween.SetPauseMode(Tween.TweenPauseMode.Process);
        tween.TweenProperty(_fadeRect, "color", to, seconds);
        tween.Finished += () => tcs.TrySetResult(true);

        return tcs.Task;
    }
}

/// <summary>
/// Optional interface for cutscene scenes that want to drive their own timeline.
/// </summary>
public interface ICutscene
{
    Task PlayAsync(CutscenePlayer player);
}
