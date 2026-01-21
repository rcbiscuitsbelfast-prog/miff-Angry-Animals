extends CanvasLayer

signal next_level_pressed()
signal retry_pressed()
signal level_select_pressed()

@export var _completed_label: NodePath
@export var _score_label: NodePath
@export var _next_level_button: NodePath
@export var _retry_button: NodePath
@export var _level_select_button: NodePath
@export var _panel: NodePath

var _completed_label_node: Label
var _score_label_node: Label
var _next_level_button_node: Button
var _retry_button_node: Button
var _level_select_button_node: Button
var _panel_node: PanelContainer

func _ready():
	initialize_ui()
	connect_signals()

func initialize_ui():
	_completed_label_node = get_node_or_null(_completed_label) as Label
	_score_label_node = get_node_or_null(_score_label) as Label
	_next_level_button_node = get_node_or_null(_next_level_button) as Button
	_retry_button_node = get_node_or_null(_retry_button) as Button
	_level_select_button_node = get_node_or_null(_level_select_button) as Button
	_panel_node = get_node_or_null(_panel) as PanelContainer
	
	# Initially hide the panel
	if _panel_node:
		_panel_node.visible = false
	
	# Connect button signals
	if _next_level_button_node:
		_next_level_button_node.pressed.connect(_on_next_level_pressed)
	
	if _retry_button_node:
		_retry_button_node.pressed.connect(_on_retry_pressed)
	
	if _level_select_button_node:
		_level_select_button_node.pressed.connect(_on_level_select_pressed)

func connect_signals():
	# Connect to GameManager signals if available
	if GameManager.instance:
		GameManager.instance.room_completed.connect(_on_room_completed)
		GameManager.instance.game_state_changed.connect(_on_game_state_changed)

func _exit_tree():
	# Disconnect signals
	if GameManager.instance:
		GameManager.instance.room_completed.disconnect(_on_room_completed)
		GameManager.instance.game_state_changed.disconnect(_on_game_state_changed)
	
	# Disconnect button signals
	if _next_level_button_node:
		_next_level_button_node.pressed.disconnect(_on_next_level_pressed)
	
	if _retry_button_node:
		_retry_button_node.pressed.disconnect(_on_retry_pressed)
	
	if _level_select_button_node:
		_level_select_button_node.pressed.disconnect(_on_level_select_pressed)

func _on_room_completed(room_index: int):
	var level_number = room_index + 1
	var score = 0
	
	# Try to get score from ScoreManager
	if ScoreManager.instance:
		score = ScoreManager.get_score()
	
	show_completion(level_number, score)

func _on_game_state_changed(state):
	match state:
		GameManager.GameState.ROOM_COMPLETE:
			if _panel_node:
				_panel_node.visible = true
		_:
			if _panel_node:
				_panel_node.visible = false

func show_completion(level_number: int, score: int):
	# Update UI
	if _completed_label_node:
		_completed_label_node.text = "Level %d Complete!" % level_number
	
	if _score_label_node:
		_score_label_node.text = "Score: %d" % score
	
	# Show panel
	if _panel_node:
		_panel_node.visible = true

func _on_next_level_pressed():
	print("Next level button pressed")
	next_level_pressed.emit()
	
	if GameManager.instance:
		GameManager.instance.next_level()

func _on_retry_pressed():
	print("Retry button pressed")
	retry_pressed.emit()
	
	if GameManager.instance:
		GameManager.instance.restart_room()

func _on_level_select_pressed():
	print("Level select button pressed")
	level_select_pressed.emit()
	
	if GameManager.instance:
		GameManager.instance.load_main()