using Godot;

/// <summary>
/// Represents a game level.
/// Manages the overall level setup. Projectile loading is now handled by ProjectilesLoader.
/// </summary>
public partial class Level : Node2D
{
	public override void _Ready()
	{
	}
	
	public override void _Process(double delta)
	{
		if (Input.IsKeyPressed(Key.Q))
		{
			GameManager.LoadMain();
		}
	}
}
