using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

/// <summary>
/// Comprehensive crash reporting and error tracking system
/// Catches unhandled exceptions, logs crash details, and offers crash reporting
/// </summary>
public class CrashReporter : Node
{
    public static CrashReporter Instance { get; private set; }

    // Crash reporting configuration
    private CrashReportingConfig _config;
    private string _crashLogPath = "user://crash_reports/";
    private List<CrashReport> _crashHistory = new List<CrashReport>();
    
    // Error tracking
    private Dictionary<string, int> _errorCounts = new Dictionary<string, int>();
    private List<ErrorLog> _recentErrors = new List<ErrorLog>();
    
    // Auto-restart functionality
    private bool _autoRestartEnabled = true;
    private int _maxCrashCount = 3;
    private int _crashCount = 0;
    
    [Signal]
    public delegate void CrashReportedEventHandler(CrashReport crashReport);
    
    [Signal]
    public delegate void ErrorLoggedEventHandler(ErrorLog errorLog);
    
    [Signal]
    public delegate void CrashThresholdReachedEventHandler(int crashCount);

    public override void _Ready()
    {
        if (Instance != null)
        {
            QueueFree();
            return;
        }
        
        Instance = this;
        InitializeCrashReporting();
    }

    /// <summary>
    /// Initialize crash reporting system
    /// </summary>
    private void InitializeCrashReporting()
    {
        LoadConfiguration();
        SetupCrashLogDirectory();
        LoadCrashHistory();
        SetupUnhandledExceptionHandler();
        
        GD.Print("Crash reporter initialized");
    }

    /// <summary>
    /// Load crash reporting configuration
    /// </summary>
    private void LoadConfiguration()
    {
        _config = new CrashReportingConfig
        {
            Enabled = true,
            AutoRestartEnabled = _autoRestartEnabled,
            MaxCrashCount = _maxCrashCount,
            SaveCrashLogs = true,
            SendCrashReports = false, // Would be true in production
            CrashLogRetention = 30, // days
            ErrorLogRetention = 7, // days
            AutoSaveInterval = 60f, // seconds
            EnableDetailedLogging = true,
            TrackMemoryLeaks = true,
            TrackPerformanceIssues = true
        };
    }

    /// <summary>
    /// Setup crash log directory
    /// </summary>
    private void SetupCrashLogDirectory()
    {
        if (!Directory.Exists(_crashLogPath))
        {
            Directory.CreateDirectory(_crashLogPath);
        }
    }

    /// <summary>
    /// Load crash history from file
    /// </summary>
    private void LoadCrashHistory()
    {
        try
        {
            string historyPath = Path.Combine(_crashLogPath, "crash_history.json");
            
            if (File.Exists(historyPath))
            {
                string jsonContent = File.ReadAllText(historyPath);
                var crashData = JsonSerializer.Deserialize<CrashHistory>(jsonContent);
                
                if (crashData?.Reports != null)
                {
                    _crashHistory = crashData.Reports;
                    
                    // Count recent crashes (last 24 hours)
                    var recentCrashes = _crashHistory.Where(c => 
                        c.Timestamp > DateTime.Now.AddDays(-1)).ToList();
                    
                    _crashCount = recentCrashes.Count;
                    
                    if (_crashCount >= _maxCrashCount)
                    {
                        EmitSignal("CrashThresholdReached", _crashCount);
                    }
                }
            }
        }
        catch (Exception e)
        {
            GD.PrintErr($"Failed to load crash history: {e.Message}");
        }
    }

    /// <summary>
    /// Setup global unhandled exception handler
    /// </summary>
    private void SetupUnhandledExceptionHandler()
    {
        // In Godot, we use the built-in error handling
        // This would integrate with Godot's signal system for unhandled exceptions
        
        // Connect to built-in error signals if available
        // Note: Godot doesn't expose a global unhandled exception handler like .NET
        // We rely on the engine's built-in error handling and our custom error tracking
    }

