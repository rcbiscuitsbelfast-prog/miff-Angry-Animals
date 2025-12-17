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

    public RoomInfo[] Rooms { get; } =
    [
        new RoomInfo("res://Scenes/Rooms/Room001.tscn", "Room 1", 200),
        new RoomInfo("res://Scenes/Rooms/Room002.tscn", "Room 2", 200),
        new RoomInfo("res://Scenes/Rooms/Room003.tscn", "Room 3", 200),
        new RoomInfo("res://Scenes/Rooms/Room004.tscn", "Room 4", 200),
        new RoomInfo("res://Scenes/Rooms/Room005.tscn", "Room 5", 200),
        new RoomInfo("res://Scenes/Rooms/Room006.tscn", "Room 6", 200),
        new RoomInfo("res://Scenes/Rooms/Room007.tscn", "Room 7", 200),
        new RoomInfo("res://Scenes/Rooms/Room008.tscn", "Room 8", 200),
        new RoomInfo("res://Scenes/Rooms/Room009.tscn", "Room 9", 200),
        new RoomInfo("res://Scenes/Rooms/Room010.tscn", "Room 10", 200),
        new RoomInfo("res://Scenes/Rooms/Room011.tscn", "Room 11", 200),
        new RoomInfo("res://Scenes/Rooms/Room012.tscn", "Room 12", 200),
        new RoomInfo("res://Scenes/Rooms/Room013.tscn", "Room 13", 200),
        new RoomInfo("res://Scenes/Rooms/Room014.tscn", "Room 14", 200),
        new RoomInfo("res://Scenes/Rooms/Room015.tscn", "Room 15", 200),
        new RoomInfo("res://Scenes/Rooms/Room016.tscn", "Room 16", 200),
        new RoomInfo("res://Scenes/Rooms/Room017.tscn", "Room 17", 200),
        new RoomInfo("res://Scenes/Rooms/Room018.tscn", "Room 18", 200),
        new RoomInfo("res://Scenes/Rooms/Room019.tscn", "Room 19", 200),
        new RoomInfo("res://Scenes/Rooms/Room020.tscn", "Room 20", 200),
        new RoomInfo("res://Scenes/Rooms/Room021.tscn", "Room 21", 800),
        new RoomInfo("res://Scenes/Rooms/Room022.tscn", "Room 22", 800),
        new RoomInfo("res://Scenes/Rooms/Room023.tscn", "Room 23", 800),
        new RoomInfo("res://Scenes/Rooms/Room024.tscn", "Room 24", 800),
        new RoomInfo("res://Scenes/Rooms/Room025.tscn", "Room 25", 800),
        new RoomInfo("res://Scenes/Rooms/Room026.tscn", "Room 26", 800),
        new RoomInfo("res://Scenes/Rooms/Room027.tscn", "Room 27", 800),
        new RoomInfo("res://Scenes/Rooms/Room028.tscn", "Room 28", 800),
        new RoomInfo("res://Scenes/Rooms/Room029.tscn", "Room 29", 800),
        new RoomInfo("res://Scenes/Rooms/Room030.tscn", "Room 30", 800),
        new RoomInfo("res://Scenes/Rooms/Room031.tscn", "Room 31", 800),
        new RoomInfo("res://Scenes/Rooms/Room032.tscn", "Room 32", 800),
        new RoomInfo("res://Scenes/Rooms/Room033.tscn", "Room 33", 800),
        new RoomInfo("res://Scenes/Rooms/Room034.tscn", "Room 34", 800),
        new RoomInfo("res://Scenes/Rooms/Room035.tscn", "Room 35", 800),
        new RoomInfo("res://Scenes/Rooms/Room036.tscn", "Room 36", 800),
        new RoomInfo("res://Scenes/Rooms/Room037.tscn", "Room 37", 800),
        new RoomInfo("res://Scenes/Rooms/Room038.tscn", "Room 38", 800),
        new RoomInfo("res://Scenes/Rooms/Room039.tscn", "Room 39", 800),
        new RoomInfo("res://Scenes/Rooms/Room040.tscn", "Room 40", 800),
        new RoomInfo("res://Scenes/Rooms/Room041.tscn", "Room 41", 800),
        new RoomInfo("res://Scenes/Rooms/Room042.tscn", "Room 42", 800),
        new RoomInfo("res://Scenes/Rooms/Room043.tscn", "Room 43", 800),
        new RoomInfo("res://Scenes/Rooms/Room044.tscn", "Room 44", 800),
        new RoomInfo("res://Scenes/Rooms/Room045.tscn", "Room 45", 800),
        new RoomInfo("res://Scenes/Rooms/Room046.tscn", "Room 46", 800),
        new RoomInfo("res://Scenes/Rooms/Room047.tscn", "Room 47", 800),
        new RoomInfo("res://Scenes/Rooms/Room048.tscn", "Room 48", 800),
        new RoomInfo("res://Scenes/Rooms/Room049.tscn", "Room 49", 800),
        new RoomInfo("res://Scenes/Rooms/Room050.tscn", "Room 50", 800),
        new RoomInfo("res://Scenes/Rooms/Room051.tscn", "Room 51", 2000),
        new RoomInfo("res://Scenes/Rooms/Room052.tscn", "Room 52", 2000),
        new RoomInfo("res://Scenes/Rooms/Room053.tscn", "Room 53", 2000),
        new RoomInfo("res://Scenes/Rooms/Room054.tscn", "Room 54", 2000),
        new RoomInfo("res://Scenes/Rooms/Room055.tscn", "Room 55", 2000),
        new RoomInfo("res://Scenes/Rooms/Room056.tscn", "Room 56", 2000),
        new RoomInfo("res://Scenes/Rooms/Room057.tscn", "Room 57", 2000),
        new RoomInfo("res://Scenes/Rooms/Room058.tscn", "Room 58", 2000),
        new RoomInfo("res://Scenes/Rooms/Room059.tscn", "Room 59", 2000),
        new RoomInfo("res://Scenes/Rooms/Room060.tscn", "Room 60", 2000),
        new RoomInfo("res://Scenes/Rooms/Room061.tscn", "Room 61", 2000),
        new RoomInfo("res://Scenes/Rooms/Room062.tscn", "Room 62", 2000),
        new RoomInfo("res://Scenes/Rooms/Room063.tscn", "Room 63", 2000),
        new RoomInfo("res://Scenes/Rooms/Room064.tscn", "Room 64", 2000),
        new RoomInfo("res://Scenes/Rooms/Room065.tscn", "Room 65", 2000),
        new RoomInfo("res://Scenes/Rooms/Room066.tscn", "Room 66", 2000),
        new RoomInfo("res://Scenes/Rooms/Room067.tscn", "Room 67", 2000),
        new RoomInfo("res://Scenes/Rooms/Room068.tscn", "Room 68", 2000),
        new RoomInfo("res://Scenes/Rooms/Room069.tscn", "Room 69", 2000),
        new RoomInfo("res://Scenes/Rooms/Room070.tscn", "Room 70", 2000),
        new RoomInfo("res://Scenes/Rooms/Room071.tscn", "Room 71", 2000),
        new RoomInfo("res://Scenes/Rooms/Room072.tscn", "Room 72", 2000),
        new RoomInfo("res://Scenes/Rooms/Room073.tscn", "Room 73", 2000),
        new RoomInfo("res://Scenes/Rooms/Room074.tscn", "Room 74", 2000),
        new RoomInfo("res://Scenes/Rooms/Room075.tscn", "Room 75", 2000),
        new RoomInfo("res://Scenes/Rooms/Room076.tscn", "Room 76", 2000),
        new RoomInfo("res://Scenes/Rooms/Room077.tscn", "Room 77", 2000),
        new RoomInfo("res://Scenes/Rooms/Room078.tscn", "Room 78", 2000),
        new RoomInfo("res://Scenes/Rooms/Room079.tscn", "Room 79", 2000),
        new RoomInfo("res://Scenes/Rooms/Room080.tscn", "Room 80", 2000),
        new RoomInfo("res://Scenes/Rooms/Room081.tscn", "Room 81", 4000),
        new RoomInfo("res://Scenes/Rooms/Room082.tscn", "Room 82", 4000),
        new RoomInfo("res://Scenes/Rooms/Room083.tscn", "Room 83", 4000),
        new RoomInfo("res://Scenes/Rooms/Room084.tscn", "Room 84", 4000),
        new RoomInfo("res://Scenes/Rooms/Room085.tscn", "Room 85", 4000),
        new RoomInfo("res://Scenes/Rooms/Room086.tscn", "Room 86", 4000),
        new RoomInfo("res://Scenes/Rooms/Room087.tscn", "Room 87", 4000),
        new RoomInfo("res://Scenes/Rooms/Room088.tscn", "Room 88", 4000),
        new RoomInfo("res://Scenes/Rooms/Room089.tscn", "Room 89", 4000),
        new RoomInfo("res://Scenes/Rooms/Room090.tscn", "Room 90", 4000),
        new RoomInfo("res://Scenes/Rooms/Room091.tscn", "Room 91", 4000),
        new RoomInfo("res://Scenes/Rooms/Room092.tscn", "Room 92", 4000),
        new RoomInfo("res://Scenes/Rooms/Room093.tscn", "Room 93", 4000),
        new RoomInfo("res://Scenes/Rooms/Room094.tscn", "Room 94", 4000),
        new RoomInfo("res://Scenes/Rooms/Room095.tscn", "Room 95", 4000),
        new RoomInfo("res://Scenes/Rooms/Room096.tscn", "Room 96", 4000),
        new RoomInfo("res://Scenes/Rooms/Room097.tscn", "Room 97", 4000),
        new RoomInfo("res://Scenes/Rooms/Room098.tscn", "Room 98", 4000),
        new RoomInfo("res://Scenes/Rooms/Room099.tscn", "Room 99", 4000),
        new RoomInfo("res://Scenes/Rooms/Room100.tscn", "Room 100", 4000)
    ];

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

        if (!PlayerProfile.IsRoomUnlocked(roomIndex))
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
        if (next < Instance.Rooms.Length)
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
}
