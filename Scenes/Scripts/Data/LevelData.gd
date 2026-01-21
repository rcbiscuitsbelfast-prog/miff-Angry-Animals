extends Resource
class_name LevelData

@export var level_id: int = 0
@export var level_name: String = ""
@export var optimal_score: int = 1000
@export var unlocked: bool = false
@export var high_score: int = 0
@export var stars_earned: int = 0

func to_dict() -> Dictionary:
	return {
		"level_id": level_id,
		"level_name": level_name,
		"optimal_score": optimal_score,
		"unlocked": unlocked,
		"high_score": high_score,
		"stars_earned": stars_earned
	}

static func from_dict(d: Dictionary) -> LevelData:
	var ld = LevelData.new()
	ld.level_id = d.get("level_id", 0)
	ld.level_name = d.get("level_name", "")
	ld.optimal_score = d.get("optimal_score", 1000)
	ld.unlocked = d.get("unlocked", false)
	ld.high_score = d.get("high_score", 0)
	ld.stars_earned = d.get("stars_earned", 0)
	return ld
