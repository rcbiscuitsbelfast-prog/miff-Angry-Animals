extends Area2D
class_name ItemPickup

@export var score_value: int = 50
@export var pickup_sound: AudioStreamPlayer2D

func _ready() -> void:
	body_entered.connect(_on_body_entered)

func _on_body_entered(body: Node2D) -> void:
	if body is Projectile or body is Animal:
		_on_picked_up()

func _on_picked_up() -> void:
	if pickup_sound:
		pickup_sound.play()
	
	var score_manager = get_node_or_null("/root/ScoreManager")
	if score_manager and score_manager.has_method("add_score"):
		score_manager.add_score(score_value)
	
	queue_free()
