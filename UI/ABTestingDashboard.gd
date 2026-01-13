extends Control

/// <summary>
/// A/B Testing Dashboard UI - shows active tests, real-time results, and statistical significance
/// Only visible in debug/dev builds with toggleable interface
/// </summary>

# A/B Testing data
var _abTestingManager
var _activeTests = []
var _testResults = []

# UI References
@onready var test_list_container = $VBoxContainer/TestsScrollContainer/TestList
@onready var active_tests_label = $VBoxContainer/Header/HBoxContainer/ActiveTestsLabel
@onready var refresh_button = $VBoxContainer/Header/HBoxContainer/RefreshButton
@onready var export_button = $VBoxContainer/Header/HBoxContainer/ExportButton
@onready var results_panel = $VBoxContainer/ResultsPanel
@onready var stats_label = $VBoxContainer/StatsPanel/StatsLabel

# Dashboard state
var _isVisible = false
var _autoRefresh = true
var _refreshInterval = 30.0 # seconds

func _ready():
    # Initialize dashboard
    _abTestingManager = ABTestingManager.Instance
    if _abTestingManager == null:
        queue_free()
        return
    
    # Connect signals
    _abTestingManager.connect("TestVariantAssigned", _on_test_variant_assigned)
    _abTestingManager.connect("TestCompleted", _on_test_completed)
    _abTestingManager.connect("ConversionTracked", _on_conversion_tracked)
    
    # Setup UI
    _setup_ui()
    _update_dashboard()
    
    # Start auto-refresh
    _start_auto_refresh()
    
    GD.Print("A/B Testing Dashboard initialized")

func _setup_ui():
    # Configure dashboard appearance
    visible = _should_show_dashboard()
    
    # Setup button connections
    refresh_button.pressed.connect(_on_refresh_pressed)
    export_button.pressed.connect(_on_export_pressed)
    
    # Style the dashboard
    _style_dashboard()

func _style_dashboard():
    # Set up colors for different test states
    # Green = winning variant
    # Yellow = neutral
    # Red = underperforming
    pass

func _should_show_dashboard() -> bool:
    # Only show in debug builds or when explicitly enabled
    return OS.IsDebugBuild() || ProjectSettings.GetSetting("debug/show_ab_testing_dashboard", false)

func _start_auto_refresh():
    # Start timer for auto-refresh
    var timer = Timer.new()
    timer.wait_time = _refreshInterval
    timer.timeout.connect(_update_dashboard)
    add_child(timer)
    timer.start()

func _update_dashboard():
    # Refresh test data
    _activeTests = _abTestingManager.GetActiveTests()
    _testResults = _abTestingManager.GetTestResultsForUI()
    
    # Update UI
    _update_test_list()
    _update_statistics()
    _update_results_panel()

func _update_test_list():
    # Clear existing test entries
    for child in test_list_container.get_children():
        child.queue_free()
    
    # Create test entry for each active test
    for test_data in _testResults:
        var test_entry = _create_test_entry(test_data)
        test_list_container.add_child(test_entry)
    
    # Update header
    active_tests_label.text = "Active Tests (%d)" % _activeTests.size()

func _create_test_entry(test_data) -> Control:
    var entry = HBoxContainer.new()
    entry.custom_minimum_size = Vector2(0, 60)
    entry.add_theme_constant_override("separation", 10)
    
    # Test name and description
    var test_info = VBoxContainer.new()
    var test_name = Label.new()
    test_name.text = test_data["test_name"]
    test_name.add_theme_font_size_override("font_size", 16)
    test_name.add_theme_color_override("font_color", Color.WHITE)
    
    var test_desc = Label.new()
    test_desc.text = test_data["description"]
    test_desc.add_theme_font_size_override("font_size", 12)
    test_desc.add_theme_color_override("font_color", Color.GRAY)
    
    test_info.add_child(test_name)
    test_info.add_child(test_desc)
    entry.add_child(test_info)
    
    # Variant progress bars
    var variants_container = VBoxContainer.new()
    variants_container.size_flags_horizontal = Control.SIZE_EXPAND_FILL
    
    for variant_data in test_data["variants"]:
        var variant_row = HBoxContainer.new()
        variant_row.size_flags_horizontal = Control.SIZE_EXPAND_FILL
        
        # Variant name
        var variant_label = Label.new()
        variant_label.text = variant_data["variant_id"]
        variant_label.custom_minimum_size = Vector2(80, 0)
        variant_label.add_theme_font_size_override("font_size", 12)
        variant_row.add_child(variant_label)
        
        # Progress bar
        var progress = ProgressBar.new()
        progress.size_flags_horizontal = Control.SIZE_EXPAND_FILL
        progress.value = variant_data["conversion_rate"] * 100
        progress.custom_minimum_size = Vector2(200, 0)
        
        # Color code based on performance
        var conversion_rate = variant_data["conversion_rate"]
        if conversion_rate > 0.15:  # High conversion
            progress.add_theme_color_override("font_color", Color.GREEN)
            progress.add_theme_color_override("theme_override_styles/fill", _create_progress_style(Color.GREEN))
        elif conversion_rate > 0.05:  # Medium conversion
            progress.add_theme_color_override("font_color", Color.YELLOW)
            progress.add_theme_color_override("theme_override_styles/fill", _create_progress_style(Color.YELLOW))
        else:  # Low conversion
            progress.add_theme_color_override("font_color", Color.RED)
            progress.add_theme_color_override("theme_override_styles/fill", _create_progress_style(Color.RED))
        
        variant_row.add_child(progress)
        
        # Stats labels
        var stats_label = Label.new()
        stats_label.text = "%d users | %.1f%%" % [variant_data["users_assigned"], conversion_rate * 100]
        stats_label.custom_minimum_size = Vector2(120, 0)
        stats_label.add_theme_font_size_override("font_size", 10)
        variant_row.add_child(stats_label)
        
        variants_container.add_child(variant_row)
    
    entry.add_child(variants_container)
    
    # Days remaining
    var days_remaining = test_data["days_remaining"]
    var days_label = Label.new()
    days_label.text = "%d days" % days_remaining
    days_label.add_theme_font_size_override("font_size", 14)
    if days_remaining < 3:
        days_label.add_theme_color_override("font_color", Color.RED)
    elif days_remaining < 7:
        days_label.add_theme_color_override("font_color", Color.YELLOW)
    else:
        days_label.add_theme_color_override("font_color", Color.GREEN)
    
    entry.add_child(days_label)
    
    return entry

