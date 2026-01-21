extends Node
class_name GameManager

signal game_state_changed(state: int)
signal room_started(room_index: int)
signal room_completed(room_index: int)

enum GameState {
    BOOT,
    MAIN_MENU,
    CUTSCENE,
    IN_ROOM,
    ROOM_COMPLETE,
    PAUSED
}

class RoomInfo:
    var scene_path: String
    var description: String
    var optimal_score: int
    
    func _init(p_scene_path: String, p_description: String, p_optimal_score: int) -> void:
        scene_path = p_scene_path
        description = p_description
        optimal_score = p_optimal_score

const TOTAL_LEVELS: int = 100
const FREE_LEVELS: int = 20

var rooms: Array[RoomInfo] = []
var state: GameState = GameState.BOOT
var current_room_index: int = -1
var current_procedural_seed: int = 0

var main_scene_path: String = "res://Scenes/Main/Main.tscn"
var procedural_room_scene_path: String = "res://Scenes/Levels/ProceduralRoom.tscn"

var _signal_manager: Node

func _ready() -> void:
    process_mode = Node.PROCESS_MODE_ALWAYS
    rooms = _create_default_rooms()
    _deferred_init.call_deferred()

func _deferred_init() -> void:
    state = GameState.MAIN_MENU
    game_state_changed.emit(state)
    
    _signal_manager = get_node_or_null("/root/SignalManager")
    if _signal_manager:
        if _signal_manager.has_signal("on_level_completed"):
            _signal_manager.on_level_completed.connect(_on_level_completed)

func _on_level_completed() -> void:
    complete_room()

func load_main() -> void:
    current_room_index = -1
    current_procedural_seed = 0
    state = GameState.MAIN_MENU
    game_state_changed.emit(state)
    Globals.goto_scene(main_scene_path)

func start_room(room_index: int) -> void:
    _start_room_internal(room_index, true)

func start_room_by_level_number(level_number: int) -> void:
    start_room(level_number - 1)

func _start_room_internal_from_cutscene(room_index: int) -> void:
    _start_room_internal(room_index, false)

func _start_room_internal(room_index: int, allow_cutscenes: bool) -> void:
    if room_index < 0 or room_index >= rooms.size():
        push_warning("start_room: invalid room index %d" % room_index)
        return
    
    var monetization = get_node_or_null("/root/MonetizationManager")
    var full_unlocked: bool = monetization.is_full_game_unlocked if monetization else false
    
    if not full_unlocked and room_index >= FREE_LEVELS:
        push_warning("start_room: paywalled room %d. Unlock full game to play." % room_index)
        return
    
    var player_profile = get_node_or_null("/root/PlayerProfile")
    if not full_unlocked and player_profile and not player_profile.is_room_unlocked(room_index):
        push_warning("start_room: room locked %d" % room_index)
        return
    
    var story_event_trigger = get_node_or_null("/root/StoryEventTrigger")
    if allow_cutscenes and story_event_trigger and story_event_trigger.has_method("try_queue_chapter_start_cutscene") and story_event_trigger.try_queue_chapter_start_cutscene(room_index):
        current_room_index = room_index
        current_procedural_seed = 0
        state = GameState.CUTSCENE
        game_state_changed.emit(state)
        Globals.goto_scene("res://Scenes/Cutscenes/CutscenePlayer.tscn")
        return
    
    current_room_index = room_index
    state = GameState.IN_ROOM
    game_state_changed.emit(state)
    room_started.emit(room_index)
    
    var score_manager = get_node_or_null("/root/ScoreManager")
    if score_manager and score_manager.has_method("set_level"):
        score_manager.set_level(room_index + 1)
    
    var use_procedural_levels: bool = player_profile.use_procedural_levels if player_profile else false
    
    if use_procedural_levels:
        var room_number = room_index + 1
        var seed_val = 0
        
        if player_profile and player_profile.last_procedural_level_number == room_number:
            seed_val = player_profile.last_procedural_seed
        
        if seed_val == 0:
            var level_generator = get_node_or_null("/root/LevelGenerator")
            if level_generator and level_generator.has_method("calculate_seed"):
                seed_val = level_generator.calculate_seed(room_number)
        
        current_procedural_seed = seed_val
    else:
        current_procedural_seed = 0
    
    var scene_path = procedural_room_scene_path if use_procedural_levels else rooms[room_index].scene_path
    Globals.goto_scene(scene_path)

func restart_room() -> void:
    if current_room_index < 0:
        return
    _start_room_internal(current_room_index, false)

func complete_room() -> void:
    if current_room_index < 0:
        return
    
    state = GameState.ROOM_COMPLETE
    game_state_changed.emit(state)
    room_completed.emit(current_room_index)
    
    unlock_next_room()
    
    var story_data = get_node_or_null("/root/StoryData")
    if story_data and story_data.has_method("mark_room_completed"):
        story_data.mark_room_completed(current_room_index)
    
    show_room_complete()

func unlock_next_room() -> void:
    if current_room_index < 0:
        return
    
    var next = current_room_index + 1
    if next >= rooms.size():
        return
    
    var monetization = get_node_or_null("/root/MonetizationManager")
    var full_unlocked: bool = monetization.is_full_game_unlocked if monetization else false
    if not full_unlocked and next >= FREE_LEVELS:
        return
    
    var player_profile = get_node_or_null("/root/PlayerProfile")
    if player_profile and player_profile.has_method("unlock_room"):
        player_profile.unlock_room(next)

func show_room_complete() -> void:
    pass

func toggle_pause() -> void:
    if get_tree().paused:
        resume_game()
    else:
        pause_game()

func pause_game() -> void:
    get_tree().paused = true
    state = GameState.PAUSED
    game_state_changed.emit(state)

func resume_game() -> void:
    get_tree().paused = false
    state = GameState.IN_ROOM if current_room_index >= 0 else GameState.MAIN_MENU
    game_state_changed.emit(state)

func _create_default_rooms() -> Array[RoomInfo]:
    var rooms_arr: Array[RoomInfo] = []
    for i in range(TOTAL_LEVELS):
        var level_number = i + 1
        var optimal_score = 1000 + (i * 500)
        rooms_arr.append(RoomInfo.new(
            "res://Scenes/Levels/Room%03d.tscn" % level_number,
            "Room %d" % level_number,
            optimal_score
        ))
    return rooms_arr
