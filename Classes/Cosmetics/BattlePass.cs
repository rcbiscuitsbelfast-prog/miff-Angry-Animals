using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Newtonsoft.Json;

/// <summary>
/// Manages the battle pass system with seasonal progression and rewards.
/// </summary>
public partial class BattlePass : Node
{
    public static BattlePass Instance { get; private set; } = null!;
    
    [Signal] public delegate void TierCompletedEventHandler(int tier);
    [Signal] public delegate void RewardClaimedEventHandler(int tier, bool isPremium);
    [Signal] public delegate void BattlePassPurchasedEventHandler(float price);
    [Signal] public delegate void SeasonEndedEventHandler();
    [Signal] public delegate void XPUpdatedEventHandler(int currentXp, int totalXp);
    
    [Header("Configuration")]
    [Export] public BattlePassSeason? CurrentSeason;
    [Export] public BattlePassSeason[] AvailableSeasons = Array.Empty<BattlePassSeason>();
    [Export] public bool AllowSeasonPurchase = true;
    [Export] public float DefaultBattlePassPrice = 4.99f;
    
    // Player progress
    private BattlePassProgress _progress = new();
    
    // Economy settings
    [Export] public int BaseXpPerLevel = 100;
    [Export] public int MaxDailyXp = 500;
    [Export] public int XpPerLevelComplete = 100;
    [Export] public int XpPerChallengeComplete = 50;
    
    public override void _Ready()
    {
        Instance = this;
        ProcessMode = ProcessModeEnum.Always;
        
        LoadProgress();
        InitializeCurrentSeason();
        
        GD.Print("BattlePass system initialized");
    }
    
    /// <summary>
    /// Initialize the current season if not set.
    /// </summary>
    private void InitializeCurrentSeason()
    {
        // If no season is set, create or find the active season
        if (CurrentSeason == null)
        {
            CurrentSeason = GetActiveSeason();
        }
        
        if (CurrentSeason == null)
        {
            // Create a default season
            CreateDefaultSeason();
        }
        
        // Check if we need to migrate progress for new season
        if (CurrentSeason != null && _progress.CurrentSeason != CurrentSeason.SeasonNumber)
        {
            StartNewSeason(CurrentSeason.SeasonNumber);
        }
    }
    
    /// <summary>
    /// Get the currently active season.
    /// </summary>
    public BattlePassSeason? GetActiveSeason()
    {
        var now = DateTime.Now;
        
        foreach (var season in AvailableSeasons)
        {
            if (now >= season.StartDate && now <= season.EndDate)
            {
                return season;
            }
        }
        
        return null;
    }
    
    /// <summary>
    /// Create a default season configuration.
    /// </summary>
    private void CreateDefaultSeason()
    {
        var season = new BattlePassSeason();
        season.SeasonNumber = 1;
        season.SeasonName = "Season 1: Ice Theme";
        season.SeasonDescription = "Cool down with icy cosmetics!";
        season.ThemeColor = "#3498db";
        season.StartDate = DateTime.Now;
        season.EndDate = DateTime.Now.AddDays(28);
        season.BattlePassPrice = DefaultBattlePassPrice;
        season.InitializeDefaults();
        
        CurrentSeason = season;
    }
    
    /// <summary>
    /// Start a new season.
    /// </summary>
    public void StartNewSeason(int seasonNumber)
    {
        var season = AvailableSeasons.FirstOrDefault(s => s.SeasonNumber == seasonNumber);
        
        if (season == null)
        {
            // Create a default season for this number
            season = new BattlePassSeason
            {
                SeasonNumber = seasonNumber,
                SeasonName = $"Season {seasonNumber}",
                SeasonDescription = "New season, new rewards!",
                ThemeColor = seasonNumber switch
                {
                    1 => "#3498db",  // Ice - Blue
                    2 => "#e74c3c",  // Fire - Red
                    3 => "#27ae60",  // Nature - Green
                    4 => "#8e44ad",  // Dark - Purple
                    _ => "#f39c12"   // Default - Orange
                },
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddDays(28),
                BattlePassPrice = DefaultBattlePassPrice
            };
            season.InitializeDefaults();
        }
        
        CurrentSeason = season;
        _progress.ResetForNewSeason(seasonNumber, season);
        SaveProgress();
        
        GD.Print($"Started new season: {season.SeasonName}");
    }
    
    // ==================== PROGRESSION ====================
    
