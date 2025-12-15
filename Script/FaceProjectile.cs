using Godot;

/// <summary>
/// A projectile with a face sprite, representing an animal character.
/// Extends the base Projectile class with visual representation.
/// </summary>
public partial class FaceProjectile : Projectile
{
	[Export] private Sprite2D _faceSprite;
	
	public override void _Ready()
	{
		base._Ready();
	}
}
