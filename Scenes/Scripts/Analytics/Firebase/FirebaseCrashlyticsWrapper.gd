extends RefCounted

## Firebase Crashlytics Wrapper - Handles crash reporting and error tracking
## Provides automatic crash reporting and custom error logging

var _config: FirebaseManager.FirebaseConfig
var _is_mock: bool
var _crash_reports: Array = []

func _init(config: FirebaseManager.FirebaseConfig, is_mock: bool = false):
	_config = config
	_is_mock = is_mock
	
	if not _is_mock:
		initialize_crashlytics()

## Initialize Firebase Crashlytics
func initialize_crashlytics():
	# Real Crashlytics initialization would go here
	# For now, use mock mode
	_is_mock = true
	print("Crashlytics initialized in mock mode")

## Record a non-fatal exception
func record_exception(type: String, message: String, additional_data: Dictionary = {}):
	if not _config.crashlytics_enabled or not _config.user_consent:
		return
	
	var crash_report = {
		"type": type,
		"message": message,
		"timestamp": Time.get_datetime_dict_from_system(),
		"platform": get_platform_name(),
		"additional_data": additional_data if additional_data.size() > 0 else {},
		"is_fatal": false
	}
	
	if _is_mock:
		mock_record_exception(crash_report)
	else:
		# Real Crashlytics implementation would go here
		mock_record_exception(crash_report)

## Record a fatal crash
func record_crash(type: String, message: String, additional_data: Dictionary = {}):
	var crash_report = {
		"type": type,
		"message": message,
		"timestamp": Time.get_datetime_dict_from_system(),
		"platform": get_platform_name(),
		"additional_data": additional_data if additional_data.size() > 0 else {},
		"is_fatal": true
	}
	
	if _is_mock:
		mock_record_crash(crash_report)
	else:
		# Real Crashlytics implementation would go here
		mock_record_crash(crash_report)

## Set custom key-value pair
func set_custom_key(key: String, value: String):
	if _is_mock:
		mock_set_custom_key(key, value)
	else:
		# Real Crashlytics implementation would go here
		mock_set_custom_key(key, value)

## Set user identifier
func set_user_id(user_id: String):
	if _is_mock:
		mock_set_user_id(user_id)
	else:
		# Real Crashlytics implementation would go here
		mock_set_user_id(user_id)

## Mock implementation for non-fatal exceptions
func mock_record_exception(report: Dictionary):
	printerr("[Crashlytics Mock] Non-Fatal Exception: %s - %s" % [report.type, report.message])
	
	if report.additional_data.size() > 0:
		var data_str = ""
		for key in report.additional_data:
			data_str += "%s=%s, " % [key, report.additional_data[key]]
		printerr("  Additional Data: %s" % data_str.substr(0, data_str.length() - 2))
	
	_crash_reports.append(report)
	store_crash_report_locally(report)

## Mock implementation for crashes
func mock_record_crash(report: Dictionary):
	printerr("[Crashlytics Mock] CRASH: %s - %s" % [report.type, report.message])
	
	if report.additional_data.size() > 0:
		var data_str = ""
		for key in report.additional_data:
			data_str += "%s=%s, " % [key, report.additional_data[key]]
		printerr("  Additional Data: %s" % data_str.substr(0, data_str.length() - 2))
	
	_crash_reports.append(report)
	store_crash_report_locally(report)

## Mock implementation for custom keys
func mock_set_custom_key(key: String, value: String):
	print("[Crashlytics Mock] Custom Key: %s = %s" % [key, value])

## Mock implementation for user ID
func mock_set_user_id(user_id: String):
	print("[Crashlytics Mock] User ID: %s" % user_id)

## Store crash report locally
func store_crash_report_locally(report: Dictionary):
	var file_path = "user://firebase_crash_reports.json"
	var reports = []
	
	# Load existing reports
	if FileAccess.file_exists(file_path):
		var file = FileAccess.open(file_path, FileAccess.READ)
		var json_text = file.get_as_text()
		file.close()
		
		var json = JSON.new()
		var parse_result = json.parse(json_text)
		if parse_result == OK:
			reports = json.data
	
	# Add new report
	reports.append(report)
	
	# Save back to file (keep only last 50 reports)
	if reports.size() > 50:
		reports = reports.slice(reports.size() - 50, reports.size())
	
	var file = FileAccess.open(file_path, FileAccess.WRITE)
	var json_string = JSON.stringify(reports)
	file.store_string(json_string)
	file.close()

## Get platform name
func get_platform_name() -> String:
	var platform = OS.get_name()
	
	if Engine.is_editor_hint():
		return "Editor"
	elif platform == "Android":
		return "Android"
	elif platform == "iOS":
		return "iOS"
	else:
		return platform

## Get crash reports for testing
func get_crash_reports() -> Array:
	return _crash_reports.duplicate()

## Clear crash reports
func clear_crash_reports():
	_crash_reports.clear()
	
	var file_path = "user://firebase_crash_reports.json"
	if FileAccess.file_exists(file_path):
		DirAccess.remove_absolute(file_path)

## Check if Crashlytics is initialized
func is_initialized() -> bool:
	return not _is_mock
