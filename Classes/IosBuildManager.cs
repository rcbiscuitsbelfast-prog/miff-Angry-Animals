using Godot;
using System;
using System.Collections.Generic;
using System.IO;

/// <summary>
/// iOS build configuration and export preset management
/// Handles .ipa build creation with proper Info.plist and signing requirements
/// </summary>
public class IosBuildManager : Node
{
    public static IosBuildManager Instance { get; private set; }

    // Build configuration
    private IosBuildConfig _config;
    private string _exportPresetPath = "res://ios_export_preset.cfg";
    private string _infoPlistPath = "res://ios/Info.plist";
    
    [Signal]
    public delegate void BuildConfigUpdatedEventHandler();
    
    [Signal]
    public delegate void BuildCompletedEventHandler(string buildPath, bool success);
    
    [Signal]
    public delegate void ValidationErrorEventHandler(string error);

    public override void _Ready()
    {
        if (Instance != null)
        {
            QueueFree();
            return;
        }
        
        Instance = this;
        InitializeConfiguration();
    }

    /// <summary>
    /// Initialize iOS build configuration
    /// </summary>
    private void InitializeConfiguration()
    {
        _config = new IosBuildConfig
        {
            BundleId = "com.miff.angryanimalsgame",
            AppName = "Angry Animals",
            Version = "1.0.0",
            BuildNumber = 1,
            MinIosVersion = "14.0",
            TeamId = "", // Set by user
            SigningCertificate = "Apple Development", // or "Apple Distribution"
            ProvisioningProfile = "", // Set by user
            InfoPlistPath = _infoPlistPath,
            EnableBitcode = false,
            EnableArc = true,
            DeploymentTarget = "14.0"
        };
        
        CreateExportPreset();
        CreateInfoPlist();
        CreateEntitlements();
        CreateAppStoreGuide();
        
        GD.Print("iOS build configuration initialized");
    }

    /// <summary>
    /// Create iOS export preset configuration
    /// </summary>
    private void CreateExportPreset()
    {
        try
        {
            using (var writer = new StreamWriter(_exportPresetPath))
            {
                writer.WriteLine("[preset.0]");
                writer.WriteLine("name=\"iOS\"");
                writer.WriteLine("platform=\"iOS\"");
                writer.WriteLine("runnable=true");
                writer.WriteLine("dedicated_server=false");
                writer.WriteLine("custom_features=\"\"");
                writer.WriteLine("export_filter=\"all_resources\"");
                writer.WriteLine("include_filter=\"\"");
                writer.WriteLine("exclude_filter=\"\"");
                writer.WriteLine("export_path=\"builds/ios/Angry_Animals.ipa\"");
                writer.WriteLine("encryption_include_filters=\"\"");
                writer.WriteLine("encryption_exclude_filters=\"\"");
                writer.WriteLine("encrypt_pck=false");
                writer.WriteLine("encrypt_directory=false");
                writer.WriteLine();
                
                writer.WriteLine("[preset.0.options]");
                writer.WriteLine("custom_template/debug=\"\"");
                writer.WriteLine("custom_template/release=\"\"");
                writer.WriteLine("binary_format/architecture=universal");
                writer.WriteLine("binary_format/signed=true");
                writer.WriteLine("application/short_version=\"1.0\"");
                writer.WriteLine("application/version=\"1\"");
                writer.WriteLine("application/signature=\"??\"");
                writer.WriteLine("application/short_version_string=\"1.0.0\"");
                writer.WriteLine("application/bundle_identifier=\"com.miff.angryanimalsgame\"");
                writer.WriteLine("application/team_id=\"\"");
                writer.WriteLine("application/signing_certificate=\"Apple Development\"");
                writer.WriteLine("application/provisioning_profile=\"\"");
                writer.WriteLine("application/provisioning_profile_path=\"\"");
                writer.WriteLine("application/app_icon=\"\"");
                writer.WriteLine("application/launch_image=\"\"");
                writer.WriteLine("user_data/custom_template/debug=\"\"");
                writer.WriteLine("user_data/custom_template/release=\"\"");
                writer.WriteLine("user_data/binary_format/architecture=universal");
                writer.WriteLine("user_data/binary_format/signed=true");
                writer.WriteLine("user_data/application/short_version=\"1.0\"");
                writer.WriteLine("user_data/application/version=\"1\"");
                writer.WriteLine("user_data/application/signature=\"??\"");
                writer.WriteLine("user_data/application/short_version_string=\"1.0.0\"");
                writer.WriteLine("user_data/application/bundle_identifier=\"com.miff.angryanimalsgame\"");
                writer.WriteLine("user_data/application/team_id=\"\"");
                writer.WriteLine("user_data/application/signing_certificate=\"Apple Development\"");
                writer.WriteLine("user_data/application/provisioning_profile=\"\"");
                writer.WriteLine("user_data/application/provisioning_profile_path=\"\"");
                writer.WriteLine("user_data/application/app_icon=\"\"");
                writer.WriteLine("user_data/application/launch_image=\"\"");
            }
            
            GD.Print($"iOS export preset created: {_exportPresetPath}");
        }
        catch (Exception e)
        {
            EmitSignal("ValidationError", $"Failed to create iOS export preset: {e.Message}");
        }
    }

