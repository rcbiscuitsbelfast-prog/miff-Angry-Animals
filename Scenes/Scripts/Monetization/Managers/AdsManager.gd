extends Node
class_name AdsManager

## Global ads manager responsible for initializing and showing ads.
## Designed to integrate with AdMob via platform-specific Godot plugins.
## On unsupported platforms (Windows/macOS/Linux/Web) this manager becomes a no-op.

signal ad_closed
signal ad_clicked
signal reward_earned
signal banner_inset_changed(inset_px: int)

enum BannerPlacement {
    BOTTOM = 0
    TOP = 1
}

var ad_mob_app_id: String = ""
var android_ad_mob_app_id: String = ""
var ios_ad_mob_app_id: String = ""
var banner_ad_unit_id: String = "ca-app-pub-6675121744131727/8033303534"
var interstitial_ad_unit_id: String = "ca-app-pub-6675121744131727/8410569879"
var rewarded_ad_unit_id: String = ""
var banner_position: BannerPlacement = BannerPlacement.BOTTOM
var persistent_banner_enabled: bool = true
var enable_banner_auto_refresh: bool = true
var banner_refresh_seconds: int = 30
var banner_height_px: int = 50
var show_editor_placeholder_banner: bool = true

var interstitial_cooldown_seconds: float = 45.0
var enable_interstitial_preloading: bool = true

var _ad_plugin: Object = null
var _initialized: bool = false
var _banner_visible: bool = false
var _interstitial_ready: bool = false
var _rewarded_ready: bool = false
var _banner_refresh_timer: Timer = null
var _interstitial_cooldown_timer: Timer = null
var _placeholder_layer: CanvasLayer = null
var _placeholder_banner: Control = null
var _last_banner_inset: int = 0
var _last_interstitial_shown_time: int = 0

static var instance: AdsManager = null

func get_current_banner_inset_px() -> int:
    return _banner_visible ? banner_height_px : 0

func _ready() -> void:
    instance = self
    process_mode = Node.PROCESS_MODE_ALWAYS
    _apply_banner_settings_from_project_settings()
    _setup_placeholder_banner_if_needed()
    call_deferred("_ensure_persistent_banner_if_possible")

func _exit_tree() -> void:
    _stop_banner_refresh_timer()
    _stop_interstitial_cooldown_timer()
    _destroy_banner()

## Initializes the underlying AdMob plugin (if available) with the provided IDs.
## This is safe to call multiple times.
func initialize(ad_mob_app_id_param: String, banner_ad_unit_id_param: String, interstitial_ad_unit_id_param: String, rewarded_ad_unit_id_param: String) -> void:
    var resolved_app_id = _resolve_ad_mob_app_id(ad_mob_app_id_param)
    
    ad_mob_app_id = resolved_app_id if resolved_app_id else ad_mob_app_id
    banner_ad_unit_id = banner_ad_unit_id_param if banner_ad_unit_id_param else banner_ad_unit_id
    interstitial_ad_unit_id = interstitial_ad_unit_id_param if interstitial_ad_unit_id_param else interstitial_ad_unit_id
    rewarded_ad_unit_id = rewarded_ad_unit_id_param if rewarded_ad_unit_id_param else rewarded_ad_unit_id
    
    _apply_banner_settings_from_project_settings()
    
    if not _is_platform_supported():
        _initialized = false
        _ad_plugin = null
        _update_placeholder_visibility()
        return
    
    _ad_plugin = _find_ad_plugin_singleton()
    if _ad_plugin == null:
        push_warning("AdsManager: No AdMob plugin singleton found. Ads are disabled.")
        _initialized = false
        _update_placeholder_visibility()
        return
    
    _try_call_plugin("initialize", [ad_mob_app_id])
    _try_call_plugin("init", [ad_mob_app_id])
    _try_call_plugin("set_app_id", [ad_mob_app_id])
    _try_call_plugin("setAppId", [ad_mob_app_id])
    
    _initialized = true
    
    await _load_ads_async()
    call_deferred("_ensure_persistent_banner_if_possible")

