extends Control

# Performance Monitor UI - Real-time performance metrics overlay
# Shows FPS, memory, CPU usage, and performance alerts

@onready var toggle_button: Button = %ToggleButton
@onready var fps_label: Label = %FPSLabel
@onready var fps_bar: ProgressBar = %FPSBar
@onready var memory_label: Label = %MemoryLabel
@onready var memory_bar: ProgressBar = %MemoryBar
@onready var cpu_label: Label = %CPULabel
@onready var cpu_bar: ProgressBar = %CPUBar
@onready var network_label: Label = %NetworkLabel
@onready var alerts_list: VBoxContainer = %AlertsList
@onready var session_label: Label = %SessionLabel

var performance_telemetry: PerformanceTelemetry
var is_minimized = false
var last_update_time = 0.0

func _ready():
	# Only show in debug builds
	if not OS.has_feature("debug"):
		queue_free()
		return
		
	performance_telemetry = PerformanceTelemetry.Instance
	if performance_telemetry == null:
		print("Performance Telemetry not found!")
		return
	
	# Connect signals
	toggle_button.pressed.connect(_on_toggle_pressed)
	performance_telemetry.performance_metric_updated.connect(_on_performance_updated)
	performance_telemetry.performance_alert_triggered.connect(_on_alert_triggered)
	
	# Connect to analytics if available
	if AnalyticsEventTracker.Instance:
		AnalyticsEventTracker.Instance.connect("analytics_updated", _on_analytics_updated)
	
	# Initial update
	_update_display()

func _on_toggle_pressed():
	is_minimized = not is_minimized
	$"VBoxContainer/MetricsContainer".visible = not is_minimized
	$"VBoxContainer/AlertsContainer".visible = not is_minimized
	$"VBoxContainer/SessionContainer".visible = not is_minimized
	
	toggle_button.text = "Maximize" if is_minimized else "Minimize"

func _on_performance_updated(metric_name: String, value: float):
	if OS.get_ticks_msec() - last_update_time < 100: # Update max 10 times per second
		return
		
	_update_display()

func _on_alert_triggered(alert: PerformanceAlert):
	var alert_label = Label.new()
	alert_label.text = "%s: %s" % [alert.AlertType.ToString(), alert.Message]
	
	# Color code by severity
	match alert.Severity:
		AlertSeverity.Low:
			alert_label.add_theme_color_override("font_color", Color.YELLOW)
		AlertSeverity.Medium:
			alert_label.add_theme_color_override("font_color", Color.ORANGE_RED)
		AlertSeverity.High:
			alert_label.add_theme_color_override("font_color", Color.RED)
	
	alerts_list.add_child(alert_label)
	
	# Remove alert after 10 seconds
	var timer = Timer.new()
	timer.one_shot = true
	timer.wait_time = 10.0
	timer.timeout.connect(func(): alert_label.queue_free())
	add_child(timer)
	timer.start()

func _on_analytics_updated(_data: Dictionary):
	_update_session_data()

func _update_display():
	if not performance_telemetry:
		return
	
	var summary = performance_telemetry.GetPerformanceSummary()
	
	# Update FPS
	var current_fps = float(summary.get("current_fps", 0))
	fps_label.text = "FPS: %.1f" % current_fps
	fps_bar.value = current_fps
	
	# Color code FPS
	if current_fps >= 50.0:
		fps_label.add_theme_color_override("font_color", Color.GREEN)
	elif current_fps >= 30.0:
		fps_label.add_theme_color_override("font_color", Color.YELLOW)
	else:
		fps_label.add_theme_color_override("font_color", Color.RED)
	
	# Update Memory
	var current_memory = float(summary.get("current_memory_mb", 0))
	var peak_memory = float(summary.get("peak_memory_mb", 0))
	memory_label.text = "Memory: %d MB (Peak: %d MB)" % [int(current_memory), int(peak_memory)]
	memory_bar.value = current_memory
	
	# Color code memory
	if current_memory < 200.0:
		memory_label.add_theme_color_override("font_color", Color.GREEN)
	elif current_memory < 400.0:
		memory_label.add_theme_color_override("font_color", Color.YELLOW)
	else:
		memory_label.add_theme_color_override("font_color", Color.RED)
	
	# Update CPU
	var cpu_usage = float(summary.get("cpu_usage", 0))
	cpu_label.text = "CPU: %.1f%%" % cpu_usage
	cpu_bar.value = cpu_usage
	
	# Color code CPU
	if cpu_usage < 50.0:
		cpu_label.add_theme_color_override("font_color", Color.GREEN)
	elif cpu_usage < 80.0:
		cpu_label.add_theme_color_override("font_color", Color.YELLOW)
	else:
		cpu_label.add_theme_color_override("font_color", Color.RED)
	
	# Update Network
	var network_kbps = float(summary.get("network_kbps", 0))
	if network_kbps > 0:
		network_label.text = "Network: %.1f KB/s" % network_kbps
		network_label.add_theme_color_override("font_color", Color.GREEN)
	else:
		network_label.text = "Network: Offline"
		network_label.add_theme_color_override("font_color", Color.RED)
	
	last_update_time = OS.get_ticks_msec()

func _update_session_data():
	if not performance_telemetry:
		return
	
	var summary = performance_telemetry.GetPerformanceSummary()
	var session_time = float(summary.get("session_time", 0))
	var levels_completed = int(summary.get("levels_completed", 0))
	var frame_drops = int(summary.get("frame_drops", 0))
	var memory_spikes = int(summary.get("memory_spikes", 0))
	var load_timeouts = int(summary.get("load_timeouts", 0))
	var active_alerts = int(summary.get("active_alerts", 0))
	
	var session_minutes = session_time / 60.0
	session_label.text = """Session: %.1f min
Levels: %d
Frame Drops: %d
Memory Spikes: %d
Load Timeouts: %d
Active Alerts: %d""" % [
		session_minutes,
		levels_completed,
		frame_drops,
		memory_spikes,
		load_timeouts,
		active_alerts
	]

func _input(event):
	if event is InputEventKey and event.pressed:
		if event.keycode == KEY_F2:
			visible = not visible
		elif event.keycode == KEY_ESCAPE and visible:
			hide()

func _process(_delta):
	# Auto-hide when not needed
	if OS.get_ticks_msec() % 1000 < 16: # Update every second
		_update_display()

func _notification(what):
	if what == NOTIFICATION_VISIBILITY_CHANGED:
		if visible:
			_update_display()

# Export performance data
func export_performance_data():
	if performance_telemetry:
		var csv_data = performance_telemetry.ExportPerformanceDataToCSV()
		_show_export_dialog(csv_data)

func _show_export_dialog(csv_data: String):
	var dialog = AcceptDialog.new()
	dialog.title = "Export Performance Data"
	dialog.size = Vector2i(600, 400)
	
	var text_edit = TextEdit.new()
	text_edit.text = csv_data
	text_edit.size_flags_vertical = Control.SIZE_EXPAND_FILL
	text_edit.readonly = true
	
	var scroll = ScrollContainer.new()
	scroll.size_flags_vertical = Control.SIZE_EXPAND_FILL
	scroll.add_child(text_edit)
	
	dialog.add_child(scroll)
	add_child(dialog)
	dialog.popup_centered()

# Get performance recommendations
func get_recommendations() -> Array:
	if performance_telemetry:
		return performance_telemetry.GetPerformanceRecommendations()
	return []

# Reset performance counters
func reset_session_counters():
	if performance_telemetry:
		performance_telemetry.ResetSessionMetrics()
	_update_display()