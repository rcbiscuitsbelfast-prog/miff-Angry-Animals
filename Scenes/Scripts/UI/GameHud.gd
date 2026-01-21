extends Control

## Main HUD controller that manages all heads-up display elements.
## Handles attempts counter, rage bar, combo counter, and pause functionality.

signal pause_requested()
signal resume_requested()

@export var _attempts_label_path: NodePath
@export var _rage_bar_path: NodePath
@export var _rage_fill_path: NodePath
@export var _combo_label_path: NodePath
@export var _pause_button_path: NodePath
@export var _pause_panel_path: NodePath
@export var _resume_button_path: NodePath
@export var _quit_button_path: NodePath
@export var _score_label_path: NodePath

var _attempts_label: Label
var _rage_bar: ProgressBar
var _rage_fill: ColorRect
var _combo_label: Label
var _pause_button: Button
var _pause_panel: Panel
var _resume_button: Button
var _quit_button: Button
var _score_label: Label

var _objective_label: Label
var _story_label: Label

func _ready():
	initialize_hud()
	connect_signals()
	setup_input_map()

func initialize_hud():
	_attempts_label = get_node_or_null(_attempts_label_path) as Label
	_rage_bar = get_node_or_null(_rage_bar_path) as ProgressBar
	_rage_fill = get_node_or_null(_rage_fill_path) as ColorRect
	_combo_label = get_node_or_null(_combo_label_path) as Label
	_pause_button = get_node_or_null(_pause_button_path) as Button
	_pause_panel = get_node_or_null(_pause_panel_path) as Panel
	_resume_button = get_node_or_null(_resume_button_path) as Button
	_quit_button = get_node_or_null(_quit_button_path) as Button
	_score_label = get_node_or_null(_score_label_path) as Label
	
	# Initialize UI elements
	if _pause_panel:
		_pause_panel.visible = false
	
	# Initialize values
	update_attempts_label(0)
	update_rage_bar(0.0)
	update_combo_label(0)
	update_score_label(0)
	
	ensure_story_and_objective_labels()
	
	# Connect button signals
	if _pause_button:
		_pause_button.pressed.connect(_on_pause_button_pressed)
	
	if _resume_button:
		_resume_button.pressed.connect(_on_resume_button_pressed)
	
	if _quit_button:
		_quit_button.pressed.connect(_on_quit_button_pressed)

func ensure_story_and_objective_labels():
	if _story_label == null:
		_story_label = Label.new()
		_story_label.name = "StoryLabel"
		_story_label.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
		_story_label.vertical_alignment = VERTICAL_ALIGNMENT_CENTER
		_story_label.text = ""
		_story_label.set_anchors_preset(Control.PRESET_TOP_WIDE)
		_story_label.offset_top = 80
		_story_label.offset_bottom = 120
		_story_label.add_theme_font_size_override("font_size", 24)
		add_child(_story_label)
	
	if _objective_label == null:
		_objective_label = Label.new()
		_objective_label.name = "ObjectiveLabel"
		_objective_label.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
		_objective_label.vertical_alignment = VERTICAL_ALIGNMENT_CENTER
		_objective_label.text = ""
		_objective_label.set_anchors_preset(Control.PRESET_BOTTOM_WIDE)
		_objective_label.offset_bottom = -80
		_objective_label.offset_top = -120
		_objective_label.add_theme_font_size_override("font_size", 24)
		add_child(_objective_label)

func connect_signals():
	# Connect to GameManager signals if available
	if GameManager.instance:
		GameManager.instance.game_state_changed.connect(_on_game_state_changed)
	
	# Connect to ScoreManager signals if available
	if ScoreManager.instance:
		ScoreManager.instance.score_changed.connect(_on_score_changed)
	
	# Connect to SignalManager signals if available
	if SignalManager.instance:
		SignalManager.instance.combo_changed.connect(_on_combo_changed)

func setup_input_map():
	# Set up pause input (ESC key)
	if not InputMap.has_action("ui_cancel"):
		InputMap.add_action("ui_cancel")
		var cancel_event := InputEventKey.new()
		cancel_event.keycode = Key.Escape
		InputMap.action_add_event("ui_cancel", cancel_event)

func _exit_tree():
	# Disconnect signals
	if GameManager.instance:
		GameManager.instance.game_state_changed.disconnect(_on_game_state_changed)
	
	if ScoreManager.instance:
		ScoreManager.instance.score_changed.disconnect(_on_score_changed)
	
	if SignalManager.instance:
		SignalManager.instance.combo_changed.disconnect(_on_combo_changed)

func _input(event):
	if event.is_action_pressed("ui_cancel"):
		_toggle_pause()

func _on_game_state_changed(state):
	match state:
		GameManager.GameState.PAUSED:
			show_pause_panel()
		GameManager.GameState.PLAYING:
			hide_pause_panel()
		_:
			hide_pause_panel()

func _on_score_changed(score: int):
	update_score_label(score)

func _on_combo_changed(combo: int):
	update_combo_label(combo)

func update_attempts_label(attempts: int):
	if _attempts_label:
		_attempts_label.text = "Attempts: %d" % attempts

func update_rage_bar(rage_value: float):
	if _rage_bar:
		_rage_bar.value = rage_value
	if _rage_fill:
		var color = Color(1, rage_value, 0) # Red to yellow based on rage
		_rage_fill.modulate = color

func update_combo_label(combo: int):
	if _combo_label:
		_combo_label.text = "Combo: %d" % combo

func update_score_label(score: int):
	if _score_label:
		_score_label.text = "Score: %d" % score

func update_objective_label(text: String):
	if _objective_label:
		_objective_label.text = text

func update_story_label(text: String):
	if _story_label:
		_story_label.text = text

func _on_pause_button_pressed():
	_toggle_pause()

func _on_resume_button_pressed():
	resume_requested.emit()
	hide_pause_panel()

func _on_quit_button_pressed():
	# Quit to main menu
	if GameManager.instance:
		GameManager.instance.load_main()

func _toggle_pause():
	if _pause_panel and _pause_panel.visible:
		resume_requested.emit()
		hide_pause_panel()
	else:
		pause_requested.emit()
		show_pause_panel()

func show_pause_panel():
	if _pause_panel:
		_pause_panel.visible = true

func hide_pause_panel():
	if _pause_panel:
		_pause_panel.visible = false