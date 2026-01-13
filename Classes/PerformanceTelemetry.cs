using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

/// <summary>
/// Real-time performance monitoring with metrics tracking and alerting
/// Monitors FPS, memory, CPU, network, and load times with historical data
/// </summary>
public class PerformanceTelemetry : Node
{
    public static PerformanceTelemetry Instance { get; private set; }

    // Performance metrics
    private float _currentFps;
    private float _minFps;
    private float _maxFps;
    private float _averageFps;
    private long _currentMemoryMB;
    private long _peakMemoryMB;
    private float _currentCpuUsage;
    private float _networkBandwidthKBps;
    private float _levelLoadTime;
    private float _menuTransitionTime;
    private float _assetLoadTime;
    
    // Historical data
    private List<PerformanceSnapshot> _fpsHistory = new List<PerformanceSnapshot>();
    private List<PerformanceSnapshot> _memoryHistory = new List<PerformanceSnapshot>();
    private List<PerformanceSnapshot> _cpuHistory = new List<PerformanceSnapshot>();
    private const int MAX_HISTORY_SIZE = 100;
    
    // Session metrics
    private DateTime _sessionStartTime;
    private float _totalGameplayTime;
    private int _levelsCompleted;
    private int _frameDrops;
    private int _memorySpikes;
    private int _loadTimeouts;
    
    // Alerts and thresholds
    private Dictionary<PerformanceAlertType, AlertConfig> _alertConfigs;
    private List<PerformanceAlert> _activeAlerts = new List<PerformanceAlert>();
    
    [Signal]
    public delegate void PerformanceMetricUpdatedEventHandler(string metricName, float value);
    
    [Signal]
    public delegate void PerformanceAlertTriggeredEventHandler(PerformanceAlert alert);
    
    [Signal]
    public delegate void PerformanceIssueDetectedEventHandler(string issue, float severity);

    public override void _Ready()
    {
        if (Instance != null)
        {
            QueueFree();
            return;
        }
        
        Instance = this;
        InitializePerformanceTelemetry();
    }

    /// <summary>
    /// Initialize performance telemetry system
    /// </summary>
    private void InitializePerformanceTelemetry()
    {
        _sessionStartTime = DateTime.Now;
        _totalGameplayTime = 0f;
        _levelsCompleted = 0;
        _frameDrops = 0;
        _memorySpikes = 0;
        _loadTimeouts = 0;
        
        InitializeAlertConfigs();
        StartMonitoring();
        
        GD.Print("Performance Telemetry initialized");
    }

    /// <summary>
    /// Initialize alert configurations and thresholds
    /// </summary>
    private void InitializeAlertConfigs()
    {
        _alertConfigs = new Dictionary<PerformanceAlertType, AlertConfig>
        {
            [PerformanceAlertType.FrameDrop] = new AlertConfig
            {
                Threshold = 30f,
                Severity = AlertSeverity.Medium,
                CooldownSeconds = 30
            },
            [PerformanceAlertType.MemorySpike] = new AlertConfig
            {
                Threshold = 500f,
                Severity = AlertSeverity.High,
                CooldownSeconds = 60
            },
            [PerformanceAlertType.LoadTimeout] = new AlertConfig
            {
                Threshold = 5f,
                Severity = AlertSeverity.High,
                CooldownSeconds = 120
            },
            [PerformanceAlertType.NetworkTimeout] = new AlertConfig
            {
                Threshold = 10f,
                Severity = AlertSeverity.Medium,
                CooldownSeconds = 30
            },
            [PerformanceAlertType.CpuHigh] = new AlertConfig
            {
                Threshold = 80f,
                Severity = AlertSeverity.Medium,
                CooldownSeconds = 60
            }
        };
    }

    /// <summary>
    /// Start performance monitoring
    /// </summary>
    private void StartMonitoring()
    {
        SetProcess(true);
        SetPhysicsProcess(true);
    }

    /// <summary>
    /// Update performance metrics
    /// </summary>
    public override void _Process(float delta)
    {
        UpdateFrameRateMetrics();
        UpdateMemoryMetrics();
        UpdateCpuUsage();
        UpdateNetworkMetrics();
        UpdateSessionMetrics();
        CheckAlerts();
        MaintainHistory();
    }

