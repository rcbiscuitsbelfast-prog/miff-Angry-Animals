# Seasonal Events Creation Guide 🎉

**Non-Coder, Detailed Guide for Creating and Managing Seasonal Events**

## Overview
This guide will teach you how to create, configure, and manage seasonal events to drive engagement, create FOMO (fear of missing out), and boost cosmetic sales. You'll learn to create themed events, assign exclusive cosmetics, set schedules, and analyze event performance.

## 🎯 What You'll Learn
- How to create a new seasonal event from scratch
- How to assign cosmetics to events and set unlock requirements
- How to configure event timing and automatic activation
- How to customize event visual themes and rewards
- How to create optional event challenges
- How to preview events before launch
- Common event configuration mistakes to avoid

## 📋 Prerequisites
- Godot Editor access
- Access to your cosmetic database/inventory
- Understanding of your game's reward system
- Basic planning skills for event strategy

## 🏗️ Creating Your First Seasonal Event

### Step 1: Understand Event Types

#### Recurring Events (Recommended for Beginners)
- **Repeat annually** on same dates
- **Same cosmetics** and rewards each year
- **Consistent player expectations**
- **Easier to manage and promote**

#### One-Time Events
- **Unique theme** and cosmetics
- **Limited availability** creates urgency
- **Higher engagement** per event
- **More complex** to manage

### Step 2: Create New Seasonal Event

#### Method 1: Using SeasonalEventDatabase (Inspector)
1. **Open Godot Editor**
2. **Find SeasonalEventDatabase** node (usually autoloaded)
3. **In Inspector, find "Create Event" button**
4. **Click "Create New Event"**

#### Method 2: Using Inspector Interface
1. **Navigate to Globals/SeasonalEventDatabase**
2. **In Project panel, find existing event** (like "Winter Wonderland")
3. **Right-click and "Duplicate"**
4. **Rename the duplicate** (e.g., "Spring Bloom 2024")

### Step 3: Configure Basic Event Information

#### Essential Settings:
1. **Event ID**: `[event_name]_[year]_[month]`
   - Examples: `winter_wonderland_2024_12`, `halloween_special_2024_10`
   - Auto-generated if left empty

2. **Event Name**: Display name for players
   - Examples: "Winter Wonderland", "Spring Bloom Festival"
   - Keep it catchy and memorable (3-6 words)

3. **Event Description**: What players can expect
   - 1-2 sentences explaining the theme and rewards
   - Example: "Bundle up for a frosty adventure! Unlock exclusive ice-themed cosmetics and special winter effects."

4. **Event Theme**: Internal category for organization
   - Examples: "Winter", "Spring", "Halloween", "Summer"
   - Used for filtering and analytics

## 🎨 Assigning Event Cosmetics

### Step 4: Select Event-Exclusive Cosmetics

#### Choosing the Right Cosmetics:
1. **Amount**: 3-5 cosmetics per event (optimal range)
   - **Too few** (1-2): Not enough variety
   - **Too many** (6+): Dilutes exclusivity

2. **Types**: Mix of cosmetic categories
   - **1-2 Hats** (most visible, popular category)
   - **1 Glasses** (second most popular)
   - **1-2 Specialty** (moustaches, wigs, projectiles)

3. **Quality Tier**: Match event importance
   - **Major events** (holidays): 1-2 legendary, rest rare
   - **Minor events** (seasonal): Mostly rare, 1 legendary
   - **Weekly events**: Uncommon, 1 rare

#### Assigning Cosmetics to Events:
1. **In Event Configuration**, find **"Event Cosmetics"** array
2. **Add cosmetic IDs** from your database:
   ```json
   [
     "winter_hat_santa",
     "winter_glasses_snow", 
     "winter_moustache_frost",
     "winter_wig_ice_queen",
     "winter_projectile_snowball"
   ]
   ```

3. **Verify cosmetics exist** in your cosmetic database
4. **Check visual consistency** with event theme

### Step 5: Set Cosmetic Unlock Requirements

#### Unlock Method Options:

**Progress-Based Unlocking**:
- **Complete event challenges** to unlock cosmetics
- **Best for**: Competitive events, long-term engagement
- **Requirements**: 50%, 75%, 100% event completion

**Time-Based Unlocking**:
- **Cosmetic unlocks automatically** over event duration
- **Best for**: Casual events, guaranteed rewards
- **Example**: Week 1 = Hat, Week 2 = Glasses, etc.