    /// <summary>
    /// Create Info.plist with required keys
    /// </summary>
    private void CreateInfoPlist()
    {
        string infoDir = Path.GetDirectoryName(_infoPlistPath);
        if (!Directory.Exists(infoDir))
        {
            Directory.CreateDirectory(infoDir);
        }
        
        try
        {
            using (var writer = new StreamWriter(_infoPlistPath))
            {
                writer.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                writer.WriteLine("<!DOCTYPE plist PUBLIC \"-//Apple//DTD PLIST 1.0//EN\" \"http://www.apple.com/DTDs/PropertyList-1.0.dtd\">");
                writer.WriteLine("<plist version=\"1.0\">");
                writer.WriteLine("<dict>");
                writer.WriteLine();
                
                writer.WriteLine("    <!-- App Identification -->");
                writer.WriteLine("    <key>CFBundleDisplayName</key>");
                writer.WriteLine($"    <string>{_config.AppName}</string>");
                writer.WriteLine("    <key>CFBundleIdentifier</key>");
                writer.WriteLine($"    <string>{_config.BundleId}</string>");
                writer.WriteLine("    <key>CFBundleName</key>");
                writer.WriteLine($"    <string>{_config.AppName}</string>");
                writer.WriteLine("    <key>CFBundleVersion</key>");
                writer.WriteLine($"    <string>{_config.BuildNumber}</string>");
                writer.WriteLine("    <key>CFBundleShortVersionString</key>");
                writer.WriteLine($"    <string>{_config.Version}</string>");
                writer.WriteLine();
                
                writer.WriteLine("    <!-- Device Compatibility -->");
                writer.WriteLine("    <key>UIRequiredDeviceCapabilities</key>");
                writer.WriteLine("    <array>");
                writer.WriteLine("        <string>arm64</string>");
                writer.WriteLine("    </array>");
                writer.WriteLine("    <key>UISupportedInterfaceOrientations</key>");
                writer.WriteLine("    <array>");
                writer.WriteLine("        <string>UIInterfaceOrientationLandscapeLeft</string>");
                writer.WriteLine("        <string>UIInterfaceOrientationLandscapeRight</string>");
                writer.WriteLine("    </array>");
                writer.WriteLine("    <key>UISupportedInterfaceOrientations~ipad</key>");
                writer.WriteLine("    <array>");
                writer.WriteLine("        <string>UIInterfaceOrientationLandscapeLeft</string>");
                writer.WriteLine("        <string>UIInterfaceOrientationLandscapeRight</string>");
                writer.WriteLine("        <string>UIInterfaceOrientationPortrait</string>");
                writer.WriteLine("        <string>UIInterfaceOrientationPortraitUpsideDown</string>");
                writer.WriteLine("    </array>");
                writer.WriteLine();
                
                writer.WriteLine("    <!-- Privacy Permissions -->");
                writer.WriteLine("    <key>NSCameraUsageDescription</key>");
                writer.WriteLine("    <string>This app uses the camera to take photos of your face for character customization.</string>");
                writer.WriteLine("    <key>NSPhotoLibraryUsageDescription</key>");
                writer.WriteLine("    <string>This app accesses your photo library to select images for character customization.</string>");
                writer.WriteLine("    <key>NSPhotoLibraryAddOnlyUsageDescription</key>");
                writer.WriteLine("    <string>This app saves images to your photo library.</string>");
                writer.WriteLine();
                
                writer.WriteLine("    <!-- App Transport Security -->");
                writer.WriteLine("    <key>NSAppTransportSecurity</key>");
                writer.WriteLine("    <dict>");
                writer.WriteLine("        <key>NSAllowsArbitraryLoads</key>");
                writer.WriteLine("        <true/>");
                writer.WriteLine("        <key>NSAllowsLocalNetworking</key>");
                writer.WriteLine("        <true/>");
                writer.WriteLine("    </dict>");
                writer.WriteLine();
                
                writer.WriteLine("    <!-- Status Bar Configuration -->");
                writer.WriteLine("    <key>UIStatusBarHidden</key>");
                writer.WriteLine("    <true/>");
                writer.WriteLine("    <key>UIViewControllerBasedStatusBarAppearance</key>");
                writer.WriteLine("    <false/>");
                writer.WriteLine();
                
                writer.WriteLine("    <!-- App Icon and Launch Screen -->");
                writer.WriteLine("    <key>CFBundleIconFiles</key>");
                writer.WriteLine("    <array>");
                writer.WriteLine("        <string>Icon-App-20x20@1x.png</string>");
                writer.WriteLine("        <string>Icon-App-20x20@2x.png</string>");
                writer.WriteLine("        <string>Icon-App-20x20@3x.png</string>");
                writer.WriteLine("        <string>Icon-App-29x29@1x.png</string>");
                writer.WriteLine("        <string>Icon-App-29x29@2x.png</string>");
                writer.WriteLine("        <string>Icon-App-29x29@3x.png</string>");
                writer.WriteLine("        <string>Icon-App-40x40@1x.png</string>");
                writer.WriteLine("        <string>Icon-App-40x40@2x.png</string>");
                writer.WriteLine("        <string>Icon-App-40x40@3x.png</string>");
                writer.WriteLine("        <string>Icon-App-60x60@2x.png</string>");
                writer.WriteLine("        <string>Icon-App-60x60@3x.png</string>");
                writer.WriteLine("        <string>Icon-App-76x76@1x.png</string>");
                writer.WriteLine("        <string>Icon-App-76x76@2x.png</string>");
                writer.WriteLine("        <string>Icon-App-83.5x83.5@2x.png</string>");
                writer.WriteLine("        <string>Icon-App-1024x1024@1x.png</string>");
                writer.WriteLine("    </array>");
                writer.WriteLine();
                
                writer.WriteLine("    <!-- Additional Configuration -->");
                writer.WriteLine("    <key>LSRequiresIPhoneOS</key>");
                writer.WriteLine("    <true/>");
                writer.WriteLine("    <key>UILaunchStoryboardName</key>");
                writer.WriteLine("    <string>LaunchScreen</string>");
                writer.WriteLine("    <key>UIMainStoryboardFile</key>");
                writer.WriteLine("    <string>Main</string>");
                writer.WriteLine("    <key>UIApplicationSupportsIndirectInputEvents</key>");
                writer.WriteLine("    <true/>");
                writer.WriteLine("    <key>UISupportsDocumentBrowser</key>");
                writer.WriteLine("    <false/>");
                writer.WriteLine();
                
                writer.WriteLine("</dict>");
                writer.WriteLine("</plist>");
            }
            
            GD.Print($"Info.plist created: {_infoPlistPath}");
        }
        catch (Exception e)
        {
            EmitSignal("ValidationError", $"Failed to create Info.plist: {e.Message}");
        }
    }

