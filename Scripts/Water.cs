using Godot;

/// <summary>
/// Represents a water area in the level.
/// Plays a splash sound whenever an animal enters.
/// </summary>
public partial class Water : Area2D
{
	[Export] AudioStreamPlayer2D _splashSound;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		// Triggers the splash sound.
		BodyEntered += OnAnimalDied;
	}


    /// <summary>
    /// Plays the splash sound when a body enters the water.
    /// </summary>
    /// <param name="body">The node that entered the water (expected to be an animal).</param>
    private void OnAnimalDied(Node2D body) => _splashSound.Play();
}