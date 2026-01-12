using System;
using System.Collections.Generic;
using Godot;
using Godot.Collections;

/// <summary>
/// Seasonal event screen UI controller
/// Shows active events in carousel, event details, countdown timers, and exclusive cosmetics
/// </summary>
public partial class SeasonalEventScreenUI : Control
{
    [Signal] public delegate void ScreenDismissedEventHandler();
    [Signal] public delegate void EventSelectedEventHandler(string eventId);
    [Signal] public delegate void UnlockEventCosmeticsEventHandler(string eventId);

    [Export] private NodePath _mainContainer;
    [Export] private NodePath _eventCarouselContainer;
    [Export] private NodePath _eventDetailsContainer;
    [Export] private NodePath _currentEventTitle;
    [Export] private NodePath _eventDescription;
    [Export] private NodePath _eventCountdown;
    [Export] private NodePath _eventProgressBar;
    [Export] private NodePath _cosmeticsGridContainer;
    [Export] private NodePath _unlockCosmeticsButton;
    [Export] private NodePath _dismissButton;
    [Export] private NodePath _eventBackground;

    private Control _mainContainerNode;
    private ScrollContainer _eventCarouselContainerNode;
    private Control _eventDetailsContainerNode;
    private Label _currentEventTitleNode;
    private RichTextLabel _eventDescriptionNode;
    private Label _eventCountdownNode;
    private ProgressBar _eventProgressBarNode;
    private GridContainer _cosmeticsGridContainerNode;
    private Button _unlockCosmeticsButtonNode;
    private Button _dismissButtonNode;
    private TextureRect _eventBackgroundNode;

    private SeasonalEventManager _eventManager;
    private List<SeasonalEvent> _activeEvents = new();
    private int _currentEventIndex = 0;
    private Timer _countdownUpdateTimer;

    public override void _Ready()
    {
        InitializeUI();
        ConnectSignals();
        LoadActiveEvents();
        SetupCountdownTimer();
        ShowCurrentEvent();
    }

    /// <summary>
    /// Initialize UI references
    /// </summary>
    private void InitializeUI()
    {
        _mainContainerNode = GetNode<Control>(_mainContainer);
        _eventCarouselContainerNode = GetNode<ScrollContainer>(_eventCarouselContainer);
        _eventDetailsContainerNode = GetNode<Control>(_eventDetailsContainer);
        _currentEventTitleNode = GetNode<Label>(_currentEventTitle);
        _eventDescriptionNode = GetNode<RichTextLabel>(_eventDescription);
        _eventCountdownNode = GetNode<Label>(_eventCountdown);
        _eventProgressBarNode = GetNode<ProgressBar>(_eventProgressBar);
        _cosmeticsGridContainerNode = GetNode<GridContainer>(_cosmeticsGridContainer);
        _unlockCosmeticsButtonNode = GetNode<Button>(_unlockCosmeticsButton);
        _dismissButtonNode = GetNode<Button>(_dismissButton);
        _eventBackgroundNode = GetNode<TextureRect>(_eventBackground);

        _eventManager = SeasonalEventManager.Instance;
    }

    /// <summary>
    /// Connect signal handlers
    /// </summary>
    private void ConnectSignals()
    {
        if (_dismissButtonNode != null)
            _dismissButtonNode.Pressed += OnDismissButtonPressed;

        if (_unlockCosmeticsButtonNode != null)
            _unlockCosmeticsButtonNode.Pressed += OnUnlockCosmeticsButtonPressed;

        // Connect event manager signals
        if (_eventManager != null)
        {
            _eventManager.EventStarted += OnEventStarted;
            _eventManager.EventEnded += OnEventEnded;
            _eventManager.EventProgressUpdated += OnEventProgressUpdated;
        }
    }

    /// <summary>
    /// Setup countdown update timer
    /// </summary>
    private void SetupCountdownTimer()
    {
        _countdownUpdateTimer = new Timer();
        _countdownUpdateTimer.WaitTime = 60f; // Update every minute
        _countdownUpdateTimer.OneShot = false;
        _countdownUpdateTimer.Timeout += UpdateCountdownDisplay;
        AddChild(_countdownUpdateTimer);
        _countdownUpdateTimer.Start();
    }

    /// <summary>
    /// Load active events
    /// </summary>
    private void LoadActiveEvents()
    {
        if (_eventManager == null) return;

        _activeEvents = _eventManager.GetActiveEvents();
        CreateEventCarousel();
        
        GD.Print($"Loaded {_activeEvents.Count} active seasonal events");
    }