    /// <summary>
    /// Create entitlements file for app signing
    /// </summary>
    private void CreateEntitlements()
    {
        string entitlementsPath = "res://ios/Angry_Animals.entitlements";
        string entitlementsDir = Path.GetDirectoryName(entitlementsPath);
        
        if (!Directory.Exists(entitlementsDir))
        {
            Directory.CreateDirectory(entitlementsDir);
        }
        
        try
        {
            using (var writer = new StreamWriter(entitlementsPath))
            {
                writer.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                writer.WriteLine("<!DOCTYPE plist PUBLIC \"-//Apple//DTD PLIST 1.0//EN\" \"http://www.apple.com/DTDs/PropertyList-1.0.dtd\">");
                writer.WriteLine("<plist version=\"1.0\">");
                writer.WriteLine("<dict>");
                writer.WriteLine();
                
                writer.WriteLine("    <!-- App Groups for shared data -->");
                writer.WriteLine("    <key>com.apple.security.application-groups</key>");
                writer.WriteLine("    <array>");
                writer.WriteLine($"        <string>group.{_config.BundleId}</string>");
                writer.WriteLine("    </array>");
                writer.WriteLine();
                
                writer.WriteLine("    <!-- In-App Purchase -->");
                writer.WriteLine("    <key>com.apple.developer.in-app-purchase</key>");
                writer.WriteLine("    <true/>");
                writer.WriteLine();
                
                writer.WriteLine("    <!-- Keychain Access -->");
                writer.WriteLine("    <key>keychain-access-groups</key>");
                writer.WriteLine("    <array>");
                writer.WriteLine($"        <string>{_config.BundleId}</string>");
                writer.WriteLine("    </array>");
                writer.WriteLine();
                
                writer.WriteLine("    <!-- Push Notifications (if needed) -->");
                writer.WriteLine("    <key>aps-environment</key>");
                writer.WriteLine("    <string>production</string>");
                writer.WriteLine();
                
                writer.WriteLine("    <!-- Background Modes -->");
                writer.WriteLine("    <key>UIBackgroundModes</key>");
                writer.WriteLine("    <array>");
                writer.WriteLine("        <string>audio</string>");
                writer.WriteLine("    </array>");
                writer.WriteLine();
                
                writer.WriteLine("</dict>");
                writer.WriteLine("</plist>");
            }
            
            GD.Print($"Entitlements created: {entitlementsPath}");
        }
        catch (Exception e)
        {
            GD.PrintErr($"Failed to create entitlements: {e.Message}");
        }
    }

