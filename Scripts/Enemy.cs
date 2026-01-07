using Godot;

/// <summary>
/// Base class for all enemies in the game.
/// Handles physics-based destruction when hit by projectiles.
/// Ported from Angry Aliens Enemy.gd
/// </summary>
public partial class Enemy : RigidBody2D
{
	[Signal] public delegate void DestroyedEventHandler(Node enemy, Node collider, Vector2 impactMomentum);

	private const float DestroyThresholdByObstacles = 400f;
	private const float DestroyThreshold = 1600f;

	public override void _IntegrateForces(PhysicsDirectBodyState2D state)
	{
		for (int i = 0; i < state.GetContactCount(); i++)
		{
			var collider = state.GetContactColliderObject(i);
			if (collider is RigidBody2D rigidBody)
			{
				var impactMomentum = rigidBody.Mass * rigidBody.LinearVelocity - Mass * LinearVelocity;
				if (impactMomentum.Length() >= GetDestructionThreshold(rigidBody))
				{
					EmitSignal(SignalName.Destroyed, new Godot.Collections.Array() { this, collider, impactMomentum });
					OnDestroyed();
				}
			}
		}
	}

	/// <summary>
	/// Gets the destruction threshold based on the collider type.
	/// Different objects have different impact requirements.
	/// </summary>
	protected virtual float GetDestructionThreshold(RigidBody2D colliderType)
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

	/// <summary>
	/// Called when enemy is destroyed. Override in subclasses.
	/// </summary>
	protected virtual void OnDestroyed()
	{
		QueueFree();
	}
}