    /// <summary>
    /// Update frame rate metrics
    /// </summary>
    private void UpdateFrameRateMetrics()
    {
        var fps = Engine.GetFramesPerSecond();
        
        if (_fpsHistory.Count == 0)
        {
            _minFps = fps;
            _maxFps = fps;
            _averageFps = fps;
        }
        else
        {
            _minFps = Mathf.Min(_minFps, fps);
            _maxFps = Mathf.Max(_maxFps, fps);
            
            // Calculate rolling average
            var recentFps = _fpsHistory.TakeLast(60).Select(s => s.Value).Concat(new[] { fps });
            _averageFps = recentFps.Average();
        }
        
        _currentFps = fps;
        
        // Check for frame drops
        if (fps < 30f)
        {
            _frameDrops++;
            TriggerAlert(PerformanceAlertType.FrameDrop, $"FPS dropped to {fps:F1}", fps);
        }
        
        EmitSignal("PerformanceMetricUpdated", "fps", fps);
    }

    /// <summary>
    /// Update memory metrics
    /// </summary>
    private void UpdateMemoryMetrics()
    {
        var memoryInfo = OS.GetStaticMemoryUsage();
        _currentMemoryMB = memoryInfo / (1024 * 1024);
        _peakMemoryMB = Mathf.Max(_peakMemoryMB, _currentMemoryMB);
        
        // Check for memory spikes
        if (_currentMemoryMB > 500)
        {
            _memorySpikes++;
            TriggerAlert(PerformanceAlertType.MemorySpike, $"Memory spike: {_currentMemoryMB}MB", _currentMemoryMB);
        }
        
        EmitSignal("PerformanceMetricUpdated", "memory_mb", _currentMemoryMB);
    }

    /// <summary>
    /// Update CPU usage
    /// </summary>
    private void UpdateCpuUsage()
    {
        // Note: This is a simplified implementation
        // In a real implementation, you'd get actual CPU usage from OS
        var frameTime = Engine.GetProcessTime();
        _currentCpuUsage = Mathf.Clamp(frameTime * 1000f, 0f, 100f);
        
        if (_currentCpuUsage > 80f)
        {
            TriggerAlert(PerformanceAlertType.CpuHigh, $"High CPU usage: {_currentCpuUsage:F1}%", _currentCpuUsage);
        }
        
        EmitSignal("PerformanceMetricUpdated", "cpu_usage", _currentCpuUsage);
    }

    /// <summary>
    /// Update network metrics
    /// </summary>
    private void UpdateNetworkMetrics()
    {
        // Simplified network monitoring
        // In a real implementation, you'd monitor actual network traffic
        if (FirebaseManager.Instance != null && FirebaseManager.Instance.IsConnected)
        {
            _networkBandwidthKBps = 50f; // Placeholder value
        }
        else
        {
            _networkBandwidthKBps = 0f;
        }
        
        EmitSignal("PerformanceMetricUpdated", "network_kbps", _networkBandwidthKBps);
    }

    /// <summary>
    /// Update session-level metrics
    /// </summary>
    private void UpdateSessionMetrics()
    {
        _totalGameplayTime = (float)(DateTime.Now - _sessionStartTime).TotalSeconds;
    }

    /// <summary>
    /// Check for performance alerts
    /// </summary>
    private void CheckAlerts()
    {
        foreach (var alertConfig in _alertConfigs)
        {
            var alertType = alertConfig.Key;
            var config = alertConfig.Value;
            
            // Check if alert should trigger based on cooldown
            var recentAlerts = _activeAlerts.Where(a => a.AlertType == alertType && 
                (DateTime.Now - a.Timestamp).TotalSeconds < config.CooldownSeconds);
            
            if (recentAlerts.Any()) continue;
            
            // Check thresholds
            bool shouldTrigger = false;
            float currentValue = 0f;
            
            switch (alertType)
            {
                case PerformanceAlertType.FrameDrop:
                    currentValue = _currentFps;
                    shouldTrigger = currentValue < config.Threshold;
                    break;
                case PerformanceAlertType.MemorySpike:
                    currentValue = _currentMemoryMB;
                    shouldTrigger = currentValue > config.Threshold;
                    break;
                case PerformanceAlertType.CpuHigh:
                    currentValue = _currentCpuUsage;
                    shouldTrigger = currentValue > config.Threshold;
                    break;
            }
            
            if (shouldTrigger)
            {
                TriggerAlert(alertType, $"{alertType} threshold exceeded: {currentValue:F1}", currentValue);
            }
        }
    }

