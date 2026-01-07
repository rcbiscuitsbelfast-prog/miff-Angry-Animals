using Godot;

/// <summary>
/// Represents a destructible cup object in the level.
/// Uses the new BreakableObstacle system for material-based damage.
/// Plays an animation before being destroyed.
/// </summary>
public partial class Cup : BreakableObstacle
{
    public const string GROUP_NAME = "cup";

    [Export] AnimationPlayer _vanishAnimation;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        // Initialize base BreakableObstacle (which includes DestructibleProp)
        base._Ready();
        
        // Ensure some defaults if not set
        if (MaxHp <= 0) MaxHp = Material.HitsToDestroy * 10; // Use material-based HP
        
        // Connects the vanish animation to its destruction event.
        if (_vanishAnimation != null)
            _vanishAnimation.AnimationFinished += OnAnimationFinished;
    }

    protected override void Die()
    {
        // Emit legacy signal
        SignalManager.EmitOnCupDestroyed();
        
<<<<<<< HEAD
        // Emit material-specific destruction information
        GD.Print($"Cup destroyed! Material: {Material.Material}, Total hits taken: {CurrentHitsTaken}");

        // Signal other systems about the destruction with material info
        SignalManager.EmitOnPropDestroyed(this, ScoreValue);
        ScoreManager.AddScore(ScoreValue);
        
        // Spawn material-appropriate rubble
        SpawnMaterialRubble();

        // Play destruction sound
        if (DestructionSound != null)
        {
            var audio = GetNodeOrNull<AudioStreamPlayer2D>("AudioStreamPlayer2D");
            if (audio != null)
            {
                audio.Stream = DestructionSound;
                audio.Play();
            }
        }

        // Play vanish animation if available
        if (_vanishAnimation != null)
        {
            _vanishAnimation.Play("vanish");
        }
        else
        {
            // Fallback: queue free immediately if no animation
            QueueFree();
        }
    }

    /// <summary>
    /// Spawns rubble appropriate to the material type.
    /// Harder materials spawn fewer, smaller rubble pieces.
    /// </summary>
    private void SpawnMaterialRubble()
    {
        if (RubbleScene == null) return;

        // Calculate rubble amount based on material hardness
        // Harder materials = less rubble
        int baseRubbleCount = GD.RandRange(MinRubbleCount, MaxRubbleCount);
        int adjustedRubbleCount = Mathf.RoundToInt(baseRubbleCount * (2.0f / Material.Hardness));
        adjustedRubbleCount = Mathf.Clamp(adjustedRubbleCount, 1, baseRubbleCount);

        for (int i = 0; i < adjustedRubbleCount; i++)
        {
            var rubble = RubbleScene.Instantiate<RigidBody2D>();
            rubble.GlobalPosition = GlobalPosition + new Vector2(GD.RandfRange(-10, 10), GD.RandfRange(-10, 10));
            
            // Apply material-specific modifications to rubble
            if (rubble is RigidBody2D body)
            {
                // Scale rubble size based on material
                float sizeModifier = 1.0f - ((Material.Hardness - 1) * 0.1f);
                body.Scale = body.Scale * sizeModifier;
                
                // Adjust mass based on material density
                body.Mass *= Material.Hardness * 0.5f;
            }
            
            GetTree().CurrentScene.CallDeferred("add_child", rubble);
=======
        // Base Die() emits PropDestroyed, spawns rubble, plays sound, and QueueFrees.
        // We want to delay QueueFree until animation finishes.
        
        SignalManager.EmitOnPropDestroyed(this, ScoreValue);
        SpawnRubble(); // Use base helper
        
        // Play sound if available (handled by helper or manually?)
        // DestructibleProp.Die has complex logic for sound/effect.
        // Let's duplicate some or call a helper?
        // DestructibleProp.Die() is:
        /*
        SignalManager.EmitOnPropDestroyed(this, ScoreValue);
        if (DestructionEffectScene != null) ...
        else { SpawnRubble(); if (DestructionSound != null) ... }
        QueueFree();
        */
        
        // We want to override the visual part (Animation instead of EffectScene) but keep score/rubble.
        
        // We already emitted score and spawned rubble above.
        
        if (_vanishAnimation != null)
        {
             _vanishAnimation.Play("vanish");
             // Sound?
             if (DestructionSound != null)
             {
                 // Play sound attached or temp
                 // We can use the base's audio player if available
                 var audio = GetNodeOrNull<AudioStreamPlayer2D>("AudioStreamPlayer2D");
                 if (audio != null)
                 {
                     audio.Stream = DestructionSound;
                     audio.Play();
                 }
             }
        }
        else
        {
             // Fallback to base Die behavior if no animation
             base.Die();
>>>>>>> origin/feat/launch-readiness-repo-docs-non-coder-cms-npc-prefabs
        }
    }

    /// <summary>
    /// Triggered when the vanish animation finishes.
    /// Emits the cup destroyed signal and removes the node from the scene.
    /// </summary>
    /// <param name="animName"></param>
    private void OnAnimationFinished(StringName animName)
<<<<<<< HEAD
    {
        QueueFree();
    }
=======
    {
        QueueFree();
    }
>>>>>>> origin/feat/launch-readiness-repo-docs-non-coder-cms-npc-prefabs
}
