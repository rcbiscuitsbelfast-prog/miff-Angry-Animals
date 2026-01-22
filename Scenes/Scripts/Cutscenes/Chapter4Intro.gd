extends Node2D

## Chapter4Intro - Chapter 4 cutscene player.
## Handles cutscene dialogue, animations, and transitions.

@export var fallback_duration: float = 2.5
@export var animated_sprite_path: NodePath
@export var speakers: Array = []
@export var lines: Array = []
@export var durations: Array = []

var _animated_sprite: AnimatedSprite2D
var _cutscene_active: bool = false
var _current_line_index: int = 0
var _line_timer: Timer

func _ready():
	initialize_cutscene()

func initialize_cutscene():
	# Initialize the cutscene
	_animated_sprite = get_node_or_null(animated_sprite_path) as AnimatedSprite2D
	print("Chapter 4 Intro initialized with %d lines" % lines.size())

func start_cutscene():
	# Start the cutscene
	_cutscene_active = true
	_current_line_index = 0
	show_current_line()
	print("Starting Chapter 4 Intro cutscene")

func show_current_line():
	# Show the current dialogue line
	if _current_line_index >= lines.size():
		end_cutscene()
		return
	
	var current_speaker = speakers[_current_line_index] if _current_line_index < speakers.size() else "UNKNOWN"
	var current_line = lines[_current_line_index] as String
	var current_duration = durations[_current_line_index] as float if _current_line_index < durations.size() else fallback_duration
	
	print("%s: %s" % [current_speaker, current_line])
	
	if _line_timer:
		_line_timer.queue_free()
	
	_line_timer = Timer.new()
	_line_timer.wait_time = current_duration
	_line_timer.one_shot = true
	_line_timer.timeout.connect(_next_line)
	add_child(_line_timer)
	_line_timer.start()
	
	_current_line_index += 1

func end_cutscene():
	# End the cutscene
	_cutscene_active = false
	
	if _line_timer:
		_line_timer.queue_free()
	
	print("Chapter 4 Intro cutscene complete")

func _next_line():
	# Show next line
	show_current_line()

func skip_cutscene():
	# Skip to end
	end_cutscene()

func _exit_tree():
	# Clean up
	if _line_timer:
		_line_timer.queue_free()