using Godot;

public partial class GameManager : Node
{
    public static GameManager Instance { get; private set; } = null!;

    public enum GameState
    {
        Boot,
        MainMenu,
        InRoom,
        RoomComplete,
        Paused
    }

    public readonly record struct RoomInfo(string ScenePath, string Description, int TargetScore);

    /// <summary>
    /// Total number of levels included in the full game.
    /// </summary>
    public const int TotalLevels = 100;

    /// <summary>
    /// Number of levels available in the free tier.
    /// </summary>
    public const int FreeLevels = 20;

    public RoomInfo[] Rooms { get; } = CreateDefaultRooms();

    public GameState State { get; private set; } = GameState.Boot;
    public int CurrentRoomIndex { get; private set; } = -1;

    public string MainScenePath { get; set; } = "res://Scenes/Main/Main.tscn";

    private SignalManager? _signalManager;

    [Signal] public delegate void GameStateChangedEventHandler(GameState state);
    [Signal] public delegate void RoomStartedEventHandler(int roomIndex);
    [Signal] public delegate void RoomCompletedEventHandler(int roomIndex);

    public override void _Ready()
    {
        Instance = this;
        ProcessMode = ProcessModeEnum.Always;
        CallDeferred(nameof(DeferredInit));
    }

    private void DeferredInit()
    {
        State = GameState.MainMenu;
        EmitSignal(SignalName.GameStateChanged, State);

        _signalManager = GetNodeOrNull<SignalManager>("/root/SignalManager");
        if (_signalManager != null)
            _signalManager.OnLevelCompleted += OnLevelCompleted;
    }

    public override void _ExitTree()
    {
        if (_signalManager != null)
        {
            _signalManager.OnLevelCompleted -= OnLevelCompleted;
            _signalManager = null;
        }
    }

    private void OnLevelCompleted() => CompleteRoom();

    public static void LoadMain() => Instance.LoadMainInternal();

    private void LoadMainInternal()
    {
        CurrentRoomIndex = -1;
        State = GameState.MainMenu;
        EmitSignal(SignalName.GameStateChanged, State);
        Globals.GotoScene(MainScenePath);
    }

    public static void StartRoom(int roomIndex) => Instance.StartRoomInternal(roomIndex);

    public static void StartRoomByLevelNumber(int levelNumber) => StartRoom(levelNumber - 1);

    private void StartRoomInternal(int roomIndex)
    {
        if (roomIndex < 0 || roomIndex >= Rooms.Length)
        {
            GD.PushWarning($"StartRoom: invalid room index {roomIndex}");
            return;
        }

        bool fullUnlocked = MonetizationManager.Instance?.IsFullGameUnlocked ?? false;

        if (!fullUnlocked && roomIndex >= FreeLevels)
        {
            GD.PushWarning($"StartRoom: paywalled room {roomIndex}. Unlock full game to play.");
            return;
        }

        if (!fullUnlocked && !PlayerProfile.IsRoomUnlocked(roomIndex))
        {
            GD.PushWarning($"StartRoom: room locked {roomIndex}");
            return;
        }

        CurrentRoomIndex = roomIndex;
        State = GameState.InRoom;
        EmitSignal(SignalName.GameStateChanged, State);
        EmitSignal(SignalName.RoomStarted, roomIndex);

        ScoreManager.SetLevel(roomIndex + 1);
        Globals.GotoScene(Rooms[roomIndex].ScenePath);
    }

    public static void RestartRoom()
    {
        if (Instance.CurrentRoomIndex < 0)
            return;

        StartRoom(Instance.CurrentRoomIndex);
    }

    public static void CompleteRoom() => Instance.CompleteRoomInternal();

    private void CompleteRoomInternal()
    {
        if (CurrentRoomIndex < 0)
            return;

        State = GameState.RoomComplete;
        EmitSignal(SignalName.GameStateChanged, State);
        EmitSignal(SignalName.RoomCompleted, CurrentRoomIndex);

        UnlockNextRoom();
        ShowRoomComplete();
    }

    public static void UnlockNextRoom()
    {
        if (Instance.CurrentRoomIndex < 0)
            return;

        int next = Instance.CurrentRoomIndex + 1;
        if (next >= Instance.Rooms.Length)
            return;

        bool fullUnlocked = MonetizationManager.Instance?.IsFullGameUnlocked ?? false;
        if (!fullUnlocked && next >= FreeLevels)
            return;

        PlayerProfile.UnlockRoom(next);
    }

    public static void ShowRoomComplete()
    {
    }

    public static void TogglePause()
    {
        if (Instance.GetTree().Paused)
            Instance.ResumeGameInternal();
        else
            Instance.PauseGameInternal();
    }

    public static void PauseGame() => Instance.PauseGameInternal();

    private void PauseGameInternal()
    {
        GetTree().Paused = true;
        State = GameState.Paused;
        EmitSignal(SignalName.GameStateChanged, State);
    }

    public static void ResumeGame() => Instance.ResumeGameInternal();

    private void ResumeGameInternal()
    {
        GetTree().Paused = false;
        State = CurrentRoomIndex >= 0 ? GameState.InRoom : GameState.MainMenu;
        EmitSignal(SignalName.GameStateChanged, State);
    }

    private static RoomInfo[] CreateDefaultRooms()
    {
        var rooms = new RoomInfo[TotalLevels];
        for (int i = 0; i < TotalLevels; i++)
        {
            int levelNumber = i + 1;
            rooms[i] = new RoomInfo(
                $"res://Scenes/Level/Level{levelNumber}.tscn",
                $"Level {levelNumber}",
                3);
        }
        return rooms;
    }
}
