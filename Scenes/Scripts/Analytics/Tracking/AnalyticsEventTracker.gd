extends Node

## Core event tracking framework for Angry Animals analytics
## Provides pre-defined events for all critical game actions with context tracking

signal event_logged(event_name: String, parameters: Dictionary)
signal performance_issue_detected(issue_type: String, value: float)

static var instance: AnalyticsEventTracker

# Configuration
var _is_enabled: bool = true
var _tracking_enabled: bool = true
var _user_id: String = ""
var _user_segment: String = "free"  # "free", "premium", "whale"

# Event tracking
var _event_queue: Array = []
var _event_counts: Dictionary = {}
var _session_start_time: Dictionary

# Performance tracking
var _last_frame_time: float = 0.0
var _low_fps_count: int = 0
var _memory_usage_threshold: float = 500.0  # MB

func _ready():
	if instance:
		queue_free()
		return
	
	instance = self
	initialize_tracker()

## Initialize event tracker
func initialize_tracker():
	_session_start_time = Time.get_datetime_dict_from_system()
	
	# Load user data
	load_user_data()
	
	# Initialize Firebase integration
	initialize_firebase_integration()
	
	print("Analytics Event Tracker initialized")

## Load user data from profile
func load_user_data():
	# Get user ID from PlayerProfile or generate one
	if has_node("/root/PlayerProfile"):
		var profile = get_node("/root/PlayerProfile")
		if profile.has_method("get_user_id"):
			_user_id = profile.get_user_id()
	
	if _user_id == "":
		_user_id = generate_user_id()
	
	# Determine user segment
	_user_segment = determine_user_segment()

## Initialize Firebase integration
func initialize_firebase_integration():
	if has_node("/root/FirebaseManager"):
		var firebase = get_node("/root/FirebaseManager")
		firebase.set_user_property("user_segment", _user_segment)
		firebase.set_user_id(_user_id)

## Generate unique user ID
func generate_user_id() -> String:
	return str(Time.get_ticks_usec()) + "_" + str(randi())

## Determine user segment based on behavior
func determine_user_segment() -> String:
	# Check if user has premium status
	if has_node("/root/MonetizationManager"):
		var monetization = get_node("/root/MonetizationManager")
		if monetization.has_method("is_premium") and monetization.is_premium():
			return "premium"
		
		# Check monetization data for whale detection
		if monetization.has_method("get_total_spent"):
			var total_spent = monetization.get_total_spent()
			if total_spent > 50.0:
				return "whale"
			elif total_spent > 5.0:
				return "payer"
	
	return "free"

# ===============================================
# CORE TRACKING
# ===============================================

## Log an analytics event
func log_event(event_name: String, parameters: Dictionary = {}):
	if not _tracking_enabled:
		return
	
	# Add common parameters
	var final_params = parameters.duplicate()
	if not final_params.has("user_id"):
		final_params["user_id"] = _user_id
	if not final_params.has("user_segment"):
		final_params["user_segment"] = _user_segment
	if not final_params.has("timestamp"):
		final_params["timestamp"] = Time.get_datetime_dict_from_system()
	
	# Count events
	if _event_counts.has(event_name):
		_event_counts[event_name] += 1
	else:
		_event_counts[event_name] = 1
	
	# Send to Firebase
	if has_node("/root/FirebaseManager"):
		var firebase = get_node("/root/FirebaseManager")
		firebase.log_event(event_name, final_params)
	
	# Emit signal
	event_logged.emit(event_name, final_params)
	
	print("[Analytics] Event: %s" % event_name)

## Enable/disable tracking
func set_tracking_enabled(enabled: bool):
	_tracking_enabled = enabled

## Check if tracking is enabled
func is_tracking_enabled() -> bool:
	return _tracking_enabled

# ===============================================
# GAMEPLAY EVENTS
# ===============================================

## Track level started event
func track_level_started(level_number: int, level_type: String = "normal"):
	if not _tracking_enabled:
		return
	
	var params = {
		"level_number": level_number,
		"level_type": level_type,
		"user_segment": _user_segment,
		"session_duration": Time.get_ticks_msec() / 1000.0,
		"levels_completed_today": get_levels_completed_today(),
		"device_type": get_device_type()
	}
	
	log_event("level_started", params)

## Track level completed event
func track_level_completed(level_number: int, completion_time: float, attempts: int = 1, score: int = 0, perfect: bool = false):
	if not _tracking_enabled:
		return
	
	var params = {
		"level_number": level_number,
		"completion_time": completion_time,
		"attempts": attempts,
		"score": score,
		"perfect": perfect,
		"user_segment": _user_segment,
		"difficulty_rating": get_level_difficulty(level_number)
	}
	
	log_event("level_completed", params)

## Track level failed event
func track_level_failed(level_number: int, attempts: int, time_spent: float, failure_reason: String = "unknown"):
	if not _tracking_enabled:
		return
	
	var params = {
		"level_number": level_number,
		"attempts": attempts,
		"time_spent": time_spent,
		"failure_reason": failure_reason,
		"user_segment": _user_segment,
		"consecutive_failures": get_consecutive_failures()
	}
	
	log_event("level_failed", params)
	
	# Track rage quit detection
	check_rage_quit_pattern(level_number)

