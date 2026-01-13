using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Level browser for viewing and playing generated/custom levels
/// </summary>
public partial class LevelBrowserScript : Control
{
    private GridContainer _levelGrid;
    private OptionButton _themeFilter;
    private OptionButton _difficultyFilter;
    private LineEdit _searchBox;
    private Label _levelCountLabel;
    private Button _playButton;
    private Button _infoButton;
    private Button _deleteButton;
    
    private List<LevelMetadata> _allLevels = new();
    private List<LevelMetadata> _filteredLevels = new();
    private LevelMetadata _selectedLevel;
    
    public override void _Ready()
    {
        InitializeNodes();
        LoadLevels();
        SetupFilters();
        SetupButtons();
        RefreshDisplay();
    }
    
    private void InitializeNodes()
    {
        _levelGrid = GetNode<GridContainer>("MainContainer/LevelScroll/LevelGrid");
        _themeFilter = GetNode<OptionButton>("MainContainer/FilterBar/ThemeFilter");
        _difficultyFilter = GetNode<OptionButton>("MainContainer/FilterBar/DifficultyFilter");
        _searchBox = GetNode<LineEdit>("MainContainer/FilterBar/SearchBox");
        _levelCountLabel = GetNode<Label>("MainContainer/Header/LevelCount");
        _playButton = GetNode<Button>("MainContainer/ButtonBar/PlayButton");
        _infoButton = GetNode<Button>("MainContainer/ButtonBar/InfoButton");
        _deleteButton = GetNode<Button>("MainContainer/ButtonBar/DeleteButton");
    }
    
    private void LoadLevels()
    {
        _allLevels.Clear();
        
        // Load generated levels
        LoadLevelsFromDirectory("user://levels/generated");
        
        // Load custom levels
        LoadLevelsFromDirectory("user://levels/custom");
        
        GD.Print($"Loaded {_allLevels.Count} levels");
    }
    
    private void LoadLevelsFromDirectory(string directory)
    {
        if (!DirAccess.DirExistsAbsolute(directory))
            return;
        
        var dir = DirAccess.Open(directory);
        if (dir == null) return;
        
        string fileName = dir.GetNext();
        while (fileName != "")
        {
            if (fileName.EndsWith(".json"))
            {
                var level = LoadLevelFromFile($"{directory}/{fileName}");
                if (level != null)
                {
                    _allLevels.Add(level);
                }
            }
            fileName = dir.GetNext();
        }
    }
    
    private LevelMetadata LoadLevelFromFile(string filePath)
    {
        try
        {
            var file = FileAccess.Open(filePath, FileAccess.ModeFlags.Read);
            if (file == null) return null;
            
            string json = file.GetAsText();
            file.Close();
            
            var data = Json.ParseString(json).AsGodotDictionary();
            if (data == null) return null;
            
            var level = new LevelMetadata
            {
                LevelId = data["LevelId"].AsString(),
                LevelName = data["LevelName"].AsString(),
                Theme = data["Theme"].AsString(),
                Description = data["Description"].AsString(),
                Difficulty = (Difficulty)data["Difficulty"].AsInt32(),
                Goal = data["Goal"].AsInt32(),
                TargetTime = data["TargetTime"].AsInt32(),
                ParScore = data["ParScore"].AsInt32(),
                CreatedTimestamp = data["CreatedTimestamp"].AsInt64(),
                CreatorName = data["CreatorName"].AsString(),
                IsGenerated = data["IsGenerated"].AsBool()
            };
            
            // Load items
            if (data.ContainsKey("Items"))
            {
                var itemsArray = data["Items"].AsGodotArray();
                foreach (var itemData in itemsArray)
                {
                    var itemDict = itemData.AsGodotDictionary();
                    var item = new ItemInstance
                    {
                        ItemId = itemDict["ItemId"].AsString(),
                        Position = new Vector2(
                            itemDict["PositionX"].AsSingle(),
                            itemDict["PositionY"].AsSingle()
                        ),
                        Rotation = itemDict["Rotation"].AsSingle(),
                        Scale = itemDict["Scale"].AsSingle(),
                        MaterialOverride = (MaterialType)itemDict["MaterialOverride"].AsInt32()
                    };
                    level.Items.Add(item);
                }
            }
            
            return level;
        }
        catch (Exception ex)
        {
            GD.PushError($"Error loading level from {filePath}: {ex.Message}");
            return null;
        }
    }
    
