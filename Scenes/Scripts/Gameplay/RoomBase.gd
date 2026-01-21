extends Node2D
class_name RoomBase

signal slingshot_phase_started()
signal traversal_phase_started()
signal room_target_reached()
signal exit_door_unlocked()

@export var slingshot_path: NodePath
@export var exit_door_path: NodePath
@export var projectiles_loader_path: NodePath
@export var target_score: int = 3
@export var is_bonus_room: bool = false
@export var next_room_path: NodePath
@export var enemy_spawner_path: NodePath

@export_group("Objectives")
@export var objectives: Array[LevelObjective] = []

var _slingshot: Slingshot
var _exit_door: Node2D
var _projectiles_loader: Node
var _next_room_marker: Node2D
var _enemy_spawner: Node

enum RoomPhase { SLINGSHOT, TRAVERSAL, COMPLETE }
var _current_phase: RoomPhase = RoomPhase.SLINGSHOT

var _destruction_score: int = 0
var _exit_unlocked: bool = false
var _handling_failure: bool = false

var _active_objectives: Array[LevelObjective] = []
var _objective_progress: Array[int] = []
var _objective_completed: Array[bool] = []
var _cups_destroyed: int = 0
var _total_cups: int = 0
var _npcs_destroyed: int = 0
var _exit_reached: bool = false

func _ready() -> void:
	_initialize_room()
	_initialize_objectives()
	_connect_signals()
	_emit_objectives_to_hud()
	_calculate_difficulty()

func _calculate_difficulty() -> void:
	var game_manager = get_node_or_null("/root/GameManager")
	var current_room_index = game_manager.current_room_index if game_manager else 0
	# DifficultyBalancer logic would go here
	pass

func _initialize_room() -> void:
	_slingshot = get_node_or_null(slingshot_path)
	_exit_door = get_node_or_null(exit_door_path)
	_projectiles_loader = get_node_or_null(projectiles_loader_path)
	_next_room_marker = get_node_or_null(next_room_path)
	_enemy_spawner = get_node_or_null(enemy_spawner_path)

	if _exit_door:
		_exit_door.set_process(false)

	var game_manager = get_node_or_null("/root/GameManager")
	var current_room_index = game_manager.current_room_index if game_manager else 0
	if game_manager and current_room_index >= 0 and current_room_index < game_manager.rooms.size():
		var optimal_score = game_manager.rooms[current_room_index].optimal_score
		target_score = int(optimal_score * 0.3)

	# Second chance reward logic
	var purchase_state_manager = get_node_or_null("/root/PurchaseStateManager")
	if purchase_state_manager and purchase_state_manager.has_method("has_pending_reward") and purchase_state_manager.has_pending_reward():
		if _projectiles_loader and _projectiles_loader.has_method("add_extra_projectiles"):
			_projectiles_loader.add_extra_projectiles(2)
		purchase_state_manager.clear_pending_reward()

func _initialize_objectives() -> void:
	_cups_destroyed = 0
	_npcs_destroyed = 0
	_exit_reached = false
	
	# Assuming Cup.GROUP_NAME is "cups"
	_total_cups = get_tree().get_nodes_in_group("cups").size()
	
	_active_objectives.clear()
	_objective_progress.clear()
	_objective_completed.clear()
	
	if objectives and objectives.size() > 0:
		for obj in objectives:
			if obj:
				_active_objectives.append(obj)
				_objective_progress.append(0)
				_objective_completed.append(false)
	else:
		var default_obj = LevelObjective.new()
		default_obj.type = LevelObjective.ObjectiveType.DESTROY_X_CUPS if _total_cups > 0 else LevelObjective.ObjectiveType.REACH_EXIT
		default_obj.count = _total_cups
		_active_objectives.append(default_obj)
		_objective_progress.append(0)
		_objective_completed.append(false)
	
	_update_objective_state()

func _emit_objectives_to_hud() -> void:
	var signal_manager = get_node_or_null("/root/SignalManager")
	if signal_manager and signal_manager.has_method("emit_on_objectives_updated"):
		signal_manager.emit_on_objectives_updated(_build_objectives_text())

func _build_objectives_text() -> String:
	if _active_objectives.is_empty():
		return ""
	
	var text = ""
	for i in range(_active_objectives.size()):
		var obj = _active_objectives[i]
		var done = _objective_completed[i]
		var progress = _objective_progress[i]
		
		var line = obj.get_display_text(progress)
		if done:
			line = "✓ " + line
		
		text += ("" if i == 0 else "\n") + line
	return text

func _update_objective_state() -> void:
	for i in range(_active_objectives.size()):
		var obj = _active_objectives[i]
		var complete = false
		var progress = 0
		
		match obj.type:
			LevelObjective.ObjectiveType.DESTROY_X_CUPS:
				var required = obj.count if obj.count > 0 else _total_cups
				progress = _cups_destroyed
				complete = required > 0 and _cups_destroyed >= required
			LevelObjective.ObjectiveType.REACH_EXIT:
				progress = 1 if _exit_reached else 0
				complete = _exit_reached
			LevelObjective.ObjectiveType.DESTROY_SPECIFIC_NPCS:
				progress = _npcs_destroyed
				complete = _npcs_destroyed >= obj.count if obj.count > 0 else _npcs_destroyed > 0
		
		_objective_progress[i] = progress
		_objective_completed[i] = complete
	
	_emit_objectives_to_hud()

