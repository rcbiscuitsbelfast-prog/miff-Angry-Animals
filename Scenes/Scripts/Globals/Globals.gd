extends Node
class_name Globals

const DEFAULT_FADE_SECONDS: float = 0.25

var _fade_layer: CanvasLayer
var _fade_rect: ColorRect
var _is_transitioning: bool = false
var music_player: AudioStreamPlayer

func _ready() -> void:
    process_mode = Node.PROCESS_MODE_ALWAYS
    _ensure_audio_buses()
    _setup_music_player()
    _ensure_fade_overlay()
    _initialize_monetization.call_deferred()

func _initialize_monetization() -> void:
    var monetization = get_node_or_null("/root/MonetizationManager")
    if monetization:
        monetization.initialize(
            _read_project_setting_string("monetization/iap/ios_product_id", "full_game_unlock"),
            _read_project_setting_string("monetization/iap/android_product_id", "full_game_unlock")
        )
    
    var ads = get_node_or_null("/root/AdsManager")
    if ads:
        ads.initialize(
            _read_project_setting_string("monetization/admob/app_id", ""),
            _read_project_setting_string("monetization/admob/banner_ad_unit_id", ""),
            _read_project_setting_string("monetization/admob/interstitial_ad_unit_id", ""),
            _read_project_setting_string("monetization/admob/rewarded_ad_unit_id", "")
        )

func _read_project_setting_string(key: String, fallback: String) -> String:
    if not ProjectSettings.has_setting(key):
        return fallback
    var value = str(ProjectSettings.get_setting(key))
    if value.strip_edges() == "":
        return fallback
    return value

func _ensure_audio_buses() -> void:
    _ensure_bus("Music", "Master")
    _ensure_bus("SFX", "Master")

func _ensure_bus(bus_name: String, send_to: String) -> void:
    var bus_index = AudioServer.get_bus_index(bus_name)
    if bus_index == -1:
        AudioServer.add_bus()
        bus_index = AudioServer.bus_count - 1
        AudioServer.set_bus_name(bus_index, bus_name)
        AudioServer.set_bus_send(bus_index, send_to)

func _setup_music_player() -> void:
    music_player = AudioStreamPlayer.new()
    music_player.name = "MusicPlayer"
    music_player.bus = "Music"
    music_player.autoplay = true
    add_child(music_player)

func _ensure_fade_overlay() -> void:
    if _fade_layer and _fade_rect:
        return
    
    _fade_layer = CanvasLayer.new()
    _fade_layer.name = "FadeLayer"
    
    _fade_rect = ColorRect.new()
    _fade_rect.name = "FadeRect"
    _fade_rect.color = Color(0, 0, 0, 0)
    _fade_rect.mouse_filter = Control.MOUSE_FILTER_IGNORE
    _fade_rect.anchor_left = 0
    _fade_rect.anchor_top = 0
    _fade_rect.anchor_right = 1
    _fade_rect.anchor_bottom = 1
    _fade_rect.offset_left = 0
    _fade_rect.offset_top = 0
    _fade_rect.offset_right = 0
    _fade_rect.offset_bottom = 0
    
    _fade_layer.add_child(_fade_rect)
    add_child(_fade_layer)

func goto_scene(scene_path: String, use_fade: bool = true, fade_seconds: float = DEFAULT_FADE_SECONDS) -> void:
    if _is_transitioning:
        return
    
    _is_transitioning = true
    _ensure_fade_overlay()
    
    if use_fade:
        await _fade_to(1.0, fade_seconds)
    
    var err = get_tree().change_scene_to_file(scene_path)
    if err != OK:
        push_warning("Globals.goto_scene failed: %s (%d)" % [scene_path, err])
    
    await get_tree().process_frame
    
    if use_fade:
        await _fade_to(0.0, fade_seconds)
    
    _is_transitioning = false

func _fade_to(target_alpha: float, fade_seconds: float) -> void:
    if not _fade_rect:
        return
    
    var from = _fade_rect.color
    var to = Color(from.r, from.g, from.b, target_alpha)
    
    var tween = create_tween()
    tween.set_pause_mode(Tween.TWEEN_PAUSE_PROCESS)
    tween.tween_property(_fade_rect, "color", to, fade_seconds)
    await tween.finished

func set_music(stream: AudioStream, autoplay: bool = true) -> void:
    music_player.stream = stream
    if autoplay and stream:
        music_player.play()

func stop_music() -> void:
    music_player.stop()
