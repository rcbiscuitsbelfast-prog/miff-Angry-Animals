extends Node

## Manages friend-based leaderboards and competitions
## Handles friend relationships, scores, and challenges

signal friend_added(friend: Dictionary)
signal friend_removed(friend_id: String)
signal friend_score_updated(friend_id: String, level_id: String, score: int)
signal friend_leaderboard_refreshed()

static var instance: FriendLeaderboard

const FRIENDS_DATA_PATH: String = "user://friends.json"
const MAX_FRIENDS: int = 100

# Friend data
var _friends: Dictionary = {}  # friend_id -> FriendData
var _friend_scores_by_level: Dictionary = {}  # level_id -> {friend_id: score}

func _ready():
	if instance:
		queue_free()
		return
	
	instance = self
	process_mode = Node.PROCESS_MODE_ALWAYS
	
	load_friends()
	print("Friend Leaderboard initialized")

func _exit_tree():
	save_friends()

## Add a friend to friends list
func add_friend(friend_id: String, friend_name: String) -> bool:
	if friend_id.is_empty():
		printerr("Cannot add friend: invalid friend ID")
		return false
	
	if _friends.size() >= MAX_FRIENDS:
		printerr("Cannot add friend: maximum limit of %d reached" % MAX_FRIENDS)
		return false
	
	if _friends.has(friend_id):
		print("Friend %s is already in your friends list" % friend_name)
		return false
	
	var friend = {
		"friend_id": friend_id,
		"friend_name": friend_name,
		"friendship_date": Time.get_datetime_dict_from_system(),
		"last_interaction_date": Time.get_datetime_dict_from_system(),
		"total_score": 0,
		"levels_completed": 0,
		"perfect_runs": 0,
		"avatar_url": ""
	}
	
	_friends[friend_id] = friend
	save_friends()
	
	friend_added.emit(friend)
	
	# Track analytics
	track_friend_added(friend_id)
	
	print("Added friend: %s (%s)" % [friend_name, friend_id])
	return true

## Remove a friend from friends list
func remove_friend(friend_id: String) -> bool:
	if not _friends.has(friend_id):
		printerr("Cannot remove friend: %s not found" % friend_id)
		return false
	
	var friend_name = _friends[friend_id]["friend_name"]
	_friends.erase(friend_id)
	save_friends()
	
	friend_removed.emit(friend_id)
	
	# Track analytics
	track_friend_removed(friend_id)
	
	print("Removed friend: %s (%s)" % [friend_name, friend_id])
	return true

## Get all friends
func get_all_friends() -> Array:
	return _friends.values()

## Get friend by ID
func get_friend(friend_id: String) -> Dictionary:
	if _friends.has(friend_id):
		return _friends[friend_id].duplicate()
	return {}

## Get friend count
func get_friend_count() -> int:
	return _friends.size()

## Update friend's score for a specific level
func update_friend_score(friend_id: String, level_id: String, score: int, stars: int):
	if not _friends.has(friend_id):
		printerr("Cannot update score: friend %s not found" % friend_id)
		return
	
	# Initialize level scores if needed
	if not _friend_scores_by_level.has(level_id):
		_friend_scores_by_level[level_id] = {}
	
	var current_score = _friend_scores_by_level[level_id].get(friend_id, 0)
	
	# Only update if new score is better
	if score > current_score:
		_friend_scores_by_level[level_id][friend_id] = score
		
		# Update friend's total score
		var friend = _friends[friend_id]
		friend["total_score"] = friend.get("total_score", 0) - current_score + score
		friend["levels_completed"] = friend.get("levels_completed", 0) + (1 if stars > 0 else 0)
		if stars == 3:
			friend["perfect_runs"] = friend.get("perfect_runs", 0) + 1
		
		friend["last_interaction_date"] = Time.get_datetime_dict_from_system()
		
		save_friends()
		
		friend_score_updated.emit(friend_id, level_id, score)
		
		# Track analytics
		track_score_updated(friend_id, level_id, score)

## Get friend's score for a specific level
func get_friend_score(friend_id: String, level_id: String) -> int:
	if _friend_scores_by_level.has(level_id) and _friend_scores_by_level[level_id].has(friend_id):
		return _friend_scores_by_level[level_id][friend_id]
	return 0

## Get friend leaderboard for a specific level
func get_friend_leaderboard(level_id: String, limit: int = 20) -> Array:
	var leaderboard = []
	
	if not _friend_scores_by_level.has(level_id):
		return leaderboard
	
	# Add all friends with scores for this level
	var friend_ids = _friend_scores_by_level[level_id].keys()
	for friend_id in friend_ids:
		if _friends.has(friend_id):
			var friend = _friends[friend_id].duplicate()
			friend["score"] = _friend_scores_by_level[level_id][friend_id]
			leaderboard.append(friend)
	
	# Sort by score descending
	leaderboard.sort_custom(func(a, b): return a.score > b.score)
	
	# Limit results
	if limit > 0 and leaderboard.size() > limit:
		leaderboard = leaderboard.slice(0, limit)
	
	return leaderboard

