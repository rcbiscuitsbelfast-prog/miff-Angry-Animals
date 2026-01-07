using Godot;
using System;
using System.Linq;

/// <summary>
/// Balances procedural level difficulty based on material distribution, obstacle count, and layout patterns.
/// Provides metrics for designer review and ensures a smooth difficulty curve.
/// </summary>
public partial class DifficultyBalancer : Node
{
    public static DifficultyBalancer Instance { get; private set; }

    [Export] private float MinDifficultyJump = 0.05f;
    [Export] private float MaxDifficultyJump = 0.15f;
    [Export] private float SoftMaterialObstacleBonus = 1.2f; // +20% obstacle count cap for soft materials

    public struct RoomDifficulty
    {
        public float OverallScore; // 0.0-1.0
        public float MaterialDifficulty;
        public float ObstacleCountDifficulty;
        public float LayoutDifficulty;
        public string Description; // "Easy", "Medium", "Hard", etc.
    }

    public override void _Ready()
    {
        Instance = this;
    }

    /// <summary>
    /// Calculates the difficulty metrics for a specific room.
    /// </summary>
    public static RoomDifficulty CalculateRoomDifficulty(int roomNumber)
    {
        int cupCount = LevelGenerator.GetCupCount(roomNumber);
        var pattern = LevelGenerator.GetPatternForRoom(roomNumber);
        float softness = MaterialDistributor.GetDifficultySoftness(roomNumber);

        float materialDifficulty = 1.0f - softness;
        
        // Normalize obstacle count difficulty (max around 15 as per requirements)
        float obstacleCountDiff = Mathf.Clamp(cupCount / 15f, 0f, 1f);

        // Patterns: Scattered is hardest, Tower is easiest
        float layoutDifficulty = pattern switch
        {
            LevelGenerator.ObstaclePattern.Tower => 0.3f,
            LevelGenerator.ObstaclePattern.Wall => 0.6f,
            LevelGenerator.ObstaclePattern.Scattered => 0.9f,
            _ => 0.5f
        };

        // Weighted average for overall score
        float overallScore = (materialDifficulty * 0.5f) + (obstacleCountDiff * 0.3f) + (layoutDifficulty * 0.2f);
        overallScore = Mathf.Clamp(overallScore, 0f, 1f);

        string description = overallScore switch
        {
            < 0.3f => "Easy",
            < 0.6f => "Medium",
            < 0.85f => "Hard",
            _ => "Extreme"
        };

        var difficulty = new RoomDifficulty
        {
            OverallScore = overallScore,
            MaterialDifficulty = materialDifficulty,
            ObstacleCountDifficulty = obstacleCountDiff,
            LayoutDifficulty = layoutDifficulty,
            Description = description
        };

        if (MaterialDistributor.Instance?.EnableDebugLogging ?? true)
        {
            GD.Print($"Difficulty for Room {roomNumber}: {description} (Score: {overallScore:F2}) [Mat: {materialDifficulty:F2}, Count: {obstacleCountDiff:F2}, Layout: {layoutDifficulty:F2}]");
        }

        return difficulty;
    }

    /// <summary>
    /// Returns the recommended maximum obstacle count for a room based on its material hardness.
    /// </summary>
    public static int GetRecommendedMaxObstacles(int roomNumber)
    {
        float softness = MaterialDistributor.GetDifficultySoftness(roomNumber);
        
        if (softness < 0.3f) // Hard rooms (Iron/Diamond)
            return 8;
        
        if (softness > 0.7f) // Softer rooms
            return 15;

        return 12;
    }
}
