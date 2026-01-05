using Godot;
using System.Collections;

/// <summary>
/// Professional-grade animation system for character sprites.
/// Supports sprite sheet-based animations with 6 states.
/// Ported from Angry Aliens StickCloneAnimator.gd
/// </summary>
public partial class StickCloneAnimator : Node
{
	[Signal] public delegate void AnimationFinishedEventHandler();

	public enum AnimState
	{
		Idle,      // Standing still (frames 0-5)
		Walk,       // Walking animation (frames 6-13)
		Jump,       // Full jump arc (frames 14-17)
		JumpUp,     // Ascending portion (frames 14-15)
		JumpDown,   // Descending portion (frames 16-17)
		Climb       // Climbing debris (frames 18-23)
	}

	private Sprite2D _sprite;
	private AnimationPlayer _animationPlayer;
	private Dictionary<AnimState, AnimationConfig> _frameConfig;

	public override void _Ready()
	{
		_sprite = GetParent().GetNodeOrNull<Sprite2D>("Sprite2D");
		_animationPlayer = GetParent().GetNodeOrNull<AnimationPlayer>("AnimationPlayer");

		if (_sprite != null)
		{
			InitializeFrameConfig();
		}
		else
		{
			GD.PrintErr("StickCloneAnimator: No Sprite2D found in parent!");
		}
	}

	private void InitializeFrameConfig()
	{
		_frameConfig = new Dictionary<AnimState, AnimationConfig>
		{
			{
				AnimState.Idle, new AnimationConfig { Start = 0, End = 5, Speed = 0.15f }
			},
			{
				AnimState.Walk, new AnimationConfig { Start = 6, End = 13, Speed = 0.10f }
			},
			{
				AnimState.Jump, new AnimationConfig { Start = 14, End = 17, Speed = 0.20f }
			},
			{
				AnimState.JumpUp, new AnimationConfig { Start = 14, End = 15, Speed = 0.15f }
			},
			{
				AnimState.JumpDown, new AnimationConfig { Start = 16, End = 17, Speed = 0.15f }
			},
			{
				AnimState.Climb, new AnimationConfig { Start = 18, End = 23, Speed = 0.12f }
			}
		};

		GD.Print("StickCloneAnimator: Animation system initialized. Add sprite sheet assets for full animations.");
	}

	/// <summary>
	/// Plays an animation state.
	/// </summary>
	public void PlayAnimation(AnimState state)
	{
		if (!_frameConfig.ContainsKey(state))
		{
			GD.Print($"StickCloneAnimator: No config for animation state {state}");
			return;
		}

		var config = _frameConfig[state];

		if (_animationPlayer != null && _sprite != null)
		{
			// If using AnimationPlayer with sprite sheet animations
			if (_animationPlayer.HasAnimation(state.ToString().ToLower()))
			{
				_animationPlayer.Play(state.ToString().ToLower());
			}
			else
			{
				// Fallback: simple frame animation using Sprite2D.Frame
				AnimateFrames(config);
			}
		}
	}

	/// <summary>
	/// Simple frame-based animation (fallback when no AnimationPlayer setup).
	/// </summary>
	private void AnimateFrames(AnimationConfig config)
	{
		if (_sprite == null) return;

		var tween = CreateTween();
		int currentFrame = config.Start;

		tween.TweenMethod(
			Callable.From<int>(SetFrame),
			config.Start,
			config.End,
			(config.End - config.Start + 1) * config.Speed
		);

		tween.SetLoops();
	}

	private void SetFrame(int frame)
	{
		if (_sprite != null && _sprite.Hframes > 0)
		{
			_sprite.Frame = frame;
		}
	}

	/// <summary>
	/// Sets the facing direction of the character.
	/// </summary>
	public void SetFacingDirection(float direction)
	{
		if (_sprite != null)
		{
			_sprite.FlipH = direction < 0;
		}
	}

	/// <summary>
	/// Stops all animations.
	/// </summary>
	public void StopAnimations()
	{
		if (_animationPlayer != null)
		{
			_animationPlayer.Stop();
		}
	}

	public override void _ExitTree()
	{
		// Clean up any running tweens
		if (IsInstanceValid(this))
		{
			KillAllTweens();
		}
	}
}

/// <summary>
/// Configuration for an animation state.
/// </summary>
public record AnimationConfig
{
	public int Start { get; init; }
	public int End { get; init; }
	public float Speed { get; init; }
}
