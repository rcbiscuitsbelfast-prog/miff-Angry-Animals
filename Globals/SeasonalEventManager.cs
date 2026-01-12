using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Godot.Collections;

/// <summary>
/// SeasonalEventManager manages active events and event lifecycle
/// Handles event activation, deactivation, and progress tracking
/// </summary>
public partial class SeasonalEventManager : Node
{
    public static SeasonalEventManager Instance { get; private set; }

    [Signal] public delegate void EventStartedEventHandler(string eventId);
    [Signal] public delegate void EventEndedEventHandler(string eventId);
    [Signal] public delegate void EventProgressUpdatedEventHandler(string eventId, float progress);
    [Signal] public delegate void EventCompletedEventHandler(string eventId);

    [Export] private bool _enableEventSystem = true;
    [Export] private float _eventCheckInterval = 300f; // 5 minutes

    private Dictionary<string, SeasonalEvent> _availableEvents = new();
    private Dictionary<string, SeasonalEventData> _playerEventData = new();
    private Timer _eventCheckTimer;
    private SeasonalEventDatabase _eventDatabase;

    public override void _Ready()
    {
        if (Instance != null)
        {
            QueueFree();
            return;
        }
        
        Instance = this;
        ProcessMode = ProcessModeEnum.Always;
        
        InitializeEventSystem();
        
        GD.Print("SeasonalEventManager initialized");
    }

    public override void _ExitTree()
    {
        SaveEventData();
    }

    /// <summary>
    /// Initialize the event system
    /// </summary>
    private void InitializeEventSystem()
    {
        if (!_enableEventSystem) return;

        _eventDatabase = new SeasonalEventDatabase();
        LoadAvailableEvents();
        LoadPlayerEventData();
        SetupEventCheckTimer();
        
        CheckAndActivateEvents();
    }

    /// <summary>
    /// Load all available events from database
    /// </summary>
    private void LoadAvailableEvents()
    {
        _availableEvents = _eventDatabase.GetAllEvents();
        GD.Print($"Loaded {_availableEvents.Count} seasonal events");
    }

    /// <summary>
    /// Setup timer for periodic event checking
    /// </summary>
    private void SetupEventCheckTimer()
    {
        _eventCheckTimer = new Timer();
        _eventCheckTimer.WaitTime = _eventCheckInterval;
        _eventCheckTimer.Timeout += CheckAndActivateEvents;
        
        AddChild(_eventCheckTimer);
        _eventCheckTimer.Start();
    }

    /// <summary>
    /// Check and activate/deactivate events based on dates
    /// </summary>
    private void CheckAndActivateEvents()
    {
        foreach (var kvp in _availableEvents)
        {
            var eventId = kvp.Key;
            var seasonalEvent = kvp.Value;
            
            var wasActive = seasonalEvent.IsActive;
            var shouldBeActive = seasonalEvent.IsEventActive();
            
            if (shouldBeActive && !wasActive)
            {
                // Event starting
                ActivateEvent(eventId, seasonalEvent);
            }
            else if (!shouldBeActive && wasActive)
            {
                // Event ending
                DeactivateEvent(eventId, seasonalEvent);
            }
        }
        
        // Check for events that should end soon
        CheckEndingEvents();
    }

    /// <summary>
    /// Activate a seasonal event
    /// </summary>
    private void ActivateEvent(string eventId, SeasonalEvent seasonalEvent)
    {
        seasonalEvent.IsActive = true;
        
        // Create player event data if it doesn't exist
        if (!_playerEventData.ContainsKey(eventId))
        {
            _playerEventData[eventId] = new SeasonalEventData(eventId);
        }
        
        EmitSignal("EventStarted", eventId);
        
        // Track analytics
        TrackEventStarted(eventId, seasonalEvent);
        
        // Show notification to player
        ShowEventStartNotification(seasonalEvent);
        
        GD.Print($"🎉 Event activated: {seasonalEvent.EventName}");
    }

    /// <summary>
    /// Deactivate a seasonal event
    /// </summary>
    private void DeactivateEvent(string eventId, SeasonalEvent seasonalEvent)
    {
        seasonalEvent.IsActive = false;
        
        // Track analytics
        TrackEventEnded(eventId, seasonalEvent);
        
        EmitSignal("EventEnded", eventId);
        
        GD.Print($"📅 Event ended: {seasonalEvent.EventName}");
    }

