extends Node

## Comprehensive data export system for analytics and analysis
## Exports A/B test results, performance metrics, difficulty heatmaps, and more

signal export_completed(export_type: String, file_path: String, success: bool)
signal export_scheduled(export_type: String, scheduled_time: Dictionary)
signal export_history_updated(history: Array)

static var instance: DataExporter

# Export configuration
var _export_configs: Dictionary = {}
var _export_directory: String = "user://exports/"

# Export history
var _export_history: Array = []
const MAX_HISTORY_SIZE: int = 50

# Scheduled exports
var _scheduled_exports: Dictionary = {}

func _ready():
	if instance:
		queue_free()
		return
	
	instance = self
	initialize_data_exporter()

## Initialize data export system
func initialize_data_exporter():
	initialize_export_configs()
	create_export_directory()
	load_export_history()
	
	print("Data Exporter initialized")

## Initialize export configurations
func initialize_export_configs():
	_export_configs["ab_test_results"] = {
		"export_name": "A/B Test Results",
		"description": "Complete A/B testing data with statistical analysis",
		"file_format": "CSV",
		"frequency": "Manual",
		"data_source": "ABTestingManager"
	}
	
	_export_configs["performance_metrics"] = {
		"export_name": "Performance Metrics",
		"description": "FPS, memory, CPU usage, and performance alerts",
		"file_format": "CSV",
		"frequency": "Daily",
		"data_source": "PerformanceTelemetry"
	}
	
	_export_configs["difficulty_heatmap"] = {
		"export_name": "Difficulty Heatmap",
		"description": "Level difficulty analysis and balancing recommendations",
		"file_format": "CSV",
		"frequency": "Weekly",
		"data_source": "DifficultyHeatmapAnalyzer"
	}
	
	_export_configs["cosmetics_sales"] = {
		"export_name": "Cosmetics Sales Data",
		"description": "Sales data by rarity, price point, and player segment",
		"file_format": "JSON",
		"frequency": "Daily",
		"data_source": "MonetizationManager"
	}
	
	_export_configs["retention_cohorts"] = {
		"export_name": "Retention Cohorts",
		"description": "D1, D7, D30 retention analysis by cohort",
		"file_format": "CSV",
		"frequency": "Weekly",
		"data_source": "AnalyticsManager"
	}
	
	_export_configs["viral_metrics"] = {
		"export_name": "Viral Metrics",
		"description": "Replay sharing, friend challenges, viral coefficients",
		"file_format": "CSV",
		"frequency": "Weekly",
		"data_source": "ReplayManager"
	}
	
	_export_configs["ad_performance"] = {
		"export_name": "Ad Performance",
		"description": "Ad frequency optimization and revenue analysis",
		"file_format": "CSV",
		"frequency": "Daily",
		"data_source": "AdFrequencyOptimizer"
	}
	
	_export_configs["crash_reports"] = {
		"export_name": "Crash Reports",
		"description": "Crash analysis and device performance data",
		"file_format": "JSON",
		"frequency": "Daily",
		"data_source": "CrashReporter"
	}
	
	_export_configs["player_data"] = {
		"export_name": "Player Data",
		"description": "Complete player profile data (GDPR export)",
		"file_format": "JSON",
		"frequency": "Manual",
		"data_source": "PlayerProfile"
	}

## Create export directory
func create_export_directory():
	var dir = DirAccess.open("user://")
	if not dir.dir_exists("exports"):
		dir.make_dir("exports")

## Load export history
func load_export_history():
	var history_path = _export_directory + "export_history.json"
	if FileAccess.file_exists(history_path):
		var file = FileAccess.open(history_path, FileAccess.READ)
		var json_string = file.get_as_text()
		file.close()
		
		var json = JSON.new()
		var parse_result = json.parse(json_string)
		if parse_result == OK:
			var data = json.data
			if data is Dictionary and data.has("exports"):
				_export_history = data["exports"]
				
				print("Loaded %d export records" % _export_history.size())

