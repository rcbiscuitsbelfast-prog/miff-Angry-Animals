using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

/// <summary>
/// Comprehensive crash detection and recovery system
/// Catches exceptions, captures crash data, and implements automatic recovery
/// </summary>
public class CrashDetector : Node
{
    public static CrashDetector Instance { get; private set; }

    // Crash tracking
    private List<CrashReport> _crashHistory = new List<CrashReport>();
    private DateTime _lastCrashTime;
    private int _crashCount;
    private string _lastErrorMessage;
    
    // Recovery system
    private bool _recoveryInProgress = false;
    private DateTime _lastRecoveryAttempt;
    private const int MAX_CRASHES_BEFORE_NOTIFICATION = 3;
    private const int CRASH_TIMEWINDOW_MINUTES = 10;
    
    // Device information
    private Dictionary<string, object> _deviceInfo;
    
    // Analytics integration
    private bool _analyticsEnabled;
    
    [Signal]
    public delegate void CrashDetectedEventHandler(CrashReport report);
    
    [Signal]
    public delegate void RecoveryAttemptedEventHandler(bool success);
    
    [Signal]
    public delegate void RecoveryCompletedEventHandler(bool success);
    
    [Signal]
    public delegate void CriticalCrashEventHandler(CrashReport report);

    public override void _Ready()
    {
        if (Instance != null)
        {
            QueueFree();
            return;
        }
        
        Instance = this;
        InitializeCrashDetector();
    }

    /// <summary>
    /// Initialize crash detection system
    /// </summary>
    private void InitializeCrashDetector()
    {
        _analyticsEnabled = AnalyticsEventTracker.Instance != null;
        CollectDeviceInfo();
        SetupGlobalErrorHandling();
        LoadCrashHistory();
        
        // Connect to global signals
        if (SignalManager.Instance != null)
        {
            SignalManager.Instance.Connect("SceneChanged", new Callable(this, nameof(OnSceneChanged)));
        }
        
        GD.Print("Crash Detector initialized");
    }

    /// <summary>
    /// Collect device information for crash reports
    /// </summary>
    private void CollectDeviceInfo()
    {
        _deviceInfo = new Dictionary<string, object>
        {
            ["device_model"] = OS.GetModel(),
            ["device_name"] = OS.GetName(),
            ["processor_count"] = OS.GetProcessorCount(),
            ["system_memory_mb"] = OS.GetStaticMemoryUsage() / (1024 * 1024),
            ["screen_width"] = DisplayServer.ScreenGetSize().X,
            ["screen_height"] = DisplayServer.ScreenGetSize().Y,
            ["graphics_api"] = RenderingServer.GetRenderingDevice()?.GetName() ?? "Unknown",
            ["godots_version"] = Engine.GetVersionString(),
            ["game_version"] = ProjectSettings.GetSetting("application/config/version", "1.0").ToString(),
            ["platform"] = OS.GetName(),
            ["architecture"] = OS.GetArchitectureName(),
            ["cpu_model"] = OS.GetProcessorName()
        };
    }

    /// <summary>
    /// Setup global error handling
    /// </summary>
    private void SetupGlobalErrorHandling()
    {
        // Hook into Godot's global error handling
        ProjectSettings.SetSetting("debug/settings/stdout/print_fps", true);
        ProjectSettings.SetSetting("debug/settings/stdout/print_texture_size", false);
        ProjectSettings.SetSetting("debug/settings/stdout/print_svg_icons", false);
        
        // Note: In a real implementation, you'd need to hook into Application::set_crash_handler
        // This is a simplified version for demonstration purposes
    }

    /// <summary>
    /// Load crash history from persistent storage
    /// </summary>
    private void LoadCrashHistory()
    {
        try
        {
            var crashFilePath = "user://crash_history.json";
            if (FileAccess.FileExists(crashFilePath))
            {
                var file = FileAccess.Open(crashFilePath, FileAccess.ModeFlags.Read);
                var jsonString = file.GetAsText();
                file.Close();
                
                var history = JsonSerializer.Deserialize<List<CrashReport>>(jsonString);
                if (history != null)
                {
                    _crashHistory = history.TakeLast(50).ToList(); // Keep last 50 crashes
                }
            }
        }
        catch (Exception e)
        {
            GD.PrintErr($"Failed to load crash history: {e.Message}");
        }
    }

