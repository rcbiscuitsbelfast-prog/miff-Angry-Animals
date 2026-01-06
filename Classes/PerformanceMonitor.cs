using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

/// <summary>
/// Comprehensive performance monitoring system
/// Tracks FPS, memory usage, frame time, and provides performance HUD
/// </summary>
public class PerformanceMonitor : Node
{
    public static PerformanceMonitor Instance { get; private set; }

    // Performance metrics
    private PerformanceMetrics _currentMetrics = new PerformanceMetrics();
    private List<PerformanceFrame> _frameHistory = new List<PerformanceFrame>();
    private Dictionary<string, PerformancePreset> _qualityPresets = new Dictionary<string, PerformancePreset>();
    
    // HUD display
    private CanvasLayer _hudLayer;
    private Control _hudPanel;
    private bool _hudVisible = false;
    private bool _showInRelease = false;
    
    // Performance tracking
    private float _frameAccumulator = 0f;
    private int _frameCount = 0;
    private DateTime _sessionStartTime;
    
    // Memory tracking
    private System.Timers.Timer _memoryTimer;
    private List<MemorySnapshot> _memoryHistory = new List<MemorySnapshot>();
    
    [Signal]
    public delegate void PerformanceWarningEventHandler(string warningType, float value);
    
    [Signal]
    public delegate void PerformanceMetricsUpdatedEventHandler(PerformanceMetrics metrics);

    public override void _Ready()
    {
        if (Instance != null)
        {
            QueueFree();
            return;
        }
        
        Instance = this;
        InitializePerformanceSystem();
    }

    /// <summary>
    /// Initialize performance monitoring system
    /// </summary>
    private void InitializePerformanceSystem()
    {
        _sessionStartTime = DateTime.Now;
        CreateQualityPresets();
        InitializeMemoryTracking();
        CreatePerformanceHud();
        
        // Enable performance monitoring
        SetProcess(true);
        SetPhysicsProcess(true);
        
        GD.Print("Performance monitor initialized");
    }

    /// <summary>
    /// Create quality presets for different performance levels
    /// </summary>
    private void CreateQualityPresets()
    {
        // High Quality Preset
        _qualityPresets["High"] = new PerformancePreset
        {
            Name = "High Quality",
            TargetFps = 60,
            MaxParticles = 200,
            TextureQuality = 1.0f,
            ShadowQuality = 1.0f,
            MsaaLevel = 2,
            AnisotropicFiltering = 4,
            ScreenShakeIntensity = 1.0f,
            ParticleCountMultiplier = 1.0f,
            AudioQuality = 1.0f,
            VsyncEnabled = true
        };

        // Balanced Preset
        _qualityPresets["Balanced"] = new PerformancePreset
        {
            Name = "Balanced",
            TargetFps = 60,
            MaxParticles = 100,
            TextureQuality = 0.8f,
            ShadowQuality = 0.7f,
            MsaaLevel = 2,
            AnisotropicFiltering = 2,
            ScreenShakeIntensity = 0.8f,
            ParticleCountMultiplier = 0.7f,
            AudioQuality = 0.9f,
            VsyncEnabled = true
        };

        // Performance Preset
        _qualityPresets["Performance"] = new PerformancePreset
        {
            Name = "Performance",
            TargetFps = 30,
            MaxParticles = 50,
            TextureQuality = 0.6f,
            ShadowQuality = 0.5f,
            MsaaLevel = 0,
            AnisotropicFiltering = 1,
            ScreenShakeIntensity = 0.6f,
            ParticleCountMultiplier = 0.5f,
            AudioQuality = 0.8f,
            VsyncEnabled = false
        };

        // Mobile Performance Preset
        _qualityPresets["Mobile"] = new PerformancePreset
        {
            Name = "Mobile Performance",
            TargetFps = 30,
            MaxParticles = 25,
            TextureQuality = 0.5f,
            ShadowQuality = 0.3f,
            MsaaLevel = 0,
            AnisotropicFiltering = 0,
            ScreenShakeIntensity = 0.5f,
            ParticleCountMultiplier = 0.3f,
            AudioQuality = 0.7f,
            VsyncEnabled = false
        };
    }

