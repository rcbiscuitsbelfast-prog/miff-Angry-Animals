extends Node2D

## Custom play room that handles custom level gameplay.
## Manages slingshot, projectiles, obstacles, and completion logic.

signal room_completed(score: int)
signal room_failed()
signal level_loaded()

@export var slingshot_path: NodePath
@export var exit_door_path: NodePath
@export var projectiles_loader_path: NodePath
@export var target_score: int = 3
@export var _obstacle_scene: PackedScene
@export var _obstacles_parent_path: NodePath

var _slingshot: Node2D
var _exit_door: Area2D
var _projectiles_loader: Node2D
var _obstacles_parent: Node2D

var _current_score: int = 0
var _total_attempts: int = 0
var _level_completed: bool = false

func _ready():
	initialize_room()
	load_obstacles()
	connect_signals()

func initialize_room():
	_slingshot = get_node_or_null(slingshot_path) as Node2D
	_exit_door = get_node_or_null(exit_door_path) as Area2D
	_projectiles_loader = get_node_or_null(projectiles_loader_path) as Node2D
	_obstacles_parent = get_node_or_null(_obstacles_parent_path) as Node2D
	
	# Connect exit door collision
	if _exit_door:
		_exit_door.body_entered.connect(_on_exit_door_entered)

func load_obstacles():
	# Load initial obstacles for the room
	if _obstacle_scene and _obstacles_parent:
		# Load some basic obstacles
		var obstacle_positions = [
			Vector2(500, 550),
			Vector2(600, 550),
			Vector2(700, 550),
			Vector2(550, 500),
			Vector2(650, 500)
		]
		
		for i in range(min(obstacle_positions.size(), 3)):
			var obstacle = _obstacle_scene.instantiate()
			obstacle.position = obstacle_positions[i]
			_obstacles_parent.add_child(obstacle)

func connect_signals():
	# Connect to GameManager if available
	if GameManager.instance:
		GameManager.instance.level_started.connect(_on_level_started)
		GameManager.instance.level_restarted.connect(_on_level_restarted)

func _on_exit_door_entered(body):
	# Player reached the exit door
	if body.name.begins_with("StickClone"):
		complete_level()

func complete_level():
	# Level completed successfully
	_level_completed = true
	room_completed.emit(_current_score)
	
	# Show completion UI
	show_level_completion()

func show_level_completion():
	# Show level completion interface
	print("Level completed with score: %d" % _current_score)
	
	# Could show a completion panel or transition to next level
	pass

func fail_level():
	# Level failed (out of projectiles, etc.)
	room_failed.emit()
	show_level_failure()

func show_level_failure():
	# Show level failure interface
	print("Level failed")
	
	# Could show a retry option
	pass

func reset_level():
	# Reset the level to initial state
	_current_score = 0
	_level_completed = false
	_total_attempts = 0
	
	# Clear and reload obstacles
	if _obstacles_parent:
		_obstacles_parent.queue_free()
		await get_tree().process_frame
		_obstacles_parent = Node2D.new()
		_obstacles_parent.name = "Obstacles"
		add_child(_obstacles_parent)
	
	load_obstacles()

func add_score(points: int):
	# Add points to current score
	_current_score += points
	check_level_completion()

func check_level_completion():
	# Check if target score is reached
	if _current_score >= target_score and not _level_completed:
		complete_level()

func increment_attempts():
	# Track attempt count
	_total_attempts += 1

func get_current_score() -> int:
	return _current_score

func get_target_score() -> int:
	return target_score

func get_attempts() -> int:
	return _total_attempts

func is_level_completed() -> bool:
	return _level_completed

func _on_level_started(level_id: int):
	# Level started event
	level_loaded.emit()
	print("Custom room level %d started" % level_id)

func _on_level_restarted():
	# Level restarted event
	reset_level()
	print("Custom room level restarted")

func _exit_tree():
	# Clean up connections
	if GameManager.instance:
		GameManager.instance.level_started.disconnect(_on_level_started)
		GameManager.instance.level_restarted.disconnect(_on_level_restarted)