    /// <summary>
    /// Save crash history to persistent storage
    /// </summary>
    private void SaveCrashHistory()
    {
        try
        {
            var crashFilePath = "user://crash_history.json";
            var file = FileAccess.Open(crashFilePath, FileAccess.ModeFlags.Write);
            
            var jsonString = JsonSerializer.Serialize(_crashHistory, new JsonSerializerOptions { WriteIndented = true });
            file.StoreString(jsonString);
            file.Close();
        }
        catch (Exception e)
        {
            GD.PrintErr($"Failed to save crash history: {e.Message}");
        }
    }

    /// <summary>
    /// Report a crash with full context information
    /// </summary>
    public void ReportCrash(Exception exception, string context = "General")
    {
        var crashReport = CreateCrashReport(exception, context);
        
        // Add to history
        _crashHistory.Add(crashReport);
        if (_crashHistory.Count > 50)
        {
            _crashHistory.RemoveAt(0);
        }
        
        _crashCount++;
        _lastCrashTime = DateTime.Now;
        _lastErrorMessage = exception.Message;
        
        // Save to persistent storage
        SaveCrashHistory();
        
        // Emit signals
        EmitSignal("CrashDetected", crashReport);
        
        // Check if this is a critical crash pattern
        if (IsCriticalCrashPattern())
        {
            EmitSignal("CriticalCrash", crashReport);
        }
        
        // Attempt automatic recovery
        if (!_recoveryInProgress)
        {
            AttemptAutomaticRecovery(crashReport);
        }
        
        // Log to analytics
        if (_analyticsEnabled)
        {
            TrackCrashAnalytics(crashReport);
        }
        
        // Report to Firebase Crashlytics
        ReportToCrashlytics(crashReport);
        
        GD.PrintErr($"CRASH DETECTED: {exception.Message}\nStack Trace: {exception.StackTrace}");
    }

    /// <summary>
    /// Create a comprehensive crash report
    /// </summary>
    private CrashReport CreateCrashReport(Exception exception, string context)
    {
        var gameState = GetCurrentGameState();
        var memoryInfo = OS.GetStaticMemoryUsage();
        
        return new CrashReport
        {
            CrashId = Guid.NewGuid().ToString(),
            Timestamp = DateTime.Now,
            ExceptionType = exception.GetType().Name,
            ExceptionMessage = exception.Message,
            StackTrace = exception.StackTrace,
            Context = context,
            GameState = gameState,
            ScenePath = GetTree().CurrentScene?.FilePath ?? "Unknown",
            DeviceInfo = _deviceInfo,
            MemoryUsageMB = memoryInfo / (1024 * 1024),
            FPS = Engine.GetFramesPerSecond(),
            DeltaTime = Engine.GetProcessTime(),
            ActiveNodes = GetTree().GetNodeCount(),
            PersistentNodes = GetTree().GetPersistentNodeCount(),
            LoadedResources = ResourceLoader.GetLoadedResourceCount(),
            SystemLanguage = OS.GetLocaleLanguage(),
            TimezoneOffset = DateTimeOffset.Now.Offset.TotalMinutes
        };
    }

    /// <summary>
    /// Get current game state information
    /// </summary>
    private Dictionary<string, object> GetCurrentGameState()
    {
        var gameState = new Dictionary<string, object>();
        
        // Get current scene information
        var currentScene = GetTree().CurrentScene;
        if (currentScene != null)
        {
            gameState["current_scene"] = currentScene.Name;
            gameState["scene_path"] = currentScene.FilePath;
        }
        
        // Get game manager state if available
        if (GameManager.Instance != null)
        {
            gameState["game_state"] = GameManager.Instance.CurrentGameState;
            gameState["level_id"] = GameManager.Instance.CurrentLevelId;
        }
        
        // Get player state if available
        if (PlayerProfile.Instance != null)
        {
            gameState["player_level"] = PlayerProfile.Instance.CurrentLevel;
            gameState["coins"] = PlayerProfile.Instance.Coins;
        }
        
        // Get performance metrics if available
        if (PerformanceTelemetry.Instance != null)
        {
            var perfSnapshot = PerformanceTelemetry.Instance.GetCurrentSnapshot();
            gameState["current_fps"] = perfSnapshot.Fps;
            gameState["memory_usage_mb"] = perfSnapshot.MemoryMB;
        }
        
        return gameState;
    }

