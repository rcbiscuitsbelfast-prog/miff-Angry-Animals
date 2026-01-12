using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

/// <summary>
/// Analytics framework for tracking gameplay telemetry
/// Collects usage data, performance metrics, and user behavior analytics
/// </summary>
public class AnalyticsManager : Node
{
    public static AnalyticsManager Instance { get; private set; }

    // Analytics data storage
    private AnalyticsData _analyticsData;
    private List<AnalyticsEvent> _eventQueue = new List<AnalyticsEvent>();
    private string _analyticsFilePath = "user://analytics_data.json";
    
    // Analytics configuration
    private AnalyticsConfig _config;
    private bool _isEnabled = true;
    private bool _userConsent = false;
    
    // Event batching
    private System.Timers.Timer _batchTimer;
    private const int BATCH_SIZE = 10;
    private const float BATCH_INTERVAL = 30f; // 30 seconds
    
    [Signal]
    public delegate void AnalyticsEventLoggedEventHandler(AnalyticsEvent analyticsEvent);
    
    [Signal]
    public delegate void AnalyticsConsentChangedEventHandler(bool consented);
    
    [Signal]
    public delegate void AnalyticsDataUploadedEventHandler(int eventCount);

    public override void _Ready()
    {
        if (Instance != null)
        {
            QueueFree();
            return;
        }
        
        Instance = this;
        InitializeAnalytics();
    }

    /// <summary>
    /// Initialize analytics system
    /// </summary>
    private void InitializeAnalytics()
    {
        LoadAnalyticsData();
        LoadConfiguration();
        InitializeBatchTimer();
        
        // Check consent status
        _userConsent = CheckUserConsent();
        
        if (_userConsent && _isEnabled)
        {
            StartAnalytics();
        }
        
        GD.Print("Analytics system initialized");
    }

    /// <summary>
    /// Load analytics data from file
    /// </summary>
    private void LoadAnalyticsData()
    {
        try
        {
            if (File.Exists(_analyticsFilePath))
            {
                string jsonContent = File.ReadAllText(_analyticsFilePath);
                _analyticsData = JsonSerializer.Deserialize<AnalyticsData>(jsonContent) ?? CreateDefaultAnalyticsData();
            }
            else
            {
                _analyticsData = CreateDefaultAnalyticsData();
            }
        }
        catch (Exception e)
        {
            GD.PrintErr($"Failed to load analytics data: {e.Message}");
            _analyticsData = CreateDefaultAnalyticsData();
        }
    }

    /// <summary>
    /// Create default analytics data structure
    /// </summary>
    private AnalyticsData CreateDefaultAnalyticsData()
    {
        return new AnalyticsData
        {
            UserId = GenerateUserId(),
            FirstSessionDate = DateTime.Now,
            TotalSessions = 0,
            TotalPlayTime = TimeSpan.Zero,
            Sessions = new List<SessionData>(),
            Events = new List<AnalyticsEvent>(),
            GameplayMetrics = new GameplayMetrics(),
            DeviceInfo = CollectDeviceInfo()
        };
    }

    /// <summary>
    /// Load analytics configuration
    /// </summary>
    private void LoadConfiguration()
    {
        _config = new AnalyticsConfig
        {
            Enabled = true,
            BatchSize = BATCH_SIZE,
            BatchInterval = BATCH_INTERVAL,
            TrackSessions = true,
            TrackGameplay = true,
            TrackPerformance = true,
            TrackCrashes = true,
            PrivacyCompliant = true,
            RetentionDays = 90,
            DataTypes = new List<string>
            {
                "sessions",
                "level_progress", 
                "feature_usage",
                "performance",
                "crashes",
                "user_preferences"
            }
        };
    }

    /// <summary>
    /// Initialize batch timer for event processing
    /// </summary>
    private void InitializeBatchTimer()
    {
        _batchTimer = new System.Timers.Timer(_config.BatchInterval * 1000);
        _batchTimer.Elapsed += OnBatchTimer;
        _batchTimer.Start();
    }

    /// <summary>
    /// Start analytics tracking
    /// </summary>
    public void StartAnalytics()
    {
        if (!_userConsent || !_isEnabled)
        {
            GD.Print("Analytics disabled due to user consent or configuration");
            return;
        }
        
        _analyticsData.TotalSessions++;
        _analyticsData.CurrentSessionStart = DateTime.Now;
        
        LogEvent("session_start", new Dictionary<string, object>
        {
            { "session_id", _analyticsData.TotalSessions },
            { "timestamp", DateTime.Now.ToString("O") }
        });
        
        // Track session start
        TrackSessionStart();
        
        GD.Print("Analytics tracking started");
    }

