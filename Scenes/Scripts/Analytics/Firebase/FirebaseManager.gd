extends Node

## Firebase Manager - Handles all Firebase operations for Angry Animals
## Provides cross-platform Firebase integration with fallback support
## Supports Analytics, Crashlytics, and Remote Config

signal firebase_initialized(success: bool)
signal event_logged(event_name: String, parameters: Dictionary)
signal crash_reported(crash_type: String, message: String)

static var instance: FirebaseManager

# Firebase components
var _analytics: FirebaseAnalyticsWrapper
var _crashlytics: FirebaseCrashlyticsWrapper
var _remote_config: FirebaseRemoteConfigWrapper

# Configuration
var _config: FirebaseConfig
var _is_initialized: bool = false
var _is_available: bool = false

# Platform detection
var _is_mobile: bool = false
var _is_editor: bool = false

# Event queue for offline support
var _event_queue: Array = []
var _flush_timer: Timer

func _ready():
	if instance:
		queue_free()
		return
	
	instance = self
	initialize_firebase()

## Initialize Firebase with platform detection
func initialize_firebase():
	detect_platform()
	load_configuration()
	
	# Try to initialize Firebase
	try_initialize_firebase()
	
	# Start event flush timer
	start_event_flush_timer()
	
	print("Firebase Manager initialized - Platform: %s, Available: %s" % [get_platform_name(), _is_available])

## Detect current platform and Firebase availability
func detect_platform():
	var platform = OS.get_name()
	
	_is_editor = Engine.is_editor_hint()
	_is_mobile = platform == "Android" || platform == "iOS"
	
	# Check for Firebase plugins
	_is_available = check_firebase_plugins()
	
	if _is_editor:
		print("Running in editor - Firebase features will be simulated")
	elif not _is_mobile:
		print("Desktop platform detected - Firebase features will be simulated")

## Check if Firebase plugins are available
func check_firebase_plugins() -> bool:
	# Try multiple possible Firebase singleton names
	var possible_names = ["Firebase", "FirebaseApp", "FirebaseAnalytics", "FirebaseCrashlytics"]
	
	for name in possible_names:
		if Engine.has_singleton(name):
			print("Found Firebase plugin: %s" % name)
			return true
	
	return false

## Load Firebase configuration
func load_configuration():
	_config = FirebaseConfig.new()
	
	# Project configuration - replace with your actual Firebase project details
	_config.project_id = "angry-animals-analytics"
	_config.api_key = "AIzaSyYourApiKeyHere"
	_config.app_id = "1:123456789:web:abcdef123456"
	
	# Feature toggles
	_config.analytics_enabled = true
	_config.crashlytics_enabled = true
	_config.remote_config_enabled = false
	
	# Performance settings
	_config.batch_size = 10
	_config.flush_interval = 30
	_config.max_queue_size = 100
	
	# Privacy settings
	_config.user_consent = check_user_consent()
	_config.data_collection_enabled = true

## Try to initialize Firebase services
func try_initialize_firebase():
	if _is_available:
		initialize_firebase_services()
	else:
		initialize_mock_firebase()
	
	_is_initialized = true
	firebase_initialized.emit(true)

## Initialize actual Firebase services
func initialize_firebase_services():
	if _config.analytics_enabled:
		_analytics = FirebaseAnalyticsWrapper.new(_config)
	
	if _config.crashlytics_enabled:
		_crashlytics = FirebaseCrashlyticsWrapper.new(_config)
	
	if _config.remote_config_enabled:
		_remote_config = FirebaseRemoteConfigWrapper.new(_config)

## Initialize mock Firebase for testing/editor
func initialize_mock_firebase():
	_analytics = FirebaseAnalyticsWrapper.new(_config, true)
	_crashlytics = FirebaseCrashlyticsWrapper.new(_config, true)
	_remote_config = FirebaseRemoteConfigWrapper.new(_config, true)
	
	print("Mock Firebase initialized - events will be logged locally")

## Start event flush timer
func start_event_flush_timer():
	_flush_timer = Timer.new()
	_flush_timer.wait_time = _config.flush_interval
	_flush_timer.autostart = true
	_flush_timer.timeout.connect(_on_flush_timer)
	add_child(_flush_timer)

