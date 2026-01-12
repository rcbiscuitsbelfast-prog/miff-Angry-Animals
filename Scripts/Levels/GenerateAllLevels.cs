using Godot;
using System;
using System.Collections.Generic;
using System.IO;

/// <summary>
/// Batch generator for creating 100+ themed levels procedurally
/// </summary>
public static class GenerateAllLevels
{
    public const int TOTAL_LEVELS = 100;
    
    private static readonly LevelDistribution[] _levelDistribution = new[]
    {
        new LevelDistribution(ProceduralLevelGenerator.LevelTheme.School, 16, 1, 16),
        new LevelDistribution(ProceduralLevelGenerator.LevelTheme.Home, 16, 17, 32),
        new LevelDistribution(ProceduralLevelGenerator.LevelTheme.Office, 16, 33, 48),
        new LevelDistribution(ProceduralLevelGenerator.LevelTheme.Government, 16, 49, 64),
        new LevelDistribution(ProceduralLevelGenerator.LevelTheme.Kitchen, 12, 65, 76),
        new LevelDistribution(ProceduralLevelGenerator.LevelTheme.Bedroom, 12, 77, 88),
        new LevelDistribution(ProceduralLevelGenerator.LevelTheme.LivingRoom, 8, 89, 96),
        new LevelDistribution(ProceduralLevelGenerator.LevelTheme.Factory, 4, 97, 100)
    };
    
    /// <summary>
    /// Generates all 100 levels and saves them to disk
    /// </summary>
    public static void GenerateAll100Levels()
    {
        GD.Print("🚀 Starting generation of 100 themed levels...");
        
        // Create directories
        CreateLevelDirectories();
        
        int generatedCount = 0;
        var errors = new List<string>();
        
        foreach (var distribution in _levelDistribution)
        {
            GD.Print($"📦 Generating {distribution.Count} {distribution.Theme} levels (Levels {distribution.StartLevel}-{distribution.EndLevel})");
            
            for (int i = 0; i < distribution.Count; i++)
            {
                int levelNumber = distribution.StartLevel + i;
                
                try
                {
                    var level = ProceduralLevelGenerator.GenerateLevel(levelNumber, distribution.Theme);
                    SaveLevel(level);
                    generatedCount++;
                    
                    if (levelNumber % 10 == 0)
                    {
                        GD.Print($"  ✅ Generated level {levelNumber}: {level.LevelName}");
                    }
                }
                catch (Exception ex)
                {
                    string error = $"❌ Failed to generate level {levelNumber} ({distribution.Theme}): {ex.Message}";
                    GD.PushError(error);
                    errors.Add(error);
                }
            }
        }
        
        GD.Print($"\n🎉 Level generation complete!");
        GD.Print($"✅ Successfully generated: {generatedCount} levels");
        GD.Print($"❌ Errors: {errors.Count}");
        
        if (errors.Count > 0)
        {
            GD.Print("\n⚠️  Errors encountered:");
            foreach (var error in errors)
            {
                GD.Print($"   {error}");
            }
        }
        
        // Generate level manifest
        GenerateManifest();
        
        GD.Print("\n📋 Level manifest generated!");
    }
    
    /// <summary>
    /// Generates all levels and returns them in memory
    /// </summary>
    public static List<LevelMetadata> GenerateAllLevelsInMemory()
    {
        var levels = new List<LevelMetadata>();
        
        foreach (var distribution in _levelDistribution)
        {
            for (int i = 0; i < distribution.Count; i++)
            {
                int levelNumber = distribution.StartLevel + i;
                var level = ProceduralLevelGenerator.GenerateLevel(levelNumber, distribution.Theme);
                levels.Add(level);
            }
        }
        
        return levels;
    }
    
    /// <summary>
    /// Regenerates a specific level range
    /// </summary>
    public static void RegenerateLevelRange(int startLevel, int endLevel)
    {
        GD.Print($"🔄 Regenerating levels {startLevel}-{endLevel}");
        
        for (int levelNumber = startLevel; levelNumber <= endLevel; levelNumber++)
        {
            var theme = GetThemeForLevelNumber(levelNumber);
            var level = ProceduralLevelGenerator.GenerateLevel(levelNumber, theme);
            SaveLevel(level);
            
            if (levelNumber % 5 == 0)
            {
                GD.Print($"  ✅ Regenerated level {levelNumber}");
            }
        }
        
        GD.Print($"✅ Completed regenerating levels {startLevel}-{endLevel}");
    }
    
    /// <summary>
    /// Gets the theme for a specific level number based on distribution
    /// </summary>
    private static ProceduralLevelGenerator.LevelTheme GetThemeForLevelNumber(int levelNumber)
    {
        foreach (var distribution in _levelDistribution)
        {
            if (levelNumber >= distribution.StartLevel && levelNumber <= distribution.EndLevel)
            {
                return distribution.Theme;
            }
        }
        
        return ProceduralLevelGenerator.LevelTheme.Office; // Default fallback
    }
    
