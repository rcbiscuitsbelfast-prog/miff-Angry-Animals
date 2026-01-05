# Integration Plan

**Date:** January 5, 2025
**From:** Angry Aliens (GDScript)
**To:** Angry Animals (C#)
**Strategy:** Port specific systems (not merge entire codebases)

---

## EXECUTIVE SUMMARY

Based on cross-repository analysis, **Angry Animals is the superior production base** with C# type-safety, complete monetization, and save systems. However, **Angry Aliens has 4 high-value systems** that would significantly enhance Angry Animals:

1. **Object Pooling** - Performance optimization
2. **Enemy AI System** - Complete feature gap
3. **Animation System** - Professional-grade visuals
4. **Advanced Cosmetics** - Enhanced customization

**Recommendation:** Port these systems individually rather than merging codebases.

**Total Effort:** 20-27 hours for all high/medium priority ports

---

## INTEGRATION STRATEGY

### Why Port, Not Merge?

#### Advantages of Porting:
1. **Language Consistency** - Keep everything in C# (type-safe, better tooling)
2. **Architecture Alignment** - Maintain Angry Animals' 10 autoload managers
3. **Signal-Based Events** - Preserve decoupled architecture
4. **Production Foundation** - Keep monetization, saves, deployment-ready state
5. **Selective Adoption** - Choose only best features

#### Disadvantages of Merging:
1. **Language Incompatibility** - GDScript and C# don't mix well
2. **Architecture Conflicts** - 10 vs. 4 autoloads
3. **Code Duplication** - Two implementations of same systems
4. **Regression Risk** - Losing Angry Animals' production features

---

## PORT 1: OBJECT POOLING SYSTEM ⭐⭐⭐⭐⭐

### Priority: HIGH
### Estimated Effort: 2-3 hours
### Value: ⭐⭐⭐⭐⭐ (Performance optimization)

### Source: Angry Aliens
**File:** `Objects/Pool/Node2DPool.gd` (83 lines)
**Features:**
- Generic object pooling for any Node2D
- Configurable pool size
- Active/inactive tracking
- Automatic cleanup
- Timer-based refresh
- Performance monitoring

### Target: Angry Animals
**Integration Point:** New autoload singleton

---

### Step-by-Step Implementation

#### Step 1: Create C# Pool Classes (30-45 minutes)
**File:** `Globals/ObjectPool.cs`

```csharp
using Godot;

public partial class ObjectPool : Node
{
    [Export] public PackedScene ObjectScene { get; set; }
    [Export] public int PoolSize { get; set; } = 5;
    [Export] public float RefreshTimer { get; set; } = 1.0f;

    private Node _inactiveContainer;
    private Godot.Collections.Array<Node> _activeObjects = new();

    public override void _Ready()
    {
        _inactiveContainer = new Node() { Name = "InactiveNodes" };
        AddChild(_inactiveContainer);

        // Populate pool
        for (int i = 0; i < PoolSize; i++)
        {
            var obj = _CreateObject();
            _inactiveContainer.AddChild(obj);
        }

        // Schedule pool refresh
        var timer = new Timer() { WaitTime = RefreshTimer, OneShot = false };
        AddChild(timer);
        timer.Start();
        timer.Timeout += CheckUnusedObjects;
    }

    public void Pool(Node obj)
    {
        var parent = obj.GetParent();
        parent.RemoveChild(obj);
        _activeObjects.Remove(obj);
        obj.Modulate = Colors.White;
        _inactiveContainer.AddChild(obj);
    }

    public Node GetInstance()
    {
        Node obj;
        if (_inactiveContainer.GetChildCount() > 0)
        {
            obj = _inactiveContainer.GetChild(0);
            _inactiveContainer.RemoveChild(obj);
        }
        else
        {
            GD.Print("Pool: EMPTY. Creating new object.");
            obj = _CreateObject();
        }

        obj.Modulate = Colors.White;
        _activeObjects.Add(obj);
        return obj;
    }

    public void CheckUnusedObjects()
    {
        foreach (var obj in _activeObjects)
        {
            if (obj.HasMeta("can_be_pooled") && (bool)obj.GetMeta("can_be_pooled"))
            {
                Pool(obj);
            }
        }
    }

    private Node _CreateObject()
    {
        var obj = ObjectScene.Instantiate();
        obj.Modulate = Colors.White;
        if (!obj.HasMeta("can_be_pooled"))
        {
            obj.SetMeta("can_be_pooled", true);
        }
        return obj;
    }

    public override void _ExitTree()
    {
        if (IsInGroup("pools"))
        {
            foreach (var obj in _inactiveContainer.GetChildren())
            {
                obj.QueueFree();
            }
            foreach (var obj in _activeObjects)
            {
                obj.QueueFree();
            }
        }
    }
}
```

**File:** `Globals/Poolable.cs`

```csharp
using Godot;

public interface IPoolable
{
    // Objects implementing this interface can be pooled
}
```

---

#### Step 2: Add to Autoload (5-10 minutes)
**Update:** `project.godot`

```ini
[autoload]

# Add after existing autoloads
ObjectPool="*res://Globals/ObjectPool.cs"
```

---

#### Step 3: Implement for Projectiles (30-45 minutes)
**Update:** `Script/Projectile.cs`

```csharp
// Add to existing class
public partial class Projectile : RigidBody2D, IPoolable
{
    // ... existing code ...

    public override void _ExitTree()
    {
        // Return to pool instead of destroying
        var pool = GetNode<ObjectPool>("/root/ObjectPool");
        if (pool != null)
        {
            pool.Pool(this);
        }
    }
}
```

---

#### Step 4: Implement for Rubble/Debris (20-30 minutes)
**Update:** `Script/Rubble.cs`

```csharp
public partial class Rubble : RigidBody2D, IPoolable
{
    // Similar pooling logic as Projectile
}
```

---

#### Step 5: Test Integration (30-60 minutes)
**Test Plan:**
1. Launch multiple projectiles - verify pooling works
2. Destroy many props - verify rubble pooling works
3. Check performance (before/after metrics)
4. Verify no memory leaks
5. Test with 100+ objects

---

#### Step 6: Performance Benchmarking (15-30 minutes)
**Metrics to Track:**
- Instantiation count reduction
- Memory usage reduction
- Frame rate improvement
- Garbage collection frequency

---

### Integration Checklist
- [ ] Create ObjectPool.cs
- [ ] Create IPoolable interface
- [ ] Add to project.godot autoload
- [ ] Update Projectile.cs for pooling
- [ ] Update Rubble.cs for pooling
- [ ] Update DestructibleProp.cs for pooling
- [ ] Test projectile pooling
- [ ] Test rubble pooling
- [ ] Benchmark performance
- [ ] Verify memory usage
- [ ] Update documentation

---

### Conflicts & Risks
**Low Risk:**
- Simple translation (GDScript → C#)
- No architecture conflicts
- Minimal code changes

**Potential Issues:**
- Projectile destruction flow needs adjustment
- Need to track pooled objects vs. destroyed objects

**Mitigation:**
- Test thoroughly
- Keep original destruction logic as fallback
- Document pooling behavior

---

## PORT 2: ENEMY AI SYSTEM ⭐⭐⭐⭐⭐

### Priority: HIGH
### Estimated Effort: 6-8 hours
### Value: ⭐⭐⭐⭐⭐ (Complete feature gap)

### Source: Angry Aliens
**Files:**
- `Objects/Enemy/Enemy.gd` (29 lines) - Base enemy class
- `Objects/Enemies/FighterEnemy.gd` (110 lines) - Advanced animated enemy

**Features:**
- Base Enemy class with physics-based destruction
- FighterEnemy subclass with:
  - Health system (100 HP)
  - Animation states (IDLE, HIT, DEATH, ATTACK)
  - Sprite sheet integration
  - Damage calculations
  - Hit reactions
  - Momentum-based destruction

### Target: Angry Animals
**Integration Point:** New enemy system + spawning

---

### Step-by-Step Implementation

#### Step 1: Create Base Enemy Class (45-60 minutes)
**File:** `Script/Enemy.cs`

```csharp
using Godot;

public partial class Enemy : RigidBody2D
{
    private const float DestroyThresholdByObstacles = 400f;
    private const float DestroyThreshold = 1600f;

    [Signal] public delegate void DestroyedEventHandler(Node enemy, Node collider, Vector2 impactMomentum);

    public override void _IntegrateForces(PhysicsDirectBodyState2D state)
    {
        for (int i = 0; i < state.GetContactCount(); i++)
        {
            var collider = state.GetContactColliderObject(i);
            if (collider is RigidBody2D)
            {
                var colliderBody = (RigidBody2D)collider;
                var impactMomentum = colliderBody.Mass * colliderBody.LinearVelocity - Mass * LinearVelocity;
                if (impactMomentum.Length() >= GetDestructionThreshold(colliderBody))
                {
                    EmitSignal(SignalName.Destroyed, new Godot.Collections.Array() { this, collider, impactMomentum });
                }
            }
        }
    }

    private float GetDestructionThreshold(RigidBody2D colliderType)
    {
        if (colliderType is Obstacle)
        {
            return DestroyThresholdByObstacles;
        }
        else if (colliderType is Projectile)
        {
            return DestroyThreshold;
        }
        else
        {
            return DestroyThreshold;
        }
    }
}
```

---

#### Step 2: Create Fighter Enemy Class (1.5-2 hours)
**File:** `Script/FighterEnemy.cs`

```csharp
using Godot;

public enum AnimationState
{
    Idle,
    Hit,
    Death,
    Attack
}

public partial class FighterEnemy : Enemy
{
    [Export] public int Health { get; set; } = 100;
    [Export] public int DamageThreshold { get; set; } = 800;

    private AnimationState _currentAnimation = AnimationState.Idle;
    private Sprite2D _sprite;
    private AnimationPlayer _animationPlayer;

    public override void _Ready()
    {
        base._Ready();
        _sprite = GetNode<Sprite2D>("Sprite2D");
        _animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");

        SetupAnimations();
        PlayAnimation(AnimationState.Idle);
    }

    private void SetupAnimations()
    {
        // Setup idle animation
        var idleFrames = new Godot.Collections.Array<Texture2D>();
        for (int i = 1; i <= 8; i++)
        {
            var texture = GD.Load<Texture2D>($"res://Assets/Enemies/fighter_Idle_{i:D4}.png");
            if (texture != null) idleFrames.Add(texture);
        }

        if (idleFrames.Count > 0)
        {
            _animationPlayer.AddAnimation("idle", CreateAnimation(idleFrames, 8, true));
        }

        // Setup hit animation
        var hitFrames = new Godot.Collections.Array<Texture2D>();
        for (int i = 48; i <= 51; i++)
        {
            var texture = GD.Load<Texture2D>($"res://Assets/Enemies/fighter_hit_{i:D4}.png");
            if (texture != null) hitFrames.Add(texture);
        }

        if (hitFrames.Count > 0)
        {
            _animationPlayer.AddAnimation("hit", CreateAnimation(hitFrames, 15, false));
        }

        // Setup death animation
        var deathFrames = new Godot.Collections.Array<Texture2D>();
        for (int i = 52; i <= 61; i++)
        {
            var texture = GD.Load<Texture2D>($"res://Assets/Enemies/fighter_death_{i:D4}.png");
            if (texture != null) deathFrames.Add(texture);
        }

        if (deathFrames.Count > 0)
        {
            _animationPlayer.AddAnimation("death", CreateAnimation(deathFrames, 15, false));
        }
    }

    private Animation CreateAnimation(Godot.Collections.Array<Texture2D> frames, float fps, bool loop)
    {
        var animation = new Animation();
        animation.LoopMode = loop ? Animation.LoopModeEnum.Linear : Animation.LoopModeEnum.None;
        animation.Length = frames.Count / fps;

        var trackIndex = animation.AddTrack(Animation.TrackType.Value);
        animation.TrackSetPath(trackIndex, $"{_sprite.GetPath()}:texture");
        animation.TrackSetInterpolationLoopWrap(trackIndex, false);

        for (int i = 0; i < frames.Count; i++)
        {
            var time = (float)i / fps;
            animation.TrackInsertKey(trackIndex, time, frames[i]);
        }

        return animation;
    }

    public void PlayAnimation(AnimationState state)
    {
        _currentAnimation = state;

        switch (state)
        {
            case AnimationState.Idle:
                if (_animationPlayer.HasAnimation("idle"))
                    _animationPlayer.Play("idle");
                break;

            case AnimationState.Hit:
                if (_animationPlayer.HasAnimation("hit"))
                    _animationPlayer.Play("hit");
                await ToSignal(_animationPlayer, AnimationPlayer.SignalName.AnimationFinished);
                PlayAnimation(AnimationState.Idle);
                break;

            case AnimationState.Death:
                if (_animationPlayer.HasAnimation("death"))
                    _animationPlayer.Play("death");
                break;

            case AnimationState.Attack:
                // Could add attack animation frames if available
                break;
        }
    }

    public override float GetDestructionThreshold(RigidBody2D colliderType)
    {
        if (colliderType is Obstacle)
        {
            return DamageThreshold / 2f; // Less damage from obstacles
        }
        else if (colliderType is Projectile)
        {
            return DamageThreshold;
        }
        else
        {
            return DamageThreshold;
        }
    }

    public void TakeDamage(int damageAmount)
    {
        Health -= damageAmount;
        PlayAnimation(AnimationState.Hit);

        if (Health <= 0)
        {
            PlayAnimation(AnimationState.Death);
            EmitSignal(SignalName.Destroyed, new Godot.Collections.Array() { this, null, Vector2.Zero });
        }
    }

    private void OnDestructionAnimationFinished()
    {
        QueueFree();
    }
}
```

---

#### Step 3: Create Enemy Spawner System (1.5-2 hours)
**File:** `Script/EnemySpawner.cs`

```csharp
using Godot;

public partial class EnemySpawner : Node
{
    [Export] public PackedScene EnemyScene { get; set; }
    [Export] public int SpawnInterval { get; set; } = 5;
    [Export] public int MaxEnemies { get; set; } = 3;
    [Export] public Vector2 SpawnAreaMin { get; set; }
    [Export] public Vector2 SpawnAreaMax { get; set; }

    private Timer _spawnTimer;
    private int _enemyCount = 0;

    public override void _Ready()
    {
        _spawnTimer = new Timer() { WaitTime = SpawnInterval, OneShot = false };
        AddChild(_spawnTimer);
        _spawnTimer.Timeout += SpawnEnemy;
        _spawnTimer.Start();

        // Connect to enemy destroyed signals
        GetTree().NodeAdded += OnNodeAdded;
    }

    private void SpawnEnemy()
    {
        if (_enemyCount >= MaxEnemies)
            return;

        var enemy = EnemyScene.Instantiate<FighterEnemy>();
        enemy.GlobalPosition = new Vector2(
            GD.RandfRange(SpawnAreaMin.X, SpawnAreaMax.X),
            GD.RandfRange(SpawnAreaMin.Y, SpawnAreaMax.Y)
        );

        enemy.Destroyed += OnEnemyDestroyed;
        GetParent().AddChild(enemy);
        _enemyCount++;
    }

    private void OnEnemyDestroyed(Node enemy, Node collider, Vector2 impactMomentum)
    {
        _enemyCount--;
    }

    private void OnNodeAdded(Node node)
    {
        if (node is FighterEnemy)
        {
            ((FighterEnemy)node).Destroyed += OnEnemyDestroyed;
        }
    }
}
```

---

#### Step 4: Integrate with Level System (1-1.5 hours)
**Update:** `Script/RoomBase.cs`

```csharp
// Add to RoomBase class
private EnemySpawner _enemySpawner;

public override void _Ready()
{
    // ... existing code ...

    // Create enemy spawner
    _enemySpawner = new EnemySpawner();
    _enemySpawner.EnemyScene = GD.Load<PackedScene>("res://Scenes/Enemies/FighterEnemy.tscn");
    _enemySpawner.SpawnInterval = 10; // Spawn every 10 seconds
    _enemySpawner.MaxEnemies = 2;
    _enemySpawner.SpawnAreaMin = new Vector2(-200, -100);
    _enemySpawner.SpawnAreaMax = new Vector2(200, -50);
    AddChild(_enemySpawner);
}
```

---

#### Step 5: Update Score System for Enemies (30-45 minutes)
**Update:** `Script/Scorer.cs`

```csharp
// Add enemy destruction scoring
public void OnEnemyDestroyed(Node enemy, Node collider, Vector2 impactMomentum)
{
    // Award points for destroying enemies
    _currentScore += EnemyPoints;
    UpdateScoreDisplay();
    GameManager.Instance.EnemiesDefeated++;
}
```

---

#### Step 6: Create Enemy Assets (2-3 hours)
**Required:**
- Fighter enemy sprite sheet (960x64 resolution)
- Idle frames (8)
- Hit frames (4)
- Death frames (10)
- Attack frames (optional)

**Sources:**
- Create placeholder sprites
- Use Kenney assets (free)
- Commission custom art

---

#### Step 7: Test Enemy System (1-1.5 hours)
**Test Plan:**
1. Spawn enemies - verify they appear
2. Check enemy destruction - verify animations play
3. Test enemy AI - verify they react to collisions
4. Verify scoring updates
5. Test with multiple enemies
6. Test enemy limit (MaxEnemies)

---

### Integration Checklist
- [ ] Create Enemy.cs (base class)
- [ ] Create FighterEnemy.cs (animated enemy)
- [ ] Create EnemySpawner.cs (spawning system)
- [ ] Update RoomBase.cs (integrate spawner)
- [ ] Update Scorer.cs (enemy scoring)
- [ ] Create enemy sprite sheet assets
- [ ] Create enemy scene files (.tscn)
- [ ] Test enemy spawning
- [ ] Test enemy destruction
- [ ] Test enemy animations
- [ ] Test scoring integration
- [ ] Update documentation

---

### Conflicts & Risks
**Medium Risk:**
- New system requires integration with existing level flow
- Need to create enemy assets (or use placeholders)
- Enemy AI may need tuning for Angry Animals' physics

**Potential Issues:**
- Enemy spawning may conflict with existing level design
- Enemy destruction may interfere with cup destruction logic
- Sprite sheet requirement (no existing assets)

**Mitigation:**
- Start with simple enemy behavior
- Use placeholder sprites for testing
- Make spawning configurable per level
- Test thoroughly before committing

---

## PORT 3: ANIMATION SYSTEM ⭐⭐⭐⭐

### Priority: MEDIUM
### Estimated Effort: 4-6 hours
### Value: ⭐⭐⭐⭐ (Professional-grade visuals)

### Source: Angry Aliens
**File:** `Objects/StickCloneAnimator.gd` (from documentation)
**Features:**
- Sprite sheet integration
- 6 animation states:
  - IDLE (frames 0-5)
  - WALK (frames 6-13)
  - JUMP (frames 14-17)
  - JUMP_UP (frames 14-15)
  - JUMP_DOWN (frames 16-17)
  - CLIMB (frames 18-23)
- Frame configuration per state
- Play/Stop/Loop controls
- Direction handling

### Target: Angry Animals
**Integration Point:** Update StickClone character

---

### Step-by-Step Implementation

#### Step 1: Create Animation Controller (45-60 minutes)
**File:** `Script/StickCloneAnimator.cs`

```csharp
using Godot;

public enum AnimState
{
    Idle,      // Standing still (frames 0-5)
    Walk,       // Walking animation (frames 6-13)
    Jump,       // Full jump arc (frames 14-17)
    JumpUp,     // Ascending portion (frames 14-15)
    JumpDown,   // Descending portion (frames 16-17)
    Climb       // Climbing rubble (frames 18-23)
}

public partial class StickCloneAnimator : Node
{
    [Signal] public delegate void AnimationFinishedEventHandler();

    private Sprite2D _sprite;
    private Dictionary<AnimState, AnimationConfig> _frameConfig;

    public override void _Ready()
    {
        _sprite = GetParent().GetNode<Sprite2D>("Sprite2D");
        InitializeFrameConfig();
    }

    private void InitializeFrameConfig()
    {
        _frameConfig = new Dictionary<AnimState, AnimationConfig>
        {
            {
                AnimState.Idle, new AnimationConfig { Start = 0, End = 5, Speed = 0.15f },
                AnimState.Walk, new AnimationConfig { Start = 6, End = 13, Speed = 0.10f },
                AnimState.Jump, new AnimationConfig { Start = 14, End = 17, Speed = 0.20f },
                AnimState.JumpUp, new AnimationConfig { Start = 14, End = 15, Speed = 0.15f },
                AnimState.JumpDown, new AnimationConfig { Start = 16, End = 17, Speed = 0.15f },
                AnimState.Climb, new AnimationConfig { Start = 18, End = 23, Speed = 0.12f }
            }
        };
    }

    public void PlayAnimation(AnimState state)
    {
        if (!_frameConfig.ContainsKey(state))
            return;

        var config = _frameConfig[state];
        StartCoroutine(AnimateCoroutine(state, config));
    }

    private System.Collections.IEnumerator AnimateCoroutine(AnimState state, AnimationConfig config)
    {
        for (int frame = config.Start; frame <= config.End; frame++)
        {
            _sprite.Frame = frame;
            yield return new WaitForSeconds(config.Speed);
        }

        EmitSignal(SignalName.AnimationFinished);
    }

    public void SetFacingDirection(float direction)
    {
        _sprite.FlipH = direction < 0;
    }
}

private record AnimationConfig
{
    public int Start { get; init; }
    public int End { get; init; }
    public float Speed { get; init; }
}
```

---

#### Step 2: Create Sprite Sheet Asset (2-3 hours)
**Required:**
- Stick character sprite sheet (960x64 resolution)
- Frames 0-23 (24 frames total)
- Format: PNG

**Frame Layout:**
```
Fighter Sprite Sheet (960x64 resolution)
│
├─ IDLE (0-5): Standing pose, slight bounce
├─ WALK (6-13): Walk cycle frames
├─ JUMP (14-17): Jump arc from ground to peak to landing
├─ CLIMB (18-23): Climbing animation
```

**Sources:**
- Use existing Angry Animals character art
- Create placeholder sprites
- Commission custom art

---

#### Step 3: Integrate with StickClone (1-1.5 hours)
**Update:** `Script/StickClone.cs`

```csharp
// Add to existing class
private StickCloneAnimator _animator;

public override void _Ready()
{
    // ... existing code ...

    _animator = new StickCloneAnimator();
    AddChild(_animator);
}

public override void _Process(double delta)
{
    // ... existing movement logic ...

    // Add animation triggers
    if (IsMoving)
    {
        _animator.PlayAnimation(AnimState.Walk);
        _animator.SetFacingDirection(FacingDirection);
    }
    else
    {
        _animator.PlayAnimation(AnimState.Idle);
    }
}

public void Jump()
{
    // ... existing jump logic ...

    _animator.PlayAnimation(AnimState.JumpUp);
    await ToSignal(_animator, StickCloneAnimator.SignalName.AnimationFinished);
    _animator.PlayAnimation(AnimState.JumpDown);
}

public void OnClimb()
{
    _animator.PlayAnimation(AnimState.Climb);
}
```

---

#### Step 4: Test Animations (1-1.5 hours)
**Test Plan:**
1. Test idle animation - verify continuous playback
2. Test walk animation - verify it triggers with movement
3. Test jump animations - verify proper sequencing
4. Test climb animation - verify it plays during traversal
5. Test facing direction - verify sprite flips correctly
6. Test frame transitions - verify they're smooth

---

### Integration Checklist
- [ ] Create StickCloneAnimator.cs
- [ ] Create sprite sheet asset (24 frames)
- [ ] Update StickClone.cs to use animator
- [ ] Test idle animation
- [ ] Test walk animation
- [ ] Test jump animations
- [ ] Test climb animation
- [ ] Test facing direction
- [ ] Verify smooth transitions
- [ ] Update documentation

---

### Conflicts & Risks
**Low Risk:**
- Self-contained system (doesn't affect other systems)
- Optional feature (can fall back to static sprites)

**Potential Issues:**
- Requires sprite sheet asset creation
- Animation states may not match all Angry Animals gameplay
- May need custom frames for Angry Animals character

**Mitigation:**
- Start with placeholder sprite sheet
- Test each animation state
- Create assets based on Angry Animals character style
- Keep static sprites as fallback

---

## PORT 4: ADVANCED COSMETICS ⭐⭐⭐⭐

### Priority: MEDIUM
### Estimated Effort: 8-10 hours
### Value: ⭐⭐⭐⭐ (Enhanced customization)

### Source: Angry Aliens
**Features:**
- 4 cosmetic types (hats, glasses, moustaches, wigs)
- Grid-based selection UI
- Preview panel
- Extensive variety

### Target: Angry Animals
**Integration Point:** Extend existing FaceCustomizationScreen.cs

---

### Step-by-Step Implementation

#### Step 1: Extend PlayerProfile (30-45 minutes)
**Update:** `Globals/PlayerProfile.cs`

```csharp
// Add to existing class
public string CurrentMoustache { get; set; } = "none";
public string CurrentWig { get; set; } = "none";

// Available cosmetics
public List<string> AvailableHats { get; } = new() { "none", "tophat", "cowboy", "beret", "crown" };
public List<string> AvailableGlasses { get; } = new() { "none", "sunglasses", "nerd_glasses", "monocle", "3d_glasses" };
public List<string> AvailableMoustaches { get; } = new() { "none", "normal", "fancy", "handlebar", "pencil", "walrus" };
public List<string> AvailableWigs { get; } = new() { "none", "afro", "long_hair", "ponytail", "mohawk" };
```

---

#### Step 2: Update Cosmetic UI (2-3 hours)
**Update:** `Script/FaceCustomizationScreen.cs`

```csharp
// Add to existing UI
// New sections for moustaches and wigs

private void CreateMoustacheSection()
{
    var section = new VBoxContainer();
    section.Name = "MoustacheSection";
    AddChild(section);

    // Title
    var title = new Label() { Text = "Moustaches" };
    section.AddChild(title);

    // Grid
    var grid = new GridContainer() { Columns = 4 };
    section.AddChild(grid);

    foreach (var moustache in PlayerProfile.AvailableMoustaches)
    {
        var button = CreateCosmeticButton("moustache", moustache);
        grid.AddChild(button);
    }
}

private void CreateWigSection()
{
    // Similar to moustache section
}

private void OnMoustacheSelected(string moustache)
{
    PlayerProfile.CurrentMoustache = moustache;
    UpdatePreview();
}
```

---

#### Step 3: Add Cosmetic Sprites (3-4 hours)
**Required:**
- Moustache sprites (5 types)
- Wig sprites (4 types)
- Hat sprites (update to 5 types from current)
- Glasses sprites (update to 5 types from current)

**Asset Structure:**
```
res://Assets/Sprites/Face/
├── Hats/
│   ├── none.png (placeholder)
│   ├── tophat.png
│   ├── cowboy.png
│   ├── beret.png
│   └── crown.png
├── Glasses/
│   ├── none.png
│   ├── sunglasses.png
│   ├── nerd_glasses.png
│   ├── monocle.png
│   └── 3d_glasses.png
├── Moustaches/
│   ├── none.png
│   ├── normal.png
│   ├── fancy.png
│   ├── handlebar.png
│   ├── pencil.png
│   └── walrus.png
└── Wigs/
    ├── none.png
    ├── afro.png
    ├── long_hair.png
    ├── ponytail.png
    └── mohawk.png
```

---

#### Step 4: Update Character Rendering (1-1.5 hours)
**Update:** `Script/Animal.cs` (projectile) or character scenes

```csharp
// Add moustache and wig sprites
private Sprite2D _moustacheSprite;
private Sprite2D _wigSprite;

public override void _Ready()
{
    // ... existing code ...

    // Load and position moustache
    _moustacheSprite = new Sprite2D();
    AddChild(_moustacheSprite);

    // Load and position wig
    _wigSprite = new Sprite2D();
    AddChild(_wigSprite);

    ApplyCosmetics();
}

public void ApplyCosmetics()
{
    // Apply existing hat and glasses
    // ... existing code ...

    // Apply moustache
    if (PlayerProfile.CurrentMoustache != "none")
    {
        _moustacheSprite.Texture = GD.Load<Texture2D>($"res://Assets/Sprites/Face/Moustaches/{PlayerProfile.CurrentMoustache}.png");
        _moustacheSprite.Visible = true;
    }
    else
    {
        _moustacheSprite.Visible = false;
    }

    // Apply wig
    if (PlayerProfile.CurrentWig != "none")
    {
        _wigSprite.Texture = GD.Load<Texture2D>($"res://Assets/Sprites/Face/Wigs/{PlayerProfile.CurrentWig}.png");
        _wigSprite.Visible = true;
    }
    else
    {
        _wigSprite.Visible = false;
    }
}
```

---

#### Step 5: Test Cosmetics (1-1.5 hours)
**Test Plan:**
1. Test all hat types - verify they render
2. Test all glasses types - verify they render
3. Test all moustache types - verify they render
4. Test all wig types - verify they render
5. Test combinations - verify they stack correctly
6. Verify persistence - saves and loads correctly
7. Test in-game - verify cosmetics appear on character

---

### Integration Checklist
- [ ] Update PlayerProfile.cs (add moustaches, wigs)
- [ ] Create moustache sprites (5 types)
- [ ] Create wig sprites (4 types)
- [ ] Update FaceCustomizationScreen.cs (add sections)
- [ ] Create grid-based UI for new cosmetics
- [ ] Add preview panel updates
- [ ] Update character rendering (Animal.cs)
- [ ] Test all cosmetic types
- [ ] Test combinations
- [ ] Verify persistence
- [ ] Test in-game display
- [ ] Update documentation

---

### Conflicts & Risks
**Low Risk:**
- Extension of existing system (not new feature)
- Self-contained cosmetic sprites
- No gameplay impact

**Potential Issues:**
- Requires many new sprite assets
- UI layout may need adjustment for 4 types
- Cosmetic positioning may overlap

**Mitigation:**
- Start with placeholder sprites
- Test positioning and layering
- Use z-index for proper layering
- Make optional cosmetics (can be disabled)

---

## OPTIONAL: LOW PRIORITY PORTS

### PORT 5: Enhanced Face Capture with Point Detection
**Priority:** LOW
**Estimated Effort:** 6-8 hours
**Value:** ⭐⭐ (Better face system)

**Implementation:**
- Add point detection for eyes and mouth
- Interactive point positioning
- Multi-step confirmation flow

**Decision:** Nice to have, but not critical

---

### PORT 6: Mobile Optimization (GL Compatibility)
**Priority:** LOW
**Estimated Effort:** 4-6 hours
**Value:** ⭐⭐ (Better mobile performance)

**Implementation:**
- Switch to GL Compatibility renderer
- Add touch emulation settings
- Responsive UI scaling

**Decision:** Test existing performance first; may not need

---

## TESTING PLAN

### Unit Testing (After Each Port)
1. Test new system in isolation
2. Verify integration with existing systems
3. Check for regressions
4. Performance benchmarking

### Integration Testing (After All Ports)
1. Play through all 100 levels
2. Test on multiple platforms (Windows, Android)
3. Stress test (100+ objects, multiple enemies)
4. Memory leak testing
5. Performance profiling

### User Acceptance Testing
1. Playability test
2. Visual quality test
3. Performance test
4. Bug testing

---

## ROLLBACK PLAN

If integration causes issues:

### Port 1 (Object Pooling)
- Disable ObjectPool autoload
- Revert to direct instantiation
- Keep code for later fix

### Port 2 (Enemies)
- Remove EnemySpawner from RoomBase
- Disable enemy scenes
- Keep code for later fix

### Port 3 (Animations)
- Remove animator from StickClone
- Fall back to static sprites
- Keep code for later fix

### Port 4 (Cosmetics)
- Hide new cosmetic sections
- Revert to 2-type system
- Keep assets and code for later fix

---

## EFFORT SUMMARY

| Port | Priority | Hours | Value | Risk |
|------|----------|--------|-------|------|
| **Object Pooling** | HIGH | 2-3 | ⭐⭐⭐⭐⭐ | Low |
| **Enemy AI System** | HIGH | 6-8 | ⭐⭐⭐⭐⭐ | Medium |
| **Animation System** | MEDIUM | 4-6 | ⭐⭐⭐⭐ | Low |
| **Advanced Cosmetics** | MEDIUM | 8-10 | ⭐⭐⭐⭐ | Low |
| **Enhanced Face Capture** | LOW | 6-8 | ⭐⭐ | Low |
| **Mobile Optimization** | LOW | 4-6 | ⭐⭐ | Low |

**Total High Priority:** 8-11 hours
**Total Medium Priority:** 12-16 hours
**Total All Ports:** 30-41 hours

**Recommended Minimum:** High Priority Only (8-11 hours)

---

## FINAL RECOMMENDATION

### Immediate Actions (Next 1-2 weeks)

1. **Port Object Pooling** (2-3 hours) - Quick win, significant performance boost
2. **Port Enemy AI System** (6-8 hours) - Major feature gap, high value

### Short-Term Actions (Next month)

3. **Port Animation System** (4-6 hours) - Professional-grade visuals
4. **Port Advanced Cosmetics** (8-10 hours) - Enhanced customization

### Future Actions (Optional)

5. **Port Enhanced Face Capture** (6-8 hours) - Nice to have
6. **Port Mobile Optimization** (4-6 hours) - If needed

---

## SUCCESS CRITERIA

Integration is successful when:

### Object Pooling:
- [x] Pool reduces instantiation overhead
- [x] Performance improves 20%+ in stress test
- [x] No memory leaks detected
- [x] Works with projectiles and rubble

### Enemy System:
- [x] Enemies spawn in levels
- [x] Enemies react to collisions
- [x] Enemy animations play correctly
- [x] Scoring includes enemy destruction
- [x] Enemy limit works correctly

### Animation System:
- [x] Character animates during gameplay
- [x] All animation states work (idle, walk, jump, climb)
- [x] Facing direction works
- [x] Transitions are smooth

### Cosmetics:
- [x] 4 cosmetic types available
- [x] All cosmetic types display correctly
- [x] Combinations work
- [x] Persistence works across sessions

---

**End of Integration Plan**
