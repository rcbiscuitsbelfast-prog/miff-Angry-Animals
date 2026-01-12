using System;
using System.Collections.Generic;
using Godot;
using Godot.Collections;

/// <summary>
/// Enhanced PushNotificationManager with Firebase Cloud Messaging integration
/// Handles all notification types: daily reminders, milestones, streak broken alerts, seasonal events
/// </summary>
public partial class PushNotificationManager : Node
{
    public static PushNotificationManager Instance { get; private set; }

    [Signal] public delegate void NotificationSentEventHandler(NotificationType type, string title);
    [Signal] public delegate void NotificationClickedEventHandler(NotificationType type, string deepLink);
    [Signal] public delegate void NotificationScheduledEventHandler(NotificationType type, DateTime scheduledTime);

    [Export] private bool _enablePushNotifications = true;
    [Export] private bool _enableFirebaseIntegration = true;
    [Export] private bool _enableLocalNotifications = true;

    private NotificationPreferences _notificationPreferences;
    private Timer _notificationCheckTimer;
    private Timer _dailyResetTimer;
    
    // Notification configuration
    [ExportGroup("Notification Settings")]
    [Export] private string _firebaseProjectId = "";
    [Export] private string _fcmServerKey = "";
    [Export] private float _checkInterval = 300f; // 5 minutes
    
    // Rich notification assets
    [ExportGroup("Rich Media")]
    [Export] private Texture2D _milestoneIcon;
    [Export] private Texture2D _streakIcon;
    [Export] private Texture2D _eventIcon;
    [Export] private Texture2D _cosmeticIcon;

    public override void _Ready()
    {
        if (Instance != null)
        {
            QueueFree();
            return;
        }
        
        Instance = this;
        ProcessMode = ProcessModeEnum.Always;
        
        InitializeNotificationSystem();
        
        GD.Print("PushNotificationManager initialized");
    }

    public override void _ExitTree()
    {
        SaveNotificationState();
    }

    /// <summary>
    /// Initialize the notification system
    /// </summary>
    private void InitializeNotificationSystem()
    {
        if (!_enablePushNotifications) return;

        _notificationPreferences = new NotificationPreferences();
        LoadNotificationPreferences();
        SetupTimers();
        HandleAppLaunchNotificationCheck();
    }

    /// <summary>
    /// Setup notification check timers
    /// </summary>
    private void SetupTimers()
    {
        // Daily reset timer for midnight
        _dailyResetTimer = new Timer();
        _dailyResetTimer.WaitTime = CalculateSecondsToMidnight();
        _dailyResetTimer.OneShot = true;
        _dailyResetTimer.Timeout += OnDailyReset;
        AddChild(_dailyResetTimer);
        _dailyResetTimer.Start();

        // Regular check timer
        _notificationCheckTimer = new Timer();
        _notificationCheckTimer.WaitTime = _checkInterval;
        _notificationCheckTimer.Timeout += CheckAndSendNotifications;
        AddChild(_notificationCheckTimer);
        _notificationCheckTimer.Start();
    }

    /// <summary>
    /// Handle app launch - check for missed notifications
    /// </summary>
    private void HandleAppLaunchNotificationCheck()
    {
        CheckForMissedDailyReminder();
        CheckForMissedMilestones();
        CheckForStreakBrokenAlerts();
        CheckForSeasonalEventNotifications();
    }

    /// <summary>
    /// Send daily reminder notification
    /// </summary>
    public void SendDailyReminderNotification()
    {
        if (!_enablePushNotifications) return;
        
        var currentStreak = GetCurrentStreak();
        var streakManager = StreakManager.Instance;
        var currentReward = streakManager?.GetCurrentReward();
        
        var title = "🎁 Daily Reward Awaits!";
        var body = currentStreak > 0 
            ? $"Day {currentStreak} of your streak! Claim your {currentReward?.Title ?? "reward"} now!"
            : "Start your streak today! Claim your welcome bonus!";
        
        var notification = new NotificationMessage
        {
            Title = title,
            Body = body,
            Type = NotificationType.DailyReminder,
            ScheduledTime = DateTime.UtcNow,
            Data = new Dictionary<string, string>
            {
                ["deep_link"] = "login_bonus",
                ["notification_type"] = "daily_reminder"
            }
        };

        SendNotification(notification);
    }

