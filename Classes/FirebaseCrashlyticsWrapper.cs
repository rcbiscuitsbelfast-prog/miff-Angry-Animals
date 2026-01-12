using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Firebase Crashlytics Wrapper - Handles crash reporting and error tracking
/// Provides automatic crash reporting and custom error logging
/// </summary>
public class FirebaseCrashlyticsWrapper
{
    private FirebaseConfig _config;
    private bool _isMock;
    private List<CrashReport> _crashReports = new List<CrashReport>();

    public FirebaseCrashlyticsWrapper(FirebaseConfig config, bool isMock = false)
    {
        _config = config;
        _isMock = isMock;
        
        if (!_isMock)
        {
            InitializeCrashlytics();
        }
    }

    /// <summary>
    /// Initialize Firebase Crashlytics
    /// </summary>
    private void InitializeCrashlytics()
    {
        try
        {
            // Real Crashlytics initialization would go here
            // For now, use mock mode
            _isMock = true;
            GD.Print("Crashlytics initialized in mock mode");
        }
        catch (Exception e)
        {
            GD.PrintErr($"Failed to initialize Crashlytics: {e.Message}");
            _isMock = true;
        }
    }

    /// <summary>
    /// Record a non-fatal exception
    /// </summary>
    public void RecordException(string type, string message, Dictionary<string, object> additionalData = null)
    {
        try
        {
            if (!_config.CrashlyticsEnabled || !_config.UserConsent)
            {
                return;
            }

            var crashReport = new CrashReport
            {
                Type = type,
                Message = message,
                Timestamp = DateTime.Now,
                Platform = GetPlatformName(),
                AdditionalData = additionalData ?? new Dictionary<string, object>(),
                IsFatal = false
            };

            if (_isMock)
            {
                MockRecordException(crashReport);
            }
            else
            {
                // Real Crashlytics implementation would go here
                MockRecordException(crashReport);
            }
        }
        catch (Exception e)
        {
            GD.PrintErr($"Error recording exception to Crashlytics: {e.Message}");
        }
    }

    /// <summary>
    /// Record a fatal crash
    /// </summary>
    public void RecordCrash(string type, string message, Dictionary<string, object> additionalData = null)
    {
        try
        {
            var crashReport = new CrashReport
            {
                Type = type,
                Message = message,
                Timestamp = DateTime.Now,
                Platform = GetPlatformName(),
                AdditionalData = additionalData ?? new Dictionary<string, object>(),
                IsFatal = true
            };

            if (_isMock)
            {
                MockRecordCrash(crashReport);
            }
            else
            {
                // Real Crashlytics implementation would go here
                MockRecordCrash(crashReport);
            }
        }
        catch (Exception e)
        {
            GD.PrintErr($"Error recording crash to Crashlytics: {e.Message}");
        }
    }

    /// <summary>
    /// Set custom key-value pair
    /// </summary>
    public void SetCustomKey(string key, string value)
    {
        try
        {
            if (_isMock)
            {
                MockSetCustomKey(key, value);
            }
            else
            {
                // Real Crashlytics implementation would go here
                MockSetCustomKey(key, value);
            }
        }
        catch (Exception e)
        {
            GD.PrintErr($"Error setting custom key '{key}': {e.Message}");
        }
    }

    /// <summary>
    /// Set user identifier
    /// </summary>
    public void SetUserId(string userId)
    {
        try
        {
            if (_isMock)
            {
                MockSetUserId(userId);
            }
            else
            {
                // Real Crashlytics implementation would go here
                MockSetUserId(userId);
            }
        }
        catch (Exception e)
        {
            GD.PrintErr($"Error setting Crashlytics user ID: {e.Message}");
        }
    }