    /// <summary>
    /// Stop analytics tracking
    /// </summary>
    public void StopAnalytics()
    {
        if (_analyticsData.CurrentSessionStart != DateTime.MinValue)
        {
            var sessionDuration = DateTime.Now - _analyticsData.CurrentSessionStart;
            _analyticsData.TotalPlayTime += sessionDuration;
            
            LogEvent("session_end", new Dictionary<string, object>
            {
                { "session_id", _analyticsData.TotalSessions },
                { "duration", sessionDuration.TotalSeconds },
                { "timestamp", DateTime.Now.ToString("O") }
            });
            
            // Save session data
            SaveSessionData(sessionDuration);
        }
        
        // Process remaining events
        ProcessEventBatch();
        
        GD.Print("Analytics tracking stopped");
    }

    /// <summary>
    /// Log analytics event
    /// </summary>
    public void LogEvent(string eventName, Dictionary<string, object> properties = null)
    {
        if (!_userConsent || !_isEnabled) return;
        
        var analyticsEvent = new AnalyticsEvent
        {
            EventId = Guid.NewGuid().ToString(),
            EventName = eventName,
            Timestamp = DateTime.Now,
            UserId = _analyticsData.UserId,
            SessionId = _analyticsData.TotalSessions,
            Properties = properties ?? new Dictionary<string, object>(),
            DeviceInfo = _analyticsData.DeviceInfo
        };
        
        _eventQueue.Add(analyticsEvent);
        
        // Process special events immediately
        if (eventName == "crash" || eventName == "level_completed")
        {
            ProcessEventBatch();
        }
        
        EmitSignal("AnalyticsEventLogged", analyticsEvent);
        
        // Auto-save if queue is getting large
        if (_eventQueue.Count >= BATCH_SIZE * 2)
        {
            ProcessEventBatch();
        }
    }

    /// <summary>
    /// Track level progression
    /// </summary>
    public void TrackLevelProgress(int levelNumber, bool completed, int attempts = 1, float timeSpent = 0f)
    {
        LogEvent("level_progress", new Dictionary<string, object>
        {
            { "level_number", levelNumber },
            { "completed", completed },
            { "attempts", attempts },
            { "time_spent", timeSpent },
            { "difficulty", GetLevelDifficulty(levelNumber) },
            { "total_levels_unlocked", GetTotalUnlockedLevels() }
        });
        
        // Update gameplay metrics
        UpdateGameplayMetrics(levelNumber, completed, attempts, timeSpent);
    }

    /// <summary>
    /// Track feature usage
    /// </summary>
    public void TrackFeatureUsage(string featureName, Dictionary<string, object> properties = null)
    {
        var eventProps = new Dictionary<string, object>
        {
            { "feature_name", featureName }
        };
        
        if (properties != null)
        {
            foreach (var kvp in properties)
            {
                eventProps[kvp.Key] = kvp.Value;
            }
        }
        
        LogEvent("feature_usage", eventProps);
    }

    /// <summary>
    /// Track cosmetics usage
    /// </summary>
    public void TrackCosmeticsUsage(string cosmeticType, string cosmeticId, bool unlocked, float cost = 0f)
    {
        LogEvent("cosmetics_usage", new Dictionary<string, object>
        {
            { "cosmetic_type", cosmeticType },
            { "cosmetic_id", cosmeticId },
            { "unlocked", unlocked },
            { "cost", cost },
            { "currency_type", cost > 0 ? "premium" : "free" }
        });
    }

    /// <summary>
    /// Track IAP events
    /// </summary>
    public void TrackIapEvent(string productId, string eventType, float amount = 0f, string currency = "USD")
    {
        LogEvent("iap_event", new Dictionary<string, object>
        {
            { "product_id", productId },
            { "event_type", eventType }, // "purchase", "refund", "consumption"
            { "amount", amount },
            { "currency", currency },
            { "platform", OS.GetName() }
        });
    }

    /// <summary>
    /// Track advertisement events
    /// </summary>
    public void TrackAdEvent(string adType, string eventType, string adUnitId = "", float revenue = 0f)
    {
        LogEvent("ad_event", new Dictionary<string, object>
        {
            { "ad_type", adType }, // "banner", "interstitial", "rewarded"
            { "event_type", eventType }, // "loaded", "shown", "clicked", "dismissed"
            { "ad_unit_id", adUnitId },
            { "revenue", revenue },
            { "currency", "USD" }
        });
    }

