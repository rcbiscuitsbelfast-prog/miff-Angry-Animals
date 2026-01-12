using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// Maps ExpressionType to bone transformations for facial animation.
/// Handles smooth transitions between expressions and blends multiple expressions.
/// </summary>
public class FaceAnimationController : Node
{
    [Signal] public delegate void ExpressionChangedEventHandler(ExpressionType expression, float intensity);

    private FaceRiggerSystem _riggerSystem;
    private ExpressionType _currentExpression = ExpressionType.Neutral;
    private float _expressionIntensity = 0f;
    private float _transitionSpeed = 5f;
    private bool _isAnimating = false;
    
    // Animation curves for smooth transitions
    private Curve _easeInOut = new Curve();
    private Curve _bounce = new Curve();
    private Curve _elastic = new Curve();

    // Current bone states for blending
    private Dictionary<string, Vector2> _currentRotations = new Dictionary<string, Vector2>();
    private Dictionary<string, Vector2> _currentScales = new Dictionary<string, Vector2>();
    private Dictionary<string, Vector2> _targetRotations = new Dictionary<string, Vector2>();
    private Dictionary<string, Vector2> _targetScales = new Dictionary<string, Vector2>();

    // Expression presets for different emotion types
    private Dictionary<ExpressionType, ExpressionAnimation> _expressionPresets;

    public FaceAnimationController(FaceRiggerSystem riggerSystem)
    {
        _riggerSystem = riggerSystem;
        InitializeAnimationCurves();
        InitializeExpressionPresets();
    }

    public override void _Ready()
    {
        _riggerSystem.Connect(FaceRiggerSystem.SignalName.RigUpdated, new Callable(this, nameof(OnRigUpdated)));
    }

    public override void _Process(double delta)
    {
        UpdateAnimations((float)delta);
    }

    /// <summary>
    /// Sets a facial expression with smooth animation
    /// </summary>
    public void SetExpression(ExpressionType expression, float intensity = 1f, float duration = 0.3f)
    {
        if (_currentExpression == expression && _expressionIntensity == intensity) return;

        _currentExpression = expression;
        _expressionIntensity = intensity;
        
        // Calculate target bone transforms for this expression
        var animation = _expressionPresets.ContainsKey(expression) ? _expressionPresets[expression] : CreateDefaultAnimation(expression);
        CalculateTargetTransforms(animation, intensity);
        
        // Start smooth transition
        _isAnimating = true;
        EmitSignal(SignalName.ExpressionChanged, expression, intensity);
        
        GD.Print($"FaceAnimationController: Set expression {expression} with intensity {intensity}");
    }

    private void InitializeAnimationCurves()
    {
        // Ease in-out curve
        _easeInOut.AddPoint(0, 0);
        _easeInOut.AddPoint(0.5f, 0.5f);
        _easeInOut.AddPoint(1, 1);
        
        // Bounce curve
        _bounce.AddPoint(0, 0);
        _bounce.AddPoint(0.3f, 1.2f);
        _bounce.AddPoint(0.7f, 0.8f);
        _bounce.AddPoint(1, 1);
        
        // Elastic curve (for extreme expressions)
        _elastic.AddPoint(0, 0);
        _elastic.AddPoint(0.2f, 1.3f);
        _elastic.AddPoint(0.5f, 0.7f);
        _elastic.AddPoint(0.8f, 1.1f);
        _elastic.AddPoint(1, 1);
    }

