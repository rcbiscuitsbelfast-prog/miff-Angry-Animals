using Godot;

/// <summary>
/// Draws a visual trajectory line and arrow to show launch direction and strength.
/// Updates in real-time during drag operations.
/// </summary>
public partial class TrajectoryDrawer : Node2D
{
	[Export] private Line2D _trajectoryLine;
	[Export] private Sprite2D _arrow;
	
	private float _arrowXScale = 0.0f;
	private const float IMPULSE_MAX = 1200.0f;
	
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
		}
	}
	
	private void UpdateArrowScale(Vector2 impulse, Vector2 dragVector)
	{
		if (_arrow == null) return;
		
		float impulseLength = impulse.Length();
		float scaleFactor = impulseLength / IMPULSE_MAX;
		
		_arrow.Scale = new Vector2((_arrowXScale * scaleFactor) + _arrowXScale, _arrow.Scale.Y);
		_arrow.Rotation = -dragVector.Angle();
	}
}
