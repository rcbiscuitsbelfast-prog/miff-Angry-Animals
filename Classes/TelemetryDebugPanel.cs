using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// Telemetry Debug Panel - In-game dashboard for real-time metrics
/// Shows session duration, event counts, performance metrics, and Firebase status
/// Accessible from main menu in debug builds only
/// </summary>
public partial class TelemetryDebugPanel : Control
{
    public static TelemetryDebugPanel Instance { get; private set; }
    
    // UI References
    private Label _sessionDurationLabel;
    private Label _eventCountLabel;
    private Label _fpsLabel;
    private Label _memoryLabel;
    private Label _firebaseStatusLabel;
    private Label _crashCountLabel;
    private Label _lastCrashLabel;
    private Label _queuedEventsLabel;
    
    private VBoxContainer _recentEventsContainer;
    private ScrollContainer _eventsScrollContainer;
    
    private Button _closeButton;
    private Button _exportButton;
    private Button _clearButton;
    private Button _flushButton;
    
    private Panel _panel;
    private Timer _updateTimer;
    
    // Performance tracking
    private float _currentFps = 0f;
    private float _currentMemoryUsage = 0f;
    private int _frameCount = 0;
    private float _fpsTimer = 0f;
    
    // Configuration
    private bool _isVisible = false;
    private const float UPDATE_INTERVAL = 1.0f; // Update every second
    
    public override void _Ready()
    {
        // Set singleton instance
        Instance = this;
        
        SetupUI();
        ConnectSignals();
        SetupUpdateTimer();
        
        // Hide by default
        Hide();
        
        GD.Print("Telemetry Debug Panel initialized");
    }

    /// <summary>
    /// Setup UI components
    /// </summary>
    private void SetupUI()
    {
        // Create main panel
        _panel = new Panel();
        _panel.Size = new Vector2(800, 600);
        _panel.Position = new Vector2(100, 100);
        _panel.Theme = new Theme();
        
        // Add to scene
        AddChild(_panel);
        
        // Create main layout
        var mainVBox = new VBoxContainer();
        mainVBox.SizeFlagsHorizontal = Control.SizeFlags.Expand;
        mainVBox.SizeFlagsVertical = Control.SizeFlags.Expand;
        mainVBox.CustomMinimumSize = new Vector2(780, 580);
        mainVBox.Position = new Vector2(10, 10);
        _panel.AddChild(mainVBox);
        
        // Title
        var titleLabel = new Label();
        titleLabel.Text = "📊 TELEMETRY DASHBOARD";
        titleLabel.HorizontalAlignment = HorizontalAlignment.Center;
        titleLabel.AddThemeColorOverride("font_color", new Color(0.2f, 0.8f, 1.0f));
        titleLabel.AddThemeFontSizeOverride("font_size", 24);
        mainVBox.AddChild(titleLabel);
        
        // Session Info Section
        mainVBox.AddChild(CreateSectionHeader("📱 SESSION INFO"));
        
        var sessionHBox = new HBoxContainer();
        sessionHBox.CustomMinimumSize = new Vector2(760, 30);
        mainVBox.AddChild(sessionHBox);
        
        _sessionDurationLabel = CreateLabel("Session Duration: 00:00:00");
        sessionHBox.AddChild(_sessionDurationLabel);
        
        _firebaseStatusLabel = CreateLabel("Firebase: Initializing...");
        _firebaseStatusLabel.Modulate = new Color(1, 0.5f, 0); // Orange
        sessionHBox.AddChild(_firebaseStatusLabel);
        
        // Performance Section
        mainVBox.AddChild(CreateSectionHeader("⚡ PERFORMANCE"));
        
        var performanceGrid = new GridContainer();
        performanceGrid.Columns = 2;
        performanceGrid.CustomMinimumSize = new Vector2(760, 60);
        mainVBox.AddChild(performanceGrid);
        
        _fpsLabel = CreateLabel("FPS: 0");
        performanceGrid.AddChild(_fpsLabel);
        
        _memoryLabel = CreateLabel("Memory: 0 MB");
        performanceGrid.AddChild(_memoryLabel);
        
        // Analytics Section
        mainVBox.AddChild(CreateSectionHeader("📈 ANALYTICS"));
        
        var analyticsGrid = new GridContainer();
        analyticsGrid.Columns = 2;
        analyticsGrid.CustomMinimumSize = new Vector2(760, 60);
        mainVBox.AddChild(analyticsGrid);
        
        _eventCountLabel = CreateLabel("Events This Session: 0");
        analyticsGrid.AddChild(_eventCountLabel);
        
        _queuedEventsLabel = CreateLabel("Queued Events: 0");
        analyticsGrid.AddChild(_queuedEventsLabel);
        
        // Crash Reports Section
        mainVBox.AddChild(CreateSectionHeader("💥 CRASH REPORTS"));
        
        var crashHBox = new HBoxContainer();
        crashHBox.CustomMinimumSize = new Vector2(760, 30);
        mainVBox.AddChild(crashHBox);
        
        _crashCountLabel = CreateLabel("Total Crashes: 0");
        crashHBox.AddChild(_crashCountLabel);
        
        _lastCrashLabel = CreateLabel("Last Crash: None");
        _lastCrashLabel.SizeFlagsHorizontal = Control.SizeFlags.Expand;
        crashHBox.AddChild(_lastCrashLabel);
        
        // Recent Events Section
        mainVBox.AddChild(CreateSectionHeader("🗂️ RECENT EVENTS"));
        
        _eventsScrollContainer = new ScrollContainer();
        _eventsScrollContainer.CustomMinimumSize = new Vector2(760, 200);
        mainVBox.AddChild(_eventsScrollContainer);
        
        _recentEventsContainer = new VBoxContainer();
        _recentEventsContainer.SizeFlagsVertical = Control.SizeFlags.Expand;
        _eventsScrollContainer.AddChild(_recentEventsContainer);
        
        // Control Buttons
        var buttonHBox = new HBoxContainer();
        buttonHBox.CustomMinimumSize = new Vector2(760, 40);
        buttonHBox.Alignment = BoxContainer.AlignmentMode.Center;
        mainVBox.AddChild(buttonHBox);
        
        _exportButton = CreateButton("📤 Export Data", ExportData);
        buttonHBox.AddChild(_exportButton);
        
        _flushButton = CreateButton("🔄 Flush Events", FlushEvents);
        buttonHBox.AddChild(_flushButton);
        
        _clearButton = CreateButton("🗑️ Clear Data", ClearData);
        buttonHBox.AddChild(_clearButton);
        
        _closeButton = CreateButton("❌ Close", ClosePanel);
        buttonHBox.AddChild(_closeButton);
    }

