extends Node2D

## ProjectilesLoader handles loading and managing projectiles for the slingshot system.

@export var _face_projectile_scene: PackedScene

func _ready():
	# Initialize projectiles loader
	print("ProjectilesLoader initialized")
	load_projectiles()

func load_projectiles():
	# Load the default projectiles for the level
	if _face_projectile_scene:
		# Pre-load some projectiles for performance
		for i in range(3):  # Load 3 projectiles initially
			var projectile = _face_projectile_scene.instantiate()
			projectile.visible = false
			add_child(projectile)

func get_next_projectile() -> Node2D:
	# Get the next available projectile
	for child in get_children():
		if child is Node2D and child.visible == false:
			child.visible = true
			return child
	
	# If no pre-loaded projectile available, create new one
	if _face_projectile_scene:
		var new_projectile = _face_projectile_scene.instantiate()
		add_child(new_projectile)
		return new_projectile
	
	return null

func reset_projectiles():
	# Reset all projectiles to initial state
	for child in get_children():
		if child.has_method("reset"):
			child.reset()
		child.visible = false