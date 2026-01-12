using System;
using Newtonsoft.Json;

/// <summary>
/// Represents a friend challenge with all associated data
/// </summary>
[Serializable]
public class FriendChallenge
{
    [JsonProperty("challenge_id")]
    public string ChallengeId { get; set; } = Guid.NewGuid().ToString();
    
    [JsonProperty("challenger_id")]
    public string ChallengerId { get; set; } = "";
    
    [JsonProperty("challenger_name")]
    public string ChallengerName { get; set; } = "";
    
    [JsonProperty("challengee_id")]
    public string ChallengeeId { get; set; } = "";
    
    [JsonProperty("challengee_name")]
    public string ChallengeeName { get; set; } = "";
    
    [JsonProperty("level_id")]
    public string LevelId { get; set; } = "";
    
    [JsonProperty("level_name")]
    public string LevelName { get; set; } = "";
    
    [JsonProperty("target_score")]
    public int TargetScore { get; set; }
    
    [JsonProperty("challenger_score")]
    public int ChallengerScore { get; set; }
    
    [JsonProperty("challengee_score")]
    public int ChallengeeScore { get; set; }
    
    [JsonProperty("challenger_stars")]
    public int ChallengerStars { get; set; }
    
    [JsonProperty("challengee_stars")]
    public int ChallengeeStars { get; set; }
    
    [JsonProperty("status")]
    public ChallengeStatus Status { get; set; } = ChallengeStatus.Pending;
    
    [JsonProperty("created_date")]
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    
    [JsonProperty("accepted_date")]
    public DateTime? AcceptedDate { get; set; }
    
    [JsonProperty("completed_date")]
    public DateTime? CompletedDate { get; set; }
    
    [JsonProperty("expiration_date")]
    public DateTime ExpirationDate { get; set; } = DateTime.UtcNow.AddDays(7);
    
    [JsonProperty("message")]
    public string Message { get; set; } = "";
    
    [JsonProperty("rewards_claimed")]
    public bool RewardsClaimed { get; set; }
    
    [JsonProperty("winner_id")]
    public string WinnerId { get; set; } = "";
    
    [JsonProperty("replay_id")]
    public string ReplayId { get; set; } = "";
    
    [JsonProperty("challenger_cosmetics")]
    public FriendCosmetics ChallengerCosmetics { get; set; } = new();
    
    [JsonProperty("challengee_cosmetics")]
    public FriendCosmetics ChallengeeCosmetics { get; set; } = new();
    
    /// <summary>
    /// Check if challenge has expired
    /// </summary>
    public bool IsExpired() => DateTime.UtcNow > ExpirationDate;
    
    /// <summary>
    /// Check if challenge can be accepted
    /// </summary>
    public bool CanBeAccepted() => Status == ChallengeStatus.Pending && !IsExpired();
    
    /// <summary>
    /// Check if challenge is completed
    /// </summary>
    public bool IsCompleted() => Status == ChallengeStatus.Completed;
    
    /// <summary>
    /// Determine winner based on scores
    /// </summary>
    public string DetermineWinner()
    {
        if (Status != ChallengeStatus.Completed)
            return "";
            
        if (ChallengeeScore > ChallengerScore)
            return ChallengeeId;
        else if (ChallengerScore > ChallengeeScore)
            return ChallengerId;
        else
            return ""; // Tie
    }
    
    /// <summary>
    /// Get challenge description for display
    /// </summary>
    public string GetChallengeDescription()
    {
        return $"{ChallengerName} challenges you to beat {TargetScore} on {LevelName}!";
    }
}

/// <summary>
/// Challenge status enumeration
/// </summary>
public enum ChallengeStatus
{
    Pending,
    Accepted,
    Completed,
    Failed,
    Expired,
    Declined
}