    /// <summary>
    /// Create section header
    /// </summary>
    private Label CreateSectionHeader(string text)
    {
        var header = new Label();
        header.Text = text;
        header.AddThemeColorOverride("font_color", new Color(1, 1, 0.8f));
        header.AddThemeFontSizeOverride("font_size", 16);
        header.AddThemeStyleboxOverride("normal", CreateHeaderStylebox());
        return header;
    }

    /// <summary>
    /// Create styled label
    /// </summary>
    private Label CreateLabel(string text)
    {
        var label = new Label();
        label.Text = text;
        label.AddThemeFontSizeOverride("font_size", 14);
        label.SizeFlagsHorizontal = Control.SizeFlags.Fill;
        return label;
    }

    /// <summary>
    /// Create styled button
    /// </summary>
    private Button CreateButton(string text, Action callback)
    {
        var button = new Button();
        button.Text = text;
        button.CustomMinimumSize = new Vector2(120, 30);
        button.Pressed += () => callback?.Invoke();
        return button;
    }

    /// <summary>
    /// Create header stylebox
    /// </summary>
    private StyleBox CreateHeaderStylebox()
    {
        var stylebox = new StyleBoxFlat();
        stylebox.BgColor = new Color(0.2f, 0.3f, 0.5f, 0.8f);
        stylebox.CornerRadiusTopLeft = 5;
        stylebox.CornerRadiusTopRight = 5;
        stylebox.CornerRadiusBottomLeft = 5;
        stylebox.CornerRadiusBottomRight = 5;
        stylebox.PaddingLeft = 10;
        stylebox.PaddingRight = 10;
        stylebox.PaddingTop = 5;
        stylebox.PaddingBottom = 5;
        return stylebox;
    }

    /// <summary>
    /// Connect signals
    /// </summary>
    private void ConnectSignals()
    {
        // Connect Analytics Event Tracker
        if (AnalyticsEventTracker.Instance != null)
        {
            AnalyticsEventTracker.Instance.EventLogged += OnEventLogged;
        }
        
        // Connect Firebase Manager
        if (FirebaseManager.Instance != null)
        {
            FirebaseManager.Instance.EventLogged += OnFirebaseEventLogged;
            FirebaseManager.Instance.CrashReported += OnCrashReported;
        }
        
        // Connect Difficulty Heatmap Tracker
        if (DifficultyHeatmapTracker.Instance != null)
        {
            DifficultyHeatmapTracker.Instance.RageQuitDetected += OnRageQuitDetected;
        }
    }

    /// <summary>
    /// Setup update timer
    /// </summary>
    private void SetupUpdateTimer()
    {
        _updateTimer = new Timer();
        _updateTimer.WaitTime = UPDATE_INTERVAL;
        _updateTimer.Timeout += OnUpdateTimer;
        AddChild(_updateTimer);
        _updateTimer.Start();
    }

