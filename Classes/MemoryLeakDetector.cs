using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

/// <summary>
/// Memory leak detection and management system
/// Tracks signal connections, object pooling, and potential memory leaks
/// </summary>
public class MemoryLeakDetector : Node
{
    public static MemoryLeakDetector Instance { get; private set; }

    // Memory tracking
    private Dictionary<string, MemoryTracker> _trackedObjects = new Dictionary<string, MemoryTracker>();
    private List<SignalConnection> _signalConnections = new List<SignalConnection>();
    private List<ObjectPool> _objectPools = new List<ObjectPool>();
    
    // Leak detection settings
    private LeakDetectionConfig _config;
    private bool _isEnabled = false;
    
    // Warning system
    private List<MemoryWarning> _activeWarnings = new List<MemoryWarning>();
    
    [Signal]
    public delegate void MemoryLeakDetectedEventHandler(string leakType, string objectInfo);
    
    [Signal]
    public delegate void MemoryWarningEventHandler(MemoryWarning warning);
    
    [Signal]
    public delegate void CleanupRequiredEventHandler(string cleanupType);

    public override void _Ready()
    {
        if (Instance != null)
        {
            QueueFree();
            return;
        }
        
        Instance = this;
        InitializeMemoryDetector();
    }

    /// <summary>
    /// Initialize memory leak detector
    /// </summary>
    private void InitializeMemoryDetector()
    {
        LoadConfiguration();
        
        // Only enable in debug builds or when explicitly enabled
        _isEnabled = OS.IsDebugBuild() || _config.AlwaysEnabled;
        
        if (_isEnabled)
        {
            StartMonitoring();
        }
        
        GD.Print("Memory leak detector initialized");
    }

    /// <summary>
    /// Load memory leak detection configuration
    /// </summary>
    private void LoadConfiguration()
    {
        _config = new LeakDetectionConfig
        {
            AlwaysEnabled = false,
            EnableSignalTracking = true,
            EnableObjectPoolTracking = true,
            EnableNodeTracking = true,
            MaxTrackedObjects = 10000,
            LeakThreshold = 100, // Number of leaked objects to trigger warning
            WarningThreshold = 50, // Number of objects to trigger warning
            CheckInterval = 5.0f, // Seconds between checks
            TrackConnections = true,
            TrackDisconnections = true,
            EnableCircularReferenceDetection = true,
            ReportMemoryUsage = true
        };
    }

    /// <summary>
    /// Start memory monitoring
    /// </summary>
    public void StartMonitoring()
    {
        _isEnabled = true;
        
        // Start periodic checks
        var timer = new Timer();
        timer.WaitTime = _config.CheckInterval;
        timer.Timeout += PerformMemoryCheck;
        timer.Start();
        
        // Connect to node signals for tracking
        ConnectToNodeSignals();
        
        GD.Print("Memory leak monitoring started");
    }

    /// <summary>
    /// Stop memory monitoring
    /// </summary>
    public void StopMonitoring()
    {
        _isEnabled = false;
        GD.Print("Memory leak monitoring stopped");
    }

    /// <summary>
    /// Connect to node signals for tracking
    /// </summary>
    private void ConnectToNodeSignals()
    {
        // This would connect to Godot's node lifecycle signals
        // For demonstration, we'll track manually
    }

    /// <summary>
    /// Track a signal connection
    /// </summary>
    public void TrackSignalConnection(object sender, string signalName, object receiver, string methodName)
    {
        if (!_isEnabled || !_config.EnableSignalTracking) return;
        
        var connection = new SignalConnection
        {
            ConnectionId = Guid.NewGuid().ToString(),
            Sender = sender.GetType().Name,
            SignalName = signalName,
            Receiver = receiver.GetType().Name,
            MethodName = methodName,
            CreatedAt = DateTime.Now,
            IsConnected = true
        };
        
        _signalConnections.Add(connection);
        
        GD.Print($"Signal tracked: {sender.GetType().Name}.{signalName} -> {receiver.GetType().Name}.{methodName}");
    }

