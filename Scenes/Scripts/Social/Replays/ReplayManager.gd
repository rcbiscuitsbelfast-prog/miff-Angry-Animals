extends Node

## Manages gameplay replay recording, playback, and sharing

signal recording_started()
signal recording_stopped(replay: Dictionary)
signal playback_started(replay: Dictionary)
signal playback_stopped()
signal replay_shared(shareable: Dictionary)

static var instance: ReplayManager

const REPLAYS_DATA_PATH: String = "user://replays/"
const MAX_REPLAYS_PER_DEVICE: int = 20
const SNAPSHOT_INTERVAL: float = 0.1  # 10 snapshots per second

# Recording state
var _is_recording: bool = false
var _current_replay: Dictionary
var _recording_start_time: float
var _last_snapshot_time: float

# Playback state
var _is_playing: bool = false
var _playback_replay: Dictionary
var _current_input_index: int
var _current_snapshot_index: int
var _playback_start_time: float
var _playback_speed: float = 1.0

# Replay library
var _replays: Dictionary = {}  # replay_id -> ReplayData

func _ready():
	if instance:
		queue_free()
		return
	
	instance = self
	process_mode = Node.PROCESS_MODE_ALWAYS
	
	create_replays_directory()
	load_replays()
	
	print("Replay Manager initialized")

func _process(delta: float):
	if _is_recording:
		update_recording(delta)
	
	if _is_playing:
		update_playback(delta)

func _exit_tree():
	if _is_recording:
		stop_recording()
	
	if _is_playing:
		stop_playback()

## Start recording a replay
func start_recording(level_id: String, level_name: String) -> bool:
	if _is_recording:
		printerr("Cannot start recording: already recording")
		return false
	
	_current_replay = {
		"replay_id": generate_replay_id(),
		"player_id": get_current_player_id(),
		"player_name": get_current_player_name(),
		"level_id": level_id,
		"level_name": level_name,
		"recorded_date": Time.get_datetime_dict_from_system(),
		"player_cosmetics": get_current_player_cosmetics(),
		"starting_conditions": {
			"slingshot_type": 0,
			"projectile_type": 0
		},
		"inputs": [],
		"snapshots": [],
		"score": 0,
		"stars": 0,
		"completion_time": 0.0,
		"is_perfect": false,
		"file_size": 0
	}
	
	_is_recording = true
	_recording_start_time = Time.get_ticks_msec() / 1000.0
	_last_snapshot_time = _recording_start_time
	
	recording_started.emit()
	
	print("Started recording replay for %s" % level_name)
	return true

## Stop recording and save replay
func stop_recording(final_score: int = 0, stars: int = 0, completion_time: float = 0.0) -> Dictionary:
	if not _is_recording:
		printerr("Cannot stop recording: not recording")
		return {}
	
	_is_recording = false
	
	_current_replay["score"] = final_score
	_current_replay["stars"] = stars
	_current_replay["completion_time"] = completion_time
	_current_replay["is_perfect"] = stars >= 3
	
	# Calculate file size
	var json_string = JSON.stringify(_current_replay)
	_current_replay["file_size"] = json_string.length()
	
	# Save replay
	save_replay(_current_replay)
	
	recording_stopped.emit(_current_replay)
	
	print("Stopped recording replay: %s" % _current_replay["replay_id"])
	return _current_replay.duplicate()

## Update recording with current frame data
func update_recording(delta: float):
	var current_time = Time.get_ticks_msec() / 1000.0
	var elapsed = current_time - _recording_start_time
	
	# Check if we need a snapshot
	if current_time - _last_snapshot_time >= SNAPSHOT_INTERVAL:
		record_snapshot(current_time, elapsed)
		_last_snapshot_time = current_time

## Record input event
func record_input(input_type: String, input_data: Dictionary):
	if not _is_recording:
		return
	
	var current_time = Time.get_ticks_msec() / 1000.0
	var elapsed = current_time - _recording_start_time
	
	var input = {
		"time": elapsed,
		"type": input_type,
		"data": input_data
	}
	
	_current_replay["inputs"].append(input)

