extends Node

## Difficulty curve analyzer and tuning system
## Analyzes level difficulty and suggests balance adjustments

signal difficulty_analyzed(suggestions: Array)
signal difficulty_spike_detected(level_number: int, spike_intensity: float)
signal level_balance_recommended(level_number: int, recommendations: Dictionary)

static var instance: DifficultyAnalyzer

# Difficulty tracking
var _level_data: Array = []  # Array of LevelDifficultyData
var _difficulty_history: Dictionary = {}  # level_number -> DifficultyMetrics

# Analysis configuration
var _config: Dictionary = {}
var _analysis_file_path: String = "user://difficulty_analysis.json"

# Difficulty curve visualization data
var _difficulty_curve: Array = []
var _difficulty_spikes: Array = []

func _ready():
	if instance:
		queue_free()
		return
	
	instance = self
	initialize_analyzer()

## Initialize difficulty analyzer
func initialize_analyzer():
	load_configuration()
	load_historical_data()
	
	print("Difficulty analyzer initialized")

## Load analysis configuration
func load_configuration():
	_config = {
		"sample_size": 10,  # Number of attempts to analyze
		"difficulty_threshold": 3.0,  # Spike threshold
		"target_completion_rate": 0.7,  # 70% target success rate
		"optimal_attempts_range": [1.5, 3.0],
		"target_play_time_range": [30.0, 120.0],  # seconds
		"spike_detection_sensitivity": 0.5,
		"balance_check_interval": 10,  # levels
		"enable_real_time_analysis": true
	}

## Load historical difficulty data
func load_historical_data():
	if FileAccess.file_exists(_analysis_file_path):
		var file = FileAccess.open(_analysis_file_path, FileAccess.READ)
		var json_text = file.get_as_text()
		file.close()
		
		var json = JSON.new()
		var parse_result = json.parse(json_text)
		if parse_result == OK:
			var data = json.data
			if data is Dictionary:
				if data.has("level_data"):
					_level_data = data["level_data"]
				if data.has("difficulty_history"):
					_difficulty_history = data["difficulty_history"]
				
				print("Loaded difficulty data")
		else:
			print("Failed to parse difficulty data: %s" % json.get_error_message())

## Record level attempt data
func record_level_attempt(level_number: int, completed: bool, attempts: int, time_spent: float, hints_used: int = 0):
	var existing_data = get_level_data(level_number)
	
	if existing_data == null:
		existing_data = create_level_data(level_number)
		_level_data.append(existing_data)
	
	# Update statistics
	existing_data["total_attempts"] += 1
	if completed:
		existing_data["successful_attempts"] += 1
	existing_data["total_time_spent"] += time_spent
	existing_data["hints_used"] += hints_used
	
	# Add attempt history
	var attempt_data = {
		"completed": completed,
		"attempts": attempts,
		"time_spent": time_spent,
		"hints_used": hints_used,
		"timestamp": Time.get_datetime_dict_from_system()
	}
	
	existing_data["attempts_history"].append(attempt_data)
	
	# Keep only recent attempts
	if existing_data["attempts_history"].size() > _config["sample_size"]:
		existing_data["attempts_history"].pop_front()
	
	# Recalculate difficulty score
	update_difficulty_score(existing_data)
	
	# Save data
	save_historical_data()
	
	# Check for difficulty issues
	check_difficulty_issues(level_number, existing_data)

## Create level data structure
func create_level_data(level_number: int) -> Dictionary:
	return {
		"level_number": level_number,
		"total_attempts": 0,
		"successful_attempts": 0,
		"total_time_spent": 0.0,
		"hints_used": 0,
		"attempts_history": [],
		"difficulty_score": 0.0,
		"completion_rate": 0.0,
		"average_attempts": 0.0,
		"average_time": 0.0
	}

## Get level data by number
func get_level_data(level_number: int):
	for data in _level_data:
		if data["level_number"] == level_number:
			return data
	return null

