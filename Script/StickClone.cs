using Godot;

/// <summary>
/// StickClone character that spawns during traversal phase after slingshot destruction.
/// Carries face customization from PlayerProfile and moves through the environment.
/// </summary>
public partial class StickClone : CharacterBody2D
{
    [Signal] public delegate void CloneReachedExitEventHandler();
    [Signal] public delegate void CloneStuckEventHandler();

    [Export] private float _moveSpeed = 150f;
    [Export] private float _jumpForce = -400f;
    [Export] private float _gravity = 980f;
    [Export] private float _friction = 0.8f;
    [Export] private NodePath _faceSpritePath;
    [Export] private NodePath _hatNodePath;
    [Export] private NodePath _glassesNodePath;

    private Sprite2D? _faceSprite;
    private Node2D? _hatNode;
    private Node2D? _glassesNode;

    // Movement state
    private Vector2 _velocity = Vector2.Zero;
    private bool _isGrounded = false;
    private bool _isMoving = false;

    // Face customization from PlayerProfile
    private string _currentHat = "none";
    private string _currentGlasses = "none";
    private string _currentEmotion = "neutral";

    // References to room systems
    private RoomBase? _currentRoom;
    private Area2D? _exitArea;

    public override void _Ready()
    {
        InitializeStickClone();
        LoadFaceCustomization();
        SetupPhysics();
        ConnectSignals();
    }

    private void InitializeStickClone()
    {
        _faceSprite = GetNodeOrNull<Sprite2D>(_faceSpritePath);
        _hatNode = GetNodeOrNull<Node2D>(_hatNodePath);
        _glassesNode = GetNodeOrNull<Node2D>(_glassesNodePath);

        // Get reference to current room
        _currentRoom = GetParent()?.GetParent() as RoomBase;
        
        // Find exit area in the scene
        _exitArea = GetNodeOrNull<Area2D>("../ExitArea") ?? GetNodeOrNull<Area2D>("ExitArea");
        
        // Set up collision layers
        CollisionLayer = 1; // StickClone layer
        CollisionMask = 2;  // Environment layer
    }

    private void LoadFaceCustomization()
    {
        if (PlayerProfile.Instance == null)
            return;

        // Load customization from PlayerProfile
        _currentHat = PlayerProfile.GetHats()[PlayerProfile.Instance.SelectedHatIndex];
        _currentGlasses = PlayerProfile.GetGlasses()[PlayerProfile.Instance.SelectedGlassesIndex];
        _currentEmotion = PlayerProfile.GetEmotions()[PlayerProfile.Instance.SelectedEmotionIndex];

        ApplyFaceCustomization();
    }

    private void ApplyFaceCustomization()
    {
        // Apply hat
        if (_hatNode != null)
        {
            ApplyHat(_currentHat);
        }

        // Apply glasses
        if (_glassesNode != null)
        {
            ApplyGlasses(_currentGlasses);
        }

        // Apply emotion to face
        if (_faceSprite != null)
        {
            ApplyEmotion(_currentEmotion);
        }

        GD.Print($"StickClone spawned with: Hat={_currentHat}, Glasses={_currentGlasses}, Emotion={_currentEmotion}");
    }

    private void ApplyHat(string hatType)
    {
        if (_hatNode == null)
            return;

        // TODO: Load and apply hat sprites based on hatType
        switch (hatType.ToLower())
        {
            case "cap":
                _hatNode.Visible = true;
                // Load cap sprite
                break;
            case "crown":
                _hatNode.Visible = true;
                // Load crown sprite
                break;
            case "beanie":
                _hatNode.Visible = true;
                // Load beanie sprite
                break;
            default:
                _hatNode.Visible = false;
                break;
        }
    }

    private void ApplyGlasses(string glassesType)
    {
        if (_glassesNode == null)
            return;

        // TODO: Load and apply glasses sprites based on glassesType
        switch (glassesType.ToLower())
        {
            case "round":
            case "aviator":
                _glassesNode.Visible = true;
                // Load glasses sprite
                break;
            default:
                _glassesNode.Visible = false;
                break;
        }
    }

    private void ApplyEmotion(string emotionType)
    {
        if (_faceSprite == null)
            return;

        // TODO: Load and apply emotion sprites
        string texturePath = $"res://Assets/Sprites/Face/face_{emotionType}.png";
        if (ResourceLoader.Exists(texturePath))
        {
            var texture = ResourceLoader.Load<Texture2D>(texturePath);
            _faceSprite.Texture = texture;
        }
    }

