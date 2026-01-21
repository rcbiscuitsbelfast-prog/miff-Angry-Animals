extends Control
class_name SettingsMenu

signal settings_closed()
signal volume_changed(master: float, music: float, sfx: float)

@export var panel_path: NodePath
@export var back_button_path: NodePath
@export var master_volume_slider_path: NodePath
@export var music_volume_slider_path: NodePath
@export var sfx_volume_slider_path: NodePath
@export var screen_shake_toggle_path: NodePath
@export var particles_toggle_path: NodePath
@export var haptics_toggle_path: NodePath
@export var easy_mode_button_path: NodePath
@export var normal_mode_button_path: NodePath
@export var hard_mode_button_path: NodePath
@export var remove_ads_button_path: NodePath

var _panel: Control
var _back_button: Button
var _master_volume_slider: HSlider
var _music_volume_slider: HSlider
var _sfx_volume_slider: HSlider
var _screen_shake_toggle: CheckBox
var _particles_toggle: CheckBox
var _haptics_toggle: CheckBox
var _easy_mode_button: Button
var _normal_mode_button: Button
var _hard_mode_button: Button
var _remove_ads_button: Button

func _ready() -> void:
	_initialize_settings_menu()
	_connect_signals()
	_load_current_settings()

func _initialize_settings_menu() -> void:
	_panel = get_node_or_null(panel_path)
	_back_button = get_node_or_null(back_button_path)
	_master_volume_slider = get_node_or_null(master_volume_slider_path)
	_music_volume_slider = get_node_or_null(music_volume_slider_path)
	_sfx_volume_slider = get_node_or_null(sfx_volume_slider_path)
	_screen_shake_toggle = get_node_or_null(screen_shake_toggle_path)
	_particles_toggle = get_node_or_null(particles_toggle_path)
	_haptics_toggle = get_node_or_null(haptics_toggle_path)
	_easy_mode_button = get_node_or_null(easy_mode_button_path)
	_normal_mode_button = get_node_or_null(normal_mode_button_path)
	_hard_mode_button = get_node_or_null(hard_mode_button_path)
	_remove_ads_button = get_node_or_null(remove_ads_button_path)

	if _panel:
		_panel.visible = false

func _connect_signals() -> void:
	if _back_button: _back_button.pressed.connect(_on_back_pressed)
	if _master_volume_slider: _master_volume_slider.value_changed.connect(_on_master_volume_changed)
	if _music_volume_slider: _music_volume_slider.value_changed.connect(_on_music_volume_changed)
	if _sfx_volume_slider: _sfx_volume_slider.value_changed.connect(_on_sfx_volume_changed)
	if _screen_shake_toggle: _screen_shake_toggle.toggled.connect(_on_screen_shake_toggled)
	if _particles_toggle: _particles_toggle.toggled.connect(_on_particles_toggled)
	if _haptics_toggle: _haptics_toggle.toggled.connect(_on_haptics_toggled)
	
	if _easy_mode_button: _easy_mode_button.pressed.connect(_apply_difficulty_preset.bind("easy"))
	if _normal_mode_button: _normal_mode_button.pressed.connect(_apply_difficulty_preset.bind("normal"))
	if _hard_mode_button: _hard_mode_button.pressed.connect(_apply_difficulty_preset.bind("hard"))
	
	if _remove_ads_button: _remove_ads_button.pressed.connect(_on_remove_ads_pressed)

func _load_current_settings() -> void:
	var settings = get_node_or_null("/root/GameSettingsManager")
	if settings:
		if _master_volume_slider: _master_volume_slider.value = settings.master_volume
		if _music_volume_slider: _music_volume_slider.value = settings.music_volume
		if _sfx_volume_slider: _sfx_volume_slider.value = settings.sfx_volume
		if _screen_shake_toggle: _screen_shake_toggle.button_pressed = true # Default
		if _particles_toggle: _particles_toggle.button_pressed = true # Default
	
	var player_profile = get_node_or_null("/root/PlayerProfile")
	if _haptics_toggle and player_profile:
		_haptics_toggle.button_pressed = player_profile.high_contrast_mode if "high_contrast_mode" in player_profile else false

	_update_remove_ads_button()

