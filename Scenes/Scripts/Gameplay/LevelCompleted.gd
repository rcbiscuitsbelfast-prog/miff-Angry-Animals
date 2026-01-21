extends Control

## Handles the level completion screen and UI.
## Displays final score, new records, and provides options for next actions.

signal next_level_button_pressed()
signal restart_level_button_pressed()
signal room_selection_button_pressed()
signal main_menu_button_pressed()

@export var panel_path: NodePath
@export var title_label_path: NodePath
@export var _final_score_label_path: NodePath
@export var _best_score_label_path: NodePath
@export var _new_record_label_path: NodePath
@export var _next_level_button_path: NodePath
@export var _restart_level_button_path: NodePath
@export var _room_selection_button_path: NodePath
@export var _main_menu_button_path: NodePath
@export var _stars_container_path: NodePath

## Whether to automatically show interstitial ads after level completion.
var show_interstitial_on_level_complete: bool = true

## Minimum level number to show interstitial ads (avoid early levels).
var minimum_level_for_interstitial: int = 2

var _panel: Panel
var _title_label: Label
var _final_score_label: Label
var _best_score_label: Label
var _new_record_label: Label
var _next_level_button: Button
var _restart_level_button: Button
var _room_selection_button: Button
var _main_menu_button: Button
var _stars_container: HBoxContainer

var _current_level: int
var _final_score: int
var _best_score: int
var _is_new_record: bool

func _ready():
    initialize_completion_screen()
    connect_signals()
    setup_star_animation()

func initialize_completion_screen():
    _panel = get_node_or_null(panel_path) as Panel
    _title_label = get_node_or_null(title_label_path) as Label
    _final_score_label = get_node_or_null(_final_score_label_path) as Label
    _best_score_label = get_node_or_null(_best_score_label_path) as Label
    _new_record_label = get_node_or_null(_new_record_label_path) as Label
    _next_level_button = get_node_or_null(_next_level_button_path) as Button
    _restart_level_button = get_node_or_null(_restart_level_button_path) as Button
    _room_selection_button = get_node_or_null(_room_selection_button_path) as Button
    _main_menu_button = get_node_or_null(_main_menu_button_path) as Button
    _stars_container = get_node_or_null(_stars_container_path) as HBoxContainer
    
    # Initially hide the panel
    if _panel != null:
        _panel.visible = false
    
    # Set up button connections
    if _next_level_button != null:
        _next_level_button.pressed.connect(_on_next_level_button_pressed)
    
    if _restart_level_button != null:
        _restart_level_button.pressed.connect(_on_restart_level_button_pressed)
    
    if _room_selection_button != null:
        _room_selection_button.pressed.connect(_on_room_selection_button_pressed)
    
    if _main_menu_button != null:
        _main_menu_button.pressed.connect(_on_main_menu_button_pressed)
    
    # Hide new record label initially
    if _new_record_label != null:
        _new_record_label.visible = false

func connect_signals():
    # Connect to GameManager for room completion events
    if GameManager.instance:
        GameManager.instance.game_state_changed.connect(_on_game_state_changed)
        GameManager.instance.room_completed.connect(_on_room_completed)
    
    # Connect to ScoreManager for score data
    if ScoreManager.instance:
        ScoreManager.instance.score_changed.connect(_on_score_changed)
    
    # Connect to SignalManager for level completion events
    if SignalManager.instance:
        SignalManager.instance.on_level_completed.connect(_on_level_completed)

func _exit_tree():
    if GameManager.instance:
        GameManager.instance.game_state_changed.disconnect(_on_game_state_changed)
        GameManager.instance.room_completed.disconnect(_on_room_completed)
    
    if ScoreManager.instance:
        ScoreManager.instance.score_changed.disconnect(_on_score_changed)
    
    if SignalManager.instance:
        SignalManager.instance.on_level_completed.disconnect(_on_level_completed)
    
    # Disconnect button signals
    if _next_level_button != null:
        _next_level_button.pressed.disconnect(_on_next_level_button_pressed)
    
    if _restart_level_button != null:
        _restart_level_button.pressed.disconnect(_on_restart_level_button_pressed)
    
    if _room_selection_button != null:
        _room_selection_button.pressed.disconnect(_on_room_selection_button_pressed)
    
    if _main_menu_button != null:
        _main_menu_button.pressed.disconnect(_on_main_menu_button_pressed)