    /// <summary>
    /// Track signal disconnection
    /// </summary>
    public void TrackSignalDisconnection(string connectionId)
    {
        if (!_isEnabled || !_config.TrackDisconnections) return;
        
        var connection = _signalConnections.FirstOrDefault(c => c.ConnectionId == connectionId);
        if (connection != null)
        {
            connection.IsConnected = false;
            connection.DisconnectedAt = DateTime.Now;
            
            GD.Print($"Signal disconnected: {connection.Sender}.{connection.SignalName} -> {connection.Receiver}.{connection.MethodName}");
        }
    }

    /// <summary>
    /// Track object instantiation
    /// </summary>
    public void TrackObjectCreation(object obj, string context = "")
    {
        if (!_isEnabled || !_config.EnableObjectTracking) return;
        
        var tracker = new MemoryTracker
        {
            ObjectId = Guid.NewGuid().ToString(),
            ObjectType = obj.GetType().Name,
            CreatedAt = DateTime.Now,
            Context = context,
            IsActive = true
        };
        
        _trackedObjects[tracker.ObjectId] = tracker;
        
        GD.Print($"Object tracked: {obj.GetType().Name} ({tracker.ObjectId})");
        
        CheckTrackingLimits();
    }

    /// <summary>
    /// Track object disposal
    /// </summary>
    public void TrackObjectDisposal(object obj)
    {
        if (!_isEnabled) return;
        
        var tracker = _trackedObjects.Values.FirstOrDefault(t => t.ObjectType == obj.GetType().Name && t.IsActive);
        if (tracker != null)
        {
            tracker.IsActive = false;
            tracker.DisposedAt = DateTime.Now;
            
            GD.Print($"Object disposed: {obj.GetType().Name} ({tracker.ObjectId})");
        }
    }

    /// <summary>
    /// Register object pool for tracking
    /// </summary>
    public void RegisterObjectPool(string poolName, int maxSize, Type objectType)
    {
        if (!_isEnabled || !_config.EnableObjectPoolTracking) return;
        
        var pool = new ObjectPool
        {
            PoolName = poolName,
            ObjectType = objectType.Name,
            MaxSize = maxSize,
            CurrentSize = 0,
            ActiveObjects = new List<string>(),
            CreatedAt = DateTime.Now
        };
        
        _objectPools.Add(pool);
        
        GD.Print($"Object pool registered: {poolName} (max: {maxSize}, type: {objectType.Name})");
    }

    /// <summary>
    /// Track pool object allocation
    /// </summary>
    public void TrackPoolAllocation(string poolName, string objectId)
    {
        if (!_isEnabled) return;
        
        var pool = _objectPools.FirstOrDefault(p => p.PoolName == poolName);
        if (pool != null)
        {
            pool.ActiveObjects.Add(objectId);
            pool.CurrentSize++;
            
            GD.Print($"Pool allocation: {poolName} -> {objectId} (active: {pool.CurrentSize}/{pool.MaxSize})");
        }
    }

    /// <summary>
    /// Track pool object release
    /// </summary>
    public void TrackPoolRelease(string poolName, string objectId)
    {
        if (!_isEnabled) return;
        
        var pool = _objectPools.FirstOrDefault(p => p.PoolName == poolName);
        if (pool != null)
        {
            pool.ActiveObjects.Remove(objectId);
            pool.CurrentSize--;
            
            GD.Print($"Pool release: {poolName} <- {objectId} (active: {pool.CurrentSize}/{pool.MaxSize})");
        }
    }

    /// <summary>
    /// Perform memory check
    /// </summary>
    private void PerformMemoryCheck()
    {
        if (!_isEnabled) return;
        
        DetectMemoryLeaks();
        CheckCircularReferences();
        CheckObjectPoolHealth();
        ReportMemoryUsage();
    }

