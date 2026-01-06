using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

/// <summary>
/// Editor utility for auditing assets and generating missing asset reports
/// Scans project for ColorRect placeholders and identifies asset requirements
/// </summary>
public class AssetValidationTool : Node
{
    public static AssetValidationTool Instance { get; private set; }

    // Validation results
    private List<PlaceholderInfo> _foundPlaceholders = new List<PlaceholderInfo>();
    private Dictionary<string, AssetRequirement> _assetRequirements = new Dictionary<string, AssetRequirement>();
    
    [Signal]
    public delegate void ValidationStartedEventHandler();
    
    [Signal]
    public delegate void ValidationProgressEventHandler(string currentItem, int completed, int total);
    
    [Signal]
    public delegate void ValidationCompleteEventHandler(ValidationReport report);
    
    [Signal]
    public delegate void AssetRequirementFoundEventHandler(string nodeName, string expectedAsset, AssetPriority priority);

    public override void _Ready()
    {
        if (Instance != null)
        {
            QueueFree();
            return;
        }
        
        Instance = this;
        GD.Print("Asset Validation Tool ready");
    }

    /// <summary>
    /// Run complete validation of the project for placeholder nodes and missing assets
    /// </summary>
    public void RunFullValidation()
    {
        EmitSignal("ValidationStarted");
        
        // Clear previous results
        _foundPlaceholders.Clear();
        _assetRequirements.Clear();
        
        GD.Print("Starting asset validation...");
        
        // Get all scene files in the project
        var sceneFiles = GetAllSceneFiles();
        int totalFiles = sceneFiles.Count;
        int completed = 0;
        
        foreach (var scenePath in sceneFiles)
        {
            EmitSignal("ValidationProgress", $"Scanning {scenePath.GetFile()}", completed, totalFiles);
            ValidateScene(scenePath);
            completed++;
        }
        
        // Generate final report
        var report = GenerateValidationReport();
        
        GD.Print($"Validation complete: Found {_foundPlaceholders.Count} placeholders, {_assetRequirements.Count} asset requirements");
        
        EmitSignal("ValidationComplete", report);
    }

    /// <summary>
    /// Get all .tscn files in the project
    /// </summary>
    private List<string> GetAllSceneFiles()
    {
        var sceneFiles = new List<string>();
        var directoriesToScan = new Queue<string>();
        
        // Start with the root directory
        directoriesToScan.Enqueue("res://");
        
        while (directoriesToScan.Count > 0)
        {
            string currentDir = directoriesToScan.Dequeue();
            
            try
            {
                var dir = Dir.Open(currentDir);
                if (dir != null)
                {
                    dir.ListDirBegin();
                    
                    string fileName = dir.GetNext();
                    while (fileName != "")
                    {
                        if (fileName.StartsWith("."))
                        {
                            fileName = dir.GetNext();
                            continue;
                        }
                        
                        string fullPath = currentDir.PathJoin(fileName);
                        
                        if (Dir.DirExistsAbsolute(fullPath))
                        {
                            // Recursively scan subdirectories
                            directoriesToScan.Enqueue(fullPath);
                        }
                        else if (fileName.EndsWith(".tscn"))
                        {
                            sceneFiles.Add(fullPath);
                        }
                        
                        fileName = dir.GetNext();
                    }
                    
                    dir.ListDirEnd();
                }
            }
            catch (Exception e)
            {
                GD.PrintErr($"Failed to scan directory {currentDir}: {e.Message}");
            }
        }
        
        return sceneFiles;
    }

    /// <summary>
    /// Validate a single scene file for placeholder nodes
    /// </summary>
    private void ValidateScene(string scenePath)
    {
        try
        {
            var packedScene = ResourceLoader.Load<PackedScene>(scenePath);
            if (packedScene == null) return;
            
            var scene = packedScene.Instantiate();
            ValidateNodeTree(scene, scenePath);
            scene.QueueFree();
        }
        catch (Exception e)
        {
            GD.PrintErr($"Failed to validate scene {scenePath}: {e.Message}");
        }
    }

    /// <summary>
    /// Recursively validate node tree for placeholders
    /// </summary>
    private void ValidateNodeTree(Node node, string scenePath)
    {
        // Check current node for placeholder indicators
        CheckNodeForPlaceholders(node, scenePath);
        
        // Recursively check children
        foreach (Node child in node.GetChildren())
        {
            ValidateNodeTree(child, scenePath);
        }
    }