    /// <summary>
    /// Initialize memory tracking
    /// </summary>
    private void InitializeMemoryTracking()
    {
        _memoryTimer = new System.Timers.Timer(5000); // Check every 5 seconds
        _memoryTimer.Elapsed += OnMemoryCheckTimer;
        _memoryTimer.Start();
    }

    /// <summary>
    /// Create performance HUD
    /// </summary>
    private void CreatePerformanceHud()
    {
        _hudLayer = new CanvasLayer();
        AddChild(_hudLayer);
        
        _hudPanel = new PanelContainer();
        _hudPanel.Name = "PerformanceHUD";
        _hudPanel.Visible = _hudVisible;
        _hudPanel.AnchorLeft = 0;
        _hudPanel.AnchorTop = 0;
        _hudPanel.AnchorRight = 0;
        _hudPanel.AnchorBottom = 0;
        _hudPanel.OffsetLeft = 10;
        _hudPanel.OffsetTop = 10;
        _hudPanel.OffsetRight = 350;
        _hudPanel.OffsetBottom = 200;
        
        var vbox = new VBoxContainer();
        vbox.SizeFlagsHorizontal = Control.SizeFlags.Fill;
        vbox.SizeFlagsVertical = Control.SizeFlags.Fill;
        
        var title = new Label();
        title.Text = "Performance Monitor";
        title.AddThemeColorOverride("font_color", Color.Yellow);
        vbox.AddChild(title);
        
        _hudPanel.AddChild(vbox);
        _hudLayer.AddChild(_hudPanel);
    }

    public override void _Process(float delta)
    {
        UpdatePerformanceMetrics(delta);
        
        if (Input.IsActionJustPressed("toggle_performance_hud"))
        {
            TogglePerformanceHud();
        }
    }

    /// <summary>
    /// Update performance metrics
    /// </summary>
    private void UpdatePerformanceMetrics(float delta)
    {
        _frameAccumulator += delta;
        _frameCount++;
        
        // Update every second
        if (_frameAccumulator >= 1.0f)
        {
            UpdateCurrentMetrics();
            UpdateHudDisplay();
            _frameAccumulator = 0f;
            _frameCount = 0;
        }
        
        // Store frame data
        StoreFrameData(delta);
    }

    /// <summary>
    /// Update current performance metrics
    /// </summary>
    private void UpdateCurrentMetrics()
    {
        var engine = Engine.GetSingleton("Engine");
        
        // FPS calculation
        float fps = 1.0f / (_frameAccumulator / Mathf.Max(_frameCount, 1));
        _currentMetrics.Fps = fps;
        _currentMetrics.AverageFps = CalculateAverageFps();
        
        // Frame time
        _currentMetrics.FrameTime = _frameAccumulator / Mathf.Max(_frameCount, 1);
        _currentMetrics.MinFrameTime = Mathf.Min(_currentMetrics.MinFrameTime, _currentMetrics.FrameTime);
        _currentMetrics.MaxFrameTime = Mathf.Max(_currentMetrics.MaxFrameTime, _currentMetrics.FrameTime);
        
        // Memory usage
        UpdateMemoryMetrics();
        
        // Active objects
        _currentMetrics.ActiveNodes = GetTree().GetNodesInGroup("").Count;
        _currentMetrics.ActivePhysicsBodies = GetTree().GetNodesInGroup("PhysicsBody2D").Count;
        
        // Performance warnings
        CheckPerformanceWarnings();
        
        EmitSignal("PerformanceMetricsUpdated", _currentMetrics);
    }

