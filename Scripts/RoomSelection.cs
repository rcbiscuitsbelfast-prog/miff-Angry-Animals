using System;
using System.Threading.Tasks;
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
    private CheckButton? _proceduralModeToggle;

    public override void _Ready()
    {
        InitializeUI();
        ConnectSignals();
        PopulateRoomButtons();
        
        // Preload interstitial ads when entering room selection
        PreloadInterstitialAdsAsync();
    }

    private async void PreloadInterstitialAdsAsync()
    {
        await Task.Delay(1000); // Small delay to let the scene load
        
        if (AdsManager.Instance != null && MonetizationManager.Instance?.ShowAds != false)
        {
            GD.Print("Preloading interstitial ads from room selection");
            await AdsManager.Instance.LoadInterstitialAd();
        }
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

        if (_proceduralModeToggle != null)
            _proceduralModeToggle.Toggled -= OnProceduralModeToggled;
    }

    private LineEdit? _seedInput;
    private Button? _randomSeedButton;
    private Button? _deterministicSeedButton;
    private Button? _useLastSeedButton;

    // Slingshot type selection UI
    private OptionButton? _slingshotTypeSelector;
    private HBoxContainer? _slingshotTypeContainer;

    private void PopulateRoomButtons()
    {
        if (_roomsContainer == null || GameManager.Instance == null)
            return;

        for (int i = _roomsContainer.GetChildCount() - 1; i >= 0; i--)
        {
            var child = _roomsContainer.GetChild(i);
            child.QueueFree();
        }

        AddGenerationControls();

        for (int i = 0; i < GameManager.Instance.Rooms.Length; i++)
        {
            var roomInfo = GameManager.Instance.Rooms[i];
            var isUnlocked = IsRoomAccessible(i);
            var roomButton = CreateRoomButton(i, roomInfo, isUnlocked);
            _roomsContainer.AddChild(roomButton);
        }

        CreateOrUpdateUnlockButton();
    }

    private void AddGenerationControls()
    {
        if (_roomsContainer == null)
            return;

        var header = new VBoxContainer
        {
            Name = "GenerationControls",
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
        };

        bool proceduralEnabled = PlayerProfile.Instance?.UseProceduralLevels ?? false;

        _proceduralModeToggle = new CheckButton
        {
            Name = "ProceduralModeToggle",
            Text = proceduralEnabled ? "Procedural Levels: ON" : "Procedural Levels: OFF",
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
            ButtonPressed = proceduralEnabled
        };
        _proceduralModeToggle.Toggled += OnProceduralModeToggled;
        header.AddChild(_proceduralModeToggle);

        // Add slingshot type selector
        AddSlingshotTypeSelector(header);

        var seedRow = new HBoxContainer
        {
            Name = "SeedRow",
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
        };

        var seedLabel = new Label { Text = "Seed:", Modulate = Colors.Yellow };

        _seedInput = new LineEdit
        {
            Name = "SeedInput",
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
            PlaceholderText = "0 = deterministic (per level)",
            Text = "0"
        };

        _randomSeedButton = new Button { Name = "RandomSeedButton", Text = "Random" };
        _randomSeedButton.Pressed += OnRandomSeedPressed;

        _useLastSeedButton = new Button { Name = "UseLastSeedButton", Text = "Use Last" };
        _useLastSeedButton.Pressed += OnUseLastSeedPressed;

        seedRow.AddChild(seedLabel);
        seedRow.AddChild(_seedInput);
        seedRow.AddChild(_randomSeedButton);
        seedRow.AddChild(_useLastSeedButton);

        _deterministicSeedButton = new Button
        {
            Name = "DeterministicSeedButton",
            Text = "Deterministic",
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
        };
        _deterministicSeedButton.Pressed += OnDeterministicSeedPressed;

        seedRow.Visible = proceduralEnabled;
        _deterministicSeedButton.Visible = proceduralEnabled;

        header.AddChild(seedRow);
        header.AddChild(_deterministicSeedButton);

        _roomsContainer.AddChild(header);
        _roomsContainer.AddChild(new HSeparator());
    }

    private void OnProceduralModeToggled(bool enabled)
    {
        PlayerProfile.SetProceduralMode(enabled);
        CallDeferred(nameof(PopulateRoomButtons));
    }

    private void AddSlingshotTypeSelector(VBoxContainer parent)
    {
        _slingshotTypeContainer = new HBoxContainer
        {
            Name = "SlingshotTypeRow",
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
        };

        var typeLabel = new Label
        {
            Text = "Slingshot:",
            Modulate = Colors.Cyan,
            CustomMinimumSize = new Vector2(100, 0)
        };

        _slingshotTypeSelector = new OptionButton
        {
            Name = "SlingshotTypeSelector",
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
            CustomMinimumSize = new Vector2(200, 0)
        };

        // Add slingshot type options
        _slingshotTypeSelector.AddItem("Catapult", (int)SlingshotType.Catapult);
        _slingshotTypeSelector.AddItem("Giant Hand", (int)SlingshotType.GiantHand);
        _slingshotTypeSelector.AddItem("Trebuchet", (int)SlingshotType.Trebuchet);
        _slingshotTypeSelector.AddItem("Spring", (int)SlingshotType.Spring);

        // Set current selection from PlayerProfile
        int currentType = PlayerProfile.GetSlingshotType();
        _slingshotTypeSelector.Selected = Mathf.Clamp(currentType, 0, 3);
        _slingshotTypeSelector.ItemSelected += OnSlingshotTypeSelected;

        _slingshotTypeContainer.AddChild(typeLabel);
        _slingshotTypeContainer.AddChild(_slingshotTypeSelector);

        parent.AddChild(_slingshotTypeContainer);
        parent.AddChild(new HSeparator());
    }

    private void OnSlingshotTypeSelected(int index)
    {
        PlayerProfile.SetSlingshotType(index);

        // Update label if needed
        if (_slingshotTypeSelector != null)
        {
            GD.Print($"Slingshot type changed to: {(SlingshotType)index}");
        }
    }

    private void OnRandomSeedPressed()
    {
        if (_seedInput == null)
            return;

        _seedInput.Text = LevelGenerator.CreateRandomSeed().ToString();
    }

    private void OnDeterministicSeedPressed()
    {
        if (_seedInput == null)
            return;

        _seedInput.Text = "0";
    }

    private void OnUseLastSeedPressed()
    {
        if (_seedInput == null || PlayerProfile.Instance == null)
            return;

        _seedInput.Text = PlayerProfile.Instance.LastProceduralSeed.ToString();
    }

    private int GetSeedOverride()
    {
        if (_seedInput == null)
            return 0;

        if (!int.TryParse(_seedInput.Text?.Trim(), out int seed))
            return 0;

        return seed;
    }

    private static int GetEffectiveSeedForRoom(int roomNumber, int seedOverride)
    {
        return seedOverride == 0 ? LevelGenerator.CalculateSeed(roomNumber) : seedOverride;
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

        var chapter = StoryData.GetChapterForRoomIndex(roomIndex);
        var subtitle = StoryData.GetLevelSubtitle(roomIndex);
        var displayName = string.IsNullOrWhiteSpace(subtitle) ? roomInfo.Description : subtitle;

        var roomLabel = new Label
        {
            Text = $"{roomIndex + 1}. {displayName}",
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
            Modulate = chapter.ThemeColor
        };

        bool proceduralEnabled = PlayerProfile.Instance?.UseProceduralLevels ?? false;
        int roomNumber = roomIndex + 1;

        var scoreLabel = new Label
        {
            Text = proceduralEnabled
                ? $"Cups: {LevelGenerator.GetCupCount(roomNumber)}"
                : $"Optimal: {roomInfo.OptimalScore}",
            Modulate = proceduralEnabled ? Colors.Cyan : Colors.Yellow
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

        var proceduralEnabled = PlayerProfile.Instance?.UseProceduralLevels ?? false;
        if (proceduralEnabled)
        {
            int seedOverride = GetSeedOverride();
            int roomNumber = roomIndex + 1;
            int effectiveSeed = GetEffectiveSeedForRoom(roomNumber, seedOverride);

            if (PlayerProfile.Instance != null)
            {
                PlayerProfile.Instance.LastProceduralSeed = effectiveSeed;
                PlayerProfile.Instance.LastProceduralLevelNumber = roomNumber;
                PlayerProfile.Instance.Save();

                DisplayServer.ClipboardSet(effectiveSeed.ToString());
                GD.Print($"Procedural seed copied to clipboard: {effectiveSeed}");
            }
        }

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