    /// <summary>
    /// Track performance metrics
    /// </summary>
    public void TrackPerformanceMetrics(float fps, float memoryUsage, int activeObjects, float frameTime)
    {
        if (!_config.TrackPerformance) return;
        
        LogEvent("performance_metrics", new Dictionary<string, object>
        {
            { "fps", fps },
            { "memory_usage_mb", memoryUsage },
            { "active_objects", activeObjects },
            { "frame_time_ms", frameTime * 1000f },
            { "device_tier", DetermineDeviceTier(fps, memoryUsage) }
        });
    }

    /// <summary>
    /// Track crash events
    /// </summary>
    public void TrackCrash(string crashType, string stackTrace, Dictionary<string, object> additionalData = null)
    {
        var eventProps = new Dictionary<string, object>
        {
            { "crash_type", crashType },
            { "stack_trace", stackTrace },
            { "scene_path", GetTree().CurrentScene?.SceneFilePath ?? "" },
            { "godot_version", Engine.GetVersionInfo()["major"].ToString() + "." + Engine.GetVersionInfo()["minor"].ToString() }
        };
        
        if (additionalData != null)
        {
            foreach (var kvp in additionalData)
            {
                eventProps[kvp.Key] = kvp.Value;
            }
        }
        
        LogEvent("crash", eventProps);
        
        // Update analytics data
        _analyticsData.GameplayMetrics.TotalCrashes++;
    }

    /// <summary>
    /// Track daily login engagement
    /// </summary>
    public void TrackDailyLogin(int streakDay, bool rewardClaimed, int sessionDuration)
    {
        LogEvent("daily_login", new Dictionary<string, object>
        {
            { "streak_day", streakDay },
            { "reward_claimed", rewardClaimed },
            { "session_duration_minutes", sessionDuration },
            { "player_segment", DeterminePlayerSegment() }
        });
    }

    /// <summary>
    /// Track streak milestone achievements
    /// </summary>
    public void TrackStreakMilestone(int milestoneDay, int totalStreak, bool celebrationShown)
    {
        LogEvent("daily_streak_milestone", new Dictionary<string, object>
        {
            { "milestone_day", milestoneDay },
            { "total_streak", totalStreak },
            { "milestone_type", GetMilestoneType(milestoneDay) },
            { "celebration_shown", celebrationShown }
        });
    }

    /// <summary>
    /// Track seasonal event participation
    /// </summary>
    public void TrackSeasonalEventParticipation(string eventId, string eventName, bool joined, float completionRate)
    {
        LogEvent("seasonal_event_participation", new Dictionary<string, object>
        {
            { "event_id", eventId },
            { "event_name", eventName },
            { "joined_event", joined },
            { "completion_rate", completionRate },
            { "participation_date", DateTime.UtcNow.ToString("O") }
        });
    }

    /// <summary>
    /// Track notification engagement
    /// </summary>
    public void TrackNotificationEngagement(string notificationType, bool opened, int timeToOpenMinutes)
    {
        LogEvent("notification_engagement", new Dictionary<string, object>
        {
            { "notification_type", notificationType },
            { "notification_opened", opened },
            { "time_to_open_minutes", timeToOpenMinutes },
            { "platform", OS.GetName() }
        });
    }

    /// <summary>
    /// Track retention metrics
    /// </summary>
    public void TrackRetentionMetrics(int daysActive, int currentStreak, int bestStreak, List<string> eventsParticipated)
    {
        LogEvent("retention_metrics", new Dictionary<string, object>
        {
            { "days_active", daysActive },
            { "current_streak", currentStreak },
            { "best_streak", bestStreak },
            { "events_participated_count", eventsParticipated.Count },
            { "retention_tier", DetermineRetentionTier(daysActive, currentStreak) }
        });
    }

    /// <summary>
    /// Process event batch
    /// </summary>
    private void ProcessEventBatch()
    {
        if (_eventQueue.Count == 0) return;
        
        var batch = _eventQueue.Take(BATCH_SIZE).ToList();
        _eventQueue.RemoveRange(0, batch.Count);
        
        // Add to analytics data
        _analyticsData.Events.AddRange(batch);
        
        // Save to file
        SaveAnalyticsData();
        
        // In a real implementation, this would upload to analytics backend
        // For now, we just save locally
        EmitSignal("AnalyticsDataUploaded", batch.Count);
        
        GD.Print($"Processed analytics batch: {batch.Count} events");
    }