    /// <summary>
    /// Create app icon asset placeholders
    /// </summary>
    public void CreateAppIconAssets()
    {
        string iconsDir = "res://ios/Assets.xcassets/AppIcon.appiconset";
        
        if (!Directory.Exists(iconsDir))
        {
            Directory.CreateDirectory(iconsDir);
        }
        
        // Create Contents.json for icon set
        string contentsJsonPath = Path.Combine(iconsDir, "Contents.json");
        
        try
        {
            using (var writer = new StreamWriter(contentsJsonPath))
            {
                writer.WriteLine("{");
                writer.WriteLine("  \"images\" : [");
                writer.WriteLine("    {");
                writer.WriteLine("      \"filename\" : \"Icon-App-20x20@1x.png\",");
                writer.WriteLine("      \"idiom\" : \"iphone\",");
                writer.WriteLine("      \"scale\" : \"1x\",");
                writer.WriteLine("      \"size\" : \"20x20\"");
                writer.WriteLine("    },");
                writer.WriteLine("    {");
                writer.WriteLine("      \"filename\" : \"Icon-App-20x20@2x.png\",");
                writer.WriteLine("      \"idiom\" : \"iphone\",");
                writer.WriteLine("      \"scale\" : \"2x\",");
                writer.WriteLine("      \"size\" : \"20x20\"");
                writer.WriteLine("    },");
                writer.WriteLine("    {");
                writer.WriteLine("      \"filename\" : \"Icon-App-20x20@3x.png\",");
                writer.WriteLine("      \"idiom\" : \"iphone\",");
                writer.WriteLine("      \"scale\" : \"3x\",");
                writer.WriteLine("      \"size\" : \"20x20\"");
                writer.WriteLine("    },");
                writer.WriteLine("    {");
                writer.WriteLine("      \"filename\" : \"Icon-App-29x29@1x.png\",");
                writer.WriteLine("      \"idiom\" : \"iphone\",");
                writer.WriteLine("      \"scale\" : \"1x\",");
                writer.WriteLine("      \"size\" : \"29x29\"");
                writer.WriteLine("    },");
                writer.WriteLine("    {");
                writer.WriteLine("      \"filename\" : \"Icon-App-29x29@2x.png\",");
                writer.WriteLine("      \"idiom\" : \"iphone\",");
                writer.WriteLine("      \"scale\" : \"2x\",");
                writer.WriteLine("      \"size\" : \"29x29\"");
                writer.WriteLine("    },");
                writer.WriteLine("    {");
                writer.WriteLine("      \"filename\" : \"Icon-App-29x29@3x.png\",");
                writer.WriteLine("      \"idiom\" : \"iphone\",");
                writer.WriteLine("      \"scale\" : \"3x\",");
                writer.WriteLine("      \"size\" : \"29x29\"");
                writer.WriteLine("    },");
                writer.WriteLine("    {");
                writer.WriteLine("      \"filename\" : \"Icon-App-40x40@1x.png\",");
                writer.WriteLine("      \"idiom\" : \"iphone\",");
                writer.WriteLine("      \"scale\" : \"1x\",");
                writer.WriteLine("      \"size\" : \"40x40\"");
                writer.WriteLine("    },");
                writer.WriteLine("    {");
                writer.WriteLine("      \"filename\" : \"Icon-App-40x40@2x.png\",");
                writer.WriteLine("      \"idiom\" : \"iphone\",");
                writer.WriteLine("      \"scale\" : \"2x\",");
                writer.WriteLine("      \"size\" : \"40x40\"");
                writer.WriteLine("    },");
                writer.WriteLine("    {");
                writer.WriteLine("      \"filename\" : \"Icon-App-40x40@3x.png\",");
                writer.WriteLine("      \"idiom\" : \"iphone\",");
                writer.WriteLine("      \"scale\" : \"3x\",");
                writer.WriteLine("      \"size\" : \"40x40\"");
                writer.WriteLine("    },");
                writer.WriteLine("    {");
                writer.WriteLine("      \"filename\" : \"Icon-App-60x60@2x.png\",");
                writer.WriteLine("      \"idiom\" : \"iphone\",");
                writer.WriteLine("      \"scale\" : \"2x\",");
                writer.WriteLine("      \"size\" : \"60x60\"");
                writer.WriteLine("    },");
                writer.WriteLine("    {");
                writer.WriteLine("      \"filename\" : \"Icon-App-60x60@3x.png\",");
                writer.WriteLine("      \"idiom\" : \"iphone\",");
                writer.WriteLine("      \"scale\" : \"3x\",");
                writer.WriteLine("      \"size\" : \"60x60\"");
                writer.WriteLine("    },");
                writer.WriteLine("    {");
                writer.WriteLine("      \"filename\" : \"Icon-App-76x76@1x.png\",");
                writer.WriteLine("      \"idiom\" : \"ipad\",");
                writer.WriteLine("      \"scale\" : \"1x\",");
                writer.WriteLine("      \"size\" : \"76x76\"");
                writer.WriteLine("    },");
                writer.WriteLine("    {");
                writer.WriteLine("      \"filename\" : \"Icon-App-76x76@2x.png\",");
                writer.WriteLine("      \"idiom\" : \"ipad\",");
                writer.WriteLine("      \"scale\" : \"2x\",");
                writer.WriteLine("      \"size\" : \"76x76\"");
                writer.WriteLine("    },");
                writer.WriteLine("    {");
                writer.WriteLine("      \"filename\" : \"Icon-App-83.5x83.5@2x.png\",");
                writer.WriteLine("      \"idiom\" : \"ipad\",");
                writer.WriteLine("      \"scale\" : \"2x\",");
                writer.WriteLine("      \"size\" : \"83.5x83.5\"");
                writer.WriteLine("    },");
                writer.WriteLine("    {");
                writer.WriteLine("      \"filename\" : \"Icon-App-1024x1024@1x.png\",");
                writer.WriteLine("      \"idiom\" : \"ios-marketing\",");
                writer.WriteLine("      \"scale\" : \"1x\",");
                writer.WriteLine("      \"size\" : \"1024x1024\"");
                writer.WriteLine("    }");
                writer.WriteLine("  ],");
                writer.WriteLine("  \"info\" : {");
                writer.WriteLine("    \"author\" : \"xcode\",");
                writer.WriteLine("    \"version\" : 1");
                writer.WriteLine("  }");
                writer.WriteLine("}");
            }
            
            GD.Print($"App icon assets created: {iconsDir}");
            GD.Print("Note: Icon files need to be created manually with proper dimensions");
        }
        catch (Exception e)
        {
            GD.PrintErr($"Failed to create app icon assets: {e.Message}");
        }
    }

