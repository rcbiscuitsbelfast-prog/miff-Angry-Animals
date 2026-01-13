extends Control

# A/B Testing Dashboard UI - Real-time test monitoring and results
# Shows active tests, statistical significance, and winner detection

@onready var active_tests_label: Label = %ActiveTestsLabel
@onready var refresh_button: Button = %RefreshButton
@onready var export_button: Button = %ExportButton
@onready var test_list: VBoxContainer = %TestList
@onready var stats_label: Label = %StatsLabel
@onready var results_label: Label = %ResultsLabel

var ab_testing_manager: ABTestingManager

func _ready():
	# Only show in debug builds
	if not OS.has_feature("debug"):
		queue_free()
		return
		
	ab_testing_manager = ABTestingManager.Instance
	if ab_testing_manager == null:
		print("A/B Testing Manager not found!")
		return
	
	# Connect signals
	refresh_button.pressed.connect(_on_refresh_pressed)
	export_button.pressed.connect(_on_export_pressed)
	ab_testing_manager.test_variant_assigned.connect(_on_test_variant_assigned)
	ab_testing_manager.test_completed.connect(_on_test_completed)
	ab_testing_manager.conversion_tracked.connect(_on_conversion_tracked)
	
	# Update UI
	_update_ui()

func _on_refresh_pressed():
	_update_ui()

func _on_export_pressed():
	if ab_testing_manager:
		var csv_data = ab_testing_manager.ExportTestDataToCSV()
		_show_export_dialog(csv_data)

func _on_test_variant_assigned(test_id: String, variant_id: String, user_id: String):
	print("Test variant assigned: ", test_id, " -> ", variant_id)
	_update_ui()

func _on_test_completed(test_id: String, winning_variant: String, result):
	print("Test completed: ", test_id, " winner: ", winning_variant)
	_update_ui()

func _on_conversion_tracked(test_id: String, variant_id: String, conversion_type: String, value: float):
	print("Conversion tracked: ", test_id, "/", variant_id, " - ", conversion_type, ": ", value)
	_update_ui()

func _update_ui():
	if not ab_testing_manager:
		return
	
	var active_tests = ab_testing_manager.GetActiveTests()
	active_tests_label.text = "Active Tests (" + str(active_tests.size()) + ")"
	
	# Clear existing test entries
	for child in test_list.get_children():
		child.queue_free()
	
	# Add test entries
	for test in active_tests:
		var test_panel = _create_test_entry(test)
		test_list.add_child(test_panel)
	
	# Update statistics
	_update_statistics(active_tests)
	
	# Update results
	_update_results()

func _create_test_entry(test: ABTest) -> Control:
	var panel = PanelContainer.new()
	panel.custom_minimum_size = Vector2(0, 120)
	
	var vbox = VBoxContainer.new()
	vbox.theme_override_constants.separation = 8
	panel.add_child(vbox)
	
	# Test header
	var header = HBoxContainer.new()
	header.theme_override_constants.separation = 10
	vbox.add_child(header)
	
	var test_name = Label.new()
	test_name.text = test.TestName
	test_name.theme_override_font_sizes.font_size = 16
	test_name.add_theme_color_override("font_color", Color.WHITE)
	header.add_child(test_name)
	
	var status_label = Label.new()
	var days_remaining = max(0, int((test.EndDate - Time.get_datetime_dict_from_system()).day))
	status_label.text = "Days: " + str(days_remaining)
	status_label.theme_override_font_sizes.font_size = 12
	header.add_child(status_label)
	
	var winner_label = Label.new()
	var current_winner = _get_current_winner(test)
	if current_winner:
		winner_label.text = "Current: " + current_winner.VariantId
		winner_label.add_theme_color_override("font_color", Color.GREEN)
	else:
		winner_label.text = "No clear winner yet"
		winner_label.add_theme_color_override("font_color", Color.YELLOW)
	header.add_child(winner_label)
	
	# Variants data
	for variant in test.Variants:
		var variant_data = HBoxContainer.new()
		variant_data.theme_override_constants.separation = 20
		vbox.add_child(variant_data)
		
		var variant_label = Label.new()
		variant_label.text = variant.VariantId + ":"
		variant_label.custom_minimum_size = Vector2(100, 0)
		variant_data.add_child(variant_label)
		
		var users_label = Label.new()
		users_label.text = "Users: " + str(variant.UsersAssigned)
		users_label.custom_minimum_size = Vector2(80, 0)
		variant_data.add_child(users_label)
		
		var conversions_label = Label.new()
		conversions_label.text = "Conversions: " + str(variant.Conversions)
		conversions_label.custom_minimum_size = Vector2(120, 0)
		variant_data.add_child(conversions_label)
		
		var rate_label = Label.new()
		var conversion_rate = float(variant.Conversions) / max(1, variant.UsersAssigned)
		rate_label.text = "Rate: " + str(round(conversion_rate * 1000) / 10.0) + "%"
		rate_label.custom_minimum_size = Vector2(100, 0)
		if current_winner and variant.VariantId == current_winner.VariantId:
			rate_label.add_theme_color_override("font_color", Color.GREEN)
		variant_data.add_child(rate_label)
	
	return panel

