extends Node

## Manages friend challenges: creation, acceptance, completion, and rewards

signal challenge_created(challenge: Dictionary)
signal challenge_accepted(challenge: Dictionary)
signal challenge_completed(challenge: Dictionary, winner_id: String)
signal challenge_expired(challenge: Dictionary)
signal challenge_notification(message: String, challenge: Dictionary)

static var instance: FriendChallengeManager

const CHALLENGES_DATA_PATH: String = "user://challenges.json"
const MAX_ACTIVE_CHALLENGES: int = 50

# Challenge rewards
const LOSER_REWARD_COINS: int = 50
const WINNER_REWARD_COINS: int = 200
const BOTH_COMPLETE_BONUS: int = 100

# Challenge data
var _challenges: Dictionary = {}  # challenge_id -> FriendChallenge
var _pending_challenge_ids: Array = []

func _ready():
	if instance:
		queue_free()
		return
	
	instance = self
	process_mode = Node.PROCESS_MODE_ALWAYS
	
	load_challenges()
	cleanup_expired_challenges()
	
	print("Friend Challenge Manager initialized")

func _exit_tree():
	save_challenges()

## Create a new friend challenge
func create_challenge(challengee_id: String, challengee_name: String, 
		level_id: String, level_name: String, target_score: int, message: String = "") -> Dictionary:
	
	if _challenges.size() >= MAX_ACTIVE_CHALLENGES:
		printerr("Cannot create challenge: maximum limit of %d reached" % MAX_ACTIVE_CHALLENGES)
		return {}
	
	if challengee_id.is_empty():
		printerr("Cannot create challenge: invalid challengee ID")
		return {}
	
	var challenge = {
		"challenge_id": generate_challenge_id(),
		"challenger_id": get_current_player_id(),
		"challenger_name": get_current_player_name(),
		"challengee_id": challengee_id,
		"challengee_name": challengee_name,
		"level_id": level_id,
		"level_name": level_name,
		"target_score": target_score,
		"challenger_score": target_score,
		"challenger_stars": 3,  # Assume 3 stars for self-challenges
		"message": message,
		"created_date": Time.get_datetime_dict_from_system(),
		"expiration_date": get_expiration_date(),
		"status": "pending",  # "pending", "accepted", "completed", "declined", "expired"
		"accepted_date": {},
		"completed_date": {},
		"challengee_score": 0,
		"challengee_stars": 0,
		"winner_id": "",
		"rewards_claimed": false,
		"challenger_cosmetics": get_current_player_cosmetics(),
		"challengee_cosmetics": {}
	}
	
	_challenges[challenge["challenge_id"]] = challenge
	_pending_challenge_ids.append(challenge["challenge_id"])
	
	save_challenges()
	
	# Update friend statistics
	if has_node("/root/FriendLeaderboard"):
		var friend_leaderboard = get_node("/root/FriendLeaderboard")
		if friend_leaderboard.has_method("increment_challenges_sent"):
			friend_leaderboard.increment_challenges_sent(challengee_id)
		if friend_leaderboard.has_method("update_friend_interaction"):
			friend_leaderboard.update_friend_interaction(challengee_id)
	
	challenge_created.emit(challenge)
	
	# Track analytics
	track_challenge_created(challenge)
	
	# Send notification
	send_challenge_notification(challenge)
	
	print("Created challenge: %s -> %s on %s" % [challenge["challenger_name"], challenge["challengee_name"], level_name])
	return challenge

## Accept a challenge
func accept_challenge(challenge_id: String) -> bool:
	if not _challenges.has(challenge_id):
		printerr("Cannot accept challenge: %s not found" % challenge_id)
		return false
	
	var challenge = _challenges[challenge_id]
	
	if not can_be_accepted(challenge):
		printerr("Cannot accept challenge: already accepted, completed, or expired")
		return false
	
	challenge["status"] = "accepted"
	challenge["accepted_date"] = Time.get_datetime_dict_from_system()
	
	_pending_challenge_ids.erase(challenge_id)
	save_challenges()
	
	# Update friend statistics
	if has_node("/root/FriendLeaderboard"):
		var friend_leaderboard = get_node("/root/FriendLeaderboard")
		if friend_leaderboard.has_method("increment_challenges_received"):
			friend_leaderboard.increment_challenges_received(challenge["challenger_id"])
		if friend_leaderboard.has_method("update_friend_interaction"):
			friend_leaderboard.update_friend_interaction(challenge["challenger_id"])
	
	challenge_accepted.emit(challenge)
	
	# Track analytics
	track_challenge_accepted(challenge)
	
	print("Challenge accepted: %s" % challenge_id)
	return true