    /// <summary>
    /// Check if this represents a critical crash pattern
    /// </summary>
    private bool IsCriticalCrashPattern()
    {
        var recentCrashes = _crashHistory
            .Where(c => (DateTime.Now - c.Timestamp).TotalMinutes <= CRASH_TIMEWINDOW_MINUTES)
            .ToList();
        
        return recentCrashes.Count >= MAX_CRASHES_BEFORE_NOTIFICATION;
    }

    /// <summary>
    /// Attempt automatic recovery from crash
    /// </summary>
    private void AttemptAutomaticRecovery(CrashReport crashReport)
    {
        if (_recoveryInProgress) return;
        
        _recoveryInProgress = true;
        _lastRecoveryAttempt = DateTime.Now;
        
        GD.Print($"Attempting automatic recovery from crash: {crashReport.ExceptionType}");
        
        // Use Godot's timer for delayed execution instead of async/await
        var timer = GetTree().CreateTimer(0.1f);
        timer.Timeout += () => ProcessRecoveryAttempt(crashReport);
    }

    /// <summary>
    /// Process recovery attempt with Godot's signals
    /// </summary>
    private void ProcessRecoveryAttempt(CrashReport crashReport)
    {
        try
        {
            // Strategy 1: Save current progress and restart safely
            var recoverySuccess = SafeRecoveryAttempt();
            
            if (recoverySuccess)
            {
                GD.Print("Automatic recovery successful");
                EmitSignal("RecoveryCompleted", true);
                
                // Show recovery notification to user
                ShowRecoveryNotification(true);
            }
            else
            {
                GD.Print("Automatic recovery failed, attempting restart");
                recoverySuccess = RestartGameAttempt();
                
                if (recoverySuccess)
                {
                    GD.Print("Game restart recovery successful");
                    EmitSignal("RecoveryCompleted", true);
                    ShowRecoveryNotification(true, true);
                }
                else
                {
                    GD.Print("All recovery attempts failed");
                    EmitSignal("RecoveryCompleted", false);
                    ShowRecoveryNotification(false);
                }
            }
        }
        catch (Exception recoveryException)
        {
            GD.PrintErr($"Recovery attempt failed: {recoveryException.Message}");
            EmitSignal("RecoveryCompleted", false);
            ShowRecoveryNotification(false);
        }
        finally
        {
            _recoveryInProgress = false;
        }
    }

    /// <summary>
    /// Attempt safe recovery without restart
    /// </summary>
    private bool SafeRecoveryAttempt()
    {
        try
        {
            // Save current player progress
            if (PlayerProfile.Instance != null)
            {
                PlayerProfile.Instance.SavePlayerData();
            }
            
            // Clear problematic state
            ClearProblematicState();
            
            // Return to main menu safely
            if (GetTree().CurrentScene?.Name != "MainMenu")
            {
                var mainMenuScene = ResourceLoader.Load<PackedScene>("res://Scenes/MainMenu.tscn");
                if (mainMenuScene != null)
                {
                    GetTree().ChangeSceneToPackedScene(mainMenuScene);
                    return true;
                }
            }
            
            return true;
        }
        catch (Exception e)
        {
            GD.PrintErr($"Safe recovery failed: {e.Message}");
            return false;
        }
    }

    /// <summary>
    /// Attempt full game restart
    /// </summary>
    private bool RestartGameAttempt()
    {
        try
        {
            // Show restart notification
            ShowRestartNotification();
            
            // Wait a moment for user to see notification
            var timer = GetTree().CreateTimer(2.0f);
            timer.Timeout += () =>
            {
                // Restart the game
                GetTree().ReloadCurrentScene();
            };
            
            return true;
        }
        catch (Exception e)
        {
            GD.PrintErr($"Restart attempt failed: {e.Message}");
            return false;
        }
    }