    /// <summary>
    /// Check for events ending soon and show warnings
    /// </summary>
    private void CheckEndingEvents()
    {
        var warningThreshold = TimeSpan.FromHours(24); // 24 hours before end
        
        foreach (var kvp in _availableEvents)
        {
            var seasonalEvent = kvp.Value;
            if (seasonalEvent.IsActive)
            {
                var timeRemaining = seasonalEvent.GetTimeRemaining();
                if (timeRemaining <= warningThreshold && timeRemaining > TimeSpan.Zero)
                {
                    // Show "ending soon" notification
                    ShowEventEndingSoonNotification(seasonalEvent);
                }
            }
        }
    }

    /// <summary>
    /// Show event start notification
    /// </summary>
    private void ShowEventStartNotification(SeasonalEvent seasonalEvent)
    {
        var message = $"🎉 {seasonalEvent.EventName} has begun!";
        var body = $"{seasonalEvent.EventDescription}\n\nExclusive cosmetics await!";
        
        NotificationManager.Instance?.SendInstantNotification(message, body);
        
        // Track notification analytics
        AnalyticsManager.Instance?.LogEvent("seasonal_event_started", new Dictionary<string, object>
        {
            ["event_id"] = seasonalEvent.EventId,
            ["event_name"] = seasonalEvent.EventName,
            ["event_theme"] = seasonalEvent.EventTheme,
            ["notification_sent"] = true
        });
    }

    /// <summary>
    /// Show event ending soon notification
    /// </summary>
    private void ShowEventEndingSoonNotification(SeasonalEvent seasonalEvent)
    {
        var timeRemaining = seasonalEvent.GetTimeRemaining();
        var hoursRemaining = Mathf.CeilToInt((float)timeRemaining.TotalHours);
        
        var message = $"⏰ {seasonalEvent.EventName} ending soon!";
        var body = $"Only {hoursRemaining} hours left to unlock exclusive cosmetics!";
        
        NotificationManager.Instance?.SendInstantNotification(message, body);
    }

    /// <summary>
    /// Get all active events
    /// </summary>
    public List<SeasonalEvent> GetActiveEvents()
    {
        var activeEvents = new List<SeasonalEvent>();
        
        foreach (var kvp in _availableEvents)
        {
            if (kvp.Value.IsActive)
            {
                activeEvents.Add(kvp.Value);
            }
        }
        
        return activeEvents.OrderByDescending(e => e.GetEventPriority()).ToList();
    }

    /// <summary>
    /// Get upcoming events
    /// </summary>
    public List<SeasonalEvent> GetUpcomingEvents()
    {
        var upcomingEvents = new List<SeasonalEvent>();
        var now = DateTime.UtcNow;
        
        foreach (var kvp in _availableEvents)
        {
            if (kvp.Value.StartDate > now)
            {
                upcomingEvents.Add(kvp.Value);
            }
        }
        
        return upcomingEvents.OrderBy(e => e.StartDate).ToList();
    }

    /// <summary>
    /// Get event by ID
    /// </summary>
    public SeasonalEvent? GetEvent(string eventId)
    {
        return _availableEvents.GetValueOrDefault(eventId);
    }

    /// <summary>
    /// Get player event data
    /// </summary>
    public SeasonalEventData? GetPlayerEventData(string eventId)
    {
        return _playerEventData.GetValueOrDefault(eventId);
    }

    /// <summary>
    /// Update event challenge progress
    /// </summary>
    public void UpdateEventChallengeProgress(string eventId, string challengeId, float progress)
    {
        if (_playerEventData.TryGetValue(eventId, out var eventData))
        {
            eventData.UpdateChallengeProgress(challengeId, progress);
            EmitSignal("EventProgressUpdated", eventId, progress);
        }
    }

    /// <summary>
    /// Unlock cosmetic from event
    /// </summary>
    public void UnlockEventCosmetic(string eventId, string cosmeticId)
    {
        if (_playerEventData.TryGetValue(eventId, out var eventData))
        {
            eventData.UnlockCosmetic(cosmeticId);
            
            // Track analytics
            AnalyticsManager.Instance?.LogEvent("cosmetic_earned_from_event", new Dictionary<string, object>
            {
                ["event_id"] = eventId,
                ["cosmetic_id"] = cosmeticId,
                ["acquisition_method"] = "event_progress"
            });
        }
    }

    /// <summary>
    /// Check if player can access event cosmetics
    /// </summary>
    public bool CanAccessEventCosmetics(string eventId)
    {
        var seasonalEvent = GetEvent(eventId);
        if (seasonalEvent == null || !seasonalEvent.CanPlayerParticipate())
            return false;

        var eventData = GetPlayerEventData(eventId);
        if (eventData == null)
            return false;

        return eventData.AreAllChallengesCompleted() || eventData.GetCompletionPercentage() >= 0.5f; // 50% progress required
    }

