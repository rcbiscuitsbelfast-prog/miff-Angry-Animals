using System;
using System.Collections.Generic;
using Godot;
using Newtonsoft.Json;

/// <summary>
/// Represents a reward at a specific battle pass tier.
/// </summary>
[Serializable]
public class BattlePassReward
{
    /// <summary>
    /// Type of reward.
    /// </summary>
    [JsonProperty("type")]
    public BattlePassRewardType Type { get; set; } = BattlePassRewardType.Cosmetic;
    
    /// <summary>
    /// ID of the reward (cosmetic ID, currency amount, etc.).
    /// </summary>
    [JsonProperty("reward_id")]
    public string RewardId { get; set; } = string.Empty;
    
    /// <summary>
    /// Amount of currency (if reward is coins/premium currency).
    /// </summary>
    [JsonProperty("amount")]
    public int Amount { get; set; } = 0;
    
    /// <summary>
    /// Display name for the reward.
    /// </summary>
    [JsonProperty("display_name")]
    public string DisplayName { get; set; } = string.Empty;
    
    /// <summary>
    /// Description of the reward.
    /// </summary>
    [JsonProperty("description")]
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// Path to reward icon/sprite.
    /// </summary>
    [JsonProperty("icon_path")]
    public string IconPath { get; set; } = string.Empty;
}

/// <summary>
/// Types of rewards available in the battle pass.
/// </summary>
public enum BattlePassRewardType
{
    Cosmetic = 0,
    Coins = 1,
    PremiumCurrency = 2,
    XPBonus = 3,
    Title = 4,
    ProfileFrame = 5
}

/// <summary>
/// Represents a single tier in the battle pass.
/// </summary>
[Serializable]
public class BattlePassTier
{
    /// <summary>
    /// Tier number (1-30).
    /// </summary>
    [JsonProperty("tier_number")]
    public int TierNumber { get; set; } = 1;
    
    /// <summary>
    /// XP required to reach this tier.
    /// </summary>
    [JsonProperty("xp_required")]
    public int XpRequired { get; set; } = 100;
    
    /// <summary>
    /// Free tier reward at this level.
    /// </summary>
    [JsonProperty("free_reward")]
    public BattlePassReward? FreeReward { get; set; }
    
    /// <summary>
    /// Premium tier reward at this level (requires battle pass).
    /// </summary>
    [JsonProperty("premium_reward")]
    public BattlePassReward? PremiumReward { get; set; }
    
    /// <summary>
    /// Whether this tier has any reward.
    /// </summary>
    [JsonProperty("has_reward")]
    public bool HasReward => FreeReward != null || PremiumReward != null;
    
    /// <summary>
    /// Get the reward for a specific tier access level.
    /// </summary>
    public BattlePassReward? GetReward(bool isPremium)
    {
        return isPremium ? PremiumReward : FreeReward;
    }
}

/// <summary>
/// Configuration for a single battle pass season.
/// </summary>
[CreateAssetMenu(fileName = "BattlePassSeason", menuName = "Battle Pass/Season")]
public class BattlePassSeason : Resource
{
    [Header("Season Info")]
    [Export] public int SeasonNumber = 1;
    [Export] public string SeasonName = "Season 1";
    [Export] public string SeasonDescription = "Welcome to the battle pass!";
    [Export] public string ThemeColor = "#3498db";
    
    [Header("Season Dates")]
    [Export] public DateTime StartDate;
    [Export] public DateTime EndDate;
    [Export] public int DurationDays = 28;
    
    [Header("Pricing")]
    [Export] public float BattlePassPrice = 4.99f;
    [Export] public string CurrencyCode = "USD";
    
    [Header("Battle Pass Tiers")]
    [Export] public int TotalTiers = 30;
    [Export] public int FreeTierCount = 20;
    [Export] public int PremiumTierCount = 10;
    [Export] public BattlePassTier[] Tiers = Array.Empty<BattlePassTier>();
    
    [Header("Theme Assets")]
    [Export] public string ThemeIconPath = "";
    [Export] public string ThemeBannerPath = "";
    
    [Header("Seasonal Cosmetics")]
    [Export] public string[] SeasonalCosmeticIds = Array.Empty<string>();
    
    /// <summary>
    /// Initialize with default values.
    /// </summary>
    public void InitializeDefaults()
    {
        StartDate = DateTime.Now;
        EndDate = StartDate.AddDays(DurationDays);
        GenerateDefaultTiers();
    }
    
    /// <summary>
    /// Generate default tier structure.
    /// </summary>
    public void GenerateDefaultTiers()
    {
        var tiers = new List<BattlePassTier>();
        int xpPerTier = 100;
        
        for (int i = 1; i <= TotalTiers; i++)
        {
            var tier = new BattlePassTier
            {
                TierNumber = i,
                XpRequired = xpPerTier * i
            };
            
            // Free rewards for first 20 tiers
            if (i <= FreeTierCount)
            {
                tier.FreeReward = GenerateFreeReward(i);
            }
            
            // Premium rewards for all 30 tiers
            tier.PremiumReward = GeneratePremiumReward(i);
            
            tiers.Add(tier);
        }
        
        Tiers = tiers.ToArray();
    }
    
