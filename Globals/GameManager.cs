using Godot;

/// <summary>
/// Global manager for handling scene transition.
/// Currently provides functionality to load the main scene.
/// </summary>
public partial class GameManager : Node
{
    public static GameManager Instance { get; private set; } // Singleton for global access as an AutoLoad.

    /// <summary>
    /// Reference to the main scen, preloaded as a PackedScene.
    /// This avoids loading it repeatedly at runtime.
    /// </summary>
    private PackedScene _mainScene = GD.Load<PackedScene>("res://Scenes/Main/Main.tscn");



    // Called when the node enters the scene tree for the first time.
    public override void _Ready() => Instance = this;


    /// <summary>
    /// Loads the main scene into the current scene tree.
    /// </summary>
    public static void LoadMain() => Instance.GetTree().ChangeSceneToPacked(Instance._mainScene);
}