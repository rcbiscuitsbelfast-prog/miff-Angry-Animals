extends Control

## Game over screen that handles level failure with retry and monetization options.

signal retry_pressed()
signal watch_ad_pressed()
signal select_level_pressed()
signal menu_pressed()

@export var _retry_button_path: NodePath
@export var _watch_ad_button_path: NodePath
@export var _select_level_button_path: NodePath
@export var _menu_button_path: NodePath
@export var _status_label_path: NodePath

var _retry_button: Button
var _watch_ad_button: Button
var _select_level_button: Button
var _menu_button: Button
var _status_label: Label

func _ready():
	initialize_ui()
	connect_signals()

func initialize_ui():
	_retry_button = get_node_or_null(_retry_button_path) as Button
	_watch_ad_button = get_node_or_null(_watch_ad_button_path) as Button
	_select_level_button = get_node_or_null(_select_level_button_path) as Button
	_menu_button = get_node_or_null(_menu_button_path) as Button
	_status_label = get_node_or_null(_status_label_path) as Label
	
	# Initially hide the screen
	visible = false
	
	# Connect button signals
	if _retry_button:
		_retry_button.pressed.connect(_on_retry_pressed)
	
	if _watch_ad_button:
		_watch_ad_button.pressed.connect(_on_watch_ad_pressed)
	
	if _select_level_button:
		_select_level_button.pressed.connect(_on_select_level_pressed)
	
	if _menu_button:
		_menu_button.pressed.connect(_on_menu_pressed)

func connect_signals():
	# Connect to GameManager signals if available
	if GameManager.instance:
		GameManager.instance.game_state_changed.connect(_on_game_state_changed)

func _exit_tree():
	# Disconnect signals
	if GameManager.instance:
		GameManager.instance.game_state_changed.disconnect(_on_game_state_changed)
	
	# Disconnect button signals
	if _retry_button:
		_retry_button.pressed.disconnect(_on_retry_pressed)
	
	if _watch_ad_button:
		_watch_ad_button.pressed.disconnect(_on_watch_ad_pressed)
	
	if _select_level_button:
		_select_level_button.pressed.disconnect(_on_select_level_pressed)
	
	if _menu_button:
		_menu_button.pressed.disconnect(_on_menu_pressed)

func _on_game_state_changed(state):
	match state:
		GameManager.GameState.GAME_OVER:
			show_game_over()
		_:
			hide_game_over()

func show_game_over(status_text: String = "You ran out of projectiles"):
	# Update status text
	if _status_label:
		_status_label.text = status_text
	
	# Show the screen
	visible = true

func hide_game_over():
	# Hide the screen
	visible = false

func _on_retry_pressed():
	print("Retry button pressed")
	retry_pressed.emit()
	
	if GameManager.instance:
		GameManager.instance.restart_room()

func _on_watch_ad_pressed():
	print("Watch ad button pressed")
	watch_ad_pressed.emit()
	
	# Show rewarded ad if available
	if AdsManager.instance:
		AdsManager.instance.show_rewarded_ad()
	else:
		print("AdsManager not available")

func _on_select_level_pressed():
	print("Select level button pressed")
	select_level_pressed.emit()
	
	if GameManager.instance:
		GameManager.instance.load_main()

func _on_menu_pressed():
	print("Menu button pressed")
	menu_pressed.emit()
	
	if GameManager.instance:
		GameManager.instance.load_main()