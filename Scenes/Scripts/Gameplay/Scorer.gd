extends Node
class_name Scorer

@export var target_score: int = 1000
@export var enemy_points: int = 100

var _total_cups: int = 0
var _cups_destroyed: int = 0
var _attempt: int = 0
var _current_destruction_score: int = 0

func _ready() -> void:
	var signal_manager = get_node_or_null("/root/SignalManager")
	if signal_manager:
		if signal_manager.has_signal("on_cup_destroyed"): signal_manager.on_cup_destroyed.connect(_on_cup_destroyed)
		if signal_manager.has_signal("on_attempt_made"): signal_manager.on_attempt_made.connect(_on_attempt_made)
		if signal_manager.has_signal("on_destruction_score_updated"): signal_manager.on_destruction_score_updated.connect(_on_destruction_score_updated)
	
	# Assuming Cup.GROUP_NAME is "cups"
	_total_cups = get_tree().get_nodes_in_group("cups").size()

func _on_cup_destroyed() -> void:
	_cups_destroyed += 1
	_check_level_completion()

func _on_destruction_score_updated(score: int) -> void:
	_current_destruction_score = score
	_check_level_completion()

func _check_level_completion() -> void:
	if _current_destruction_score >= target_score:
		var signal_manager = get_node_or_null("/root/SignalManager")
		if signal_manager and signal_manager.has_method("emit_on_level_completed"):
			signal_manager.emit_on_level_completed()
		
		var score_manager = get_node_or_null("/root/ScoreManager")
		if score_manager and score_manager.has_method("set_level_score"):
			score_manager.set_level_score(score_manager.get_level(), _attempt)

func _on_attempt_made() -> void:
	_attempt += 1
	var signal_manager = get_node_or_null("/root/SignalManager")
	if signal_manager and signal_manager.has_method("emit_on_score_updated"):
		signal_manager.emit_on_score_updated(_attempt)

func add_score(points: int, pos: Vector2) -> void:
	var score_manager = get_node_or_null("/root/ScoreManager")
	if score_manager and score_manager.has_method("add_score"):
		score_manager.add_score(points)
	
	_check_level_completion()
	
	var popup_scene = load("res://Scenes/ScorePopup.tscn")
	if popup_scene:
		var popup = popup_scene.instantiate()
		get_parent().add_child(popup)
		popup.global_position = pos
		if popup.has_method("show_score"):
			popup.show_score(points)
