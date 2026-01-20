extends Resource
class_name LevelObjective

enum ObjectiveType {
	DESTROY_X_CUPS,
	DESTROY_SPECIFIC_NPCS,
	CAGE_OR_CONTAIN_NPCS,
	KNOCK_NPC_INTO_HAZARD,
	REACH_EXIT,
	COLLECT_ITEMS
}

@export var type: ObjectiveType = ObjectiveType.DESTROY_X_CUPS
@export var target: String = ""
@export var count: int = 0
@export_multiline var override_text: String = ""

func get_display_text(progress_count: int) -> String:
	if not override_text.is_empty():
		return override_text

	match type:
		ObjectiveType.DESTROY_X_CUPS:
			return "Destroy %d/%d cups" % [progress_count, count] if count > 0 else "Destroy cups (%d)" % progress_count
		ObjectiveType.DESTROY_SPECIFIC_NPCS:
			return "Destroy the target NPC" if target.is_empty() else "Destroy: %s" % target
		ObjectiveType.CAGE_OR_CONTAIN_NPCS:
			return "Cage/contain NPCs" if target.is_empty() else "Cage: %s" % target
		ObjectiveType.KNOCK_NPC_INTO_HAZARD:
			return "Knock NPC into hazard" if target.is_empty() else "Hazard: %s" % target
		ObjectiveType.REACH_EXIT:
			return "Reach the exit"
		ObjectiveType.COLLECT_ITEMS:
			return "Collect %d/%d items" % [progress_count, count] if count > 0 else "Collect items"
		_:
			return "Objective"