func _on_master_volume_changed(value: float) -> void:
	var settings = get_node_or_null("/root/GameSettingsManager")
	if settings: settings.master_volume = value
	_update_volume_display()

func _on_music_volume_changed(value: float) -> void:
	var settings = get_node_or_null("/root/GameSettingsManager")
	if settings: settings.music_volume = value
	_update_volume_display()

func _on_sfx_volume_changed(value: float) -> void:
	var settings = get_node_or_null("/root/GameSettingsManager")
	if settings: settings.sfx_volume = value
	_update_volume_display()

func _update_volume_display() -> void:
	if not (_master_volume_slider and _music_volume_slider and _sfx_volume_slider): return
	volume_changed.emit(_master_volume_slider.value, _music_volume_slider.value, _sfx_volume_slider.value)

func _on_screen_shake_toggled(_pressed: bool) -> void:
	var game_feel_manager = get_node_or_null("/root/GameFeelManager")
	if game_feel_manager and game_feel_manager.has_method("on_button_press"): game_feel_manager.on_button_press(self)

func _on_particles_toggled(_pressed: bool) -> void:
	var game_feel_manager = get_node_or_null("/root/GameFeelManager")
	if game_feel_manager and game_feel_manager.has_method("on_button_press"): game_feel_manager.on_button_press(self)

func _on_haptics_toggled(_pressed: bool) -> void:
	var game_feel_manager = get_node_or_null("/root/GameFeelManager")
	if game_feel_manager and game_feel_manager.has_method("on_button_press"): game_feel_manager.on_button_press(self)

func _apply_difficulty_preset(mode: String) -> void:
	var game_feel_manager = get_node_or_null("/root/GameFeelManager")
	if game_feel_manager and game_feel_manager.has_method("on_button_press"): game_feel_manager.on_button_press(self)
	
	var settings = get_node_or_null("/root/GameSettingsManager")
	if settings and settings.has_method("apply_difficulty_preset"):
		settings.apply_difficulty_preset(mode)

func _on_remove_ads_pressed() -> void:
	var game_feel_manager = get_node_or_null("/root/GameFeelManager")
	if game_feel_manager and game_feel_manager.has_method("on_button_press"): game_feel_manager.on_button_press(self)
	
	var premium_manager = get_node_or_null("/root/PremiumManager")
	if not premium_manager: return
	
	if premium_manager.is_ad_free_version: return
	
	if premium_manager.has_method("purchase_remove_ads"):
		premium_manager.purchase_remove_ads()

func _update_remove_ads_button() -> void:
	if not _remove_ads_button: return
	var premium_manager = get_node_or_null("/root/PremiumManager")
	if premium_manager and premium_manager.is_ad_free_version:
		_remove_ads_button.text = "✓ Ad-Free"
		_remove_ads_button.disabled = true
	else:
		var price = premium_manager.remove_ads_price if premium_manager and "remove_ads_price" in premium_manager else "$0.99"
		_remove_ads_button.text = "Remove Ads - %s" % price
		_remove_ads_button.disabled = false

func _on_back_pressed() -> void:
	var game_feel_manager = get_node_or_null("/root/GameFeelManager")
	if game_feel_manager and game_feel_manager.has_method("on_button_press"): game_feel_manager.on_button_press(self)
	hide_settings()
	settings_closed.emit()

func show_settings() -> void:
	if _panel:
		_panel.visible = true
		_panel.modulate.a = 0.0
		var tween = create_tween()
		tween.tween_property(_panel, "modulate:a", 1.0, 0.3).set_trans(Tween.TRANS_SINE)
	_load_current_settings()

func hide_settings() -> void:
	if _panel:
		var tween = create_tween()
		tween.tween_property(_panel, "modulate:a", 0.0, 0.2)
		tween.tween_callback(func(): _panel.visible = false)

func is_visible_on_screen() -> bool:
	return _panel != null and _panel.visible
