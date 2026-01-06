# Developer Guide: Asset Integration and App Store Submission

## Overview

This comprehensive guide covers the complete infrastructure system for integrating assets and preparing for app store submission. The system includes 10 major phases with extensive tooling for asset management, build validation, and store preparation.

## Infrastructure Components

### Phase 1: Asset Loading & Integration Framework

#### AssetLoader.cs
- **Purpose**: Robust framework for loading sprites with fallback support
- **Features**:
  - Maps scene nodes to asset paths
  - Loads Sprite2D textures from folder structure
  - Falls back to ColorRect placeholders if assets missing
  - Caches loaded textures for reuse
  - Logs missing assets on startup

#### AssetRegistry.cs
- **Purpose**: Defines all asset requirements and validation
- **Features**:
  - Node name → asset path mapping
  - Generates validation reports (% of assets loaded)
  - Supports hot-reloading during development
  - Exports requirements to CSV/JSON

#### SpriteSheetLoader.cs
- **Purpose**: Multi-frame animation support
- **Features**:
  - Texture atlasing support
  - AnimatedSprite2D wrapper
  - Animation configuration files (.json)
  - Projectile launch, impact, and particle animations

#### AssetValidationTool.cs
- **Purpose**: Editor utility for auditing assets
- **Features**:
  - Scans project for ColorRect placeholders
  - Generates missing asset checklists
  - Creates visual preview of placeholders
  - Auto-generates folder structure

### Phase 2: Mobile Build Configuration

#### AndroidBuildManager.cs
- **Purpose**: Android build configuration and export
- **Features**:
  - Complete .aab build setup
  - AndroidManifest.xml with permissions
  - ProGuard/R8 obfuscation rules
  - Keystore management
  - Build validation scripts

#### IosBuildManager.cs
- **Purpose**: iOS build configuration and export
- **Features**:
  - Complete .ipa build setup
  - Info.plist with required keys
  - Entitlements configuration
  - App icon asset placeholders
  - App Store Connect publishing guide

#### DesktopBuildManager.cs
- **Purpose**: Desktop build optimization
- **Features**:
  - Windows, macOS, Linux presets
  - Platform-specific optimizations
  - Code signing guides
  - Build validation scripts

#### BuildValidator.cs
- **Purpose**: Automated build validation
- **Features**:
  - Validates all required configurations
  - Checks signing certificate status
  - Confirms export presets
  - Generates pre-submission reports

### Phase 3: Store Submission Infrastructure

#### PrivacyPolicyManager.cs
- **Purpose**: Privacy policy and legal compliance
- **Features**:
  - GDPR/CCPA compliance
  - Consent management
  - Privacy policy generation
  - Terms of service
  - Cookie policy

#### VersionInfo.cs
- **Purpose**: Centralized version management
- **Features**:
  - App version and build number management
  - Platform-specific versions
  - Release notes generation
  - Version consistency across platforms

#### StoreListingManager.cs
- **Purpose**: Store metadata management
- **Features**:
  - Google Play Store metadata
  - Apple App Store metadata
  - Content rating compliance
  - Screenshot requirements
  - Feature descriptions

### Phase 4: Performance Profiling & Optimization

#### PerformanceMonitor.cs
- **Purpose**: In-game performance monitoring
- **Features**:
  - FPS, frame time, memory usage tracking
  - Performance HUD (F12 toggle)
  - Quality presets (High/Balanced/Performance/Mobile)
  - Auto-quality adjustment
  - Performance reports

#### MemoryLeakDetector.cs
- **Purpose**: Memory management and leak detection
- **Features**:
  - Signal connection tracking
  - Object pooling validation
  - Memory leak detection
  - Circular reference detection
  - Memory usage reports

### Phase 5: Enhanced Audio System

#### EnhancedAudioSystem.cs
- **Purpose**: Advanced audio management
- **Features**:
  - 5+ variants per sound type
  - Pitch/volume variation
  - Spatial audio support
  - Audio ducking
  - Dynamic mixing
  - Audio pools for performance

