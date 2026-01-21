extends CanvasLayer

## Main menu controller that handles the main menu interface.
## Provides navigation to room selection, settings, and other menu options.

signal play_button_pressed()
signal room_selection_button_pressed()
signal settings_button_pressed()
signal quit_button_pressed()

@export var _play_button: NodePath
@export var _quit_button: NodePath
@export var _title_container: NodePath
@export var _title_label: NodePath

var _play_button_node: Button
var _quit_button_node: Button
var _title_label_node: Label

var _unlock_full_game_button: Button
var _unlock_confirmation: ConfirmationDialog
var _purchase_complete_dialog: AcceptDialog
var _purchase_failed_dialog: ConfirmationDialog

var _purchase_in_progress: bool = false

func _ready():
	initialize_menu()
	add_daily_challenge_button()
	add_customize_face_button()
	add_level_editor_buttons()
	add_unlock_full_game_button()
	add_retention_system_buttons()
	add_telemetry_metrics_button()
	connect_signals()
	setup_input_map()
	
	# Initialize retention systems
	initialize_retention_systems()

func add_daily_challenge_button():
	if _play_button_node != null and _play_button_node.get_parent() is Control container:
		var daily_btn := Button.new()
		daily_btn.text = "Daily Challenge"
		daily_btn.name = "DailyChallengeButton"
		daily_btn.modulate = Color(0.5, 1, 0.5)
		daily_btn.pressed.connect(_on_daily_challenge_button_pressed)
		
		container.add_child(daily_btn)
		
		if _play_button_node != null:
			container.move_child(daily_btn, _play_button_node.get_index() + 1)

func _on_daily_challenge_button_pressed():
	print("Daily Challenge button pressed")
	play_ui_click_sound()
	
	if DailyChallengeManager.instance:
		DailyChallengeManager.instance.start_daily_challenge()

func add_customize_face_button():
	if _play_button_node != null and _play_button_node.get_parent() is Control container:
		var customize_btn := Button.new()
		customize_btn.text = "Customize Face"
		customize_btn.name = "CustomizeFaceButton"
		customize_btn.pressed.connect(_on_customize_face_button_pressed)
		
		container.add_child(customize_btn)

func _on_customize_face_button_pressed():
	print("Customize Face button pressed")
	play_ui_click_sound()
	
	var screen = load("res://Scenes/FaceCustomizationScreen.tscn").instantiate()
	get_tree().root.add_child(screen)

func add_level_editor_buttons():
	if _play_button_node != null and _play_button_node.get_parent() is Control container:
		var create_level_btn := Button.new()
		create_level_btn.text = "Create Level"
		create_level_btn.name = "CreateLevelButton"
		create_level_btn.modulate = Color(0.5, 0.8, 1)
		create_level_btn.pressed.connect(_on_create_level_button_pressed)
		container.add_child(create_level_btn)
		
		var play_custom_btn := Button.new()
		play_custom_btn.text = "📁 Play Custom Levels"
		play_custom_btn.name = "PlayCustomLevelsButton"
		play_custom_btn.modulate = Color(0.7, 0.5, 1)
		play_custom_btn.pressed.connect(_on_play_custom_levels_button_pressed)
		container.add_child(play_custom_btn)
		
		var generate_levels_btn := Button.new()
		generate_levels_btn.text = "🎲 Generate 100 Levels"
		generate_levels_btn.name = "GenerateLevelsButton"
		generate_levels_btn.modulate = Color(1, 0.8, 0.2)
		generate_levels_btn.pressed.connect(_on_generate_levels_button_pressed)
		container.add_child(generate_levels_btn)

func _on_create_level_button_pressed():
	print("Create Level button pressed")
	play_ui_click_sound()
	get_tree().change_scene_to_file("res://Scenes/LevelEditor/LevelEditor.tscn")

func _on_play_custom_levels_button_pressed():
	print("Play Custom Levels button pressed")
	play_ui_click_sound()
	get_tree().change_scene_to_file("res://Scenes/Levels/LevelBrowser.tscn")

