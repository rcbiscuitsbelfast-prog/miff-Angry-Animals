using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

/// <summary>
/// Registry for mapping scene nodes to their required asset paths
/// Provides validation reports and supports hot-reloading during development
/// </summary>
public class AssetRegistry : Node
{
    private Dictionary<string, string> _assetMap = new Dictionary<string, string>();
    private string _registryPath = "res://AssetRegistry.json";
    
    public override void _Ready()
    {
        LoadRegistry();
    }

    /// <summary>
    /// Load the asset registry from JSON file
    /// </summary>
    private void LoadRegistry()
    {
        if (!File.Exists(_registryPath))
        {
            CreateDefaultRegistry();
            SaveRegistry();
        }
        
        try
        {
            string jsonContent = File.ReadAllText(_registryPath);
            var registryData = JsonSerializer.Deserialize<Dictionary<string, string>>(jsonContent);
            
            if (registryData != null)
            {
                _assetMap = registryData;
            }
            
            GD.Print($"Asset Registry loaded: {_assetMap.Count} asset mappings");
        }
        catch (Exception e)
        {
            GD.PrintErr($"Failed to load Asset Registry: {e.Message}");
            CreateDefaultRegistry();
        }
    }

    /// <summary>
    /// Create default asset mappings based on known scene structure
    /// </summary>
    private void CreateDefaultRegistry()
    {
        _assetMap = new Dictionary<string, string>
        {
            // Projectile sprites
            { "Projectile", "res://Assets/Sprites/Projectiles/projectile.png" },
            { "Projectile2", "res://Assets/Sprites/Projectiles/projectile2.png" },
            { "Projectile3", "res://Assets/Sprites/Projectiles/projectile3.png" },
            { "Projectile4", "res://Assets/Sprites/Projectiles/projectile4.png" },
            { "Projectile5", "res://Assets/Sprites/Projectiles/projectile5.png" },
            
            // Character faces
            { "FaceProjectile", "res://Assets/Sprites/Characters/face_projectile.png" },
            { "FaceProjectile2", "res://Assets/Sprites/Characters/face_projectile2.png" },
            { "FaceProjectile3", "res://Assets/Sprites/Characters/face_projectile3.png" },
            
            // Slingshot variants
            { "SlingshotBasic", "res://Assets/Sprites/Slingshots/slingshot_basic.png" },
            { "SlingshotPower", "res://Assets/Sprites/Slingshots/slingshot_power.png" },
            { "SlingshotLaser", "res://Assets/Sprites/Slingshots/slingshot_laser.png" },
            { "SlingshotExplosive", "res://Assets/Sprites/Slingshots/slingshot_explosive.png" },
            
            // Cups and targets
            { "Cup", "res://Assets/Sprites/Environment/cup.png" },
            { "Cup2", "res://Assets/Sprites/Environment/cup2.png" },
            { "Cup3", "res://Assets/Sprites/Environment/cup3.png" },
            { "Target", "res://Assets/Sprites/Environment/target.png" },
            
            // Environment props
            { "DestructibleProp", "res://Assets/Sprites/Environment/destructible.png" },
            { "Rubble", "res://Assets/Sprites/Environment/rubble.png" },
            { "Water", "res://Assets/Sprites/Environment/water.png" },
            
            // UI elements
            { "MainMenuBackground", "res://Assets/Sprites/UI/main_menu_bg.png" },
            { "LevelSelectBackground", "res://Assets/Sprites/UI/level_select_bg.png" },
            { "ButtonNormal", "res://Assets/Sprites/UI/button_normal.png" },
            { "ButtonHover", "res://Assets/Sprites/UI/button_hover.png" },
            { "ButtonPressed", "res://Assets/Sprites/UI/button_pressed.png" },
            { "ButtonDisabled", "res://Assets/Sprites/UI/button_disabled.png" },
            { "SettingsBackground", "res://Assets/Sprites/UI/settings_bg.png" },
            { "Slider", "res://Assets/Sprites/UI/slider.png" },
            { "SliderHandle", "res://Assets/Sprites/UI/slider_handle.png" },
            
            // Icons and indicators
            { "StarIcon", "res://Assets/Sprites/UI/star.png" },
            { "CoinIcon", "res://Assets/Sprites/UI/coin.png" },
            { "SettingsIcon", "res://Assets/Sprites/UI/settings_icon.png" },
            { "SoundIcon", "res://Assets/Sprites/UI/sound_icon.png" },
            { "VibrationIcon", "res://Assets/Sprites/UI/vibration_icon.png" },
            
            // Particle effects
            { "ExplosionParticles", "res://Assets/Sprites/Particles/explosion.png" },
            { "DustParticles", "res://Assets/Sprites/Particles/dust.png" },
            { "SparkleParticles", "res://Assets/Sprites/Particles/sparkle.png" },
            { "SmokeParticles", "res://Assets/Sprites/Particles/smoke.png" },
            
            // Speech bubbles and expressions
            { "SpeechBubbleHappy", "res://Assets/Sprites/UI/speech_bubble_happy.png" },
            { "SpeechBubbleAngry", "res://Assets/Sprites/UI/speech_bubble_angry.png" },
            { "SpeechBubbleSad", "res://Assets/Sprites/UI/speech_bubble_sad.png" },
            { "SpeechBubbleSurprised", "res://Assets/Sprites/UI/speech_bubble_surprised.png" },
            
            // Background elements
            { "BackgroundLevel1", "res://Assets/Sprites/Backgrounds/level1_bg.png" },
            { "BackgroundLevel2", "res://Assets/Sprites/Backgrounds/level2_bg.png" },
            { "BackgroundLevel3", "res://Assets/Sprites/Backgrounds/level3_bg.png" },
            { "BackgroundProcedural", "res://Assets/Sprites/Backgrounds/procedural_bg.png" }
        };
    }

