extends Node
class_name ObjectPool

@export var object_scene: PackedScene
@export var pool_size: int = 5
@export var refresh_timer: float = 1.0

var _inactive_container: Node
var _active_objects: Array[Node] = []

func _ready() -> void:
	_inactive_container = Node.new()
	_inactive_container.name = "InactiveNodes"
	add_child(_inactive_container)

	# Populate pool
	for i in range(pool_size):
		var obj = _create_object()
		if obj:
			_inactive_container.add_child(obj)

	# Schedule pool refresh
	var timer = Timer.new()
	timer.wait_time = refresh_timer
	timer.one_shot = false
	add_child(timer)
	timer.timeout.connect(check_unused_objects)
	timer.start()

func pool(obj: Node) -> void:
	if not obj:
		return

	var parent = obj.get_parent()
	if parent:
		parent.remove_child(obj)
	
	_active_objects.erase(obj)

	# Reset object state
	obj.modulate = Color.WHITE

	if obj is RigidBody2D:
		obj.linear_velocity = Vector2.ZERO
		obj.angular_velocity = 0.0
		obj.rotation = 0.0
		obj.freeze = true

	_inactive_container.add_child(obj)

func get_instance() -> Node:
	var obj: Node

	if _inactive_container.get_child_count() > 0:
		obj = _inactive_container.get_child(0)
		_inactive_container.remove_child(obj)
	else:
		# print("ObjectPool: Pool empty. Creating new object.")
		obj = _create_object()

	if obj:
		obj.modulate = Color.WHITE
		_active_objects.append(obj)

		# Wake up rigid bodies
		if obj is RigidBody2D:
			obj.freeze = false

	return obj

func check_unused_objects() -> void:
	for i in range(_active_objects.size() - 1, -1, -1):
		var obj = _active_objects[i]
		if obj.has_meta("can_be_pooled") and obj.get_meta("can_be_pooled") == true:
			pool(obj)

func _create_object() -> Node:
	if not object_scene:
		push_error("ObjectPool: No object_scene set!")
		return null

	var obj = object_scene.instantiate()
	obj.set_meta("can_be_pooled", false)
	return obj

func _exit_tree() -> void:
	if is_in_group("pools"):
		for obj in _inactive_container.get_children():
			obj.queue_free()
		for obj in _active_objects:
			obj.queue_free()