    /// <summary>
    /// Detect potential memory leaks
    /// </summary>
    private void DetectMemoryLeaks()
    {
        // Find objects that should have been cleaned up
        var leakedObjects = _trackedObjects.Values
            .Where(t => t.IsActive && (DateTime.Now - t.CreatedAt).TotalMinutes > 30)
            .GroupBy(t => t.ObjectType)
            .ToList();
        
        foreach (var group in leakedObjects)
        {
            int leakCount = group.Count();
            if (leakCount > _config.LeakThreshold)
            {
                var warning = new MemoryWarning
                {
                    WarningType = "Potential Memory Leak",
                    ObjectType = group.Key,
                    Count = leakCount,
                    Description = $"Found {leakCount} objects of type {group.Key} that haven't been cleaned up",
                    Severity = WarningSeverity.High
                };
                
                _activeWarnings.Add(warning);
                EmitSignal("MemoryLeakDetected", "ObjectLeak", $"{group.Key}: {leakCount} objects");
                EmitSignal("MemoryWarning", warning);
            }
        }
        
        // Find orphaned signal connections
        var orphanedConnections = _signalConnections
            .Where(c => c.IsConnected && (DateTime.Now - c.CreatedAt).TotalMinutes > 60)
            .ToList();
        
        if (orphanedConnections.Count > 10)
        {
            var warning = new MemoryWarning
            {
                WarningType = "Orphaned Signal Connections",
                ObjectType = "SignalConnection",
                Count = orphanedConnections.Count,
                Description = $"Found {orphanedConnections.Count} signal connections that may be orphaned",
                Severity = WarningSeverity.Medium
            };
            
            _activeWarnings.Add(warning);
            EmitSignal("MemoryWarning", warning);
        }
    }

    /// <summary>
    /// Check for circular references
    /// </summary>
    private void CheckCircularReferences()
    {
        if (!_config.EnableCircularReferenceDetection) return;
        
        // This would require more sophisticated analysis
        // For now, we'll look for common patterns
        
        var potentialCircularRefs = new List<string>();
        
        // Check for mutual references between common types
        var nodeTypes = _trackedObjects.Values
            .Where(t => t.ObjectType.Contains("Node") && t.IsActive)
            .GroupBy(t => t.ObjectType)
            .ToList();
        
        foreach (var group in nodeTypes)
        {
            if (group.Count() > 20) // Arbitrary threshold
            {
                potentialCircularRefs.Add($"High count of {group.Key} nodes: {group.Count()}");
            }
        }
        
        if (potentialCircularRefs.Any())
        {
            var warning = new MemoryWarning
            {
                WarningType = "Potential Circular References",
                ObjectType = "Mixed",
                Count = potentialCircularRefs.Count,
                Description = string.Join("; ", potentialCircularRefs),
                Severity = WarningSeverity.Medium
            };
            
            _activeWarnings.Add(warning);
            EmitSignal("MemoryWarning", warning);
        }
    }

    /// <summary>
    /// Check object pool health
    /// </summary>
    private void CheckObjectPoolHealth()
    {
        foreach (var pool in _objectPools)
        {
            // Check for pool overflow
            if (pool.CurrentSize > pool.MaxSize)
            {
                var warning = new MemoryWarning
                {
                    WarningType = "Object Pool Overflow",
                    ObjectType = pool.ObjectType,
                    Count = pool.CurrentSize,
                    Description = $"Pool {pool.PoolName} exceeded capacity: {pool.CurrentSize}/{pool.MaxSize}",
                    Severity = WarningSeverity.High
                };
                
                _activeWarnings.Add(warning);
                EmitSignal("MemoryWarning", warning);
            }
            
            // Check for pool underutilization (objects not being released)
            var utilizationRate = pool.CurrentSize / (float)pool.MaxSize;
            if (utilizationRate > 0.9f) // 90% utilization
            {
                var warning = new MemoryWarning
                {
                    WarningType = "High Pool Utilization",
                    ObjectType = pool.ObjectType,
                    Count = pool.CurrentSize,
                    Description = $"Pool {pool.PoolName} highly utilized: {utilizationRate:P}",
                    Severity = WarningSeverity.Low
                };
                
                _activeWarnings.Add(warning);
                EmitSignal("MemoryWarning", warning);
            }
        }
    }

