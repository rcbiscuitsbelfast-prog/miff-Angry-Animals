using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Main procedural level generator with theme-specific generation algorithms
/// </summary>
public partial class ProceduralLevelGenerator : Node
{
    private const float CANVAS_WIDTH = 650f;  // Usable area width
    private const float CANVAS_HEIGHT = 480f; // Usable area height
    private const float SAFE_ZONE_TOP = 50f;
    private const float SAFE_ZONE_BOTTOM = 530f;
    private const float SAFE_ZONE_LEFT = 300f;
    private const float SAFE_ZONE_RIGHT = 950f;
    
    public enum LevelTheme
    {
        School = 0,
        Home = 1,
        Office = 2,
        Government = 3,
        Kitchen = 4,
        Bedroom = 5,
        LivingRoom = 6,
        Factory = 7,
        Arcade = 8,
        Library = 9,
        Museum = 10,
        Park = 11,
        Beach = 12,
        Forest = 13,
        Desert = 14,
        Mountain = 15,
        Space = 16,
        Underwater = 17,
        Futuristic = 18,
        Medieval = 19
    }
    
    public static LevelMetadata GenerateLevel(int levelNumber, LevelTheme theme)
    {
        var generator = new ProceduralLevelGenerator();
        
        return theme switch
        {
            LevelTheme.School => generator.GenerateSchool(levelNumber),
            LevelTheme.Home => generator.GenerateHome(levelNumber),
            LevelTheme.Office => generator.GenerateOffice(levelNumber),
            LevelTheme.Government => generator.GenerateGovernment(levelNumber),
            LevelTheme.Kitchen => generator.GenerateKitchen(levelNumber),
            LevelTheme.Bedroom => generator.GenerateBedroom(levelNumber),
            LevelTheme.LivingRoom => generator.GenerateLivingRoom(levelNumber),
            LevelTheme.Factory => generator.GenerateFactory(levelNumber),
            LevelTheme.Arcade => generator.GenerateArcade(levelNumber),
            _ => generator.GenerateGeneric(levelNumber)
        };
    }
    
    private LevelMetadata GenerateSchool(int levelNumber)
    {
        var level = new LevelMetadata
        {
            Theme = "school",
            LevelId = $"school_{levelNumber:D3}",
            LevelName = $"Classroom Chaos #{levelNumber}",
            Difficulty = GetDifficultyForLevel(levelNumber),
            Description = "Clean up the classroom mess!",
            IsGenerated = true
        };
        
        var random = new Random(GetSeed(levelNumber, LevelTheme.School));
        
        // Add classroom furniture
        AddThemedItems(level, "school", levelNumber, random, "desk_school", 3, 5);
        AddThemedItems(level, "school", levelNumber, random, "chair_school", 3, 5);
        AddThemedItems(level, "school", levelNumber, random, "bookshelf", 1, 2);
        AddThemedItems(level, "school", levelNumber, random, "blackboard", 1, 1);
        
        // Add supplies
        AddThemedItems(level, "school", levelNumber, random, "pencil", 5, 8);
        AddThemedItems(level, "school", levelNumber, random, "apple", 2, 4);
        AddThemedItems(level, "school", levelNumber, random, "backpack", 1, 2);
        
        // Add explosives (difficulty-based)
        if (levelNumber > 5)
            AddThemedItems(level, "all", levelNumber, random, "tnt_small", 2, 4);
        
        if (levelNumber > 15)
            AddThemedItems(level, "all", levelNumber, random, "firecracker", 3, 6);
        
        level.Goal = CalculateGoal(levelNumber);
        level.TargetTime = CalculateTargetTime(levelNumber);
        
        return level;
    }
    
    private LevelMetadata GenerateHome(int levelNumber)
    {
        var level = new LevelMetadata
        {
            Theme = "home",
            LevelId = $"home_{levelNumber:D3}",
            LevelName = $"House Party Cleanup #{levelNumber}",
            Difficulty = GetDifficultyForLevel(levelNumber),
            Description = "Tidy up the house after the party!",
            IsGenerated = true
        };
        
        var random = new Random(GetSeed(levelNumber, LevelTheme.Home));
        
        // Living room
        AddThemedItems(level, "home", levelNumber, random, "couch", 1, 2);
        AddThemedItems(level, "home", levelNumber, random, "tv", 1, 1);
        AddThemedItems(level, "home", levelNumber, random, "lamp", 1, 3);
        AddThemedItems(level, "home", levelNumber, random, "picture_frame", 2, 4);
        
        // Kitchen area
        AddThemedItems(level, "home", levelNumber, random, "fridge", 1, 1);
        AddThemedItems(level, "home", levelNumber, random, "oven", 1, 1);
        AddThemedItems(level, "home", levelNumber, random, "kitchen_table", 1, 1);
        
        // Food scattered around
        AddThemedItems(level, "home", levelNumber, random, "apple", 3, 6);
        AddThemedItems(level, "all", levelNumber, random, "box", 2, 4);
        
        // Explosives
        if (levelNumber > 10)
            AddThemedItems(level, "all", levelNumber, random, "tnt_small", 2, 3);
        
        level.Goal = CalculateGoal(levelNumber);
        level.TargetTime = CalculateTargetTime(levelNumber);
        
        return level;
    }
    
