using Godot;
<<<<<<< HEAD
using Godot.Collections;

/// <summary>
/// Base NPC class for story levels (family, schoolmates, authority, etc.).
/// This is intentionally lightweight: levels can place NPC nodes visually and set exports in the Inspector.
/// </summary>
public partial class NPC : Node2D
{
    [Signal] public delegate void NpcHitEventHandler(NPC npc);
    [Signal] public delegate void NpcDestroyedEventHandler(NPC npc);

    public enum NpcType
    {
        Family,
        Schoolmate,
        Authority,
        Soldier
    }

    public enum FaceSource
    {
        PlayerFace,
        NpcUnique
    }

    public enum BehaviorType
    {
        Static,
        MovingPatrol,
        Caged,
        Destructible
    }

    public enum AnimationState
    {
        Idle,
        Walk,
        Hit,
        Destroyed
    }

    [ExportGroup("Identity")]
    [Export] public NpcType Type { get; set; } = NpcType.Schoolmate;
    [Export] public FaceSource Face { get; set; } = FaceSource.PlayerFace;

    [ExportGroup("Visuals")]
    [Export] public NodePath FaceSpritePath { get; set; }
    [Export] public string UniqueFaceTexturePath { get; set; } = string.Empty;
    [Export] public Array<string> CosmeticOverlays { get; set; } = new();

    [ExportGroup("Behaviour")]
    [Export] public BehaviorType Behaviour { get; set; } = BehaviorType.Static;
    [Export] public float PatrolSpeed { get; set; } = 60f;
    [Export] public Vector2 PatrolPointA { get; set; } = new Vector2(-60, 0);
    [Export] public Vector2 PatrolPointB { get; set; } = new Vector2(60, 0);

    [ExportGroup("Combat")]
    [Export] public int Health { get; set; } = 1;

    [ExportGroup("Dialogue")]
    [Export] public Array<string> Dialogue { get; set; } = new();
    [Export] public string SpeakerId { get; set; } = string.Empty;
    [Export] public string PortraitVariant { get; set; } = string.Empty;

    private Sprite2D? _faceSprite;
    private AnimationState _animState = AnimationState.Idle;

    private bool _patrolForward = true;
    private Vector2 _origin;

    public override void _Ready()
    {
        _origin = Position;
        _faceSprite = GetNodeOrNull<Sprite2D>(FaceSpritePath);

        ApplyFace();
    }

    public override void _Process(double delta)
    {
        if (Behaviour == BehaviorType.MovingPatrol)
            ProcessPatrol((float)delta);
    }

    private void ApplyFace()
    {
        if (_faceSprite == null)
            return;

        if (Face == FaceSource.PlayerFace)
        {
            var path = PlayerProfile.Instance?.FaceImagePath ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(path) && FileAccess.FileExists(path))
            {
                var image = new Image();
                if (image.Load(path) == Error.Ok)
                    _faceSprite.Texture = ImageTexture.CreateFromImage(image);
            }
        }
        else if (!string.IsNullOrWhiteSpace(UniqueFaceTexturePath) && ResourceLoader.Exists(UniqueFaceTexturePath))
        {
            _faceSprite.Texture = ResourceLoader.Load<Texture2D>(UniqueFaceTexturePath);
        }

