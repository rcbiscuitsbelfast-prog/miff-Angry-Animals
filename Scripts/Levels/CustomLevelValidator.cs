using Godot;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Validates player-created custom levels to ensure they are playable and balanced.
/// </summary>
public static class CustomLevelValidator
{
    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public string Message { get; set; }
        public float DifficultyScore { get; set; }
        public List<string> Warnings { get; set; } = new List<string>();
        public ValidationLevel Level { get; set; }
    }

    public enum ValidationLevel
    {
        Valid,
        Warning,
        Invalid
    }

    private const int MIN_OBSTACLES = 3;
    private const int MAX_OBSTACLES = 20;
    private const float OVERLAP_THRESHOLD = 50f;
    private const float PLAYABLE_MIN_X = 300f;
    private const float PLAYABLE_MAX_X = 950f;
    private const float PLAYABLE_MIN_Y = 50f;
    private const float PLAYABLE_MAX_Y = 530f;

    /// <summary>
    /// Validates a custom level and returns detailed results.
    /// </summary>
    public static ValidationResult ValidateLevel(CustomLevelData level)
    {
        if (level == null)
        {
            return Failed("Level data is null");
        }

        // Check 1: Obstacle count (min 3, max 20)
        if (level.Obstacles.Count < MIN_OBSTACLES)
        {
            return Failed($"Level needs at least {MIN_OBSTACLES} obstacles (currently has {level.Obstacles.Count})");
        }

        if (level.Obstacles.Count > MAX_OBSTACLES)
        {
            return Failed($"Level has too many obstacles (max {MAX_OBSTACLES}, currently has {level.Obstacles.Count})");
        }

        // Check 2: Material distribution (not all same material)
        var uniqueMaterials = level.Obstacles.Select(o => o.Material).Distinct().Count();
        if (uniqueMaterials < 2)
        {
            return Failed("Level must have at least 2 different materials for variety");
        }

        // Check 3: Difficulty balance
        var difficulty = CalculateCustomLevelDifficulty(level);
        level.DifficultyRating = difficulty.OverallScore;
        level.DifficultyLabel = difficulty.Description;

        var warnings = new List<string>();

        if (difficulty.OverallScore < 0.2f)
        {
            warnings.Add($"Level is very easy ({difficulty.Description}). Consider adding harder materials.");
        }
        else if (difficulty.OverallScore > 0.95f)
        {
            warnings.Add($"Level is extremely hard ({difficulty.Description}). Consider using softer materials.");
        }

        // Check 4: Obstacle positioning
        if (HasOverlappingObstacles(level))
        {
            warnings.Add("Some obstacles overlap - gameplay may be confusing");
        }

        if (HasOutOfBoundsObstacles(level, out var outOfBoundsCount))
        {
            return Failed($"{outOfBoundsCount} obstacle(s) are outside the playable area");
        }

        // Check 5: Reachability
        if (!HasReachableObstacles(level))
        {
            warnings.Add("Some obstacles may be unreachable from the slingshot - check positioning");
        }

        // All checks passed
        if (warnings.Count > 0)
        {
            return Warning($"Level '{level.LevelName}' is playable with minor issues. Difficulty: {difficulty.Description}", 
                         difficulty.OverallScore, warnings);
        }

        return Success($"Level '{level.LevelName}' is playable! Difficulty: {difficulty.Description}", 
                      difficulty.OverallScore);
    }

    /// <summary>
    /// Calculates difficulty metrics for a custom level.
    /// </summary>
    public static DifficultyBalancer.RoomDifficulty CalculateCustomLevelDifficulty(CustomLevelData level)
    {
        // Calculate material difficulty (average hardness)
        float totalHardness = 0f;
        foreach (var obstacle in level.Obstacles)
        {
            totalHardness += (int)obstacle.Material;
        }
        float avgHardness = totalHardness / level.Obstacles.Count;
        float materialDifficulty = (avgHardness - 1f) / 4f; // Normalize from 1-5 to 0-1

        // Calculate obstacle count difficulty
        float obstacleCountDiff = Mathf.Clamp(level.Obstacles.Count / 15f, 0f, 1f);

        // Estimate layout difficulty based on obstacle spread
        float layoutDifficulty = EstimateLayoutDifficulty(level);

        // Weighted average
        float overallScore = (materialDifficulty * 0.5f) + (obstacleCountDiff * 0.3f) + (layoutDifficulty * 0.2f);
        overallScore = Mathf.Clamp(overallScore, 0f, 1f);

        string description = overallScore switch
        {
            < 0.3f => "Easy",
            < 0.6f => "Medium",
            < 0.85f => "Hard",
            _ => "Extreme"
        };

        return new DifficultyBalancer.RoomDifficulty
        {
            OverallScore = overallScore,
            MaterialDifficulty = materialDifficulty,
            ObstacleCountDifficulty = obstacleCountDiff,
            LayoutDifficulty = layoutDifficulty,
            Description = description
        };
    }

    private static float EstimateLayoutDifficulty(CustomLevelData level)
    {
        if (level.Obstacles.Count < 2) return 0.3f;

        // Calculate average distance between obstacles
        float totalDistance = 0f;
        int pairCount = 0;

        for (int i = 0; i < level.Obstacles.Count - 1; i++)
        {
            for (int j = i + 1; j < level.Obstacles.Count; j++)
            {
                var pos1 = level.Obstacles[i].Position;
                var pos2 = level.Obstacles[j].Position;
                totalDistance += pos1.DistanceTo(pos2);
                pairCount++;
            }
        }

        float avgDistance = totalDistance / pairCount;

        // Scattered (high distance) = harder, clustered (low distance) = easier
        // Normalize: 50-300 units distance -> 0.3-0.9 difficulty
        float layoutDiff = Mathf.Clamp((avgDistance - 50f) / 250f, 0.3f, 0.9f);
        return layoutDiff;
    }

    private static bool HasOverlappingObstacles(CustomLevelData level)
    {
        for (int i = 0; i < level.Obstacles.Count - 1; i++)
        {
            for (int j = i + 1; j < level.Obstacles.Count; j++)
            {
                var pos1 = level.Obstacles[i].Position;
                var pos2 = level.Obstacles[j].Position;
                
                if (pos1.DistanceTo(pos2) < OVERLAP_THRESHOLD)
                {
                    return true;
                }
            }
        }
        return false;
    }

    private static bool HasOutOfBoundsObstacles(CustomLevelData level, out int outOfBoundsCount)
    {
        outOfBoundsCount = 0;
        
        foreach (var obstacle in level.Obstacles)
        {
            var pos = obstacle.Position;
            if (pos.X < PLAYABLE_MIN_X || pos.X > PLAYABLE_MAX_X ||
                pos.Y < PLAYABLE_MIN_Y || pos.Y > PLAYABLE_MAX_Y)
            {
                outOfBoundsCount++;
            }
        }

        return outOfBoundsCount > 0;
    }

    private static bool HasReachableObstacles(CustomLevelData level)
    {
        // Simple heuristic: at least one obstacle should be within reasonable range of slingshot
        const float SLINGSHOT_X = 200f;
        const float MAX_REACH = 600f;

        int reachableCount = 0;
        foreach (var obstacle in level.Obstacles)
        {
            float distance = Mathf.Abs(obstacle.Position.X - SLINGSHOT_X);
            if (distance < MAX_REACH)
            {
                reachableCount++;
            }
        }

        // At least 50% of obstacles should be reachable
        return reachableCount >= level.Obstacles.Count * 0.5f;
    }

    private static ValidationResult Success(string message, float difficultyScore)
    {
        return new ValidationResult
        {
            IsValid = true,
            Message = message,
            DifficultyScore = difficultyScore,
            Level = ValidationLevel.Valid
        };
    }

    private static ValidationResult Warning(string message, float difficultyScore, List<string> warnings)
    {
        return new ValidationResult
        {
            IsValid = true,
            Message = message,
            DifficultyScore = difficultyScore,
            Warnings = warnings,
            Level = ValidationLevel.Warning
        };
    }

    private static ValidationResult Failed(string message)
    {
        return new ValidationResult
        {
            IsValid = false,
            Message = message,
            DifficultyScore = 0f,
            Level = ValidationLevel.Invalid
        };
    }
}
