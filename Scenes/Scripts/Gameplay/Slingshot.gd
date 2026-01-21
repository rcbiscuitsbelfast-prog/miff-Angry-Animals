extends Node2D
class_name Slingshot

signal projectile_launched(projectile: Projectile)

enum SlingshotType {
	CATAPULT,
	GIANT_HAND,
	TREBUCHET,
	SPRING
}

@export_group("Slingshot Configuration")
@export var input_area: Node # Was InputArea.cs
@export var trajectory_drawer: TrajectoryDrawer
@export var projectile_holder: Marker2D
@export var rest_position: Marker2D
@export var visual_mesh: Node2D

@export_group("Slingshot Type Settings")
@export var slingshot_type: SlingshotType = SlingshotType.CATAPULT

@export_group("Animation Settings")
@export var launch_animation_duration: float = 0.3
@export var squish_scale: float = 0.7
@export var stretch_scale: float = 1.3

var _settings: GameSettingsManager
const DEFAULT_IMPULSE_MULT: float = 20.0
const DEFAULT_IMPULSE_MAX: float = 1200.0
const DEFAULT_DRAG_LIM_MAX: Vector2 = Vector2(0, 60)
const DEFAULT_DRAG_LIM_MIN: Vector2 = Vector2(-60, 0)

enum State { IDLE, DRAGGING }

var _state: State = State.IDLE
var _current_projectile: Node # Will be Projectile
var _drag_start: Vector2 = Vector2.ZERO
var _dragged_vector: Vector2 = Vector2.ZERO
var _last_dragged_vector: Vector2 = Vector2.ZERO

func _ready() -> void:
	var player_profile = get_node_or_null("/root/PlayerProfile")
	if player_profile and player_profile.has_method("get_slingshot_type"):
		slingshot_type = player_profile.get_slingshot_type()
	
	_settings = get_node_or_null("/root/GameSettingsManager")
	_connect_signals()

func set_slingshot_type(p_type: SlingshotType) -> void:
	slingshot_type = p_type

func get_slingshot_type() -> SlingshotType:
	return slingshot_type

func _physics_process(_delta: float) -> void:
	if _state == State.DRAGGING:
		_update_dragging()

func _connect_signals() -> void:
	if input_area:
		if input_area.has_signal("drag_started"): input_area.drag_started.connect(_on_drag_started)
		if input_area.has_signal("drag_ended"): input_area.drag_ended.connect(_on_drag_ended)

func load_projectile(projectile: Node) -> void:
	_current_projectile = projectile
	if projectile_holder and _current_projectile:
		_current_projectile.global_position = projectile_holder.global_position

func _on_drag_started() -> void:
	if not _current_projectile:
		return
	
	_state = State.DRAGGING
	_drag_start = get_global_mouse_position()
	
	if trajectory_drawer:
		trajectory_drawer.show_trajectory(Vector2.ZERO, Vector2.ZERO)

func _on_drag_ended() -> void:
	if _state != State.DRAGGING or not _current_projectile:
		return
	
	_state = State.IDLE
	
	if trajectory_drawer:
		trajectory_drawer.hide_trajectory()
	
	_launch_projectile()

func _update_dragging() -> void:
	if not _current_projectile:
		return
	
	_update_dragged_vector()
	_play_stretch_sound()
	_constrain_drag_within_limits()
	
	var impulse = _calculate_impulse()
	
	if trajectory_drawer:
		trajectory_drawer.show_trajectory(_dragged_vector, impulse)

func _update_dragged_vector() -> void:
	_dragged_vector = get_global_mouse_position() - _drag_start

func _constrain_drag_within_limits() -> void:
	if not _current_projectile:
		return
	
	_last_dragged_vector = _dragged_vector
	
	var drag_max_val = _settings.slingshot_drag_max if _settings else 60.0
	var drag_min_val = _settings.slingshot_drag_min if _settings else 10.0 # Wait, C# code had some inconsistency here
	# C# Slingshot.cs: 
	# Vector2 dragMax = _settings?.SlingshotDragMax != null ? new Vector2(0, _settings.SlingshotDragMax) : DEFAULT_DRAG_LIM_MAX;
	# Vector2 dragMin = _settings?.SlingshotDragMin != null ? new Vector2(-_settings.SlingshotDragMin, 0) : DEFAULT_DRAG_LIM_MIN;
	
	var drag_max = Vector2(0, drag_max_val)
	var drag_min = Vector2(-drag_max_val, 0) # SlingshotDragMin in settings seems to be used differently in C#
	
	_dragged_vector = _dragged_vector.clamp(drag_min, drag_max)
	_current_projectile.global_position = _drag_start + _dragged_vector