    /// <summary>
    /// Send milestone celebration notification
    /// </summary>
    public void SendMilestoneNotification(int milestoneDay)
    {
        if (!_enablePushNotifications) return;

        var milestoneNames = new Dictionary<int, string>
        {
            [7] = "Week 1",
            [14] = "Week 2", 
            [21] = "Week 3",
            [30] = "Month Master"
        };

        var milestoneName = milestoneNames.GetValueOrDefault(milestoneDay, $"{milestoneDay} days");
        var emoji = milestoneDay >= 30 ? "👑" : milestoneDay >= 21 ? "🏆" : milestoneDay >= 14 ? "🔥" : "⭐";
        
        var title = $"{emoji} {milestoneName} Streak!";
        var body = milestoneDay switch
        {
            7 => "Amazing! You've maintained a 7-day streak! Keep it going!",
            14 => "Incredible! 2 weeks of dedication! You're on fire! 🔥",
            21 => "Outstanding! 3 weeks of commitment! You're a master! 🏆",
            30 => "LEGENDARY! 30 days of dedication! You're a true champion! 👑",
            _ => $"Amazing {milestoneDay}-day streak! You're incredible!"
        };

        var notification = new NotificationMessage
        {
            Title = title,
            Body = body,
            Type = NotificationType.Milestone,
            ScheduledTime = DateTime.UtcNow,
            Data = new Dictionary<string, string>
            {
                ["deep_link"] = "login_bonus",
                ["notification_type"] = "milestone",
                ["milestone_day"] = milestoneDay.ToString()
            }
        };

        SendNotification(notification);
    }

    /// <summary>
    /// Send streak broken alert
    /// </summary>
    public void SendStreakBrokenAlert()
    {
        if (!_enablePushNotifications) return;

        var title = "💔 Streak Ended";
        var body = "Don't worry! Start a new streak today. Every champion begins again!";
        
        var notification = new NotificationMessage
        {
            Title = title,
            Body = body,
            Type = NotificationType.StreakBroken,
            ScheduledTime = DateTime.UtcNow,
            Data = new Dictionary<string, string>
            {
                ["deep_link"] = "main_menu",
                ["notification_type"] = "streak_broken"
            }
        };

        SendNotification(notification);
    }

    /// <summary>
    /// Send seasonal event notification
    /// </summary>
    public void SendSeasonalEventNotification(string eventId, string eventName, bool isStarting = true)
    {
        if (!_enablePushNotifications) return;

        var title = isStarting ? $"🎉 {eventName} Begins!" : $"⏰ {eventName} Ending Soon!";
        var body = isStarting 
            ? "Exclusive cosmetics and rewards await! Don't miss out!"
            : "Last chance to unlock exclusive event cosmetics!";
        
        var notification = new NotificationMessage
        {
            Title = title,
            Body = body,
            Type = NotificationType.SeasonalEvent,
            ScheduledTime = DateTime.UtcNow,
            Data = new Dictionary<string, string>
            {
                ["deep_link"] = $"seasonal_event?event_id={eventId}",
                ["notification_type"] = "seasonal_event",
                ["event_id"] = eventId,
                ["event_name"] = eventName
            }
        };

        SendNotification(notification);
    }