### Phase 6: Game Feel Tuning

#### GameFeelTuningSystem.cs
- **Purpose**: Real-time parameter adjustment
- **Features**:
  - All game feel parameters exposed
  - Tuning presets (Juicy/Subtle/Arcade/Realistic)
  - Real-time parameter adjustment
  - Save/load tuning presets

### Phase 7: Analytics & Crash Reporting

#### AnalyticsManager.cs
- **Purpose**: Gameplay telemetry
- **Features**:
  - Level progression tracking
  - Feature usage analytics
  - Performance metrics
  - User retention tracking
  - Privacy-compliant analytics

#### CrashReporter.cs
- **Purpose**: Crash reporting and error tracking
- **Features**:
  - Unhandled exception handling
  - Crash report generation
  - Auto-restart functionality
  - Crash threshold detection
  - Error statistics

### Phase 8: Testing Framework

#### TestingFramework.cs
- **Purpose**: Automated testing
- **Features**:
  - Unit tests for physics, expressions, levels
  - Integration tests for system flows
  - Performance benchmarks
  - Automated test reporting

### Phase 9: Level Balance Tools

#### DifficultyAnalyzer.cs
- **Purpose**: Difficulty curve analysis
- **Features**:
  - Level difficulty calculation
  - Difficulty spike detection
  - Balance suggestions
  - Difficulty curve visualization
  - Player behavior analysis

### Phase 10: Release Notes

#### ReleaseNotesGenerator.cs
- **Purpose**: Release notes automation
- **Features**:
  - Git commit analysis
  - Platform-specific formatting
  - Automated note generation
  - Export for all platforms

## Usage Instructions

### Setting Up Asset Integration

1. **Initialize Asset System**:
   ```csharp
   // Asset loader automatically initializes
   AssetLoader.Instance.LoadTexture("res://Assets/Projectiles/animal1.png");
   ```

2. **Register Assets**:
   ```csharp
   // Asset registry maps nodes to assets
   var registry = new AssetRegistry();
   registry.SetAssetMapping("Projectile", "res://Assets/Projectiles/projectile.png");
   ```

3. **Validate Assets**:
   ```csharp
   // Run validation tool
   var validator = new AssetValidationTool();
   validator.RunFullValidation();
   ```

### Building for Stores

1. **Android Build**:
   ```csharp
   var androidBuilder = new AndroidBuildManager();
   androidBuilder.SetupKeystore(keystorePath, password, alias, keyPassword);
   androidBuilder.BuildAab();
   ```

2. **iOS Build**:
   ```csharp
   var iosBuilder = new IosBuildManager();
   iosBuilder.SetupSigning(teamId, provisioningProfilePath);
   iosBuilder.BuildIpa();
   ```

3. **Validate Build**:
   ```csharp
   var validator = new BuildValidator();
   validator.ValidateBuild("android");
   validator.GeneratePreSubmissionReport("android");
   ```

### Store Submission

1. **Privacy Compliance**:
   ```csharp
   var privacyManager = new PrivacyPolicyManager();
   privacyManager.ShowPrivacyNotice();
   privacyManager.UpdateConsentPreferences(analytics, advertising, personalization);
   ```

2. **Store Metadata**:
   ```csharp
   var storeManager = new StoreListingManager();
   storeManager.UpdatePlatformMetadata("android", (metadata) => {
       metadata.Title = "Angry Animals";
       metadata.Keywords = "physics,puzzle,slingshot";
   });
   ```

3. **Release Notes**:
   ```csharp
   var releaseNotes = new ReleaseNotesGenerator();
   string notes = releaseNotes.GenerateReleaseNotes("1.0.0", "google_play");
   ```

### Performance Monitoring

1. **Enable Performance HUD**:
   ```csharp
   var perfMonitor = new PerformanceMonitor();
   perfMonitor.TogglePerformanceHud(); // F12 in-game
   ```

