extends Node
class_name PlayerProfile

const PROFILE_PATH = "user://profile.json"

var player_name: String = "Player"
var is_full_game_unlocked: bool = false
var use_procedural_levels: bool = false
var last_procedural_seed: int = 0
var last_procedural_level_number: int = 1

var selected_hat_index: int = 0
var selected_glasses_index: int = 0
var selected_moustache_index: int = 0
var selected_wig_index: int = 0
var selected_filter_index: int = 0
var selected_emotion_index: int = 0

var selected_slingshot_skin_index: int = 0
var selected_projectile_skin_index: int = 0
var selected_trail_effect_index: int = 0
var selected_hit_effect_index: int = 0
var selected_victory_effect_index: int = 0
var selected_slingshot_type: int = 0

var unlocked_cosmetics: Array[String] = []

var colorblind_mode: bool = false
var text_scale: float = 1.0
var high_contrast_mode: bool = false
var reduce_motion: bool = false
var difficulty_preset: String = "Normal"

var face_image_path: String = ""
var highest_unlocked_room_index: int = 0
var highest_unlocked_chapter_index: int = 0
var completed_chapters: Array[int] = []
var story_flags_seen: Array[String] = []

var current_rage: float = 0.0
var current_combo: int = 0

func _ready() -> void:
    process_mode = Node.PROCESS_MODE_ALWAYS
    load_profile()
    _connect_rage_system.call_deferred()

func _exit_tree() -> void:
    save_profile()

func _connect_rage_system() -> void:
    var rage_system = get_node_or_null("/root/RageSystem")
    if rage_system:
        if rage_system.has_signal("rage_changed"): rage_system.rage_changed.connect(func(v): current_rage = v)
        if rage_system.has_signal("combo_changed"): rage_system.combo_changed.connect(func(v): current_combo = v)

func get_hats() -> Array: return ["none", "cap", "crown", "beanie", "tophat", "cowboy", "beret"]
func get_glasses() -> Array: return ["none", "round", "aviator", "sunglasses", "nerd_glasses", "monocle", "3d_glasses"]
func get_emotions() -> Array: return ["neutral", "happy", "angry", "sad"]

func set_cosmetics(hat: int, glasses: int, moustache: int, wig: int, filter: int, emotion: int) -> void:
    selected_hat_index = hat
    selected_glasses_index = glasses
    selected_moustache_index = moustache
    selected_wig_index = wig
    selected_filter_index = filter
    selected_emotion_index = emotion
    save_profile()

func set_player_name(p_name: String) -> void:
    player_name = p_name.strip_edges() if not p_name.strip_edges().is_empty() else "Player"
    save_profile()

func unlock_room(room_index: int) -> void:
    if room_index <= highest_unlocked_room_index: return
    highest_unlocked_room_index = room_index
    save_profile()

func is_room_unlocked(room_index: int) -> bool:
    return room_index <= highest_unlocked_room_index

func save_profile() -> void:
    var data = {
        "version": 3,
        "profile_name": player_name,
        "is_full_game_unlocked": is_full_game_unlocked,
        "use_procedural_levels": use_procedural_levels,
        "last_procedural_seed": last_procedural_seed,
        "last_procedural_level_number": last_procedural_level_number,
        "face_image_path": face_image_path,
        "highest_unlocked_room_index": highest_unlocked_room_index,
        "story": {
            "highest_unlocked_chapter_index": highest_unlocked_chapter_index,
            "completed_chapters": completed_chapters,
            "seen_flags": story_flags_seen
        },
        "accessibility": {
            "colorblind_mode": colorblind_mode,
            "text_scale": text_scale,
            "high_contrast_mode": high_contrast_mode,
            "reduce_motion": reduce_motion,
            "difficulty_preset": difficulty_preset
        },
        "cosmetics": {
            "hat_index": selected_hat_index,
            "glasses_index": selected_glasses_index,
            "moustache_index": selected_moustache_index,
            "wig_index": selected_wig_index,
            "filter_index": selected_filter_index,
            "emotion_index": selected_emotion_index,
            "slingshot_skin_index": selected_slingshot_skin_index,
            "projectile_skin_index": selected_projectile_skin_index,
            "trail_effect_index": selected_trail_effect_index,
            "hit_effect_index": selected_hit_effect_index,
            "victory_effect_index": selected_victory_effect_index,
            "slingshot_type": selected_slingshot_type,
            "unlocked_list": unlocked_cosmetics
        },
        "last_session_date": Time.get_datetime_string_from_system(true)
    }
    
    var file = FileAccess.open(PROFILE_PATH, FileAccess.WRITE)
    if file:
        file.store_string(JSON.stringify(data, "\t"))

