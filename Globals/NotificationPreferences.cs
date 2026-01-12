using System;
using System.Collections.Generic;
using Godot;

/// <summary>
/// Notification preferences for push notification system
/// Handles opt-in/opt-out, timing preferences, and privacy compliance
/// </summary>
public partial class NotificationPreferences : Node
{
    [Signal] public delegate void PreferencesUpdatedEventHandler();
    [Signal] public delegate void NotificationOptInChangedEventHandler(bool optedIn);

    /// <summary>
    /// Whether user has opted into push notifications
    /// </summary>
    public bool PushNotificationsEnabled { get; private set; } = false;

    /// <summary>
    /// Daily reminder notification enabled
    /// </summary>
    public bool DailyReminderEnabled { get; private set; } = false;

    /// <summary>
    /// Milestone notification enabled
    /// </summary>
    public bool MilestoneNotificationsEnabled { get; private set; } = true;

    /// <summary>
    /// Streak broken alert enabled
    /// </summary>
    public bool StreakBrokenAlertsEnabled { get; private set; } = true;

    /// <summary>
    /// Seasonal event notifications enabled
    /// </summary>
    public bool SeasonalEventNotificationsEnabled { get; private set; } = true;

    /// <summary>
    /// Lapsed player notification enabled
    /// </summary>
    public bool LapsedPlayerNotificationsEnabled { get; private set; } = true;

    /// <summary>
    /// Daily reminder time (local time)
    /// </summary>
    public TimeSpan DailyReminderTime { get; private set; } = new TimeSpan(9, 0, 0); // 9 AM

    /// <summary>
    /// Quiet hours start time
    /// </summary>
    public TimeSpan QuietHoursStart { get; private set; } = new TimeSpan(22, 0, 0); // 10 PM

    /// <summary>
    /// Quiet hours end time
    /// </summary>
    public TimeSpan QuietHoursEnd { get; private set; } = new TimeSpan(8, 0, 0); // 8 AM

    /// <summary>
    /// Days of week to send daily reminders (0 = Sunday, 6 = Saturday)
    /// </summary>
    public List<int> ActiveDaysOfWeek { get; private set; } = new List<int> { 0, 1, 2, 3, 4, 5, 6 };

    /// <summary>
    /// Lapsed player threshold (days before notification)
    /// </summary>
    public int LapsedPlayerThreshold { get; private set; } = 3;

    /// <summary>
    /// Maximum notifications per day per type
    /// </summary>
    public int MaxNotificationsPerDay { get; private set; } = 1;

    /// <summary>
    /// Last notification sent for each type
    /// </summary>
    private Dictionary<NotificationType, DateTime> _lastNotificationSent = new();
    
    /// <summary>
    /// Notification count for today
    /// </summary>
    private Dictionary<NotificationType, int> _notificationCountToday = new();

    /// <summary>
    /// Consent date for GDPR compliance
    /// </summary>
    public DateTime? ConsentDate { get; private set; } = null;

    /// <summary>
    /// Consent version for tracking policy changes
    /// </summary>
    public string ConsentVersion { get; private set; } = "1.0";

    public override void _Ready()
    {
        LoadPreferences();
    }

    /// <summary>
    /// Enable push notifications with consent
    /// </summary>
    public void EnablePushNotifications(bool enable, string consentVersion = "1.0")
    {
        PushNotificationsEnabled = enable;
        
        if (enable && ConsentDate == null)
        {
            ConsentDate = DateTime.UtcNow;
            ConsentVersion = consentVersion;
        }
        
        SavePreferences();
        EmitSignal("NotificationOptInChanged", enable);
        EmitSignal("PreferencesUpdated");
        
        GD.Print($"Push notifications {(enable ? "enabled" : "disabled")} with consent version {ConsentVersion}");
    }

    /// <summary>
    /// Set daily reminder time
    /// </summary>
    public void SetDailyReminderTime(int hour, int minute)
    {
        DailyReminderTime = new TimeSpan(hour, minute, 0);
        SavePreferences();
        EmitSignal("PreferencesUpdated");
    }

    /// <summary>
    /// Set quiet hours
    /// </summary>
    public void SetQuietHours(int startHour, int endHour)
    {
        QuietHoursStart = new TimeSpan(startHour, 0, 0);
        QuietHoursEnd = new TimeSpan(endHour, 0, 0);
        SavePreferences();
        EmitSignal("PreferencesUpdated");
    }

    /// <summary>
    /// Set active days of week for notifications
    /// </summary>
    public void SetActiveDaysOfWeek(List<int> daysOfWeek)
    {
        ActiveDaysOfWeek = daysOfWeek;
        SavePreferences();
        EmitSignal("PreferencesUpdated");
    }

    /// <summary>
    /// Check if notifications are allowed at current time
    /// </summary>
    public bool IsNotificationAllowed(NotificationType notificationType)
    {
        if (!PushNotificationsEnabled) return false;
        
        // Check quiet hours
        var now = DateTime.Now.TimeOfDay;
        if (IsInQuietHours(now)) return false;
        
        // Check daily limit
        if (GetNotificationCountToday(notificationType) >= MaxNotificationsPerDay) return false;
        
        // Check type-specific preferences
        return ShouldSendNotificationType(notificationType);
    }

