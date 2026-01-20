extends Node
class_name SignalManager

signal on_animal_died()
signal on_cup_destroyed()
signal on_level_completed()
signal on_attempt_made()
signal on_score_updated(score: int)
signal on_prop_damaged(prop: Node, damage: int)
signal on_prop_destroyed(prop: Node, score_value: int)
signal on_destruction_score_updated(score: int)
signal on_npc_hit(npc: Node)
signal on_npc_destroyed(npc: Node)
signal on_objectives_updated(text: String)
signal reward_earned()

func _ready() -> void:
	process_mode = Node.PROCESS_MODE_ALWAYS

func emit_on_animal_died() -> void: on_animal_died.emit()
func emit_on_cup_destroyed() -> void: on_cup_destroyed.emit()
func emit_on_level_completed() -> void: on_level_completed.emit()
func emit_on_attempt_made() -> void: on_attempt_made.emit()
func emit_on_score_updated(score: int) -> void: on_score_updated.emit(score)
func emit_on_prop_damaged(prop: Node, damage: int) -> void: on_prop_damaged.emit(prop, damage)
func emit_on_prop_destroyed(prop: Node, score_value: int) -> void: on_prop_destroyed.emit(prop, score_value)
func emit_on_destruction_score_updated(score: int) -> void: on_destruction_score_updated.emit(score)
func emit_on_npc_hit(npc: Node) -> void: on_npc_hit.emit(npc)
func emit_on_npc_destroyed(npc: Node) -> void: on_npc_destroyed.emit(npc)
func emit_on_objectives_updated(text: String) -> void: on_objectives_updated.emit(text)
func emit_reward_earned() -> void: reward_earned.emit()
