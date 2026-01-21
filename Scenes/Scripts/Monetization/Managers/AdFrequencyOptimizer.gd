extends Node
class_name AdFrequencyOptimizer

## Intelligent ad placement optimizer using A/B testing to balance revenue and retention
## Implements smart placement strategies based on player behavior and game state

signal ad_strategy_changed(new_strategy)
signal ad_placement_optimized(placement_reason: String, ad_shown: bool)
signal ad_frequency_metrics_updated(metrics: Dictionary)

enum AdStrategyType {
	AGGRESSIVE,
	BALANCED,
	CONSERVATIVE
}

enum AdType {
	BANNER,
	INTERSTITIAL,
	REWARDED
}

var _ad_strategies: Dictionary = {}
var _current_strategy: AdStrategyType = AdStrategyType.BALANCED
var _current_config: Dictionary = {}
var _last_ad_shown_time: int = 0
var _ads_shown_this_session: int = 0
var _recent_ads: Array = []
var _session_play_time: float = 0.0
var _player_is_frustrated: bool = false

var _quiet_start_hour: int = 22  # 10 PM
var _quiet_end_hour: int = 8    # 8 AM
var _max_ads_per_30_min: int = 3
var _min_ad_interval_seconds: float = 60.0

static var instance: AdFrequencyOptimizer = null

const QUIET_START_KEY = "ad_frequency/quiet_start_hour"
const QUIET_END_KEY = "ad_frequency/quiet_end_hour"
const MAX_ADS_KEY = "ad_frequency/max_ads_per_30_min"
const MIN_INTERVAL_KEY = "ad_frequency/min_ad_interval_seconds"

func _ready() -> void:
	if instance != null:
		queue_free()
		return
	
	instance = self
	_initialize_ad_optimizer()

## Initialize ad frequency optimizer
func _initialize_ad_optimizer() -> void:
	_initialize_ad_strategies()
	_load_current_strategy()
	_reset_session_metrics()
	
	print("Ad Frequency Optimizer initialized")

## Initialize ad strategy configurations
func _initialize_ad_strategies() -> void:
	_ad_strategies = {
		AdStrategyType.AGGRESSIVE: {
			"strategy_name": "Aggressive",
			"interstitial_frequency": 2,
			"rewarded_prominence": 0.9,
			"banner_always_visible": true,
			"expected_arppu": 8.50,
			"expected_retention_drop": 0.15,
			"description": "Maximum revenue, may hurt retention"
		},
		AdStrategyType.BALANCED: {
			"strategy_name": "Balanced",
			"interstitial_frequency": 5,
			"rewarded_prominence": 0.6,
			"banner_always_visible": false,
			"expected_arppu": 6.20,
			"expected_retention_drop": 0.05,
			"description": "Good balance of revenue and retention"
		},
		AdStrategyType.CONSERVATIVE: {
			"strategy_name": "Conservative",
			"interstitial_frequency": 8,
			"rewarded_prominence": 0.3,
			"banner_always_visible": false,
			"expected_arppu": 4.80,
			"expected_retention_drop": 0.01,
			"description": "Best retention, lower revenue"
		}
	}

## Load current strategy (from player preferences or A/B testing)
func _load_current_strategy() -> void:
	# Check A/B testing assignment first
	var ab_variant = _get_ab_variant("ad_frequency_test")
	
	if ab_variant:
		match ab_variant.to_lower():
			"control":
				_current_strategy = AdStrategyType.BALANCED
			"variant_1":
				_current_strategy = AdStrategyType.AGGRESSIVE
			"variant_2":
				_current_strategy = AdStrategyType.CONSERVATIVE
			_:
				_current_strategy = AdStrategyType.BALANCED
	else:
		# Default to balanced strategy
		_current_strategy = AdStrategyType.BALANCED
	
	_current_config = _ad_strategies[_current_strategy]
	
	print("Using ad strategy: %s" % _current_config["strategy_name"])

## Get A/B variant from testing system (placeholder for now)
func _get_ab_variant(test_id: String) -> String:
	# In the full implementation, this would query ABTestingManager
	# For now, return null to use default strategy
	return ""

## Reset session metrics
func _reset_session_metrics() -> void:
	_last_ad_shown_time = 0
	_ads_shown_this_session = 0
	_recent_ads.clear()
	_session_play_time = 0.0
	_player_is_frustrated = false

