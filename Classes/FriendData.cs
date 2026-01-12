using System;
using Newtonsoft.Json;

/// <summary>
/// Represents a friend relationship with metadata
/// </summary>
[Serializable]
public class FriendData
{
    [JsonProperty("friend_id")]
    public string FriendId { get; set; } = "";
    
    [JsonProperty("friend_name")]
    public string FriendName { get; set; } = "Unknown";
    
    [JsonProperty("friendship_date")]
    public DateTime FriendshipDate { get; set; } = DateTime.UtcNow;
    
    [JsonProperty("total_challenges_sent")]
    public int TotalChallengesSent { get; set; }
    
    [JsonProperty("total_challenges_received")]
    public int TotalChallengesReceived { get; set; }
    
    [JsonProperty("challenges_won")]
    public int ChallengesWon { get; set; }
    
    [JsonProperty("challenges_lost")]
    public int ChallengesLost { get; set; }
    
    [JsonProperty("last_interaction_date")]
    public DateTime LastInteractionDate { get; set; } = DateTime.UtcNow;
    
    [JsonProperty("current_streak")]
    public int CurrentStreak { get; set; }
    
    [JsonProperty("highest_streak")]
    public int HighestStreak { get; set; }
    
    [JsonProperty("favorite_cosmetics")]
    public FriendCosmetics FavoriteCosmetics { get; set; } = new();
    
    [JsonProperty("achievements_count")]
    public int AchievementsCount { get; set; }
    
    [JsonProperty("global_rank")]
    public int GlobalRank { get; set; }
    
    [JsonProperty("total_score")]
    public int TotalScore { get; set; }
    
    [JsonProperty("perfect_levels_count")]
    public int PerfectLevelsCount { get; set; }
    
    [JsonProperty("is_online")]
    public bool IsOnline { get; set; }
    
    [JsonProperty("last_seen")]
    public DateTime LastSeen { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Friend's favorite cosmetics for profile display
/// </summary>
[Serializable]
public class FriendCosmetics
{
    [JsonProperty("hat_index")]
    public int HatIndex { get; set; }
    
    [JsonProperty("glasses_index")]
    public int GlassesIndex { get; set; }
    
    [JsonProperty("moustache_index")]
    public int MoustacheIndex { get; set; }
    
    [JsonProperty("wig_index")]
    public int WigIndex { get; set; }
    
    [JsonProperty("slingshot_skin_index")]
    public int SlingshotSkinIndex { get; set; }
    
    [JsonProperty("projectile_skin_index")]
    public int ProjectileSkinIndex { get; set; }
}