    /// <summary>
    /// Trigger a performance alert
    /// </summary>
    private void TriggerAlert(PerformanceAlertType alertType, string message, float severity)
    {
        var alert = new PerformanceAlert
        {
            AlertType = alertType,
            Message = message,
            Severity = _alertConfigs[alertType].Severity,
            Timestamp = DateTime.Now,
            Value = severity
        };
        
        _activeAlerts.Add(alert);
        
        // Keep only recent alerts
        _activeAlerts = _activeAlerts.Where(a => (DateTime.Now - a.Timestamp).TotalMinutes < 10).ToList();
        
        EmitSignal("PerformanceAlertTriggered", alert);
        
        // Log to analytics
        if (AnalyticsEventTracker.Instance != null)
        {
            AnalyticsEventTracker.Instance.TrackEvent("performance_alert", new Dictionary<string, object>
            {
                ["alert_type"] = alertType.ToString(),
                ["severity"] = alert.Severity.ToString(),
                ["message"] = message,
                ["value"] = severity,
                ["timestamp"] = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            });
        }
        
        GD.PrintErr($"Performance Alert: {alertType} - {message}");
    }

    /// <summary>
    /// Maintain historical data with size limits
    /// </summary>
    private void MaintainHistory()
    {
        var snapshot = new PerformanceSnapshot
        {
            Timestamp = DateTime.Now,
            Fps = _currentFps,
            MemoryMB = _currentMemoryMB,
            CpuUsage = _currentCpuUsage
        };
        
        _fpsHistory.Add(snapshot);
        _memoryHistory.Add(snapshot);
        _cpuHistory.Add(snapshot);
        
        // Trim history to max size
        if (_fpsHistory.Count > MAX_HISTORY_SIZE)
        {
            _fpsHistory.RemoveAt(0);
            _memoryHistory.RemoveAt(0);
            _cpuHistory.RemoveAt(0);
        }
    }

    /// <summary>
    /// Record level completion for metrics
    /// </summary>
    public void RecordLevelCompletion(string levelId, float completionTime)
    {
        _levelsCompleted++;
        
        // Track level-specific performance
        if (AnalyticsEventTracker.Instance != null)
        {
            AnalyticsEventTracker.Instance.TrackEvent("level_completed_with_performance", new Dictionary<string, object>
            {
                ["level_id"] = levelId,
                ["completion_time"] = completionTime,
                ["fps_average"] = _averageFps,
                ["memory_peak"] = _peakMemoryMB,
                ["frame_drops"] = _frameDrops
            });
        }
    }

    /// <summary>
    /// Record load time for specific operations
    /// </summary>
    public void RecordLoadTime(LoadOperation operation, float loadTime)
    {
        switch (operation)
        {
            case LoadOperation.LevelLoad:
                _levelLoadTime = loadTime;
                if (loadTime > 5f)
                {
                    _loadTimeouts++;
                    TriggerAlert(PerformanceAlertType.LoadTimeout, $"Level load time: {loadTime:F1}s", loadTime);
                }
                break;
            case LoadOperation.MenuTransition:
                _menuTransitionTime = loadTime;
                break;
            case LoadOperation.AssetLoad:
                _assetLoadTime = loadTime;
                break;
        }
        
        EmitSignal("PerformanceMetricUpdated", $"load_time_{operation.ToString().ToLower()}", loadTime);
    }

    /// <summary>
    /// Get current performance snapshot
    /// </summary>
    public PerformanceSnapshot GetCurrentSnapshot()
    {
        return new PerformanceSnapshot
        {
            Timestamp = DateTime.Now,
            Fps = _currentFps,
            MemoryMB = _currentMemoryMB,
            CpuUsage = _currentCpuUsage,
            NetworkKBps = _networkBandwidthKBps
        };
    }

    /// <summary>
    /// Get performance summary for UI
    /// </summary>
    public Dictionary<string, object> GetPerformanceSummary()
    {
        return new Dictionary<string, object>
        {
            ["current_fps"] = _currentFps,
            ["average_fps"] = _averageFps,
            ["min_fps"] = _minFps,
            ["max_fps"] = _maxFps,
            ["current_memory_mb"] = _currentMemoryMB,
            ["peak_memory_mb"] = _peakMemoryMB,
            ["cpu_usage"] = _currentCpuUsage,
            ["network_kbps"] = _networkBandwidthKBps,
            ["session_time"] = _totalGameplayTime,
            ["levels_completed"] = _levelsCompleted,
            ["frame_drops"] = _frameDrops,
            ["memory_spikes"] = _memorySpikes,
            ["load_timeouts"] = _loadTimeouts,
            ["active_alerts"] = _activeAlerts.Count
        };
    }

