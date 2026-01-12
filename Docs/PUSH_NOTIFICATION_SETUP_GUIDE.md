# Push Notification Setup Guide 📱

**Non-Coder, Step-by-Step Guide for Configuring Push Notifications**

## Overview
This guide will teach you how to set up push notifications to keep players engaged with daily reminders, milestone celebrations, and seasonal event announcements. We'll cover Firebase Cloud Messaging integration, notification preferences, and privacy compliance.

## 🎯 What You'll Learn
- How to set up Firebase Cloud Messaging integration
- How to enable push notifications in Godot export settings
- How to configure notification preferences in the Inspector
- How to set daily reminder times per timezone
- How to create custom notification messages
- How to test notifications in debug mode
- Privacy/GDPR compliance requirements

## 📋 Prerequisites
- Firebase Console account
- Godot Editor access
- Google Firebase SDK access
- Basic understanding of your game project structure

## 🔥 Firebase Cloud Messaging Setup

### Step 1: Create Firebase Project

1. **Go to Firebase Console**: https://console.firebase.google.com/
2. **Click "Add Project"**
3. **Enter project name**: `[YourGameName]-Notifications`
4. **Accept terms** and click "Continue"
5. **Choose Google Analytics** (recommended for notification analytics)
6. **Select analytics account** or create new one
7. **Click "Create Project"**
8. **Wait for project creation** (30-60 seconds)

### Step 2: Add Your App to Firebase

1. **In Firebase Console**, click the **Web icon** (</>) to add web app
2. **Enter app nickname**: `[YourGameName] Web`
3. **Check "Also set up Firebase Hosting"** (optional)
4. **Click "Register app"**
5. **Copy the Firebase config** - you'll need this later:
   ```javascript
   const firebaseConfig = {
     apiKey: "your-api-key",
     authDomain: "your-project.firebaseapp.com",
     projectId: "your-project-id",
     storageBucket: "your-project.appspot.com",
     messagingSenderId: "123456789",
     appId: "your-app-id"
   };
   ```

### Step 3: Enable Cloud Messaging

1. **In Firebase Console**, go to **"Build"** section
2. **Click "Cloud Messaging"**
3. **Click "Get started"**
4. **Web Configuration**:
   - **Check "Use a service worker"**
   - **Copy the provided script** - save as `firebase-messaging-sw.js`

### Step 4: Configure FCM for Android

1. **In Firebase Console**, click **Android icon**
2. **Enter Android package name**: `com.yourcompany.yourgame`
3. **Enter App nickname**: `[YourGameName] Android`
4. **Enter Debug signing certificate**: (for testing)
5. **Click "Register app"**
6. **Download `google-services.json`** - place in your Godot project root
7. **Click "Next"** through the remaining steps

### Step 5: Configure FCM for iOS

1. **In Firebase Console**, click **iOS icon**
2. **Enter iOS bundle ID**: `com.yourcompany.yourgame`
3. **Enter App nickname**: `[YourGameName] iOS`
4. **Click "Register app"**
5. **Download `GoogleService-Info.plist`** - place in your Godot project
6. **Follow remaining setup steps**

## 🔧 Godot Configuration

### Step 6: Configure Godot Export Settings

#### For Android:
1. **In Godot Editor**, go to **Project > Export**
2. **Select Android** preset
3. **In "Configuration" tab**:
   - **Check "Enable Firebase Cloud Messaging"**
   - **Paste your Firebase config** from Step 2
4. **In "Permissions" tab**, ensure these are enabled:
   - `android.permission.INTERNET`
   - `android.permission.VIBRATE`
   - `android.permission.RECEIVE_BOOT_COMPLETED` (optional, for auto-start)

#### For iOS:
1. **In Godot Editor**, go to **Project > Export**
2. **Select iOS** preset
3. **In "Configuration" tab**:
   - **Check "Enable Firebase Cloud Messaging"**
   - **Enable "Remote notifications"** in Capabilities
4. **Add Required Frameworks**:
   - `UserNotifications.framework`
   - `FirebaseMessaging.framework`

### Step 7: Configure Notification Manager

1. **Find PushNotificationManager** in your scene or as autoload
2. **In Inspector, configure**:
   - **Enable Push Notifications**: ✅ True
   - **Enable Firebase Integration**: ✅ True
   - **Firebase Project ID**: `[Your Firebase Project ID]`
   - **Check Interval**: 300 (5 minutes)

