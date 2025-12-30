using Godot;

/// <summary>
/// Main slingshot controller that manages projectile launching.
/// Handles drag input, visual feedback, and physics impulse application.
/// Supports cross-platform input (mouse/touch) with haptic feedback.
/// </summary>
public partial class Slingshot : Node2D
{
    [Signal] public delegate void ProjectileLaunchedEventHandler(Projectile projectile);

    [Export] private InputArea _inputArea;
    [Export] private TrajectoryDrawer _trajectoryDrawer;
    [Export] private Marker2D _projectileHolder;
    [Export] private Marker2D _restPosition;
    [Export] private bool _enableHapticFeedback = true;

    // Audio now managed by AudioManager globally

    private const float IMPULSE_MULT = 20.0f;
    private const float IMPULSE_MAX = 1200.0f;
    private static readonly Vector2 DRAG_LIM_MAX = new Vector2(0, 60);
    private static readonly Vector2 DRAG_LIM_MIN = new Vector2(-60, 0);

    private enum State { IDLE, DRAGGING }

    private State _state = State.IDLE;
    private Projectile? _currentProjectile;
    private Vector2 _dragStart = Vector2.Zero;
    private Vector2 _draggedVector = Vector2.Zero;
    private Vector2 _lastDraggedVector = Vector2.Zero;
    private bool _inputInitialized = false;

    public override void _Ready()
    {
        SetupInputMap();
        ConnectSignals();
    }

    private void SetupInputMap()
    {
        if (_inputInitialized) return;

        // Ensure input area has proper input setup
        if (_inputArea != null && _inputArea.HasMethod("SetupInputMap"))
        {
            _inputArea.Call("SetupInputMap");
        }

        _inputInitialized = true;
    }

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

        // Trigger haptic feedback on mobile devices
        if (_enableHapticFeedback)
        {
            TriggerHapticFeedback();
        }

        _currentProjectile.Launch(impulse);

        EmitSignal(SignalName.ProjectileLaunched, _currentProjectile);
        SignalManager.EmitOnAttemptMade();

        _currentProjectile = null;
    }

    private void TriggerHapticFeedback()
    {
        // Trigger vibration on mobile devices
        if (OS.GetName() == "Android" || OS.GetName() == "iOS")
        {
            Input.VibrateHandheld(50); // Vibrate for 50ms
        }
    }

    /// <summary>
    /// Gets the current drag vector for external use.
    /// </summary>
    public Vector2 GetDragVector() => _draggedVector;

    /// <summary>
    /// Gets the current drag state.
    /// </summary>
    public bool IsDragging() => _state == State.DRAGGING;

    /// <summary>
    /// Enables or disables haptic feedback.
    /// </summary>
    public void SetHapticFeedbackEnabled(bool enabled) => _enableHapticFeedback = enabled;
}
