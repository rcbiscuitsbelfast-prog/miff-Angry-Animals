using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// CosmeticOverlay.cs - Visual customization system for NPCs
/// 
/// This script manages cosmetic overlays like moustaches, glasses, hats, etc.
/// Simple system that non-coders can use to customize NPC appearance.
/// 
/// HOW TO USE (Non-Coders):
/// 1. Select NPC node in scene
/// 2. Inspector → cosmetic_overlays → Add Element
/// 3. Choose cosmetic from dropdown
/// 4. Save and test
/// 
/// For detailed guide, see: Docs/GUIDES/COSMETIC_CUSTOMIZATION.md
/// </summary>
public partial class CosmeticOverlay : Node2D
{
    // ========== PUBLIC PROPERTIES (Inspector settings) ==========
    
    /// <summary>List of cosmetics to display on this NPC</summary>
    [Export] public List<string> cosmetic_overlays = new();
    
    /// <summary>Scale of cosmetic overlays (0.5 = half size, 2.0 = double size)</summary>
    [Export] public float cosmetic_scale = 1.0f;
    
    /// <summary>Position offset for cosmetics (x, y pixels)</summary>
    [Export] public Vector2 cosmetic_offset = Vector2.Zero;
    
    // ========== ENUM (Add new cosmetics here) ==========
    
    public enum CosmeticType
    {
        moustache,        // Fatherly appearance
        glasses,          // Smart/nerdy look
        academic_hat,     // Graduation cap
        military_helmet, // Military style
        crown,           // Authority figure
        beanie,          // Casual hat
        bandana,         // Rebel attitude
        scarf,           // Weather protection
        pirate_hat,      // Special character
        bow_tie,         // Formal appearance
        apron,           // Homemaker look
        police_cap,      // Law enforcement
        alien_antenna,   // Sci-fi character
        mortarboard,     // Academic formal
        cap              // Baseball cap
    }
    
    // ========== PRIVATE PROPERTIES ==========
    
    private Sprite2D _mainSprite;
    private AnimatedSprite2D _animatedSprite;
    private Dictionary<string, Sprite2D> _cosmeticSprites = new();
    private Texture2D _defaultTexture;
    
    // ========== PUBLIC METHODS ==========
    
    /// <summary>
    /// Initialize cosmetic system
    /// YOU WILL NOT CALL THIS (engine calls it automatically)
    /// </summary>
    public override void _Ready()
    {
        InitializeOverlay();
        ApplyCosmetics();
    }
    
    /// <summary>
    /// Add a cosmetic to this NPC
    /// YOU CAN CALL THIS from code if needed
    /// </summary>
    public void AddCosmetic(string cosmeticName)
    {
        if (!cosmetic_overlays.Contains(cosmeticName))
        {
            cosmetic_overlays.Add(cosmeticName);
            ApplyCosmetic(cosmeticName);
        }
    }
    
    /// <summary>
    /// Remove a cosmetic from this NPC
    /// YOU CAN CALL THIS from code if needed
    /// </summary>
    public void RemoveCosmetic(string cosmeticName)
    {
        if (cosmetic_overlays.Contains(cosmeticName))
        {
            cosmetic_overlays.Remove(cosmeticName);
            RemoveCosmeticSprite(cosmeticName);
        }
    }
    
    /// <summary>
    /// Clear all cosmetics
    /// YOU CAN CALL THIS from code if needed
    /// </summary>
    public void ClearAllCosmetics()
    {
        cosmetic_overlays.Clear();
        ClearAllCosmeticSprites();
    }
    
    /// <summary>
    /// Get list of available cosmetics
    /// YOU CAN CALL THIS to see what's possible
    /// </summary>
    public string[] GetAvailableCosmetics()
    {
        return Enum.GetNames(typeof(CosmeticType));
    }
    
    // ========== PRIVATE METHODS ==========
    
    private void InitializeOverlay()
    {
        // Get references to sprite nodes
        _mainSprite = GetNodeOrNull<Sprite2D>("CosmeticSprite");
        
        // Setup main sprite if available
        if (_mainSprite != null)
        {
            _defaultTexture = _mainSprite.Texture;
        }
    }
    
