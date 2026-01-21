extends Node
class_name CosmeticLootTable

## Manages cosmetic loot drops for perfect score achievements.
## Provides a weighted random drop system for hats, glasses, emotions, and other cosmetics.
## Designed to encourage replayability through organic unlock progression.

signal cosmetic_earned(cosmetic_id: String, cosmetic_type: String)

var loot_drops_enabled: bool = true
var base_drop_chance: float = 1.0
var duplicate_drop_multiplier: float = 0.3
var perfect_score_bonus_chance: float = 0.2
var dry_spell_bonus_chance: float = 0.15

# Hat loot table
var cap_drop_weight: float = 1.0
var crown_drop_weight: float = 0.8
var beanie_drop_weight: float = 1.2
var top_hat_drop_weight: float = 0.6
var cowboy_hat_drop_weight: float = 0.7
var beret_drop_weight: float = 0.9

# Glasses loot table
var round_glasses_drop_weight: float = 1.0
var aviator_glasses_drop_weight: float = 0.9
var sunglasses_drop_weight: float = 1.1
var nerd_glasses_drop_weight: float = 0.8
var monocle_drop_weight: float = 0.6
var three_d_glasses_drop_weight: float = 0.7

# Emotion loot table
var happy_emotion_drop_weight: float = 1.0
var angry_emotion_drop_weight: float = 0.9
var sad_emotion_drop_weight: float = 0.8
var excited_emotion_drop_weight: float = 1.1
var surprised_emotion_drop_weight: float = 0.7

# Special cosmetics
var moustache_drop_weight: float = 0.5
var wig_drop_weight: float = 0.4
var slingshot_skin_drop_weight: float = 0.3
var projectile_skin_drop_weight: float = 0.3
var trail_effect_drop_weight: float = 0.2
var hit_effect_drop_weight: float = 0.2
var victory_effect_drop_weight: float = 0.1

# Drop history tracking
var _perfect_scores_since_last_drop: int = 0
var _total_perfect_scores: int = 0
var _last_drop_time: int = 0

static var instance: CosmeticLootTable = null

const DROP_HISTORY_PATH = "user://cosmetic_drop_history.json"

func _ready() -> void:
	instance = self
	process_mode = Node.PROCESS_MODE_ALWAYS
	_load_drop_history()

func _exit_tree() -> void:
	_save_drop_history()

## Attempts to award a cosmetic drop based on current performance.
## Called automatically when perfect score is achieved.
func try_award_cosmetic_drop(star_count: int, score: int, level_number: int) -> bool:
	if not loot_drops_enabled:
		return false
	
	# Only award drops on perfect scores (3 stars)
	if star_count < 3:
		return false
	
	_total_perfect_scores += 1
	_perfect_scores_since_last_drop += 1
	
	# Calculate drop chance with bonuses
	var drop_chance = _calculate_drop_chance()
	
	# Random roll
	var rng = RandomNumberGenerator.new()
	rng.randomize()
	var drop_occurred = rng.randf() < drop_chance
	
	if drop_occurred:
		var cosmetic_id = _roll_random_cosmetic(rng)
		if not cosmetic_id.is_empty():
			_award_cosmetic(cosmetic_id, level_number)
			_perfect_scores_since_last_drop = 0
			_last_drop_time = Time.get_ticks_msec()
			return true
	
	return false

## Calculates the current drop chance with all bonuses applied.
func _calculate_drop_chance() -> float:
	var chance = base_drop_chance
	
	# Perfect score bonus
	chance += perfect_score_bonus_chance
	
	# Dry spell bonus (if no drops for multiple perfect scores)
	if _perfect_scores_since_last_drop >= 3:
		chance += dry_spell_bonus_chance * (_perfect_scores_since_last_drop - 2)
	
	# Cap at 100%
	return clampf(chance, 0.0, 1.0)

## Rolls a random cosmetic from the weighted loot table.
func _roll_random_cosmetic(rng: RandomNumberGenerator) -> String:
	var all_cosmetics = _build_weighted_cosmetic_list()
	if all_cosmetics.is_empty():
		return ""
	
	var total_weight = 0.0
	for cosmetic in all_cosmetics:
		total_weight += cosmetic.weight
	
	var roll = rng.randf() * total_weight
	var current_weight = 0.0
	
	for cosmetic in all_cosmetics:
		current_weight += cosmetic.weight
		if roll < current_weight:
			return cosmetic.cosmetic_id
	
	return all_cosmetics[-1].cosmetic_id  # Fallback

