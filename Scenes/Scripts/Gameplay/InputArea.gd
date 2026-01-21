extends Area2D

## Simple input area for handling user input.

signal input_area_pressed(position: Vector2)

func _ready():
	# Initialize input area
	connect("input_event", _on_input_event)

func _on_input_event(viewport, event, shape_idx):
	if event is InputEventMouseButton and event.pressed:
		input_area_pressed.emit(event.position)

func _draw():
	# Draw a simple circle to indicate the input area
	draw_circle(Vector2.ZERO, 60, Color(0, 1, 0, 0.3))