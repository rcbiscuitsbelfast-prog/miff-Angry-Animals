extends Node
class_name AudioManager

signal music_volume_changed(volume: float)
signal sfx_volume_changed(volume: float)

# Audio buses
const MUSIC_BUS = "Music"
const SFX_BUS = "SFX"
const UI_BUS = "SFX" # UI sounds use SFX bus

# Audio streams
var _background_music_player: AudioStreamPlayer
var _slingshot_sfx_player: AudioStreamPlayer
var _destruction_sfx_player: AudioStreamPlayer
var _ui_click_player: AudioStreamPlayer
var _combo_player: AudioStreamPlayer
var _rage_player: AudioStreamPlayer

# Vocal audio streams (with pitch/volume randomization support)
var _vocal_launch_player: AudioStreamPlayer
var _vocal_impact_player: AudioStreamPlayer
var _vocal_expression_player: AudioStreamPlayer

# Audio resources (to be loaded from res://Assets/Audio/)
var _background_music: AudioStream
var _slingshot_sound: AudioStream
var _destruction_sound: AudioStream
var _ui_click_sound: AudioStream
var _combo_sound: AudioStream
var _rage_sound: AudioStream

# Launch vocal resources
var _launch_grunt1: AudioStream
var _launch_grunt2: AudioStream
var _launch_whoosh1: AudioStream
var _launch_whoosh2: AudioStream

# Impact vocal resources
var _impact_oof1: AudioStream
var _impact_oof2: AudioStream
var _impact_thud1: AudioStream
var _impact_crash1: AudioStream

# Expression vocal resources
var _vocal_laugh: AudioStream
var _vocal_scream: AudioStream
var _vocal_angry_roar: AudioStream
var _vocal_dizzy_groan: AudioStream

# Volume settings
@export var music_volume: float = 0.7:
	set(value):
		music_volume = clamp(value, 0.0, 1.0)
		if _background_music_player:
			_background_music_player.volume_db = linear_to_db(music_volume)
		music_volume_changed.emit(music_volume)

@export var sfx_volume: float = 0.8:
	set(value):
		sfx_volume = clamp(value, 0.0, 1.0)
		var db = linear_to_db(sfx_volume)
		if _slingshot_sfx_player: _slingshot_sfx_player.volume_db = db
		if _destruction_sfx_player: _destruction_sfx_player.volume_db = db
		if _ui_click_player: _ui_click_player.volume_db = db
		if _combo_player: _combo_player.volume_db = db
		if _rage_player: _rage_player.volume_db = db
		if _vocal_launch_player: _vocal_launch_player.volume_db = db
		if _vocal_impact_player: _vocal_impact_player.volume_db = db
		if _vocal_expression_player: _vocal_expression_player.volume_db = db
		sfx_volume_changed.emit(sfx_volume)

@export var mute_music: bool = false:
	set(value):
		mute_music = value
		if _background_music_player:
			_background_music_player.stream_paused = mute_music

@export var mute_sfx: bool = false:
	set(value):
		mute_sfx = value
		# In Godot 4, there isn't a direct 'muted' property on AudioStreamPlayer, 
		# but we can use bus mute or just not play sounds.
		# The original C# code used .Muted which might be a custom property or something else.
		# Actually, AudioStreamPlayer doesn't have .Muted. It has .volume_db.
		# Let's check AudioServer for bus muting.

func _ready() -> void:
	process_mode = Node.PROCESS_MODE_ALWAYS
	_initialize_audio_players()
	_load_audio_resources()
	_connect_signals()
	_start_background_music()

func _initialize_audio_players() -> void:
	_background_music_player = _create_player("BackgroundMusicPlayer", MUSIC_BUS, music_volume)
	_slingshot_sfx_player = _create_player("SlingshotSfxPlayer", SFX_BUS, sfx_volume)
	_destruction_sfx_player = _create_player("DestructionSfxPlayer", SFX_BUS, sfx_volume)
	_ui_click_player = _create_player("UiClickPlayer", UI_BUS, sfx_volume)
	_combo_player = _create_player("ComboPlayer", SFX_BUS, sfx_volume)
	_rage_player = _create_player("RagePlayer", SFX_BUS, sfx_volume)
	
	_vocal_launch_player = _create_player("VocalLaunchPlayer", SFX_BUS, sfx_volume)
	_vocal_impact_player = _create_player("VocalImpactPlayer", SFX_BUS, sfx_volume)
	_vocal_expression_player = _create_player("VocalExpressionPlayer", SFX_BUS, sfx_volume)