    /// <summary>
    /// Get the asset path for a given node name
    /// </summary>
    public string GetAssetPath(string nodeName)
    {
        return _assetMap.TryGetValue(nodeName, out string path) ? path : "";
    }

    /// <summary>
    /// Get all asset mappings
    /// </summary>
    public Dictionary<string, string> GetAssetMap()
    {
        return new Dictionary<string, string>(_assetMap);
    }

    /// <summary>
    /// Add or update an asset mapping
    /// </summary>
    public void SetAssetMapping(string nodeName, string assetPath)
    {
        _assetMap[nodeName] = assetPath;
    }

    /// <summary>
    /// Remove an asset mapping
    /// </summary>
    public void RemoveAssetMapping(string nodeName)
    {
        _assetMap.Remove(nodeName);
    }

    /// <summary>
    /// Generate a validation report with asset status
    /// </summary>
    public AssetValidationReport GenerateValidationReport()
    {
        var report = new AssetValidationReport();
        report.TotalAssets = _assetMap.Count;
        report.LoadedAssets = 0;
        report.MissingAssets = new List<MissingAssetInfo>();
        report.AssetDetails = new List<AssetDetail>();
        
        foreach (var kvp in _assetMap)
        {
            string nodeName = kvp.Key;
            string assetPath = kvp.Value;
            
            bool exists = File.Exists(assetPath);
            var detail = new AssetDetail
            {
                NodeName = nodeName,
                AssetPath = assetPath,
                Exists = exists,
                Size = exists ? GetFileSize(assetPath) : 0,
                LastModified = exists ? File.GetLastWriteTime(assetPath) : DateTime.MinValue
            };
            
            report.AssetDetails.Add(detail);
            
            if (exists)
            {
                report.LoadedAssets++;
            }
            else
            {
                report.MissingAssets.Add(new MissingAssetInfo
                {
                    NodeName = nodeName,
                    AssetPath = assetPath,
                    Priority = DeterminePriority(nodeName)
                });
            }
        }
        
        report.CompletionPercentage = report.TotalAssets > 0 ? (report.LoadedAssets / (float)report.TotalAssets) * 100f : 100f;
        report.GenerationTime = DateTime.Now;
        
        return report;
    }

    /// <summary>
    /// Determine asset priority for missing asset reporting
    /// </summary>
    private AssetPriority DeterminePriority(string nodeName)
    {
        // High priority assets that affect core gameplay
        if (nodeName.Contains("Projectile") || nodeName.Contains("Slingshot") || nodeName.Contains("Cup"))
        {
            return AssetPriority.High;
        }
        
        // Medium priority assets that affect visual quality
        if (nodeName.Contains("Background") || nodeName.Contains("Speech") || nodeName.Contains("Particles"))
        {
            return AssetPriority.Medium;
        }
        
        // Low priority assets (UI elements)
        return AssetPriority.Low;
    }

