# Daily Streak Setup Guide 📅

**Non-Coder, Step-by-Step Guide for Configuring 30-Day Streak Rewards**

## Overview
This guide will teach you how to configure the daily login streak system without touching any code. You'll learn how to set up 30 days of escalating rewards, customize milestone celebrations, and manage player engagement through streaks.

## 🎯 What You'll Learn
- How to configure 30-day streak rewards in the Inspector
- Understanding the reward progression system
- How to customize milestone celebrations (Days 7, 14, 21, 30)
- How to adjust streak reset timing
- How to monitor player streak data

## 📋 Prerequisites
- Godot Editor access
- Basic understanding of the project structure
- No programming knowledge required!

## 🚀 Getting Started

### Step 1: Locate the StreakManager
1. Open Godot Editor
2. In the Scene panel, look for `StreakManager` node (usually in the root or under GameManager)
3. If not visible, check the `Globals` folder in the Project panel
4. The StreakManager will have a blue icon with "Streak" text

### Step 2: Understanding the Inspector Interface
When you select the StreakManager, you'll see these key properties in the Inspector:

#### Basic Settings
- **Enable Streak System** (Boolean): Turn the entire system on/off
- **Auto Claim Rewards** (Boolean): Automatically give rewards when player logs in
- **Show Streak Notifications** (Boolean): Display streak-related popups

#### Reward Configuration
- **Streak Rewards** (Dictionary): Contains all 30 days of rewards
- **Milestone Sound** (Audio): Sound played during milestone celebrations
- **Reward Claim Sound** (Audio): Sound played when claiming daily rewards

## 🏆 Configuring the 30-Day Reward Progression

### Understanding Reward Tiers
The streak system is divided into 4 reward tiers:

**Tier 1: Days 1-7 (Common Rewards)**
- Basic cosmetics and small coin amounts
- Goal: Get players hooked with easy wins
- Typical rewards: 50-200 coins + basic hat/glasses

**Tier 2: Days 8-14 (Uncommon Rewards)** 
- Better cosmetics and moderate coins
- Goal: Build momentum with improved rewards
- Typical rewards: 225-400 coins + uncommon cosmetics

**Tier 3: Days 15-21 (Rare Rewards)**
- High-quality cosmetics and premium currency
- Goal: Create excitement with rare items
- Typical rewards: 450-800 coins + rare cosmetics

**Tier 4: Days 22-30 (Legendary Rewards)**
- Exclusive cosmetics and premium bonuses
- Goal: Maximum engagement for month-long commitment
- Typical rewards: 900-2000 coins + legendary cosmetics

### Step 3: Customize Each Day's Reward

#### Method 1: Using the Inspector (Recommended for Non-Coders)

1. **Find the StreakManager node** in your scene
2. **Expand the "Streak Rewards" section** in the Inspector
3. **Click on Day 1** to see the current reward configuration:
   - **Day**: Should be "1"
   - **Reward Type**: Select "Common" for Days 1-7
   - **Cosmetic ID**: Enter the cosmetic identifier (e.g., "basic_hat_01")
   - **Coins**: Set the coin amount (e.g., 50)
   - **Premium Currency**: Set premium coins (usually 0 for early days)
   - **Title**: Display name for the reward (e.g., "Welcome Bonus!")
   - **Description**: What the player sees (e.g., "50 coins to get you started!")

4. **Continue for each day**, adjusting rewards to match your progression strategy

#### Method 2: Using JSON Configuration (Advanced)

For batch editing, you can modify the reward data in JSON format:

```json
{
  "1": {
    "Day": 1,
    "RewardType": "Common",
    "CosmeticId": "basic_hat_01",
    "Coins": 50,
    "PremiumCurrency": 0,
    "Title": "Welcome Bonus!",
    "Description": "50 coins to get you started!"
  }
}
```

## 🎉 Setting Up Milestone Celebrations

### Milestone Days: 7, 14, 21, 30

These are special days that deserve extra celebration:

#### Step 1: Configure Milestone Rewards
1. **Go to Day 7** in your StreakManager
2. **Check the "Is Milestone"** checkbox
3. **Set Milestone Day** to "7"
4. **Choose a legendary cosmetic** for this day
5. **Increase coin and premium currency** amounts significantly

Example Day 7 Configuration:
- **Cosmetic ID**: "legendary_hat_week1"
- **Coins**: 200
- **Premium Currency**: 1
- **Title**: "WEEK 1 COMPLETE! 🎉"
- **Description**: "200 coins, 1 premium coin, and exclusive hat!"

#### Step 2: Configure Celebration Effects
1. **Set Milestone Sound**: Choose a celebratory audio file
2. **Set Reward Claim Sound**: Choose a satisfying reward sound
3. **Test the effects** in play mode

## ⏰ Adjusting Streak Reset Timing

### Understanding Reset Behavior
- **Default**: Resets at midnight UTC (recommended for global players)
- **Alternative**: Can be set to local player time

### Step 1: Modify Reset Timing
1. **Select StreakManager** in your scene
2. **Look for "Reset Time"** setting in Inspector
3. **Choose your preferred timezone**:
   - **UTC**: Best for international audience
   - **Local Player Time**: Better for region-specific games

### Step 2: Test Reset Behavior
1. **Enter Play Mode**
2. **Simulate time passing** to test midnight reset
3. **Verify rewards reset** correctly at boundary

## 📊 Monitoring Player Streak Data

