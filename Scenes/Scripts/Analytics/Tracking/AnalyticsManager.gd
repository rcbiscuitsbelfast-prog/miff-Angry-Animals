extends Node

## Analytics framework for tracking gameplay telemetry
## Collects usage data, performance metrics, and user behavior analytics

signal analytics_event_logged(analytics_event: Dictionary)
signal analytics_consent_changed(consented: bool)
signal analytics_data_uploaded(event_count: int)

static var instance: AnalyticsManager

# Analytics data storage
var _analytics_data: Dictionary = {}
var _event_queue: Array = []
var _analytics_file_path: String = "user://analytics_data.json"

# Analytics configuration
var _config: Dictionary = {}
var _is_enabled: bool = true
var _user_consent: bool = false

# Event batching
var _batch_timer: Timer
const BATCH_SIZE: int = 10
const BATCH_INTERVAL: float = 30.0  # seconds

func _ready():
	if instance:
		queue_free()
		return
	
	instance = self
	initialize_analytics()

## Initialize analytics system
func initialize_analytics():
	load_analytics_data()
	load_configuration()
	initialize_batch_timer()
	
	# Check consent status
	_user_consent = check_user_consent()
	
	if _user_consent and _is_enabled:
		start_analytics()
	
	print("Analytics system initialized")

## Load analytics data from file
func load_analytics_data():
	if FileAccess.file_exists(_analytics_file_path):
		var file = FileAccess.open(_analytics_file_path, FileAccess.READ)
		var json_text = file.get_as_text()
		file.close()
		
		var json = JSON.new()
		var parse_result = json.parse(json_text)
		if parse_result == OK:
			_analytics_data = json.data
		else:
			print("Failed to parse analytics data: %s" % json.get_error_message())
	else:
		_analytics_data = create_default_analytics_data()

## Create default analytics data structure
func create_default_analytics_data() -> Dictionary:
	return {
		"user_id": generate_user_id(),
		"first_session_date": Time.get_datetime_dict_from_system(),
		"total_sessions": 0,
		"total_play_time": 0,
		"sessions": [],
		"events": [],
		"gameplay_metrics": {},
		"device_info": collect_device_info()
	}

## Load analytics configuration
func load_configuration():
	_config = {
		"enabled": true,
		"batch_size": BATCH_SIZE,
		"batch_interval": BATCH_INTERVAL,
		"track_sessions": true,
		"track_gameplay": true,
		"track_performance": true,
		"track_crashes": true,
		"privacy_compliant": true,
		"retention_days": 90,
		"data_types": ["sessions", "level_progress", "feature_usage", "performance", "crashes", "user_preferences"]
	}

## Initialize batch timer for event processing
func initialize_batch_timer():
	_batch_timer = Timer.new()
	_batch_timer.wait_time = _config.batch_interval
	_batch_timer.autostart = true
	_batch_timer.timeout.connect(_on_batch_timer)
	add_child(_batch_timer)

## Start analytics tracking
func start_analytics():
	if not _user_consent or not _is_enabled:
		print("Analytics disabled due to user consent or configuration")
		return
	
	_analytics_data["total_sessions"] += 1
	_analytics_data["current_session_start"] = Time.get_datetime_dict_from_system()
	
	log_event("session_start", {
		"session_id": _analytics_data["total_sessions"],
		"timestamp": Time.get_datetime_dict_from_system()
	})
	
	# Track session start
	track_session_start()
	
	print("Analytics tracking started")

## Stop analytics tracking
func stop_analytics():
	if _analytics_data.has("current_session_start"):
		var session_start = _analytics_data["current_session_start"]
		var current_time = Time.get_datetime_dict_from_system()
		var session_duration = calculate_duration(session_start, current_time)
		
		_analytics_data["total_play_time"] += session_duration
		
		log_event("session_end", {
			"session_id": _analytics_data["total_sessions"],
			"duration": session_duration,
			"timestamp": Time.get_datetime_dict_from_system()
		})
		
		# Save session data
		save_session_data(session_duration)
	
	# Process remaining events
	process_event_batch()
	
	print("Analytics tracking stopped")