    private void InitializeExpressionPresets()
    {
        _expressionPresets = new Dictionary<ExpressionType, ExpressionAnimation>
        {
            // HAPPY EXPRESSIONS
            {
                ExpressionType.Happy, new ExpressionAnimation
                {
                    BoneRotations = new Dictionary<string, float>
                    {
                        { "JawBottom", 0.3f }, // Slight jaw drop
                        { "MouthLeftCorner", 0.4f }, // Smile up
                        { "MouthRightCorner", 0.4f },
                        { "LeftCheek", 0.2f }, // Cheek raise
                        { "RightCheek", 0.2f },
                        { "LeftEyebrow", -0.1f }, // Slight eyebrow raise
                        { "RightEyebrow", -0.1f }
                    },
                    BoneScales = new Dictionary<string, Vector2>
                    {
                        { "LeftEye", new Vector2(1.0f, 0.9f) }, // Slight eye squint
                        { "RightEye", new Vector2(1.0f, 0.9f) },
                        { "LeftCheek", new Vector2(1.1f, 1.0f) }, // Cheek puff
                        { "RightCheek", new Vector2(1.1f, 1.0f) }
                    },
                    AnimationCurve = _easeInOut,
                    Duration = 0.5f
                }
            },
            
            // ANGRY EXPRESSIONS
            {
                ExpressionType.Angry, new ExpressionAnimation
                {
                    BoneRotations = new Dictionary<string, float>
                    {
                        { "LeftEyebrow", -0.4f }, // Eyebrows down and in
                        { "RightEyebrow", 0.4f },
                        { "LeftEye", -0.1f }, // Eyes narrow
                        { "RightEye", 0.1f },
                        { "JawBottom", 0.1f }, // Jaw tight
                        { "MouthLeftCorner", -0.2f }, // Mouth corners down
                        { "MouthRightCorner", 0.2f }
                    },
                    BoneScales = new Dictionary<string, Vector2>
                    {
                        { "LeftEye", new Vector2(1.1f, 0.8f) }, // Narrowed eyes
                        { "RightEye", new Vector2(1.1f, 0.8f) },
                        { "MouthCenter", new Vector2(1.0f, 0.7f) } // Tight lips
                    },
                    AnimationCurve = _bounce,
                    Duration = 0.4f
                }
            },

            // SURPRISED EXPRESSIONS
            {
                ExpressionType.Excited, new ExpressionAnimation
                {
                    BoneRotations = new Dictionary<string, float>
                    {
                        { "LeftEyebrow", -0.6f }, // High eyebrow raise
                        { "RightEyebrow", 0.6f },
                        { "LeftEye", 0.2f }, // Eyes widen
                        { "RightEye", -0.2f },
                        { "JawBottom", 0.5f }, // Jaw drops
                        { "MouthCenter", 0.3f }, // Mouth opens
                        { "LeftCheek", 0.3f }, // Cheeks lift
                        { "RightCheek", -0.3f }
                    },
                    BoneScales = new Dictionary<string, Vector2>
                    {
                        { "LeftEye", new Vector2(0.8f, 1.3f) }, // Wide eyes
                        { "RightEye", new Vector2(0.8f, 1.3f) },
                        { "MouthCenter", new Vector2(1.3f, 1.1f) }, // Open mouth
                        { "Forehead", new Vector2(1.0f, 1.2f) } // Forehead stretch
                    },
                    AnimationCurve = _elastic,
                    Duration = 0.6f
                }
            },

            // SCARED EXPRESSIONS
            {
                ExpressionType.Scared, new ExpressionAnimation
                {
                    BoneRotations = new Dictionary<string, float>
                    {
                        { "LeftEyebrow", -0.5f }, // High raised eyebrows
                        { "RightEyebrow", 0.5f },
                        { "LeftEye", 0.3f }, // Wide eyes
                        { "RightEye", -0.3f },
                        { "JawBottom", 0.4f }, // Jaw drops
                        { "MouthCenter", 0.2f }, // O-shaped mouth
                        { "Nose", 0.1f } // Nose flares
                    },
                    BoneScales = new Dictionary<string, Vector2>
                    {
                        { "LeftEye", new Vector2(0.7f, 1.4f) }, // Very wide eyes
                        { "RightEye", new Vector2(0.7f, 1.4f) },
                        { "MouthCenter", new Vector2(1.0f, 1.2f) }, // O-shaped
                        { "LeftCheek", new Vector2(0.9f, 1.1f) }, // Tense cheeks
                        { "RightCheek", new Vector2(0.9f, 1.1f) }
                    },
                    AnimationCurve = _bounce,
                    Duration = 0.5f
                }
            },

            // SAD EXPRESSIONS
            {
                ExpressionType.Melting, new ExpressionAnimation
                {
                    BoneRotations = new Dictionary<string, float>
                    {
                        { "LeftEyebrow", 0.3f }, // Inner corners down
                        { "RightEyebrow", -0.3f },
                        { "LeftEye", -0.1f }, // Eyes slightly down
                        { "RightEye", 0.1f },
                        { "MouthLeftCorner", -0.3f }, // Mouth corners down
                        { "MouthRightCorner", 0.3f },
                        { "JawBottom", -0.2f } // Jaw drops slightly
                    },
                    BoneScales = new Dictionary<string, Vector2>
                    {
                        { "LeftEye", new Vector2(1.0f, 0.9f) }, // Droopy eyes
                        { "RightEye", new Vector2(1.0f, 0.9f) },
                        { "LeftCheek", new Vector2(0.9f, 0.8f) }, // Droopy cheeks
                        { "RightCheek", new Vector2(0.9f, 0.8f) },
                        { "MouthCenter", new Vector2(1.0f, 0.8f) } // Droopy mouth
                    },
                    AnimationCurve = _easeInOut,
                    Duration = 0.7f
                }
            },

            // DIZZY EXPRESSIONS
            {
                ExpressionType.Dizzy, new ExpressionAnimation
                {
                    BoneRotations = new Dictionary<string, float>
                    {
                        { "LeftEye", 0.8f }, // Crossed eyes
                        { "RightEye", -0.8f },
                        { "LeftEyebrow", 0.2f }, // Confused eyebrows
                        { "RightEyebrow", -0.2f },
                        { "MouthCenter", 0.5f }, // Wavy mouth
                        { "JawBottom", 0.1f }
                    },
                    BoneScales = new Dictionary<string, Vector2>
                    {
                        { "LeftEye", new Vector2(0.9f, 0.9f) }, // Smaller eyes
                        { "RightEye", new Vector2(0.9f, 0.9f) },
                        { "MouthCenter", new Vector2(1.2f, 0.6f) }, // Flat mouth
                        { "Nose", new Vector2(1.1f, 1.0f) } // Slightly squashed nose
                    },
                    AnimationCurve = _elastic,
                    Duration = 0.3f
                }
            },

            // DETERMINED EXPRESSIONS
            {
                ExpressionType.Determined, new ExpressionAnimation
                {
                    BoneRotations = new Dictionary<string, float>
                    {
                        { "LeftEyebrow", -0.2f }, // Eyebrows focused
                        { "RightEyebrow", 0.2f },
                        { "LeftEye", 0.1f }, // Focused eyes
                        { "RightEye", -0.1f },
                        { "JawBottom", 0.2f }, // Determined jaw
                        { "MouthLeftCorner", 0.1f }, // Slight smile
                        { "MouthRightCorner", -0.1f }
                    },
                    BoneScales = new Dictionary<string, Vector2>
                    {
                        { "LeftEye", new Vector2(1.0f, 1.0f) }, // Normal eyes
                        { "RightEye", new Vector2(1.0f, 1.0f) },
                        { "Forehead", new Vector2(1.0f, 1.1f) } // Slight forehead tension
                    },
                    AnimationCurve = _easeInOut,
                    Duration = 0.4f
                }
            },

            // CURIOUS EXPRESSIONS
            {
                ExpressionType.Curious, new ExpressionAnimation
                {
                    BoneRotations = new Dictionary<string, float>
                    {
                        { "LeftEyebrow", -0.1f }, // One eyebrow raised
                        { "RightEyebrow", 0.3f },
                        { "LeftEye", 0.0f }, // Normal eyes
                        { "RightEye", 0.0f },
                        { "JawBottom", 0.1f }, // Slight jaw drop
                        { "MouthCenter", 0.1f } // Slight mouth open
                    },
                    BoneScales = new Dictionary<string, Vector2>
                    {
                        { "RightEye", new Vector2(1.0f, 1.1f) }, // One eye slightly larger
                        { "MouthCenter", new Vector2(1.0f, 0.9f) } // Slight mouth pucker
                    },
                    AnimationCurve = _easeInOut,
                    Duration = 0.3f
                }
            },

            // BORED EXPRESSIONS
            {
                ExpressionType.Bored, new ExpressionAnimation
                {
                    BoneRotations = new Dictionary<string, float>
                    {
                        { "LeftEyebrow", 0.1f }, // Droopy eyebrows
                        { "RightEyebrow", -0.1f },
                        { "LeftEye", -0.2f }, // Half-closed eyes
                        { "RightEye", 0.2f },
                        { "MouthCenter", -0.1f }, // Slight frown
                        { "JawBottom", -0.1f }
                    },
                    BoneScales = new Dictionary<string, Vector2>
                    {
                        { "LeftEye", new Vector2(1.0f, 0.6f) }, // Half-closed eyes
                        { "RightEye", new Vector2(1.0f, 0.6f) },
                        { "MouthCenter", new Vector2(0.9f, 0.7f) } // Flat mouth
                    },
                    AnimationCurve = _easeInOut,
                    Duration = 0.6f
                }
            },

            // NAUSEOUS EXPRESSIONS
            {
                ExpressionType.Nauseous, new ExpressionAnimation
                {
                    BoneRotations = new Dictionary<string, float>
                    {
                        { "LeftEyebrow", 0.1f }, // Uneven eyebrows
                        { "RightEyebrow", -0.2f },
                        { "LeftEye", 0.1f }, // Squinting eyes
                        { "RightEye", -0.1f },
                        { "JawBottom", 0.2f }, // Jaw tight
                        { "MouthCenter", 0.3f }, // Mouth pucker
                        { "Nose", -0.1f } // Nose wrinkled
                    },
                    BoneScales = new Dictionary<string, Vector2>
                    {
                        { "LeftEye", new Vector2(1.1f, 0.7f) }, // Squinting
                        { "RightEye", new Vector2(1.1f, 0.7f) },
                        { "MouthCenter", new Vector2(0.7f, 0.8f) }, // Puckered mouth
                        { "Nose", new Vector2(1.2f, 0.9f) } // Wrinkled nose
                    },
                    AnimationCurve = _bounce,
                    Duration = 0.4f
                }
            },

            // COLD EXPRESSIONS
            {
                ExpressionType.Cold, new ExpressionAnimation
                {
                    BoneRotations = new Dictionary<string, float>
                    {
                        { "LeftEyebrow", -0.1f }, // Furrowed brows
                        { "RightEyebrow", 0.1f },
                        { "LeftEye", 0.0f }, // Tight eyes
                        { "RightEye", 0.0f },
                        { "JawBottom", 0.1f }, // Tense jaw
                        { "MouthLeftCorner", -0.1f }, // Tight lips
                        { "MouthRightCorner", 0.1f },
                        { "Nose", 0.2f } // Flared nostrils
                    },
                    BoneScales = new Dictionary<string, Vector2>
                    {
                        { "LeftEye", new Vector2(1.0f, 0.8f) }, // Narrowed eyes
                        { "RightEye", new Vector2(1.0f, 0.8f) },
                        { "MouthCenter", new Vector2(0.8f, 0.6f) }, // Tight lips
                        { "Nose", new Vector2(1.0f, 1.3f) } // Flared nose
                    },
                    AnimationCurve = _easeInOut,
                    Duration = 0.3f
                }
            },

            // DISGUSTED EXPRESSIONS
            {
                ExpressionType.Disgusted, new ExpressionAnimation
                {
                    BoneRotations = new Dictionary<string, float>
                    {
                        { "LeftEyebrow", 0.2f }, // Raised and wrinkled
                        { "RightEyebrow", -0.2f },
                        { "LeftEye", 0.1f }, // Squinting
                        { "RightEye", -0.1f },
                        { "JawBottom", 0.1f }, // Lip curl
                        { "MouthLeftCorner", -0.4f }, // Lip corner down
                        { "MouthRightCorner", 0.4f },
                        { "Nose", 0.3f } // Nose wrinkled
                    },
                    BoneScales = new Dictionary<string, Vector2>
                    {
                        { "LeftEye", new Vector2(1.1f, 0.7f) }, // Squinting
                        { "RightEye", new Vector2(1.1f, 0.7f) },
                        { "MouthCenter", new Vector2(1.2f, 0.8f) }, // Upper lip raised
                        { "Nose", new Vector2(1.1f, 0.9f) } // Wrinkled nose
                    },
                    AnimationCurve = _bounce,
                    Duration = 0.4f
                }
            }
        };
    }

