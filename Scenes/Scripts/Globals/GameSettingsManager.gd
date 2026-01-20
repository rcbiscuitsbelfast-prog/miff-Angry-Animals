extends Node
class_name GameSettingsManager

const SETTINGS_FILE_PATH = "user://game_settings.json"

# Physics Settings
@export_group("Physics Settings")
## Multiplier for slingshot impulse force. Higher = more powerful shots.
@export var slingshot_impulse_multiplier: float = 20.0
## Maximum slingshot impulse force. Caps the maximum shot power.
@export var slingshot_impulse_max: float = 1200.0
## Maximum drag distance for slingshot. Controls shot angle range.
@export var slingshot_drag_max: float = 60.0
## Minimum drag distance to launch projectile. Prevents accidental shots.
@export var slingshot_drag_min: float = 10.0
## Projectile speed threshold for 'almost stopped' detection.
@export var projectile_stopped_threshold: float = 0.1
## Gravity scale applied to all projectiles.
@export var projectile_gravity_scale: float = 1.0
## Bounce coefficient for projectile-wall collisions.
@export var projectile_bounce_coefficient: float = 0.7
## Character movement speed in pixels per second.
@export var character_move_speed: float = 200.0
## Character jump force in pixels per second.
@export var character_jump_force: float = 400.0
## Character movement acceleration.
@export var character_acceleration: float = 1500.0

# Ragdoll Physics Settings
@export_group("Ragdoll Physics Settings")
## Joint stiffness for ragdoll limbs. 0.1 = very loose, 1.0 = very stiff.
@export var ragdoll_joint_stiffness: float = 0.7
## Angular damping for ragdoll limbs. Higher = less spinning.
@export var ragdoll_angular_damping: float = 3.0
## Linear damping for ragdoll limbs. Higher = slower movement.
@export var ragdoll_linear_damping: float = 2.0
## Ragdoll limb mass. Affects force response.
@export var ragdoll_limb_mass: float = 1.0
## Time in seconds before ragdoll limbs are automatically cleaned up.
@export var ragdoll_lifetime: float = 8.0
## Explosion force multiplier applied to ragdoll limbs.
@export var ragdoll_explosion_force_multiplier: float = 1.0
## Explosion radius in pixels. Higher = affects more limbs.
@export var ragdoll_explosion_radius: float = 150.0
## Enable/disable gravity for ragdoll limbs.
@export var ragdoll_gravity_enabled: bool = true

# UI/Transition Settings
@export_group("UI & Transition Settings")
## Duration of level complete fade effect in seconds.
@export var level_complete_fade_duration: float = 1.0
## Color used for fade effects.
@export var level_complete_fade_color: Color = Color.BLACK
## Duration of menu transition animations in seconds.
@export var menu_transition_speed: float = 0.3
## Time to hold score screen before auto-advance in seconds.
@export var score_screen_hold_duration: float = 3.0
## Duration of star animation in level complete screen.
@export var star_animation_duration: float = 0.3
## Scale factor for star appearance animation.
@export var star_bounce_scale: float = 1.3
## Settings panel fade-in duration.
@export var settings_panel_fade_in_duration: float = 0.3
## Settings panel fade_out duration.
@export var settings_panel_fade_out_duration: float = 0.2

# Difficulty Settings
@export_group("Difficulty Settings")
## Base difficulty multiplier applied to all challenges.
@export var base_difficulty_multiplier: float = 1.0
## Multiplier for enemy health across all enemy types.
@export var enemy_health_multiplier: float = 1.0
## Multiplier for enemy damage output.
@export var enemy_damage_multiplier: float = 1.0
## Offset added to room target scores. Can be negative to make easier.
@export var room_target_score_offset: int = 0
## Percentage of optimal score required for 3-star rating.
@export var perfect_score_threshold: float = 0.9
## Percentage of optimal score required for 2-star rating.
@export var good_score_threshold: float = 0.6
## Bonus points awarded for watching rewarded ads.
@export var rewarded_ad_bonus_points: int = 5

# Audio Settings
@export_group("Audio Settings")
## Master volume multiplier.
@export var master_volume: float = 1.0
## Music volume multiplier.
@export var music_volume: float = 0.7
## Sound effects volume multiplier.
@export var sfx_volume: float = 1.0
## Voice/vocal sound effects volume multiplier.
@export var voice_volume: float = 0.8
## Enable/disable impact vocal sounds.
@export var enable_impact_vocals: bool = true
## Maximum number of simultaneous sound effects.
@export var max_simultaneous_sounds: int = 3

