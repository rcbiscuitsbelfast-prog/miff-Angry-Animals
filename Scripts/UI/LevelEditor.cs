using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// In-game level editor for creating custom levels.
/// Provides drag-and-drop obstacle placement, material selection, and difficulty validation.
/// </summary>
public partial class LevelEditor : Control
{
    private Control _placementArea;
    private Container _materialSelector;
    private ItemList _obstacleList;
    private Label _difficultyLabel;
    private LineEdit _levelNameInput;
    private LineEdit _creatorNameInput;

    private CustomLevelData _currentLevel;
    private MaterialType _selectedMaterial = MaterialType.Wood;
    private List<Node2D> _obstacleNodes = new List<Node2D>();
    private int _nextObstacleId = 0;
    private Node2D _selectedObstacle;
    private bool _isDragging = false;
    private Vector2 _dragOffset;

    // Grid settings
    private const float GRID_SIZE = 40f;
    private const bool SNAP_TO_GRID = true;

    // Playable area bounds
    private const float PLAYABLE_MIN_X = 300f;
    private const float PLAYABLE_MAX_X = 950f;
    private const float PLAYABLE_MIN_Y = 50f;
    private const float PLAYABLE_MAX_Y = 530f;

    public override void _Ready()
    {
        InitializeNodes();
        InitializeNewLevel();
        SetupMaterialSelector();
        SetupButtons();
        UpdateDifficultyDisplay();
    }

    private void InitializeNodes()
    {
        _placementArea = GetNodeOrNull<Control>("MainContainer/ContentContainer/CenterPanel/PlacementArea");
        _materialSelector = GetNodeOrNull<Container>("MainContainer/ContentContainer/LeftPanel/MaterialSelector");
        _obstacleList = GetNodeOrNull<ItemList>("MainContainer/ContentContainer/RightPanel/ObstacleList");
        _difficultyLabel = GetNodeOrNull<Label>("%DifficultyLabel");
        _levelNameInput = GetNodeOrNull<LineEdit>("%LevelNameInput");
        _creatorNameInput = GetNodeOrNull<LineEdit>("%CreatorNameInput");

        if (_placementArea == null)
        {
            GD.PrintErr("Placement area not found");
        }
    }

    private void InitializeNewLevel()
    {
        _currentLevel = new CustomLevelData
        {
            LevelName = "My Custom Level",
            CreatorName = "Player",
            CreatedTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
        };

        if (_levelNameInput != null)
        {
            _levelNameInput.Text = _currentLevel.LevelName;
            _levelNameInput.TextChanged += OnLevelNameChanged;
        }

        if (_creatorNameInput != null)
        {
            _creatorNameInput.Text = _currentLevel.CreatorName;
            _creatorNameInput.TextChanged += OnCreatorNameChanged;
        }
    }

    private void SetupMaterialSelector()
    {
        if (_materialSelector == null) return;

        // Create material selection buttons
        var materials = new[] { MaterialType.Wood, MaterialType.Stone, MaterialType.Brick, MaterialType.Iron, MaterialType.Diamond };
        var colors = new Dictionary<MaterialType, Color>
        {
            { MaterialType.Wood, new Color(0.6f, 0.4f, 0.2f) },
            { MaterialType.Stone, new Color(0.5f, 0.5f, 0.5f) },
            { MaterialType.Brick, new Color(0.7f, 0.3f, 0.2f) },
            { MaterialType.Iron, new Color(0.3f, 0.3f, 0.3f) },
            { MaterialType.Diamond, new Color(0.4f, 0.7f, 0.9f) }
        };

        foreach (var material in materials)
        {
            var button = new Button();
            button.Text = material.ToString();
            button.CustomMinimumSize = new Vector2(80, 40);
            
            if (colors.TryGetValue(material, out Color color))
            {
                var styleBox = new StyleBoxFlat();
                styleBox.BgColor = color;
                button.AddThemeStyleboxOverride("normal", styleBox);
            }

            var mat = material; // Capture for lambda
            button.Pressed += () => OnMaterialSelected(mat);
            _materialSelector.AddChild(button);
        }
    }

    private void SetupButtons()
    {
        // Find and connect buttons
        var clearBtn = GetNodeOrNull<Button>("%ClearButton");
        if (clearBtn != null) clearBtn.Pressed += OnClearPressed;

        var validateBtn = GetNodeOrNull<Button>("%ValidateButton");
        if (validateBtn != null) validateBtn.Pressed += OnValidatePressed;

        var saveBtn = GetNodeOrNull<Button>("%SaveButton");
        if (saveBtn != null) saveBtn.Pressed += OnSavePressed;

        var backBtn = GetNodeOrNull<Button>("%BackButton");
        if (backBtn != null) backBtn.Pressed += OnBackPressed;
    }

    private void OnMaterialSelected(MaterialType material)
    {
        _selectedMaterial = material;
        GD.Print($"Selected material: {material}");
    }

