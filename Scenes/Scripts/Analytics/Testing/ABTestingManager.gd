extends Node

## Manages A/B test variants, user segmentation, and statistical analysis

signal test_variant_assigned(test_id: String, variant_id: String, user_id: String)
signal test_completed(test_id: String, winning_variant: String, result: Dictionary)
signal conversion_tracked(test_id: String, variant_id: String, conversion_type: String, value: float)

static var instance: ABTestingManager

# Test management
var _active_tests: Array = []  # Array of ABTest
var _completed_tests: Dictionary = {}  # test_id -> ABTestResult
var _user_assignments: Dictionary = {}  # test_id -> variant_id

# Configuration
var _test_configs: Dictionary = {}
var _current_user_id: String

func _ready():
	if instance:
		queue_free()
		return
	
	instance = self
	initialize_ab_testing()

## Initialize A/B testing system
func initialize_ab_testing():
	_current_user_id = get_current_user_id()
	load_test_configurations()
	initialize_preconfigured_tests()
	
	print("A/B Testing Manager initialized")

## Get current user ID
func get_current_user_id() -> String:
	if has_node("/root/PlayerProfile"):
		var profile = get_node("/root/PlayerProfile")
		if profile.has_method("get_player_id"):
			return profile.get_player_id()
	return "anonymous"

## Load test configurations
func load_test_configurations():
	# Default test configurations - can be overridden by Firebase Remote Config
	_test_configs["cosmetics_price_test"] = {
		"test_name": "Cosmetics Pricing Test",
		"description": "Test different price points for cosmetics",
		"variants": {
			"control": {"price": 2.99},
			"variant_1": {"price": 3.99},
			"variant_2": {"price": 4.99}
		},
		"traffic_split": {
			"control": 0.33,
			"variant_1": 0.33,
			"variant_2": 0.34
		},
		"duration_days": 14,
		"target_metric": "conversion_rate"
	}
	
	_test_configs["ad_frequency_test"] = {
		"test_name": "Ad Frequency Test",
		"description": "Test different ad placement strategies",
		"variants": {
			"control": {"interstitial_frequency": 5},
			"variant_1": {"interstitial_frequency": 3},
			"variant_2": {"interstitial_frequency": 8}
		},
		"traffic_split": {
			"control": 0.33,
			"variant_1": 0.33,
			"variant_2": 0.34
		},
		"duration_days": 21,
		"target_metric": "arpu"
	}
	
	_test_configs["notification_test"] = {
		"test_name": "Push Notification Test",
		"description": "Test different notification strategies",
		"variants": {
			"control": {"send_time": "09:00", "message_type": "standard"},
			"variant_1": {"send_time": "07:00", "message_type": "personalized"},
			"variant_2": {"send_time": "11:00", "message_type": "emojis"}
		},
		"traffic_split": {
			"control": 0.33,
			"variant_1": 0.33,
			"variant_2": 0.34
		},
		"duration_days": 28,
		"target_metric": "retention_d1"
	}

## Initialize pre-configured A/B tests
func initialize_preconfigured_tests():
	create_test_from_config("cosmetics_price_test")
	create_test_from_config("ad_frequency_test")
	create_test_from_config("notification_test")

## Create an A/B test from configuration
func create_test_from_config(config_key: String):
	if not _test_configs.has(config_key):
		return
	
	var config = _test_configs[config_key]
	var test_id = config_key
	
	var start_date = Time.get_datetime_dict_from_system()
	var end_date = start_date.duplicate()
	end_date["day"] += config["duration_days"]
	
	var test = {
		"test_id": test_id,
		"test_name": config["test_name"],
		"description": config["description"],
		"start_date": start_date,
		"end_date": end_date,
		"variants": config["variants"].duplicate(),
		"traffic_split": config["traffic_split"].duplicate(),
		"target_metric": config["target_metric"],
		"conversion_counts": {},
		"conversion_values": {},
		"participant_counts": {},
		"is_active": true
	}
	
	# Initialize counts for each variant
	for variant_id in config["variants"]:
		test["conversion_counts"][variant_id] = 0
		test["conversion_values"][variant_id] = 0.0
		test["participant_counts"][variant_id] = 0
	
	_active_tests.append(test)
	
	print("Created A/B test: %s" % test["test_name"])

