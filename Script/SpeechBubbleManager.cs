using Godot;
using System.Collections.Generic;

/// <summary>
/// Manager for spawning speech bubbles with character dialogue during gameplay.
/// Handles contextual dialogue based on expression, events, and random intervals.
/// </summary>
public partial class SpeechBubbleManager : Node
{
    public static SpeechBubbleManager Instance { get; private set; } = null!;

    [ExportGroup("Settings")]
    [Export] private bool _enableSpeechBubbles = true;
    [Export] private float _bubbleLifetime = 2.5f;
    [Export] private float _randomBubbleIntervalMin = 2.0f;
    [Export] private float _randomBubbleIntervalMax = 3.0f;
    [Export] private float _bubbleFloatSpeed = 30f;

    [ExportGroup("Bubble Appearance")]
    [Export] private Color _bubbleColor = new Color(1f, 1f, 0.9f, 0.95f);
    [Export] private Color _bubbleBorderColor = new Color(0.2f, 0.2f, 0.2f, 1f);
    [Export] private int _bubblePadding = 10;
    [Export] private int _bubbleCornerRadius = 12;
    [Export] private int _bubbleFontSize = 16;

    private PackedScene? _bubbleScene;
    private float _randomBubbleTimer = 0f;
    private float _nextRandomBubbleTime = 0f;
    private FaceProjectile? _currentProjectile;

    // Dialogue pools for each expression type
    private readonly Dictionary<ExpressionType, string[]> _dialoguePools = new()
    {
        [ExpressionType.Scared] = new[]
        {
            "Ahhhhh!", "Help!", "No no no!", "Waaaaa!", "Pleease!"
        },
        [ExpressionType.Happy] = new[]
        {
            "Wheee!", "Yay!", "Let's go!", "This is fun!", "Soaring!"
        },
        [ExpressionType.Angry] = new[]
        {
            "Take that!", "Pow!", "Hah!", "Boom!", "Take this!"
        },
        [ExpressionType.Dizzy] = new[]
        {
            "Whoa...", "Spinning...", "Dizzy...", "Wobbly...", "Where am I?"
        },
        [ExpressionType.Determined] = new[]
        {
            "Focus...", "Got this!", "Aiming...", "Steady...", "Almost..."
        },
        [ExpressionType.Excited] = new[]
        {
            "Here we go!", "Woooo!", "So fast!", "Look at me!", "Amazing!"
        },
        [ExpressionType.Bored] = new[]
        {
            "Is this it?", "So slow...", "Already?", "Done yet?", "Boring..."
        },
        [ExpressionType.Frightened] = new[]
        {
            "Too high!", "Watch out!", "Scary!", "Oh no!", "Help me!"
        },
        [ExpressionType.Curious] = new[]
        {
            "What's this?", "Ooh!", "Neat!", "Interesting!", "Wow!"
        },
        [ExpressionType.Nauseous] = new[]
        {
            "Whoa there!", "Upside down!", "Motion sick!", "Whoosh!", "Nauseated!"
        },
        [ExpressionType.Melting] = new[]
        {
            "Hot hot hot!", "Melting!", "Sizzle!", "Too warm!", "Warm fuzzy feeling!"
        },
        [ExpressionType.Cold] = new[]
        {
            "Brr!", "Chilly!", "Freezing!", "Ice ice baby!", "Brrr cold!"
        },
        [ExpressionType.Disgusted] = new[]
        {
            "Eww!", "Gross!", "No thanks!", "Yuck!", "Disgusting!"
        },
        [ExpressionType.Neutral] = new[]
        {
            "Hmm...", "Okay then", "Alright", "Yo!", "Hey there!"
        }
    };

    // Impact dialogue pool
    private readonly string[] _impactDialogue = new[]
    {
        "Oof!", "Boom!", "Crash!", "Ouch!", "Bam!", "Whack!", "Smack!", "Thud!"
    };

