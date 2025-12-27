using System;
using Godot;

/// <summary>
/// Handles room selection UI and integrates with GameManager.
/// Shows available rooms based on unlock state and allows selection.
/// </summary>
public partial class RoomSelection : Control
{
    [Signal] public delegate void RoomSelectedEventHandler(int roomIndex);

    [Export] private NodePath _roomsContainerPath;
    [Export] private NodePath _titleLabelPath;
    [Export] private NodePath _backButtonPath;
    [Export] private PackedScene _roomButtonScene;

    private VBoxContainer? _roomsContainer;
    private Label? _titleLabel;
    private Button? _backButton;

    private Button? _unlockFullGameButton;
    private AcceptDialog? _purchaseDialog;

    public override void _Ready()
    {
        InitializeUI();
        ConnectSignals();
        PopulateRoomButtons();
    }

    private void InitializeUI()
    {
        _roomsContainer = GetNodeOrNull<VBoxContainer>(_roomsContainerPath);
        _titleLabel = GetNodeOrNull<Label>(_titleLabelPath);
        _backButton = GetNodeOrNull<Button>(_backButtonPath);

        if (_titleLabel != null)
            _titleLabel.Text = "Select a Room";

        if (_backButton != null)
        {
            _backButton.Text = "Back to Main Menu";
            _backButton.Pressed += OnBackButtonPressed;
        }

        _purchaseDialog = new AcceptDialog
        {
            Title = "Purchase",
            DialogText = "",
            ProcessMode = ProcessModeEnum.Always
        };
        AddChild(_purchaseDialog);
    }

    private void ConnectSignals()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.GameStateChanged += OnGameStateChanged;

        if (SignalManager.Instance != null)
            SignalManager.Instance.OnLevelCompleted += OnLevelCompleted;

