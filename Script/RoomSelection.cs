using Godot;
using System.Collections.Generic;

/// <summary>
/// RoomSelection manages the room/level selection screen.
/// Displays available rooms with unlock status and best scores.
/// Handles room selection and progression to LevelBase.
/// </summary>
public partial class RoomSelection : Node2D
{
    [Export] Node _roomButtonsContainer; // Container for room selection buttons
    [Export] Label _titleLabel;
    [Export] Label _descriptionLabel;
    [Export] int _maxRooms = 10;

    private Dictionary<int, RoomButton> _roomButtons = new();

    public override void _Ready()
    {
        // Find room buttons container if not assigned
        if (_roomButtonsContainer == null)
        {
            var container = GetNodeOrNull<Node>("CanvasLayer/UI/VBoxContainer/RoomButtons");
            if (container != null)
            {
                _roomButtonsContainer = container;
            }
        }

        // Find title and description labels
        if (_titleLabel == null)
        {
            _titleLabel = GetNodeOrNull<Label>("CanvasLayer/UI/VBoxContainer/Title");
        }

        if (_descriptionLabel == null)
        {
            _descriptionLabel = GetNodeOrNull<Label>("CanvasLayer/UI/VBoxContainer/Description");
        }

        // Initialize room selection UI
        InitializeRoomButtons();
        UpdateUnlockStatus();
    }

    public override void _Process(double delta)
    {
        // Allow returning to main menu
        if (Input.IsKeyPressed(Key.Escape))
        {
            GameManager.LoadMain();
        }
    }

    /// <summary>
    /// Initializes all room selection buttons.
    /// </summary>
    private void InitializeRoomButtons()
    {
        if (_roomButtonsContainer == null)
        {
            GD.PrintErr("RoomButtonsContainer not assigned");
            return;
        }

        // Get all button children
        foreach (Node child in _roomButtonsContainer.GetChildren())
        {
            if (child is RoomButton roomButton)
            {
                int roomNumber = roomButton.GetRoomNumber();
                _roomButtons[roomNumber] = roomButton;

                // Connect button pressed signal
                roomButton.Connect(BaseButton.SignalName.Pressed, Callable.From(() => OnRoomSelected(roomNumber)));
            }
        }
    }

    /// <summary>
    /// Updates the unlock status of all rooms.
    /// </summary>
    private void UpdateUnlockStatus()
    {
        for (int i = 1; i <= _maxRooms; i++)
        {
            if (_roomButtons.TryGetValue(i, out RoomButton button))
            {
                bool isUnlocked = GameManager.IsLevelUnlocked(i);
                button.SetUnlocked(isUnlocked);

                if (isUnlocked)
                {
                    int bestScore = ScoreManager.GetLevelBestScore(i);
                    button.SetBestScore(bestScore);
                }
            }
        }
    }

    /// <summary>
    /// Called when a room is selected.
    /// </summary>
    private void OnRoomSelected(int roomNumber)
    {
        // Check if room is unlocked
        if (!GameManager.IsLevelUnlocked(roomNumber))
        {
            GD.Print($"Room {roomNumber} is locked");
            return;
        }

        // Set the current level and load it
        ScoreManager.SetLevel(roomNumber);
        LoadLevel(roomNumber);
    }

    /// <summary>
    /// Loads a level by number.
    /// </summary>
    private void LoadLevel(int levelNumber)
    {
        // Try to load the level scene
        string levelPath = $"res://Scenes/Levels/Level{levelNumber}/LevelBase.tscn";
        var packedScene = GD.Load<PackedScene>(levelPath);

        if (packedScene != null)
        {
            GetTree().ChangeSceneToPacked(packedScene);
        }
        else
        {
            GD.PrintErr($"Failed to load level scene: {levelPath}");
        }
    }

    /// <summary>
    /// Gets the unlock status of a room.
    /// </summary>
    public bool IsRoomUnlocked(int roomNumber) => GameManager.IsLevelUnlocked(roomNumber);
}

/// <summary>
/// RoomButton represents a single room selection button.
/// Displays room number, unlock status, and best score.
/// </summary>
public partial class RoomButton : TextureButton
{
    [Export] int _roomNumber = 1;
    [Export] Label _roomNumberLabel;
    [Export] Label _scoreLabel;
    [Export] Label _lockLabel;
    [Export] Color _lockedColor = Colors.Gray;
    [Export] Color _unlockedColor = Colors.White;

    private bool _isUnlocked = false;

    public override void _Ready()
    {
        if (_roomNumberLabel == null)
        {
            // Try to find the label in children
            _roomNumberLabel = GetNodeOrNull<Label>("Label");
        }

        if (_roomNumberLabel != null)
        {
            _roomNumberLabel.Text = _roomNumber.ToString();
        }

        if (_scoreLabel == null)
        {
            _scoreLabel = GetNodeOrNull<Label>("ScoreLabel");
        }

        if (_lockLabel == null)
        {
            _lockLabel = GetNodeOrNull<Label>("LockLabel");
        }

        // Connect hover events for visual feedback
        MouseEntered += OnMouseEntered;
        MouseExited += OnMouseExited;
    }

    /// <summary>
    /// Sets the unlock status of this button.
    /// </summary>
    public void SetUnlocked(bool unlocked)
    {
        _isUnlocked = unlocked;

        if (_lockLabel != null)
        {
            _lockLabel.Visible = !unlocked;
        }

        Disabled = !unlocked;

        if (_roomNumberLabel != null)
        {
            _roomNumberLabel.Modulate = unlocked ? _unlockedColor : _lockedColor;
        }
    }

    /// <summary>
    /// Sets the best score display.
    /// </summary>
    public void SetBestScore(int score)
    {
        if (_scoreLabel != null)
        {
            _scoreLabel.Text = score.ToString("D4");
        }
    }

    /// <summary>
    /// Gets the room number this button represents.
    /// </summary>
    public int GetRoomNumber() => _roomNumber;

    private void OnMouseEntered()
    {
        if (_isUnlocked)
        {
            Scale = new Vector2(1.1f, 1.1f);
        }
    }

    private void OnMouseExited()
    {
        Scale = Vector2.One;
    }
}
