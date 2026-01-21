extends Node

## Comprehensive crash reporting and error tracking system

signal crash_reported(crash_report: Dictionary)
signal error_logged(error_log: Dictionary)
signal crash_threshold_reached(crash_count: int)

static var instance: CrashReporter

# Crash reporting configuration
var _config: Dictionary = {}
var _crash_log_path: String = "user://crash_reports/"
var _crash_history: Array = []

# Error tracking
var _error_counts: Dictionary = {}
var _recent_errors: Array = []

# Auto-restart functionality
var _auto_restart_enabled: bool = true
var _max_crash_count: int = 3
var _crash_count: int = 0

func _ready():
	if instance:
		queue_free()
		return
	
	instance = self
	initialize_crash_reporting()

## Initialize crash reporting system
func initialize_crash_reporting():
	load_configuration()
	setup_crash_log_directory()
	load_crash_history()
	
	print("Crash reporter initialized")

## Load crash reporting configuration
func load_configuration():
	_config = {
		"enabled": true,
		"auto_restart_enabled": _auto_restart_enabled,
		"max_crash_count": _max_crash_count,
		"save_crash_logs": true,
		"send_crash_reports": false,  # Would be true in production
		"crash_log_retention": 30,  # days
		"error_log_retention": 7,  # days
		"auto_save_interval": 60.0,  # seconds
		"enable_detailed_logging": true,
		"track_memory_leaks": true,
		"track_performance_issues": true
	}

## Setup crash log directory
func setup_crash_log_directory():
	var dir = DirAccess.open("user://")
	if not dir.dir_exists("crash_reports"):
		dir.make_dir("crash_reports")

## Load crash history from file
func load_crash_history():
	var history_path = _crash_log_path + "crash_history.json"
	
	if FileAccess.file_exists(history_path):
		var file = FileAccess.open(history_path, FileAccess.READ)
		var json_string = file.get_as_text()
		file.close()
		
		var json = JSON.new()
		var parse_result = json.parse(json_string)
		if parse_result == OK:
			var crash_data = json.data
			if crash_data is Dictionary and crash_data.has("reports"):
				_crash_history = crash_data["reports"]
				
				# Count recent crashes (last 24 hours)
				var current_time = Time.get_unix_time_from_datetime_dict(Time.get_datetime_dict_from_system())
				var recent_crashes = []
				
				for report in _crash_history:
					var report_time = Time.get_unix_time_from_datetime_dict(report["timestamp"])
					if current_time - report_time <= 86400:  # 24 hours
						recent_crashes.append(report)
				
				_crash_count = recent_crashes.size()
				
				if _crash_count >= _max_crash_count:
					crash_threshold_reached.emit(_crash_count)

## Report a crash
func report_crash(crash_type: String, message: String, stack_trace: String = "", additional_data: Dictionary = {}):
	if not _config["enabled"]:
		return
	
	var crash_report = {
		"crash_id": generate_crash_id(),
		"timestamp": Time.get_datetime_dict_from_system(),
		"crash_type": crash_type,
		"message": message,
		"stack_trace": stack_trace,
		"scene_name": get_tree().current_scene.name if get_tree().current_scene else "Unknown",
		"device_info": collect_device_info(),
		"additional_data": additional_data,
		"is_fatal": is_fatal_crash(crash_type),
		"memory_snapshot": collect_memory_info(),
		"performance_info": collect_performance_info()
	}
	
	_crash_history.append(crash_report)
	_crash_count += 1
	
	# Save crash report
	save_crash_report(crash_report)
	save_crash_history()
	
	crash_reported.emit(crash_report)
	
	# Check threshold
	if _crash_count >= _max_crash_count:
		crash_threshold_reached.emit(_crash_count)
	
	print("Crash reported: %s - %s" % [crash_type, message])

## Log an error
func log_error(error_level: String, message: String, source: String = ""):
	var error_log = {
		"error_id": generate_error_id(),
		"timestamp": Time.get_datetime_dict_from_system(),
		"level": error_level,
		"message": message,
		"source": source,
		"scene_name": get_tree().current_scene.name if get_tree().current_scene else "Unknown"
	}
	
	_recent_errors.append(error_log)
	
	# Count errors
	if _error_counts.has(message):
		_error_counts[message] += 1
	else:
		_error_counts[message] = 1
	
	# Limit recent errors
	if _recent_errors.size() > 100:
		_recent_errors.pop_front()
	
	error_logged.emit(error_log)

