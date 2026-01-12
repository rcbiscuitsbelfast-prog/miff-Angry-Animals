using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Core event tracking framework for Angry Animals analytics
/// Provides pre-defined events for all critical game actions with context tracking
/// Can be triggered from code or Inspector UI for non-coders
/// </summary>
public class AnalyticsEventTracker : Node
{
    public static AnalyticsEventTracker Instance { get; private set; }

    // Configuration
    private bool _isEnabled = true;
    private bool _trackingEnabled = true;
    private string _userId = "";
    private string _userSegment = "free"; // "free", "premium", "whale"
    
    // Event tracking
    private List<AnalyticsEvent> _eventQueue = new List<AnalyticsEvent>();
    private Dictionary<string, int> _eventCounts = new Dictionary<string, int>();
    private DateTime _sessionStartTime;
    
    // Performance tracking
    private float _lastFrameTime = 0f;
    private int _lowFpsCount = 0;
    private float _memoryUsageThreshold = 500f; // MB
    
    [Signal]
    public delegate void EventLoggedEventHandler(string eventName, Dictionary<string, object> parameters);
    
    [Signal]
    public delegate void PerformanceIssueDetectedEventHandler(string issueType, float value);

    public override void _Ready()
    {
        if (Instance != null)
        {
            QueueFree();
            return;
        }
        
        Instance = this;
        InitializeTracker();
    }

    /// <summary>
    /// Initialize event tracker
    /// </summary>
    private void InitializeTracker()
    {
        _sessionStartTime = DateTime.Now;
        
        // Load user data
        LoadUserData();
        
        // Initialize Firebase integration
        InitializeFirebaseIntegration();
        
        GD.Print("Analytics Event Tracker initialized");
    }

    /// <summary>
    /// Load user data from profile
    /// </summary>
    private void LoadUserData()
    {
        try
        {
            // Get user ID from PlayerProfile or generate one
            if (PlayerProfile.HasProperty("UserId"))
            {
                _userId = PlayerProfile.UserId;
            }
            else
            {
                _userId = GenerateUserId();
                if (PlayerProfile.HasProperty("UserId"))
                {
                    PlayerProfile.UserId = _userId;
                }
            }
            
            // Determine user segment
            _userSegment = DetermineUserSegment();
        }
        catch (Exception e)
        {
            GD.PrintErr($"Error loading user data: {e.Message}");
            _userId = GenerateUserId();
            _userSegment = "free";
        }
    }

    /// <summary>
    /// Initialize Firebase integration
    /// </summary>
    private void InitializeFirebaseIntegration()
    {
        if (FirebaseManager.Instance != null)
        {
            // Set user properties
            FirebaseManager.Instance.SetUserProperty("user_segment", _userSegment);
            FirebaseManager.Instance.SetUserId(_userId);
        }
    }

    /// <summary>
    /// Generate unique user ID
    /// </summary>
    private string GenerateUserId()
    {
        return Guid.NewGuid().ToString("N").Substring(0, 16);
    }

    /// <summary>
    /// Determine user segment based on behavior
    /// </summary>
    private string DetermineUserSegment()
    {
        try
        {
            // Check if user has premium status
            if (PremiumManager.HasProperty("IsPremium") && PremiumManager.IsPremium)
            {
                return "premium";
            }
            
            // Check monetization data for whale detection
            if (MonetizationManager.HasProperty("TotalSpent"))
            {
                float totalSpent = MonetizationManager.TotalSpent;
                if (totalSpent > 50f) return "whale";
                if (totalSpent > 5f) return "payer";
            }
            
            return "free";
        }
        catch
        {
            return "free";
        }
    }

    // ===============================================
    // GAMEPLAY EVENTS
    // ===============================================

    /// <summary>
    /// Track level started event
    /// </summary>
    public void TrackLevelStarted(int levelNumber, string levelType = "normal")
    {
        if (!_trackingEnabled) return;
        
        var parameters = new Dictionary<string, object>
        {
            { "level_number", levelNumber },
            { "level_type", levelType },
            { "user_segment", _userSegment },
            { "session_duration", (DateTime.Now - _sessionStartTime).TotalSeconds },
            { "levels_completed_today", GetLevelsCompletedToday() },
            { "device_type", GetDeviceType() }
        };
        
        LogEvent("level_started", parameters);
    }

