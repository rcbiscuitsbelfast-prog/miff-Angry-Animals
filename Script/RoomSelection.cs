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
			var isUnlocked = GameManager.IsLevelUnlocked(i + 1);

			var roomButton = CreateRoomButton(i, roomInfo, isUnlocked);
			_roomsContainer.AddChild(roomButton);
		}
	}

	private Button CreateRoomButton(int roomIndex, GameManager.RoomInfo roomInfo, bool isUnlocked)
	{
		var button = new Button();
		button.Name = $