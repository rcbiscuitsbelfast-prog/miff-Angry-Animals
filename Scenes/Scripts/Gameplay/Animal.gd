extends RigidBody2D
class_name Animal

enum AnimalState { READY, DRAG, RELEASE }

const IMPULSE_MULT: float = 20.0
const IMPULSE_MAX: float = 1200.0
const DRAG_LIM_MAX: Vector2 = Vector2(0, 60)
const DRAG_LIM_MIN: Vector2 = Vector2(-60, 0)

@export var arrow: Sprite2D
@export var catapult_sound: AudioStreamPlayer2D
@export var stretch_sound: AudioStreamPlayer2D
@export var kick_wood_sound: AudioStreamPlayer2D
@export var on_screen_notifier: VisibleOnScreenNotifier2D

var _state: AnimalState = AnimalState.READY
var _initial_position: Vector2 = Vector2.ZERO
var _drag_start: Vector2 = Vector2.ZERO
var _dragged_vector: Vector2 = Vector2.ZERO
var _last_dragged_vector: Vector2 = Vector2.ZERO
var _arrow_x_scale: float = 0.0
var _last_collision_count: int = 0

func _ready() -> void:
	_initial_position = position
	if arrow:
		_arrow_x_scale = arrow.scale.x
		arrow.visible = false
	
	if on_screen_notifier:
		on_screen_notifier.screen_exited.connect(_on_off_screen)
	
	input_event.connect(_on_input_event)
	sleeping_state_changed.connect(_on_sleeping_state_changed)

func _physics_process(_delta: float) -> void:
	_update_state()

func _update_state() -> void:
	match _state:
		AnimalState.DRAG:
			_handle_dragging()
		AnimalState.RELEASE:
			_handle_flight()

func _change_state(new_state: AnimalState) -> void:
	_state = new_state
	match _state:
		AnimalState.DRAG:
			_start_dragging()
		AnimalState.RELEASE:
			_start_releasing()

func _start_dragging() -> void:
	_drag_start = get_global_mouse_position()
	if arrow:
		arrow.visible = true

func _start_releasing() -> void:
	freeze = false
	if arrow:
		arrow.visible = false
	if catapult_sound:
		catapult_sound.play()
	
	apply_central_impulse(_calculate_impulse())
	
	var signal_manager = get_node_or_null("/root/SignalManager")
	if signal_manager and signal_manager.has_method("emit_on_attempt_made"):
		signal_manager.emit_on_attempt_made()

func _handle_dragging() -> void:
	if _detect_release():
		return
	_update_dragged_vector()
	_play_stretch_sound()
	_constrain_drag_within_limits()
	_update_arrow_scale()

func _handle_flight() -> void:
	_play_kick_sound_on_collision()

func _update_dragged_vector() -> void:
	_dragged_vector = get_global_mouse_position() - _drag_start

func _constrain_drag_within_limits() -> void:
	_last_dragged_vector = _dragged_vector
	_dragged_vector = _dragged_vector.clamp(DRAG_LIM_MIN, DRAG_LIM_MAX)
	position = _initial_position + _dragged_vector

func _update_arrow_scale() -> void:
	if not arrow:
		return
	var impulse_length = _calculate_impulse().length()
	var scale_factor = impulse_length / IMPULSE_MAX
	arrow.scale = Vector2((_arrow_x_scale * scale_factor) + _arrow_x_scale, arrow.scale.y)
	arrow.rotation = (_drag_start - position).angle()

func _play_kick_sound_on_collision() -> void:
	if _last_collision_count == 0 and get_contact_count() > 0:
		if kick_wood_sound and not kick_wood_sound.playing:
			kick_wood_sound.play()
	_last_collision_count = get_contact_count()

func _play_stretch_sound() -> void:
	var diff = _dragged_vector - _last_dragged_vector
	if diff.length() > 0:
		if stretch_sound and not stretch_sound.playing:
			stretch_sound.play()

func _calculate_impulse() -> Vector2:
	return _dragged_vector * -IMPULSE_MULT

func _on_input_event(_viewport: Node, event: InputEvent, _shape_idx: int) -> void:
	if _state == AnimalState.READY and event.is_action_pressed("drag"):
		_change_state(AnimalState.DRAG)

func _detect_release() -> bool:
	if _state == AnimalState.DRAG and Input.is_action_just_released("drag"):
		_change_state(AnimalState.RELEASE)
		return true
	return false

func _on_sleeping_state_changed() -> void:
	if sleeping:
		for body in get_colliding_bodies():
			if body.has_method("die"):
				body.die()
		die.call_deferred()

func die() -> void:
	var signal_manager = get_node_or_null("/root/SignalManager")
	if signal_manager and signal_manager.has_method("emit_on_animal_died"):
		signal_manager.emit_on_animal_died()
	queue_free()

func _on_off_screen() -> void:
	die()
