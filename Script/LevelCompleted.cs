using Godot;

/// <summary>
/// Handles the level completion screen and UI.
/// Displays final score, new records, and provides options for next actions.
/// </summary>
public partial class LevelCompleted : Control
{
    [Signal] public delegate void NextLevelButtonPressedEventHandler();
    [Signal] public delegate void RestartLevelButtonPressedEventHandler();
    [Signal] public delegate void RoomSelectionButtonPressedEventHandler();
    [Signal] public delegate void MainMenuButtonPressedEventHandler();

    [Export] private NodePath _panelPath;
    [Export] private NodePath _titleLabelPath;
    [Export] private NodePath _finalScoreLabelPath;
    [Export] private NodePath _bestScoreLabelPath;
    [Export] private NodePath _newRecordLabelPath;
    [Export] private NodePath _nextLevelButtonPath;
    [Export] private NodePath _restartLevelButtonPath;
    [Export] private NodePath _roomSelectionButtonPath;
    [Export] private NodePath _mainMenuButtonPath;
    [Export] private NodePath _starsContainerPath;

    private Panel? _panel;
    private Label? _titleLabel;
    private Label? _finalScoreLabel;
    private Label? _bestScoreLabel;
    private Label? _newRecordLabel;
    private Button? _nextLevelButton;
    private Button? _restartLevelButton;
    private Button? _roomSelectionButton;
    private Button? _mainMenuButton;
    private HBoxContainer? _starsContainer;

    private int _currentLevel;
    private int _finalScore;
    private int _bestScore;
    private bool _isNewRecord;

    public override void _Ready()
    {
        InitializeCompletionScreen();
        ConnectSignals();
        SetupStarAnimation();
    }

    private void InitializeCompletionScreen()
    {
        _panel = GetNodeOrNull<Panel>(_panelPath);
        _titleLabel = GetNodeOrNull<Label>(_titleLabelPath);
        _finalScoreLabel = GetNodeOrNull<Label>(_finalScoreLabelPath);
        _bestScoreLabel = GetNodeOrNull<Label>(_bestScoreLabelPath);
        _newRecordLabel = GetNodeOrNull<Label>(_newRecordLabelPath);
        _nextLevelButton = GetNodeOrNull<Button>(_nextLevelButtonPath);
        _restartLevelButton = GetNodeOrNull<Button>(_restartLevelButtonPath);
        _roomSelectionButton = GetNodeOrNull<Button>(_roomSelectionButtonPath);
        _mainMenuButton = GetNodeOrNull<Button>(_mainMenuButtonPath);
        _starsContainer = GetNodeOrNull<HBoxContainer>(_starsContainerPath);

        // Initially hide the panel
        if (_panel != null)
        {
            _panel.Visible = false;
        }

        // Set up button connections
        if (_nextLevelButton != null)
        {
            _nextLevelButton.Pressed += OnNextLevelButtonPressed;
        }

        if (_restartLevelButton != null)
        {
            _restartLevelButton.Pressed += OnRestartLevelButtonPressed;
        }

        if (_roomSelectionButton != null)
        {
            _roomSelectionButton.Pressed += OnRoomSelectionButtonPressed;
        }

        if (_mainMenuButton != null)
        {
            _mainMenuButton.Pressed += OnMainMenuButtonPressed;
        }

        // Hide new record label initially
        if (_newRecordLabel != null)
        {
            _newRecordLabel.Visible = false;
        }
    }

    private void ConnectSignals()
    {
        // Connect to GameManager for room completion events
        if (GameManager.Instance != null)
        {
            GameManager.Instance.GameStateChanged += OnGameStateChanged;
            GameManager.Instance.RoomCompleted += OnRoomCompleted;
        }

        // Connect to ScoreManager for score data
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.ScoreChanged += OnScoreChanged;
        }

