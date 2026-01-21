extends Control
class_name RoomSelection

signal room_selected(room_index: int)

@export var rooms_container_path: NodePath
@export var title_label_path: NodePath
@export var back_button_path: NodePath
@export var room_button_scene: PackedScene

var _rooms_container: VBoxContainer
var _title_label: Label
var _back_button: Button

var _unlock_full_game_button: Button
var _purchase_dialog: AcceptDialog
var _procedural_mode_toggle: CheckButton

var _seed_input: LineEdit
var _random_seed_button: Button
var _deterministic_seed_button: Button
var _use_last_seed_button: Button
var _slingshot_type_selector: OptionButton
var _slingshot_type_container: HBoxContainer

func _ready() -> void:
	_initialize_ui()
	_connect_signals()
	_populate_room_buttons()
	_preload_interstitial_ads_async()

func _preload_interstitial_ads_async() -> void:
	await get_tree().create_timer(1.0).timeout
	var ads_manager = get_node_or_null("/root/AdsManager")
	var monetization_manager = get_node_or_null("/root/MonetizationManager")
	if ads_manager and monetization_manager and monetization_manager.show_ads:
		if ads_manager.has_method("load_interstitial_ad"):
			ads_manager.load_interstitial_ad()

func _initialize_ui() -> void:
	_rooms_container = get_node_or_null(rooms_container_path)
	_title_label = get_node_or_null(title_label_path)
	_back_button = get_node_or_null(back_button_path)

	if _title_label:
		_title_label.text = "Select a Room"

	if _back_button:
		_back_button.text = "Back to Main Menu"
		_back_button.pressed.connect(_on_back_button_pressed)

	_purchase_dialog = AcceptDialog.new()
	_purchase_dialog.title = "Purchase"
	_purchase_dialog.process_mode = Node.PROCESS_MODE_ALWAYS
	add_child(_purchase_dialog)

func _connect_signals() -> void:
	var game_manager = get_node_or_null("/root/GameManager")
	if game_manager:
		game_manager.game_state_changed.connect(_on_game_state_changed)

	var signal_manager = get_node_or_null("/root/SignalManager")
	if signal_manager:
		if signal_manager.has_signal("on_level_completed"):
			signal_manager.on_level_completed.connect(_on_level_completed)

	var monetization_manager = get_node_or_null("/root/MonetizationManager")
	if monetization_manager:
		if monetization_manager.has_signal("purchase_succeeded"): monetization_manager.purchase_succeeded.connect(_on_purchase_succeeded)
		if monetization_manager.has_signal("purchase_failed"): monetization_manager.purchase_failed.connect(_on_purchase_failed)

func _populate_room_buttons() -> void:
	if not _rooms_container:
		return
	
	for child in _rooms_container.get_children():
		child.queue_free()
	
	_add_generation_controls()
	
	var game_manager = get_node_or_null("/root/GameManager")
	if game_manager:
		for i in range(game_manager.rooms.size()):
			var room_info = game_manager.rooms[i]
			var is_unlocked = _is_room_accessible(i)
			var room_button = _create_room_button(i, room_info, is_unlocked)
			_rooms_container.add_child(room_button)
		
		_create_or_update_unlock_button()

func _add_generation_controls() -> void:
	var header = VBoxContainer.new()
	header.name = "GenerationControls"
	header.size_flags_horizontal = Control.SIZE_FLAGS_EXPAND_FILL
	
	var player_profile = get_node_or_null("/root/PlayerProfile")
	var procedural_enabled = player_profile.use_procedural_levels if player_profile else false
	
	_procedural_mode_toggle = CheckButton.new()
	_procedural_mode_toggle.name = "ProceduralModeToggle"
	_procedural_mode_toggle.text = "Procedural Levels: ON" if procedural_enabled else "Procedural Levels: OFF"
	_procedural_mode_toggle.size_flags_horizontal = Control.SIZE_FLAGS_EXPAND_FILL
	_procedural_mode_toggle.button_pressed = procedural_enabled
	_procedural_mode_toggle.toggled.connect(_on_procedural_mode_toggled)
	header.add_child(_procedural_mode_toggle)
	
	_add_slingshot_type_selector(header)
	
	var seed_row = HBoxContainer.new()
	seed_row.name = "SeedRow"
	seed_row.size_flags_horizontal = Control.SIZE_FLAGS_EXPAND_FILL
	
	var seed_label = Label.new()
	seed_label.text = "Seed:"
	seed_label.modulate = Color.YELLOW
	
	_seed_input = LineEdit.new()
	_seed_input.name = "SeedInput"
	_seed_input.size_flags_horizontal = Control.SIZE_FLAGS_EXPAND_FILL
	_seed_input.placeholder_text = "0 = deterministic (per level)"
	_seed_input.text = "0"
	
	_random_seed_button = Button.new()
	_random_seed_button.text = "Random"
	_random_seed_button.pressed.connect(_on_random_seed_pressed)
	
	_use_last_seed_button = Button.new()
	_use_last_seed_button.text = "Use Last"
	_use_last_seed_button.pressed.connect(_on_use_last_seed_pressed)
	
	seed_row.add_child(seed_label)
	seed_row.add_child(_seed_input)
	seed_row.add_child(_random_seed_button)
	seed_row.add_child(_use_last_seed_button)
	
	_deterministic_seed_button = Button.new()
	_deterministic_seed_button.text = "Deterministic"
	_deterministic_seed_button.size_flags_horizontal = Control.SIZE_FLAGS_EXPAND_FILL
	_deterministic_seed_button.pressed.connect(_on_deterministic_seed_pressed)
	
	seed_row.visible = procedural_enabled
	_deterministic_seed_button.visible = procedural_enabled
	
	header.add_child(seed_row)
	header.add_child(_deterministic_seed_button)
	
	_rooms_container.add_child(header)
	_rooms_container.add_child(HSeparator.new())