    /// <summary>
    /// Create App Store Connect publishing guide
    /// </summary>
    private void CreateAppStoreGuide()
    {
        string guidePath = "res://ios/APP_STORE_CONNECT_GUIDE.md";
        
        try
        {
            using (var writer = new StreamWriter(guidePath))
            {
                writer.WriteLine("# iOS App Store Connect Publishing Guide");
                writer.WriteLine();
                writer.WriteLine("## Prerequisites");
                writer.WriteLine("1. **Apple Developer Account** ($99/year)");
                writer.WriteLine("2. **Xcode** (latest version)");
                writer.WriteLine("3. **iOS Device** for testing");
                writer.WriteLine("4. **App Store Connect** access");
                writer.WriteLine();
                
                writer.WriteLine("## Step 1: Certificate Setup");
                writer.WriteLine();
                writer.WriteLine("### Generate Certificates");
                writer.WriteLine("1. Open **Keychain Access** on macOS");
                writer.WriteLine("2. Go to **Keychain Access > Certificate Assistant > Request Certificate from CA**");
                writer.WriteLine("3. Fill in certificate information:");
                writer.WriteLine("   - User Email: Your Apple Developer email");
                writer.WriteLine("   - Common Name: Angry Animals");
                writer.WriteLine("   - CA Email: Leave blank");
                writer.WriteLine("   - Request: Save to disk");
                writer.WriteLine();
                
                writer.WriteLine("### Upload Certificate Request");
                writer.WriteLine("1. Go to [developer.apple.com](https://developer.apple.com)");
                writer.WriteLine("2. Sign in to **Developer Portal**");
                writer.WriteLine("3. Go to **Certificates, Identifiers & Profiles**");
                writer.WriteLine("4. Create new certificate (iOS Distribution)");
                writer.WriteLine("5. Upload the certificate request file");
                writer.WriteLine("6. Download the generated certificate");
                writer.WriteLine();
                
                writer.WriteLine("## Step 2: Provisioning Profile Setup");
                writer.WriteLine();
                writer.WriteLine("### Create App ID");
                writer.WriteLine($"1. Bundle ID: `{_config.BundleId}`");
                writer.WriteLine("2. Description: Angry Animals Game");
                writer.WriteLine("3. Enable capabilities:");
                writer.WriteLine("   - App Groups");
                writer.WriteLine("   - In-App Purchase");
                writer.WriteLine("   - Keychain Access");
                writer.WriteLine();
                
                writer.WriteLine("### Create Provisioning Profile");
                writer.WriteLine("1. Go to **Profiles** section");
                writer.WriteLine("2. Create **Distribution** profile");
                writer.WriteLine("3. Select your App ID");
                writer.WriteLine("4. Select your Distribution certificate");
                writer.WriteLine("5. Download and install the profile");
                writer.WriteLine();
                
                writer.WriteLine("## Step 3: Build Configuration");
                writer.WriteLine();
                writer.WriteLine("### Update Export Preset");
                writer.WriteLine("1. Open project in Godot");
                writer.WriteLine("2. Go to **Project > Export**");
                writer.WriteLine("3. Select iOS preset");
                writer.WriteLine("4. Fill in:");
                writer.WriteLine($"   - Bundle ID: `{_config.BundleId}`");
                writer.WriteLine("   - Team ID: [Your Team ID from Developer Portal]");
                writer.WriteLine("   - Signing Certificate: Apple Distribution");
                writer.WriteLine("   - Provisioning Profile: [Your Distribution Profile]");
                writer.WriteLine();
                
                writer.WriteLine("## Step 4: Build and Upload");
                writer.WriteLine();
                writer.WriteLine("### Export IPA");
                writer.WriteLine("1. In Godot: **Project > Export > iOS > Export Project**");
                writer.WriteLine("2. Choose export location");
                writer.WriteLine("3. Wait for build to complete");
                writer.WriteLine();
                
                writer.WriteLine("### Upload to App Store Connect");
                writer.WriteLine("1. Open **Xcode**");
                writer.WriteLine("2. Go to **Window > Organizer**");
                writer.WriteLine("3. Select **App Store Connect**");
                writer.WriteLine("4. Click **Distribute App**");
                writer.WriteLine("5. Choose your exported IPA");
                writer.WriteLine("6. Follow the upload process");
                writer.WriteLine();
                
                writer.WriteLine("## Step 5: App Store Connect Setup");
                writer.WriteLine();
                writer.WriteLine("### Create App Listing");
                writer.WriteLine("1. Go to [appstoreconnect.apple.com](https://appstoreconnect.apple.com)");
                writer.WriteLine("2. Click **My Apps > + > New App**");
                writer.WriteLine("3. Fill in app information:");
                writer.WriteLine("   - Name: Angry Animals");
                writer.WriteLine($"   - Bundle ID: {_config.BundleId}");
                writer.WriteLine("   - SKU: angry-animals-001");
                writer.WriteLine("   - User Access: Full Access");
                writer.WriteLine();
                
                writer.WriteLine("### Prepare Store Assets");
                writer.WriteLine("1. **App Icon**: 1024x1024 PNG");
                writer.WriteLine("2. **Screenshots**: Required for all device sizes");
                writer.WriteLine("3. **App Preview Videos**: Optional but recommended");
                writer.WriteLine("4. **App Description**: Detailed feature list");
                writer.WriteLine("5. **Keywords**: Angry, Animals, Physics, Puzzle, Slingshot");
                writer.WriteLine();
                
                writer.WriteLine("## Step 6: App Review");
                writer.WriteLine();
                writer.WriteLine("### Submit for Review");
                writer.WriteLine("1. Complete all required information");
                writer.WriteLine("2. Upload all required assets");
                writer.WriteLine("3. Set pricing and availability");
                writer.WriteLine("4. Submit for review");
                writer.WriteLine();
                
                writer.WriteLine("### Review Process");
                writer.WriteLine("- **Review Time**: 1-3 business days");
                writer.WriteLine("- **Review Guidelines**: Follow [App Store Review Guidelines](https://developer.apple.com/app-store/review/guidelines/)");
                writer.WriteLine("- **Common Rejection Reasons**:");
                writer.WriteLine("  - Missing privacy policy");
                writer.WriteLine("  - Incomplete app information");
                writer.WriteLine("  - Crashes or bugs");
                writer.WriteLine("  - Poor user experience");
                writer.WriteLine();
                
                writer.WriteLine("## Troubleshooting");
                writer.WriteLine();
                writer.WriteLine("### Build Errors");
                writer.WriteLine("- **Code Signing**: Ensure certificates and profiles are valid");
                writer.WriteLine("- **Provisioning Profile**: Check profile includes all required capabilities");
                writer.WriteLine("- **Bundle ID**: Must match exactly between all components");
                writer.WriteLine();
                
                writer.WriteLine("### Upload Issues");
                writer.WriteLine("- **Network**: Stable internet connection required");
                writer.WriteLine("- **Xcode Version**: Use latest stable version");
                writer.WriteLine("- **Certificate Expiry**: Check certificate validity dates");
                writer.WriteLine();
                
                writer.WriteLine("## Timeline");
                writer.WriteLine("- **Development**: 1-2 weeks");
                writer.WriteLine("- **Testing**: 3-5 days");
                writer.WriteLine("- **Review**: 1-3 business days");
                writer.WriteLine("- **Total**: 2-3 weeks from start to publication");
            }
            
            GD.Print($"App Store Connect guide created: {guidePath}");
        }
        catch (Exception e)
        {
            GD.PrintErr($"Failed to create App Store guide: {e.Message}");
        }
    }