## Update difficulty score for a level
func update_difficulty_score(level_data: Dictionary):
	if level_data["total_attempts"] == 0:
		return
	
	# Calculate metrics
	level_data["completion_rate"] = float(level_data["successful_attempts"]) / float(level_data["total_attempts"])
	level_data["average_time"] = level_data["total_time_spent"] / float(level_data["total_attempts"])
	
	# Calculate average attempts from history
	var total_attempts = 0
	for attempt in level_data["attempts_history"]:
		total_attempts += attempt["attempts"]
	
	if level_data["attempts_history"].size() > 0:
		level_data["average_attempts"] = float(total_attempts) / float(level_data["attempts_history"].size())
	else:
		level_data["average_attempts"] = 1.0
	
	# Calculate difficulty score (0-10 scale)
	var target_completion = _config["target_completion_rate"]
	var completion_diff = abs(level_data["completion_rate"] - target_completion)
	
	var optimal_attempts = _config["optimal_attempts_range"]
	var attempts_diff = 0.0
	if level_data["average_attempts"] < optimal_attempts[0]:
		attempts_diff = optimal_attempts[0] - level_data["average_attempts"]
	elif level_data["average_attempts"] > optimal_attempts[1]:
		attempts_diff = level_data["average_attempts"] - optimal_attempts[1]
	
	# Combine factors
	level_data["difficulty_score"] = (completion_diff * 10.0) + (attempts_diff * 2.0)
	level_data["difficulty_score"] = clamp(level_data["difficulty_score"], 0.0, 10.0)

## Check for difficulty issues
func check_difficulty_issues(level_number: int, level_data: Dictionary):
	var suggestions = []
	
	# Check completion rate
	if level_data["completion_rate"] < _config["target_completion_rate"] - 0.2:
		suggestions.append({
			"type": "too_hard",
			"severity": "high",
			"message": "Completion rate too low (%.1f%%)" % (level_data["completion_rate"] * 100),
			"suggestion": "Reduce enemy count or add more power-ups"
		})
	elif level_data["completion_rate"] > 0.95:
		suggestions.append({
			"type": "too_easy",
			"severity": "low",
			"message": "Completion rate very high (%.1f%%)" % (level_data["completion_rate"] * 100),
			"suggestion": "Consider increasing difficulty slightly"
		})
	
	# Check average attempts
	var optimal_attempts = _config["optimal_attempts_range"]
	if level_data["average_attempts"] > optimal_attempts[1] * 2:
		suggestions.append({
			"type": "too_many_attempts",
			"severity": "high",
			"message": "Average attempts too high (%.1f)" % level_data["average_attempts"],
			"suggestion": "Reduce puzzle complexity"
		})
	
	# Check time spent
	var target_time = _config["target_play_time_range"]
	if level_data["average_time"] > target_time[1] * 1.5:
		suggestions.append({
			"type": "too_long",
			"severity": "medium",
			"message": "Average time too long (%.1f seconds)" % level_data["average_time"],
			"suggestion": "Reduce level length or add checkpoints"
		})
	
	# Emit suggestions if any
	if suggestions.size() > 0:
		difficulty_analyzed.emit(suggestions)

## Analyze all levels
func analyze_all_levels():
	_difficulty_curve.clear()
	_difficulty_spikes.clear()
	
	# Sort by level number
	_level_data.sort_custom(func(a, b): return a.level_number < b.level_number)
	
	var previous_difficulty = 0.0
	
	for level_data in _level_data:
		var level_number = level_data["level_number"]
		var difficulty = level_data["difficulty_score"]
		
		# Add to curve
		_difficulty_curve.append(Vector2(level_number, difficulty))
		
		# Check for spikes
		if previous_difficulty > 0:
			var spike = abs(difficulty - previous_difficulty)
			if spike > _config["difficulty_threshold"]:
				_difficulty_spikes.append(str(level_number))
				difficulty_spike_detected.emit(level_number, spike)
		
		previous_difficulty = difficulty
	
	# Generate balance recommendations
	generate_balance_recommendations()

