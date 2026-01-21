extends Node
class_name RewardedAdManager

## Core handler for rewarded video ads.
## Manages lifecycle, preloading, and callback handling for rewards.

signal reward_granted
signal reward_failed(reason: String)

var rewarded_ad_unit_id: String = "ca-app-pub-6675121744131727/8406522837"
var preload_on_startup: bool = true

var _is_ad_loaded: bool = false
var _is_ad_showing: bool = false
var _on_reward_callback: Callable = null

static var instance: RewardedAdManager = null

func _ready() -> void:
	instance = self
	process_mode = Node.PROCESS_MODE_ALWAYS
	
	if preload_on_startup:
		initialize()

## Initialize the rewarded ad manager.
func initialize() -> void:
	print("RewardedAdManager: Initializing...")
	preload()

## Preload the rewarded ad.
func preload() -> void:
	if _is_ad_loaded or _is_ad_showing:
		return
	_load_rewarded_ad()

## Load a rewarded ad.
func _load_rewarded_ad() -> void:
	var ads_manager = get_node_or_null("/root/AdsManager")
	
	print("RewardedAdManager: Loading ad %s..." % rewarded_ad_unit_id)
	
	if ads_manager and ads_manager.has_method("load_interstitial_ad"):
		# Use AdsManager if available - it handles platform-specific loading
		await ads_manager.load_interstitial_ad()
		_is_ad_loaded = true
		print("RewardedAdManager: Ad loaded and ready.")
	else:
		# Fallback simulation for editor or missing AdsManager
		await get_tree().create_timer(1.0).timeout
		_is_ad_loaded = true
		print("RewardedAdManager: Ad loaded and ready (simulated).")

## Show a rewarded ad with a callback.
func show_rewarded_ad(callback: Callable) -> void:
	if _is_ad_showing:
		push_warning("RewardedAdManager: Ad is already showing")
		return
	
	_on_reward_callback = callback
	
	if not is_rewarded_ad_ready():
		push_warning("RewardedAdManager: Ad not ready, attempting to load...")
		_load_rewarded_ad()
		if not is_rewarded_ad_ready():
			reward_failed.emit("Ad failed to load")
			return
	
	_is_ad_showing = true
	print("RewardedAdManager: Showing ad...")
	
	var ads_manager = get_node_or_null("/root/AdsManager")
	if ads_manager and ads_manager.has_method("show_rewarded_ad"):
		# Connect to reward signal
		ads_manager.reward_earned.connect(_on_user_earned_reward)
		ads_manager.ad_closed.connect(_on_ad_closed)
		
		await ads_manager.show_rewarded_ad()
	else:
		# Fallback for editor or missing AdsManager
		print("RewardedAdManager: AdsManager not found, simulating reward in editor...")
		await get_tree().create_timer(1.0).timeout
		_on_user_earned_reward()
		_on_ad_closed()

## Check if rewarded ad is ready to show.
func is_rewarded_ad_ready() -> bool:
	return _is_ad_loaded

## Handle user earned a reward.
func _on_user_earned_reward() -> void:
	print("RewardedAdManager: User earned reward!")
	
	reward_granted.emit()
	
	# Call the provided callback
	if _on_reward_callback != null:
		_on_reward_callback.call()
	
	# Track analytics
	_analytics_track_reward_earned()

## Handle ad closed.
func _on_ad_closed() -> void:
	print("RewardedAdManager: Ad closed.")
	_is_ad_showing = false
	_is_ad_loaded = false
	
	var ads_manager = get_node_or_null("/root/AdsManager")
	if ads_manager:
		# Disconnect signals
		if ads_manager.reward_earned.is_connected(_on_user_earned_reward):
			ads_manager.reward_earned.disconnect(_on_user_earned_reward)
		if ads_manager.ad_closed.is_connected(_on_ad_closed):
			ads_manager.ad_closed.disconnect(_on_ad_closed)
	
	# Track analytics
	_analytics_track_ad_closed()
	
	# Preload next ad
	preload()

## Analytics tracking (placeholder).
func _analytics_track_reward_earned() -> void:
	var event_tracker = get_node_or_null("/root/AnalyticsEventTracker")
	if event_tracker and event_tracker.has_method("track_event"):
		event_tracker.track_event("reward_earned", {
			"ad_unit_id": rewarded_ad_unit_id
		})

func _analytics_track_ad_closed() -> void:
	var event_tracker = get_node_or_null("/root/AnalyticsEventTracker")
	if event_tracker and event_tracker.has_method("track_event"):
		event_tracker.track_event("rewarded_ad_closed", {
			"ad_unit_id": rewarded_ad_unit_id
		})
