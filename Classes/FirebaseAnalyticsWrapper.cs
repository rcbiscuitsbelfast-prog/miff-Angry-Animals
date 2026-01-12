using Godot;
using System;
using System.Collections.Generic;
using System.Text.Json;

/// <summary>
/// Firebase Analytics Wrapper - Handles all Firebase Analytics operations
/// Provides a unified interface for logging events, setting user properties, and managing analytics
/// </summary>
public class FirebaseAnalyticsWrapper
{
    private FirebaseConfig _config;
    private bool _isMock;
    private Dictionary<string, string> _userProperties = new Dictionary<string, string>();
    private string _userId = null;

    public FirebaseAnalyticsWrapper(FirebaseConfig config, bool isMock = false)
    {
        _config = config;
        _isMock = isMock;
        
        if (!_isMock)
        {
            InitializeFirebaseAnalytics();
        }
    }

    /// <summary>
    /// Initialize Firebase Analytics
    /// </summary>
    private void InitializeFirebaseAnalytics()
    {
        try
        {
            #if UNITY_ANDROID && !UNITY_EDITOR
            // Android-specific initialization would happen here
            GD.Print("Firebase Analytics initialized for Android");
            #elif UNITY_IOS && !UNITY_EDITOR
            // iOS-specific initialization would happen here
            GD.Print("Firebase Analytics initialized for iOS");
            #else
            // Editor or unsupported platform
            GD.Print("Firebase Analytics not available on this platform - using mock mode");
            _isMock = true;
            #endif
        }
        catch (Exception e)
        {
            GD.PrintErr($"Failed to initialize Firebase Analytics: {e.Message}");
            _isMock = true;
        }
    }

    /// <summary>
    /// Log an analytics event
    /// </summary>
    public void LogEvent(string eventName, Dictionary<string, object> parameters = null)
    {
        try
        {
            if (!_config.AnalyticsEnabled || !_config.UserConsent)
            {
                return;
            }

            // Add common parameters
            var eventParams = new Dictionary<string, object>();
            
            // Add user ID if available
            if (!string.IsNullOrEmpty(_userId))
            {
                eventParams["user_id"] = _userId;
            }
            
            // Add platform info
            eventParams["platform"] = GetPlatformName();
            eventParams["timestamp"] = DateTime.Now.ToString("O");
            
            // Add custom parameters
            if (parameters != null)
            {
                foreach (var kvp in parameters)
                {
                    eventParams[kvp.Key] = kvp.Value;
                }
            }

            if (_isMock)
            {
                // Mock implementation - log to console and store locally
                MockLogEvent(eventName, eventParams);
            }
            else
            {
                // Real Firebase implementation would go here
                // For Godot, this would use GodotFirebase or similar plugins
                MockLogEvent(eventName, eventParams);
            }
        }
        catch (Exception e)
        {
            GD.PrintErr($"Error logging Firebase Analytics event '{eventName}': {e.Message}");
        }
    }

    /// <summary>
    /// Mock implementation for testing/editor
    /// </summary>
    private void MockLogEvent(string eventName, Dictionary<string, object> parameters)
    {
        GD.Print($"[Firebase Analytics Mock] Event: {eventName}");
        
        if (parameters != null && parameters.Count > 0)
        {
            GD.Print($"  Parameters: {string.Join(", ", parameters.Select(kvp => $"{kvp.Key}={kvp.Value}"))}");
        }
        
        // Store locally for testing
        StoreEventLocally(eventName, parameters);
    }

    /// <summary>
    /// Store event locally for testing
    /// </summary>
    private void StoreEventLocally(string eventName, Dictionary<string, object> parameters)
    {
        try
        {
            var filePath = "user://firebase_analytics_events.json";
            var events = new List<StoredEvent>();
            
            // Load existing events
            if (System.IO.File.Exists(filePath))
            {
                string json = System.IO.File.ReadAllText(filePath);
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                events = JsonSerializer.Deserialize<List<StoredEvent>>(json, options) ?? new List<StoredEvent>();
            }
            
            // Add new event
            events.Add(new StoredEvent
            {
                EventName = eventName,
                Parameters = parameters,
                Timestamp = DateTime.Now.ToString("O")
            });
            
            // Save back to file
            var options = new JsonSerializerOptions { WriteIndented = true };
            System.IO.File.WriteAllText(filePath, JsonSerializer.Serialize(events, options));
        }
        catch (Exception e)
        {
            GD.PrintErr($"Error storing event locally: {e.Message}");
        }
    }