## Track perfect score achieved event
func track_perfect_score_achieved(level_number: int, completion_time: float):
	if not _tracking_enabled:
		return
	
	var params = {
		"level_number": level_number,
		"completion_time": completion_time,
		"user_segment": _user_segment,
		"total_perfect_scores": get_total_perfect_scores()
	}
	
	log_event("perfect_score_achieved", params)

# ===============================================
# MONETIZATION EVENTS
# ===============================================

## Track cosmetic purchased event
func track_cosmetic_purchased(cosmetic_type: String, cosmetic_id: String, cost: float, currency: String = "USD"):
	if not _tracking_enabled:
		return
	
	var params = {
		"cosmetic_type": cosmetic_type,
		"cosmetic_id": cosmetic_id,
		"cost": cost,
		"currency": currency,
		"user_segment": _user_segment,
		"total_spent": get_total_spent(),
		"purchase_source": get_purchase_source()
	}
	
	log_event("cosmetic_purchased", params)

## Track cosmetic unlocked event
func track_cosmetic_unlocked(cosmetic_type: String, cosmetic_id: String, unlock_method: String = "purchase"):
	if not _tracking_enabled:
		return
	
	var params = {
		"cosmetic_type": cosmetic_type,
		"cosmetic_id": cosmetic_id,
		"unlock_method": unlock_method,
		"user_segment": _user_segment,
		"total_cosmetics_unlocked": get_total_cosmetics_unlocked()
	}
	
	log_event("cosmetic_unlocked", params)

## Track battle pass purchased event
func track_battle_pass_purchased(cost: float, currency: String = "USD", season: int = 1):
	if not _tracking_enabled:
		return
	
	var params = {
		"cost": cost,
		"currency": currency,
		"season": season,
		"user_segment": _user_segment,
		"battle_pass_owner": true
	}
	
	log_event("battle_pass_purchased", params)

## Track remove ads purchased event
func track_remove_ads_purchased(cost: float, currency: String = "USD"):
	if not _tracking_enabled:
		return
	
	var params = {
		"cost": cost,
		"currency": currency,
		"user_segment": _user_segment,
		"ads_removed": true,
		"previous_ads_purchased": get_previous_ads_purchased()
	}
	
	log_event("remove_ads_purchased", params)

## Track rewarded ad watched event
func track_rewarded_ad_watched(reward_type: String, reward_amount: float, ad_source: String = "admob"):
	if not _tracking_enabled:
		return
	
	var params = {
		"reward_type": reward_type,
		"reward_amount": reward_amount,
		"ad_source": ad_source,
		"user_segment": _user_segment,
		"ads_watched_today": get_ads_watched_today()
	}
	
	log_event("rewarded_ad_watched", params)

# ===============================================
# ENGAGEMENT EVENTS
# ===============================================

## Track daily login streak reached event
func track_daily_login_streak_reached(streak_days: int):
	if not _tracking_enabled:
		return
	
	var params = {
		"streak_days": streak_days,
		"user_segment": _user_segment,
		"longest_streak": get_longest_streak(),
		"is_new_record": streak_days > get_longest_streak()
	}
	
	log_event("daily_login_streak_reached", params)

## Track achievement unlocked event
func track_achievement_unlocked(achievement_id: String, achievement_type: String = "progressive"):
	if not _tracking_enabled:
		return
	
	var params = {
		"achievement_id": achievement_id,
		"achievement_type": achievement_type,
		"user_segment": _user_segment,
		"total_achievements": get_total_achievements(),
		"rarity": get_achievement_rarity(achievement_id)
	}
	
	log_event("achievement_unlocked", params)

## Track seasonal event started event
func track_seasonal_event_started(event_id: String, event_type: String):
	if not _tracking_enabled:
		return
	
	var params = {
		"event_id": event_id,
		"event_type": event_type,
		"user_segment": _user_segment,
		"participation_count": get_event_participation_count(event_id)
	}
	
	log_event("seasonal_event_started", params)

# ===============================================
# QUALITY EVENTS
# ===============================================

## Track crash detected event
func track_crash_detected(crash_type: String, scene_name: String = "", additional_info: String = ""):
	if not _tracking_enabled:
		return
	
	var session_duration = (Time.get_ticks_msec() - int(_session_start_time.get("hour", 0) * 3600000)) / 1000.0
	
	var params = {
		"crash_type": crash_type,
		"scene_name": scene_name,
		"additional_info": additional_info,
		"platform": OS.get_name(),
		"device_type": get_device_type(),
		"session_duration": session_duration
	}
	
	log_event("crash_detected", params)
	
	# Report to Firebase Crashlytics
	if has_node("/root/FirebaseManager"):
		var firebase = get_node("/root/FirebaseManager")
		firebase.report_crash(crash_type, additional_info, params)

