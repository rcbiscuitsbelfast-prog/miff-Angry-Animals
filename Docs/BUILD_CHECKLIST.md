# Angry Animals - Mobile Build Checklist

🎮 **Game:** Angry Animals (Godot 4.4 C#)  
📋 **Version:** 1.0  
📱 **Platforms:** Android & iOS  
✅ **Purpose:** Final checks before store submission

---

## 📋 BUILD CHECKLIST OVERVIEW

This checklist ensures your game is ready for mobile app stores. Work through each section systematically.

**Checklist Structure:**
- [ ] **Must Have** - Required for functional game
- [ ] **Should Have** - Required for store approval  
- [ ] **Nice to Have** - Polish and quality items

---

## 🎯 PRE-BUILD REQUIREMENTS

### ✅ CORE FUNCTIONALITY
- [ ] **Game runs without crashes** for 10+ minutes
- [ ] **All 100 levels load** (free + premium)
- [ ] **Slingshot mechanics work** on touch screen
- [ ] **Traversal phase completes** properly
- [ ] **Progression system works** (level unlocks)
- [ ] **Save/load functions** preserve data
- [ ] **UI responsive** on different screen sizes
- [ ] **Audio plays** without glitches

### ✅ ASSET COMPLETENESS
- [ ] **All 100 level scenes** created and configured
- [ ] **All audio files** present in Assets/Audio/
- [ ] **All sprite files** present in Assets/Sprites/
- [ ] **Face customization assets** (hats, glasses, emotions)
- [ ] **App icon** created (1024x1024 PNG)
- [ ] **Screenshots** prepared for store (see requirements below)

---

## 📦 ANDROID BUILD CHECKLIST

### 🔧 EXPORT CONFIGURATION
- [ ] **Export template downloaded** (via Editor → Manage Export Templates)
- [ ] **Android export preset created**
- [ ] **Release mode selected** (not debug)
- [ ] **Package Name set:** com.rcbiscuits.angryanimals
- [ ] **Version Name:** 1.0 (or your version)
- [ ] **Version Code:** 1 (increment for each release)
- [ ] **Min SDK:** 28 (Android 9.0)
- [ ] **Target SDK:** 34 (latest)
- [ ] **Architectures:** ARM64 (required), ARMv7 (optional)

### 🔑 SIGNING CONFIGURATION
- [ ] **Keystore created** (Tools → Android → Generate Signing Certificate)
- [ ] **Keystore path configured** in export preset
- [ ] **Keystore password set**
- [ ] **Key alias:** UploadKey (or similar)
- [ ] **Key password set**
- [ ] **Remember passwords** (store securely)

### 🎨 ASSETS & RESOURCES
- [ ] **App icon:** 512x512 PNG in export preset
- [ ] **Feature graphic:** 1024x500 PNG (for Google Play)
- [ ] **Banner:** 1024x500 PNG (for TV/devices)
- [ ] **Round icon:** 512x512 PNG (adaptive icons)

### 🔐 PERMISSIONS (Project Settings → Permissions)
**Required:**
- [ ] **Internet** (for ads, IAP)
- [ ] **Access Network State**

**Optional (if needed):**
- [ ] **Camera** (if face customization uses camera)
- [ ] **Read External Storage** (if loading custom images)
- [ ] **Write External Storage** (for saving images)

**Remove unnecessary permissions:**
- [ ] No location permission (unless game uses it)
- [ ] No phone permission
- [ ] No SMS permission

### 🎯 MONETIZATION SETUP
- [ ] **AdMob App ID** configured (project.godot)
- [ ] **Banner Ad Unit ID** configured
- [ ] **Interstitial Ad Unit ID** configured
- [ ] **Rewarded Ad Unit ID** configured
- [ ] **IAP Product ID** set: full_game_unlock
- [ ] **Google Play Console** product configured
- [ ] **Billing library** added to Android build
- [ ] **Test ads** verified working

### 📄 LEGAL & COMPLIANCE
- [ ] **Privacy Policy** created (PRIVACY_POLICY.md)
- [ ] **Privacy Policy URL** (for Play Console)
- [ ] **App content rating** questionnaire completed
- [ ] **Data safety form** filled out
- [ ] **Accessibility** considerations addressed

### 🧪 TESTING
- [ ] **Test on physical Android device** (not just emulator)
- [ ] **Test on minimum SDK version** (Android 9.0)
- [ ] **Test in-app purchases** with test accounts
- [ ] **Test ad functionality** with test ad units
- [ ] **Test offline mode** (no internet)
- [ ] **Test level progression** (complete 5+ levels)
- [ ] **Test memory usage** under 100MB

### 📲 BUILD
- [ ] **Build App Bundle (.aab)** (preferred over .apk)
- [ ] **Bundle size under 150MB** (Google Play limit)
- [ ] **ProGuard/R8 enabled** for code shrinking
- [ ] **NDK r23+** installed for native compilation
- [ ] **JDK 11+** installed for build tools

---

## 🍎 IOS BUILD CHECKLIST

### 🔧 EXPORT CONFIGURATION
- [ ] **iOS export template downloaded**
- [ ] **iOS export preset created**
- [ ] **Bundle Identifier:** com.rcbiscuits.angryanimals
- [ ] **Version:** 1.0.0
- [ ] **Build Number:** 1 (increment each build)
- [ ] **Minimum iOS Version:** 14.0
- [ ] **Orientation:** Portrait or Landscape set
- [ ] **Device family:** iPhone/iPad as appropriate

### 🔑 SIGNING & PROVISIONING
- [ ] **Apple Developer account** active
- [ ] **App ID configured** in Certificates & IDs
- [ ] **Distribution certificate** created
- [ ] **App Store provisioning profile** created
- [ ] **Certificate installed** in Keychain
- [ ] **Provisioning profile downloaded** and installed

### 🎨 ASSETS & ICONS
**App Icon (for iOS, create full set):**
- [ ] **iPhone:** 120x120, 180x180 PNG
- [ ] **iPad:** 76x76, 152x152, 167x167 PNG
- [ ] **App Store:** 1024x1024 PNG (no alpha/transparency)
- [ ] **Launch screen** storyboard configured

**Screenshots (required sizes):**
- [ ] **iPhone 6.7" Display:** 1290x2796 PNG
- [ ] **iPhone 6.5" Display:** 1242x2688 PNG
- [ ] **iPad Pro 12.9":** 2048x2732 PNG

### 🔐 PERMISSIONS & PRIVACY (Info.plist)
- [ ] **NSCameraUsageDescription:** "Used for face customization in gameplay"
- [ ] **NSPhotoLibraryUsageDescription:** "Used to select custom face images"
- [ ] **NSPrivacyAccessedAPITypes** configured
- [ ] **NSPrivacyTracking** set (if using tracking)
- [ ] **NSUserTrackingUsageDescription** (if personalized ads)

### 🎯 MONETIZATION SETUP
- [ ] **AdMob App ID** configured
- [ ] **Ad unit IDs** configured (test versions first)
- [ ] **IAP Product ID** set: full_game_unlock
- [ ] **App Store Connect** product configured
- [ ] **StoreKit framework** included
- [ ] **Test purchases** verified in sandbox

### 📄 LEGAL & COMPLIANCE
- [ ] **Privacy Policy** URL (Apple requires)
- [ ] **App Privacy Questionnaire** completed (App Store Connect)
- [ ] **Age rating** determined
- [ ] **Copyright infringement** review passed
- [ ] **Export compliance** (encryption declaration)
- [ ] **App Review Information** filled out

### 🧪 TESTING
- [ ] **Test on physical iPhone** (various sizes: SE, 13/14, Plus)
- [ ] **Test on iPad** (if supporting)
- [ ] **Test on older iOS version** (iOS 14.0+)
- [ ] **Test in-app purchases** in sandbox
- [ ] **Test ad functionality** with test units
- [ ] **Test with/without internet**
- [ ] **Test memory usage** with Instruments
- [ ] **Test with VoiceOver** (accessibility)

### 📲 BUILD
- [ ] **Export for App Store** (not development)
- [ ] **Create XCArchive** in Xcode
- [ ] **Validate archive** in Xcode
- [ ] **Upload to App Store Connect**
- [ ] **Archive size checked** for cellular download limit

---

## 🎨 CREATIVE ASSETS CHECKLIST

### 📸 SCREENSHOTS
**Each platform needs:** 3-5 screenshots

**Requirements:**
- [ ] **No placeholder graphics** (all custom art)
- [ ] **Show actual gameplay** (not menus only)
- [ ] **Different levels shown** (variety)
- [ ] **Face customization visible** (key feature)
- [ ] **Portrait orientation** (or landscape if game is)
- [ ] **No borders or frames**
- [ ] **Minimum dimensions:**
  - Android: 320x480 to 3840x2160
  - iOS: See specific sizes above

**What to show:**
1. Level in action (slingshot aiming)
2. Destruction/combo happening
3. Level completed screen
4. Face customization menu
5. Multiple levels/obstacles

### 🎬 VIDEO PREVIEW (Optional but Recommended)
**For Google Play (30-60 seconds):**
- [ ] **Show gameplay loop** (launch → destruction → traversal)
- [ ] **Show face customization**
- [ ] **Show multiple levels**
- [ ] **Show progression**
- [ ] **Show special features** (combos, rage system if implemented)

**For App Store (up to 30 seconds):**
- [ ] **No ads shown** (cannot monetize App Preview)
- [ ] **Maximum 5 seconds** of branding/titles
- [ ] **Show actual gameplay**
- [ ] **Portrait or landscape** (must match game)

---

## 🔍 QUALITY CHECKLIST

### 🎮 GAMEPLAY POLISH
- [ ] **No placeholder text** (all UI custom)
- [ ] **Tutorial or instructions** (first level or popup)
- [ ] **Visual feedback** on interactions (button presses)
- [ ] **Error handling** (graceful failures, retry options)
- [ ] **Loading indicators** if needed
- [ ] **Pause functionality** working (Escape button)
- [ ] **Resume functionality** from pause

### 🎨 VISUAL POLISH
- [ ] **Consistent art style** across all scenes
- [ ] **No stretched/squashed sprites** (proper aspect ratio)
- [ ] **Smooth animations** (if implemented)
- [ ] **Particle effects** (nice to have)
- [ ] **Screen transitions** (not jarring jumps)
- [ ] **Resolution independence** (UI scales correctly)

### 🎵 AUDIO POLISH
- [ ] **Music loops seamlessly**
- [ ] **SFX not too loud/quiet** (balanced with music)
- [ ] **Audio ducking** (music quiet during important SFX)
- [ ] **Volume controls** in settings (if implemented)
- [ ] **Mute functionality** working

### 🦾 ACCESSIBILITY
- [ ] **Text readable** (sufficient contrast)
- [ ] **Font size adequate** (not too small)
- [ ] **Color not only indicator** (use symbols too)
- [ ] **Touch targets adequate** (minimum 44x44 points on iOS)
- [ ] **VoiceOver/TalkBack** tested (if implementing)

### 📱 PLATFORM INTEGRATION
- [ ] **App icon** shows on device
- [ ] **Splash screen** displays correctly
- [ ] **Status bar handling** appropriate
- [ ] **Back button** (Android) handled
- [ ] **Home indicator** (iOS) doesn't interfere
- [ ] **Notch/punch-hole** devices supported
- [ ] **Multitasking** (iPad) supported if applicable

---

## 🧪 FINAL TESTING PROTOCOL

### 📋 PRE-SUBMISSION TESTING
**Test on each of these devices:**

**Android:**
- [ ] **Low-end device** (Android 9, 2GB RAM)
- [ ] **Mid-range device** (Android 11, 4GB RAM)
- [ ] **High-end device** (Android 13, 8GB+ RAM)
- [ ] **Tablet** (if supporting)
- [ ] **Different aspect ratios** (16:9, 19:9, 21:9)

**iOS:**
- [ ] **iPhone** (various sizes: SE, 13/14, Plus)
- [ ] **iPad** (if supporting)
- [ ] **Different iOS versions** (14, 15, 16, 17)

### 🔄 FUNCTIONAL TESTING
**Complete these flows:**

1. **First Launch Flow:**
   - [ ] App opens without crash
   - [ ] Main menu displays
   - [ ] No immediate errors

2. **Gameplay Flow:**
   - [ ] Start level 1
   - [ ] Launch projectile
   - [ ] Destroy cups
   - [ ] Traversal phase starts
   - [ ] Reach exit
   - [ ] Level complete shows
   - [ ] Can progress to next level

3. **Progression Flow:**
   - [ ] Complete 3+ levels
   - [ ] Unlock next level
   - [ ] Return to menu
   - [ ] Progress saved
   - [ ] Restart app - progress maintained

4. **Monetization Flow:**
   - [ ] Watch interstitial ad
   - [ ] Rewarded ad offered on failure
   - [ ] Watch rewarded ad
   - [ ] Bonus points applied
   - [ ] Purchase IAP (test)
   - [ ] All levels unlocked after purchase
   - [ ] Ads removed after purchase

5. **Edge Case Flow:**
   - [ ] Minimize app → return (no crash)
   - [ ] Receive phone call → return (no crash)
   - [ ] Lock device → unlock (no crash)
   - [ ] Play with no internet (ads disabled gracefully)
   - [ ] Play with low battery (power save mode)

---

## 📊 METADATA & STORE SETUP

### 📝 GOOGLE PLAY STORE
**Store Listing:**
- [ ] **App title:** "Angry Animals" (30 char max)
- [ ] **Short description:** (80 char max, key features)
- [ ] **Full description:** (4000 char max, detailed)
- [ ] **Feature graphic:** 1024x500 PNG
- [ ] **App icon:** 512x512 PNG
- [ ] **Screenshots:** 2 minimum per form factor
- [ ] **Video:** (30-60 seconds, YouTube link)
- [ ] **Categories:** Games → Puzzle or similar
- [ ] **Content rating:** Accurate (PEGI/ESRB)
- [ ] **Contact email:** Valid support email
- [ ] **Privacy policy:** URL (required)

**Pricing & Distribution:**
- [ ] **App type:** Free with IAP
- [ ] **Price:** Free
- [ ] **IAP item:** full_game_unlock configured (£1.50)
- [ ] **Countries:** Selected markets
- [ ] **Content guidelines:** Compliant
- [ ] **Age rating:** Completed questionnaire

### 🍎 APP STORE CONNECT
**App Information:**
- [ ] **App name:** "Angry Animals"
- [ ] **Privacy URL:** Required
- [ ] **Terms of Use:** URL (standard Apple)
- [ ] **Primary category:** Games
- [ ] **Secondary category:** Puzzle
- [ ] **Rating:** Accurate (4+, 9+, 12+, etc.)
- [ ] **Copyright:** Your company/owning entity
- [ ] **Age rating:** App Review information

**Version Information:**
- [ ] **Version number:** 1.0.0
- [ ] **Build number:** 1
- [ ] **Release notes:** First release
- [ ] **Test information:** (for reviewer if needed)

**Screenshots:**
- [ ] **All required sizes:** See iOS build checklist
- [ ] **Screenshots accurate:** Show actual gameplay
- [ ] **No marketing overlaid on screenshots**

---

## 🚀 SUBMISSION PROCESS

### 📤 GOOGLE PLAY SUBMISSION
**Pre-submission:**
- [ ] **Internal testing track:** Upload and test
- [ ] **Closed testing:** Invite testers
- [ ] **Open testing:** (optional)
- [ ] **Pre-launch report:** Review results

**Submission:**
- [ ] **Production track:** Upload final AAB
- [ ] **Complete all store listing sections**
- [ ] **Fill content rating questionnaire**
- [ ] **Set pricing and distribution**
- [ ] **Submit for review**

**Timeline:** 2-7 days for review

---

### 📤 APP STORE SUBMISSION
**Pre-submission:**
- [ ] **TestFlight:** Upload build, invite testers
- [ ] **Test:** Minimum 1-2 weeks of testing
- [ ] **Feedback:** Address all issues

**Submission:**
- [ ] **Archive and upload** to App Store Connect
- [ ] **Complete App Privacy questionnaire**
- [ ] **Fill in all metadata**
- [ ] **Submit for review**

**Timeline:** 1-3 days for review (often faster)

---

## 📋 POST-LAUNCH CHECKLIST

**After approval:**

### 🎉 LAUNCH DAY
- [ ] **Monitor crashes** (Firebase Crashlytics or similar)
- [ ] **Check reviews** daily for first week
- [ ] **Respond to user feedback**
- [ ] **Monitor analytics** (if implemented)
- [ ] **Prepare fast-follow update** for any critical issues

**First Week:**
- [ ] **Address urgent bugs** immediately
- [ ] **Engage with community** (social media, forums)
- [ ] **Plan first content update** (if applicable)
- [ ] **Check monetization metrics** (if using analytics)

---

## 🎯 FINAL SIGN-OFF

**Before you hit "Submit", confirm:**

### ✅ FUNCTIONAL
- [ ] **Crashes:** None experienced in testing
- [ ] **Bugs:** No known major bugs
- [ ] **Flow:** All user paths tested
- [ ] **Performance:** Acceptable on all test devices

### ✅ COMPLETE
- [ ] **Assets:** All final, no placeholders
- [ ] **Text:** No lorem ipsum, all real copy
- [ ] **Audio:** All final music/SFX
- [ ] **Levels:** 100 complete levels

### ✅ COMPLIANT
- [ ] **Platform guidelines:** Met (Apple/Google)
- [ ] **Legal:** Privacy policy, terms
- [ ] **Assets:** No copyrighted material
- [ ] **Age rating:** Accurate

### ✅ POLISHED
- [ ] **First impression:** Positive
- [ ] **User experience:** Smooth and intuitive
- [ ] **Visual quality:** Professional
- [ ] **Audio quality:** Professional

---

## 🚀 YOU'RE READY!

If you've checked all applicable boxes above, your game is ready for submission!

**Good luck with your launch! 🎉**

---

**Remember:** Store requirements change periodically. Always check current guidelines:
- [Google Play Policy Center](https://play.google.com/about/policies/)
- [Apple App Store Review Guidelines](https://developer.apple.com/app-store/review/guidelines/)

**Last Updated:** $(date)