    /// <summary>
    /// Track level completed event
    /// </summary>
    public void TrackLevelCompleted(int levelNumber, float completionTime, int attempts = 1, int score = 0, bool perfect = false)
    {
        if (!_trackingEnabled) return;
        
        var parameters = new Dictionary<string, object>
        {
            { "level_number", levelNumber },
            { "completion_time", completionTime },
            { "attempts", attempts },
            { "score", score },
            { "perfect", perfect },
            { "user_segment", _userSegment },
            { "difficulty_rating", GetLevelDifficulty(levelNumber) }
        };
        
        LogEvent("level_completed", parameters);
    }

    /// <summary>
    /// Track level failed event
    /// </summary>
    public void TrackLevelFailed(int levelNumber, int attempts, float timeSpent, string failureReason = "unknown")
    {
        if (!_trackingEnabled) return;
        
        var parameters = new Dictionary<string, object>
        {
            { "level_number", levelNumber },
            { "attempts", attempts },
            { "time_spent", timeSpent },
            { "failure_reason", failureReason },
            { "user_segment", _userSegment },
            { "consecutive_failures", GetConsecutiveFailures() }
        };
        
        LogEvent("level_failed", parameters);
        
        // Track rage quit detection
        CheckRageQuitPattern(levelNumber);
    }

    /// <summary>
    /// Track perfect score achieved event
    /// </summary>
    public void TrackPerfectScoreAchieved(int levelNumber, float completionTime)
    {
        if (!_trackingEnabled) return;
        
        var parameters = new Dictionary<string, object>
        {
            { "level_number", levelNumber },
            { "completion_time", completionTime },
            { "user_segment", _userSegment },
            { "total_perfect_scores", GetTotalPerfectScores() }
        };
        
        LogEvent("perfect_score_achieved", parameters);
    }

    // ===============================================
    // MONETIZATION EVENTS
    // ===============================================

    /// <summary>
    /// Track cosmetic purchased event
    /// </summary>
    public void TrackCosmeticPurchased(string cosmeticType, string cosmeticId, float cost, string currency = "USD")
    {
        if (!_trackingEnabled) return;
        
        var parameters = new Dictionary<string, object>
        {
            { "cosmetic_type", cosmeticType },
            { "cosmetic_id", cosmeticId },
            { "cost", cost },
            { "currency", currency },
            { "user_segment", _userSegment },
            { "total_spent", GetTotalSpent() },
            { "purchase_source", GetPurchaseSource() }
        };
        
        LogEvent("cosmetic_purchased", parameters);
    }

    /// <summary>
    /// Track cosmetic unlocked event
    /// </summary>
    public void TrackCosmeticUnlocked(string cosmeticType, string cosmeticId, string unlockMethod = "purchase")
    {
        if (!_trackingEnabled) return;
        
        var parameters = new Dictionary<string, object>
        {
            { "cosmetic_type", cosmeticType },
            { "cosmetic_id", cosmeticId },
            { "unlock_method", unlockMethod },
            { "user_segment", _userSegment },
            { "total_cosmetics_unlocked", GetTotalCosmeticsUnlocked() }
        };
        
        LogEvent("cosmetic_unlocked", parameters);
    }

    /// <summary>
    /// Track battle pass purchased event
    /// </summary>
    public void TrackBattlePassPurchased(float cost, string currency = "USD", int season = 1)
    {
        if (!_trackingEnabled) return;
        
        var parameters = new Dictionary<string, object>
        {
            { "cost", cost },
            { "currency", currency },
            { "season", season },
            { "user_segment", _userSegment },
            { "battle_pass_owner", true }
        };
        
        LogEvent("battle_pass_purchased", parameters);
    }

