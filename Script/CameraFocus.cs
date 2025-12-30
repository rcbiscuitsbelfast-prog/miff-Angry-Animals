using Godot;

/// <summary>
/// Controls camera follow and zoom behavior.
/// Follows launched projectiles and smoothly transitions between slingshot and projectile views.
/// </summary>
public partial class CameraFocus : Camera2D
{
    [Export] private Slingshot _slingshot;
    [Export] private Node2D _slingshotPosition;
    [Export] private float _followSpeed = 5.0f;
    [Export] private float _zoomSpeed = 3.0f;
    [Export] private Vector2 _defaultZoom = new Vector2(1.0f, 1.0f);
    [Export] private Vector2 _followZoom = new Vector2(0.8f, 0.8f);
    
    private Projectile _targetProjectile = null;
    private Vector2 _targetPosition = Vector2.Zero;
    private Vector2 _targetZoom = Vector2.One;
    private bool _isFollowingProjectile = false;
    
    public override void _Ready()
    {
        ConnectSignals();
        
        if (_slingshotPosition != null)
        {
            _targetPosition = _slingshotPosition.GlobalPosition;
            GlobalPosition = _targetPosition;
        }
        
        _targetZoom = _defaultZoom;
        Zoom = _targetZoom;
    }
    
    public override void _Process(double delta)
    {
        UpdateCameraPosition(delta);
        UpdateCameraZoom(delta);
    }
    
    private void ConnectSignals()
    {
        if (_slingshot != null)
        {
            _slingshot.ProjectileLaunched += OnProjectileLaunched;
        }
    }
    
    public override void _ExitTree()
    {
        if (_slingshot != null)
        {
            _slingshot.ProjectileLaunched -= OnProjectileLaunched;
        }

        if (_targetProjectile != null && IsInstanceValid(_targetProjectile))
        {
            _targetProjectile.AlmostStopped -= OnProjectileAlmostStopped;
        }
    }

    private void OnProjectileLaunched(Projectile projectile)
    {
        if (_targetProjectile != null && IsInstanceValid(_targetProjectile))
        {
            _targetProjectile.AlmostStopped -= OnProjectileAlmostStopped;
        }

        _targetProjectile = projectile;
        _isFollowingProjectile = true;
        _targetZoom = _followZoom;
        
        if (_targetProjectile != null)
        {
            _targetProjectile.AlmostStopped += OnProjectileAlmostStopped;
        }
    }

    private void OnProjectileAlmostStopped()
    {
        if (_targetProjectile != null && IsInstanceValid(_targetProjectile))
        {
            _targetProjectile.AlmostStopped -= OnProjectileAlmostStopped;
        }

        _isFollowingProjectile = false;
        _targetZoom = _defaultZoom;
        
        if (_slingshotPosition != null)
        {
            _targetPosition = _slingshotPosition.GlobalPosition;
        }
        
        _targetProjectile = null;
    }
    
    private void UpdateCameraPosition(double delta)
    {
        if (_isFollowingProjectile && _targetProjectile != null && IsInstanceValid(_targetProjectile))
        {
            _targetPosition = _targetProjectile.GlobalPosition;
        }
        
        GlobalPosition = GlobalPosition.Lerp(_targetPosition, _followSpeed * (float)delta);
    }
    
    private void UpdateCameraZoom(double delta)
    {
        Zoom = Zoom.Lerp(_targetZoom, _zoomSpeed * (float)delta);
    }
}