## Get user's variant for a test
func get_variant(test_id: String) -> Dictionary:
	if _user_assignments.has(test_id):
		var variant_id = _user_assignments[test_id]
		return get_variant_config(test_id, variant_id)
	
	# Assign user to a variant
	return assign_variant(test_id)

## Assign user to a variant
func assign_variant(test_id: String) -> Dictionary:
	var test = get_active_test(test_id)
	if test == null:
		return {}
	
	# Generate consistent variant based on user ID
	var variant_id = get_deterministic_variant(test_id, _current_user_id)
	
	# Record assignment
	_user_assignments[test_id] = variant_id
	test["participant_counts"][variant_id] = test["participant_counts"][variant_id] + 1
	
	test_variant_assigned.emit(test_id, variant_id, _current_user_id)
	
	print("User %s assigned to variant %s for test %s" % [_current_user_id, variant_id, test_id])
	
	return get_variant_config(test_id, variant_id)

## Get variant configuration
func get_variant_config(test_id: String, variant_id: String) -> Dictionary:
	var test = get_active_test(test_id)
	if test == null:
		return {}
	
	if test["variants"].has(variant_id):
		return test["variants"][variant_id].duplicate()
	
	return {}

## Track conversion for a test
func track_conversion(test_id: String, conversion_type: String, value: float = 0.0):
	if not _user_assignments.has(test_id):
		printerr("Cannot track conversion: user not assigned to test %s" % test_id)
		return
	
	var variant_id = _user_assignments[test_id]
	var test = get_active_test(test_id)
	if test == null:
		return
	
	# Update conversion counts
	test["conversion_counts"][variant_id] = test["conversion_counts"][variant_id] + 1
	test["conversion_values"][variant_id] = test["conversion_values"][variant_id] + value
	
	conversion_tracked.emit(test_id, variant_id, conversion_type, value)
	
	print("Tracked conversion: test=%s, variant=%s, type=%s, value=%s" % [test_id, variant_id, conversion_type, value])

## Get active test by ID
func get_active_test(test_id: String):
	for test in _active_tests:
		if test["test_id"] == test_id and test["is_active"]:
			return test
	return null

## Get all active tests
func get_active_tests() -> Array:
	var active = []
	for test in _active_tests:
		if test["is_active"] and not is_test_expired(test):
			active.append(test.duplicate())
	return active

## Get completed tests
func get_completed_tests() -> Array:
	return _completed_tests.values()

## Get test results
func get_test_results(test_id: String) -> Dictionary:
	var test = get_active_test(test_id)
	if test == null:
		if _completed_tests.has(test_id):
			return _completed_tests[test_id].duplicate()
		return {}
	
	var results = {
		"test_id": test_id,
		"test_name": test["test_name"],
		"variants": [],
		"total_conversions": 0,
		"total_value": 0.0,
		"total_participants": 0
	}
	
	for variant_id in test["variants"]:
		var variant_result = {
			"variant_id": variant_id,
			"config": test["variants"][variant_id],
			"conversions": test["conversion_counts"][variant_id],
			"total_value": test["conversion_values"][variant_id],
			"participants": test["participant_counts"][variant_id],
			"conversion_rate": 0.0,
			"average_value": 0.0
		}
		
		if test["participant_counts"][variant_id] > 0:
			variant_result["conversion_rate"] = float(test["conversion_counts"][variant_id]) / float(test["participant_counts"][variant_id])
		if test["conversion_counts"][variant_id] > 0:
			variant_result["average_value"] = test["conversion_values"][variant_id] / float(test["conversion_counts"][variant_id])
		
		results["variants"].append(variant_result)
		results["total_conversions"] += test["conversion_counts"][variant_id]
		results["total_value"] += test["conversion_values"][variant_id]
		results["total_participants"] += test["participant_counts"][variant_id]
	
	return results

