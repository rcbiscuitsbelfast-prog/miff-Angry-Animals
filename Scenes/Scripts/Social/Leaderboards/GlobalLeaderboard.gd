extends Node

## Manages global leaderboards with Firebase integration
## Tracks top scores per level and global rankings

signal leaderboard_updated(type: int)
signal leaderboard_sync_started()
signal leaderboard_sync_completed(success: bool)
signal player_rank_changed(type: int, old_rank: int, new_rank: int)

static var instance: GlobalLeaderboard

const LEADERBOARD_DATA_PATH: String = "user://leaderboard_cache.json"
const TOP_100_COUNT: int = 100
const SYNC_INTERVAL_MINUTES: float = 5.0

# Leaderboard types
enum LeaderboardType {
	BY_LEVEL = 0,
	BY_TOTAL_SCORE = 1,
	BY_PERFECT_LEVELS = 2,
	BY_LEVELS_COMPLETED = 3
}

# Leaderboard data
var _leaderboards = {
	"by_level": {},  # level_id -> LeaderboardData
	"by_total_score": null,  # LeaderboardData
	"by_perfect_levels": null,  # LeaderboardData
	"by_levels_completed": null  # LeaderboardData
}
var _last_sync_time: Dictionary = {}
var _is_syncing: bool = false
var _sync_timer: Timer

func _ready():
	if instance:
		queue_free()
		return
	
	instance = self
	process_mode = Node.PROCESS_MODE_ALWAYS
	
	load_cached_leaderboards()
	
	# Start periodic sync
	_sync_timer = Timer.new()
	_sync_timer.wait_time = SYNC_INTERVAL_MINUTES * 60
	_sync_timer.autostart = true
	_sync_timer.timeout.connect(_on_sync_timer_timeout)
	add_child(_sync_timer)
	
	print("Global Leaderboard initialized")

func _exit_tree():
	save_leaderboard_cache()

## Submit score to global leaderboard
func submit_score(level_id: String, level_name: String, score: int, stars: int, completion_time: float, replay_id: String = ""):
	var entry = {
		"player_id": get_current_player_id(),
		"player_name": get_current_player_name(),
		"score": score,
		"stars": stars,
		"completion_time": completion_time,
		"date_achieved": Time.get_datetime_dict_from_system(),
		"cosmetics": get_current_player_cosmetics(),
		"replay_id": replay_id,
		"is_current_player": true
	}
	
	# Update level-specific leaderboard
	update_level_leaderboard(level_id, level_name, entry)
	
	# Update total score leaderboard
	update_total_score_leaderboard()
	
	# Update perfect levels count if applicable
	if stars >= 3:
		update_perfect_levels_leaderboard()
	
	# Sync to server (async)
	sync_to_server(level_id, entry)
	
	print("Submitted score: %d on %s" % [score, level_name])

## Update level-specific leaderboard
func update_level_leaderboard(level_id: String, level_name: String, entry: Dictionary):
	if not _leaderboards["by_level"].has(level_id):
		_leaderboards["by_level"][level_id] = {
			"leaderboard_id": level_id,
			"type": LeaderboardType.BY_LEVEL,
			"level_id": level_id,
			"level_name": level_name,
			"entries": [],
			"last_updated": Time.get_datetime_dict_from_system()
		}
	
	var leaderboard = _leaderboards["by_level"][level_id]
	var old_rank = -1
	
	# Check if player already has an entry
	var existing_index = -1
	for i in range(leaderboard["entries"].size()):
		if leaderboard["entries"][i]["player_id"] == entry["player_id"]:
			existing_index = i
			if leaderboard["entries"][i]["is_current_player"]:
				old_rank = i + 1
			break
	
	if existing_index >= 0:
		# Update if score is higher
		if entry["score"] > leaderboard["entries"][existing_index]["score"]:
			leaderboard["entries"].remove_at(existing_index)
			leaderboard["entries"].append(entry)
	else:
		leaderboard["entries"].append(entry)
	
	# Sort and limit to top 100
	leaderboard["entries"].sort_custom(func(a, b): return a.score > b.score)
	if leaderboard["entries"].size() > TOP_100_COUNT:
		leaderboard["entries"] = leaderboard["entries"].slice(0, TOP_100_COUNT)
	
	# Assign ranks
	for i in range(leaderboard["entries"].size()):
		leaderboard["entries"][i]["rank"] = i + 1
	
	leaderboard["last_updated"] = Time.get_datetime_dict_from_system()
	save_leaderboard_cache()
	
	# Check if player rank changed
	var new_rank = -1
	for i in range(leaderboard["entries"].size()):
		if leaderboard["entries"][i]["player_id"] == entry["player_id"] and leaderboard["entries"][i]["is_current_player"]:
			new_rank = i + 1
			break
	
	if old_rank >= 0 and new_rank >= 0 and old_rank != new_rank:
		player_rank_changed.emit(LeaderboardType.BY_LEVEL, old_rank, new_rank)
	
	leaderboard_updated.emit(LeaderboardType.BY_LEVEL)