**Purchase-Based Unlocking**:
- **Cosmetics available for purchase** with premium currency
- **Best for**: Monetization focus
- **Price suggestion**: 2-5 premium coins per cosmetic

## ⏰ Configuring Event Timing

### Step 6: Set Event Schedule

#### Event Duration Guidelines:
- **Short Events**: 1-2 weeks (creates urgency)
  - **Best for**: Holiday-themed, special occasions
  - **Duration**: 7-14 days
  - **Engagement**: High urgency, FOMO-driven

- **Medium Events**: 3-4 weeks (balanced approach)
  - **Best for**: Seasonal changes, regular content
  - **Duration**: 21-28 days
  - **Engagement**: Sustainable, allows vacation missed days

- **Long Events**: 4+ weeks (maximum engagement)
  - **Best for**: Major updates, new content launches
  - **Duration**: 28+ days
  - **Engagement**: Deep engagement, lifestyle integration

#### Date Configuration:
1. **Start Date**: When event becomes active
   - **Consider**: Player timezone, weekend timing
   - **Recommendation**: Tuesday-Thursday (avoids weekend confusion)

2. **End Date**: When event expires
   - **Consider**: Midnight UTC for global players
   - **Warning**: Events ending on holidays may lose engagement

3. **Timezone Handling**:
   - **UTC**: Recommended for global games
   - **Server Time**: For region-specific games
   - **Local**: Only for single-region games

### Step 7: Configure Automatic Activation

#### Auto-Activation Setup:
1. **In SeasonalEventManager**, ensure:
   - **Enable Event System**: ✅ True
   - **Event Check Interval**: 300 seconds (5 minutes)
   - **Auto-Activate Events**: ✅ True

2. **Event Status Tracking**:
   - **Scheduled**: Future start date
   - **Active**: Currently running
   - **Ended**: Past end date

#### Event Lifecycle Management:
- **System checks** every 5 minutes for status changes
- **Automatic notifications** sent when events start/end
- **Player data** preserved for ended events (analytics)

## 🎁 Setting Event Rewards

### Step 8: Design Reward Progression

#### Tiered Reward System:
1. **Tier 1 (Early Event)**:
   - **Requirements**: Join event
   - **Rewards**: Basic cosmetic + small coins (100-200)
   - **Purpose**: Encourage initial participation

2. **Tier 2 (Mid Event)**:
   - **Requirements**: Complete basic challenges
   - **Rewards**: Uncommon cosmetic + coins (200-300)
   - **Purpose**: Maintain engagement

3. **Tier 3 (Late Event)**:
   - **Requirements**: Complete advanced challenges
   - **Rewards**: Rare cosmetic + premium currency (2-3 coins)
   - **Purpose**: Create urgency, reward dedication

4. **Tier 4 (Completion)**:
   - **Requirements**: 100% event completion
   - **Rewards**: Legendary cosmetic + premium currency (5+ coins)
   - **Purpose**: Maximum achievement, bragging rights

#### Sample Reward Configuration:
```json
{
  "tier_1": {
    "coins": 150,
    "cosmetic_id": "event_hat_basic",
    "description": "150 coins + Basic Event Hat"
  },
  "tier_2": {
    "coins": 250,
    "cosmetic_id": "event_glasses_uncommon",
    "description": "250 coins + Uncommon Event Glasses"
  },
  "tier_3": {
    "coins": 400,
    "cosmetic_id": "event_moustache_rare",
    "description": "400 coins + Rare Event Moustache"
  },
  "tier_4": {
    "coins": 600,
    "cosmetic_id": "event_crown_legendary",
    "description": "600 coins + Legendary Event Crown"
  }
}
```

## 🎨 Customizing Event Visuals

### Step 9: Theme Configuration

#### Visual Theme Elements:
1. **Theme Color**: Primary color for UI elements
   - **Winter**: Ice blue (0.7, 0.9, 1.0)
   - **Spring**: Fresh green (0.6, 0.8, 0.4)
   - **Summer**: Ocean blue (0.2, 0.7, 1.0)
   - **Fall**: Autumn orange (0.8, 0.4, 0.2)

2. **Event Background**: Custom background image
   - **Requirements**: 1920x1080 or higher resolution
   - **Style**: Matches event theme
   - **Format**: PNG with transparency support

3. **Event Music**: Background audio (optional)
   - **Length**: 30-60 second loop
   - **Style**: Matches theme mood
   - **Format**: OGG or MP3

