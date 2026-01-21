extends RigidBody2D

## FaceProjectile represents the projectiles (faces) launched from the slingshot.
## Handles physics, collisions, and effects.

signal projectile_launched()
signal projectile_impacted()
signal projectile_out_of_bounds()

@export var _face_sprite: NodePath
@export var _kick_wood_sound: NodePath
@export var _on_screen_notifier: NodePath

var _face_sprite_node: Sprite2D
var _kick_wood_sound_node: AudioStreamPlayer2D
var _on_screen_notifier_node: VisibleOnScreenNotifier2D

func _ready():
	initialize_projectile()
	connect_signals()

func initialize_projectile():
	_face_sprite_node = get_node_or_null(_face_sprite) as Sprite2D
	_kick_wood_sound_node = get_node_or_null(_kick_wood_sound) as AudioStreamPlayer2D
	_on_screen_notifier_node = get_node_or_null(_on_screen_notifier) as VisibleOnScreenNotifier2D
	
	# Set up collision layers
	collision_layer = 2
	collision_mask = 13
	
	# Start frozen until launched
	freeze = true
	
	# Set up contact monitoring
	max_contacts_reported = 4
	contact_monitor = true
	
	# Apply face customization if available
	apply_face_customization()

func connect_signals():
	# Connect to screen notifier
	if _on_screen_notifier_node:
		_on_screen_notifier_node.screen_exited.connect(_on_screen_exited)

func apply_face_customization():
	# Apply player face customization if PlayerProfile is available
	if PlayerProfile.instance and _face_sprite_node:
		# Load the selected face texture
		var selected_face = PlayerProfile.get_faces()[PlayerProfile.instance.selected_face_index]
		_face_sprite_node.texture = load("res://Assets/Faces/" + selected_face + ".png")

func launch(velocity: Vector2, position: Vector2):
	# Launch the projectile with given velocity and position
	global_position = position
	linear_velocity = velocity
	freeze = false
	
	projectile_launched.emit()

func _on_screen_exited():
	# Projectile has left the screen
	projectile_out_of_bounds.emit()
	reset()

func _physics_process(delta):
	# Handle projectile physics and effects
	if not freeze:
		# Check for impacts
		check_impacts()

func check_impacts():
	# Check for collision impacts and play appropriate effects
	var contact_count = get_contact_count()
	if contact_count > 0:
		# Play impact sound
		play_impact_sound()
		
		# Trigger impact effects
		projectile_impacted.emit()

func play_impact_sound():
	# Play the kick wood sound on impact
	if _kick_wood_sound_node:
		_kick_wood_sound_node.play()

func reset():
	# Reset projectile to initial state
	freeze = true
	linear_velocity = Vector2.ZERO
	angular_velocity = 0
	visible = false

func get_face_texture() -> Texture2D:
	# Return the current face texture
	return _face_sprite_node.texture if _face_sprite_node else null

func set_face_texture(texture: Texture2D):
	# Set the face texture
	if _face_sprite_node:
		_face_sprite_node.texture = texture