    /// <summary>
    /// Check a node for placeholder characteristics
    /// </summary>
    private void CheckNodeForPlaceholders(Node node, string scenePath)
    {
        // Check for ColorRect nodes (common placeholder type)
        if (node is ColorRect colorRect)
        {
            var placeholder = new PlaceholderInfo
            {
                NodeName = node.Name,
                NodeType = "ColorRect",
                ScenePath = scenePath,
                Position = colorRect.Position,
                Size = colorRect.Size,
                Color = colorRect.Color,
                DetectedAt = DateTime.Now
            };
            
            _foundPlaceholders.Add(placeholder);
            
            // Generate asset requirement based on node name and properties
            var requirement = GenerateAssetRequirement(node, placeholder);
            if (!string.IsNullOrEmpty(requirement.ExpectedAssetPath))
            {
                _assetRequirements[requirement.NodeName] = requirement;
                EmitSignal("AssetRequirementFound", requirement.NodeName, requirement.ExpectedAssetPath, requirement.Priority);
            }
        }
        
        // Check for Sprite2D nodes with missing textures
        if (node is Sprite2D sprite)
        {
            if (sprite.Texture == null || sprite.Texture is PlaceholderTexture)
            {
                var placeholder = new PlaceholderInfo
                {
                    NodeName = node.Name,
                    NodeType = "Sprite2D (No Texture)",
                    ScenePath = scenePath,
                    Position = sprite.Position,
                    Size = sprite.Texture?.GetSize() ?? Vector2.Zero,
                    Color = Color.White,
                    DetectedAt = DateTime.Now
                };
                
                _foundPlaceholders.Add(placeholder);
                
                var requirement = GenerateAssetRequirement(node, placeholder);
                if (!string.IsNullOrEmpty(requirement.ExpectedAssetPath))
                {
                    _assetRequirements[requirement.NodeName] = requirement;
                    EmitSignal("AssetRequirementFound", requirement.NodeName, requirement.ExpectedAssetPath, requirement.Priority);
                }
            }
        }
        
        // Check for AnimatedSprite2D nodes with missing animations
        if (node is AnimatedSprite2D animatedSprite)
        {
            if (animatedSprite.SpriteFrames == null || animatedSprite.SpriteFrames.GetFrameCount() == 0)
            {
                var placeholder = new PlaceholderInfo
                {
                    NodeName = node.Name,
                    NodeType = "AnimatedSprite2D (No Frames)",
                    ScenePath = scenePath,
                    Position = animatedSprite.Position,
                    Size = Vector2.Zero,
                    Color = Color.White,
                    DetectedAt = DateTime.Now
                };
                
                _foundPlaceholders.Add(placeholder);
                
                var requirement = GenerateAssetRequirement(node, placeholder);
                if (!string.IsNullOrEmpty(requirement.ExpectedAssetPath))
                {
                    _assetRequirements[requirement.NodeName] = requirement;
                    EmitSignal("AssetRequirementFound", requirement.NodeName, requirement.ExpectedAssetPath, requirement.Priority);
                }
            }
        }
    }

