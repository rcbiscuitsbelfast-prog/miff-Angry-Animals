# Login Bonus Screen Customization Guide 🎁

**Non-Coder Guide for Customizing the Daily Login Bonus Screen**

## Overview
This guide will teach you how to customize the appearance, behavior, and content of the daily login bonus screen without writing any code. You'll learn to adjust visuals, animations, layouts, and messaging to match your game's style.

## 🎯 What You'll Learn
- How to change login bonus screen appearance
- How to customize countdown timers and progress displays
- How to add/remove celebration effects
- How to adjust streak calendar layouts
- How to customize reward preview text
- How to set background and theme colors

## 📋 Prerequisites
- Godot Editor access
- Basic familiarity with UI design
- Access to your game's visual assets

## 🎨 Customizing the Visual Design

### Step 1: Locate the Login Bonus Screen Scene
1. **Open Godot Editor**
2. **Navigate to Scenes/UI/** in the Project panel
3. **Find `LoginBonusScreen.tscn`**
4. **Double-click to open** the scene

### Step 2: Understanding the Scene Structure
The login bonus screen consists of these key components:
- **Background**: Dark overlay behind the content
- **MainContainer**: Main content area with all UI elements
- **StreakCounterLabel**: Shows current streak day
- **RewardPreview**: Displays today's reward
- **ProgressBar**: Visual progress to next milestone
- **StreakCalendar**: 30-day calendar grid
- **CelebrationEffects**: Confetti and particle effects

### Step 3: Customizing Background and Layout

#### Changing the Background
1. **Select "Background" node** in the Scene panel
2. **In Inspector, find "Color" property**
3. **Modify the color values**:
   - **Alpha**: Controls transparency (0.0 = transparent, 1.0 = opaque)
   - **RGB**: Set background color
   - **Example**: `Color(0.2, 0.1, 0.3, 0.9)` = Dark purple background

#### Adding a Custom Background Image
1. **Create new TextureRect** node
2. **Position it behind MainContainer**
3. **Set Stretch Mode** to "Keep Aspect Centered" or "Fit Width"
4. **Assign your background image** in the Texture property

#### Adjusting Main Container Layout
1. **Select "MainContainer" node**
2. **Modify Size and Position**:
   - **Offset Left/Right**: Control horizontal position
   - **Offset Top/Bottom**: Control vertical position
   - **Size**: Adjust width and height of content area

## 🏷️ Customizing Text and Labels

### Step 4: Personalizing Text Content

#### Changing Main Title
1. **Select "TitleLabel" node**
2. **In Inspector, modify "Text" property**
3. **Examples**:
   - "🎁 DAILY LOGIN BONUS" (Default)
   - "✨ YOUR DAILY REWARD AWAITS"
   - "🌟 DAILY STASH OF GOODIES"

#### Customizing Streak Counter
1. **Select "StreakCounterLabel" node**
2. **Modify text formatting**:
   - **Default**: "🔥 Day X of 30!"
   - **Custom**: "⚡ X-Day Streak Champion!"
   - **Alternative**: "🎯 Day X Complete!"

#### Personalizing Reward Messages
1. **Select "RewardTitle" node**
2. **Change text format**:
   - **Default**: "Day X Reward: [Reward Name]"
   - **Creative**: "🎁 Your Day X Treasure: [Name]"
   - **Exciting**: "🏆 Day X Reward Unlocked: [Name]"

#### Customizing Reward Descriptions
1. **Select "RewardDescription" node**
2. **Modify motivation text**:
   - **Day 1**: "Welcome to the adventure! Here's your starter pack!"
   - **Day 7**: "Amazing! A full week of dedication!"
   - **Day 30**: "LEGENDARY! 30 days of pure mastery!"

### Step 5: Adjusting Visual Styling

#### Changing Colors
1. **Select any text node**
2. **In Inspector, find "Modulate" property**
3. **Set color schemes**:

**Default Color Scheme**:
- Streak Counter: Orange (1.0, 0.8, 0.2)
- Reward Title: Yellow (1.0, 1.0, 0.0)
- Description: White (1.0, 1.0, 1.0)

**Alternative Color Schemes**:

**Dark Mode Theme**:
- Background: Dark blue (0.1, 0.1, 0.3)
- Text: Light blue (0.8, 0.9, 1.0)
- Accents: Cyan (0.0, 1.0, 1.0)

**Warm Theme**:
- Background: Warm orange (0.3, 0.2, 0.1)
- Text: Cream (1.0, 0.95, 0.8)
- Accents: Gold (1.0, 0.8, 0.2)

**Cool Theme**:
- Background: Cool purple (0.2, 0.1, 0.3)
- Text: Ice blue (0.8, 0.9, 1.0)
- Accents: Silver (0.8, 0.8, 0.9)

#### Adjusting Fonts and Sizes
1. **Select text node**
2. **In Inspector**:
   - **Font Size**: Adjust text size
   - **Font**: Change font family if available
   - **Alignment**: Left, Center, Right alignment

## ⏰ Customizing Countdown Timer Display

### Step 6: Timer and Progress Customization

#### Modifying Progress Bar
1. **Select "ProgressBar" node**
2. **Customize appearance**:
   - **Min Value**: 0 (usually stays 0)
   - **Max Value**: 100 (or adjust based on preference)
   - **Value**: Current progress (managed by code)
   - **Colors**: Customize progress bar colors

#### Customizing Progress Display Text
1. **Find "NextRewardPreview" node**
2. **Modify milestone messaging**:
   - **Default**: "🎯 Next milestone (X): [Reward Name]"
   - **Custom**: "🚀 Next Goal (Day X): [Reward Name]"
   - **Motivational**: "💪 Almost there! Day X: [Reward Name]"

## 📅 Customizing the Streak Calendar

### Step 7: Calendar Layout and Appearance

#### Adjusting Calendar Grid
1. **Select "StreakCalendar" node**
2. **Modify grid properties**:
   - **Columns**: Set to 15 (shows 2 rows of 15 days)
   - **H Separation**: Horizontal spacing between day cells
   - **V Separation**: Vertical spacing between day cells

#### Customizing Day Cell Appearance
1. **The calendar is generated by code**, but you can modify:
2. **Color schemes for completed/incomplete days**:
   - **Completed**: Bright green with high alpha
   - **Incomplete**: Dark gray with low alpha
   - **Milestones**: Special icons and colors

#### Alternative Calendar Layouts

**Compact View** (Fewer Columns):
- Set **Columns** to 10 for tighter layout
- Days wrap to 3 rows instead of 2

**Detailed View** (More Columns):
- Set **Columns** to 30 for single row
- Shows all days in one line

**Calendar View** (Grid Layout):
- Set **Columns** to 7 for week-based layout
- Creates proper calendar week structure

## 🎊 Customizing Celebration Effects

### Step 8: Adding and Modifying Celebrations

#### Confetti Effects
1. **Select "CelebrationEffects" node**
2. **Customize particle effects**:
   - **Confetti Colors**: Configure multiple colors for confetti
   - **Spawn Rate**: How often particles appear
   - **Duration**: How long effects play

#### Sound Customization
1. **Modify celebration sounds** in the LoginBonusScreenUI.cs script
2. **Or integrate with AudioManager** for better audio control
3. **Recommended celebration sounds**:
   - **Milestone**: Triumphant orchestral hit
   - **Daily**: Light, satisfying "ding"
   - **Week 1**: Upbeat celebration
   - **Month**: Epic, memorable fanfare

#### Adding Custom Animations
1. **Create new AnimationPlayer** node
2. **Add custom animations**:
   - **Scale pulse**: Make reward item pulse
   - **Bounce**: Make text bounce when appearing
   - **Shine**: Add sparkle effects to rewards

### Step 9: Seasonal and Event Theming

#### Dynamic Backgrounds
1. **Create multiple background variants**:
   - **Default**: Standard game background
   - **Winter**: Snowy, cold theme
   - **Summer**: Bright, warm theme
   - **Holidays**: Festive colors and elements

#### Event-Specific Overlays
1. **Create overlay layers** that change based on active events
2. **Examples**:
   - **Spring Event**: Flower petals floating
   - **Summer Event**: Sun rays or beach elements
   - **Fall Event**: Autumn leaves falling
   - **Winter Event**: Snow effects

## 📱 Responsive Design Adjustments

### Step 10: Mobile Optimization

#### Scaling for Different Screen Sizes
1. **Select MainContainer**
2. **Modify anchor presets**:
   - **Center**: Keeps content centered
   - **Expand**: Stretches to fill screen
   - **Custom**: Manual positioning

#### Text Scaling
1. **Adjust font sizes** for readability on mobile
2. **Test on various screen sizes**:
   - **Phone**: Smaller text, compact layout
   - **Tablet**: Medium text, balanced layout
   - **Desktop**: Larger text, spacious layout

## 🎨 Brand Customization Examples

### Fantasy Game Theme
- **Background**: Magical purple with sparkles
- **Title**: "✨ Your Daily Blessing Awaits"
- **Streak Counter**: "⚡ Day X of Your Quest"
- **Rewards**: "🏆 Daily Treasure: [Name]"
- **Colors**: Purple, gold, silver accents

### Sci-Fi Game Theme
- **Background**: Futuristic blue with grid lines
- **Title**: "🚀 Daily Supply Drop"
- **Streak Counter**: "⚡ Day X Deployment"
- **Rewards**: "💎 Mission Reward: [Name]"
- **Colors**: Blue, cyan, white accents

### Casual Game Theme
- **Background**: Bright, cheerful colors
- **Title**: "🎈 Your Daily Fun Pack"
- **Streak Counter**: "🌟 Day X Fun Complete"
- **Rewards**: "🎁 Today's Surprise: [Name]"
- **Colors**: Bright pastels, rainbow accents

## 🔧 Advanced Customization

### Custom Icons and Graphics
1. **Replace text-based icons** with custom graphics
2. **Create sprite sheets** for different states
3. **Add custom particle effects** for milestones

### Dynamic Content Loading
1. **Integrate with your localization system**
2. **Load different content** based on player preferences
3. **Customize based on player progress** or achievements

### Interactive Elements
1. **Add hover effects** to buttons
2. **Create click animations** for engagement
3. **Add sound effects** to UI interactions

## 📊 Testing Your Customizations

### Step 11: Testing Guidelines

#### Visual Testing
1. **Test on multiple screen sizes**
2. **Verify readability** at different zoom levels
3. **Check color contrast** for accessibility
4. **Ensure mobile compatibility**

#### Functional Testing
1. **Test streak progression** through all 30 days
2. **Verify milestone celebrations** trigger correctly
3. **Check celebration effects** work properly
4. **Test dismiss functionality**

#### Performance Testing
1. **Monitor frame rate** with effects enabled
2. **Test on lower-end devices**
3. **Verify memory usage** isn't excessive

## 🚨 Common Customization Issues

### Problem: Text Overlapping
**Solution**: Adjust container sizes or font sizes

### Problem: Background Image Distorted
**Solution**: Change Stretch Mode to "Keep Aspect"

### Problem: Celebration Effects Too Slow/Fast
**Solution**: Adjust timer durations in the script

### Problem: Mobile Layout Broken
**Solution**: Use responsive anchors and test on actual devices

### Problem: Colors Don't Match Game Theme
**Solution**: Create a consistent color palette and apply across all elements

## ✅ Customization Checklist

Before finalizing your login bonus screen:
- [ ] Background matches your game's art style
- [ ] Text is readable and motivational
- [ ] Colors create emotional engagement
- [ ] Progress displays are clear and encouraging
- [ ] Calendar layout is intuitive
- [ ] Celebration effects enhance the experience
- [ ] Mobile layout works properly
- [ ] All milestone days are properly highlighted
- [ ] Sounds and effects are appropriately themed
- [ ] Performance is acceptable on target devices

## 🎯 Pro Tips

### Engagement Optimization
1. **Use action words** in descriptions ("Unlock", "Discover", "Earn")
2. **Create anticipation** with next reward previews
3. **Celebrate milestones** with special effects and sounds
4. **Use progressive language** ("You're getting better!")

### Visual Hierarchy
1. **Make reward title** the most prominent text
2. **Use size and color** to guide player attention
3. **Ensure countdown** is always visible
4. **Highlight special days** (milestones) differently

### Emotional Connection
1. **Use encouraging language** for streak building
2. **Celebrate player dedication** rather than just rewards
3. **Create sense of progress** and achievement
4. **Make failures feel like opportunities** to start fresh

---

**Remember**: The login bonus screen is often the first thing players see daily. Make it engaging, encouraging, and aligned with your game's personality. Test with real players and iterate based on feedback!