    /// <summary>
    /// Update memory usage metrics
    /// </summary>
    private void UpdateMemoryMetrics()
    {
        try
        {
            // Note: Godot doesn't expose direct memory usage APIs
            // This would need to be implemented using platform-specific APIs
            // For now, we'll use estimated values based on frame timing
            
            if (OS.GetName() == "Windows" || OS.GetName() == "Linux" || OS.GetName() == "macOS")
            {
                // Desktop memory estimation
                _currentMetrics.MemoryUsage = EstimateDesktopMemory();
            }
            else
            {
                // Mobile memory estimation
                _currentMetrics.MemoryUsage = EstimateMobileMemory();
            }
        }
        catch (Exception e)
        {
            GD.PrintErr($"Failed to update memory metrics: {e.Message}");
        }
    }

    /// <summary>
    /// Estimate desktop memory usage
    /// </summary>
    private float EstimateDesktopMemory()
    {
        // Simplified estimation based on active objects and frame time
        float baseMemory = 50f; // Base 50MB
        float objectMemory = _currentMetrics.ActiveNodes * 0.01f; // ~10KB per node
        float frameMemory = _currentMetrics.FrameTime * 100f; // Frame time impact
        
        return baseMemory + objectMemory + frameMemory;
    }

    /// <summary>
    /// Estimate mobile memory usage
    /// </summary>
    private float EstimateMobileMemory()
    {
        // More conservative estimation for mobile
        float baseMemory = 30f; // Base 30MB
        float objectMemory = _currentMetrics.ActiveNodes * 0.005f; // ~5KB per node on mobile
        float frameMemory = _currentMetrics.FrameTime * 50f; // Lower impact on mobile
        
        return baseMemory + objectMemory + frameMemory;
    }

    /// <summary>
    /// Calculate average FPS over recent history
    /// </summary>
    private float CalculateAverageFps()
    {
        if (_frameHistory.Count == 0) return _currentMetrics.Fps;
        
        var recentFrames = _frameHistory.TakeLast(60); // Last 60 frames
        return recentFrames.Average(frame => frame.Fps);
    }

    /// <summary>
    /// Store frame data for analysis
    /// </summary>
    private void StoreFrameData(float delta)
    {
        var frame = new PerformanceFrame
        {
            Timestamp = DateTime.Now,
            Fps = 1.0f / delta,
            FrameTime = delta,
            MemoryUsage = _currentMetrics.MemoryUsage,
            ActiveNodes = _currentMetrics.ActiveNodes
        };
        
        _frameHistory.Add(frame);
        
        // Keep only last 1000 frames
        if (_frameHistory.Count > 1000)
        {
            _frameHistory.RemoveAt(0);
        }
    }

    /// <summary>
    /// Check for performance warnings
    /// </summary>
    private void CheckPerformanceWarnings()
    {
        // Low FPS warning
        if (_currentMetrics.Fps < 30)
        {
            EmitSignal("PerformanceWarning", "LowFPS", _currentMetrics.Fps);
        }
        
        // High memory usage warning (mobile)
        if (OS.GetName() != "Windows" && OS.GetName() != "Linux" && OS.GetName() != "macOS")
        {
            if (_currentMetrics.MemoryUsage > 500) // 500MB on mobile
            {
                EmitSignal("PerformanceWarning", "HighMemory", _currentMetrics.MemoryUsage);
            }
        }
        
        // Frame time spike warning
        if (_currentMetrics.FrameTime > 0.033f) // 33ms = 30 FPS
        {
            EmitSignal("PerformanceWarning", "FrameTimeSpike", _currentMetrics.FrameTime);
        }
    }

