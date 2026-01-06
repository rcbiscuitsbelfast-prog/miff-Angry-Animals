using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

/// <summary>
/// Store listing metadata management system
/// Handles Google Play Store and Apple App Store requirements
/// </summary>
public class StoreListingManager : Node
{
    public static StoreListingManager Instance { get; private set; }

    // Store metadata
    private StoreMetadata _storeMetadata;
    private string _metadataPath = "res://store/store_metadata.json";
    
    [Signal]
    public delegate void StoreMetadataUpdatedEventHandler(string platform);
    
    [Signal]
    public delegate void StoreValidationCompleteEventHandler(string platform, bool valid, List<string> errors);

    public override void _Ready()
    {
        if (Instance != null)
        {
            QueueFree();
            return;
        }
        
        Instance = this;
        InitializeStoreMetadata();
    }

    /// <summary>
    /// Initialize store metadata with platform-specific requirements
    /// </summary>
    private void InitializeStoreMetadata()
    {
        LoadStoreMetadata();
        CreateStoreSpecificFiles();
        
        GD.Print("Store listing manager initialized");
    }

    /// <summary>
    /// Load store metadata from file
    /// </summary>
    private void LoadStoreMetadata()
    {
        if (!File.Exists(_metadataPath))
        {
            CreateDefaultMetadata();
            SaveStoreMetadata();
        }
        
        try
        {
            string jsonContent = File.ReadAllText(_metadataPath);
            _storeMetadata = JsonSerializer.Deserialize<StoreMetadata>(jsonContent) ?? CreateDefaultMetadata();
        }
        catch (Exception e)
        {
            GD.PrintErr($"Failed to load store metadata: {e.Message}");
            _storeMetadata = CreateDefaultMetadata();
        }
    }

    /// <summary>
    /// Create default store metadata
    /// </summary>
    private StoreMetadata CreateDefaultMetadata()
    {
        return new StoreMetadata
        {
            AppName = "Angry Animals",
            Subtitle = "Physics Puzzle Adventure",
            ShortDescription = "Launch adorable animals through challenging physics-based puzzles with customizable characters!",
            LongDescription = GenerateDefaultLongDescription(),
            Keywords = GenerateDefaultKeywords(),
            Category = "Games",
            ContentRating = new ContentRatingInfo
            {
                Rating = "E",
                RatingAuthority = "ESRB",
                Descriptors = new List<string>
                {
                    "Mild Cartoon Violence",
                    "Users Interact"
                }
            },
            ContactInfo = new ContactInfo
            {
                SupportEmail = "support@miffgames.com",
                SupportWebsite = "www.miffgames.com",
                PrivacyPolicyUrl = "www.miffgames.com/privacy"
            },
            PlatformMetadata = new Dictionary<string, PlatformMetadata>
            {
                ["android"] = CreateAndroidMetadata(),
                ["ios"] = CreateIosMetadata()
            },
            Features = GenerateFeatureList(),
            Screenshots = GenerateScreenshotDescriptions(),
            ReleaseNotes = GenerateReleaseNotes()
        };
    }

    /// <summary>
    /// Create Android-specific metadata
    /// </summary>
    private PlatformMetadata CreateAndroidMetadata()
    {
        return new PlatformMetadata
        {
            Platform = "android",
            PackageName = "com.miff.angryanimalsgame",
            AppId = "com.miff.angryanimalsgame",
            Title = "Angry Animals",
            ShortDescription = "Launch animals through physics puzzles!",
            FullDescription = "Physics-based puzzle game with customizable characters. 100 levels + procedural generation!",
            Keywords = "angry animals,puzzle,physics,slingshot,launcher,casual,addictive",
            Category = "GAME_PUZZLE",
            ContentRating = new ContentRatingInfo
            {
                Rating = "PEGI 3",
                RatingAuthority = "PEGI",
                Descriptors = new List<string> { "PEGI 3" }
            },
            WhatNew = "• New physics engine\n• Character customization\n• Procedural levels\n• Multiple slingshot types",
            TargetAge = "3+",
            Requirements = new List<string>
            {
                "Android 7.0 (API level 24) or higher",
                "2GB RAM minimum",
                "500MB storage space",
                "OpenGL ES 2.0 support"
            }
        };
    }