    /// <summary>
    /// Check if quiet hours are active
    /// </summary>
    private bool IsInQuietHours(TimeSpan currentTime)
    {
        if (QuietHoursStart < QuietHoursEnd)
        {
            // Same day (e.g., 10 PM - 8 AM)
            return currentTime >= QuietHoursStart || currentTime <= QuietHoursEnd;
        }
        else
        {
            // Overnight (e.g., 10 PM - 8 AM next day)
            return currentTime >= QuietHoursStart || currentTime <= QuietHoursEnd;
        }
    }

    /// <summary>
    /// Check if specific notification type should be sent
    /// </summary>
    private bool ShouldSendNotificationType(NotificationType notificationType)
    {
        return notificationType switch
        {
            NotificationType.DailyReminder => DailyReminderEnabled,
            NotificationType.Milestone => MilestoneNotificationsEnabled,
            NotificationType.StreakBroken => StreakBrokenAlertsEnabled,
            NotificationType.SeasonalEvent => SeasonalEventNotificationsEnabled,
            NotificationType.LapsedPlayer => LapsedPlayerNotificationsEnabled,
            NotificationType.LimitedTimeCosmetic => SeasonalEventNotificationsEnabled,
            _ => true
        };
    }

    /// <summary>
    /// Check if current day is an active day for notifications
    /// </summary>
    public bool IsActiveDay()
    {
        var today = (int)DateTime.Now.DayOfWeek;
        return ActiveDaysOfWeek.Contains(today);
    }

    /// <summary>
    /// Check if enough time has passed since last notification of this type
    /// </summary>
    public bool CanSendNotification(NotificationType notificationType, TimeSpan minimumInterval)
    {
        if (!_lastNotificationSent.ContainsKey(notificationType))
            return true;

        var lastSent = _lastNotificationSent[notificationType];
        return DateTime.UtcNow - lastSent >= minimumInterval;
    }

    /// <summary>
    /// Record that a notification was sent
    /// </summary>
    public void RecordNotificationSent(NotificationType notificationType)
    {
        _lastNotificationSent[notificationType] = DateTime.UtcNow;
        IncrementNotificationCount(notificationType);
        SavePreferences();
    }

    /// <summary>
    /// Reset daily notification counts (called at midnight)
    /// </summary>
    public void ResetDailyCounts()
    {
        _notificationCountToday.Clear();
        GD.Print("Notification counts reset for new day");
    }

    /// <summary>
    /// Get notification count for today
    /// </summary>
    public int GetNotificationCountToday(NotificationType notificationType)
    {
        return _notificationCountToday.GetValueOrDefault(notificationType, 0);
    }

    /// <summary>
    /// Increment notification count for today
    /// </summary>
    private void IncrementNotificationCount(NotificationType notificationType)
    {
        if (!_notificationCountToday.ContainsKey(notificationType))
            _notificationCountToday[notificationType] = 0;
        
        _notificationCountToday[notificationType]++;
    }

    /// <summary>
    /// Get last notification sent time for type
    /// </summary>
    public DateTime? GetLastNotificationTime(NotificationType notificationType)
    {
        return _lastNotificationSent.GetValueOrDefault(notificationType);
    }

    /// <summary>
    /// Check if user has given valid consent
    /// </summary>
    public bool HasValidConsent()
    {
        return ConsentDate.HasValue && PushNotificationsEnabled;
    }

    /// <summary>
    /// Get consent information for display
    /// </summary>
    public Dictionary<string, Variant> GetConsentInfo()
    {
        return new Dictionary<string, Variant>
        {
            ["has_consent"] = HasValidConsent(),
            ["consent_date"] = ConsentDate?.ToString("O") ?? "",
            ["consent_version"] = ConsentVersion,
            ["notifications_enabled"] = PushNotificationsEnabled,
            ["last_updated"] = DateTime.UtcNow.ToString("O")
        };
    }

    /// <summary>
    /// Serialize preferences for storage
    /// </summary>
    public string Serialize()
    {
        var data = new
        {
            push_notifications_enabled = PushNotificationsEnabled,
            daily_reminder_enabled = DailyReminderEnabled,
            milestone_notifications_enabled = MilestoneNotificationsEnabled,
            streak_broken_alerts_enabled = StreakBrokenAlertsEnabled,
            seasonal_event_notifications_enabled = SeasonalEventNotificationsEnabled,
            lapsed_player_notifications_enabled = LapsedPlayerNotificationsEnabled,
            daily_reminder_time = DailyReminderTime.ToString(),
            quiet_hours_start = QuietHoursStart.ToString(),
            quiet_hours_end = QuietHoursEnd.ToString(),
            active_days_of_week = ActiveDaysOfWeek,
            lapsed_player_threshold = LapsedPlayerThreshold,
            max_notifications_per_day = MaxNotificationsPerDay,
            consent_date = ConsentDate?.ToString("O"),
            consent_version = ConsentVersion,
            last_notification_sent = _lastNotificationSent.ToDictionary(kvp => kvp.Key.ToString(), kvp => kvp.Value.ToString("O")),
            notification_count_today = _notificationCountToday.ToDictionary(kvp => kvp.Key.ToString(), kvp => kvp.Value)
        };

        return Newtonsoft.Json.JsonConvert.SerializeObject(data, Newtonsoft.Json.Formatting.Indented);
    }

