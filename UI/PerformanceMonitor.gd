extends Control

/// <summary>
/// Performance Monitor UI - shows real-time performance metrics overlay
/// Only visible in debug builds with toggleable interface
/// </summary>

# Performance telemetry reference
var _performanceTelemetry
var _currentMetrics = {}

# UI References
@onready var fps_label = $VBoxContainer/MetricsContainer/FPSContainer/FPSLabel
@onready var fps_bar = $VBoxContainer/MetricsContainer/FPSContainer/FPSBar
@onready var memory_label = $VBoxContainer/MetricsContainer/MemoryContainer/MemoryLabel
@onready var memory_bar = $VBoxContainer/MetricsContainer/MemoryContainer/MemoryBar
@onready var cpu_label = $VBoxContainer/MetricsContainer/CPUContainer/CPULabel
@onready var cpu_bar = $VBoxContainer/MetricsContainer/CPUContainer/CPUBar
@onready var network_label = $VBoxContainer/MetricsContainer/NetworkContainer/NetworkLabel
@onready var alerts_container = $VBoxContainer/AlertsContainer/AlertsList
@onready var session_label = $VBoxContainer/SessionContainer/SessionLabel
@onready var toggle_button = $VBoxContainer/HeaderContainer/ToggleButton

# Performance monitor state
var _isVisible = false
var _isMinimized = false
var _updateInterval = 1.0 # seconds
var _autoHideTimer = 0.0
var _autoHideDelay = 10.0 # seconds

func _ready():
    # Initialize performance monitor
    _performanceTelemetry = PerformanceTelemetry.Instance
    if _performanceTelemetry == null:
        queue_free()
        return
    
    # Connect signals
    _performanceTelemetry.connect("PerformanceMetricUpdated", _on_performance_metric_updated)
    _performanceTelemetry.connect("PerformanceAlertTriggered", _on_performance_alert_triggered)
    
    # Setup UI
    _setup_ui()
    _update_performance_display()
    
    # Start update timer
    _start_update_timer()
    
    GD.Print("Performance Monitor UI initialized")

func _setup_ui():
    # Configure monitor appearance
    visible = _should_show_monitor()
    
    # Setup button connections
    toggle_button.pressed.connect(_on_toggle_pressed)
    
    # Style the monitor
    _style_monitor()

func _style_monitor():
    # Set up colors and styling for performance indicators
    # FPS colors: Green > 30, Yellow 20-30, Red < 20
    # Memory colors: Green < 300MB, Yellow 300-500MB, Red > 500MB
    # CPU colors: Green < 50%, Yellow 50-80%, Red > 80%
    pass

func _should_show_monitor() -> bool:
    # Only show in debug builds or when explicitly enabled
    return OS.IsDebugBuild() || ProjectSettings.GetSetting("debug/show_performance_monitor", false)

func _start_update_timer():
    # Start timer for periodic updates
    var timer = Timer.new()
    timer.wait_time = _updateInterval
    timer.timeout.connect(_update_performance_display)
    add_child(timer)
    timer.start()

func _update_performance_display():
    # Get current performance metrics
    _currentMetrics = _performanceTelemetry.GetPerformanceSummary()
    
    # Update FPS display
    _update_fps_display()
    
    # Update memory display
    _update_memory_display()
    
    # Update CPU display
    _update_cpu_display()
    
    # Update network display
    _update_network_display()
    
    # Update session metrics
    _update_session_display()
    
    # Update alerts
    _update_alerts_display()

func _update_fps_display():
    var current_fps = _currentMetrics.get("current_fps", 0.0)
    var average_fps = _currentMetrics.get("average_fps", 0.0)
    
    fps_label.text = "FPS: %.1f (avg: %.1f)" % [current_fps, average_fps]
    
    # Set FPS bar color based on performance
    fps_bar.value = current_fps
    fps_bar.max_value = 60.0
    
    if current_fps >= 30.0:
        fps_bar.add_theme_color_override("theme_override_styles/fill", _create_performance_style(Color.GREEN))
        fps_label.add_theme_color_override("font_color", Color.GREEN)
    elif current_fps >= 20.0:
        fps_bar.add_theme_color_override("theme_override_styles/fill", _create_performance_style(Color.YELLOW))
        fps_label.add_theme_color_override("font_color", Color.YELLOW)
    else:
        fps_bar.add_theme_color_override("theme_override_styles/fill", _create_performance_style(Color.RED))
        fps_label.add_theme_color_override("font_color", Color.RED)

func _update_memory_display():
    var current_memory = _currentMetrics.get("current_memory_mb", 0)
    var peak_memory = _currentMetrics.get("peak_memory_mb", 0)
    
    memory_label.text = "Memory: %d MB (peak: %d MB)" % [current_memory, peak_memory]
    
    # Set memory bar color based on usage
    memory_bar.value = current_memory
    memory_bar.max_value = 800.0
    
    if current_memory < 300:
        memory_bar.add_theme_color_override("theme_override_styles/fill", _create_performance_style(Color.GREEN))
        memory_label.add_theme_color_override("font_color", Color.GREEN)
    elif current_memory < 500:
        memory_bar.add_theme_color_override("theme_override_styles/fill", _create_performance_style(Color.YELLOW))
        memory_label.add_theme_color_override("font_color", Color.YELLOW)
    else:
        memory_bar.add_theme_color_override("theme_override_styles/fill", _create_performance_style(Color.RED))
        memory_label.add_theme_color_override("font_color", Color.RED)

