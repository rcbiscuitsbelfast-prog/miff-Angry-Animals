extends Node
class_name LeaderboardManager

class LeaderboardEntry:
	var player_name: String
	var score: int
	var date: String
	
	func _init(p_player_name: String = "", p_score: int = 0) -> void:
		player_name = p_player_name
		score = p_score
		date = Time.get_datetime_string_from_system()
	
	func to_dict() -> Dictionary:
		return {
			"PlayerName": player_name,
			"Score": score,
			"Date": date
		}
	
	static func from_dict(d: Dictionary) -> LeaderboardEntry:
		var entry = LeaderboardEntry.new(
			d.get("PlayerName", ""),
			int(d.get("Score", 0))
		)
		entry.date = d.get("Date", "")
		return entry

const LEADERBOARD_PATH = "user://leaderboard.json"

# Key is level number (String because JSON keys must be strings), value is list of top entries
var _leaderboards: Dictionary = {}

func _ready() -> void:
	load_data()

func add_entry(level_number: int, player_name: String, score: int) -> void:
	var level_key = str(level_number)
	if not _leaderboards.has(level_key):
		_leaderboards[level_key] = []
	
	_leaderboards[level_key].append(LeaderboardEntry.new(player_name, score))
	
	# Sort by score descending and keep top 10
	_leaderboards[level_key].sort_custom(func(a, b): return a.score > b.score)
	if _leaderboards[level_key].size() > 10:
		_leaderboards[level_key] = _leaderboards[level_key].slice(0, 10)
	
	save_data()

func get_top_entries(level_number: int) -> Array:
	var level_key = str(level_number)
	if _leaderboards.has(level_key):
		return _leaderboards[level_key]
	return []

func save_data() -> void:
	var file = FileAccess.open(LEADERBOARD_PATH, FileAccess.WRITE)
	if file:
		var data_to_save = {}
		for level_key in _leaderboards:
			var entries = []
			for entry in _leaderboards[level_key]:
				entries.append(entry.to_dict())
			data_to_save[level_key] = entries
		
		var json_str = JSON.stringify(data_to_save, "\t")
		file.store_string(json_str)

func load_data() -> void:
	if not FileAccess.file_exists(LEADERBOARD_PATH):
		return
	
	var file = FileAccess.open(LEADERBOARD_PATH, FileAccess.READ)
	if file:
		var json_str = file.get_as_text()
		if json_str.is_empty():
			return
		
		var json = JSON.new()
		var error = json.parse(json_str)
		if error == OK:
			if typeof(json.data) == TYPE_DICTIONARY:
				_leaderboards = {}
				for level_key in json.data:
					var entries = []
					for entry_dict in json.data[level_key]:
						entries.append(LeaderboardEntry.from_dict(entry_dict))
					_leaderboards[level_key] = entries
