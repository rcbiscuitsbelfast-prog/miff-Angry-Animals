extends Node
class_name LevelScore

var level_number: int
var best_score: int
var star_rating: int
var date_set: String # Using String for DateTime for simplicity in JSON

func _init(p_level_number: int = 0, p_best_score: int = 0, p_star_rating: int = 0) -> void:
	level_number = p_level_number
	best_score = p_best_score
	star_rating = p_star_rating
	date_set = Time.get_datetime_string_from_system()

func to_dict() -> Dictionary:
	return {
		"LevelNumber": level_number,
		"BestScore": best_score,
		"StarRating": star_rating,
		"DateSet": date_set
	}

static func from_dict(d: Dictionary) -> LevelScore:
	var score = LevelScore.new(
		int(d.get("LevelNumber", 0)),
		int(d.get("BestScore", 0)),
		int(d.get("StarRating", 0))
	)
	score.date_set = d.get("DateSet", "")
	return score
