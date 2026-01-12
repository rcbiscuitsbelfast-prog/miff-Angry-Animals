# Firebase Setup Guide for Angry Animals 📱

**Complete step-by-step guide to set up Firebase Analytics and Crashlytics in 10 minutes**

---

## 🎯 What You'll Accomplish

By the end of this guide, you'll have:
- ✅ Firebase project created and configured
- ✅ Analytics tracking all game events
- ✅ Automatic crash reporting enabled
- ✅ Real-time data appearing in Firebase console
- ✅ Ready-to-use configuration for Angry Animals

---

## 📋 Prerequisites

- Google account (Gmail, etc.)
- Access to Angry Animals project files
- 10 minutes of your time

---

## 🚀 Step 1: Create Firebase Project (2 minutes)

### 1.1 Go to Firebase Console
1. Open your web browser and go to: **https://console.firebase.google.com**
2. Sign in with your Google account
3. Click **"Create a project"**

### 1.2 Configure Project
1. **Project name**: Enter `angry-animals-analytics` (or your preferred name)
2. **Google Analytics**: Enable this ✅ (recommended for better insights)
3. **Analytics location**: Choose your preferred region
4. **Data sharing settings**: Accept the default settings
5. Click **"Create project"**

### 1.3 Wait for Setup
- Project creation takes 30-60 seconds
- You'll see a progress screen

---

## 🔧 Step 2: Configure for Your Platform (3 minutes)

### 2.1 For Android Development

1. **Add Android App**:
   - Click the Android icon in Firebase console
   - **Android package name**: `com.yourcompany.angryanimals`
   - **App nickname**: `Angry Animals Android`
   - **Debug signing certificate**: Leave blank for now
   - Click **"Register app"**

2. **Download Configuration**:
   - Click **"Download google-services.json"**
   - Save this file in your Angry Animals project folder at: `Assets/Configs/google-services.json`

### 2.2 For iOS Development

1. **Add iOS App**:
   - Click the iOS icon in Firebase console
   - **iOS bundle ID**: `com.yourcompany.angryanimals`
   - **App nickname**: `Angry Animals iOS`
   - Click **"Register app"**

2. **Download Configuration**:
   - Click **"Download GoogleService-Info.plist"**
   - Save this file in your Angry Animals project folder at: `Assets/Configs/GoogleService-Info.plist`

### 2.3 For Desktop/Editor Testing

No additional configuration needed - the game will use mock analytics mode.

---

## 📊 Step 3: Enable Analytics and Crashlytics (2 minutes)

### 3.1 Enable Google Analytics
1. In Firebase console, go to **"Analytics"** → **"Dashboard"**
2. Click **"Enable Google Analytics"**
3. Analytics is now active! 🎉

### 3.2 Enable Crashlytics
1. Go to **"Crashlytics"** in the left sidebar
2. Click **"Get started"**
3. Crashlytics is now enabled! 📈