    /// <summary>
    /// Report current memory usage
    /// </summary>
    private void ReportMemoryUsage()
    {
        if (!_config.ReportMemoryUsage) return;
        
        var totalObjects = _trackedObjects.Values.Count(t => t.IsActive);
        var activeConnections = _signalConnections.Count(c => c.IsConnected);
        var totalPools = _objectPools.Count;
        var poolObjects = _objectPools.Sum(p => p.CurrentSize);
        
        GD.Print($"Memory Usage Report:");
        GD.Print($"  Active Objects: {totalObjects}");
        GD.Print($"  Signal Connections: {activeConnections}");
        GD.Print($"  Object Pools: {totalPools} (total objects: {poolObjects})");
        GD.Print($"  Active Warnings: {_activeWarnings.Count}");
        
        // Trigger cleanup if memory usage is high
        if (totalObjects > 5000)
        {
            EmitSignal("CleanupRequired", "HighObjectCount");
        }
    }

    /// <summary>
    /// Check if we're tracking too many objects
    /// </summary>
    private void CheckTrackingLimits()
    {
        var activeObjectCount = _trackedObjects.Values.Count(t => t.IsActive);
        
        if (activeObjectCount > _config.MaxTrackedObjects)
        {
            var warning = new MemoryWarning
            {
                WarningType = "Tracking Limit Exceeded",
                ObjectType = "System",
                Count = activeObjectCount,
                Description = $"Exceeded maximum tracked objects: {activeObjectCount}/{_config.MaxTrackedObjects}",
                Severity = WarningSeverity.Medium
            };
            
            _activeWarnings.Add(warning);
            EmitSignal("MemoryWarning", warning);
        }
    }

    /// <summary>
    /// Generate memory leak report
    /// </summary>
    public MemoryLeakReport GenerateLeakReport()
    {
        var report = new MemoryLeakReport
        {
            GeneratedAt = DateTime.Now,
            TotalTrackedObjects = _trackedObjects.Count,
            ActiveObjects = _trackedObjects.Values.Count(t => t.IsActive),
            TotalSignalConnections = _signalConnections.Count,
            ActiveConnections = _signalConnections.Count(c => c.IsConnected),
            OrphanedConnections = _signalConnections.Count(c => c.IsConnected && (DateTime.Now - c.CreatedAt).TotalMinutes > 60),
            ObjectPools = _objectPools.Count,
            PoolObjects = _objectPools.Sum(p => p.CurrentSize),
            ActiveWarnings = _activeWarnings.Count,
            LeakedObjects = DetectLeakedObjects(),
            CircularReferences = DetectCircularReferences(),
            PoolHealth = GetPoolHealth()
        };
        
        return report;
    }

    /// <summary>
    /// Detect leaked objects
    /// </summary>
    private List<LeakedObject> DetectLeakedObjects()
    {
        var cutoffTime = DateTime.Now.AddMinutes(-30); // Objects older than 30 minutes
        
        return _trackedObjects.Values
            .Where(t => t.IsActive && t.CreatedAt < cutoffTime)
            .GroupBy(t => t.ObjectType)
            .Select(group => new LeakedObject
            {
                ObjectType = group.Key,
                Count = group.Count(),
                OldestCreatedAt = group.Min(t => t.CreatedAt),
                Contexts = group.Where(t => !string.IsNullOrEmpty(t.Context)).Select(t => t.Context).Distinct().ToList()
            })
            .OrderByDescending(lo => lo.Count)
            .ToList();
    }