    /// <summary>
    /// Add XP to the battle pass.
    /// </summary>
    public int AddXp(int amount)
    {
        if (CurrentSeason == null)
            return 0;
        
        var tiersGained = _progress.AddXp(amount, CurrentSeason);
        _progress.CheckDailyReset();
        
        // Check for tier completions
        for (int i = Math.Max(1, CurrentSeason.TotalTiers - 5); i <= _progress.CurrentTier; i++)
        {
            if (_progress.CanClaimTier(i, _progress.HasBattlePass) && 
                !_progress.ClaimedTiers.Contains(i))
            {
                // Tier reward available
            }
        }
        
        EmitSignal(SignalName.XPUpdated, _progress.CurrentXp, _progress.TotalXpEarned);
        
        // Check if tier was completed
        if (tiersGained > 0)
        {
            EmitSignal(SignalName.TierCompleted, _progress.CurrentTier);
        }
        
        SaveProgress();
        return tiersGained;
    }
    
    /// <summary>
    /// Add XP for completing a level.
    /// </summary>
    public int AddLevelCompleteXp(int levelNumber, bool perfect = false)
    {
        var baseXp = XpPerLevelComplete;
        var bonusXp = perfect ? baseXp : 0;
        var totalXp = baseXp + bonusXp;
        
        return AddXp(totalXp);
    }
    
    /// <summary>
    /// Add XP for completing a challenge.
    /// </summary>
    public int AddChallengeXp(int challengeDifficulty = 1)
    {
        var xp = XpPerChallengeComplete * challengeDifficulty;
        return AddXp(xp);
    }
    
    /// <summary>
    /// Add XP for daily bonus.
    /// </summary>
    public int ClaimDailyBonus()
    {
        var bonus = _progress.ClaimDailyBonus();
        if (bonus > 0)
        {
            return AddXp(bonus);
        }
        return 0;
    }
    
    /// <summary>
    /// Get current progress as a percentage.
    /// </summary>
    public float GetProgressPercentage()
    {
        if (CurrentSeason == null)
            return 0f;
        
        return _progress.GetSeasonCompletion(CurrentSeason);
    }
    
    /// <summary>
    /// Get current tier progress (0-1).
    /// </summary>
    public float GetTierProgress()
    {
        return _progress.GetTierProgress();
    }
    
    /// <summary>
    /// Get current tier number.
    /// </summary>
    public int GetCurrentTier()
    {
        return _progress.CurrentTier;
    }
    
    /// <summary>
    /// Get remaining XP to next tier.
    /// </summary>
    public int GetRemainingXp()
    {
        return _progress.GetRemainingXpToNextTier();
    }
    
    /// <summary>
    /// Get total XP earned this season.
    /// </summary>
    public int GetTotalXp()
    {
        return _progress.TotalXpEarned;
    }
    
    // ==================== REWARDS ====================
    
    /// <summary>
    /// Claim reward for a completed tier.
    /// </summary>
    public bool ClaimReward(int tier, bool isPremium)
    {
        if (CurrentSeason == null)
            return false;
        
        if (!_progress.CanClaimTier(tier, isPremium))
            return false;
        
        var tierData = CurrentSeason.GetTier(tier);
        if (tierData == null)
            return false;
        
        var reward = isPremium ? tierData.PremiumReward : tierData.FreeReward;
        if (reward == null)
            return false;
        
        // Grant the reward
        GrantReward(reward);
        
        // Mark as claimed
        _progress.ClaimTier(tier, CurrentSeason);
        SaveProgress();
        
        EmitSignal(SignalName.RewardClaimed, tier, isPremium);
        
        return true;
    }
    
    /// <summary>
    /// Grant a reward to the player.
    /// </summary>
    private void GrantReward(BattlePassReward reward)
    {
        switch (reward.Type)
        {
            case BattlePassRewardType.Coins:
                PlayerProfile.Instance?.AddCoins(reward.Amount);
                break;
                
            case BattlePassRewardType.PremiumCurrency:
                _progress.AddPremiumCurrency(reward.Amount);
                break;
                
            case BattlePassRewardType.Cosmetic:
                if (!string.IsNullOrEmpty(reward.RewardId))
                {
                    CosmeticsShop.Instance?.UnlockCosmetic(reward.RewardId, "battle_pass");
                }
                break;
        }
    }
    
