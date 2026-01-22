extends Control

## ProceduralDifficultyTest - Test interface for difficulty generation.
## Provides testing tools for procedural level generation and difficulty analysis.

signal test_refreshed()
signal test_generated()

@export var grid_container_path: NodePath
@export var refresh_button_path: NodePath

var _grid_container: GridContainer
var _refresh_button: Button

var _test_data: Array = []
var _current_difficulty_level: int = 1

func _ready():
    initialize_test_interface()

func initialize_test_interface():
    # Initialize the test interface
    _grid_container = get_node_or_null(grid_container_path) as GridContainer
    _refresh_button = get_node_or_null(refresh_button_path) as Button
    
    # Connect button signal
    if _refresh_button:
        _refresh_button.pressed.connect(_on_refresh_button_pressed)
    
    # Generate initial test data
    generate_test_data()
    
    print("ProceduralDifficultyTest initialized")

func generate_test_data():
    # Generate test data for procedural difficulty
    _test_data.clear()
    
    # Generate test levels with different difficulty metrics
    for i in range(20):
        var test_level = {
            "level": i + 1,
            "difficulty": calculate_difficulty(i + 1),
            "obstacles": randi_range(3, 15),
            "enemy_count": randi_range(1, 8),
            "complexity": randf_range(0.1, 1.0),
            "completion_rate": randf_range(0.3, 0.95),
            "avg_attempts": randf_range(1.2, 8.5),
            "score_requirement": randi_range(100, 2000)
        }
        _test_data.append(test_level)
    
    display_test_data()
    test_generated.emit()
    print("Generated %d test levels" % _test_data.size())

func calculate_difficulty(level_number: int) -> float:
    # Calculate difficulty based on level number and factors
    var base_difficulty = level_number * 0.1
    
    # Add some randomization
    var random_factor = randf_range(-0.05, 0.05)
    
    # Ensure difficulty is between 0 and 1
    return clamp(base_difficulty + random_factor, 0.0, 1.0)

func display_test_data():
    # Display the test data in the grid
    if _grid_container:
        _grid_container.clear()
        
        # Add header row
        add_header_row()
        
        # Add data rows
        for test_level in _test_data:
            add_test_row(test_level)

func add_header_row():
    # Add header row to grid
    var headers = ["Level", "Difficulty", "Obstacles", "Enemies", "Complexity", "Completion %", "Avg Attempts", "Score Req."]
    
    for header in headers:
        var label = Label.new()
        label.text = header
        label.add_theme_font_size_override("font_size", 14)
        label.add_theme_color_override("font_color", Color.Yellow)
        _grid_container.add_child(label)

func add_test_row(test_level: Dictionary):
    # Add a single test level row
    var values = [
        str(test_level["level"]),
        "%.2f" % test_level["difficulty"],
        str(test_level["obstacles"]),
        str(test_level["enemy_count"]),
        "%.2f" % test_level["complexity"],
        "%d%%" % int(test_level["completion_rate"] * 100),
        "%.1f" % test_level["avg_attempts"],
        str(test_level["score_requirement"])
    ]
    
    # Color code based on difficulty
    var difficulty = test_level["difficulty"]
    var row_color = Color.White
    if difficulty < 0.3:
        row_color = Color.Green  # Easy
    elif difficulty < 0.6:
        row_color = Color.Yellow  # Medium
    else:
        row_color = Color.Red  # Hard
    
    for i in range(values.size()):
        var label = Label.new()
        label.text = values[i]
        
        if i == 1:  # Difficulty column
            label.add_theme_color_override("font_color", row_color)
        
        _grid_container.add_child(label)

func analyze_difficulty_distribution():
    # Analyze the distribution of difficulty levels
    var easy_count = 0
    var medium_count = 0
    var hard_count = 0
    
    for test_level in _test_data:
        var difficulty = test_level["difficulty"]
        if difficulty < 0.3:
            easy_count += 1
        elif difficulty < 0.6:
            medium_count += 1
        else:
            hard_count += 1
    
    var total = _test_data.size()
    var easy_percent = (easy_count * 100.0) / total
    var medium_percent = (medium_count * 100.0) / total
    var hard_percent = (hard_count * 100.0) / total
    
    print("Difficulty Distribution:")
    print("  Easy (0-0.3): %d levels (%.1f%%)" % [easy_count, easy_percent])
    print("  Medium (0.3-0.6): %d levels (%.1f%%)" % [medium_count, medium_percent])
    print("  Hard (0.6-1.0): %d levels (%.1f%%)" % [hard_count, hard_percent])

func get_difficulty_metrics() -> Dictionary:
    # Get current difficulty metrics
    var total_difficulty = 0.0
    var total_obstacles = 0
    var total_enemies = 0
    
    for test_level in _test_data:
        total_difficulty += test_level["difficulty"]
        total_obstacles += test_level["obstacles"]
        total_enemies += test_level["enemy_count"]
    
    return {
        "avg_difficulty": total_difficulty / _test_data.size(),
        "total_levels": _test_data.size(),
        "total_obstacles": total_obstacles,
        "total_enemies": total_enemies,
        "avg_obstacles": total_obstacles / float(_test_data.size()),
        "avg_enemies": total_enemies / float(_test_data.size())
    }

func _on_refresh_button_pressed():
    # Refresh the test data
    print("Refreshing procedural difficulty test...")
    _current_difficulty_level += 1
    generate_test_data()
    analyze_difficulty_distribution()
    test_refreshed.emit()

func regenerate_with_seed(seed: int):
    # Regenerate with specific seed
    rand_seed(seed)
    print("Regenerating with seed: %d" % seed)
    generate_test_data()

func export_test_data():
    # Export test data to JSON (for analysis)
    var export_data = {
        "metadata": {
            "generated_at": Time.get_unix_time_from_system(),
            "total_levels": _test_data.size(),
            "metrics": get_difficulty_metrics()
        },
        "levels": _test_data
    }
    
    print("Test data ready for export:")
    print(JSON.stringify(export_data, "  "))
    
    return export_data

func set_difficulty_range(min_diff: float, max_diff: float):
    # Set difficulty range for generation
    for test_level in _test_data:
        var current_diff = test_level["difficulty"]
        var normalized = (current_diff - 0.1) / 0.9  # Normalize to 0-1
        test_level["difficulty"] = lerp(min_diff, max_diff, normalized)

func _exit_tree():
    # Clean up connections
    if _refresh_button:
        _refresh_button.pressed.disconnect(_on_refresh_button_pressed)