    /// <summary>
    /// Get event cosmetics available to player
    /// </summary>
    public List<string> GetAvailableEventCosmetics(string eventId)
    {
        var availableCosmetics = new List<string>();
        var seasonalEvent = GetEvent(eventId);
        
        if (seasonalEvent == null) return availableCosmetics;

        var eventData = GetPlayerEventData(eventId);
        var completionPercentage = eventData?.GetCompletionPercentage() ?? 0f;

        foreach (var cosmeticId in seasonalEvent.EventCosmetics)
        {
            if (completionPercentage >= 1f || eventData?.UnlockedCosmetics.Contains(cosmeticId) == true)
            {
                availableCosmetics.Add(cosmeticId);
            }
        }

        return availableCosmetics;
    }

    /// <summary>
    /// Create new seasonal event
    /// </summary>
    public void CreateSeasonalEvent(SeasonalEvent seasonalEvent)
    {
        if (string.IsNullOrEmpty(seasonalEvent.EventId))
        {
            seasonalEvent.EventId = GenerateEventId(seasonalEvent.EventName);
        }

        _availableEvents[seasonalEvent.EventId] = seasonalEvent;
        _eventDatabase.SaveEvent(seasonalEvent);
        
        GD.Print($"Created seasonal event: {seasonalEvent.EventName}");
    }

    /// <summary>
    /// Generate unique event ID
    /// </summary>
    private string GenerateEventId(string eventName)
    {
        var baseId = eventName.Replace(" ", "_").ToLower();
        var timestamp = DateTime.UtcNow.Ticks;
        return $"{baseId}_{timestamp}";
    }

    /// <summary>
    /// Track event started analytics
    /// </summary>
    private void TrackEventStarted(string eventId, SeasonalEvent seasonalEvent)
    {
        AnalyticsManager.Instance?.LogEvent("seasonal_event_started", new Dictionary<string, object>
        {
            ["event_id"] = eventId,
            ["event_name"] = seasonalEvent.EventName,
            ["event_theme"] = seasonalEvent.EventTheme,
            ["start_date"] = seasonalEvent.StartDate.ToString("O"),
            ["end_date"] = seasonalEvent.EndDate.ToString("O"),
            ["event_duration_hours"] = seasonalEvent.GetEventDuration().TotalHours
        });
    }

    /// <summary>
    /// Track event ended analytics
    /// </summary>
    private void TrackEventEnded(string eventId, SeasonalEvent seasonalEvent)
    {
        AnalyticsManager.Instance?.LogEvent("seasonal_event_ended", new Dictionary<string, object>
        {
            ["event_id"] = eventId,
            ["event_name"] = seasonalEvent.EventName,
            ["end_date"] = seasonalEvent.EndDate.ToString("O"),
            ["actual_duration_hours"] = seasonalEvent.GetEventDuration().TotalHours
        });
    }

    /// <summary>
    /// Save player event data
    /// </summary>
    private void SaveEventData()
    {
        // This would integrate with PlayerProfile saving
        foreach (var kvp in _playerEventData)
        {
            var eventData = kvp.Value;
            var serializedData = eventData.Serialize();
            GD.Print($"Saving event data for {kvp.Key}: {serializedData}");
        }
    }

    /// <summary>
    /// Load player event data
    /// </summary>
    private void LoadPlayerEventData()
    {
        // This would integrate with PlayerProfile loading
        // For now, we'll start fresh
        GD.Print("Loaded player event data");
    }

    /// <summary>
    /// Get event statistics for analytics
    /// </summary>
    public Dictionary<string, Variant> GetEventAnalytics(string eventId)
    {
        var analytics = new Dictionary<string, Variant>();
        
        var seasonalEvent = GetEvent(eventId);
        if (seasonalEvent != null)
        {
            analytics["event_name"] = seasonalEvent.EventName;
            analytics["is_active"] = seasonalEvent.IsActive;
            analytics["event_progress"] = seasonalEvent.GetEventProgress();
            analytics["time_remaining"] = seasonalEvent.GetFormattedTimeRemaining();
        }

        var eventData = GetPlayerEventData(eventId);
        if (eventData != null)
        {
            analytics["player_completion"] = eventData.GetCompletionPercentage();
            analytics["event_completed"] = eventData.EventCompleted;
            analytics["unlocked_cosmetics"] = eventData.UnlockedCosmetics.Count;
        }

        return analytics;
    }
}