        CosmeticOverlay.ApplyOverlays(_faceSprite, CosmeticOverlays);
    }

    private void ProcessPatrol(float delta)
    {
        var a = _origin + PatrolPointA;
        var b = _origin + PatrolPointB;

        var target = _patrolForward ? b : a;
        Position = Position.MoveToward(target, PatrolSpeed * delta);

        if (Position.DistanceTo(target) < 1f)
            _patrolForward = !_patrolForward;

        SetAnimationState(AnimationState.Walk);
    }

    public void TakeDamage(int amount)
    {
        if (Behaviour != BehaviorType.Destructible)
        {
            React();
            EmitSignal(SignalName.NpcHit, this);
            SignalManager.EmitOnNpcHit(this);
            return;
        }

        if (_animState == AnimationState.Destroyed)
            return;

        Health -= Mathf.Max(1, amount);
        SetAnimationState(AnimationState.Hit);

        React();
        EmitSignal(SignalName.NpcHit, this);
        SignalManager.EmitOnNpcHit(this);

        if (Health <= 0)
            DestroyNpc();
    }

    private void DestroyNpc()
    {
        SetAnimationState(AnimationState.Destroyed);

        EmitSignal(SignalName.NpcDestroyed, this);
        SignalManager.EmitOnNpcDestroyed(this);

        ReactDestroyed();

        // Optional: hide rather than free so designers can keep placement.
        Visible = false;
        SetProcess(false);
    }

    private void SetAnimationState(AnimationState state)
    {
        _animState = state;
    }

    private void React()
    {
        var line = PickDialogueLine();
        if (string.IsNullOrWhiteSpace(line))
            return;

        DialogueManager.Instance?.EnqueueDialogue(GetSpeakerId(), line, 1.6f, PortraitVariant);
    }

    private void ReactDestroyed()
    {
        var line = PickDialogueLine();
        if (string.IsNullOrWhiteSpace(line))
            return;

        DialogueManager.Instance?.EnqueueDialogue(GetSpeakerId(), line, 2.0f, PortraitVariant);
    }

    private string GetSpeakerId()
    {
        if (!string.IsNullOrWhiteSpace(SpeakerId))
            return SpeakerId;

        return Type switch
        {
            NpcType.Family => "FAMILY",
            NpcType.Schoolmate => "SCHOOLMATE",
            NpcType.Authority => "AUTHORITY",
            NpcType.Soldier => "SOLDIER",
            _ => "NPC"
        };
    }

    private string PickDialogueLine()
    {
        if (Dialogue == null || Dialogue.Count == 0)
            return string.Empty;

        var idx = (int)GD.RandRange(0, Dialogue.Count - 1);
        return Dialogue[idx];
    }
}
=======
using System;
using System.Collections.Generic;

/// <summary>
/// NPC.cs - Non-Player Character system
/// 
/// This script handles all NPC behavior: patrolling, destruction, dialogue.
/// Creates reusable NPCs that non-coders can drag-drop into any level.
/// 
/// HOW TO USE (Non-Coders):
/// 1. Drag NPC prefab into a level scene
/// 2. Select NPC node
/// 3. Modify Inspector properties (no code changes needed)
/// 4. Save and test
/// 
/// For customization guide, see: Docs/GUIDES/NPC_PLACEMENT_GUIDE.md
/// </summary>
public partial class NPC : Node2D
{
    // ========== PUBLIC PROPERTIES (You can change these in Inspector) ==========
    
    /// <summary>Type of NPC (FAMILY, SCHOOLMATE, AUTHORITY, SOLDIER)</summary>
    [Export] public NPCType npc_type = NPCType.SCHOOLMATE;
    
    /// <summary>Use player's face or unique NPC face?</summary>
    [Export] public FaceSource face_source = FaceSource.PLAYER_FACE;
    
    /// <summary>List of cosmetics to apply (moustache, glasses, etc.)</summary>
    [Export] public List<string> cosmetic_overlays = new();
    
    /// <summary>How the NPC behaves</summary>
    [Export] public BehaviorType behavior_type = BehaviorType.STATIC;
    
    /// <summary>Health points (destructible NPCs)</summary>
    [Export] public int health = 100;
    
    /// <summary>Dialogue lines for speech bubbles</summary>
    [Export] public string[] dialogue = { "HELP!", "Get me out!", "This is insane!" };
    
    /// <summary>Time between dialogue bubbles (seconds)</summary>
    [Export] public float dialogue_interval = 3.0f;
    
    /// <summary>Show dialogue when hit by projectile</summary>
    [Export] public bool speak_on_hit = true;
    
