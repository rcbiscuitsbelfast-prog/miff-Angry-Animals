extends Node

## RagdollLimbConnector - Connects ragdoll limbs with physics joints.
## Handles limb connections, constraints, and physics interactions.

signal limb_connected(limb1: String, limb2: String)
signal limb_disconnected(limb1: String, limb2: String)

@export var joint_strength: float = 100.0
@export var joint_damping: float = 10.0

var _connected_limbs: Array = []
var _joints: Array = []

func _ready():
	initialize_connector()

func initialize_connector():
	# Initialize the limb connector
	print("RagdollLimbConnector initialized")

func connect_limbs(limb1: Node, limb2: Node):
	# Connect two limbs with a physics joint
	if not _connected_limbs.has(limb1) or not _connected_limbs.has(limb2):
		# Add limbs to connected list
		if not _connected_limbs.has(limb1):
			_connected_limbs.append(limb1)
		if not _connected_limbs.has(limb2):
			_connected_limbs.append(limb2)
		
		# Create a joint between the limbs
		create_joint(limb1, limb2)
		
		limb_connected.emit(limb1.name, limb2.name)
		print("Connected limbs: %s <-> %s" % [limb1.name, limb2.name])

func create_joint(limb1: Node, limb2: Node):
	# Create a physics joint between two limbs
	# This would typically use PinJoint2D, DampedSpringJoint2D, etc.
	# For now, we'll simulate the connection with constraints
	
	var joint_data = {
		"limb1": limb1,
		"limb2": limb2,
		"strength": joint_strength,
		"damping": joint_damping
	}
	_joints.append(joint_data)

func disconnect_limbs(limb1: Node, limb2: Node):
	# Disconnect two limbs
	if _connected_limbs.has(limb1):
		_connected_limbs.erase(limb1)
	if _connected_limbs.has(limb2):
		_connected_limbs.erase(limb2)
	
	# Remove joints between these limbs
	for i in range(_joints.size() - 1, -1, -1):
		var joint = _joints[i]
		if (joint.limb1 == limb1 and joint.limb2 == limb2) or (joint.limb1 == limb2 and joint.limb2 == limb1):
			_joints.remove_at(i)
	
	limb_disconnected.emit(limb1.name, limb2.name)
	print("Disconnected limbs: %s <-> %s" % [limb1.name, limb2.name])

func get_connected_limbs() -> Array:
	# Get list of connected limbs
	return _connected_limbs.duplicate()

func get_joint_count() -> int:
	# Get number of active joints
	return _joints.size()

func set_joint_strength(strength: float):
	# Set the strength for all joints
	joint_strength = strength
	for joint in _joints:
		joint.strength = strength

func set_joint_damping(damping: float):
	# Set the damping for all joints
	joint_damping = damping
	for joint in _joints:
		joint.damping = damping

func apply_joint_forces():
	# Apply forces to maintain joint constraints
	# This would be called each frame to maintain proper ragdoll physics
	for joint in _joints:
		apply_joint_constraint(joint)

func apply_joint_constraint(joint: Dictionary):
	# Apply physics constraints for a single joint
	var limb1 = joint.limb1
	var limb2 = joint.limb2
	
	if not is_instance_valid(limb1) or not is_instance_valid(limb2):
		return
	
	# Calculate distance between limbs
	var distance = limb1.global_position.distance_to(limb2.global_position)
	
	# Apply forces to maintain proper distance (simplified spring physics)
	var direction = (limb2.global_position - limb1.global_position).normalized()
	var desired_distance = 50.0  # Ideal joint length
	
	var force_magnitude = (distance - desired_distance) * joint.strength * 0.01
	
	# Apply forces to limbs
	if limb1.has_property("apply_central_force"):
		limb1.apply_central_force(-direction * force_magnitude)
		limb2.apply_central_force(direction * force_magnitude)

func _physics_process(delta):
	# Apply joint constraints each physics frame
	if _joints.size() > 0:
		apply_joint_forces()

func disconnect_all():
	# Disconnect all limbs and joints
	for limb in _connected_limbs:
		pass  # Would disconnect from all other limbs
	
	_connected_limbs.clear()
	_joints.clear()
	print("All limb connections disconnected")