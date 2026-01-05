using Godot;
using System.Collections.Generic;

/// <summary>
/// Manages all visual effects including particles, screen shake, and UI animations.
/// Provides easy-to-use methods for game juice and feedback.
/// AUTOMATED: This is an autoload singleton - do not create instances manually.
/// </summary>
public partial class EffectsManager : Node
{
    public static EffectsManager Instance { get; private set; } = null!;

    // Screen shake settings
    [ExportGroup("Screen Shake", "Shake")]
    [Export] private float _shakeDefaultIntensity = 5.0f;
    [Export] private float _shakeDefaultDuration = 0.3f;
    [Export] private float _shakeImpactIntensity = 15.0f;
    [Export] private float _shakeImpactDuration = 0.5f;

    // Particle effect settings
    [ExportGroup("Particle Effects", "Particles")]
    [Export] private PackedScene? _confettiParticleScene;
    [Export] private PackedScene? _explosionParticleScene;
    [Export] private PackedScene? _dustParticleScene;
    [Export] private PackedScene? _sparkleParticleScene;

    private Camera2D? _mainCamera;
    private float _shakeIntensity = 0f;
    private float _shakeDuration = 0f;
    private float _shakeTimer = 0f;
    private Vector2 _shakeOffset = Vector2.Zero;

    private readonly List<Node2D> _activeEffects = new List<Node2D>();

    public override void _Ready()
    {
        Instance = this;
        _mainCamera = GetViewport().GetCamera2D();
    }

    public override void _Process(double delta)
    {
        UpdateScreenShake(delta);
    }

    /// <summary>
    /// Shakes the camera with default intensity (for minor impacts)
    /// </summary>
    public void ShakeScreen()
    {
        ShakeScreen(_shakeDefaultIntensity, _shakeDefaultDuration);
    }

    /// <summary>
    /// Shakes the camera with custom intensity
    /// </summary>
    public void ShakeScreen(float intensity, float duration)
    {
        _shakeIntensity = intensity;
        _shakeDuration = duration;
        _shakeTimer = 0f;

        if (_mainCamera != null)
        {
            _mainCamera.Offset = Vector2.Zero;
        }
    }

    /// <summary>
    /// Intense screen shake for major impacts (explosions, boss hits)
    /// </summary>
    public void ShakeScreenIntense()
    {
        ShakeScreen(_shakeImpactIntensity, _shakeImpactDuration);
    }

    private void UpdateScreenShakeInternal(double delta)
    {
        if (_shakeTimer >= _shakeDuration)
        {
            if (_mainCamera != null)
            {
                _mainCamera.Offset = Vector2.Zero;
            }
            _shakeOffset = Vector2.Zero;
            return;
        }

        _shakeTimer += (float)delta;

        // Random shake offset
        float x = (float)GD.RandRange(-1.0, 1.0) * _shakeIntensity;
        float y = (float)GD.RandRange(-1.0, 1.0) * _shakeIntensity;
        _shakeOffset = new Vector2(x, y);

        // Fade out intensity
        float progress = _shakeTimer / _shakeDuration;
        _shakeOffset *= 1.0f - progress;

        if (_mainCamera != null)
        {
            _mainCamera.Offset = _shakeOffset;
        }
    }

    private void UpdateScreenShake(double delta)
    {
        UpdateScreenShakeInternal(delta);
    }

    /// <summary>
    /// Spawns confetti particles at the specified position
    /// </summary>
    public void SpawnConfetti(Vector2 position)
    {
        if (_confettiParticleScene == null)
        {
            CreateDefaultConfetti(position);
            return;
        }

        var particle = _confettiParticleScene.Instantiate<CPUParticles2D>();
        if (particle != null)
        {
            particle.GlobalPosition = position;
            AddChild(particle);
            _activeEffects.Add(particle);

            // Auto-remove after particle lifetime
            particle.Emitting = true;
            particle.OneShot = true;
            particle.Finished += () => RemoveEffect(particle);
        }
    }

    /// <summary>
    /// Spawns explosion particles at the specified position
    /// </summary>
    public void SpawnExplosion(Vector2 position)
    {
        if (_explosionParticleScene == null)
        {
            CreateDefaultExplosion(position);
            return;
        }

        var particle = _explosionParticleScene.Instantiate<CPUParticles2D>();
        if (particle != null)
        {
            particle.GlobalPosition = position;
            AddChild(particle);
            _activeEffects.Add(particle);

            particle.Emitting = true;
            particle.OneShot = true;
            particle.Finished += () => RemoveEffect(particle);
        }
    }