    /// <summary>
    /// Detect circular references (simplified)
    /// </summary>
    private List<CircularReference> DetectCircularReferences()
    {
        // This would require more sophisticated analysis
        // For now, return empty list
        return new List<CircularReference>();
    }

    /// <summary>
    /// Get object pool health status
    /// </summary>
    private List<PoolHealthStatus> GetPoolHealth()
    {
        return _objectPools.Select(pool => new PoolHealthStatus
        {
            PoolName = pool.PoolName,
            ObjectType = pool.ObjectType,
            CurrentSize = pool.CurrentSize,
            MaxSize = pool.MaxSize,
            UtilizationRate = pool.MaxSize > 0 ? pool.CurrentSize / (float)pool.MaxSize : 0f,
            IsHealthy = pool.CurrentSize <= pool.MaxSize,
            HealthIssues = GeneratePoolHealthIssues(pool)
        }).ToList();
    }

    /// <summary>
    /// Generate pool health issues
    /// </summary>
    private List<string> GeneratePoolHealthIssues(ObjectPool pool)
    {
        var issues = new List<string>();
        
        if (pool.CurrentSize > pool.MaxSize)
        {
            issues.Add($"Pool overflow: {pool.CurrentSize}/{pool.MaxSize}");
        }
        
        if (pool.CurrentSize > pool.MaxSize * 0.9f)
        {
            issues.Add("High utilization rate");
        }
        
        if (pool.CurrentSize == 0 && (DateTime.Now - pool.CreatedAt).TotalMinutes > 10)
        {
            issues.Add("Pool appears unused");
        }
        
        return issues;
    }

    /// <summary>
    /// Clear all warnings
    /// </summary>
    public void ClearWarnings()
    {
        _activeWarnings.Clear();
        GD.Print("Memory warnings cleared");
    }

    /// <summary>
    /// Force cleanup of potential leaks
    /// </summary>
    public void ForceCleanup()
    {
        // Clean up old signal connections
        var oldConnections = _signalConnections
            .Where(c => c.IsConnected && (DateTime.Now - c.CreatedAt).TotalMinutes > 120)
            .ToList();
        
        foreach (var connection in oldConnections)
        {
            connection.IsConnected = false;
            connection.DisconnectedAt = DateTime.Now;
        }
        
        GD.Print($"Force cleanup completed: {oldConnections.Count} connections marked as disconnected");
    }

    /// <summary>
    /// Get current configuration
    /// </summary>
    public LeakDetectionConfig GetConfig()
    {
        return _config;
    }

    /// <summary>
    /// Update configuration
    /// </summary>
    public void UpdateConfig(Action<LeakDetectionConfig> configUpdater)
    {
        configUpdater(_config);
        
        if (_isEnabled && !_config.AlwaysEnabled && !OS.IsDebugBuild())
        {
            StopMonitoring();
        }
    }

    /// <summary>
    /// Export leak report to file
    /// </summary>
    public void ExportLeakReport(string filePath)
    {
        var report = GenerateLeakReport();
        
        try
        {
            var options = new System.Text.Json.JsonSerializerOptions 
            { 
                WriteIndented = true,
                PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
            };
            
            string json = System.Text.Json.JsonSerializer.Serialize(report, options);
            System.IO.File.WriteAllText(filePath, json);
            
            GD.Print($"Memory leak report exported: {filePath}");
        }
        catch (Exception e)
        {
            GD.PrintErr($"Failed to export leak report: {e.Message}");
        }
    }

    /// <summary>
    /// Check if monitoring is enabled
    /// </summary>
    public bool IsMonitoringEnabled()
    {
        return _isEnabled;
    }

    /// <summary>
    /// Enable monitoring
    /// </summary>
    public void EnableMonitoring()
    {
        if (!_isEnabled)
        {
            StartMonitoring();
        }
    }