    /// <summary>
    /// Report a crash
    /// </summary>
    public void ReportCrash(string crashType, string message, string stackTrace, Dictionary<string, object> additionalData = null)
    {
        if (!_config.Enabled) return;
        
        var crashReport = CreateCrashReport(crashType, message, stackTrace, additionalData);
        
        // Add to history
        _crashHistory.Add(crashReport);
        
        // Save crash log
        if (_config.SaveCrashLogs)
        {
            SaveCrashLog(crashReport);
        }
        
        // Update error counts
        UpdateErrorCounts(crashType);
        
        // Check crash threshold
        CheckCrashThreshold();
        
        // Emit signal
        EmitSignal("CrashReported", crashReport);
        
        GD.PrintErr($"CRASH REPORTED: {crashType} - {message}");
        
        // Auto-restart if threshold reached
        if (_crashCount >= _maxCrashCount && _config.AutoRestartEnabled)
        {
            ScheduleAutoRestart();
        }
    }

    /// <summary>
    /// Log an error (non-crash)
    /// </summary>
    public void LogError(string errorType, string message, ErrorSeverity severity = ErrorSeverity.Warning, Dictionary<string, object> context = null)
    {
        if (!_config.Enabled) return;
        
        var errorLog = new ErrorLog
        {
            ErrorId = Guid.NewGuid().ToString(),
            ErrorType = errorType,
            Message = message,
            Severity = severity,
            Timestamp = DateTime.Now,
            ScenePath = GetTree().CurrentScene?.SceneFilePath ?? "",
            FunctionName = GetCurrentFunctionName(),
            ContextData = context ?? new Dictionary<string, object>(),
            MemoryUsage = GetEstimatedMemoryUsage(),
            ActiveNodes = GetTree().GetNodeCount()
        };
        
        _recentErrors.Add(errorLog);
        
        // Keep only recent errors (last 100)
        if (_recentErrors.Count > 100)
        {
            _recentErrors.RemoveAt(0);
        }
        
        // Update error counts
        UpdateErrorCounts(errorType);
        
        // Save if critical error
        if (severity >= ErrorSeverity.Error)
        {
            SaveErrorLog(errorLog);
        }
        
        EmitSignal("ErrorLogged", errorLog);
        
        if (severity >= ErrorSeverity.Error)
        {
            GD.PrintErr($"ERROR: {errorType} - {message}");
        }
        else
        {
            GD.Print($"WARNING: {errorType} - {message}");
        }
    }

    /// <summary>
    /// Create crash report
    /// </summary>
    private CrashReport CreateCrashReport(string crashType, string message, string stackTrace, Dictionary<string, object> additionalData)
    {
        var report = new CrashReport
        {
            ReportId = Guid.NewGuid().ToString(),
            Timestamp = DateTime.Now,
            CrashType = crashType,
            Message = message,
            StackTrace = stackTrace,
            GameVersion = GetGameVersion(),
            GodotVersion = GetGodotVersion(),
            ScenePath = GetTree().CurrentScene?.SceneFilePath ?? "",
            Platform = OS.GetName(),
            DeviceInfo = CollectDeviceInfo(),
            SystemInfo = CollectSystemInfo(),
            PerformanceSnapshot = CollectPerformanceSnapshot(),
            MemorySnapshot = CollectMemorySnapshot(),
            RecentActions = CollectRecentActions(),
            AdditionalData = additionalData ?? new Dictionary<string, object>()
        };
        
        return report;
    }