# Visual Settings
@export_group("Visual Settings")
## Screen shake intensity multiplier.
@export var screen_shake_intensity: float = 1.0
## Particle effect density multiplier.
@export var particle_density: float = 1.0
## UI animation speed multiplier.
@export var ui_animation_speed: float = 1.0
## Enable/disable colorblind mode for accessibility.
@export var colorblind_mode: bool = false
## Enable/disable high contrast mode for accessibility.
@export var high_contrast_mode: bool = false
## Reduce motion effects for accessibility.
@export var reduce_motion: bool = false
## Scale factor for UI text.
@export var text_scale: float = 1.0
## Enable/disable haptic feedback on supported devices.
@export var haptic_feedback_enabled: bool = true

# Unlockable Gameplay Modifiers
@export_group("Unlockable Gameplay Modifiers")
## Double ragdoll intensity and chaos physics.
@export var extreme_physics_mode: bool = false
## Enlarge character heads for comedic effect.
@export var big_heads_mode: bool = false
## Each projectile triggers two explosions.
@export var double_explosions_mode: bool = false
## Enable slow motion time control ability.
@export var slow_motion_mode: bool = false
## Ragdoll limbs float without gravity.
@export var no_gravity_mode: bool = false
## Characters use bright neon colors.
@export var colorful_mode: bool = false
## Disables all modifiers for hardcore/speedrun mode.
@export var hardcore_mode: bool = false

func _ready() -> void:
	process_mode = Node.PROCESS_MODE_ALWAYS
	load_settings()

func _exit_tree() -> void:
	save_settings()

func save_settings() -> void:
	var file = FileAccess.open(SETTINGS_FILE_PATH, FileAccess.WRITE)
	if file:
		var json_string = JSON.stringify(_build_settings_dict())
		file.store_string(json_string)
	else:
		push_warning("Failed to save settings: Could not open file for writing.")

func load_settings() -> void:
	if not FileAccess.file_exists(SETTINGS_FILE_PATH):
		return
	
	var file = FileAccess.open(SETTINGS_FILE_PATH, FileAccess.READ)
	if file:
		var json_string = file.get_as_text()
		var json = JSON.new()
		var error = json.parse(json_string)
		if error == OK:
			_apply_settings_dict(json.data)
		else:
			push_warning("Failed to load settings: JSON parse error.")

func _build_settings_dict() -> Dictionary:
	return {
		"slingshot_impulse_multiplier": slingshot_impulse_multiplier,
		"slingshot_impulse_max": slingshot_impulse_max,
		"slingshot_drag_max": slingshot_drag_max,
		"slingshot_drag_min": slingshot_drag_min,
		"projectile_stopped_threshold": projectile_stopped_threshold,
		"projectile_gravity_scale": projectile_gravity_scale,
		"projectile_bounce_coefficient": projectile_bounce_coefficient,
		"character_move_speed": character_move_speed,
		"character_jump_force": character_jump_force,
		"character_acceleration": character_acceleration,
		"ragdoll_joint_stiffness": ragdoll_joint_stiffness,
		"ragdoll_angular_damping": ragdoll_angular_damping,
		"ragdoll_linear_damping": ragdoll_linear_damping,
		"ragdoll_limb_mass": ragdoll_limb_mass,
		"ragdoll_lifetime": ragdoll_lifetime,
		"ragdoll_explosion_force_multiplier": ragdoll_explosion_force_multiplier,
		"ragdoll_explosion_radius": ragdoll_explosion_radius,
		"ragdoll_gravity_enabled": ragdoll_gravity_enabled,
		"level_complete_fade_duration": level_complete_fade_duration,
		"level_complete_fade_color": level_complete_fade_color.to_html(),
		"menu_transition_speed": menu_transition_speed,
		"score_screen_hold_duration": score_screen_hold_duration,
		"star_animation_duration": star_animation_duration,
		"star_bounce_scale": star_bounce_scale,
		"settings_panel_fade_in_duration": settings_panel_fade_in_duration,
		"settings_panel_fade_out_duration": settings_panel_fade_out_duration,
		"base_difficulty_multiplier": base_difficulty_multiplier,
		"enemy_health_multiplier": enemy_health_multiplier,
		"enemy_damage_multiplier": enemy_damage_multiplier,
		"room_target_score_offset": room_target_score_offset,
		"perfect_score_threshold": perfect_score_threshold,
		"good_score_threshold": good_score_threshold,
		"rewarded_ad_bonus_points": rewarded_ad_bonus_points,
		"master_volume": master_volume,
		"music_volume": music_volume,
		"sfx_volume": sfx_volume,
		"voice_volume": voice_volume,
		"enable_impact_vocals": enable_impact_vocals,
		"max_simultaneous_sounds": max_simultaneous_sounds,
		"screen_shake_intensity": screen_shake_intensity,
		"particle_density": particle_density,
		"ui_animation_speed": ui_animation_speed,
		"colorblind_mode": colorblind_mode,
		"high_contrast_mode": high_contrast_mode,
		"reduce_motion": reduce_motion,
		"text_scale": text_scale,
		"haptic_feedback_enabled": haptic_feedback_enabled,
		"extreme_physics_mode": extreme_physics_mode,
		"big_heads_mode": big_heads_mode,
		"double_explosions_mode": double_explosions_mode,
		"slow_motion_mode": slow_motion_mode,
		"no_gravity_mode": no_gravity_mode,
		"colorful_mode": colorful_mode,
		"hardcore_mode": hardcore_mode,
	}