    /// <summary>
    /// Update HUD display
    /// </summary>
    private void UpdateHudDisplay()
    {
        if (_hudPanel == null || !_hudVisible) return;
        
        var vbox = _hudPanel.GetChild(0) as VBoxContainer;
        if (vbox == null) return;
        
        // Clear existing labels except title
        for (int i = vbox.GetChildCount() - 1; i >= 1; i--)
        {
            vbox.GetChild(i).QueueFree();
        }
        
        // Add performance metrics
        AddHudLabel(vbox, $"FPS: {_currentMetrics.Fps:F1} (avg: {_currentMetrics.AverageFps:F1})", GetFpsColor());
        AddHudLabel(vbox, $"Frame Time: {_currentMetrics.FrameTime * 1000:F1}ms", GetFrameTimeColor());
        AddHudLabel(vbox, $"Memory: {_currentMetrics.MemoryUsage:F1}MB", GetMemoryColor());
        AddHudLabel(vbox, $"Active Nodes: {_currentMetrics.ActiveNodes}", Color.White);
        AddHudLabel(vbox, $"Physics Bodies: {_currentMetrics.ActivePhysicsBodies}", Color.White);
        
        // Add quality preset info
        var currentPreset = GetCurrentQualityPreset();
        if (currentPreset != null)
        {
            AddHudLabel(vbox, $"Quality: {currentPreset.Name}", Color.Cyan);
        }
        
        // Add session info
        var sessionTime = DateTime.Now - _sessionStartTime;
        AddHudLabel(vbox, $"Session: {sessionTime.Hours:D2}:{sessionTime.Minutes:D2}:{sessionTime.Seconds:D2}", Color.Gray);
    }

    /// <summary>
    /// Add label to HUD
    /// </summary>
    private void AddHudLabel(VBoxContainer vbox, string text, Color color)
    {
        var label = new Label();
        label.Text = text;
        label.AddThemeColorOverride("font_color", color);
        vbox.AddChild(label);
    }

    /// <summary>
    /// Get FPS color based on performance
    /// </summary>
    private Color GetFpsColor()
    {
        if (_currentMetrics.Fps >= 50) return Color.Green;
        if (_currentMetrics.Fps >= 30) return Color.Yellow;
        return Color.Red;
    }

    /// <summary>
    /// Get frame time color based on performance
    /// </summary>
    private Color GetFrameTimeColor()
    {
        if (_currentMetrics.FrameTime <= 0.016f) return Color.Green; // 60 FPS
        if (_currentMetrics.FrameTime <= 0.033f) return Color.Yellow; // 30 FPS
        return Color.Red;
    }

    /// <summary>
    /// Get memory usage color
    /// </summary>
    private Color GetMemoryColor()
    {
        if (_currentMetrics.MemoryUsage < 200) return Color.Green;
        if (_currentMetrics.MemoryUsage < 400) return Color.Yellow;
        return Color.Red;
    }

    /// <summary>
    /// Toggle performance HUD visibility
    /// </summary>
    public void TogglePerformanceHud()
    {
        _hudVisible = !_hudVisible;
        if (_hudPanel != null)
        {
            _hudPanel.Visible = _hudVisible;
        }
    }

    /// <summary>
    /// Show performance HUD
    /// </summary>
    public void ShowPerformanceHud()
    {
        _hudVisible = true;
        if (_hudPanel != null)
        {
            _hudPanel.Visible = true;
        }
    }

    /// <summary>
    /// Hide performance HUD
    /// </summary>
    public void HidePerformanceHud()
    {
        _hudVisible = false;
        if (_hudPanel != null)
        {
            _hudPanel.Visible = false;
        }
    }

    /// <summary>
    /// Set performance quality preset
    /// </summary>
    public void SetQualityPreset(string presetName)
    {
        if (_qualityPresets.TryGetValue(presetName, out PerformancePreset preset))
        {
            ApplyQualityPreset(preset);
            GD.Print($"Applied quality preset: {presetName}");
        }
        else
        {
            GD.PrintErr($"Quality preset not found: {presetName}");
        }
    }

    /// <summary>
    /// Apply quality preset settings
    /// </summary>
    private void ApplyQualityPreset(PerformancePreset preset)
    {
        // Update rendering settings
        DisplayServer.WindowSetVsyncMode(preset.VsyncEnabled ? DisplayServer.VSyncMode.Enabled : DisplayServer.VSyncMode.Disabled);
        
        // Apply to game systems
        ApplyToGameFeelManager(preset);
        ApplyToEffectsManager(preset);
        ApplyToAudioManager(preset);
    }

    /// <summary>
    /// Apply preset to game feel manager
    /// </summary>
    private void ApplyToGameFeelManager(PerformancePreset preset)
    {
        // This would integrate with the actual GameFeelManager
        // For now, we'll log the intended changes
        GD.Print($"Would apply to GameFeelManager: Screen shake intensity {preset.ScreenShakeIntensity}");
    }