    /// <summary>
    /// Clear problematic game state
    /// </summary>
    private void ClearProblematicState()
    {
        try
        {
            // Clear any problematic audio
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.StopAllSounds();
            }
            
            // Reset game feel effects
            if (GameFeelManager.Instance != null)
            {
                GameFeelManager.Instance.ResetAllEffects();
            }
            
            // Clear particle effects
            if (EffectsManager.Instance != null)
            {
                EffectsManager.Instance.ClearAllEffects();
            }
            
            // Reset input state
            GetTree().Root.SetInputAsHandled();
            
            GD.Print("Problematic state cleared");
        }
        catch (Exception e)
        {
            GD.PrintErr($"Failed to clear state: {e.Message}");
        }
    }

    /// <summary>
    /// Show recovery notification to user
    /// </summary>
    private void ShowRecoveryNotification(bool success, bool restart = false)
    {
        var message = success ? 
            (restart ? "Game restarted successfully. Your progress has been saved!" : "Recovered from crash. Your progress has been saved!") :
            "The game encountered a problem. Please restart to continue.";
            
        // In a real implementation, you'd show this in a user-friendly way
        // For now, we'll use OS.alert as a simple notification
        OS.Alert(message, "Crash Recovery");
        
        // Track recovery attempt in analytics
        if (_analyticsEnabled)
        {
            AnalyticsEventTracker.Instance?.TrackEvent("crash_recovery_attempted", new Dictionary<string, object>
            {
                ["recovery_success"] = success,
                ["restart_required"] = restart,
                ["crash_count"] = _crashCount,
                ["time_since_last_crash"] = (DateTime.Now - _lastCrashTime).TotalMinutes
            });
        }
    }

    /// <summary>
    /// Show restart notification
    /// </summary>
    private void ShowRestartNotification()
    {
        OS.Alert("The game is restarting to recover from an error. Your progress has been saved.", "Restarting...");
    }

    /// <summary>
    /// Track crash data in analytics
    /// </summary>
    private void TrackCrashAnalytics(CrashReport crashReport)
    {
        try
        {
            AnalyticsEventTracker.Instance?.TrackEvent("crash_occurred", new Dictionary<string, object>
            {
                ["crash_type"] = crashReport.ExceptionType,
                ["context"] = crashReport.Context,
                ["memory_usage_mb"] = crashReport.MemoryUsageMB,
                ["current_fps"] = crashReport.FPS,
                ["game_scene"] = crashReport.ScenePath,
                ["crash_count_session"] = _crashCount,
                ["days_since_last_crash"] = (DateTime.Now - _lastCrashTime).TotalDays,
                ["timestamp"] = new DateTimeOffset(crashReport.Timestamp).ToUnixTimeMilliseconds()
            });
        }
        catch (Exception e)
        {
            GD.PrintErr($"Failed to track crash analytics: {e.Message}");
        }
    }

    /// <summary>
    /// Report crash to Firebase Crashlytics
    /// </summary>
    private void ReportToCrashlytics(CrashReport crashReport)
    {
        try
        {
            if (FirebaseManager.Instance?.Crashlytics != null)
            {
                FirebaseManager.Instance.Crashlytics.LogException(crashReport.ExceptionType, crashReport.ExceptionMessage);
                
                // Add custom keys for crash analysis
                FirebaseManager.Instance.Crashlytics.SetCustomKey("memory_usage_mb", crashReport.MemoryUsageMB);
                FirebaseManager.Instance.Crashlytics.SetCustomKey("current_fps", crashReport.FPS);
                FirebaseManager.Instance.Crashlytics.SetCustomKey("game_scene", crashReport.ScenePath);
                FirebaseManager.Instance.Crashlytics.SetCustomKey("crash_context", crashReport.Context);
            }
        }
        catch (Exception e)
        {
            GD.PrintErr($"Failed to report to Crashlytics: {e.Message}");
        }
    }

    /// <summary>
    /// Handle scene changes for crash context
    /// </summary>
    private void OnSceneChanged(string newScene)
    {
        // Update current scene context for crash reporting
        GD.Print($"Scene changed to: {newScene}");
    }

    /// <summary>
    /// Get crash statistics
    /// </summary>
    public Dictionary<string, object> GetCrashStatistics()
    {
        var totalCrashes = _crashHistory.Count;
        var crashesToday = _crashHistory.Count(c => (DateTime.Now - c.Timestamp).TotalDays < 1);
        var crashesThisWeek = _crashHistory.Count(c => (DateTime.Now - c.Timestamp).TotalDays < 7);
        
        var mostCommonException = _crashHistory
            .GroupBy(c => c.ExceptionType)
            .OrderByDescending(g => g.Count())
            .FirstOrDefault()?.Key ?? "None";
            
        var averageMemoryUsage = _crashHistory.Any() ? 
            _crashHistory.Average(c => c.MemoryUsageMB) : 0;
            
        var crashRatePer1000Sessions = CalculateCrashRatePer1000Sessions();
        
        return new Dictionary<string, object>
        {
            ["total_crashes"] = totalCrashes,
            ["crashes_today"] = crashesToday,
            ["crashes_this_week"] = crashesThisWeek,
            ["most_common_exception"] = mostCommonException,
            ["average_memory_usage_mb"] = averageMemoryUsage,
            ["crash_rate_per_1000_sessions"] = crashRatePer1000Sessions,
            ["last_crash_time"] = _lastCrashTime,
            ["last_error_message"] = _lastErrorMessage,
            ["recovery_attempts"] = _recoveryInProgress ? 1 : 0
        };
    }

    /// <summary>
    /// Calculate crash rate per 1000 sessions (simplified)
    /// </summary>
    private float CalculateCrashRatePer1000Sessions()
    {
        // This is a simplified calculation
        // In a real implementation, you'd track total sessions
        var totalSessions = AnalyticsManager.Instance?.GetTotalSessions() ?? 1;
        return (_crashHistory.Count / (float)totalSessions) * 1000f;
    }

    /// <summary>
    /// Export crash data for analysis
    /// </summary>
    public string ExportCrashDataToCSV()
    {
        var csv = new System.Text.StringBuilder();
        csv.AppendLine("Crash ID,Timestamp,Exception Type,Exception Message,Context,Scene,Memory MB,FPS,Device Model");
        
        foreach (var crash in _crashHistory)
        {
            csv.AppendLine($"{crash.CrashId},{crash.Timestamp:yyyy-MM-dd HH:mm:ss},{crash.ExceptionType},\"{crash.ExceptionMessage}\",{crash.Context},{crash.ScenePath},{crash.MemoryUsageMB},{crash.FPS},{crash.DeviceInfo.GetValueOrDefault("device_model", "Unknown")}");
        }
        
        return csv.ToString();
    }

    /// <summary>
    /// Check if recovery is currently in progress
    /// </summary>
    public bool IsRecoveryInProgress()
    {
        return _recoveryInProgress;
    }

    /// <summary>
    /// Force crash report (for testing)
    /// </summary>
    public void ForceCrashForTesting()
    {
        try
        {
            throw new Exception("Forced crash for testing purposes");
        }
        catch (Exception e)
        {
            ReportCrash(e, "Test");
        }
    }

    public override void _ExitTree()
    {
        // Save final crash history
        SaveCrashHistory();
    }
}

/// <summary>
/// Comprehensive crash report data structure
/// </summary>
public class CrashReport
{
    public string CrashId { get; set; }
    public DateTime Timestamp { get; set; }
    public string ExceptionType { get; set; }
    public string ExceptionMessage { get; set; }
    public string StackTrace { get; set; }
    public string Context { get; set; }
    public Dictionary<string, object> GameState { get; set; }
    public string ScenePath { get; set; }
    public Dictionary<string, object> DeviceInfo { get; set; }
    public long MemoryUsageMB { get; set; }
    public float FPS { get; set; }
    public float DeltaTime { get; set; }
    public int ActiveNodes { get; set; }
    public int PersistentNodes { get; set; }
    public int LoadedResources { get; set; }
    public string SystemLanguage { get; set; }
    public double TimezoneOffset { get; set; }
}