using Godot;

/// <summary>
/// Main slingshot controller that manages projectile launching.
/// Handles drag input, visual feedback, and physics impulse application.
/// </summary>
public partial class Slingshot : Node2D
{
    [Signal] public delegate void ProjectileLaunchedEventHandler(Projectile projectile);

    [ExportGroup("Slingshot Configuration")]
    [Export] private InputArea _inputArea;
    [Export] private TrajectoryDrawer _trajectoryDrawer;
    [Export] private Marker2D _projectileHolder;
    [Export] private Marker2D _restPosition;
    [Export] private Node2D? _visualMesh;

    [ExportGroup("Slingshot Type Settings")]
    [Export] private SlingshotType _slingshotType = SlingshotType.Catapult;

    [ExportGroup("Animation Settings")]
    [Export] private float _launchAnimationDuration = 0.3f;
    [Export] private float _squishScale = 0.7f;
    [Export] private float _stretchScale = 1.3f;

    // Audio now managed by AudioManager globally
    
    private const float IMPULSE_MULT = 20.0f;
    private const float IMPULSE_MAX = 1200.0f;
    private static readonly Vector2 DRAG_LIM_MAX = new Vector2(0, 60);
    private static readonly Vector2 DRAG_LIM_MIN = new Vector2(-60, 0);
    
    private enum State { IDLE, DRAGGING }
    
    private State _state = State.IDLE;
    private Projectile _currentProjectile = null;
    private Vector2 _dragStart = Vector2.Zero;
    private Vector2 _draggedVector = Vector2.Zero;
    private Vector2 _lastDraggedVector = Vector2.Zero;
    
    public override void _Ready()
    {
        // Load slingshot type from PlayerProfile
        if (PlayerProfile.Instance != null)
        {
            _slingshotType = (SlingshotType)PlayerProfile.GetSlingshotType();
        }

        ConnectSignals();
    }

    /// <summary>
    /// Sets the slingshot type for visual variations.
    /// Each type has unique particle effects and animations.
    /// </summary>
    public void SetSlingshotType(SlingshotType type)
    {
        _slingshotType = type;
    }

    /// <summary>
    /// Gets the current slingshot type.
    /// </summary>
    public SlingshotType GetSlingshotType() => _slingshotType;
    
    public override void _PhysicsProcess(double delta)
    {
        if (_state == State.DRAGGING)
        {
            UpdateDragging();
        }
    }
    
    private void ConnectSignals()
    {
        if (_inputArea != null)
        {
            _inputArea.DragStarted += OnDragStarted;
            _inputArea.DragEnded += OnDragEnded;
        }
    }

    public override void _ExitTree()
    {
        // Disconnect all signals to prevent memory leaks
        if (_inputArea != null)
        {
            _inputArea.DragStarted -= OnDragStarted;
            _inputArea.DragEnded -= OnDragEnded;
        }
    }
    
    public void LoadProjectile(Projectile projectile)
    {
        _currentProjectile = projectile;
        
        if (_projectileHolder != null && _currentProjectile != null)
        {
            _currentProjectile.GlobalPosition = _projectileHolder.GlobalPosition;
        }
    }
    
    private void OnDragStarted()
    {
        if (_currentProjectile == null) return;
        
        _state = State.DRAGGING;
        _dragStart = GetGlobalMousePosition();
        
        if (_trajectoryDrawer != null)
        {
            _trajectoryDrawer.ShowTrajectory(Vector2.Zero, Vector2.Zero);
        }
    }
    
    private void OnDragEnded()
    {
        if (_state != State.DRAGGING || _currentProjectile == null) return;
        
        _state = State.IDLE;
        
        if (_trajectoryDrawer != null)
        {
            _trajectoryDrawer.HideTrajectory();
        }
        
        LaunchProjectile();
    }
    
    private void UpdateDragging()
    {
        if (_currentProjectile == null) return;
        
        UpdateDraggedVector();
        PlayStretchSound();
        ConstrainDragWithinLimits();
        
        Vector2 impulse = CalculateImpulse();
        
        if (_trajectoryDrawer != null)
        {
            _trajectoryDrawer.ShowTrajectory(_draggedVector, impulse);
        }
    }
    
    private void UpdateDraggedVector()
    {
        _draggedVector = GetGlobalMousePosition() - _dragStart;
    }
    
    private void ConstrainDragWithinLimits()
    {
        if (_currentProjectile == null) return;
        
        _lastDraggedVector = _draggedVector;
        _draggedVector = _draggedVector.Clamp(DRAG_LIM_MIN, DRAG_LIM_MAX);
        
        _currentProjectile.GlobalPosition = _dragStart + _draggedVector;
    }
    
