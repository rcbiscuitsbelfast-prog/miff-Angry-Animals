using Godot;

/// <summary>
/// Global manager for handling scene transitions.
/// Provides functionality to load various game scenes (menu, levels, etc.).
/// </summary>
public partial class GameManager : Node
{
	public static GameManager Instance { get; private set; } // Singleton for global access as an AutoLoad.

	private PackedScene _mainMenuScene = GD.Load<PackedScene>("res://Scenes/MainMenu/MainMenu.tscn");
	private PackedScene _levelSelectionScene = GD.Load<PackedScene>("res://Scenes/LevelSelection/LevelSelection.tscn");
	private PackedScene _levelCompletedScene = GD.Load<PackedScene>("res://Scenes/LevelCompleted/LevelCompleted.tscn");

	// Called when the node enters the scene tree for the first time.
	public override void _Ready() => Instance = this;

	/// <summary>
	/// Loads the main menu scene.
	/// </summary>
	public static void LoadMainMenu()
	{
		if (Instance._mainMenuScene != null)
			Instance.GetTree().ChangeSceneToPacked(Instance._mainMenuScene);
	}

	/// <summary>
	/// Loads the level selection scene.
	/// </summary>
	public static void LoadLevelSelection()
	{
		if (Instance._levelSelectionScene != null)
			Instance.GetTree().ChangeSceneToPacked(Instance._levelSelectionScene);
	}

	/// <summary>
	/// Loads a specific level scene.
	/// </summary>
	public static void LoadLevel(int levelNumber)
	{
		string levelPath = $"res://Scenes/Level{levelNumber}/Level{levelNumber}.tscn";
		var levelScene = GD.Load<PackedScene>(levelPath);

		if (levelScene != null)
		{
			Instance.GetTree().ChangeSceneToPacked(levelScene);
		}
		else
		{
			GD.PrintErr($"Failed to load level: {levelPath}");
			LoadLevelSelection();
		}
	}

	/// <summary>
	/// Loads the level completed scene.
	/// </summary>
	public static void LoadLevelCompleted()
	{
		if (Instance._levelCompletedScene != null)
			Instance.GetTree().ChangeSceneToPacked(Instance._levelCompletedScene);
	}

	/// <summary>
	/// Legacy method for loading main scene.
	/// </summary>
	public static void LoadMain()
	{
		LoadMainMenu();
	}
}
