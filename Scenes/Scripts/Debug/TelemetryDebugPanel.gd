extends Control

## Telemetry debug panel for monitoring game performance and analytics.
## Shows real-time statistics for development and debugging.

signal export_data_requested()
signal flush_events_requested()
signal clear_data_requested()
signal panel_closed()

var _session_start_time: float
var _fps_counter: float = 0
var _frame_count: int = 0
var _current_fps: float = 0
var _session_duration_label: Label
var _firebase_status_label: Label
var _fps_label: Label
var _memory_label: Label
var _event_count_label: Label
var _queued_events_label: Label
var _crash_count_label: Label
var _last_crash_label: Label
var _events_container: VBoxContainer

func _ready():
	initialize_panel()
	_session_start_time = Time.get_unix_time_from_system()

func initialize_panel():
	# Find UI elements
	_session_duration_label = get_node_or_null("Panel/MainVBox/SessionHBox/SessionDuration") as Label
	_firebase_status_label = get_node_or_null("Panel/MainVBox/SessionHBox/FirebaseStatus") as Label
	_fps_label = get_node_or_null("Panel/MainVBox/PerformanceGrid/FPSLabel") as Label
	_memory_label = get_node_or_null("Panel/MainVBox/PerformanceGrid/MemoryLabel") as Label
	_event_count_label = get_node_or_null("Panel/MainVBox/AnalyticsGrid/EventCount") as Label
	_queued_events_label = get_node_or_null("Panel/MainVBox/AnalyticsGrid/QueuedEvents") as Label
	_crash_count_label = get_node_or_null("Panel/MainVBox/CrashHBox/CrashCount") as Label
	_last_crash_label = get_node_or_null("Panel/MainVBox/CrashHBox/LastCrash") as Label
	_events_container = get_node_or_null("Panel/MainVBox/EventsScroll/EventsContainer") as VBoxContainer
	
	# Connect button signals
	var export_button = get_node_or_null("Panel/MainVBox/ButtonHBox/ExportButton") as Button
	var flush_button = get_node_or_null("Panel/MainVBox/ButtonHBox/FlushButton") as Button
	var clear_button = get_node_or_null("Panel/MainVBox/ButtonHBox/ClearButton") as Button
	var close_button = get_node_or_null("Panel/MainVBox/ButtonHBox/CloseButton") as Button
	
	if export_button:
		export_button.pressed.connect(_on_export_pressed)
	if flush_button:
		flush_button.pressed.connect(_on_flush_pressed)
	if clear_button:
		clear_button.pressed.connect(_on_clear_pressed)
	if close_button:
		close_button.pressed.connect(_on_close_pressed)
	
	# Start updating
	_start_update_timer()

func _start_update_timer():
	# Update panel every second
	var update_timer = Timer.new()
	update_timer.wait_time = 1.0
	update_timer.timeout.connect(_update_panel)
	add_child(update_timer)
	update_timer.start()

func _process(delta):
	# Calculate FPS
	_frame_count += 1
	_fps_counter += delta
	if _fps_counter >= 1.0:
		_current_fps = _frame_count / _fps_counter
		_frame_count = 0
		_fps_counter = 0

func _update_panel():
	# Update session duration
	if _session_duration_label:
		var current_time = Time.get_unix_time_from_system()
		var session_duration = current_time - _session_start_time
		var hours = int(session_duration) / 3600
		var minutes = (int(session_duration) % 3600) / 60
		var seconds = int(session_duration) % 60
		_session_duration_label.text = "Session Duration: %02d:%02d:%02d" % [hours, minutes, seconds]
	
	# Update Firebase status
	if _firebase_status_label:
		var firebase_status = get_firebase_status()
		_firebase_status_label.text = "Firebase: %s" % firebase_status
	
	# Update FPS
	if _fps_label:
		_fps_label.text = "FPS: %d" % int(_current_fps)
	
	# Update memory usage
	if _memory_label:
		var memory_mb = OS.get_static_memory_usage() / (1024 * 1024)
		_memory_label.text = "Memory: %d MB" % memory_mb
	
	# Update analytics
	update_analytics_display()
	
	# Update crash reports
	update_crash_display()

