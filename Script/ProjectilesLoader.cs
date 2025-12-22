using System.Collections.Generic;
using Godot;

/// <summary>
/// Manages the queue of projectiles to be loaded into the slingshot.
/// Listens for projectile death events and loads the next projectile automatically.
/// Emits a signal when all projectiles are used.
/// </summary>
public partial class ProjectilesLoader : Node2D
{
    [Signal] public delegate void ProjectileLaunchedEventHandler(Projectile projectile);
    [Signal] public delegate void AllProjectilesUsedEventHandler();

    [Export] private PackedScene _faceProjectileScene;
    [Export] private Slingshot _slingshot;
    [Export] private int _projectileCount = 3;

    private readonly Queue<Projectile> _projectileQueue = new();
    private bool _isLevelComplete;

    /// <summary>
    /// Returns true if at least one projectile remains to be loaded into the slingshot.
    /// </summary>
    public bool HasMoreProjectiles => _projectileQueue.Count > 0;

    public override void _Ready()
    {
        InitializeProjectiles();
        ConnectSignals();
        LoadNextProjectile();
    }

    private void InitializeProjectiles()
    {
        if (_faceProjectileScene == null)
            return;

        for (int i = 0; i < _projectileCount; i++)
        {
            var projectile = _faceProjectileScene.Instantiate<Projectile>();
            projectile.Freeze = true;
            AddChild(projectile);
            _projectileQueue.Enqueue(projectile);
        }
    }

    private void ConnectSignals()
    {
        if (SignalManager.Instance != null)
        {
            SignalManager.Instance.Connect(
                SignalManager.SignalName.OnAnimalDied,
                Callable.From(OnProjectileDied)
            );
        }

        if (_slingshot != null)
        {
            _slingshot.ProjectileLaunched += OnSlingshotProjectileLaunched;
        }
    }

    public override void _ExitTree()
    {
        if (_slingshot != null)
            _slingshot.ProjectileLaunched -= OnSlingshotProjectileLaunched;
    }

    private void OnProjectileDied()
    {
        CallDeferred(MethodName.LoadNextProjectile);
    }

    private void OnSlingshotProjectileLaunched(Projectile projectile)
    {
        EmitSignal(SignalName.ProjectileLaunched, projectile);
    }

    private void LoadNextProjectile()
    {
        if (_isLevelComplete)
            return;

        if (_projectileQueue.Count > 0)
        {
            var nextProjectile = _projectileQueue.Dequeue();

            if (_slingshot != null)
                _slingshot.LoadProjectile(nextProjectile);
        }
        else
        {
            _isLevelComplete = true;
            EmitSignal(SignalName.AllProjectilesUsed);
        }
    }
}