    private void OnLevelNameChanged(string newName)
    {
        _currentLevel.LevelName = newName;
    }

    private void OnCreatorNameChanged(string newName)
    {
        _currentLevel.CreatorName = newName;
    }

    public override void _Input(InputEvent @event)
    {
        if (_placementArea == null) return;

        if (@event is InputEventMouseButton mouseButton)
        {
            HandleMouseButton(mouseButton);
        }
        else if (@event is InputEventMouseMotion mouseMotion)
        {
            HandleMouseMotion(mouseMotion);
        }
    }

    private void HandleMouseButton(InputEventMouseButton mouseButton)
    {
        var mousePos = mouseButton.Position;

        if (mouseButton.ButtonIndex == MouseButton.Left && mouseButton.Pressed)
        {
            // Check if clicking in placement area
            if (_placementArea.GetGlobalRect().HasPoint(mousePos))
            {
                var localPos = _placementArea.GetGlobalTransform().AffineInverse() * mousePos;
                
                // Check if clicking existing obstacle
                var clickedObstacle = GetObstacleAtPosition(localPos);
                if (clickedObstacle != null)
                {
                    _selectedObstacle = clickedObstacle;
                    _isDragging = true;
                    _dragOffset = clickedObstacle.Position - localPos;
                }
                else
                {
                    // Place new obstacle
                    PlaceObstacle(localPos);
                }
            }
        }
        else if (mouseButton.ButtonIndex == MouseButton.Left && !mouseButton.Pressed)
        {
            _isDragging = false;
            _selectedObstacle = null;
        }
        else if (mouseButton.ButtonIndex == MouseButton.Right && mouseButton.Pressed)
        {
            // Delete obstacle
            if (_placementArea.GetGlobalRect().HasPoint(mousePos))
            {
                var localPos = _placementArea.GetGlobalTransform().AffineInverse() * mousePos;
                var clickedObstacle = GetObstacleAtPosition(localPos);
                if (clickedObstacle != null)
                {
                    DeleteObstacle(clickedObstacle);
                }
            }
        }
    }

    private void HandleMouseMotion(InputEventMouseMotion mouseMotion)
    {
        if (_isDragging && _selectedObstacle != null && _placementArea != null)
        {
            var mousePos = mouseMotion.Position;
            var localPos = _placementArea.GetGlobalTransform().AffineInverse() * mousePos;
            
            Vector2 newPos = localPos + _dragOffset;
            if (SNAP_TO_GRID)
            {
                newPos = SnapToGrid(newPos);
            }

            _selectedObstacle.Position = newPos;
            UpdateObstacleDataPosition(_selectedObstacle);
            UpdateDifficultyDisplay();
        }
    }

    private Node2D GetObstacleAtPosition(Vector2 position)
    {
        foreach (var obstacle in _obstacleNodes)
        {
            if (obstacle.Position.DistanceTo(position) < 30f)
            {
                return obstacle;
            }
        }
        return null;
    }

    private void PlaceObstacle(Vector2 position)
    {
        if (_currentLevel.Obstacles.Count >= 20)
        {
            ShowMessage("Maximum 20 obstacles allowed!");
            return;
        }

        if (SNAP_TO_GRID)
        {
            position = SnapToGrid(position);
        }

        // Create obstacle data
        var obstacleData = new CustomLevelData.ObstacleData
        {
            Material = _selectedMaterial,
            Position = position,
            Rotation = 0f,
            Scale = 1.0f,
            Id = _nextObstacleId++
        };

        _currentLevel.Obstacles.Add(obstacleData);

        // Create visual representation
        var obstacleNode = CreateObstacleNode(obstacleData);
        _placementArea.AddChild(obstacleNode);
        _obstacleNodes.Add(obstacleNode);

        UpdateObstacleList();
        UpdateDifficultyDisplay();

        GD.Print($"Placed obstacle {obstacleData.Id} at {position}");
    }

    private Node2D CreateObstacleNode(CustomLevelData.ObstacleData data)
    {
        var node = new Node2D();
        node.Position = data.Position;
        node.Rotation = data.Rotation;
        node.Scale = new Vector2(data.Scale, data.Scale);
        node.Name = $"Obstacle_{data.Id}";

        // Create visual representation
        var sprite = new ColorRect();
        sprite.Size = new Vector2(40, 40);
        sprite.Position = new Vector2(-20, -20);
        sprite.Color = GetMaterialColor(data.Material);
        node.AddChild(sprite);

        // Add label
        var label = new Label();
        label.Text = data.Material.ToString().Substring(0, 1);
        label.Position = new Vector2(-5, -5);
        node.AddChild(label);

        return node;
    }

