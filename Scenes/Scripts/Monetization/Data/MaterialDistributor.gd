extends Node
class_name MaterialDistributor

## Singleton that determines material distribution for procedural levels based on room difficulty.
## Allows for fine-tuning of material progression and ensures balanced gameplay.

enum MaterialType {
	WOOD,
	STONE,
	BRICK,
	IRON,
	DIAMOND
}

var easy_mode_toughness_factor: float = 0.7
var medium_mode_toughness_factor: float = 1.0
var hard_mode_toughness_factor: float = 1.3

var enable_debug_logging: bool = true

var easy_distribution: Vector3 = Vector3(70, 20, 10)  # Wood, Stone, Brick
var medium_distribution: Vector4 = Vector4(30, 40, 20, 10)  # Wood, Stone, Brick, Iron
var hard_distribution: Vector4 = Vector4(20, 30, 40, 10)  # Stone, Brick, Iron, Diamond
var extreme_distribution: Vector3 = Vector3(10, 40, 50)  # Brick, Iron, Diamond

static var instance: MaterialDistributor = null

func _ready() -> void:
	instance = self

## Gets an array of materials for a specific room and number of obstacles.
static func get_materials_for_room(room_number: int, obstacle_count: int) -> Array[MaterialType]:
	# Use a deterministic seed derived from room number, but offset to avoid correlation with layout
	var level_generator = get_node_or_null("/root/LevelGenerator")
	var seed = room_number + 555
	
	var rng: RandomNumberGenerator = null
	if level_generator and level_generator.has_method("calculate_seed"):
		seed = level_generator.calculate_seed(room_number) + 555
	
	rng = RandomNumberGenerator.new()
	rng.seed = seed
	
	var materials: Array[MaterialType] = []
	var distribution = get_detailed_distribution(room_number)
	
	for i in range(obstacle_count):
		materials.append(_pick_random_material(distribution, rng))
	
	# Ensure at least one varied material per room if count > 1
	if obstacle_count > 1:
		var all_same = true
		var first_material = materials[0]
		for material in materials:
			if material != first_material:
				all_same = false
				break
		
		if all_same:
			var available_materials = []
			for i in range(distribution.materials.size()):
				if distribution.materials[i] != first_material:
					available_materials.append(distribution.materials[i])
			
			if available_materials.size() > 0:
				var random_index = rng.randi_range(0, available_materials.size() - 1)
				materials[rng.randi_range(0, materials.size() - 1)] = available_materials[random_index]
	
	return materials

## Returns a softness value from 0.0 (all hard) to 1.0 (all soft).
static func get_difficulty_softness(room_number: int) -> float:
	var dist = get_detailed_distribution(room_number)
	var weighted_sum = 0.0
	var total_weight = 0.0
	
	for i in range(dist.materials.size()):
		var hardness = int(dist.materials[i])
		weighted_sum += hardness * dist.weights[i]
		total_weight += dist.weights[i]
	
	if total_weight == 0:
		return 1.0
	
	var avg_hardness = weighted_sum / total_weight
	# Hardness ranges from 0 (Wood) to 4 (Diamond)
	# Softness 1.0 at hardness 0, 0.0 at hardness 4
	return clampf((4.0 - avg_hardness) / 4.0, 0.0, 1.0)

func get_detailed_distribution(room_number: int) -> Dictionary:
	# Fallback if instance is not yet initialized (e.g., in editor or during early init)
	var easy = instance.easy_distribution if instance else easy_distribution
	var medium = instance.medium_distribution if instance else medium_distribution
	var hard = instance.hard_distribution if instance else hard_distribution
	var extreme = instance.extreme_distribution if instance else extreme_distribution
	
	if room_number <= 20:
		return {
			"materials": [MaterialType.WOOD, MaterialType.STONE, MaterialType.BRICK],
			"weights": [easy.x, easy.y, easy.z]
		}
	elif room_number <= 40:
		return {
			"materials": [MaterialType.WOOD, MaterialType.STONE, MaterialType.BRICK, MaterialType.IRON],
			"weights": [medium.x, medium.y, medium.z, medium.w]
		}
	elif room_number <= 60:
		return {
			"materials": [MaterialType.STONE, MaterialType.BRICK, MaterialType.IRON, MaterialType.DIAMOND],
			"weights": [hard.x, hard.y, hard.z, hard.w]
		}
	else:
		return {
			"materials": [MaterialType.BRICK, MaterialType.IRON, MaterialType.DIAMOND],
			"weights": [extreme.x, extreme.y, extreme.z]
		}

## Pick a random material from the weighted distribution
func _pick_random_material(distribution: Dictionary, rng: RandomNumberGenerator) -> MaterialType:
	var materials = distribution.get("materials", [])
	var weights = distribution.get("weights", [])
	
	var total_weight = 0.0
	for weight in weights:
		total_weight += weight
	
	if total_weight <= 0:
		return distribution.materials[0] if materials.size() > 0 else MaterialType.WOOD
	
	var roll = rng.randf() * total_weight
	var current = 0.0
	
	for i in range(materials.size()):
		current += weights[i]
		if roll <= current:
			return distribution.materials[i]
	
	return distribution.materials[materials.size() - 1]  # Fallback
