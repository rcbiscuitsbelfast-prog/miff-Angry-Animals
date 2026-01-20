extends Node2D
class_name TrajectoryDrawer

@export var trajectory_line: Line2D
@export var arrow: Sprite2D

var _arrow_x_scale: float = 0.0
const IMPULSE_MAX: float = 1200.0

func _ready() -> void:
	if arrow:
		_arrow_x_scale = arrow.scale.x
		arrow.visible = false
	
	if trajectory_line:
		trajectory_line.visible = false

func show_trajectory(drag_vector: Vector2, impulse: Vector2) -> void:
	if arrow:
		arrow.visible = true
		_update_arrow_scale(impulse, drag_vector)
	
	if trajectory_line:
		trajectory_line.visible = true
		# Note: Trajectory line drawing logic would go here if needed
		# The original C# didn't seem to update the line itself in ShowTrajectory

func hide_trajectory() -> void:
	if arrow:
		arrow.visible = false
	
	if trajectory_line:
		trajectory_line.visible = false

func _update_arrow_scale(impulse: Vector2, drag_vector: Vector2) -> void:
	if not arrow:
		return
	
	var impulse_length = impulse.length()
	var scale_factor = impulse_length / IMPULSE_MAX
	
	arrow.scale = Vector2((_arrow_x_scale * scale_factor) + _arrow_x_scale, arrow.scale.y)
	arrow.rotation = -drag_vector.angle()