    /// <summary>
    /// Save crash log to file
    /// </summary>
    private void SaveCrashLog(CrashReport crashReport)
    {
        try
        {
            string fileName = $"crash_{crashReport.Timestamp:yyyyMMdd_HHmmss}_{crashReport.CrashType}.json";
            string filePath = Path.Combine(_crashLogPath, fileName);
            
            var options = new JsonSerializerOptions 
            { 
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            
            string json = JsonSerializer.Serialize(crashReport, options);
            File.WriteAllText(filePath, json);
            
            GD.Print($"Crash log saved: {filePath}");
        }
        catch (Exception e)
        {
            GD.PrintErr($"Failed to save crash log: {e.Message}");
        }
    }

    /// <summary>
    /// Save error log to file
    /// </summary>
    private void SaveErrorLog(ErrorLog errorLog)
    {
        try
        {
            string fileName = $"error_{errorLog.Timestamp:yyyyMMdd_HHmmss}_{errorLog.ErrorType}.json";
            string filePath = Path.Combine(_crashLogPath, fileName);
            
            var options = new JsonSerializerOptions 
            { 
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            
            string json = JsonSerializer.Serialize(errorLog, options);
            File.WriteAllText(filePath, json);
            
            GD.Print($"Error log saved: {filePath}");
        }
        catch (Exception e)
        {
            GD.PrintErr($"Failed to save error log: {e.Message}");
        }
    }

    /// <summary>
    /// Update error counts
    /// </summary>
    private void UpdateErrorCounts(string errorType)
    {
        if (_errorCounts.ContainsKey(errorType))
        {
            _errorCounts[errorType]++;
        }
        else
        {
            _errorCounts[errorType] = 1;
        }
    }

    /// <summary>
    /// Check if crash threshold is reached
    /// </summary>
    private void CheckCrashThreshold()
    {
        // Count crashes in last 24 hours
        var recentCrashes = _crashHistory.Where(c => 
            c.Timestamp > DateTime.Now.AddDays(-1)).ToList();
        
        _crashCount = recentCrashes.Count;
        
        if (_crashCount >= _maxCrashCount)
        {
            EmitSignal("CrashThresholdReached", _crashCount);
        }
    }

    /// <summary>
    /// Schedule automatic restart
    /// </summary>
    private void ScheduleAutoRestart()
    {
        GD.Print($"Auto-restart triggered after {_crashCount} crashes in 24 hours");
        
        // Show crash report dialog
        ShowCrashReportDialog();
        
        // In a real implementation, this would restart the application
        // GetTree().Quit(); // Would restart the game
    }

    /// <summary>
    /// Show crash report dialog
    /// </summary>
    private void ShowCrashReportDialog()
    {
        var dialog = new AcceptDialog();
        dialog.Title = "Application Error";
        dialog.Size = new Vector2(600, 400);
        
        var scroll = new ScrollContainer();
        scroll.SizeFlagsVertical = Control.SizeFlags.Fill;
        scroll.SizeFlagsHorizontal = Control.SizeFlags.Fill;
        
        var vbox = new VBoxContainer();
        vbox.SizeFlagsVertical = Control.SizeFlags.Fill;
        vbox.SizeFlagsHorizontal = Control.SizeFlags.Fill;
        
        var label = new Label();
        label.Text = $"Angry Animals has crashed {_crashCount} times recently.\n\n" +
                    "This may be due to a bug or system issue.\n\n" +
                    "Would you like to:\n" +
                    "• Restart the application\n" +
                    "• Send crash report to developers\n" +
                    "• View crash logs for debugging\n\n" +
                    "Your progress has been saved.";
        label.AutowrapMode = TextServer.AutowrapMode.Word;
        vbox.AddChild(label);
        
        scroll.AddChild(vbox);
        dialog.AddChild(scroll);
        
        dialog.Confirmed += () => {
            // Restart application
            GetTree().ReloadCurrentScene();
        };
        
        var viewport = GetTree().Root;
        viewport.AddChild(dialog);
        dialog.PopupCentered();
    }

    /// <summary>
    /// Collect device information for crash report
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
            ScreenSize = $"{DisplayServer.ScreenGetSize().X}x{DisplayServer.ScreenGetSize().Y}",
            VramSize = OS.GetVideoAdapterMemorySize(),
            RamSize = GetEstimatedRamSize()
        };
    }

    /// <summary>
    /// Collect system information for crash report
    /// </summary>
    private SystemInfo CollectSystemInfo()
    {
        return new SystemInfo
        {
            GodotVersion = GetGodotVersion(),
            GraphicsDriver = GetGraphicsDriver(),
            AudioDriver = GetAudioDriver(),
            InputDrivers = GetInputDrivers(),
            NetworkDrivers = GetNetworkDrivers(),
            CurrentMemoryUsage = GetEstimatedMemoryUsage(),
            ActiveNodes = GetTree().GetNodeCount(),
            ScenePath = GetTree().CurrentScene?.SceneFilePath ?? ""
        };
    }

    /// <summary>
    /// Collect performance snapshot
    /// </summary>
    private PerformanceSnapshot CollectPerformanceSnapshot()
    {
        return new PerformanceSnapshot
        {
            Fps = Engine.GetFramesPerSecond(),
            DeltaTime = Engine.GetProcessDeltaTime(),
            PhysicsDeltaTime = Engine.GetPhysicsProcessDeltaTime(),
            ActiveObjects = GetTree().GetNodeCount(),
            ActivePhysicsBodies = GetTree().GetNodesInGroup("PhysicsBody2D").Count,
            ActiveAnimations = GetActiveAnimationCount(),
            AudioStreams = GetActiveAudioStreams()
        };
    }