    /// <summary>
    /// Track remove ads purchased event
    /// </summary>
    public void TrackRemoveAdsPurchased(float cost, string currency = "USD")
    {
        if (!_trackingEnabled) return;
        
        var parameters = new Dictionary<string, object>
        {
            { "cost", cost },
            { "currency", currency },
            { "user_segment", _userSegment },
            { "ads_removed", true },
            { "previous_ads_purchased", GetPreviousAdsPurchased() }
        };
        
        LogEvent("remove_ads_purchased", parameters);
    }

    /// <summary>
    /// Track rewarded ad watched event
    /// </summary>
    public void TrackRewardedAdWatched(string rewardType, float rewardAmount, string adSource = "admob")
    {
        if (!_trackingEnabled) return;
        
        var parameters = new Dictionary<string, object>
        {
            { "reward_type", rewardType },
            { "reward_amount", rewardAmount },
            { "ad_source", adSource },
            { "user_segment", _userSegment },
            { "ads_watched_today", GetAdsWatchedToday() }
        };
        
        LogEvent("rewarded_ad_watched", parameters);
    }

    // ===============================================
    // ENGAGEMENT EVENTS
    // ===============================================

    /// <summary>
    /// Track daily login streak reached event
    /// </summary>
    public void TrackDailyLoginStreakReached(int streakDays)
    {
        if (!_trackingEnabled) return;
        
        var parameters = new Dictionary<string, object>
        {
            { "streak_days", streakDays },
            { "user_segment", _userSegment },
            { "longest_streak", GetLongestStreak() },
            { "is_new_record", streakDays > GetLongestStreak() }
        };
        
        LogEvent("daily_login_streak_reached", parameters);
    }

    /// <summary>
    /// Track achievement unlocked event
    /// </summary>
    public void TrackAchievementUnlocked(string achievementId, string achievementType = "progressive")
    {
        if (!_trackingEnabled) return;
        
        var parameters = new Dictionary<string, object>
        {
            { "achievement_id", achievementId },
            { "achievement_type", achievementType },
            { "user_segment", _userSegment },
            { "total_achievements", GetTotalAchievements() },
            { "rarity", GetAchievementRarity(achievementId) }
        };
        
        LogEvent("achievement_unlocked", parameters);
    }

    /// <summary>
    /// Track seasonal event started event
    /// </summary>
    public void TrackSeasonalEventStarted(string eventId, string eventType)
    {
        if (!_trackingEnabled) return;
        
        var parameters = new Dictionary<string, object>
        {
            { "event_id", eventId },
            { "event_type", eventType },
            { "user_segment", _userSegment },
            { "participation_count", GetEventParticipationCount(eventId) }
        };
        
        LogEvent("seasonal_event_started", parameters);
    }

    // ===============================================
    // QUALITY EVENTS
    // ===============================================

    /// <summary>
    /// Track crash detected event
    /// </summary>
    public void TrackCrashDetected(string crashType, string sceneName = "", string additionalInfo = "")
    {
        if (!_trackingEnabled) return;
        
        var parameters = new Dictionary<string, object>
        {
            { "crash_type", crashType },
            { "scene_name", sceneName },
            { "additional_info", additionalInfo },
            { "platform", OS.GetName() },
            { "device_type", GetDeviceType() },
            { "session_duration", (DateTime.Now - _sessionStartTime).TotalSeconds }
        };
        
        LogEvent("crash_detected", parameters);
        
        // Report to Firebase Crashlytics
        if (FirebaseManager.Instance != null)
        {
            FirebaseManager.Instance.ReportCrash(crashType, additionalInfo, parameters);
        }
    }

    /// <summary>
    /// Track performance frame drop event
    /// </summary>
    public void TrackPerformanceFrameDrop(float fps, float frameTime)
    {
        if (!_trackingEnabled) return;
        
        _lowFpsCount++;
        
        if (_lowFpsCount >= 5) // Threshold for reporting
        {
            var parameters = new Dictionary<string, object>
            {
                { "fps", fps },
                { "frame_time", frameTime },
                { "device_type", GetDeviceType() },
                { "memory_usage", GetMemoryUsage() },
                { "platform", OS.GetName() }
            };
            
            LogEvent("performance_frame_drop", parameters);
            EmitSignal("PerformanceIssueDetected", "low_fps", fps);
            
            _lowFpsCount = 0; // Reset counter
        }
    }