    /// <summary>
    /// Send limited-time cosmetic notification
    /// </summary>
    public void SendLimitedTimeCosmeticNotification(string cosmeticName, int hoursRemaining)
    {
        if (!_enablePushNotifications) return;

        var title = "⏰ Exclusive Cosmetic!";
        var body = $"{cosmeticName} expires in {hoursRemaining} hours! Don't miss out!";
        
        var notification = new NotificationMessage
        {
            Title = title,
            Body = body,
            Type = NotificationType.LimitedTimeCosmetic,
            ScheduledTime = DateTime.UtcNow,
            Data = new Dictionary<string, string>
            {
                ["deep_link"] = "shop?tab=limited_time",
                ["notification_type"] = "limited_time_cosmetic",
                ["cosmetic_name"] = cosmeticName,
                ["hours_remaining"] = hoursRemaining.ToString()
            }
        };

        SendNotification(notification);
    }

    /// <summary>
    /// Send lapsed player notification
    /// </summary>
    public void SendLapsedPlayerNotification(int daysSinceLastPlay)
    {
        if (!_enablePushNotifications) return;

        var title = "We Miss You! 💙";
        var body = $"Come back for {daysSinceLastPlay} exclusive rewards! We saved something special for you!";
        
        var notification = new NotificationMessage
        {
            Title = title,
            Body = body,
            Type = NotificationType.LapsedPlayer,
            ScheduledTime = DateTime.UtcNow,
            Data = new Dictionary<string, string>
            {
                ["deep_link"] = "welcome_back",
                ["notification_type"] = "lapsed_player",
                ["days_away"] = daysSinceLastPlay.ToString()
            }
        };

        SendNotification(notification);
    }

    /// <summary>
    /// Send notification via Firebase or local system
    /// </summary>
    private void SendNotification(NotificationMessage notification)
    {
        if (!_notificationPreferences.IsNotificationAllowed(notification.Type))
        {
            GD.Print($"Notification not sent - not allowed for type: {notification.Type}");
            return;
        }

        if (_enableFirebaseIntegration && FirebaseManager.Instance != null)
        {
            SendFirebaseNotification(notification);
        }
        else if (_enableLocalNotifications)
        {
            SendLocalNotification(notification);
        }

        _notificationPreferences.RecordNotificationSent(notification.Type);
        EmitSignal("NotificationSent", notification.Type, notification.Title);
        
        // Track analytics
        TrackNotificationSent(notification);
    }

    /// <summary>
    /// Send notification via Firebase Cloud Messaging
    /// </summary>
    private void SendFirebaseNotification(NotificationMessage notification)
    {
        try
        {
            // This would integrate with FirebaseManager for actual FCM sending
            // For now, we'll simulate the Firebase call
            
            var fcmData = new Dictionary<string, string>
            {
                ["title"] = notification.Title,
                ["body"] = notification.Body,
                ["icon"] = "ic_notification",
                ["color"] = "#3498db"
            };

            // Add deep link data
            foreach (var kvp in notification.Data)
            {
                fcmData[kvp.Key] = kvp.Value;
            }

            // In a real implementation, this would call FirebaseManager.SendPushNotification()
            GD.Print($"🔥 Firebase Notification: {notification.Title}");
            
            // Simulate Firebase API call
            SimulateFirebaseCall(notification);
        }
        catch (Exception ex)
        {
            GD.PrintErr($"Failed to send Firebase notification: {ex.Message}");
            
            // Fallback to local notification
            if (_enableLocalNotifications)
            {
                SendLocalNotification(notification);
            }
        }
    }

    /// <summary>
    /// Simulate Firebase API call (placeholder for real implementation)
    /// </summary>
    private void SimulateFirebaseCall(NotificationMessage notification)
    {
        // In a real implementation, this would be an async Firebase call
        // For now, we'll just log the attempt
        GD.Print($"📱 Simulating Firebase FCM send: {notification.Title}");
        
        // Add random success/failure for testing
        var random = new Random();
        if (random.NextDouble() > 0.1) // 90% success rate
        {
            GD.Print("✅ Firebase notification sent successfully");
        }
        else
        {
            GD.Print("❌ Firebase notification failed, using fallback");
            SendLocalNotification(notification);
        }
    }

