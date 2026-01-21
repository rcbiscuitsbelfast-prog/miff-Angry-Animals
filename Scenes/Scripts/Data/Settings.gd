extends Resource
class_name SettingsData

@export var master_volume: float = 1.0
@export var music_volume: float = 0.7
@export var sfx_volume: float = 1.0
@export var screen_shake: bool = true
@export var particles_enabled: bool = true

func to_dict() -> Dictionary:
	return {
		"master_volume": master_volume,
		"music_volume": music_volume,
		"sfx_volume": sfx_volume,
		"screen_shake": screen_shake,
		"particles_enabled": particles_enabled
	}

static func from_dict(d: Dictionary) -> SettingsData:
	var s = SettingsData.new()
	s.master_volume = d.get("master_volume", 1.0)
	s.music_volume = d.get("music_volume", 0.7)
	s.sfx_volume = d.get("sfx_volume", 1.0)
	s.screen_shake = d.get("screen_shake", true)
	s.particles_enabled = d.get("particles_enabled", true)
	return s
