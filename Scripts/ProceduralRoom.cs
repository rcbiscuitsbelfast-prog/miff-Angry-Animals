using Godot;
using System;

/// <summary>
/// Procedurally generated room that uses LevelGenerator to create cup layouts dynamically.
/// Extends RoomBase to inherit core gameplay mechanics.
/// </summary>
public partial class ProceduralRoom : RoomBase
{
    [Export] private PackedScene _cupScene;
    [Export] private NodePath _obstaclesParentPath;
    [Export] private NodePath _backgroundRectPath;

    private Node2D? _obstaclesParent;
    private ColorRect? _backgroundRect;

    public override void _Ready()
    {
        GenerateProceduralLevel();
        base._Ready();
    }

    private void GenerateProceduralLevel()
    {
        if (_cupScene == null)
        {
            GD.PushError("ProceduralRoom: Cup scene not set.");
            return;
        }

        _obstaclesParent = GetNodeOrNull<Node2D>(_obstaclesParentPath);
        _backgroundRect = GetNodeOrNull<ColorRect>(_backgroundRectPath);

        if (_obstaclesParent == null)
        {
            GD.PushError("ProceduralRoom: Obstacles parent not found.");
            return;
        }

        var currentRoomIndex = GameManager.Instance?.CurrentRoomIndex ?? 0;
        var roomNumber = currentRoomIndex + 1;

        var seed = GameManager.Instance?.CurrentProceduralSeed ?? LevelGenerator.CalculateSeed(roomNumber);

        ApplyTheme(roomNumber);
        SpawnProceduralCups(roomNumber, seed);

        GD.Print($"Generated procedural room {roomNumber} with seed {seed}.");
    }

    private void ApplyTheme(int roomNumber)
    {
        var theme = LevelGenerator.GetTheme(roomNumber);
        var backgroundColor = LevelGenerator.GetBackgroundColor(roomNumber);

        if (_backgroundRect != null)
            _backgroundRect.Color = backgroundColor;

        var floorRect = GetNodeOrNull<ColorRect>("Environment/Floor/ColorRect");
        if (floorRect != null)
            floorRect.Color = theme.FloorColor;
    }

    private void SpawnProceduralCups(int roomNumber, int seed)
    {
        foreach (Node child in _obstaclesParent!.GetChildren())
            child.QueueFree();

        int cupCount = LevelGenerator.GetCupCount(roomNumber);
        
        var cupConfigs = LevelGenerator.GenerateCups(roomNumber, cupCount, seed);

        int totalPossibleScore = 0;
        foreach (var config in cupConfigs)
        {
            totalPossibleScore += (int)config.Material * 50;
        }

        // Target 40% of total possible score to unlock door, minimum 50
        _targetScore = Math.Max(50, (int)(totalPossibleScore * 0.4f));

        for (int i = 0; i < cupConfigs.Length; i++)
            SpawnCup(cupConfigs[i], i);
    }

    private void SpawnCup(LevelGenerator.CupConfig config, int index)
    {
        var cupInstance = _cupScene!.Instantiate<Node2D>();
        cupInstance.Name = $"Cup{index + 1}";
        cupInstance.Position = config.Position;
        cupInstance.Rotation = config.Rotation;
        cupInstance.Scale = new Vector2(config.Scale, config.Scale);

        // Ensure it's in the cup group if it's not already
        if (!cupInstance.IsInGroup(Cup.GROUP_NAME))
            cupInstance.AddToGroup(Cup.GROUP_NAME);

        // Apply material properties to the cup
        if (cupInstance is BreakableObstacle breakableObstacle)
        {
            breakableObstacle.SetMaterial(config.Material);
            GD.Print($"Applied material {config.Material} to cup {index + 1} at position {config.Position}");
        }

        _obstaclesParent!.AddChild(cupInstance);
    }
}