## Update total score leaderboard
func update_total_score_leaderboard():
	if _leaderboards["by_total_score"] == null:
		_leaderboards["by_total_score"] = {
			"leaderboard_id": "total_score",
			"type": LeaderboardType.BY_TOTAL_SCORE,
			"entries": [],
			"last_updated": Time.get_datetime_dict_from_system()
		}
	
	var leaderboard = _leaderboards["by_total_score"]
	var player_id = get_current_player_id()
	var player_name = get_current_player_name()
	
	# Update player's total score
	var player_total_score = get_player_total_score()
	
	# Find or create player entry
	var entry = null
	for e in leaderboard["entries"]:
		if e["player_id"] == player_id:
			entry = e
			break
	
	if entry == null:
		entry = {
			"player_id": player_id,
			"player_name": player_name,
			"score": player_total_score,
			"date_achieved": Time.get_datetime_dict_from_system(),
			"cosmetics": get_current_player_cosmetics(),
			"is_current_player": true
		}
		leaderboard["entries"].append(entry)
	else:
		entry["score"] = player_total_score
		entry["cosmetics"] = get_current_player_cosmetics()
	
	# Sort and limit
	leaderboard["entries"].sort_custom(func(a, b): return a.score > b.score)
	if leaderboard["entries"].size() > TOP_100_COUNT:
		leaderboard["entries"] = leaderboard["entries"].slice(0, TOP_100_COUNT)
	
	# Assign ranks
	for i in range(leaderboard["entries"].size()):
		leaderboard["entries"][i]["rank"] = i + 1
	
	leaderboard["last_updated"] = Time.get_datetime_dict_from_system()
	save_leaderboard_cache()
	
	leaderboard_updated.emit(LeaderboardType.BY_TOTAL_SCORE)

## Update perfect levels leaderboard
func update_perfect_levels_leaderboard():
	if _leaderboards["by_perfect_levels"] == null:
		_leaderboards["by_perfect_levels"] = {
			"leaderboard_id": "perfect_levels",
			"type": LeaderboardType.BY_PERFECT_LEVELS,
			"entries": [],
			"last_updated": Time.get_datetime_dict_from_system()
		}
	
	var leaderboard = _leaderboards["by_perfect_levels"]
	var player_id = get_current_player_id()
	var player_name = get_current_player_name()
	
	# Update player's perfect levels count
	var perfect_levels = get_player_perfect_levels_count()
	
	# Find or create player entry
	var entry = null
	for e in leaderboard["entries"]:
		if e["player_id"] == player_id:
			entry = e
			break
	
	if entry == null:
		entry = {
			"player_id": player_id,
			"player_name": player_name,
			"score": perfect_levels,
			"date_achieved": Time.get_datetime_dict_from_system(),
			"cosmetics": get_current_player_cosmetics(),
			"is_current_player": true
		}
		leaderboard["entries"].append(entry)
	else:
		entry["score"] = perfect_levels
		entry["cosmetics"] = get_current_player_cosmetics()
	
	# Sort and limit
	leaderboard["entries"].sort_custom(func(a, b): return a.score > b.score)
	if leaderboard["entries"].size() > TOP_100_COUNT:
		leaderboard["entries"] = leaderboard["entries"].slice(0, TOP_100_COUNT)
	
	# Assign ranks
	for i in range(leaderboard["entries"].size()):
		leaderboard["entries"][i]["rank"] = i + 1
	
	leaderboard["last_updated"] = Time.get_datetime_dict_from_system()
	save_leaderboard_cache()
	
	leaderboard_updated.emit(LeaderboardType.BY_PERFECT_LEVELS)

## Get leaderboard by type
func get_leaderboard(type: int, level_id: String = "") -> Dictionary:
	match type:
		LeaderboardType.BY_LEVEL:
			if level_id != "" and _leaderboards["by_level"].has(level_id):
				return _leaderboards["by_level"][level_id].duplicate()
		LeaderboardType.BY_TOTAL_SCORE:
			if _leaderboards["by_total_score"] != null:
				return _leaderboards["by_total_score"].duplicate()
		LeaderboardType.BY_PERFECT_LEVELS:
			if _leaderboards["by_perfect_levels"] != null:
				return _leaderboards["by_perfect_levels"].duplicate()
		LeaderboardType.BY_LEVELS_COMPLETED:
			if _leaderboards["by_levels_completed"] != null:
				return _leaderboards["by_levels_completed"].duplicate()
	
	return {}

## Get player rank for a leaderboard
func get_player_rank(type: int, level_id: String = "") -> int:
	var leaderboard = get_leaderboard(type, level_id)
	if not leaderboard.has("entries"):
		return -1
	
	var player_id = get_current_player_id()
	for entry in leaderboard["entries"]:
		if entry["player_id"] == player_id:
			return entry["rank"]
	
	return -1