    /// <summary>
    /// Get file size in bytes
    /// </summary>
    private long GetFileSize(string filePath)
    {
        try
        {
            return File.Exists(filePath) ? new FileInfo(filePath).Length : 0;
        }
        catch
        {
            return 0;
        }
    }

    /// <summary>
    /// Export validation report to CSV
    /// </summary>
    public void ExportValidationReportToCsv(string filePath)
    {
        var report = GenerateValidationReport();
        
        using (var writer = new StreamWriter(filePath))
        {
            writer.WriteLine("Node Name,Asset Path,Status,Size (bytes),Priority");
            
            foreach (var detail in report.AssetDetails)
            {
                string status = detail.Exists ? "Loaded" : "Missing";
                writer.WriteLine($"{detail.NodeName},{detail.AssetPath},{status},{detail.Size},{detail.Priority}");
            }
        }
        
        GD.Print($"Validation report exported to: {filePath}");
    }

    /// <summary>
    /// Export validation report to JSON
    /// </summary>
    public void ExportValidationReportToJson(string filePath)
    {
        var report = GenerateValidationReport();
        
        var options = new JsonSerializerOptions 
        { 
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        
        string json = JsonSerializer.Serialize(report, options);
        File.WriteAllText(filePath, json);
        
        GD.Print($"Validation report exported to: {filePath}");
    }

    /// <summary>
    /// Auto-generate folder structure for assets
    /// </summary>
    public void GenerateAssetFolderStructure()
    {
        var directories = new HashSet<string>();
        
        foreach (var assetPath in _assetMap.Values)
        {
            string directory = Path.GetDirectoryName(assetPath);
            if (!string.IsNullOrEmpty(directory))
            {
                directories.Add(directory);
            }
        }
        
        foreach (var directory in directories)
        {
            if (!Dir.Exists(directory))
            {
                Dir.MakeDirRecursive(directory);
                GD.Print($"Created directory: {directory}");
            }
        }
    }

    /// <summary>
    /// Save the current registry to file
    /// </summary>
    public void SaveRegistry()
    {
        try
        {
            var options = new JsonSerializerOptions 
            { 
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            
            string json = JsonSerializer.Serialize(_assetMap, options);
            File.WriteAllText(_registryPath, json);
            
            GD.Print($"Asset Registry saved: {_registryPath}");
        }
        catch (Exception e)
        {
            GD.PrintErr($"Failed to save Asset Registry: {e.Message}");
        }
    }

    /// <summary>
    /// Reload registry from file (for development)
    /// </summary>
    public void ReloadRegistry()
    {
        LoadRegistry();
        GD.Print("Asset Registry reloaded");
    }

    /// <summary>
    /// Get missing assets grouped by priority
    /// </summary>
    public Dictionary<AssetPriority, List<string>> GetMissingAssetsByPriority()
    {
        var grouped = new Dictionary<AssetPriority, List<string>>
        {
            { AssetPriority.High, new List<string>() },
            { AssetPriority.Medium, new List<string>() },
            { AssetPriority.Low, new List<string>() }
        };
        
        foreach (var kvp in _assetMap)
        {
            if (!File.Exists(kvp.Value))
            {
                grouped[DeterminePriority(kvp.Key)].Add(kvp.Value);
            }
        }
        
        return grouped;
    }
}

/// <summary>
/// Asset validation report data structure
/// </summary>
public class AssetValidationReport
{
    public int TotalAssets { get; set; }
    public int LoadedAssets { get; set; }
    public float CompletionPercentage { get; set; }
    public DateTime GenerationTime { get; set; }
    public List<MissingAssetInfo> MissingAssets { get; set; } = new List<MissingAssetInfo>();
    public List<AssetDetail> AssetDetails { get; set; } = new List<AssetDetail>();
}

/// <summary>
/// Missing asset information
/// </summary>
public class MissingAssetInfo
{
    public string NodeName { get; set; }
    public string AssetPath { get; set; }
    public AssetPriority Priority { get; set; }
}

/// <summary>
/// Individual asset detail
/// </summary>
public class AssetDetail
{
    public string NodeName { get; set; }
    public string AssetPath { get; set; }
    public bool Exists { get; set; }
    public long Size { get; set; }
    public DateTime LastModified { get; set; }
}

/// <summary>
/// Asset priority levels for validation reporting
/// </summary>
public enum AssetPriority
{
    Low,
    Medium,
    High
}