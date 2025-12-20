using Godot;

/// <summary>
/// Represents an animal that the player launches from a "catapult".
/// Handles input (drag/release), physics intereactions, sound effects, and game events such as dying or leaving the screen.
/// </summary>
public partial class Animal : RigidBody2D
{
    /// <summary>
    /// States representing the animal's lifecycle during gameplay.
    /// </summary>
    public enum AnimalState { READY, DRAG, RELEASE }



    // --------------------------------------------------------------------------------
    // Constants
    // --------------------------------------------------------------------------------

    private const float IMPULSE_MULT = 20.0f; // Multiplier for launch impulse.
    private const float IMPULSE_MAX = 1200.0f; // Maximum impulse allowed.

    // Drag boundaries (defines how far the player can pull the animal).
    private static readonly Vector2 DRAG_LIM_MAX = new Vector2(0, 60);
    private static readonly Vector2 DRAG_LIM_MIN = new Vector2(-60, 0);



    // --------------------------------------------------------------------------------
    // Exported Fields (set in the godot editor).
    // --------------------------------------------------------------------------------

    [Export] Sprite2D _arrow;                      // Visual indicator for launch strength/direction
    [Export] AudioStreamPlayer2D _catapultSound;   // Sound when the animal is launched.
    [Export] AudioStreamPlayer2D _stretchSound;    // Sound when dragging/stretching.
    [Export] AudioStreamPlayer2D _kickWoodSound;   // Sound when colliding with obstacles.
    [Export] VisibleOnScreenNotifier2D _OnScreenNotifier; // Detects when the animal leaves the screen.



    // --------------------------------------------------------------------------------
    // Private State
    // --------------------------------------------------------------------------------
    

    private AnimalState _state = AnimalState.READY; // Current gameplay state.

    private Vector2 _initialPosition = Vector2.Zero; // Starting position before drag.
    private Vector2 _dragStart = Vector2.Zero;       // Position where drag began.
    private Vector2 _draggedVector = Vector2.Zero;   // Current drag vector.
    private Vector2 _lastDraggedVector = Vector2.Zero; // Previous drag vector.

    private float _arrowXScale = 0.0f;   // Original horizontal scale of the arrow.

    private int _lastCollisionCount = 0; // Used to detect first collision impact.

    


