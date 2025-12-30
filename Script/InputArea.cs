using Godot;

/// <summary>
/// Invisible input area for detecting drag interactions on the slingshot.
/// Handles both mouse and touch input for cross-platform compatibility.
/// </summary>
public partial class InputArea : Area2D
{
    [Signal] public delegate void DragStartedEventHandler();
    [Signal] public delegate void DragEndedEventHandler();

    private bool _isDragging = false;
    private Vector2 _dragStartPosition;
    private bool _inputInitialized = false;

    public override void _Ready()
    {
        InputEvent += OnInputEvent;
        SetupInputMap();
    }

    private void SetupInputMap()
    {
        if (_inputInitialized) return;

        // Ensure "drag" action exists in InputMap for cross-platform input
        if (!InputMap.HasAction("drag"))
        {
            InputMap.AddAction("drag");

            // Add mouse/touch input for drag action
            var mouseEvent = new InputEventMouseButton
            {
                ButtonIndex = MouseButton.Left,
                Pressed = false
            };
            InputMap.ActionAddEvent("drag", mouseEvent);

            // Add touch input for mobile support
            var touchEvent = new InputEventScreenTouch
            {
                Pressed = false
            };
            InputMap.ActionAddEvent("drag", touchEvent);
        }

        _inputInitialized = true;
    }

    public override void _Process(double delta)
    {
        // Handle drag end via input release
        if (_isDragging)
        {
            bool inputReleased = Input.IsActionJustReleased("drag");
            if (inputReleased)
            {
                _isDragging = false;
                EmitSignal(SignalName.DragEnded);
            }
        }
    }

    private void OnInputEvent(Node viewport, InputEvent @event, long shapeIdx)
    {
        if (@event is InputEventMouseButton mouseButton)
        {
            HandleMouseInput(mouseButton);
        }
        else if (@event is InputEventScreenTouch screenTouch)
        {
            HandleTouchInput(screenTouch);
        }
        else if (@event is InputEventMouseMotion motionEvent && _isDragging)
        {
            // Allow drag tracking via motion events for smooth input
        }
    }

    private void HandleMouseInput(InputEventMouseButton mouseEvent)
    {
        if (mouseEvent.ButtonIndex != MouseButton.Left)
            return;

        if (mouseEvent.Pressed)
        {
            _isDragging = true;
            _dragStartPosition = mouseEvent.Position;
            EmitSignal(SignalName.DragStarted);
        }
    }

    private void HandleTouchInput(InputEventScreenTouch screenTouch)
    {
        if (screenTouch.Pressed)
        {
            _isDragging = true;
            _dragStartPosition = screenTouch.Position;
            EmitSignal(SignalName.DragStarted);
        }
    }

    /// <summary>
    /// Returns whether a drag operation is currently in progress.
    /// </summary>
    public bool IsDragging() => _isDragging;

    /// <summary>
    /// Gets the starting position of the current drag operation.
    /// </summary>
    public Vector2 GetDragStartPosition() => _dragStartPosition;
}
