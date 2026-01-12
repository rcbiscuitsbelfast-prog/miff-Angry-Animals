# Retention Systems Example Data 📋

**Sample Data and Configuration Examples for Testing and Setup**

## Overview
This document provides concrete examples of how to configure and test the retention systems. It includes sample data, configuration examples, and real-world scenarios you can use for testing and setup.

## 📊 Sample 30-Day Streak Reward Progression

### Tier 1: Days 1-7 (Common Rewards)
```json
{
  "1": {
    "Day": 1,
    "RewardType": "Common",
    "CosmeticId": "basic_hat_01",
    "Coins": 50,
    "PremiumCurrency": 0,
    "Title": "Welcome Bonus!",
    "Description": "50 coins to get you started!",
    "IsMilestone": false
  },
  "2": {
    "Day": 2,
    "RewardType": "Common", 
    "CosmeticId": "basic_glasses_01",
    "Coins": 75,
    "PremiumCurrency": 0,
    "Title": "Nice Streak!",
    "Description": "75 coins and new glasses!",
    "IsMilestone": false
  },
  "3": {
    "Day": 3,
    "RewardType": "Common",
    "CosmeticId": "basic_moustache_01", 
    "Coins": 100,
    "PremiumCurrency": 0,
    "Title": "Building Momentum!",
    "Description": "100 coins and moustache!",
    "IsMilestone": false
  },
  "4": {
    "Day": 4,
    "RewardType": "Common",
    "CosmeticId": "",
    "Coins": 125,
    "PremiumCurrency": 0,
    "Title": "Consistency Pays!",
    "Description": "125 coins for staying consistent!",
    "IsMilestone": false
  },
  "5": {
    "Day": 5,
    "RewardType": "Common",
    "CosmeticId": "basic_wig_01",
    "Coins": 150,
    "PremiumCurrency": 0,
    "Title": "Half Week!",
    "Description": "150 coins and a new wig!",
    "IsMilestone": false
  },
  "6": {
    "Day": 6,
    "RewardType": "Common",
    "CosmeticId": "",
    "Coins": 175,
    "PremiumCurrency": 0,
    "Title": "Almost There!",
    "Description": "175 coins, almost a week!",
    "IsMilestone": false
  },
  "7": {
    "Day": 7,
    "RewardType": "Common",
    "CosmeticId": "legendary_hat_week1",
    "Coins": 200,
    "PremiumCurrency": 1,
    "Title": "WEEK 1 COMPLETE! 🎉",
    "Description": "200 coins, 1 premium coin, and exclusive hat!",
    "IsMilestone": true,
    "MilestoneDay": 7
  }
}
```

### Tier 2: Days 8-14 (Uncommon Rewards)
```json
{
  "8": {
    "Day": 8,
    "RewardType": "Uncommon",
    "CosmeticId": "uncommon_glasses_01",
    "Coins": 225,
    "PremiumCurrency": 0,
    "Title": "Week 2 Begins!",
    "Description": "225 coins and rare glasses!",
    "IsMilestone": false
  },
  "9": {
    "Day": 9,
    "RewardType": "Uncommon",
    "CosmeticId": "",
    "Coins": 250,
    "PremiumCurrency": 0,
    "Title": "Strong Streak!",
    "Description": "250 coins for staying committed!",
    "IsMilestone": false
  },
  "10": {
    "Day": 10,
    "RewardType": "Uncommon",
    "CosmeticId": "uncommon_projectile_skin_01",
    "Coins": 275,
    "PremiumCurrency": 0,
    "Title": "Double Digits!",
    "Description": "275 coins and projectile skin!",
    "IsMilestone": false
  },
  "11": {
    "Day": 11,
    "RewardType": "Uncommon",
    "CosmeticId": "",
    "Coins": 300,
    "PremiumCurrency": 0,
    "Title": "On Fire! 🔥",
    "Description": "300 coins, keeping the momentum!",
    "IsMilestone": false
  },
  "12": {
    "Day": 12,
    "RewardType": "Uncommon",
    "CosmeticId": "uncommon_slinghot_skin_01",
    "Coins": 325,
    "PremiumCurrency": 0,
    "Title": "Twelve Strong!",
    "Description": "325 coins and slingshot skin!",
    "IsMilestone": false
  },
  "13": {
    "Day": 13,
    "RewardType": "Uncommon",
    "CosmeticId": "",
    "Coins": 350,
    "PremiumCurrency": 0,
    "Title": "Almost Legendary!",
    "Description": "350 coins, so close to milestone!",
    "IsMilestone": false
  },
  "14": {
    "Day": 14,
    "RewardType": "Uncommon",
    "CosmeticId": "legendary_glasses_week2",
    "Coins": 400,
    "PremiumCurrency": 2,
    "Title": "2 WEEK STREAK! 🔥🔥",
    "Description": "400 coins, 2 premium coins, legendary glasses!",
    "IsMilestone": true,
    "MilestoneDay": 14
  }
}
```