func _create_progress_style(color: Color) -> StyleBoxFlat:
    var style = StyleBoxFlat.new()
    style.bg_color = color
    style.corner_radius_top_left = 2
    style.corner_radius_top_right = 2
    style.corner_radius_bottom_left = 2
    style.corner_radius_bottom_right = 2
    return style

func _update_statistics():
    # Calculate overall statistics
    var total_tests = _activeTests.size()
    var total_users = 0
    var total_conversions = 0
    
    for test in _activeTests:
        total_users += test.Variants.Sum(func(v): return v.UsersAssigned)
        total_conversions += test.Variants.Sum(func(v): return v.Conversions)
    
    var overall_conversion_rate = total_users > 0 ? float(total_conversions) / total_users : 0.0
    
    stats_label.text = """A/B Testing Statistics:
    
Tests Running: %d
Total Users: %d
Total Conversions: %d
Overall Conversion Rate: %.2f%%

Last Updated: %s""" % [
        total_tests,
        total_users,
        total_conversions,
        overall_conversion_rate * 100,
        Time.get_datetime_string_from_system()
    ]

func _update_results_panel():
    # Update results panel with detailed information
    # This could show historical completed tests, statistical significance, etc.
    pass

# Signal handlers
func _on_test_variant_assigned(test_id: String, variant_id: String, user_id: String):
    GD.Print("Test variant assigned: %s -> %s" % [test_id, variant_id])
    _update_dashboard()

func _on_test_completed(test_id: String, winning_variant: String, result):
    GD.Print("Test completed: %s - Winner: %s" % [test_id, winning_variant])
    _update_dashboard()

func _on_conversion_tracked(test_id: String, variant_id: String, conversion_type: String, value: float):
    # Update dashboard with new conversion data
    _update_dashboard()

# UI Event handlers
func _on_refresh_pressed():
    _update_dashboard()
    GD.Print("A/B Testing Dashboard refreshed")

func _on_export_pressed():
    var csv_data = _abTestingManager.ExportTestDataToCSV()
    var file_name = "ab_test_results_%s.csv" % Time.get_datetime_string_from_system().replace(":", "-")
    var file_path = "user://exports/%s" % file_name
    
    # Ensure exports directory exists
    var dir = DirAccess.open("user://exports")
    if dir == null:
        DirAccess.make_dir("user://exports")
        dir = DirAccess.open("user://exports")
    
    # Write CSV file
    var file = FileAccess.open(file_path, FileAccess.ModeFlags.Write)
    if file:
        file.store_string(csv_data)
        file.close()
        GD.Print("A/B test results exported to: %s" % file_path)
        
        # Show confirmation
        show_export_confirmation(file_name)
    else:
        GD.PrintErr("Failed to export A/B test results")

func show_export_confirmation(file_name: String):
    var dialog = AcceptDialog.new()
    dialog.title = "Export Complete"
    dialog.dialog_text = "A/B test results exported successfully!\n\nFile: %s" % file_name
    add_child(dialog)
    dialog.popup_centered()
    
    # Auto-close after 3 seconds
    var timer = Timer.new()
    timer.wait_time = 3.0
    timer.timeout.connect(func(): dialog.queue_free())
    add_child(timer)
    timer.start()

func toggle_visibility():
    _isVisible = !_isVisible
    visible = _isVisible and _should_show_dashboard()

func _input(event):
    if event.is_action_pressed("toggle_ab_dashboard"):
        toggle_visibility()

func _exit_tree():
    # Clean up resources
    pass