    /// <summary>
    /// Create iOS-specific metadata
    /// </summary>
    private PlatformMetadata CreateIosMetadata()
    {
        return new PlatformMetadata
        {
            Platform = "ios",
            BundleId = "com.miff.angryanimalsgame",
            AppStoreId = "",
            Title = "Angry Animals",
            Subtitle = "Physics Puzzle Adventure",
            Keywords = "angry animals,puzzle,physics,slingshot,launcher,casual",
            Category = "7002", // Games > Puzzle
            ContentRating = new ContentRatingInfo
            {
                Rating = "4+",
                RatingAuthority = "App Store",
                Descriptors = new List<string> { "Infrequent/Mild Cartoon or Realistic Violence" }
            },
            WhatNew = "New physics engine and character customization features!\n\n• 100 hand-crafted levels\n• Procedural level generation\n• Character face customization\n• Multiple slingshot variants\n• Haptic feedback",
            AgeRating = "4+",
            Requirements = new List<string>
            {
                "iOS 14.0 or later",
                "iPhone, iPad, and iPod touch",
                "2GB RAM minimum",
                "500MB storage space"
            }
        };
    }

    /// <summary>
    /// Generate default long description
    /// </summary>
    private string GenerateDefaultLongDescription()
    {
        return @"🎮 ABOUT ANGRY ANIMALS

Angry Animals is a delightful physics-based puzzle game that combines adorable characters with challenging gameplay! Launch your custom animals through increasingly difficult levels using various slingshot techniques.

🐾 KEY FEATURES

• Custom Character Creation: Take a photo to create your unique animal character!
• Physics-Based Gameplay: Realistic ballistics and destruction mechanics
• 100 Hand-Crafted Levels: Carefully designed challenges for every skill level
• Infinite Procedural Levels: Endless gameplay with AI-generated puzzles
• Multiple Slingshot Variants: Basic, Power, Laser, and Explosive launchers
• Character Expressions: Watch your animals react with speech bubbles!
• Haptic Feedback: Feel every launch and impact on supported devices
• Cross-Platform Save: Continue your progress anywhere
• Daily Challenges: Fresh puzzles every day
• Leaderboards: Compete with friends and players worldwide

🎯 GAMEPLAY

Use your finger to pull back the slingshot, aim carefully, and release to launch your animal! Watch as they fly through the air, collide with obstacles, and knock down targets. Each level presents unique challenges that require both skill and strategy to master.

🌟 CHARACTER CUSTOMIZATION

Bring your personality to the game! Use your device's camera to take a photo and see your face on your animal characters. Express yourself with different expressions that change based on your animal's mood and actions during gameplay.

🏆 PROGRESSION

Start with simple tutorials and progress through increasingly complex puzzles. Unlock new slingshot types, discover hidden mechanics, and become the ultimate animal launcher!

📱 SUPPORTED FEATURES

• iOS and Android compatibility
• Touch and mouse controls
• Cloud saves and sync
• Family sharing support
• Accessibility features

🎵 AUDIO & VISUALS

Enjoy vibrant cartoon-style graphics with smooth animations, satisfying sound effects, and dynamic particle effects that bring each launch to life!

Ready to launch into the most adorable physics puzzle adventure? Download Angry Animals today and start your journey through hundreds of challenging levels!";
    }

    /// <summary>
    /// Generate default keywords
    /// </summary>
    private List<string> GenerateDefaultKeywords()
    {
        return new List<string>
        {
            "angry animals",
            "physics puzzle",
            "slingshot game",
            "ballistic physics",
            "character customization",
            "face photo integration",
            "casual puzzle",
            "brain training",
            "addictive gameplay",
            "family friendly",
            "physics simulation",
            "projectile game",
            "launcher puzzle",
            "mobile physics",
            "procedural generation"
        };
    }

    /// <summary>
    /// Generate feature list
    /// </summary>
    private List<string> GenerateFeatureList()
    {
        return new List<string>
        {
            "🎮 100 Hand-Crafted Levels",
            "🌟 Infinite Procedural Levels",
            "📸 Face Photo Customization",
            "🏹 Multiple Slingshot Variants",
            "💬 Dynamic Speech Bubbles",
            "📳 Haptic Feedback Support",
            "☁️ Cross-Platform Save Sync",
            "🏆 Global Leaderboards",
            "🎯 Daily Challenge Puzzles",
            "🎵 Satisfying Sound Effects",
            "✨ Dynamic Particle Effects",
            "🌈 Vibrant Cartoon Graphics",
            "👨‍👩‍👧‍👦 Family-Friendly Content",
            "♿ Accessibility Features",
            "🔄 Auto-Save Progress"
        };
    }