### Tier 3: Days 15-21 (Rare Rewards)
```json
{
  "15": {
    "Day": 15,
    "RewardType": "Rare",
    "CosmeticId": "rare_hat_01",
    "Coins": 450,
    "PremiumCurrency": 0,
    "Title": "Half Month!",
    "Description": "450 coins and rare hat!",
    "IsMilestone": false
  },
  "21": {
    "Day": 21,
    "RewardType": "Rare",
    "CosmeticId": "legendary_moustache_week3",
    "Coins": 800,
    "PremiumCurrency": 3,
    "Title": "3 WEEK MASTER! 🏆",
    "Description": "800 coins, 3 premium coins, legendary moustache!",
    "IsMilestone": true,
    "MilestoneDay": 21
  }
}
```

### Tier 4: Days 22-30 (Legendary Rewards)
```json
{
  "22": {
    "Day": 22,
    "RewardType": "Legendary",
    "CosmeticId": "legendary_wig_01",
    "Coins": 900,
    "PremiumCurrency": 0,
    "Title": "Final Stretch!",
    "Description": "900 coins and legendary wig!",
    "IsMilestone": false
  },
  "30": {
    "Day": 30,
    "RewardType": "Legendary",
    "CosmeticId": "legendary_crown_month_complete",
    "Coins": 2000,
    "PremiumCurrency": 5,
    "Title": "MONTH MASTER! 👑",
    "Description": "2000 coins, 5 premium coins, LEGENDARY CROWN!",
    "IsMilestone": true,
    "MilestoneDay": 30
  }
}
```

## 🎉 Sample Seasonal Events

### Winter Wonderland Event
```json
{
  "EventId": "winter_wonderland_2024",
  "EventName": "Winter Wonderland",
  "EventDescription": "Bundle up for a frosty adventure! Unlock exclusive ice-themed cosmetics and special winter effects.",
  "EventTheme": "Winter",
  "StartDate": "2024-12-01T00:00:00Z",
  "EndDate": "2025-01-31T23:59:59Z",
  "ThemeColor": "#B3E5FC",
  "EventCosmetics": [
    "winter_hat_santa",
    "winter_glasses_snow",
    "winter_moustache_frost",
    "winter_wig_ice_queen",
    "winter_projectile_snowball"
  ],
  "EventChallenges": [
    "play_5_levels_in_snow_conditions",
    "complete_10_perfect_scores",
    "unlock_all_winter_cosmetics"
  ],
  "EventRewards": {
    "tier_1": {
      "coins": 500,
      "cosmetic_id": "winter_hat_santa",
      "description": "500 coins + Santa Hat"
    },
    "tier_2": {
      "coins": 750,
      "cosmetic_id": "winter_glasses_snow", 
      "description": "750 coins + Snow Glasses"
    },
    "tier_3": {
      "coins": 1000,
      "cosmetic_id": "winter_wig_ice_queen",
      "description": "1000 coins + Ice Queen Wig"
    }
  }
}
```

### Spring Bloom Event
```json
{
  "EventId": "spring_bloom_2024",
  "EventName": "Spring Bloom",
  "EventDescription": "Celebrate new beginnings! Discover nature-themed cosmetics and flower effects as you blossom through challenges.",
  "EventTheme": "Spring",
  "StartDate": "2024-03-01T00:00:00Z",
  "EndDate": "2024-04-30T23:59:59Z",
  "ThemeColor": "#A5D6A7",
  "EventCosmetics": [
    "spring_hat_flower_crown",
    "spring_glasses_butterfly",
    "spring_moustache_dandelion",
    "spring_wig_garden",
    "spring_projectile_petals"
  ],
  "EventChallenges": [
    "complete_3_daily_challenges",
    "play_with_3_different_animals",
    "achieve_5_combo_masteries"
  ]
}
```