func _update_cpu_display():
    var cpu_usage = _currentMetrics.get("cpu_usage", 0.0)
    
    cpu_label.text = "CPU: %.1f%%" % cpu_usage
    
    # Set CPU bar color based on usage
    cpu_bar.value = cpu_usage
    cpu_bar.max_value = 100.0
    
    if cpu_usage < 50.0:
        cpu_bar.add_theme_color_override("theme_override_styles/fill", _create_performance_style(Color.GREEN))
        cpu_label.add_theme_color_override("font_color", Color.GREEN)
    elif cpu_usage < 80.0:
        cpu_bar.add_theme_color_override("theme_override_styles/fill", _create_performance_style(Color.YELLOW))
        cpu_label.add_theme_color_override("font_color", Color.YELLOW)
    else:
        cpu_bar.add_theme_color_override("theme_override_styles/fill", _create_performance_style(Color.RED))
        cpu_label.add_theme_color_override("font_color", Color.RED)

func _update_network_display():
    var network_kbps = _currentMetrics.get("network_kbps", 0.0)
    var is_connected = network_kbps > 0
    
    if is_connected:
        network_label.text = "Network: %.1f KB/s" % network_kbps
        network_label.add_theme_color_override("font_color", Color.GREEN)
    else:
        network_label.text = "Network: Offline"
        network_label.add_theme_color_override("font_color", Color.RED)

func _update_session_display():
    var session_time = _currentMetrics.get("session_time", 0.0)
    var levels_completed = _currentMetrics.get("levels_completed", 0)
    var frame_drops = _currentMetrics.get("frame_drops", 0)
    var memory_spikes = _currentMetrics.get("memory_spikes", 0)
    
    session_label.text = """Session: %.1f min
Levels: %d
Frame Drops: %d
Memory Spikes: %d""" % [
        session_time / 60.0,
        levels_completed,
        frame_drops,
        memory_spikes
    ]

func _update_alerts_display():
    # Clear existing alerts
    for child in alerts_container.get_children():
        child.queue_free()
    
    # Add current alerts
    var active_alerts = _currentMetrics.get("active_alerts", 0)
    if active_alerts > 0:
        var alert_label = Label.new()
        alert_label.text = "⚠️ %d Active Alerts" % active_alerts
        alert_label.add_theme_color_override("font_color", Color.ORANGE)
        alert_label.add_theme_font_size_override("font_size", 12)
        alerts_container.add_child(alert_label)
    else:
        var ok_label = Label.new()
        ok_label.text = "✅ No Alerts"
        ok_label.add_theme_color_override("font_color", Color.GREEN)
        ok_label.add_theme_font_size_override("font_size", 12)
        alerts_container.add_child(ok_label)

func _create_performance_style(color: Color) -> StyleBoxFlat:
    var style = StyleBoxFlat.new()
    style.bg_color = color
    style.corner_radius_top_left = 3
    style.corner_radius_top_right = 3
    style.corner_radius_bottom_left = 3
    style.corner_radius_bottom_right = 3
    return style

# Signal handlers
func _on_performance_metric_updated(metric_name: String, value: float):
    # Update specific metric display when signal is received
    match metric_name:
        "fps":
            _update_fps_display()
        "memory_mb":
            _update_memory_display()
        "cpu_usage":
            _update_cpu_display()

func _on_performance_alert_triggered(alert):
    # Add new alert to display
    var alert_label = Label.new()
    alert_label.text = "⚠️ %s: %s" % [alert.AlertType, alert.Message]
    alert_label.add_theme_color_override("font_color", _get_alert_color(alert.Severity))
    alert_label.add_theme_font_size_override("font_size", 10)
    
    alerts_container.add_child(alert_label)
    
    # Auto-remove old alerts
    var timer = Timer.new()
    timer.wait_time = 5.0
    timer.timeout.connect(func(): alert_label.queue_free())
    add_child(timer)
    timer.start()

func _get_alert_color(severity) -> Color:
    match severity:
        AlertSeverity.Low:
            return Color.BLUE
        AlertSeverity.Medium:
            return Color.YELLOW
        AlertSeverity.High:
            return Color.ORANGE
        AlertSeverity.Critical:
            return Color.RED
        _:
            return Color.WHITE

# UI Event handlers
func _on_toggle_pressed():
    _isMinimized = !_isMinimized
    $VBoxContainer/MetricsContainer.visible = !_isMinimized
    $VBoxContainer/AlertsContainer.visible = !_isMinimized
    $VBoxContainer/SessionContainer.visible = !_isMinimized
    
    toggle_button.text = "Maximize" if _isMinimized else "Minimize"

func toggle_visibility():
    _isVisible = !_isVisible
    visible = _isVisible and _should_show_monitor()

func _input(event):
    if event.is_action_pressed("toggle_performance_monitor"):
        toggle_visibility()

func _process(delta):
    # Auto-hide functionality
    if visible:
        _autoHideTimer += delta
        if _autoHideTimer > _autoHideDelay:
            visible = false
            _isVisible = false
    else:
        _autoHideTimer = 0.0

func _exit_tree():
    # Clean up resources
    pass