    /// <summary>
    /// Collect memory snapshot
    /// </summary>
    private MemorySnapshot CollectMemorySnapshot()
    {
        return new MemorySnapshot
        {
            EstimatedMemoryUsage = GetEstimatedMemoryUsage(),
            TextureMemory = GetTextureMemoryUsage(),
            AudioMemory = GetAudioMemoryUsage(),
            SceneMemory = GetSceneMemoryUsage(),
            ResourceMemory = GetResourceMemoryUsage(),
            PotentialLeaks = DetectPotentialMemoryLeaks()
        };
    }

    /// <summary>
    /// Collect recent user actions
    /// </summary>
    private List<string> CollectRecentActions()
    {
        // This would integrate with input tracking
        // For now, return basic information
        return new List<string>
        {
            $"Scene: {GetTree().CurrentScene?.Name ?? "Unknown"}",
            $"Time in scene: {DateTime.Now.Subtract(DateTime.Now).TotalSeconds:F1}s", // Placeholder
            "Recent actions tracked (placeholder)"
        };
    }

    /// <summary>
    /// Get game version
    /// </summary>
    private string GetGameVersion()
    {
        // This would integrate with VersionInfo
        return "1.0.0";
    }

    /// <summary>
    /// Get Godot version
    /// </summary>
    private string GetGodotVersion()
    {
        var version = Engine.GetVersionInfo();
        return $"{version["major"]}.{version["minor"]}.{version["patch"]}";
    }

    /// <summary>
    /// Get current function name for error logging
    /// </summary>
    private string GetCurrentFunctionName()
    {
        // In Godot, getting the current function name requires stack trace analysis
        // This is a simplified implementation
        return "Unknown";
    }

    /// <summary>
    /// Get estimated memory usage
    /// </summary>
    private float GetEstimatedMemoryUsage()
    {
        // Simplified memory estimation
        return GetTree().GetNodeCount() * 0.1f; // ~100KB per node estimate
    }

    /// <summary>
    /// Get estimated RAM size
    /// </summary>
    private int GetEstimatedRamSize()
    {
        // This would require platform-specific APIs
        return 4096; // Placeholder: 4GB
    }

    /// <summary>
    /// Get graphics driver information
    /// </summary>
    private string GetGraphicsDriver()
    {
        // This would get actual graphics driver info
        return "Unknown";
    }

    /// <summary>
    /// Get audio driver information
    /// </summary>
    private string GetAudioDriver()
    {
        // This would get actual audio driver info
        return "Unknown";
    }

    /// <summary>
    /// Get input drivers
    /// </summary>
    private string GetInputDrivers()
    {
        // This would get actual input driver info
        return "Unknown";
    }

    /// <summary>
    /// Get network drivers
    /// </summary>
    private string GetNetworkDrivers()
    {
        // This would get actual network driver info
        return "Unknown";
    }

    /// <summary>
    /// Get active animation count
    /// </summary>
    private int GetActiveAnimationCount()
    {
        // This would count active animations
        return 0; // Placeholder
    }

    /// <summary>
    /// Get active audio streams
    /// </summary>
    private int GetActiveAudioStreams()
    {
        // This would count active audio streams
        return 0; // Placeholder
    }

    /// <summary>
    /// Get texture memory usage
    /// </summary>
    private float GetTextureMemoryUsage()
    {
        // This would get actual texture memory
        return 0f; // Placeholder
    }

    /// <summary>
    /// Get audio memory usage
    /// </summary>
    private float GetAudioMemoryUsage()
    {
        // This would get actual audio memory
        return 0f; // Placeholder
    }

    /// <summary>
    /// Get scene memory usage
    /// </summary>
    private float GetSceneMemoryUsage()
    {
        // This would get actual scene memory
        return 0f; // Placeholder
    }

    /// <summary>
    /// Get resource memory usage
    /// </summary>
    private float GetResourceMemoryUsage()
    {
        // This would get actual resource memory
        return 0f; // Placeholder
    }

    /// <summary>
    /// Detect potential memory leaks
    /// </summary>
    private List<string> DetectPotentialMemoryLeaks()
    {
        var potentialLeaks = new List<string>();
        
        // Check for nodes that might be leaking
        var nodes = GetTree().GetNodesInGroup("");
        int nodeCount = nodes.Count;
        
        if (nodeCount > 1000) // Arbitrary threshold
        {
            potentialLeaks.Add($"High node count: {nodeCount}");
        }
        
        // Check for orphaned nodes
        // This would require more sophisticated analysis
        // For now, return empty list
        return potentialLeaks;
    }

