using Godot;
using System.Collections.Generic;

/// <summary>
/// Global manager for handling scene transitions and room/level progression.
/// Manages room unlocking, level metadata, and transitions between scenes.
/// Implements the room-based progression system for the Toppler-like gameplay flow.
/// </summary>
public partial class GameManager : Node
{
	public static GameManager Instance { get; private set; } // Singleton for global access as an AutoLoad.

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
	/// Reference to the main scene, preloaded as a PackedScene.
	/// This avoids loading it repeatedly at runtime.
	/// </summary>
	private PackedScene _mainScene = GD.Load<PackedScene>("res://Scenes/Main/Main.tscn");

	/// <summary>
	/// Reference to the room selection scene.
	/// </summary>
	private PackedScene _roomSelectionScene = GD.Load<PackedScene>("res://Scenes/RoomSelection/RoomSelection.tscn");

	/// <summary>
	/// All rooms in the game with their metadata.
	/// </summary>
	public RoomInfo[] Rooms { get; } =
	[
		new RoomInfo("res://Scenes/Level/Level1.tscn", "Room 1", 3),
		new RoomInfo("res://Scenes/Level/Level2.tscn", "Room 2", 3),
		new RoomInfo("res://Scenes/Level/Level3.tscn", "Room 3", 4),
		new RoomInfo("res://Scenes/Level/Level4.tscn", "Room 4", 4),
		new RoomInfo("res://Scenes/Level/Level5.tscn", "Room 5", 5),
		new RoomInfo("res://Scenes/Level/Level6.tscn", "Room 6", 5),
		new RoomInfo("res://Scenes/Level/Level7.tscn", "Room 7", 6),
		new RoomInfo("res://Scenes/Level/Level8.tscn", "Room 8", 6),
		new RoomInfo("res://Scenes/Level/Level9.tscn", "Room 9", 7),
		new RoomInfo("res://Scenes/Level/Level10.tscn", "Room 10", 7)
	];

	/// <summary>
	/// Tracks which levels/rooms are unlocked.
	/// Level 1 is always unlocked by default.
	/// </summary>
	private HashSet<int> _unlockedLevels = new() { 1 };

	/// <summary>
	/// Tracks which bonus levels are unlocked.
	/// </summary>
	private HashSet<int> _unlockedBonusLevels = new();

	public GameState State { get; private set; } = GameState.Boot;
	public int CurrentRoomIndex { get; private set; } = -1;

	public string MainScenePath { get; set; } = "res://Scenes/Main/Main.tscn";

	private SignalManager? _signalManager;

	[Signal] public delegate void GameStateChangedEventHandler(GameState state);
	[Signal] public delegate void RoomStartedEventHandler(int roomIndex);
	[Signal] public delegate void RoomCompletedEventHandler(int roomIndex);

	// Called when the node enters the scene tree for the first time.
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

	/// <summary>
	/// Loads the main scene into the current scene tree.
	/// </summary>
	public static void LoadMain() => Instance.LoadMainInternal();

	private void LoadMainInternal()
	{
		CurrentRoomIndex = -1;
		State = GameState.MainMenu;
		EmitSignal(SignalName.GameStateChanged, State);
		GetTree().ChangeSceneToPacked(_mainScene);
	}

	/// <summary>
	/// Loads the room selection scene.
	/// </summary>
	public static void LoadRoomSelection()
	{
		if (Instance._roomSelectionScene != null)
		{
			Instance.GetTree().ChangeSceneToPacked(Instance._roomSelectionScene);
		}
		else
		{
			GD.PrintErr("Room selection scene not loaded");
		}
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

		if (!IsLevelUnlocked(roomIndex + 1))
		{
			GD.PushWarning($"StartRoom: room locked {roomIndex}");
			return;
		}

		CurrentRoomIndex = roomIndex;
		State = GameState.InRoom;
		EmitSignal(SignalName.GameStateChanged, State);
		EmitSignal(SignalName.RoomStarted, roomIndex);

		ScoreManager.SetLevel(roomIndex + 1);
		GetTree().ChangeSceneToPacked(Rooms[roomIndex].ScenePath);
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
			UnlockLevel(next + 1);
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

	/// <summary>
	/// Checks if a level is valid (exists in the game).
	/// </summary>
	public static bool IsLevelValid(int levelNumber) => levelNumber > 0 && levelNumber <= Rooms.Length;

	/// <summary>
	/// Checks if a level is unlocked.
	/// </summary>
	public static bool IsLevelUnlocked(int levelNumber)
	{
		if (!IsLevelValid(levelNumber))
			return false;

		return Instance._unlockedLevels.Contains(levelNumber);
	}

	/// <summary>
	/// Unlocks a level.
	/// </summary>
	public static void UnlockLevel(int levelNumber)
	{
		if (IsLevelValid(levelNumber))
		{
			Instance._unlockedLevels.Add(levelNumber);
		}
	}

	/// <summary>
	/// Checks if a bonus level should be unlocked based on the current level.
	/// </summary>
	public static bool ShouldUnlockBonusLevel(int levelNumber)
	{
		// Example: Unlock bonus level after every 3 levels
		return levelNumber > 0 && levelNumber % 3 == 0;
	}

	/// <summary>
	/// Unlocks a bonus level.
	/// </summary>
	public static void UnlockBonusLevel(int levelNumber)
	{
		Instance._unlockedBonusLevels.Add(levelNumber);
	}

	/// <summary>
	/// Checks if a bonus level is unlocked.
	/// </summary>
	public static bool IsBonusLevelUnlocked(int bonusNumber)
	{
		return Instance._unlockedBonusLevels.Contains(bonusNumber);
	}

	/// <summary>
	/// Gets the total number of levels.
	/// </summary>
	public static int GetTotalLevels() => Rooms.Length;
}