    private void ApplyCosmetics()
    {
        // Clear existing cosmetics
        ClearAllCosmeticSprites();
        
        // Apply each cosmetic in the list
        foreach (string cosmetic in cosmetic_overlays)
        {
            ApplyCosmetic(cosmetic);
        }
    }
    
    private void ApplyCosmetic(string cosmeticName)
    {
        try
        {
            // Try to parse as enum
            if (Enum.TryParse<CosmeticType>(cosmeticName, true, out CosmeticType cosmeticType))
            {
                CreateCosmeticSprite(cosmeticType);
            }
            else
            {
                // Try to load as custom texture
                CreateCustomCosmeticSprite(cosmeticName);
            }
        }
        catch (Exception e)
        {
            GD.PrintErr($"CosmeticOverlay: Failed to apply cosmetic '{cosmeticName}': {e.Message}");
        }
    }
    
    private void CreateCosmeticSprite(CosmeticType cosmeticType)
    {
        var sprite = new Sprite2D();
        sprite.Name = cosmeticType.ToString();
        
        // Load the cosmetic texture based on enum value
        switch (cosmeticType)
        {
            case CosmeticType.moustache:
                sprite.Texture = LoadCosmeticTexture("moustache");
                sprite.Position = new Vector2(0, -5); // Face level
                break;
                
            case CosmeticType.glasses:
                sprite.Texture = LoadCosmeticTexture("glasses");
                sprite.Position = new Vector2(0, -8); // Eye level
                break;
                
            case CosmeticType.academic_hat:
                sprite.Texture = LoadCosmeticTexture("academic_hat");
                sprite.Position = new Vector2(0, -25); // Top of head
                break;
                
            case CosmeticType.military_helmet:
                sprite.Texture = LoadCosmeticTexture("military_helmet");
                sprite.Position = new Vector2(0, -20); // Head top
                break;
                
            case CosmeticType.crown:
                sprite.Texture = LoadCosmeticTexture("crown");
                sprite.Position = new Vector2(0, -22); // Above head
                break;
                
            case CosmeticType.beanie:
                sprite.Texture = LoadCosmeticTexture("beanie");
                sprite.Position = new Vector2(0, -18); // Head top
                break;
                
            case CosmeticType.bandana:
                sprite.Texture = LoadCosmeticTexture("bandana");
                sprite.Position = new Vector2(0, -12); // Forehead
                break;
                
            case CosmeticType.scarf:
                sprite.Texture = LoadCosmeticTexture("scarf");
                sprite.Position = new Vector2(0, 5); // Neck level
                break;
                
            case CosmeticType.pirate_hat:
                sprite.Texture = LoadCosmeticTexture("pirate_hat");
                sprite.Position = new Vector2(0, -20); // Head top
                break;
                
            case CosmeticType.bow_tie:
                sprite.Texture = LoadCosmeticTexture("bow_tie");
                sprite.Position = new Vector2(0, 10); // Neck level
                break;
                
            case CosmeticType.apron:
                sprite.Texture = LoadCosmeticTexture("apron");
                sprite.Position = new Vector2(0, 15); // Body level
                break;
                
            case CosmeticType.police_cap:
                sprite.Texture = LoadCosmeticTexture("police_cap");
                sprite.Position = new Vector2(0, -18); // Head top
                break;
                
            case CosmeticType.alien_antenna:
                sprite.Texture = LoadCosmeticTexture("alien_antenna");
                sprite.Position = new Vector2(0, -25); // Above head
                break;
                
            case CosmeticType.mortarboard:
                sprite.Texture = LoadCosmeticTexture("mortarboard");
                sprite.Position = new Vector2(0, -25); // Head top
                break;
                
            case CosmeticType.cap:
                sprite.Texture = LoadCosmeticTexture("cap");
                sprite.Position = new Vector2(0, -18); // Head top
                break;
                
            default:
                GD.PrintWarn($"CosmeticOverlay: Unknown cosmetic type: {cosmeticType}");
                return;
        }
        
        // Apply scale and offset
        sprite.Scale = Vector2.One * cosmetic_scale;
        sprite.Position += cosmetic_offset;
        
        // Add to parent
        AddChild(sprite);
        _cosmeticSprites[cosmeticType.ToString()] = sprite;
        
        GD.Print($"CosmeticOverlay: Applied {cosmeticType} cosmetic");
    }
    