    /// <summary>Movement speed for patrol behavior</summary>
    [Export] public float patrol_speed = 50.0f;
    
    /// <summary>How far to patrol from starting position</summary>
    [Export] public float patrol_distance = 100.0f;
    
    // ========== ENUMS ==========
    
    public enum NPCType
    {
        FAMILY,        // Mom, Dad characters
        SCHOOLMATE,    // Classmates, friends
        AUTHORITY,     // Teachers, principals
        SOLDIER        // Military, security
    }
    
    public enum FaceSource
    {
        PLAYER_FACE,   // Use player's face from profile
        NPC_UNIQUE     // Use unique NPC face
    }
    
    public enum BehaviorType
    {
        STATIC,        // Stays in one place
        MOVING_PATROL, // Walks back and forth
        CAGED          // Trapped (bars visible)
    }
    
    // ========== PRIVATE PROPERTIES (You should NOT change these) ==========
    
    private Sprite2D sprite;
    private CollisionShape2D collision;
    private AnimatedSprite2D animated_sprite;
    private AnimationPlayer animation_player;
    private CosmeticOverlay cosmetic_overlay;
    private SpeechBubbleManager speech_bubble;
    private Area2D patrol_area;
    
    // Internal state
    private Vector2 _start_position;
    private Vector2 _patrol_direction = Vector2.Right;
    private float _dialogue_timer = 0.0f;
    private int _current_dialogue_index = 0;
    private bool _is_patrolling = false;
    
    // ========== PUBLIC METHODS (Safe to use from other scripts) ==========
    
    /// <summary>
    /// Initialize this NPC
    /// YOU WILL NOT CALL THIS (engine calls it automatically)
    /// </summary>
    public override void _Ready()
    {
        InitializeNPC();
        SetupAppearance();
        SetupBehavior();
        ConnectSignals();
    }
    
    /// <summary>
    /// Called every frame
    /// YOU WILL NOT CALL THIS (engine calls it automatically)
    /// </summary>
    public override void _Process(double delta)
    {
        UpdateDialogue((float)delta);
        UpdateBehavior((float)delta);
    }
    
    /// <summary>
    /// Called when projectile hits this NPC
    /// YOU WILL NOT CALL THIS (projectiles call it automatically)
    /// </summary>
    public void TakeDamage(int amount)
    {
        health -= amount;
        
        if (speak_on_hit && dialogue.Length > 0)
        {
            ShowRandomDialogue();
        }
        
        // Play hit animation if available
        if (animation_player != null && animation_player.HasAnimation("hit"))
        {
            animation_player.Play("hit");
        }
        
        if (health <= 0)
        {
            OnDestroyed();
        }
    }
    
    /// <summary>
    /// Shows the next dialogue line
    /// YOU CAN CALL THIS from other scripts if needed
    /// </summary>
    public void ShowNextDialogue()
    {
        if (dialogue.Length == 0 || speech_bubble == null)
            return;
            
        speech_bubble.ShowDialogue(dialogue[_current_dialogue_index]);
        _current_dialogue_index = (_current_dialogue_index + 1) % dialogue.Length;
    }
    
    /// <summary>
    /// Shows a random dialogue line
    /// YOU CAN CALL THIS from other scripts if needed
    /// </summary>
    public void ShowRandomDialogue()
    {
        if (dialogue.Length == 0 || speech_bubble == null)
            return;
            
        var random = new Random();
        int index = random.Next(dialogue.Length);
        speech_bubble.ShowDialogue(dialogue[index]);
    }
    
    // ========== PRIVATE METHODS (Internal logic - ignore these) ==========
    
