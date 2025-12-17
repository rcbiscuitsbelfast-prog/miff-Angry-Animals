using Godot;
using System.Collections.Generic;

/// <summary>
/// Manages the queue of projectiles to be loaded into the slingshot.
/// Listens for animal death events and loads the next projectile automatically.
/// Emits level completion when all projectiles are used.
/// </summary>
public partial class ProjectilesLoader : Node2D
{
    [Signal] public delegate void ProjectileLaunchedEventHandler(Projectile projectile);
    [Signal] public delegate void AllProjectilesUsedEventHandler();

    [Export] private PackedScene _faceProjectileScene;
    [Export] private Slingshot _slingshot;
    [Export] private int _projectileCount = 3;
    
    private Queue<Projectile> _projectileQueue = new Queue<Projectile>();
    private bool _isLevelComplete = false;

    public bool HasMoreProjectiles => _projectileQueue.Count > 0;
    
    public override void _Ready()
    {
        InitializeProjectiles();
        ConnectSignals();
        LoadNextProjectile();
    }
    
    private void InitializeProjectiles()
    {
        if (_faceProjectileScene == null) return;
        
        for (int i = 0; i < _projectileCount; i++)
        {
            Projectile projectile = _faceProjectileScene.Instantiate<Projectile>();
            projectile.Freeze = true;
            AddChild(projectile);
            _projectileQueue.Enqueue(projectile);
        }
    }
    
    private void ConnectSignals()
    {
        SignalManager.Instance.Connect(
            SignalManager.SignalName.OnAnimalDied,
            Callable.From(OnProjectileDied)
        );
    }
    
    private void OnProjectileDied()
    {
        CallDeferred(MethodName.LoadNextProjectile);
    }
    
    private void LoadNextProjectile()
    {
        if (_isLevelComplete) return;
        
        if (_projectileQueue.Count > 0)
        {
            Projectile nextProjectile = _projectileQueue.Dequeue();
            
            if (_slingshot != null)
            {
                _slingshot.LoadProjectile(nextProjectile);
            }
        }
        else
        {
            _isLevelComplete = true;
            EmitSignal(SignalName.AllProjectilesUsed);
        }
    }
}
