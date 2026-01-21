extends Node2D
class_name ScorePopup

func show_score(value: int) -> void:
	setup(value)

func setup(value: int) -> void:
	var text = str(value)
	var offset = 0.0
	
	for c in text:
		var texture_path = "res://Assets/graphics/kenney2/numbers/%s.png" % c
		
		if ResourceLoader.exists(texture_path):
			var texture = load(texture_path)
			var sprite = Sprite2D.new()
			sprite.texture = texture
			sprite.position = Vector2(offset, 0)
			add_child(sprite)
			offset += texture.get_width()
		else:
			var label = Label.new()
			label.text = c
			label.position = Vector2(offset, 0)
			add_child(label)
			offset += 20
	
	var total_width = offset
	for child in get_children():
		if child is Node2D or child is Control:
			child.position -= Vector2(total_width / 2.0, 0)
	
	_animate()

func _animate() -> void:
	scale = Vector2.ZERO
	modulate.a = 0.0
	
	var tween = create_tween()
	tween.set_parallel(true)
	tween.tween_property(this, "scale", Vector2.ONE, 0.3).set_trans(Tween.TRANS_BACK).set_ease(Tween.EASE_OUT)
	tween.tween_property(this, "modulate:a", 1.0, 0.2)
	
	tween.chain().tween_property(this, "position", position + Vector2(0, -40), 0.5)
	tween.parallel().tween_property(this, "modulate:a", 0.0, 0.5).set_delay(0.3)
	
	tween.chain().tween_callback(queue_free)
