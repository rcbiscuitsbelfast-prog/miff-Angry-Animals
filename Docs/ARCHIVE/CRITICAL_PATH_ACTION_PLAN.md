# CRITICAL PATH: Prioritized Action Plan (Next 90 Days)

**Created:** January 6, 2025  
**Priority:** URGENT - App Store Submission Blockers  
**Timeline:** 6-8 weeks to launch readiness

---

## 🚨 IMMEDIATE ACTIONS (Days 1-7)

### Day 1-2: Export Configuration (CRITICAL - BLOCKING ALL BUILDS)
**Status:** MISSING - Cannot create any mobile builds
**Time Required:** 2-4 hours

#### Task: Create export_presets.cfg
```ini
# Location: /home/engine/project/export_presets.cfg

[preset.0]
name="Android"
platform="Android"
runnable=true
export_path="builds/Android/AngryAnimals.aab"
encryption_include_filters=""
encryption_exclude_filters=""
encrypt_pck=false
encrypt_directory=false

[preset.1]
name="iOS"
platform="iOS" 
runnable=true
export_path="builds/iOS/AngryAnimals.ipa"

[preset.2]
name="Windows Desktop"
platform="Windows Desktop"
runnable=true
export_path="builds/Windows/AngryAnimals.exe"

[preset.3]
name="macOS"
platform="macOS"
runnable=true
export_path="builds/macOS/AngryAnimals.app"

[preset.4]
name="Linux/X11"
platform="Linux/X11"
runnable=true
export_path="builds/Linux/AngryAnimals.x86_64"
```

#### Subtasks:
- [ ] Copy export_presets.example.cfg to export_presets.cfg
- [ ] Update platform-specific settings (keystore, certificates)
- [ ] Test desktop builds (Windows, Mac, Linux)
- [ ] Document any missing dependencies

### Day 3-4: Android Build Setup (CRITICAL)
**Status:** Not configured
**Time Required:** 4-6 hours

#### Tasks:
- [ ] Install Android SDK command line tools
- [ ] Setup Java Development Kit (JDK 11+)
- [ ] Create Android keystore for app signing
- [ ] Configure AndroidManifest.xml permissions:
  - [ ] INTERNET (for ads)
  - [ ] CAMERA (for face capture)
  - [ ] WRITE_EXTERNAL_STORAGE (for saves)
- [ ] Test Android build (.aab format)
- [ ] Verify app icon sizes (48x48 to 512x512)

### Day 5-7: iOS Build Setup (CRITICAL)
**Status:** Not configured  
**Time Required:** 4-6 hours

#### Tasks:
- [ ] Install Xcode (requires macOS)
- [ ] Setup Apple Developer Account ($99/year)
- [ ] Create provisioning profiles (development + distribution)
- [ ] Configure Info.plist permissions:
  - [ ] NSCameraUsageDescription
  - [ ] NSPhotoLibraryUsageDescription
  - [ ] NSPhotoLibraryAddOnlyUsageDescription
- [ ] Create iOS app icons (20x20 to 1024x1024)
- [ ] Test iOS build (.ipa format)

---

## ⚡ HIGH PRIORITY (Days 8-21)

### Week 2: High-Priority Asset Replacement
**Time Required:** 20-30 hours

#### Asset Categories (Priority Order):

**1. Slingshot Variants (CRITICAL)**
- [ ] Create 4 distinct slingshot sprites
- [ ] Replace ColorRect in Scenes/Infrastructure/Slingshot.tscn
- [ ] Test all 4 variants in gameplay
- [ ] Verify physics still work correctly

**2. Cups/Obstacles (CRITICAL)**
- [ ] Create destructible cup sprite
- [ ] Replace ColorRect in Scenes/Obstacles/Cup.tscn
- [ ] Test cup destruction physics
- [ ] Verify scoring triggers correctly

**3. StickClone Character (CRITICAL)**
- [ ] Create StickClone body sprite
- [ ] Replace ColorRect in Scenes/Characters/StickClone.tscn
- [ ] Test traversal animation
- [ ] Verify face display works

