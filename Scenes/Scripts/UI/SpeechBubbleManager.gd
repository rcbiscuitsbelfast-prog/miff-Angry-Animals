extends Node

## Speech bubble manager that handles temporary speech bubbles in the game.

signal speech_bubble_created(text: String, duration: float)
signal speech_bubble_removed()

@export var _enable_speech_bubbles: bool = true
@export var _bubble_lifetime: float = 2.5
@export var _random_bubble_interval_min: float = 2.0
@export var _random_bubble_interval_max: float = 3.0
@export var _bubble_float_speed: float = 30.0
@export var _bubble_color: Color = Color(1, 1, 0.9, 0.95)
@export var bubble_border_color: Color = Color(0.2, 0.2, 0.2, 1)
@export var bubble_padding: int = 10
@export var bubble_corner_radius: int = 12
@export var bubble_font_size: int = 16

var _current_bubbles: Array = []
var _random_timer: Timer

func _ready():
    # Initialize speech bubble manager
    print("SpeechBubbleManager initialized")
    if _enable_speech_bubbles:
        _start_random_bubbles()

func _start_random_bubbles():
    # Start showing random speech bubbles
    if _random_timer:
        _random_timer.queue_free()
    
    _random_timer = Timer.new()
    _random_timer.one_shot = false
    _random_timer.timeout.connect(_show_random_bubble)
    add_child(_random_timer)
    _random_timer.start()

func _show_random_bubble():
    # Show a random speech bubble
    var random_messages = [
        "Hello there!",
        "This is fun!",
        "Nice shot!",
        "Can you do better?",
        "Watch this!",
        "Ready for more?",
        "Try harder!",
        "You're doing great!",
        "What a challenge!",
        "Almost there!"
    ]
    
    var random_message = random_messages[randi() % random_messages.size()]
    show_speech_bubble(random_message, _bubble_lifetime)

func show_speech_bubble(text: String, duration: float = 3.0, position: Vector2 = Vector2.ZERO):
    # Show a speech bubble with the given text
    if not _enable_speech_bubbles:
        return
    
    speech_bubble_created.emit(text, duration)
    
    # Create the speech bubble
    var bubble = create_speech_bubble(text, duration, position)
    add_child(bubble)
    _current_bubbles.append(bubble)

func create_speech_bubble(text: String, duration: float, position: Vector2) -> Control:
    # Create a speech bubble control
    var bubble_container = Control.new()
    bubble_container.name = "SpeechBubble"
    
    # Create the background
    var background = Panel.new()
    background.modulate = _bubble_color
    background.border_color = bubble_border_color
    background.border_width = Vector2(2, 2)
    background.corner_radius = bubble_corner_radius
    background.custom_minimum_size = Vector2(200, 60)
    background.position = position
    background.z_index = 1000  # Keep bubbles on top
    bubble_container.add_child(background)
    
    # Create the text label
    var text_label = Label.new()
    text_label.text = text
    text_label.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
    text_label.vertical_alignment = VERTICAL_ALIGNMENT_CENTER
    text_label.autowrap_mode = TextServer.AUTOWRAP_WORD_SMART
    text_label.size = Vector2(180, 40)
    text_label.position = Vector2(10, 10)
    text_label.add_theme_font_size_override("font_size", bubble_font_size)
    background.add_child(text_label)
    
    # Create a timer for the bubble lifetime
    var timer = Timer.new()
    timer.wait_time = duration
    timer.one_shot = true
    timer.timeout.connect(func(): _remove_speech_bubble(bubble_container))
    background.add_child(timer)
    timer.start()
    
    # Add float animation
    animate_bubble_float(bubble_container, duration)
    
    return bubble_container

func animate_bubble_float(bubble: Control, duration: float):
    # Add floating animation to the bubble
    var tween = create_tween()
    var start_pos = bubble.position
    var float_height = 20.0
    var float_time = 2.0
    
    # Float up and down
    tween.tween_property(bubble, "position:y", start_pos.y - float_height, float_time)
    tween.tween_property(bubble, "position:y", start_pos.y, float_time)
    tween.set_loops()  # Loop forever
    
    # Add fade out towards the end
    tween.tween_property(bubble, "modulate:a", 0.0, 0.5).set_delay(duration - 0.5)

func _remove_speech_bubble(bubble: Control):
    # Remove a speech bubble
    if bubble in _current_bubbles:
        _current_bubbles.erase(bubble)
    
    if bubble.get_parent():
        bubble.get_parent().remove_child(bubble)
        bubble.queue_free()
    
    speech_bubble_removed.emit()

func clear_all_bubbles():
    # Remove all current speech bubbles
    for bubble in _current_bubbles:
        _remove_speech_bubble(bubble)

func set_enabled(enabled: bool):
    # Enable or disable speech bubbles
    _enable_speech_bubbles = enabled
    
    if not enabled:
        clear_all_bubbles()
        if _random_timer:
            _random_timer.stop()
    else:
        _start_random_bubbles()

func show_character_speech(character_name: String, text: String, duration: float = 3.0):
    # Show speech from a specific character
    var formatted_text = "%s: %s" % [character_name, text]
    show_speech_bubble(formatted_text, duration)

func show_hint(hint_text: String, duration: float = 5.0):
    # Show a hint message
    show_speech_bubble("💡 " + hint_text, duration)

func show_encouragement(message: String, duration: float = 2.5):
    # Show an encouraging message
    show_speech_bubble("👍 " + message, duration)

func _exit_tree():
    # Clean up when the node is removed
    clear_all_bubbles()
    if _random_timer:
        _random_timer.queue_free()