    // Launch dialogue pool
    private readonly string[] _launchDialogue = new[]
    {
        "Launch!", "Flying!", "Up and away!", "Go!", "Away we go!"
    };

    public override void _Ready()
    {
        Instance = this;
    }

    public override void _Process(double delta)
    {
        if (!_enableSpeechBubbles) return;

        _randomBubbleTimer += (float)delta;

        if (_randomBubbleTimer >= _nextRandomBubbleTime)
        {
            TrySpawnRandomBubble();
            _randomBubbleTimer = 0f;
            _nextRandomBubbleTime = (float)GD.RandRange(_randomBubbleIntervalMin, _randomBubbleIntervalMax);
        }
    }

    /// <summary>
    /// Sets the current projectile to track for speech bubbles
    /// </summary>
    public void SetCurrentProjectile(FaceProjectile? projectile)
    {
        _currentProjectile = projectile;
        if (projectile != null)
        {
            _randomBubbleTimer = 0f;
            _nextRandomBubbleTime = (float)GD.RandRange(_randomBubbleIntervalMin, _randomBubbleIntervalMax);
        }
    }

    /// <summary>
    /// Spawns a speech bubble with launch dialogue
    /// </summary>
    public void OnLaunch()
    {
        if (!_enableSpeechBubbles || _currentProjectile == null) return;
        SpawnBubble(_launchDialogue[GD.RandRange(0, _launchDialogue.Length - 1)]);
    }

    /// <summary>
    /// Spawns a speech bubble with impact dialogue
    /// </summary>
    public void OnImpact(float impactForce)
    {
        if (!_enableSpeechBubbles || _currentProjectile == null) return;
        SpawnBubble(_impactDialogue[GD.RandRange(0, _impactDialogue.Length - 1)]);
    }

    /// <summary>
    /// Spawns a speech bubble with expression-specific dialogue
    /// </summary>
    public void OnExpressionChanged(ExpressionType expression)
    {
        if (!_enableSpeechBubbles || _currentProjectile == null) return;

        if (_dialoguePools.TryGetValue(expression, out var dialogues) && dialogues.Length > 0)
        {
            var dialogue = dialogues[GD.RandRange(0, dialogues.Length - 1)];
            SpawnBubble(dialogue);
        }
    }

    private void TrySpawnRandomBubble()
    {
        if (_currentProjectile == null) return;

        float speed = _currentProjectile.LinearVelocity.Length();
        if (speed > 100f)
        {
            // Get current expression from expression manager
            var expressionManager = _currentProjectile.GetNodeOrNull<ExpressionManager>("ExpressionManager");
            if (expressionManager != null)
            {
                var currentExpr = expressionManager.CurrentExpression;
                if (_dialoguePools.TryGetValue(currentExpr, out var dialogues) && dialogues.Length > 0)
                {
                    // Only spawn random bubble 30% of the time to avoid over-spamming
                    if (GD.Randf() < 0.3f)
                    {
                        var dialogue = dialogues[GD.RandRange(0, dialogues.Length - 1)];
                        SpawnBubble(dialogue);
                    }
                }
            }
        }
    }

    private void SpawnBubble(string text)
    {
        if (_currentProjectile == null || string.IsNullOrEmpty(text)) return;

        var bubble = new SpeechBubble
        {
            Text = text,
            Lifetime = _bubbleLifetime,
            FloatSpeed = _bubbleFloatSpeed,
            BubbleColor = _bubbleColor,
            BorderColor = _bubbleBorderColor,
            Padding = _bubblePadding,
            CornerRadius = _bubbleCornerRadius,
            FontSize = _bubbleFontSize
        };

        // Position bubble above projectile
        bubble.GlobalPosition = _currentProjectile.GlobalPosition + new Vector2(0, -60);

        GetTree().Root.AddChild(bubble);
    }

