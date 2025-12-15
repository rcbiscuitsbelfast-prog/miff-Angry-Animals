using System.Threading.Tasks;
using Godot;

public partial class Globals : Node
{
    public static Globals Instance { get; private set; } = null!;

    private const float DefaultFadeSeconds = 0.25f;

    private CanvasLayer? _fadeLayer;
    private ColorRect? _fadeRect;
    private bool _isTransitioning;

    public AudioStreamPlayer MusicPlayer { get; private set; } = null!;

    public override void _Ready()
    {
        Instance = this;
        ProcessMode = ProcessModeEnum.Always;

        EnsureAudioBuses();
        SetupMusicPlayer();
        EnsureFadeOverlay();
    }

    private void EnsureAudioBuses()
    {
        EnsureBus("Music", "Master");
        EnsureBus("SFX", "Master");
    }

    private static void EnsureBus(string busName, string sendTo)
    {
        int busIndex = AudioServer.GetBusIndex(busName);
        if (busIndex == -1)
        {
            AudioServer.AddBus();
            busIndex = AudioServer.BusCount - 1;
            AudioServer.SetBusName(busIndex, busName);
            AudioServer.SetBusSend(busIndex, sendTo);
        }
    }

    private void SetupMusicPlayer()
    {
        MusicPlayer = new AudioStreamPlayer
        {
            Name = "MusicPlayer",
            Bus = "Music",
            Autoplay = true
        };
        AddChild(MusicPlayer);
    }

    private void EnsureFadeOverlay()
    {
        if (_fadeLayer != null && _fadeRect != null)
            return;

        _fadeLayer = new CanvasLayer { Name = "FadeLayer" };
        _fadeRect = new ColorRect
        {
            Name = "FadeRect",
            Color = new Color(0, 0, 0, 0),
            MouseFilter = Control.MouseFilterEnum.Ignore,
            AnchorLeft = 0,
            AnchorTop = 0,
            AnchorRight = 1,
            AnchorBottom = 1,
            OffsetLeft = 0,
            OffsetTop = 0,
            OffsetRight = 0,
            OffsetBottom = 0
        };

        _fadeLayer.AddChild(_fadeRect);
        AddChild(_fadeLayer);
    }

    public static void GotoScene(string scenePath, bool useFade = true, float fadeSeconds = DefaultFadeSeconds)
    {
        Instance.GotoSceneInternal(scenePath, useFade, fadeSeconds);
    }

    private async void GotoSceneInternal(string scenePath, bool useFade, float fadeSeconds)
    {
        if (_isTransitioning)
            return;

        _isTransitioning = true;
        EnsureFadeOverlay();

        try
        {
            if (useFade)
                await FadeTo(1f, fadeSeconds);

            var err = GetTree().ChangeSceneToFile(scenePath);
            if (err != Error.Ok)
                GD.PushWarning($"Globals.GotoScene failed: {scenePath} ({err})");

            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

            if (useFade)
                await FadeTo(0f, fadeSeconds);
        }
        finally
        {
            _isTransitioning = false;
        }
    }

    private Task FadeTo(float targetAlpha, float fadeSeconds)
    {
        if (_fadeRect == null)
            return Task.CompletedTask;

        var from = _fadeRect.Color;
        var to = new Color(from.R, from.G, from.B, targetAlpha);

        var tcs = new TaskCompletionSource<bool>();
        var tween = CreateTween();
        tween.SetPauseMode(Tween.TweenPauseMode.Process);
        tween.TweenProperty(_fadeRect, "color", to, fadeSeconds);
        tween.Finished += () => tcs.SetResult(true);
        return tcs.Task;
    }

    public static void SetMusic(AudioStream? stream, bool autoplay = true)
    {
        Instance.MusicPlayer.Stream = stream;
        if (autoplay && stream != null)
            Instance.MusicPlayer.Play();
    }

    public static void StopMusic() => Instance.MusicPlayer.Stop();
}