## Record game state snapshot
func record_snapshot(current_time: float, elapsed: float):
	if not _is_recording:
		return
	
	var snapshot = {
		"time": elapsed,
		"timestamp": current_time,
		"state": capture_game_state()
	}
	
	_current_replay["snapshots"].append(snapshot)

## Capture current game state
func capture_game_state() -> Dictionary:
	var state = {}
	
	# Get relevant game objects and their state
	var game_root = get_tree().current_scene
	if game_root:
		# Capture bird positions
		var birds = game_root.get_tree().get_nodes_in_group("birds")
		state["birds"] = []
		for bird in birds:
			state["birds"].append({
				"position": str(bird.global_position),
				"rotation": bird.global_rotation,
				"velocity": str(bird.linear_velocity) if bird.has_method("get_linear_velocity") else "0,0"
			})
		
		# Capture block states
		var blocks = game_root.get_tree().get_nodes_in_group("blocks")
		state["blocks"] = []
		for block in blocks:
			state["blocks"].append({
				"position": str(block.global_position),
				"rotation": block.global_rotation,
				"health": block.get("health", 0) if block.has_method("get") else 0
			})
	
	return state

## Start playback of a replay
func start_playback(replay_id: String) -> bool:
	if _is_playing:
		printerr("Cannot start playback: already playing")
		return false
	
	if not _replays.has(replay_id):
		printerr("Cannot start playback: replay %s not found" % replay_id)
		return false
	
	_playback_replay = _replays[replay_id].duplicate()
	_is_playing = true
	_current_input_index = 0
	_current_snapshot_index = 0
	_playback_start_time = Time.get_ticks_msec() / 1000.0
	_playback_speed = 1.0
	
	# Initialize game state from replay
	initialize_game_state(_playback_replay["starting_conditions"])
	
	playback_started.emit(_playback_replay)
	
	print("Started playback of replay: %s" % replay_id)
	return true

## Stop playback
func stop_playback():
	if not _is_playing:
		printerr("Cannot stop playback: not playing")
		return
	
	_is_playing = false
	
	playback_stopped.emit()
	
	print("Stopped playback of replay: %s" % _playback_replay["replay_id"])

## Update playback
func update_playback(delta: float):
	var current_time = Time.get_ticks_msec() / 1000.0
	var elapsed = (current_time - _playback_start_time) * _playback_speed
	
	# Process inputs that should have occurred by now
	while _current_input_index < _playback_replay["inputs"].size():
		var input = _playback_replay["inputs"][_current_input_index]
		if input["time"] <= elapsed:
			apply_input(input)
			_current_input_index += 1
		else:
			break
	
	# Apply snapshots for synchronization
	while _current_snapshot_index < _playback_replay["snapshots"].size():
		var snapshot = _playback_replay["snapshots"][_current_snapshot_index]
		if snapshot["time"] <= elapsed:
			apply_snapshot(snapshot["state"])
			_current_snapshot_index += 1
		else:
			break
	
	# Check if replay is complete
	if _current_input_index >= _playback_replay["inputs"].size() and \
	   _current_snapshot_index >= _playback_replay["snapshots"].size():
		stop_playback()

## Apply input from replay
func apply_input(input: Dictionary):
	var input_type = input["type"]
	var input_data = input["data"]
	
	# Dispatch input to relevant game systems
	match input_type:
		"slingshot_pull":
			apply_slingshot_pull(input_data)
		"slingshot_release":
			apply_slingshot_release(input_data)
		"special_ability":
			apply_special_ability(input_data)

## Apply snapshot state
func apply_snapshot(state: Dictionary):
	# Restore game state from snapshot
	# This would synchronize positions, velocities, etc.
	print("Applying snapshot at time %s" % state.get("time", 0))

## Initialize game state from replay starting conditions
func initialize_game_state(conditions: Dictionary):
	# Load the level with starting conditions
	var level_id = _playback_replay["level_id"]
	
	print("Initializing game state for level %s" % level_id)
	# Implementation would load level and set up starting state