    /// <summary>
    /// Batch timer callback
    /// </summary>
    private void OnBatchTimer(object sender, System.Timers.ElapsedEventArgs e)
    {
        ProcessEventBatch();
    }

    /// <summary>
    /// Save analytics data to file
    /// </summary>
    private void SaveAnalyticsData()
    {
        try
        {
            // Prune old events based on retention policy
            PruneOldEvents();
            
            var options = new JsonSerializerOptions 
            { 
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            
            string json = JsonSerializer.Serialize(_analyticsData, options);
            File.WriteAllText(_analyticsFilePath, json);
        }
        catch (Exception e)
        {
            GD.PrintErr($"Failed to save analytics data: {e.Message}");
        }
    }

    /// <summary>
    /// Prune old events based on retention policy
    /// </summary>
    private void PruneOldEvents()
    {
        var cutoffDate = DateTime.Now.AddDays(-_config.RetentionDays);
        
        // Prune old events
        _analyticsData.Events.RemoveAll(e => e.Timestamp < cutoffDate);
        
        // Prune old sessions
        _analyticsData.Sessions.RemoveAll(s => s.StartTime < cutoffDate);
    }

    /// <summary>
    /// Generate user ID
    /// </summary>
    private string GenerateUserId()
    {
        return Guid.NewGuid().ToString("N").Substring(0, 16);
    }

    /// <summary>
    /// Check user consent for analytics
    /// </summary>
    private bool CheckUserConsent()
    {
        // This would integrate with PrivacyPolicyManager
        // For now, return true for analytics
        return true;
    }

    /// <summary>
    /// Collect device information
    /// </summary>
    private DeviceInfo CollectDeviceInfo()
    {
        return new DeviceInfo
        {
            Platform = OS.GetName(),
            Model = OS.GetModel(),
            OsVersion = OS.GetVersion(),
            Architecture = OS.GetArchitecture(),
            ProcessorCount = OS.GetProcessorCount(),
            ScreenWidth = DisplayServer.ScreenGetSize().X,
            ScreenHeight = DisplayServer.ScreenGetSize().Y,
            VramSize = OS.GetVideoAdapterMemorySize(),
            GodotVersion = Engine.GetVersionInfo()["string"].ToString()
        };
    }

    /// <summary>
    /// Determine player segment based on behavior
    /// </summary>
    private string DeterminePlayerSegment()
    {
        // This would integrate with streak and engagement data
        // For now, return a placeholder based on session count
        var sessions = _analyticsData.TotalSessions;
        
        if (sessions < 7) return "new_player";
        if (sessions < 30) return "growing_player";
        if (sessions < 100) return "established_player";
        return "veteran_player";
    }

    /// <summary>
    /// Get milestone type for analytics
    /// </summary>
    private string GetMilestoneType(int milestoneDay)
    {
        return milestoneDay switch
        {
            7 => "week_1",
            14 => "week_2", 
            21 => "week_3",
            30 => "month_master",
            _ => $"day_{milestoneDay}"
        };
    }

    /// <summary>
    /// Determine retention tier for analytics
    /// </summary>
    private string DetermineRetentionTier(int daysActive, int currentStreak)
    {
        if (currentStreak >= 30) return "legendary_player";
        if (currentStreak >= 14) return "dedicated_player";
        if (currentStreak >= 7) return "engaged_player";
        if (currentStreak >= 3) return "building_player";
        if (daysActive >= 7) return "regular_player";
        return "casual_player";
    }

    /// <summary>
    /// Get level difficulty for analytics
    /// </summary>
    private string GetLevelDifficulty(int levelNumber)
    {
        if (levelNumber <= 10) return "easy";
        if (levelNumber <= 25) return "medium";
        if (levelNumber <= 50) return "hard";
        return "expert";
    }

    /// <summary>
    /// Get total unlocked levels
    /// </summary>
    private int GetTotalUnlockedLevels()
    {
        return PlayerProfile.Instance?.HighestUnlockedRoomIndex ?? 0;
    }

    /// <summary>
    /// Determine device tier for performance analytics
    /// </summary>
    private string DetermineDeviceTier(float fps, float memoryUsage)
    {
        if (fps >= 60 && memoryUsage < 500) return "high_end";
        if (fps >= 30 && memoryUsage < 1000) return "mid_range";
        return "low_end";
    }

    /// <summary>
    /// Track session start
    /// </summary>
    private void TrackSessionStart()
    {
        var sessionData = new SessionData
        {
            SessionId = _analyticsData.TotalSessions,
            StartTime = DateTime.Now,
            DeviceInfo = _analyticsData.DeviceInfo,
            GameVersion = "1.0.0" // Would get from VersionInfo
        };
        
        _analyticsData.Sessions.Add(sessionData);
    }

    /// <summary>
    /// Save session data
    /// </summary>
    private void SaveSessionData(TimeSpan duration)
    {
        var session = _analyticsData.Sessions.LastOrDefault();
        if (session != null)
        {
            session.EndTime = DateTime.Now;
            session.Duration = duration;
            session.EventsLogged = _eventQueue.Count;
        }
    }

    /// <summary>
    /// Update gameplay metrics
    /// </summary>
    private void UpdateGameplayMetrics(int levelNumber, bool completed, int attempts, float timeSpent)
    {
        var metrics = _analyticsData.GameplayMetrics;
        
        if (completed)
        {
            metrics.LevelsCompleted++;
            metrics.TotalAttempts += attempts;
            metrics.TotalPlayTime += timeSpent;
            
            // Calculate success rate
            metrics.SuccessRate = metrics.LevelsCompleted / (float)metrics.TotalAttempts * 100f;
        }
        else
        {
            metrics.TotalAttempts++;
        }
        
        // Update difficulty progression
        if (levelNumber > metrics.HighestLevelUnlocked)
        {
            metrics.HighestLevelUnlocked = levelNumber;
        }
    }

    /// <summary>
    /// Get level difficulty
    /// </summary>
    private string GetLevelDifficulty(int levelNumber)
    {
        if (levelNumber <= 10) return "Easy";
        if (levelNumber <= 30) return "Medium";
        if (levelNumber <= 60) return "Hard";
        return "Expert";
    }

    /// <summary>
    /// Get total unlocked levels
    /// </summary>
    private int GetTotalUnlockedLevels()
    {
        return _analyticsData.GameplayMetrics.HighestLevelUnlocked;
    }

    /// <summary>
    /// Determine device tier based on performance
    /// </summary>
    private string DetermineDeviceTier(float fps, float memoryUsage)
    {
        if (fps >= 55 && memoryUsage < 200) return "High-End";
        if (fps >= 35 && memoryUsage < 400) return "Mid-Range";
        return "Low-End";
    }

    /// <summary>
    /// Set analytics enabled state
    /// </summary>
    public void SetEnabled(bool enabled)
    {
        _isEnabled = enabled;
        
        if (!enabled)
        {
            StopAnalytics();
        }
    }

    /// <summary>
    /// Set user consent
    /// </summary>
    public void SetUserConsent(bool consented)
    {
        _userConsent = consented;
        EmitSignal("AnalyticsConsentChanged", consented);
        
        if (consented && _isEnabled)
        {
            StartAnalytics();
        }
        else
        {
            StopAnalytics();
        }
    }

    /// <summary>
    /// Get analytics data
    /// </summary>
    public AnalyticsData GetAnalyticsData()
    {
        return _analyticsData;
    }

    /// <summary>
    /// Generate analytics report
    /// </summary>
    public AnalyticsReport GenerateReport()
    {
        var report = new AnalyticsReport
        {
            GeneratedAt = DateTime.Now,
            UserId = _analyticsData.UserId,
            TotalSessions = _analyticsData.TotalSessions,
            TotalPlayTime = _analyticsData.TotalPlayTime,
            AverageSessionLength = _analyticsData.TotalSessions > 0 ? _analyticsData.TotalPlayTime.TotalMinutes / _analyticsData.TotalSessions : 0,
            LevelsCompleted = _analyticsData.GameplayMetrics.LevelsCompleted,
            SuccessRate = _analyticsData.GameplayMetrics.SuccessRate,
            TotalAttempts = _analyticsData.GameplayMetrics.TotalAttempts,
            HighestLevelUnlocked = _analyticsData.GameplayMetrics.HighestLevelUnlocked,
            TotalCrashes = _analyticsData.GameplayMetrics.TotalCrashes,
            RetentionData = new Dictionary<int, int>(_analyticsData.GameplayMetrics.RetentionData
                .ToDictionary(kvp => int.Parse(kvp.Key.Replace("day_", "")), kvp => kvp.Value))
        };
        
        return report;
    }

    /// <summary>
    /// Export analytics data
    /// </summary>
    public void ExportAnalytics(string filePath)
    {
        try
        {
            var report = GenerateReport();
            
            var options = new JsonSerializerOptions 
            { 
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            
            string json = JsonSerializer.Serialize(report, options);
            File.WriteAllText(filePath, json);
            
            GD.Print($"Analytics report exported: {filePath}");
        }
        catch (Exception e)
        {
            GD.PrintErr($"Failed to export analytics: {e.Message}");
        }
    }

    /// <summary>
    /// Clear all analytics data
    /// </summary>
    public void ClearData()
    {
        _analyticsData = CreateDefaultAnalyticsData();
        _eventQueue.Clear();
        SaveAnalyticsData();
        
        GD.Print("Analytics data cleared");
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _batchTimer?.Dispose();
            StopAnalytics();
        }
        base.Dispose(disposing);
    }
}

/// <summary>
/// Analytics event data structure
/// </summary>
public class AnalyticsEvent
{
    public string EventId { get; set; }
    public string EventName { get; set; }
    public DateTime Timestamp { get; set; }
    public string UserId { get; set; }
    public int SessionId { get; set; }
    public Dictionary<string, object> Properties { get; set; } = new Dictionary<string, object>();
    public DeviceInfo DeviceInfo { get; set; }
}

/// <summary>
/// Analytics data container
/// </summary>
public class AnalyticsData
{
    public string UserId { get; set; }
    public DateTime FirstSessionDate { get; set; }
    public int TotalSessions { get; set; }
    public TimeSpan TotalPlayTime { get; set; }
    public DateTime CurrentSessionStart { get; set; }
    public List<SessionData> Sessions { get; set; } = new List<SessionData>();
    public List<AnalyticsEvent> Events { get; set; } = new List<AnalyticsEvent>();
    public GameplayMetrics GameplayMetrics { get; set; } = new GameplayMetrics();
    public DeviceInfo DeviceInfo { get; set; }
}

/// <summary>
/// Session data
/// </summary>
public class SessionData
{
    public int SessionId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public TimeSpan Duration { get; set; }
    public int EventsLogged { get; set; }
    public DeviceInfo DeviceInfo { get; set; }
}

/// <summary>
/// Gameplay metrics
/// </summary>
public class GameplayMetrics
{
    public int LevelsCompleted { get; set; }
    public int TotalAttempts { get; set; }
    public float SuccessRate { get; set; }
    public int HighestLevelUnlocked { get; set; }
    public float TotalPlayTime { get; set; }
    public int TotalCrashes { get; set; }
    public Dictionary<string, int> RetentionData { get; set; } = new Dictionary<string, int>();
}

/// <summary>
/// Device information
/// </summary>
public class DeviceInfo
{
    public string Platform { get; set; }
    public string Model { get; set; }
    public string OsVersion { get; set; }
    public string Architecture { get; set; }
    public int ProcessorCount { get; set; }
    public float ScreenWidth { get; set; }
    public float ScreenHeight { get; set; }
    public int VramSize { get; set; }
    public string GodotVersion { get; set; }
}

/// <summary>
/// Analytics configuration
/// </summary>
public class AnalyticsConfig
{
    public bool Enabled { get; set; }
    public int BatchSize { get; set; }
    public float BatchInterval { get; set; }
    public bool TrackSessions { get; set; }
    public bool TrackGameplay { get; set; }
    public bool TrackPerformance { get; set; }
    public bool TrackCrashes { get; set; }
    public bool PrivacyCompliant { get; set; }
    public int RetentionDays { get; set; }
    public List<string> DataTypes { get; set; } = new List<string>();
}

/// <summary>
/// Analytics report
/// </summary>
public class AnalyticsReport
{
    public DateTime GeneratedAt { get; set; }
    public string UserId { get; set; }
    public int TotalSessions { get; set; }
    public TimeSpan TotalPlayTime { get; set; }
    public double AverageSessionLength { get; set; }
    public int LevelsCompleted { get; set; }
    public float SuccessRate { get; set; }
    public int TotalAttempts { get; set; }
    public int HighestLevelUnlocked { get; set; }
    public int TotalCrashes { get; set; }
    public Dictionary<int, int> RetentionData { get; set; } = new Dictionary<int, int>();
}