## Builds a weighted list of all available cosmetics.
func _build_weighted_cosmetic_list() -> Array:
	var cosmetics = []
	
	# Hats
	cosmetics.append({"cosmetic_id": "cap", "type": "hat", "weight": cap_drop_weight})
	cosmetics.append({"cosmetic_id": "crown", "type": "hat", "weight": crown_drop_weight})
	cosmetics.append({"cosmetic_id": "beanie", "type": "hat", "weight": beanie_drop_weight})
	cosmetics.append({"cosmetic_id": "tophat", "type": "hat", "weight": top_hat_drop_weight})
	cosmetics.append({"cosmetic_id": "cowboy", "type": "hat", "weight": cowboy_hat_drop_weight})
	cosmetics.append({"cosmetic_id": "beret", "type": "hat", "weight": beret_drop_weight})
	
	# Glasses
	cosmetics.append({"cosmetic_id": "round", "type": "glasses", "weight": round_glasses_drop_weight})
	cosmetics.append({"cosmetic_id": "aviator", "type": "glasses", "weight": aviator_glasses_drop_weight})
	cosmetics.append({"cosmetic_id": "sunglasses", "type": "glasses", "weight": sunglasses_drop_weight})
	cosmetics.append({"cosmetic_id": "nerd_glasses", "type": "glasses", "weight": nerd_glasses_drop_weight})
	cosmetics.append({"cosmetic_id": "monocle", "type": "glasses", "weight": monocle_drop_weight})
	cosmetics.append({"cosmetic_id": "3d_glasses", "type": "glasses", "weight": three_d_glasses_drop_weight})
	
	# Emotions
	cosmetics.append({"cosmetic_id": "happy", "type": "emotion", "weight": happy_emotion_drop_weight})
	cosmetics.append({"cosmetic_id": "angry", "type": "emotion", "weight": angry_emotion_drop_weight})
	cosmetics.append({"cosmetic_id": "sad", "type": "emotion", "weight": sad_emotion_drop_weight})
	cosmetics.append({"cosmetic_id": "excited", "type": "emotion", "weight": excited_emotion_drop_weight})
	cosmetics.append({"cosmetic_id": "surprised", "type": "emotion", "weight": surprised_emotion_drop_weight})
	
	# Special cosmetics (lower drop rates)
	cosmetics.append({"cosmetic_id": "normal", "type": "moustache", "weight": moustache_drop_weight})
	cosmetics.append({"cosmetic_id": "afro", "type": "wig", "weight": wig_drop_weight})
	cosmetics.append({"cosmetic_id": "golden_slingshot", "type": "slingshot_skin", "weight": slingshot_skin_drop_weight})
	cosmetics.append({"cosmetic_id": "rainbow_projectile", "type": "projectile_skin", "weight": projectile_skin_drop_weight})
	cosmetics.append({"cosmetic_id": "sparkle_trail", "type": "trail_effect", "weight": trail_effect_drop_weight})
	cosmetics.append({"cosmetic_id": "explosion_hit", "type": "hit_effect", "weight": hit_effect_drop_weight})
	cosmetics.append({"cosmetic_id": "victory_fireworks", "type": "victory_effect", "weight": victory_effect_drop_weight})
	
	# Filter out cosmetics player already owns (unless dry spell bonus applies)
	if _perfect_scores_since_last_drop < 5:
		var player_profile = get_node_or_null("/root/PlayerProfile")
		if player_profile:
			cosmetics = cosmetics.filter(func(c): return not player_profile.unlocked_cosmetics.has(c.cosmetic_id))
	
	return cosmetics

## Awards a cosmetic to player and shows celebration UI.
func _award_cosmetic(cosmetic_id: String, level_number: int) -> void:
	var player_profile = get_node_or_null("/root/PlayerProfile")
	if player_profile and player_profile.has_method("unlock_cosmetic"):
		player_profile.unlock_cosmetic(cosmetic_id)
	
	_last_drop_time = Time.get_ticks_msec()
	
	print("🎁 COSMETIC EARNED! Level %d: %s" % [level_number, cosmetic_id])
	
	# Emit signal for UI to show celebration
	cosmetic_earned.emit(cosmetic_id, _get_cosmetic_type(cosmetic_id))
	
	# Play celebration sound
	var audio_manager = get_node_or_null("/root/AudioManager")
	if audio_manager and audio_manager.has_method("play_combo_sound"):
		audio_manager.play_combo_sound()  # Reuse combo sound for now
	
	# Show loot drop animation/UI
	_show_loot_drop_celebration(cosmetic_id)

## Shows the loot drop celebration UI.
func _show_loot_drop_celebration(cosmetic_id: String) -> void:
	# Create floating cosmetic icon with particles
	var celebration = _create_loot_drop_celebration(cosmetic_id)
	if celebration != null:
		get_tree().current_scene.add_child(celebration)
		
		# Auto-remove after animation
		var timer = get_tree().create_timer(4.0)
		timer.timeout.connect(func(): celebration.queue_free())
		add_child(timer)
		timer.start()