## Set playback speed
func set_playback_speed(speed: float):
	if speed > 0:
		_playback_speed = speed

## Get current playback progress
func get_playback_progress() -> float:
	if not _is_playing or _playback_replay["inputs"].size() == 0:
		return 0.0
	
	return float(_current_input_index) / float(_playback_replay["inputs"].size())

## Get replay by ID
func get_replay(replay_id: String) -> Dictionary:
	if _replays.has(replay_id):
		return _replays[replay_id].duplicate()
	return {}

## Get all replays
func get_all_replays() -> Array:
	return _replays.values()

## Get replays for a level
func get_replays_for_level(level_id: String) -> Array:
	var level_replays = []
	for replay in _replays.values():
		if replay["level_id"] == level_id:
			level_replays.append(replay.duplicate())
	
	# Sort by date descending
	level_replays.sort_custom(func(a, b): return b["recorded_date"] > a["recorded_date"])
	
	return level_replays

## Delete a replay
func delete_replay(replay_id: String) -> bool:
	if not _replays.has(replay_id):
		printerr("Cannot delete replay: %s not found" % replay_id)
		return false
	
	# Delete file
	var file_path = REPLAYS_DATA_PATH + replay_id + ".json"
	if FileAccess.file_exists(file_path):
		DirAccess.remove_absolute(file_path)
	
	_replays.erase(replay_id)
	
	print("Deleted replay: %s" % replay_id)
	return true

## Share a replay (generate shareable code)
func share_replay(replay_id: String) -> Dictionary:
	if not _replays.has(replay_id):
		printerr("Cannot share replay: %s not found" % replay_id)
		return {}
	
	var replay = _replays[replay_id]
	
	# Create shareable replay (minimal data)
	var shareable = {
		"replay_id": replay_id,
		"player_name": replay["player_name"],
		"level_id": replay["level_id"],
		"level_name": replay["level_name"],
		"score": replay["score"],
		"stars": replay["stars"],
		"is_perfect": replay["is_perfect"],
		"completion_time": replay["completion_time"],
		"share_code": generate_share_code(replay_id),
		"share_url": generate_share_url(replay_id)
	}
	
	replay_shared.emit(shareable)
	
	print("Shared replay: %s" % replay_id)
	return shareable

## Generate share code for replay
func generate_share_code(replay_id: String) -> String:
	# Base64 encode the replay ID for sharing
	return Marshalls.base64_encode(replay_id.to_utf8_buffer())

## Generate share URL for replay
func generate_share_url(replay_id: String) -> String:
	var share_code = generate_share_code(replay_id)
	return "game://replay/" + share_code

## Import replay from share code
func import_replay_from_share(share_code: String) -> Dictionary:
	var replay_id = Marshalls.base64_to_utf8(share_code)
	
	if not _replays.has(replay_id):
		printerr("Cannot import replay: %s not found" % replay_id)
		return {}
	
	return _replays[replay_id].duplicate()

## Save replay to disk
func save_replay(replay: Dictionary):
	var replay_id = replay["replay_id"]
	var file_path = REPLAYS_DATA_PATH + replay_id + ".json"
	
	# Limit number of replays
	if _replays.size() >= MAX_REPLAYS_PER_DEVICE:
		# Remove oldest replay
		var oldest_replay_id = ""
		var oldest_time = Time.get_unix_time_from_datetime_dict(Time.get_datetime_dict_from_system())
		
		for id in _replays:
			var replay_time = Time.get_unix_time_from_datetime_dict(_replays[id]["recorded_date"])
			if replay_time < oldest_time:
				oldest_time = replay_time
				oldest_replay_id = id
		
		if oldest_replay_id != "":
			delete_replay(oldest_replay_id)
	
	# Save replay
	_replays[replay_id] = replay.duplicate()
	
	var file = FileAccess.open(file_path, FileAccess.WRITE)
	var json_string = JSON.stringify(replay)
	file.store_string(json_string)
	file.close()
	
	print("Saved replay: %s" % replay_id)