    /// <summary>
    /// Track memory warning event
    /// </summary>
    public void TrackMemoryWarning(float memoryUsage)
    {
        if (!_trackingEnabled) return;
        
        if (memoryUsage > _memoryUsageThreshold)
        {
            var parameters = new Dictionary<string, object>
            {
                { "memory_usage", memoryUsage },
                { "memory_threshold", _memoryUsageThreshold },
                { "device_type", GetDeviceType() },
                { "platform", OS.GetName() }
            };
            
            LogEvent("memory_warning", parameters);
            EmitSignal("PerformanceIssueDetected", "high_memory", memoryUsage);
        }
    }

    // ===============================================
    // CORE LOGIC
    // ===============================================

    /// <summary>
    /// Log analytics event
    /// </summary>
    public void LogEvent(string eventName, Dictionary<string, object> parameters = null)
    {
        if (!_isEnabled || !_trackingEnabled) return;
        
        try
        {
            var evt = new AnalyticsEvent
            {
                EventName = eventName,
                Timestamp = DateTime.Now,
                UserId = _userId,
                Parameters = parameters ?? new Dictionary<string, object>()
            };
            
            // Add common parameters
            evt.Parameters["platform"] = OS.GetName();
            evt.Parameters["user_segment"] = _userSegment;
            evt.Parameters["session_id"] = GetSessionId();
            
            _eventQueue.Add(evt);
            
            // Update event counts
            if (!_eventCounts.ContainsKey(eventName))
            {
                _eventCounts[eventName] = 0;
            }
            _eventCounts[eventName]++;
            
            // Send to Firebase
            if (FirebaseManager.Instance != null)
            {
                FirebaseManager.Instance.LogEvent(eventName, evt.Parameters);
            }
            
            EmitSignal("EventLogged", eventName, evt.Parameters);
            
            // Auto-flush if queue is large
            if (_eventQueue.Count > 50)
            {
                ProcessEventQueue();
            }
        }
        catch (Exception e)
        {
            GD.PrintErr($"Error logging event '{eventName}': {e.Message}");
        }
    }

    /// <summary>
    /// Process event queue
    /// </summary>
    private void ProcessEventQueue()
    {
        try
        {
            // Save events to local storage
            SaveEventsToStorage();
            
            // Clear processed events
            _eventQueue.Clear();
        }
        catch (Exception e)
        {
            GD.PrintErr($"Error processing event queue: {e.Message}");
        }
    }

    /// <summary>
    /// Save events to local storage
    /// </summary>
    private void SaveEventsToStorage()
    {
        try
        {
            var filePath = "user://analytics_events.json";
            var allEvents = new List<AnalyticsEvent>();
            
            // Load existing events
            if (FileAccess.FileExists(filePath))
            {
                using (var file = FileAccess.Open(filePath, FileAccess.ModeFlags.Read))
                {
                    string json = file.GetAsText();
                    var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var existingEvents = System.Text.Json.JsonSerializer.Deserialize<List<AnalyticsEvent>>(json, options);
                    if (existingEvents != null)
                    {
                        allEvents = existingEvents;
                    }
                }
            }
            
            // Add new events
            allEvents.AddRange(_eventQueue);
            
            // Keep only last 1000 events
            if (allEvents.Count > 1000)
            {
                allEvents = allEvents.TakeLast(1000).ToList();
            }
            
            // Save back to file
            using (var file = FileAccess.Open(filePath, FileAccess.ModeFlags.Write))
            {
                var options = new System.Text.Json.JsonSerializerOptions { WriteIndented = true };
                string json = System.Text.Json.JsonSerializer.Serialize(allEvents, options);
                file.StoreString(json);
            }
        }
        catch (Exception e)
        {
            GD.PrintErr($"Error saving events to storage: {e.Message}");
        }
    }

    // ===============================================
    // HELPER METHODS
    // ===============================================