    private LevelMetadata GenerateOffice(int levelNumber)
    {
        var level = new LevelMetadata
        {
            Theme = "office",
            LevelId = $"office_{levelNumber:D3}",
            LevelName = $"Office Overtime #{levelNumber}",
            Difficulty = GetDifficultyForLevel(levelNumber),
            Description = "Clean up the office workspace!",
            IsGenerated = true
        };
        
        var random = new Random(GetSeed(levelNumber, LevelTheme.Office));
        
        // Desks and chairs
        AddThemedItems(level, "office", levelNumber, random, "desk_office", 3, 6);
        AddThemedItems(level, "office", levelNumber, random, "office_chair", 3, 6);
        AddThemedItems(level, "office", levelNumber, random, "filing_cabinet", 2, 3);
        
        // Electronics
        AddThemedItems(level, "office", levelNumber, random, "computer", 2, 4);
        AddThemedItems(level, "office", levelNumber, random, "phone", 2, 3);
        AddThemedItems(level, "office", levelNumber, random, "printer", 1, 1);
        
        // Papers, coffee
        AddThemedItems(level, "office", levelNumber, random, "paper_stack", 3, 5);
        AddThemedItems(level, "office", levelNumber, random, "coffee_cup", 2, 4);
        
        // Explosives
        if (levelNumber > 8)
            AddThemedItems(level, "all", levelNumber, random, "tnt_medium", 1, 2);
        
        level.Goal = CalculateGoal(levelNumber);
        level.TargetTime = CalculateTargetTime(levelNumber);
        
        return level;
    }
    
    private LevelMetadata GenerateGovernment(int levelNumber)
    {
        var level = new LevelMetadata
        {
            Theme = "government",
            LevelId = $"government_{levelNumber:D3}",
            LevelName = $"Governing Body #{levelNumber}",
            Difficulty = GetDifficultyForLevel(levelNumber),
            Description = "Navigate the bureaucracy!",
            IsGenerated = true
        };
        
        var random = new Random(GetSeed(levelNumber, LevelTheme.Government));
        
        // Government building elements
        AddThemedItems(level, "government", levelNumber, random, "podium", 1, 1);
        AddThemedItems(level, "government", levelNumber, random, "flag_stand", 1, 2);
        AddThemedItems(level, "government", levelNumber, random, "portrait", 2, 3);
        AddThemedItems(level, "government", levelNumber, random, "desk_gov", 3, 4);
        AddThemedItems(level, "government", levelNumber, random, "government_seal", 1, 1);
        
        // Formal furniture
        AddThemedItems(level, "government", levelNumber, random, "formal_chair", 4, 6);
        AddThemedItems(level, "government", levelNumber, random, "conference_table", 1, 2);
        
        // Documents, papers
        AddThemedItems(level, "government", levelNumber, random, "document_stack", 3, 5);
        AddThemedItems(level, "government", levelNumber, random, "file_cabinet", 2, 3);
        
        // Explosives (rare in government buildings!)
        if (levelNumber > 20)
            AddThemedItems(level, "all", levelNumber, random, "tnt_large", 1, 2);
        
        level.Goal = CalculateGoal(levelNumber);
        level.TargetTime = CalculateTargetTime(levelNumber);
        
        return level;
    }
    
