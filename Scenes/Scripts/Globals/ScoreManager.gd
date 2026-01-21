extends Node
class_name ScoreManager

signal score_changed(score: int)
signal attempts_changed(attempts: int)

const DEFAULT_SCORE: int = 0
const SCORE_FILE: String = "user://animals.save"

var _score: int = 0
var _attempts: int = 0
var _selected_level: int = 0

var _level_scores: Array[LevelScore] = []

func _ready() -> void:
	process_mode = Node.PROCESS_MODE_ALWAYS
	
	var file_manager = get_node_or_null("/root/FileManager")
	if file_manager and file_manager.has_method("load_level_score_from_file"):
		_level_scores = file_manager.load_level_score_from_file(SCORE_FILE)
	
	reset_run()
	_deferred_connect_signals.call_deferred()

func _exit_tree() -> void:
	var file_manager = get_node_or_null("/root/FileManager")
	if file_manager and file_manager.has_method("save_level_score_to_file"):
		file_manager.save_level_score_to_file(SCORE_FILE, _level_scores)

func _deferred_connect_signals() -> void:
	var signal_manager = get_node_or_null("/root/SignalManager")
	if signal_manager:
		if signal_manager.has_signal("on_attempt_made"): signal_manager.on_attempt_made.connect(_on_attempt_made)
		if signal_manager.has_signal("on_score_updated"): signal_manager.on_score_updated.connect(_on_score_updated)
		if signal_manager.has_signal("on_destruction_score_updated"): signal_manager.on_destruction_score_updated.connect(_on_destruction_score_updated)

	var game_manager = get_node_or_null("/root/GameManager")
	if game_manager:
		if game_manager.has_signal("room_started"): game_manager.room_started.connect(_on_room_started)

func _on_room_started(_room_index: int) -> void:
	reset_run()

func _on_attempt_made() -> void:
	_attempts += 1
	attempts_changed.emit(_attempts)

func _on_score_updated(p_score: int) -> void:
	_score = p_score
	score_changed.emit(_score)

func _on_destruction_score_updated(p_score: int) -> void:
	_score = p_score
	score_changed.emit(_score)

func reset_run() -> void:
	_score = 0
	_attempts = 0
	score_changed.emit(_score)
	attempts_changed.emit(_attempts)

func get_level() -> int:
	return _selected_level

func set_level(new_level: int) -> void:
	_selected_level = new_level

func get_score() -> int:
	return _score

func get_attempts() -> int:
	return _attempts

func add_score(amount: int) -> void:
	if amount == 0:
		return
	_score += amount
	score_changed.emit(_score)
	var signal_manager = get_node_or_null("/root/SignalManager")
	if signal_manager and signal_manager.has_method("emit_on_destruction_score_updated"):
		signal_manager.emit_on_destruction_score_updated(_score)

func get_level_score_obj(level_number: int) -> LevelScore:
	for ls in _level_scores:
		if ls.level_number == level_number:
			return ls
	return null

func get_level_best_score(level_number: int) -> int:
	var ls = get_level_score_obj(level_number)
	return ls.best_score if ls else DEFAULT_SCORE

func set_level_score(level_number: int, score: int, stars: int = 0) -> void:
	var ls = get_level_score_obj(level_number)
	if ls:
		if score < ls.best_score or ls.best_score == 0:
			ls.best_score = score
			ls.date_set = Time.get_datetime_string_from_system()
		if stars > ls.star_rating:
			ls.star_rating = stars
	else:
		_level_scores.append(LevelScore.new(level_number, score, stars))

func get_level_star_rating(level_number: int) -> int:
	var ls = get_level_score_obj(level_number)
	return ls.star_rating if ls else 0