## Get top N entries from leaderboard
func get_top_entries(type: int, count: int, level_id: String = "") -> Array:
	var leaderboard = get_leaderboard(type, level_id)
	if not leaderboard.has("entries"):
		return []
	
	var entries = leaderboard["entries"].duplicate()
	if entries.size() > count:
		entries = entries.slice(0, count)
	
	return entries

## Sync to server
func sync_to_server(level_id: String, entry: Dictionary):
	if _is_syncing:
		return
	
	# In a real implementation, this would send data to your backend
	# For now, just simulate the sync
	print("Syncing score to server for level %s..." % level_id)

## Sync all leaderboards from server
func sync_from_server():
	if _is_syncing:
		return
	
	_is_syncing = true
	leaderboard_sync_started.emit()
	
	# In a real implementation, this would fetch from your backend
	# For now, just simulate the sync
	await get_tree().create_timer(1.0).timeout
	
	print("Leaderboards synced from server")
	
	_is_syncing = false
	leaderboard_sync_completed.emit(true)

## Sync timer callback
func _on_sync_timer_timeout():
	sync_from_server()

## Get current player ID
func get_current_player_id() -> String:
	if has_node("/root/PlayerProfile"):
		var profile = get_node("/root/PlayerProfile")
		if profile.has_method("get_player_name"):
			return profile.get_player_name()
	return "Player"

## Get current player name
func get_current_player_name() -> String:
	if has_node("/root/PlayerProfile"):
		var profile = get_node("/root/PlayerProfile")
		if profile.has_method("get_player_name"):
			return profile.get_player_name()
	return "Player"

## Get current player cosmetics
func get_current_player_cosmetics() -> Dictionary:
	var cosmetics = {
		"hat_index": 0,
		"glasses_index": 0,
		"moustache_index": 0,
		"wig_index": 0,
		"slingshot_skin_index": 0,
		"projectile_skin_index": 0
	}
	
	if has_node("/root/PlayerProfile"):
		var profile = get_node("/root/PlayerProfile")
		if profile.has_method("get_selected_hat_index"):
			cosmetics["hat_index"] = profile.get_selected_hat_index()
		if profile.has_method("get_selected_glasses_index"):
			cosmetics["glasses_index"] = profile.get_selected_glasses_index()
		if profile.has_method("get_selected_moustache_index"):
			cosmetics["moustache_index"] = profile.get_selected_moustache_index()
		if profile.has_method("get_selected_wig_index"):
			cosmetics["wig_index"] = profile.get_selected_wig_index()
		if profile.has_method("get_selected_slingshot_skin_index"):
			cosmetics["slingshot_skin_index"] = profile.get_selected_slingshot_skin_index()
		if profile.has_method("get_selected_projectile_skin_index"):
			cosmetics["projectile_skin_index"] = profile.get_selected_projectile_skin_index()
	
	return cosmetics

## Get player's total score
func get_player_total_score() -> int:
	if has_node("/root/PlayerProfile"):
		var profile = get_node("/root/PlayerProfile")
		if profile.has_method("get_total_score"):
			return profile.get_total_score()
	return 0

## Get player's perfect levels count
func get_player_perfect_levels_count() -> int:
	if has_node("/root/PlayerProfile"):
		var profile = get_node("/root/PlayerProfile")
		if profile.has_method("get_perfect_levels_count"):
			return profile.get_perfect_levels_count()
	return 0

## Save leaderboard cache
func save_leaderboard_cache():
	var data = {
		"leaderboards": _leaderboards,
		"last_sync_time": _last_sync_time
	}
	
	var file = FileAccess.open(LEADERBOARD_DATA_PATH, FileAccess.WRITE)
	var json_string = JSON.stringify(data)
	file.store_string(json_string)
	file.close()

## Load cached leaderboards
func load_cached_leaderboards():
	if not FileAccess.file_exists(LEADERBOARD_DATA_PATH):
		return
	
	var file = FileAccess.open(LEADERBOARD_DATA_PATH, FileAccess.READ)
	var json_text = file.get_as_text()
	file.close()
	
	if json_text.is_empty():
		return
	
	var json = JSON.new()
	var parse_result = json.parse(json_text)
	if parse_result != OK:
		print("Failed to parse leaderboard cache: %s" % json.get_error_message())
		return
	
	var data = json.data
	if data is Dictionary:
		if data.has("leaderboards"):
			_leaderboards = data["leaderboards"]
		if data.has("last_sync_time"):
			_last_sync_time = data["last_sync_time"]
	
	print("Loaded cached leaderboards")

## Clear all leaderboards
func clear_leaderboards():
	_leaderboards = {
		"by_level": {},
		"by_total_score": null,
		"by_perfect_levels": null,
		"by_levels_completed": null
	}
	_last_sync_time.clear()
	save_leaderboard_cache()
	print("All leaderboards cleared")

## Force immediate sync
func force_sync():
	sync_from_server()