    /// <summary>
    /// Mock implementation for non-fatal exceptions
    /// </summary>
    private void MockRecordException(CrashReport report)
    {
        GD.PrintErr($"[Crashlytics Mock] Non-Fatal Exception: {report.Type} - {report.Message}");
        
        if (report.AdditionalData.Count > 0)
        {
            GD.PrintErr($"  Additional Data: {string.Join(", ", report.AdditionalData.Select(kvp => $"{kvp.Key}={kvp.Value}"))}");
        }
        
        _crashReports.Add(report);
        StoreCrashReportLocally(report);
    }

    /// <summary>
    /// Mock implementation for crashes
    /// </summary>
    private void MockRecordCrash(CrashReport report)
    {
        GD.PrintErr($"[Crashlytics Mock] CRASH: {report.Type} - {report.Message}");
        
        if (report.AdditionalData.Count > 0)
        {
            GD.PrintErr($"  Additional Data: {string.Join(", ", report.AdditionalData.Select(kvp => $"{kvp.Key}={kvp.Value}"))}");
        }
        
        _crashReports.Add(report);
        StoreCrashReportLocally(report);
    }

    /// <summary>
    /// Mock implementation for custom keys
    /// </summary>
    private void MockSetCustomKey(string key, string value)
    {
        GD.Print($"[Crashlytics Mock] Custom Key: {key} = {value}");
    }

    /// <summary>
    /// Mock implementation for user ID
    /// </summary>
    private void MockSetUserId(string userId)
    {
        GD.Print($"[Crashlytics Mock] User ID: {userId}");
    }

    /// <summary>
    /// Store crash report locally
    /// </summary>
    private void StoreCrashReportLocally(CrashReport report)
    {
        try
        {
            var filePath = "user://firebase_crash_reports.json";
            var reports = new List<CrashReport>();
            
            // Load existing reports
            if (FileAccess.FileExists(filePath))
            {
                using (var file = FileAccess.Open(filePath, FileAccess.ModeFlags.Read))
                {
                    string json = file.GetAsText();
                    var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var existingReports = System.Text.Json.JsonSerializer.Deserialize<List<CrashReport>>(json, options);
                    if (existingReports != null)
                    {
                        reports = existingReports;
                    }
                }
            }
            
            // Add new report
            reports.Add(report);
            
            // Save back to file (keep only last 50 reports)
            if (reports.Count > 50)
            {
                reports = reports.TakeLast(50).ToList();
            }
            
            using (var file = FileAccess.Open(filePath, FileAccess.ModeFlags.Write))
            {
                var options = new System.Text.Json.JsonSerializerOptions { WriteIndented = true };
                string json = System.Text.Json.JsonSerializer.Serialize(reports, options);
                file.StoreString(json);
            }
        }
        catch (Exception e)
        {
            GD.PrintErr($"Error storing crash report locally: {e.Message}");
        }
    }

    /// <summary>
    /// Get platform name
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
        else
        {
            return platform;
        }
    }

    /// <summary>
    /// Get crash reports for testing
    /// </summary>
    public List<CrashReport> GetCrashReports()
    {
        return new List<CrashReport>(_crashReports);
    }

    /// <summary>
    /// Clear crash reports
    /// </summary>
    public void ClearCrashReports()
    {
        _crashReports.Clear();
        
        try
        {
            var filePath = "user://firebase_crash_reports.json";
            if (FileAccess.FileExists(filePath))
            {
                DirAccess.RemoveAbsolute(filePath);
            }
        }
        catch (Exception e)
        {
            GD.PrintErr($"Error clearing crash reports: {e.Message}");
        }
    }

    /// <summary>
    /// Check if Crashlytics is initialized
    /// </summary>
    public bool IsInitialized()
    {
        return !_isMock;
    }
}

/// <summary>
/// Crash report data structure
/// </summary>
public class CrashReport
{
    public string Type { get; set; }
    public string Message { get; set; }
    public DateTime Timestamp { get; set; }
    public string Platform { get; set; }
    public Dictionary<string, object> AdditionalData { get; set; } = new Dictionary<string, object>();
    public bool IsFatal { get; set; }
}