2. **Set Quality Preset**:
   ```csharp
   perfMonitor.SetQualityPreset("Balanced");
   perfMonitor.AutoAdjustQuality();
   ```

3. **Monitor Analytics**:
   ```csharp
   var analytics = new AnalyticsManager();
   analytics.TrackLevelProgress(levelNumber, completed, attempts, timeSpent);
   analytics.TrackFeatureUsage("slingshot_variant", properties);
   ```

## Configuration Files

### export_presets.cfg
Complete export presets for all platforms:
- Android (.aab)
- iOS (.ipa) 
- Windows Desktop (.exe)
- macOS (.app)
- Linux (.AppImage)

### Version Management
```json
{
  "version": "1.0.0",
  "buildNumber": 1,
  "platformVersions": {
    "android": "1.0.0",
    "ios": "1.0.0"
  }
}
```

### Store Metadata
```json
{
  "appName": "Angry Animals",
  "subtitle": "Physics Puzzle Adventure",
  "keywords": ["physics", "puzzle", "slingshot"],
  "contentRating": {
    "rating": "E",
    "descriptors": ["Mild Cartoon Violence"]
  }
}
```

## Best Practices

### Asset Integration
1. Use descriptive node names for automatic asset mapping
2. Follow the established folder structure
3. Test with placeholder fallbacks before adding real assets
4. Use the validation tool regularly during development

### Build Process
1. Always validate builds before submission
2. Test on actual devices, not just emulators
3. Keep signing certificates secure and backed up
4. Use the pre-submission reports for final checks

### Performance
1. Monitor performance metrics during development
2. Use appropriate quality presets for target devices
3. Profile memory usage regularly
4. Test with high object counts

### Testing
1. Run the full test suite before major releases
2. Include both unit and integration tests
3. Test performance benchmarks on target hardware
4. Validate difficulty curves with player data

## Troubleshooting

### Common Issues

1. **Missing Assets**:
   - Check AssetRegistry.json mappings
   - Verify file paths and extensions
   - Use AssetValidationTool to identify issues

2. **Build Failures**:
   - Validate export presets with BuildValidator
   - Check signing certificate validity
   - Verify platform-specific requirements

3. **Performance Issues**:
   - Enable performance HUD (F12)
   - Check memory usage with MemoryLeakDetector
   - Adjust quality presets as needed

4. **Store Rejection**:
   - Review pre-submission reports
   - Ensure privacy policy compliance
   - Validate all metadata requirements

## Integration Checklist

- [ ] AssetLoader and AssetRegistry implemented
- [ ] SpriteSheetLoader with animation support
- [ ] AssetValidationTool generating reports
- [ ] Android build configuration complete
- [ ] iOS build configuration complete
- [ ] Desktop builds configured
- [ ] BuildValidator validating builds
- [ ] Privacy policy and legal documents created
- [ ] Version management system active
- [ ] Store listing metadata prepared
- [ ] Performance monitoring enabled
- [ ] Memory leak detection active
- [ ] Enhanced audio system configured
- [ ] Game feel tuning system available
- [ ] Analytics framework implemented
- [ ] Crash reporting system active
- [ ] Testing framework operational
- [ ] Difficulty analysis tools ready
- [ ] Release notes generator functional

## Next Steps

1. **Asset Integration**:
   - Add real assets following the established structure
   - Use validation tools to ensure completeness
   - Test asset loading and fallback systems

2. **Store Preparation**:
   - Complete signing certificate setup
   - Generate all required store assets
   - Validate builds with pre-submission reports

3. **Performance Optimization**:
   - Profile on target devices
   - Adjust quality presets based on performance data
   - Optimize memory usage patterns

4. **Testing and Validation**:
   - Run comprehensive test suites
   - Conduct user acceptance testing
   - Validate difficulty curves with real player data

The infrastructure is now complete and ready for asset integration and app store submission!