## Shows a banner ad.
## Does nothing when ads are unavailable.
func show_banner_ad() -> void:
    if not _is_ready_for_showing_ads() or not _should_show_ads():
        return
    
    if _banner_visible:
        return
    
    _configure_banner_position_and_size()
    
    var shown = false
    if banner_ad_unit_id:
        shown = (_try_call_plugin("show_banner", [banner_ad_unit_id]) or
                _try_call_plugin("showBanner", [banner_ad_unit_id]) or
                _try_call_plugin("show_banner_ad", [banner_ad_unit_id]) or
                _try_call_plugin("show_banner_ad_unit", [banner_ad_unit_id]))
    else:
        shown = (_try_call_plugin("show_banner") or
                _try_call_plugin("showBanner") or
                _try_call_plugin("show_banner_ad"))
    
    _banner_visible = shown
    _update_banner_inset()
    
    if _banner_visible:
        _start_banner_refresh_timer_if_needed()

## Hides the banner ad.
func hide_banner_ad() -> void:
    _banner_visible = false
    _stop_banner_refresh_timer()
    _update_banner_inset()
    
    if not _is_platform_supported():
        _update_placeholder_visibility()
        return
    
    _try_call_plugin("hide_banner")
    _try_call_plugin("hideBanner")
    _try_call_plugin("hide_banner_ad")

## Pauses/resumes manual banner refresh. (Optional optimization for pause menus.)
func set_banner_refresh_paused(paused: bool) -> void:
    if _banner_refresh_timer == null:
        return
    
    _banner_refresh_timer.paused = paused

## Shows an interstitial ad. If no ad is available, emits ad_closed on the next frame.
func show_interstitial_ad() -> void:
    if not _is_ready_for_showing_ads() or not _should_show_ads():
        await _emit_ad_closed_next_frame_async()
        return
    
    if not _can_show_interstitial():
        print("Interstitial ad skipped - cooldown active (%0.1fs remaining)" % _get_remaining_cooldown_seconds())
        await _emit_ad_closed_next_frame_async()
        return
    
    if not _interstitial_ready:
        await _load_interstitial_async()
    
    if not _interstitial_ready:
        await _emit_ad_closed_next_frame_async()
        return
    
    var shown = (_try_call_plugin("show_interstitial") or
                _try_call_plugin("showInterstitial") or
                _try_call_plugin("show_interstitial_ad"))
    
    if not shown:
        await _emit_ad_closed_next_frame_async()
        return
    
    _last_interstitial_shown_time = Time.get_ticks_msec()
    _start_interstitial_cooldown_timer()
    
    await _wait_for_ad_closed_or_timeout_async(10.0)
    
    _interstitial_ready = false
    if enable_interstitial_preloading:
        _load_interstitial_async()

## Manually load an interstitial ad in the background.
func load_interstitial_ad() -> void:
    await _load_interstitial_async()

## Check if an interstitial ad is currently loaded and ready to show.
func is_interstitial_ready() -> bool:
    return _interstitial_ready and _can_show_interstitial()

## Force reset the interstitial cooldown timer. Use with caution - only for testing.
func reset_interstitial_cooldown() -> void:
    _last_interstitial_shown_time = 0
    if _interstitial_cooldown_timer != null:
        _interstitial_cooldown_timer.stop()
        _interstitial_cooldown_timer.queue_free()
        _interstitial_cooldown_timer = null

## Get remaining cooldown time in seconds for interstitial ads.
func get_remaining_cooldown_seconds() -> float:
    var elapsed = (Time.get_ticks_msec() - _last_interstitial_shown_time) / 1000.0
    return maxf(0.0, interstitial_cooldown_seconds - elapsed)

## Shows a rewarded video ad. Emits reward_earned when a reward is granted by the ad network.
## If rewarded ads are unavailable, the call completes without throwing.
func show_rewarded_ad() -> void:
    if not _is_ready_for_showing_ads():
        await _emit_ad_closed_next_frame_async()
        return
    
    if not _rewarded_ready:
        await _load_rewarded_async()
    
    if not _rewarded_ready:
        await _emit_ad_closed_next_frame_async()
        return
    
    var shown = (_try_call_plugin("show_rewarded") or
                _try_call_plugin("showRewarded") or
                _try_call_plugin("show_rewarded_ad"))
    
    if not shown:
        await _emit_ad_closed_next_frame_async()
        return
    
    await _wait_for_ad_closed_or_timeout_async(15.0)
    
    _rewarded_ready = false
    _load_rewarded_async()

## Returns whether any full-screen ad is currently ready (interstitial or rewarded).
func is_ad_ready() -> bool:
    return _interstitial_ready or _rewarded_ready

