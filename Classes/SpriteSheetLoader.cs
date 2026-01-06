using Godot;
using System;
using System.Collections.Generic;
using System.Text.Json;

/// <summary>
/// System for loading and managing sprite sheets with multi-frame animation support
/// Handles texture atlasing and provides animated sprite wrappers
/// </summary>
public class SpriteSheetLoader : Node
{
    public static SpriteSheetLoader Instance { get; private set; }

    // Sprite sheet cache
    private Dictionary<string, SpriteSheetData> _spriteSheetCache = new Dictionary<string, SpriteSheetData>();
    
    // Animation configuration cache
    private Dictionary<string, AnimationConfig> _animationConfigCache = new Dictionary<string, AnimationConfig>();
    
    [Signal]
    public delegate void SpriteSheetLoadedEventHandler(string sheetPath, SpriteSheetData data);
    
    [Signal]
    public delegate void AnimationConfigLoadedEventHandler(string configPath, AnimationConfig config);

    public override void _Ready()
    {
        if (Instance != null)
        {
            QueueFree();
            return;
        }
        
        Instance = this;
        ProcessMode = ProcessMode.Always;
    }

    /// <summary>
    /// Load a sprite sheet and extract a specific frame
    /// </summary>
    /// <param name="sheetPath">Path to the sprite sheet image</param>
    /// <param name="frameWidth">Width of individual frames</param>
    /// <param name="frameHeight">Height of individual frames</param>
    /// <param name="frameIndex">Index of the frame to extract (0-based)</param>
    /// <returns>Texture2D of the extracted frame</returns>
    public Texture2D LoadFrame(string sheetPath, int frameWidth, int frameHeight, int frameIndex)
    {
        if (!ResourceLoader.Exists(sheetPath))
        {
            GD.PrintErr($"Sprite sheet not found: {sheetPath}");
            return CreatePlaceholderTexture(new Vector2(frameWidth, frameHeight));
        }

        var spriteSheet = GetSpriteSheet(sheetPath, frameWidth, frameHeight);
        return spriteSheet.GetFrame(frameIndex);
    }

    /// <summary>
    /// Get or create a sprite sheet data object
    /// </summary>
    private SpriteSheetData GetSpriteSheet(string sheetPath, int frameWidth, int frameHeight)
    {
        string cacheKey = $"{sheetPath}_{frameWidth}x{frameHeight}";
        
        if (_spriteSheetCache.TryGetValue(cacheKey, out SpriteSheetData cachedSheet))
        {
            return cachedSheet;
        }

        var sheet = new SpriteSheetData(sheetPath, frameWidth, frameHeight);
        _spriteSheetCache[cacheKey] = sheet;
        
        EmitSignal("SpriteSheetLoaded", sheetPath, sheet);
        return sheet;
    }

    /// <summary>
    /// Load animation configuration from JSON file
    /// </summary>
    /// <param name="configPath">Path to animation config JSON</param>
    /// <returns>Animation configuration object</returns>
    public AnimationConfig LoadAnimationConfig(string configPath)
    {
        if (_animationConfigCache.TryGetValue(configPath, out AnimationConfig cachedConfig))
        {
            return cachedConfig;
        }

        try
        {
            string jsonContent = File.ReadAllText(configPath);
            var config = JsonSerializer.Deserialize<AnimationConfig>(jsonContent);
            
            if (config != null)
            {
                _animationConfigCache[configPath] = config;
                EmitSignal("AnimationConfigLoaded", configPath, config);
                return config;
            }
        }
        catch (Exception e)
        {
            GD.PrintErr($"Failed to load animation config {configPath}: {e.Message}");
        }

        // Return default configuration
        return CreateDefaultAnimationConfig();
    }

    /// <summary>
    /// Create an AnimatedSprite2D wrapper for sprite sheet animations
    /// </summary>
    /// <param name="sheetPath">Path to sprite sheet</param>
    /// <param name="frameWidth">Frame width</param>
    /// <param name="frameHeight">Frame height</param>
    /// <param name="animationName">Name of animation to create</param>
    /// <param name="frameRange">Start and end frame indices (startIndex, endIndex)</param>
    /// <param name="fps">Frames per second for animation</param>
    /// <returns>Configured AnimatedSprite2D</returns>
    public AnimatedSprite2D CreateAnimatedSprite(string sheetPath, int frameWidth, int frameHeight, 
        string animationName, Vector2i frameRange, int fps = 10)
    {
        var animatedSprite = new AnimatedSprite2D();
        animatedSprite.Name = "AnimatedSprite_" + animationName;
        
        // Create sprite frames resource
        var frames = new SpriteFrames();
        
        // Calculate frame count
        int frameCount = frameRange.Y - frameRange.X + 1;
        
        // Load all frames for this animation
        for (int i = frameRange.X; i <= frameRange.Y; i++)
        {
            Texture2D frameTexture = LoadFrame(sheetPath, frameWidth, frameHeight, i);
            frames.AddFrame(animationName, frameTexture);
        }
        
        animatedSprite.SpriteFrames = frames;
        animatedSprite.Animation = animationName;
        animatedSprite.Frame = 0;
        animatedSprite.Speed = fps; // FPS for animation playback
        
        return animatedSprite;
    }

