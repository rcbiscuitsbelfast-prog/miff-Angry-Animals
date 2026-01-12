using System;
using Godot;

/// <summary>
/// Main menu controller that handles the main menu interface.
/// Provides navigation to room selection, settings, and other menu options.
/// </summary>
public partial class MainMenu : CanvasLayer
{
    [Signal] public delegate void PlayButtonPressedEventHandler();
    [Signal] public delegate void RoomSelectionButtonPressedEventHandler();
    [Signal] public delegate void SettingsButtonPressedEventHandler();
    [Signal] public delegate void QuitButtonPressedEventHandler();

    [Export] private NodePath _playButton;
    [Export] private NodePath _roomSelectionButton;
    [Export] private NodePath _settingsButton;
    [Export] private NodePath _quitButton;
    [Export] private NodePath _titleLabel;
    [Export] private NodePath _versionLabel;

    private Button? _playButtonNode;
    private Button? _roomSelectionButtonNode;
    private Button? _settingsButtonNode;
    private Button? _quitButtonNode;
    private Label? _titleLabelNode;
    private Label? _versionLabelNode;

    private Button? _unlockFullGameButton;
    private ConfirmationDialog? _unlockConfirmation;
    private AcceptDialog? _purchaseCompleteDialog;
    private ConfirmationDialog? _purchaseFailedDialog;

    private bool _purchaseInProgress;

    public override void _Ready()
    {
        InitializeMenu();
        AddDailyChallengeButton();
        AddCustomizeFaceButton();
        AddLevelEditorButtons();
        AddUnlockFullGameButton();
        AddRetentionSystemButtons(); // Add retention system buttons
        AddTelemetryMetricsButton(); // Add telemetry button for debug builds
        ConnectSignals();
        SetupInputMap();
        
        // Initialize retention systems
        InitializeRetentionSystems();
    }

    private void AddDailyChallengeButton()
    {
        if (_playButtonNode != null && _playButtonNode.GetParent() is Control container)
        {
            var dailyBtn = new Button
            {
                Text = "Daily Challenge",
                Name = "DailyChallengeButton",
                Modulate = new Color(0.5f, 1f, 0.5f)
            };
            dailyBtn.Pressed += OnDailyChallengeButtonPressed;

            container.AddChild(dailyBtn);

            if (_playButtonNode != null)
                container.MoveChild(dailyBtn, _playButtonNode.GetIndex() + 1);
        }
    }

    private void OnDailyChallengeButtonPressed()
    {
        GD.Print("Daily Challenge button pressed");
        PlayUiClickSound();

        DailyChallengeManager.Instance?.StartDailyChallenge();
    }

    private void AddCustomizeFaceButton()
    {
        if (_playButtonNode != null && _playButtonNode.GetParent() is Control container)
        {
            var customizeBtn = new Button
            {
                Text = "Customize Face",
                Name = "CustomizeFaceButton"
            };
            customizeBtn.Pressed += OnCustomizeFaceButtonPressed;

            container.AddChild(customizeBtn);

            if (_roomSelectionButtonNode != null)
                container.MoveChild(customizeBtn, _roomSelectionButtonNode.GetIndex() + 1);
        }
    }

    private void AddLevelEditorButtons()
    {
        if (_playButtonNode != null && _playButtonNode.GetParent() is Control container)
        {
            var createLevelBtn = new Button
            {
                Text = "Create Level",
                Name = "CreateLevelButton",
                Modulate = new Color(0.5f, 0.8f, 1f)
            };
            createLevelBtn.Pressed += OnCreateLevelButtonPressed;
            container.AddChild(createLevelBtn);

            var playCustomBtn = new Button
            {
                Text = "📁 Play Custom Levels",
                Name = "PlayCustomLevelsButton",
                Modulate = new Color(0.7f, 0.5f, 1f)
            };
            playCustomBtn.Pressed += OnPlayCustomLevelsButtonPressed;
            container.AddChild(playCustomBtn);

            // "Generate 100 Levels" button
            var generateLevelsBtn = new Button
            {
                Text = "🎲 Generate 100 Levels",
                Name = "GenerateLevelsButton",
                Modulate = new Color(1f, 0.8f, 0.2f)
            };
            generateLevelsBtn.Pressed += OnGenerateLevelsButtonPressed;
            container.AddChild(generateLevelsBtn);

            if (_roomSelectionButtonNode != null)
            {
                container.MoveChild(createLevelBtn, _roomSelectionButtonNode.GetIndex() + 1);
                container.MoveChild(playCustomBtn, _roomSelectionButtonNode.GetIndex() + 2);
                container.MoveChild(generateLevelsBtn, _roomSelectionButtonNode.GetIndex() + 3);
            }
        }
    }