### Step 1: Access Analytics
1. **Look for "Analytics" or "Telemetry" section** in your game menu
2. **Find "Player Retention"** or "Streak Analytics" tab
3. **View real-time streak statistics**

### Key Metrics to Monitor
- **Current Active Streaks**: How many players have active streaks
- **Average Streak Length**: Typical streak duration
- **Milestone Achievement Rate**: How many reach Day 7, 14, 21, 30
- **Streak Break Points**: Where players typically give up

### Step 2: Interpret the Data
- **High Day 1-3 drop-off**: Rewards too small, not engaging enough
- **Day 7 achievement rate**: Target 40-60% for healthy engagement
- **Day 30 achievement rate**: Target 5-15% for legendary players

## 🔧 Common Configuration Scenarios

### Scenario 1: Casual Mobile Game
**Goal**: Easy entry, quick rewards
- Days 1-3: Very easy rewards (25-50 coins)
- Days 4-7: Small cosmetic + 75-100 coins
- Day 7: "Week 1" celebration
- Days 8-14: Moderate cosmetics + 150-250 coins
- Days 15-21: Better cosmetics + 300-500 coins
- Day 30: Premium cosmetic + 1000+ coins

### Scenario 2: Hardcore Gaming Community
**Goal**: Challenge and exclusivity
- Days 1-3: Basic rewards (10-25 coins)
- Days 4-7: Cosmetic focus over coins
- Day 7: Rare cosmetic milestone
- Days 8-21: Consistent cosmetic progression
- Days 22-30: Exclusive legendary cosmetics only

### Scenario 3: Freemium Monetization
**Goal**: Drive premium currency usage
- Days 1-7: Coins only (no premium currency)
- Days 8-14: 1-2 premium coins per week
- Days 15-21: 2-3 premium coins per week
- Days 22-30: 3-5 premium coins per day

## 🚨 Troubleshooting Common Issues

### Problem: Streaks Not Counting
**Symptoms**: Players report streaks aren't incrementing
**Solutions**:
1. **Check "Enable Streak System"** is checked in Inspector
2. **Verify reset timing** matches your intended schedule
3. **Test with fresh player** to see if initial streak starts

### Problem: Rewards Not Appearing
**Symptoms**: Players don't see daily rewards
**Solutions**:
1. **Check "Auto Claim Rewards"** is enabled
2. **Verify reward data** is properly configured for current day
3. **Check cosmetic IDs** exist in your cosmetic database

### Problem: Milestone Celebrations Not Triggering
**Symptoms**: Day 7, 14, 21, 30 don't show special effects
**Solutions**:
1. **Verify "Is Milestone"** is checked for milestone days
2. **Confirm "Milestone Day"** field matches the day number
3. **Check milestone sounds** are properly assigned

### Problem: Streak Resets Too Frequently/Infrequently
**Symptoms**: Timing doesn't match expectations
**Solutions**:
1. **Review reset timezone settings** (UTC vs Local)
2. **Check for time zone issues** if serving global audience
3. **Test with different device times** to verify behavior

## 🎨 Customization Tips

### Reward Descriptions
Make descriptions exciting and encouraging:
- **Bad**: "You get 50 coins"
- **Good**: "Starter pack loaded! 50 coins for your journey!"

### Cosmetic Naming
Use descriptive names that create excitement:
- **Basic**: "Red Hat"
- **Better**: "Starter Cap of Courage"
- **Best**: "Novice Warrior's Ceremonial Crown"

### Milestone Messaging
Create emotional connection:
- **Day 7**: "You've built an amazing habit! Week 1 Complete!"
- **Day 14**: "Two weeks of dedication! You're absolutely incredible!"
- **Day 30**: "LEGENDARY STATUS ACHIEVED! 30 days of pure dedication!"

## 📈 Optimization Guidelines

### Testing Your Configuration
1. **Start with conservative rewards** and test engagement
2. **Monitor Day 1-7 completion rates** as primary health metric
3. **Gradually increase rewards** based on player feedback
4. **A/B test different milestone rewards** to find optimal balance

### Seasonal Adjustments
- **Holidays**: Boost rewards during holiday seasons
- **Summer**: Add special summer-themed cosmetics
- **Back-to-School**: Create "New Semester" themed rewards

### Player Feedback Integration
- **Monitor player complaints** about reward amounts
- **Adjust based on community feedback**
- **Track which cosmetics** are most/least popular

## ✅ Success Checklist

Before going live, verify:
- [ ] All 30 days have configured rewards
- [ ] Milestone days (7, 14, 21, 30) have special configurations
- [ ] Celebration sounds are properly assigned
- [ ] Reset timing matches your intended schedule
- [ ] Analytics tracking is enabled
- [ ] Test streak progression with multiple test accounts
- [ ] Verify milestone celebrations trigger correctly
- [ ] Check that rewards persist across app restarts

## 🎯 Next Steps

After setting up your streak system:
1. **Read the Push Notification Setup Guide** to keep players engaged
2. **Check the Seasonal Events Creation Guide** for advanced engagement
3. **Review the Notification Strategy Guide** for optimal messaging
4. **Monitor analytics** and adjust based on player behavior

---

**Need Help?** 
- Check the in-game telemetry dashboard for real-time streak analytics
- Review the troubleshooting section above for common issues
- Test thoroughly before major launches

**Remember**: The goal is to create an engaging, rewarding experience that makes players want to return daily. Start simple and iterate based on player feedback and analytics data.