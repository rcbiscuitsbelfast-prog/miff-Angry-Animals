using Godot;

/// <summary>
/// Represents a game level.
/// Spawns animals at the defined marker and listens for their death to respawn a new one.
/// </summary>
public partial class Level : Node2D
{
	[Export] PackedScene _animalScene;
	[Export] Marker2D _spawnMarker;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		if (_animalScene == null)
		{
			GD.PrintErr("Animal scene not assigned in Level");
			return;
		}

		if (_spawnMarker == null)
		{
			GD.PrintErr("Spawn marker not assigned in Level");
			return;
		}

		SpawnAnimal();

		if (SignalManager.Instance != null)
			SignalManager.Instance.Connect(SignalManager.SignalName.OnAnimalDied, Callable.From(SpawnAnimal));
	}

	public override void _Process(double delta)
	{
		if (Input.IsKeyPressed(Key.Q)) GameManager.LoadMain();
	}

	/// <summary>
	/// Instanciates and spawns a new animal at the spawn marker position.
	/// </summary>
	private void SpawnAnimal()
	{
		if (_animalScene == null || _spawnMarker == null) return;

		Animal newAnimal = _animalScene.Instantiate<Animal>();
		newAnimal.Position = _spawnMarker.Position;
		AddChild(newAnimal);
	}
}