    private Color GetMaterialColor(MaterialType material)
    {
        return material switch
        {
            MaterialType.Wood => new Color(0.6f, 0.4f, 0.2f),
            MaterialType.Stone => new Color(0.5f, 0.5f, 0.5f),
            MaterialType.Brick => new Color(0.7f, 0.3f, 0.2f),
            MaterialType.Iron => new Color(0.3f, 0.3f, 0.3f),
            MaterialType.Diamond => new Color(0.4f, 0.7f, 0.9f),
            _ => Colors.White
        };
    }

    private void DeleteObstacle(Node2D obstacle)
    {
        int id = int.Parse(obstacle.Name.ToString().Replace("Obstacle_", ""));
        
        var obstacleData = _currentLevel.Obstacles.FirstOrDefault(o => o.Id == id);
        if (obstacleData != null)
        {
            _currentLevel.Obstacles.Remove(obstacleData);
        }

        _obstacleNodes.Remove(obstacle);
        obstacle.QueueFree();

        UpdateObstacleList();
        UpdateDifficultyDisplay();

        GD.Print($"Deleted obstacle {id}");
    }

    private void UpdateObstacleDataPosition(Node2D obstacle)
    {
        int id = int.Parse(obstacle.Name.ToString().Replace("Obstacle_", ""));
        
        var obstacleData = _currentLevel.Obstacles.FirstOrDefault(o => o.Id == id);
        if (obstacleData != null)
        {
            obstacleData.Position = obstacle.Position;
        }
    }

    private void UpdateObstacleList()
    {
        if (_obstacleList == null) return;

        _obstacleList.Clear();
        foreach (var obstacle in _currentLevel.Obstacles)
        {
            _obstacleList.AddItem($"{obstacle.Material} - {obstacle.Id}");
        }

        // Update obstacle count label
        var countLabel = GetNodeOrNull<Label>("MainContainer/BottomBar/ObstacleCount");
        if (countLabel != null)
        {
            countLabel.Text = $"Obstacles: {_currentLevel.Obstacles.Count}/20";
        }
    }

    private void UpdateDifficultyDisplay()
    {
        if (_difficultyLabel == null) return;

        if (_currentLevel.Obstacles.Count < 3)
        {
            _difficultyLabel.Text = "Difficulty: N/A (Need 3+ obstacles)";
            return;
        }

        var difficulty = CustomLevelValidator.CalculateCustomLevelDifficulty(_currentLevel);
        _difficultyLabel.Text = $"Difficulty: {difficulty.Description} ({difficulty.OverallScore:F2})";
    }

    private Vector2 SnapToGrid(Vector2 position)
    {
        return new Vector2(
            Mathf.Round(position.X / GRID_SIZE) * GRID_SIZE,
            Mathf.Round(position.Y / GRID_SIZE) * GRID_SIZE
        );
    }

    private void OnClearPressed()
    {
        foreach (var obstacle in _obstacleNodes)
        {
            obstacle.QueueFree();
        }
        
        _obstacleNodes.Clear();
        _currentLevel.Obstacles.Clear();
        _nextObstacleId = 0;

        UpdateObstacleList();
        UpdateDifficultyDisplay();

        ShowMessage("Level cleared!");
    }

    private void OnValidatePressed()
    {
        var validation = CustomLevelValidator.ValidateLevel(_currentLevel);
        
        string message = validation.Message;
        if (validation.Warnings.Count > 0)
        {
            message += "\n\nWarnings:\n- " + string.Join("\n- ", validation.Warnings);
        }

        ShowMessage(message);
    }

    private void OnSavePressed()
    {
        // Validate first
        var validation = CustomLevelValidator.ValidateLevel(_currentLevel);
        
        if (!validation.IsValid)
        {
            ShowMessage($"Cannot save: {validation.Message}");
            return;
        }

        // Generate share code
        string shareCode = _currentLevel.ToBase64();
        
        if (shareCode != null)
        {
            // Save to local storage
            LocalLevelStorage.SaveDraft(_currentLevel);

            // Show share code dialog
            ShowShareCodeDialog(shareCode);
        }
        else
        {
            ShowMessage("Failed to generate share code!");
        }
    }

    private void ShowShareCodeDialog(string shareCode)
    {
        var dialog = new AcceptDialog();
        dialog.DialogText = $"Level saved!\n\nShare Code:\n{shareCode}\n\nCopy this code to share with friends!";
        dialog.Title = "Level Saved";
        
        AddChild(dialog);
        dialog.PopupCentered();
        
        // Copy to clipboard
        DisplayServer.ClipboardSet(shareCode);
        GD.Print($"Share code copied to clipboard: {shareCode}");
    }

    private void OnBackPressed()
    {
        GetTree().ChangeSceneToFile("res://Scenes/MainMenu.tscn");
    }

    private void ShowMessage(string message)
    {
        var dialog = new AcceptDialog();
        dialog.DialogText = message;
        dialog.Title = "Level Editor";
        
        AddChild(dialog);
        dialog.PopupCentered();
    }
}