    /// <summary>
    /// Create event carousel
    /// </summary>
    private void CreateEventCarousel()
    {
        if (_eventCarouselContainerNode == null) return;

        // Clear existing carousel items
        foreach (var child in _eventCarouselContainerNode.GetChildren())
        {
            child.QueueFree();
        }

        var carouselContainer = new HBoxContainer();
        carouselContainer.SizeFlagsHorizontal = Control.SizeFlags.ShrinkEnd;
        _eventCarouselContainerNode.AddChild(carouselContainer);

        foreach (var eventItem in _activeEvents)
        {
            var eventButton = CreateEventCarouselItem(eventItem);
            carouselContainer.AddChild(eventButton);
        }
    }

    /// <summary>
    /// Create carousel item for event
    /// </summary>
    private Button CreateEventCarouselItem(SeasonalEvent seasonalEvent)
    {
        var button = new Button();
        button.SizeFlagsHorizontal = Control.SizeFlags.ShrinkEnd;
        button.Size = new Vector2(120, 80);
        button.Modulate = seasonalEvent.ThemeColor;

        var vbox = new VBoxContainer();
        vbox.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        button.AddChild(vbox);

        var titleLabel = new Label();
        titleLabel.Text = seasonalEvent.EventName;
        titleLabel.HorizontalAlignment = HorizontalAlignment.Center;
        titleLabel.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        titleLabel.AddThemeColorOverride("font_color", Colors.White);
        vbox.AddChild(titleLabel);

        var statusLabel = new Label();
        statusLabel.Text = seasonalEvent.IsEventActive() ? "ACTIVE" : "UPCOMING";
        statusLabel.HorizontalAlignment = HorizontalAlignment.Center;
        statusLabel.AddThemeColorOverride("font_color", Colors.Yellow);
        vbox.AddChild(statusLabel);

        button.Pressed += () => SelectEvent(seasonalEvent.EventId);
        button.TooltipText = seasonalEvent.EventDescription;

        return button;
    }

    /// <summary>
    /// Show current event details
    /// </summary>
    private void ShowCurrentEvent()
    {
        if (_activeEvents.Count == 0)
        {
            ShowNoEventsMessage();
            return;
        }

        var currentEvent = _activeEvents[Mathf.Clamp(_currentEventIndex, 0, _activeEvents.Count - 1)];
        UpdateEventDisplay(currentEvent);
    }

    /// <summary>
    /// Show no events message
    /// </summary>
    private void ShowNoEventsMessage()
    {
        if (_currentEventTitleNode != null)
            _currentEventTitleNode.Text = "No Active Events";

        if (_eventDescriptionNode != null)
            _eventDescriptionNode.Text = "Check back soon for exciting seasonal events with exclusive cosmetics!";

        if (_cosmeticsGridContainerNode != null)
            _cosmeticsGridContainerNode.Visible = false;

        if (_unlockCosmeticsButtonNode != null)
            _unlockCosmeticsButtonNode.Visible = false;
    }

    /// <summary>
    /// Update event display with selected event
    /// </summary>
    private void UpdateEventDisplay(SeasonalEvent seasonalEvent)
    {
        // Update title and description
        if (_currentEventTitleNode != null)
            _currentEventTitleNode.Text = seasonalEvent.EventName;

        if (_eventDescriptionNode != null)
            _eventDescriptionNode.Text = seasonalEvent.EventDescription;

        // Update background
        if (_eventBackgroundNode != null && seasonalEvent.EventBackground != null)
            _eventBackgroundNode.Texture = seasonalEvent.EventBackground;
        else
            _eventBackgroundNode.Modulate = seasonalEvent.ThemeColor;

        // Update countdown
        UpdateCountdownDisplay();

        // Update progress
        UpdateEventProgress(seasonalEvent);

        // Update cosmetics
        UpdateCosmeticsDisplay(seasonalEvent);

        // Update unlock button
        UpdateUnlockButton(seasonalEvent);
    }