## Flush event queue
func _on_flush_timer():
	flush_event_queue()

## Flush queued events
func flush_event_queue():
	while _event_queue.size() >= _config.batch_size:
		var batch = []
		
		for i in range(min(_config.batch_size, _event_queue.size())):
			batch.append(_event_queue.pop_front())
		
		process_event_batch(batch)

## Process a batch of events
func process_event_batch(events: Array):
	if _analytics != null:
		for event_data in events:
			_analytics.log_event(event_data.event_name, event_data.parameters)

## Log analytics event
func log_event(event_name: String, parameters: Dictionary = {}):
	var firebase_event = {
		"event_name": event_name,
		"parameters": parameters,
		"timestamp": Time.get_datetime_dict_from_system()
	}
	
	if _is_initialized:
		if _analytics != null:
			_analytics.log_event(event_name, parameters)
		
		event_logged.emit(event_name, parameters)
	else:
		# Queue event for later
		_event_queue.append(firebase_event)
		
		if _event_queue.size() > _config.max_queue_size:
			# Remove oldest event if queue is full
			_event_queue.pop_front()

## Report crash to Firebase Crashlytics
func report_crash(crash_type: String, message: String, additional_data: Dictionary = {}):
	if _crashlytics != null:
		_crashlytics.record_exception(crash_type, message, additional_data)
	
	# Also log as analytics event
	log_event("crash_reported", {
		"crash_type": crash_type,
		"message": message,
		"platform": get_platform_name(),
		"timestamp": Time.get_datetime_dict_from_system()
	})
	
	crash_reported.emit(crash_type, message)

## Set user property
func set_user_property(property_name: String, value: String):
	if _analytics != null:
		_analytics.set_user_property(property_name, value)
	
	# Also store locally for fallback
	set_user_property_locally(property_name, value)

## Set user ID
func set_user_id(user_id: String):
	if _analytics != null:
		_analytics.set_user_id(user_id)
	
	set_user_id_locally(user_id)

## Get remote config value
func get_remote_config_value(key: String, default_value = null):
	if _remote_config != null:
		return _remote_config.get_value(key, default_value)
	
	return default_value

## Fetch remote config
func fetch_remote_config(callback: Callable = Callable()):
	if _remote_config != null:
		_remote_config.fetch(callback)
	elif not callback.is_null():
		callback.call(false)

## Check user consent for data collection
func check_user_consent() -> bool:
	# This would integrate with PrivacyPolicyManager
	# For now, check if analytics consent was given
	return true

## Get platform name
func get_platform_name() -> String:
	if _is_editor:
		return "Editor"
	if OS.get_name() == "Android":
		return "Android"
	if OS.get_name() == "iOS":
		return "iOS"
	return OS.get_name()

## Check if Firebase is available
func is_firebase_available() -> bool:
	return _is_available and _is_initialized

## Get queued event count
func get_queued_event_count() -> int:
	return _event_queue.size()

## Get Firebase configuration
func get_firebase_config() -> FirebaseConfig:
	return _config

## Force flush all queued events
func flush_events():
	while _event_queue.size() > 0:
		flush_event_queue()

## Clear all queued events
func clear_event_queue():
	_event_queue.clear()

## Set user property locally
func set_user_property_locally(property_name: String, value: String):
	# Store in player preferences for local fallback
	var prefs = ConfigFile.new()
	prefs.set_value("firebase_user_properties", property_name, value)
	prefs.save("user://firebase_properties.cfg")

## Set user ID locally
func set_user_id_locally(user_id: String):
	# Store in player preferences for local fallback
	var prefs = ConfigFile.new()
	prefs.set_value("firebase_user_properties", "user_id", user_id)
	prefs.save("user://firebase_properties.cfg")

## Firebase configuration class
class FirebaseConfig extends RefCounted:
	var project_id: String
	var api_key: String
	var app_id: String
	var analytics_enabled: bool
	var crashlytics_enabled: bool
	var remote_config_enabled: bool
	var batch_size: int
	var flush_interval: int
	var max_queue_size: int
	var user_consent: bool
	var data_collection_enabled: bool