func setup_star_animation():
    # Set up timer for delayed star animation
    var timer := Timer.new()
    timer.wait_time = 0.5
    timer.one_shot = true
    timer.timeout.connect(animate_stars)
    add_child(timer)

func _on_room_completed(room_index: int):
    _current_level = room_index + 1 # Convert to 1-based level number
    
    # Try to get score from ScoreManager, fallback to 0
    _final_score = ScoreManager.get_score() if ScoreManager.instance else 0
    _best_score = ScoreManager.get_level_best_score(_current_level) if ScoreManager.instance else 0
    
    var star_count = calculate_star_count()
    var is_new_best = _final_score > _best_score or _best_score == 0
    _is_new_record = is_new_best
    
    if is_new_best:
        _best_score = _final_score
        if ScoreManager.instance:
            ScoreManager.set_level_score(_current_level, _final_score, star_count)
    
    # Check for cosmetic loot drop on perfect score
    try_award_cosmetic_loot(star_count)
    
    # Update the UI with completion data
    update_completion_ui()
    show_completion_panel()
    
    # Add game feel polish
    # GameFeelManager.instance?.on_level_complete(star_count)
    
    animate_stars()
    play_completion_sound()
    
    # Show interstitial ad after level completion (with conditions)
    show_interstitial_after_delay()

func show_interstitial_after_delay():
    # Wait a moment for the completion UI to show
    await get_tree().create_timer(2.0).timeout
    
    # Check if we should show interstitial
    if not show_interstitial_on_level_complete or _current_level < minimum_level_for_interstitial:
        return
    
    # Check monetization settings
    if MonetizationManager.instance and not MonetizationManager.instance.show_ads:
        return
    
    # Check if AdsManager is available
    if AdsManager.instance == null:
        return
    
    # Check if interstitial is ready and cooldown allows it
    if not AdsManager.instance.is_interstitial_ready():
        print("Interstitial not ready - preloading for next time")
        AdsManager.instance.load_interstitial_ad()
        return
    
    print("Showing interstitial after level %d completion" % _current_level)
    AdsManager.instance.show_interstitial_ad()

func _on_level_completed():
    # This is called when all projectiles are used up
    # Room completion is handled by GameManager.room_completed signal
    pass

func try_award_cosmetic_loot(star_count: int):
    # Try to award cosmetic loot if available
    var loot_table = CosmeticLootTable.instance
    if loot_table == null:
        return
    
    var drop_awarded = loot_table.try_award_cosmetic_drop(star_count, _final_score, _current_level)
    
    if drop_awarded:
        # Show special loot drop UI
        show_loot_drop_notification()

func show_loot_drop_notification():
    # TODO: Create actual UI notification
    print("🎁 You earned a new cosmetic!")
    
    # For now, just update the completion UI to show loot drop message
    if _new_record_label != null and not _is_new_record:
        _new_record_label.visible = true
        _new_record_label.text = "🎁 NEW COSMETIC EARNED! 🎁"
        _new_record_label.modulate = Color.Gold

func _on_score_changed(score: int):
    # Update final score if needed during completion screen
    if is_visible_in_tree():
        _final_score = score
        if _final_score_label != null:
            _final_score_label.text = "Final Score: %d" % score

func _on_game_state_changed(state):
    match state:
        GameManager.GameState.ROOM_COMPLETE:
            visible = true
        _:
            visible = false

func update_completion_ui():
    # Update title
    if _title_label != null:
        _title_label.text = "Room %d Complete!" % _current_level
    
    # Update final score
    if _final_score_label != null:
        _final_score_label.text = "Final Score: %d" % _final_score
    
    # Update best score
    if _best_score_label != null:
        _best_score_label.text = "Best Score: %d" % _best_score
    
    # Show/hide new record label
    if _new_record_label != null:
        _new_record_label.visible = _is_new_record
        if _is_new_record:
            _new_record_label.text = "🎉 NEW RECORD! 🎉"
            _new_record_label.modulate = Color.Gold
    
    # Update next level button availability
    update_next_level_button()