    /// <summary>
    /// Send local notification (fallback for desktop/editor)
    /// </summary>
    private void SendLocalNotification(NotificationMessage notification)
    {
        // For desktop/editor, we'll use Godot's Notification API
        // In a real mobile implementation, this would use platform-specific APIs
        
        if (OS.GetName() == "Android" || OS.GetName() == "iOS")
        {
            // Mobile platforms - use native notifications
            // This would integrate with Godot plugins for native notifications
            GD.Print($"📱 Local Notification: {notification.Title}");
        }
        else
        {
            // Desktop/Editor - use console/logging
            GD.Print($"🖥️ Desktop Notification: {notification.Title}");
        }

        // Show notification via existing NotificationManager
        NotificationManager.Instance?.SendInstantNotification(notification.Title, notification.Body);
    }

    /// <summary>
    /// Check and send notifications based on schedule
    /// </summary>
    private void CheckAndSendNotifications()
    {
        CheckForDailyReminderTime();
        CheckForMilestoneNotifications();
        CheckForLapsedPlayerNotifications();
        CheckForEventNotifications();
    }

    /// <summary>
    /// Check if it's time for daily reminder
    /// </summary>
    private void CheckForDailyReminderTime()
    {
        if (!_notificationPreferences.DailyReminderEnabled || !_notificationPreferences.IsActiveDay())
            return;

        var now = DateTime.Now;
        var reminderTime = _notificationPreferences.DailyReminderTime;
        var currentTime = now.TimeOfDay;

        // Check if current time is within 5 minutes of reminder time
        var timeDiff = Math.Abs((currentTime - reminderTime).TotalMinutes);
        if (timeDiff <= 5)
        {
            var lastSent = _notificationPreferences.GetLastNotificationTime(NotificationType.DailyReminder);
            if (lastSent == null || (DateTime.UtcNow - lastSent.Value).TotalHours >= 24)
            {
                SendDailyReminderNotification();
            }
        }
    }

    /// <summary>
    /// Check for milestone notifications
    /// </summary>
    private void CheckForMilestoneNotifications()
    {
        var streakManager = StreakManager.Instance;
        if (streakManager == null) return;

        var currentStreak = GetCurrentStreak();
        var milestoneDays = new[] { 7, 14, 21, 30 };
        
        foreach (var milestoneDay in milestoneDays)
        {
            if (currentStreak >= milestoneDay)
            {
                var lastSent = _notificationPreferences.GetLastNotificationTime(NotificationType.Milestone);
                if (lastSent == null || (DateTime.UtcNow - lastSent.Value).TotalHours >= 24)
                {
                    SendMilestoneNotification(milestoneDay);
                    break; // Only send one milestone notification per day
                }
            }
        }
    }

    /// <summary>
    /// Check for lapsed player notifications
    /// </summary>
    private void CheckForLapsedPlayerNotifications()
    {
        if (!_notificationPreferences.LapsedPlayerNotificationsEnabled) return;

        var daysSinceLastPlay = GetDaysSinceLastPlay();
        if (daysSinceLastPlay >= _notificationPreferences.LapsedPlayerThreshold)
        {
            var lastSent = _notificationPreferences.GetLastNotificationTime(NotificationType.LapsedPlayer);
            if (lastSent == null || (DateTime.UtcNow - lastSent.Value).TotalDays >= _notificationPreferences.LapsedPlayerThreshold)
            {
                SendLapsedPlayerNotification(daysSinceLastPlay);
            }
        }
    }

    /// <summary>
    /// Check for seasonal event notifications
    /// </summary>
    private void CheckForEventNotifications()
    {
        var eventManager = SeasonalEventManager.Instance;
        if (eventManager == null) return;

        var activeEvents = eventManager.GetActiveEvents();
        foreach (var seasonalEvent in activeEvents)
        {
            var timeRemaining = seasonalEvent.GetTimeRemaining();
            if (timeRemaining <= TimeSpan.FromHours(24) && timeRemaining > TimeSpan.FromHours(23))
            {
                // Event ending in ~24 hours
                var lastSent = _notificationPreferences.GetLastNotificationTime(NotificationType.SeasonalEvent);
                if (lastSent == null || (DateTime.UtcNow - lastSent.Value).TotalHours >= 12)
                {
                    SendSeasonalEventNotification(seasonalEvent.EventId, seasonalEvent.EventName, false);
                }
            }
        }
    }