    private void OnCreateLevelButtonPressed()
    {
        GD.Print("Create Level button pressed");
        PlayUiClickSound();

        GetTree().ChangeSceneToFile("res://Scenes/LevelEditor/LevelEditor.tscn");
    }

    private void OnPlayCustomLevelButtonPressed()
    {
        GD.Print("Play Custom Level button pressed");
        PlayUiClickSound();

        CustomLevelInputDialog.ShowDialog(this);
    }

    private void OnPlayCustomLevelsButtonPressed()
    {
        GD.Print("Play Custom Levels button pressed");
        PlayUiClickSound();

        GetTree().ChangeSceneToFile("res://Scenes/Levels/LevelBrowser.tscn");
    }

    private void OnGenerateLevelsButtonPressed()
    {
        GD.Print("Generate 100 Levels button pressed");
        PlayUiClickSound();

        // Show confirmation dialog
        var dialog = new ConfirmationDialog();
        dialog.Title = "Generate 100 Levels";
        dialog.DialogText = "This will generate 100 themed levels procedurally. This may take a moment.\n\nContinue?";
        
        dialog.Confirmed += () => {
            GD.Print("🎲 Starting batch level generation...");
            GenerateAllLevels.GenerateAll100Levels();
            
            // Show completion dialog
            var completeDialog = new AcceptDialog();
            completeDialog.Title = "Generation Complete";
            completeDialog.DialogText = "✅ Successfully generated 100 themed levels!\n\nYou can now play them from the Level Browser.";
            AddChild(completeDialog);
            completeDialog.PopupCentered();
        };
        
        AddChild(dialog);
        dialog.PopupCentered();
    }

    private void AddUnlockFullGameButton()
    {
        if (_playButtonNode == null || _playButtonNode.GetParent() is not Control container)
            return;

        if (MonetizationManager.Instance?.IsFullGameUnlocked ?? false)
            return;

        _unlockFullGameButton = new Button
        {
            Text = "Unlock Full Game - £1.50",
            Name = "UnlockFullGameButton",
            Modulate = new Color(1f, 0.95f, 0.5f)
        };

        _unlockFullGameButton.Pressed += OnUnlockButtonPressed;

        container.AddChild(_unlockFullGameButton);

        // Put it near the Play/Room Selection actions.
        if (_roomSelectionButtonNode != null)
            container.MoveChild(_unlockFullGameButton, _roomSelectionButtonNode.GetIndex() + 1);
    }

    private void AddTelemetryMetricsButton()
    {
        #if DEBUG
        if (_playButtonNode == null || _playButtonNode.GetParent() is not Control container)
            return;

        var telemetryBtn = new Button
        {
            Text = "📊 View Metrics",
            Name = "TelemetryMetricsButton",
            Modulate = new Color(0.5f, 0.8f, 1f),
            CustomMinimumSize = new Vector2(200, 40)
        };
        telemetryBtn.Pressed += OnTelemetryMetricsButtonPressed;

        container.AddChild(telemetryBtn);

        // Position it near the bottom of the menu
        if (_quitButtonNode != null)
            container.MoveChild(telemetryBtn, _quitButtonNode.GetIndex());
        #endif
    }

    private void OnTelemetryMetricsButtonPressed()
    {
        GD.Print("Telemetry Metrics button pressed");
        PlayUiClickSound();

        // Show the telemetry debug panel
        if (TelemetryDebugPanel.Instance != null)
        {
            TelemetryDebugPanel.Instance.ShowPanel();
        }
        else
        {
            // Create and show telemetry panel if not already created
            var telemetryPanel = TelemetryDebugPanel.Instance;
            if (telemetryPanel == null)
            {
                // Create a new instance
                var panel = new TelemetryDebugPanel();
                AddChild(panel);
                panel.ShowPanel();
            }
        }
    }