    private void CreateCustomCosmeticSprite(string cosmeticName)
    {
        // Try to load a custom texture with this name
        var texture = LoadCosmeticTexture(cosmeticName);
        if (texture != null)
        {
            var sprite = new Sprite2D();
            sprite.Name = cosmeticName;
            sprite.Texture = texture;
            sprite.Position = cosmetic_offset;
            sprite.Scale = Vector2.One * cosmetic_scale;
            
            AddChild(sprite);
            _cosmeticSprites[cosmeticName] = sprite;
            
            GD.Print($"CosmeticOverlay: Applied custom cosmetic '{cosmeticName}'");
        }
        else
        {
            GD.PrintWarn($"CosmeticOverlay: Could not load cosmetic texture '{cosmeticName}'");
        }
    }
    
    private Texture2D LoadCosmeticTexture(string cosmeticName)
    {
        // Try multiple possible paths for the cosmetic
        string[] possiblePaths = {
            $"res://Assets/Sprites/Cosmetics/{cosmeticName}.png",
            $"res://Assets/Sprites/Cosmetics/{cosmeticName.ToLower()}.png",
            $"res://Assets/Sprites/Cosmetics/{cosmeticName.ToUpper()}.png"
        };
        
        foreach (string path in possiblePaths)
        {
            if (ResourceLoader.Exists(path))
            {
                return GD.Load<Texture2D>(path);
            }
        }
        
        // Return placeholder if no texture found
        return CreatePlaceholderTexture(cosmeticName);
    }
    
    private Texture2D CreatePlaceholderTexture(string cosmeticName)
    {
        // Create a simple placeholder texture for missing cosmetics
        var image = new Image();
        image.Create(32, 32, false, Image.Format.Rgba8);
        image.Fill(new Color(0.5f, 0.5f, 0.5f, 0.5f)); // Gray semi-transparent
        
        // Draw cosmetic name as text (simplified)
        var font = ThemeDB.FallbackFont;
        var fontSize = 8;
        var textSize = font.GetStringSize(cosmeticName, HORIZONTAL_ALIGNMENT_LEFT, -1, fontSize);
        var textPos = new Vector2((32 - textSize.X) / 2, (32 - textSize.Y) / 2);
        
        // Note: In a real implementation, you'd use draw_string with the font
        // For now, just return a simple colored texture
        
        return ImageTexture.CreateFromImage(image);
    }
    
    private void RemoveCosmeticSprite(string cosmeticName)
    {
        if (_cosmeticSprites.TryGetValue(cosmeticName, out Sprite2D sprite))
        {
            sprite.QueueFree();
            _cosmeticSprites.Remove(cosmeticName);
            GD.Print($"CosmeticOverlay: Removed {cosmeticName} cosmetic");
        }
    }
    
    private void ClearAllCosmeticSprites()
    {
        foreach (var sprite in _cosmeticSprites.Values)
        {
            sprite.QueueFree();
        }
        _cosmeticSprites.Clear();
    }
    
    // ========== UTILITY METHODS ==========
    
    /// <summary>
    /// Check if this NPC has a specific cosmetic
    /// </summary>
    public bool HasCosmetic(string cosmeticName)
    {
        return cosmetic_overlays.Contains(cosmeticName);
    }
    
    /// <summary>
    /// Get the number of cosmetics currently applied
    /// </summary>
    public int GetCosmeticCount()
    {
        return cosmetic_overlays.Count;
    }
    
    /// <summary>
    /// Get list of currently applied cosmetics
    /// </summary>
    public List<string> GetCurrentCosmetics()
    {
        return new List<string>(cosmetic_overlays);
    }
}