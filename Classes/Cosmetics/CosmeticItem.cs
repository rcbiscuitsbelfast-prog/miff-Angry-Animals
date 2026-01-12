using System;
using System.Collections.Generic;
using Godot;
using Newtonsoft.Json;

/// <summary>
/// Rarity tiers for cosmetics with visual and pricing characteristics.
/// </summary>
public enum CosmeticRarity
{
    Common = 0,
    Uncommon = 1,
    Rare = 2,
    Epic = 3,
    Legendary = 4
}

/// <summary>
/// Category classifications for cosmetics.
/// </summary>
public enum CosmeticCategory
{
    Hat = 0,
    Glasses = 1,
    Mustache = 2,
    Wig = 3,
    Emotion = 4,
    SlingshotSkin = 5,
    ProjectileSkin = 6,
    TrailEffect = 7,
    HitEffect = 8,
    VictoryEffect = 9
}

/// <summary>
/// Unlock conditions for cosmetics.
/// </summary>
public enum UnlockCondition
{
    Always = 0,
    PerfectScore = 1,
    BattlePassTier = 2,
    Seasonal = 3,
    IAP = 4,
    LevelUnlock = 5,
    Achievement = 6
}

/// <summary>
/// Represents a single cosmetic item in the game.
/// </summary>
[Serializable]
public class CosmeticItem
{
    /// <summary>
    /// Unique identifier for this cosmetic.
    /// </summary>
    [JsonProperty("id")]
    public string Id { get; set; } = string.Empty;
    
    /// <summary>
    /// Display name shown to players.
    /// </summary>
    [JsonProperty("display_name")]
    public string DisplayName { get; set; } = string.Empty;
    
    /// <summary>
    /// Flavor text description.
    /// </summary>
    [JsonProperty("description")]
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// Rarity tier of this cosmetic.
    /// </summary>
    [JsonProperty("rarity")]
    public CosmeticRarity Rarity { get; set; } = CosmeticRarity.Common;
    
    /// <summary>
    /// Category this cosmetic belongs to.
    /// </summary>
    [JsonProperty("category")]
    public CosmeticCategory Category { get; set; } = CosmeticCategory.Hat;
    
    /// <summary>
    /// Price in coins (0 for free items).
    /// </summary>
    [JsonProperty("price_coins")]
    public int PriceCoins { get; set; } = 0;
    
    /// <summary>
    /// Price in USD (0 for coin-only items).
    /// </summary>
    [JsonProperty("price_usd")]
    public float PriceUsd { get; set; } = 0f;
    
    /// <summary>
    /// Path to the visual asset/sprite.
    /// </summary>
    [JsonProperty("asset_path")]
    public string AssetPath { get; set; } = string.Empty;
    
    /// <summary>
    /// How this cosmetic can be unlocked.
    /// </summary>
    [JsonProperty("unlock_condition")]
    public UnlockCondition UnlockCondition { get; set; } = UnlockCondition.Always;
    
    /// <summary>
    /// Required value for unlock condition (tier number, level, etc.).
    /// </summary>
    [JsonProperty("unlock_requirement")]
    public int UnlockRequirement { get; set; } = 0;
    
    /// <summary>
    /// Season number if this is a seasonal cosmetic (1-4).
    /// </summary>
    [JsonProperty("season_number")]
    public int SeasonNumber { get; set; } = 0;
    
    /// <summary>
    /// End date for seasonal cosmetics (ISO 8601 format).
    /// </summary>
    [JsonProperty("seasonal_end_date")]
    public string SeasonalEndDate { get; set; } = string.Empty;
    
    /// <summary>
    /// Whether this cosmetic is limited time.
    /// </summary>
    [JsonProperty("is_limited_time")]
    public bool IsLimitedTime { get; set; } = false;
    
    /// <summary>
    /// Sort order for display in shop.
    /// </summary>
    [JsonProperty("sort_order")]
    public int SortOrder { get; set; } = 0;
    
    /// <summary>
    /// Whether this cosmetic is currently available.
    /// </summary>
    [JsonProperty("is_active")]
    public bool IsActive { get; set; } = true;
    
    /// <summary>
    /// Get the display price string.
    /// </summary>
    public string GetPriceString()
    {
        if (PriceUsd > 0)
        {
            return $"${PriceUsd:F2}";
        }
        if (PriceCoins > 0)
        {
            return $"{PriceCoins} coins";
        }
        return "FREE";
    }
    
    /// <summary>
    /// Get rarity color for UI display.
    /// </summary>
    public Color GetRarityColor()
    {
        return Rarity switch
        {
            CosmeticRarity.Common => new Color(0.7f, 0.7f, 0.7f),       // Gray
            CosmeticRarity.Uncommon => new Color(0.3f, 0.8f, 0.3f),     // Green
            CosmeticRarity.Rare => new Color(0.3f, 0.5f, 0.9f),         // Blue
            CosmeticRarity.Epic => new Color(0.6f, 0.3f, 0.8f),         // Purple
            CosmeticRarity.Legendary => new Color(1f, 0.6f, 0f),        // Gold
            _ => Colors.White
        };
    }
    
    /// <summary>
    /// Get rarity name for display.
    /// </summary>
    public string GetRarityName()
    {
        return Rarity switch
        {
            CosmeticRarity.Common => "Common",
            CosmeticRarity.Uncommon => "Uncommon",
            CosmeticRarity.Rare => "Rare",
            CosmeticRarity.Epic => "Epic",
            CosmeticRarity.Legendary => "Legendary",
            _ => "Unknown"
        };
    }
    
    /// <summary>
    /// Check if this cosmetic can be purchased with real money.
    /// </summary>
    public bool CanPurchaseWithMoney()
    {
        return PriceUsd > 0;
    }
    
    /// <summary>
    /// Check if this cosmetic can be purchased with coins.
    /// </summary>
    public bool CanPurchaseWithCoins()
    {
        return PriceCoins > 0;
    }
    
    /// <summary>
    /// Check if this cosmetic is free.
    /// </summary>
    public bool IsFree()
    {
        return PriceCoins == 0 && PriceUsd == 0;
    }
    
    /// <summary>
    /// Check if this cosmetic is exclusive to premium/battle pass owners.
    /// </summary>
    public bool IsPremiumExclusive()
    {
        return UnlockCondition == UnlockCondition.IAP || 
               UnlockCondition == UnlockCondition.BattlePassTier ||
               UnlockCondition == UnlockCondition.Seasonal;
    }
    
    /// <summary>
    /// Create a copy of this cosmetic.
    /// </summary>
    public CosmeticItem Clone()
    {
        return new CosmeticItem
        {
            Id = Id,
            DisplayName = DisplayName,
            Description = Description,
            Rarity = Rarity,
            Category = Category,
            PriceCoins = PriceCoins,
            PriceUsd = PriceUsd,
            AssetPath = AssetPath,
            UnlockCondition = UnlockCondition,
            UnlockRequirement = UnlockRequirement,
            SeasonNumber = SeasonNumber,
            SeasonalEndDate = SeasonalEndDate,
            IsLimitedTime = IsLimitedTime,
            SortOrder = SortOrder,
            IsActive = IsActive
        };
    }
}
