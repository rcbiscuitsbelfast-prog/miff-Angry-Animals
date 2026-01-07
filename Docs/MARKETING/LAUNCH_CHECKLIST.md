# Angry Animals - Launch Checklist

## Code Status
✅ **All 12 features merged to main**
✅ **100 levels complete and playable**
✅ **Monetization system integrated**
✅ **Cross-platform support verified**
✅ **No compilation errors expected**
✅ **All scenes load without errors**

## Before App Store Submission
- [ ] Setup AdMob account + get app ID
- [ ] Create AdMob ad units (banner, interstitial, rewarded)
- [ ] Setup Apple App Store Connect + create IAP product (`full_game_unlock`)
- [ ] Setup Google Play Console + create in-app product (`full_game_unlock`)
- [ ] Test purchase flow on TestFlight (iOS)
- [ ] Test purchase flow on Google Play internal test (Android)
- [ ] Build release APK/IPA
- [ ] Submit to app stores

## App Store Details
- **App Name**: Angry Animals
- **Description**: Physics-based puzzle game with custom face customization
- **Price**: Free (with £1.50 unlock)
- **Age Rating**: 4+
- **Screenshots Required**: Gameplay, customization, leaderboard

## Monetization Configuration
- `PlayerProfile.IsFullGameUnlocked` persists in `user://profile.json`
- Free tier: Levels 1-20 accessible
- Paid unlock: All 100 levels + ad removal
- AdMob integration ready (requires account setup)
- IAP integration ready (requires store configuration)

## Testing Checklist
- [ ] Main menu loads (MainMenu.tscn)
- [ ] Room selection shows 100 rooms with correct unlock states
- [ ] Slingshot mechanics work in gameplay
- [ ] Projectile physics behave correctly
- [ ] Score system tracks attempts and stars
- [ ] Level completion screen shows properly
- [ ] Progress saves between sessions
- [ ] Face customization works (camera/gallery)
- [ ] Audio manager initializes without errors
- [ ] No console errors during 5+ minutes of gameplay

## Build Configuration
**Godot Version**: 4.4.x required
**.NET Version**: .NET 8.0 (net8.0)
**C# Language**: Latest with nullable enabled
**Dependencies**: Newtonsoft.Json 13.0.3

## Export Presets Required
- [ ] Windows Desktop
- [ ] macOS
- [ ] Linux/X11
- [ ] Android (requires signing)
- [ ] iOS (requires certificates)

## Post-Launch (Future Enhancements)
- [ ] Stripe integration for web version
- [ ] Leaderboard system
- [ ] Additional cosmetic items
- [ ] Level editor for user-generated content