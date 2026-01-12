# AdMob Interstitial Ad Integration Documentation

## Overview
Integrated interstitial (full-screen) AdMob ads with smart placement strategy for optimal revenue without disrupting gameplay experience.

## Ad Unit Configuration

**Interstitial Ad Unit ID:** `ca-app-pub-6675121744131727/8410569879`
**Banner Ad Unit ID:** `ca-app-pub-6675121744131727/8033303534`

## Implementation Details

### 1. AdsManager.cs Enhancements
- **Interstitial Cooldown System:** 45-second minimum between ads to prevent spam
- **Background Preloading:** Automatically loads next ad while user plays
- **Smart Ad Readiness Check:** Only shows ads when both loaded and cooldown expired
- **Graceful Fallback:** Game continues normally if ads fail to load
- **Testing Support:** Reset cooldown method for development

#### Key Methods Added:
- `LoadInterstitialAd()` - Manual ad preloading
- `IsInterstitialReady()` - Check ad availability + cooldown
- `ResetInterstitialCooldown()` - Testing utility
- `GetRemainingCooldownSeconds()` - Debug info

### 2. Integration Points

#### A. LevelCompleted.cs
**Trigger:** After level completion screen shows
**Conditions:**
- Level ≥ 2 (avoids early game frustration)
- Ads enabled in monetization settings
- Cooldown expired
- Ad loaded and ready

**Flow:**
1. Level completes → Results screen shows
2. 2-second delay for UI polish
3. Check all conditions
4. Show interstitial or preload for next time
5. Return to normal flow after ad

#### B. RoomSelection.cs  
**Trigger:** When entering level selection
**Purpose:** Preload interstitial in background
**Benefits:** Ad ready by time player completes next level

#### C. Ui.cs
**Trigger:** After repeated game over failures
**Conditions:**
- 3 consecutive failures
- Ads enabled
- Cooldown expired

**Purpose:** Revenue opportunity when player might quit

## User Experience Strategy

### Revenue-Optimized Placement
1. **Level Completion** (Primary)
   - Player feels successful
   - Natural break in gameplay
   - High engagement moment = better CPM

2. **Repeated Failures** (Secondary)
   - Player might quit anyway
   - Ad as "breather" before retry
   - 3-failure threshold prevents frustration

3. **Room Selection** (Background)
   - Preloading only, no interruption
   - Ensures smooth ad flow

### Anti-Frustration Measures
- **45-second cooldown** prevents ad spam
- **Level 2+ requirement** avoids early game disruption  
- **Multiple failure threshold** ensures player engagement
- **Graceful degradation** if ads unavailable
- **No ads during active gameplay** (aiming, flight)

## Technical Architecture

### Cooldown System
```csharp
// Prevents rapid-fire ads
private DateTime _lastInterstitialShownTime;
private Timer? _interstitialCooldownTimer;

// Checks both ad availability AND cooldown
public bool IsInterstitialReady()
{
    return _interstitialReady && CanShowInterstitial();
}
```

### Preloading Strategy
```csharp
// Load next ad immediately after showing one
if (EnableInterstitialPreloading)
{
    _ = LoadInterstitialAsync();
}
```

### Background Preloading
```csharp
// In RoomSelection - load ads when player not in active gameplay
private async void PreloadInterstitialAdsAsync()
{
    await Task.Delay(1000);
    await AdsManager.Instance.LoadInterstitialAd();
}
```

## Mobile Build Configuration

### Android (AndroidManifest.xml)
```xml
<uses-permission android:name="android.permission.INTERNET" />
<uses-permission android:name="android.permission.ACCESS_NETWORK_STATE" />
```

### Gradle Dependencies
```gradle
implementation 'com.google.android.gms:play-services-ads:22.6.0'
```

### iOS (Info.plist)
```xml
<key>GADApplicationIdentifier</key>
<string>ca-app-pub-6675121744131727~3341711713</string>
```

## Testing Checklist

### Development Testing
- [ ] Test interstitial shows after level 2+ completion
- [ ] Verify 45-second cooldown prevents rapid ads
- [ ] Check ads don't show on levels 1
- [ ] Test preloading works from room selection
- [ ] Verify 3-failure threshold for game over ads
- [ ] Test graceful fallback when ads unavailable
- [ ] Check ad closes properly and returns to game

### Production Testing
- [ ] Test on physical Android device
- [ ] Test on physical iOS device  
- [ ] Verify AdMob console shows impressions
- [ ] Check earnings tracking
- [ ] Test offline/poor connection behavior
- [ ] Verify no crashes during ad flow

### Edge Cases
- [ ] What happens if user spams "Next Level" button
- [ ] Behavior during slow internet connection
- [ ] App backgrounding during ad display
- [ ] Rapid scene switching (memory cleanup)

## Revenue Impact

### Banner Ads (Persistent)
- **Type:** Bottom banner, always visible
- **Revenue:** Steady base impressions
- **User Impact:** Minimal, non-intrusive

### Interstitial Ads (Strategic)
- **Type:** Full-screen between game sessions
- **Revenue:** High CPM due to full attention
- **User Impact:** Acceptable at natural breaks

### Combined Strategy
- **Banner:** Continuous passive revenue
- **Interstitial:** High-value engagement moments
- **Result:** Optimal monetization without UX degradation

## Configuration Options

### AdsManager Settings
```csharp
[Export] public float InterstitialCooldownSeconds = 45.0f;
[Export] public bool EnableInterstitialPreloading = true;
[Export] public string InterstitialAdUnitId = "ca-app-pub-6675121744131727/8410569879";
```

### LevelCompleted Settings
```csharp
[Export] public bool ShowInterstitialOnLevelComplete = true;
[Export] public int MinimumLevelForInterstitial = 2;
```

### Ui.cs Settings
```csharp  
[Export] public bool ShowInterstitialOnGameOver = true;
[Export] public int FailedAttemptsBeforeInterstitial = 3;
```

## Monitoring & Analytics

### Key Metrics to Track
- **Interstitial Impressions:** Daily count in AdMob console
- **Fill Rate:** % of requested ads that actually show
- **CPM:** Revenue per 1000 impressions  
- **User Retention:** Does ad placement affect player churn
- **Session Length:** Impact on average playtime

### Success Indicators
- ✅ Smooth ad display without crashes
- ✅ No player complaints about ad frequency
- ✅ Revenue increase from interstitial placement
- ✅ Retention rates maintained or improved
- ✅ AdMob policy compliance

## Future Enhancements

### Potential Improvements
1. **Dynamic Cooldown:** Adjust based on player behavior
2. **A/B Testing:** Different placement strategies
3. **Rewarded Interstitials:** Optional ad for bonuses
4. **Analytics Integration:** Detailed performance tracking
5. **GDPR Compliance:** Consent management system

### Advanced Features
- Machine learning for optimal ad timing
- Player segment-based ad frequency
- Cross-promotion with other games
- Seasonal ad campaign integration

## Troubleshooting

### Common Issues
1. **Ad not showing:** Check unit ID, network, cooldown
2. **Crashes during ad:** Verify plugin integration
3. **No revenue:** Confirm AdMob account setup
4. **Player complaints:** Adjust frequency/cooldown

### Debug Tools
- Console logging for ad events
- Reset cooldown for testing
- Ad readiness checking
- Network connectivity validation

This integration provides a professional-grade monetization system that balances revenue generation with user experience, following industry best practices for mobile game advertising.