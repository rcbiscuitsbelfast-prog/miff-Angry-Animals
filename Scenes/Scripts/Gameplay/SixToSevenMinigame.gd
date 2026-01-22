extends Control

## Six to Seven Minigame - Special minigame between levels 6 and 7.
## Provides entertaining break gameplay with MemeGateway integration.

signal minigame_completed(success: bool)
signal minigame_started()
signal minigame_ended()

var _minigame_active: bool = false
var _score: int = 0
var _time_remaining: float = 30.0

func _ready():
	initialize_minigame()

func initialize_minigame():
	# Initialize the minigame
	print("SixToSeven Minigame initialized")
	visible = false  # Hidden by default

func start_minigame():
	# Start the minigame
	_minigame_active = true
	_score = 0
	_time_remaining = 30.0
	visible = true
	minigame_started.emit()
	print("SixToSeven Minigame started")

func end_minigame(success: bool):
	# End the minigame
	_minigame_active = false
	visible = false
	minigame_ended.emit()
	minigame_completed.emit(success)
	print("SixToSeven Minigame ended with success: %s" % success)

func _process(delta):
	if _minigame_active:
		# Update timer
		_time_remaining -= delta
		if _time_remaining <= 0:
			end_minigame(false)  # Time up = fail

func add_points(points: int):
	# Add points to score
	_score += points

func get_score() -> int:
	return _score

func is_active() -> bool:
	return _minigame_active

func get_time_remaining() -> float:
	return _time_remaining