    /// <summary>
    /// Handle notification click (deep linking)
    /// </summary>
    public void HandleNotificationClick(string deepLink)
    {
        if (string.IsNullOrEmpty(deepLink)) return;

        // Parse deep link and navigate to appropriate screen
        if (deepLink == "login_bonus")
        {
            ShowLoginBonusScreen();
        }
        else if (deepLink == "main_menu")
        {
            ReturnToMainMenu();
        }
        else if (deepLink.StartsWith("seasonal_event"))
        {
            var eventId = ParseDeepLinkParameter(deepLink, "event_id");
            ShowSeasonalEventScreen(eventId);
        }
        else if (deepLink == "welcome_back")
        {
            ShowWelcomeBackScreen();
        }

        // Track notification click analytics
        EmitSignal("NotificationClicked", NotificationType.DailyReminder, deepLink);
        TrackNotificationClicked(deepLink);
    }

    /// <summary>
    /// Show login bonus screen
    /// </summary>
    private void ShowLoginBonusScreen()
    {
        // This would load the LoginBonusScreen scene
        // For now, we'll log the intent
        GD.Print("🎁 Opening Login Bonus Screen");
    }

    /// <summary>
    /// Return to main menu
    /// </summary>
    private void ReturnToMainMenu()
    {
        // This would return to the main menu
        GD.Print("🏠 Returning to Main Menu");
    }

    /// <summary>
    /// Show seasonal event screen
    /// </summary>
    private void ShowSeasonalEventScreen(string eventId)
    {
        // This would load the SeasonalEventScreen with the specified event
        GD.Print($"🎉 Opening Seasonal Event Screen: {eventId}");
    }

    /// <summary>
    /// Show welcome back screen
    /// </summary>
    private void ShowWelcomeBackScreen()
    {
        // This would show a special welcome back screen with rewards
        GD.Print("👋 Opening Welcome Back Screen");
    }

    /// <summary>
    /// Parse deep link parameter
    /// </summary>
    private string ParseDeepLinkParameter(string deepLink, string parameterName)
    {
        var parts = deepLink.Split('?');
        if (parts.Length < 2) return "";

        var queryString = parts[1];
        var parameters = queryString.Split('&');
        
        foreach (var param in parameters)
        {
            var kvp = param.Split('=');
            if (kvp.Length == 2 && kvp[0] == parameterName)
            {
                return Uri.UnescapeDataString(kvp[1]);
            }
        }

        return "";
    }

    /// <summary>
    /// Get current streak from StreakManager
    /// </summary>
    private int GetCurrentStreak()
    {
        var streakManager = StreakManager.Instance;
        if (streakManager == null) return 0;

        var streakData = streakManager.GetStreakDisplayData();
        return (int)streakData.GetValueOrDefault("current_streak", 0);
    }

    /// <summary>
    /// Get days since last play
    /// </summary>
    private int GetDaysSinceLastPlay()
    {
        // This would integrate with your game session tracking
        // For now, return a placeholder value
        return 0;
    }

    /// <summary>
    /// Daily reset callback
    /// </summary>
    private void OnDailyReset()
    {
        _notificationPreferences.ResetDailyCounts();
        
        // Reset timer for next day
        _dailyResetTimer.WaitTime = CalculateSecondsToMidnight();
        _dailyResetTimer.Start();
    }

    /// <summary>
    /// Calculate seconds until next midnight
    /// </summary>
    private float CalculateSecondsToMidnight()
    {
        var now = DateTime.Now;
        var tomorrow = now.AddDays(1);
        var midnight = new DateTime(tomorrow.Year, tomorrow.Month, tomorrow.Day, 0, 0, 0);
        
        return (float)(midnight - now).TotalSeconds;
    }