#### Creating Custom Backgrounds:
1. **Design Considerations**:
   - **Subtle patterns** that don't interfere with UI
   - **Consistent color palette** with theme colors
   - **Seasonal elements** relevant to event
   - **Non-distracting** while game is being played

2. **Technical Requirements**:
   - **Resolution**: Minimum 1920x1080
   - **Format**: PNG (for transparency) or JPG
   - **File Size**: <2MB for mobile performance
   - **Aspect Ratio**: 16:9 preferred

### Step 10: UI Customization

#### Event Screen Modifications:
1. **Event Title Styling**:
   - **Font**: Bold, game-appropriate font
   - **Size**: Large enough to be prominent
   - **Color**: Contrasts well with background
   - **Effects**: Subtle glow or shadow for depth

2. **Countdown Timer Styling**:
   - **Format**: "X days Y hours remaining"
   - **Color Coding**:
     - **Green**: >7 days remaining
     - **Yellow**: 3-7 days remaining
     - **Red**: <3 days remaining (urgency)

3. **Progress Bar Styling**:
   - **Theme colors** matching event
   - **Smooth gradients** for polish
   - **Milestone markers** at 25%, 50%, 75%, 100%

## 🎯 Creating Event Challenges (Optional)

### Step 11: Design Event Challenges

#### Challenge Types:
1. **Gameplay Challenges**:
   - "Complete 10 levels with perfect accuracy"
   - "Score 50,000 points in a single level"
   - "Use 5 different animal types"

2. **Engagement Challenges**:
   - "Play for 3 consecutive days"
   - "Share your score on social media"
   - "Invite 2 friends to play"

3. **Collection Challenges**:
   - "Unlock 3 basic cosmetics"
   - "Earn 1,000 coins"
   - "Complete 5 daily challenges"

#### Challenge Configuration:
1. **Add Challenge IDs** to Event Challenges array:
   ```json
   [
     "complete_10_perfect_scores",
     "unlock_all_event_cosmetics",
     "play_daily_for_7_days"
   ]
   ```

2. **Set Challenge Requirements**:
   - **Clear objectives** that players understand
   - **Reasonable difficulty** for average players
   - **Mix of challenge types** for variety
   - **Achievement rewards** for completion

### Step 12: Challenge Integration

#### Connecting Challenges to Game Systems:
1. **Daily Challenge Integration**:
   - Link event challenges with existing DailyChallengeManager
   - Track progress in real-time
   - Award event progress for daily completions

2. **Leaderboard Integration**:
   - Create event-specific leaderboards
   - Award bonus points for event participation
   - Show event rankings prominently

3. **Achievement Integration**:
   - Create event-specific achievements
   - Track challenge completion percentages
   - Award bonus rewards for achievement hunting

## 🧪 Event Testing and Preview

### Step 13: Preview Events Before Launch

#### Testing Checklist:
1. **Visual Preview**:
   - **Background loads** correctly
   - **Colors display** as intended
   - **UI elements** don't overlap
   - **Text is readable** on background

2. **Timing Preview**:
   - **Start/end dates** are correct
   - **Countdown timer** displays accurately
   - **Timezone handling** works properly
   - **DST transitions** handled correctly

3. **Content Preview**:
   - **All cosmetics** are assigned correctly
   - **Challenge definitions** are clear
   - **Reward amounts** are appropriate
   - **Milestone celebrations** trigger correctly

#### Testing Methods:
1. **Event Database Preview**:
   - **Select event** in SeasonalEventDatabase
   - **Click "Preview"** button
   - **Review all settings** in preview window

2. **Live Testing**:
   - **Create test event** with near start date
   - **Monitor automatic activation** process
   - **Verify player experience** from start to end

### Step 14: Event Launch Checklist

#### Pre-Launch Verification:
- [ ] Event name and description finalized
- [ ] All cosmetics assigned and verified
- [ ] Start/end dates confirmed
- [ ] Visual themes tested on multiple devices
- [ ] Event challenges configured and tested
- [ ] Reward progression balanced
- [ ] Analytics tracking enabled
- [ ] Notification templates updated
- [ ] Social media announcements prepared
- [ ] Customer support briefed on event details

## 📊 Event Analytics and Optimization

### Step 15: Monitoring Event Performance