    private void OnCustomizeFaceButtonPressed()
    {
        GD.Print("Customize Face button pressed");
        PlayUiClickSound();

        var screen = new FaceCustomizationScreen();
        GetTree().Root.AddChild(screen);
    }

    private void InitializeMenu()
    {
        _playButtonNode = GetNodeOrNull<Button>(_playButton);
        _roomSelectionButtonNode = GetNodeOrNull<Button>(_roomSelectionButton);
        _settingsButtonNode = GetNodeOrNull<Button>(_settingsButton);
        _quitButtonNode = GetNodeOrNull<Button>(_quitButton);
        _titleLabelNode = GetNodeOrNull<Label>(_titleLabel);
        _versionLabelNode = GetNodeOrNull<Label>(_versionLabel);

        if (_titleLabelNode != null)
            _titleLabelNode.Text = "Angry Animals";

        if (_versionLabelNode != null)
            _versionLabelNode.Text = "Version 1.0.0";

        if (_playButtonNode != null)
            _playButtonNode.Pressed += OnPlayButtonPressed;

        if (_roomSelectionButtonNode != null)
            _roomSelectionButtonNode.Pressed += OnRoomSelectionButtonPressed;

        if (_settingsButtonNode != null)
            _settingsButtonNode.Pressed += OnSettingsButtonPressed;

        if (_quitButtonNode != null)
            _quitButtonNode.Pressed += OnQuitButtonPressed;

        EnsureDialogs();
    }

    private void EnsureDialogs()
    {
        _unlockConfirmation = new ConfirmationDialog
        {
            Name = "UnlockConfirmationDialog",
            Title = "Unlock Full Game",
            DialogText = "Unlock all 100 levels and remove ads?",
            ProcessMode = ProcessModeEnum.Always
        };
        _unlockConfirmation.GetOkButton().Text = "Continue";
        _unlockConfirmation.GetCancelButton().Text = "Cancel";
        _unlockConfirmation.Confirmed += OnUnlockConfirmationAccepted;
        AddChild(_unlockConfirmation);

        _purchaseCompleteDialog = new AcceptDialog
        {
            Name = "PurchaseCompleteDialog",
            Title = "Purchase Complete",
            DialogText = "Purchase Complete! Enjoy all 100 levels!",
            ProcessMode = ProcessModeEnum.Always
        };
        AddChild(_purchaseCompleteDialog);

        _purchaseFailedDialog = new ConfirmationDialog
        {
            Name = "PurchaseFailedDialog",
            Title = "Purchase Failed",
            DialogText = "Purchase failed.",
            ProcessMode = ProcessModeEnum.Always
        };
        _purchaseFailedDialog.GetOkButton().Text = "Retry";
        _purchaseFailedDialog.GetCancelButton().Text = "Cancel";
        _purchaseFailedDialog.Confirmed += OnPurchaseRetry;
        _purchaseFailedDialog.Canceled += OnPurchaseFailedDialogCanceled;
        AddChild(_purchaseFailedDialog);
    }

    private void ConnectSignals()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.GameStateChanged += OnGameStateChanged;

