extends MarginContainer
class_name Ui

@export var level_label: Label
@export var attempt_label: Label
@export var game_over_vb: BoxContainer

@export var show_interstitial_on_game_over: bool = true
@export var failed_attempts_before_interstitial: int = 3

var _consecutive_failures: int = 0

func _ready() -> void:
	if game_over_vb:
		game_over_vb.hide()
	
	var score_manager = get_node_or_null("/root/ScoreManager")
	if score_manager and level_label:
		level_label.text = "Level: %d" % score_manager.get_level()
	
	var signal_manager = get_node_or_null("/root/SignalManager")
	if signal_manager:
		if signal_manager.has_signal("on_score_updated"): signal_manager.on_score_updated.connect(_on_update_attempts_label)
		if signal_manager.has_signal("on_level_completed"): signal_manager.on_level_completed.connect(_on_level_finished)
	
	var game_manager = get_node_or_null("/root/GameManager")
	if game_manager:
		game_manager.game_state_changed.connect(_on_game_state_changed)

func _on_game_state_changed(state: int) -> void:
	# 3 is IN_ROOM (equivalent to Playing)
	if state == 3:
		_consecutive_failures = 0

func _process(_delta: float) -> void:
	if game_over_vb and game_over_vb.visible and Input.is_action_just_pressed("level_completed"):
		var game_manager = get_node_or_null("/root/GameManager")
		if game_manager: game_manager.load_main()

func _on_update_attempts_label(attempts: int) -> void:
	if attempt_label:
		attempt_label.text = "Attempts: %d" % attempts

func _on_level_finished() -> void:
	if game_over_vb:
		game_over_vb.show()
	
	_consecutive_failures += 1
	
	if show_interstitial_on_game_over and _consecutive_failures >= failed_attempts_before_interstitial:
		_consecutive_failures = 0
		_show_interstitial_after_failures_async()

func _show_interstitial_after_failures_async() -> void:
	var monetization_manager = get_node_or_null("/root/MonetizationManager")
	if monetization_manager and not monetization_manager.show_ads:
		return
	
	var ads_manager = get_node_or_null("/root/AdsManager")
	if not ads_manager:
		return
	
	await get_tree().create_timer(1.0).timeout
	
	if ads_manager.has_method("is_interstitial_ready") and not ads_manager.is_interstitial_ready():
		if ads_manager.has_method("load_interstitial_ad"):
			ads_manager.load_interstitial_ad()
		return
	
	if ads_manager.has_method("show_interstitial_ad"):
		ads_manager.show_interstitial_ad()