func get_firebase_status() -> String:
	# Check Firebase status
	if FirebaseManager.instance:
		if FirebaseManager.instance.is_initialized:
			return "Connected"
		else:
			return "Initializing"
	return "Not Available"

func update_analytics_display():
	# Update analytics counters
	if _event_count_label:
		var event_count = get_events_this_session()
		_event_count_label.text = "Events This Session: %d" % event_count
	
	if _queued_events_label:
		var queued_count = get_queued_events_count()
		_queued_events_label.text = "Queued Events: %d" % queued_count

func update_crash_display():
	# Update crash information
	if _crash_count_label:
		var crash_count = get_total_crashes()
		_crash_count_label.text = "Total Crashes: %d" % crash_count
	
	if _last_crash_label:
		var last_crash = get_last_crash_info()
		_last_crash_label.text = "Last Crash: %s" % last_crash

func get_events_this_session() -> int:
	# Get number of events this session
	if AnalyticsEventTracker.instance:
		return AnalyticsEventTracker.instance.get_session_event_count()
	return 0

func get_queued_events_count() -> int:
	# Get number of queued events
	if FirebaseManager.instance:
		return FirebaseManager.instance.get_queued_events_count()
	return 0

func get_total_crashes() -> int:
	# Get total crash count
	if CrashDetector.instance:
		return CrashDetector.instance.get_total_crash_count()
	return 0

func get_last_crash_info() -> String:
	# Get last crash information
	if CrashDetector.instance:
		var last_crash = CrashDetector.instance.get_last_crash()
		if last_crash:
			return last_crash.get("message", "Unknown")
	return "None"

func add_event_to_display(event_name: String, timestamp: float):
	# Add an event to the recent events display
	if _events_container:
		var event_label = Label.new()
		event_label.text = "[%s] %s" % [format_timestamp(timestamp), event_name]
		_events_container.add_child(event_label)
		
		# Keep only last 20 events
		while _events_container.get_child_count() > 20:
			_events_container.get_child(0).queue_free()

func format_timestamp(timestamp: float) -> String:
	# Format timestamp for display
	var datetime = Time.get_datetime_dict_from_unix_time(int(timestamp))
	return "%02d:%02d:%02d" % [datetime.hour, datetime.minute, datetime.second]

func _on_export_pressed():
	print("Export data requested")
	export_data_requested.emit()
	show_message("Export started...")

func _on_flush_pressed():
	print("Flush events requested")
	flush_events_requested.emit()
	show_message("Events flushed...")

func _on_clear_pressed():
	print("Clear data requested")
	clear_data_requested.emit()
	show_message("Data cleared...")
	# Clear events display
	if _events_container:
		for child in _events_container.get_children():
			child.queue_free()

func _on_close_pressed():
	print("Telemetry panel closed")
	panel_closed.emit()
	hide()

func show_message(message: String):
	# Show a temporary message
	var message_label = Label.new()
	message_label.text = message
	message_label.add_theme_color_override("font_color", Color.Green)
	
	if _events_container:
		_events_container.add_child(message_label)
		# Remove message after 2 seconds
		var timer = Timer.new()
		timer.wait_time = 2.0
		timer.one_shot = true
		timer.timeout.connect(func(): message_label.queue_free())
		add_child(timer)
		timer.start()

func show_panel():
	# Show the telemetry panel
	visible = true
	_session_start_time = Time.get_unix_time_from_system()

func hide_panel():
	# Hide the telemetry panel
	visible = false

func log_custom_event(event_name: String, parameters: Dictionary = {}):
	# Log a custom event to the display
	add_event_to_display(event_name, Time.get_unix_time_from_system())
	print("Telemetry: %s - %s" % [event_name, str(parameters)])

func record_crash(crash_info: Dictionary):
	# Record a crash in the display
	var crash_message = crash_info.get("message", "Unknown crash")
	add_event_to_display("CRASH: %s" % crash_message, Time.get_unix_time_from_system())