    private ExpressionAnimation CreateDefaultAnimation(ExpressionType expression)
    {
        // Default animation for any expressions not explicitly defined
        return new ExpressionAnimation
        {
            BoneRotations = new Dictionary<string, float>
            {
                { "FaceCenter", 0.1f * (float)expression / 10f }
            },
            BoneScales = new Dictionary<string, Vector2>
            {
                { "FaceCenter", Vector2.One }
            },
            AnimationCurve = _easeInOut,
            Duration = 0.3f
        };
    }

    private void CalculateTargetTransforms(ExpressionAnimation animation, float intensity)
    {
        _targetRotations.Clear();
        _targetScales.Clear();

        // Calculate rotations
        foreach (var kvp in animation.BoneRotations)
        {
            _targetRotations[kvp.Key] = new Vector2(kvp.Value * intensity, 0f);
        }

        // Calculate scales
        foreach (var kvp in animation.BoneScales)
        {
            _targetScales[kvp.Key] = kvp.Value;
        }

        // Ensure we have entries for all bones (even if not animated)
        if (_riggerSystem.CurrentRig != null)
        {
            foreach (var boneName in _riggerSystem.CurrentRig.Bones.Keys)
            {
                if (!_targetRotations.ContainsKey(boneName))
                {
                    _targetRotations[boneName] = Vector2.Zero;
                }
                if (!_targetScales.ContainsKey(boneName))
                {
                    _targetScales[boneName] = Vector2.One;
                }
            }
        }
    }