func _on_procedural_mode_toggled(enabled: bool) -> void:
	var player_profile = get_node_or_null("/root/PlayerProfile")
	if player_profile and player_profile.has_method("set_procedural_mode"):
		player_profile.set_procedural_mode(enabled)
	_populate_room_buttons.call_deferred()

func _add_slingshot_type_selector(parent: VBoxContainer) -> void:
	_slingshot_type_container = HBoxContainer.new()
	_slingshot_type_container.size_flags_horizontal = Control.SIZE_FLAGS_EXPAND_FILL
	
	var type_label = Label.new()
	type_label.text = "Slingshot:"
	type_label.modulate = Color.CYAN
	type_label.custom_minimum_size = Vector2(100, 0)
	
	_slingshot_type_selector = OptionButton.new()
	_slingshot_type_selector.size_flags_horizontal = Control.SIZE_FLAGS_EXPAND_FILL
	
	_slingshot_type_selector.add_item("Catapult", 0)
	_slingshot_type_selector.add_item("Giant Hand", 1)
	_slingshot_type_selector.add_item("Trebuchet", 2)
	_slingshot_type_selector.add_item("Spring", 3)
	
	var player_profile = get_node_or_null("/root/PlayerProfile")
	var current_type = player_profile.get_slingshot_type() if player_profile and player_profile.has_method("get_slingshot_type") else 0
	_slingshot_type_selector.selected = clamp(current_type, 0, 3)
	_slingshot_type_selector.item_selected.connect(_on_slingshot_type_selected)
	
	_slingshot_type_container.add_child(type_label)
	_slingshot_type_container.add_child(_slingshot_type_selector)
	parent.add_child(_slingshot_type_container)
	parent.add_child(HSeparator.new())

func _on_slingshot_type_selected(index: int) -> void:
	var player_profile = get_node_or_null("/root/PlayerProfile")
	if player_profile and player_profile.has_method("set_slingshot_type"):
		player_profile.set_slingshot_type(index)

func _on_random_seed_pressed() -> void:
	var level_generator = get_node_or_null("/root/LevelGenerator")
	if level_generator and level_generator.has_method("create_random_seed"):
		_seed_input.text = str(level_generator.create_random_seed())

func _on_deterministic_seed_pressed() -> void:
	_seed_input.text = "0"

func _on_use_last_seed_pressed() -> void:
	var player_profile = get_node_or_null("/root/PlayerProfile")
	if player_profile:
		_seed_input.text = str(player_profile.last_procedural_seed)

func _is_room_accessible(room_index: int) -> bool:
	var monetization_manager = get_node_or_null("/root/MonetizationManager")
	var full_unlocked = monetization_manager.is_full_game_unlocked if monetization_manager else false
	if full_unlocked:
		return true
	if room_index >= 20:
		return false
	var player_profile = get_node_or_null("/root/PlayerProfile")
	if player_profile and player_profile.has_method("is_room_unlocked"):
		return player_profile.is_room_unlocked(room_index)
	return room_index == 0