    /// <summary>
    /// Set user property
    /// </summary>
    public void SetUserProperty(string propertyName, string value)
    {
        try
        {
            _userProperties[propertyName] = value;
            
            if (_isMock)
            {
                MockSetUserProperty(propertyName, value);
            }
            else
            {
                // Real Firebase implementation would go here
                MockSetUserProperty(propertyName, value);
            }
        }
        catch (Exception e)
        {
            GD.PrintErr($"Error setting user property '{propertyName}': {e.Message}");
        }
    }

    /// <summary>
    /// Mock implementation for user properties
    /// </summary>
    private void MockSetUserProperty(string propertyName, string value)
    {
        GD.Print($"[Firebase Analytics Mock] User Property: {propertyName} = {value}");
        
        // Store locally
        var prefs = new ConfigFile();
        prefs.SetValue("firebase_user_properties", propertyName, value);
        prefs.Save("user://firebase_properties.cfg");
    }

    /// <summary>
    /// Set user ID
    /// </summary>
    public void SetUserId(string userId)
    {
        try
        {
            _userId = userId;
            
            if (_isMock)
            {
                MockSetUserId(userId);
            }
            else
            {
                // Real Firebase implementation would go here
                MockSetUserId(userId);
            }
        }
        catch (Exception e)
        {
            GD.PrintErr($"Error setting user ID: {e.Message}");
        }
    }

    /// <summary>
    /// Mock implementation for user ID
    /// </summary>
    private void MockSetUserId(string userId)
    {
        GD.Print($"[Firebase Analytics Mock] User ID: {userId}");
        
        // Store locally
        var prefs = new ConfigFile();
        prefs.SetValue("firebase_user_properties", "user_id", userId);
        prefs.Save("user://firebase_properties.cfg");
    }

    /// <summary>
    /// Check if Firebase Analytics is initialized
    /// </summary>
    public bool IsInitialized()
    {
        return !_isMock; // For now, return true if not in mock mode
    }

    /// <summary>
    /// Get platform name for analytics
    /// </summary>
    private string GetPlatformName()
    {
        string platform = OS.GetName();
        
        if (EngineEditorInterface.IsEditorHint())
        {
            return "Editor";
        }
        else if (platform == "Android")
        {
            return "Android";
        }
        else if (platform == "iOS")
        {
            return "iOS";
        }
        else if (platform == "Windows")
        {
            return "Windows";
        }
        else if (platform == "macOS")
        {
            return "macOS";
        }
        else if (platform == "Linux")
        {
            return "Linux";
        }
        else
        {
            return "Unknown";
        }
    }

    /// <summary>
    /// Get stored events for testing
    /// </summary>
    public List<StoredEvent> GetStoredEvents()
    {
        try
        {
            var filePath = "user://firebase_analytics_events.json";
            
            if (FileAccess.FileExists(filePath))
            {
                using (var file = FileAccess.Open(filePath, FileAccess.ModeFlags.Read))
                {
                    string json = file.GetAsText();
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    return JsonSerializer.Deserialize<List<StoredEvent>>(json, options) ?? new List<StoredEvent>();
                }
            }
        }
        catch (Exception e)
        {
            GD.PrintErr($"Error loading stored events: {e.Message}");
        }
        
        return new List<StoredEvent>();
    }

    /// <summary>
    /// Clear stored events
    /// </summary>
    public void ClearStoredEvents()
    {
        try
        {
            var filePath = "user://firebase_analytics_events.json";
            if (FileAccess.FileExists(filePath))
            {
                DirAccess.RemoveAbsolute(filePath);
            }
        }
        catch (Exception e)
        {
            GD.PrintErr($"Error clearing stored events: {e.Message}");
        }
    }
}

/// <summary>
/// Stored event for local testing
/// </summary>
public class StoredEvent
{
    public string EventName { get; set; }
    public Dictionary<string, object> Parameters { get; set; } = new Dictionary<string, object>();
    public string Timestamp { get; set; }
}