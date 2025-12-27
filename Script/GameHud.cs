using Godot;

/// <summary>
/// Main HUD controller that manages all heads-up display elements.
/// Handles attempts counter, rage bar, combo counter, and pause functionality.
/// Listens to SignalManager and RageSystem for real-time updates.
/// </summary>
public partial class GameHud : Control
{
    [Signal] public delegate void PauseRequestedEventHandler();
    [Signal] public delegate void ResumeRequestedEventHandler();

    [Export] private NodePath _attemptsLabelPath;
    [Export] private NodePath _rageBarPath;
    [Export] private NodePath _rageFillPath;
    [Export] private NodePath _comboLabelPath;
    [Export] private NodePath _pauseButtonPath;
    [Export] private NodePath _pausePanelPath;
    [Export] private NodePath _resumeButtonPath;
    [Export] private NodePath _quitButtonPath;
    [Export] private NodePath _scoreLabelPath;

    private Label? _attemptsLabel;
    private ProgressBar? _rageBar;
    private ColorRect? _rageFill;
    private Label? _comboLabel;
    private Button? _pauseButton;
    private Panel? _pausePanel;
    private Button? _resumeButton;
    private Button? _quitButton;
    private Label? _scoreLabel;

    private RageSystem? _rageSystem;

    public override void _Ready()
    {
        InitializeHUD();
        ConnectSignals();
        SetupInputMap();
        ShowBannerAd();
    }

    private void InitializeHUD()
    {
        _attemptsLabel = GetNodeOrNull<Label>(_attemptsLabelPath);
        _rageBar = GetNodeOrNull<ProgressBar>(_rageBarPath);
        _rageFill = GetNodeOrNull<ColorRect>(_rageFillPath);
        _comboLabel = GetNodeOrNull<Label>(_comboLabelPath);
        _pauseButton = GetNodeOrNull<Button>(_pauseButtonPath);
        _pausePanel = GetNodeOrNull<Panel>(_pausePanelPath);
        _resumeButton = GetNodeOrNull<Button>(_resumeButtonPath);
        _quitButton = GetNodeOrNull<Button>(_quitButtonPath);
        _scoreLabel = GetNodeOrNull<Label>(_scoreLabelPath);

        // Initialize UI elements
        if (_pausePanel != null)
        {
            _pausePanel.Visible = false;
        }

        // Get reference to RageSystem
        _rageSystem = GetNodeOrNull<RageSystem>("/root/RageSystem");

        // Initialize values
        UpdateAttemptsLabel(0);
        UpdateRageBar(0f);
        UpdateComboLabel(0);
        UpdateScoreLabel(0);
    }

    private void ConnectSignals()
    {
        // Connect to SignalManager for game events
        if (SignalManager.Instance != null)
        {
            SignalManager.Instance.OnAttemptMade += OnAttemptMade;
            SignalManager.Instance.OnScoreUpdated += OnScoreUpdated;
            SignalManager.Instance.OnDestructionScoreUpdated += OnDestructionScoreUpdated;
            SignalManager.Instance.OnLevelCompleted += OnLevelCompleted;
        }

        // Connect to RageSystem for rage/combo updates
        if (_rageSystem != null)
        {
            _rageSystem.RageChanged += OnRageChanged;
            _rageSystem.ComboChanged += OnComboChanged;
        }

        // Connect to GameManager for game state changes
        if (GameManager.Instance != null)
        {
            GameManager.Instance.GameStateChanged += OnGameStateChanged;
            GameManager.Instance.RoomStarted += OnRoomStarted;
            GameManager.Instance.RoomCompleted += OnRoomCompleted;
        }

        // Connect to ScoreManager for score updates
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.ScoreChanged += OnScoreManagerScoreChanged;
        }

        // Connect button signals
        if (_pauseButton != null)
        {
            _pauseButton.Pressed += OnPauseButtonPressed;
        }

        if (_resumeButton != null)
        {
            _resumeButton.Pressed += OnResumeButtonPressed;
        }