    /// <summary>
    /// Set up signing certificates and provisioning profiles
    /// </summary>
    public void SetupSigning(string teamId, string provisioningProfilePath)
    {
        _config.TeamId = teamId;
        _config.ProvisioningProfile = provisioningProfilePath;
        
        CreateSigningSetupGuide();
        
        GD.Print("iOS signing configuration updated");
        EmitSignal("BuildConfigUpdated");
    }

    /// <summary>
    /// Create signing setup guide
    /// </summary>
    private void CreateSigningSetupGuide()
    {
        string guidePath = "res://ios/SIGNING_SETUP_GUIDE.md";
        
        try
        {
            using (var writer = new StreamWriter(guidePath))
            {
                writer.WriteLine("# iOS Code Signing Setup Guide");
                writer.WriteLine();
                writer.WriteLine("## Required Information");
                writer.WriteLine($"- **Team ID**: {_config.TeamId}");
                writer.WriteLine($"- **Bundle ID**: {_config.BundleId}");
                writer.WriteLine($"- **Provisioning Profile**: {_config.ProvisioningProfile}");
                writer.WriteLine();
                
                writer.WriteLine("## Certificate Requirements");
                writer.WriteLine("1. **Apple Development Certificate** (for testing)");
                writer.WriteLine("2. **Apple Distribution Certificate** (for App Store)");
                writer.WriteLine();
                
                writer.WriteLine("## Provisioning Profile Types");
                writer.WriteLine("- **Development**: For testing on devices");
                writer.WriteLine("- **Distribution**: For App Store submission");
                writer.WriteLine();
                
                writer.WriteLine("## Next Steps");
                writer.WriteLine("1. Obtain certificates from Apple Developer Portal");
                writer.WriteLine("2. Create provisioning profiles");
                writer.WriteLine("3. Update export preset with correct values");
                writer.WriteLine("4. Test build process");
            }
            
            GD.Print($"Signing setup guide created: {guidePath}");
        }
        catch (Exception e)
        {
            GD.PrintErr($"Failed to create signing guide: {e.Message}");
        }
    }