## Complete a challenge with score
func complete_challenge(challenge_id: String, score: int, stars: int) -> bool:
	if not _challenges.has(challenge_id):
		printerr("Cannot complete challenge: %s not found" % challenge_id)
		return false
	
	var challenge = _challenges[challenge_id]
	
	if challenge["status"] != "accepted":
		printerr("Cannot complete challenge: must be accepted first")
		return false
	
	challenge["challengee_score"] = score
	challenge["challengee_stars"] = stars
	challenge["status"] = "completed"
	challenge["completed_date"] = Time.get_datetime_dict_from_system()
	challenge["challengee_cosmetics"] = get_current_player_cosmetics()
	
	# Determine winner
	var winner_id = determine_winner(challenge)
	challenge["winner_id"] = winner_id
	
	save_challenges()
	
	# Update friend statistics
	if has_node("/root/FriendLeaderboard"):
		var friend_leaderboard = get_node("/root/FriendLeaderboard")
		if friend_leaderboard.has_method("increment_challenges_won"):
			friend_leaderboard.increment_challenges_won(winner_id)
		if friend_leaderboard.has_method("increment_challenges_lost"):
			var loser_id = challenge["challenger_id"] if winner_id == challenge["challengee_id"] else challenge["challengee_id"]
			friend_leaderboard.increment_challenges_lost(loser_id)
		if friend_leaderboard.has_method("update_friend_interaction"):
			friend_leaderboard.update_friend_interaction(challenge["challenger_id"])
	
	# Award rewards
	award_challenge_rewards(challenge)
	
	challenge_completed.emit(challenge, winner_id)
	
	# Track analytics
	track_challenge_completed(challenge, winner_id)
	
	print("Challenge completed: %s, Winner: %s" % [challenge_id, winner_id])
	return true

## Decline a challenge
func decline_challenge(challenge_id: String) -> bool:
	if not _challenges.has(challenge_id):
		printerr("Cannot decline challenge: %s not found" % challenge_id)
		return false
	
	var challenge = _challenges[challenge_id]
	challenge["status"] = "declined"
	
	_pending_challenge_ids.erase(challenge_id)
	save_challenges()
	
	print("Challenge declined: %s" % challenge_id)
	return true

## Get all pending challenges
func get_pending_challenges() -> Array:
	var pending = []
	for challenge_id in _pending_challenge_ids:
		if _challenges.has(challenge_id):
			var challenge = _challenges[challenge_id]
			if can_be_accepted(challenge):
				pending.append(challenge.duplicate())
	return pending

## Get all challenges
func get_all_challenges() -> Array:
	return _challenges.values()

## Get challenge by ID
func get_challenge(challenge_id: String) -> Dictionary:
	if _challenges.has(challenge_id):
		return _challenges[challenge_id].duplicate()
	return {}

## Get completed challenges
func get_completed_challenges() -> Array:
	var completed = []
	for challenge in _challenges.values():
		if challenge["status"] == "completed":
			completed.append(challenge.duplicate())
	
	# Sort by completed date descending
	completed.sort_custom(func(a, b): return b["completed_date"] > a["completed_date"])
	return completed

## Get challenges for a specific friend
func get_challenges_with_friend(friend_id: String) -> Array:
	var friend_challenges = []
	for challenge in _challenges.values():
		if challenge["challenger_id"] == friend_id or challenge["challengee_id"] == friend_id:
			friend_challenges.append(challenge.duplicate())
	
	# Sort by created date descending
	friend_challenges.sort_custom(func(a, b): return b["created_date"] > a["created_date"])
	return friend_challenges

## Award challenge rewards
func award_challenge_rewards(challenge: Dictionary):
	if challenge["rewards_claimed"]:
		return
	
	var winner_id = challenge["winner_id"]
	var current_player_id = get_current_player_id()
	
	# Award coins to current player
	if current_player_id == challenge["challengee_id"]:
		if winner_id == current_player_id:
			# Winner gets medium reward
			award_coins(WINNER_REWARD_COINS)
			print("Challenge won! Awarded %d coins" % WINNER_REWARD_COINS)
		else:
			# Loser gets small reward
			award_coins(LOSER_REWARD_COINS)
			print("Challenge lost. Awarded %d coins" % LOSER_REWARD_COINS)
		
		# Both complete bonus
		if winner_id != "":
			award_coins(BOTH_COMPLETE_BONUS)
			print("Both players completed! Bonus %d coins" % BOTH_COMPLETE_BONUS)
	
	challenge["rewards_claimed"] = true
	save_challenges()

## Award coins to player
func award_coins(amount: int):
	if has_node("/root/MonetizationManager"):
		var monetization = get_node("/root/MonetizationManager")
		if monetization.has_method("add_coins"):
			monetization.add_coins(amount)

