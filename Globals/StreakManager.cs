using System;
using System.Collections.Generic;
using Godot;
using Godot.Collections;

/// <summary>
/// StreakManager handles daily login tracking and streak maintenance
/// Manages the 30-day progressive streak system with escalating rewards
/// </summary>
public partial class StreakManager : Node
{
    public static StreakManager Instance { get; private set; }

    [Signal] public delegate void StreakIncrementedEventHandler(int newStreak);
    [Signal] public delegate void StreakBrokenEventHandler();
    [Signal] public delegate void DailyRewardClaimedEventHandler(StreakReward reward);
    [Signal] public delegate void MilestoneCelebrationEventHandler(int milestoneDay);

    [Export] private bool _enableStreakSystem = true;
    [Export] private bool _autoClaimRewards = true;
    [Export] private bool _showStreakNotifications = true;

    private StreakData _streakData;
    private Timer _dailyResetTimer;
    private Dictionary<int, StreakReward> _streakRewards;
    
    // Reward configuration
    [ExportGroup("Streak Rewards")]
    [Export] private PackedScene _rewardChestScene;
    [Export] private AudioStream _milestoneSound;
    [Export] private AudioStream _rewardClaimSound;

    public override void _Ready()
    {
        if (Instance != null)
        {
            QueueFree();
            return;
        }
        
        Instance = this;
        ProcessMode = ProcessModeEnum.Always;
        
        InitializeStreakSystem();
        SetupDailyResetTimer();
        
        GD.Print("StreakManager initialized");
    }

    public override void _ExitTree()
    {
        SaveStreakData();
    }

    /// <summary>
    /// Initialize the streak system
    /// </summary>
    private void InitializeStreakSystem()
    {
        if (!_enableStreakSystem) return;

        _streakData = new StreakData();
        LoadStreakData();
        InitializeRewardSystem();
        SetupDailyResetTimer();
        
        // Check if player needs to claim today's reward
        CheckDailyLogin();
    }