        if (_quitButton != null)
        {
            _quitButton.Pressed += OnQuitButtonPressed;
        }
    }

    public override void _ExitTree()
    {
        // Disconnect all signals to prevent memory leaks
        if (SignalManager.Instance != null)
        {
            SignalManager.Instance.OnAttemptMade -= OnAttemptMade;
            SignalManager.Instance.OnScoreUpdated -= OnScoreUpdated;
            SignalManager.Instance.OnDestructionScoreUpdated -= OnDestructionScoreUpdated;
            SignalManager.Instance.OnLevelCompleted -= OnLevelCompleted;
        }

        if (_rageSystem != null)
        {
            _rageSystem.RageChanged -= OnRageChanged;
            _rageSystem.ComboChanged -= OnComboChanged;
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.GameStateChanged -= OnGameStateChanged;
            GameManager.Instance.RoomStarted -= OnRoomStarted;
            GameManager.Instance.RoomCompleted -= OnRoomCompleted;
        }

        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.ScoreChanged -= OnScoreManagerScoreChanged;
        }

        if (_pauseButton != null)
        {
            _pauseButton.Pressed -= OnPauseButtonPressed;
        }

        if (_resumeButton != null)
        {
            _resumeButton.Pressed -= OnResumeButtonPressed;
        }

        if (_quitButton != null)
        {
            _quitButton.Pressed -= OnQuitButtonPressed;
        }

        HideBannerAd();
    }

    private void SetupInputMap()
    {
        // Add pause action to InputMap if it doesn't exist
        if (!InputMap.HasAction("ui_pause"))
        {
            InputMap.AddAction("ui_pause");
            var pauseEvent = new InputEventKey();
            pauseEvent.Keycode = Key.Escape;
            InputMap.ActionAddEvent("ui_pause", pauseEvent);
        }
    }

    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("ui_pause"))
        {
            TogglePause();
        }
    }

    private void OnAttemptMade()
    {
        // Get current attempts from ScoreManager
        var currentAttempts = ScoreManager.GetAttempts();
        UpdateAttemptsLabel(currentAttempts);
    }

    private void OnScoreUpdated(int score)
    {
        UpdateScoreLabel(score);
    }

    private void OnDestructionScoreUpdated(int score)
    {
        UpdateScoreLabel(score);
    }

    private void OnLevelCompleted()
    {
        HideBannerAd();

        // Hide pause button when level is complete
        if (_pauseButton != null)
            _pauseButton.Visible = false;
    }

    private void OnRageChanged(float rage)
    {
        UpdateRageBar(rage);
        
        // Change rage bar color based on rage level
        if (_rageFill != null)
        {
            Color rageColor = rage switch
            {
                >= 80f => Colors.Red,
                >= 60f => Colors.Orange,
                >= 40f => Colors.Yellow,
                >= 20f => Colors.LightBlue,
                _ => Colors.Green
            };
            _rageFill.Modulate = rageColor;
        }
    }

    private void OnComboChanged(int combo)
    {
        UpdateComboLabel(combo);
        
        // Show combo popup if combo is significant
        if (combo >= 3)
        {
            ShowComboPopup(combo);
        }
    }

    private void OnGameStateChanged(GameManager.GameState state)
    {
        switch (state)
        {
            case GameManager.GameState.Paused:
                OnGamePaused();
                ShowPausePanel();
                break;
            case GameManager.GameState.InRoom:
                HidePausePanel();
                OnLevelStarted();
                break;
            case GameManager.GameState.RoomComplete:
                HidePausePanel();
                HideBannerAd();
                break;
        }
    }

    private void OnScoreManagerScoreChanged(int score)
    {
        UpdateScoreLabel(score);
    }

    private void UpdateAttemptsLabel(int attempts)
    {
        if (_attemptsLabel != null)
        {
            _attemptsLabel.Text = $"Attempts: {attempts}";
        }
    }

    private void UpdateRageBar(float rage)
    {
        if (_rageBar != null)
        {
            _rageBar.Value = rage;
        }
    }

    private void UpdateComboLabel(int combo)
    {
        if (_comboLabel != null)
        {
            if (combo > 1)
            {
                _comboLabel.Text = $"COMBO x{combo}";
                _comboLabel.Visible = true;
            }
            else
            {
                _comboLabel.Visible = false;
            }
        }
    }

    private void UpdateScoreLabel(int score)
    {
        if (_scoreLabel != null)
        {
            _scoreLabel.Text = $"Score: {score}";
        }
    }

    private void ShowComboPopup(int combo)
    {
        // TODO: Create animated combo popup
        GD.Print($"Combo x{combo}!");
    }

    private void OnPauseButtonPressed()
    {
        TogglePause();
    }

    private void OnResumeButtonPressed()
    {
        ResumeGame();
    }

    private void OnQuitButtonPressed()
    {
        GameManager.LoadMain();
    }

    private void TogglePause()
    {
        if (GameManager.Instance?.GetTree().Paused == true)
        {
            ResumeGame();
        }
        else
        {
            PauseGame();
        }
    }

    private void PauseGame()
    {
        GameManager.PauseGame();
        EmitSignal(SignalName.PauseRequested);
    }

    private void ResumeGame()
    {
        GameManager.ResumeGame();
        EmitSignal(SignalName.ResumeRequested);
    }

    private void ShowPausePanel()
    {
        if (_pausePanel != null)
        {
            _pausePanel.Visible = true;
        }
    }

    private void HidePausePanel()
    {
        if (_pausePanel != null)
        {
            _pausePanel.Visible = false;
        }
    }

    private void OnRoomStarted(int roomIndex)
    {
        OnLevelStarted();
    }

    private void OnRoomCompleted(int roomIndex)
    {
        HideBannerAd();
    }

    /// <summary>
    /// Called when a playable level starts.
    /// </summary>
    private void OnLevelStarted()
    {
        ShowBannerAd();
    }

    /// <summary>
    /// Called when the game is paused.
    /// </summary>
    private void OnGamePaused()
    {
        HideBannerAd();
    }

    /// <summary>
    /// Shows a banner ad at the bottom of the screen when the player is in a playable room.
    /// </summary>
    private void ShowBannerAd()
    {
        if (MonetizationManager.Instance?.ShowAds != true)
            return;

        AdsManager.Instance?.ShowBannerAd();
    }

    /// <summary>
    /// Hides the banner ad.
    /// </summary>
    private void HideBannerAd()
    {
        AdsManager.Instance?.HideBannerAd();
    }

    /// <summary>
    /// Shows or hides the HUD elements based on game state.
    /// </summary>
    /// <param name="visible">Whether the HUD should be visible</param>
    public void SetHudVisible(bool visible)
    {
        Visible = visible;
    }

    /// <summary>
    /// Updates all HUD elements with current game state.
    /// </summary>
    public void RefreshHud()
    {
        if (ScoreManager.Instance != null)
        {
            UpdateAttemptsLabel(ScoreManager.GetAttempts());
            UpdateScoreLabel(ScoreManager.GetScore());
        }

        if (_rageSystem != null)
        {
            UpdateRageBar(_rageSystem.Rage);
            UpdateComboLabel(_rageSystem.Combo);
        }
    }
}