    // --------------------------------------------------------------------------------
    // Godot Lifecycle
    // --------------------------------------------------------------------------------

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        InitializeVariable();
        ConnectSignals();
    }


    // Called every physics frame.
    public override void _PhysicsProcess(double delta) => UpdateState(); // Updates the animal state logic depending on interaction.



    // --------------------------------------------------------------------------------
    // Initialization
    // --------------------------------------------------------------------------------

    /// <summary>
    /// Sets the initial values for variables and prepares the arrow indicator.
    /// </summary>
    private void InitializeVariable()
    {
        _initialPosition = Position;
        _arrowXScale = _arrow.Scale.X;
        _arrow.Visible = false;
    }


    /// <summary>
    /// Connects internal and Godot signals.
    /// </summary>
    private void ConnectSignals()
    {
        _OnScreenNotifier.ScreenExited += IsOffScreen;
        InputEvent += DetectDrag;
        SleepingStateChanged += ChangeSleepingState;
    }



    // --------------------------------------------------------------------------------
    // State Handling
    // --------------------------------------------------------------------------------

    /// <summary>
    /// Changes the animal's current state and triggers related behavior.
    /// </summary>
    /// <param name="newState">The new state to apply.</param>
    private void ChangeState(AnimalState newState)
    {
        _state = newState;

        switch (_state)
        {
            case AnimalState.DRAG: StartDragging(); break;
            case AnimalState.RELEASE: StartReleasing(); break;
            default: break;
        }
    }


    /// <summary>
    /// Updates the logic depending on the current state.
    /// </summary>
    private void UpdateState()
    {
        switch (_state)
        {
            case AnimalState.DRAG: HandleDragging(); break;
            case AnimalState.RELEASE: HandleFlight(); break;
            default: break;
        }
    }



    // --------------------------------------------------------------------------------
    // Dragging & Launching
    // --------------------------------------------------------------------------------

    /// <summary>
    /// Called when dragging starts. Shows the arrow indicator.
    /// </summary>
    private void StartDragging()
    {
        _dragStart = GetGlobalMousePosition();
        _arrow.Visible = true;
    }


    /// <summary>
    /// Called when releasing. Applies physics impulse and plays sound.
    /// </summary>
    private void StartReleasing()
    {
        Freeze = false;

        _arrow.Visible = false;
        _catapultSound.Play();

        ApplyCentralImpulse(CalculateImpulse());
        SignalManager.EmitOnAttemptMade();
    }


    /// <summary>
    /// Handles continuous updates while dragging.
    /// </summary>
    private void HandleDragging()
    {
        if (DetectRelease()) return;
        UpdateDraggedVector();
        PlayStretchSound();
        ConstrainDragWithinLimits();
        UpdateArrowScale();
    }


    /// <summary>
    /// Handles behavior after release (in flight).
    /// </summary>
    private void HandleFlight() => PlayKickSoundOnCollison();



    // --------------------------------------------------------------------------------
    // Dragging Helpers
    // --------------------------------------------------------------------------------

    /// <summary>
    /// Updates the dragged vector based on mouse position.
    /// </summary>
    private void UpdateDraggedVector() => _draggedVector = GetGlobalMousePosition() - _dragStart;


    /// <summary>
    /// Ensures the drag vector stays within allowed limits and updates position.
    /// </summary>
    private void ConstrainDragWithinLimits()
    {
        _lastDraggedVector = _draggedVector;
        _draggedVector = _draggedVector.Clamp(DRAG_LIM_MIN, DRAG_LIM_MAX);

        Position = _initialPosition + _draggedVector;
    }


    /// <summary>
    /// Updates the arrow scale and rotation to reflect drag strength and direction.
    /// </summary>
    private void UpdateArrowScale()
    {
        float impulseLength = CalculateImpulse().Length();
        float scaleFactor = impulseLength / IMPULSE_MAX;

        _arrow.Scale = new Vector2((_arrowXScale * scaleFactor) + _arrowXScale, _arrow.Scale.Y);
        _arrow.Rotation = (_dragStart - Position).Angle();
    }



    // --------------------------------------------------------------------------------
    // Sound & Feedback
    // --------------------------------------------------------------------------------

    /// <summary>
    /// Plays the kick sound on the first collision during flight.
    /// </summary>
    private void PlayKickSoundOnCollison()
    {
        if ((_lastCollisionCount == 0) && (GetContactCount() > 0) && (!_kickWoodSound.Playing)) 
        { 
            _kickWoodSound.Play(); 
        }

        _lastCollisionCount = GetContactCount();
    }


    /// <summary>
    /// Plays the stretch sound if dragging distance increases.
    /// </summary>
    private void PlayStretchSound()
    {
        Vector2 diff = _draggedVector - _lastDraggedVector;

        if (diff.Length() > 0 && !_stretchSound.Playing) _stretchSound.Play();
    }



    // --------------------------------------------------------------------------------
    // Input & Physics
    // --------------------------------------------------------------------------------

    /// <summary>
    /// Calculates the inpulse vector applied to the animal when launched.
    /// </summary>
    /// <returns>The launch impulse vector.</returns>
    private Vector2 CalculateImpulse() => _draggedVector * -IMPULSE_MULT;


    /// <summary>
    /// Detectis when the drag action starts.
    /// </summary>
    /// <param name="viewport"></param>
    /// <param name="event"></param>
    /// <param name="shapeIdx"></param>
    private void DetectDrag(Node viewport, InputEvent @event, long shapeIdx)
    {
        if (_state == AnimalState.READY && @event.IsActionPressed("drag"))
        {
            ChangeState(AnimalState.DRAG);
        }
    }


    /// <summary>
    /// Detects when the drag input is released.
    /// </summary>
    /// <returns>True if release was detected, otherwase false.</returns>
    private bool DetectRelease()
    {
        if (_state == AnimalState.DRAG && Input.IsActionJustReleased("drag"))
        {
            ChangeState(AnimalState.RELEASE);
            return true;
        }

        return false;
    }


    /// <summary>
    /// Triggered when the animal gors to "sleeping" state (physics resting).
    /// Kills any cups it collied with and destroys itself.
    /// </summary>
    private void ChangeSleepingState()
    {
        if (Sleeping)
        {
            foreach (Node2D body in GetCollidingBodies())
            {
                if (body is Cup cup)
                {
                    cup.Die();
                }
            }

            Die();
        }
    }


    /// <summary>
    /// Triggered when the animal leaves the screen.
    /// </summary>
    private void IsOffScreen() => Die();


    /// <summary>
    /// Destroys the animal and emits the death signal.
    /// </summary>
    private void Die()
    {
        SignalManager.EmitOnAnimalDied();
        QueueFree();
    }


    /// <summary>
    /// Gets the current state of the animal.
    /// </summary>
    /// <returns>The current state.</returns>
    public AnimalState GetState() => _state;


    /// <summary>
    /// Sets the animal's position.
    /// </summary>
    /// <param name="position">The new position.</param>
    public void SetPosition(Vector2 position)
    {
        Position = position;
        _initialPosition = position;
    }


    /// <summary>
    /// Resets the animal to its initial state.
    /// </summary>
    public void Reset()
    {
        Position = _initialPosition;
        _state = AnimalState.READY;
        _arrow.Visible = false;
        Freeze = true;
    }
}