## Save export history
func save_export_history():
	var history_path = _export_directory + "export_history.json"
	var data = {
		"exports": _export_history.slice(max(0, _export_history.size() - MAX_HISTORY_SIZE)),
		"last_updated": Time.get_datetime_dict_from_system()
	}
	
	var file = FileAccess.open(history_path, FileAccess.WRITE)
	var json_string = JSON.stringify(data)
	file.store_string(json_string)
	file.close()

## Export data
func export_data(export_type: String) -> bool:
	if not _export_configs.has(export_type):
		printerr("Unknown export type: %s" % export_type)
		return false
	
	var config = _export_configs[export_type]
	var data_source = config["data_source"]
	var file_format = config["file_format"]
	
	var export_data = collect_export_data(data_source)
	if export_data == null:
		printerr("Failed to collect export data from %s" % data_source)
		return false
	
	# Generate file name
	var timestamp = Time.get_datetime_dict_from_system()
	var file_name = "%s_%04d%02d%02d_%02d%02d%02d" % [
		export_type,
		timestamp["year"], timestamp["month"], timestamp["day"],
		timestamp["hour"], timestamp["minute"], timestamp["second"]
	]
	
	var file_path = _export_directory + file_name + "." + file_format.to_lower()
	
	# Write data based on format
	var success = false
	match file_format.to_upper():
		"CSV":
			success = write_csv(file_path, export_data)
		"JSON":
			success = write_json(file_path, export_data)
		_:
			printerr("Unsupported file format: %s" % file_format)
	
	if success:
		# Record export in history
		var export_record = {
			"export_type": export_type,
			"file_name": file_name,
			"file_path": file_path,
			"file_format": file_format,
			"data_source": data_source,
			"export_time": Time.get_datetime_dict_from_system(),
			"record_count": get_record_count(export_data)
		}
		
		_export_history.append(export_record)
		save_export_history()
		export_history_updated.emit(_export_history)
		
		print("Export completed: %s" % file_path)
	else:
		printerr("Export failed: %s" % export_type)
	
	export_completed.emit(export_type, file_path, success)
	return success

## Collect export data from source
func collect_export_data(source: String):
	match source:
		"ABTestingManager":
			return export_ab_test_data()
		"AnalyticsManager":
			return export_analytics_data()
		"DifficultyHeatmapAnalyzer":
			return export_difficulty_data()
		"MonetizationManager":
			return export_monetization_data()
		"ReplayManager":
			return export_viral_metrics()
		"CrashReporter":
			return export_crash_data()
		"PlayerProfile":
			return export_player_data()
		_:
			printerr("Unknown data source: %s" % source)
			return null

## Export A/B test data
func export_ab_test_data() -> Dictionary:
	if not has_node("/root/ABTestingManager"):
		return {}
	
	var ab_manager = get_node("/root/ABTestingManager")
	var active_tests = ab_manager.get_active_tests()
	var completed_tests = ab_manager.get_completed_tests()
	
	return {
		"active_tests": active_tests,
		"completed_tests": completed_tests,
		"export_timestamp": Time.get_datetime_dict_from_system()
	}

## Export analytics data
func export_analytics_data() -> Dictionary:
	if not has_node("/root/AnalyticsManager"):
		return {}
	
	var analytics = get_node("/root/AnalyticsManager")
	var analytics_data = analytics.get_analytics_data()
	var retention = analytics.get_retention_metrics()
	
	return {
		"analytics_data": analytics_data,
		"retention_metrics": retention,
		"export_timestamp": Time.get_datetime_dict_from_system()
	}