### 3.3 Verify Setup
- You should see both Analytics and Crashlytics in your left sidebar
- No crashes or events will appear yet (that's normal)

---

## 🔑 Step 4: Get Your Configuration Keys (1 minute)

### 4.1 Find Your Project ID
1. Go to **Project Settings** (gear icon)
2. Copy your **Project ID** (looks like: `angry-animals-analytics-123`)

### 4.2 Find Your Web API Key
1. In **Project Settings**, go to **"General"** tab
2. Scroll down to **"Your apps"**
3. Click **"</> Web"** to create a web app
4. **App nickname**: `Angry Animals Web`
5. Copy the **Web API Key** (starts with `AIza...`)

### 4.3 Find Your App ID
1. Still in **Project Settings**
2. Copy the **App ID** (starts with `1:123456789:web:...`)

---

## 💾 Step 5: Configure Angry Animals (2 minutes)

### 5.1 Update Firebase Configuration
1. Open your Angry Animals project
2. Navigate to: `Classes/FirebaseManager.cs`
3. Find the `LoadConfiguration()` method (around line 125)
4. Replace the placeholder values with your actual Firebase info:

```csharp
_config = new FirebaseConfig
{
    // Replace with your actual Firebase project details
    ProjectId = "YOUR_ACTUAL_PROJECT_ID",           // From Step 4.1
    ApiKey = "YOUR_ACTUAL_API_KEY",                 // From Step 4.2  
    AppId = "YOUR_ACTUAL_APP_ID",                   // From Step 4.3
    AnalyticsEnabled = true,                        // Keep true
    CrashlyticsEnabled = true,                      // Keep true
    // ... rest of config stays the same
};
```

### 5.2 Save Configuration
1. Save the `FirebaseManager.cs` file
2. Your Firebase integration is now complete! 🎉

---

## 🧪 Step 6: Test Your Setup (1 minute)

### 6.1 Start the Game
1. Run Angry Animals in the editor or build
2. Check the console output for Firebase initialization messages

### 6.2 Look for These Messages
- ✅ `Firebase Manager initialized - Platform: Editor, Available: false` (editor mode - normal)
- ✅ `Firebase Manager initialized - Platform: Android, Available: true` (Android build - should connect)

### 6.3 Play the Game
1. Start a level
2. Complete or fail a level
3. Try crashing the game intentionally (for testing)

### 6.4 Check Firebase Console
1. Go back to Firebase console
2. Click **"Analytics"** → **"Events"**
3. You should see events appearing within 1-2 minutes:
   - `level_started`
   - `level_completed` or `level_failed`
   - `session_start`

---

## 📈 What Events Will You See?

Your game will automatically track these events:

### 🎮 Gameplay Events
- `level_started` - When a player starts any level
- `level_completed` - When a level is successfully completed
- `level_failed` - When a player fails a level
- `perfect_score_achieved` - When someone gets a perfect score

### 💰 Monetization Events
- `cosmetic_purchased` - When players buy cosmetics
- `remove_ads_purchased` - When players buy ad removal
- `rewarded_ad_watched` - When players watch reward ads

### 📱 Engagement Events
- `daily_login_streak_reached` - When players have login streaks
- `achievement_unlocked` - When achievements are unlocked
- `seasonal_event_started` - When seasonal events begin

### ⚠️ Quality Events
- `crash_detected` - When the game crashes
- `performance_frame_drop` - When performance issues occur
- `memory_warning` - When memory usage is high

---

## 🔧 Troubleshooting

### "Firebase not connecting" Issues

**Problem**: Game shows "Firebase: ❌ Unavailable"

**Solutions**:
1. **Check Configuration**: Verify your Project ID, API Key, and App ID are correct in `FirebaseManager.cs`
2. **Platform Support**: Firebase only works on Android/iOS builds, not in editor
3. **Network**: Ensure your device has internet connection
4. **Project Setup**: Make sure you enabled both Analytics and Crashlytics

### "No events appearing in console" Issues

**Problem**: Events aren't showing up in Firebase console

**Solutions**:
1. **Wait Time**: Events can take 1-5 minutes to appear
2. **Event Collection**: Make sure you're actually triggering events (play the game!)
3. **Debug Mode**: Check game console for Firebase debug messages
4. **Test Events**: Use the in-game telemetry dashboard to see events being logged

### "Build fails with Firebase" Issues

**Problem**: Compilation errors related to Firebase

**Solutions**:
1. **Missing Files**: Ensure `google-services.json` (Android) or `GoogleService-Info.plist` (iOS) are in correct folders
2. **Project Setup**: Re-download configuration files from Firebase console
3. **Dependencies**: Firebase plugins are included in the Angry Animals codebase

---

## 🎯 Success Criteria

You've successfully set up Firebase when:

- ✅ You can see Firebase initialization messages in the console
- ✅ Game runs without Firebase-related errors
- ✅ Events appear in Firebase console within 5 minutes of gameplay
- ✅ Telemetry dashboard (debug builds only) shows Firebase as connected
- ✅ Crash reports appear in Crashlytics section (if you test crashes)

---

## 📚 Next Steps

After completing this setup:

1. **Read Analytics Configuration Guide**: Learn how to customize what events are tracked
2. **Check Telemetry Dashboard**: Use the debug panel to monitor real-time metrics
3. **Review Firebase Console**: Explore the analytics reports and insights
4. **Set up Crash Alerts**: Configure notifications for critical crashes

---

## ⏱️ Total Setup Time: ~10 Minutes

| Step | Time | What You Do |
|------|------|-------------|
| Step 1 | 2 min | Create Firebase project |
| Step 2 | 3 min | Configure for your platform |
| Step 3 | 2 min | Enable Analytics & Crashlytics |
| Step 4 | 1 min | Get configuration keys |
| Step 5 | 2 min | Update Angry Animals config |
| **Total** | **10 min** | **Complete Firebase setup!** |

---

## 🆘 Need Help?

If you run into issues:

1. **Double-check configuration values** - Typos are the #1 cause of problems
2. **Wait for propagation** - Firebase changes can take a few minutes to take effect
3. **Check console logs** - Firebase debug messages provide useful troubleshooting info
4. **Test on actual devices** - Editor mode uses mock analytics by design

**You're all set!** 🎉 Your game now has enterprise-grade analytics and crash reporting.