extends CharacterBody2D

## StickClone character that spawns during traversal phase after slingshot destruction.
## Carries face customization from PlayerProfile and moves through the environment.

signal clone_reached_exit()
signal clone_stuck()
signal about_to_enter_explosion(explosion_position: Vector2)

@export var _jump_force: float = -400.0
@export var _gravity: float = 980.0
@export var _friction: float = 0.8
@export var _face_sprite_path: NodePath
@export var _hat_node_path: NodePath
@export var _glasses_node_path: NodePath

var _face_sprite: Sprite2D
var _hat_node: Node2D
var _glasses_node: Node2D

# Animation system
var _animator: Node

# Movement state
var _velocity: Vector2 = Vector2.ZERO
var _is_grounded: bool = false
var _is_moving: bool = false

# Face customization from PlayerProfile
var _current_hat: String = "none"
var _current_glasses: String = "none"
var _current_emotion: String = "neutral"

# References to room systems
var _current_room: Node
var _exit_area: Area2D

func _ready():
	initialize_stick_clone()
	load_face_customization()
	setup_physics()
	setup_explosion_detection()
	connect_signals()

func initialize_stick_clone():
	_face_sprite = get_node_or_null(_face_sprite_path) as Sprite2D
	_hat_node = get_node_or_null(_hat_node_path) as Node2D
	_glasses_node = get_node_or_null(_glasses_node_path) as Node2D

	# Initialize animation system - simplified for now
	_animator = Node.new()
	_animator.name = "Animator"
	add_child(_animator)

	# Get reference to current room
	_current_room = get_parent().get_parent() if get_parent() else null

	# Find exit area in the scene
	_exit_area = get_node_or_null("../ExitArea") as Area2D
	if _exit_area == null:
		_exit_area = get_node_or_null("ExitArea") as Area2D

	# Set up collision layers
	collision_layer = 1 # StickClone layer
	collision_mask = 2  # Environment layer

func load_face_customization():
	# Try to load customization from PlayerProfile if available
	if PlayerProfile.instance:
		var hats = PlayerProfile.get_hats()
		var glasses = PlayerProfile.get_glasses()
		var emotions = PlayerProfile.get_emotions()
		
		if hats.size() > PlayerProfile.instance.selected_hat_index:
			_current_hat = hats[PlayerProfile.instance.selected_hat_index]
		if glasses.size() > PlayerProfile.instance.selected_glasses_index:
			_current_glasses = glasses[PlayerProfile.instance.selected_glasses_index]
		if emotions.size() > PlayerProfile.instance.selected_emotion_index:
			_current_emotion = emotions[PlayerProfile.instance.selected_emotion_index]

	apply_face_customization()

func apply_face_customization():
	# Apply hat
	if _hat_node != null:
		apply_hat(_current_hat)

	# Apply glasses
	if _glasses_node != null:
		apply_glasses(_current_glasses)

	# Apply emotion to face sprite
	if _face_sprite != null:
		apply_emotion(_current_emotion)

func apply_hat(hat_name: String):
	# Simplified hat application
	if hat_name == "none":
		if _hat_node:
			_hat_node.visible = false
	elif hat_name == "cap":
		# Apply cap hat
		if _hat_node:
			_hat_node.visible = true
			_hat_node.position = Vector2(0, -35)
	elif hat_name == "top_hat":
		# Apply top hat
		if _hat_node:
			_hat_node.visible = true
			_hat_node.position = Vector2(0, -40)

func apply_glasses(glasses_name: String):
	# Simplified glasses application
	if glasses_name == "none":
		if _glasses_node:
			_glasses_node.visible = false
	elif glasses_name == "regular":
		# Apply regular glasses
		if _glasses_node:
			_glasses_node.visible = true
			_glasses_node.position = Vector2(0, -35)

func apply_emotion(emotion_name: String):
	# Simplified emotion application
	if _face_sprite:
		match emotion_name:
			"angry":
				_face_sprite.modulate = Color(1, 0.3, 0.3) # Red tint for angry
			"happy":
				_face_sprite.modulate = Color(0.3, 1, 0.3) # Green tint for happy
			"surprised":
				_face_sprite.modulate = Color(0.3, 0.3, 1) # Blue tint for surprised
			_:
				_face_sprite.modulate = Color(1, 1, 1) # Normal white

func setup_physics():
	# Set up physics properties
	velocity = _velocity

func setup_explosion_detection():
	# Set up explosion detection if needed
	pass

func connect_signals():
	# Connect to room signals if available
	if _current_room:
		if _current_room.has_signal("room_completed"):
			_current_room.room_completed.connect(_on_room_completed)
	
	# Connect to exit area
	if _exit_area:
		if _exit_area.has_signal("body_entered"):
			_exit_area.body_entered.connect(_on_exit_area_body_entered)

func _physics_process(delta):
	# Apply gravity
	if not _is_grounded:
		_velocity.y += _gravity * delta
	
	# Handle horizontal movement
	_velocity.x = move_toward(_velocity.x, 0, _friction * delta)
	
	# Apply velocity
	velocity = _velocity
	move_and_slide()

func _on_room_completed():
	# Room is completed, clone should stop or continue to exit
	pass

func _on_exit_area_body_entered(body):
	# Clone reached the exit
	if body == self:
		clone_reached_exit.emit()

func jump():
	if _is_grounded:
		_velocity.y = _jump_force
		_is_grounded = false

func move_left():
	_velocity.x = -150.0 # Default move speed
	_is_moving = true

func move_right():
	_velocity.x = 150.0 # Default move speed
	_is_moving = true

func stop_moving():
	_velocity.x = 0.0
	_is_moving = false

func get_current_emotion() -> String:
	return _current_emotion

func set_emotion(emotion: String):
	_current_emotion = emotion
	apply_emotion(emotion)