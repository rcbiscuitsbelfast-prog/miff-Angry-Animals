extends Resource
class_name SaveData

@export var player_name: String = "Player"
@export var high_score: int = 0
@export var unlocked_levels: int = 1
@export var total_score: int = 0
@export var coins: int = 0

func to_dict() -> Dictionary:
	return {
		"player_name": player_name,
		"high_score": high_score,
		"unlocked_levels": unlocked_levels,
		"total_score": total_score,
		"coins": coins
	}

static func from_dict(d: Dictionary) -> SaveData:
	var sd = SaveData.new()
	sd.player_name = d.get("player_name", "Player")
	sd.high_score = d.get("high_score", 0)
	sd.unlocked_levels = d.get("unlocked_levels", 1)
	sd.total_score = d.get("total_score", 0)
	sd.coins = d.get("coins", 0)
	return sd
