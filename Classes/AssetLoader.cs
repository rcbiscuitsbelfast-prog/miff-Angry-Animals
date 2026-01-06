using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Singleton system for loading and managing game assets with fallback support
/// Automatically handles missing assets by falling back to ColorRect placeholders
/// Logs asset loading status for debugging and validation
/// </summary>
public class AssetLoader : Node
{
    public static AssetLoader Instance { get; private set; }

    // Asset cache for reusing loaded textures
    private Dictionary<string, Texture2D> _textureCache = new Dictionary<string, Texture2D>();
    
    // Asset registry mapping node names to asset paths
    private AssetRegistry _registry;
    
    // Asset validation tracking
    private Dictionary<string, bool> _assetLoadStatus = new Dictionary<string, bool>();
    private List<string> _missingAssets = new List<string>();
    
    // Signals for asset loading events
    [Signal]
    public delegate void AssetLoadedEventHandler(string assetPath, Texture2D texture);
    
    [Signal]
    public delegate void AssetFailedEventHandler(string assetPath);
    
    [Signal]
    public delegate void ValidationCompleteEventHandler(int totalAssets, int loadedAssets, float percentage);

    public override void _Ready()
    {
        if (Instance != null)
        {
            QueueFree();
            return;
        }
        
        Instance = this;
        ProcessMode = ProcessMode.Always;
        
        _registry = new AssetRegistry();
        ValidateAllAssets();
    }

    /// <summary>
    /// Load a texture from the specified path, falling back to placeholder if missing
    /// </summary>
    /// <param name="assetPath">Path to the asset (e.g., "res://Assets/Projectiles/animal1.png")</param>
    /// <param name="nodeName">Name of the node this asset is for (for placeholder creation)</param>
    /// <param name="size">Size for the placeholder ColorRect</param>
    /// <returns>Loaded Texture2D or placeholder texture</returns>
    public Texture2D LoadTexture(string assetPath, string nodeName = "", Vector2? size = null)
    {
        // Check cache first
        if (_textureCache.TryGetValue(assetPath, out Texture2D cachedTexture))
        {
            return cachedTexture;
        }

        Texture2D texture = null;
        bool assetExists = false;

        try
        {
            // Attempt to load the actual asset
            if (ResourceLoader.Exists(assetPath))
            {
                var resource = ResourceLoader.Load(assetPath);
                if (resource is Texture2D tex)
                {
                    texture = tex;
                    assetExists = true;
                    _assetLoadStatus[assetPath] = true;
                    GD.Print($"✓ Asset loaded: {assetPath}");
                }
            }
        }
        catch (Exception e)
        {
            GD.PrintErr($"Failed to load asset {assetPath}: {e.Message}");
        }

        if (!assetExists)
        {
            _missingAssets.Add(assetPath);
            _assetLoadStatus[assetPath] = false;
            texture = CreatePlaceholderTexture(nodeName, size ?? new Vector2(64, 64));
            GD.Print($"⚠ Using placeholder for missing asset: {assetPath}");
        }

        // Cache the result
        _textureCache[assetPath] = texture;
        
        if (assetExists)
        {
            EmitSignal("AssetLoaded", assetPath, texture);
        }
        else
        {
            EmitSignal("AssetFailed", assetPath);
        }

        return texture;
    }

    /// <summary>
    /// Apply loaded texture to a Sprite2D node, falling back to placeholder if needed
    /// </summary>
    public void ApplyToSprite(Sprite2D sprite, string assetPath, string nodeName = "", Vector2? size = null)
    {
        if (sprite == null) return;
        
        var texture = LoadTexture(assetPath, nodeName, size);
        sprite.Texture = texture;
        
        // If using placeholder, convert to ColorRect
        if (texture is PlaceholderTexture placeholderTex)
        {
            ConvertSpriteToPlaceholder(sprite, placeholderTex.Size, nodeName);
        }
    }

    /// <summary>
    /// Convert a Sprite2D to ColorRect when using placeholder
    /// </summary>
    private void ConvertSpriteToPlaceholder(Sprite2D sprite, Vector2 size, string nodeName)
    {
        var colorRect = new ColorRect();
        colorRect.Name = sprite.Name;
        colorRect.Position = sprite.Position;
        colorRect.Size = size;
        
        // Generate distinct colors based on node name
        Color placeholderColor = GeneratePlaceholderColor(nodeName);
        colorRect.Color = placeholderColor;
        
        // Replace in parent
        var parent = sprite.GetParent();
        int index = parent.GetChildPosition(sprite);
        parent.RemoveChild(sprite);
        parent.AddChild(colorRect);
        parent.MoveChild(colorRect, index);
        
        sprite.QueueFree();
    }

