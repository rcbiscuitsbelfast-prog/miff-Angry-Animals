using Godot;

/// <summary>
/// Main menu controller that handles the main menu interface.
/// Provides navigation to room selection, settings, and other menu options.
/// </summary>
public partial class MainMenu : Control
{
    [Signal] public delegate void PlayButtonPressedEventHandler();
    [Signal] public delegate void RoomSelectionButtonPressedEventHandler();
    [Signal] public delegate void SettingsButtonPressedEventHandler();
    [Signal] public delegate void QuitButtonPressedEventHandler();

    [Export] private NodePath _playButtonPath;
    [Export] private NodePath _roomSelectionButtonPath;
    [Export] private NodePath _settingsButtonPath;
    [Export] private NodePath _quitButtonPath;
    [Export] private NodePath _titleLabelPath;
    [Export] private NodePath _versionLabelPath;

    private Button? _playButton;
    private Button? _roomSelectionButton;
    private Button? _settingsButton;
    private Button? _quitButton;
    private Label? _titleLabel;
    private Label? _versionLabel;

    public override void _Ready()
    {
        InitializeMenu();
        ConnectSignals();
        SetupInputMap();
    }

    private void InitializeMenu()
    {
        _playButton = GetNodeOrNull<Button>(_playButtonPath);
        _roomSelectionButton = GetNodeOrNull<Button>(_roomSelectionButtonPath);
        _settingsButton = GetNodeOrNull<Button>(_settingsButtonPath);
        _quitButton = GetNodeOrNull<Button>(_quitButtonPath);
        _titleLabel = GetNodeOrNull<Label>(_titleLabelPath);
        _versionLabel = GetNodeOrNull<Label>(_versionLabelPath);

        // Set up title
        if (_titleLabel != null)
        {
            _titleLabel.Text = "Angry Animals";
        }

        // Set up version
        if (_versionLabel != null)
        {
            _versionLabel.Text = "Version 1.0.0";
        }

        // Connect button signals
        if (_playButton != null)
        {
            _playButton.Pressed += OnPlayButtonPressed;
        }

        if (_roomSelectionButton != null)
        {
            _roomSelectionButton.Pressed += OnRoomSelectionButtonPressed;
        }

        if (_settingsButton != null)
        {
            _settingsButton.Pressed += OnSettingsButtonPressed;
        }

        if (_quitButton != null)
        {
            _quitButton.Pressed += OnQuitButtonPressed;
        }
    }

    private void ConnectSignals()
    {
        // Connect to GameManager for state changes
        if (GameManager.Instance != null)
        {
            GameManager.Instance.GameStateChanged += OnGameStateChanged;
        }

        // Connect to AudioManager for UI sound effects
        var audioManager = GetNodeOrNull<AudioManager>("/root/AudioManager");
        if (audioManager != null)
        {
            // AudioManager handles its own button sound effects
        }
    }

    public override void _ExitTree()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.GameStateChanged -= OnGameStateChanged;
        }

        // Disconnect button signals
        if (_playButton != null)
        {
            _playButton.Pressed -= OnPlayButtonPressed;
        }

        if (_roomSelectionButton != null)
        {
            _roomSelectionButton.Pressed -= OnRoomSelectionButtonPressed;
        }

        if (_settingsButton != null)
        {
            _settingsButton.Pressed -= OnSettingsButtonPressed;
        }

        if (_quitButton != null)
        {
            _quitButton.Pressed -= OnQuitButtonPressed;
        }
    }

    private void SetupInputMap()
    {
        // Add menu navigation actions to InputMap if they don't exist
        if (!InputMap.HasAction("ui_menu_select"))
        {
            InputMap.AddAction("ui_menu_select");
            var selectEvent = new InputEventKey();
            selectEvent.Keycode = Key.Enter;
            InputMap.ActionAddEvent("ui_menu_select", selectEvent);
        }

        if (!InputMap.HasAction("ui_menu_back"))
        {
            InputMap.AddAction("ui_menu_back");
            var backEvent = new InputEventKey();
            backEvent.Keycode = Key.Escape;
            InputMap.ActionAddEvent("ui_menu_back", backEvent);
        }
    }

    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("ui_menu_select"))
        {
            HandleMenuSelection();
        }
        else if (@event.IsActionPressed("ui_menu_back"))
        {
            HandleMenuBack();
        }
    }

    private void HandleMenuSelection()
    {
        // Find the currently focused button and trigger it
        var focusedControl = GetViewport().GuiGetFocusOwner();
        if (focusedControl is Button focusedButton && focusedButton.Disabled == false)
        {
            focusedButton.EmitSignal("pressed");
        }
    }

    private void HandleMenuBack()
    {
        // Handle back navigation - could close settings panel or quit to main menu
        var settingsPanel = GetNodeOrNull<Control>("SettingsPanel");
        if (settingsPanel != null && settingsPanel.Visible)
        {
            settingsPanel.Visible = false;
        }
    }

    private void OnPlayButtonPressed()
    {
        GD.Print("Play button pressed");
        EmitSignal(SignalName.PlayButtonPressed);
        PlayUiClickSound();
        
        // Go to room selection (most common flow)
        GameManager.StartRoomByLevelNumber(1);
    }

    private void OnRoomSelectionButtonPressed()
    {
        GD.Print("Room selection button pressed");
        EmitSignal(SignalName.RoomSelectionButtonPressed);
        PlayUiClickSound();
        
        // Navigate to room selection scene
        // TODO: Load room selection scene
        GameManager.StartRoomByLevelNumber(1); // Temporary fallback
    }

    private void OnSettingsButtonPressed()
    {
        GD.Print("Settings button pressed");
        EmitSignal(SignalName.SettingsButtonPressed);
        PlayUiClickSound();
        
        // Show settings panel
        ShowSettingsPanel();
    }

    private void OnQuitButtonPressed()
    {
        GD.Print("Quit button pressed");
        EmitSignal(SignalName.QuitButtonPressed);
        PlayUiClickSound();
        
        // Quit the game
        GetTree().Quit();
    }

    private void OnGameStateChanged(GameManager.GameState state)
    {
        // Handle game state changes if needed
        switch (state)
        {
            case GameManager.GameState.MainMenu:
                // Ensure we're visible when at main menu
                Visible = true;
                break;
            case GameManager.GameState.InRoom:
            case GameManager.GameState.RoomComplete:
            case GameManager.GameState.Paused:
                // Hide when not at main menu
                Visible = false;
                break;
        }
    }

    private void ShowSettingsPanel()
    {
        // TODO: Implement settings panel
        GD.Print("Settings panel not yet implemented");
    }

    private void PlayUiClickSound()
    {
        var audioManager = GetNodeOrNull<AudioManager>("/root/AudioManager");
        audioManager?.PlayUiClickSound();
    }

    /// <summary>
    /// Sets whether the menu should be visible.
    /// </summary>
    /// <param name="visible">Whether the menu should be visible</param>
    public void SetMenuVisible(bool visible)
    {
        Visible = visible;
    }

    /// <summary>
    /// Updates the menu with the latest game state.
    /// </summary>
    public void RefreshMenu()
    {
        // Refresh any dynamic content
        if (PlayerProfile.Instance != null)
        {
            // Update any player-specific information
        }
    }
}