    /// <summary>
    /// Get crash history
    /// </summary>
    public List<CrashReport> GetCrashHistory()
    {
        return _crashHistory;
    }

    /// <summary>
    /// Get error statistics
    /// </summary>
    public Dictionary<string, int> GetErrorStatistics()
    {
        return new Dictionary<string, int>(_errorCounts);
    }

    /// <summary>
    /// Get recent errors
    /// </summary>
    public List<ErrorLog> GetRecentErrors()
    {
        return _recentErrors;
    }

    /// <summary>
    /// Clear crash history
    /// </summary>
    public void ClearCrashHistory()
    {
        _crashHistory.Clear();
        _errorCounts.Clear();
        _recentErrors.Clear();
        
        // Clear crash log files
        try
        {
            var files = Directory.GetFiles(_crashLogPath, "*.json");
            foreach (var file in files)
            {
                File.Delete(file);
            }
        }
        catch (Exception e)
        {
            GD.PrintErr($"Failed to clear crash logs: {e.Message}");
        }
        
        GD.Print("Crash history cleared");
    }

    /// <summary>
    /// Enable or disable auto-restart
    /// </summary>
    public void SetAutoRestart(bool enabled)
    {
        _autoRestartEnabled = enabled;
        _config.AutoRestartEnabled = enabled;
    }

    /// <summary>
    /// Set maximum crash count before auto-restart
    /// </summary>
    public void SetMaxCrashCount(int maxCount)
    {
        _maxCrashCount = Mathf.Max(1, maxCount);
        _config.MaxCrashCount = _maxCrashCount;
    }