    /// <summary>
    /// Generate a consistent color based on node name for placeholders
    /// </summary>
    private Color GeneratePlaceholderColor(string nodeName)
    {
        if (string.IsNullOrEmpty(nodeName)) return Color.Gray;
        
        // Simple hash function for consistent colors
        int hash = nodeName.GetHashCode();
        float r = ((hash >> 16) & 0xFF) / 255f;
        float g = ((hash >> 8) & 0xFF) / 255f;
        float b = (hash & 0xFF) / 255f;
        
        return new Color(r, g, b, 1.0f);
    }

    /// <summary>
    /// Create a placeholder texture with node name overlay
    /// </summary>
    private Texture2D CreatePlaceholderTexture(string nodeName, Vector2 size)
    {
        var placeholder = new PlaceholderTexture();
        placeholder.Size = size;
        return placeholder;
    }

    /// <summary>
    /// Validate all registered assets and generate report
    /// </summary>
    private void ValidateAllAssets()
    {
        var assetMap = _registry.GetAssetMap();
        int totalAssets = assetMap.Count;
        int loadedAssets = 0;

        foreach (var kvp in assetMap)
        {
            string nodeName = kvp.Key;
            string assetPath = kvp.Value;
            
            // Check if asset exists without loading it
            bool exists = ResourceLoader.Exists(assetPath);
            _assetLoadStatus[assetPath] = exists;
            
            if (exists)
            {
                loadedAssets++;
            }
            else
            {
                _missingAssets.Add(assetPath);
            }
        }

        float percentage = totalAssets > 0 ? (loadedAssets / (float)totalAssets) * 100f : 100f;
        GD.Print($"Asset Validation: {loadedAssets}/{totalAssets} loaded ({percentage:F1}%)");
        
        if (_missingAssets.Count > 0)
        {
            GD.Print("Missing Assets:");
            foreach (var missing in _missingAssets)
            {
                GD.Print($"  - {missing}");
            }
        }

        EmitSignal("ValidationComplete", totalAssets, loadedAssets, percentage);
    }

    /// <summary>
    /// Get asset validation report
    /// </summary>
    public (int Total, int Loaded, float Percentage, List<string> Missing) GetValidationReport()
    {
        var assetMap = _registry.GetAssetMap();
        return (
            assetMap.Count,
            _assetLoadStatus.Values.Count(status => status),
            _assetLoadStatus.Count > 0 ? (_assetLoadStatus.Values.Count(status => status) / (float)_assetLoadStatus.Count) * 100f : 100f,
            _missingAssets.ToList()
        );
    }

    /// <summary>
    /// Reload all cached assets (for development)
    /// </summary>
    public void ReloadAllAssets()
    {
        _textureCache.Clear();
        _assetLoadStatus.Clear();
        _missingAssets.Clear();
        
        GD.Print("Asset cache cleared, reloading...");
        ValidateAllAssets();
    }

    /// <summary>
    /// Hot-reload a specific asset
    /// </summary>
    public void ReloadAsset(string assetPath)
    {
        if (_textureCache.ContainsKey(assetPath))
        {
            _textureCache.Remove(assetPath);
        }
        
        LoadTexture(assetPath);
    }

    /// <summary>
    /// Check if an asset exists without loading it
    /// </summary>
    public bool AssetExists(string assetPath)
    {
        return ResourceLoader.Exists(assetPath);
    }

    /// <summary>
    /// Get all missing assets for this session
    /// </summary>
    public List<string> GetMissingAssets()
    {
        return _missingAssets.ToList();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _textureCache.Clear();
        }
        base.Dispose(disposing);
    }
}

/// <summary>
/// Placeholder texture for missing assets
/// </summary>
public class PlaceholderTexture : Texture2D
{
    public Vector2 Size { get; set; } = new Vector2(64, 64);
    
    public override Vector2 GetSize()
    {
        return Size;
    }
    
    public override bool HasAlpha()
    {
        return false;
    }
    
    public override Image GetImage()
    {
        // Create a simple placeholder image with a checkerboard pattern
        var image = new Image();
        image.Create((int)Size.X, (int)Size.Y, false, Image.Format.Rgba8);
        
        for (int x = 0; x < Size.X; x++)
        {
            for (int y = 0; y < Size.Y; y++)
            {
                bool isChecker = ((x / 8) + (y / 8)) % 2 == 0;
                Color color = isChecker ? Color.DarkGray : Color.Gray;
                image.SetPixel(x, y, color);
            }
        }
        
        return image;
    }
}