## Update session metrics
func _process(delta: float) -> void:
	_session_play_time += delta
	
	# Clean up old ad timestamps (older than 30 minutes)
	var now = Time.get_ticks_msec()
	var thirty_min_ms = 30 * 60 * 1000
	_recent_ads = _recent_ads.filter(func(timestamp): return now - timestamp < thirty_min_ms)
	
	# Check for player frustration
	_check_player_frustration()
	
	# Update metrics periodically
	if int(Time.get_ticks_msec() / 30000) % 2 == 1:  # Every 30 seconds
		_update_ad_frequency_metrics()

## Check if player is frustrated based on recent behavior
func _check_player_frustration() -> void:
	# Simple frustration indicators:
	# - Rapid level failures
	# - Short session times
	# - Quick app switching
	
	# For now, use a simple heuristic
	var recent_failures = 0
	if recent_failures >= 3:
		_player_is_frustrated = true

## Determine if an interstitial ad should be shown
func should_show_interstitial_ad(game_state: String, levels_completed: int) -> bool:
	# Check quiet hours
	if _is_quiet_hours():
		return false
	
	# Check ad frequency limits
	if _is_ad_limit_reached():
		return false
	
	# Check interval since last ad
	var now = Time.get_ticks_msec()
	if now - _last_ad_shown_time < _min_ad_interval_seconds * 1000:
		return false
	
	# Don't show ads when player is frustrated
	if _player_is_frustrated and game_state == "level_failed":
		return false
	
	# Check strategic frequency
	var frequency = _current_config.get("interstitial_frequency", 5)
	if levels_completed % frequency != 0:
		return false
	
	# Don't show ads immediately after starting the game
	if _session_play_time < 60.0:  # First minute
		return false
	
	# A/B test: some variants may disable certain ad types
	var ad_variant = _get_ab_variant("ad_frequency_test")
	if ad_variant == "variant_2" and _current_strategy == AdStrategyType.CONSERVATIVE and levels_completed < 10:
		return false  # Conservative variant delays early ads
	
	return true

## Determine if rewarded ad should be prominently displayed
func get_rewarded_ad_prominence() -> float:
	# Base prominence from strategy
	var prominence = _current_config.get("rewarded_prominence", 0.6)
	
	# Reduce prominence if player is frustrated
	if _player_is_frustrated:
		prominence *= 0.5
	
	# Reduce prominence during quiet hours
	if _is_quiet_hours():
		prominence *= 0.3
	
	# A/B test modifier
	var ad_variant = _get_ab_variant("ad_frequency_test")
	if ad_variant == "variant_1" and _current_strategy == AdStrategyType.AGGRESSIVE:
		prominence = minf(prominence * 1.2, 1.0)  # Boost in aggressive variant
	
	return clampf(prominence, 0.0, 1.0)

## Determine if banner ad should be visible
func should_show_banner_ad() -> bool:
	# Always check quiet hours
	if _is_quiet_hours():
		return false
	
	# Strategy-based visibility
	if not _current_config.get("banner_always_visible", false):
		return false
	
	# Don't show banners when frustrated
	if _player_is_frustrated:
		return false
	
	# A/B test consideration
	var ad_variant = _get_ab_variant("ad_frequency_test")
	if ad_variant == "control" and _current_strategy == AdStrategyType.BALANCED:
		return false  # Control group hides banners
	
	return true

## Record that an ad was shown
func record_ad_shown(ad_type: AdType) -> void:
	_last_ad_shown_time = Time.get_ticks_msec()
	_ads_shown_this_session += 1
	_recent_ads.append(Time.get_ticks_msec())
	
	# Track in analytics (placeholder)
	_analytics_track_ad_shown(ad_type)
	
	ad_placement_optimized.emit("ad_shown", true)

## Record ad completion (for revenue optimization)
func record_ad_completed(ad_type: AdType) -> void:
	# Track in analytics (placeholder)
	_analytics_track_ad_completed(ad_type)

## Record ad skip (for placement optimization)
func record_ad_skipped(ad_type: AdType) -> void:
	# Track in analytics (placeholder)
	_analytics_track_ad_skipped(ad_type)

