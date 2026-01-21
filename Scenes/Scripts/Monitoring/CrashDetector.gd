extends Node

## Comprehensive crash detection and recovery system

signal crash_detected(report: Dictionary)
signal recovery_attempted(success: bool)
signal recovery_completed(success: bool)
signal critical_crash(report: Dictionary)

static var instance: CrashDetector

# Crash tracking
var _crash_history: Array = []  # Array of CrashReport
var _last_crash_time: Dictionary
var _crash_count: int = 0
var _last_error_message: String = ""

# Recovery system
var _recovery_in_progress: bool = false
var _last_recovery_attempt: Dictionary
const MAX_CRASHES_BEFORE_NOTIFICATION: int = 3
const CRASH_TIMEWINDOW_MINUTES: int = 10

# Device information
var _device_info: Dictionary = {}

# Analytics integration
var _analytics_enabled: bool = false

func _ready():
	if instance:
		queue_free()
		return
	
	instance = self
	initialize_crash_detector()

## Initialize crash detection system
func initialize_crash_detector():
	_analytics_enabled = has_node("/root/AnalyticsEventTracker")
	collect_device_info()
	setup_global_error_handling()
	load_crash_history()
	
	print("Crash Detector initialized")

## Collect device information for crash reports
func collect_device_info():
	_device_info = {
		"device_model": OS.get_model_name(),
		"device_name": OS.get_name(),
		"processor_count": OS.get_processor_count(),
		"system_memory_mb": OS.get_static_memory_usage() / (1024 * 1024),
		"screen_width": DisplayServer.screen_get_size().x,
		"screen_height": DisplayServer.screen_get_size().y,
		"graphics_api": "Unknown",
		"godot_version": Engine.get_version_info()["string"],
		"game_version": ProjectSettings.get_setting("application/config/version", "1.0"),
		"platform": OS.get_name(),
		"architecture": OS.get_architecture_name(),
		"cpu_model": OS.get_processor_name()
	}

## Setup global error handling
func setup_global_error_handling():
	# Hook into Godot's global error handling
	ProjectSettings.set_setting("debug/settings/stdout/print_fps", true)
	
	# Note: In a real implementation, you'd need to hook into Application::set_crash_handler
	# This is a simplified version for demonstration purposes

## Load crash history from persistent storage
func load_crash_history():
	var crash_file_path = "user://crash_history.json"
	if FileAccess.file_exists(crash_file_path):
		var file = FileAccess.open(crash_file_path, FileAccess.READ)
		var json_string = file.get_as_text()
		file.close()
		
		var json = JSON.new()
		var parse_result = json.parse(json_string)
		if parse_result == OK:
			var history = json.data
			if history is Array:
				_crash_history = history.slice(max(0, history.size() - 50))  # Keep last 50 crashes
				
				print("Loaded %d crash reports" % _crash_history.size())

## Save crash history to persistent storage
func save_crash_history():
	var crash_file_path = "user://crash_history.json"
	var file = FileAccess.open(crash_file_path, FileAccess.WRITE)
	var json_string = JSON.stringify(_crash_history)
	file.store_string(json_string)
	file.close()

## Report a crash
func report_crash(error_type: String, message: String, stack_trace: String = "", additional_info: Dictionary = {}):
	var report = {
		"crash_id": generate_crash_id(),
		"timestamp": Time.get_datetime_dict_from_system(),
		"error_type": error_type,
		"message": message,
		"stack_trace": stack_trace,
		"scene_name": get_tree().current_scene.name if get_tree().current_scene else "Unknown",
		"device_info": _device_info.duplicate(),
		"additional_info": additional_info,
		"is_critical": is_critical_crash(error_type)
	}
	
	_crash_history.append(report)
	_crash_count += 1
	_last_crash_time = report["timestamp"]
	_last_error_message = message
	
	save_crash_history()
	crash_detected.emit(report)
	
	# Check if critical
	if report["is_critical"]:
		critical_crash.emit(report)
	
	# Report to Firebase
	if has_node("/root/FirebaseManager"):
		var firebase = get_node("/root/FirebaseManager")
		firebase.report_crash(error_type, message, report)
	
	# Report to analytics
	if _analytics_enabled:
		var tracker = get_node("/root/AnalyticsEventTracker")
		tracker.track_crash_detected(error_type, report["scene_name"], message)
	
	print("Crash reported: %s - %s" % [error_type, message])

## Check if crash is critical
func is_critical_crash(error_type: String) -> bool:
	var critical_types = ["segmentation_fault", "access_violation", "out_of_memory", "assertion_failure"]
	for type in critical_types:
		if error_type.to_lower() in type.to_lower():
			return true
	return false

## Get crash count in time window
func get_crash_count_in_window(minutes: int) -> int:
	var current_time = Time.get_unix_time_from_datetime_dict(Time.get_datetime_dict_from_system())
	var count = 0
	
	for report in _crash_history:
		var crash_time = Time.get_unix_time_from_datetime_dict(report["timestamp"])
		var diff = (current_time - crash_time) / 60.0  # Convert to minutes
		if diff <= minutes:
			count += 1
	
	return count

