using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Manages social cosmetics unlocked through social actions
/// </summary>
public partial class SocialCosmetics : Node
{
    public static SocialCosmetics Instance { get; private set; }
    
    // Social cosmetic definitions
    private Dictionary<string, SocialCosmeticDefinition> _cosmetics = new();
    
    // Signals
    [Signal]
    public delegate void SocialCosmeticUnlockedEventHandler(string cosmeticId);
    
    public override void _Ready()
    {
        if (Instance != null)
        {
            QueueFree();
            return;
        }
        
        Instance = this;
        ProcessMode = ProcessModeEnum.Always;
        
        InitializeCosmetics();
        CheckAllUnlocks();
        
        GD.Print("Social Cosmetics initialized");
    }
    
    /// <summary>
    /// Initialize social cosmetic definitions
    /// </summary>
    private void InitializeCosmetics()
    {
        _cosmetics = new Dictionary<string, SocialCosmeticDefinition>
        {
            ["friendship_hat"] = new SocialCosmeticDefinition
            {
                Id = "friendship_hat",
                Name = "Friendship Hat",
                Description = "Unlocked by adding 5 friends",
                UnlockCondition = SocialUnlockCondition.AddFriends,
                RequiredCount = 5,
                CosmeticType = "hat",
                Rarity = "rare"
            },
            ["challenge_champion_crown"] = new SocialCosmeticDefinition
            {
                Id = "challenge_champion_crown",
                Name = "Challenge Champion Crown",
                Description = "Unlocked by winning 10 friend challenges",
                UnlockCondition = SocialUnlockCondition.WinChallenges,
                RequiredCount = 10,
                CosmeticType = "hat",
                Rarity = "epic"
            },
            ["viral_legend_glasses"] = new SocialCosmeticDefinition
            {
                Id = "viral_legend_glasses",
                Name = "Viral Legend Glasses",
                Description = "Unlocked by getting 100 replay views",
                UnlockCondition = SocialUnlockCondition.ReplayViews,
                RequiredCount = 100,
                CosmeticType = "glasses",
                Rarity = "legendary"
            },
            ["team_player_wig"] = new SocialCosmeticDefinition
            {
                Id = "team_player_wig",
                Name = "Team Player Wig",
                Description = "Unlocked by participating in 50 challenges",
                UnlockCondition = SocialUnlockCondition.ParticipateChallenges,
                RequiredCount = 50,
                CosmeticType = "wig",
                Rarity = "rare"
            },
            ["leaderboard_elite_moustache"] = new SocialCosmeticDefinition
            {
                Id = "leaderboard_elite_moustache",
                Name = "Leaderboard Elite Moustache",
                Description = "Unlocked by ranking in top 100 on any level",
                UnlockCondition = SocialUnlockCondition.LeaderboardTop100,
                RequiredCount = 1,
                CosmeticType = "moustache",
                Rarity = "epic"
            }
        };
    }
    
    /// <summary>
    /// Check all unlock conditions
    /// </summary>
    public void CheckAllUnlocks()
    {
        foreach (var cosmetic in _cosmetics.Values)
        {
            CheckUnlock(cosmetic);
        }
    }
    
    /// <summary>
    /// Check specific unlock condition
    /// </summary>
    private void CheckUnlock(SocialCosmeticDefinition cosmetic)
    {
        if (IsUnlocked(cosmetic.Id))
            return;
        
        var currentProgress = GetProgress(cosmetic);
        
        if (currentProgress >= cosmetic.RequiredCount)
        {
            UnlockCosmetic(cosmetic.Id);
        }
    }
    
    /// <summary>
    /// Get progress toward unlocking cosmetic
    /// </summary>
    public int GetProgress(SocialCosmeticDefinition cosmetic)
    {
        return cosmetic.UnlockCondition switch
        {
            SocialUnlockCondition.AddFriends => FriendLeaderboard.Instance?.GetFriendCount() ?? 0,
            SocialUnlockCondition.WinChallenges => GetTotalChallengesWon(),
            SocialUnlockCondition.ReplayViews => GetTotalReplayViews(),
            SocialUnlockCondition.ParticipateChallenges => GetTotalChallengesParticipated(),
            SocialUnlockCondition.LeaderboardTop100 => IsInTop100() ? 1 : 0,
            _ => 0
        };
    }
    