func _on_generate_levels_button_pressed():
	print("Generate 100 Levels button pressed")
	play_ui_click_sound()
	
	# Show confirmation dialog
	var dialog := ConfirmationDialog.new()
	dialog.title = "Generate 100 Levels"
	dialog.dialog_text = "This will generate 100 themed levels procedurally. This may take moment.\n\nContinue?"
	dialog.confirmed.connect(_on_generate_levels_confirmed)
	
	add_child(dialog)
	dialog.popup_centered()

func _on_generate_levels_confirmed():
	print("🎲 Starting batch level generation...")
	GenerateAllLevels.generate_all_100_levels()
	
	# Show completion dialog
	var complete_dialog := AcceptDialog.new()
	complete_dialog.title = "Generation Complete"
	complete_dialog.dialog_text = "✅ Successfully generated 100 themed levels!\n\nYou can now play them from the Level Browser."
	add_child(complete_dialog)
	complete_dialog.popup_centered()

func add_unlock_full_game_button():
	if _play_button_node == null or not (_play_button_node.get_parent() is Control container):
		return
	
	if MonetizationManager.instance and MonetizationManager.instance.is_full_game_unlocked:
		return
	
	_unlock_full_game_button = Button.new()
	_unlock_full_game_button.text = "Unlock Full Game - £1.50"
	_unlock_full_game_button.name = "UnlockFullGameButton"
	_unlock_full_game_button.modulate = Color(1, 0.95, 0.5)
	_unlock_full_game_button.pressed.connect(_on_unlock_button_pressed)
	
	container.add_child(_unlock_full_game_button)

func add_telemetry_metrics_button():
	# Only for debug builds
	if not Engine.is_editor_hint() and OS.has_feature("debug"):
		return
		
	if _play_button_node == null or not (_play_button_node.get_parent() is Control container):
		return
	
	var telemetry_btn := Button.new()
	telemetry_btn.text = "📊 View Metrics"
	telemetry_btn.name = "TelemetryMetricsButton"
	telemetry_btn.modulate = Color(0.5, 0.8, 1)
	telemetry_btn.custom_minimum_size = Vector2(200, 40)
	telemetry_btn.pressed.connect(_on_telemetry_metrics_button_pressed)
	
	container.add_child(telemetry_btn)

func _on_telemetry_metrics_button_pressed():
	print("Telemetry Metrics button pressed")
	play_ui_click_sound()
	
	# Show the telemetry debug panel - would need to implement this
	print("Telemetry panel functionality needs to be implemented")

func initialize_menu():
	_play_button_node = get_node_or_null(_play_button) as Button
	_quit_button_node = get_node_or_null(_quit_button) as Button
	_title_label_node = get_node_or_null(_title_label) as Label
	
	if _title_label_node != null:
		_title_label_node.text = "Angry Animals"
	
	if _play_button_node != null:
		_play_button_node.pressed.connect(_on_play_button_pressed)
	
	if _quit_button_node != null:
		_quit_button_node.pressed.connect(_on_quit_button_pressed)
	
	ensure_dialogs()

func ensure_dialogs():
	_unlock_confirmation = ConfirmationDialog.new()
	_unlock_confirmation.name = "UnlockConfirmationDialog"
	_unlock_confirmation.title = "Unlock Full Game"
	_unlock_confirmation.dialog_text = "Unlock all 100 levels and remove ads?"
	_unlock_confirmation.process_mode = ProcessMode.Always
	_unlock_confirmation.confirmed.connect(_on_unlock_confirmation_accepted)
	add_child(_unlock_confirmation)
	
	_purchase_complete_dialog = AcceptDialog.new()
	_purchase_complete_dialog.name = "PurchaseCompleteDialog"
	_purchase_complete_dialog.title = "Purchase Complete"
	_purchase_complete_dialog.dialog_text = "Purchase Complete! Enjoy all 100 levels!"
	_purchase_complete_dialog.process_mode = ProcessMode.Always
	add_child(_purchase_complete_dialog)
	
	_purchase_failed_dialog = ConfirmationDialog.new()
	_purchase_failed_dialog.name = "PurchaseFailedDialog"
	_purchase_failed_dialog.title = "Purchase Failed"
	_purchase_failed_dialog.dialog_text = "Purchase failed."
	_purchase_failed_dialog.process_mode = ProcessMode.Always
	_purchase_failed_dialog.confirmed.connect(_on_purchase_retry)
	_purchase_failed_dialog.canceled.connect(_on_purchase_failed_dialog_canceled)
	add_child(_purchase_failed_dialog)