        if (MonetizationManager.Instance != null)
        {
            MonetizationManager.Instance.PurchaseSucceeded += OnPurchaseSucceeded;
            MonetizationManager.Instance.PurchaseFailed += OnPurchaseFailed;
        }
    }

    public override void _ExitTree()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.GameStateChanged -= OnGameStateChanged;

        if (SignalManager.Instance != null)
            SignalManager.Instance.OnLevelCompleted -= OnLevelCompleted;

        if (MonetizationManager.Instance != null)
        {
            MonetizationManager.Instance.PurchaseSucceeded -= OnPurchaseSucceeded;
            MonetizationManager.Instance.PurchaseFailed -= OnPurchaseFailed;
        }

        if (_backButton != null)
            _backButton.Pressed -= OnBackButtonPressed;

        if (_unlockFullGameButton != null)
            _unlockFullGameButton.Pressed -= OnUnlockButtonPressed;
    }

    private void PopulateRoomButtons()
    {
        if (_roomsContainer == null || GameManager.Instance == null)
            return;

        for (int i = _roomsContainer.GetChildCount() - 1; i >= 0; i--)
        {
            var child = _roomsContainer.GetChild(i);
            child.QueueFree();
        }

        for (int i = 0; i < GameManager.Instance.Rooms.Length; i++)
        {
            var roomInfo = GameManager.Instance.Rooms[i];
            var isUnlocked = IsRoomAccessible(i);
            var roomButton = CreateRoomButton(i, roomInfo, isUnlocked);
            _roomsContainer.AddChild(roomButton);
        }

        CreateOrUpdateUnlockButton();
    }

    private bool IsRoomAccessible(int roomIndex)
    {
        var fullUnlocked = MonetizationManager.Instance?.IsFullGameUnlocked ?? false;
        if (fullUnlocked)
            return true;

        if (roomIndex >= 20)
            return false;

        return PlayerProfile.IsRoomUnlocked(roomIndex);
    }

    private Button CreateRoomButton(int roomIndex, GameManager.RoomInfo roomInfo, bool isUnlocked)
    {
        var button = new Button
        {
            Name = $"RoomButton_{roomIndex}",
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
            CustomMinimumSize = new Vector2(400, 60)
        };

        var container = new HBoxContainer { SizeFlagsHorizontal = Control.SizeFlags.ExpandFill };

        var roomLabel = new Label
        {
            Text = $"{roomIndex + 1}. {roomInfo.Description}",
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
        };

        var scoreLabel = new Label
        {
            Text = $"Target: {roomInfo.TargetScore}",
            Modulate = Colors.Yellow
        };

        var lockLabel = new Label();
        if (isUnlocked)
        {
            lockLabel.Text = "✓";
            lockLabel.Modulate = Colors.Green;
            button.Disabled = false;
        }
        else
        {
            lockLabel.Text = "🔒";
            lockLabel.Modulate = Colors.Red;
            button.Disabled = true;

            if (roomIndex >= 20)
                button.TooltipText = "Unlock Full Game to access levels 21-100";
            else
                button.TooltipText = "Complete previous rooms to unlock";
        }

        container.AddChild(roomLabel);
        container.AddChild(new Control { SizeFlagsHorizontal = Control.SizeFlags.ExpandFill });
        container.AddChild(scoreLabel);
        container.AddChild(lockLabel);
        button.AddChild(container);

        if (!button.Disabled)
            button.Pressed += () => OnRoomButtonPressed(roomIndex);

        return button;
    }

    private void CreateOrUpdateUnlockButton()
    {
        if (_roomsContainer == null)
            return;

        var showUnlock = !(MonetizationManager.Instance?.IsFullGameUnlocked ?? false) && GameManager.Instance != null && GameManager.Instance.Rooms.Length > 20;
        if (!showUnlock)
        {
            _unlockFullGameButton = null;
            return;
        }

        _unlockFullGameButton = new Button
        {
            Name = "UnlockFullGameButton",
            Text = "Unlock Full Game - £1.50",
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
            CustomMinimumSize = new Vector2(400, 60),
            Modulate = new Color(1f, 0.95f, 0.5f)
        };
        _unlockFullGameButton.Pressed += OnUnlockButtonPressed;

        _roomsContainer.AddChild(new HSeparator());
        _roomsContainer.AddChild(_unlockFullGameButton);
    }

    /// <summary>
    /// Refreshes lock state based on the monetization status.
    /// </summary>
    public void UpdateLockUI()
    {
        PopulateRoomButtons();
    }

    private async void OnUnlockButtonPressed()
    {
        if (_unlockFullGameButton != null)
        {
            _unlockFullGameButton.Disabled = true;
            _unlockFullGameButton.Text = "Unlocking...";
        }

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

    private void OnPurchaseSucceeded()
    {
        if (_purchaseDialog != null)
        {
            _purchaseDialog.DialogText = "Purchase Complete! Enjoy all 100 levels!";
            _purchaseDialog.PopupCentered();
        }
        CallDeferred(nameof(UpdateLockUI));
    }

    private void OnPurchaseFailed(string reason)
    {
        if (_unlockFullGameButton != null)
        {
            _unlockFullGameButton.Disabled = false;
            _unlockFullGameButton.Text = "Unlock Full Game - £1.50";
        }

        if (_purchaseDialog != null)
        {
            _purchaseDialog.DialogText = string.IsNullOrWhiteSpace(reason) ? "Purchase failed." : reason;
            _purchaseDialog.PopupCentered();
        }
    }

    private void OnRoomButtonPressed(int roomIndex)
    {
        GD.Print($"Room selected: {roomIndex}");
        EmitSignal(SignalName.RoomSelected, roomIndex);
        GameManager.StartRoom(roomIndex);
    }

    private void OnBackButtonPressed()
    {
        GD.Print("Back to main menu");
        GameManager.LoadMain();
    }

    private void OnGameStateChanged(GameManager.GameState state)
    {
        if (state == GameManager.GameState.MainMenu)
            PopulateRoomButtons();
    }

    private void OnLevelCompleted()
    {
        CallDeferred(nameof(PopulateRoomButtons));
    }

    /// <summary>
    /// Refreshes the room selection buttons to reflect current unlock state.
    /// </summary>
    public void RefreshRoomButtons() => PopulateRoomButtons();

    /// <summary>
    /// Gets the currently selected room index, or -1 if none selected.
    /// </summary>
    public int GetSelectedRoomIndex() => -1;
}