    /// <summary>
    /// Generate screenshot descriptions
    /// </summary>
    private List<string> GenerateScreenshotDescriptions()
    {
        return new List<string>
        {
            "Main menu showing custom character with angry expression",
            "Level selection screen with 100+ levels and daily challenges",
            "Gameplay screenshot showing slingshot aiming interface",
            "Character customization using face photo integration",
            "Physics-based destruction with multiple animals launched",
            "Speech bubble expressions during gameplay",
            "Multiple slingshot variants (Basic, Power, Laser, Explosive)",
            "Procedural level generation showcase",
            "Settings menu with accessibility options",
            "Leaderboard showing global competition"
        };
    }

    /// <summary>
    /// Generate release notes
    /// </summary>
    private List<string> GenerateReleaseNotes()
    {
        return new List<string>
        {
            "Initial release of Angry Animals!",
            "100 hand-crafted levels with progressive difficulty",
            "Physics-based slingshot gameplay",
            "Character customization with face photo integration",
            "Multiple slingshot variants to unlock",
            "Procedural level generation for endless play",
            "Daily challenges for regular content",
            "Global leaderboards and achievements",
            "Haptic feedback for immersive gameplay",
            "Cross-platform save synchronization"
        };
    }

    /// <summary>
    /// Save store metadata to file
    /// </summary>
    public void SaveStoreMetadata()
    {
        try
        {
            var options = new JsonSerializerOptions 
            { 
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            
            string json = JsonSerializer.Serialize(_storeMetadata, options);
            File.WriteAllText(_metadataPath, json);
            
            GD.Print($"Store metadata saved: {_metadataPath}");
        }
        catch (Exception e)
        {
            GD.PrintErr($"Failed to save store metadata: {e.Message}");
        }
    }

    /// <summary>
    /// Create platform-specific metadata files
    /// </summary>
    private void CreateStoreSpecificFiles()
    {
        CreateGooglePlayMetadata();
        CreateAppStoreMetadata();
    }

    /// <summary>
    /// Create Google Play Store metadata file
    /// </summary>
    private void CreateGooglePlayMetadata()
    {
        string storeDir = Path.GetDirectoryName(_metadataPath);
        string playPath = Path.Combine(storeDir, "google_play_metadata.json");
        
        try
        {
            var androidMeta = _storeMetadata.PlatformMetadata["android"];
            var playMetadata = new GooglePlayMetadata
            {
                Title = androidMeta.Title,
                ShortDescription = androidMeta.ShortDescription,
                FullDescription = androidMeta.FullDescription,
                Keywords = androidMeta.Keywords,
                Category = androidMeta.Category,
                ContentRating = androidMeta.ContentRating,
                WhatNew = androidMeta.WhatNew,
                TargetAge = androidMeta.TargetAge,
                Requirements = androidMeta.Requirements,
                AppId = androidMeta.AppId,
                PackageName = androidMeta.PackageName
            };
            
            var options = new JsonSerializerOptions 
            { 
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            
            string json = JsonSerializer.Serialize(playMetadata, options);
            File.WriteAllText(playPath, json);
            
            GD.Print($"Google Play metadata created: {playPath}");
        }
        catch (Exception e)
        {
            GD.PrintErr($"Failed to create Google Play metadata: {e.Message}");
        }
    }

    /// <summary>
    /// Create Apple App Store metadata file
    /// </summary>
    private void CreateAppStoreMetadata()
    {
        string storeDir = Path.GetDirectoryName(_metadataPath);
        string appStorePath = Path.Combine(storeDir, "app_store_metadata.json");
        
        try
        {
            var iosMeta = _storeMetadata.PlatformMetadata["ios"];
            var appStoreMetadata = new AppStoreMetadata
            {
                Title = iosMeta.Title,
                Subtitle = iosMeta.Subtitle,
                Keywords = iosMeta.Keywords,
                Category = iosMeta.Category,
                ContentRating = iosMeta.ContentRating,
                WhatNew = iosMeta.WhatNew,
                AgeRating = iosMeta.AgeRating,
                Requirements = iosMeta.Requirements,
                BundleId = iosMeta.BundleId,
                AppStoreId = iosMeta.AppStoreId
            };
            
            var options = new JsonSerializerOptions 
            { 
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            
            string json = JsonSerializer.Serialize(appStoreMetadata, options);
            File.WriteAllText(appStorePath, json);
            
            GD.Print($"App Store metadata created: {appStorePath}");
        }
        catch (Exception e)
        {
            GD.PrintErr($"Failed to create App Store metadata: {e.Message}");
        }
    }

    /// <summary>
    /// Get store metadata for specific platform
    /// </summary>
    public PlatformMetadata GetPlatformMetadata(string platform)
    {
        if (_storeMetadata.PlatformMetadata.TryGetValue(platform.ToLower(), out PlatformMetadata metadata))
        {
            return metadata;
        }
        
        return null;
    }

    /// <summary>
    /// Update metadata for specific platform
    /// </summary>
    public void UpdatePlatformMetadata(string platform, Action<PlatformMetadata> updater)
    {
        if (_storeMetadata.PlatformMetadata.TryGetValue(platform.ToLower(), out PlatformMetadata metadata))
        {
            updater(metadata);
            SaveStoreMetadata();
            CreateStoreSpecificFiles();
            EmitSignal("StoreMetadataUpdated", platform);
        }
    }

    /// <summary>
    /// Validate metadata for store submission
    /// </summary>
    public void ValidateMetadata(string platform)
    {
        var errors = new List<string>();
        
        if (platform.ToLower() == "android")
        {
            ValidateAndroidMetadata(errors);
        }
        else if (platform.ToLower() == "ios")
        {
            ValidateIosMetadata(errors);
        }
        
        bool isValid = errors.Count == 0;
        EmitSignal("StoreValidationComplete", platform, isValid, errors);
        
        if (isValid)
        {
            GD.Print($"{platform} metadata validation passed");
        }
        else
        {
            GD.PrintErr($"{platform} metadata validation failed:\n" + string.Join("\n", errors));
        }
    }

    /// <summary>
    /// Validate Android-specific metadata requirements
    /// </summary>
    private void ValidateAndroidMetadata(List<string> errors)
    {
        var androidMeta = _storeMetadata.PlatformMetadata["android"];
        
        if (string.IsNullOrEmpty(androidMeta.Title) || androidMeta.Title.Length > 30)
            errors.Add("Title must be 1-30 characters");
        
        if (string.IsNullOrEmpty(androidMeta.ShortDescription) || androidMeta.ShortDescription.Length > 80)
            errors.Add("Short description must be 1-80 characters");
        
        if (string.IsNullOrEmpty(androidMeta.FullDescription) || androidMeta.FullDescription.Length > 4000)
            errors.Add("Full description must be 1-4000 characters");
        
        if (string.IsNullOrEmpty(androidMeta.PackageName) || !androidMeta.PackageName.StartsWith("com."))
            errors.Add("Package name must start with 'com.'");
        
        if (string.IsNullOrEmpty(androidMeta.ContentRating.Rating))
            errors.Add("Content rating is required");
        
        if (androidMeta.Keywords.Split(',').Length > 100)
            errors.Add("Keywords must be comma-separated with max 100 characters total");
    }

    /// <summary>
    /// Validate iOS-specific metadata requirements
    /// </summary>
    private void ValidateIosMetadata(List<string> errors)
    {
        var iosMeta = _storeMetadata.PlatformMetadata["ios"];
        
        if (string.IsNullOrEmpty(iosMeta.Title) || iosMeta.Title.Length > 30)
            errors.Add("Title must be 1-30 characters");
        
        if (string.IsNullOrEmpty(iosMeta.Subtitle) || iosMeta.Subtitle.Length > 30)
            errors.Add("Subtitle must be 1-30 characters");
        
        if (string.IsNullOrEmpty(iosMeta.Keywords) || iosMeta.Keywords.Length > 100)
            errors.Add("Keywords must be 1-100 characters");
        
        if (string.IsNullOrEmpty(iosMeta.BundleId) || !iosMeta.BundleId.Contains("."))
            errors.Add("Bundle ID must contain a dot");
        
        if (string.IsNullOrEmpty(iosMeta.Category))
            errors.Add("Category is required");
        
        if (string.IsNullOrEmpty(iosMeta.ContentRating.Rating))
            errors.Add("Content rating is required");
    }

    /// <summary>
    /// Export metadata for store submission
    /// </summary>
    public void ExportMetadata(string platform, string outputPath)
    {
        try
        {
            var metadata = GetPlatformMetadata(platform);
            if (metadata == null)
            {
                GD.PrintErr($"No metadata found for platform: {platform}");
                return;
            }
            
            var options = new JsonSerializerOptions 
            { 
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            
            string json = JsonSerializer.Serialize(metadata, options);
            File.WriteAllText(outputPath, json);
            
            GD.Print($"Metadata exported for {platform}: {outputPath}");
        }
        catch (Exception e)
        {
            GD.PrintErr($"Failed to export metadata for {platform}: {e.Message}");
        }
    }

    /// <summary>
    /// Get current store metadata
    /// </summary>
    public StoreMetadata GetStoreMetadata()
    {
        return _storeMetadata;
    }

    /// <summary>
    /// Create screenshot guide for store requirements
    /// </summary>
    public void CreateScreenshotGuide()
    {
        string guidePath = "res://store/SCREENSHOT_GUIDE.md";
        string storeDir = Path.GetDirectoryName(guidePath);
        
        if (!Directory.Exists(storeDir))
        {
            Directory.CreateDirectory(storeDir);
        }
        
        try
        {
            using (var writer = new StreamWriter(guidePath))
            {
                writer.WriteLine("# Store Screenshot Guide");
                writer.WriteLine();
                writer.WriteLine("## Google Play Store Requirements");
                writer.WriteLine();
                writer.WriteLine("### Required Screenshots");
                writer.WriteLine("- **Phone**: 1080x1920 (9:16 aspect ratio)");
                writer.WriteLine("- **7-inch Tablet**: 1200x1920 (9:16 aspect ratio)");
                writer.WriteLine("- **10-inch Tablet**: 1536x2048 (4:3 aspect ratio)");
                writer.WriteLine();
                
                writer.WriteLine("### Screenshot Guidelines");
                writer.WriteLine("- Use only in-game content");
                writer.WriteLine("- Show key features and gameplay");
                writer.WriteLine("- Include character customization");
                writer.WriteLine("- Demonstrate physics mechanics");
                writer.WriteLine("- Show progression and levels");
                writer.WriteLine();
                
                writer.WriteLine("### Recommended Screenshots");
                writer.WriteLine("1. **Main Menu** - Shows custom character");
                writer.WriteLine("2. **Gameplay** - Slingshot aiming interface");
                writer.WriteLine("3. **Character Customization** - Face photo integration");
                writer.WriteLine("4. **Physics Destruction** - Multiple animals in action");
                writer.WriteLine("5. **Level Selection** - 100+ levels showcase");
                writer.WriteLine("6. **Speech Bubbles** - Character expressions");
                writer.WriteLine("7. **Slingshot Variants** - Different launcher types");
                writer.WriteLine("8. **Procedural Levels** - Endless gameplay");
                writer.WriteLine();
                
                writer.WriteLine("## Apple App Store Requirements");
                writer.WriteLine();
                writer.WriteLine("### iPhone Screenshot Sizes");
                writer.WriteLine("- **iPhone 14 Pro Max**: 1290x2796");
                writer.WriteLine("- **iPhone 14 Pro**: 1179x2556");
                writer.WriteLine("- **iPhone 14/13**: 1170x2532");
                writer.WriteLine("- **iPhone SE**: 750x1334");
                writer.WriteLine("- **iPad Pro 12.9\"**: 2048x2732");
                writer.WriteLine("- **iPad Pro 11\"**: 1668x2388");
                writer.WriteLine();
                
                writer.WriteLine("### App Preview Video (Optional)");
                writer.WriteLine("- Duration: 15-30 seconds");
                writer.WriteLine("- Format: MP4 or MOV");
                writer.WriteLine("- Resolution: Match device screenshots");
                writer.WriteLine("- No external branding");
                writer.WriteLine("- Show actual gameplay");
                writer.WriteLine();
                
                writer.WriteLine("## Safe Zones and Guidelines");
                writer.WriteLine();
                writer.WriteLine("### Avoid Overlay Areas");
                writer.WriteLine("- Keep important content away from edges");
                writer.WriteLine("- Avoid status bar areas");
                writer.WriteLine("- Don't place text too close to corners");
                writer.WriteLine("- Leave room for UI elements");
                writer.WriteLine();
                
                writer.WriteLine("### Quality Standards");
                writer.WriteLine("- High resolution (minimum requirements listed above)");
                writer.WriteLine("- Clear, readable text");
                writer.WriteLine("- Vibrant, appealing colors");
                writer.WriteLine("- Professional appearance");
                writer.WriteLine("- No blur or pixelation");
                writer.WriteLine();
                
                writer.WriteLine("## File Naming Convention");
                writer.WriteLine();
                writer.WriteLine("### Google Play");
                writer.WriteLine("```");
                writer.WriteLine("phone_screenshot_1.png");
                writer.WriteLine("phone_screenshot_2.png");
                writer.WriteLine("seven_inch_tablet_screenshot_1.png");
                writer.WriteLine("ten_inch_tablet_screenshot_1.png");
                writer.WriteLine("```");
                writer.WriteLine();
                
                writer.WriteLine("### Apple App Store");
                writer.WriteLine("```");
                writer.WriteLine("iphone_screenshot_1.png");
                writer.WriteLine("iphone_screenshot_2.png");
                writer.WriteLine("ipad_screenshot_1.png");
                writer.WriteLine("```");
            }
            
            GD.Print($"Screenshot guide created: {guidePath}");
        }
        catch (Exception e)
        {
            GD.PrintErr($"Failed to create screenshot guide: {e.Message}");
        }
    }
}

/// <summary>
/// Store metadata data structure
/// </summary>
public class StoreMetadata
{
    public string AppName { get; set; }
    public string Subtitle { get; set; }
    public string ShortDescription { get; set; }
    public string LongDescription { get; set; }
    public List<string> Keywords { get; set; } = new List<string>();
    public string Category { get; set; }
    public ContentRatingInfo ContentRating { get; set; }
    public ContactInfo ContactInfo { get; set; }
    public Dictionary<string, PlatformMetadata> PlatformMetadata { get; set; } = new Dictionary<string, PlatformMetadata>();
    public List<string> Features { get; set; } = new List<string>();
    public List<string> Screenshots { get; set; } = new List<string>();
    public List<string> ReleaseNotes { get; set; } = new List<string>();
}

/// <summary>
/// Platform-specific metadata
/// </summary>
public class PlatformMetadata
{
    public string Platform { get; set; }
    public string Title { get; set; }
    public string Subtitle { get; set; }
    public string ShortDescription { get; set; }
    public string FullDescription { get; set; }
    public string Keywords { get; set; }
    public string Category { get; set; }
    public ContentRatingInfo ContentRating { get; set; }
    public string WhatNew { get; set; }
    public string TargetAge { get; set; }
    public string AgeRating { get; set; }
    public List<string> Requirements { get; set; } = new List<string>();
    public string PackageName { get; set; }
    public string BundleId { get; set; }
    public string AppId { get; set; }
    public string AppStoreId { get; set; }
}

/// <summary>
/// Content rating information
/// </summary>
public class ContentRatingInfo
{
    public string Rating { get; set; }
    public string RatingAuthority { get; set; }
    public List<string> Descriptors { get; set; } = new List<string>();
}

/// <summary>
/// Contact information for store listings
/// </summary>
public class ContactInfo
{
    public string SupportEmail { get; set; }
    public string SupportWebsite { get; set; }
    public string PrivacyPolicyUrl { get; set; }
}

/// <summary>
/// Google Play Store specific metadata
/// </summary>
public class GooglePlayMetadata
{
    public string Title { get; set; }
    public string ShortDescription { get; set; }
    public string FullDescription { get; set; }
    public string Keywords { get; set; }
    public string Category { get; set; }
    public ContentRatingInfo ContentRating { get; set; }
    public string WhatNew { get; set; }
    public string TargetAge { get; set; }
    public List<string> Requirements { get; set; } = new List<string>();
    public string AppId { get; set; }
    public string PackageName { get; set; }
}

/// <summary>
/// Apple App Store specific metadata
/// </summary>
public class AppStoreMetadata
{
    public string Title { get; set; }
    public string Subtitle { get; set; }
    public string Keywords { get; set; }
    public string Category { get; set; }
    public ContentRatingInfo ContentRating { get; set; }
    public string WhatNew { get; set; }
    public string AgeRating { get; set; }
    public List<string> Requirements { get; set; } = new List<string>();
    public string BundleId { get; set; }
    public string AppStoreId { get; set; }
}