func _create_player(p_name: String, p_bus: String, p_volume: float) -> AudioStreamPlayer:
	var player = AudioStreamPlayer.new()
	player.name = p_name
	player.bus = p_bus
	player.volume_db = linear_to_db(p_volume)
	add_child(player)
	return player

func _load_audio_resources() -> void:
	_background_music = _load_audio_resource("res://Assets/Audio/Music/BackgroundMusic.ogg")
	_slingshot_sound = _load_audio_resource("res://Assets/Audio/SFX/SlingshotSound.ogg")
	_destruction_sound = _load_audio_resource("res://Assets/Audio/SFX/DestructionSound.ogg")
	_ui_click_sound = _load_audio_resource("res://Assets/Audio/SFX/UiClickSound.ogg")
	_combo_sound = _load_audio_resource("res://Assets/Audio/SFX/ComboSound.ogg")
	_rage_sound = _load_audio_resource("res://Assets/Audio/SFX/RageSound.ogg")

	_launch_grunt1 = _load_audio_resource("res://Assets/Audio/SFX/Vocals/LaunchGrunt1.wav")
	_launch_grunt2 = _load_audio_resource("res://Assets/Audio/SFX/Vocals/LaunchGrunt2.wav")
	_launch_whoosh1 = _load_audio_resource("res://Assets/Audio/SFX/Vocals/LaunchWhoosh1.wav")
	_launch_whoosh2 = _load_audio_resource("res://Assets/Audio/SFX/Vocals/LaunchWhoosh2.wav")

	_impact_oof1 = _load_audio_resource("res://Assets/Audio/SFX/Vocals/ImpactOof1.wav")
	_impact_oof2 = _load_audio_resource("res://Assets/Audio/SFX/Vocals/ImpactOof2.wav")
	_impact_thud1 = _load_audio_resource("res://Assets/Audio/SFX/Vocals/ImpactThud1.wav")
	_impact_crash1 = _load_audio_resource("res://Assets/Audio/SFX/Vocals/ImpactCrash1.wav")

	_vocal_laugh = _load_audio_resource("res://Assets/Audio/SFX/Vocals/VocalLaugh.wav")
	_vocal_scream = _load_audio_resource("res://Assets/Audio/SFX/Vocals/VocalScream.wav")
	_vocal_angry_roar = _load_audio_resource("res://Assets/Audio/SFX/Vocals/VocalAngryRoar.wav")
	_vocal_dizzy_groan = _load_audio_resource("res://Assets/Audio/SFX/Vocals/VocalDizzyGroan.wav")

	if _background_music_player: _background_music_player.stream = _background_music
	if _slingshot_sfx_player: _slingshot_sfx_player.stream = _slingshot_sound
	if _destruction_sfx_player: _destruction_sfx_player.stream = _destruction_sound
	if _ui_click_player: _ui_click_player.stream = _ui_click_sound
	if _combo_player: _combo_player.stream = _combo_sound
	if _rage_player: _rage_player.stream = _rage_sound

func _load_audio_resource(path: String) -> AudioStream:
	if not FileAccess.file_exists(path) and not ResourceLoader.exists(path):
		# push_warning("Audio resource not found: %s" % path)
		return null
	return load(path)

func _connect_signals() -> void:
	var signal_manager = get_node_or_null("/root/SignalManager")
	if signal_manager:
		if signal_manager.has_signal("on_attempt_made"): signal_manager.on_attempt_made.connect(_on_attempt_made)
		if signal_manager.has_signal("on_cup_destroyed"): signal_manager.on_cup_destroyed.connect(_on_cup_destroyed)
		if signal_manager.has_signal("on_prop_destroyed"): signal_manager.on_prop_destroyed.connect(_on_prop_destroyed)
		if signal_manager.has_signal("on_animal_died"): signal_manager.on_animal_died.connect(_on_animal_died)

	var rage_system = get_node_or_null("/root/RageSystem")
	if rage_system:
		if rage_system.has_signal("rage_threshold_reached"): rage_system.rage_threshold_reached.connect(_on_rage_threshold_reached)
		if rage_system.has_signal("combo_changed"): rage_system.combo_changed.connect(_on_combo_changed)

	var game_manager = get_node_or_null("/root/GameManager")
	if game_manager:
		if game_manager.has_signal("game_state_changed"): game_manager.game_state_changed.connect(_on_game_state_changed)