func _create_room_button(room_index: int, room_info: Node, is_unlocked: bool) -> Button:
	var button = Button.new()
	button.size_flags_horizontal = Control.SIZE_FLAGS_EXPAND_FILL
	button.custom_minimum_size = Vector2(400, 60)
	
	var container = HBoxContainer.new()
	container.size_flags_horizontal = Control.SIZE_FLAGS_EXPAND_FILL
	
	var display_name = room_info.description
	var room_label = Label.new()
	room_label.text = "%d. %s" % [room_index + 1, display_name]
	room_label.size_flags_horizontal = Control.SIZE_FLAGS_EXPAND_FILL
	
	var player_profile = get_node_or_null("/root/PlayerProfile")
	var procedural_enabled = player_profile.use_procedural_levels if player_profile else false
	
	var score_label = Label.new()
	if procedural_enabled:
		var level_generator = get_node_or_null("/root/LevelGenerator")
		var cup_count = level_generator.get_cup_count(room_index + 1) if level_generator and level_generator.has_method("get_cup_count") else 0
		score_label.text = "Cups: %d" % cup_count
		score_label.modulate = Color.CYAN
	else:
		score_label.text = "Optimal: %d" % room_info.optimal_score
		score_label.modulate = Color.YELLOW
	
	var lock_label = Label.new()
	if is_unlocked:
		lock_label.text = "✓"
		lock_label.modulate = Color.GREEN
		button.disabled = false
	else:
		lock_label.text = "🔒"
		lock_label.modulate = Color.RED
		button.disabled = true
		if room_index >= 20:
			button.tooltip_text = "Unlock Full Game to access levels 21-100"
		else:
			button.tooltip_text = "Complete previous rooms to unlock"
	
	container.add_child(room_label)
	var spacer = Control.new()
	spacer.size_flags_horizontal = Control.SIZE_FLAGS_EXPAND_FILL
	container.add_child(spacer)
	container.add_child(score_label)
	container.add_child(lock_label)
	button.add_child(container)
	
	if not button.disabled:
		button.pressed.connect(_on_room_button_pressed.bind(room_index))
	
	return button

func _create_or_update_unlock_button() -> void:
	var monetization_manager = get_node_or_null("/root/MonetizationManager")
	var game_manager = get_node_or_null("/root/GameManager")
	var is_full_unlocked = monetization_manager.is_full_game_unlocked if monetization_manager else false
	var show_unlock = not is_full_unlocked and game_manager and game_manager.rooms.size() > 20
	
	if not show_unlock:
		_unlock_full_game_button = null
		return
	
	_unlock_full_game_button = Button.new()
	_unlock_full_game_button.text = "Unlock Full Game - £1.50"
	_unlock_full_game_button.size_flags_horizontal = Control.SIZE_FLAGS_EXPAND_FILL
	_unlock_full_game_button.custom_minimum_size = Vector2(400, 60)
	_unlock_full_game_button.modulate = Color(1.0, 0.95, 0.5)
	_unlock_full_game_button.pressed.connect(_on_unlock_button_pressed)
	
	_rooms_container.add_child(HSeparator.new())
	_rooms_container.add_child(_unlock_full_game_button)

func _on_unlock_button_pressed() -> void:
	if _unlock_full_game_button:
		_unlock_full_game_button.disabled = true
		_unlock_full_game_button.text = "Unlocking..."
	
	var monetization_manager = get_node_or_null("/root/MonetizationManager")
	if monetization_manager and monetization_manager.has_method("purchase_full_game"):
		monetization_manager.purchase_full_game()
	else:
		_on_purchase_failed("Monetization manager unavailable.")

func _on_purchase_succeeded() -> void:
	_purchase_dialog.dialog_text = "Purchase Complete! Enjoy all 100 levels!"
	_purchase_dialog.popup_centered()
	_populate_room_buttons.call_deferred()

func _on_purchase_failed(reason: String) -> void:
	if _unlock_full_game_button:
		_unlock_full_game_button.disabled = false
		_unlock_full_game_button.text = "Unlock Full Game - £1.50"
	_purchase_dialog.dialog_text = reason if not reason.is_empty() else "Purchase failed."
	_purchase_dialog.popup_centered()

func _on_room_button_pressed(room_index: int) -> void:
	var player_profile = get_node_or_null("/root/PlayerProfile")
	if player_profile and player_profile.use_procedural_levels:
		var seed_val = int(_seed_input.text)
		var room_number = room_index + 1
		var level_generator = get_node_or_null("/root/LevelGenerator")
		var effective_seed = seed_val if seed_val != 0 else (level_generator.calculate_seed(room_number) if level_generator else 0)
		player_profile.last_procedural_seed = effective_seed
		player_profile.last_procedural_level_number = room_number
		if player_profile.has_method("save_profile"): player_profile.save_profile()
		DisplayServer.clipboard_set(str(effective_seed))
	
	room_selected.emit(room_index)
	var game_manager = get_node_or_null("/root/GameManager")
	if game_manager: game_manager.start_room(room_index)

func _on_back_button_pressed() -> void:
	var game_manager = get_node_or_null("/root/GameManager")
	if game_manager: game_manager.load_main()

func _on_game_state_changed(_state: int) -> void:
	pass

func _on_level_completed() -> void:
	pass