func _are_all_objectives_complete() -> bool:
	for completed in _objective_completed:
		if not completed:
			return false
	return not _objective_completed.is_empty()

func _connect_signals() -> void:
	var signal_manager = get_node_or_null("/root/SignalManager")
	if signal_manager:
		if signal_manager.has_signal("on_destruction_score_updated"): signal_manager.on_destruction_score_updated.connect(_on_destruction_score_updated)
		if signal_manager.has_signal("on_cup_destroyed"): signal_manager.on_cup_destroyed.connect(_on_cup_destroyed)
		if signal_manager.has_signal("on_prop_destroyed"): signal_manager.on_prop_destroyed.connect(_on_prop_destroyed)
		if signal_manager.has_signal("on_animal_died"): signal_manager.on_animal_died.connect(_on_animal_died)
		if signal_manager.has_signal("on_npc_destroyed"): signal_manager.on_npc_destroyed.connect(_on_npc_destroyed)

	if _projectiles_loader:
		if _projectiles_loader.has_signal("projectile_launched"): _projectiles_loader.projectile_launched.connect(_on_projectile_launched)
		if _projectiles_loader.has_signal("all_projectiles_used"): _projectiles_loader.all_projectiles_used.connect(_on_all_projectiles_used)

	if _slingshot:
		if _slingshot.has_signal("projectile_launched"): _slingshot.projectile_launched.connect(_on_slingshot_projectile_launched)

func _on_destruction_score_updated(score: int) -> void:
	_destruction_score = score
	if _destruction_score >= target_score and not _exit_unlocked:
		_unlock_exit_door()

func _on_cup_destroyed() -> void:
	_cups_destroyed += 1
	_update_objective_state()

func _on_prop_destroyed(_prop: Node, _score_value: int) -> void:
	pass

func _on_npc_destroyed(_npc: Node) -> void:
	_npcs_destroyed += 1
	_update_objective_state()

func _on_animal_died() -> void:
	if _current_phase == RoomPhase.SLINGSHOT and _projectiles_loader:
		if _projectiles_loader.has_more_projectiles:
			_start_traversal_phase()
		else:
			_handle_attempts_failed()

func _on_projectile_launched(_projectile: Node) -> void:
	pass

func _on_slingshot_projectile_launched(projectile: Node) -> void:
	if projectile.has_signal("almost_stopped"):
		projectile.almost_stopped.connect(_on_projectile_almost_stopped.bind(projectile))

func _on_projectile_almost_stopped(_projectile: Node) -> void:
	if _current_phase == RoomPhase.SLINGSHOT:
		_start_traversal_phase()

func _on_all_projectiles_used() -> void:
	if _exit_unlocked:
		_complete_room()
	else:
		_handle_attempts_failed()

func _start_traversal_phase() -> void:
	if _current_phase != RoomPhase.SLINGSHOT:
		return
	
	_current_phase = RoomPhase.TRAVERSAL
	traversal_phase_started.emit()
	_spawn_stick_clone()

func _spawn_stick_clone() -> void:
	# Implementation of spawn_stick_clone
	pass

func _unlock_exit_door() -> void:
	_exit_unlocked = true
	if _exit_door:
		_exit_door.set_process(true)
	exit_door_unlocked.emit()
	
	var game_feel_manager = get_node_or_null("/root/GameFeelManager")
	if game_feel_manager and _exit_door and game_feel_manager.has_method("on_door_unlocked"):
		game_feel_manager.on_door_unlocked(_exit_door.global_position)

func _complete_room() -> void:
	if _current_phase == RoomPhase.COMPLETE:
		return
	
	_current_phase = RoomPhase.COMPLETE
	room_target_reached.emit()
	_on_level_completed()

func _on_level_completed() -> void:
	if is_bonus_room and _next_room_marker:
		_handle_bonus_room_transition()
	else:
		var signal_manager = get_node_or_null("/root/SignalManager")
		if signal_manager and signal_manager.has_method("emit_on_level_completed"):
			signal_manager.emit_on_level_completed()

func _handle_bonus_room_transition() -> void:
	var signal_manager = get_node_or_null("/root/SignalManager")
	if signal_manager and signal_manager.has_method("emit_on_level_completed"):
		signal_manager.emit_on_level_completed()

func _handle_attempts_failed() -> void:
	if _handling_failure or _current_phase == RoomPhase.COMPLETE:
		return
	_handling_failure = true
	_show_game_over_screen()

func _show_game_over_screen() -> void:
	var game_over_scene = load("res://Scenes/UI/GameOverScreen.tscn")
	if not game_over_scene:
		var game_manager = get_node_or_null("/root/GameManager")
		if game_manager: game_manager.restart_room()
		return
	
	var game_over_screen = game_over_scene.instantiate()
	add_child(game_over_screen)
	if game_over_screen.has_method("set_status"):
		game_over_screen.set_status("You destroyed %d points of obstacles.\nTarget: %d" % [_destruction_score, target_score])

func on_exit_reached() -> void:
	_exit_reached = true
	_update_objective_state()
	if _exit_unlocked:
		_complete_room()

func get_destruction_score() -> int:
	return _destruction_score

func get_target_score() -> int:
	return target_score

func is_exit_unlocked() -> bool:
	return _exit_unlocked