    /// <summary>
    /// Get historical performance data
    /// </summary>
    public List<PerformanceSnapshot> GetHistoricalData(string metric, int minutes = 5)
    {
        var cutoffTime = DateTime.Now.AddMinutes(-minutes);
        
        switch (metric.ToLower())
        {
            case "fps":
                return _fpsHistory.Where(s => s.Timestamp >= cutoffTime).ToList();
            case "memory":
                return _memoryHistory.Where(s => s.Timestamp >= cutoffTime).ToList();
            case "cpu":
                return _cpuHistory.Where(s => s.Timestamp >= cutoffTime).ToList();
            default:
                return new List<PerformanceSnapshot>();
        }
    }

    /// <summary>
    /// Export performance data to CSV
    /// </summary>
    public string ExportPerformanceDataToCSV()
    {
        var csv = new System.Text.StringBuilder();
        csv.AppendLine("Timestamp,FPS,Memory(MB),CPU(%),Network(KB/s),Frame Drops,Memory Spikes");
        
        foreach (var snapshot in _fpsHistory)
        {
            csv.AppendLine($"{snapshot.Timestamp:yyyy-MM-dd HH:mm:ss},{snapshot.Fps:F1},{snapshot.MemoryMB},{snapshot.CpuUsage:F1},{snapshot.NetworkKBps:F1},{_frameDrops},{_memorySpikes}");
        }
        
        return csv.ToString();
    }

    /// <summary>
    /// Reset performance counters (call on new session)
    /// </summary>
    public void ResetSessionMetrics()
    {
        _sessionStartTime = DateTime.Now;
        _totalGameplayTime = 0f;
        _levelsCompleted = 0;
        _frameDrops = 0;
        _memorySpikes = 0;
        _loadTimeouts = 0;
        _activeAlerts.Clear();
        
        GD.Print("Performance session metrics reset");
    }

    /// <summary>
    /// Get performance recommendations based on current metrics
    /// </summary>
    public List<string> GetPerformanceRecommendations()
    {
        var recommendations = new List<string>();
        
        if (_averageFps < 30f)
        {
            recommendations.Add("FPS below target - consider reducing particle effects or physics complexity");
        }
        
        if (_peakMemoryMB > 400)
        {
            recommendations.Add("High memory usage - consider asset optimization or memory pooling");
        }
        
        if (_loadTimeouts > 5)
        {
            recommendations.Add("Frequent load timeouts - consider async loading or asset compression");
        }
        
        if (_frameDrops > 10)
        {
            recommendations.Add("Frequent frame drops - optimize rendering pipeline");
        }
        
        return recommendations;
    }
}

/// <summary>
/// Performance snapshot for historical data
/// </summary>
public class PerformanceSnapshot
{
    public DateTime Timestamp { get; set; }
    public float Fps { get; set; }
    public long MemoryMB { get; set; }
    public float CpuUsage { get; set; }
    public float NetworkKBps { get; set; }
}

/// <summary>
/// Performance alert types
/// </summary>
public enum PerformanceAlertType
{
    FrameDrop,
    MemorySpike,
    LoadTimeout,
    NetworkTimeout,
    CpuHigh
}

/// <summary>
/// Alert severity levels
/// </summary>
public enum AlertSeverity
{
    Low,
    Medium,
    High,
    Critical
}

/// <summary>
/// Performance alert configuration
/// </summary>
public class AlertConfig
{
    public float Threshold { get; set; }
    public AlertSeverity Severity { get; set; }
    public int CooldownSeconds { get; set; }
}

/// <summary>
/// Performance alert instance
/// </summary>
public class PerformanceAlert
{
    public PerformanceAlertType AlertType { get; set; }
    public string Message { get; set; }
    public AlertSeverity Severity { get; set; }
    public DateTime Timestamp { get; set; }
    public float Value { get; set; }
}

/// <summary>
/// Load operation types
/// </summary>
public enum LoadOperation
{
    LevelLoad,
    MenuTransition,
    AssetLoad
}