    /// <summary>
    /// Unlock social cosmetic
    /// </summary>
    private void UnlockCosmetic(string cosmeticId)
    {
        if (PlayerProfile.Instance != null)
        {
            PlayerProfile.Instance.UnlockCosmetic(cosmeticId);
        }
        
        EmitSignal(SignalName.SocialCosmeticUnlocked, cosmeticId);
        
        // Track analytics
        TrackSocialCosmeticUnlocked(cosmeticId);
        
        GD.Print($"Unlocked social cosmetic: {cosmeticId}");
    }
    
    /// <summary>
    /// Check if cosmetic is unlocked
    /// </summary>
    public bool IsUnlocked(string cosmeticId)
    {
        return PlayerProfile.Instance?.UnlockedCosmetics.Contains(cosmeticId) ?? false;
    }
    
    /// <summary>
    /// Get all social cosmetics
    /// </summary>
    public List<SocialCosmeticDefinition> GetAllCosmetics()
    {
        return _cosmetics.Values.ToList();
    }
    
    /// <summary>
    /// Get cosmetic by ID
    /// </summary>
    public SocialCosmeticDefinition? GetCosmetic(string cosmeticId)
    {
        return _cosmetics.GetValueOrDefault(cosmeticId);
    }
    
    /// <summary>
    /// Get total challenges won
    /// </summary>
    private int GetTotalChallengesWon()
    {
        if (FriendLeaderboard.Instance == null)
            return 0;
        
        var currentPlayerId = PlayerProfile.Instance?.PlayerName ?? "";
        var friends = FriendLeaderboard.Instance.GetAllFriends();
        
        return friends.Where(f => f.FriendId == currentPlayerId)
            .Sum(f => f.ChallengesWon);
    }
    
    /// <summary>
    /// Get total replay views
    /// </summary>
    private int GetTotalReplayViews()
    {
        if (ReplayManager.Instance == null)
            return 0;
        
        return ReplayManager.Instance.GetAllReplays()
            .Sum(r => r.ViewCount);
    }
    
    /// <summary>
    /// Get total challenges participated
    /// </summary>
    private int GetTotalChallengesParticipated()
    {
        if (FriendChallengeManager.Instance == null)
            return 0;
        
        return FriendChallengeManager.Instance.GetAllChallenges()
            .Count(c => c.Status == ChallengeStatus.Completed || c.Status == ChallengeStatus.Accepted);
    }
    
    /// <summary>
    /// Check if player is in top 100
    /// </summary>
    private bool IsInTop100()
    {
        if (GlobalLeaderboard.Instance == null)
            return false;
        
        var rank = GlobalLeaderboard.Instance.GetPlayerRank(LeaderboardType.TotalScore);
        return rank > 0 && rank <= 100;
    }
    
    /// <summary>
    /// Track social cosmetic unlocked analytics
    /// </summary>
    private void TrackSocialCosmeticUnlocked(string cosmeticId)
    {
        try
        {
            if (AnalyticsEventTracker.Instance != null)
            {
                var parameters = new Dictionary<string, object>
                {
                    ["cosmetic_id"] = cosmeticId
                };
                AnalyticsEventTracker.Instance.LogEvent("social_cosmetic_unlocked", parameters);
            }
        }
        catch (Exception ex)
        {
            GD.PrintErr($"Failed to track social_cosmetic_unlocked: {ex.Message}");
        }
    }
}

/// <summary>
/// Social cosmetic definition
/// </summary>
public class SocialCosmeticDefinition
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public SocialUnlockCondition UnlockCondition { get; set; }
    public int RequiredCount { get; set; }
    public string CosmeticType { get; set; } = "";
    public string Rarity { get; set; } = "common";
}

/// <summary>
/// Social unlock conditions
/// </summary>
public enum SocialUnlockCondition
{
    AddFriends,
    WinChallenges,
    ReplayViews,
    ParticipateChallenges,
    LeaderboardTop100
}