func update_next_level_button():
    if _next_level_button == null:
        return
    
    var current_room_index = _current_level - 1
    var has_next_room = false
    var is_next_room_unlocked = true
    
    if GameManager.instance and GameManager.instance.rooms.size() > 0:
        has_next_room = current_room_index + 1 < GameManager.instance.rooms.size()
        is_next_room_unlocked = PlayerProfile.is_room_unlocked(current_room_index + 1) if PlayerProfile.instance else true
    
    if has_next_room and is_next_room_unlocked:
        _next_level_button.text = "Next: Room %d" % (_current_level + 1)
        _next_level_button.disabled = false
    elif has_next_room:
        _next_level_button.text = "Next Room (Locked)"
        _next_level_button.disabled = true
        _next_level_button.tooltip_text = "Complete previous rooms to unlock"
    else:
        _next_level_button.text = "Game Complete!"
        _next_level_button.disabled = true

func show_completion_panel():
    if _panel != null:
        _panel.visible = true

func animate_stars():
    if _stars_container == null:
        return
    
    # Calculate star rating based on performance
    var star_count = calculate_star_count()
    
    # Animate stars one by one
    for i in _stars_container.get_child_count():
        var star = _stars_container.get_child(i)
        if star is Label star_label:
            if i < star_count:
                star_label.text = "⭐"
                star_label.scale = Vector2.ZERO
                
                # Animate star appearance
                var tween := create_tween()
                tween.tween_property(star_label, "scale", Vector2.ONE, 0.3).set_trans(Tween.TRANS_BOUNCE)
            else:
                star_label.text = "☆"
                star_label.modulate = Color.Gray

func calculate_star_count() -> int:
    # Simple star calculation for now
    if GameManager.instance and GameManager.instance.rooms.size() >= _current_level:
        var room_info = GameManager.instance.rooms[_current_level - 1]
        var optimal_score = room_info.optimal_score if "optimal_score" in room_info else 100
        
        # Use default thresholds
        var perfect_threshold = 0.9
        var good_threshold = 0.6
        
        if _final_score >= optimal_score * perfect_threshold:
            return 3
        elif _final_score >= optimal_score * good_threshold:
            return 2
    
    return 1

func play_completion_sound():
    # Play completion sound effect
    if AudioManager.instance:
        AudioManager.instance.play_combo_sound() # Reuse combo sound for now

# Button event handlers
func _on_next_level_button_pressed():
    print("Next level button pressed")
    next_level_button_pressed.emit()
    play_ui_click_sound()
    
    if GameManager.instance:
        var current_room_index = _current_level - 1
        if current_room_index + 1 < GameManager.instance.rooms.size():
            if MemeGateway.instance and MemeGateway.instance.try_play_minigame_then_load_next(_current_level):
                return
            
            GameManager.instance.start_room(current_room_index + 1)

func _on_restart_level_button_pressed():
    print("Restart level button pressed")
    restart_level_button_pressed.emit()
    play_ui_click_sound()
    
    if GameManager.instance:
        GameManager.instance.restart_room()

func _on_room_selection_button_pressed():
    print("Room selection button pressed")
    room_selection_button_pressed.emit()
    play_ui_click_sound()
    
    if GameManager.instance:
        GameManager.instance.load_main()

func _on_main_menu_button_pressed():
    print("Main menu button pressed")
    main_menu_button_pressed.emit()
    play_ui_click_sound()
    
    if GameManager.instance:
        GameManager.instance.load_main()

func play_ui_click_sound():
    if AudioManager.instance:
        AudioManager.instance.play_ui_click()

func show_completion(level_number: int, final_score: int, best_score: int):
    _current_level = level_number
    _final_score = final_score
    _best_score = best_score
    _is_new_record = final_score > best_score
    
    update_completion_ui()
    show_completion_panel()