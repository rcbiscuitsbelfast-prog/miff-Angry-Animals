extends Node

## RagdollSpawner - Spawns ragdoll characters for testing and gameplay.
## Handles ragdoll instantiation, positioning, and lifecycle management.

signal ragdoll_spawned(ragdoll: Node)
signal ragdoll_removed(ragdoll: Node)

@export var ragdoll_scene_path: String = "res://Scenes/RagdollStickClone.tscn"
@export var max_ragdolls: int = 10
@export var spawn_interval: float = 5.0

var _ragdoll_scene: PackedScene
var _spawned_ragdolls: Array = []
var _spawn_timer: Timer

func _ready():
	initialize_spawner()

func initialize_spawner():
	# Load the ragdoll scene
	_ragdoll_scene = load(ragdoll_scene_path)
	
	# Set up spawn timer
	_spawn_timer = Timer.new()
	_spawn_timer.wait_time = spawn_interval
	_spawn_timer.timeout.connect(_spawn_ragdoll)
	add_child(_spawn_timer)
	
	print("RagdollSpawner initialized with scene: %s" % ragdoll_scene_path)

func spawn_ragdoll_at(position: Vector2, force: Vector2 = Vector2.ZERO) -> Node:
	# Spawn a ragdoll at the specified position
	if _spawned_ragdolls.size() >= max_ragdolls:
		print("Max ragdolls reached (%d), removing oldest" % max_ragdolls)
		remove_oldest_ragdoll()
	
	if _ragdoll_scene:
		var ragdoll = _ragdoll_scene.instantiate()
		ragdoll.global_position = position
		get_parent().add_child(ragdoll)
		
		# Activate the ragdoll with force if provided
		if force != Vector2.ZERO and ragdoll.has_method("activate_ragdoll"):
			ragdoll.activate_ragdoll(force)
		
		_spawned_ragdolls.append(ragdoll)
		ragdoll_spawned.emit(ragdoll)
		
		print("Ragdoll spawned at %s with force %s" % [str(position), str(force)])
		return ragdoll
	else:
		print("Error: Could not load ragdoll scene: %s" % ragdoll_scene_path)
		return null

func spawn_ragdoll():
	# Spawn a ragdoll at a random position
	var random_position = Vector2(
		randf_range(200, 1000),  # Random X
		randf_range(300, 500)     # Random Y
	)
	var random_force = Vector2(
		randf_range(-200, 200),
		randf_range(-100, -300)
	)
	
	spawn_ragdoll_at(random_position, random_force)

func remove_ragdoll(ragdoll: Node):
	# Remove a specific ragdoll
	if ragdoll in _spawned_ragdolls:
		_spawned_ragdolls.erase(ragdoll)
		ragdoll.queue_free()
		ragdoll_removed.emit(ragdoll)
		print("Ragdoll removed")

func remove_oldest_ragdoll():
	# Remove the oldest ragdoll
	if _spawned_ragdolls.size() > 0:
		var oldest_ragdoll = _spawned_ragdolls[0]
		remove_ragdoll(oldest_ragdoll)

func remove_all_ragdolls():
	# Remove all spawned ragdolls
	for ragdoll in _spawned_ragdolls:
		remove_ragdoll(ragdoll)
	
	_spawned_ragdolls.clear()

func start_auto_spawn():
	# Start automatic ragdoll spawning
	if _spawn_timer:
		_spawn_timer.start()
		print("Auto spawning started")

func stop_auto_spawn():
	# Stop automatic ragdoll spawning
	if _spawn_timer:
		_spawn_timer.stop()
		print("Auto spawning stopped")

func _spawn_ragdoll():
	# Timer callback for automatic spawning
	spawn_ragdoll()

func get_spawned_count() -> int:
	# Get number of currently spawned ragdolls
	return _spawned_ragdolls.size()

func get_spawned_ragdolls() -> Array:
	# Get list of all spawned ragdolls
	return _spawned_ragdolls.duplicate()

func set_max_ragdolls(count: int):
	# Set maximum number of ragdolls
	max_ragdolls = count
	
	# Remove excess ragdolls if needed
	while _spawned_ragdolls.size() > max_ragdolls:
		remove_oldest_ragdoll()

func set_spawn_interval(interval: float):
	# Set interval between automatic spawns
	spawn_interval = interval
	if _spawn_timer:
		_spawn_timer.wait_time = interval

func _exit_tree():
	# Clean up when the node is removed
	if _spawn_timer:
		_spawn_timer.queue_free()
	remove_all_ragdolls()