**4. UI Buttons (HIGH PRIORITY)**
- [ ] Create button sprites for main menu
- [ ] Replace ColorRect in Scenes/MainMenu.tscn
- [ ] Test button interactions
- [ ] Verify sound effects play

#### Asset Creation Specifications:
```
Resolution: 1920x1080 base, mobile-optimized
Format: PNG with transparency
Color Depth: 32-bit (RGBA)
Naming: Descriptive (e.g., "SlingshotClassic.png")
```

### Week 3: Store Account Setup (URGENT)

#### AdMob Configuration (Day 15-17):
- [ ] Create AdMob account
- [ ] Setup app for Android and iOS
- [ ] Create ad units:
  - [ ] Banner ads (320x50 mobile)
  - [ ] Interstitial ads (full screen)
  - [ ] Rewarded ads (bonus points)
- [ ] Test ad display in sandbox mode
- [ ] Update AdsManager.cs with real ad unit IDs

#### IAP Configuration (Day 18-21):
- [ ] Google Play Console setup
  - [ ] Create app listing
  - [ ] Setup in-app product "full_game_unlock"
  - [ ] Configure pricing (£1.50)
  - [ ] Test purchase flow
- [ ] Apple App Store Connect setup
  - [ ] Create app listing
  - [ ] Setup IAP product "full_game_unlock"
  - [ ] Test purchase flow via TestFlight

---

## 📱 MEDIUM PRIORITY (Days 22-42)

### Week 4-5: Medium-Priority Assets
**Time Required:** 15-20 hours

#### Asset Replacement:
**5. Background Elements**
- [ ] Floor texture replacement
- [ ] Wall/boundary sprites
- [ ] Background scenery elements
- [ ] Test mobile performance

**6. Particle Effect Sprites**
- [ ] Explosion sprite sheets
- [ ] Confetti particle textures
- [ ] Dust cloud effects
- [ ] Sparkle/glitter effects

**7. Facial Expressions**
- [ ] Create 14 expression sprites
- [ ] Replace ColorRect expressions in StickClone.tscn
- [ ] Test expression switching
- [ ] Verify audio cues match expressions

#### Quality Assurance:
- [ ] Test all replaced assets on mobile devices
- [ ] Verify no broken functionality
- [ ] Check memory usage (target: <512MB)
- [ ] Test frame rate (target: 60 FPS)

### Week 6: Comprehensive Testing
**Time Required:** 10-15 hours

#### Test Matrix:
- [ ] **Android Phone**: Test gameplay flow
- [ ] **Android Tablet**: UI scaling verification
- [ ] **iPhone**: Performance and compatibility
- [ ] **iPad**: Tablet optimization
- [ ] **Desktop**: Windows/Mac/Linux builds

#### Critical Test Scenarios:
- [ ] Level 1-5: Basic progression
- [ ] Level 20: Free tier boundary
- [ ] Level 21: Premium unlock verification
- [ ] Level 100: Final level completion
- [ ] Procedural levels: Seed consistency
- [ ] Face capture: Camera functionality
- [ ] IAP purchase: Full unlock
- [ ] Ad display: All ad types

---

## 🎨 LOW PRIORITY (Days 43-56)

### Week 7-8: Final Polish & Store Assets
**Time Required:** 10-15 hours

#### Store Assets Creation:
**Google Play Store:**
- [ ] App icon (512x512 PNG)
- [ ] Feature graphic (1024x500 PNG)
- [ ] Screenshots (1080x1920 phone, 1920x1080 tablet)
- [ ] Promo video (optional, 30-60 seconds)