## Get global friend leaderboard (by total score)
func get_global_friend_leaderboard(limit: int = 20) -> Array:
	var leaderboard = _friends.values().duplicate()
	
	# Sort by total score descending
	leaderboard.sort_custom(func(a, b): return a.total_score > b.total_score)
	
	# Limit results
	if limit > 0 and leaderboard.size() > limit:
		leaderboard = leaderboard.slice(0, limit)
	
	return leaderboard

## Get player's rank among friends
func get_player_rank(level_id: String) -> int:
	var leaderboard = get_friend_leaderboard(level_id)
	
	# Find player's score
	var player_score = get_player_score(level_id)
	if player_score == 0:
		return leaderboard.size() + 1  # At the bottom
	
	# Count friends with higher scores
	var rank = 1
	for friend in leaderboard:
		if friend.score > player_score:
			rank += 1
	
	return rank

## Get player's score for a level
func get_player_score(level_id: String) -> int:
	# This would get the player's best score from PlayerProfile
	if has_node("/root/PlayerProfile"):
		var profile = get_node("/root/PlayerProfile")
		if profile.has_method("get_best_score"):
			return profile.get_best_score(level_id)
	return 0

## Search friends by name
func search_friends(query: String) -> Array:
	var results = []
	var query_lower = query.to_lower()
	
	for friend in _friends.values():
		var name = friend["friend_name"].to_lower()
		if query_lower in name:
			results.append(friend.duplicate())
	
	return results

## Get friend stats
func get_friend_stats(friend_id: String) -> Dictionary:
	if not _friends.has(friend_id):
		return {}
	
	var friend = _friends[friend_id]
	
	# Count levels with scores
	var levels_with_scores = 0
	for level_id in _friend_scores_by_level:
		if _friend_scores_by_level[level_id].has(friend_id):
			levels_with_scores += 1
	
	return {
		"friend_id": friend_id,
		"friend_name": friend["friend_name"],
		"total_score": friend.get("total_score", 0),
		"levels_completed": friend.get("levels_completed", 0),
		"perfect_runs": friend.get("perfect_runs", 0),
		"levels_with_scores": levels_with_scores,
		"friendship_date": friend["friendship_date"],
		"last_interaction": friend["last_interaction_date"]
	}

## Refresh friend leaderboard (fetch from server)
func refresh_friend_leaderboard():
	# In a real implementation, this would fetch from your backend server
	print("Refreshing friend leaderboard...")
	
	# For now, just emit the signal
	friend_leaderboard_refreshed.emit()

## Load friends from file
func load_friends():
	if FileAccess.file_exists(FRIENDS_DATA_PATH):
		var file = FileAccess.open(FRIENDS_DATA_PATH, FileAccess.READ)
		var json_text = file.get_as_text()
		file.close()
		
		var json = JSON.new()
		var parse_result = json.parse(json_text)
		if parse_result == OK:
			var data = json.data
			if data.has("friends"):
				_friends = data["friends"]
			if data.has("scores"):
				_friend_scores_by_level = data["scores"]
			
			print("Loaded %d friends" % _friends.size())
		else:
			print("Failed to parse friends data: %s" % json.get_error_message())

## Save friends to file
func save_friends():
	var data = {
		"friends": _friends,
		"scores": _friend_scores_by_level
	}
	
	var file = FileAccess.open(FRIENDS_DATA_PATH, FileAccess.WRITE)
	var json_string = JSON.stringify(data)
	file.store_string(json_string)
	file.close()

## Track friend added
func track_friend_added(friend_id: String):
	if has_node("/root/AnalyticsEventTracker"):
		var tracker = get_node("/root/AnalyticsEventTracker")
		tracker.log_event("friend_added", {
			"friend_id": friend_id,
			"total_friends": get_friend_count()
		})

## Track friend removed
func track_friend_removed(friend_id: String):
	if has_node("/root/AnalyticsEventTracker"):
		var tracker = get_node("/root/AnalyticsEventTracker")
		tracker.log_event("friend_removed", {
			"friend_id": friend_id,
			"total_friends": get_friend_count()
		})

## Track score updated
func track_score_updated(friend_id: String, level_id: String, score: int):
	if has_node("/root/AnalyticsEventTracker"):
		var tracker = get_node("/root/AnalyticsEventTracker")
		tracker.log_event("friend_score_updated", {
			"friend_id": friend_id,
			"level_id": level_id,
			"score": score
		})

## Clear all friends
func clear_friends():
	_friends.clear()
	_friend_scores_by_level.clear()
	save_friends()
	print("All friends cleared")