## Check for crash patterns
func check_crash_patterns():
	var recent_crashes = get_crash_count_in_window(CRASH_TIMEWINDOW_MINUTES)
	
	if recent_crashes >= MAX_CRASHES_BEFORE_NOTIFICATION:
		print("Warning: Multiple crashes detected in short time window")
		return true
	
	return false

## Attempt recovery
func attempt_recovery() -> bool:
	if _recovery_in_progress:
		print("Recovery already in progress")
		return false
	
	_recovery_in_progress = true
	_last_recovery_attempt = Time.get_datetime_dict_from_system()
	
	var success = false
	
	# Recovery strategies
	if check_crash_patterns():
		# Aggressive recovery
		success = aggressive_recovery()
	else:
		# Standard recovery
		success = standard_recovery()
	
	recovery_attempted.emit(success)
	
	if success:
		print("Recovery successful")
	else:
		print("Recovery failed - user intervention may be required")
	
	_recovery_in_progress = false
	recovery_completed.emit(success)
	
	return success

## Standard recovery
func standard_recovery() -> bool:
	# Try to reload current scene
	var current_scene = get_tree().current_scene
	if current_scene != null:
		var scene_path = current_scene.scene_file_path
		if scene_path != "":
			get_tree().change_scene_to_file(scene_path)
			return true
	
	return false

## Aggressive recovery
func aggressive_recovery() -> bool:
	# Clear cache and reload
	clear_caches()
	
	# Go to main menu
	get_tree().change_scene_to_file("res://Scenes/MainMenu.tscn")
	
	return true

## Clear caches
func clear_caches():
	# Clear texture cache
	var resources = get_tree().get_root().get_children()
	for resource in resources:
		if resource.is_queued_for_deletion():
			continue
		
		if resource.has_method("queue_free"):
			resource.queue_free()
	
	print("Caches cleared")

## Get crash statistics
func get_crash_statistics() -> Dictionary:
	if _crash_history.size() == 0:
		return {}
	
	var error_counts = {}
	var scene_counts = {}
	var total_crashes = _crash_history.size()
	
	for report in _crash_history:
		var error_type = report["error_type"]
		var scene_name = report["scene_name"]
		
		if error_counts.has(error_type):
			error_counts[error_type] += 1
		else:
			error_counts[error_type] = 1
		
		if scene_counts.has(scene_name):
			scene_counts[scene_name] += 1
		else:
			scene_counts[scene_name] = 1
	
	return {
		"total_crashes": total_crashes,
		"recent_crashes": get_crash_count_in_window(CRASH_TIMEWINDOW_MINUTES),
		"crash_types": error_counts,
		"crash_scenes": scene_counts,
		"most_common_error": get_most_common(error_counts),
		"most_problematic_scene": get_most_common(scene_counts),
		"last_crash": _crash_history[-1] if _crash_history.size() > 0 else null
	}

## Get most common item from dictionary
func get_most_common(dict: Dictionary) -> String:
	var max_count = 0
	var most_common = ""
	
	for key in dict:
		if dict[key] > max_count:
			max_count = dict[key]
			most_common = key
	
	return most_common

## Get crash history
func get_crash_history(limit: int = 50) -> Array:
	if _crash_history.size() > limit:
		return _crash_history.slice(_crash_history.size() - limit)
	return _crash_history.duplicate()

## Get crashes by type
func get_crashes_by_type(error_type: String) -> Array:
	var crashes = []
	for report in _crash_history:
		if report["error_type"] == error_type:
			crashes.append(report.duplicate())
	return crashes

## Get crashes by scene
func get_crashes_by_scene(scene_name: String) -> Array:
	var crashes = []
	for report in _crash_history:
		if report["scene_name"] == scene_name:
			crashes.append(report.duplicate())
	return crashes

## Export crash data for analysis
func export_crash_data() -> Dictionary:
	return {
		"crash_history": _crash_history,
		"statistics": get_crash_statistics(),
		"device_info": _device_info,
		"export_time": Time.get_datetime_dict_from_system()
	}

## Clear crash history
func clear_crash_history():
	_crash_history.clear()
	_crash_count = 0
	_last_crash_time.clear()
	_last_error_message = ""
	
	save_crash_history()
	print("Crash history cleared")

## Generate unique crash ID
func generate_crash_id() -> String:
	return "crash_" + str(Time.get_ticks_usec()) + "_" + str(randi())

## Check if recovery is in progress
func is_recovery_in_progress() -> bool:
	return _recovery_in_progress

## Get last crash time
func get_last_crash_time() -> Dictionary:
	return _last_crash_time.duplicate()

## Get total crash count
func get_total_crash_count() -> int:
	return _crash_count