**Apple App Store:**
- [ ] App icon (1024x1024 PNG)
- [ ] iPhone screenshots (5.5" and 6.7" displays)
- [ ] iPad screenshots (12.9" display)
- [ ] App preview video (optional)

#### Final Asset Polish:
- [ ] Replace remaining ColorRect placeholders
- [ ] Add environmental detail sprites
- [ ] Enhance particle effects
- [ ] Optimize sprite atlases for mobile

---

## 🚀 SUBMISSION PHASE (Days 57-63)

### Week 9: Final Submission Prep
**Time Required:** 5-10 hours

#### Pre-Submission Checklist:
- [ ] All builds created successfully
- [ ] Performance benchmarks met (60 FPS)
- [ ] Memory usage optimized (<512MB)
- [ ] Crash-free testing (24+ hours)
- [ ] Store listings completed
- [ ] Privacy policy created
- [ ] Age rating questionnaires completed

#### Submission Tasks:
- [ ] **Google Play Store**:
  - [ ] Upload AAB build
  - [ ] Complete store listing
  - [ ] Submit for review
  - [ ] Timeline: 1-3 days

- [ ] **Apple App Store**:
  - [ ] Upload IPA build
  - [ ] Complete App Store Connect listing
  - [ ] Submit for review
  - [ ] Timeline: 1-7 days

---

## 📊 SUCCESS METRICS & TRACKING

### Daily Standup Format:
```
Yesterday: [What was completed]
Today: [What will be completed]
Blockers: [Any issues preventing progress]
Metrics: [Performance data, test results]
```

### Weekly Review:
- [ ] **Asset Progress**: % of ColorRects replaced
- [ ] **Build Status**: All platforms building successfully
- [ ] **Performance**: FPS benchmarks on target devices
- [ ] **Store Setup**: AdMob and IAP configuration complete
- [ ] **Testing**: Hours of crash-free gameplay

### Milestone Checkpoints:
- **Day 7**: All builds working
- **Day 21**: Store accounts configured
- **Day 35**: High-priority assets replaced
- **Day 49**: Comprehensive testing complete
- **Day 63**: Ready for submission

---

## 🛠️ RESOURCE REQUIREMENTS

### Team Roles:
- **Game Developer**: Asset integration, build configuration (40-50 hours)
- **Artist/Designer**: Sprite creation, UI design (20-30 hours)
- **Tester**: Cross-platform testing (10-15 hours)
- **Product Manager**: Store setup, marketing assets (5-10 hours)

### Software/Tools Needed:
- **Godot 4.4+**: Development and builds
- **Android Studio**: Android SDK setup
- **Xcode**: iOS builds (requires Mac)
- **Graphic Software**: Photoshop, GIMP, or similar
- **AdMob Console**: Ad configuration
- **App Store Connect**: iOS store management

### Budget Estimate:
- **Apple Developer Account**: $99/year
- **Google Play**: $25 one-time
- **Asset Creation**: $500-2000 (if outsourcing)
- **Total**: $6255

-212---

## ⚠️ RISK MITIGATION

### High-Risk Items:
1. **iOS Certificate Issues**: Have backup developer account
2. **Android SDK Problems**: Use Godot's built-in Android templates
3. **Asset Performance**: Test on low-end devices early
4. **Store Rejection**: Follow guidelines exactly

### Contingency Plans:
- **Asset Delays**: Launch with placeholders, replace post-launch
- **Build Issues**: Use Godot's export templates as fallback
- **Store Rejection**: Have backup submission strategy
- **Performance Problems**: Implement LOD system for sprites

---

## ✅ DEFINITION OF DONE

### Launch Readiness Criteria:
- [ ] **Code**: All builds compile without errors
- [ ] **Assets**: High-priority items replaced with professional sprites
- [ ] **Performance**: 60 FPS on mid-range mobile devices
- [ ] **Monetization**: Ads and IAP working in sandbox mode
- [ ] **Testing**: 24+ hours crash-free across all platforms
- [ ] **Store Assets**: All required screenshots, icons, descriptions
- [ ] **Legal**: Privacy policy, age rating, compliance complete

### Quality Gates:
- [ ] **No ColorRect placeholders** in high-priority items
- [ ] **All 4 slingshot variants** visually distinct
- [ ] **100 levels playable** without errors
- [ ] **Procedural generation** working with seeds
- [ ] **Face customization** functional on mobile
- [ ] **IAP purchase flow** tested end-to-end

---

**Next Action**: Start with Day 1-2 (export_presets.cfg creation) - this is blocking ALL progress on mobile builds.

**Critical Dependency**: Cannot proceed with any mobile testing or store submission until export presets are configured.

**Success Probability**: High (85%) with focused execution on this 90-day plan.