    /// <summary>
    /// Show the telemetry panel
    /// </summary>
    public void ShowPanel()
    {
        #if DEBUG
        Show();
        _isVisible = true;
        UpdateAllMetrics();
        #else
        GD.Print("Telemetry panel only available in debug builds");
        #endif
    }

    /// <summary>
    /// Hide the telemetry panel
    /// </summary>
    public void HidePanel()
    {
        Hide();
        _isVisible = false;
    }

    /// <summary>
    /// Update timer callback
    /// </summary>
    private void OnUpdateTimer()
    {
        if (!_isVisible) return;
        
        UpdatePerformanceMetrics();
        UpdateAllMetrics();
    }

    /// <summary>
    /// Update performance metrics
    /// </summary>
    private void UpdatePerformanceMetrics()
    {
        // Calculate FPS
        _frameCount++;
        _fpsTimer += GetProcessDeltaTime();
        
        if (_fpsTimer >= 1.0f)
        {
            _currentFps = _frameCount / _fpsTimer;
            _frameCount = 0;
            _fpsTimer = 0f;
        }
        
        // Get memory usage
        _currentMemoryUsage = OS.GetStaticMemoryUsage() / (1024f * 1024f); // Convert to MB
    }

    /// <summary>
    /// Update all metrics
    /// </summary>
    private void UpdateAllMetrics()
    {
        UpdateSessionInfo();
        UpdatePerformanceDisplay();
        UpdateAnalyticsDisplay();
        UpdateCrashDisplay();
    }

    /// <summary>
    /// Update session information
    /// </summary>
    private void UpdateSessionInfo()
    {
        if (AnalyticsEventTracker.Instance != null)
        {
            var sessionDuration = AnalyticsEventTracker.Instance.GetSessionDuration();
            _sessionDurationLabel.Text = $"Session Duration: {sessionDuration.Hours:D2}:{sessionDuration.Minutes:D2}:{sessionDuration.Seconds:D2}";
        }
        
        // Update Firebase status
        if (FirebaseManager.Instance != null)
        {
            bool isAvailable = FirebaseManager.Instance.IsFirebaseAvailable();
            if (isAvailable)
            {
                _firebaseStatusLabel.Text = "Firebase: ✅ Connected";
                _firebaseStatusLabel.Modulate = new Color(0.2f, 0.8f, 0.2f); // Green
            }
            else
            {
                _firebaseStatusLabel.Text = "Firebase: ❌ Unavailable";
                _firebaseStatusLabel.Modulate = new Color(0.8f, 0.2f, 0.2f); // Red
            }
        }
    }

    /// <summary>
    /// Update performance display
    /// </summary>
    private void UpdatePerformanceDisplay()
    {
        _fpsLabel.Text = $"FPS: {_currentFps:F1}";
        
        // Color code FPS
        if (_currentFps >= 50)
        {
            _fpsLabel.Modulate = new Color(0.2f, 0.8f, 0.2f); // Green
        }
        else if (_currentFps >= 30)
        {
            _fpsLabel.Modulate = new Color(1, 0.5f, 0); // Orange
        }
        else
        {
            _fpsLabel.Modulate = new Color(0.8f, 0.2f, 0.2f); // Red
        }
        
        _memoryLabel.Text = $"Memory: {_currentMemoryUsage:F1} MB";
        
        // Color code memory usage
        if (_currentMemoryUsage < 200)
        {
            _memoryLabel.Modulate = new Color(0.2f, 0.8f, 0.2f); // Green
        }
        else if (_currentMemoryUsage < 500)
        {
            _memoryLabel.Modulate = new Color(1, 0.5f, 0); // Orange
        }
        else
        {
            _memoryLabel.Modulate = new Color(0.8f, 0.2f, 0.2f); // Red
        }
    }

    /// <summary>
    /// Update analytics display
    /// </summary>
    private void UpdateAnalyticsDisplay()
    {
        if (AnalyticsEventTracker.Instance != null)
        {
            var eventCounts = AnalyticsEventTracker.Instance.GetEventCounts();
            int totalEvents = 0;
            
            foreach (var count in eventCounts.Values)
            {
                totalEvents += count;
            }
            
            _eventCountLabel.Text = $"Events This Session: {totalEvents}";
            _queuedEventsLabel.Text = $"Queued Events: {FirebaseManager.Instance?.GetQueuedEventCount() ?? 0}";
        }
    }

    /// <summary>
    /// Update crash display
    /// </summary>
    private void UpdateCrashDisplay()
    {
        // This would integrate with Firebase Crashlytics data
        // For now, show mock data
        _crashCountLabel.Text = "Total Crashes: 0";
        _lastCrashLabel.Text = "Last Crash: None";
    }

