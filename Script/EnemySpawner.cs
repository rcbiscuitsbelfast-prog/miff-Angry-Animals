using Godot;

/// <summary>
/// Manages enemy spawning in levels.
/// Spawns enemies at random positions within specified area.
/// Ported from Angry Aliens EnemySpawner
/// </summary>
public partial class EnemySpawner : Node
{
	[Export] public PackedScene EnemyScene { get; set; }
	[Export] public float SpawnInterval { get; set; } = 10.0f;
	[Export] public int MaxEnemies { get; set; } = 3;
	[Export] public Vector2 SpawnAreaMin { get; set; } = new Vector2(-200, -100);
	[Export] public Vector2 SpawnAreaMax { get; set; } = new Vector2(200, -50);

	private Timer _spawnTimer;
	private int _enemyCount = 0;

	public override void _Ready()
	{
		_spawnTimer = new Timer()
		{
			WaitTime = SpawnInterval,
			OneShot = false,
			Autostart = true
		};
		AddChild(_spawnTimer);
		_spawnTimer.Timeout += SpawnEnemy;
		_spawnTimer.Start();
	}

	public void SpawnEnemy()
	{
		if (_enemyCount >= MaxEnemies)
		{
			return;
		}

		if (EnemyScene == null)
		{
			GD.PrintErr("EnemySpawner: No EnemyScene set!");
			return;
		}

		var enemy = EnemyScene.Instantiate<FighterEnemy>();
		enemy.GlobalPosition = new Vector2(
			GD.RandfRange(SpawnAreaMin.X, SpawnAreaMax.X),
			GD.RandfRange(SpawnAreaMin.Y, SpawnAreaMax.Y)
		);

		enemy.Destroyed += OnEnemyDestroyed;
		GetParent().AddChild(enemy);
		_enemyCount++;

		GD.Print($"EnemySpawner: Spawned enemy at {enemy.GlobalPosition}. Total: {_enemyCount}");
	}

	private void OnEnemyDestroyed(Node enemy, Node collider, Vector2 impactMomentum)
	{
		_enemyCount--;
		GD.Print($"EnemySpawner: Enemy destroyed. Remaining: {_enemyCount}");
	}

	/// <summary>
	/// Pauses enemy spawning.
	/// </summary>
	public void PauseSpawning()
	{
		if (_spawnTimer != null)
		{
			_spawnTimer.Paused = true;
		}
	}

	/// <summary>
	/// Resumes enemy spawning.
	/// </summary>
	public void ResumeSpawning()
	{
		if (_spawnTimer != null)
		{
			_spawnTimer.Paused = false;
		}
	}
}
