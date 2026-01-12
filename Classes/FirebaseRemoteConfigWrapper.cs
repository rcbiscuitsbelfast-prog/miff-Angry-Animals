using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// Firebase Remote Config Wrapper - Handles remote configuration and feature flags
/// Provides dynamic configuration updates without app updates
/// </summary>
public class FirebaseRemoteConfigWrapper
{
    private FirebaseConfig _config;
    private bool _isMock;
    private Dictionary<string, object> _remoteConfigValues = new Dictionary<string, object>();
    private bool _isFetched = false;
    private DateTime _lastFetchTime = DateTime.MinValue;

    public FirebaseRemoteConfigWrapper(FirebaseConfig config, bool isMock = false)
    {
        _config = config;
        _isMock = isMock;
        
        if (!_isMock)
        {
            InitializeRemoteConfig();
        }
        else
        {
            InitializeMockValues();
        }
    }

    /// <summary>
    /// Initialize Firebase Remote Config
    /// </summary>
    private void InitializeRemoteConfig()
    {
        try
        {
            // Real Remote Config initialization would go here
            _isMock = true;
            GD.Print("Remote Config initialized in mock mode");
        }
        catch (Exception e)
        {
            GD.PrintErr($"Failed to initialize Remote Config: {e.Message}");
            _isMock = true;
        }
    }

    /// <summary>
    /// Initialize mock values for testing
    /// </summary>
    private void InitializeMockValues()
    {
        // Default feature flags and configuration values
        _remoteConfigValues["enable_new_levels"] = true;
        _remoteConfigValues["enable_seasonal_events"] = true;
        _remoteConfigValues["max_daily_rewards"] = 5;
        _remoteConfigValues["premium_currency_multiplier"] = 1.0;
        _remoteConfigValues["daily_challenge_count"] = 3;
        _remoteConfigValues["tutorial_completion_bonus"] = 100;
        _remoteConfigValues["event_end_date"] = DateTime.MaxValue.ToString("O");
        _remoteConfigValues["maintenance_mode"] = false;
        _remoteConfigValues["min_level_for_ads"] = 5;
        _remoteConfigValues["max_retry_attempts"] = 3;
        
        GD.Print("Remote Config mock values initialized");
    }

    /// <summary>
    /// Fetch remote config values from server
    /// </summary>
    public void Fetch(Action<bool> callback = null)
    {
        try
        {
            if (!_config.RemoteConfigEnabled)
            {
                callback?.Invoke(false);
                return;
            }

            if (_isMock)
            {
                // Simulate network delay
                CallDeferred(nameof(SimulateFetch), callback);
            }
            else
            {
                // Real Remote Config fetch would go here
                CallDeferred(nameof(SimulateFetch), callback);
            }
        }
        catch (Exception e)
        {
            GD.PrintErr($"Error fetching remote config: {e.Message}");
            callback?.Invoke(false);
        }
    }

    /// <summary>
    /// Simulate remote config fetch for testing
    /// </summary>
    private void SimulateFetch(Action<bool> callback)
    {
        try
        {
            // Simulate random network delay
            System.Threading.Thread.Sleep(100);
            
            // In a real implementation, this would fetch from Firebase
            _isFetched = true;
            _lastFetchTime = DateTime.Now;
            
            GD.Print("Remote config fetched successfully (mock)");
            callback?.Invoke(true);
        }
        catch (Exception e)
        {
            GD.PrintErr($"Error in mock fetch: {e.Message}");
            callback?.Invoke(false);
        }
    }

    /// <summary>
    /// Get remote config value
    /// </summary>
    public object GetValue(string key, object defaultValue = null)
    {
        if (_remoteConfigValues.ContainsKey(key))
        {
            return _remoteConfigValues[key];
        }
        
        return defaultValue;
    }

    /// <summary>
    /// Get string value
    /// </summary>
    public string GetString(string key, string defaultValue = "")
    {
        var value = GetValue(key, defaultValue);
        return value?.ToString() ?? defaultValue;
    }

    /// <summary>
    /// Get integer value
    /// </summary>
    public int GetInt(string key, int defaultValue = 0)
    {
        var value = GetValue(key, defaultValue);
        if (value is int intValue) return intValue;
        if (value is string stringValue && int.TryParse(stringValue, out int parsedValue)) return parsedValue;
        return defaultValue;
    }

    /// <summary>
    /// Get float value
    /// </summary>
    public float GetFloat(string key, float defaultValue = 0f)
    {
        var value = GetValue(key, defaultValue);
        if (value is float floatValue) return floatValue;
        if (value is double doubleValue) return (float)doubleValue;
        if (value is string stringValue && float.TryParse(stringValue, out float parsedValue)) return parsedValue;
        return defaultValue;
    }