## Generate balance recommendations
func generate_balance_recommendations():
	for level_data in _level_data:
		var level_number = level_data["level_number"]
		var recommendations = {}
		
		# Difficulty adjustment
		if level_data["difficulty_score"] > 7:
			recommendations["difficulty_adjustment"] = "decrease"
			recommendations["suggested_difficulty"] = level_data["difficulty_score"] - 2
		elif level_data["difficulty_score"] < 2:
			recommendations["difficulty_adjustment"] = "increase"
			recommendations["suggested_difficulty"] = level_data["difficulty_score"] + 2
		
		# Time adjustment
		var target_time = _config["target_play_time_range"]
		if level_data["average_time"] > target_time[1]:
			recommendations["time_adjustment"] = "decrease"
		elif level_data["average_time"] < target_time[0]:
			recommendations["time_adjustment"] = "increase"
		
		if recommendations.size() > 0:
			level_balance_recommended.emit(level_number, recommendations)

## Get difficulty for a specific level
func get_level_difficulty(level_number: int) -> float:
	var level_data = get_level_data(level_number)
	if level_data != null:
		return level_data["difficulty_score"]
	return 5.0  # Default medium difficulty

## Get completion rate for a level
func get_completion_rate(level_number: int) -> float:
	var level_data = get_level_data(level_number)
	if level_data != null:
		return level_data["completion_rate"]
	return 0.5  # Default

## Get average attempts for a level
func get_average_attempts(level_number: int) -> float:
	var level_data = get_level_data(level_number)
	if level_data != null:
		return level_data["average_attempts"]
	return 2.0  # Default

## Get difficulty curve data
func get_difficulty_curve() -> Array:
	return _difficulty_curve.duplicate()

## Get difficulty spikes
func get_difficulty_spikes() -> Array:
	return _difficulty_spikes.duplicate()

## Get all level data
func get_all_level_data() -> Array:
	return _level_data.duplicate()

## Get level data range
func get_level_data_in_range(start_level: int, end_level: int) -> Array:
	var result = []
	for data in _level_data:
		if data["level_number"] >= start_level and data["level_number"] <= end_level:
			result.append(data.duplicate())
	return result

## Save historical data
func save_historical_data():
	var data = {
		"level_data": _level_data,
		"difficulty_history": _difficulty_history,
		"last_updated": Time.get_datetime_dict_from_system()
	}
	
	var file = FileAccess.open(_analysis_file_path, FileAccess.WRITE)
	var json_string = JSON.stringify(data)
	file.store_string(json_string)
	file.close()

## Clear all difficulty data
func clear_data():
	_level_data.clear()
	_difficulty_history.clear()
	_difficulty_curve.clear()
	_difficulty_spikes.clear()
	
	save_historical_data()
	print("All difficulty data cleared")

## Get difficulty statistics summary
func get_summary_statistics() -> Dictionary:
	if _level_data.size() == 0:
		return {}
	
	var total_difficulty = 0.0
	var total_completion_rate = 0.0
	var total_attempts = 0
	var max_difficulty = 0.0
	var min_difficulty = 10.0
	
	for level_data in _level_data:
		var difficulty = level_data["difficulty_score"]
		total_difficulty += difficulty
		total_completion_rate += level_data["completion_rate"]
		total_attempts += level_data["total_attempts"]
		
		if difficulty > max_difficulty:
			max_difficulty = difficulty
		if difficulty < min_difficulty:
			min_difficulty = difficulty
	
	return {
		"total_levels": _level_data.size(),
		"average_difficulty": total_difficulty / float(_level_data.size()),
		"average_completion_rate": total_completion_rate / float(_level_data.size()),
		"total_attempts": total_attempts,
		"max_difficulty": max_difficulty,
		"min_difficulty": min_difficulty,
		"difficulty_range": max_difficulty - min_difficulty
	}