## Check if crash is fatal
func is_fatal_crash(crash_type: String) -> bool:
	var fatal_types = ["segmentation_fault", "access_violation", "out_of_memory", "assertion_failure"]
	for type in fatal_types:
		if crash_type.to_lower() in type.to_lower():
			return true
	return false

## Collect device information
func collect_device_info() -> Dictionary:
	return {
		"device_model": OS.get_model_name(),
		"device_name": OS.get_name(),
		"processor_count": OS.get_processor_count(),
		"system_memory_mb": OS.get_static_memory_usage() / (1024 * 1024),
		"screen_width": DisplayServer.screen_get_size().x,
		"screen_height": DisplayServer.screen_get_size().y,
		"godot_version": Engine.get_version_info()["string"],
		"game_version": ProjectSettings.get_setting("application/config/version", "1.0"),
		"platform": OS.get_name()
	}

## Collect memory information
func collect_memory_info() -> Dictionary:
	return {
		"static_memory_mb": OS.get_static_memory_usage() / (1024 * 1024),
		"dynamic_memory_mb": OS.get_dynamic_memory_usage() / (1024 * 1024),
		"peak_memory_mb": OS.get_static_memory_peak_usage() / (1024 * 1024)
	}

## Collect performance information
func collect_performance_info() -> Dictionary:
	return {
		"fps": Engine.get_frames_per_second(),
		"object_count": Performance.get_monitor(Performance.OBJECT_COUNT),
		"physics_objects": Performance.get_monitor(Performance.PHYSICS_3D_ACTIVE_OBJECTS),
		"draw_calls": Performance.get_monitor(Performance.RENDER_DRAW_CALLS_IN_FRAME)
	}

## Save crash report
func save_crash_report(crash_report: Dictionary):
	var file_name = "crash_" + crash_report["crash_id"] + ".json"
	var file_path = _crash_log_path + file_name
	
	var file = FileAccess.open(file_path, FileAccess.WRITE)
	var json_string = JSON.stringify(crash_report)
	file.store_string(json_string)
	file.close()

## Save crash history
func save_crash_history():
	var history_path = _crash_log_path + "crash_history.json"
	var data = {
		"reports": _crash_history.slice(max(0, _crash_history.size() - 100)),  # Keep last 100
		"last_updated": Time.get_datetime_dict_from_system()
	}
	
	var file = FileAccess.open(history_path, FileAccess.WRITE)
	var json_string = JSON.stringify(data)
	file.store_string(json_string)
	file.close()

## Get crash history
func get_crash_history(limit: int = 50) -> Array:
	if _crash_history.size() > limit:
		return _crash_history.slice(_crash_history.size() - limit)
	return _crash_history.duplicate()

## Get recent errors
func get_recent_errors(limit: int = 20) -> Array:
	if _recent_errors.size() > limit:
		return _recent_errors.slice(_recent_errors.size() - limit)
	return _recent_errors.duplicate()

## Get error statistics
func get_error_statistics() -> Dictionary:
	return {
		"total_errors": _recent_errors.size(),
		"unique_errors": _error_counts.size(),
		"error_counts": _error_counts.duplicate(),
		"most_common_error": get_most_common_error()
	}

## Get most common error
func get_most_common_error() -> String:
	var max_count = 0
	var most_common = ""
	
	for error in _error_counts:
		if _error_counts[error] > max_count:
			max_count = _error_counts[error]
			most_common = error
	
	return most_common

## Generate crash ID
func generate_crash_id() -> String:
	return "crash_" + str(Time.get_ticks_usec()) + "_" + str(randi())

## Generate error ID
func generate_error_id() -> String:
	return "error_" + str(Time.get_ticks_usec()) + "_" + str(randi())

## Clear crash history
func clear_crash_history():
	_crash_history.clear()
	_crash_count = 0
	_error_counts.clear()
	_recent_errors.clear()
	
	save_crash_history()
	print("Crash history cleared")

## Export crash data
func export_crash_data() -> Dictionary:
	return {
		"crash_history": _crash_history,
		"recent_errors": _recent_errors,
		"error_statistics": get_error_statistics(),
		"export_time": Time.get_datetime_dict_from_system()
	}