    /// <summary>
    /// Update countdown display
    /// </summary>
    private void UpdateCountdownDisplay()
    {
        if (_activeEvents.Count == 0) return;

        var currentEvent = _activeEvents[Mathf.Clamp(_currentEventIndex, 0, _activeEvents.Count - 1)];
        
        if (_eventCountdownNode == null) return;

        if (currentEvent.IsEventActive())
        {
            var timeRemaining = currentEvent.GetTimeRemaining();
            if (timeRemaining > TimeSpan.Zero)
            {
                var days = timeRemaining.Days;
                var hours = timeRemaining.Hours;
                var minutes = timeRemaining.Minutes;

                string timeText;
                if (days > 0)
                    timeText = $"⏰ Ends in {days}d {hours}h";
                else if (hours > 0)
                    timeText = $"⏰ Ends in {hours}h {minutes}m";
                else
                    timeText = $"⏰ Ends in {minutes}m";

                _eventCountdownNode.Text = timeText;
                _eventCountdownNode.Modulate = timeRemaining.TotalHours < 24 ? Colors.Red : Colors.White;
            }
            else
            {
                _eventCountdownNode.Text = "Event Ended";
                _eventCountdownNode.Modulate = Colors.Gray;
            }
        }
        else if (currentEvent.IsEventScheduled())
        {
            var timeUntilStart = currentEvent.GetTimeUntilStart();
            if (timeUntilStart > TimeSpan.Zero)
            {
                var days = timeUntilStart.Days;
                var hours = timeUntilStart.Hours;

                _eventCountdownNode.Text = $"Starts in {days}d {hours}h";
                _eventCountdownNode.Modulate = Colors.LightBlue;
            }
        }
        else
        {
            _eventCountdownNode.Text = "Event Completed";
            _eventCountdownNode.Modulate = Colors.Gray;
        }
    }

    /// <summary>
    /// Update event progress
    /// </summary>
    private void UpdateEventProgress(SeasonalEvent seasonalEvent)
    {
        if (_eventProgressBarNode == null) return;

        var progress = seasonalEvent.GetEventProgress();
        _eventProgressBarNode.Value = progress * 100f;

        // Set progress color based on progress
        if (progress >= 0.8f)
            _eventProgressBarNode.Modulate = Colors.Red;
        else if (progress >= 0.5f)
            _eventProgressBarNode.Modulate = Colors.Yellow;
        else
            _eventProgressBarNode.Modulate = Colors.Green;
    }

    /// <summary>
    /// Update cosmetics display
    /// </summary>
    private void UpdateCosmeticsDisplay(SeasonalEvent seasonalEvent)
    {
        if (_cosmeticsGridContainerNode == null) return;

        // Clear existing cosmetics
        foreach (var child in _cosmeticsGridContainerNode.GetChildren())
        {
            child.QueueFree();
        }

        // Add cosmetics to grid
        foreach (var cosmeticId in seasonalEvent.EventCosmetics)
        {
            var cosmeticItem = CreateCosmeticItem(cosmeticId, seasonalEvent.EventId);
            _cosmeticsGridContainerNode.AddChild(cosmeticItem);
        }

        _cosmeticsGridContainerNode.Visible = seasonalEvent.EventCosmetics.Count > 0;
    }

    /// <summary>
    /// Create cosmetic item
    /// </summary>
    private Control CreateCosmeticItem(string cosmeticId, string eventId)
    {
        var container = new PanelContainer();
        container.Size = new Vector2(80, 100);

        var vbox = new VBoxContainer();
        vbox.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        container.AddChild(vbox);

        // Cosmetic icon (placeholder)
        var iconLabel = new Label();
        iconLabel.Text = GetCosmeticIcon(cosmeticId);
        iconLabel.HorizontalAlignment = HorizontalAlignment.Center;
        iconLabel.VerticalAlignment = VerticalAlignment.Center;
        iconLabel.Size = new Vector2(80, 60);
        vbox.AddChild(iconLabel);

        // Cosmetic name
        var nameLabel = new Label();
        nameLabel.Text = GetCosmeticName(cosmeticId);
        nameLabel.HorizontalAlignment = HorizontalAlignment.Center;
        nameLabel.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        vbox.AddChild(nameLabel);

        // Lock status
        var lockLabel = new Label();
        var isUnlocked = IsCosmeticUnlocked(cosmeticId, eventId);
        lockLabel.Text = isUnlocked ? "✅" : "🔒";
        lockLabel.HorizontalAlignment = HorizontalAlignment.Center;
        vbox.AddChild(lockLabel);

        return container;
    }

    /// <summary>
    /// Get cosmetic icon based on type
    /// </summary>
    private string GetCosmeticIcon(string cosmeticId)
    {
        if (cosmeticId.Contains("hat")) return "🎩";
        if (cosmeticId.Contains("glasses")) return "👓";
        if (cosmeticId.Contains("moustache")) return "👨";
        if (cosmeticId.Contains("wig")) return "💇";
        if (cosmeticId.Contains("projectile")) return "🏹";
        if (cosmeticId.Contains("slingshot")) return "🏹";
        if (cosmeticId.Contains("trail")) return "✨";
        if (cosmeticId.Contains("hit")) return "💥";
        return "🎁";
    }