    /// <summary>
    /// Generate asset requirement based on node characteristics
    /// </summary>
    private AssetRequirement GenerateAssetRequirement(Node node, PlaceholderInfo placeholder)
    {
        var requirement = new AssetRequirement
        {
            NodeName = node.Name,
            ScenePath = placeholder.ScenePath,
            ExpectedAssetPath = "",
            Priority = AssetPriority.Medium,
            EstimatedSize = 0,
            Category = DetermineCategory(node.Name)
        };

        // Determine asset path based on node name and type
        string nodeName = node.Name.ToLower();
        
        if (nodeName.Contains("projectile") || nodeName.Contains("animal"))
        {
            requirement.ExpectedAssetPath = $"res://Assets/Sprites/Projectiles/{node.Name.ToLower()}.png";
            requirement.Priority = AssetPriority.High;
            requirement.EstimatedSize = 16384; // ~16KB for 128x128 texture
        }
        else if (nodeName.Contains("cup") || nodeName.Contains("target"))
        {
            requirement.ExpectedAssetPath = $"res://Assets/Sprites/Environment/{node.Name.ToLower()}.png";
            requirement.Priority = AssetPriority.High;
            requirement.EstimatedSize = 8192; // ~8KB for 64x64 texture
        }
        else if (nodeName.Contains("slingshot"))
        {
            requirement.ExpectedAssetPath = $"res://Assets/Sprites/Slingshots/{node.Name.ToLower()}.png";
            requirement.Priority = AssetPriority.High;
            requirement.EstimatedSize = 32768; // ~32KB for detailed texture
        }
        else if (nodeName.Contains("button") || nodeName.Contains("ui"))
        {
            requirement.ExpectedAssetPath = $"res://Assets/Sprites/UI/{node.Name.ToLower()}.png";
            requirement.Priority = AssetPriority.Medium;
            requirement.EstimatedSize = 4096; // ~4KB for UI textures
        }
        else if (nodeName.Contains("background") || nodeName.Contains("bg"))
        {
            requirement.ExpectedAssetPath = $"res://Assets/Sprites/Backgrounds/{node.Name.ToLower()}.png";
            requirement.Priority = AssetPriority.Medium;
            requirement.EstimatedSize = 131072; // ~128KB for backgrounds
        }
        else if (nodeName.Contains("particle") || nodeName.Contains("effect"))
        {
            requirement.ExpectedAssetPath = $"res://Assets/Sprites/Particles/{node.Name.ToLower()}.png";
            requirement.Priority = AssetPriority.Low;
            requirement.EstimatedSize = 2048; // ~2KB for particle textures
        }
        else if (nodeName.Contains("bubble") || nodeName.Contains("speech"))
        {
            requirement.ExpectedAssetPath = $"res://Assets/Sprites/UI/{node.Name.ToLower()}.png";
            requirement.Priority = AssetPriority.Medium;
            requirement.EstimatedSize = 4096; // ~4KB for UI textures
        }
        else
        {
            // Generic fallback
            requirement.ExpectedAssetPath = $"res://Assets/Sprites/Misc/{node.Name.ToLower()}.png";
            requirement.Priority = AssetPriority.Low;
            requirement.EstimatedSize = 4096; // ~4KB default
        }

        return requirement;
    }

    /// <summary>
    /// Determine asset category based on node name
    /// </summary>
    private string DetermineCategory(string nodeName)
    {
        string name = nodeName.ToLower();
        
        if (name.Contains("projectile") || name.Contains("animal")) return "Projectiles";
        if (name.Contains("slingshot")) return "Slingshots";
        if (name.Contains("cup") || name.Contains("target") || name.Contains("environment")) return "Environment";
        if (name.Contains("ui") || name.Contains("button") || name.Contains("menu")) return "UI";
        if (name.Contains("background")) return "Backgrounds";
        if (name.Contains("particle") || name.Contains("effect")) return "Particles";
        if (name.Contains("bubble") || name.Contains("speech")) return "Speech";
        
        return "Misc";
    }

    /// <summary>
    /// Generate comprehensive validation report
    /// </summary>
    private ValidationReport GenerateValidationReport()
    {
        var report = new ValidationReport
        {
            GeneratedAt = DateTime.Now,
            TotalPlaceholders = _foundPlaceholders.Count,
            TotalRequirements = _assetRequirements.Count,
            PlaceholderDetails = _foundPlaceholders.ToList(),
            AssetRequirements = _assetRequirements.Values.ToList()
        };

        // Calculate statistics
        var priorityGroups = _assetRequirements.Values.GroupBy(req => req.Priority);
        report.RequirementsByPriority = new Dictionary<AssetPriority, int>();
        
        foreach (var group in priorityGroups)
        {
            report.RequirementsByPriority[group.Key] = group.Count();
        }

        // Calculate category breakdown
        var categoryGroups = _assetRequirements.Values.GroupBy(req => req.Category);
        report.RequirementsByCategory = new Dictionary<string, int>();
        
        foreach (var group in categoryGroups)
        {
            report.RequirementsByCategory[group.Key] = group.Count();
        }

        // Estimate total asset size needed
        report.EstimatedTotalSize = _assetRequirements.Values.Sum(req => req.EstimatedSize);

        // Generate asset folder structure
        report.RequiredFolders = GenerateRequiredFolderStructure();

        return report;
    }

    /// <summary>
    /// Generate list of required folder paths
    /// </summary>
    private List<string> GenerateRequiredFolderStructure()
    {
        var folders = new HashSet<string>();
        
        foreach (var requirement in _assetRequirements.Values)
        {
            string folder = System.IO.Path.GetDirectoryName(requirement.ExpectedAssetPath);
            if (!string.IsNullOrEmpty(folder))
            {
                folders.Add(folder);
            }
        }
        
        return folders.OrderBy(f => f).ToList();
    }