#### Key Metrics to Track:
1. **Participation Metrics**:
   - **Total participants**: Number of players who joined
   - **Daily active participants**: Day-over-day engagement
   - **Completion rate**: % who completed all challenges
   - **Retention rate**: % still active at event end

2. **Engagement Metrics**:
   - **Challenge completion**: Which challenges most/least popular
   - **Time to completion**: Average time to finish event
   - **Session frequency**: How often players return
   - **Social sharing**: Players who share event progress

3. **Monetization Metrics**:
   - **Cosmetics unlocked**: Which cosmetics most popular
   - **Premium currency usage**: Coins spent during event
   - **Conversion rate**: % who purchased event cosmetics
   - **Revenue per participant**: Average spend during event

#### Using Analytics for Optimization:
1. **Identify successful elements**:
   - **High completion rates**: Replicate in future events
   - **Popular cosmetics**: Use similar designs
   - **Optimal timing**: Use same day/time for launches

2. **Address problem areas**:
   - **Low completion rates**: Reduce challenge difficulty
   - **Poor engagement**: Improve reward distribution
   - **Low monetization**: Adjust cosmetic appeal/price

## 🚨 Common Event Configuration Mistakes

### Mistake 1: Too Many Cosmetics
**Problem**: 8+ cosmetics per event dilutes exclusivity
**Solution**: Stick to 3-5 high-quality cosmetics

### Mistake 2: Unrealistic Timeframes
**Problem**: 1-week event with 10 difficult challenges
**Solution**: Match challenge difficulty to available time

### Mistake 3: Poor Theme Consistency
**Problem**: Summer event with winter cosmetics
**Solution**: Ensure all elements match the declared theme

### Mistake 4: No Clear Rewards
**Problem**: Players don't understand what they can earn
**Solution**: Clear progression with visible milestones

### Mistake 5: Overlapping Events
**Problem**: Multiple events running simultaneously confuse players
**Solution**: Stagger events with clear gaps between them

### Mistake 6: No Analytics Tracking
**Problem**: Can't measure event success
**Solution**: Always enable analytics for every event

## 🎯 Event Strategy Templates

### Template 1: Holiday Event (2-3 weeks)
- **Theme**: Holiday-specific (Christmas, Halloween, etc.)
- **Cosmetics**: 4-5 high-quality themed items
- **Challenges**: Mix of gameplay and engagement (5-7 total)
- **Rewards**: Escalating progression + completion bonuses
- **Urgency**: Countdown timers, limited availability

### Template 2: Seasonal Event (3-4 weeks)
- **Theme**: Natural season transition (Spring, Summer, etc.)
- **Cosmetics**: 3-4 moderate quality themed items
- **Challenges**: Focus on gameplay mastery (4-6 total)
- **Rewards**: Steady progression, steady engagement
- **Strategy**: Sustainable engagement, lifestyle integration

### Template 3: Special Event (1-2 weeks)
- **Theme**: Game anniversary, update celebration
- **Cosmetics**: 2-3 exclusive high-value items
- **Challenges**: Simple, achievable goals (3-4 total)
- **Rewards**: Immediate gratification + exclusive items
- **Strategy**: FOMO-driven, maximum impact in short time

## ✅ Event Success Checklist

Before each event launch:
- [ ] Theme is clear and consistent
- [ ] 3-5 cosmetics selected and themed appropriately
- [ ] Timing fits target audience and global considerations
- [ ] Challenges are achievable and engaging
- [ ] Rewards provide appropriate progression
- [ ] Visual design matches theme
- [ ] Analytics tracking enabled
- [ ] Notification schedule prepared
- [ ] Social media content ready
- [ ] Customer support documentation updated
- [ ] Event tested in preview mode
- [ ] Backup plan prepared for technical issues

## 🎮 Integration with Other Systems

### Connect with Streak System:
- **Event participation** counts toward streak bonuses
- **Milestone celebrations** during events get enhanced rewards
- **Daily challenges** incorporate event elements

### Connect with Notification System:
- **Event start announcements** 24-48 hours before
- **Progress update notifications** during event
- **Final countdown warnings** 24-72 hours before end
- **Event completion celebrations** for participants

### Connect with Analytics System:
- **Track event participation** rates and demographics
- **Monitor challenge completion** patterns
- **Analyze cosmetic unlock** preferences
- **Measure retention impact** of events

---

**Remember**: Seasonal events are your most powerful engagement and monetization tool. Start with simple, well-executed events and gradually increase complexity based on player feedback and analytics data. Quality always trumps quantity!