    private void SetupFilters()
    {
        // Add theme filter options
        _themeFilter.AddItem("All Themes");
        foreach (LevelTheme theme in Enum.GetValues(typeof(LevelTheme)))
        {
            _themeFilter.AddItem(theme.ToString());
        }
        
        // Add difficulty filter options
        _difficultyFilter.AddItem("All Difficulties");
        foreach (Difficulty difficulty in Enum.GetValues(typeof(Difficulty)))
        {
            _difficultyFilter.AddItem(difficulty.ToString());
        }
        
        // Connect filter events
        _themeFilter.ItemSelected += OnThemeFilterChanged;
        _difficultyFilter.ItemSelected += OnDifficultyFilterChanged;
        _searchBox.TextChanged += OnSearchChanged;
    }
    
    private void SetupButtons()
    {
        _playButton.Pressed += OnPlayPressed;
        _infoButton.Pressed += OnInfoPressed;
        _deleteButton.Pressed += OnDeletePressed;
        
        // Initially disable buttons
        _playButton.Disabled = true;
        _infoButton.Disabled = true;
        _deleteButton.Disabled = true;
    }
    
    private void OnThemeFilterChanged(long index)
    {
        RefreshDisplay();
    }
    
    private void OnDifficultyFilterChanged(long index)
    {
        RefreshDisplay();
    }
    
    private void OnSearchChanged(string newText)
    {
        RefreshDisplay();
    }
    
    private void RefreshDisplay()
    {
        FilterLevels();
        CreateLevelButtons();
        UpdateLevelCount();
    }
    
    private void FilterLevels()
    {
        _filteredLevels = _allLevels.Where(level =>
        {
            // Apply theme filter
            if (_themeFilter.Selected > 0)
            {
                var selectedTheme = (ProceduralLevelGenerator.LevelTheme)(_themeFilter.Selected - 1);
                if (!level.Theme.Equals(selectedTheme.ToString(), StringComparison.OrdinalIgnoreCase))
                    return false;
            }
            
            // Apply difficulty filter
            if (_difficultyFilter.Selected > 0)
            {
                var selectedDifficulty = (Difficulty)(_difficultyFilter.Selected - 1);
                if (level.Difficulty != selectedDifficulty)
                    return false;
            }
            
            // Apply search filter
            string searchText = _searchBox.Text.ToLower();
            if (!string.IsNullOrEmpty(searchText))
            {
                if (!level.LevelName.ToLower().Contains(searchText) &&
                    !level.Theme.ToLower().Contains(searchText))
                    return false;
            }
            
            return true;
        }).ToList();
    }
    
    private void CreateLevelButtons()
    {
        // Clear existing buttons
        foreach (var child in _levelGrid.GetChildren())
        {
            child.QueueFree();
        }
        
        // Create buttons for filtered levels
        foreach (var level in _filteredLevels)
        {
            var button = CreateLevelButton(level);
            _levelGrid.AddChild(button);
        }
    }
    
    private Button CreateLevelButton(LevelMetadata level)
    {
        var button = new Button();
        button.CustomMinimumSize = new Vector2(150, 120);
        
        // Create button text
        string difficultySymbol = level.Difficulty switch
        {
            Difficulty.Easy => "🟢",
            Difficulty.Medium => "🟡",
            Difficulty.Hard => "🔴",
            Difficulty.Extreme => "💀",
            _ => "⚪"
        };
        
        button.Text = $"{difficultySymbol} {level.LevelName}\n" +
                     $"Theme: {level.Theme}\n" +
                     $"Goal: {level.Goal} blocks\n" +
                     $"Time: {level.TargetTime}s";
        
        // Store level reference
        button.SetMeta("level", level);
        button.Pressed += () => OnLevelSelected(level);
        
        return button;
    }
    