    /// <summary>
    /// Export validation report to CSV
    /// </summary>
    public void ExportToCsv(string filePath)
    {
        var report = GenerateValidationReport();
        
        using (var writer = new System.IO.StreamWriter(filePath))
        {
            writer.WriteLine("Asset Validation Report");
            writer.WriteLine($"Generated: {report.GeneratedAt}");
            writer.WriteLine($"Total Placeholders: {report.TotalPlaceholders}");
            writer.WriteLine($"Total Requirements: {report.TotalRequirements}");
            writer.WriteLine();
            
            writer.WriteLine("Asset Requirements");
            writer.WriteLine("Node Name,Expected Asset Path,Priority,Category,Estimated Size (bytes),Scene Path");
            
            foreach (var requirement in report.AssetRequirements)
            {
                writer.WriteLine($"{requirement.NodeName},{requirement.ExpectedAssetPath},{requirement.Priority},{requirement.Category},{requirement.EstimatedSize},{requirement.ScenePath}");
            }
            
            writer.WriteLine();
            writer.WriteLine("Placeholder Details");
            writer.WriteLine("Node Name,Node Type,Scene Path,Position,Size,Color");
            
            foreach (var placeholder in report.PlaceholderDetails)
            {
                writer.WriteLine($"{placeholder.NodeName},{placeholder.NodeType},{placeholder.ScenePath},{placeholder.Position},{placeholder.Size},{placeholder.Color}");
            }
        }
        
        GD.Print($"Validation report exported to: {filePath}");
    }

    /// <summary>
    /// Export validation report to JSON
    /// </summary>
    public void ExportToJson(string filePath)
    {
        var report = GenerateValidationReport();
        
        var options = new JsonSerializerOptions 
        { 
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        
        string json = JsonSerializer.Serialize(report, options);
        System.IO.File.WriteAllText(filePath, json);
        
        GD.Print($"Validation report exported to: {filePath}");
    }

    /// <summary>
    /// Create visual preview of placeholders in editor (simplified)
    /// </summary>
    public void CreatePlaceholderPreview()
    {
        var report = GenerateValidationReport();
        
        GD.Print("Placeholder Preview:");
        GD.Print($"Found {report.TotalPlaceholders} placeholders in {report.TotalPlaceholders} scenes");
        
        foreach (var placeholder in report.PlaceholderDetails.Take(10)) // Show first 10
        {
            GD.Print($"  - {placeholder.NodeName} ({placeholder.NodeType}) in {placeholder.ScenePath.GetFile()}");
        }
        
        if (report.PlaceholderDetails.Count > 10)
        {
            GD.Print($"  ... and {report.PlaceholderDetails.Count - 10} more");
        }
    }

    /// <summary>
    /// Auto-generate missing folder structure
    /// </summary>
    public void CreateMissingFolders()
    {
        var report = GenerateValidationReport();
        
        foreach (var folder in report.RequiredFolders)
        {
            if (!Dir.DirExists(folder))
            {
                Dir.MakeDirRecursive(folder);
                GD.Print($"Created folder: {folder}");
            }
        }
    }

    /// <summary>
    /// Get validation report for current scan
    /// </summary>
    public ValidationReport GetCurrentReport()
    {
        return GenerateValidationReport();
    }
}

/// <summary>
/// Placeholder node information
/// </summary>
public class PlaceholderInfo
{
    public string NodeName { get; set; }
    public string NodeType { get; set; }
    public string ScenePath { get; set; }
    public Vector2 Position { get; set; }
    public Vector2 Size { get; set; }
    public Color Color { get; set; }
    public DateTime DetectedAt { get; set; }
}

/// <summary>
/// Asset requirement specification
/// </summary>
public class AssetRequirement
{
    public string NodeName { get; set; }
    public string ScenePath { get; set; }
    public string ExpectedAssetPath { get; set; }
    public AssetPriority Priority { get; set; }
    public int EstimatedSize { get; set; }
    public string Category { get; set; }
}

/// <summary>
/// Complete validation report
/// </summary>
public class ValidationReport
{
    public DateTime GeneratedAt { get; set; }
    public int TotalPlaceholders { get; set; }
    public int TotalRequirements { get; set; }
    public List<PlaceholderInfo> PlaceholderDetails { get; set; } = new List<PlaceholderInfo>();
    public List<AssetRequirement> AssetRequirements { get; set; } = new List<AssetRequirement>();
    public Dictionary<AssetPriority, int> RequirementsByPriority { get; set; } = new Dictionary<AssetPriority, int>();
    public Dictionary<string, int> RequirementsByCategory { get; set; } = new Dictionary<string, int>();
    public long EstimatedTotalSize { get; set; }
    public List<string> RequiredFolders { get; set; } = new List<string>();
}