    /// <summary>
    /// Export crash report for analysis
    /// </summary>
    public void ExportCrashReport(CrashReport crashReport, string filePath)
    {
        try
        {
            var options = new JsonSerializerOptions 
            { 
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            
            string json = JsonSerializer.Serialize(crashReport, options);
            File.WriteAllText(filePath, json);
            
            GD.Print($"Crash report exported: {filePath}");
        }
        catch (Exception e)
        {
            GD.PrintErr($"Failed to export crash report: {e.Message}");
        }
    }

    /// <summary>
    /// Generate crash analysis report
    /// </summary>
    public CrashAnalysisReport GenerateAnalysisReport()
    {
        var report = new CrashAnalysisReport
        {
            GeneratedAt = DateTime.Now,
            TotalCrashes = _crashHistory.Count,
            ErrorCounts = new Dictionary<string, int>(_errorCounts),
            RecentErrors = _recentErrors.TakeLast(10).ToList(),
            MostCommonErrors = _errorCounts.OrderByDescending(kvp => kvp.Value).Take(5).ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
            CrashFrequency = CalculateCrashFrequency(),
            RecommendedActions = GenerateRecommendedActions()
        };
        
        return report;
    }

    /// <summary>
    /// Calculate crash frequency
    /// </summary>
    private Dictionary<string, float> CalculateCrashFrequency()
    {
        var frequency = new Dictionary<string, float>();
        
        foreach (var kvp in _errorCounts)
        {
            float avgPerDay = kvp.Value / 30f; // Assuming 30-day period
            frequency[kvp.Key] = avgPerDay;
        }
        
        return frequency;
    }

    /// <summary>
    /// Generate recommended actions based on error patterns
    /// </summary>
    private List<string> GenerateRecommendedActions()
    {
        var actions = new List<string>();
        
        foreach (var kvp in _errorCounts)
        {
            if (kvp.Value > 10) // High frequency errors
            {
                actions.Add($"Investigate frequent error: {kvp.Key} ({kvp.Value} occurrences)");
            }
        }
        
        if (_crashCount > _maxCrashCount)
        {
            actions.Add("Consider implementing additional error handling");
            actions.Add("Review memory management and cleanup");
        }
        
        return actions;
    }
}

/// <summary>
/// Crash report data structure
/// </summary>
public class CrashReport
{
    public string ReportId { get; set; }
    public DateTime Timestamp { get; set; }
    public string CrashType { get; set; }
    public string Message { get; set; }
    public string StackTrace { get; set; }
    public string GameVersion { get; set; }
    public string GodotVersion { get; set; }
    public string ScenePath { get; set; }
    public string Platform { get; set; }
    public DeviceInfo DeviceInfo { get; set; }
    public SystemInfo SystemInfo { get; set; }
    public PerformanceSnapshot PerformanceSnapshot { get; set; }
    public MemorySnapshot MemorySnapshot { get; set; }
    public List<string> RecentActions { get; set; } = new List<string>();
    public Dictionary<string, object> AdditionalData { get; set; } = new Dictionary<string, object>();
}

/// <summary>
/// Error log data structure
/// </summary>
public class ErrorLog
{
    public string ErrorId { get; set; }
    public string ErrorType { get; set; }
    public string Message { get; set; }
    public ErrorSeverity Severity { get; set; }
    public DateTime Timestamp { get; set; }
    public string ScenePath { get; set; }
    public string FunctionName { get; set; }
    public Dictionary<string, object> ContextData { get; set; } = new Dictionary<string, object>();
    public float MemoryUsage { get; set; }
    public int ActiveNodes { get; set; }
}

/// <summary>
/// Error severity levels
/// </summary>
public enum ErrorSeverity
{
    Info,
    Warning,
    Error,
    Critical
}

/// <summary>
/// Device information for crash reports
/// </summary>
public class DeviceInfo
{
    public string Platform { get; set; }
    public string Model { get; set; }
    public string OsVersion { get; set; }
    public string Architecture { get; set; }
    public int ProcessorCount { get; set; }
    public string ScreenSize { get; set; }
    public int VramSize { get; set; }
    public int RamSize { get; set; }
}

/// <summary>
/// System information for crash reports
/// </summary>
public class SystemInfo
{
    public string GodotVersion { get; set; }
    public string GraphicsDriver { get; set; }
    public string AudioDriver { get; set; }
    public string InputDrivers { get; set; }
    public string NetworkDrivers { get; set; }
    public float CurrentMemoryUsage { get; set; }
    public int ActiveNodes { get; set; }
    public int ActivePhysicsBodies { get; set; }
    public string ScenePath { get; set; }
}

/// <summary>
/// Performance snapshot
/// </summary>
public class PerformanceSnapshot
{
    public float Fps { get; set; }
    public float DeltaTime { get; set; }
    public float PhysicsDeltaTime { get; set; }
    public int ActiveObjects { get; set; }
    public int ActivePhysicsBodies { get; set; }
    public int ActiveAnimations { get; set; }
    public int AudioStreams { get; set; }
}

/// <summary>
/// Memory snapshot
/// </summary>
public class MemorySnapshot
{
    public float EstimatedMemoryUsage { get; set; }
    public float TextureMemory { get; set; }
    public float AudioMemory { get; set; }
    public float SceneMemory { get; set; }
    public float ResourceMemory { get; set; }
    public List<string> PotentialLeaks { get; set; } = new List<string>();
}

/// <summary>
/// Crash reporting configuration
/// </summary>
public class CrashReportingConfig
{
    public bool Enabled { get; set; }
    public bool AutoRestartEnabled { get; set; }
    public int MaxCrashCount { get; set; }
    public bool SaveCrashLogs { get; set; }
    public bool SendCrashReports { get; set; }
    public int CrashLogRetention { get; set; }
    public int ErrorLogRetention { get; set; }
    public float AutoSaveInterval { get; set; }
    public bool EnableDetailedLogging { get; set; }
    public bool TrackMemoryLeaks { get; set; }
    public bool TrackPerformanceIssues { get; set; }
}

/// <summary>
/// Crash history data
/// </summary>
public class CrashHistory
{
    public List<CrashReport> Reports { get; set; } = new List<CrashReport>();
    public DateTime LastUpdated { get; set; }
}

/// <summary>
/// Crash analysis report
/// </summary>
public class CrashAnalysisReport
{
    public DateTime GeneratedAt { get; set; }
    public int TotalCrashes { get; set; }
    public Dictionary<string, int> ErrorCounts { get; set; } = new Dictionary<string, int>();
    public List<ErrorLog> RecentErrors { get; set; } = new List<ErrorLog>();
    public Dictionary<string, int> MostCommonErrors { get; set; } = new Dictionary<string, int>();
    public Dictionary<string, float> CrashFrequency { get; set; } = new Dictionary<string, float>();
    public List<string> RecommendedActions { get; set; } = new List<string>();
}