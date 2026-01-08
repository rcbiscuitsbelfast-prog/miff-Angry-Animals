using Godot;
using System;

/// <summary>
/// Room for playing custom levels created by players.
/// Extends RoomBase to integrate with existing game systems.
/// </summary>
public partial class CustomPlayRoom : RoomBase
{
    [Export] private PackedScene _obstacleScene;
    [Export] private NodePath _obstaclesParentPath;

    private CustomLevelData _customLevel;
    private Node2D _obstaclesParent;

    public void LoadCustomLevel(CustomLevelData level)
    {
        _customLevel = level;
        GD.Print($"Loading custom level: {level.LevelName}");
    }

    public override void _Ready()
    {
        _obstaclesParent = GetNodeOrNull<Node2D>(_obstaclesParentPath);
        if (_obstaclesParent == null)
        {
            _obstaclesParent = GetNodeOrNull<Node2D>("Obstacles");
        }

        if (_customLevel != null)
        {
            GenerateCustomLevel();
        }

        base._Ready();
    }

    private void GenerateCustomLevel()
    {
        if (_customLevel == null)
        {
            GD.PrintErr("No custom level loaded");
            return;
        }

        if (_obstaclesParent == null)
        {
            GD.PrintErr("Obstacles parent node not found");
            return;
        }

        // Clear any existing obstacles
        foreach (Node child in _obstaclesParent.GetChildren())
        {
            child.QueueFree();
        }

        // Set target score based on obstacle count
        _targetScore = Mathf.Max(1, (int)(_customLevel.Obstacles.Count * 0.3f));

        // Spawn each obstacle from custom level data
        for (int i = 0; i < _customLevel.Obstacles.Count; i++)
        {
            var obstacleData = _customLevel.Obstacles[i];
            SpawnCustomObstacle(obstacleData, i);
        }

        // Log difficulty for player
        GD.Print($"Custom Level: {_customLevel.LevelName}");
        GD.Print($"Created by: {_customLevel.CreatorName}");
        GD.Print($"Difficulty: {_customLevel.DifficultyLabel} ({_customLevel.DifficultyRating:F2})");
        GD.Print($"Obstacles: {_customLevel.Obstacles.Count}");
        GD.Print($"Target Score: {_targetScore}");
    }

    private void SpawnCustomObstacle(CustomLevelData.ObstacleData data, int index)
    {
        PackedScene obstaclePackedScene = _obstacleScene;
        
        // If no obstacle scene is provided, try to load default cup scene
        if (obstaclePackedScene == null)
        {
            obstaclePackedScene = GD.Load<PackedScene>("res://Scenes/Obstacles/Cup.tscn");
        }

        if (obstaclePackedScene == null)
        {
            GD.PrintErr("Obstacle scene not found");
            return;
        }

        var obstacle = obstaclePackedScene.Instantiate<Node2D>();
        obstacle.Name = $"CustomObstacle_{index}";
        obstacle.Position = data.Position;
        obstacle.Rotation = data.Rotation;
        obstacle.Scale = new Vector2(data.Scale, data.Scale);

        // Apply material properties if the obstacle supports it
        if (obstacle is Cup cup)
        {
            cup.SetMaterial(data.Material);
        }
        else if (obstacle is BreakableObstacle breakable)
        {
            breakable.SetMaterial(data.Material);
        }

        _obstaclesParent.AddChild(obstacle);
        
        GD.Print($"Spawned obstacle {index}: {data.Material} at {data.Position}");
    }
}
