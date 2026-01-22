extends Node2D

## RagdollStickClone - Advanced ragdoll character system.
## Handles limb physics, destruction, and connector management.

signal ragdoll_activated()
signal ragdoll_deactivated()
signal ragdoll_part_removed(limb_name: String)

@export var _limb_connector_path: NodePath
@export var _lifetime_timer_path: NodePath

var _limb_connector: Node
var _lifetime_timer: Timer

var _limbs: Array = []
var _ragdoll_active: bool = false

func _ready():
	initialize_ragdoll()

func initialize_ragdoll():
	# Initialize ragdoll system
	_limb_connector = get_node_or_null(_limb_connector_path)
	_lifetime_timer = get_node_or_null(_lifetime_timer_path)
	
	# Find all ragdoll limbs
	find_all_limbs()
	
	# Set up lifetime timer
	if _lifetime_timer:
		_lifetime_timer.timeout.connect(_on_lifetime_timer_timeout)
	
	print("RagdollStickClone initialized with %d limbs" % _limbs.size())

func find_all_limbs():
	# Find all ragdoll limb nodes
	var limb_container = get_node_or_null("LimbContainer")
	if limb_container:
		for child in limb_container.get_children():
			if child is RigidBody2D:
				_limbs.append(child)
				# Set up collision for each limb
				child.contact_monitor = true
				child.max_contacts_reported = 4

func activate_ragdoll(force: Vector2 = Vector2.ZERO):
	# Activate ragdoll physics
	if _ragdoll_active:
		return
	
	_ragdoll_active = true
	ragdoll_activated.emit()
	
	# Activate all limbs
	for limb in _limbs:
		if limb.has_property("freeze"):
			limb.freeze = false
		
		# Apply initial force if provided
		if force != Vector2.ZERO and limb.has_property("apply_central_impulse"):
			limb.apply_central_impulse(force)
	
	# Start lifetime timer
	if _lifetime_timer:
		_lifetime_timer.start()
	
	print("Ragdoll activated with force: %s" % str(force))

func deactivate_ragdoll():
	# Deactivate ragdoll physics
	if not _ragdoll_active:
		return
	
	_ragdoll_active = false
	ragdoll_deactivated.emit()
	
	# Freeze all limbs
	for limb in _limbs:
		if limb.has_property("freeze"):
			limb.freeze = true
	
	print("Ragdoll deactivated")

func remove_limb(limb: RigidBody2D):
	# Remove a specific limb from the ragdoll
	if limb in _limbs:
		_limbs.erase(limb)
		limb.queue_free()
		ragdoll_part_removed.emit(limb.name)
		print("Removed limb: %s" % limb.name)

func get_limb_count() -> int:
	# Get number of active limbs
	return _limbs.size()

func is_active() -> bool:
	# Check if ragdoll is active
	return _ragdoll_active

func _on_lifetime_timer_timeout():
	# Ragdoll lifetime expired
	print("Ragdoll lifetime expired")
	deactivate_ragdoll()
	# Could remove the ragdoll entirely after deactivation
	var cleanup_timer = Timer.new()
	cleanup_timer.wait_time = 2.0
	cleanup_timer.one_shot = true
	cleanup_timer.timeout.connect(queue_free)
	add_child(cleanup_timer)
	cleanup_timer.start()

func apply_impulse_to_limb(limb_index: int, impulse: Vector2):
	# Apply impulse to a specific limb
	if limb_index >= 0 and limb_index < _limbs.size():
		var limb = _limbs[limb_index]
		if limb.has_property("apply_central_impulse"):
			limb.apply_central_impulse(impulse)

func set_gravity_scale(scale: float):
	# Set gravity scale for all limbs
	for limb in _limbs:
		if limb.has_property("gravity_scale"):
			limb.gravity_scale = scale

func _exit_tree():
	# Clean up connections
	if _lifetime_timer:
		_lifetime_timer.timeout.disconnect(_on_lifetime_timer_timeout)