func _apply_settings_dict(data: Variant) -> void:
	if typeof(data) != TYPE_DICTIONARY:
		return
	
	var d = data as Dictionary
	if d.has("slingshot_impulse_multiplier"): slingshot_impulse_multiplier = d["slingshot_impulse_multiplier"]
	if d.has("slingshot_impulse_max"): slingshot_impulse_max = d["slingshot_impulse_max"]
	if d.has("slingshot_drag_max"): slingshot_drag_max = d["slingshot_drag_max"]
	if d.has("slingshot_drag_min"): slingshot_drag_min = d["slingshot_drag_min"]
	if d.has("projectile_stopped_threshold"): projectile_stopped_threshold = d["projectile_stopped_threshold"]
	if d.has("projectile_gravity_scale"): projectile_gravity_scale = d["projectile_gravity_scale"]
	if d.has("projectile_bounce_coefficient"): projectile_bounce_coefficient = d["projectile_bounce_coefficient"]
	if d.has("character_move_speed"): character_move_speed = d["character_move_speed"]
	if d.has("character_jump_force"): character_jump_force = d["character_jump_force"]
	if d.has("character_acceleration"): character_acceleration = d["character_acceleration"]
	if d.has("ragdoll_joint_stiffness"): ragdoll_joint_stiffness = d["ragdoll_joint_stiffness"]
	if d.has("ragdoll_angular_damping"): ragdoll_angular_damping = d["ragdoll_angular_damping"]
	if d.has("ragdoll_linear_damping"): ragdoll_linear_damping = d["ragdoll_linear_damping"]
	if d.has("ragdoll_limb_mass"): ragdoll_limb_mass = d["ragdoll_limb_mass"]
	if d.has("ragdoll_lifetime"): ragdoll_lifetime = d["ragdoll_lifetime"]
	if d.has("ragdoll_explosion_force_multiplier"): ragdoll_explosion_force_multiplier = d["ragdoll_explosion_force_multiplier"]
	if d.has("ragdoll_explosion_radius"): ragdoll_explosion_radius = d["ragdoll_explosion_radius"]
	if d.has("ragdoll_gravity_enabled"): ragdoll_gravity_enabled = d["ragdoll_gravity_enabled"]
	if d.has("level_complete_fade_duration"): level_complete_fade_duration = d["level_complete_fade_duration"]
	if d.has("level_complete_fade_color"): level_complete_fade_color = Color.from_string(d["level_complete_fade_color"], Color.BLACK)
	if d.has("menu_transition_speed"): menu_transition_speed = d["menu_transition_speed"]
	if d.has("score_screen_hold_duration"): score_screen_hold_duration = d["score_screen_hold_duration"]
	if d.has("star_animation_duration"): star_animation_duration = d["star_animation_duration"]
	if d.has("star_bounce_scale"): star_bounce_scale = d["star_bounce_scale"]
	if d.has("settings_panel_fade_in_duration"): settings_panel_fade_in_duration = d["settings_panel_fade_in_duration"]
	if d.has("settings_panel_fade_out_duration"): settings_panel_fade_out_duration = d["settings_panel_fade_out_duration"]
	if d.has("base_difficulty_multiplier"): base_difficulty_multiplier = d["base_difficulty_multiplier"]
	if d.has("enemy_health_multiplier"): enemy_health_multiplier = d["enemy_health_multiplier"]
	if d.has("enemy_damage_multiplier"): enemy_damage_multiplier = d["enemy_damage_multiplier"]
	if d.has("room_target_score_offset"): room_target_score_offset = d["room_target_score_offset"]
	if d.has("perfect_score_threshold"): perfect_score_threshold = d["perfect_score_threshold"]
	if d.has("good_score_threshold"): good_score_threshold = d["good_score_threshold"]
	if d.has("rewarded_ad_bonus_points"): rewarded_ad_bonus_points = d["rewarded_ad_bonus_points"]
	if d.has("master_volume"): master_volume = d["master_volume"]
	if d.has("music_volume"): music_volume = d["music_volume"]
	if d.has("sfx_volume"): sfx_volume = d["sfx_volume"]
	if d.has("voice_volume"): voice_volume = d["voice_volume"]
	if d.has("enable_impact_vocals"): enable_impact_vocals = d["enable_impact_vocals"]
	if d.has("max_simultaneous_sounds"): max_simultaneous_sounds = d["max_simultaneous_sounds"]
	if d.has("screen_shake_intensity"): screen_shake_intensity = d["screen_shake_intensity"]
	if d.has("particle_density"): particle_density = d["particle_density"]
	if d.has("ui_animation_speed"): ui_animation_speed = d["ui_animation_speed"]
	if d.has("colorblind_mode"): colorblind_mode = d["colorblind_mode"]
	if d.has("high_contrast_mode"): high_contrast_mode = d["high_contrast_mode"]
	if d.has("reduce_motion"): reduce_motion = d["reduce_motion"]
	if d.has("text_scale"): text_scale = d["text_scale"]
	if d.has("haptic_feedback_enabled"): haptic_feedback_enabled = d["haptic_feedback_enabled"]
	if d.has("extreme_physics_mode"): extreme_physics_mode = d["extreme_physics_mode"]
	if d.has("big_heads_mode"): big_heads_mode = d["big_heads_mode"]
	if d.has("double_explosions_mode"): double_explosions_mode = d["double_explosions_mode"]
	if d.has("slow_motion_mode"): slow_motion_mode = d["slow_motion_mode"]
	if d.has("no_gravity_mode"): no_gravity_mode = d["no_gravity_mode"]
	if d.has("colorful_mode"): colorful_mode = d["colorful_mode"]
	if d.has("hardcore_mode"): hardcore_mode = d["hardcore_mode"]