    /// <summary>
    /// Get the next unclaimed reward.
    /// </summary>
    public (int tier, BattlePassReward? reward, bool isPremium)? GetNextReward()
    {
        if (CurrentSeason == null)
            return null;
        
        for (int tier = 1; tier <= _progress.CurrentTier; tier++)
        {
            if (_progress.CanClaimTier(tier, true) && CurrentSeason.GetTier(tier)?.PremiumReward != null)
            {
                return (tier, CurrentSeason.GetTier(tier)!.PremiumReward, true);
            }
            
            if (_progress.CanClaimTier(tier, false) && CurrentSeason.GetTier(tier)?.FreeReward != null)
            {
                return (tier, CurrentSeason.GetTier(tier)!.FreeReward, false);
            }
        }
        
        return null;
    }
    
    /// <summary>
    /// Get all available rewards for claiming.
    /// </summary>
    public List<(int tier, BattlePassReward reward, bool isPremium)> GetAvailableRewards()
    {
        var rewards = new List<(int tier, BattlePassReward reward, bool isPremium)>();
        
        if (CurrentSeason == null)
            return rewards;
        
        for (int tier = 1; tier <= _progress.CurrentTier; tier++)
        {
            if (_progress.CanClaimTier(tier, true))
            {
                var premiumReward = CurrentSeason.GetTier(tier)?.PremiumReward;
                if (premiumReward != null)
                {
                    rewards.Add((tier, premiumReward, true));
                }
            }
            
            if (_progress.CanClaimTier(tier, false))
            {
                var freeReward = CurrentSeason.GetTier(tier)?.FreeReward;
                if (freeReward != null)
                {
                    rewards.Add((tier, freeReward, false));
                }
            }
        }
        
        return rewards;
    }
    
    /// <summary>
    /// Get the reward at a specific tier.
    /// </summary>
    public (BattlePassReward? free, BattlePassReward? premium) GetTierRewards(int tier)
    {
        if (CurrentSeason == null)
            return (null, null);
        
        var tierData = CurrentSeason.GetTier(tier);
        if (tierData == null)
            return (null, null);
        
        return (tierData.FreeReward, tierData.PremiumReward);
    }
    
    // ==================== PURCHASE ====================
    
