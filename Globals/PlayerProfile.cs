using Godot;

/// <summary>
/// Global manager for player profile information.
/// Handles player name, unlocked levels, and other persistent player data.
/// </summary>
public partial class PlayerProfile : Node
{
	public static PlayerProfile Instance { get; private set; }

	private string _playerName = "Player";
	private int _unlockedLevels = 1;

	public override void _Ready() => Instance = this;

	public static string GetPlayerName() => Instance._playerName;

	public static void SetPlayerName(string name) => Instance._playerName = name;

	public static int GetUnlockedLevels() => Instance._unlockedLevels;

	public static void SetUnlockedLevels(int levels) => Instance._unlockedLevels = levels;

	public static bool IsLevelUnlocked(int levelNumber) => levelNumber <= Instance._unlockedLevels;
}