## Check if test is expired
func is_test_expired(test: Dictionary) -> bool:
	var current_time = Time.get_unix_time_from_datetime_dict(Time.get_datetime_dict_from_system())
	var end_time = Time.get_unix_time_from_datetime_dict(test["end_date"])
	return current_time > end_time

## Complete a test
func complete_test(test_id: String):
	var test = get_active_test(test_id)
	if test == null:
		return
	
	test["is_active"] = false
	
	# Calculate winner
	var results = get_test_results(test_id)
	var winning_variant = determine_winner(results)
	
	var test_result = {
		"test_id": test_id,
		"test_name": test["test_name"],
		"winning_variant": winning_variant,
		"results": results,
		"completed_date": Time.get_datetime_dict_from_system()
	}
	
	_completed_tests[test_id] = test_result
	_active_tests.erase(test)
	
	test_completed.emit(test_id, winning_variant, results)
	
	print("Test completed: %s, Winner: %s" % [test_id, winning_variant])

## Determine winning variant from results
func determine_winner(results: Dictionary) -> String:
	var target_metric = results.get("target_metric", "conversion_rate")
	var best_variant = ""
	var best_value = -1.0
	
	for variant in results["variants"]:
		var value = 0.0
		match target_metric:
			"conversion_rate":
				value = variant["conversion_rate"]
			"average_value":
				value = variant["average_value"]
			_:
				value = variant["conversion_rate"]
		
		if value > best_value:
			best_value = value
			best_variant = variant["variant_id"]
	
	return best_variant

## Get deterministic variant for user
func get_deterministic_variant(test_id: String, user_id: String) -> String:
	var test = get_active_test(test_id)
	if test == null:
		return "control"
	
	# Hash user ID + test ID for consistency
	var hash_string = (user_id + test_id).md5_text()
	var hash_value = int(hash_string.substr(0, 8), 16)
	var random_value = float(hash_value % 1000) / 1000.0
	
	# Assign based on traffic split
	var cumulative = 0.0
	for variant_id in test["traffic_split"]:
		cumulative += test["traffic_split"][variant_id]
		if random_value < cumulative:
			return variant_id
	
	# Fallback to control
	return "control"

## Get variant probability
func get_variant_probability(test_id: String, variant_id: String) -> float:
	var test = get_active_test(test_id)
	if test == null or not test["traffic_split"].has(variant_id):
		return 0.0
	
	return test["traffic_split"][variant_id]

## Check if user is in test
func is_user_in_test(test_id: String) -> bool:
	return _user_assignments.has(test_id)

## Get user's assignment
func get_user_assignment(test_id: String) -> String:
	if _user_assignments.has(test_id):
		return _user_assignments[test_id]
	return ""

## Clear all test assignments
func clear_assignments():
	_user_assignments.clear()
	print("All test assignments cleared")

## Save test state
func save_test_state():
	var state = {
		"user_assignments": _user_assignments,
		"completed_tests": _completed_tests
	}
	
	var file = FileAccess.open("user://ab_testing_state.json", FileAccess.WRITE)
	var json_string = JSON.stringify(state)
	file.store_string(json_string)
	file.close()

## Load test state
func load_test_state():
	if not FileAccess.file_exists("user://ab_testing_state.json"):
		return
	
	var file = FileAccess.open("user://ab_testing_state.json", FileAccess.READ)
	var json_text = file.get_as_text()
	file.close()
	
	var json = JSON.new()
	var parse_result = json.parse(json_text)
	if parse_result != OK:
		return
	
	var state = json.data
	if state is Dictionary:
		if state.has("user_assignments"):
			_user_assignments = state["user_assignments"]
		if state.has("completed_tests"):
			_completed_tests = state["completed_tests"]
	
	print("Loaded A/B testing state")