    private LevelMetadata GenerateKitchen(int levelNumber)
    {
        var level = new LevelMetadata
        {
            Theme = "kitchen",
            LevelId = $"kitchen_{levelNumber:D3}",
            LevelName = $"Kitchen Catastrophe #{levelNumber}",
            Difficulty = GetDifficultyForLevel(levelNumber),
            Description = "Clean up the kitchen disaster!",
            IsGenerated = true
        };
        
        var random = new Random(GetSeed(levelNumber, LevelTheme.Kitchen));
        
        // Appliances
        AddThemedItems(level, "kitchen", levelNumber, random, "microwave", 1, 2);
        AddThemedItems(level, "kitchen", levelNumber, random, "toaster", 1, 1);
        AddThemedItems(level, "kitchen", levelNumber, random, "blender", 1, 1);
        AddThemedItems(level, "kitchen", levelNumber, random, "coffee_maker", 1, 1);
        AddThemedItems(level, "kitchen", levelNumber, random, "dishwasher", 1, 1);
        
        // Tools and cookware
        AddThemedItems(level, "kitchen", levelNumber, random, "cutting_board", 2, 3);
        AddThemedItems(level, "kitchen", levelNumber, random, "pot", 2, 4);
        AddThemedItems(level, "kitchen", levelNumber, random, "pan", 2, 4);
        
        // Food and mess
        AddThemedItems(level, "all", levelNumber, random, "apple", 3, 6);
        AddThemedItems(level, "all", levelNumber, random, "box", 2, 4);
        
        level.Goal = CalculateGoal(levelNumber);
        level.TargetTime = CalculateTargetTime(levelNumber);
        
        return level;
    }
    
    private LevelMetadata GenerateBedroom(int levelNumber)
    {
        var level = new LevelMetadata
        {
            Theme = "bedroom",
            LevelId = $"bedroom_{levelNumber:D3}",
            LevelName = $"Bedroom Blitz #{levelNumber}",
            Difficulty = GetDifficultyForLevel(levelNumber),
            Description = "Organize the messy bedroom!",
            IsGenerated = true
        };
        
        var random = new Random(GetSeed(levelNumber, LevelTheme.Bedroom));
        
        // Furniture
        AddThemedItems(level, "bedroom", levelNumber, random, "bed", 1, 1);
        AddThemedItems(level, "bedroom", levelNumber, random, "dresser", 1, 2);
        AddThemedItems(level, "bedroom", levelNumber, random, "nightstand", 1, 2);
        AddThemedItems(level, "bedroom", levelNumber, random, "wardrobe", 1, 1);
        
        // Decorations
        AddThemedItems(level, "bedroom", levelNumber, random, "mirror", 1, 2);
        AddThemedItems(level, "bedroom", levelNumber, random, "alarm_clock", 1, 2);
        AddThemedItems(level, "bedroom", levelNumber, random, "pillow", 3, 5);
        AddThemedItems(level, "bedroom", levelNumber, random, "blanket", 2, 3);
        
        // Clutter
        AddThemedItems(level, "all", levelNumber, random, "box", 2, 4);
        AddThemedItems(level, "all", levelNumber, random, "bookshelf", 1, 1);
        
        level.Goal = CalculateGoal(levelNumber);
        level.TargetTime = CalculateTargetTime(levelNumber);
        
        return level;
    }
    
    private LevelMetadata GenerateLivingRoom(int levelNumber)
    {
        var level = new LevelMetadata
        {
            Theme = "livingroom",
            LevelId = $"livingroom_{levelNumber:D3}",
            LevelName = $"Living Room Mayhem #{levelNumber}",
            Difficulty = GetDifficultyForLevel(levelNumber),
            Description = "Tidy up the living room!",
            IsGenerated = true
        };
        
        var random = new Random(GetSeed(levelNumber, LevelTheme.LivingRoom));
        
        // Furniture
        AddThemedItems(level, "home", levelNumber, random, "couch", 2, 3);
        AddThemedItems(level, "livingroom", levelNumber, random, "coffee_table", 1, 1);
        AddThemedItems(level, "livingroom", levelNumber, random, "bookshelf_lr", 1, 2);
        AddThemedItems(level, "livingroom", levelNumber, random, "tv_stand", 1, 1);
        
        // Electronics and decor
        AddThemedItems(level, "home", levelNumber, random, "tv", 1, 1);
        AddThemedItems(level, "livingroom", levelNumber, random, "game_console", 1, 2);
        AddThemedItems(level, "livingroom", levelNumber, random, "plant", 2, 4);
        AddThemedItems(level, "livingroom", levelNumber, random, "vase", 1, 3);
        AddThemedItems(level, "home", levelNumber, random, "lamp", 1, 2);
        
        level.Goal = CalculateGoal(levelNumber);
        level.TargetTime = CalculateTargetTime(levelNumber);
        
        return level;
    }
    