    private void UpdateAnimations(float delta)
    {
        if (!_isAnimating || _riggerSystem.CurrentRig == null) return;

        float step = delta * _transitionSpeed;

        // Smoothly interpolate to target values
        foreach (var kvp in _targetRotations)
        {
            var boneName = kvp.Key;
            var target = kvp.Value;
            
            if (!_currentRotations.ContainsKey(boneName))
            {
                _currentRotations[boneName] = Vector2.Zero;
            }
            
            _currentRotations[boneName] = _currentRotations[boneName].Lerp(target, step);
            _riggerSystem.AnimateBone(boneName, _currentRotations[boneName], _currentScales.GetValueOrDefault(boneName, Vector2.One));
        }

        foreach (var kvp in _targetScales)
        {
            var boneName = kvp.Key;
            var target = kvp.Value;
            
            if (!_currentScales.ContainsKey(boneName))
            {
                _currentScales[boneName] = Vector2.One;
            }
            
            _currentScales[boneName] = _currentScales[boneName].Lerp(target, step);
            _riggerSystem.AnimateBone(boneName, _currentRotations.GetValueOrDefault(boneName, Vector2.Zero), _currentScales[boneName]);
        }

        // Check if animation is complete
        bool allComplete = true;
        foreach (var kvp in _targetRotations)
        {
            var current = _currentRotations.GetValueOrDefault(kvp.Key, Vector2.Zero);
            if (current.DistanceTo(kvp.Value) > 0.01f)
            {
                allComplete = false;
                break;
            }
        }

        if (allComplete)
        {
            _isAnimating = false;
        }
    }