    /// <summary>
    /// Purchase the battle pass for the current season.
    /// </summary>
    public async Task<bool> PurchaseBattlePass()
    {
        if (CurrentSeason == null)
            return false;
        
        if (_progress.HasBattlePass)
            return true;
        
        try
        {
            // In a real implementation, this would call MonetizationManager
            // For now, we simulate a successful purchase
            
            // Simulate purchase delay
            await Task.Delay(1000);
            
            _progress.PurchaseBattlePass();
            SaveProgress();
            
            AnalyticsEventTracker.Instance?.TrackBattlePassPurchased(
                CurrentSeason.BattlePassPrice,
                CurrentSeason.CurrencyCode,
                CurrentSeason.SeasonNumber
            );
            
            EmitSignal(SignalName.BattlePassPurchased, CurrentSeason.BattlePassPrice);
            
            return true;
        }
        catch (Exception ex)
        {
            GD.PushError($"BattlePass: Purchase failed: {ex.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// Check if player has purchased the battle pass.
    /// </summary>
    public bool HasBattlePass()
    {
        return _progress.HasBattlePass;
    }
    
    /// <summary>
    /// Get the battle pass price.
    /// </summary>
    public float GetBattlePassPrice()
    {
        return CurrentSeason?.BattlePassPrice ?? DefaultBattlePassPrice;
    }
    
    // ==================== SEASON INFO ====================
    
    /// <summary>
    /// Get the current season name.
    /// </summary>
    public string GetSeasonName()
    {
        return CurrentSeason?.SeasonName ?? "Unknown Season";
    }
    
    /// <summary>
    /// Get the current season description.
    /// </summary>
    public string GetSeasonDescription()
    {
        return CurrentSeason?.SeasonDescription ?? "";
    }
    
    /// <summary>
    /// Get days remaining in the season.
    /// </summary>
    public int GetDaysRemaining()
    {
        return CurrentSeason?.GetDaysRemaining() ?? 0;
    }
    
    /// <summary>
    /// Check if the season is active.
    /// </summary>
    public bool IsSeasonActive()
    {
        return CurrentSeason?.IsSeasonActive() ?? false;
    }
    
    /// <summary>
    /// Get the total number of tiers.
    /// </summary>
    public int GetTotalTiers()
    {
        return CurrentSeason?.TotalTiers ?? 30;
    }
    
    /// <summary>
    /// Get the season theme color.
    /// </summary>
    public Color GetSeasonThemeColor()
    {
        return CurrentSeason?.GetThemeColor() ?? new Color(0.2f, 0.6f, 0.9f);
    }
    
    /// <summary>
    /// Get the season number.
    /// </summary>
    public int GetSeasonNumber()
    {
        return CurrentSeason?.SeasonNumber ?? 0;
    }
    
    // ==================== PREMIUM CURRENCY ====================
    
    /// <summary>
    /// Get available premium currency balance.
    /// </summary>
    public int GetPremiumCurrencyBalance()
    {
        return _progress.GetPremiumCurrencyBalance();
    }
    
    /// <summary>
    /// Spend premium currency.
    /// </summary>
    public bool SpendPremiumCurrency(int amount)
    {
        return _progress.SpendPremiumCurrency(amount);
    }
    
    // ==================== DATA PERSISTENCE ====================
    
    /// <summary>
    /// Load battle pass progress.
    /// </summary>
    private void LoadProgress()
    {
        var savePath = "user://battlepass_progress.json";
        
        try
        {
            if (FileAccess.FileExists(savePath))
            {
                using var file = FileAccess.Open(savePath, FileAccess.ModeFlags.Read);
                var json = file?.GetAsText() ?? string.Empty;
                
                if (!string.IsNullOrEmpty(json))
                {
                    var loaded = JsonConvert.DeserializeObject<BattlePassProgress>(json);
                    if (loaded != null)
                    {
                        _progress = loaded;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            GD.PushWarning($"BattlePass: Failed to load progress: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Save battle pass progress.
    /// </summary>
    public void SaveProgress()
    {
        var savePath = "user://battlepass_progress.json";
        
        try
        {
            var json = JsonConvert.SerializeObject(_progress, Formatting.Indented);
            using var file = FileAccess.Open(savePath, FileAccess.ModeFlags.Write);
            file?.StoreString(json);
        }
        catch (Exception ex)
        {
            GD.PushWarning($"BattlePass: Failed to save progress: {ex.Message}");
        }
    }
    
    // ==================== SEASON MANAGEMENT ====================
    
    /// <summary>
    /// Add a season to available seasons.
    /// </summary>
    public void AddSeason(BattlePassSeason season)
    {
        var seasons = AvailableSeasons.ToList();
        
        // Remove existing season with same number
        seasons.RemoveAll(s => s.SeasonNumber == season.SeasonNumber);
        
        seasons.Add(season);
        AvailableSeasons = seasons.ToArray();
    }
    
    /// <summary>
    /// Get all available seasons.
    /// </summary>
    public BattlePassSeason[] GetAllSeasons()
    {
        return AvailableSeasons;
    }
    
    /// <summary>
    /// Get a specific season by number.
    /// </summary>
    public BattlePassSeason? GetSeason(int seasonNumber)
    {
        return AvailableSeasons.FirstOrDefault(s => s.SeasonNumber == seasonNumber);
    }
    
    /// <summary>
    /// End the current season and check for season end.
    /// </summary>
    public void CheckSeasonEnd()
    {
        if (CurrentSeason == null)
            return;
        
        if (DateTime.Now > CurrentSeason.EndDate)
        {
            EmitSignal(SignalName.SeasonEnded);
            GD.Print($"Season {CurrentSeason.SeasonNumber} has ended");
        }
    }
    
    // ==================== STATISTICS ====================
    
    /// <summary>
    /// Get battle pass statistics.
    /// </summary>
    public string GetStatistics()
    {
        var sb = new System.Text.StringBuilder();
        
        sb.AppendLine("=== Battle Pass Statistics ===");
        sb.AppendLine($"Current Season: {GetSeasonName()}");
        sb.AppendLine($"Current Tier: {_progress.CurrentTier}/{GetTotalTiers()}");
        sb.AppendLine($"Total XP: {_progress.TotalXpEarned}");
        sb.AppendLine($"Season Progress: {GetProgressPercentage():P1}");
        sb.AppendLine($"Battle Pass Owned: {(_progress.HasBattlePass ? "Yes" : "No")}");
        sb.AppendLine($"Days Remaining: {GetDaysRemaining()}");
        sb.AppendLine($"Premium Currency: {GetPremiumCurrencyBalance()}");
        sb.AppendLine($"Tiers Claimed: {_progress.ClaimedTiers.Count}");
        
        return sb.ToString();
    }
}
