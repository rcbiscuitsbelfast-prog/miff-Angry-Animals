extends Node
class_name SocialCosmetics

## Manages social cosmetics unlocked through social actions

signal social_cosmetic_unlocked(cosmetic_id: String)

var _cosmetics: Dictionary = {}

static var instance: SocialCosmetics = null

func _ready() -> void:
	if instance != null:
		queue_free()
		return
	
	instance = self
	process_mode = Node.PROCESS_MODE_ALWAYS
	
	_initialize_cosmetics()
	_check_all_unlocks()
	
	print("Social Cosmetics initialized")

## Initialize social cosmetic definitions
func _initialize_cosmetics() -> void:
	_cosmetics = {
		"friendship_hat": {
			"id": "friendship_hat",
			"name": "Friendship Hat",
			"description": "Unlocked by adding 5 friends",
			"unlock_condition": "add_friends",
			"required_count": 5,
			"cosmetic_type": "hat",
			"rarity": "rare"
		},
		"challenge_champion_crown": {
			"id": "challenge_champion_crown",
			"name": "Challenge Champion Crown",
			"description": "Unlocked by winning 10 friend challenges",
			"unlock_condition": "win_challenges",
			"required_count": 10,
			"cosmetic_type": "hat",
			"rarity": "epic"
		},
		"viral_legend_glasses": {
			"id": "viral_legend_glasses",
			"name": "Viral Legend Glasses",
			"description": "Unlocked by getting 100 replay views",
			"unlock_condition": "replay_views",
			"required_count": 100,
			"cosmetic_type": "glasses",
			"rarity": "legendary"
		},
		"team_player_wig": {
			"id": "team_player_wig",
			"name": "Team Player Wig",
			"description": "Unlocked by participating in 50 challenges",
			"unlock_condition": "participate_challenges",
			"required_count": 50,
			"cosmetic_type": "wig",
			"rarity": "rare"
		},
		"leaderboard_elite_moustache": {
			"id": "leaderboard_elite_moustache",
			"name": "Leaderboard Elite Moustache",
			"description": "Unlocked by ranking in top 100 on any level",
			"unlock_condition": "leaderboard_top_100",
			"required_count": 1,
			"cosmetic_type": "moustache",
			"rarity": "epic"
		}
	}

## Check all unlock conditions
func check_all_unlocks() -> void:
	for cosmetic_id in _cosmetics:
		_check_unlock(cosmetic_id)

## Check specific unlock condition
func _check_unlock(cosmetic_id: String) -> void:
	if _is_unlocked(cosmetic_id):
		return
	
	var cosmetic = _cosmetics[cosmetic_id]
	var current_progress = _get_progress(cosmetic)
	
	if current_progress >= cosmetic["required_count"]:
		_unlock_cosmetic(cosmetic_id)

## Get progress toward unlocking cosmetic
func _get_progress(cosmetic_id: String) -> int:
	var cosmetic = _cosmetics.get(cosmetic_id, {})
	var condition = cosmetic.get("unlock_condition", "")
	
	match condition:
		"add_friends":
			return _get_friend_count()
		"win_challenges":
			return _get_total_challenges_won()
		"replay_views":
			return _get_total_replay_views()
		"participate_challenges":
			return _get_total_challenges_participated()
		"leaderboard_top_100":
			return 1 if _is_in_top_100() else 0
		_:
			return 0

## Unlock social cosmetic
func _unlock_cosmetic(cosmetic_id: String) -> void:
	var player_profile = get_node_or_null("/root/PlayerProfile")
	if player_profile and player_profile.has_method("unlock_cosmetic"):
		player_profile.unlock_cosmetic(cosmetic_id)
	
	social_cosmetic_unlocked.emit(cosmetic_id)
	
	# Track analytics
	_track_social_cosmetic_unlocked(cosmetic_id)
	
	print("Unlocked social cosmetic: %s" % cosmetic_id)

## Check if cosmetic is unlocked
func _is_unlocked(cosmetic_id: String) -> bool:
	var player_profile = get_node_or_null("/root/PlayerProfile")
	if player_profile:
		return cosmetic_id in player_profile.unlocked_cosmetics
	return false

## Get all social cosmetics
func get_all_cosmetics() -> Array:
	return _cosmetics.values()

## Get cosmetic by ID
func get_cosmetic(cosmetic_id: String) -> Dictionary:
	return _cosmetics.get(cosmetic_id, {})

## Get total challenges won
func _get_total_challenges_won() -> int:
	var friend_challenge_manager = get_node_or_null("/root/FriendChallengeManager")
	if not friend_challenge_manager:
		return 0
	
	var current_player_id = ""
	var player_profile = get_node_or_null("/root/PlayerProfile")
	if player_profile:
		current_player_id = player_profile.player_name
	
	var challenges_won = 0
	
	# Placeholder: In real implementation, query the friend challenge system
	# for challenges won by current player
	
	return challenges_won

## Get total replay views
func _get_total_replay_views() -> int:
	var replay_manager = get_node_or_null("/root/ReplayManager")
	if not replay_manager:
		return 0
	
	var total_views = 0
	
	# Placeholder: In real implementation, query the replay system
	# for total view count
	
	return total_views

## Get total challenges participated
func _get_total_challenges_participated() -> int:
	var friend_challenge_manager = get_node_or_null("/root/FriendChallengeManager")
	if not friend_challenge_manager:
		return 0
	
	var total_participated = 0
	
	# Placeholder: In real implementation, query the friend challenge system
	# for total participation count
	
	return total_participated

## Check if player is in top 100
func _is_in_top_100() -> bool:
	var global_leaderboard = get_node_or_null("/root/GlobalLeaderboard")
	if not global_leaderboard:
		return false
	
	# Placeholder: In real implementation, query the leaderboard system
	# for player's rank
	
	return false

## Track social cosmetic unlocked analytics
func _track_social_cosmetic_unlocked(cosmetic_id: String) -> void:
	var event_tracker = get_node_or_null("/root/AnalyticsEventTracker")
	if event_tracker and event_tracker.has_method("track_event"):
		event_tracker.track_event("social_cosmetic_unlocked", {
			"cosmetic_id": cosmetic_id
		})
