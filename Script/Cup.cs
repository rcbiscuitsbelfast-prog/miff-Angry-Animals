using Godot;

/// <summary>
/// Represents a destructible cup object in the level.
/// Plays an animation before being destroyed.
/// </summary>
public partial class Cup : DestructibleProp
{
    public const string GROUP_NAME = "cup";

    [Export] AnimationPlayer _vanishAnimation;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        // Initialize base DestructibleProp
        base._Ready();
        
        if (MaxHp <= 0) MaxHp = 10;
        CurrentHp = MaxHp;
        
        //Connects the vanish animation to its destruction event.
        if (_vanishAnimation != null)
            _vanishAnimation.AnimationFinished += OnAnimationFinished;
    }

    public override void _ExitTree()
    {
        if (_vanishAnimation != null)
            _vanishAnimation.AnimationFinished -= OnAnimationFinished;
            
        base._ExitTree();
    }

    protected override void Die()
    {
        // Emit legacy signal
        SignalManager.EmitOnCupDestroyed();
        
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
        }
    }

    /// <summary>
    /// Triggered when the vanish animation finishes.
    /// Emits the cup destroyed signal and removes the node from the scene.
    /// </summary>
    /// <param name="animName"></param>
    private void OnAnimationFinished(StringName animName)
    {
        QueueFree();
    }
}
