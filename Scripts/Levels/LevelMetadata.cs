using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Serializable level metadata for both custom and generated levels
/// </summary>
[Serializable]
public class LevelMetadata
{
    public string LevelId { get; set; }
    public string LevelName { get; set; } = "Untitled Level";
    public string Theme { get; set; } = "generic";
    public string Description { get; set; } = "";
    
    public List<ItemInstance> Items { get; set; } = new();
    
    public Difficulty Difficulty { get; set; } = Difficulty.Medium;
    public int Goal { get; set; } = 30; // Blocks to destroy
    public int TargetTime { get; set; } = 120; // Seconds
    public int ParScore { get; set; } = 1000;
    
    public long CreatedTimestamp { get; set; }
    public string CreatorName { get; set; } = "System";
    public bool IsGenerated { get; set; } = false;
    
    public LevelMetadata()
    {
        CreatedTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }
}

[Serializable]
public class ItemInstance
{
    public string ItemId { get; set; }
    public Vector2 Position { get; set; }
    public float Rotation { get; set; } = 0f;
    public float Scale { get; set; } = 1.0f;
    public MaterialType MaterialOverride { get; set; } = MaterialType.Wood;
    
    public ItemInstance() { }
    
    public ItemInstance(string itemId, Vector2 position)
    {
        ItemId = itemId;
        Position = position;
    }
}

public enum Difficulty
{
    Easy = 0,
    Medium = 1,
    Hard = 2,
    Extreme = 3
}

/// <summary>
/// Difficulty analysis for levels
/// </summary>
public class DifficultyAnalysis
{
    public Difficulty Level { get; set; }
    public float OverallScore { get; set; }
    public string Description { get; set; }
    public List<string> Factors { get; set; } = new();
}