func apply_difficulty_preset(preset_name: String) -> void:
	match preset_name.to_lower():
		"easy": _apply_easy_preset()
		"normal": _apply_normal_preset()
		"hard": _apply_hard_preset()
		"extreme": _apply_extreme_preset()
	save_settings()

func _apply_easy_preset() -> void:
	slingshot_impulse_multiplier = 25.0
	base_difficulty_multiplier = 0.8
	enemy_health_multiplier = 0.7
	enemy_damage_multiplier = 0.7
	room_target_score_offset = -10
	perfect_score_threshold = 0.85
	good_score_threshold = 0.5

func _apply_normal_preset() -> void:
	slingshot_impulse_multiplier = 20.0
	base_difficulty_multiplier = 1.0
	enemy_health_multiplier = 1.0
	enemy_damage_multiplier = 1.0
	room_target_score_offset = 0
	perfect_score_threshold = 0.9
	good_score_threshold = 0.6

func _apply_hard_preset() -> void:
	slingshot_impulse_multiplier = 16.0
	base_difficulty_multiplier = 1.3
	enemy_health_multiplier = 1.4
	enemy_damage_multiplier = 1.3
	room_target_score_offset = 15
	perfect_score_threshold = 0.95
	good_score_threshold = 0.7

func _apply_extreme_preset() -> void:
	slingshot_impulse_multiplier = 12.0
	base_difficulty_multiplier = 1.6
	enemy_health_multiplier = 1.8
	enemy_damage_multiplier = 1.6
	room_target_score_offset = 25
	perfect_score_threshold = 1.0
	good_score_threshold = 0.8
