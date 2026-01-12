using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Main level editor script for drag-and-drop level creation
/// </summary>
public partial class LevelEditorScript : Control
{
    [Export] private Control _canvas;
    [Export] private ItemLibraryPanel _itemLibrary;
    [Export] private LineEdit _levelNameInput;
    [Export] private OptionButton _difficultySelector;
    [Export] private SpinBox _goalInput;
    [Export] private LineEdit _descriptionInput;
    [Export] private Label _statusLabel;
    
    private LevelMetadata _currentLevel;
    private Dictionary<Node2D, ItemInstance> _placedItems = new();
    private ItemDefinition _selectedItem;
    private Vector2 _dragOffset;
    private bool _isDragging;
    private int _nextItemId;
    
    // Grid settings
    private const float GRID_SIZE = 40f;
    private const bool SNAP_TO_GRID = true;
    
    public override void _Ready()
    {
        GD.Print("🎨 Initializing Level Editor...");
        
        SetupEditor();
        SetupCanvas();
        SetupItemLibrary();
        SetupInputs();
        CreateNewLevel();
        
        GD.Print("✅ Level Editor ready");
    }
    
    private void SetupEditor()
    {
        // Set up window properties
        Size = new Vector2(1280, 720);
        AnchorRight = 1.0f;
        AnchorBottom = 1.0f;
        
        // Create UI layout if nodes are not exported
        if (_canvas == null)
        {
            CreateEditorLayout();
        }
    }
    
    private void CreateEditorLayout()
    {
        // Main container
        var mainContainer = new HBoxContainer();
        mainContainer.AnchorRight = 1.0f;
        mainContainer.AnchorBottom = 1.0f;
        AddChild(mainContainer);
        
        // Left panel - Item Library
        var leftPanel = new PanelContainer();
        leftPanel.CustomMinimumSize = new Vector2(300, 0);
        mainContainer.AddChild(leftPanel);
        
        var leftVBox = new VBoxContainer();
        leftPanel.AddChild(leftVBox);
        
        var libraryLabel = new Label();
        libraryLabel.Text = "📦 Item Library";
        libraryLabel.AddThemeFontSizeOverride("font_size", 20);
        leftVBox.AddChild(libraryLabel);
        
        // Item library panel
        _itemLibrary = new ItemLibraryPanel();
        _itemLibrary.CustomMinimumSize = new Vector2(280, 400);
        leftVBox.AddChild(_itemLibrary);
        
        // Center - Canvas
        var centerPanel = new PanelContainer();
        centerPanel.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        mainContainer.AddChild(centerPanel);
        
        var centerVBox = new VBoxContainer();
        centerPanel.AddChild(centerVBox);
        
        var canvasLabel = new Label();
        canvasLabel.Text = "🎨 Level Canvas";
        canvasLabel.AddThemeFontSizeOverride("font_size", 20);
        centerVBox.AddChild(canvasLabel);
        
        // Canvas area
        _canvas = new Control();
        _canvas.CustomMinimumSize = new Vector2(650, 480);
        _canvas.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        var canvasStyle = new StyleBoxFlat();
        canvasStyle.BgColor = new Color(0.2f, 0.2f, 0.2f);
        _canvas.AddThemeStyleboxOverride("panel", canvasStyle);
        centerVBox.AddChild(_canvas);
        
        // Right panel - Properties
        var rightPanel = new PanelContainer();
        rightPanel.CustomMinimumSize = new Vector2(250, 0);
        mainContainer.AddChild(rightPanel);
        
        var rightVBox = new VBoxContainer();
        rightPanel.AddChild(rightVBox);
        
        var propertiesLabel = new Label();
        propertiesLabel.Text = "⚙️ Properties";
        propertiesLabel.AddThemeFontSizeOverride("font_size", 20);
        rightVBox.AddChild(propertiesLabel);
        
        // Level name
        var nameLabel = new Label();
        nameLabel.Text = "Level Name:";
        rightVBox.AddChild(nameLabel);
        
        _levelNameInput = new LineEdit();
        _levelNameInput.PlaceholderText = "Enter level name...";
        rightVBox.AddChild(_levelNameInput);
        
        // Difficulty selector
        var difficultyLabel = new Label();
        difficultyLabel.Text = "Difficulty:";
        rightVBox.AddChild(difficultyLabel);
        
        _difficultySelector = new OptionButton();
        _difficultySelector.AddItem("Easy");
        _difficultySelector.AddItem("Medium");
        _difficultySelector.AddItem("Hard");
        _difficultySelector.AddItem("Extreme");
        _difficultySelector.Selected = 1; // Default to Medium
        rightVBox.AddChild(_difficultySelector);
        
        // Goal input
        var goalLabel = new Label();
        goalLabel.Text = "Goal (Blocks):";
        rightVBox.AddChild(goalLabel);
        
        _goalInput = new SpinBox();
        _goalInput.MinValue = 10;
        _goalInput.MaxValue = 100;
        _goalInput.Value = 30;
        rightVBox.AddChild(_goalInput);
        
        // Description
        var descLabel = new Label();
        descLabel.Text = "Description:";
        rightVBox.AddChild(descLabel);
        
        _descriptionInput = new LineEdit();
        _descriptionInput.PlaceholderText = "Enter description...";
        rightVBox.AddChild(_descriptionInput);
        
        // Buttons
        var buttonSpacer = new Control();
        buttonSpacer.CustomMinimumSize = new Vector2(0, 20);
        rightVBox.AddChild(buttonSpacer);
        
        var saveButton = new Button();
        saveButton.Text = "💾 Save Level";
        saveButton.Pressed += OnSavePressed;
        rightVBox.AddChild(saveButton);
        
        var testButton = new Button();
        testButton.Text = "🎮 Test Level";
        testButton.Pressed += OnTestPressed;
        rightVBox.AddChild(testButton);
        
        var clearButton = new Button();
        clearButton.Text = "🗑️ Clear All";
        clearButton.Pressed += OnClearPressed;
        rightVBox.AddChild(clearButton);
        
        var exitButton = new Button();
        exitButton.Text = "❌ Exit Editor";
        exitButton.Pressed += OnExitPressed;
        rightVBox.AddChild(exitButton);
        
        // Status label
        _statusLabel = new Label();
        _statusLabel.Text = "Ready";
        rightVBox.AddChild(_statusLabel);
    }
    