func _start_background_music() -> void:
	if _background_music_player and _background_music and not mute_music:
		_background_music_player.play()

func _stop_background_music() -> void:
	if _background_music_player:
		_background_music_player.stop()

func _on_attempt_made() -> void:
	play_slingshot_sound()

func _on_cup_destroyed() -> void:
	play_destruction_sound()

func _on_prop_destroyed(_prop: Node, _score_value: int) -> void:
	play_destruction_sound()

func _on_animal_died() -> void:
	pass

func _on_rage_threshold_reached(_threshold_index: int) -> void:
	play_rage_sound()

func _on_combo_changed(combo: int) -> void:
	if combo > 1:
		play_combo_sound()

func _on_game_state_changed(state: int) -> void:
	# Assuming GameManager.GameState enum matches
	match state:
		1: # MainMenu
			_stop_background_music()
		3: # InRoom
			_start_background_music()
		5: # Paused
			pass

func play_slingshot_sound() -> void:
	if _slingshot_sfx_player and not mute_sfx:
		_slingshot_sfx_player.play()

func play_destruction_sound() -> void:
	if _destruction_sfx_player and not mute_sfx:
		_destruction_sfx_player.play()

func play_ui_click_sound() -> void:
	if _ui_click_player and not mute_sfx:
		_ui_click_player.play()

func play_combo_sound() -> void:
	if _combo_player and not mute_sfx:
		_combo_player.play()

func play_rage_sound() -> void:
	if _rage_player and not mute_sfx:
		_rage_player.play()

func play_launch_vocal() -> void:
	if not _vocal_launch_player or mute_sfx: return
	var vocals = [_launch_grunt1, _launch_grunt2, _launch_whoosh1, _launch_whoosh2].filter(func(v): return v != null)
	if vocals.is_empty(): return
	var selected = vocals[randi() % vocals.size()]
	_vocal_launch_player.stream = selected
	_vocal_launch_player.pitch_scale = 1.0 + randf_range(-0.2, 0.2)
	_vocal_launch_player.volume_db = linear_to_db(sfx_volume) + randf_range(-2.0, 2.0)
	_vocal_launch_player.play()

func play_impact_vocal() -> void:
	if not _vocal_impact_player or mute_sfx: return
	var vocals = [_impact_oof1, _impact_oof2, _impact_thud1, _impact_crash1].filter(func(v): return v != null)
	if vocals.is_empty(): return
	var selected = vocals[randi() % vocals.size()]
	_vocal_impact_player.stream = selected
	_vocal_impact_player.pitch_scale = 1.0 + randf_range(-0.2, 0.2)
	_vocal_impact_player.volume_db = linear_to_db(sfx_volume) + randf_range(-2.0, 2.0)
	_vocal_impact_player.play()

func play_expression_vocal(type: String) -> void:
	if not _vocal_expression_player or mute_sfx: return
	var stream: AudioStream = null
	match type:
		"laugh": stream = _vocal_laugh
		"scream": stream = _vocal_scream
		"roar": stream = _vocal_angry_roar
		"groan": stream = _vocal_dizzy_groan
	
	if stream:
		_vocal_expression_player.stream = stream
		_vocal_expression_player.pitch_scale = 1.0 + randf_range(-0.1, 0.1)
		_vocal_expression_player.play()

func set_music_volume(p_volume: float) -> void:
	music_volume = p_volume

func set_sfx_volume(p_volume: float) -> void:
	sfx_volume = p_volume

func set_music_mute(p_muted: bool) -> void:
	mute_music = p_muted

func set_sfx_mute(p_muted: bool) -> void:
	mute_sfx = p_muted