## Check if current time is within quiet hours
func _is_quiet_hours() -> bool:
	var datetime = Time.get_datetime_dict_from_system()
	var current_hour = datetime.hour
	
	if _quiet_start_hour > _quiet_end_hour:
		# Quiet hours span midnight
		return current_hour >= _quiet_start_hour or current_hour <= _quiet_end_hour
	else:
		# Normal quiet hours
		return current_hour >= _quiet_start_hour and current_hour <= _quiet_end_hour

## Check if ad frequency limit is reached
func _is_ad_limit_reached() -> bool:
	return _recent_ads.size() >= _max_ads_per_30_min

## Update ad frequency metrics
func _update_ad_frequency_metrics() -> void:
	var ads_per_hour = 0.0
	if _session_play_time > 0:
		ads_per_hour = float(_ads_shown_this_session) / (_session_play_time / 3600.0)
	
	var last_ad_ago_minutes = -1
	if _last_ad_shown_time > 0:
		last_ad_ago_minutes = int((Time.get_ticks_msec() - _last_ad_shown_time) / 1000 / 60)
	
	var metrics = {
		"current_strategy": _current_strategy,
		"ads_shown_this_session": _ads_shown_this_session,
		"ads_per_hour": ads_per_hour,
		"player_frustrated": _player_is_frustrated,
		"quiet_hours": _is_quiet_hours(),
		"last_ad_ago_minutes": last_ad_ago_minutes,
		"recent_ads_count": _recent_ads.size(),
		"interstitial_frequency": _current_config.get("interstitial_frequency", 5),
		"rewarded_prominence": get_rewarded_ad_prominence()
	}
	
	ad_frequency_metrics_updated.emit(metrics)

## Get current ad strategy configuration
func get_current_strategy_config() -> Dictionary:
	return _current_config.duplicate()

## Get all available strategies
func get_all_strategies() -> Dictionary:
	return _ad_strategies.duplicate()

## Switch to a different ad strategy (for testing)
func switch_strategy(new_strategy: AdStrategyType) -> void:
	if not _ad_strategies.has(new_strategy):
		return
	
	_current_strategy = new_strategy
	_current_config = _ad_strategies[new_strategy]
	
	print("Switched to ad strategy: %s" % _current_config["strategy_name"])
	ad_strategy_changed.emit(new_strategy)

## Get optimal ad placement recommendations
func get_ad_placement_recommendations() -> Array:
	var recommendations = []
	
	# Strategy-specific recommendations
	match _current_strategy:
		AdStrategyType.AGGRESSIVE:
			recommendations.append("Consider reducing interstitial frequency if retention drops")
			recommendations.append("Monitor user feedback for ad fatigue")
		AdStrategyType.BALANCED:
			recommendations.append("Good baseline strategy - monitor metrics for optimization")
			recommendations.append("Test banner ad visibility in future A/B tests")
		AdStrategyType.CONSERVATIVE:
			recommendations.append("Consider testing slightly higher frequency for revenue")
			recommendations.append("Monitor if players complete more levels with fewer ads")
	
	# Player behavior recommendations
	if _player_is_frustrated:
		recommendations.append("Player appears frustrated - reduce ad frequency temporarily")
	
	return recommendations

## Analytics tracking (placeholder methods)
func _analytics_track_ad_shown(ad_type: AdType) -> void:
	var event_tracker = get_node_or_null("/root/AnalyticsEventTracker")
	if event_tracker and event_tracker.has_method("track_event"):
		event_tracker.track_event("ad_shown", {
			"ad_type": str(ad_type),
			"strategy": str(_current_strategy),
			"player_frustrated": _player_is_frustrated,
			"session_time": _session_play_time,
			"ads_this_session": _ads_shown_this_session
		})

func _analytics_track_ad_completed(ad_type: AdType) -> void:
	var event_tracker = get_node_or_null("/root/AnalyticsEventTracker")
	if event_tracker and event_tracker.has_method("track_event"):
		event_tracker.track_event("ad_completed", {
			"ad_type": str(ad_type),
			"strategy": str(_current_strategy)
		})

func _analytics_track_ad_skipped(ad_type: AdType) -> void:
	var event_tracker = get_node_or_null("/root/AnalyticsEventTracker")
	if event_tracker and event_tracker.has_method("track_event"):
		event_tracker.track_event("ad_skipped", {
			"ad_type": str(ad_type),
			"strategy": str(_current_strategy)
		})
