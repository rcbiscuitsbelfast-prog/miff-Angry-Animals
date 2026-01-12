using System;
using Godot;
using Newtonsoft.Json;

/// <summary>
/// Data structure for daily login streak tracking
/// </summary>
public partial class StreakData : Node
{
    [Signal] public delegate void StreakUpdatedEventHandler(int currentStreak, int bestStreak);
    [Signal] public delegate void StreakBrokenEventHandler();
    [Signal] public delegate void MilestoneReachedEventHandler(int dayNumber);

    /// <summary>
    /// Current consecutive login streak
    /// </summary>
    public int CurrentStreak { get; private set; } = 0;

    /// <summary>
    /// Best streak achieved historically
    /// </summary>
    public int BestStreak { get; private set; } = 0;

    /// <summary>
    /// Date of last successful login (UTC)
    /// </summary>
    public DateTime LastLoginDate { get; private set; } = DateTime.MinValue;

    /// <summary>
    /// Date when streak was last broken
    /// </summary>
    public DateTime LastStreakBrokenDate { get; private set; } = DateTime.MinValue;

    /// <summary>
    /// Whether player has claimed today's reward
    /// </summary>
    public bool TodaysRewardClaimed { get; private set; } = false;

    /// <summary>
    /// Total days participated in streak system
    /// </summary>
    public int TotalStreakDays { get; private set; } = 0;

    /// <summary>
    /// List of milestone days achieved (7, 14, 21, 30)
    /// </summary>
    public System.Collections.Generic.List<int> AchievedMilestones { get; private set; } = new();

    /// <summary>
    /// Calculate if a date is consecutive with last login
    /// </summary>
    public bool IsConsecutiveDay(DateTime currentDate)
    {
        if (LastLoginDate == DateTime.MinValue) return true;
        
        var expectedDate = LastLoginDate.AddDays(1);
        return currentDate.Date == expectedDate.Date;
    }

    /// <summary>
    /// Check if a given date is today (UTC)
    /// </summary>
    public bool IsToday(DateTime date)
    {
        return date.Date == DateTime.UtcNow.Date;
    }

    /// <summary>
    /// Update streak data for a new login
    /// </summary>
    public void UpdateStreak(DateTime loginDate)
    {
        var wasStreakActive = CurrentStreak > 0;
        var previousStreak = CurrentStreak;

        if (IsConsecutiveDay(loginDate))
        {
            // Continue streak
            CurrentStreak++;
            LastLoginDate = loginDate;
            TodaysRewardClaimed = false;
        }
        else if (LastLoginDate == DateTime.MinValue || IsToday(loginDate))
        {
            // First login or same day login
            if (LastLoginDate == DateTime.MinValue)
            {
                CurrentStreak = 1;
                TotalStreakDays++;
            }
            // For same day login, don't increment streak
            LastLoginDate = loginDate;
            TodaysRewardClaimed = false;
        }
        else
        {
            // Streak broken
            if (wasStreakActive)
            {
                LastStreakBrokenDate = loginDate;
                EmitSignal("StreakBroken");
            }
            
            CurrentStreak = 1;
            LastLoginDate = loginDate;
            TodaysRewardClaimed = false;
            TotalStreakDays++;
        }

        // Update best streak
        if (CurrentStreak > BestStreak)
        {
            BestStreak = CurrentStreak;
        }

        // Check for milestones
        CheckForMilestones(previousStreak, CurrentStreak);

        // Save data
        Save();
        
        EmitSignal("StreakUpdated", CurrentStreak, BestStreak);
    }

    /// <summary>
    /// Mark today's reward as claimed
    /// </summary>
    public void MarkRewardClaimed()
    {
        TodaysRewardClaimed = true;
        Save();
    }

    /// <summary>
    /// Check if current day qualifies for milestone celebration
    /// </summary>
    public bool IsMilestoneDay()
    {
        return CurrentStreak > 0 && (CurrentStreak == 7 || CurrentStreak == 14 || CurrentStreak == 21 || CurrentStreak == 30);
    }

