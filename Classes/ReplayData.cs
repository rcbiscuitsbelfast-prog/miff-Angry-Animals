using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Godot;

/// <summary>
/// Represents a recorded gameplay replay
/// </summary>
[Serializable]
public class ReplayData
{
    [JsonProperty("replay_id")]
    public string ReplayId { get; set; } = Guid.NewGuid().ToString();
    
    [JsonProperty("player_id")]
    public string PlayerId { get; set; } = "";
    
    [JsonProperty("player_name")]
    public string PlayerName { get; set; } = "";
    
    [JsonProperty("level_id")]
    public string LevelId { get; set; } = "";
    
    [JsonProperty("level_name")]
    public string LevelName { get; set; } = "";
    
    [JsonProperty("score")]
    public int Score { get; set; }
    
    [JsonProperty("stars")]
    public int Stars { get; set; }
    
    [JsonProperty("completion_time")]
    public float CompletionTime { get; set; }
    
    [JsonProperty("recorded_date")]
    public DateTime RecordedDate { get; set; } = DateTime.UtcNow;
    
    [JsonProperty("input_events")]
    public List<ReplayInputEvent> InputEvents { get; set; } = new();
    
    [JsonProperty("physics_snapshots")]
    public List<PhysicsSnapshot> PhysicsSnapshots { get; set; } = new();
    
    [JsonProperty("starting_conditions")]
    public ReplayStartingConditions StartingConditions { get; set; } = new();
    
    [JsonProperty("player_cosmetics")]
    public FriendCosmetics PlayerCosmetics { get; set; } = new();
    
    [JsonProperty("view_count")]
    public int ViewCount { get; set; }
    
    [JsonProperty("share_count")]
    public int ShareCount { get; set; }
    
    [JsonProperty("is_perfect")]
    public bool IsPerfect { get; set; }
    
    [JsonProperty("file_size_bytes")]
    public long FileSizeBytes { get; set; }
    
    [JsonProperty("version")]
    public string Version { get; set; } = "1.0";
    
    /// <summary>
    /// Get estimated file size in KB
    /// </summary>
    public float GetFileSizeKB() => FileSizeBytes / 1024f;
    
    /// <summary>
    /// Check if replay is within size limit
    /// </summary>
    public bool IsWithinSizeLimit() => GetFileSizeKB() < 500f;
}

/// <summary>
/// Represents a single input event during replay
/// </summary>
[Serializable]
public class ReplayInputEvent
{
    [JsonProperty("timestamp")]
    public float Timestamp { get; set; }
    
    [JsonProperty("event_type")]
    public ReplayEventType EventType { get; set; }
    
    [JsonProperty("position_x")]
    public float PositionX { get; set; }
    
    [JsonProperty("position_y")]
    public float PositionY { get; set; }
    
    [JsonProperty("drag_angle")]
    public float DragAngle { get; set; }
    
    [JsonProperty("drag_strength")]
    public float DragStrength { get; set; }
    
    [JsonProperty("slingshot_type")]
    public int SlingshotType { get; set; }
}

/// <summary>
/// Physics snapshot for deterministic replay
/// </summary>
[Serializable]
public class PhysicsSnapshot
{
    [JsonProperty("timestamp")]
    public float Timestamp { get; set; }
    
    [JsonProperty("projectile_position_x")]
    public float ProjectilePositionX { get; set; }
    
    [JsonProperty("projectile_position_y")]
    public float ProjectilePositionY { get; set; }
    
    [JsonProperty("projectile_velocity_x")]
    public float ProjectileVelocityX { get; set; }
    
    [JsonProperty("projectile_velocity_y")]
    public float ProjectileVelocityY { get; set; }
    
    [JsonProperty("projectile_rotation")]
    public float ProjectileRotation { get; set; }
    
    [JsonProperty("destroyed_objects")]
    public List<string> DestroyedObjects { get; set; } = new();
}

/// <summary>
/// Starting conditions for replay
/// </summary>
[Serializable]
public class ReplayStartingConditions
{
    [JsonProperty("level_seed")]
    public int LevelSeed { get; set; }
    
    [JsonProperty("weather_condition")]
    public string WeatherCondition { get; set; } = "clear";
    
    [JsonProperty("difficulty_modifier")]
    public float DifficultyModifier { get; set; } = 1.0f;
    
    [JsonProperty("slingshot_type")]
    public int SlingshotType { get; set; }
    
    [JsonProperty("projectile_type")]
    public int ProjectileType { get; set; }
}

/// <summary>
/// Replay event types
/// </summary>
public enum ReplayEventType
{
    DragStart,
    DragUpdate,
    DragEnd,
    Launch,
    Impact,
    LevelComplete
}

/// <summary>
/// Compressed shareable replay format
/// </summary>
[Serializable]
public class ShareableReplay
{
    [JsonProperty("replay_data")]
    public ReplayData ReplayData { get; set; } = new();
    
    [JsonProperty("encoded_string")]
    public string EncodedString { get; set; } = "";
    
    [JsonProperty("share_url")]
    public string ShareUrl { get; set; } = "";
    
    [JsonProperty("share_message")]
    public string ShareMessage { get; set; } = "";
    
    /// <summary>
    /// Generate share URL for this replay
    /// </summary>
    public string GenerateShareUrl()
    {
        ShareUrl = $"game://replay/{EncodedString}";
        return ShareUrl;
    }
    
    /// <summary>
    /// Generate share message for social media
    /// </summary>
    public string GenerateShareMessage(string platform = "default")
    {
        var stars = new string('⭐', ReplayData.Stars);
        
        return platform switch
        {
            "twitter" => $"I got {stars} on {ReplayData.LevelName} with {ReplayData.Score} points! Beat my score! {ShareUrl}",
            "discord" => $"Check out my {stars} run on {ReplayData.LevelName}! Score: {ReplayData.Score}. Can you do better? {ShareUrl}",
            "whatsapp" => $"Hey! I just got {ReplayData.Score} points on {ReplayData.LevelName}. Think you can beat it? {ShareUrl}",
            _ => $"I scored {ReplayData.Score} on {ReplayData.LevelName}! Beat my score: {ShareUrl}"
        };
    }
}
