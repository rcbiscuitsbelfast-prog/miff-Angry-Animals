using System;
using System.Collections.Generic;
using Godot;
using Godot.Collections;
using Newtonsoft.Json;

/// <summary>
/// ScriptableObject for defining seasonal events
/// </summary>
[GlobalClass]
public partial class SeasonalEvent : Resource
{
    [Signal] public delegate void EventStartedEventHandler(string eventId);
    [Signal] public delegate void EventEndedEventHandler(string eventId);

    [Export] public string EventId { get; set; } = "";
    [Export] public string EventName { get; set; } = "";
    [Export] public string EventDescription { get; set; } = "";
    [Export] public string EventTheme { get; set; } = "";
    
    [Export] public DateTime StartDate { get; set; }
    [Export] public DateTime EndDate { get; set; }
    
    [Export] public Color ThemeColor { get; set; } = Colors.Blue;
    [Export] public Texture2D EventBackground { get; set; }
    [Export] public AudioStream EventMusic { get; set; }
    
    [Export] public Array<string> EventCosmetics { get; set; } = new Array<string>();
    [Export] public Array<string> EventChallenges { get; set; } = new Array<string>();
    
    [Export] public Dictionary<string, Variant> EventRewards { get; set; } = new Dictionary<string, Variant>();
    
    [Export] public bool IsActive { get; set; } = false;
    [Export] public bool IsRepeating { get; set; } = false;

    public override void _Ready()
    {
        if (string.IsNullOrEmpty(EventId))
        {
            EventId = GenerateEventId();
        }
    }

    /// <summary>
    /// Generate unique event ID
    /// </summary>
    private string GenerateEventId()
    {
        return $"{EventName}_{StartDate.Year}_{StartDate.Month}";
    }

    /// <summary>
    /// Check if event is currently active
    /// </summary>
    public bool IsEventActive()
    {
        var now = DateTime.UtcNow;
        return now >= StartDate && now <= EndDate;
    }

    /// <summary>
    /// Check if event starts in the future
    /// </summary>
    public bool IsEventScheduled()
    {
        return DateTime.UtcNow < StartDate;
    }

    /// <summary>
    /// Check if event has ended
    /// </summary>
    public bool IsEventEnded()
    {
        return DateTime.UtcNow > EndDate;
    }

    /// <summary>
    /// Get time remaining until event ends
    /// </summary>
    public TimeSpan GetTimeRemaining()
    {
        var now = DateTime.UtcNow;
        if (now > EndDate) return TimeSpan.Zero;
        return EndDate - now;
    }

    /// <summary>
    /// Get time until event starts
    /// </summary>
    public TimeSpan GetTimeUntilStart()
    {
        var now = DateTime.UtcNow;
        if (now >= StartDate) return TimeSpan.Zero;
        return StartDate - now;
    }

    /// <summary>
    /// Get event duration
    /// </summary>
    public TimeSpan GetEventDuration()
    {
        return EndDate - StartDate;
    }

    /// <summary>
    /// Get formatted time remaining string
    /// </summary>
    public string GetFormattedTimeRemaining()
    {
        var remaining = GetTimeRemaining();
        if (remaining <= TimeSpan.Zero) return "Event Ended";

        var days = remaining.Days;
        var hours = remaining.Hours;
        var minutes = remaining.Minutes;

        if (days > 0)
            return $"{days}d {hours}h remaining";
        else if (hours > 0)
            return $"{hours}h {minutes}m remaining";
        else
            return $"{minutes}m remaining";
    }

    /// <summary>
    /// Get formatted start time string
    /// </summary>
    public string GetFormattedStartTime()
    {
        var timeUntilStart = GetTimeUntilStart();
        if (timeUntilStart <= TimeSpan.Zero) return "Event Active";

        var days = timeUntilStart.Days;
        var hours = timeUntilStart.Hours;
        var minutes = timeUntilStart.Minutes;

        if (days > 0)
            return $"Starts in {days}d {hours}h";
        else if (hours > 0)
            return $"Starts in {hours}h {minutes}m";
        else
            return $"Starts in {minutes}m";
    }

    /// <summary>
    /// Get event progress (0.0 to 1.0)
    /// </summary>
    public float GetEventProgress()
    {
        var now = DateTime.UtcNow;
        if (now <= StartDate) return 0f;
        if (now >= EndDate) return 1f;

        var totalDuration = GetEventDuration();
        var elapsed = now - StartDate;
        
        return (float)(elapsed.TotalSeconds / totalDuration.TotalSeconds);
    }

    /// <summary>
    /// Check if player can participate in event
    /// </summary>
    public bool CanPlayerParticipate()
    {
        return IsEventActive() && !IsEventEnded();
    }

    /// <summary>
    /// Get event priority for sorting (higher = more important)
    /// </summary>
    public int GetEventPriority()
    {
        if (!IsEventActive()) return 0;
        if (IsEventScheduled()) return 1;
        return 2; // Active events have highest priority
    }

    /// <summary>
    /// Serialize event data for persistence
    /// </summary>
    public Dictionary<string, Variant> SerializeEventData()
    {
        return new Dictionary<string, Variant>
        {
            ["event_id"] = EventId,
            ["event_name"] = EventName,
            ["event_description"] = EventDescription,
            ["event_theme"] = EventTheme,
            ["start_date"] = StartDate.ToString("O"),
            ["end_date"] = EndDate.ToString("O"),
            ["theme_color"] = ThemeColor.ToHtml(),
            ["is_active"] = IsActive,
            ["is_repeating"] = IsRepeating,
            ["event_cosmetics"] = new Array(EventCosmetics),
            ["event_challenges"] = new Array(EventChallenges)
        };
    }

