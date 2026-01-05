using Godot;

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
        base._Ready();
        GenerateProceduralLevel();
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
        int targetScore = cupCount;

        // Ensure the gameplay target matches the difficulty.
        // RoomBase reads its target score from GameManager on _Ready, so we update after base._Ready.
        // This keeps manual rooms unchanged while making procedural rooms scale.
        _targetScore = targetScore;

        var cupConfigs = LevelGenerator.GenerateCups(roomNumber, cupCount, seed);

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

        _obstaclesParent!.AddChild(cupInstance);
    }
}
