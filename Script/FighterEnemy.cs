using Godot;

/// <summary>
/// Advanced animated enemy with health system and animations.
/// Ported from Angry Aliens FighterEnemy.gd
/// </summary>
public partial class FighterEnemy : Enemy
{
	[Export] public int Health { get; set; } = 100;
	[Export] public int DamageThreshold { get; set; } = 800;

	private AnimationState _currentAnimation = AnimationState.Idle;
	private Sprite2D _sprite;
	private AnimationPlayer _animationPlayer;

	public enum AnimationState
	{
		Idle,
		Hit,
		Death,
		Attack
	}

	public override void _Ready()
	{
		base._Ready();

		_sprite = GetNodeOrNull<Sprite2D>("Sprite2D");
		_animationPlayer = GetNodeOrNull<AnimationPlayer>("AnimationPlayer");

		if (_sprite != null)
		{
			SetupAnimations();
		}

		PlayAnimation(AnimationState.Idle);
	}

	private void SetupAnimations()
	{
		// For now, we'll use placeholder animations
		// When sprite sheets are available, uncomment and configure:
		// SetupIdleAnimation();
		// SetupHitAnimation();
		// SetupDeathAnimation();

		GD.Print("FighterEnemy: SetupAnimations called. Add sprite sheet assets for full animations.");
	}

	/// <summary>
	/// Plays an animation state.
	/// </summary>
	public void PlayAnimation(AnimationState state)
	{
		if (_animationPlayer == null) return;

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
				break;

			case AnimationState.Death:
				if (_animationPlayer.HasAnimation("death"))
					_animationPlayer.Play("death");
				break;

			case AnimationState.Attack:
				if (_animationPlayer.HasAnimation("attack"))
					_animationPlayer.Play("attack");
				break;
		}
	}

	/// <summary>
	/// Takes damage from a hit.
	/// </summary>
	public void TakeDamage(int damageAmount)
	{
		Health -= damageAmount;

		if (_animationPlayer != null)
		{
			PlayAnimation(AnimationState.Hit);

			if (_animationPlayer.HasAnimation("hit"))
			{
				// Wait for hit animation to finish, then return to idle or die
				ToSignal(_animationPlayer, AnimationPlayer.SignalName.AnimationFinished).OnCompleted(() =>
				{
					if (Health > 0)
					{
						PlayAnimation(AnimationState.Idle);
					}
				});
			}
		}

		if (Health <= 0)
		{
			PlayAnimation(AnimationState.Death);
			OnDestroyed();
		}
	}

	protected override float GetDestructionThreshold(RigidBody2D colliderType)
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

	protected override void OnDestroyed()
	{
		base.OnDestroyed();

		// Award points for destroying enemy
		if (GameManager.Instance != null)
		{
			GameManager.Instance.EnemiesDefeated++;
		}

		// Show score popup
		Scorer scorer = GetTree().CurrentScene as Scorer;
		if (scorer != null)
		{
			scorer.AddScore(100, GlobalPosition);
		}
	}

	// TODO: Add sprite sheet animation setup methods when assets are available
	/*
	private void SetupIdleAnimation() { }
	private void SetupHitAnimation() { }
	private void SetupDeathAnimation() { }
	*/
}