    /// <summary>
    /// Add recent event to display
    /// </summary>
    private void AddRecentEvent(string eventName, Dictionary<string, object> parameters)
    {
        var eventLabel = new Label();
        eventLabel.Text = $"[{DateTime.Now:HH:mm:ss}] {eventName}";
        eventLabel.AddThemeFontSizeOverride("font_size", 12);
        eventLabel.AddThemeColorOverride("font_color", new Color(0.8f, 0.8f, 0.8f));
        
        // Limit to last 10 events
        if (_recentEventsContainer.GetChildCount() >= 10)
        {
            _recentEventsContainer.GetChild(0).QueueFree();
        }
        
        _recentEventsContainer.AddChild(eventLabel);
        
        // Auto-scroll to bottom
        _eventsScrollContainer.VScroll = _eventsScrollContainer.VScrollBar.MaxValue;
    }

    // ===============================================
    // EVENT HANDLERS
    // ===============================================

    /// <summary>
    /// Handle analytics event logged
    /// </summary>
    private void OnEventLogged(string eventName, Dictionary<string, object> parameters)
    {
        if (_isVisible)
        {
            CallDeferred(nameof(AddRecentEvent), eventName, parameters);
        }
    }

    /// <summary>
    /// Handle Firebase event logged
    /// </summary>
    private void OnFirebaseEventLogged(string eventName, Dictionary<string, object> parameters)
    {
        if (_isVisible)
        {
            CallDeferred(nameof(AddRecentEvent), $"[Firebase] {eventName}", parameters);
        }
    }

    /// <summary>
    /// Handle crash reported
    /// </summary>
    private void OnCrashReported(string crashType, string message)
    {
        if (_isVisible)
        {
            var crashEvent = new Dictionary<string, object>
            {
                { "crash_type", crashType },
                { "message", message }
            };
            
            CallDeferred(nameof(AddRecentEvent), "💥 CRASH REPORTED", crashEvent);
        }
    }

    /// <summary>
    /// Handle rage quit detected
    /// </summary>
    private void OnRageQuitDetected(int levelNumber, int failureCount, TimeSpan timeSpan)
    {
        if (_isVisible)
        {
            var rageEvent = new Dictionary<string, object>
            {
                { "level_number", levelNumber },
                { "failure_count", failureCount },
                { "time_span", timeSpan.TotalMinutes }
            };
            
            CallDeferred(nameof(AddRecentEvent), "😡 RAGE QUIT DETECTED", rageEvent);
        }
    }

    // ===============================================
    // BUTTON CALLBACKS
    // ===============================================

    /// <summary>
    /// Export data button callback
    /// </summary>
    private void ExportData()
    {
        try
        {
            // Export analytics data
            AnalyticsEventTracker.Instance?.ExportAnalytics("user://analytics_export.json");
            
            // Export heatmap data
            DifficultyHeatmapTracker.Instance?.ExportToCsv("user://heatmap_export.csv");
            
            GD.Print("Telemetry data exported successfully");
        }
        catch (Exception e)
        {
            GD.PrintErr($"Error exporting telemetry data: {e.Message}");
        }
    }

    /// <summary>
    /// Flush events button callback
    /// </summary>
    private void FlushEvents()
    {
        try
        {
            FirebaseManager.Instance?.FlushEvents();
            AnalyticsEventTracker.Instance?.FlushEvents();
            GD.Print("Events flushed successfully");
        }
        catch (Exception e)
        {
            GD.PrintErr($"Error flushing events: {e.Message}");
        }
    }

    /// <summary>
    /// Clear data button callback
    /// </summary>
    private void ClearData()
    {
        try
        {
            // Clear analytics events
            // Note: This would need to be implemented in AnalyticsEventTracker
            
            // Clear heatmap data
            DifficultyHeatmapTracker.Instance?.ClearData();
            
            GD.Print("Telemetry data cleared successfully");
        }
        catch (Exception e)
        {
            GD.PrintErr($"Error clearing telemetry data: {e.Message}");
        }
    }

    /// <summary>
    /// Close panel button callback
    /// </summary>
    private void ClosePanel()
    {
        HidePanel();
    }

    /// <summary>
    /// Handle input for panel dragging
    /// </summary>
    public override void _GuiInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton eventButton && eventButton.ButtonIndex == MouseButton.Left && eventButton.Pressed)
        {
            // Enable dragging
            Dragging = true;
            DragOffset = eventButton.Position - _panel.Position;
        }
        else if (@event is InputEventMouseButton mouseButton && mouseButton.ButtonIndex == MouseButton.Left && !mouseButton.Pressed)
        {
            // Disable dragging
            Dragging = false;
        }
    }

    /// <summary>
    /// Handle input processing for dragging
    /// </summary>
    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseMotion mouseMotion && Dragging)
        {
            _panel.Position = mouseMotion.Position - DragOffset;
        }
    }

    // Properties for dragging
    public bool Dragging { get; set; } = false;
    public Vector2 DragOffset { get; set; }
}