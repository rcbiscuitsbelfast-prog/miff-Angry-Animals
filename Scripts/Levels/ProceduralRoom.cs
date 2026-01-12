using Godot;
using System;
using System.Linq;

/// <summary>
/// Game scene that loads and plays custom or generated levels
/// </summary>
public partial class ProceduralRoom : RoomBase
{
    private const string FALLBACK_SCENE = "res://Scenes/Levels/ProceduralRoom.tscn";
    
    public override void _Ready()
    {
        base._Ready();
        
        // Load the selected level from LevelSession
        if (LevelSession.SelectedLevel != null)
        {
            LoadLevelFromMetadata(LevelSession.SelectedLevel);
        }
        else
        {
            // Fallback: generate a random level
            GD.Print("No level selected, generating random level...");
            var randomLevel = GenerateRandomLevel();
            LoadLevelFromMetadata(randomLevel);
        }
    }
    
    /// <summary>
    /// Loads a level from LevelMetadata
    /// </summary>
    private void LoadLevelFromMetadata(LevelMetadata levelData)
    {
        GD.Print($"🎮 Loading level: {levelData.LevelName}");
        GD.Print($"  Theme: {levelData.Theme}");
        GD.Print($"  Difficulty: {levelData.Difficulty}");
        GD.Print($"  Items: {levelData.Items.Count}");
        GD.Print($"  Goal: {levelData.Goal} blocks");
        
        // Set up level properties
        CurrentLevelName = levelData.LevelName;
        LevelGoal = levelData.Goal;
        TargetTime = levelData.TargetTime;
        
        // Spawn all items
        foreach (var itemInstance in levelData.Items)
        {
            SpawnItem(itemInstance);
        }
        
        // Apply theme-based settings
        ApplyThemeSettings(levelData.Theme);
        
        // Start the level
        StartLevel();
    }
    
    /// <summary>
    /// Spawns an item instance in the scene
    /// </summary>
    private void SpawnItem(ItemInstance itemInstance)
    {
        try
        {
            var itemNode = ItemFactory.CreateItemInstance(itemInstance);
            if (itemNode != null)
            {
                AddChild(itemNode);
                
                // Add to tracking if it has breakable component
                var breakable = itemNode.GetNodeOrNull<BreakableObstacle>(".");
                if (breakable != null)
                {
                    _obstacles.Add(breakable);
                }
            }
        }
        catch (Exception ex)
        {
            GD.PushWarning($"Failed to spawn item {itemInstance.ItemId}: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Applies visual settings based on the theme
    /// </summary>
    private void ApplyThemeSettings(string theme)
    {
        // Set background color based on theme
        Color backgroundColor = theme.ToLower() switch
        {
            "school" => new Color(0.3f, 0.6f, 0.9f),
            "home" => new Color(0.4f, 0.7f, 0.4f),
            "office" => new Color(0.5f, 0.5f, 0.6f),
            "government" => new Color(0.7f, 0.3f, 0.3f),
            "kitchen" => new Color(0.9f, 0.6f, 0.3f),
            "bedroom" => new Color(0.6f, 0.4f, 0.7f),
            "livingroom" => new Color(0.6f, 0.5f, 0.4f),
            "factory" => new Color(0.4f, 0.4f, 0.4f),
            "arcade" => new Color(0.2f, 0.2f, 0.3f),
            _ => new Color(0.3f, 0.3f, 0.4f)
        };
        
        // Apply to background
        var bg = GetNodeOrNull<ColorRect>("Background");
        if (bg != null)
        {
            bg.Color = backgroundColor;
        }
        
        // Set level info text
        var levelInfo = GetNodeOrNull<Label>("LevelInfoText");
        if (levelInfo != null)
        {
            levelInfo.Text = $"Theme: {theme}\nGoal: {LevelGoal} blocks";
        }
    }
    
    /// <summary>
    /// Generates a random level when no level is selected
    /// </summary>
    private LevelMetadata GenerateRandomLevel()
    {
        var random = new Random();
        var themes = Enum.GetValues(typeof(ProceduralLevelGenerator.LevelTheme));
        var randomTheme = (ProceduralLevelGenerator.LevelTheme)themes.GetValue(random.Next(themes.Length));
        
        return ProceduralLevelGenerator.GenerateLevel(1, randomTheme);
    }
    
    /// <summary>
    /// Reloads the current level
    /// </summary>
    public override void Restart()
    {
        if (LevelSession.SelectedLevel != null)
        {
            GetTree().ReloadCurrentScene();
        }
        else
        {
            base.Restart();
        }
    }
    
    /// <summary>
    /// Returns to level browser or main menu
    /// </summary>
    protected override void OnRoomComplete()
    {
        base.OnRoomComplete();
        
        // Show completion dialog specific to custom/generated levels
        ShowCustomCompleteDialog();
    }
    
    private void ShowCustomCompleteDialog()
    {
        var dialog = new AcceptDialog();
        dialog.Title = "Level Complete!";
        
        string message = $"🎉 Congratulations!\n\n" +
                        $"Level: {CurrentLevelName}\n" +
                        $"Blocks Destroyed: {BlocksDestroyed}/{LevelGoal}\n" +
                        $\"Time: {Math.Floor(GameTime)}s\";
        
        if (BlocksDestroyed >= LevelGoal)
        {
            message += "\n\n✅ Goal Achieved!";
        }
        else
        {
            message += "\n\n❌ Goal Not Reached";
        }
        
        dialog.DialogText = message;
        dialog.Confirmed += () => {
            // Return to level browser
            LevelSession.SelectedLevel = null;
            GetTree().ChangeSceneToFile("res://Scenes/Levels/LevelBrowser.tscn");
        };
        
        AddChild(dialog);
        dialog.PopupCentered();
    }
}