## Log analytics event
func log_event(event_name: String, properties: Dictionary = {}):
	if not _user_consent or not _is_enabled:
		return
	
	var analytics_event = {
		"event_id": generate_event_id(),
		"event_name": event_name,
		"timestamp": Time.get_datetime_dict_from_system(),
		"user_id": _analytics_data["user_id"],
		"session_id": _analytics_data["total_sessions"],
		"properties": properties if properties.size() > 0 else {},
		"device_info": _analytics_data["device_info"]
	}
	
	_event_queue.append(analytics_event)
	
	# Process special events immediately
	if event_name == "crash" or event_name == "level_completed":
		process_event_batch()
	
	analytics_event_logged.emit(analytics_event)
	
	# Auto-save if queue is getting large
	if _event_queue.size() >= BATCH_SIZE * 2:
		process_event_batch()

## Track level progression
func track_level_progress(level_number: int, completed: bool, attempts: int = 1, time_spent: float = 0.0):
	log_event("level_progress", {
		"level_number": level_number,
		"completed": completed,
		"attempts": attempts,
		"time_spent": time_spent,
		"difficulty": get_level_difficulty(level_number),
		"total_levels_unlocked": get_total_unlocked_levels()
	})
	
	# Update gameplay metrics
	update_gameplay_metrics(level_number, completed, attempts, time_spent)

## Track feature usage
func track_feature_usage(feature_name: String, properties: Dictionary = {}):
	var event_props = {"feature_name": feature_name}
	
	for key in properties:
		event_props[key] = properties[key]
	
	log_event("feature_usage", event_props)

## Track cosmetics usage
func track_cosmetics_usage(cosmetic_type: String, cosmetic_id: String, unlocked: bool, cost: float = 0.0):
	log_event("cosmetics_usage", {
		"cosmetic_type": cosmetic_type,
		"cosmetic_id": cosmetic_id,
		"unlocked": unlocked,
		"cost": cost,
		"currency_type": "premium" if cost > 0 else "free"
	})

## Track IAP events
func track_iap_event(product_id: String, event_type: String, amount: float = 0.0, currency: String = "USD"):
	log_event("iap_event", {
		"product_id": product_id,
		"event_type": event_type,
		"amount": amount,
		"currency": currency,
		"timestamp": Time.get_datetime_dict_from_system()
	})

## Track session start
func track_session_start():
	var session_data = {
		"session_id": _analytics_data["total_sessions"],
		"start_time": Time.get_datetime_dict_from_system(),
		"device_info": _analytics_data["device_info"]
	}
	
	if not _analytics_data.has("sessions"):
		_analytics_data["sessions"] = []
	
	_analytics_data["sessions"].append(session_data)

## Save session data
func save_session_data(duration: float):
	if _analytics_data.has("sessions") and _analytics_data["sessions"].size() > 0:
		var last_session = _analytics_data["sessions"][-1]
		last_session["end_time"] = Time.get_datetime_dict_from_system()
		last_session["duration"] = duration

## Process event batch
func process_event_batch():
	if _event_queue.size() == 0:
		return
	
	var batch_size = min(_config.batch_size, _event_queue.size())
	var batch = []
	
	for i in range(batch_size):
		batch.append(_event_queue.pop_front())
	
	# Add to analytics data
	if not _analytics_data.has("events"):
		_analytics_data["events"] = []
	
	for event in batch:
		_analytics_data["events"].append(event)
	
	# Save to file
	save_analytics_data()
	
	# Upload to server (placeholder)
	upload_analytics_data(batch)
	
	print("Processed %d analytics events" % batch.size())

## Batch timer callback
func _on_batch_timer():
	process_event_batch()

## Upload analytics data to server
func upload_analytics_data(events: Array):
	# In a real implementation, this would send data to your analytics server
	# For now, just emit a signal
	analytics_data_uploaded.emit(events.size())