    /// <summary>
    /// Initialize the reward system with 30-day progression
    /// </summary>
    private void InitializeRewardSystem()
    {
        _streakRewards = new Dictionary<int, StreakReward>();
        
        // Days 1-7: Common rewards (basic cosmetics)
        AddReward(1, new StreakReward
        {
            Day = 1,
            RewardType = MilestoneType.Common,
            CosmeticId = "basic_hat_01",
            Coins = 50,
            PremiumCurrency = 0,
            Title = "Welcome Bonus!",
            Description = "50 coins to get you started"
        });

        AddReward(2, new StreakReward
        {
            Day = 2,
            RewardType = MilestoneType.Common,
            CosmeticId = "basic_glasses_01",
            Coins = 75,
            PremiumCurrency = 0,
            Title = "Nice Streak!",
            Description = "75 coins and new glasses"
        });

        AddReward(3, new StreakReward
        {
            Day = 3,
            RewardType = MilestoneType.Common,
            CosmeticId = "basic_moustache_01",
            Coins = 100,
            PremiumCurrency = 0,
            Title = "Building Momentum!",
            Description = "100 coins and moustache"
        });

        AddReward(4, new StreakReward
        {
            Day = 4,
            RewardType = MilestoneType.Common,
            CosmeticId = "",
            Coins = 125,
            PremiumCurrency = 0,
            Title = "Consistency Pays!",
            Description = "125 coins for staying consistent"
        });

        AddReward(5, new StreakReward
        {
            Day = 5,
            RewardType = MilestoneType.Common,
            CosmeticId = "basic_wig_01",
            Coins = 150,
            PremiumCurrency = 0,
            Title = "Half Week!",
            Description = "150 coins and a new wig"
        });

        AddReward(6, new StreakReward
        {
            Day = 6,
            RewardType = MilestoneType.Common,
            CosmeticId = "",
            Coins = 175,
            PremiumCurrency = 0,
            Title = "Almost There!",
            Description = "175 coins, almost a week!"
        });

        AddReward(7, new StreakReward
        {
            Day = 7,
            RewardType = MilestoneType.Common,
            CosmeticId = "legendary_hat_week1",
            Coins = 200,
            PremiumCurrency = 1,
            Title = "WEEK 1 COMPLETE! 🎉",
            Description = "200 coins, 1 premium coin, and exclusive hat!",
            IsMilestone = true,
            MilestoneDay = 7
        });

        // Days 8-14: Uncommon rewards
        AddReward(8, new StreakReward
        {
            Day = 8,
            RewardType = MilestoneType.Uncommon,
            CosmeticId = "uncommon_glasses_01",
            Coins = 225,
            PremiumCurrency = 0,
            Title = "Week 2 Begins!",
            Description = "225 coins and rare glasses"
        });

        AddReward(9, new StreakReward
        {
            Day = 9,
            RewardType = MilestoneType.Uncommon,
            CosmeticId = "",
            Coins = 250,
            PremiumCurrency = 0,
            Title = "Strong Streak!",
            Description = "250 coins for staying committed"
        });

        AddReward(10, new StreakReward
        {
            Day = 10,
            RewardType = MilestoneType.Uncommon,
            CosmeticId = "uncommon_projectile_skin_01",
            Coins = 275,
            PremiumCurrency = 0,
            Title = "Double Digits!",
            Description = "275 coins and projectile skin"
        });

        AddReward(11, new StreakReward
        {
            Day = 11,
            RewardType = MilestoneType.Uncommon,
            CosmeticId = "",
            Coins = 300,
            PremiumCurrency = 0,
            Title = "On Fire! 🔥",
            Description = "300 coins, keeping the momentum!"
        });

        AddReward(12, new StreakReward
        {
            Day = 12,
            RewardType = MilestoneType.Uncommon,
            CosmeticId = "uncommon_slinghot_skin_01",
            Coins = 325,
            PremiumCurrency = 0,
            Title = "Twelve Strong!",
            Description = "325 coins and slingshot skin"
        });

        AddReward(13, new StreakReward
        {
            Day = 13,
            RewardType = MilestoneType.Uncommon,
            CosmeticId = "",
            Coins = 350,
            PremiumCurrency = 0,
            Title = "Almost Legendary!",
            Description = "350 coins, so close to milestone!"
        });

        AddReward(14, new StreakReward
        {
            Day = 14,
            RewardType = MilestoneType.Uncommon,
            CosmeticId = "legendary_glasses_week2",
            Coins = 400,
            PremiumCurrency = 2,
            Title = "2 WEEK STREAK! 🔥🔥",
            Description = "400 coins, 2 premium coins, legendary glasses!",
            IsMilestone = true,
            MilestoneDay = 14
        });

        // Days 15-21: Rare rewards
        AddReward(15, new StreakReward
        {
            Day = 15,
            RewardType = MilestoneType.Rare,
            CosmeticId = "rare_hat_01",
            Coins = 450,
            PremiumCurrency = 0,
            Title = "Half Month!",
            Description = "450 coins and rare hat"
        });

        AddReward(16, new StreakReward
        {
            Day = 16,
            RewardType = MilestoneType.Rare,
            CosmeticId = "",
            Coins = 500,
            PremiumCurrency = 0,
            Title = "Dedication!",
            Description = "500 coins for your dedication"
        });

        AddReward(17, new StreakReward
        {
            Day = 17,
            RewardType = MilestoneType.Rare,
            CosmeticId = "rare_trail_effect_01",
            Coins = 550,
            PremiumCurrency = 0,
            Title = "Trail Blazer!",
            Description = "550 coins and trail effect"
        });

        AddReward(18, new StreakReward
        {
            Day = 18,
            RewardType = MilestoneType.Rare,
            CosmeticId = "",
            Coins = 600,
            PremiumCurrency = 0,
            Title = "Almost There!",
            Description = "600 coins, almost 3 weeks!"
        });

        AddReward(19, new StreakReward
        {
            Day = 19,
            RewardType = MilestoneType.Rare,
            CosmeticId = "rare_hit_effect_01",
            Coins = 650,
            PremiumCurrency = 0,
            Title = "Impact Player!",
            Description = "650 coins and hit effect"
        });

        AddReward(20, new StreakReward
        {
            Day = 20,
            RewardType = MilestoneType.Rare,
            CosmeticId = "",
            Coins = 700,
            PremiumCurrency = 0,
            Title = "One More Day!",
            Description = "700 coins, one day to milestone!"
        });

        AddReward(21, new StreakReward
        {
            Day = 21,
            RewardType = MilestoneType.Rare,
            CosmeticId = "legendary_moustache_week3",
            Coins = 800,
            PremiumCurrency = 3,
            Title = "3 WEEK MASTER! 🏆",
            Description = "800 coins, 3 premium coins, legendary moustache!",
            IsMilestone = true,
            MilestoneDay = 21
        });

        // Days 22-30: Legendary rewards
        AddReward(22, new StreakReward
        {
            Day = 22,
            RewardType = MilestoneType.Legendary,
            CosmeticId = "legendary_wig_01",
            Coins = 900,
            PremiumCurrency = 0,
            Title = "Final Stretch!",
            Description = "900 coins and legendary wig"
        });

        AddReward(23, new StreakReward
        {
            Day = 23,
            RewardType = MilestoneType.Legendary,
            CosmeticId = "",
            Coins = 1000,
            PremiumCurrency = 0,
            Title = "Legend Status!",
            Description = "1000 coins, you're a legend!"
        });

        AddReward(24, new StreakReward
        {
            Day = 24,
            RewardType = MilestoneType.Legendary,
            CosmeticId = "legendary_projectile_skin_02",
            Coins = 1100,
            PremiumCurrency = 0,
            Title = "Legendary Projectile!",
            Description = "1100 coins and projectile skin"
        });

        AddReward(25, new StreakReward
        {
            Day = 25,
            RewardType = MilestoneType.Legendary,
            CosmeticId = "",
            Coins = 1200,
            PremiumCurrency = 0,
            Title = "Silver Anniversary!",
            Description = "1200 coins, quarter milestone!"
        });

        AddReward(26, new StreakReward
        {
            Day = 26,
            RewardType = MilestoneType.Legendary,
            CosmeticId = "legendary_slinghot_skin_02",
            Coins = 1300,
            PremiumCurrency = 0,
            Title = "Legendary Slingshot!",
            Description = "1300 coins and slingshot skin"
        });

        AddReward(27, new StreakReward
        {
            Day = 27,
            RewardType = MilestoneType.Legendary,
            CosmeticId = "",
            Coins = 1400,
            PremiumCurrency = 0,
            Title = "Almost Perfect!",
            Description = "1400 coins, almost there!"
        });

        AddReward(28, new StreakReward
        {
            Day = 28,
            RewardType = MilestoneType.Legendary,
            CosmeticId = "legendary_trail_effect_02",
            Coins = 1500,
            PremiumCurrency = 0,
            Title = "Legendary Trail!",
            Description = "1500 coins and trail effect"
        });

        AddReward(29, new StreakReward
        {
            Day = 29,
            RewardType = MilestoneType.Legendary,
            CosmeticId = "",
            Coins = 1600,
            PremiumCurrency = 0,
            Title = "One More Day!",
            Description = "1600 coins, FINAL DAY!"
        });

        AddReward(30, new StreakReward
        {
            Day = 30,
            RewardType = MilestoneType.Legendary,
            CosmeticId = "legendary_crown_month_complete",
            Coins = 2000,
            PremiumCurrency = 5,
            Title = "MONTH MASTER! 👑",
            Description = "2000 coins, 5 premium coins, LEGENDARY CROWN!",
            IsMilestone = true,
            MilestoneDay = 30
        });

        GD.Print($"Initialized {_streakRewards.Count} streak rewards");
    }