    /// <summary>
    /// Deserialize preferences from storage
    /// </summary>
    public void Deserialize(string jsonData)
    {
        try
        {
            var data = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(jsonData);
            if (data != null)
            {
                PushNotificationsEnabled = data.push_notifications_enabled ?? false;
                DailyReminderEnabled = data.daily_reminder_enabled ?? false;
                MilestoneNotificationsEnabled = data.milestone_notifications_enabled ?? true;
                StreakBrokenAlertsEnabled = data.streak_broken_alerts_enabled ?? true;
                SeasonalEventNotificationsEnabled = data.seasonal_event_notifications_enabled ?? true;
                LapsedPlayerNotificationsEnabled = data.lapsed_player_notifications_enabled ?? true;

                if (TimeSpan.TryParse(data.daily_reminder_time?.ToString(), out var reminderTime))
                    DailyReminderTime = reminderTime;

                if (TimeSpan.TryParse(data.quiet_hours_start?.ToString(), out var quietStart))
                    QuietHoursStart = quietStart;

                if (TimeSpan.TryParse(data.quiet_hours_end?.ToString(), out var quietEnd))
                    QuietHoursEnd = quietEnd;

                if (data.active_days_of_week != null)
                {
                    ActiveDaysOfWeek = new List<int>();
                    foreach (var day in data.active_days_of_week)
                    {
                        ActiveDaysOfWeek.Add((int)day);
                    }
                }

                LapsedPlayerThreshold = data.lapsed_player_threshold ?? 3;
                MaxNotificationsPerDay = data.max_notifications_per_day ?? 1;

                if (DateTime.TryParse(data.consent_date?.ToString(), out var consentDate))
                    ConsentDate = consentDate;

                ConsentVersion = data.consent_version?.ToString() ?? "1.0";

                // Load notification timing data
                if (data.last_notification_sent != null)
                {
                    _lastNotificationSent.Clear();
                    foreach (var kvp in data.last_notification_sent)
                    {
                        if (Enum.TryParse<NotificationType>(kvp.Name, out var notifType) &&
                            DateTime.TryParse(kvp.Value.ToString(), out var sentTime))
                        {
                            _lastNotificationSent[notifType] = sentTime;
                        }
                    }
                }

                if (data.notification_count_today != null)
                {
                    _notificationCountToday.Clear();
                    foreach (var kvp in data.notification_count_today)
                    {
                        if (Enum.TryParse<NotificationType>(kvp.Name, out var notifType))
                        {
                            _notificationCountToday[notifType] = (int)kvp.Value;
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            GD.PrintErr($"Failed to deserialize notification preferences: {ex.Message}");
        }
    }

    /// <summary>
    /// Save preferences to PlayerProfile
    /// </summary>
    private void SavePreferences()
    {
        // This would integrate with PlayerProfile saving
        PlayerProfile.Instance?.Save();
    }

    /// <summary>
    /// Load preferences from PlayerProfile
    /// </summary>
    private void LoadPreferences()
    {
        // This would be called after PlayerProfile loads
        // The actual loading would happen through the profile deserialization
    }

    /// <summary>
    /// Get preferences summary for UI
    /// </summary>
    public Dictionary<string, Variant> GetPreferencesSummary()
    {
        return new Dictionary<string, Variant>
        {
            ["push_notifications_enabled"] = PushNotificationsEnabled,
            ["daily_reminder_enabled"] = DailyReminderEnabled,
            ["daily_reminder_time"] = DailyReminderTime.ToString(@"hh\:mm"),
            ["quiet_hours_active"] = IsInQuietHours(DateTime.Now.TimeOfDay),
            ["milestone_notifications_enabled"] = MilestoneNotificationsEnabled,
            ["streak_broken_alerts_enabled"] = StreakBrokenAlertsEnabled,
            ["seasonal_event_notifications_enabled"] = SeasonalEventNotificationsEnabled,
            ["lapsed_player_notifications_enabled"] = LapsedPlayerNotificationsEnabled,
            ["has_valid_consent"] = HasValidConsent(),
            ["consent_date"] = ConsentDate?.ToString("MM/dd/yyyy") ?? "Never"
        };
    }
}

/// <summary>
/// Types of notifications that can be sent
/// </summary>
public enum NotificationType
{
    DailyReminder,
    Milestone,
    StreakBroken,
    SeasonalEvent,
    LimitedTimeCosmetic,
    LapsedPlayer
}

/// <summary>
/// Notification message structure
/// </summary>
public partial class NotificationMessage : GodotObject
{
    public string Title { get; set; } = "";
    public string Body { get; set; } = "";
    public string? ImageUrl { get; set; } = null;
    public Dictionary<string, string> Data { get; set; } = new();
    public NotificationType Type { get; set; }
    public DateTime ScheduledTime { get; set; } = DateTime.UtcNow;
}