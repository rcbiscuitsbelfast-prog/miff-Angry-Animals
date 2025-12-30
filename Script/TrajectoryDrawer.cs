using Godot;

/// <summary>
/// Draws a visual trajectory line and arrow to show launch direction and strength.
/// Updates in real-time during drag operations.
/// Supports dynamic line colors based on drag intensity.
/// </summary>
public partial class TrajectoryDrawer : Node2D
{
    [Export] private Line2D _trajectoryLine;
    [Export] private Sprite2D _arrow;

    private float _arrowXScale = 1.0f;
    private const float IMPULSE_MAX = 1200.0f;
    private const int MAX_LINE_POINTS = 20;

    // Colors for different drag intensities
    private readonly Color _weakColor = Colors.Green;
    private readonly Color _mediumColor = Colors.Yellow;
    private readonly Color _strongColor = Colors.Red;

    public override void _Ready()
    {
        if (_arrow != null)
        {
            _arrowXScale = _arrow.Scale.X;
            _arrow.Visible = false;
        }

        if (_trajectoryLine != null)
        {
            _trajectoryLine.Visible = false;
            _trajectoryLine.Width = 3.0f;
        }
    }

    public void ShowTrajectory(Vector2 dragVector, Vector2 impulse)
    {
        if (_arrow != null)
        {
            _arrow.Visible = true;
            UpdateArrowScale(impulse, dragVector);
        }

        if (_trajectoryLine != null)
        {
            _trajectoryLine.Visible = true;
            UpdateTrajectoryLine(dragVector, impulse);
            UpdateLineColor(impulse);
        }
    }

    public void HideTrajectory()
    {
        if (_arrow != null)
        {
            _arrow.Visible = false;
        }

        if (_trajectoryLine != null)
        {
            _trajectoryLine.Visible = false;
            _trajectoryLine.ClearPoints();
        }
    }

    private void UpdateArrowScale(Vector2 impulse, Vector2 dragVector)
    {
        if (_arrow == null) return;

        float impulseLength = impulse.Length();
        float scaleFactor = Mathf.Clamp(impulseLength / IMPULSE_MAX, 0.1f, 1.5f);

        // Scale arrow based on impulse magnitude
        _arrow.Scale = new Vector2(_arrowXScale * scaleFactor, _arrow.Scale.Y);

        // Rotate arrow to point in launch direction (opposite of drag)
        _arrow.Rotation = -dragVector.Angle();
    }

    private void UpdateTrajectoryLine(Vector2 dragVector, Vector2 impulse)
    {
        if (_trajectoryLine == null) return;

        _trajectoryLine.ClearPoints();

        // Calculate trajectory points
        Vector2 startPoint = -dragVector; // Start from slingshot center
        Vector2 launchDirection = impulse.Normalized();
        float launchSpeed = impulse.Length() / IMPULSE_MAX; // 0-1 scale

        // Simulate projectile path (parabolic arc)
        float gravity = 980f; // Standard Godot gravity

        for (int i = 0; i < MAX_LINE_POINTS; i++)
        {
            float t = i * 0.05f; // Time step

            // Simple projectile motion: position = velocity * t + 0.5 * gravity * t^2
            float x = launchDirection.X * launchSpeed * IMPULSE_MAX * t * 0.05f;
            float y = launchDirection.Y * launchSpeed * IMPULSE_MAX * t * 0.05f + 0.5f * gravity * t * t * 0.05f;

            _trajectoryLine.AddPoint(new Vector2(x, y));
        }
    }

    private void UpdateLineColor(Vector2 impulse)
    {
        if (_trajectoryLine == null) return;

        float impulsePercent = impulse.Length() / IMPULSE_MAX;

        // Color gradient from weak to strong
        Color lineColor;
        if (impulsePercent < 0.5f)
        {
            lineColor = _weakColor.Lerp(_mediumColor, impulsePercent * 2f);
        }
        else
        {
            lineColor = _mediumColor.Lerp(_strongColor, (impulsePercent - 0.5f) * 2f);
        }

        _trajectoryLine.DefaultColor = lineColor;
    }

    /// <summary>
    /// Updates the trajectory line with a custom prediction algorithm.
    /// </summary>
    public void UpdateTrajectoryPrediction(Vector2 startPosition, Vector2 launchVelocity)
    {
        if (_trajectoryLine == null) return;

        _trajectoryLine.ClearPoints();

        float gravity = 980f;
        float timeStep = 0.08f;

        for (int i = 0; i < MAX_LINE_POINTS; i++)
        {
            float t = i * timeStep;

            // Projectile motion formula
            float x = startPosition.X + launchVelocity.X * t;
            float y = startPosition.Y + launchVelocity.Y * t + 0.5f * gravity * t * t;

            _trajectoryLine.AddPoint(new Vector2(x, y));

            // Stop if below floor
            if (y > 530f) break;
        }
    }
}