    private void SetupCanvas()
    {
        if (_canvas == null) return;
        
        // Set up canvas visuals
        var canvasStyle = new StyleBoxFlat();
        canvasStyle.BgColor = new Color(0.15f, 0.15f, 0.15f);
        canvasStyle.BorderColor = new Color(0.5f, 0.5f, 0.5f);
        canvasStyle.BorderWidthTop = 2;
        canvasStyle.BorderWidthBottom = 2;
        canvasStyle.BorderWidthLeft = 2;
        canvasStyle.BorderWidthRight = 2;
        _canvas.AddThemeStyleboxOverride("panel", canvasStyle);
        
        // Connect input events
        _canvas.GuiInput += OnCanvasInput;
        _canvas.Draw += OnCanvasDraw;
    }
    
    private void SetupItemLibrary()
    {
        if (_itemLibrary == null) return;
        
        _itemLibrary.ItemSelected += OnItemSelected;
        
        // Populate with all available items
        var items = ContentManager.Items.GetAllItems();
        _itemLibrary.SetItems(items);
    }
    
    private void SetupInputs()
    {
        if (_levelNameInput != null)
        {
            _levelNameInput.TextChanged += OnLevelNameChanged;
        }
        
        if (_goalInput != null)
        {
            _goalInput.ValueChanged += OnGoalChanged;
        }
    }
    
    private void CreateNewLevel()
    {
        _currentLevel = new LevelMetadata
        {
            LevelName = "My Custom Level",
            CreatorName = "Player",
            Difficulty = Difficulty.Medium,
            Goal = 30,
            Description = "A custom created level"
        };
        
        if (_levelNameInput != null)
        {
            _levelNameInput.Text = _currentLevel.LevelName;
        }
        
        if (_goalInput != null)
        {
            _goalInput.Value = _currentLevel.Goal;
        }
        
        UpdateStatus("New level created");
    }
    
    private void OnItemSelected(ItemDefinition item)
    {
        _selectedItem = item;
        UpdateStatus($"Selected: {item.ItemName}");
    }
    
    private void OnCanvasInput(InputEvent @event)
    {
        if (!(@event is InputEventMouseButton mouseEvent)) return;
        
        var localPos = _canvas.GetLocalMousePosition();
        
        if (mouseEvent.ButtonIndex == MouseButton.Left)
        {
            if (mouseEvent.Pressed)
            {
                // Check if clicking an existing item
                var clickedItem = GetItemAtPosition(localPos);
                if (clickedItem != null)
                {
                    _isDragging = true;
                    _dragOffset = clickedItem.Position - localPos;
                    GD.Print($"Started dragging item");
                }
                else if (_selectedItem != null)
                {
                    // Place new item
                    PlaceItem(_selectedItem, localPos);
                }
            }
            else
            {
                _isDragging = false;
            }
        }
        else if (mouseEvent.ButtonIndex == MouseButton.Right && mouseEvent.Pressed)
        {
            // Delete item on right-click
            var clickedItem = GetItemAtPosition(localPos);
            if (clickedItem != null)
            {
                DeleteItem(clickedItem);
            }
        }
    }
    