    private void OnLevelSelected(LevelMetadata level)
    {
        _selectedLevel = level;
        
        // Enable buttons
        _playButton.Disabled = false;
        _infoButton.Disabled = false;
        _deleteButton.Disabled = !level.IsGenerated; // Only allow deleting generated levels
        
        GD.Print($"Selected level: {level.LevelName}");
    }
    
    private void OnPlayPressed()
    {
        if (_selectedLevel == null) return;
        
        GD.Print($"🎮 Playing level: {_selectedLevel.LevelName}");
        
        // Store level data for the game scene
        LevelSession.SelectedLevel = _selectedLevel;
        
        // Load the game scene
        GetTree().ChangeSceneToFile("res://Scenes/Levels/ProceduralRoom.tscn");
    }
    
    private void OnInfoPressed()
    {
        if (_selectedLevel == null) return;
        
        var dialog = new AcceptDialog();
        dialog.Title = "Level Information";
        dialog.DialogText = $"Level: {_selectedLevel.LevelName}\n" +
                           $"Theme: {_selectedLevel.Theme}\n" +
                           $"Difficulty: {_selectedLevel.Difficulty}\n" +
                           $"Goal: {_selectedLevel.Goal} blocks\n" +
                           $"Target Time: {_selectedLevel.TargetTime} seconds\n" +
                           $"Items: {_selectedLevel.Items.Count}\n" +
                           $"Created: {DateTimeOffset.FromUnixTimeSeconds(_selectedLevel.CreatedTimestamp):yyyy-MM-dd}\n" +
                           $"Creator: {_selectedLevel.CreatorName}\n" +
                           $"Type: {(_selectedLevel.IsGenerated ? "Generated" : "Custom")}";
        
        AddChild(dialog);
        dialog.PopupCentered();
    }
    
    private void OnDeletePressed()
    {
        if (_selectedLevel == null || !_selectedLevel.IsGenerated) return;
        
        var dialog = new ConfirmationDialog();
        dialog.Title = "Delete Level";
        dialog.DialogText = $"Are you sure you want to delete \"{_selectedLevel.LevelName}\"?";
        dialog.Confirmed += () => DeleteSelectedLevel();
        
        AddChild(dialog);
        dialog.PopupCentered();
    }
    
    private void DeleteSelectedLevel()
    {
        if (_selectedLevel == null) return;
        
        try
        {
            string filePath = GetLevelFilePath(_selectedLevel.LevelId);
            if (FileAccess.FileExists(filePath))
            {
                DirAccess.RemoveAbsolute(filePath);
                
                // Remove from list
                _allLevels.Remove(_selectedLevel);
                _filteredLevels.Remove(_selectedLevel);
                
                // Clear selection
                _selectedLevel = null;
                _playButton.Disabled = true;
                _infoButton.Disabled = true;
                _deleteButton.Disabled = true;
                
                // Refresh display
                RefreshDisplay();
                
                GD.Print($"✅ Deleted level: {_selectedLevel?.LevelName}");
            }
        }
        catch (Exception ex)
        {
            GD.PushError($"Error deleting level: {ex.Message}");
        }
    }
    
    private void UpdateLevelCount()
    {
        _levelCountLabel.Text = $"Showing {_filteredLevels.Count} of {_allLevels.Count} levels";
    }
    
    private string GetLevelFilePath(string levelId)
    {
        // Try generated directory first
        string generatedPath = $"user://levels/generated/{levelId}.json";
        if (FileAccess.FileExists(generatedPath))
            return generatedPath;
        
        // Try custom directory
        return $"user://levels/custom/{levelId}.json";
    }
    
    private void OnRefreshPressed()
    {
        LoadLevels();
        RefreshDisplay();
        GD.Print(\"🔄 Level list refreshed\");
    }
}

/// <summary>
/// Session data for passing level information to game scene
/// </summary>
public static class LevelSession
{
    public static LevelMetadata SelectedLevel { get; set; }
}