## 🎛️ Notification Preferences Configuration

### Step 8: Set Up Notification Types

The system supports 6 types of notifications:

#### 1. Daily Reminder Notifications
**Purpose**: Remind players to claim daily login rewards
**Best Time**: 9:00 AM (configurable per user timezone)
**Example**: "🎁 Daily Reward Awaits! Day 5 of your streak! Claim your reward now!"

**Configuration**:
1. **Enable Daily Reminders**: ✅ True
2. **Daily Reminder Time**: 9:00 (24-hour format)
3. **Active Days**: All days (0-6 = Sunday-Saturday)

#### 2. Milestone Celebrations
**Purpose**: Celebrate streak achievements (Days 7, 14, 21, 30)
**Timing**: Immediately when milestone reached
**Examples**:
- "🔥 7-Day Streak! Amazing! Keep it going!"
- "🏆 2-Week Champion! You're absolutely incredible!"

**Configuration**:
1. **Enable Milestone Notifications**: ✅ True
2. **Milestone Days**: 7, 14, 21, 30

#### 3. Streak Broken Alerts
**Purpose**: Encourage players to restart their streak
**Timing**: 24 hours after streak breaks
**Example**: "💔 Streak Ended. Don't worry! Start a new streak today!"

**Configuration**:
1. **Enable Streak Broken Alerts**: ✅ True
2. **Alert Timing**: 24 hours after break

#### 4. Seasonal Event Announcements
**Purpose**: Notify about event starts and endings
**Timing**: At event start + 24 hours before end
**Example**: "🎉 Winter Wonderland Begins! Exclusive cosmetics await!"

**Configuration**:
1. **Enable Event Notifications**: ✅ True
2. **Ending Alerts**: 24 hours before events end

#### 5. Limited-Time Cosmetics
**Purpose**: Create urgency for premium cosmetics
**Timing**: 48 hours before expiry
**Example**: "⏰ Exclusive Crown expires in 48 hours!"

**Configuration**:
1. **Enable Limited-Time Alerts**: ✅ True
2. **Expiry Warning Time**: 48 hours

#### 6. Lapsed Player Notifications
**Purpose**: Re-engage inactive players
**Timing**: 3 days after last session
**Example**: "We Miss You! Come back for exclusive rewards!"

**Configuration**:
1. **Enable Lapsed Player Alerts**: ✅ True
2. **Lapsed Player Threshold**: 3 days

## 🌍 Timezone Configuration

### Step 9: Set Up Timezone Handling

#### Understanding Timezone Logic
- **Daily reminders** send at player's local time
- **Milestone celebrations** send immediately (no timezone dependency)
- **Lapsed player alerts** based on last session time (universal)

#### Configuration Options:

1. **Timezone Source**:
   - **Device Timezone**: Uses player's device settings (recommended)
   - **Fixed UTC**: Sends all notifications at UTC times
   - **Server Time**: Uses your backend timezone

2. **DST Handling**:
   - **Automatic**: Adjusts for daylight saving time
   - **Fixed**: No DST adjustments (simpler)

#### Setting Up Player Timezone Preferences:

1. **In PushNotificationManager**, configure:
   - **Timezone Detection**: Automatic (device-based)
   - **DST Adjustment**: Enabled
   - **Default Reminder Time**: 9:00 AM

## 📝 Creating Custom Notification Messages

### Step 10: Customize Message Templates

#### Daily Reminder Messages:
1. **Navigate to PushNotificationManager** script
2. **Find "SendDailyReminderNotification"** method
3. **Modify message templates**:

```csharp
// Current streak versions
var title = "🎁 Daily Reward Awaits!";
var body = currentStreak > 0 
    ? $"Day {currentStreak} of your streak! Claim your {currentReward?.Title ?? "reward"} now!"
    : "Start your streak today! Claim your welcome bonus!";

// Custom alternatives
var title = "⚡ Your Daily Challenge Awaits!";
var body = $"Ready for Day {currentStreak + 1}? Let's keep this streak alive!";

// Motivational version
var title = "🌟 Daily Adventure Time!";
var body = $"The game misses you! Come claim your Day {currentStreak} treasure!";
```

#### Milestone Celebration Messages:
1. **Find "SendMilestoneNotification"** method
2. **Customize milestone responses**:

```csharp
// Creative milestone names
var milestoneNames = new Dictionary<int, string>
{
    [7] = "Week 1 Warrior",
    [14] = "Fortnight Fighter", 
    [21] = "Three-Week Champion",
    [30] = "Monthly Master"
};

// Emotional messages
var body = milestoneDay switch
{
    7 => "You've built an incredible habit! 7 days of dedication!",
    14 => "Two weeks of amazing commitment! You're unstoppable!",
    21 => "Three weeks of pure dedication! You're a legend!",
    30 => "30 DAYS! You're officially a retention master!",
    _ => $"Amazing {milestoneDay}-day achievement!"
};
```

### Step 11: Create Event-Specific Messages

#### Seasonal Event Templates:
1. **Find "SendSeasonalEventNotification"** method
2. **Create themed message variants**:

```csharp
// Winter Wonderland
if (eventName.Contains("Winter"))
{
    title = "❄️ Winter Wonderland Begins!";
    body = "Bundle up and unlock frosty cosmetics! Snow much fun ahead!";
}

// Summer Splash
if (eventName.Contains("Summer"))
{
    title = "🏖️ Summer Splash Event!";
    body = "Dive into summer fun! Beach-themed cosmetics await!";
}
```

## 🧪 Testing Notifications

### Step 12: Debug Mode Testing

#### Enable Debug Notifications:
1. **In PushNotificationManager**, set:
   - **Enable Local Notifications**: ✅ True (for desktop testing)
   - **Debug Mode**: ✅ True (shows notifications in console)
   - **Test Notification Interval**: 30 seconds (instead of real timing)

#### Manual Testing Procedures:

1. **Test Daily Reminders**:
   - Set reminder time to current time + 1 minute
   - Wait for notification to fire
   - Verify message content and deep linking

2. **Test Milestone Notifications**:
   - Manually trigger streak increment in editor
   - Verify celebration notifications fire at milestones
   - Check that milestone sounds play

3. **Test Event Notifications**:
   - Create test seasonal event
   - Trigger event start/end notifications
   - Verify countdown messages work

#### Firebase Console Testing:

1. **Send Test Messages**:
   - Go to Firebase Console > Cloud Messaging
   - Click "Send test message"
   - Enter device token
   - Send custom test message

2. **Monitor Delivery**:
   - Check "Message delivery rate"
   - Verify "Click-through rates"
   - Monitor "Opt-out rates"

## 🔒 Privacy and GDPR Compliance

### Step 13: Implement Consent Management

#### Required Consent Elements:
1. **Clear Opt-In Process**:
   - Explain notification benefits clearly
   - Show what types of notifications they'll receive
   - Allow granular control (daily, milestone, events separately)

2. **Easy Opt-Out**:
   - Provide clear unsubscribe options
   - Allow disabling specific notification types
   - Include "Stop all notifications" option

3. **Data Protection**:
   - Store consent timestamp
   - Track consent version (for policy changes)
   - Log opt-in/opt-out events

#### Implementation in Settings:
1. **Create Notification Settings Screen**:
   - **Master Toggle**: Enable/Disable all notifications
   - **Daily Reminders**: ✅ Individual control
   - **Milestone Celebrations**: ✅ Individual control
   - **Event Notifications**: ✅ Individual control
   - **Lapsed Player Alerts**: ✅ Individual control

2. **Privacy Compliance**:
   - Link to privacy policy
   - Explain data usage
   - Provide contact information for data requests

### Step 14: Configure Quiet Hours

#### Setting Up Quiet Hours:
1. **In NotificationPreferences**, configure:
   - **Quiet Hours Start**: 22:00 (10 PM)
   - **Quiet Hours End**: 08:00 (8 AM)
   - **Respect Quiet Hours**: ✅ True

2. **Override Options**:
   - **Emergency Notifications**: Allow during quiet hours (milestone celebrations)
   - **Player Override**: Allow players to temporarily disable quiet hours

## 📊 Analytics and Monitoring

### Step 15: Set Up Notification Analytics

#### Key Metrics to Track:
1. **Delivery Rates**:
   - `notification_sent` event
   - `notification_delivered` event
   - `notification_failed` event

2. **Engagement Rates**:
   - `notification_clicked` event
   - `notification_engaged` event
   - `app_opened_from_notification` event

3. **Opt-Out Rates**:
   - `notification_opt_out` event
   - `notification_unsubscribe` event

#### Firebase Analytics Integration:
1. **Enable Analytics Events**:
   - Track each notification type
   - Record delivery success/failure
   - Monitor user engagement

