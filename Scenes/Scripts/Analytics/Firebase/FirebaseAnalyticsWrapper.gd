extends RefCounted

## Firebase Analytics Wrapper - Handles all Firebase Analytics operations
## Provides a unified interface for logging events, setting user properties, and managing analytics

var _config: FirebaseManager.FirebaseConfig
var _is_mock: bool
var _user_properties: Dictionary = {}
var _user_id: String = ""

func _init(config: FirebaseManager.FirebaseConfig, is_mock: bool = false):
	_config = config
	_is_mock = is_mock
	
	if not _is_mock:
		initialize_firebase_analytics()

## Initialize Firebase Analytics
func initialize_firebase_analytics():
	var platform = OS.get_name()
	
	# Real Firebase initialization would go here
	# For Godot, this would use GodotFirebase or similar plugins
	if platform == "Android":
		print("Firebase Analytics initialized for Android")
	elif platform == "iOS":
		print("Firebase Analytics initialized for iOS")
	else:
		print("Firebase Analytics not available on this platform - using mock mode")
		_is_mock = true

## Log an analytics event
func log_event(event_name: String, parameters: Dictionary = {}):
	if not _config.analytics_enabled or not _config.user_consent:
		return
	
	# Add common parameters
	var event_params = {}
	
	# Add user ID if available
	if _user_id != "":
		event_params["user_id"] = _user_id
	
	# Add platform info
	event_params["platform"] = get_platform_name()
	event_params["timestamp"] = Time.get_datetime_dict_from_system()
	
	# Add custom parameters
	for key in parameters:
		event_params[key] = parameters[key]
	
	if _is_mock:
		# Mock implementation - log to console and store locally
		mock_log_event(event_name, event_params)
	else:
		# Real Firebase implementation would go here
		mock_log_event(event_name, event_params)

## Mock implementation for testing/editor
func mock_log_event(event_name: String, parameters: Dictionary):
	print("[Firebase Analytics Mock] Event: %s" % event_name)
	
	if parameters.size() > 0:
		var param_str = ""
		for key in parameters:
			param_str += "%s=%s, " % [key, parameters[key]]
		print("  Parameters: %s" % param_str.substr(0, param_str.length() - 2))
	
	# Store locally for testing
	store_event_locally(event_name, parameters)

## Store event locally for testing
func store_event_locally(event_name: String, parameters: Dictionary):
	var file_path = "user://firebase_analytics_events.json"
	var events = []
	
	# Load existing events
	if FileAccess.file_exists(file_path):
		var file = FileAccess.open(file_path, FileAccess.READ)
		var json_text = file.get_as_text()
		file.close()
		
		var json = JSON.new()
		var parse_result = json.parse(json_text)
		if parse_result == OK:
			events = json.data
	
	# Add new event
	events.append({
		"event_name": event_name,
		"parameters": parameters,
		"timestamp": Time.get_datetime_dict_from_system()
	})
	
	# Save back to file
	var file = FileAccess.open(file_path, FileAccess.WRITE)
	var json_string = JSON.stringify(events)
	file.store_string(json_string)
	file.close()

## Set user property
func set_user_property(property_name: String, value: String):
	_user_properties[property_name] = value
	
	if _is_mock:
		mock_set_user_property(property_name, value)
	else:
		# Real Firebase implementation would go here
		mock_set_user_property(property_name, value)

## Mock implementation for user properties
func mock_set_user_property(property_name: String, value: String):
	print("[Firebase Analytics Mock] User Property: %s = %s" % [property_name, value])
	
	# Store locally
	var prefs = ConfigFile.new()
	prefs.set_value("firebase_user_properties", property_name, value)
	prefs.save("user://firebase_properties.cfg")

## Set user ID
func set_user_id(user_id: String):
	_user_id = user_id
	
	if _is_mock:
		mock_set_user_id(user_id)
	else:
		# Real Firebase implementation would go here
		mock_set_user_id(user_id)

## Mock implementation for user ID
func mock_set_user_id(user_id: String):
	print("[Firebase Analytics Mock] User ID: %s" % user_id)
	
	# Store locally
	var prefs = ConfigFile.new()
	prefs.set_value("firebase_user_properties", "user_id", user_id)
	prefs.save("user://firebase_properties.cfg")

## Check if Firebase Analytics is initialized
func is_initialized() -> bool:
	return not _is_mock

## Get platform name for analytics
func get_platform_name() -> String:
	var platform = OS.get_name()
	
	if Engine.is_editor_hint():
		return "Editor"
	elif platform == "Android":
		return "Android"
	elif platform == "iOS":
		return "iOS"
	elif platform == "Windows":
		return "Windows"
	elif platform == "macOS":
		return "macOS"
	elif platform == "Linux":
		return "Linux"
	else:
		return "Unknown"

## Get stored events for testing
func get_stored_events() -> Array:
	var file_path = "user://firebase_analytics_events.json"
	
	if FileAccess.file_exists(file_path):
		var file = FileAccess.open(file_path, FileAccess.READ)
		var json_text = file.get_as_text()
		file.close()
		
		var json = JSON.new()
		var parse_result = json.parse(json_text)
		if parse_result == OK:
			return json.data
	
	return []

## Clear stored events
func clear_stored_events():
	var file_path = "user://firebase_analytics_events.json"
	if FileAccess.file_exists(file_path):
		DirAccess.remove_absolute(file_path)
