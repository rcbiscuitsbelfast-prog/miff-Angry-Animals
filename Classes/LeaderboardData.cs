using System;
using System.Collections.Generic;
using Newtonsoft.Json;

/// <summary>
/// Represents a leaderboard entry
/// </summary>
[Serializable]
public class LeaderboardEntry
{
    [JsonProperty("player_id")]
    public string PlayerId { get; set; } = "";
    
    [JsonProperty("player_name")]
    public string PlayerName { get; set; } = "";
    
    [JsonProperty("score")]
    public int Score { get; set; }
    
    [JsonProperty("rank")]
    public int Rank { get; set; }
    
    [JsonProperty("stars")]
    public int Stars { get; set; }
    
    [JsonProperty("completion_time")]
    public float CompletionTime { get; set; }
    
    [JsonProperty("date_achieved")]
    public DateTime DateAchieved { get; set; } = DateTime.UtcNow;
    
    [JsonProperty("cosmetics")]
    public FriendCosmetics Cosmetics { get; set; } = new();
    
    [JsonProperty("replay_id")]
    public string ReplayId { get; set; } = "";
    
    [JsonProperty("is_friend")]
    public bool IsFriend { get; set; }
    
    [JsonProperty("is_current_player")]
    public bool IsCurrentPlayer { get; set; }
}

/// <summary>
/// Leaderboard data container
/// </summary>
[Serializable]
public class LeaderboardData
{
    [JsonProperty("leaderboard_id")]
    public string LeaderboardId { get; set; } = "";
    
    [JsonProperty("leaderboard_type")]
    public LeaderboardType Type { get; set; }
    
    [JsonProperty("level_id")]
    public string LevelId { get; set; } = "";
    
    [JsonProperty("entries")]
    public List<LeaderboardEntry> Entries { get; set; } = new();
    
    [JsonProperty("last_updated")]
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    
    [JsonProperty("total_players")]
    public int TotalPlayers { get; set; }
    
    [JsonProperty("current_player_rank")]
    public int CurrentPlayerRank { get; set; }
    
    [JsonProperty("is_cached")]
    public bool IsCached { get; set; }
    
    /// <summary>
    /// Get top N entries
    /// </summary>
    public List<LeaderboardEntry> GetTopEntries(int count = 100)
    {
        return Entries.Count > count ? Entries.GetRange(0, count) : Entries;
    }
    
    /// <summary>
    /// Get friend entries only
    /// </summary>
    public List<LeaderboardEntry> GetFriendEntries()
    {
        return Entries.FindAll(e => e.IsFriend);
    }
    
    /// <summary>
    /// Find player's entry
    /// </summary>
    public LeaderboardEntry? FindPlayerEntry(string playerId)
    {
        return Entries.Find(e => e.PlayerId == playerId);
    }
    
    /// <summary>
    /// Check if data needs refresh (older than 5 minutes)
    /// </summary>
    public bool NeedsRefresh()
    {
        return (DateTime.UtcNow - LastUpdated).TotalMinutes > 5;
    }
}

/// <summary>
/// Leaderboard types
/// </summary>
public enum LeaderboardType
{
    ByLevel,
    TotalScore,
    PerfectLevels,
    ReplayViews,
    ChallengesWon
}

/// <summary>
/// Global leaderboard collection
/// </summary>
[Serializable]
public class GlobalLeaderboardCollection
{
    [JsonProperty("by_level")]
    public Dictionary<string, LeaderboardData> ByLevel { get; set; } = new();
    
    [JsonProperty("total_score")]
    public LeaderboardData TotalScore { get; set; } = new();
    
    [JsonProperty("perfect_levels")]
    public LeaderboardData PerfectLevels { get; set; } = new();
    
    [JsonProperty("replay_views")]
    public LeaderboardData ReplayViews { get; set; } = new();
    
    [JsonProperty("challenges_won")]
    public LeaderboardData ChallengesWon { get; set; } = new();
    
    [JsonProperty("last_sync")]
    public DateTime LastSync { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Check if any leaderboard needs sync
    /// </summary>
    public bool NeedsSync()
    {
        return (DateTime.UtcNow - LastSync).TotalMinutes > 5;
    }
}