    /// <summary>
    /// Add a reward to the streak progression
    /// </summary>
    private void AddReward(int day, StreakReward reward)
    {
        _streakRewards[day] = reward;
    }

    /// <summary>
    /// Setup daily reset timer for midnight UTC
    /// </summary>
    private void SetupDailyResetTimer()
    {
        _dailyResetTimer = new Timer();
        _dailyResetTimer.WaitTime = CalculateSecondsToMidnight();
        _dailyResetTimer.OneShot = true;
        _dailyResetTimer.Timeout += OnDailyReset;
        
        AddChild(_dailyResetTimer);
        _dailyResetTimer.Start();
        
        GD.Print($"Daily reset timer set for {Time.Get_datetime_string_from_system()} (in {_dailyResetTimer.WaitTime} seconds)");
    }

    /// <summary>
    /// Calculate seconds until next midnight UTC
    /// </summary>
    private float CalculateSecondsToMidnight()
    {
        var now = DateTime.UtcNow;
        var tomorrow = now.AddDays(1);
        var midnight = new DateTime(tomorrow.Year, tomorrow.Month, tomorrow.Day, 0, 0, 0);
        
        return (float)(midnight - now).TotalSeconds;
    }

    /// <summary>
    /// Check if player needs to claim daily reward
    /// </summary>
    public void CheckDailyLogin()
    {
        if (!_enableStreakSystem) return;

        var now = DateTime.UtcNow;
        _streakData.UpdateStreak(now);
        
        // Auto-claim reward if eligible
        if (_autoClaimRewards && _streakData.IsEligibleForReward())
        {
            ClaimDailyReward();
        }
    }

