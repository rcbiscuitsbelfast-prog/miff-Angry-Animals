using Godot;

/// <summary>
/// StickClone is the playable character in the traversal phase.
/// Implements CharacterBody2D movement with walking, jumping, and climbing mechanics.
/// Customizable with face cosmetics set during spawn after the projectile phase.
/// </summary>
public partial class StickClone : CharacterBody2D
{
	/// <summary>
	/// Movement states for StickClone during traversal.
	/// </summary>
	public enum MovementState { IDLE, WALKING, JUMPING, CLIMBING, FALLING }

	// Configuration
	[Export] float _walkSpeed = 200.0f;
	[Export] float _jumpForce = -400.0f;
	[Export] float _gravity = 800.0f;
	[Export] float _climbSpeed = 150.0f;
	[Export] float _maxFallSpeed = 400.0f;

	// Face customization
	[Export] Sprite2D _faceSprite;

	private MovementState _currentState = MovementState.IDLE;
	private Vector2 _velocity = Vector2.Zero;
	private bool _isOnFloor = false;
	private bool _isClimbing = false;
	private Vector2 _faceTexture = Vector2.Zero; // Store face cosmetic if needed

	// Signals for progression
	[Signal]
	public delegate void OnReachedExitEventHandler();

	[Signal]
	public delegate void OnDiedEventHandler();

	public override void _Ready()
	{
		if (this is CharacterBody2D)
		{
			// CharacterBody2D is ready for physics operations
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		HandleInput();
		UpdateMovement((float)delta);
		UpdateState();
	}

	/// <summary>
	/// Handles input for movement control.
	/// </summary>
	private void HandleInput()
	{
		float horizontalInput = 0;

		if (Input.IsActionPressed("ui_left"))
		{
			horizontalInput = -1;
		}
		else if (Input.IsActionPressed("ui_right"))
		{
			horizontalInput = 1;
		}

		if (horizontalInput != 0)
		{
			_velocity.X = horizontalInput * _walkSpeed;
			_currentState = MovementState.WALKING;
		}
		else
		{
			_velocity.X = 0;
			if (_isOnFloor && !_isClimbing)
			{
				_currentState = MovementState.IDLE;
			}
		}

		if (Input.IsActionJustPressed("ui_accept") && _isOnFloor && !_isClimbing)
		{
			_velocity.Y = _jumpForce;
			_currentState = MovementState.JUMPING;
		}
	}

	/// <summary>
	/// Updates movement based on current velocity and state.
	/// </summary>
	private void UpdateMovement(float delta)
	{
		// Apply gravity if not climbing
		if (!_isClimbing && !_isOnFloor)
		{
			_velocity.Y += _gravity * delta;
			if (_velocity.Y > _maxFallSpeed)
			{
				_velocity.Y = _maxFallSpeed;
			}
			_currentState = MovementState.FALLING;
		}

		// Limit falling speed
		if (_velocity.Y > _maxFallSpeed)
		{
			_velocity.Y = _maxFallSpeed;
		}

		// Move the character
		Velocity = _velocity;
		MoveAndSlide();

		// Check if on floor
		_isOnFloor = IsOnFloor();

		// Reset vertical velocity if on floor
		if (_isOnFloor && _velocity.Y > 0)
		{
			_velocity.Y = 0;
		}
	}

	/// <summary>
	/// Updates the current state based on velocity and floor contact.
	/// </summary>
	private void UpdateState()
	{
		if (_isOnFloor && _velocity.X == 0)
		{
			_currentState = MovementState.IDLE;
		}
		else if (_isOnFloor && _velocity.X != 0)
		{
			_currentState = MovementState.WALKING;
		}
		else if (!_isOnFloor && _velocity.Y < 0)
		{
			_currentState = MovementState.JUMPING;
		}
		else if (!_isOnFloor && _velocity.Y > 0)
		{
			_currentState = MovementState.FALLING;
		}
	}

	/// <summary>
	/// Sets the face customization for StickClone.
	/// </summary>
	/// <param name="faceResourcePath">Path to the face texture resource.</param>
	public void SetFaceCustomization(string faceResourcePath)
	{
		if (_faceSprite != null && !string.IsNullOrEmpty(faceResourcePath))
		{
			try
			{
				Texture2D faceTexture = GD.Load<Texture2D>(faceResourcePath);
				if (faceTexture != null)
				{
					_faceSprite.Texture = faceTexture;
				}
			}
			catch
			{
				GD.PrintErr($"Failed to load face texture: {faceResourcePath}");
			}
		}
	}

	/// <summary>
	/// Handles collision with exit door to signal completion.
	/// </summary>
	public void TriggerExitDoor()
	{
		EmitSignal(SignalName.OnReachedExit);
	}

	/// <summary>
	/// Handles StickClone death (e.g., falling off screen or hazard).
	/// </summary>
	public void Die()
	{
		EmitSignal(SignalName.OnDied);
		QueueFree();
	}

	/// <summary>
	/// Gets the current movement state.
	/// </summary>
	public MovementState GetCurrentState() => _currentState;

	/// <summary>
	/// Enables climbing mode (e.g., on ladders or vines).
	/// </summary>
	public void EnableClimbing()
	{
		_isClimbing = true;
		_velocity.Y = 0;

		if (Input.IsActionPressed("ui_up"))
		{
			_velocity.Y = -_climbSpeed;
		}
		else if (Input.IsActionPressed("ui_down"))
		{
			_velocity.Y = _climbSpeed;
		}

		_currentState = MovementState.CLIMBING;
	}

	/// <summary>
	/// Disables climbing mode.
	/// </summary>
	public void DisableClimbing()
	{
		_isClimbing = false;
	}
}