    private Vector2 CalculateImpulse()
    {
        Vector2 impulse = _draggedVector * -IMPULSE_MULT;
        
        if (impulse.Length() > IMPULSE_MAX)
        {
            impulse = impulse.Normalized() * IMPULSE_MAX;
        }
        
        return impulse;
    }
    
    private void PlayStretchSound()
    {
        Vector2 diff = _draggedVector - _lastDraggedVector;
        
        if (diff.Length() > 0)
        {
            // Use AudioManager for stretch sound
            var audioManager = GetNodeOrNull<AudioManager>("/root/AudioManager");
            audioManager?.PlaySlingshotSound();
        }
    }
    
    private void LaunchProjectile()
    {
        if (_currentProjectile == null) return;

        Vector2 impulse = CalculateImpulse();

        // Use AudioManager for catapult sound
        var audioManager = GetNodeOrNull<AudioManager>("/root/AudioManager");
        audioManager?.PlaySlingshotSound();

        // Play launch vocal
        AudioManager.PlayLaunchVocalSfx();

        // Trigger slingshot type-specific launch animation
        PlayLaunchAnimation();

        // Trigger slingshot type-specific particle effects via GameFeelManager
        if (GameFeelManager.Instance != null)
        {
            GameFeelManager.Instance.OnSlingshotLaunched(_slingshotType, _projectileHolder?.GlobalPosition ?? GlobalPosition);
        }

        // Add game feel launch feedback
        GameFeelManager.Instance?.OnProjectileLaunched();

        // Spawn speech bubble on launch if we have a FaceProjectile
        if (SpeechBubbleManager.Instance != null && _currentProjectile is FaceProjectile faceProjectile)
        {
            SpeechBubbleManager.Instance.SetCurrentProjectile(faceProjectile);
            SpeechBubbleManager.Instance.OnLaunch();
        }

        _currentProjectile.Launch(impulse);

        EmitSignal(SignalName.ProjectileLaunched, _currentProjectile);
        SignalManager.EmitOnAttemptMade();

        _currentProjectile = null;
    }

    private void PlayLaunchAnimation()
    {
        if (_visualMesh == null) return;

        var tween = CreateTween();
        tween.SetParallel(true);

        // Different animations based on slingshot type
        switch (_slingshotType)
        {
            case SlingshotType.GiantHand:
                // Giant hand squishes down then bounces back
                tween.TweenProperty(_visualMesh, "scale", new Vector2(_squishScale, _squishScale), _launchAnimationDuration * 0.3f)
                    .SetTrans(Tween.TransitionType.Elastic);
                tween.TweenProperty(_visualMesh, "scale", Vector2.One, _launchAnimationDuration * 0.7f)
                    .SetTrans(Tween.TransitionType.Elastic)
                    .SetDelay(_launchAnimationDuration * 0.3f);
                break;

            case SlingshotType.Trebuchet:
                // Trebuchet spins/rotates slightly
                tween.TweenProperty(_visualMesh, "rotation", Mathf.DegToRad(-15f), _launchAnimationDuration * 0.4f)
                    .SetTrans(Tween.TransitionType.Back);
                tween.TweenProperty(_visualMesh, "rotation", 0f, _launchAnimationDuration * 0.6f)
                    .SetTrans(Tween.TransitionType.Bounce)
                    .SetDelay(_launchAnimationDuration * 0.4f);
                break;

            case SlingshotType.Spring:
                // Spring compresses and releases
                tween.TweenProperty(_visualMesh, "scale", new Vector2(_stretchScale, _squishScale), _launchAnimationDuration * 0.25f)
                    .SetTrans(Tween.TransitionType.Quad);
                tween.TweenProperty(_visualMesh, "scale", Vector2.One, _launchAnimationDuration * 0.75f)
                    .SetTrans(Tween.TransitionType.Bounce)
                    .SetDelay(_launchAnimationDuration * 0.25f);
                break;

            case SlingshotType.Catapult:
            default:
                // Classic catapult - slight stretch then bounce back
                tween.TweenProperty(_visualMesh, "scale", new Vector2(_stretchScale, _squishScale), _launchAnimationDuration * 0.2f)
                    .SetTrans(Tween.TransitionType.Quad);
                tween.TweenProperty(_visualMesh, "scale", Vector2.One, _launchAnimationDuration * 0.8f)
                    .SetTrans(Tween.TransitionType.Elastic)
                    .SetDelay(_launchAnimationDuration * 0.2f);
                break;
        }
    }
}
