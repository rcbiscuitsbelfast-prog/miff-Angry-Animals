extends Area2D
class_name Water

@export var splash_sound: AudioStreamPlayer2D

func _ready() -> void:
	body_entered.connect(_on_body_entered)

func _on_body_entered(_body: Node2D) -> void:
	if splash_sound:
		splash_sound.play()