func _calculate_impulse() -> Vector2:
	var impulse_multiplier = _settings.slingshot_impulse_multiplier if _settings else DEFAULT_IMPULSE_MULT
	var impulse_max = _settings.slingshot_impulse_max if _settings else DEFAULT_IMPULSE_MAX
	
	var impulse = _dragged_vector * -impulse_multiplier
	
	if impulse.length() > impulse_max:
		impulse = impulse.normalized() * impulse_max
	
	return impulse

func _play_stretch_sound() -> void:
	var diff = _dragged_vector - _last_dragged_vector
	if diff.length() > 0:
		var audio_manager = get_node_or_null("/root/AudioManager")
		if audio_manager:
			audio_manager.play_slingshot_sound()

func _launch_projectile() -> void:
	if not _current_projectile:
		return

	var impulse = _calculate_impulse()

	var audio_manager = get_node_or_null("/root/AudioManager")
	if audio_manager:
		audio_manager.play_slingshot_sound()
		if audio_manager.has_method("play_launch_vocal"):
			audio_manager.play_launch_vocal()

	_play_launch_animation()

	var game_feel_manager = get_node_or_null("/root/GameFeelManager")
	if game_feel_manager:
		if game_feel_manager.has_method("on_slingshot_launched"):
			game_feel_manager.on_slingshot_launched(slingshot_type, projectile_holder.global_position if projectile_holder else global_position)
		if game_feel_manager.has_method("on_projectile_launched"):
			game_feel_manager.on_projectile_launched()

	var speech_bubble_manager = get_node_or_null("/root/SpeechBubbleManager")
	if speech_bubble_manager and _current_projectile.has_method("is_face_projectile") and _current_projectile.is_face_projectile():
		speech_bubble_manager.set_current_projectile(_current_projectile)
		speech_bubble_manager.on_launch()

	if _current_projectile.has_method("launch"):
		_current_projectile.launch(impulse)

	projectile_launched.emit(_current_projectile)
	
	var signal_manager = get_node_or_null("/root/SignalManager")
	if signal_manager and signal_manager.has_method("emit_on_attempt_made"):
		signal_manager.emit_on_attempt_made()

	_current_projectile = null

func _play_launch_animation() -> void:
	if not visual_mesh:
		return

	var tween = create_tween()
	tween.set_parallel(true)

	match slingshot_type:
		SlingshotType.GIANT_HAND:
			tween.tween_property(visual_mesh, "scale", Vector2(squish_scale, squish_scale), launch_animation_duration * 0.3).set_trans(Tween.TRANS_ELASTIC)
			tween.tween_property(visual_mesh, "scale", Vector2.ONE, launch_animation_duration * 0.7).set_trans(Tween.TRANS_ELASTIC).set_delay(launch_animation_duration * 0.3)
		SlingshotType.TREBUCHET:
			tween.tween_property(visual_mesh, "rotation", deg_to_rad(-15.0), launch_animation_duration * 0.4).set_trans(Tween.TRANS_BACK)
			tween.tween_property(visual_mesh, "rotation", 0.0, launch_animation_duration * 0.6).set_trans(Tween.TRANS_BOUNCE).set_delay(launch_animation_duration * 0.4)
		SlingshotType.SPRING:
			tween.tween_property(visual_mesh, "scale", Vector2(stretch_scale, squish_scale), launch_animation_duration * 0.25).set_trans(Tween.TRANS_QUAD)
			tween.tween_property(visual_mesh, "scale", Vector2.ONE, launch_animation_duration * 0.75).set_trans(Tween.TRANS_BOUNCE).set_delay(launch_animation_duration * 0.25)
		_: # CATAPULT
			tween.tween_property(visual_mesh, "scale", Vector2(stretch_scale, squish_scale), launch_animation_duration * 0.2).set_trans(Tween.TRANS_QUAD)
			tween.tween_property(visual_mesh, "scale", Vector2.ONE, launch_animation_duration * 0.8).set_trans(Tween.TRANS_ELASTIC).set_delay(launch_animation_duration * 0.2)