## Load replay from disk
func load_replay(replay_id: String) -> Dictionary:
	var file_path = REPLAYS_DATA_PATH + replay_id + ".json"
	
	if not FileAccess.file_exists(file_path):
		return {}
	
	var file = FileAccess.open(file_path, FileAccess.READ)
	var json_text = file.get_as_text()
	file.close()
	
	var json = JSON.new()
	var parse_result = json.parse(json_text)
	if parse_result != OK:
		print("Failed to parse replay: %s" % json.get_error_message())
		return {}
	
	return json.data

## Load all replays from disk
func load_replays():
	var dir = DirAccess.open(REPLAYS_DATA_PATH)
	if not dir:
		return
	
	dir.list_dir_begin()
	var file_name = dir.get_next()
	
	while file_name != "":
		if file_name.ends_with(".json"):
			var replay_id = file_name.replace(".json", "")
			var replay = load_replay(replay_id)
			if replay.size() > 0:
				_replays[replay_id] = replay
		
		file_name = dir.get_next()
	
	print("Loaded %d replays" % _replays.size())

## Create replays directory
func create_replays_directory():
	var dir = DirAccess.open("user://")
	if not dir.dir_exists("replays"):
		dir.make_dir("replays")

## Get current player ID
func get_current_player_id() -> String:
	if has_node("/root/PlayerProfile"):
		var profile = get_node("/root/PlayerProfile")
		if profile.has_method("get_player_name"):
			return profile.get_player_name()
	return "Player"

## Get current player name
func get_current_player_name() -> String:
	if has_node("/root/PlayerProfile"):
		var profile = get_node("/root/PlayerProfile")
		if profile.has_method("get_player_name"):
			return profile.get_player_name()
	return "Player"

## Get current player cosmetics
func get_current_player_cosmetics() -> Dictionary:
	var cosmetics = {
		"hat_index": 0,
		"glasses_index": 0,
		"moustache_index": 0,
		"wig_index": 0,
		"slingshot_skin_index": 0,
		"projectile_skin_index": 0
	}
	
	if has_node("/root/PlayerProfile"):
		var profile = get_node("/root/PlayerProfile")
		if profile.has_method("get_selected_hat_index"):
			cosmetics["hat_index"] = profile.get_selected_hat_index()
		if profile.has_method("get_selected_glasses_index"):
			cosmetics["glasses_index"] = profile.get_selected_glasses_index()
		if profile.has_method("get_selected_moustache_index"):
			cosmetics["moustache_index"] = profile.get_selected_moustache_index()
		if profile.has_method("get_selected_wig_index"):
			cosmetics["wig_index"] = profile.get_selected_wig_index()
		if profile.has_method("get_selected_slingshot_skin_index"):
			cosmetics["slingshot_skin_index"] = profile.get_selected_slingshot_skin_index()
		if profile.has_method("get_selected_projectile_skin_index"):
			cosmetics["projectile_skin_index"] = profile.get_selected_projectile_skin_index()
	
	return cosmetics

## Generate unique replay ID
func generate_replay_id() -> String:
	return "replay_" + str(Time.get_ticks_usec()) + "_" + str(randi())

## Apply slingshot pull input
func apply_slingshot_pull(data: Dictionary):
	# This would apply the slingshot pull to the game
	print("Applying slingshot pull: %s" % str(data))

## Apply slingshot release input
func apply_slingshot_release(data: Dictionary):
	# This would release the slingshot
	print("Applying slingshot release: %s" % str(data))

## Apply special ability input
func apply_special_ability(data: Dictionary):
	# This would trigger special abilities
	print("Applying special ability: %s" % str(data))

## Clear all replays
func clear_all_replays():
	for replay_id in _replays:
		var file_path = REPLAYS_DATA_PATH + replay_id + ".json"
		if FileAccess.file_exists(file_path):
			DirAccess.remove_absolute(file_path)
	
	_replays.clear()
	print("All replays cleared")