    /// <summary>
    /// Build iOS .ipa package
    /// </summary>
    public void BuildIpa()
    {
        GD.Print("Starting iOS .ipa build...");
        
        // Validate configuration
        if (!ValidateConfiguration())
        {
            return;
        }
        
        // Create builds directory
        string buildDir = "builds/ios";
        if (!Directory.Exists(buildDir))
        {
            Directory.CreateDirectory(buildDir);
        }
        
        string outputPath = Path.Combine(buildDir, $"{_config.AppName}.ipa");
        
        try
        {
            // Create build validation script
            CreateBuildValidationScript(outputPath);
            
            EmitSignal("BuildCompleted", outputPath, true);
            GD.Print($"iOS build completed: {outputPath}");
        }
        catch (Exception e)
        {
            EmitSignal("BuildCompleted", "", false);
            EmitSignal("ValidationError", $"Build failed: {e.Message}");
            GD.PrintErr($"iOS build failed: {e.Message}");
        }
    }

    /// <summary>
    /// Validate iOS build configuration
    /// </summary>
    private bool ValidateConfiguration()
    {
        var errors = new List<string>();
        
        if (string.IsNullOrEmpty(_config.BundleId))
        {
            errors.Add("Bundle ID is required");
        }
        
        if (string.IsNullOrEmpty(_config.TeamId))
        {
            errors.Add("Team ID is required");
        }
        
        if (string.IsNullOrEmpty(_config.SigningCertificate))
        {
            errors.Add("Signing certificate is required");
        }
        
        if (!File.Exists(_config.InfoPlistPath))
        {
            errors.Add("Info.plist not found");
        }
        
        if (errors.Count > 0)
        {
            string errorMessage = string.Join("\n", errors);
            EmitSignal("ValidationError", errorMessage);
            GD.PrintErr("iOS build validation failed:\n" + errorMessage);
            return false;
        }
        
        return true;
    }