    private void InitializeNPC()
    {
        // Get references to child nodes
        sprite = GetNodeOrNull<Sprite2D>("Sprite2D");
        collision = GetNodeOrNull<CollisionShape2D>("CollisionShape2D");
        animated_sprite = GetNodeOrNull<AnimatedSprite2D>("AnimatedSprite2D");
        animation_player = GetNodeOrNull<AnimationPlayer>("AnimationPlayer");
        cosmetic_overlay = GetNodeOrNull<CosmeticOverlay>("CosmeticOverlay");
        speech_bubble = GetNodeOrNull<SpeechBubbleManager>("SpeechBubbleManager");
        
        _start_position = GlobalPosition;
        
        // Setup physics if collision exists
        if (collision != null)
        {
            var rigid_body = GetParent() as RigidBody2D;
            if (rigid_body != null)
            {
                // Connect destruction signal
                rigid_body.Connect("body_entered", Callable.From<Node2D>(OnBodyEntered));
            }
        }
    }
    
    private void SetupAppearance()
    {
        // Apply face source
        if (face_source == FaceSource.PLAYER_FACE)
        {
            // Load player's face from profile
            // This would integrate with PlayerProfile system
            GD.Print("NPC: Using player's face customization");
        }
        else
        {
            // Use unique NPC face
            GD.Print("NPC: Using unique NPC face");
        }
        
        // Apply cosmetics
        if (cosmetic_overlay != null && cosmetic_overlays.Count > 0)
        {
            foreach (string cosmetic in cosmetic_overlays)
            {
                cosmetic_overlay.AddCosmetic(cosmetic);
            }
        }
    }
    
    private void SetupBehavior()
    {
        switch (behavior_type)
        {
            case BehaviorType.STATIC:
                // Do nothing, NPC stays in place
                break;
                
            case BehaviorType.MOVING_PATROL:
                _is_patrolling = true;
                break;
                
            case BehaviorType.CAGED:
                // Could add visual cage effects here
                break;
        }
    }
    
    private void UpdateBehavior(float delta)
    {
        if (!_is_patrolling || GetParent() is not RigidBody2D rigid_body)
            return;
            
        // Simple patrol AI
        Vector2 current_pos = GlobalPosition;
        Vector2 target_pos = _start_position + (_patrol_direction * patrol_distance);
        
        // Check if we've reached the target
        if (current_pos.DistanceTo(target_pos) < 10.0f)
        {
            // Reverse direction
            _patrol_direction = -_patrol_direction;
            target_pos = _start_position + (_patrol_direction * patrol_distance);
        }
        
        // Move towards target
        Vector2 direction = (target_pos - current_pos).Normalized();
        rigid_body.LinearVelocity = direction * patrol_speed;
    }
    
    private void UpdateDialogue(float delta)
    {
        _dialogue_timer += delta;
        
        if (_dialogue_timer >= dialogue_interval && dialogue.Length > 0)
        {
            ShowRandomDialogue();
            _dialogue_timer = 0.0f;
        }
    }
    
    private void ConnectSignals()
    {
        // Connect to destruction signals
        if (GetParent() is RigidBody2D rigid_body)
        {
            rigid_body.Connect("destroyed", Callable.From(OnDestroyed));
        }
    }
    
    private void OnBodyEntered(Node2D body)
    {
        // Handle collisions with projectiles
        if (body is Projectile)
        {
            var projectile = body as Projectile;
            TakeDamage(projectile.Damage);
        }
    }
    
    private void OnDestroyed()
    {
        // Emit signal that this NPC was destroyed
        // Other systems can listen for this
        GD.Print($"NPC destroyed: {npc_type}");
        
        // Award points if this was a meaningful destruction
        if (GameManager.Instance != null)
        {
            GameManager.Instance.EnemiesDefeated++;
        }
        
        // Show score popup
        var scorer = GetTree().CurrentScene as Scorer;
        if (scorer != null)
        {
            scorer.AddScore(50, GlobalPosition);
        }
        
        // Remove from scene
        QueueFree();
    }
}
>>>>>>> origin/feat/launch-readiness-repo-docs-non-coder-cms-npc-prefabs