## Clean up expired challenges
func cleanup_expired_challenges():
	var current_time = Time.get_unix_time_from_datetime_dict(Time.get_datetime_dict_from_system())
	var expired_ids = []
	
	for challenge_id in _challenges:
		var challenge = _challenges[challenge_id]
		if is_expired(challenge) and challenge["status"] == "pending":
			expired_ids.append(challenge_id)
	
	for challenge_id in expired_ids:
		var challenge = _challenges[challenge_id]
		challenge["status"] = "expired"
		_pending_challenge_ids.erase(challenge_id)
		
		challenge_expired.emit(challenge)
	
	if expired_ids.size() > 0:
		save_challenges()
		print("Cleaned up %d expired challenges" % expired_ids.size())

## Send challenge notification
func send_challenge_notification(challenge: Dictionary):
	var message = "%s challenged you to beat %d on %s!" % [challenge["challenger_name"], challenge["target_score"], challenge["level_name"]]
	challenge_notification.emit(message, challenge)
	
	# Play notification sound
	if has_node("/root/AudioManager"):
		var audio = get_node("/root/AudioManager")
		if audio.has_method("play_sfx"):
			audio.play_sfx("challenge_notification")

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

## Save challenges to disk
func save_challenges():
	var data = {
		"challenges": _challenges,
		"pending_ids": _pending_challenge_ids,
		"version": "1.0"
	}
	
	var file = FileAccess.open(CHALLENGES_DATA_PATH, FileAccess.WRITE)
	var json_string = JSON.stringify(data)
	file.store_string(json_string)
	file.close()

## Load challenges from disk
func load_challenges():
	if not FileAccess.file_exists(CHALLENGES_DATA_PATH):
		return
	
	var file = FileAccess.open(CHALLENGES_DATA_PATH, FileAccess.READ)
	var json_text = file.get_as_text()
	file.close()
	
	if json_text.is_empty():
		return
	
	var json = JSON.new()
	var parse_result = json.parse(json_text)
	if parse_result != OK:
		print("Failed to parse challenges data: %s" % json.get_error_message())
		return
	
	var data = json.data
	if not data is Dictionary:
		return
	
	if data.has("challenges"):
		_challenges = data["challenges"]
	if data.has("pending_ids"):
		_pending_challenge_ids = data["pending_ids"]
	
	print("Loaded %d challenges" % _challenges.size())

## Generate unique challenge ID
func generate_challenge_id() -> String:
	return "challenge_" + str(Time.get_ticks_usec()) + "_" + str(randi())

## Get expiration date (7 days from now)
func get_expiration_date() -> Dictionary:
	var current = Time.get_datetime_dict_from_system()
	var days = current["day"] + 7
	
	# Handle month/year rollover
	while days > 31:
		current["month"] += 1
		if current["month"] > 12:
			current["month"] = 1
			current["year"] += 1
		days -= 31
	
	current["day"] = days
	return current

## Check if challenge can be accepted
func can_be_accepted(challenge: Dictionary) -> bool:
	return challenge["status"] == "pending" and not is_expired(challenge)

## Check if challenge is expired
func is_expired(challenge: Dictionary) -> bool:
	var current_time = Time.get_unix_time_from_datetime_dict(Time.get_datetime_dict_from_system())
	var expiration_time = Time.get_unix_time_from_datetime_dict(challenge["expiration_date"])
	return current_time > expiration_time

## Determine winner of challenge
func determine_winner(challenge: Dictionary) -> String:
	var challenger_score = challenge["challenger_score"]
	var challengee_score = challenge["challengee_score"]
	
	if challengee_score > challenger_score:
		return challenge["challengee_id"]
	elif challenger_score > challengee_score:
		return challenge["challenger_id"]
	else:
		return ""  # Tie

## Track challenge created
func track_challenge_created(challenge: Dictionary):
	if has_node("/root/AnalyticsEventTracker"):
		var tracker = get_node("/root/AnalyticsEventTracker")
		tracker.log_event("challenge_created", {
			"challenge_id": challenge["challenge_id"],
			"challengee_id": challenge["challengee_id"],
			"level_id": challenge["level_id"],
			"target_score": challenge["target_score"]
		})

## Track challenge accepted
func track_challenge_accepted(challenge: Dictionary):
	if has_node("/root/AnalyticsEventTracker"):
		var tracker = get_node("/root/AnalyticsEventTracker")
		tracker.log_event("challenge_accepted", {
			"challenge_id": challenge["challenge_id"],
			"challenger_id": challenge["challenger_id"]
		})

## Track challenge completed
func track_challenge_completed(challenge: Dictionary, winner_id: String):
	if has_node("/root/AnalyticsEventTracker"):
		var tracker = get_node("/root/AnalyticsEventTracker")
		tracker.log_event("challenge_completed", {
			"challenge_id": challenge["challenge_id"],
			"winner_id": winner_id,
			"challenger_score": challenge["challenger_score"],
			"challengee_score": challenge["challengee_score"]
		})

## Clear all challenges
func clear_challenges():
	_challenges.clear()
	_pending_challenge_ids.clear()
	save_challenges()
	print("All challenges cleared")