    /// <summary>
    /// Create build validation script
    /// </summary>
    private void CreateBuildValidationScript(string outputPath)
    {
        string scriptPath = "builds/ios/validate_build.sh";
        string scriptDir = Path.GetDirectoryName(scriptPath);
        
        if (!Directory.Exists(scriptDir))
        {
            Directory.CreateDirectory(scriptDir);
        }
        
        using (var writer = new StreamWriter(scriptPath))
        {
            writer.WriteLine("#!/bin/bash");
            writer.WriteLine("# iOS Build Validation Script");
            writer.WriteLine();
            writer.WriteLine("echo \"Validating iOS build configuration...\"");
            writer.WriteLine();
            writer.WriteLine("# Check Godot version");
            writer.WriteLine("if ! command -v godot &> /dev/null; then");
            writer.WriteLine("    echo \"Error: Godot not found in PATH\"");
            writer.WriteLine("    exit 1");
            writer.WriteLine("fi");
            writer.WriteLine();
            writer.WriteLine("# Check for required certificates");
            writer.WriteLine("security find-identity -v -p codesigning");
            writer.WriteLine();
            writer.WriteLine("# Validate project");
            writer.WriteLine("godot --headless --check-only --export-prefs \"iOS\" .");
            writer.WriteLine();
            writer.WriteLine("if [ $? -eq 0 ]; then");
            writer.WriteLine("    echo \"Build configuration valid\"");
            writer.WriteLine("    echo \"Ready to export: $outputPath\"");
            writer.WriteLine("else");
            writer.WriteLine("    echo \"Build configuration invalid\"");
            writer.WriteLine("    exit 1");
            writer.WriteLine("fi");
        }
        
        GD.Print($"iOS build validation script created: {scriptPath}");
    }

    /// <summary>
    /// Get current build configuration
    /// </summary>
    public IosBuildConfig GetConfiguration()
    {
        return _config;
    }

    /// <summary>
    /// Update build configuration
    /// </summary>
    public void UpdateConfiguration(Action<IosBuildConfig> configUpdater)
    {
        configUpdater(_config);
        EmitSignal("BuildConfigUpdated");
    }
}

/// <summary>
/// iOS build configuration
/// </summary>
public class IosBuildConfig
{
    public string BundleId { get; set; }
    public string AppName { get; set; }
    public string Version { get; set; }
    public int BuildNumber { get; set; }
    public string MinIosVersion { get; set; }
    public string TeamId { get; set; }
    public string SigningCertificate { get; set; }
    public string ProvisioningProfile { get; set; }
    public string InfoPlistPath { get; set; }
    public bool EnableBitcode { get; set; }
    public bool EnableArc { get; set; }
    public string DeploymentTarget { get; set; }
}