## Returns whether ads should be shown based on premium status.
## Ads are hidden if user has purchased either full game unlock or remove ads.
func should_show_ads() -> bool:
    var monetization_manager = get_node_or_null("/root/MonetizationManager")
    var premium_manager = get_node_or_null("/root/PremiumManager")
    
    var is_full_game_unlocked = false
    if monetization_manager and monetization_manager.has_method("get_is_full_game_unlocked"):
        is_full_game_unlocked = monetization_manager.get_is_full_game_unlocked()
    
    var is_ad_free = false
    if premium_manager and premium_manager.has_method("is_ad_free_version"):
        is_ad_free = premium_manager.is_ad_free_version()
    
    return not is_full_game_unlocked and not is_ad_free

## Callback hook for plugins to notify that an ad was closed.
## This method is safe to call from platform code via Callable/Call.
func notify_ad_closed() -> void:
    ad_closed.emit()

## Callback hook for plugins to notify that an ad was clicked.
func notify_ad_clicked() -> void:
    ad_clicked.emit()

## Callback hook for plugins to notify that a reward has been earned.
func notify_reward_earned() -> void:
    reward_earned.emit()

## Private methods

func _ensure_persistent_banner_if_possible() -> void:
    if not persistent_banner_enabled:
        hide_banner_ad()
        return
    
    if not should_show_ads():
        hide_banner_ad()
        return
    
    _update_placeholder_visibility()
    
    if not _is_ready_for_showing_ads():
        return
    
    _ensure_banner_loaded_and_shown_async()

func _ensure_banner_loaded_and_shown_async() -> void:
    if _banner_visible:
        return
    
    if persistent_banner_enabled:
        show_banner_ad()

func _apply_banner_settings_from_project_settings() -> void:
    if ProjectSettings.has_setting("monetization/admob/banner_ad_unit_id"):
        banner_ad_unit_id = ProjectSettings.get_setting("monetization/admob/banner_ad_unit_id", banner_ad_unit_id)
    if ProjectSettings.has_setting("monetization/admob/interstitial_ad_unit_id"):
        interstitial_ad_unit_id = ProjectSettings.get_setting("monetization/admob/interstitial_ad_unit_id", interstitial_ad_unit_id)
    if ProjectSettings.has_setting("monetization/admob/rewarded_ad_unit_id"):
        rewarded_ad_unit_id = ProjectSettings.get_setting("monetization/admob/rewarded_ad_unit_id", rewarded_ad_unit_id)
    if ProjectSettings.has_setting("monetization/admob/banner_position"):
        var position_str = ProjectSettings.get_setting("monetization/admob/banner_position", "bottom")
        banner_position = BannerPlacement.TOP if position_str == "top" else BannerPlacement.BOTTOM
    if ProjectSettings.has_setting("monetization/admob/persistent_banner"):
        persistent_banner_enabled = ProjectSettings.get_setting("monetization/admob/persistent_banner", true)
    if ProjectSettings.has_setting("monetization/admob/banner_auto_refresh"):
        enable_banner_auto_refresh = ProjectSettings.get_setting("monetization/admob/banner_auto_refresh", true)
    if ProjectSettings.has_setting("monetization/admob/banner_refresh_seconds"):
        banner_refresh_seconds = ProjectSettings.get_setting("monetization/admob/banner_refresh_seconds", 30)
    if ProjectSettings.has_setting("monetization/admob/banner_height_px"):
        banner_height_px = ProjectSettings.get_setting("monetization/admob/banner_height_px", 50)

func _setup_placeholder_banner_if_needed() -> void:
    if not show_editor_placeholder_banner:
        return
    
    _placeholder_layer = CanvasLayer.new()
    _placeholder_layer.name = "AdPlaceholderLayer"
    
    _placeholder_banner = ColorRect.new()
    _placeholder_banner.name = "AdPlaceholder"
    _placeholder_banner.color = Color(0.8, 0.8, 0.8, 0.5)
    _placeholder_banner.set_anchors_and_offsets_preset(Control.PRESET_BOTTOM_WIDE)
    _placeholder_banner.offset_bottom = banner_height_px
    _placeholder_banner.visible = false
    
    _placeholder_layer.add_child(_placeholder_banner)
    get_tree().current_scene.add_child(_placeholder_layer)

func _destroy_banner() -> void:
    if _placeholder_banner != null:
        _placeholder_banner.queue_free()
        _placeholder_banner = null
    if _placeholder_layer != null:
        _placeholder_layer.queue_free()
        _placeholder_layer = null

func _update_placeholder_visibility() -> void:
    if _placeholder_banner == null:
        return
    
    var should_show = show_editor_placeholder_banner and not _is_platform_supported()
    _placeholder_banner.visible = should_show and _banner_visible