    /// <summary>
    /// Claim daily streak reward
    /// </summary>
    public void ClaimDailyReward()
    {
        if (!_streakData.IsEligibleForReward()) return;

        var currentDay = _streakData.CurrentStreak;
        if (_streakRewards.TryGetValue(currentDay, out var reward))
        {
            GrantReward(reward);
            _streakData.MarkRewardClaimed();
            
            EmitSignal("DailyRewardClaimed", reward);
            
            // Play reward sound
            PlayRewardSound();
            
            GD.Print($"Claimed reward for day {currentDay}: {reward.Title}");
        }
    }

    /// <summary>
    /// Grant reward to player
    /// </summary>
    private void GrantReward(StreakReward reward)
    {
        // Grant coins
        if (reward.Coins > 0)
        {
            // This would integrate with your currency system
            GD.Print($"Granted {reward.Coins} coins");
        }

        // Grant premium currency
        if (reward.PremiumCurrency > 0)
        {
            // This would integrate with your premium currency system
            GD.Print($"Granted {reward.PremiumCurrency} premium coins");
        }

        // Unlock cosmetic
        if (!string.IsNullOrEmpty(reward.CosmeticId))
        {
            // This would integrate with your cosmetics system
            GD.Print($"Unlocked cosmetic: {reward.CosmeticId}");
        }

        // Show milestone celebration
        if (reward.IsMilestone)
        {
            ShowMilestoneCelebration(reward);
        }
    }

    /// <summary>
    /// Show milestone celebration
    /// </summary>
    private void ShowMilestoneCelebration(StreakReward milestoneReward)
    {
        EmitSignal("MilestoneCelebration", milestoneReward.MilestoneDay);
        
        // Play milestone sound
        if (_milestoneSound != null)
        {
            AudioManager.Instance?.PlaySound(_milestoneSound);
        }
        
        // Show celebration effects
        if (EffectsManager.Instance != null)
        {
            EffectsManager.Instance.PlayCelebrationEffects();
        }
        
        GD.Print($"🎉 MILESTONE CELEBRATION: {milestoneReward.MilestoneDay}-day streak achieved!");
    }

    /// <summary>
    /// Play reward sound effect
    /// </summary>
    private void PlayRewardSound()
    {
        if (_rewardClaimSound != null)
        {
            AudioManager.Instance?.PlaySound(_rewardClaimSound);
        }
    }

    /// <summary>
    /// Daily reset callback (midnight UTC)
    /// </summary>
    private void OnDailyReset()
    {
        GD.Print("Daily reset triggered - resetting timers");
        
        // Reset daily claimed status
        _streakData.TodaysRewardClaimed = false;
        
        // Reset timer for next day
        SetupDailyResetTimer();
        
        // Save data
        SaveStreakData();
    }

