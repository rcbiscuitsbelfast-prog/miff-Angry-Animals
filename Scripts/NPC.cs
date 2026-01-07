using Godot;
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
