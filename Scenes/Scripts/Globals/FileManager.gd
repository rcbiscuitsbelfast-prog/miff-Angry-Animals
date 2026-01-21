extends Node
class_name FileManager

func save_level_score_to_file(path: String, content: Array) -> void:
	var file = FileAccess.open(path, FileAccess.WRITE)
	if file:
		var data_to_save = []
		for item in content:
			if item.has_method("to_dict"):
				data_to_save.append(item.to_dict())
			else:
				data_to_save.append(item)
		
		var json_str = JSON.stringify(data_to_save, "\t")
		file.store_string(json_str)

func load_level_score_from_file(path: String) -> Array:
	if not FileAccess.file_exists(path):
		return []
	
	var file = FileAccess.open(path, FileAccess.READ)
	if file:
		var json_str = file.get_as_text()
		if json_str.is_empty():
			return []
		
		var json = JSON.new()
		var error = json.parse(json_str)
		if error == OK:
			var result = []
			if typeof(json.data) == TYPE_ARRAY:
				for item in json.data:
					if typeof(item) == TYPE_DICTIONARY:
						result.append(LevelScore.from_dict(item))
				return result
	
	return []
