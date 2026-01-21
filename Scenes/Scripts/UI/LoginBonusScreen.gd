extends Control

## Login bonus screen that shows daily rewards and streak tracking.

signal dismissed()
signal view_events_pressed()

@export var main_container: NodePath
@export var reward_preview_container: NodePath
@export var streak_counter_label: NodePath
@export var reward_title_label: NodePath
@export var reward_description_label: NodePath
@export var next_reward_preview_label: NodePath
@export var streak_calendar_container: NodePath
@export var dismiss_button: NodePath
@export var view_events_button: NodePath
@export var celebration_effects: NodePath
@export var progress_fill: NodePath

var _main_container: VBoxContainer
var _reward_preview_container: VBoxContainer
var _streak_counter_label: Label
var _reward_title_label: Label
var _reward_description_label: Label
var _next_reward_preview_label: Label
var _streak_calendar_container: GridContainer
var _dismiss_button: Button
var _view_events_button: Button
var _celebration_effects: Control
var _progress_fill: ProgressBar

var _current_streak: int = 1
var _total_streak_days: int = 30

func _ready():
    initialize_bonus_screen()
    connect_signals()
    update_display()

func initialize_bonus_screen():
    _main_container = get_node_or_null(main_container) as VBoxContainer
    _reward_preview_container = get_node_or_null(reward_preview_container) as VBoxContainer
    _streak_counter_label = get_node_or_null(streak_counter_label) as Label
    _reward_title_label = get_node_or_null(reward_title_label) as Label
    _reward_description_label = get_node_or_null(reward_description_label) as Label
    _next_reward_preview_label = get_node_or_null(next_reward_preview_label) as Label
    _streak_calendar_container = get_node_or_null(streak_calendar_container) as GridContainer
    _dismiss_button = get_node_or_null(dismiss_button) as Button
    _view_events_button = get_node_or_null(view_events_button) as Button
    _celebration_effects = get_node_or_null(celebration_effects) as Control
    _progress_fill = get_node_or_null(progress_fill) as ProgressBar
    
    # Load current streak from player profile if available
    if PlayerProfile.instance:
        _current_streak = PlayerProfile.instance.login_streak
    
    # Connect button signals
    if _dismiss_button:
        _dismiss_button.pressed.connect(_on_dismiss_pressed)
    
    if _view_events_button:
        _view_events_button.pressed.connect(_on_view_events_pressed)

func connect_signals():
    # Connect to player profile signals if available
    if PlayerProfile.instance:
        # Connect to any relevant signals
        pass

func _exit_tree():
    # Disconnect signals
    if PlayerProfile.instance:
        pass
    
    # Disconnect button signals
    if _dismiss_button:
        _dismiss_button.pressed.disconnect(_on_dismiss_pressed)
    
    if _view_events_button:
        _view_events_button.pressed.disconnect(_on_view_events_pressed)

func update_display():
    # Update streak counter
    if _streak_counter_label:
        _streak_counter_label.text = "🔥 Day %d of %d!" % [_current_streak, _total_streak_days]
    
    # Update reward information
    if _reward_title_label:
        _reward_title_label.text = "Day %d Reward: %s" % [_current_streak, get_reward_title(_current_streak)]
    
    if _reward_description_label:
        _reward_description_label.text = get_reward_description(_current_streak)
    
    # Update progress bar
    if _progress_fill:
        var progress = (float(_current_streak) / float(_total_streak_days)) * 100.0
        _progress_fill.value = progress
    
    # Update next reward preview
    if _next_reward_preview_label:
        _next_reward_preview_label.text = "🎯 Next milestone (%d): %s" % [_current_streak + 1, get_next_reward_preview(_current_streak + 1)]
    
    # Update streak calendar
    update_streak_calendar()

func get_reward_title(day: int) -> String:
    # Return reward title based on day
    match day:
        1:
            return "Welcome Bonus!"
        7:
            return "Week 1 Complete!"
        14:
            return "Week 2 Complete!"
        21:
            return "Week 3 Complete!"
        30:
            return "Full Month Complete!"
        _:
            return "Daily Reward!"

func get_reward_description(day: int) -> String:
    # Return reward description based on day
    match day:
        1:
            return "Welcome to the game! Here's a starter reward to get you going!"
        7:
            return "Amazing! You've played for a whole week. Here's something special!"
        14:
            return "Two weeks of dedication! Your reward awaits!"
        21:
            return "Three weeks strong! Keep it up!"
        30:
            return "A full month! You're officially an Angry Animals pro!"
        _:
            return "Thanks for playing daily! Here's your reward!"

func get_next_reward_preview(day: int) -> String:
    # Return next reward preview
    match day:
        7:
            return "Week 1 Complete Hat + 200 coins"
        14:
            return "Week 2 Complete Glasses + 300 coins"
        21:
            return "Week 3 Complete Face + 400 coins"
        30:
            return "Legendary Hat + 500 coins"
        _:
            return "Daily coins + cosmetics"

func update_streak_calendar():
    # Update the streak calendar display
    if _streak_calendar_container:
        _streak_calendar_container.clear()
        
        # Create calendar items for the first 30 days
        for i in range(_total_streak_days):
            var day_label := Label.new()
            day_label.text = str(i + 1)
            day_label.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
            day_label.vertical_alignment = VERTICAL_ALIGNMENT_CENTER
            day_label.custom_minimum_size = Vector2(20, 20)
            
            if i + 1 <= _current_streak:
                # Completed day
                day_label.add_theme_color_override("font_color", Color.Green)
                day_label.text = "✓"  # Checkmark for completed days
            else:
                # Future day
                day_label.add_theme_color_override("font_color", Color.Gray)
            
            _streak_calendar_container.add_child(day_label)

func claim_reward():
    # Claim the current day's reward
    var reward = calculate_reward(_current_streak)
    
    # Apply reward to player
    if PlayerProfile.instance:
        PlayerProfile.instance.add_coins(reward.coins)
        PlayerProfile.instance.add_cosmetics(reward.cosmetics)
        PlayerProfile.instance.login_streak = _current_streak + 1
    
    # Show celebration effects
    show_celebration_effects()

func calculate_reward(day: int) -> Dictionary:
    # Calculate reward based on day
    var reward := {
        "coins": 50,
        "cosmetics": [],
        "special_items": []
    }
    
    match day:
        1:
            reward.coins = 100
            reward.cosmetics = ["starter_hat"]
        7:
            reward.coins = 200
            reward.cosmetics = ["week1_hat"]
        14:
            reward.coins = 300
            reward.cosmetics = ["week2_glasses"]
        21:
            reward.coins = 400
            reward.cosmetics = ["week3_face"]
        30:
            reward.coins = 500
            reward.cosmetics = ["legendary_hat"]
            reward.special_items = ["special_emote"]
    
    return reward

func show_celebration_effects():
    # Show celebration effects (confetti, etc.)
    if _celebration_effects:
        _celebration_effects.visible = true
        
        # Simple celebration - could be enhanced with particles
        var timer := Timer.new()
        timer.wait_time = 3.0
        timer.one_shot = true
        timer.timeout.connect(_hide_celebration_effects)
        add_child(timer)
        timer.start()

func _hide_celebration_effects():
    if _celebration_effects:
        _celebration_effects.visible = false

func _on_dismiss_pressed():
    print("Login bonus dismissed")
    dismissed.emit()
    claim_reward()
    hide()

func _on_view_events_pressed():
    print("View events button pressed")
    view_events_pressed.emit()
    # Show active events screen
    hide()

func show():
    # Show the login bonus screen
    visible = true
    update_display()

func hide():
    # Hide the login bonus screen
    visible = false