    /// <summary>
    /// Create a specialized projectile launch animation wrapper
    /// </summary>
    /// <param name="spriteSheetPath">Path to projectile launch spritesheet</param>
    /// <param name="frameSize">Size of each frame</param>
    /// <returns>Pre-configured AnimatedSprite2D for launch animation</returns>
    public AnimatedSprite2D CreateLaunchAnimation(string spriteSheetPath, Vector2i frameSize)
    {
        var config = LoadAnimationConfig("res://Assets/Animations/launch_animation.json");
        
        var animation = CreateAnimatedSprite(
            spriteSheetPath,
            frameSize.X,
            frameSize.Y,
            "launch",
            new Vector2i(config.StartFrame, config.EndFrame),
            config.Fps
        );
        
        return animation;
    }

    /// <summary>
    /// Create a destruction/impact animation wrapper
    /// </summary>
    /// <param name="spriteSheetPath">Path to impact spritesheet</param>
    /// <param name="frameSize">Size of each frame</param>
    /// <returns>Pre-configured AnimatedSprite2D for impact animation</returns>
    public AnimatedSprite2D CreateImpactAnimation(string spriteSheetPath, Vector2i frameSize)
    {
        var config = LoadAnimationConfig("res://Assets/Animations/impact_animation.json");
        
        var animation = CreateAnimatedSprite(
            spriteSheetPath,
            frameSize.X,
            frameSize.Y,
            "impact",
            new Vector2i(config.StartFrame, config.EndFrame),
            config.Fps
        );
        
        return animation;
    }

    /// <summary>
    /// Create a particle effect animation wrapper
    /// </summary>
    /// <param name="spriteSheetPath">Path to particle spritesheet</param>
    /// <param name="frameSize">Size of each frame</param>
    /// <param name="effectType">Type of particle effect</param>
    /// <returns>Pre-configured AnimatedSprite2D for particle effect</returns>
    public AnimatedSprite2D CreateParticleEffectAnimation(string spriteSheetPath, Vector2i frameSize, string effectType)
    {
        string configPath = $"res://Assets/Animations/particles/{effectType}_effect.json";
        var config = LoadAnimationConfig(configPath);
        
        var animation = CreateAnimatedSprite(
            spriteSheetPath,
            frameSize.X,
            frameSize.Y,
            effectType,
            new Vector2i(config.StartFrame, config.EndFrame),
            config.Fps
        );
        
        // Configure for looping if specified
        if (config.Loop)
        {
            animation.SpriteFrames.SetAnimationLoop(effectType, true);
        }
        
        return animation;
    }

    /// <summary>
    /// Create a UI element animation wrapper
    /// </summary>
    /// <param name="spriteSheetPath">Path to UI animation spritesheet</param>
    /// <param name="frameSize">Size of each frame</param>
    /// <param name="uiType">Type of UI animation (button_pulse, icon_bounce, etc.)</param>
    /// <returns>Pre-configured AnimatedSprite2D for UI animation</returns>
    public AnimatedSprite2D CreateUiAnimation(string spriteSheetPath, Vector2i frameSize, string uiType)
    {
        string configPath = $"res://Assets/Animations/ui/{uiType}_animation.json";
        var config = LoadAnimationConfig(configPath);
        
        var animation = CreateAnimatedSprite(
            spriteSheetPath,
            frameSize.X,
            frameSize.Y,
            uiType,
            new Vector2i(config.StartFrame, config.EndFrame),
            config.Fps
        );
        
        // UI animations typically loop
        if (config.Loop)
        {
            animation.SpriteFrames.SetAnimationLoop(uiType, true);
        }
        
        return animation;
    }

