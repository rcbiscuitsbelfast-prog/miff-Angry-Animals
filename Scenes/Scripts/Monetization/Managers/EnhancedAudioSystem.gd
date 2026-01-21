extends Node
class_name EnhancedAudioSystem

## Enhanced audio system with ad-aware muting capabilities.
## Automatically mutes audio during ads and restores on ad completion.

signal audio_muted(is_muted: bool)

var _ad_mute_volume: float = 0.0
var _current_volume: float = 1.0
var _was_muted_by_ad: bool = false
var _audio_manager: Node = null

static var instance: EnhancedAudioSystem = null

func _ready() -> void:
	if instance != null:
		queue_free()
		return
	
	instance = self
	_audio_manager = get_node_or_null("/root/AudioManager")
	
	# Connect to ad events
	var ads_manager = get_node_or_null("/root/AdsManager")
	if ads_manager:
		_connect_ad_signals(ads_manager)
	
	_current_volume = 1.0
	print("Enhanced Audio System initialized")

## Connect to ad manager signals for automatic muting
func _connect_ad_signals(ads_manager: Node) -> void:
	if ads_manager.has_signal("ad_closed"):
		ads_manager.ad_closed.connect(_on_ad_closed)
	if ads_manager.has_signal("ad_clicked"):
		ads_manager.ad_clicked.connect(_on_ad_clicked)

## Mute audio (for ads, pause menu, etc.)
func mute_audio() -> void:
	if not _audio_manager:
		return
	
	# Store current volume before muting
	if not _was_muted_by_ad:
		_current_volume = _get_current_volume()
	
	# Apply mute
	_set_volume(0.0)
	_was_muted_by_ad = true
	
	audio_muted.emit(true)
	print("Audio muted for ad")

## Unmute audio and restore previous volume
func unmute_audio() -> void:
	if not _audio_manager:
		return
	
	if _was_muted_by_ad:
		_set_volume(_current_volume)
		_was_muted_by_ad = false
		audio_muted.emit(false)
		print("Audio unmuted after ad")

## Get current volume from audio manager
func _get_current_volume() -> float:
	if not _audio_manager:
		return 1.0
	
	# Try to get volume via common property names/methods
	if _audio_manager.has_property("master_volume"):
		return _audio_manager.master_volume
	elif _audio_manager.has_property("volume"):
		return _audio_manager.volume
	elif _audio_manager.has_method("get_master_volume"):
		return _audio_manager.get_master_volume()
	elif _audio_manager.has_method("get_volume"):
		return _audio_manager.get_volume()
	
	return 1.0

## Set volume on audio manager
func _set_volume(volume: float) -> void:
	if not _audio_manager:
		return
	
	# Try to set volume via common property names/methods
	if _audio_manager.has_property("master_volume"):
		_audio_manager.master_volume = volume
	elif _audio_manager.has_property("volume"):
		_audio_manager.volume = volume
	elif _audio_manager.has_method("set_master_volume"):
		_audio_manager.set_master_volume(volume)
	elif _audio_manager.has_method("set_volume"):
		_audio_manager.set_volume(volume)

## Handle ad clicked - mute audio
func _on_ad_clicked() -> void:
	mute_audio()

## Handle ad closed - unmute audio
func _on_ad_closed() -> void:
	unmute_audio()

## Fade audio to volume
func fade_to_volume(target_volume: float, duration: float) -> void:
	if not _audio_manager:
		return
	
	var current = _get_current_volume()
	var start_time = Time.get_ticks_msec()
	var end_time = start_time + duration * 1000
	
	# This would normally be done with a Tween node
	# For simplicity, just set the volume directly
	_set_volume(target_volume)

## Play a sound effect
func play_sfx(sound_name: String) -> void:
	if not _audio_manager:
		return
	
	if _audio_manager.has_method("play_sfx"):
		_audio_manager.play_sfx(sound_name)
	elif _audio_manager.has_method("play_sound"):
		_audio_manager.play_sound(sound_name)

## Play a music track
func play_music(music_name: String) -> void:
	if not _audio_manager:
		return
	
	if _audio_manager.has_method("play_music"):
		_audio_manager.play_music(music_name)
	elif _audio_manager.has_method("play_bgm"):
		_audio_manager.play_bgm(music_name)

## Stop all audio
func stop_all() -> void:
	if not _audio_manager:
		return
	
	if _audio_manager.has_method("stop_all"):
		_audio_manager.stop_all()

## Check if audio is currently muted
func is_muted() -> bool:
	return _get_current_volume() == 0.0