    /// <summary>
    /// Get milestone reward type for current streak day
    /// </summary>
    public MilestoneType GetMilestoneType()
    {
        if (CurrentStreak >= 30) return MilestoneType.Legendary;
        if (CurrentStreak >= 22) return MilestoneType.Rare;
        if (CurrentStreak >= 15) return MilestoneType.Uncommon;
        if (CurrentStreak >= 8) return MilestoneType.Uncommon;
        return MilestoneType.Common;
    }

    /// <summary>
    /// Check for new milestones achieved
    /// </summary>
    private void CheckForMilestones(int previousStreak, int currentStreak)
    {
        var milestoneDays = new[] { 7, 14, 21, 30 };
        
        foreach (var milestoneDay in milestoneDays)
        {
            if (previousStreak < milestoneDay && currentStreak >= milestoneDay && !AchievedMilestones.Contains(milestoneDay))
            {
                AchievedMilestones.Add(milestoneDay);
                EmitSignal("MilestoneReached", milestoneDay);
                GD.Print($"🎉 Milestone reached: {milestoneDay}-day streak!");
            }
        }
    }

    /// <summary>
    /// Serialize streak data for JSON storage
    /// </summary>
    public string Serialize()
    {
        var data = new
        {
            current_streak = CurrentStreak,
            best_streak = BestStreak,
            last_login_date = LastLoginDate.ToString("O"),
            last_streak_broken_date = LastStreakBrokenDate.ToString("O"),
            todays_reward_claimed = TodaysRewardClaimed,
            total_streak_days = TotalStreakDays,
            achieved_milestones = AchievedMilestones
        };

        return JsonConvert.SerializeObject(data, Formatting.Indented);
    }

    /// <summary>
    /// Deserialize streak data from JSON
    /// </summary>
    public void Deserialize(string jsonData)
    {
        try
        {
            var data = JsonConvert.DeserializeObject<dynamic>(jsonData);
            if (data != null)
            {
                CurrentStreak = data.current_streak ?? 0;
                BestStreak = data.best_streak ?? 0;
                LastLoginDate = DateTime.Parse(data.last_login_date ?? DateTime.MinValue.ToString("O"));
                LastStreakBrokenDate = DateTime.Parse(data.last_streak_broken_date ?? DateTime.MinValue.ToString("O"));
                TodaysRewardClaimed = data.todays_reward_claimed ?? false;
                TotalStreakDays = data.total_streak_days ?? 0;
                
                AchievedMilestones.Clear();
                if (data.achieved_milestones != null)
                {
                    foreach (var milestone in data.achieved_milestones)
                    {
                        AchievedMilestones.Add((int)milestone);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            GD.PrintErr($"Failed to deserialize streak data: {ex.Message}");
        }
    }

    /// <summary>
    /// Save streak data to PlayerProfile
    /// </summary>
    private void Save()
    {
        if (PlayerProfile.Instance != null)
        {
            // This would integrate with PlayerProfile saving mechanism
            // For now, we'll trigger a save through the profile system
            PlayerProfile.Instance.Save();
        }
    }

    /// <summary>
    /// Load streak data from PlayerProfile
    /// </summary>
    public void Load()
    {
        // This would be called after PlayerProfile loads
        // The actual loading would happen through the profile deserialization
    }

    /// <summary>
    /// Get streak status summary for display
    /// </summary>
    public string GetStreakStatus()
    {
        if (CurrentStreak == 0)
        {
            return "Start your streak today!";
        }
        else if (CurrentStreak == 1)
        {
            return "Day 1 of your streak!";
        }
        else
        {
            return $"Day {CurrentStreak} of your streak!";
        }
    }

    /// <summary>
    /// Check if player is eligible for daily reward
    /// </summary>
    public bool IsEligibleForReward()
    {
        return CurrentStreak > 0 && !TodaysRewardClaimed;
    }
}

/// <summary>
/// Milestone reward types for streak progression
/// </summary>
public enum MilestoneType
{
    Common,      // Days 1-7
    Uncommon,    // Days 8-14  
    Rare,        // Days 15-21
    Legendary    // Days 22-30
}