func load_profile() -> void:
    if not FileAccess.file_exists(PROFILE_PATH):
        return
    
    var file = FileAccess.open(PROFILE_PATH, FileAccess.READ)
    if file:
        var json_string = file.get_as_text()
        var json = JSON.new()
        var error = json.parse(json_string)
        if error == OK:
            var data = json.data
            if typeof(data) == TYPE_DICTIONARY:
                player_name = data.get("profile_name", "Player")
                is_full_game_unlocked = data.get("is_full_game_unlocked", false)
                use_procedural_levels = data.get("use_procedural_levels", false)
                last_procedural_seed = int(data.get("last_procedural_seed", 0))
                last_procedural_level_number = int(data.get("last_procedural_level_number", 1))
                face_image_path = data.get("face_image_path", "")
                highest_unlocked_room_index = int(data.get("highest_unlocked_room_index", 0))
                
                var story = data.get("story", {})
                highest_unlocked_chapter_index = int(story.get("highest_unlocked_chapter_index", 0))
                completed_chapters = Array(story.get("completed_chapters", []))
                story_flags_seen = Array(story.get("seen_flags", []))
                
                var accessibility = data.get("accessibility", {})
                colorblind_mode = accessibility.get("colorblind_mode", false)
                text_scale = float(accessibility.get("text_scale", 1.0))
                high_contrast_mode = accessibility.get("high_contrast_mode", false)
                reduce_motion = accessibility.get("reduce_motion", false)
                difficulty_preset = accessibility.get("difficulty_preset", "Normal")
                
                var cosmetics = data.get("cosmetics", {})
                selected_hat_index = int(cosmetics.get("hat_index", 0))
                selected_glasses_index = int(cosmetics.get("glasses_index", 0))
                selected_moustache_index = int(cosmetics.get("moustache_index", 0))
                selected_wig_index = int(cosmetics.get("wig_index", 0))
                selected_filter_index = int(cosmetics.get("filter_index", 0))
                selected_emotion_index = int(cosmetics.get("emotion_index", 0))
                selected_slingshot_skin_index = int(cosmetics.get("slingshot_skin_index", 0))
                selected_projectile_skin_index = int(cosmetics.get("projectile_skin_index", 0))
                selected_trail_effect_index = int(cosmetics.get("trail_effect_index", 0))
                selected_hit_effect_index = int(cosmetics.get("hit_effect_index", 0))
                selected_victory_effect_index = int(cosmetics.get("victory_effect_index", 0))
                selected_slingshot_type = int(cosmetics.get("slingshot_type", 0))
                unlocked_cosmetics = Array(cosmetics.get("unlocked_list", []))

func get_slingshot_type() -> int:
    return selected_slingshot_type

func set_slingshot_type(type_index: int) -> void:
    selected_slingshot_type = clamp(type_index, 0, 3)
    save_profile()

func set_procedural_mode(enabled: bool) -> void:
    use_procedural_levels = enabled
    save_profile()

func unlock_cosmetic(cosmetic_id: String) -> void:
    if not cosmetic_id in unlocked_cosmetics:
        unlocked_cosmetics.append(cosmetic_id)
        save_profile()