## 📱 Sample Push Notification Messages

### Daily Reminder Notifications
```json
{
  "daily_reminder_new_player": {
    "Title": "🎁 Daily Reward Awaits!",
    "Body": "Start your streak today! Claim your welcome bonus!",
    "Type": "DailyReminder",
    "Scheduling": "9:00 AM local time"
  },
  "daily_reminder_active_streak": {
    "Title": "🔥 Day {streak_day} of your streak!",
    "Body": "Claim your {reward_name} now and keep this amazing momentum going!",
    "Type": "DailyReminder",
    "Scheduling": "9:00 AM local time"
  },
  "daily_reminder_milestone_buildup": {
    "Title": "⚡ Almost there!",
    "Body": "Day {streak_day} complete! Only {days_to_milestone} more days until your {milestone_name} milestone!",
    "Type": "DailyReminder", 
    "Scheduling": "9:00 AM local time"
  }
}
```

### Milestone Celebration Notifications
```json
{
  "milestone_week_1": {
    "Title": "🏆 WEEK 1 COMPLETE!",
    "Body": "Amazing! You've maintained a 7-day streak! Keep it going!",
    "Type": "Milestone",
    "Emoji": "🏆",
    "Priority": "High"
  },
  "milestone_week_2": {
    "Title": "🔥 2 WEEK CHAMPION!",
    "Body": "Incredible! 2 weeks of dedication! You're on fire!",
    "Type": "Milestone",
    "Emoji": "🔥",
    "Priority": "High"
  },
  "milestone_month_master": {
    "Title": "👑 LEGENDARY STATUS!",
    "Body": "30 days of dedication! You're officially a retention master!",
    "Type": "Milestone",
    "Emoji": "👑",
    "Priority": "Critical"
  }
}
```

### Lapsed Player Notifications
```json
{
  "lapsed_gentle_reminder": {
    "Title": "We Miss You! 💙",
    "Body": "Come back for {days_away} exclusive rewards! We saved something special for you!",
    "Type": "LapsedPlayer",
    "Timing": "3 days after last session"
  },
  "lapsed_fomo": {
    "Title": "⏰ Don't Miss Out!",
    "Body": "Your {milestone_name} milestone expires soon! Come claim your rewards!",
    "Type": "LapsedPlayer", 
    "Timing": "5 days after last session"
  },
  "lapsed_comeback": {
    "Title": "🎁 Welcome Back Bundle!",
    "Body": "Special comeback rewards unlocked! We missed having you here!",
    "Type": "LapsedPlayer",
    "Timing": "7 days after last session"
  }
}
```

## 🎮 Testing Scenarios

### Test Scenario 1: New Player Journey
**Goal**: Test complete new player experience
**Steps**:
1. Create fresh player profile
2. Open game → Should see login bonus screen
3. Claim Day 1 reward → Verify coin/cosmetic granted
4. Exit and reopen game next day → Should increment to Day 2
5. Continue for 7 days → Verify milestone celebration at Day 7

**Expected Results**:
- Day 1: Login bonus screen appears, reward granted
- Days 2-6: Daily login works, no celebration
- Day 7: Milestone celebration triggers, special reward granted