    public override void _Process(double delta)
    {
        if (_isDragging)
        {
            var mousePos = _canvas.GetLocalMousePosition();
            var draggedItem = GetItemAtPosition(mousePos - _dragOffset);
            if (draggedItem != null)
            {
                Vector2 newPos = mousePos - _dragOffset;
                if (SNAP_TO_GRID)
                {
                    newPos = SnapToGrid(newPos);
                }
                
                draggedItem.Position = newPos;
                UpdateItemData(draggedItem);
            }
        }
    }
    
    private void OnCanvasDraw()
    {
        // Draw grid if snapping is enabled
        if (SNAP_TO_GRID && _canvas != null)
        {
            var drawRect = new Rect2(Vector2.Zero, _canvas.Size);
            
            // Draw vertical grid lines
            for (float x = 0; x < drawRect.Size.X; x += GRID_SIZE)
            {
                _canvas.DrawLine(new Vector2(x, 0), new Vector2(x, drawRect.Size.Y), new Color(0.3f, 0.3f, 0.3f), 1f);
            }
            
            // Draw horizontal grid lines
            for (float y = 0; y < drawRect.Size.Y; y += GRID_SIZE)
            {
                _canvas.DrawLine(new Vector2(0, y), new Vector2(drawRect.Size.X, y), new Color(0.3f, 0.3f, 0.3f), 1f);
            }
        }
    }
    
    private void PlaceItem(ItemDefinition definition, Vector2 position)
    {
        if (definition == null) return;
        
        if (SNAP_TO_GRID)
        {
            position = SnapToGrid(position);
        }
        
        // Create item instance data
        var itemInstance = new ItemInstance(definition.ItemId, position)
        {
            Id = _nextItemId++,
            Rotation = 0f,
            Scale = definition.BaseScale
        };
        
        _currentLevel.Items.Add(itemInstance);
        
        // Create visual representation
        var itemNode = ItemFactory.CreateEditorItem(definition, position);
        _canvas.AddChild(itemNode);
        _placedItems[itemNode] = itemInstance;
        
        UpdateStatus($"Placed {definition.ItemName}");
        UpdateItemCount();
    }
    
    private Node2D GetItemAtPosition(Vector2 position)
    {
        foreach (var item in _placedItems.Keys)
        {
            if (item.Position.DistanceTo(position) < 30f)
            {
                return item;
            }
        }
        return null;
    }
    
    private void DeleteItem(Node2D itemNode)
    {
        if (_placedItems.TryGetValue(itemNode, out var itemInstance))
        {
            _currentLevel.Items.Remove(itemInstance);
            _placedItems.Remove(itemNode);
            itemNode.QueueFree();
            
            UpdateStatus("Item deleted");
            UpdateItemCount();
        }
    }
    
    private void UpdateItemData(Node2D itemNode)
    {
        if (_placedItems.TryGetValue(itemNode, out var itemInstance))
        {
            itemInstance.Position = itemNode.Position;
        }
    }
    
    private Vector2 SnapToGrid(Vector2 position)
    {
        return new Vector2(
            Mathf.Round(position.X / GRID_SIZE) * GRID_SIZE,
            Mathf.Round(position.Y / GRID_SIZE) * GRID_SIZE
        );
    }
    
    private void UpdateItemCount()
    {
        UpdateStatus($"Items: {_currentLevel.Items.Count}");
    }
    
    private void UpdateStatus(string message)
    {
        if (_statusLabel != null)
        {
            _statusLabel.Text = message;
        }
        GD.Print($"[Level Editor] {message}");
    }
    
    private void OnLevelNameChanged(string newName)
    {
        _currentLevel.LevelName = newName;
        UpdateStatus("Level name updated");
    }
    
    private void OnGoalChanged(double newValue)
    {
        _currentLevel.Goal = (int)newValue;
        UpdateStatus("Goal updated");
    }
    