func connect_signals():
	if GameManager.instance:
		GameManager.instance.game_state_changed.connect(_on_game_state_changed)
	
	if MonetizationManager.instance:
		MonetizationManager.instance.purchase_succeeded.connect(_on_purchase_completed)
		MonetizationManager.instance.purchase_failed.connect(_on_purchase_failed)

func _exit_tree():
	if GameManager.instance:
		GameManager.instance.game_state_changed.disconnect(_on_game_state_changed)
	
	if MonetizationManager.instance:
		MonetizationManager.instance.purchase_succeeded.disconnect(_on_purchase_completed)
		MonetizationManager.instance.purchase_failed.disconnect(_on_purchase_failed)
	
	if _play_button_node != null:
		_play_button_node.pressed.disconnect(_on_play_button_pressed)
	
	if _quit_button_node != null:
		_quit_button_node.pressed.disconnect(_on_quit_button_pressed)

func setup_input_map():
	if not InputMap.has_action("ui_menu_select"):
		InputMap.add_action("ui_menu_select")
		var select_event := InputEventKey.new()
		select_event.keycode = Key.Enter
		InputMap.action_add_event("ui_menu_select", select_event)
	
	if not InputMap.has_action("ui_menu_back"):
		InputMap.add_action("ui_menu_back")
		var back_event := InputEventKey.new()
		back_event.keycode = Key.Escape
		InputMap.action_add_event("ui_menu_back", back_event)

func _input(event):
	if event.is_action_pressed("ui_menu_select"):
		_handle_menu_selection()
	elif event.is_action_pressed("ui_menu_back"):
		_handle_menu_back()

func _handle_menu_selection():
	var focused_control = get_viewport().gui_get_focus_owner()
	if focused_control is Button focused_button and not focused_button.disabled:
		focused_button.pressed.emit()

func _handle_menu_back():
	var settings_panel = get_node_or_null("SettingsPanel")
	if settings_panel != null and settings_panel.visible:
		settings_panel.visible = false

func _on_play_button_pressed():
	print("Play button pressed")
	play_button_pressed.emit()
	play_ui_click_sound()
	
	if GameManager.instance:
		GameManager.instance.start_room_by_level_number(1)

func _on_quit_button_pressed():
	print("Quit button pressed")
	quit_button_pressed.emit()
	play_ui_click_sound()
	
	get_tree().quit()

func _on_game_state_changed(state):
	match state:
		GameManager.GameState.MAIN_MENU:
			visible = true
			refresh_menu()
		_:
			visible = false

func _on_unlock_button_pressed():
	show_unlock_confirmation()

func show_unlock_confirmation():
	if _purchase_in_progress:
		return
	
	_unlock_confirmation?.popup_centered()

func _on_unlock_confirmation_accepted():
	if _purchase_in_progress:
		return
	
	on_purchase_started()
	
	if MonetizationManager.instance:
		await MonetizationManager.instance.purchase_full_game()
	else:
		on_purchase_failed("Monetization manager unavailable.")

func initialize_retention_systems():
	# Initialize retention systems if needed
	pass

func add_retention_system_buttons():
	# Add retention system buttons if needed
	pass

func play_ui_click_sound():
	if AudioManager.instance:
		AudioManager.instance.play_ui_click()

func refresh_menu():
	# Refresh menu state
	pass

func on_purchase_started():
	_purchase_in_progress = true

func _on_purchase_completed():
	_purchase_in_progress = false
	_purchase_complete_dialog.popup_centered()
	
	if _unlock_full_game_button:
		_unlock_full_game_button.queue_free()

func _on_purchase_failed(reason: String = ""):
	_purchase_in_progress = false
	_purchase_failed_dialog.popup_centered()

func _on_purchase_retry():
	# Retry purchase
	if MonetizationManager.instance:
		await MonetizationManager.instance.purchase_full_game()

func _on_purchase_failed_dialog_canceled():
	# User canceled purchase retry
	_purchase_in_progress = false