## Export difficulty data
func export_difficulty_data() -> Dictionary:
	if not has_node("/root/DifficultyAnalyzer"):
		return {}
	
	var analyzer = get_node("/root/DifficultyAnalyzer")
	var level_data = analyzer.get_all_level_data()
	var difficulty_curve = analyzer.get_difficulty_curve()
	var spikes = analyzer.get_difficulty_spikes()
	
	return {
		"level_data": level_data,
		"difficulty_curve": difficulty_curve,
		"difficulty_spikes": spikes,
		"summary": analyzer.get_summary_statistics(),
		"export_timestamp": Time.get_datetime_dict_from_system()
	}

## Export monetization data
func export_monetization_data() -> Dictionary:
	if not has_node("/root/MonetizationManager"):
		return {}
	
	var monetization = get_node("/root/MonetizationManager")
	
	# Collect monetization data
	return {
		"monetization_data": {},  # Would collect from monetization manager
		"export_timestamp": Time.get_datetime_dict_from_system()
	}

## Export viral metrics
func export_viral_metrics() -> Dictionary:
	if not has_node("/root/ReplayManager"):
		return {}
	
	var replay_manager = get_node("/root/ReplayManager")
	var replays = replay_manager.get_all_replays()
	
	# Collect viral metrics from replay and challenge data
	var challenges = []
	if has_node("/root/FriendChallengeManager"):
		var challenge_manager = get_node("/root/FriendChallengeManager")
		challenges = challenge_manager.get_all_challenges()
	
	return {
		"replays": replays,
		"challenges": challenges,
		"viral_coefficient": calculate_viral_coefficient(replays.size(), challenges.size()),
		"export_timestamp": Time.get_datetime_dict_from_system()
	}

## Export crash data
func export_crash_data() -> Dictionary:
	if not has_node("/root/CrashReporter"):
		return {}
	
	var reporter = get_node("/root/CrashReporter")
	var crash_history = reporter.get_crash_history()
	var error_stats = reporter.get_error_statistics()
	
	return {
		"crash_history": crash_history,
		"error_statistics": error_stats,
		"export_timestamp": Time.get_datetime_dict_from_system()
	}

## Export player data (GDPR)
func export_player_data() -> Dictionary:
	var player_data = {
		"player_profile": {},
		"analytics_events": [],
		"purchases": [],
		"friends": [],
		"challenges": [],
		"replays": []
	}
	
	# Collect from all managers
	if has_node("/root/PlayerProfile"):
		var profile = get_node("/root/PlayerProfile")
		player_data["player_profile"] = {}
		# Would collect profile data
	
	if has_node("/root/AnalyticsManager"):
		var analytics = get_node("/root/AnalyticsManager")
		player_data["analytics_events"] = analytics.get_analytics_data()
	
	if has_node("/root/FriendLeaderboard"):
		var friends = get_node("/root/FriendLeaderboard")
		player_data["friends"] = friends.get_all_friends()
	
	if has_node("/root/FriendChallengeManager"):
		var challenges = get_node("/root/FriendChallengeManager")
		player_data["challenges"] = challenges.get_all_challenges()
	
	if has_node("/root/ReplayManager"):
		var replays = get_node("/root/ReplayManager")
		player_data["replays"] = replays.get_all_replays()
	
	player_data["export_timestamp"] = Time.get_datetime_dict_from_system()
	
	return player_data

## Write data as CSV
func write_csv(file_path: String, data: Dictionary) -> bool:
	var file = FileAccess.open(file_path, FileAccess.WRITE)
	if file == null:
		return false
	
	# Convert data to CSV format
	var csv_content = convert_to_csv(data)
	
	file.store_string(csv_content)
	file.close()
	
	return true

## Write data as JSON
func write_json(file_path: String, data: Dictionary) -> bool:
	var file = FileAccess.open(file_path, FileAccess.WRITE)
	if file == null:
		return false
	
	var json_string = JSON.stringify(data, "\t")
	file.store_string(json_string)
	file.close()
	
	return true

## Convert data to CSV format
func convert_to_csv(data: Dictionary) -> String:
	var csv_lines = []
	
	# Convert dictionary to flat CSV
	flatten_dictionary_to_csv(data, csv_lines, "")
	
	return "\n".join(csv_lines)