    /// <summary>
    /// Check rage quit pattern
    /// </summary>
    private void CheckRageQuitPattern(int levelNumber)
    {
        // Implementation would track rapid succession failures
        // For now, simplified version
        var recentFailures = GetRecentFailures();
        if (recentFailures.Count >= 3)
        {
            LogEvent("rage_quit_detected", new Dictionary<string, object>
            {
                { "level_number", levelNumber },
                { "failure_count", recentFailures.Count },
                { "time_span_minutes", GetTimeSpanMinutes(recentFailures) }
            });
        }
    }

    /// <summary>
    /// Get device type classification
    /// </summary>
    private string GetDeviceType()
    {
        // Simple device classification based on performance
        float totalMemory = OS.GetStaticMemoryUsage() / (1024f * 1024f); // Convert to MB
        
        if (totalMemory > 1000) return "high_end";
        if (totalMemory > 500) return "mid_range";
        return "low_end";
    }

    /// <summary>
    /// Get current session ID
    /// </summary>
    private string GetSessionId()
    {
        return _sessionStartTime.ToString("yyyyMMdd_HHmmss");
    }

    // Placeholder methods - would integrate with actual game systems
    private int GetLevelsCompletedToday() => 0;
    private string GetLevelDifficulty(int level) => "medium";
    private int GetConsecutiveFailures() => 0;
    private int GetTotalPerfectScores() => 0;
    private float GetTotalSpent() => 0f;
    private string GetPurchaseSource() => "unknown";
    private int GetTotalCosmeticsUnlocked() => 0;
    private int GetPreviousAdsPurchased() => 0;
    private int GetAdsWatchedToday() => 0;
    private int GetLongestStreak() => 0;
    private int GetTotalAchievements() => 0;
    private string GetAchievementRarity(string achievementId) => "common";
    private int GetEventParticipationCount(string eventId) => 0;
    private List<DateTime> GetRecentFailures() => new List<DateTime>();
    private float GetTimeSpanMinutes(List<DateTime> failures) => 0f;
    private float GetMemoryUsage() => OS.GetStaticMemoryUsage() / (1024f * 1024f);

    // ===============================================
    // PUBLIC API
    // ===============================================

    /// <summary>
    /// Enable/disable event tracking
    /// </summary>
    public void SetTrackingEnabled(bool enabled)
    {
        _trackingEnabled = enabled;
    }

    /// <summary>
    /// Get event counts for this session
    /// </summary>
    public Dictionary<string, int> GetEventCounts()
    {
        return new Dictionary<string, int>(_eventCounts);
    }

    /// <summary>
    /// Get session duration
    /// </summary>
    public TimeSpan GetSessionDuration()
    {
        return DateTime.Now - _sessionStartTime;
    }

    /// <summary>
    /// Force process event queue
    /// </summary>
    public void FlushEvents()
    {
        ProcessEventQueue();
    }

    /// <summary>
    /// Export analytics data
    /// </summary>
    public void ExportAnalytics(string filePath)
    {
        try
        {
            var exportData = new
            {
                export_timestamp = DateTime.Now.ToString("O"),
                session_info = new
                {
                    session_id = GetSessionId(),
                    duration = GetSessionDuration().ToString(),
                    platform = OS.GetName(),
                    user_segment = _userSegment
                },
                event_counts = _eventCounts,
                total_events = _eventCounts.Values.Sum(),
                events = _eventQueue.Select(evt => new
                {
                    event_name = evt.EventName,
                    timestamp = evt.Timestamp.ToString("O"),
                    parameters = evt.Parameters
                }).ToList()
            };

            var options = new System.Text.Json.JsonSerializerOptions { WriteIndented = true };
            string json = System.Text.Json.JsonSerializer.Serialize(exportData, options);
            
            using (var file = FileAccess.Open(filePath, FileAccess.ModeFlags.Write))
            {
                file.StoreString(json);
            }
            
            GD.Print($"Analytics data exported to: {filePath}");
        }
        catch (Exception e)
        {
            GD.PrintErr($"Error exporting analytics data: {e.Message}");
        }
    }
}

/// <summary>
/// Analytics event data structure
/// </summary>
public class AnalyticsEvent
{
    public string EventName { get; set; }
    public DateTime Timestamp { get; set; }
    public string UserId { get; set; }
    public Dictionary<string, object> Parameters { get; set; } = new Dictionary<string, object>();
}