## Track performance frame drop event
func track_performance_frame_drop(fps: float, frame_time: float):
	if not _tracking_enabled:
		return
	
	_low_fps_count += 1
	
	if _low_fps_count >= 5:  # Threshold for reporting
		var params = {
			"fps": fps,
			"frame_time": frame_time,
			"device_type": get_device_type(),
			"memory_usage": get_memory_usage(),
			"platform": OS.get_name()
		}
		
		log_event("performance_frame_drop", params)
		performance_issue_detected.emit("low_fps", fps)
		
		_low_fps_count = 0  # Reset counter

## Track memory usage warning
func track_memory_warning():
	if not _tracking_enabled:
		return
	
	var params = {
		"memory_usage_mb": get_memory_usage(),
		"device_type": get_device_type(),
		"platform": OS.get_name(),
		"active_objects": get_active_object_count()
	}
	
	log_event("memory_warning", params)
	performance_issue_detected.emit("high_memory", get_memory_usage())

# ===============================================
# HELPER FUNCTIONS
# ===============================================

## Get levels completed today
func get_levels_completed_today() -> int:
	# This would be tracked in PlayerProfile
	if has_node("/root/PlayerProfile"):
		var profile = get_node("/root/PlayerProfile")
		if profile.has_method("get_levels_completed_today"):
			return profile.get_levels_completed_today()
	return 0

## Get consecutive failures
func get_consecutive_failures() -> int:
	# This would be tracked in PlayerProfile
	if has_node("/root/PlayerProfile"):
		var profile = get_node("/root/PlayerProfile")
		if profile.has_method("get_consecutive_failures"):
			return profile.get_consecutive_failures()
	return 0

## Get level difficulty rating
func get_level_difficulty(level_number: int) -> int:
	# This would be calculated based on level data
	return min(5, ceil(level_number / 20.0))

## Get total perfect scores
func get_total_perfect_scores() -> int:
	if has_node("/root/PlayerProfile"):
		var profile = get_node("/root/PlayerProfile")
		if profile.has_method("get_total_perfect_scores"):
			return profile.get_total_perfect_scores()
	return 0

## Get total spent
func get_total_spent() -> float:
	if has_node("/root/MonetizationManager"):
		var monetization = get_node("/root/MonetizationManager")
		if monetization.has_method("get_total_spent"):
			return monetization.get_total_spent()
	return 0.0

## Get purchase source
func get_purchase_source() -> String:
	return "in_game"  # Could be "in_game", "web", "gift"

## Get total cosmetics unlocked
func get_total_cosmetics_unlocked() -> int:
	if has_node("/root/PlayerProfile"):
		var profile = get_node("/root/PlayerProfile")
		if profile.has_method("get_total_cosmetics_unlocked"):
			return profile.get_total_cosmetics_unlocked()
	return 0

## Get previous ads purchased
func get_previous_ads_purchased() -> bool:
	if has_node("/root/MonetizationManager"):
		var monetization = get_node("/root/MonetizationManager")
		if monetization.has_method("has_ads_removed"):
			return monetization.has_ads_removed()
	return false

## Get ads watched today
func get_ads_watched_today() -> int:
	if has_node("/root/AdsManager"):
		var ads = get_node("/root/AdsManager")
		if ads.has_method("get_ads_watched_today"):
			return ads.get_ads_watched_today()
	return 0

## Get longest streak
func get_longest_streak() -> int:
	if has_node("/root/PlayerProfile"):
		var profile = get_node("/root/PlayerProfile")
		if profile.has_method("get_longest_streak"):
			return profile.get_longest_streak()
	return 0

## Get total achievements
func get_total_achievements() -> int:
	if has_node("/root/PlayerProfile"):
		var profile = get_node("/root/PlayerProfile")
		if profile.has_method("get_total_achievements"):
			return profile.get_total_achievements()
	return 0

## Get achievement rarity
func get_achievement_rarity(achievement_id: String) -> String:
	# This would look up achievement data
	return "common"  # "common", "rare", "epic", "legendary"

## Get event participation count
func get_event_participation_count(event_id: String) -> int:
	# This would track event participation
	return 1

## Get device type
func get_device_type() -> String:
	var platform = OS.get_name()
	if platform == "Android" or platform == "iOS":
		return "mobile"
	return "desktop"

## Get memory usage in MB
func get_memory_usage() -> float:
	# Approximate memory usage
	return OS.get_static_memory_usage_by_type(0) / 1024.0 / 1024.0

## Get active object count
func get_active_object_count() -> int:
	return Performance.get_monitor(Performance.OBJECT_COUNT)

## Check for rage quit pattern
func check_rage_quit_pattern(level_number: int):
	if get_consecutive_failures() >= 3:
		# Track rage quit potential
		var params = {
			"level_number": level_number,
			"consecutive_failures": get_consecutive_failures(),
			"user_segment": _user_segment
		}
		log_event("rage_quit_potential", params)

## Get event count for a specific event
func get_event_count(event_name: String) -> int:
	if _event_counts.has(event_name):
		return _event_counts[event_name]
	return 0

## Get all event counts
func get_all_event_counts() -> Dictionary:
	return _event_counts.duplicate()

## Clear event counts
func clear_event_counts():
	_event_counts.clear()
