extends RigidBody2D
class_name Projectile

signal almost_stopped()
signal explosion_occurred(epicenter: Vector2, force: float, radius: float)

@export var kick_wood_sound: AudioStreamPlayer2D
@export var on_screen_notifier: VisibleOnScreenNotifier2D

var _settings: GameSettingsManager
const DEFAULT_STOPPED_THRESHOLD: float = 0.1

var _has_been_launched: bool = false
var _almost_stopped_emitted: bool = false
var _last_collision_count: int = 0

var _pool: Node # Will be ObjectPool
var _is_pooled: bool = false

func _ready() -> void:
	_pool = get_node_or_null("/root/ObjectPool")
	_is_pooled = _pool != null
	
	_settings = get_node_or_null("/root/GameSettingsManager")
	_connect_signals()

func _physics_process(_delta: float) -> void:
	_check_if_almost_stopped()

func _connect_signals() -> void:
	if on_screen_notifier:
		on_screen_notifier.screen_exited.connect(_on_screen_exited)
	
	sleeping_state_changed.connect(_on_sleeping_state_changed)

func launch(impulse: Vector2) -> void:
	_has_been_launched = true
	freeze = false
	apply_central_impulse(impulse)

func _check_if_almost_stopped() -> void:
	if not _has_been_launched or _almost_stopped_emitted:
		return
	
	var stopped_threshold = _settings.projectile_stopped_threshold if _settings else DEFAULT_STOPPED_THRESHOLD
	if linear_velocity.length() < stopped_threshold:
		_almost_stopped_emitted = true
		almost_stopped.emit()

func _integrate_forces(state: PhysicsDirectBodyState2D) -> void:
	if _has_been_launched and kick_wood_sound:
		var contact_count = state.get_contact_count()
		if _last_collision_count == 0 and contact_count > 0 and not kick_wood_sound.playing:
			kick_wood_sound.play()
			
			var audio_manager = get_node_or_null("/root/AudioManager")
			if audio_manager and audio_manager.has_method("play_impact_vocal"):
				audio_manager.play_impact_vocal()
			
			var speech_bubble_manager = get_node_or_null("/root/SpeechBubbleManager")
			if speech_bubble_manager and speech_bubble_manager.has_method("on_impact"):
				speech_bubble_manager.on_impact(linear_velocity.length())
			
			var game_feel_manager = get_node_or_null("/root/GameFeelManager")
			if game_feel_manager and game_feel_manager.has_method("on_impact"):
				game_feel_manager.on_impact(self, linear_velocity.length())
				
		_last_collision_count = contact_count

func _on_sleeping_state_changed() -> void:
	if sleeping and _has_been_launched:
		for body in get_colliding_bodies():
			if body.has_method("die"):
				body.die()
		
		die.call_deferred()

func _on_screen_exited() -> void:
	if _has_been_launched:
		die()

func die() -> void:
	var force = linear_velocity.length()
	var explosion_position = global_position
	var radius = get_explosion_radius()
	
	explosion_occurred.emit(explosion_position, force, radius)
	
	var signal_manager = get_node_or_null("/root/SignalManager")
	if signal_manager and signal_manager.has_method("emit_on_animal_died"):
		signal_manager.emit_on_animal_died()
	
	if _is_pooled:
		mark_for_pooling()
	else:
		queue_free()

func get_explosion_radius() -> float:
	var base_radius = 80.0
	var velocity_scale = linear_velocity.length() * 0.1
	return clamp(base_radius + velocity_scale, 80.0, 200.0)

func reset_for_pool() -> void:
	_has_been_launched = false
	_almost_stopped_emitted = false
	_last_collision_count = 0
	linear_velocity = Vector2.ZERO
	angular_velocity = 0.0
	rotation = 0.0
	freeze = true

func mark_for_pooling() -> void:
	set_meta("can_be_pooled", true)

func is_face_projectile() -> bool:
	return false