    private LevelMetadata GenerateFactory(int levelNumber)
    {
        var level = new LevelMetadata
        {
            Theme = "factory",
            LevelId = $"factory_{levelNumber:D3}",
            LevelName = "Factory Floor #{levelNumber}",
            Difficulty = GetDifficultyForLevel(levelNumber),
            Description = "Clean up the industrial mess!",
            IsGenerated = true
        };
        
        var random = new Random(GetSeed(levelNumber, LevelTheme.Factory));
        
        // Industrial equipment
        AddThemedItems(level, "factory", levelNumber, random, "conveyor_belt", 2, 4);
        AddThemedItems(level, "factory", levelNumber, random, "industrial_barrel", 4, 8);
        AddThemedItems(level, "factory", levelNumber, random, "machine_part", 6, 10);
        
        // Storage
        AddThemedItems(level, "all", levelNumber, random, "crate", 3, 5);
        AddThemedItems(level, "all", levelNumber, random, "barrel", 3, 5);
        
        // Explosives (common in factory theme)
        if (levelNumber > 3)
            AddThemedItems(level, "all", levelNumber, random, "tnt_medium", 2, 4);
        if (levelNumber > 10)
            AddThemedItems(level, "all", levelNumber, random, "dynamite", 1, 3);
        
        level.Goal = CalculateGoal(levelNumber);
        level.TargetTime = CalculateTargetTime(levelNumber);
        
        return level;
    }
    
    private LevelMetadata GenerateArcade(int levelNumber)
    {
        var level = new LevelMetadata
        {
            Theme = "arcade",
            LevelId = $"arcade_{levelNumber:D3}",
            LevelName = "Arcade After Hours #{levelNumber}",
            Difficulty = GetDifficultyForLevel(levelNumber),
            Description = "Clean up the arcade!",
            IsGenerated = true
        };
        
        var random = new Random(GetSeed(levelNumber, LevelTheme.Arcade));
        
        // Arcade machines
        AddThemedItems(level, "arcade", levelNumber, random, "arcade_cabinet", 3, 6);
        AddThemedItems(level, "arcade", levelNumber, random, "pinball_machine", 2, 3);
        AddThemedItems(level, "arcade", levelNumber, random, "claw_machine", 1, 2);
        
        // Decorations
        AddThemedItems(level, "all", levelNumber, random, "game_console", 2, 4);
        AddThemedItems(level, "all", levelNumber, random, "tv", 1, 2);
        
        level.Goal = CalculateGoal(levelNumber);
        level.TargetTime = CalculateTargetTime(levelNumber);
        
        return level;
    }
    
    private LevelMetadata GenerateGeneric(int levelNumber)
    {
        return GenerateOffice(levelNumber); // Fallback to office theme
    }
    
    #region Helper Methods
    
    private void AddThemedItems(LevelMetadata level, string theme, int levelNumber, 
                               Random random, string itemId, int minCount, int maxCount)
    {
        var definition = ContentManager.Items.GetItem(itemId);
        if (definition == null) return;
        
        var count = random.Next(minCount, maxCount + 1);
        
        for (int i = 0; i < count; i++)
        {
            var position = GetRandomPosition(random);
            var instance = new ItemInstance(itemId, position)
            {
                Rotation = (float)random.NextDouble() * 0.5f - 0.25f,
                Scale = 1.0f + (float)random.NextDouble() * 0.2f
            };
            
            level.Items.Add(instance);
        }
    }
    
    private Vector2 GetRandomPosition(Random random)
    {
        return new Vector2(
            SAFE_ZONE_LEFT + (float)random.NextDouble() * (SAFE_ZONE_RIGHT - SAFE_ZONE_LEFT),
            SAFE_ZONE_TOP + (float)random.NextDouble() * (SAFE_ZONE_BOTTOM - SAFE_ZONE_TOP)
        );
    }
    
    private int GetSeed(int levelNumber, LevelTheme theme)
    {
        return levelNumber * 1000 + (int)theme;
    }
    
    private Difficulty GetDifficultyForLevel(int levelNumber)
    {
        if (levelNumber < 10) return Difficulty.Easy;
        if (levelNumber < 30) return Difficulty.Medium;
        if (levelNumber < 60) return Difficulty.Hard;
        return Difficulty.Extreme;
    }
    
    private int CalculateGoal(int levelNumber)
    {
        // Start at 20, increase by 2 every 5 levels, cap at 50
        return Math.Min(50, 20 + (levelNumber / 5) * 2);
    }
    
    private int CalculateTargetTime(int levelNumber)
    {
        // Base 120 seconds, decrease by 5 every 10 levels (minimum 60)
        return Math.Max(60, 120 - (levelNumber / 10) * 5);
    }
    
    #endregion
}