## Save analytics data to file
func save_analytics_data():
	var file = FileAccess.open(_analytics_file_path, FileAccess.WRITE)
	var json_string = JSON.stringify(_analytics_data)
	file.store_string(json_string)
	file.close()

## Update gameplay metrics
func update_gameplay_metrics(level_number: int, completed: bool, attempts: int, time_spent: float):
	if not _analytics_data.has("gameplay_metrics"):
		_analytics_data["gameplay_metrics"] = {
			"total_attempts": 0,
			"total_completions": 0,
			"total_time_spent": 0.0,
			"levels_completed": {},
			"difficulty_distribution": {}
		}
	
	var metrics = _analytics_data["gameplay_metrics"]
	metrics["total_attempts"] += attempts
	
	if completed:
		metrics["total_completions"] += 1
		var level_key = "level_" + str(level_number)
		if not metrics["levels_completed"].has(level_key):
			metrics["levels_completed"][level_key] = 0
		metrics["levels_completed"][level_key] += 1
	
	metrics["total_time_spent"] += time_spent

## Check user consent
func check_user_consent() -> bool:
	# This would integrate with a consent system
	# For now, return true (consent granted)
	return true

## Set user consent
func set_user_consent(consented: bool):
	_user_consent = consented
	analytics_consent_changed.emit(consented)
	
	if consented and _is_enabled:
		start_analytics()
	elif not consented:
		stop_analytics()

## Enable/disable analytics
func set_enabled(enabled: bool):
	_is_enabled = enabled
	
	if not enabled:
		stop_analytics()

## Get analytics data
func get_analytics_data() -> Dictionary:
	return _analytics_data.duplicate()

## Get queued event count
func get_queued_event_count() -> int:
	return _event_queue.size()

## Get total sessions
func get_total_sessions() -> int:
	return _analytics_data.get("total_sessions", 0)

## Get total play time in seconds
func get_total_play_time() -> float:
	return _analytics_data.get("total_play_time", 0.0)

## Get retention metrics
func get_retention_metrics() -> Dictionary:
	return {
		"d1_retention": calculate_retention(1),
		"d7_retention": calculate_retention(7),
		"d30_retention": calculate_retention(30)
	}

## Calculate retention for a specific day
func calculate_retention(days: int) -> float:
	# This would calculate retention based on session data
	# For now, return a placeholder
	return 0.5

## Calculate duration between two timestamps
func calculate_duration(start: Dictionary, end: Dictionary) -> float:
	var start_time = Time.get_unix_time_from_datetime_dict(start)
	var end_time = Time.get_unix_time_from_datetime_dict(end)
	return end_time - start_time

## Get level difficulty
func get_level_difficulty(level_number: int) -> int:
	return min(5, ceil(level_number / 20.0))

## Get total unlocked levels
func get_total_unlocked_levels() -> int:
	if has_node("/root/PlayerProfile"):
		var profile = get_node("/root/PlayerProfile")
		if profile.has_method("get_max_unlocked_level"):
			return profile.get_max_unlocked_level()
	return 1

## Collect device information
func collect_device_info() -> Dictionary:
	return {
		"platform": OS.get_name(),
		"device_model": OS.get_model_name(),
		"os_version": OS.get_version(),
		"screen_resolution": OS.get_screen_size(),
		"locale": OS.get_locale_language(),
		"engine_version": Engine.get_version_info()["string"]
	}

## Generate unique user ID
func generate_user_id() -> String:
	return "user_" + str(Time.get_ticks_usec()) + "_" + str(randi())

## Generate unique event ID
func generate_event_id() -> String:
	return "event_" + str(Time.get_ticks_usec()) + "_" + str(randi())

## Export analytics data
func export_analytics_data() -> Dictionary:
	return {
		"analytics_data": _analytics_data,
		"configuration": _config,
		"export_time": Time.get_datetime_dict_from_system()
	}

## Clear all analytics data
func clear_analytics_data():
	_analytics_data = create_default_analytics_data()
	_event_queue.clear()
	save_analytics_data()
	print("Analytics data cleared")