    /// <summary>
    /// Get current streak reward
    /// </summary>
    public StreakReward GetCurrentReward()
    {
        var currentDay = _streakData.CurrentStreak;
        if (_streakRewards.TryGetValue(currentDay, out var reward))
        {
            return reward;
        }
        
        // Default reward for days beyond 30
        return new StreakReward
        {
            Day = currentDay,
            RewardType = MilestoneType.Legendary,
            CosmeticId = "",
            Coins = 2000,
            PremiumCurrency = 5,
            Title = "Legendary Bonus!",
            Description = "2000 coins and 5 premium coins"
        };
    }

    /// <summary>
    /// Get next milestone day
    /// </summary>
    public int GetNextMilestoneDay()
    {
        var currentDay = _streakData.CurrentStreak;
        var milestones = new[] { 7, 14, 21, 30 };
        
        foreach (var milestone in milestones)
        {
            if (currentDay < milestone)
                return milestone;
        }
        
        // After 30, cycle back to 7
        return 7;
    }

    /// <summary>
    /// Get days until next milestone
    /// </summary>
    public int GetDaysUntilNextMilestone()
    {
        var currentDay = _streakData.CurrentStreak;
        var nextMilestone = GetNextMilestoneDay();
        
        if (nextMilestone <= currentDay)
            nextMilestone += 30; // Cycle to next set
        
        return nextMilestone - currentDay;
    }

    /// <summary>
    /// Save streak data to persistent storage
    /// </summary>
    private void SaveStreakData()
    {
        // This would integrate with PlayerProfile saving
        // For now, we'll serialize to JSON and save
        
        var jsonData = _streakData.Serialize();
        GD.Print($"Saving streak data: {jsonData}");
        
        // In a real implementation, this would save to PlayerProfile
    }

    /// <summary>
    /// Load streak data from persistent storage
    /// </summary>
    private void LoadStreakData()
    {
        // This would integrate with PlayerProfile loading
        // For now, we'll start fresh
        
        _streakData.Load();
    }

    /// <summary>
    /// Get streak data for UI display
    /// </summary>
    public Dictionary<string, Variant> GetStreakDisplayData()
    {
        return new Dictionary<string, Variant>
        {
            ["current_streak"] = _streakData.CurrentStreak,
            ["best_streak"] = _streakData.BestStreak,
            ["streak_status"] = _streakData.GetStreakStatus(),
            ["is_eligible_for_reward"] = _streakData.IsEligibleForReward(),
            ["days_until_next_milestone"] = GetDaysUntilNextMilestone(),
            ["next_milestone_day"] = GetNextMilestoneDay(),
            ["current_reward"] = GetCurrentReward().Title,
            ["progress_to_next_milestone"] = GetProgressToNextMilestone()
        };
    }

    /// <summary>
    /// Get progress percentage to next milestone
    /// </summary>
    public float GetProgressToNextMilestone()
    {
        var currentDay = _streakData.CurrentStreak;
        var nextMilestone = GetNextMilestoneDay();
        
        var progress = (float)currentDay / nextMilestone;
        return Mathf.Clamp(progress, 0f, 1f);
    }

    /// <summary>
    /// Check if player has streak active
    /// </summary>
    public bool HasActiveStreak()
    {
        return _streakData.CurrentStreak > 0;
    }

    /// <summary>
    /// Get streak analytics data
    /// </summary>
    public Dictionary<string, Variant> GetStreakAnalytics()
    {
        return new Dictionary<string, Variant>
        {
            ["current_streak"] = _streakData.CurrentStreak,
            ["best_streak"] = _streakData.BestStreak,
            ["total_streak_days"] = _streakData.TotalStreakDays,
            ["achieved_milestones"] = new Array(_streakData.AchievedMilestones),
            ["streak_active"] = HasActiveStreak(),
            ["is_milestone_day"] = _streakData.IsMilestoneDay()
        };
    }
}

/// <summary>
/// Streak reward structure
/// </summary>
public partial class StreakReward : GodotObject
{
    public int Day { get; set; }
    public MilestoneType RewardType { get; set; }
    public string CosmeticId { get; set; } = "";
    public int Coins { get; set; }
    public int PremiumCurrency { get; set; }
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public bool IsMilestone { get; set; } = false;
    public int MilestoneDay { get; set; }
}