func _configure_banner_position_and_size() -> void:
    if banner_position == BannerPlacement.TOP:
        _try_call_plugin("set_banner_position", ["top"])
    else:
        _try_call_plugin("set_banner_position", ["bottom"])
    
    _try_call_plugin("set_banner_size", ["BANNER"])

func _update_banner_inset() -> void:
    var inset = get_current_banner_inset_px()
    if inset != _last_banner_inset:
        _last_banner_inset = inset
        banner_inset_changed.emit(inset)

func _start_banner_refresh_timer_if_needed() -> void:
    if not enable_banner_auto_refresh:
        return
    
    if _banner_refresh_timer == null:
        _banner_refresh_timer = Timer.new()
        _banner_refresh_timer.wait_time = banner_refresh_seconds
        _banner_refresh_timer.one_shot = false
        _banner_refresh_timer.timeout.connect(_on_banner_refresh_timeout)
        add_child(_banner_refresh_timer)
    
    _banner_refresh_timer.start()

func _stop_banner_refresh_timer() -> void:
    if _banner_refresh_timer != null:
        _banner_refresh_timer.stop()

func _on_banner_refresh_timeout() -> void:
    if _banner_visible:
        _try_call_plugin("reload_banner")

func _start_interstitial_cooldown_timer() -> void:
    if _interstitial_cooldown_timer == null:
        _interstitial_cooldown_timer = Timer.new()
        _interstitial_cooldown_timer.wait_time = interstitial_cooldown_seconds
        _interstitial_cooldown_timer.one_shot = true
        _interstitial_cooldown_timer.timeout.connect(_on_interstitial_cooldown_complete)
        add_child(_interstitial_cooldown_timer)
    
    _interstitial_cooldown_timer.start()

func _stop_interstitial_cooldown_timer() -> void:
    if _interstitial_cooldown_timer != null:
        _interstitial_cooldown_timer.stop()

func _on_interstitial_cooldown_complete() -> void:
    print("Interstitial cooldown complete")

func _can_show_interstitial() -> bool:
    return get_remaining_cooldown_seconds() <= 0.0

func _is_ready_for_showing_ads() -> bool:
    return _initialized and _ad_plugin != null

func _should_show_ads() -> bool:
    return should_show_ads()

func _resolve_ad_mob_app_id(ad_mob_app_id_param: String) -> String:
    if not ad_mob_app_id_param:
        var os_name = OS.get_name()
        if os_name == "Android" and android_ad_mob_app_id:
            return android_ad_mob_app_id
        elif os_name == "iOS" and ios_ad_mob_app_id:
            return ios_ad_mob_app_id
    
    return ad_mob_app_id_param

func _find_ad_plugin_singleton() -> Object:
    var candidates = ["AdMob", "GodotGoogleMobileAds", "MobileAds", "AdMobWrapper"]
    
    for name in candidates:
        if Engine.has_singleton(name):
            return Engine.get_singleton(name)
    
    return null

func _try_call_plugin(method_name: String, args: Array = []) -> bool:
    if _ad_plugin == null:
        return false
    
    if not _ad_plugin.has_method(method_name):
        return false
    
    _ad_plugin.callv(method_name, args)
    return true

func _load_ads_async() -> void:
    await _load_banner_async()
    await _load_interstitial_async()
    await _load_rewarded_async()

func _load_banner_async() -> void:
    _try_call_plugin("load_banner", [banner_ad_unit_id])

func _load_interstitial_async() -> void:
    _interstitial_ready = false
    _try_call_plugin("load_interstitial", [interstitial_ad_unit_id])
    _interstitial_ready = true

func _load_rewarded_async() -> void:
    _rewarded_ready = false
    _try_call_plugin("load_rewarded", [rewarded_ad_unit_id])
    _rewarded_ready = true

func _emit_ad_closed_next_frame_async() -> void:
    await get_tree().process_frame
    ad_closed.emit()

func _wait_for_ad_closed_or_timeout_async(timeout_seconds: float) -> void:
    var timer = Timer.new()
    timer.wait_time = timeout_seconds
    timer.one_shot = true
    add_child(timer)
    timer.start()
    
    await Promise.any([Promise.from_signal(self, "ad_closed"), Promise.from_signal(timer, "timeout")])
    
    timer.queue_free()

func _is_platform_supported() -> bool:
    var os_name = OS.get_name()
    return os_name == "Android" or os_name == "iOS"