    /// <summary>
    /// Deserialize event data
    /// </summary>
    public void DeserializeEventData(Dictionary<string, Variant> data)
    {
        EventId = data.GetValueOrDefault("event_id", "").ToString();
        EventName = data.GetValueOrDefault("event_name", "").ToString();
        EventDescription = data.GetValueOrDefault("event_description", "").ToString();
        EventTheme = data.GetValueOrDefault("event_theme", "").ToString();
        
        var startDateStr = data.GetValueOrDefault("start_date", "").ToString();
        if (!string.IsNullOrEmpty(startDateStr) && DateTime.TryParse(startDateStr, out var startDate))
            StartDate = startDate;

        var endDateStr = data.GetValueOrDefault("end_date", "").ToString();
        if (!string.IsNullOrEmpty(endDateStr) && DateTime.TryParse(endDateStr, out var endDate))
            EndDate = endDate;

        var colorStr = data.GetValueOrDefault("theme_color", "").ToString();
        if (!string.IsNullOrEmpty(colorStr))
            ThemeColor = Color.FromHtml(colorStr);

        IsActive = data.GetValueOrDefault("is_active", false);
        IsRepeating = data.GetValueOrDefault("is_repeating", false);

        var cosmetics = data.GetValueOrDefault("event_cosmetics", new Array()).As<Array>();
        EventCosmetics = new Array<string>(cosmetics);

        var challenges = data.GetValueOrDefault("event_challenges", new Array()).As<Array>();
        EventChallenges = new Array<string>(challenges);
    }
}

/// <summary>
/// Player's progress in a seasonal event
/// </summary>
public partial class SeasonalEventData : Node
{
    [Signal] public delegate void EventProgressUpdatedEventHandler(string eventId, float progress);
    [Signal] public delegate void EventCompletedEventHandler(string eventId);

    public string EventId { get; private set; }
    public DateTime ParticipationStartDate { get; private set; }
    public Dictionary<string, float> ChallengeProgress { get; private set; } = new();
    public List<string> UnlockedCosmetics { get; private set; } = new();
    public bool EventCompleted { get; private set; } = false;
    public float CompletionPercentage { get; private set; } = 0f;

    public SeasonalEventData(string eventId)
    {
        EventId = eventId;
        ParticipationStartDate = DateTime.UtcNow;
    }

    /// <summary>
    /// Update challenge progress
    /// </summary>
    public void UpdateChallengeProgress(string challengeId, float progress)
    {
        if (ChallengeProgress.ContainsKey(challengeId))
        {
            ChallengeProgress[challengeId] = Mathf.Clamp(progress, 0f, 1f);
        }
        else
        {
            ChallengeProgress[challengeId] = Mathf.Clamp(progress, 0f, 1f);
        }

        UpdateCompletionPercentage();
        EmitSignal("EventProgressUpdated", EventId, CompletionPercentage);
    }

    /// <summary>
    /// Mark cosmetic as unlocked
    /// </summary>
    public void UnlockCosmetic(string cosmeticId)
    {
        if (!UnlockedCosmetics.Contains(cosmeticId))
        {
            UnlockedCosmetics.Add(cosmeticId);
            UpdateCompletionPercentage();
        }
    }

    /// <summary>
    /// Check if all challenges are completed
    /// </summary>
    public bool AreAllChallengesCompleted()
    {
        foreach (var progress in ChallengeProgress.Values)
        {
            if (progress < 1f) return false;
        }
        return true;
    }

    /// <summary>
    /// Update overall completion percentage
    /// </summary>
    private void UpdateCompletionPercentage()
    {
        if (ChallengeProgress.Count == 0)
        {
            CompletionPercentage = 1f; // No challenges means automatically complete
        }
        else
        {
            var totalProgress = 0f;
            foreach (var progress in ChallengeProgress.Values)
            {
                totalProgress += progress;
            }
            CompletionPercentage = totalProgress / ChallengeProgress.Count;
        }

        if (CompletionPercentage >= 1f && !EventCompleted)
        {
            EventCompleted = true;
            EmitSignal("EventCompleted", EventId);
        }
    }

    /// <summary>
    /// Get completion percentage
    /// </summary>
    public float GetCompletionPercentage()
    {
        return CompletionPercentage;
    }

    /// <summary>
    /// Serialize event progress data
    /// </summary>
    public Dictionary<string, Variant> Serialize()
    {
        return new Dictionary<string, Variant>
        {
            ["event_id"] = EventId,
            ["participation_start_date"] = ParticipationStartDate.ToString("O"),
            ["challenge_progress"] = ChallengeProgress,
            ["unlocked_cosmetics"] = new Array(UnlockedCosmetics),
            ["event_completed"] = EventCompleted,
            ["completion_percentage"] = CompletionPercentage
        };
    }

    /// <summary>
    /// Deserialize event progress data
    /// </summary>
    public void Deserialize(Dictionary<string, Variant> data)
    {
        EventId = data.GetValueOrDefault("event_id", "").ToString();
        
        var participationDateStr = data.GetValueOrDefault("participation_start_date", "").ToString();
        if (!string.IsNullOrEmpty(participationDateStr) && DateTime.TryParse(participationDateStr, out var date))
            ParticipationStartDate = date;

        var progressDict = data.GetValueOrDefault("challenge_progress", new Dictionary<string, float>()).As<Dictionary<string, float>>();
        ChallengeProgress = new Dictionary<string, float>(progressDict);

        var unlockedCosmetics = data.GetValueOrDefault("unlocked_cosmetics", new Array()).As<Array>();
        UnlockedCosmetics = new List<string>(unlockedCosmetics.Select(x => x.ToString()));

        EventCompleted = data.GetValueOrDefault("event_completed", false);
        CompletionPercentage = (float)data.GetValueOrDefault("completion_percentage", 0f);
    }
}