    /// <summary>
    /// Generate a free reward for a tier.
    /// </summary>
    private BattlePassReward? GenerateFreeReward(int tier)
    {
        // Early tiers: coins
        if (tier <= 5)
        {
            return new BattlePassReward
            {
                Type = BattlePassRewardType.Coins,
                RewardId = $"coins_{tier * 100}",
                Amount = tier * 100,
                DisplayName = $"{tier * 100} Coins",
                Description = "Free coins for progressing!"
            };
        }
        
        // Mid tiers: more coins or XP
        if (tier <= 15)
        {
            return new BattlePassReward
            {
                Type = tier % 2 == 0 ? BattlePassRewardType.Coins : BattlePassRewardType.XPBonus,
                RewardId = $"free_{tier}",
                Amount = 200,
                DisplayName = tier % 2 == 0 ? "200 Coins" : "2x XP Boost",
                Description = "Progress faster with rewards!"
            };
        }
        
        // Late free tiers: better rewards
        return new BattlePassReward
        {
            Type = BattlePassRewardType.Coins,
            RewardId = $"free_late_{tier}",
            Amount = 500,
            DisplayName = "500 Coins",
            Description = "Big coin reward for reaching here!"
        };
    }
    
    /// <summary>
    /// Generate a premium reward for a tier.
    /// </summary>
    private BattlePassReward? GeneratePremiumReward(int tier)
    {
        // Every 5 tiers: exclusive cosmetic
        if (tier % 5 == 0)
        {
            return new BattlePassReward
            {
                Type = BattlePassRewardType.Cosmetic,
                RewardId = $"season_{SeasonNumber}_cosmetic_tier_{tier}",
                DisplayName = $"Season {SeasonNumber} Exclusive",
                Description = "Exclusive cosmetic for battle pass holders!"
            };
        }
        
        // Premium tiers: premium currency and coins
        if (tier > FreeTierCount)
        {
            return new BattlePassReward
            {
                Type = BattlePassRewardType.PremiumCurrency,
                RewardId = $"premium_currency_{tier}",
                Amount = 50,
                DisplayName = "50 Premium Coins",
                Description = "Premium currency for exclusive items!"
            };
        }
        
        // Early premium: bonus coins
        return new BattlePassReward
        {
            Type = BattlePassRewardType.Coins,
            RewardId = $"premium_coins_{tier}",
            Amount = tier * 200,
            DisplayName = $"{tier * 200} Coins",
            Description = "Premium bonus coins!"
        };
    }
    
    /// <summary>
    /// Check if the season is currently active.
    /// </summary>
    public bool IsSeasonActive()
    {
        var now = DateTime.Now;
        return now >= StartDate && now <= EndDate;
    }
    
    /// <summary>
    /// Get days remaining in the season.
    /// </summary>
    public int GetDaysRemaining()
    {
        var now = DateTime.Now;
        var remaining = (EndDate - now).TotalDays;
        return Math.Max(0, (int)Math.Ceiling(remaining));
    }
    
    /// <summary>
    /// Get the season progress percentage.
    /// </summary>
    public float GetSeasonProgress()
    {
        var now = DateTime.Now;
        var totalDuration = (EndDate - StartDate).TotalDays;
        var elapsed = (now - StartDate).TotalDays;
        return Math.Min(1f, Math.Max(0f, (float)(elapsed / totalDuration)));
    }
    
    /// <summary>
    /// Get a specific tier by number.
    /// </summary>
    public BattlePassTier? GetTier(int tierNumber)
    {
        if (tierNumber < 1 || tierNumber > TotalTiers)
            return null;
            
        return Tiers[tierNumber - 1];
    }
    
    /// <summary>
    /// Get all premium rewards.
    /// </summary>
    public BattlePassReward[] GetAllPremiumRewards()
    {
        var rewards = new List<BattlePassReward>();
        foreach (var tier in Tiers)
        {
            if (tier.PremiumReward != null)
                rewards.Add(tier.PremiumReward);
        }
        return rewards.ToArray();
    }
    
    /// <summary>
    /// Get all free rewards.
    /// </summary>
    public BattlePassReward[] GetAllFreeRewards()
    {
        var rewards = new List<BattlePassReward>();
        foreach (var tier in Tiers)
        {
            if (tier.FreeReward != null)
                rewards.Add(tier.FreeReward);
        }
        return rewards.ToArray();
    }
    
    /// <summary>
    /// Get the total premium value (sum of all rewards).
    /// </summary>
    public float GetTotalPremiumValue()
    {
        float total = BattlePassPrice;
        foreach (var tier in Tiers)
        {
            if (tier.PremiumReward != null)
            {
                total += tier.PremiumReward.Amount * 0.01f; // Rough estimate
            }
        }
        return total;
    }
    
    /// <summary>
    /// Export season configuration to JSON.
    /// </summary>
    public string ExportToJson()
    {
        return JsonConvert.SerializeObject(new
        {
            season_number = SeasonNumber,
            season_name = SeasonName,
            season_description = SeasonDescription,
            theme_color = ThemeColor,
            start_date = StartDate.ToString("yyyy-MM-dd"),
            end_date = EndDate.ToString("yyyy-MM-dd"),
            duration_days = DurationDays,
            battle_pass_price = BattlePassPrice,
            currency_code = CurrencyCode,
            total_tiers = TotalTiers,
            free_tier_count = FreeTierCount,
            premium_tier_count = PremiumTierCount,
            tiers = Tiers,
            seasonal_cosmetics = SeasonalCosmeticIds
        }, Formatting.Indented);
    }
    
    /// <summary>
    /// Get the season theme color as Color.
    /// </summary>
    public Color GetThemeColor()
    {
        if (Color.TryParse(ThemeColor, out var color))
            return color;
        return new Color(0.2f, 0.6f, 0.9f);
    }
}