2. **Create Custom Events**:
   ```csharp
   // Track notification sent
   AnalyticsManager.Instance.LogEvent("notification_sent", new Dictionary<string, object>
   {
       ["notification_type"] = notification.Type.ToString(),
       ["scheduled_time"] = notification.ScheduledTime.ToString("O")
   });
   ```

## 🚨 Troubleshooting Common Issues

### Problem: Notifications Not Sending
**Symptoms**: Players don't receive push notifications
**Solutions**:
1. **Check Firebase Configuration**:
   - Verify project ID and API keys
   - Ensure FCM is enabled in Firebase Console
   - Check Firebase SDK version compatibility

2. **Verify Export Settings**:
   - Ensure `google-services.json` is in project root
   - Check Android/iOS permissions
   - Verify push notification capabilities enabled

3. **Test with Firebase Console**:
   - Send test message manually
   - Check if device receives notifications
   - Verify Firebase project configuration

### Problem: Notifications Wrong Time
**Symptoms**: Daily reminders send at incorrect times
**Solutions**:
1. **Check Timezone Settings**:
   - Verify timezone detection logic
   - Test with devices in different timezones
   - Ensure DST handling is correct

2. **Validate Timing Logic**:
   - Check `CalculateSecondsToMidnight()` function
   - Verify daily reset timer configuration
   - Test edge cases (DST transitions, etc.)

### Problem: Too Many/Few Notifications
**Symptoms**: Players complain about spam or lack of engagement
**Solutions**:
1. **Adjust Frequency Limits**:
   - Check `MaxNotificationsPerDay` setting
   - Verify quiet hours configuration
   - Review milestone notification timing

2. **Optimize Message Timing**:
   - Test optimal send times for your audience
   - A/B test different reminder times
   - Monitor engagement rates per time slot

### Problem: Notifications Not Clickable
**Symptoms**: Notifications appear but don't open app
**Solutions**:
1. **Check Deep Link Configuration**:
   - Verify `deep_link` parameter format
   - Test with Firebase Console first
   - Check app handles deep links correctly

2. **Verify App State Handling**:
   - Ensure app can handle notification clicks when closed
   - Test foreground/background notification handling
   - Check iOS/Android deep link differences

## ✅ Pre-Launch Checklist

Before going live with notifications:

- [ ] Firebase project fully configured
- [ ] `google-services.json`/`GoogleService-Info.plist` properly placed
- [ ] All notification types tested and working
- [ ] Privacy policy updated with notification data usage
- [ ] Consent flow implemented and tested
- [ ] Quiet hours configuration verified
- [ ] Analytics events firing correctly
- [ ] Deep linking to correct app screens
- [ ] Opt-out mechanisms working
- [ ] Notification timing optimized for target audience
- [ ] Fallback notifications working (local notifications)
- [ ] Test notifications sent successfully from Firebase Console
- [ ] Cross-platform testing completed (iOS + Android)

## 🎯 Optimization Guidelines

### A/B Testing Notifications:
1. **Test Different Times**:
   - Morning (8-10 AM) vs. Evening (6-8 PM)
   - Weekday vs. Weekend preferences
   - Different timezones separately

2. **Test Message Variations**:
   - Casual vs. Formal tone
   - Short vs. Long messages
   - Emoji usage impact

3. **Test Frequency**:
   - Daily vs. Every other day
   - Multiple milestone vs. Single milestone alerts
   - Event announcements vs. Only event end warnings

### Personalization Strategies:
1. **Based on Player Behavior**:
   - Heavy gamers: Fewer notifications
   - Casual gamers: More frequent reminders
   - Weekend warriors: Weekend-focused timing

2. **Based on Game Progress**:
   - New players: More frequent milestone celebrations
   - Veteran players: Focus on rare events
   - Completionists: Detailed progress notifications

## 🔗 Integration with Other Systems

### Connect with Analytics:
- Track notification effectiveness
- Monitor retention impact
- A/B test different strategies

### Connect with Monetization:
- Drive cosmetic purchases through FOMO
- Promote premium features
- Seasonal event revenue tracking

### Connect with Game Balance:
- Adjust difficulty based on notification response
- Personalize content based on engagement
- Optimize retention mechanics

---

**Remember**: Push notifications are a powerful engagement tool but should be used thoughtfully. Respect player preferences, provide clear value, and always allow easy opt-out. Test thoroughly across all platforms before launching!