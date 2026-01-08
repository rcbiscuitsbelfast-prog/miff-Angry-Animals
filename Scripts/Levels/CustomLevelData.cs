using Godot;
using System;
using System.Collections.Generic;
using System.Text;

/// <summary>
/// Serializable structure for player-created levels.
/// Supports JSON serialization and base64 encoding for sharing.
/// </summary>
[System.Serializable]
public class CustomLevelData
{
    public string LevelName { get; set; } = "Untitled Level";
    public string CreatorName { get; set; } = "Anonymous";
    public long CreatedTimestamp { get; set; }
    public float DifficultyRating { get; set; }
    public string DifficultyLabel { get; set; } = "Medium";
    public List<ObstacleData> Obstacles { get; set; } = new List<ObstacleData>();
    public int MaxAttempts { get; set; } = 3;

    [System.Serializable]
    public class ObstacleData
    {
        public MaterialType Material { get; set; }
        public float PositionX { get; set; }
        public float PositionY { get; set; }
        public float Rotation { get; set; }
        public float Scale { get; set; }
        public int Id { get; set; }

        public Vector2 Position
        {
            get => new Vector2(PositionX, PositionY);
            set
            {
                PositionX = value.X;
                PositionY = value.Y;
            }
        }

        public ObstacleData()
        {
            Material = MaterialType.Wood;
            Scale = 1.0f;
        }
    }

    public CustomLevelData()
    {
        CreatedTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    /// <summary>
    /// Converts the level data to a JSON string.
    /// </summary>
    public string ToJson()
    {
        return System.Text.Json.JsonSerializer.Serialize(this, new System.Text.Json.JsonSerializerOptions
        {
            WriteIndented = false
        });
    }

    /// <summary>
    /// Parses a CustomLevelData from a JSON string.
    /// </summary>
    public static CustomLevelData FromJson(string json)
    {
        try
        {
            return System.Text.Json.JsonSerializer.Deserialize<CustomLevelData>(json);
        }
        catch (Exception ex)
        {
            GD.PrintErr($"Failed to parse CustomLevelData from JSON: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Encodes the level as a base64 share code.
    /// Format: AA1_[base64_encoded_json]
    /// </summary>
    public string ToBase64()
    {
        try
        {
            string json = ToJson();
            byte[] bytes = Encoding.UTF8.GetBytes(json);
            string base64 = Convert.ToBase64String(bytes);
            return $"AA1_{base64}";
        }
        catch (Exception ex)
        {
            GD.PrintErr($"Failed to encode level to base64: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Decodes a CustomLevelData from a base64 share code.
    /// </summary>
    public static CustomLevelData FromBase64(string code)
    {
        try
        {
            if (!code.StartsWith("AA1_"))
            {
                GD.PrintErr("Invalid share code format - must start with AA1_");
                return null;
            }

            string base64 = code.Substring(4);
            byte[] bytes = Convert.FromBase64String(base64);
            string json = Encoding.UTF8.GetString(bytes);
            return FromJson(json);
        }
        catch (Exception ex)
        {
            GD.PrintErr($"Failed to decode level from base64: {ex.Message}");
            return null;
        }
    }
}