    /// <summary>
    /// Apply preset to effects manager
    /// </summary>
    private void ApplyToEffectsManager(PerformancePreset preset)
    {
        // This would integrate with the actual EffectsManager
        GD.Print($"Would apply to EffectsManager: Max particles {preset.MaxParticles}, multiplier {preset.ParticleCountMultiplier}");
    }

    /// <summary>
    /// Apply preset to audio manager
    /// </summary>
    private void ApplyToAudioManager(PerformancePreset preset)
    {
        // This would integrate with the actual AudioManager
        GD.Print($"Would apply to AudioManager: Quality {preset.AudioQuality}");
    }

    /// <summary>
    /// Get current quality preset
    /// </summary>
    private PerformancePreset GetCurrentQualityPreset()
    {
        // Simple heuristic based on current performance
        if (_currentMetrics.Fps >= 55) return _qualityPresets["High"];
        if (_currentMetrics.Fps >= 35) return _qualityPresets["Balanced"];
        if (_currentMetrics.MemoryUsage < 300) return _qualityPresets["Performance"];
        return _qualityPresets["Mobile"];
    }

    /// <summary>
    /// Auto-adjust quality based on performance
    /// </summary>
    public void AutoAdjustQuality()
    {
        var currentPreset = GetCurrentQualityPreset();
        var targetPreset = DetermineOptimalPreset();
        
        if (targetPreset != currentPreset)
        {
            ApplyQualityPreset(targetPreset);
            GD.Print($"Auto-adjusted quality from {currentPreset.Name} to {targetPreset.Name}");
        }
    }

    /// <summary>
    /// Determine optimal quality preset based on current performance
    /// </summary>
    private PerformancePreset DetermineOptimalPreset()
    {
        // Mobile device detection
        bool isMobile = OS.GetName() != "Windows" && OS.GetName() != "Linux" && OS.GetName() != "macOS";
        
        if (isMobile)
        {
            if (_currentMetrics.MemoryUsage > 400) return _qualityPresets["Mobile"];
            if (_currentMetrics.Fps < 25) return _qualityPresets["Mobile"];
        }
        
        // Desktop quality determination
        if (_currentMetrics.Fps >= 50 && _currentMetrics.MemoryUsage < 300)
            return _qualityPresets["High"];
        
        if (_currentMetrics.Fps >= 35 && _currentMetrics.MemoryUsage < 400)
            return _qualityPresets["Balanced"];
        
        if (_currentMetrics.Fps >= 25 && _currentMetrics.MemoryUsage < 500)
            return _qualityPresets["Performance"];
        
        return _qualityPresets["Mobile"];
    }

    /// <summary>
    /// Memory check timer callback
    /// </summary>
    private void OnMemoryCheckTimer(object sender, System.Timers.ElapsedEventArgs e)
    {
        var snapshot = new MemorySnapshot
        {
            Timestamp = DateTime.Now,
            MemoryUsage = _currentMetrics.MemoryUsage,
            ActiveNodes = _currentMetrics.ActiveNodes
        };
        
        _memoryHistory.Add(snapshot);
        
        // Keep only last 100 snapshots (about 8 minutes at 5-second intervals)
        if (_memoryHistory.Count > 100)
        {
            _memoryHistory.RemoveAt(0);
        }
    }

    /// <summary>
    /// Generate performance report
    /// </summary>
    public PerformanceReport GeneratePerformanceReport()
    {
        var report = new PerformanceReport
        {
            GeneratedAt = DateTime.Now,
            SessionDuration = DateTime.Now - _sessionStartTime,
            CurrentMetrics = _currentMetrics
        };
        
        if (_frameHistory.Count > 0)
        {
            report.AverageFps = _frameHistory.Average(f => f.Fps);
            report.MinFps = _frameHistory.Min(f => f.Fps);
            report.MaxFps = _frameHistory.Max(f => f.Fps);
            report.AverageFrameTime = _frameHistory.Average(f => f.FrameTime);
            report.MinFrameTime = _frameHistory.Min(f => f.FrameTime);
            report.MaxFrameTime = _frameHistory.Max(f => f.FrameTime);
        }
        
        if (_memoryHistory.Count > 0)
        {
            report.AverageMemoryUsage = _memoryHistory.Average(m => m.MemoryUsage);
            report.MaxMemoryUsage = _memoryHistory.Max(m => m.MemoryUsage);
        }
        
        return report;
    }

