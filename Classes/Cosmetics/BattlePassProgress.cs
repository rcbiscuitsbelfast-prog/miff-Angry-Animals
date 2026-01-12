using System;
using System.Collections.Generic;
using Godot;
using Newtonsoft.Json;

/// <summary>
/// Player's battle pass progress for the current season.
/// Persisted in player profile.
/// </summary>
[Serializable]
public class BattlePassProgress
{
    /// <summary>
    /// Current season number.
    /// </summary>
    [JsonProperty("current_season")]
    public int CurrentSeason { get; set; } = 1;
    
    /// <summary>
    /// Whether the player owns the battle pass for this season.
    /// </summary>
    [JsonProperty("has_battle_pass")]
    public bool HasBattlePass { get; set; } = false;
    
    /// <summary>
    /// Current tier (1-30).
    /// </summary>
    [JsonProperty("current_tier")]
    public int CurrentTier { get; set; } = 1;
    
    /// <summary>
    /// Current XP in the current tier.
    /// </summary>
    [JsonProperty("current_xp")]
    public int CurrentXp { get; set; } = 0;
    
    /// <summary>
    /// Total XP earned this season.
    /// </summary>
    [JsonProperty("total_xp_earned")]
    public int TotalXpEarned { get; set; } = 0;
    
    /// <summary>
    /// Total XP needed for current tier.
    /// </summary>
    [JsonProperty("xp_for_current_tier")]
    public int XpForCurrentTier { get; set; } = 100;
    
    /// <summary>
    /// Tiers that have been claimed.
    /// </summary>
    [JsonProperty("claimed_tiers")]
    public HashSet<int> ClaimedTiers { get; set; } = new();
    
    /// <summary>
    /// When the player last gained XP.
    /// </summary>
    [JsonProperty("last_xp_gain")]
    public DateTime LastXpGain { get; set; } = DateTime.Now;
    
    /// <summary>
    /// Daily XP bonus available.
    /// </summary>
    [JsonProperty("daily_xp_bonus")]
    public int DailyXpBonus { get; set; } = 0;
    
    /// <summary>
    /// Whether daily bonus was claimed today.
    /// </summary>
    [JsonProperty("daily_bonus_claimed_today")]
    public bool DailyBonusClaimedToday { get; set; } = false;
    
    /// <summary>
    /// Premium currency earned from battle pass.
    /// </summary>
    [JsonProperty("earned_premium_currency")]
    public int EarnedPremiumCurrency { get; set; } = 0;
    
    /// <summary>
    /// Total premium currency spent from battle pass.
    /// </summary>
    [JsonProperty("spent_premium_currency")]
    public int SpentPremiumCurrency { get; set; } = 0;
    
    /// <summary>
    /// XP multiplier active (from boosts).
    /// </summary>
    [JsonProperty("xp_multiplier")]
    public float XpMultiplier { get; set; } = 1.0f;
    
    /// <summary>
    /// Get total XP including current tier progress.
    /// </summary>
    public int GetTotalProgressXp()
    {
        return (CurrentTier - 1) * XpForCurrentTier + CurrentXp;
    }
    
    /// <summary>
    /// Get progress to next tier as 0-1 value.
    /// </summary>
    public float GetTierProgress()
    {
        if (XpForCurrentTier <= 0) return 0f;
        return (float)CurrentXp / XpForCurrentTier;
    }
    
    /// <summary>
    /// Add XP and handle tier progression.
    /// </summary>
    public int AddXp(int amount, BattlePassSeason season)
    {
        var actualAmount = (int)(amount * XpMultiplier);
        CurrentXp += actualAmount;
        TotalXpEarned += actualAmount;
        LastXpGain = DateTime.Now;
        
        var tiersGained = 0;
        
        // Check for tier up
        while (CurrentXp >= XpForCurrentTier && CurrentTier < season.TotalTiers)
        {
            CurrentXp -= XpForCurrentTier;
            CurrentTier++;
            tiersGained++;
            
            // Increase XP requirement for next tier (gradual scaling)
            XpForCurrentTier = GetScaledXpRequirement(CurrentTier);
        }
        
        // Cap at max tier
        if (CurrentTier >= season.TotalTiers)
        {
            CurrentTier = season.TotalTiers;
            CurrentXp = XpForCurrentTier;
        }
        
        return tiersGained;
    }
    
