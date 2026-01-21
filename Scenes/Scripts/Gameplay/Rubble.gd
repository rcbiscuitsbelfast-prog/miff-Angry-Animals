extends RigidBody2D
class_name Rubble

@export var fade_delay: float = 2.0
@export var fade_duration: float = 1.0

var _pool: Node
var _is_pooled: bool = false
var _fade_tween: Tween

func _ready() -> void:
	_pool = get_node_or_null("/root/ObjectPool")
	_is_pooled = _pool != null
	
	add_to_group("walkable_rubble")
	
	rotation = randf() * TAU
	apply_torque_impulse(randf_range(-10.0, 10.0))
	
	get_tree().create_timer(fade_delay).timeout.connect(_start_fade)

func _start_fade() -> void:
	if _is_pooled:
		mark_for_pooling()
	else:
		_fade_tween = create_tween()
		_fade_tween.tween_property(self, "modulate:a", 0.0, fade_duration)
		_fade_tween.tween_callback(queue_free)

func reset_for_pool() -> void:
	modulate = Color.WHITE
	linear_velocity = Vector2.ZERO
	angular_velocity = 0.0
	rotation = randf() * TAU
	freeze = true

func mark_for_pooling() -> void:
	set_meta("can_be_pooled", true)