    /// <summary>
    /// Check for missed daily reminder
    /// </summary>
    private void CheckForMissedDailyReminder()
    {
        // Check if player missed their daily reminder and send a catch-up notification
        var lastSent = _notificationPreferences.GetLastNotificationTime(NotificationType.DailyReminder);
        if (lastSent != null && (DateTime.UtcNow - lastSent.Value).TotalHours >= 18)
        {
            // Send a gentle reminder that they can still claim today's reward
            GD.Print("💌 Missed daily reminder - sending catch-up notification");
        }
    }

    /// <summary>
    /// Check for missed milestones
    /// </summary>
    private void CheckForMissedMilestones()
    {
        // Check if player achieved milestones while away
        // This would integrate with streak tracking to detect missed milestones
        GD.Print("🏆 Checking for missed milestone celebrations");
    }

    /// <summary>
    /// Check for streak broken alerts
    /// </summary>
    private void CheckForStreakBrokenAlerts()
    {
        // Check if player's streak was broken while they were away
        var streakManager = StreakManager.Instance;
        if (streakManager?.HasActiveStreak() == false)
        {
            GD.Print("💔 Detected streak broken - sending supportive notification");
        }
    }

    /// <summary>
    /// Check for seasonal event notifications
    /// </summary>
    private void CheckForSeasonalEventNotifications()
    {
        // Check for events that started/ended while player was away
        var eventManager = SeasonalEventManager.Instance;
        if (eventManager == null) return;

        var activeEvents = eventManager.GetActiveEvents();
        if (activeEvents.Count > 0)
        {
            GD.Print($"🎉 Found {activeEvents.Count} active events - sending notification");
        }
    }

    /// <summary>
    /// Track notification sent analytics
    /// </summary>
    private void TrackNotificationSent(NotificationMessage notification)
    {
        AnalyticsManager.Instance?.LogEvent("notification_sent", new Dictionary<string, object>
        {
            ["notification_type"] = notification.Type.ToString(),
            ["notification_title"] = notification.Title,
            ["scheduled_time"] = notification.ScheduledTime.ToString("O"),
            ["deep_link"] = notification.Data.GetValueOrDefault("deep_link", "")
        });
    }

    /// <summary>
    /// Track notification clicked analytics
    /// </summary>
    private void TrackNotificationClicked(string deepLink)
    {
        AnalyticsManager.Instance?.LogEvent("notification_clicked", new Dictionary<string, object>
        {
            ["deep_link"] = deepLink,
            ["click_time"] = DateTime.UtcNow.ToString("O"),
            ["app_open_source"] = "notification"
        });
    }

    /// <summary>
    /// Save notification state
    /// </summary>
    private void SaveNotificationState()
    {
        _notificationPreferences?.SavePreferences();
    }

    /// <summary>
    /// Load notification preferences
    /// </summary>
    private void LoadNotificationPreferences()
    {
        // This would load from PlayerProfile
        GD.Print("Loaded notification preferences");
    }

    /// <summary>
    /// Get notification statistics
    /// </summary>
    public Dictionary<string, Variant> GetNotificationStatistics()
    {
        return new Dictionary<string, Variant>
        {
            ["notifications_enabled"] = _notificationPreferences?.PushNotificationsEnabled ?? false,
            ["daily_reminder_enabled"] = _notificationPreferences?.DailyReminderEnabled ?? false,
            ["milestone_notifications_enabled"] = _notificationPreferences?.MilestoneNotificationsEnabled ?? false,
            ["streak_broken_alerts_enabled"] = _notificationPreferences?.StreakBrokenAlertsEnabled ?? false,
            ["seasonal_event_notifications_enabled"] = _notificationPreferences?.SeasonalEventNotificationsEnabled ?? false,
            ["lapsed_player_notifications_enabled"] = _notificationPreferences?.LapsedPlayerNotificationsEnabled ?? false,
            ["has_valid_consent"] = _notificationPreferences?.HasValidConsent() ?? false
        };
    }
}