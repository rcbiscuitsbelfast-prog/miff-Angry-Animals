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
	/// Tracks which levels/rooms are unlocked.
	/// Level 1 is always unlocked by default.
	/// </summary>
	private HashSet<int> _unlockedLevels = new() { 1 };

	/// <summary>
	/// Tracks which bonus levels are unlocked.
	/// </summary>
	private HashSet<int> _unlockedBonusLevels = new();

	/// <summary>
	/// Total number of levels in the game.
	/// </summary>
	private const int TOTAL_LEVELS = 10;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Instance = this;
		// Load unlock status from save data if available
		LoadUnlockStatus();
	}

	/// <summary>
	/// Loads the main scene into the current scene tree.
	/// </summary>
	public static void LoadMain() => Instance.GetTree().ChangeSceneToPacked(Instance._mainScene);

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

	/// <summary>
	/// Checks if a level is valid (exists in the game).
	/// </summary>
	public static bool IsLevelValid(int levelNumber) => levelNumber > 0 && levelNumber <= TOTAL_LEVELS;

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
			Instance.SaveUnlockStatus();
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
		Instance.SaveUnlockStatus();
	}

	/// <summary>
	/// Checks if a bonus level is unlocked.
	/// </summary>
	public static bool IsBonusLevelUnlocked(int bonusNumber)
	{
		return Instance._unlockedBonusLevels.Contains(bonusNumber);
	}

	/// <summary>
	/// Saves the unlock status to the save file.
	/// </summary>
	private void SaveUnlockStatus()
	{
		// Could save unlock status to a JSON file similar to scores
		// For now, data is stored in memory during gameplay
		GD.Print("Unlock status updated (save not implemented yet)");
	}

	/// <summary>
	/// Loads the unlock status from the save file.
	/// </summary>
	private void LoadUnlockStatus()
	{
		// Could load unlock status from a JSON file similar to scores
		// For now, start with level 1 unlocked
		_unlockedLevels.Clear();
		_unlockedLevels.Add(1);
	}

	/// <summary>
	/// Gets the total number of levels.
	/// </summary>
	public static int GetTotalLevels() => TOTAL_LEVELS;
}