    private void OnSavePressed()
    {
        if (string.IsNullOrEmpty(_currentLevel.LevelName))
        {
            UpdateStatus("❌ Please enter a level name");
            return;
        }
        
        if (_currentLevel.Items.Count == 0)
        {
            UpdateStatus("❌ Please add some items to the level");
            return;
        }
        
        try
        {
            SaveLevel();
            UpdateStatus("✅ Level saved successfully!");
            
            // Show save confirmation
            var dialog = new AcceptDialog();
            dialog.Title = "Save Successful";
            dialog.DialogText = $"Level '\\{_currentLevel.LevelName\\}' has been saved!";
            AddChild(dialog);
            dialog.PopupCentered();
        }
        catch (Exception ex)
        {
            UpdateStatus($"❌ Save failed: {ex.Message}");
            GD.PushError($"Failed to save level: {ex.Message}");
        }
    }
    
    private void SaveLevel()
    {
        // Generate level ID
        _currentLevel.LevelId = $"custom_{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}";
        _currentLevel.IsGenerated = false;
        
        // Ensure directories exist
        var dir = DirAccess.Open("user://");
        if (dir != null && !DirAccess.DirExistsAbsolute("user://levels"))
        {
            dir.MakeDir("levels");
            dir.MakeDir("levels/custom");
        }
        
        // Serialize and save
        string json = LevelEditorSaveSystem.SerializeLevel(_currentLevel);
        string filePath = $"user://levels/custom/{_currentLevel.LevelId}.json";
        
        var file = FileAccess.Open(filePath, FileAccess.ModeFlags.Write);
        if (file != null)
        {
            file.StoreString(json);
            file.Close();
            GD.Print($"Level saved to {filePath}");
        }
        else
        {
            throw new Exception("Failed to open file for writing");
        }
    }
    
    private void OnTestPressed()
    {
        if (_currentLevel.Items.Count == 0)
        {
            UpdateStatus("❌ Please add some items first");
            return;
        }
        
        // Temporarily save the level
        _currentLevel.LevelId = "_test_level";
        LevelSession.SelectedLevel = _currentLevel;
        
        GD.Print("🎮 Testing level...");
        GetTree().ChangeSceneToFile("res://Scenes/Levels/ProceduralRoom.tscn");
    }
    
    private void OnClearPressed()
    {
        // Confirm clear
        var dialog = new ConfirmationDialog();
        dialog.Title = "Clear Level";
        dialog.DialogText = "Are you sure you want to clear all items?";
        dialog.Confirmed += () => ClearLevel();
        AddChild(dialog);
        dialog.PopupCentered();
    }
    
    private void ClearLevel()
    {
        // Remove all visual items
        foreach (var itemNode in _placedItems.Keys)
        {
            itemNode.QueueFree();
        }
        
        _placedItems.Clear();
        _currentLevel.Items.Clear();
        _nextItemId = 0;
        
        UpdateStatus("Level cleared");
        UpdateItemCount();
    }
    
    private void OnExitPressed()
    {
        var dialog = new ConfirmationDialog();
        dialog.Title = "Exit Editor";
        dialog.DialogText = "Exit to main menu? Unsaved changes will be lost.";
        dialog.Confirmed += () => GetTree().ChangeSceneToFile("res://Scenes/MainMenu.tscn");
        AddChild(dialog);
        dialog.PopupCentered();
    }
}

/// <summary>
/// Save system for level editor
/// </summary>
public static class LevelEditorSaveSystem
{
    public static string SerializeLevel(LevelMetadata level)
    {
        var data = new Godot.Collections.Dictionary
        {
            ["LevelId"] = level.LevelId,
            ["LevelName"] = level.LevelName,
            ["Theme"] = level.Theme,
            ["Description"] = level.Description,
            ["Difficulty"] = (int)level.Difficulty,
            ["Goal"] = level.Goal,
            ["TargetTime"] = level.TargetTime,
            ["ParScore\"] = level.ParScore,
            [\"CreatedTimestamp\"] = level.CreatedTimestamp,
            [\"CreatorName\"] = level.CreatorName,
            [\"IsGenerated\"] = level.IsGenerated,
            [\"Items\"] = SerializeItems(level.Items)
        };
        
        return Json.Stringify(data);
    }
    
    private static Godot.Collections.Array SerializeItems(List<ItemInstance> items)
    {
        var array = new Godot.Collections.Array();
        
        foreach (var item in items)
        {
            var itemData = new Godot.Collections.Dictionary
            {
                [\"ItemId\"] = item.ItemId,
                [\"PositionX\"] = item.Position.X,
                [\"PositionY\"] = item.Position.Y,
                [\"Rotation\"] = item.Rotation,
                [\"Scale\"] = item.Scale,
                [\"MaterialOverride\"] = (int)item.MaterialOverride
            };
            array.Add(itemData);
        }
        
        return array;
    }
}