    /// <summary>
    /// Get cosmetic name from ID
    /// </summary>
    private string GetCosmeticName(string cosmeticId)
    {
        var parts = cosmeticId.Split('_');
        if (parts.Length >= 3)
        {
            var type = parts[1];
            var name = parts[2];
            return $"{char.ToUpper(type[0])}{type.Substring(1)} {char.ToUpper(name[0])}{name.Substring(1)}";
        }
        return cosmeticId;
    }

    /// <summary>
    /// Check if cosmetic is unlocked
    /// </summary>
    private bool IsCosmeticUnlocked(string cosmeticId, string eventId)
    {
        if (_eventManager == null) return false;

        var eventData = _eventManager.GetPlayerEventData(eventId);
        if (eventData == null) return false;

        return eventData.UnlockedCosmetics.Contains(cosmeticId);
    }

    /// <summary>
    /// Update unlock cosmetics button
    /// </summary>
    private void UpdateUnlockButton(SeasonalEvent seasonalEvent)
    {
        if (_unlockCosmeticsButtonNode == null) return;

        var hasCosmetics = seasonalEvent.EventCosmetics.Count > 0;
        _unlockCosmeticsButtonNode.Visible = hasCosmetics && seasonalEvent.CanPlayerParticipate();

        if (hasCosmetics)
        {
            var unlockedCount = 0;
            foreach (var cosmeticId in seasonalEvent.EventCosmetics)
            {
                if (IsCosmeticUnlocked(cosmeticId, seasonalEvent.EventId))
                    unlockedCount++;
            }

            _unlockCosmeticsButtonNode.Text = $"Unlock Event Cosmetics ({unlockedCount}/{seasonalEvent.EventCosmetics.Count})";
        }
    }

    /// <summary>
    /// Select event by ID
    /// </summary>
    private void SelectEvent(string eventId)
    {
        for (int i = 0; i < _activeEvents.Count; i++)
        {
            if (_activeEvents[i].EventId == eventId)
            {
                _currentEventIndex = i;
                ShowCurrentEvent();
                EmitSignal("EventSelected", eventId);
                break;
            }
        }
    }

    /// <summary>
    /// Handle dismiss button pressed
    /// </summary>
    private void OnDismissButtonPressed()
    {
        GD.Print("Seasonal event screen dismissed");
        _countdownUpdateTimer?.Stop();
        EmitSignal("ScreenDismissed");
    }

    /// <summary>
    /// Handle unlock cosmetics button pressed
    /// </summary>
    private void OnUnlockCosmeticsButtonPressed()
    {
        if (_activeEvents.Count == 0) return;

        var currentEvent = _activeEvents[Mathf.Clamp(_currentEventIndex, 0, _activeEvents.Count - 1)];
        GD.Print($"Unlock cosmetics for event: {currentEvent.EventId}");
        EmitSignal("UnlockEventCosmetics", currentEvent.EventId);
    }

    /// <summary>
    /// Handle event started
    /// </summary>
    private void OnEventStarted(string eventId)
    {
        GD.Print($"Event started: {eventId}");
        LoadActiveEvents(); // Refresh events list
    }

    /// <summary>
    /// Handle event ended
    /// </summary>
    private void OnEventEnded(string eventId)
    {
        GD.Print($"Event ended: {eventId}");
        LoadActiveEvents(); // Refresh events list
    }

    /// <summary>
    /// Handle event progress updated
    /// </summary>
    private void OnEventProgressUpdated(string eventId, float progress)
    {
        var currentEvent = _activeEvents.Count > 0 ? _activeEvents[Mathf.Clamp(_currentEventIndex, 0, _activeEvents.Count - 1)] : null;
        if (currentEvent != null && currentEvent.EventId == eventId)
        {
            UpdateEventProgress(currentEvent);
            UpdateUnlockButton(currentEvent);
        }
    }

    /// <summary>
    /// Clean up resources
    /// </summary>
    public override void _ExitTree()
    {
        _countdownUpdateTimer?.Stop();
        _countdownUpdateTimer?.QueueFree();
    }

    /// <summary>
    /// Navigate to next event
    /// </summary>
    public void NextEvent()
    {
        if (_activeEvents.Count <= 1) return;

        _currentEventIndex = (_currentEventIndex + 1) % _activeEvents.Count;
        ShowCurrentEvent();
    }

    /// <summary>
    /// Navigate to previous event
    /// </summary>
    public void PreviousEvent()
    {
        if (_activeEvents.Count <= 1) return;

        _currentEventIndex = _currentEventIndex > 0 ? _currentEventIndex - 1 : _activeEvents.Count - 1;
        ShowCurrentEvent();
    }
}