    private void SetupPhysics()
    {
        // Set up the physics material for better collision response
        var physicsMaterial = new PhysicsMaterial();
        physicsMaterial.Friction = 1.0f;
        physicsMaterial.Bounce = 0.0f;
        
        // Apply to collision shapes if they exist
        var collisionShapes = GetTree().GetNodesInGroup("collision_shapes");
        foreach (var shape in collisionShapes)
        {
            if (shape is CollisionShape2D collisionShape)
            {
                collisionShape.PhysicsMaterialOverride = physicsMaterial;
            }
        }
    }

    private void ConnectSignals()
    {
        // Connect to exit area if found
        if (_exitArea != null)
        {
            _exitArea.BodyEntered += OnExitAreaBodyEntered;
        }

        // Connect to SignalManager for gameplay events
        if (SignalManager.Instance != null)
        {
            SignalManager.Instance.OnAnimalDied += OnAnimalDied;
        }
    }

    public override void _ExitTree()
    {
        // Disconnect signals
        if (_exitArea != null)
        {
            _exitArea.BodyEntered -= OnExitAreaBodyEntered;
        }

        if (SignalManager.Instance != null)
        {
            SignalManager.Instance.OnAnimalDied -= OnAnimalDied;
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        ApplyGravity(delta);
        HandleMovement(delta);
        MoveAndSlide();
        UpdateAnimationState();
    }

    private void ApplyGravity(double delta)
    {
        if (!_isGrounded)
        {
            _velocity.Y += _gravity * (float)delta;
        }
    }

    private void HandleMovement(double delta)
    {
        // Simple AI for movement towards exit
        if (_exitArea != null && GlobalPosition.DistanceTo(_exitArea.GlobalPosition) > 50)
        {
            Vector2 direction = (_exitArea.GlobalPosition - GlobalPosition).Normalized();
            
            // Move towards exit
            _velocity.X = direction.X * _moveSpeed;
            
            // Simple jump logic - jump if there's an obstacle ahead
            if (_isGrounded && ShouldJump())
            {
                Jump();
            }
        }
        else
        {
            // At exit, stop moving
            _velocity.X = 0;
            if (_isGrounded)
            {
                // We've reached the exit!
                OnReachedExit();
            }
        }

        // Apply friction when on ground and not moving
        if (_isGrounded && Mathf.Abs(_velocity.X) < 10)
        {
            _velocity.X *= _friction;
        }
    }

    private bool ShouldJump()
    {
        // Simple obstacle detection - raycast ahead
        var spaceState = GetWorld2D().DirectSpaceState;
        var query = PhysicsRayQueryParameters2D.Create(GlobalPosition, GlobalPosition + Vector2.Right * 32);
        query.CollisionMask = 2; // Environment layer
        
        var result = spaceState.IntersectRay(query);
        return result.Count > 0;
    }

    private void Jump()
    {
        _velocity.Y = _jumpForce;
        _isGrounded = false;
    }

    private void UpdateAnimationState()
    {
        _isMoving = Mathf.Abs(_velocity.X) > 10;
        
        // TODO: Update animation based on movement state
        // For now, just flip sprite based on direction
        if (_faceSprite != null)
        {
            _faceSprite.FlipH = _velocity.X < 0;
        }
    }

    private void OnExitAreaBodyEntered(Node2D body)
    {
        if (body == this)
        {
            OnReachedExit();
        }
    }

    private void OnAnimalDied()
    {
        // An animal (projectile) died, start StickClone movement
        GD.Print("StickClone starting traversal phase");
        StartTraversalMovement();
    }

    private void StartTraversalMovement()
    {
        // Begin moving towards the exit
        _isMoving = true;
    }

    private void OnReachedExit()
    {
        GD.Print("StickClone reached exit door!");
        EmitSignal(SignalName.CloneReachedExit);
        
        // Tell the room we've reached the exit
        _currentRoom?.OnExitReached();
        
        // Queue_free to remove from scene
        QueueFree();
    }

    /// <summary>
    /// Sets the StickClone to move towards a specific position (e.g., exit door).
    /// </summary>
    /// <param name="targetPosition">Position to move towards</param>
    public void SetTargetPosition(Vector2 targetPosition)
    {
        // TODO: Implement specific target positioning
        GD.Print($"StickClone targeting position: {targetPosition}");
    }

    /// <summary>
    /// Gets the current face customization info.
    /// </summary>
    /// <returns>Dictionary with customization info</returns>
    public Dictionary<string, string> GetFaceCustomization()
    {
        return new Dictionary<string, string>
        {
            { "hat", _currentHat },
            { "glasses", _currentGlasses },
            { "emotion", _currentEmotion }
        };
    }

    /// <summary>
    /// Sets the movement speed of the StickClone.
    /// </summary>
    /// <param name="speed">Movement speed</param>
    public void SetMoveSpeed(float speed)
    {
        _moveSpeed = Mathf.Max(50f, speed);
    }

    private void OnGroundStateChanged(bool grounded)
    {
        _isGrounded = grounded;
    }
}