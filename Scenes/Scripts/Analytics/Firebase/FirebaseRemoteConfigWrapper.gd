extends RefCounted

## Firebase Remote Config Wrapper - Handles remote configuration and feature flags
## Provides dynamic configuration updates without app updates

var _config: FirebaseManager.FirebaseConfig
var _is_mock: bool
var _remote_config_values: Dictionary = {}
var _is_fetched: bool = false
var _last_fetch_time: Dictionary = {}

func _init(config: FirebaseManager.FirebaseConfig, is_mock: bool = false):
	_config = config
	_is_mock = is_mock
	
	if not _is_mock:
		initialize_remote_config()
	else:
		initialize_mock_values()

## Initialize Firebase Remote Config
func initialize_remote_config():
	# Real Remote Config initialization would go here
	_is_mock = true
	print("Remote Config initialized in mock mode")

## Initialize mock values for testing
func initialize_mock_values():
	# Default feature flags and configuration values
	_remote_config_values["enable_new_levels"] = true
	_remote_config_values["enable_seasonal_events"] = true
	_remote_config_values["max_daily_rewards"] = 5
	_remote_config_values["premium_currency_multiplier"] = 1.0
	_remote_config_values["daily_challenge_count"] = 3
	_remote_config_values["tutorial_completion_bonus"] = 100
	_remote_config_values["event_end_date"] = "9999-12-31T23:59:59"
	_remote_config_values["maintenance_mode"] = false
	_remote_config_values["min_level_for_ads"] = 5
	_remote_config_values["max_retry_attempts"] = 3
	
	print("Remote Config mock values initialized")

## Fetch remote config values from server
func fetch(callback: Callable = Callable()):
	if not _config.remote_config_enabled:
		if not callback.is_null():
			callback.call(false)
		return
	
	if _is_mock:
		# Simulate network delay
		await get_tree().create_timer(0.1).timeout
		simulate_fetch(callback)
	else:
		# Real Remote Config fetch would go here
		await get_tree().create_timer(0.1).timeout
		simulate_fetch(callback)

## Simulate remote config fetch for testing
func simulate_fetch(callback: Callable):
	# Simulate random network delay
	await get_tree().process_frame
	
	# In a real implementation, this would fetch from Firebase
	_is_fetched = true
	_last_fetch_time = Time.get_datetime_dict_from_system()
	
	print("Remote config fetched successfully (mock)")
	if not callback.is_null():
		callback.call(true)

## Get remote config value
func get_value(key: String, default_value = null):
	if _remote_config_values.has(key):
		return _remote_config_values[key]
	
	return default_value

## Get string value
func get_string(key: String, default_value: String = "") -> String:
	var value = get_value(key, default_value)
	return str(value) if value != null else default_value

## Get integer value
func get_int(key: String, default_value: int = 0) -> int:
	var value = get_value(key, default_value)
	if value is int:
		return value
	elif value is String:
		return int(value) if value.is_valid_int() else default_value
	elif value is float:
		return int(value)
	return default_value

## Get float value
func get_float(key: String, default_value: float = 0.0) -> float:
	var value = get_value(key, default_value)
	if value is float:
		return value
	elif value is int:
		return float(value)
	elif value is String:
		return float(value) if value.is_valid_float() else default_value
	return default_value

## Get boolean value
func get_bool(key: String, default_value: bool = false) -> bool:
	var value = get_value(key, default_value)
	if value is bool:
		return value
	elif value is String:
		if value.to_lower() == "true":
			return true
		elif value.to_lower() == "false":
			return false
	elif value is int:
		return value != 0
	return default_value

## Set remote config value (for testing)
func set_value(key: String, value):
	_remote_config_values[key] = value
	
	# Store locally for persistence
	store_config_value_locally(key, value)

## Check if a key exists
func has_key(key: String) -> bool:
	return _remote_config_values.has(key)

## Get all keys
func get_all_keys() -> Array:
	return _remote_config_values.keys()

## Check if config has been fetched
func is_fetched() -> bool:
	return _is_fetched

## Get last fetch time
func get_last_fetch_time() -> Dictionary:
	return _last_fetch_time

## Activate fetched values
func activate():
	if _is_fetched:
		print("Remote config values activated")
		# In real implementation, this would activate fetched values

## Store config value locally for persistence
func store_config_value_locally(key: String, value):
	var file_path = "user://firebase_remote_config.json"
	var config = {}
	
	# Load existing config
	if FileAccess.file_exists(file_path):
		var file = FileAccess.open(file_path, FileAccess.READ)
		var json_text = file.get_as_text()
		file.close()
		
		var json = JSON.new()
		var parse_result = json.parse(json_text)
		if parse_result == OK:
			config = json.data
	
	# Update value
	config[key] = value
	
	# Save back to file
	var file = FileAccess.open(file_path, FileAccess.WRITE)
	var json_string = JSON.stringify(config)
	file.store_string(json_string)
	file.close()

## Load config values from local storage
func load_from_local():
	var file_path = "user://firebase_remote_config.json"
	
	if FileAccess.file_exists(file_path):
		var file = FileAccess.open(file_path, FileAccess.READ)
		var json_text = file.get_as_text()
		file.close()
		
		var json = JSON.new()
		var parse_result = json.parse(json_text)
		if parse_result == OK:
			var local_config = json.data
			
			if local_config is Dictionary:
				for key in local_config:
					_remote_config_values[key] = local_config[key]
				
				print("Loaded %d config values from local storage" % local_config.size())

## Clear all remote config values
func clear():
	_remote_config_values.clear()
	_is_fetched = false
	_last_fetch_time.clear()
	
	var file_path = "user://firebase_remote_config.json"
	if FileAccess.file_exists(file_path):
		DirAccess.remove_absolute(file_path)

## Check if Remote Config is initialized
func is_initialized() -> bool:
	return true  # Always initialized in mock mode

## Get all config values
func get_all_values() -> Dictionary:
	return _remote_config_values.duplicate()
