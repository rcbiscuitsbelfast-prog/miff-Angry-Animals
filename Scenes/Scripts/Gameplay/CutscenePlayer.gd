extends CanvasLayer

## Simple cutscene player that handles fade effects and scene transitions.

func _ready():
	# Initialize cutscene player
	print("CutscenePlayer initialized")

func fade_in(duration: float = 1.0):
	# Fade from black to transparent
	var fade_rect = get_node_or_null("FadeRect") as ColorRect
	if fade_rect:
		fade_rect.color = Color(0, 0, 0, 1) # Start with black
		var tween := create_tween()
		tween.tween_property(fade_rect, "color:a", 0.0, duration)

func fade_out(duration: float = 1.0):
	# Fade from transparent to black
	var fade_rect = get_node_or_null("FadeRect") as ColorRect
	if fade_rect:
		fade_rect.color = Color(0, 0, 0, 0) # Start transparent
		var tween := create_tween()
		tween.tween_property(fade_rect, "color:a", 1.0, duration)