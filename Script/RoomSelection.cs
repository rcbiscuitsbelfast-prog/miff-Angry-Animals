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
		{
			_titleLabel.Text = "Select a Room";
		}

		if (_backButton != null)
		{
			_backButton.Text = "Back to Main Menu";
			_backButton.Pressed += OnBackButtonPressed;
		}
	}

	private void ConnectSignals()
	{
		// Connect to GameManager for state changes
		if (GameManager.Instance != null)
		{
			GameManager.Instance.GameStateChanged += OnGameStateChanged;
		}

		// Connect to SignalManager for room completion events
		if (SignalManager.Instance != null)
		{
			SignalManager.Instance.OnLevelCompleted += OnLevelCompleted;
		}

		// Connect to PlayerProfile for unlock state changes
		if (PlayerProfile.Instance != null)
		{
			// PlayerProfile doesn't have signals, so we'll refresh when needed
		}
	}

	public override void _ExitTree()
	{
		if (GameManager.Instance != null)
		{
			GameManager.Instance.GameStateChanged -= OnGameStateChanged;
		}

		if (SignalManager.Instance != null)
		{
			SignalManager.Instance.OnLevelCompleted -= OnLevelCompleted;
		}

		if (_backButton != null)
		{
			_backButton.Pressed -= OnBackButtonPressed;
		}
	}

	private void PopulateRoomButtons()
	{
		if (_roomsContainer == null || GameManager.Instance == null)
			return;

		// Clear existing buttons
		for (int i = _roomsContainer.GetChildCount() - 1; i >= 0; i--)
		{
			var child = _roomsContainer.GetChild(i);
			child.QueueFree();
		}

		// Create buttons for each room
		for (int i = 0; i < GameManager.Instance.Rooms.Length; i++)
		{
			var roomInfo = GameManager.Instance.Rooms[i];
			var isUnlocked = PlayerProfile.IsRoomUnlocked(i);

			var roomButton = CreateRoomButton(i, roomInfo, isUnlocked);
			_roomsContainer.AddChild(roomButton);
		}
	}

	private Button CreateRoomButton(int roomIndex, GameManager.RoomInfo roomInfo, bool isUnlocked)
	{
		var button = new Button();
		button.Name = $"RoomButton_{roomIndex}";
		button.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
		button.CustomMinimumSize = new Vector2(400, 60);

		// Create container for button content
		var container = new HBoxContainer();
		container.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;

		// Room index and name
		var roomLabel = new Label();
		roomLabel.Text = $"{roomIndex + 1}. {roomInfo.Description}";
		roomLabel.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
		
		// Target score
		var scoreLabel = new Label();
		scoreLabel.Text = $"Target: {roomInfo.TargetScore}";
		scoreLabel.Modulate = Colors.Yellow;

		// Lock status
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
			button.TooltipText = "Complete previous rooms to unlock";
		}

		container.AddChild(roomLabel);
		container.AddChild(new Control { SizeFlagsHorizontal = Control.SizeFlags.ExpandFill });
		container.AddChild(scoreLabel);
		container.AddChild(lockLabel);

		button.AddChild(container);

		// Connect button signal
		if (!button.Disabled)
		{
			button.Pressed += () => OnRoomButtonPressed(roomIndex);
		}

		return button;
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
		// Refresh UI when game state changes
		if (state == GameManager.GameState.MainMenu)
		{
			PopulateRoomButtons();
		}
	}

	private void OnLevelCompleted()
	{
		// A level was completed, refresh room buttons to show new unlocks
		CallDeferred(nameof(PopulateRoomButtons));
	}

	/// <summary>
	/// Refreshes the room selection buttons to reflect current unlock state.
	/// </summary>
	public void RefreshRoomButtons()
	{
		PopulateRoomButtons();
	}

	/// <summary>
	/// Gets the currently selected room index, or -1 if none selected.
	/// </summary>
	public int GetSelectedRoomIndex()
	{
		// This would be implemented if we had a concept of "selected" room
		return -1;
	}
}