    private void OnRigUpdated()
    {
        // Rig was updated, reset animation states
        _currentRotations.Clear();
        _currentScales.Clear();
        _isAnimating = false;
    }

    /// <summary>
    /// Adds a blinking animation
    /// </summary>
    public void Blink(float duration = 0.15f)
    {
        if (_riggerSystem.CurrentRig == null) return;

        // Animate eye bones to close
        _riggerSystem.AnimateBone("LeftEye", Vector2.Zero, new Vector2(1.0f, 0.1f), 1f);
        _riggerSystem.AnimateBone("RightEye", Vector2.Zero, new Vector2(1.0f, 0.1f), 1f);
        
        // Create timer to reopen eyes
        var timer = new Timer();
        timer.WaitTime = duration;
        timer.OneShot = true;
        timer.Timeout += () => 
        {
            _riggerSystem.AnimateBone("LeftEye", Vector2.Zero, Vector2.One, 1f);
            _riggerSystem.AnimateBone("RightEye", Vector2.Zero, Vector2.One, 1f);
            timer.QueueFree();
        };
        AddChild(timer);
        timer.Start();
    }

    /// <summary>
    /// Gets the current expression
    /// </summary>
    public ExpressionType CurrentExpression => _currentExpression;
    
    /// <summary>
    /// Gets the current expression intensity
    /// </summary>
    public float CurrentIntensity => _expressionIntensity;
}

// Animation data structure for expressions
public class ExpressionAnimation
{
    public Dictionary<string, float> BoneRotations;
    public Dictionary<string, Vector2> BoneScales;
    public Curve AnimationCurve;
    public float Duration = 0.3f;
}