## Creates a visual celebration for the earned cosmetic.
func _create_loot_drop_celebration(cosmetic_id: String) -> Node:
	# This would be implemented with actual UI elements
	# For now, just log the celebration
	print("🎉 CELEBRATION: You earned %s!" % cosmetic_id)
	
	# TODO: Create actual UI with:
	# - Floating cosmetic icon
	# - Particle effects
	# - "You earned [Cosmetic]!" text
	# - Animation scaling up and fading out
	
	return null  # Placeholder

## Gets the type category of a cosmetic ID.
func _get_cosmetic_type(cosmetic_id: String) -> String:
	var hat_cosmetics = ["cap", "crown", "beanie", "tophat", "cowboy", "beret"]
	var glasses_cosmetics = ["round", "aviator", "sunglasses", "nerd_glasses", "monocle", "3d_glasses"]
	var emotion_cosmetics = ["happy", "angry", "sad", "excited", "surprised"]
	var moustache_cosmetics = ["normal", "fancy", "handlebar", "pencil", "walrus"]
	var wig_cosmetics = ["afro", "long_hair", "ponytail", "mohawk"]
	var slingshot_cosmetics = ["golden_slingshot", "rainbow_slingshot"]
	var projectile_cosmetics = ["rainbow_projectile", "fire_projectile"]
	var trail_cosmetics = ["sparkle_trail", "smoke_trail"]
	var hit_cosmetics = ["explosion_hit", "flash_hit"]
	var victory_cosmetics = ["victory_fireworks", "confetti_victory"]
	
	if cosmetic_id in hat_cosmetics:
		return "hat"
	elif cosmetic_id in glasses_cosmetics:
		return "glasses"
	elif cosmetic_id in emotion_cosmetics:
		return "emotion"
	elif cosmetic_id in moustache_cosmetics:
		return "moustache"
	elif cosmetic_id in wig_cosmetics:
		return "wig"
	elif cosmetic_id in slingshot_cosmetics:
		return "slingshot_skin"
	elif cosmetic_id in projectile_cosmetics:
		return "projectile_skin"
	elif cosmetic_id in trail_cosmetics:
		return "trail_effect"
	elif cosmetic_id in hit_cosmetics:
		return "hit_effect"
	elif cosmetic_id in victory_cosmetics:
		return "victory_effect"
	else:
		return "cosmetic"

## Persistence methods

func _load_drop_history() -> void:
	if not FileAccess.file_exists(DROP_HISTORY_PATH):
		return
	
	var file = FileAccess.open(DROP_HISTORY_PATH, FileAccess.READ)
	if file:
		var json_string = file.get_as_text()
		file.close()
		
		if not json_string.is_empty():
			var json = JSON.new()
			var error = json.parse(json_string)
			if error == OK:
				_apply_drop_history_json(json.data)

func _save_drop_history() -> void:
	var history = _build_drop_history_json()
	var file = FileAccess.open(DROP_HISTORY_PATH, FileAccess.WRITE)
	if file:
		file.store_string(JSON.stringify(history, "\t"))
		file.close()

func _build_drop_history_json() -> Dictionary:
	return {
		"perfect_scores_since_last_drop": _perfect_scores_since_last_drop,
		"total_perfect_scores": _total_perfect_scores,
		"last_drop_time": _last_drop_time
	}

func _apply_drop_history_json(json_data: Dictionary) -> void:
	if typeof(json_data) != TYPE_DICTIONARY:
		return
	
	_perfect_scores_since_last_drop = json_data.get("perfect_scores_since_last_drop", 0)
	_total_perfect_scores = json_data.get("total_perfect_scores", 0)
	_last_drop_time = json_data.get("last_drop_time", 0)

## Utility methods

## Manually awards a cosmetic (for testing or admin commands).
func force_award_cosmetic(cosmetic_id: String, level_number: int = 0) -> void:
	var player_profile = get_node_or_null("/root/PlayerProfile")
	if player_profile and player_profile.has_method("unlock_cosmetic"):
		player_profile.unlock_cosmetic(cosmetic_id)
	
	_last_drop_time = Time.get_ticks_msec()
	_perfect_scores_since_last_drop = 0
	
	print("🛠️ FORCE AWARD: %s (Level %d)" % [cosmetic_id, level_number])
	cosmetic_earned.emit(cosmetic_id, _get_cosmetic_type(cosmetic_id))

## Resets all drop history (for testing).
func reset_drop_history() -> void:
	_perfect_scores_since_last_drop = 0
	_total_perfect_scores = 0
	_last_drop_time = 0
	
	_save_drop_history()
	print("Drop history reset")

## Gets current drop chance percentage for UI display.
func get_current_drop_chance_percentage() -> float:
	return _calculate_drop_chance() * 100.0