    /// <summary>
    /// Get scaled XP requirement for a tier (gradual increase).
    /// </summary>
    private int GetScaledXpRequirement(int tier)
    {
        // Base 100, increases by 10 every 5 tiers
        var baseXp = 100;
        var increase = (tier / 5) * 10;
        return baseXp + increase;
    }
    
    /// <summary>
    /// Claim rewards for a tier.
    /// </summary>
    public bool ClaimTier(int tier, BattlePassSeason season)
    {
        if (tier > CurrentTier)
            return false;
            
        if (ClaimedTiers.Contains(tier))
            return false;
            
        ClaimedTiers.Add(tier);
        return true;
    }
    
    /// <summary>
    /// Check if a tier reward is available to claim.
    /// </summary>
    public bool CanClaimTier(int tier, bool hasBattlePass)
    {
        if (tier > CurrentTier)
            return false;
            
        if (ClaimedTiers.Contains(tier))
            return false;
            
        return true;
    }
    
    /// <summary>
    /// Purchase the battle pass.
    /// </summary>
    public void PurchaseBattlePass()
    {
        HasBattlePass = true;
    }
    
    /// <summary>
    /// Reset progress for a new season.
    /// </summary>
    public void ResetForNewSeason(int newSeasonNumber, BattlePassSeason season)
    {
        CurrentSeason = newSeasonNumber;
        CurrentTier = 1;
        CurrentXp = 0;
        TotalXpEarned = 0;
        XpForCurrentTier = 100;
        ClaimedTiers.Clear();
        EarnedPremiumCurrency = 0;
        SpentPremiumCurrency = 0;
        XpMultiplier = 1.0f;
    }
    
    /// <summary>
    /// Get remaining XP to next tier.
    /// </summary>
    public int GetRemainingXpToNextTier()
    {
        return Math.Max(0, XpForCurrentTier - CurrentXp);
    }
    
    /// <summary>
    /// Get completion percentage for the season.
    /// </summary>
    public float GetSeasonCompletion(BattlePassSeason season)
    {
        if (CurrentTier >= season.TotalTiers)
            return 1f;
            
        var totalRequiredXp = 0;
        for (int i = 1; i <= season.TotalTiers; i++)
        {
            totalRequiredXp += GetScaledXpRequirement(i);
        }
        
        if (totalRequiredXp <= 0)
            return 0f;
            
        return (float)TotalXpEarned / totalRequiredXp;
    }
    
    /// <summary>
    /// Get available premium currency balance.
    /// </summary>
    public int GetPremiumCurrencyBalance()
    {
        return EarnedPremiumCurrency - SpentPremiumCurrency;
    }
    
    /// <summary>
    /// Spend premium currency.
    /// </summary>
    public bool SpendPremiumCurrency(int amount)
    {
        if (GetPremiumCurrencyBalance() < amount)
            return false;
            
        SpentPremiumCurrency += amount;
        return true;
    }
    
    /// <summary>
    /// Add earned premium currency.
    /// </summary>
    public void AddPremiumCurrency(int amount)
    {
        EarnedPremiumCurrency += amount;
    }
    
    /// <summary>
    /// Set XP multiplier.
    /// </summary>
    public void SetXpMultiplier(float multiplier)
    {
        XpMultiplier = Math.Clamp(multiplier, 1f, 5f);
    }
    
    /// <summary>
    /// Check if daily bonus is available.
    /// </summary>
    public bool CanClaimDailyBonus()
    {
        if (DailyBonusClaimedToday)
            return false;
            
        return DailyXpBonus > 0;
    }
    
    /// <summary>
    /// Claim daily XP bonus.
    /// </summary>
    public int ClaimDailyBonus()
    {
        if (!CanClaimDailyBonus())
            return 0;
            
        DailyBonusClaimedToday = true;
        var bonus = DailyXpBonus;
        DailyXpBonus = 0;
        return bonus;
    }
    
    /// <summary>
    /// Check if new day has started (for daily bonus reset).
    /// </summary>
    public void CheckDailyReset()
    {
        var today = DateTime.Now.Date;
        var lastGainDate = LastXpGain.Date;
        
        if (today > lastGainDate)
        {
            DailyBonusClaimedToday = false;
            DailyXpBonus = 50; // Reset daily bonus
        }
    }
}