    /// <summary>
    /// Generate animation configuration files
    /// </summary>
    public void GenerateAnimationConfigFiles()
    {
        // Launch animation config
        var launchConfig = new AnimationConfig
        {
            Name = "launch",
            StartFrame = 0,
            EndFrame = 4,
            Fps = 12,
            Loop = false,
            Events = new Dictionary<int, string>
            {
                { 0, "on_launch_start" },
                { 2, "on_mid_flight" },
                { 4, "on_launch_complete" }
            }
        };
        
        SaveAnimationConfig("res://Assets/Animations/launch_animation.json", launchConfig);
        
        // Impact animation config
        var impactConfig = new AnimationConfig
        {
            Name = "impact",
            StartFrame = 0,
            EndFrame = 6,
            Fps = 15,
            Loop = false,
            Events = new Dictionary<int, string>
            {
                { 0, "on_impact_start" },
                { 3, "on_impact_peak" },
                { 6, "on_impact_complete" }
            }
        };
        
        SaveAnimationConfig("res://Assets/Animations/impact_animation.json", impactConfig);
        
        // Particle effects configs
        var explosionConfig = new AnimationConfig
        {
            Name = "explosion",
            StartFrame = 0,
            EndFrame = 8,
            Fps = 20,
            Loop = true
        };
        
        SaveAnimationConfig("res://Assets/Animations/particles/explosion_effect.json", explosionConfig);
        
        // UI animations configs
        var buttonPulseConfig = new AnimationConfig
        {
            Name = "button_pulse",
            StartFrame = 0,
            EndFrame = 3,
            Fps = 6,
            Loop = true
        };
        
        SaveAnimationConfig("res://Assets/Animations/ui/button_pulse_animation.json", buttonPulseConfig);
        
        GD.Print("Animation configuration files generated");
    }

    /// <summary>
    /// Save animation configuration to JSON file
    /// </summary>
    private void SaveAnimationConfig(string filePath, AnimationConfig config)
    {
        try
        {
            var options = new JsonSerializerOptions 
            { 
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            
            string json = JsonSerializer.Serialize(config, options);
            File.WriteAllText(filePath, json);
            
            GD.Print($"Animation config saved: {filePath}");
        }
        catch (Exception e)
        {
            GD.PrintErr($"Failed to save animation config {filePath}: {e.Message}");
        }
    }

    /// <summary>
    /// Create default animation configuration
    /// </summary>
    private AnimationConfig CreateDefaultAnimationConfig()
    {
        return new AnimationConfig
        {
            Name = "default",
            StartFrame = 0,
            EndFrame = 0,
            Fps = 10,
            Loop = false
        };
    }

    /// <summary>
    /// Create a placeholder texture for missing sprite sheets
    /// </summary>
    private Texture2D CreatePlaceholderTexture(Vector2 size)
    {
        var placeholder = new PlaceholderTexture();
        placeholder.Size = size;
        return placeholder;
    }

    /// <summary>
    /// Clear all caches (for development)
    /// </summary>
    public void ClearCaches()
    {
        _spriteSheetCache.Clear();
        _animationConfigCache.Clear();
        GD.Print("SpriteSheetLoader caches cleared");
    }
}

/// <summary>
/// Sprite sheet data structure for managing frame extraction
/// </summary>
public class SpriteSheetData
{
    public string SheetPath { get; set; }
    public int FrameWidth { get; set; }
    public int FrameHeight { get; set; }
    public Texture2D SourceTexture { get; set; }
    
    private int _columns;
    private int _rows;
    
    public SpriteSheetData(string sheetPath, int frameWidth, int frameHeight)
    {
        SheetPath = sheetPath;
        FrameWidth = frameWidth;
        FrameHeight = frameHeight;
        
        // Load the source texture
        SourceTexture = ResourceLoader.Load<Texture2D>(sheetPath);
        
        if (SourceTexture != null)
        {
            _columns = (int)SourceTexture.GetWidth() / frameWidth;
            _rows = (int)SourceTexture.GetHeight() / frameHeight;
        }
    }
    
    /// <summary>
    /// Get a specific frame from the sprite sheet
    /// </summary>
    public Texture2D GetFrame(int frameIndex)
    {
        if (SourceTexture == null || frameIndex < 0 || frameIndex >= _columns * _rows)
        {
            return CreatePlaceholderTexture(new Vector2(FrameWidth, FrameHeight));
        }
        
        int col = frameIndex % _columns;
        int row = frameIndex / _columns;
        
        // Create a sub-texture for the frame
        var frameTexture = new AtlasTexture();
        frameTexture.Atlas = SourceTexture;
        frameTexture.Region = new Rect2(col * FrameWidth, row * FrameHeight, FrameWidth, FrameHeight);
        
        return frameTexture;
    }
    
    /// <summary>
    /// Get total number of frames in the sprite sheet
    /// </summary>
    public int GetTotalFrames()
    {
        return _columns * _rows;
    }
    
    /// <summary>
    /// Get frame index from row and column
    /// </summary>
    public int GetFrameIndex(int row, int col)
    {
        return row * _columns + col;
    }
    
    private Texture2D CreatePlaceholderTexture(Vector2 size)
    {
        var placeholder = new PlaceholderTexture();
        placeholder.Size = size;
        return placeholder;
    }
}

/// <summary>
/// Animation configuration structure
/// </summary>
public class AnimationConfig
{
    public string Name { get; set; }
    public int StartFrame { get; set; }
    public int EndFrame { get; set; }
    public int Fps { get; set; }
    public bool Loop { get; set; }
    public Dictionary<int, string> Events { get; set; } = new Dictionary<int, string>();
}