        if (MonetizationManager.Instance != null)
        {
            MonetizationManager.Instance.PurchaseSucceeded += OnPurchaseCompleted;
            MonetizationManager.Instance.PurchaseFailed += OnPurchaseFailed;
        }
    }

    public override void _ExitTree()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.GameStateChanged -= OnGameStateChanged;

        if (MonetizationManager.Instance != null)
        {
            MonetizationManager.Instance.PurchaseSucceeded -= OnPurchaseCompleted;
            MonetizationManager.Instance.PurchaseFailed -= OnPurchaseFailed;
        }

        if (_playButtonNode != null)
            _playButtonNode.Pressed -= OnPlayButtonPressed;

        if (_roomSelectionButtonNode != null)
            _roomSelectionButtonNode.Pressed -= OnRoomSelectionButtonPressed;

        if (_settingsButtonNode != null)
            _settingsButtonNode.Pressed -= OnSettingsButtonPressed;

        if (_quitButtonNode != null)
            _quitButtonNode.Pressed -= OnQuitButtonPressed;

        if (_unlockFullGameButton != null)
            _unlockFullGameButton.Pressed -= OnUnlockButtonPressed;

        if (_unlockConfirmation != null)
            _unlockConfirmation.Confirmed -= OnUnlockConfirmationAccepted;

        if (_purchaseFailedDialog != null)
        {
            _purchaseFailedDialog.Confirmed -= OnPurchaseRetry;
            _purchaseFailedDialog.Canceled -= OnPurchaseFailedDialogCanceled;
        }
    }

    private void SetupInputMap()
    {
        if (!InputMap.HasAction("ui_menu_select"))
        {
            InputMap.AddAction("ui_menu_select");
            var selectEvent = new InputEventKey { Keycode = Key.Enter };
            InputMap.ActionAddEvent("ui_menu_select", selectEvent);
        }

        if (!InputMap.HasAction("ui_menu_back"))
        {
            InputMap.AddAction("ui_menu_back");
            var backEvent = new InputEventKey { Keycode = Key.Escape };
            InputMap.ActionAddEvent("ui_menu_back", backEvent);
        }
    }

    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("ui_menu_select"))
            HandleMenuSelection();
        else if (@event.IsActionPressed("ui_menu_back"))
            HandleMenuBack();
    }

    private void HandleMenuSelection()
    {
        var focusedControl = GetViewport().GuiGetFocusOwner();
        if (focusedControl is Button focusedButton && focusedButton.Disabled == false)
            focusedButton.EmitSignal(BaseButton.SignalName.Pressed);
    }

    private void HandleMenuBack()
    {
        var settingsPanel = GetNodeOrNull<Control>("SettingsPanel");
        if (settingsPanel != null && settingsPanel.Visible)
            settingsPanel.Visible = false;
    }

    private void OnPlayButtonPressed()
    {
        GD.Print("Play button pressed");
        EmitSignal(SignalName.PlayButtonPressed);
        PlayUiClickSound();

        GameManager.StartRoomByLevelNumber(1);
    }

    private void OnRoomSelectionButtonPressed()
    {
        GD.Print("Room selection button pressed");
        EmitSignal(SignalName.RoomSelectionButtonPressed);
        PlayUiClickSound();

        GameManager.StartRoomByLevelNumber(1);
    }

    private void OnSettingsButtonPressed()
    {
        GD.Print("Settings button pressed");
        EmitSignal(SignalName.SettingsButtonPressed);
        PlayUiClickSound();

        ShowSettingsPanel();
    }

    private void OnQuitButtonPressed()
    {
        GD.Print("Quit button pressed");
        EmitSignal(SignalName.QuitButtonPressed);
        PlayUiClickSound();

        GetTree().Quit();
    }

    private void OnGameStateChanged(GameManager.GameState state)
    {
        switch (state)
        {
            case GameManager.GameState.MainMenu:
                Visible = true;
                RefreshMenu();
                break;
            default:
                Visible = false;
                break;
        }
    }

    private void OnUnlockButtonPressed()
    {
        ShowUnlockConfirmation();
    }

    private void ShowUnlockConfirmation()
    {
        if (_purchaseInProgress)
            return;

        _unlockConfirmation?.PopupCentered();
    }

    private async void OnUnlockConfirmationAccepted()
    {
        if (_purchaseInProgress)
            return;

        OnPurchaseStarted();

        try
        {
            if (MonetizationManager.Instance != null)
                await MonetizationManager.Instance.PurchaseFullGame();
            else
                OnPurchaseFailed("Monetization manager unavailable.");
        }
        catch (Exception ex)
        {
            OnPurchaseFailed(ex.Message);
        }
    }

    private void OnPurchaseStarted()
    {
        _purchaseInProgress = true;

        if (_unlockFullGameButton != null)
        {
            _unlockFullGameButton.Disabled = true;
            _unlockFullGameButton.Text = "Processing...";
        }
    }

    private void OnPurchaseCompleted()
    {
        _purchaseInProgress = false;

        _purchaseCompleteDialog?.PopupCentered();
        RefreshMenu();
    }

    private void OnPurchaseFailed(string reason)
    {
        _purchaseInProgress = false;

        if (_unlockFullGameButton != null)
        {
            _unlockFullGameButton.Disabled = false;
            _unlockFullGameButton.Text = "Unlock Full Game - £1.50";
        }

        if (_purchaseFailedDialog != null)
        {
            _purchaseFailedDialog.DialogText = string.IsNullOrWhiteSpace(reason) ? "Purchase failed." : reason;
            _purchaseFailedDialog.PopupCentered();
        }
    }

    private void OnPurchaseRetry()
    {
        if (_unlockConfirmation == null)
            return;

        OnUnlockConfirmationAccepted();
    }

    private void OnPurchaseFailedDialogCanceled()
    {
        // no-op
    }

    private void ShowSettingsPanel()
    {
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
        var unlocked = MonetizationManager.Instance?.IsFullGameUnlocked ?? false;

        if (_unlockFullGameButton != null)
            _unlockFullGameButton.Visible = !unlocked;

        if (_unlockFullGameButton != null && !_purchaseInProgress)
        {
            _unlockFullGameButton.Disabled = false;
            _unlockFullGameButton.Text = "Unlock Full Game - £1.50";
        }
        
        // Update retention system UI
        UpdateRetentionSystemUI();
    }

    /// <summary>
    /// Initialize retention systems (streak, events, notifications)
    /// </summary>
    private void InitializeRetentionSystems()
    {
        // Initialize streak system
        if (StreakManager.Instance != null)
        {
            StreakManager.Instance.CheckDailyLogin();
        }

        // Initialize seasonal event system
        if (SeasonalEventManager.Instance != null)
        {
            SeasonalEventManager.Instance.CheckAndActivateEvents();
        }

        // Initialize push notification system
        if (PushNotificationManager.Instance != null)
        {
            // Handle app launch notification check
            GD.Print("Retention systems initialized");
        }
    }

    /// <summary>
    /// Add retention system buttons to main menu
    /// </summary>
    private void AddRetentionSystemButtons()
    {
        if (_playButtonNode != null && _playButtonNode.GetParent() is Control container)
        {
            // Add streak indicator and login bonus button
            AddStreakIndicatorButton();
            AddLoginBonusButton();
            AddSeasonalEventsButton();
            AddNotificationSettingsButton();
        }
    }

    /// <summary>
    /// Add streak indicator button showing current streak
    /// </summary>
    private void AddStreakIndicatorButton()
    {
        var streakBtn = new Button
        {
            Text = "🔥 Day 1 of 30!",
            Name = "StreakIndicatorButton",
            Modulate = new Color(1f, 0.8f, 0.2f) // Gold color
        };
        streakBtn.Pressed += OnStreakIndicatorPressed;

        container.AddChild(streakBtn);

        if (_playButtonNode != null)
            container.MoveChild(streakBtn, _playButtonNode.GetIndex() + 1);
    }

    /// <summary>
    /// Add login bonus button
    /// </summary>
    private void AddLoginBonusButton()
    {
        var loginBonusBtn = new Button
        {
            Text = "🎁 Daily Bonus",
            Name = "LoginBonusButton",
            Modulate = new Color(0.5f, 1f, 0.5f) // Green color
        };
        loginBonusBtn.Pressed += OnLoginBonusButtonPressed;

        container.AddChild(loginBonusBtn);

        if (_playButtonNode != null)
            container.MoveChild(loginBonusBtn, _playButtonNode.GetIndex() + 2);
    }

    /// <summary>
    /// Add seasonal events button
    /// </summary>
    private void AddSeasonalEventsButton()
    {
        var eventsBtn = new Button
        {
            Text = "🎉 Seasonal Events",
            Name = "SeasonalEventsButton",
            Modulate = new Color(0.7f, 0.5f, 1f) // Purple color
        };
        eventsBtn.Pressed += OnSeasonalEventsButtonPressed;

        container.AddChild(eventsBtn);

        if (_playButtonNode != null)
            container.MoveChild(eventsBtn, _playButtonNode.GetIndex() + 3);
    }

    /// <summary>
    /// Add notification settings button
    /// </summary>
    private void AddNotificationSettingsButton()
    {
        var notifBtn = new Button
        {
            Text = "🔔 Notifications",
            Name = "NotificationSettingsButton",
            Modulate = new Color(1f, 0.7f, 0.5f) // Orange color
        };
        notifBtn.Pressed += OnNotificationSettingsButtonPressed;

        container.AddChild(notifBtn);

        if (_playButtonNode != null)
            container.MoveChild(notifBtn, _playButtonNode.GetIndex() + 4);
    }

    /// <summary>
    /// Update retention system UI
    /// </summary>
    private void UpdateRetentionSystemUI()
    {
        UpdateStreakIndicator();
    }

    /// <summary>
    /// Update streak indicator display
    /// </summary>
    private void UpdateStreakIndicator()
    {
        var streakBtn = GetNodeOrNull<Button>("StreakIndicatorButton");
        if (streakBtn == null || StreakManager.Instance == null) return;

        var streakData = StreakManager.Instance.GetStreakDisplayData();
        var currentStreak = (int)streakData.GetValueOrDefault("current_streak", 0);

        if (currentStreak > 0)
        {
            streakBtn.Text = $"🔥 Day {currentStreak} of 30!";
            streakBtn.Modulate = currentStreak switch
            {
                >= 22 => Colors.Gold,
                >= 15 => Colors.Purple,
                >= 8 => Colors.Blue,
                _ => new Color(1f, 0.8f, 0.2f)
            };
        }
        else
        {
            streakBtn.Text = "🌟 Start your streak!";
            streakBtn.Modulate = Colors.Gray;
        }
    }

    /// <summary>
    /// Handle streak indicator pressed
    /// </summary>
    private void OnStreakIndicatorPressed()
    {
        GD.Print("Streak indicator pressed");
        PlayUiClickSound();

        // Show streak details or login bonus screen
        OnLoginBonusButtonPressed();
    }

    /// <summary>
    /// Handle login bonus button pressed
    /// </summary>
    private void OnLoginBonusButtonPressed()
    {
        GD.Print("Login bonus button pressed");
        PlayUiClickSound();

        // Load and show login bonus screen
        var loginBonusScreen = ResourceLoader.Load<PackedScene>("res://Scenes/UI/LoginBonusScreen.tscn");
        if (loginBonusScreen != null)
        {
            var screenInstance = loginBonusScreen.Instantiate();
            AddChild(screenInstance);
        }
    }

    /// <summary>
    /// Handle seasonal events button pressed
    /// </summary>
    private void OnSeasonalEventsButtonPressed()
    {
        GD.Print("Seasonal events button pressed");
        PlayUiClickSound();

        // Load and show seasonal events screen
        var eventScreen = ResourceLoader.Load<PackedScene>("res://Scenes/UI/SeasonalEventScreen.tscn");
        if (eventScreen != null)
        {
            var screenInstance = eventScreen.Instantiate();
            AddChild(screenInstance);
        }
    }

    /// <summary>
    /// Handle notification settings button pressed
    /// </summary>
    private void OnNotificationSettingsButtonPressed()
    {
        GD.Print("Notification settings button pressed");
        PlayUiClickSound();

        // Show notification settings dialog
        ShowNotificationSettingsDialog();
    }

    /// <summary>
    /// Show notification settings dialog
    /// </summary>
    private void ShowNotificationSettingsDialog()
    {
        // This would show a dialog for configuring notification preferences
        // For now, we'll show a simple message
        GD.Print("Notification settings - this would open a settings dialog");
        
        // Example: Show confirmation dialog
        var dialog = new ConfirmationDialog();
        dialog.Title = "Notification Settings";
        dialog.DialogText = "Push notifications help keep you engaged with daily rewards and events!\n\nEnable notifications for:\n• Daily login reminders\n• Streak milestone celebrations\n• Seasonal event announcements\n\nWould you like to enable push notifications?";
        
        dialog.Confirmed += () => EnablePushNotifications();
        dialog.Canceled += () => GD.Print("Notification settings cancelled");
        
        AddChild(dialog);
        dialog.PopupCentered();
    }

    /// <summary>
    /// Enable push notifications
    /// </summary>
    private void EnablePushNotifications()
    {
        // This would integrate with the PushNotificationManager
        GD.Print("Enabling push notifications...");
        
        // Show success message
        var successDialog = new AcceptDialog();
        successDialog.Title = "Notifications Enabled!";
        successDialog.DialogText = "You'll now receive daily reminders and event notifications to help maintain your streak!";
        AddChild(successDialog);
        successDialog.PopupCentered();
    }
}
