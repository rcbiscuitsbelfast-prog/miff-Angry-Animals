extends Node
class_name LevelGenerator

enum MaterialType {
	WOOD = 1,
	STONE = 2,
	BRICK = 3,
	IRON = 4,
	DIAMOND = 5
}

enum ObstaclePattern { TOWER, WALL, SCATTERED }

class ThemeConfig:
	var background_color: Color
	var floor_color: Color
	var has_premium_effects: bool
	var theme_name: String
	
	func _init(p_bg: Color, p_floor: Color, p_premium: bool, p_name: String) -> void:
		background_color = p_bg
		floor_color = p_floor
		has_premium_effects = p_premium
		theme_name = p_name

class CupConfig:
	var position: Vector2
	var rotation: float
	var scale: float
	var is_premium: bool
	var material: MaterialType
	var pattern: ObstaclePattern
	var difficulty_coefficient: float
	
	func _init(p_pos: Vector2, p_rot: float, p_scale: float, p_premium: bool, p_mat: MaterialType, p_pat: ObstaclePattern, p_diff: float) -> void:
		position = p_pos
		rotation = p_rot
		scale = p_scale
		is_premium = p_premium
		material = p_mat
		pattern = p_pat
		difficulty_coefficient = p_diff

const SLINGSHOT_SAFE_X: float = 300.0
const EXIT_DOOR_SAFE_X: float = 900.0
const FLOOR_Y: float = 530.0

func _ready() -> void:
	process_mode = Node.PROCESS_MODE_ALWAYS

func calculate_seed(room_number: int) -> int:
	return create_seed_from_parameters(room_number)

func create_seed_from_parameters(room_number: int, custom_layout: int = -1, material_variant: int = -1) -> int:
	var layout = custom_layout if custom_layout != -1 else (room_number % 3)
	var variant = material_variant if material_variant != -1 else (room_number % 100)
	
	# Encode into 32-bit int
	var seed_val = (room_number & 0xFFFF) | ((layout & 0x3) << 16) | ((variant & 0x3FF) << 18)
	return seed_val

func try_decode_seed_to_parameters(seed: int) -> Dictionary:
	return {
		"room_number": seed & 0xFFFF,
		"layout": (seed >> 16) & 0x3,
		"variant": (seed >> 18) & 0x3FF
	}

func create_random_seed() -> int:
	return int(Time.get_ticks_msec()) ^ int(randi())

func get_theme_for_room(room_number: int) -> ThemeConfig:
	if room_number <= 30:
		return ThemeConfig.new(Color(0.3, 0.6, 0.9), Color(0.4, 0.5, 0.3), false, "Blue")
	if room_number <= 60:
		return ThemeConfig.new(Color(0.5, 0.3, 0.7), Color(0.4, 0.3, 0.5), true, "Purple")
	return ThemeConfig.new(Color(0.8, 0.4, 0.3), Color(0.5, 0.3, 0.2), true, "Red")

func get_interpolated_background_color(room_number: int) -> Color:
	var blue = Color(0.3, 0.6, 0.9)
	var purple = Color(0.5, 0.3, 0.7)
	var red = Color(0.8, 0.4, 0.3)
	
	if room_number <= 30: return blue
	if room_number <= 45: return blue.lerp(purple, (room_number - 30) / 15.0)
	if room_number <= 60: return purple
	if room_number <= 75: return purple.lerp(red, (room_number - 60) / 15.0)
	return red

func get_cup_count_for_room(room_number: int) -> int:
	var max_obstacles = 10 # Default fallback
	var difficulty_balancer = get_node_or_null("/root/DifficultyBalancer")
	if difficulty_balancer and difficulty_balancer.has_method("get_recommended_max_obstacles"):
		max_obstacles = difficulty_balancer.get_recommended_max_obstacles(room_number)
	
	var base_count = 3 + (room_number / 10)
	return clampi(base_count, 1, max_obstacles)

func generate_cups(room_number: int, target_cup_count: int, seed_val: int = -1) -> Array[CupConfig]:
	if seed_val == -1:
		seed_val = calculate_seed(room_number)
	
	var params = try_decode_seed_to_parameters(seed_val)
	var layout_index = params["layout"]
	var pattern = layout_index as ObstaclePattern
	
	var cup_configs: Array[CupConfig] = []
	# Simplified generation logic for GDScript version
	var random = RandomNumberGenerator.new()
	random.seed = seed_val
	
	var difficulty = 1.0 # Default
	var difficulty_balancer = get_node_or_null("/root/DifficultyBalancer")
	if difficulty_balancer and difficulty_balancer.has_method("calculate_room_difficulty"):
		difficulty = difficulty_balancer.calculate_room_difficulty(room_number).overall_score

	for i in range(target_cup_count):
		var pos = Vector2(
			random.randf_range(SLINGSHOT_SAFE_X + 100, EXIT_DOOR_SAFE_X - 100),
			random.randf_range(FLOOR_Y - 300, FLOOR_Y - 50)
		)
		var material = (i % 5) + 1 # Simple material distribution
		cup_configs.append(CupConfig.new(pos, random.randf_range(-0.2, 0.2), 1.0, room_number > 20, material as MaterialType, pattern, difficulty))
	
	return cup_configs

func is_position_safe(pos: Vector2) -> bool:
	if pos.x < SLINGSHOT_SAFE_X and pos.y > 450.0: return false
	if pos.x > EXIT_DOOR_SAFE_X - 100.0 and pos.y > 450.0: return false
	if pos.y > FLOOR_Y - 20.0: return false
	return true

# Helper aliases
func get_theme(room_number: int) -> ThemeConfig: return get_theme_for_room(room_number)
func get_background_color(room_number: int) -> Color: return get_interpolated_background_color(room_number)
func get_cup_count(room_number: int) -> int: return get_cup_count_for_room(room_number)
