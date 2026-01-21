extends ConfirmationDialog
class_name CustomLevelInputDialog

var _share_code_input: TextEdit
var _status_label: Label

func _ready() -> void:
	title = "Load Custom Level"
	dialog_text = "Enter the share code from a friend:"
	
	# Create input field
	_share_code_input = TextEdit.new()
	_share_code_input.custom_minimum_size = Vector2(400, 100)
	_share_code_input.placeholder_text = "Paste share code here (e.g., AA1_...)"
	
	# Create status label
	_status_label = Label.new()
	_status_label.text = ""
	_status_label.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER

	# Add to dialog
	var vbox = VBoxContainer.new()
	vbox.add_child(_share_code_input)
	vbox.add_child(_status_label)
	add_child(vbox)

	# Connect signals
	confirmed.connect(_on_confirmed)
	canceled.connect(_on_canceled)

func _on_confirmed() -> void:
	var code = _share_code_input.text.strip_edges()
	
	if code.is_empty():
		_show_error("Please enter a share code")
		return

	# Try to decode the level (assuming CustomLevelCode.gd exists or will exist)
	var custom_level_code = get_node_or_null("/root/CustomLevelCode")
	if not custom_level_code or not custom_level_code.has_method("try_decode_level"):
		_show_error("Custom level decoder not available")
		return
		
	var level = custom_level_code.try_decode_level(code)
	if not level:
		_show_error("Invalid share code! Please check and try again.")
		return

	# Validate the level
	var custom_level_validator = get_node_or_null("/root/CustomLevelValidator")
	if custom_level_validator and custom_level_validator.has_method("validate_level"):
		var validation = custom_level_validator.validate_level(level)
		if not validation.is_valid:
			_show_error("Level cannot be played: %s" % validation.message)
			return

	# Load and play the level
	_load_custom_level(level)

func _on_canceled() -> void:
	queue_free()

func _load_custom_level(level: Node) -> void:
	var scene = load("res://Scenes/CustomPlay/CustomPlayRoom.tscn")
	if not scene:
		_show_error("Custom play room scene not found!")
		return

	var room = scene.instantiate()
	if room.has_method("load_custom_level"):
		room.load_custom_level(level)

	get_tree().root.add_child(room)
	get_tree().current_scene.queue_free()
	get_tree().current_scene = room

func _show_error(message: String) -> void:
	_status_label.text = message
	_status_label.add_theme_color_override("font_color", Color.RED)
	
	var error_dialog = AcceptDialog.new()
	error_dialog.dialog_text = message
	error_dialog.title = "Error"
	get_tree().root.add_child(error_dialog)
	error_dialog.popup_centered()

static func show_dialog(parent: Node) -> void:
	var dialog = load("res://Scenes/Scripts/Gameplay/CustomLevelInput.gd").new()
	parent.add_child(dialog)
	dialog.popup_centered()
