extends Resource
class_name PlayerStats

@export var total_shots_fired: int = 0
@export var total_enemies_destroyed: int = 0
@export var total_cups_destroyed: int = 0
@export var games_played: int = 0
@export var time_played_seconds: float = 0.0

func to_dict() -> Dictionary:
	return {
		"total_shots_fired": total_shots_fired,
		"total_enemies_destroyed": total_enemies_destroyed,
		"total_cups_destroyed": total_cups_destroyed,
		"games_played": games_played,
		"time_played_seconds": time_played_seconds
	}

static func from_dict(d: Dictionary) -> PlayerStats:
	var ps = PlayerStats.new()
	ps.total_shots_fired = d.get("total_shots_fired", 0)
	ps.total_enemies_destroyed = d.get("total_enemies_destroyed", 0)
	ps.total_cups_destroyed = d.get("total_cups_destroyed", 0)
	ps.games_played = d.get("games_played", 0)
	ps.time_played_seconds = d.get("time_played_seconds", 0.0)
	return ps
