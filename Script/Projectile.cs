using Godot;

/// <summary>
/// Base class for all projectiles launched from the slingshot.
/// Handles physics interactions, collision detection, sound effects, and lifecycle management.
/// </summary>
public partial class Projectile : RigidBody2D
{
	[Signal] public delegate void AlmostStoppedEventHandler();
	
	[Export] private AudioStreamPlayer2D _kickWoodSound;
	[Export] private VisibleOnScreenNotifier2D _onScreenNotifier;
	
	private const float STOPPED_THRESHOLD = 0.1f;
	private bool _hasBeenLaunched = false;
	private bool _almostStoppedEmitted = false;
	private int _lastCollisionCount = 0;
	
	public override void _Ready()
	{
		ConnectSignals();
	}
	
	public override void _PhysicsProcess(double delta)
	{
		CheckIfAlmostStopped();
	}
	
	private void ConnectSignals()
	{
		if (_onScreenNotifier != null)
		{
			_onScreenNotifier.ScreenExited += OnScreenExited;
		}
		
		SleepingStateChanged += OnSleepingStateChanged;
	}
	
	public void Launch(Vector2 impulse)
	{
		_hasBeenLaunched = true;
		Freeze = false;
		ApplyCentralImpulse(impulse);
	}
	
	private void CheckIfAlmostStopped()
	{
		if (!_hasBeenLaunched || _almostStoppedEmitted) return;
		
		if (LinearVelocity.Length() < STOPPED_THRESHOLD)
		{
			_almostStoppedEmitted = true;
			EmitSignal(SignalName.AlmostStopped);
		}
	}
	
	public override void _IntegrateForces(PhysicsDirectBodyState2D state)
	{
		if (_hasBeenLaunched && _kickWoodSound != null)
		{
			int contactCount = state.GetContactCount();
			if (_lastCollisionCount == 0 && contactCount > 0 && !_kickWoodSound.Playing)
			{
				_kickWoodSound.Play();
			}
			_lastCollisionCount = contactCount;
		}
	}
	
	private void OnSleepingStateChanged()
	{
		if (Sleeping && _hasBeenLaunched)
		{
			foreach (Node2D body in GetCollidingBodies())
			{
				if (body is Cup cup)
				{
					cup.Die();
				}
			}
			
			CallDeferred(MethodName.Die);
		}
	}
	
	private void OnScreenExited()
	{
		if (_hasBeenLaunched)
		{
			Die();
		}
	}
	
	private void Die()
	{
		SignalManager.EmitOnAnimalDied();
		QueueFree();
	}
}
