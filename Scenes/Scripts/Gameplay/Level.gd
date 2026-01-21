extends Node2D

## Represents a game level.
## Manages the overall level setup. Projectile loading is now handled by ProjectilesLoader.

func _ready():
	pass

func _process(delta):
	if Input.is_key_pressed(Key.Q):
		GameManager.load_main()