    /// <summary>
    /// Get boolean value
    /// </summary>
    public bool GetBool(string key, bool defaultValue = false)
    {
        var value = GetValue(key, defaultValue);
        if (value is bool boolValue) return boolValue;
        if (value is string stringValue && bool.TryParse(stringValue, out bool parsedValue)) return parsedValue;
        return defaultValue;
    }

    /// <summary>
    /// Set remote config value (for testing)
    /// </summary>
    public void SetValue(string key, object value)
    {
        _remoteConfigValues[key] = value;
        
        // Store locally for persistence
        StoreConfigValueLocally(key, value);
    }

    /// <summary>
    /// Check if a key exists
    /// </summary>
    public bool HasKey(string key)
    {
        return _remoteConfigValues.ContainsKey(key);
    }

    /// <summary>
    /// Get all keys
    /// </summary>
    public List<string> GetAllKeys()
    {
        return new List<string>(_remoteConfigValues.Keys);
    }

    /// <summary>
    /// Check if config has been fetched
    /// </summary>
    public bool IsFetched()
    {
        return _isFetched;
    }

    /// <summary>
    /// Get last fetch time
    /// </summary>
    public DateTime GetLastFetchTime()
    {
        return _lastFetchTime;
    }

    /// <summary>
    /// Get time since last fetch
    /// </summary>
    public TimeSpan GetTimeSinceLastFetch()
    {
        return DateTime.Now - _lastFetchTime;
    }

    /// <summary>
    /// Activate fetched values
    /// </summary>
    public void Activate()
    {
        if (_isFetched)
        {
            GD.Print("Remote config values activated");
            // In real implementation, this would activate fetched values
        }
    }

    /// <summary>
    /// Store config value locally for persistence
    /// </summary>
    private void StoreConfigValueLocally(string key, object value)
    {
        try
        {
            var filePath = "user://firebase_remote_config.json";
            var config = new Dictionary<string, object>();
            
            // Load existing config
            if (FileAccess.FileExists(filePath))
            {
                using (var file = FileAccess.Open(filePath, FileAccess.ModeFlags.Read))
                {
                    string json = file.GetAsText();
                    var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var existingConfig = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(json, options);
                    if (existingConfig != null)
                    {
                        config = existingConfig;
                    }
                }
            }
            
            // Update value
            config[key] = value;
            
            // Save back to file
            using (var file = FileAccess.Open(filePath, FileAccess.ModeFlags.Write))
            {
                var options = new System.Text.Json.JsonSerializerOptions { WriteIndented = true };
                string json = System.Text.Json.JsonSerializer.Serialize(config, options);
                file.StoreString(json);
            }
        }
        catch (Exception e)
        {
            GD.PrintErr($"Error storing config value locally: {e.Message}");
        }
    }

    /// <summary>
    /// Load config values from local storage
    /// </summary>
    public void LoadFromLocal()
    {
        try
        {
            var filePath = "user://firebase_remote_config.json";
            
            if (FileAccess.FileExists(filePath))
            {
                using (var file = FileAccess.Open(filePath, FileAccess.ModeFlags.Read))
                {
                    string json = file.GetAsText();
                    var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var localConfig = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(json, options);
                    
                    if (localConfig != null)
                    {
                        foreach (var kvp in localConfig)
                        {
                            _remoteConfigValues[kvp.Key] = kvp.Value;
                        }
                        
                        GD.Print($"Loaded {localConfig.Count} config values from local storage");
                    }
                }
            }
        }
        catch (Exception e)
        {
            GD.PrintErr($"Error loading config from local storage: {e.Message}");
        }
    }

    /// <summary>
    /// Clear all remote config values
    /// </summary>
    public void Clear()
    {
        _remoteConfigValues.Clear();
        _isFetched = false;
        _lastFetchTime = DateTime.MinValue;
        
        try
        {
            var filePath = "user://firebase_remote_config.json";
            if (FileAccess.FileExists(filePath))
            {
                DirAccess.RemoveAbsolute(filePath);
            }
        }
        catch (Exception e)
        {
            GD.PrintErr($"Error clearing remote config: {e.Message}");
        }
    }

    /// <summary>
    /// Check if Remote Config is initialized
    /// </summary>
    public bool IsInitialized()
    {
        return true; // Always initialized in mock mode
    }

    /// <summary>
    /// Get all config values
    /// </summary>
    public Dictionary<string, object> GetAllValues()
    {
        return new Dictionary<string, object>(_remoteConfigValues);
    }
}