    /// <summary>
    /// Disable monitoring
    /// </summary>
    public void DisableMonitoring()
    {
        if (_isEnabled)
        {
            StopMonitoring();
        }
    }
}

/// <summary>
/// Memory tracker for objects
/// </summary>
public class MemoryTracker
{
    public string ObjectId { get; set; }
    public string ObjectType { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? DisposedAt { get; set; }
    public string Context { get; set; }
    public bool IsActive { get; set; }
}

/// <summary>
/// Signal connection tracker
/// </summary>
public class SignalConnection
{
    public string ConnectionId { get; set; }
    public string Sender { get; set; }
    public string SignalName { get; set; }
    public string Receiver { get; set; }
    public string MethodName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? DisconnectedAt { get; set; }
    public bool IsConnected { get; set; }
}

/// <summary>
/// Object pool tracker
/// </summary>
public class ObjectPool
{
    public string PoolName { get; set; }
    public string ObjectType { get; set; }
    public int MaxSize { get; set; }
    public int CurrentSize { get; set; }
    public List<string> ActiveObjects { get; set; } = new List<string>();
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Memory warning
/// </summary>
public class MemoryWarning
{
    public string WarningType { get; set; }
    public string ObjectType { get; set; }
    public int Count { get; set; }
    public string Description { get; set; }
    public WarningSeverity Severity { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}

/// <summary>
/// Memory leak report
/// </summary>
public class MemoryLeakReport
{
    public DateTime GeneratedAt { get; set; }
    public int TotalTrackedObjects { get; set; }
    public int ActiveObjects { get; set; }
    public int TotalSignalConnections { get; set; }
    public int ActiveConnections { get; set; }
    public int OrphanedConnections { get; set; }
    public int ObjectPools { get; set; }
    public int PoolObjects { get; set; }
    public int ActiveWarnings { get; set; }
    public List<LeakedObject> LeakedObjects { get; set; } = new List<LeakedObject>();
    public List<CircularReference> CircularReferences { get; set; } = new List<CircularReference>();
    public List<PoolHealthStatus> PoolHealth { get; set; } = new List<PoolHealthStatus>();
}

/// <summary>
/// Leaked object information
/// </summary>
public class LeakedObject
{
    public string ObjectType { get; set; }
    public int Count { get; set; }
    public DateTime OldestCreatedAt { get; set; }
    public List<string> Contexts { get; set; } = new List<string>();
}

/// <summary>
/// Circular reference information
/// </summary>
public class CircularReference
{
    public string ObjectType1 { get; set; }
    public string ObjectType2 { get; set; }
    public int ReferenceCount { get; set; }
}

/// <summary>
/// Pool health status
/// </summary>
public class PoolHealthStatus
{
    public string PoolName { get; set; }
    public string ObjectType { get; set; }
    public int CurrentSize { get; set; }
    public int MaxSize { get; set; }
    public float UtilizationRate { get; set; }
    public bool IsHealthy { get; set; }
    public List<string> HealthIssues { get; set; } = new List<string>();
}

/// <summary>
/// Memory leak detection configuration
/// </summary>
public class LeakDetectionConfig
{
    public bool AlwaysEnabled { get; set; }
    public bool EnableSignalTracking { get; set; }
    public bool EnableObjectPoolTracking { get; set; }
    public bool EnableObjectTracking { get; set; }
    public int MaxTrackedObjects { get; set; }
    public int LeakThreshold { get; set; }
    public int WarningThreshold { get; set; }
    public float CheckInterval { get; set; }
    public bool TrackConnections { get; set; }
    public bool TrackDisconnections { get; set; }
    public bool EnableCircularReferenceDetection { get; set; }
    public bool ReportMemoryUsage { get; set; }
}

/// <summary>
/// Warning severity levels
/// </summary>
public enum WarningSeverity
{
    Low,
    Medium,
    High,
    Critical
}