    /// <summary>
    /// Spawns dust particles at the specified position
    /// </summary>
    public void SpawnDust(Vector2 position)
    {
        if (_dustParticleScene == null)
        {
            CreateDefaultDust(position);
            return;
        }

        var particle = _dustParticleScene.Instantiate<CPUParticles2D>();
        if (particle != null)
        {
            particle.GlobalPosition = position;
            AddChild(particle);
            _activeEffects.Add(particle);

            particle.Emitting = true;
            particle.OneShot = true;
            particle.Finished += () => RemoveEffect(particle);
        }
    }

    /// <summary>
    /// Spawns sparkle particles for success/collection feedback
    /// </summary>
    public void SpawnSparkle(Vector2 position)
    {
        if (_sparkleParticleScene == null)
        {
            CreateDefaultSparkle(position);
            return;
        }

        var particle = _sparkleParticleScene.Instantiate<CPUParticles2D>();
        if (particle != null)
        {
            particle.GlobalPosition = position;
            AddChild(particle);
            _activeEffects.Add(particle);

            particle.Emitting = true;
            particle.OneShot = true;
            particle.Finished += () => RemoveEffect(particle);
        }
    }

    private void CreateDefaultConfetti(Vector2 position)
    {
        var particles = new CPUParticles2D
        {
            GlobalPosition = position,
            Emitting = true,
            OneShot = true,
            Amount = 50,
            Lifetime = 2.0f,
            Spread = 180.0f,
            Gravity = Vector2.Down * 300,
            InitialVelocityMin = 200,
            InitialVelocityMax = 400,
            AngularVelocityMin = -180,
            AngularVelocityMax = 180,
            ScaleMin = 0.5f,
            ScaleMax = 1.2f,
            Color = Colors.White
        };

        // Create rainbow colors
        for (int i = 0; i < 10; i++)
        {
            Color color = Color.FromHsv((float)i / 10.0f, 0.8f, 1.0f);
            particles.Colors.Add(color);
        }

        AddChild(particles);
        _activeEffects.Add(particles);
        particles.Finished += () => RemoveEffect(particles);
    }

    private void CreateDefaultExplosion(Vector2 position)
    {
        var particles = new CPUParticles2D
        {
            GlobalPosition = position,
            Emitting = true,
            OneShot = true,
            Amount = 30,
            Lifetime = 0.8f,
            Spread = 360.0f,
            Gravity = Vector2.Zero,
            InitialVelocityMin = 150,
            InitialVelocityMax = 300,
            ScaleMin = 0.5f,
            ScaleMax = 1.5f,
            Color = Colors.Orange
        };

        // Explosion gradient colors
        particles.Colors.Add(Color.FromHsv(0.08f, 1.0f, 1.0f)); // Orange
        particles.Colors.Add(Color.FromHsv(0.05f, 1.0f, 1.0f)); // Yellow
        particles.Colors.Add(Color.FromHsv(0.0f, 1.0f, 1.0f));  // Red

        AddChild(particles);
        _activeEffects.Add(particles);
        particles.Finished += () => RemoveEffect(particles);
    }

    private void CreateDefaultDust(Vector2 position)
    {
        var particles = new CPUParticles2D
        {
            GlobalPosition = position,
            Emitting = true,
            OneShot = true,
            Amount = 20,
            Lifetime = 1.0f,
            Spread = 60.0f,
            Gravity = Vector2.Down * 100,
            InitialVelocityMin = 50,
            InitialVelocityMax = 100,
            ScaleMin = 0.3f,
            ScaleMax = 0.8f,
            Color = Colors.Gray
        };

        AddChild(particles);
        _activeEffects.Add(particles);
        particles.Finished += () => RemoveEffect(particles);
    }

    private void CreateDefaultSparkle(Vector2 position)
    {
        var particles = new CPUParticles2D
        {
            GlobalPosition = position,
            Emitting = true,
            OneShot = true,
            Amount = 25,
            Lifetime = 0.6f,
            Spread = 360.0f,
            Gravity = Vector2.Zero,
            InitialVelocityMin = 100,
            InitialVelocityMax = 200,
            ScaleMin = 0.2f,
            ScaleMax = 0.6f,
            Color = Colors.Gold
        };

        AddChild(particles);
        _activeEffects.Add(particles);
        particles.Finished += () => RemoveEffect(particles);
    }

    private void RemoveEffect(Node2D effect)
    {
        _activeEffects.Remove(effect);
        effect.QueueFree();
    }

    /// <summary>
    /// Clears all active effects
    /// </summary>
    public void ClearAllEffects()
    {
        foreach (var effect in _activeEffects)
        {
            if (IsInstanceValid(effect))
            {
                effect.QueueFree();
            }
        }
        _activeEffects.Clear();
    }
}
