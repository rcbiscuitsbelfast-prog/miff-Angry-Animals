extends RigidBody2D

## Cup obstacle that can be destroyed when hit by projectiles.
## Handles physics interactions and destruction animations.

signal cup_destroyed(cup: Node)
signal cup_damaged(hp: int)

@export var max_hp: int = 10
@export var score_value: int = 500
@export var _vanish_animation: NodePath

var _vanish_animation_player: AnimationPlayer
var _current_hp: int

func _ready():
    initialize_cup()

func initialize_cup():
    _vanish_animation_player = get_node_or_null(_vanish_animation) as AnimationPlayer
    _current_hp = max_hp
    
    # Set up collision layers
    collision_layer = 4
    collision_mask = 7
    
    # Enable contact monitoring for damage detection
    max_contacts_reported = 4
    contact_monitor = true

func _physics_process(delta):
    # Check for collisions and handle damage
    handle_collisions()

func handle_collisions():
    # Check for collisions and apply damage
    var contacts = get_contact_count()
    if contacts > 0:
        for i in range(contacts):
            var contact = get_contact_collider(i)
            if contact and contact != self:
                # Apply damage to this cup
                take_damage(1)

func take_damage(damage: int):
    # Take damage and check for destruction
    _current_hp -= damage
    cup_damaged.emit(_current_hp)
    
    if _current_hp <= 0:
        destroy_cup()
    else:
        # Show damage effect (could flash color)
        show_damage_effect()

func show_damage_effect():
    # Flash the cup to show it was damaged
    var original_modulate = modulate
    modulate = Color(1, 0.5, 0.5)  # Red tint for damage
    
    var tween = create_tween()
    tween.tween_property(self, "modulate", original_modulate, 0.2)

func destroy_cup():
    # Destroy the cup and award points
    cup_destroyed.emit(self)
    
    # Award score
    if ScoreManager.instance:
        ScoreManager.add_score(score_value)
    
    # Play vanish animation
    play_vanish_animation()
    
    # Queue for deletion after animation
    var timer = Timer.new()
    timer.wait_time = 1.0
    timer.one_shot = true
    timer.timeout.connect(queue_free)
    add_child(timer)
    timer.start()

func play_vanish_animation():
    # Play the vanish animation if available
    if _vanish_animation_player:
        _vanish_animation_player.play("vanish")
    else:
        # Fallback: fade out manually
        var tween = create_tween()
        tween.tween_property(self, "modulate:a", 0.0, 0.5)

func heal(amount: int):
    # Heal the cup (if needed for power-ups)
    _current_hp = min(_current_hp + amount, max_hp)

func get_current_hp() -> int:
    return _current_hp

func get_max_hp() -> int:
    return max_hp

func is_destroyed() -> bool:
    return _current_hp <= 0