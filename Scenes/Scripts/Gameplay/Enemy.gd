extends RigidBody2D
class_name Enemy

signal destroyed(enemy: Node, collider: Node, impact_momentum: Vector2)

const DESTROY_THRESHOLD_BY_OBSTACLES: float = 400.0
const DESTROY_THRESHOLD: float = 1600.0

func _integrate_forces(state: PhysicsDirectBodyState2D) -> void:
	for i in range(state.get_contact_count()):
		var collider = state.get_contact_collider_object(i)
		if collider is RigidBody2D:
			var impact_momentum = collider.mass * collider.linear_velocity - mass * linear_velocity
			if impact_momentum.length() >= _get_destruction_threshold(collider):
				destroyed.emit(self, collider, impact_momentum)
				_on_destroyed()

func _get_destruction_threshold(collider: RigidBody2D) -> float:
	# Check for Obstacle type (need to define Obstacle class or check by method/meta)
	if collider.has_method("is_obstacle") or collider.has_meta("is_obstacle"):
		return DESTROY_THRESHOLD_BY_OBSTACLES
	return DESTROY_THRESHOLD

func _on_destroyed() -> void:
	queue_free()

func die() -> void:
	_on_destroyed()