    /// <summary>
    /// Gets a random dialogue line for the given expression type
    /// </summary>
    public string GetRandomDialogue(ExpressionType expression)
    {
        if (_dialoguePools.TryGetValue(expression, out var dialogues) && dialogues.Length > 0)
        {
            return dialogues[GD.RandRange(0, dialogues.Length - 1)];
        }
        return "";
    }

    /// <summary>
    /// Gets a random impact dialogue
    /// </summary>
    public string GetRandomImpactDialogue()
    {
        return _impactDialogue[GD.RandRange(0, _impactDialogue.Length - 1)];
    }
}

/// <summary>
/// A speech bubble that floats upward and fades out over time.
/// </summary>
public partial class SpeechBubble : Control
{
    [Export] public string Text { get; set; } = "";
    [Export] public float Lifetime { get; set; } = 2.5f;
    [Export] public float FloatSpeed { get; set; } = 30f;
    [Export] public Color BubbleColor { get; set; } = new Color(1f, 1f, 0.9f, 0.95f);
    [Export] public Color BorderColor { get; set; } = new Color(0.2f, 0.2f, 0.2f, 1f);
    [Export] public int Padding { get; set; } = 10;
    [Export] public int CornerRadius { get; set; } = 12;
    [Export] public int FontSize { get; set; } = 16;

    private float _timer = 0f;
    private Label? _label;
    private ColorRect? _background;
    private Vector2 _startPosition;
    private Vector2 _targetPosition;

    public override void _Ready()
    {
        _startPosition = GlobalPosition;
        _targetPosition = _startPosition + new Vector2(0, -80);

        SetupVisuals();
    }

    private void SetupVisuals()
    {
        // Create label first to measure text
        _label = new Label
        {
            Text = Text,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };

        if (FontSize > 0)
        {
            _label.AddThemeFontSizeOverride("font_size", FontSize);
        }

        AddChild(_label);

        // Wait a frame for label to size itself
        CallDeferred(nameof(CreateBackground));
    }

    private void CreateBackground()
    {
        if (_label == null) return;

        var labelSize = _label.GetMinimumSize();
        var bubbleSize = new Vector2(
            labelSize.X + (Padding * 2),
            labelSize.Y + (Padding * 2)
        );

        // Create rounded rectangle background using 9-patch or panel
        var panel = new Panel
        {
            CustomMinimumSize = bubbleSize,
            Size = bubbleSize
        };

        // Style the panel
        var style = new StyleBoxFlat
        {
            BgColor = BubbleColor,
            BorderColor = BorderColor,
            CornerRadiusTopLeft = CornerRadius,
            CornerRadiusTopRight = CornerRadius,
            CornerRadiusBottomLeft = CornerRadius,
            CornerRadiusBottomRight = CornerRadius,
            ContentMarginLeft = Padding,
            ContentMarginRight = Padding,
            ContentMarginTop = Padding,
            ContentMarginBottom = Padding,
            BorderWidthBottom = 2,
            BorderWidthLeft = 2,
            BorderWidthRight = 2,
            BorderWidthTop = 2
        };

        panel.AddThemeStyleboxOverride("panel", style);

        // Move panel to front
        MoveChild(panel, 0);

        _background = panel;

        // Position label
        _label.Position = new Vector2(Padding, Padding);
    }

    public override void _Process(double delta)
    {
        _timer += (float)delta;

        // Calculate fade progress (fade out in last 30% of lifetime)
        float fadeStart = Lifetime * 0.7f;
        float fadeProgress = (_timer - fadeStart) / (Lifetime - fadeStart);
        float alpha = Mathf.Clamp(1f - fadeProgress, 0f, 1f);

        // Float upward
        float floatProgress = _timer / Lifetime;
        float smoothProgress = 1f - Mathf.Pow(1f - floatProgress, 2f); // Ease out
        GlobalPosition = _startPosition.Lerp(_targetPosition, smoothProgress);

        // Apply fade
        Modulate = new Color(1f, 1f, 1f, alpha);

        // Cleanup
        if (_timer >= Lifetime)
        {
            QueueFree();
        }
    }
}