    /// <summary>
    /// Saves a generated level to disk
    /// </summary>
    private static void SaveLevel(LevelMetadata level)
    {
        try
        {
            string json = SerializeLevel(level);
            string filePath = GetLevelFilePath(level.LevelId);
            
            var file = FileAccess.Open(filePath, FileAccess.ModeFlags.Write);
            if (file != null)
            {
                file.StoreString(json);
                file.Close();
            }
            else
            {
                GD.PushError($"Failed to open file for writing: {filePath}");
            }
        }
        catch (Exception ex)
        {
            GD.PushError($"Error saving level {level.LevelId}: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Serializes level metadata to JSON
    /// </summary>
    private static string SerializeLevel(LevelMetadata level)
    {
        var data = new Godot.Collections.Dictionary
        {
            ["LevelId"] = level.LevelId,
            ["LevelName"] = level.LevelName,
            ["Theme"] = level.Theme,
            ["Description"] = level.Description,
            ["Difficulty"] = (int)level.Difficulty,
            ["Goal"] = level.Goal,
            ["TargetTime"] = level.TargetTime,
            ["ParScore"] = level.ParScore,
            ["CreatedTimestamp"] = level.CreatedTimestamp,
            [\"CreatorName\"] = level.CreatorName,
            [\"IsGenerated\"] = level.IsGenerated,
            [\"Items\"] = SerializeItems(level.Items)
        };
        
        return Json.Stringify(data);
    }
    
    /// <summary>
    /// Serializes item instances to an array
    /// </summary>
    private static Godot.Collections.Array SerializeItems(List<ItemInstance> items)
    {
        var array = new Godot.Collections.Array();
        
        foreach (var item in items)
        {
            var itemData = new Godot.Collections.Dictionary
            {
                [\"ItemId\"] = item.ItemId,
                [\"PositionX\"] = item.Position.X,
                [\"PositionY\"] = item.Position.Y,
                [\"Rotation\"] = item.Rotation,
                [\"Scale\"] = item.Scale,
                [\"MaterialOverride\"] = (int)item.MaterialOverride
            };
            array.Add(itemData);
        }
        
        return array;
    }
    
    /// <summary>
    /// Gets the file path for a level
    /// </summary>
    private static string GetLevelFilePath(string levelId)
    {
        return $\"user://levels/generated/{levelId}.json\";
    }
    
    /// <summary>
    /// Creates necessary directories for level storage
    /// </summary>
    private static void CreateLevelDirectories()
    {
        var dir = DirAccess.Open(\"user://\");
        if (dir != null)
        {
            if (!DirAccess.DirExistsAbsolute(\"user://levels\"))
                dir.MakeDir(\"levels\");
            
            if (!DirAccess.DirExistsAbsolute(\"user://levels/generated\"))
                dir.MakeDir(\"levels/generated\");
            
            if (!DirAccess.DirExistsAbsolute(\"user://levels/custom\"))
                dir.MakeDir(\"levels/custom\");
        }
    }
    
    /// <summary>
    /// Generates a manifest of all generated levels
    /// </summary>
    private static void GenerateManifest()
    {
        var manifest = new Godot.Collections.Dictionary();
        var levelList = new Godot.Collections.Array();
        
        // Get all generated levels
        var dir = DirAccess.Open(\"user://levels/generated\");
        if (dir != null)
        {
            string fileName = dir.GetNext();
            while (fileName != \"\")
            {
                if (fileName.EndsWith(\".json\"))
                {
                    var levelData = new Godot.Collections.Dictionary
                    {
                        [\"FileName\"] = fileName,
                        [\"LevelId\"] = fileName.Replace(\".json\", \"\")
                    };
                    levelList.Add(levelData);
                }
                fileName = dir.GetNext();
            }
        }
        
        manifest[\"TotalLevels\"] = levelList.Count;
        manifest[\"GeneratedDate\"] = DateTime.UtcNow.ToString(\"o\");
        manifest[\"Levels\"] = levelList;
        
        string manifestJson = Json.Stringify(manifest);
        var file = FileAccess.Open(\"user://levels/generated/manifest.json\", FileAccess.ModeFlags.Write);
        if (file != null)
        {
            file.StoreString(manifestJson);
            file.Close();
        }
    }
    
    /// <summary>
    /// Distribution configuration for level generation
    /// </summary>
    private class LevelDistribution
    {
        public ProceduralLevelGenerator.LevelTheme Theme { get; set; }
        public int Count { get; set; }
        public int StartLevel { get; set; }
        public int EndLevel { get; set; }
        
        public LevelDistribution(ProceduralLevelGenerator.LevelTheme theme, int count, int startLevel, int endLevel)
        {
            Theme = theme;
            Count = count;
            StartLevel = startLevel;
            EndLevel = endLevel;
        }
    }
}