        // Connect to SignalManager for level completion events
        if (SignalManager.Instance != null)
        {
            SignalManager.Instance.OnLevelCompleted += OnLevelCompleted;
        }
    }

    public override void _ExitTree()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.GameStateChanged -= OnGameStateChanged;
            GameManager.Instance.RoomCompleted -= OnRoomCompleted;
        }

        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.ScoreChanged -= OnScoreChanged;
        }

        if (SignalManager.Instance != null)
        {
            SignalManager.Instance.OnLevelCompleted -= OnLevelCompleted;
        }

        // Disconnect button signals
        if (_nextLevelButton != null)
        {
            _nextLevelButton.Pressed -= OnNextLevelButtonPressed;
        }

        if (_restartLevelButton != null)
        {
            _restartLevelButton.Pressed -= OnRestartLevelButtonPressed;
        }

        if (_roomSelectionButton != null)
        {
            _roomSelectionButton.Pressed -= OnRoomSelectionButtonPressed;
        }

        if (_mainMenuButton != null)
        {
            _mainMenuButton.Pressed -= OnMainMenuButtonPressed;
        }
    }

    private void SetupStarAnimation()
    {
        // Set up timer for delayed star animation
        var timer = new Timer();
        timer.WaitTime = 0.5;
        timer.OneShot = true;
        timer.Timeout += AnimateStars;
        AddChild(timer);
    }

    private void OnRoomCompleted(int roomIndex)
    {
        _currentLevel = roomIndex + 1; // Convert to 1-based level number
        _finalScore = ScoreManager.GetScore();
        _bestScore = ScoreManager.GetLevelBestScore(_currentLevel);
        _isNewRecord = _finalScore < _bestScore;

        // Update the UI with completion data
        UpdateCompletionUI();
        ShowCompletionPanel();
        AnimateStars();
        
        PlayCompletionSound();
    }

    private void OnLevelCompleted()
    {
        // This is called when all projectiles are used up
        // Room completion is handled by GameManager.RoomCompleted signal
    }

    private void OnScoreChanged(int score)
    {
        // Update final score if needed during completion screen
        if (IsVisibleInTree())
        {
            _finalScore = score;
            if (_finalScoreLabel != null)
            {
                _finalScoreLabel.Text = $"Final Score: {score}";
            }
        }
    }

    private void OnGameStateChanged(GameManager.GameState state)
    {
        switch (state)
        {
            case GameManager.GameState.RoomComplete:
                Visible = true;
                break;
            default:
                Visible = false;
                break;
        }
    }

    private void UpdateCompletionUI()
    {
        // Update title
        if (_titleLabel != null)
        {
            _titleLabel.Text = $"Room {_currentLevel} Complete!";
        }

        // Update final score
        if (_finalScoreLabel != null)
        {
            _finalScoreLabel.Text = $"Final Score: {_finalScore}";
        }

        // Update best score
        if (_bestScoreLabel != null)
        {
            _bestScoreLabel.Text = $"Best Score: {_bestScore}";
        }

        // Show/hide new record label
        if (_newRecordLabel != null)
        {
            _newRecordLabel.Visible = _isNewRecord;
            if (_isNewRecord)
            {
                _newRecordLabel.Text = "🎉 NEW RECORD! 🎉";
                _newRecordLabel.Modulate = Colors.Gold;
            }
        }

        // Update next level button availability
        UpdateNextLevelButton();
    }

    private void UpdateNextLevelButton()
    {
        if (_nextLevelButton == null)
            return;

        var currentRoomIndex = _currentLevel - 1;
        var hasNextRoom = currentRoomIndex + 1 < GameManager.Instance.Rooms.Length;
        var isNextRoomUnlocked = PlayerProfile.IsRoomUnlocked(currentRoomIndex + 1);

        if (hasNextRoom && isNextRoomUnlocked)
        {
            _nextLevelButton.Text = $"Next: Room {_currentLevel + 1}";
            _nextLevelButton.Disabled = false;
        }
        else if (hasNextRoom)
        {
            _nextLevelButton.Text = "Next Room (Locked)";
            _nextLevelButton.Disabled = true;
            _nextLevelButton.TooltipText = "Complete previous rooms to unlock";
        }
        else
        {
            _nextLevelButton.Text = "Game Complete!";
            _nextLevelButton.Disabled = true;
        }
    }

    private void ShowCompletionPanel()
    {
        if (_panel != null)
        {
            _panel.Visible = true;
        }
    }

    private void AnimateStars()
    {
        if (_starsContainer == null)
            return;

        // Calculate star rating based on performance
        var starCount = CalculateStarCount();
        
        // Animate stars one by one
        for (int i = 0; i < _starsContainer.GetChildCount(); i++)
        {
            var star = _starsContainer.GetChild(i);
            if (star is Label starLabel)
            {
                if (i < starCount)
                {
                    starLabel.Text = "⭐";
                    starLabel.Scale = Vector2.Zero;
                    
                    // Animate star appearance
                    var tween = CreateTween();
                    tween.TweenProperty(starLabel, "scale", Vector2.One, 0.3).SetTrans(Tween.TransitionType.Bounce);
                }
                else
                {
                    starLabel.Text = "☆";
                    starLabel.Modulate = Colors.Gray;
                }
            }
        }
    }

    private int CalculateStarCount()
    {
        // Star calculation based on score vs target score
        // Higher score = more stars (score represents destruction achieved)
        // 3 stars: Score >= 3x target (exceptional destruction)
        // 2 stars: Score >= 2x target (good destruction)
        // 1 star: Score >= target score (minimum completion)
        // 0 stars: Score < target (incomplete)

        int targetScore = GameManager.Instance?.GetCurrentTargetScore() ?? 3;

        if (_finalScore >= targetScore * 3)
            return 3;
        else if (_finalScore >= targetScore * 2)
            return 2;
        else if (_finalScore >= targetScore)
            return 1;

        return 0; // No stars if target not reached
    }

    private void PlayCompletionSound()
    {
        // Play completion sound effect
        var audioManager = GetNodeOrNull<AudioManager>("/root/AudioManager");
        audioManager?.PlayComboSoundSfx();
    }

    // Button event handlers
    private void OnNextLevelButtonPressed()
    {
        GD.Print("Next level button pressed");
        EmitSignal(SignalName.NextLevelButtonPressed);
        PlayUiClickSound();

        var currentRoomIndex = _currentLevel - 1;
        if (currentRoomIndex + 1 < GameManager.Instance.Rooms.Length)
        {
            GameManager.StartRoom(currentRoomIndex + 1);
        }
    }

    private void OnRestartLevelButtonPressed()
    {
        GD.Print("Restart level button pressed");
        EmitSignal(SignalName.RestartLevelButtonPressed);
        PlayUiClickSound();

        GameManager.RestartRoom();
    }

    private void OnRoomSelectionButtonPressed()
    {
        GD.Print("Room selection button pressed");
        EmitSignal(SignalName.RoomSelectionButtonPressed);
        PlayUiClickSound();

        GameManager.LoadMain();
    }

    private void OnMainMenuButtonPressed()
    {
        GD.Print("Main menu button pressed");
        EmitSignal(SignalName.MainMenuButtonPressed);
        PlayUiClickSound();

        GameManager.LoadMain();
    }

    private void PlayUiClickSound()
    {
        var audioManager = GetNodeOrNull<AudioManager>("/root/AudioManager");
        audioManager?.PlayUiClickSoundSfx();
    }

    /// <summary>
    /// Shows the completion screen with specified data.
    /// </summary>
    /// <param name="levelNumber">The level that was completed</param>
    /// <param name="finalScore">The final score achieved</param>
    /// <param name="bestScore">The best score for this level</param>
    public void ShowCompletion(int levelNumber, int finalScore, int bestScore)
    {
        _currentLevel = levelNumber;
        _finalScore = finalScore;
        _bestScore = bestScore;
        _isNewRecord = finalScore < bestScore;

        UpdateCompletionUI();
        ShowCompletionPanel();
        
        CallDeferred(nameof(AnimateStars));
    }

    /// <summary>
    /// Hides the completion screen.
    /// </summary>
    public void HideCompletion()
    {
        if (_panel != null)
        {
            _panel.Visible = false;
        }
        Visible = false;
    }
}