## Flatten dictionary to CSV
func flatten_dictionary_to_csv(data: Dictionary, lines: Array, prefix: String):
	for key in data:
		var value = data[key]
		var full_key = (prefix + "." + key) if prefix != "" else key
		
		if value is Dictionary:
			flatten_dictionary_to_csv(value, lines, full_key)
		elif value is Array:
			for i in range(value.size()):
				var array_key = full_key + "[" + str(i) + "]"
				var array_value = value[i]
				if array_value is Dictionary:
					flatten_dictionary_to_csv(array_value, lines, array_key)
				else:
					lines.append("%s,%s" % [array_key, str(array_value)])
		else:
			lines.append("%s,%s" % [full_key, str(value)])

## Get record count from data
func get_record_count(data: Dictionary) -> int:
	var count = 0
	for key in data:
		var value = data[key]
		if value is Array:
			count += value.size()
		elif value is Dictionary:
			count += get_record_count(value)
		else:
			count += 1
	return count

## Calculate viral coefficient
func calculate_viral_coefficient(replay_count: int, challenge_count: int) -> float:
	if replay_count == 0:
		return 0.0
	
	# Viral coefficient = (replays * avg_shares_per_replay + challenges * avg_challenges_per_user) / total_users
	# This is a simplified calculation
	return float(challenge_count) / float(replay_count)

## Get export configuration
func get_export_config(export_type: String) -> Dictionary:
	if _export_configs.has(export_type):
		return _export_configs[export_type].duplicate()
	return {}

## Get all export types
func get_export_types() -> Array:
	return _export_configs.keys()

## Get export history
func get_export_history() -> Array:
	return _export_history.duplicate()

## Schedule export
func schedule_export(export_type: String, delay_minutes: int):
	var current_time = Time.get_datetime_dict_from_system()
	var scheduled_time = current_time.duplicate()
	scheduled_time["minute"] += delay_minutes
	
	# Handle hour/day/month rollover
	while scheduled_time["minute"] >= 60:
		scheduled_time["minute"] -= 60
		scheduled_time["hour"] += 1
	
	_scheduled_exports[export_type] = scheduled_time
	
	export_scheduled.emit(export_type, scheduled_time)
	
	print("Scheduled export: %s at %s" % [export_type, str(scheduled_time)])

## Cancel scheduled export
func cancel_scheduled_export(export_type: String):
	if _scheduled_exports.has(export_type):
		_scheduled_exports.erase(export_type)
		print("Cancelled scheduled export: %s" % export_type)

## Clear export history
func clear_export_history():
	_export_history.clear()
	save_export_history()
	print("Export history cleared")

## Export all data (GDPR complete export)
func export_all_data() -> String:
	var timestamp = Time.get_datetime_dict_from_system()
	var export_id = "export_%04d%02d%02d_%02d%02d%02d" % [
		timestamp["year"], timestamp["month"], timestamp["day"],
		timestamp["hour"], timestamp["minute"], timestamp["second"]
	]
	
	# Create export directory
	var export_dir = _export_directory + export_id + "/"
	var dir = DirAccess.open("user://exports")
	dir.make_dir(export_id)
	
	var export_file = export_dir + "complete_export.json"
	var all_data = {
		"export_id": export_id,
		"export_timestamp": Time.get_datetime_dict_from_system(),
		"data": {
			"player_data": export_player_data(),
			"analytics_data": export_analytics_data(),
			"difficulty_data": export_difficulty_data(),
			"monetization_data": export_monetization_data(),
			"viral_metrics": export_viral_metrics(),
			"crash_data": export_crash_data(),
			"ab_test_data": export_ab_test_data()
		}
	}
	
	var file = FileAccess.open(export_file, FileAccess.WRITE)
	var json_string = JSON.stringify(all_data, "\t")
	file.store_string(json_string)
	file.close()
	
	return export_file
