using Godot;
using System.Collections.Generic;

/// <summary>
/// LevelBase manages the overall level structure and progression through rooms.
/// Coordinates GUI, audio, particles, and area of interest handlers.
/// Manages transitions between slingshot phases and traversal phases across multiple rooms.
/// </summary>
public partial class LevelBase : Node2D
{
    // Configuration
    [Export] int _levelNumber = 1;
    [Export] PackedScene _currentRoomScene;
    [Export] Node2D _roomContainer; // Container for room instances
    [Export] CanvasLayer _guiRoot; // GUI elements
    [Export] Node _audioRoot; // Audio manager
    [Export] Node _particlesManager; // Particles manager
    [Export] Node _areaOfInterestHandler; // Camera/view handler

    private RoomBase _currentRoom;
    private int _currentRoomIndex = 0;
    private List<PackedScene> _roomScenes = new();
    private int _targetScore = 0; // Score needed to unlock next room

    // Signals for level progression
    [Signal]
    public delegate void OnLevelStartedEventHandler();

    [Signal]
    public delegate void OnRoomChangedEventHandler(int roomNumber);

    [Signal]
    public delegate void OnLevelCompletedEventHandler();

    [Signal]
    public delegate void OnBonusLevelUnlockedEventHandler();

    public override void _Ready()
    {
        // Initialize level
        ScoreManager.SetLevel(_levelNumber);

        // Find room container if not assigned
        if (_roomContainer == null)
        {
            _roomContainer = GetNodeOrNull<Node2D>("RoomContainer");
        }

        // Load the first room
        LoadRoom(_currentRoomIndex);

        // Connect signals
        ConnectSignals();

        // Emit level started signal
        EmitSignal(SignalName.OnLevelStarted);
    }

    public override void _Process(double delta)
    {
        // Allow returning to room selection with Q key
        if (Input.IsKeyPressed(Key.Q))
        {
            ReturnToRoomSelection();
        }
    }

    /// <summary>
    /// Connects relevant signals for level progression.
    /// </summary>
    private void ConnectSignals()
    {
        if (_currentRoom != null)
        {
            _currentRoom.Connect(RoomBase.SignalName.OnRoomCompleted, Callable.From((int roomNumber) => OnRoomCompleted(roomNumber)));
            _currentRoom.Connect(RoomBase.SignalName.OnRoomFailed, Callable.From(OnRoomFailed));
        }
    }

    /// <summary>
    /// Loads a room by index.
    /// </summary>
    private void LoadRoom(int roomIndex)
    {
        // If the room scene is not set, use a default configuration
        if (_currentRoomScene == null)
        {
            GD.PrintErr("Current room scene not assigned to LevelBase");
            return;
        }

        // Clear previous room if exists
        if (_currentRoom != null)
        {
            _currentRoom.QueueFree();
        }

        // Instantiate and add new room
        var roomInstance = _currentRoomScene.Instantiate<Node2D>();
        _currentRoom = roomInstance as RoomBase;

        if (_currentRoom != null)
        {
            _currentRoom.SetRoomNumber(roomIndex + 1);

            if (_roomContainer != null)
            {
                _roomContainer.AddChild(_currentRoom);
            }
            else
            {
                AddChild(_currentRoom);
            }

            _currentRoomIndex = roomIndex;

            // Connect the new room's signals
            ConnectSignals();

            // Emit room changed signal
            EmitSignal(SignalName.OnRoomChanged, roomIndex + 1);
        }
        else
        {
            GD.PrintErr($"Room instance is not a RoomBase: {roomInstance.GetClass()}");
        }
    }

    /// <summary>
    /// Called when a room is completed successfully.
    /// </summary>
    private void OnRoomCompleted(int roomNumber)
    {
        GD.Print($"Room {roomNumber} completed!");

        // Check if there are more rooms
        if (_currentRoomIndex < _roomScenes.Count - 1)
        {
            // Load next room
            LoadRoom(_currentRoomIndex + 1);
        }
        else
        {
            // Level completed
            OnLevelCompleted();
        }
    }

    /// <summary>
    /// Called when a room fails (e.g., StickClone dies).
    /// </summary>
    private void OnRoomFailed()
    {
        GD.Print($"Room {_currentRoomIndex + 1} failed!");
        // Could implement retry logic or reset room here
    }

    /// <summary>
    /// Called when the entire level is completed.
    /// </summary>
    private void OnLevelCompleted()
    {
        GD.Print($"Level {_levelNumber} completed!");
        EmitSignal(SignalName.OnLevelCompleted);

        // Transition to next level or return to selection
        LoadNextLevel();
    }

    /// <summary>
    /// Loads the next level or returns to room selection.
    /// Updates unlock flags in GameManager.
    /// </summary>
    private void LoadNextLevel()
    {
        int nextLevel = _levelNumber + 1;

        // Update unlock flags if there's a next level
        if (GameManager.IsLevelValid(nextLevel))
        {
            GameManager.UnlockLevel(nextLevel);
        }

        // Check for bonus level unlock
        if (GameManager.ShouldUnlockBonusLevel(_levelNumber))
        {
            EmitSignal(SignalName.OnBonusLevelUnlocked);
            GameManager.UnlockBonusLevel(_levelNumber);
        }

        // Return to room selection
        ReturnToRoomSelection();
    }

    /// <summary>
    /// Returns to the room selection scene.
    /// </summary>
    private void ReturnToRoomSelection()
    {
        GetTree().ChangeSceneToFile("res://Scenes/RoomSelection/RoomSelection.tscn");
    }

    /// <summary>
    /// Gets the current level number.
    /// </summary>
    public int GetLevelNumber() => _levelNumber;

    /// <summary>
    /// Gets the current room.
    /// </summary>
    public RoomBase GetCurrentRoom() => _currentRoom;

    /// <summary>
    /// Gets the current room index.
    /// </summary>
    public int GetCurrentRoomIndex() => _currentRoomIndex;
}