### Test Scenario 2: Streak Break and Recovery
**Goal**: Test streak breaking and recovery
**Steps**:
1. Start with 5-day streak
2. Skip one day (don't open game)
3. Open game on day 7 → Should reset to Day 1
4. Check if streak broken notification sent
5. Continue for 7 days → Should celebrate Day 7 recovery

**Expected Results**:
- Day 7: Streak resets to 1, broken alert notification
- Day 8-13: Normal progression
- Day 14: Milestone celebration for recovered streak

### Test Scenario 3: Seasonal Event Participation
**Goal**: Test complete event experience
**Steps**:
1. Set up Winter Wonderland event (active dates)
2. Player opens game → Should see event notification
3. Navigate to Seasonal Events screen
4. View event cosmetics and challenges
5. Complete event challenges → Unlock cosmetics
6. Event ends → Should send completion notification

**Expected Results**:
- Event start: Notification sent, event visible in menu
- During event: Challenges track progress, cosmetics unlock
- Event end: Completion celebration, event marked as ended

### Test Scenario 4: Notification Optimization
**Goal**: Test notification timing and effectiveness
**Steps**:
1. Enable all notification types
2. Set daily reminder to current time + 1 minute
3. Create milestone scenario (set streak to 6, increment to 7)
4. Simulate lapsed player (skip 3+ days)
5. Verify all notifications send correctly

**Expected Results**:
- Daily reminder: Sends at configured time
- Milestone: Sends immediately upon streak increment
- Lapsed player: Sends after 3-day threshold

## 📊 Sample Analytics Data

### Retention Metrics Sample
```json
{
  "retention_metrics": {
    "daily_active_users": 12500,
    "d1_retention": 0.68,
    "d7_retention": 0.31,
    "d30_retention": 0.15,
    "average_session_length": 14.5,
    "daily_login_rate": 0.72
  },
  "streak_metrics": {
    "players_with_active_streaks": 8750,
    "active_streak_rate": 0.70,
    "average_streak_length": 4.8,
    "milestone_achievement_rates": {
      "day_7": 0.42,
      "day_14": 0.18,
      "day_21": 0.08,
      "day_30": 0.03
    },
    "streak_break_rate": 0.15
  },
  "event_metrics": {
    "winter_event_participation": 5625,
    "participation_rate": 0.45,
    "completion_rate": 0.23,
    "most_popular_cosmetic": "winter_hat_santa",
    "least_popular_cosmetic": "winter_projectile_snowball"
  },
  "notification_metrics": {
    "daily_reminder_open_rate": 0.22,
    "milestone_notification_open_rate": 0.38,
    "notification_opt_out_rate": 0.02,
    "conversion_rate_to_gameplay": 0.65
  }
}
```

### Cohort Analysis Sample
```json
{
  "cohort_analysis": {
    "cohort_2024_01_01": {
      "player_count": 1500,
      "d1_retention": 0.72,
      "d7_retention": 0.35,
      "d30_retention": 0.18,
      "average_streak_length": 5.2,
      "event_participation_rate": 0.48
    },
    "cohort_2024_01_08": {
      "player_count": 1680,
      "d1_retention": 0.75,
      "d7_retention": 0.38,
      "d30_retention": 0.21,
      "average_streak_length": 5.8,
      "event_participation_rate": 0.52
    },
    "cohort_2024_01_15": {
      "player_count": 1420,
      "d1_retention": 0.71,
      "d7_retention": 0.33,
      "d30_retention": 0.17,
      "average_streak_length": 4.9,
      "event_participation_rate": 0.44
    }
  }
}
```

## 🔧 Configuration Examples

### Sample Notification Preferences
```json
{
  "notification_preferences": {
    "push_notifications_enabled": true,
    "daily_reminder_enabled": true,
    "daily_reminder_time": "09:00",
    "milestone_notifications_enabled": true,
    "streak_broken_alerts_enabled": true,
    "seasonal_event_notifications_enabled": true,
    "lapsed_player_notifications_enabled": true,
    "quiet_hours_start": "22:00",
    "quiet_hours_end": "08:00",
    "max_notifications_per_day": 3,
    "lapsed_player_threshold": 3,
    "consent_date": "2024-01-15T10:30:00Z",
    "consent_version": "1.0"
  }
}
```

### Sample Player Profile with Retention Data
```json
{
  "profile_name": "TestPlayer",
  "retention": {
    "streak_data": {
      "current_streak": 12,
      "best_streak": 18,
      "total_streak_days": 45,
      "achieved_milestones": [7, 14],
      "streak_active": true,
      "last_login_date": "2024-01-15T14:30:00Z"
    },
    "seasonal_events": {
      "winter_wonderland_2024": {
        "participation_start_date": "2024-01-01T00:00:00Z",
        "completion_percentage": 0.65,
        "event_completed": false,
        "unlocked_cosmetics": ["winter_hat_santa", "winter_glasses_snow"]
      }
    },
    "notification_preferences": {
      "notifications_enabled": true,
      "daily_reminder_enabled": true,
      "milestone_notifications_enabled": true
    },
    "last_session_date": "2024-01-15T14:30:00Z"
  }
}
```

## 🎯 Real-World Testing Scenarios

### Scenario A: Optimistic Player (High Engagement)
**Profile**:
- New player who loves daily rewards
- Checks game multiple times per day
- Excited about cosmetics and achievements
- Likely to maintain long streaks

**Expected Behavior**:
- Claims all daily rewards
- Reaches 30+ day streaks
- High event participation
- Responds well to notifications
- Becomes premium player

**Test Case**: Verify milestone celebrations don't become annoying

### Scenario B: Casual Player (Medium Engagement)
**Profile**:
- Plays 3-4 times per week
- Interested but not obsessed
- Forgets to check daily sometimes
- Responds to FOMO tactics

**Expected Behavior**:
- Inconsistent streak maintenance
- Moderate event participation
- Responds to "last chance" notifications
- Will purchase cosmetics during events

**Test Case**: Verify streak break alerts encourage return

### Scenario C: Busy Player (Low Engagement)
**Profile**:
- Very limited time for gaming
- Plays maybe once per week
- Only engages during special events
- Easily overwhelmed by notifications

**Expected Behavior**:
- Rarely maintains streaks >3 days
- Event participation only during major events
- Opts out of most notifications
- High churn risk

**Test Case**: Verify retention systems don't create frustration

### Scenario D: Returning Player (Lapsed)
**Profile**:
- Used to play regularly
- Stopped for work/personal reasons
- Still has positive sentiment
- Reachable with right incentives

**Expected Behavior**:
- Responds to "we miss you" messaging
- Motivated by comeback bonuses
- May restart streaks if motivated
- High potential for re-engagement

**Test Case**: Verify lapse detection and comeback campaigns work

## 📈 Performance Benchmarks

### Expected Results (10K DAU Baseline)
- **Daily Login Rate**: 72% (7,200 players daily)
- **Streak Participation**: 70% (5,040 players with active streaks)
- **Day 7 Milestone Achievement**: 42% (2,100 players)
- **Event Participation**: 45% (3,375 players)
- **Notification Open Rates**:
  - Daily Reminders: 22%
  - Milestone Celebrations: 38%
  - Lapsed Player Alerts: 12%

### Retention Impact Projections
- **Baseline D7 Retention**: 20% (2,000 players)
- **With Streak System**: +15% = 23% (2,300 players) 
- **With Push Notifications**: +10% = 22% (2,200 players)
- **With Seasonal Events**: +5% = 21% (2,100 players)
- **Combined System**: ~+25% D7 retention = 25% (2,500 players)

**Net Retention Gain**: +500 players D7 retention = +25% improvement

### Revenue Impact Projections
- **Event Cosmetic Sales**: $5,000-10,000/month
- **Premium Currency Sales**: +15% during events
- **User Lifetime Value**: +20-30% for retained players
- **Year 1 Total Revenue Lift**: $60,000-120,000

## ✅ Testing Checklist

### Pre-Launch Testing:
- [ ] All 30 streak days configured with appropriate rewards
- [ ] Milestone celebrations (7, 14, 21, 30) trigger correctly
- [ ] Seasonal events activate/deactivate on schedule
- [ ] Push notifications send at correct times
- [ ] Player profile saves/loads retention data correctly
- [ ] Analytics events fire for all retention actions
- [ ] Cross-platform testing (Android/iOS)
- [ ] Privacy compliance and consent flow

### User Acceptance Testing:
- [ ] New player journey feels engaging and rewarding
- [ ] Streak progression motivates continued play
- [ ] Event cosmetics feel exclusive and desirable
- [ ] Notifications provide value without being spammy
- [ ] Milestone celebrations feel special and earned
- [ ] System handles edge cases gracefully

### Performance Testing:
- [ ] System doesn't impact game performance
- [ ] Analytics events batch properly
- [ ] Data persistence is reliable
- [ ] No memory leaks with long-running sessions
- [ ] Notification delivery is reliable

---

**Remember**: Use these examples as starting points and adapt them to your specific game theme, player base, and business goals. The key is testing with real players and iterating based on actual behavior data!