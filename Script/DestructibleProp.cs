using Godot;

public partial class DestructibleProp : StaticBody2D
{
    [ExportCategory("Stats")]
    [Export] public int MaxHp = 100;
    [Export] public int ScoreValue = 50;

    [ExportCategory("Visuals")]
    [Export] public Texture2D[] DamageSprites; // Sprites for different HP levels

    [ExportCategory("Effects")]
    [Export] public PackedScene RubbleScene;
    [Export] public int MinRubbleCount = 3;
    [Export] public int MaxRubbleCount = 6;
    [Export] public AudioStream DestructionSound;
    [Export] public AudioStream HitSound;
    [Export] public PackedScene DestructionEffectScene; // Optional separate effect scene

    protected int CurrentHp;
    protected Sprite2D Sprite;
    private Tween _shakeTween;
    private AudioStreamPlayer2D _audioPlayer;

    public override void _Ready()
    {
        CurrentHp = MaxHp;
        Sprite = GetNodeOrNull<Sprite2D>("Sprite2D");
        
        // Create audio player if not exists
        _audioPlayer = GetNodeOrNull<AudioStreamPlayer2D>("AudioStreamPlayer2D");
        if (_audioPlayer == null)
        {
            _audioPlayer = new AudioStreamPlayer2D();
            AddChild(_audioPlayer);
        }
        
        UpdateVisuals();
    }

    public virtual void Hit(int damage)
    {
        if (CurrentHp <= 0) return;

        CurrentHp -= damage;
        SignalManager.EmitOnPropDamaged(this, damage);

        if (CurrentHp <= 0)
        {
            Die();
        }
        else
        {
            PlaySound(HitSound);
            Shake();
            UpdateVisuals();
        }
    }

    public virtual void Die()
    {
        SignalManager.EmitOnPropDestroyed(this, ScoreValue);

        if (DestructionEffectScene != null)
        {
            var effect = DestructionEffectScene.Instantiate<Node2D>();
            effect.GlobalPosition = GlobalPosition;
            GetTree().CurrentScene.AddChild(effect);
        }
        else
        {
            // Fallback if no effect scene: spawn rubble and play sound here
            SpawnRubble();
            // Sound is tricky if we QueueFree immediately.
            // But if we have no effect scene, maybe we should just play sound in a temporary node.
            if (DestructionSound != null)
            {
                // Create a temporary audio player in the scene
                var tempAudio = new AudioStreamPlayer2D();
                tempAudio.Stream = DestructionSound;
                tempAudio.GlobalPosition = GlobalPosition;
                tempAudio.Finished += tempAudio.QueueFree;
                tempAudio.Autoplay = true;
                GetTree().CurrentScene.AddChild(tempAudio);
            }
        }

        QueueFree();
    }

    protected void SpawnRubble()
    {
        if (RubbleScene == null) return;

        int count = GD.RandRange(MinRubbleCount, MaxRubbleCount);
        for (int i = 0; i < count; i++)
        {
            var rubble = RubbleScene.Instantiate<RigidBody2D>();
            rubble.GlobalPosition = GlobalPosition + new Vector2(GD.RandfRange(-10, 10), GD.RandfRange(-10, 10));
            GetTree().CurrentScene.CallDeferred(Node.MethodName.AddChild, rubble);
        }
    }

    private void Shake()
    {
        if (Sprite == null) return;
        
        if (_shakeTween != null && _shakeTween.IsRunning()) _shakeTween.Kill();
        
        _shakeTween = CreateTween();
        Vector2 originalPos = Vector2.Zero; // Assuming local position starts at 0,0 relative to parent body
        
        // We shake the sprite relative to its current position (which should be originalPos usually)
        // Resetting to 0,0 first to be safe or assuming it is at 0,0.
        // Better: use Sprite.Position as base.
        Vector2 startPos = Sprite.Position;
        
        for(int i=0; i<5; i++)
        {
            Vector2 offset = new Vector2(GD.RandfRange(-3, 3), GD.RandfRange(-3, 3));
            _shakeTween.TweenProperty(Sprite, "position", startPos + offset, 0.05f);
        }
        _shakeTween.TweenProperty(Sprite, "position", startPos, 0.05f);
    }

    private void PlaySound(AudioStream stream)
    {
        if (_audioPlayer != null && stream != null)
        {
            _audioPlayer.Stream = stream;
            _audioPlayer.Play();
        }
    }

    private void UpdateVisuals()
    {
        if (Sprite == null || DamageSprites == null || DamageSprites.Length == 0) return;

        float hpPercent = (float)CurrentHp / MaxHp;
        
        // Determine stage. 
        // Example: 2 damage sprites.
        // 100% - 67%: No damage sprite (original texture remains?)
        // 66% - 34%: DamageSprite[0]
        // 33% - 1%: DamageSprite[1]
        
        int stages = DamageSprites.Length;
        // The original sprite is stage 0 (implicit). 
        // DamageSprites are stages 1 to N.
        
        // Actually the prompt says "multi-stage sprites". It implies we swap textures.
        // Let's assume DamageSprites are ONLY the damaged versions.
        // And we keep the original texture if HP is high?
        // Or DamageSprites includes all stages?
        // Let's assume DamageSprites contains the DAMAGED versions.
        
        int index = -1;
        float step = 1.0f / (stages + 1);
        
        // 1.0 -> 0.66 : safe
        // 0.66 -> 0.33 : sprite 0
        // 0.33 -> 0.0 : sprite 1
        
        for(int i = 0; i < stages; i++)
        {
            float threshold = 1.0f - (step * (i + 1));
            if (hpPercent <= threshold)
            {
                index = i;
            }
        }
        
        if (index >= 0)
        {
            Sprite.Texture = DamageSprites[index];
        }
    }
}