    /// <summary>
    /// Export performance report to file
    /// </summary>
    public void ExportPerformanceReport(string filePath)
    {
        var report = GeneratePerformanceReport();
        
        try
        {
            var options = new JsonSerializerOptions 
            { 
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            
            string json = JsonSerializer.Serialize(report, options);
            File.WriteAllText(filePath, json);
            
            GD.Print($"Performance report exported: {filePath}");
        }
        catch (Exception e)
        {
            GD.PrintErr($"Failed to export performance report: {e.Message}");
        }
    }

    /// <summary>
    /// Get current performance metrics
    /// </summary>
    public PerformanceMetrics GetCurrentMetrics()
    {
        return _currentMetrics;
    }

    /// <summary>
    /// Get available quality presets
    /// </summary>
    public Dictionary<string, PerformancePreset> GetQualityPresets()
    {
        return _qualityPresets;
    }

    /// <summary>
    /// Check if HUD should be visible
    /// </summary>
    public bool ShouldShowHud()
    {
        return _showInRelease || OS.IsDebugBuild();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _memoryTimer?.Dispose();
        }
        base.Dispose(disposing);
    }
}

/// <summary>
/// Performance metrics data structure
/// </summary>
public class PerformanceMetrics
{
    public float Fps { get; set; }
    public float AverageFps { get; set; }
    public float MinFps { get; set; } = float.MaxValue;
    public float MaxFps { get; set; }
    public float FrameTime { get; set; }
    public float MinFrameTime { get; set; } = float.MaxValue;
    public float MaxFrameTime { get; set; }
    public float MemoryUsage { get; set; }
    public int ActiveNodes { get; set; }
    public int ActivePhysicsBodies { get; set; }
    public int GcPauseCount { get; set; }
    public float GcPauseTime { get; set; }
}

/// <summary>
/// Performance frame data
/// </summary>
public class PerformanceFrame
{
    public DateTime Timestamp { get; set; }
    public float Fps { get; set; }
    public float FrameTime { get; set; }
    public float MemoryUsage { get; set; }
    public int ActiveNodes { get; set; }
}

/// <summary>
/// Memory usage snapshot
/// </summary>
public class MemorySnapshot
{
    public DateTime Timestamp { get; set; }
    public float MemoryUsage { get; set; }
    public int ActiveNodes { get; set; }
}

/// <summary>
/// Performance quality preset
/// </summary>
public class PerformancePreset
{
    public string Name { get; set; }
    public int TargetFps { get; set; }
    public int MaxParticles { get; set; }
    public float TextureQuality { get; set; }
    public float ShadowQuality { get; set; }
    public int MsaaLevel { get; set; }
    public int AnisotropicFiltering { get; set; }
    public float ScreenShakeIntensity { get; set; }
    public float ParticleCountMultiplier { get; set; }
    public float AudioQuality { get; set; }
    public bool VsyncEnabled { get; set; }
}

/// <summary>
/// Comprehensive performance report
/// </summary>
public class PerformanceReport
{
    public DateTime GeneratedAt { get; set; }
    public TimeSpan SessionDuration { get; set; }
    public PerformanceMetrics CurrentMetrics { get; set; }
    public float AverageFps { get; set; }
    public float MinFps { get; set; }
    public float MaxFps { get; set; }
    public float AverageFrameTime { get; set; }
    public float MinFrameTime { get; set; }
    public float MaxFrameTime { get; set; }
    public float AverageMemoryUsage { get; set; }
    public float MaxMemoryUsage { get; set; }
}