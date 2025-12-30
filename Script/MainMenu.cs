using System;
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

    private Button? _unlockFullGameButton;
    private ConfirmationDialog? _unlockConfirmation;
    private AcceptDialog? _purchaseCompleteDialog;
    private ConfirmationDialog? _purchaseFailedDialog;

    private bool _purchaseInProgress;

    public override void _Ready()
    {
        InitializeMenu();
        AddCustomizeFaceButton();
        AddUnlockFullGameButton();
        ConnectSignals();
        SetupInputMap();
    }

    private void AddCustomizeFaceButton()
    {
        if (_playButton != null && _playButton.GetParent() is Control container)
        {
            var customizeBtn = new Button
            {
                Text = "Customize Face",
                Name = "CustomizeFaceButton"
            };
            customizeBtn.Pressed += OnCustomizeFaceButtonPressed;

            container.AddChild(customizeBtn);

            if (_roomSelectionButton != null)
                container.MoveChild(customizeBtn, _roomSelectionButton.GetIndex() + 1);
        }
    }

    private void AddUnlockFullGameButton()
    {
        if (_playButton == null || _playButton.GetParent() is not Control container)
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
        if (_roomSelectionButton != null)
            container.MoveChild(_unlockFullGameButton, _roomSelectionButton.GetIndex() + 1);
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
        _playButton = GetNodeOrNull<Button>(_playButtonPath);
        _roomSelectionButton = GetNodeOrNull<Button>(_roomSelectionButtonPath);
        _settingsButton = GetNodeOrNull<Button>(_settingsButtonPath);
        _quitButton = GetNodeOrNull<Button>(_quitButtonPath);
        _titleLabel = GetNodeOrNull<Label>(_titleLabelPath);
        _versionLabel = GetNodeOrNull<Label>(_versionLabelPath);

        if (_titleLabel != null)
            _titleLabel.Text = "Angry Animals";

        if (_versionLabel != null)
            _versionLabel.Text = "Version 1.0.0";

        if (_playButton != null)
            _playButton.Pressed += OnPlayButtonPressed;

        if (_roomSelectionButton != null)
            _roomSelectionButton.Pressed += OnRoomSelectionButtonPressed;

        if (_settingsButton != null)
            _settingsButton.Pressed += OnSettingsButtonPressed;

        if (_quitButton != null)
            _quitButton.Pressed += OnQuitButtonPressed;

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

        if (_playButton != null)
            _playButton.Pressed -= OnPlayButtonPressed;

        if (_roomSelectionButton != null)
            _roomSelectionButton.Pressed -= OnRoomSelectionButtonPressed;

        if (_settingsButton != null)
            _settingsButton.Pressed -= OnSettingsButtonPressed;

        if (_quitButton != null)
            _quitButton.Pressed -= OnQuitButtonPressed;

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
    }
}