func _get_current_winner(test: ABTest) -> ABTestVariant:
	if test.Variants.is_empty():
		return null
	
	return test.Variants.max_by(func(v): return float(v.Conversions) / max(1, v.UsersAssigned))

func _update_statistics(active_tests: Array):
	var total_tests = active_tests.size()
	var total_users = 0
	var total_conversions = 0
	
	for test in active_tests:
		for variant in test.Variants:
			total_users += variant.UsersAssigned
			total_conversions += variant.Conversions
	
	var overall_rate = float(total_conversions) / max(1, total_users)
	
	stats_label.text = """Statistics:
Total Active Tests: %d
Total Users in Tests: %d
Total Conversions: %d
Overall Conversion Rate: %.2f%%""" % [
		total_tests,
		total_users, 
		total_conversions,
		overall_rate * 100.0
	]

func _update_results():
	var results = ab_testing_manager.GetTestResultsForUI()
	var result_text = "Recent Test Results:\n\n"
	
	for result in results:
		result_text += "Test: %s\n" % result["test_name"]
		result_text += "  Description: %s\n" % result["description"]
		result_text += "  Days Remaining: %d\n" % result["days_remaining"]
		
		var best_variant = null
		var best_rate = 0.0
		
		for variant in result["variants"]:
			var rate = variant["conversion_rate"]
			if rate > best_rate:
				best_rate = rate
				best_variant = variant
		
		if best_variant:
			result_text += "  Current Leader: %s (%.2f%%)\n" % [
				best_variant["variant_id"], 
				best_rate * 100.0
			]
		
		result_text += "\n"
	
	results_label.text = result_text

func _show_export_dialog(csv_data: String):
	var dialog = AcceptDialog.new()
	dialog.title = "Export A/B Test Data"
	dialog.size = Vector2i(600, 400)
	
	var text_edit = TextEdit.new()
	text_edit.text = csv_data
	text_edit.size_flags_vertical = Control.SIZE_EXPAND_FILL
	text_edit.readonly = true
	
	var scroll = ScrollContainer.new()
	scroll.size_flags_vertical = Control.SIZE_EXPAND_FILL
	scroll.add_child(text_edit)
	
	dialog.add_child(scroll)
	add_child(dialog)
	dialog.popup_centered()

func _input(event):
	if event is InputEventKey and event.pressed:
		if event.keycode == KEY_ESCAPE:
			hide()
		elif event.keycode == KEY_F1:
			visible = not visible

func _process(_delta):
	# Auto-refresh every 5 seconds
	if OS.get_ticks_msec() % 5000 < 16:
		_update_ui()