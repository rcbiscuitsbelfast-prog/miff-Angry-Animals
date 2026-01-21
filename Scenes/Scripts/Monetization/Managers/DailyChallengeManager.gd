extends Node
class_name DailyChallengeManager

static var instance: DailyChallengeManager = null

func _ready() -> void:
	instance = self

## Use current date as seed for daily challenge
func get_daily_seed() -> int:
	var datetime = Time.get_datetime_dict_from_system()
	return datetime.year * 10000 + datetime.month * 100 + datetime.day

## Start the daily challenge
func start_daily_challenge() -> void:
	var seed = get_daily_seed()
	# Set a random level number for variety, but deterministic for the day
	var level_number = (seed % 100) + 1
	
	var player_profile = get_node_or_null("/root/PlayerProfile")
	if player_profile:
		player_profile.use_procedural_levels = true
		player_profile.last_procedural_seed = seed
		player_profile.last_procedural_level_number = level_number
		player_profile.save_profile()
	
	var game_manager = get_node_or_null("/root/GameManager")
	if game_manager and game_manager.has_method("start_room_by_level_number"):
		game_manager.start_room_by_level_number(level_number)
