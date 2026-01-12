using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Content management system for items, themes, and level generation.
/// Provides access to all game content for the level editor and procedural generator.
/// </summary>
public static class ContentManager
{
    /// <summary>
    /// Item definition for use in level editor and generation
    /// </summary>
    [Serializable]
    public class ItemDefinition
    {
        public string ItemId { get; set; }
        public string ItemName { get; set; }
        public ItemCategory Category { get; set; }
        public MaterialType Material { get; set; }
        public string Theme { get; set; }
        public int SpawnWeight { get; set; }
        public string PlaceholderSprite { get; set; }
        public float BaseScale { get; set; } = 1.0f;
        public bool IsExplosive { get; set; }
        
        public ItemDefinition(string itemId, string itemName, ItemCategory category, 
                            MaterialType material, string theme, int spawnWeight = 10)
        {
            ItemId = itemId;
            ItemName = itemName;
            Category = category;
            Material = material;
            Theme = theme;
            SpawnWeight = spawnWeight;
        }
    }
    
    public enum ItemCategory
    {
        Furniture,
        Electronics,
        Food,
        Decoration,
        Structure,
        Tool,
        Explosive
    }
    
    public enum LevelTheme
    {
        School,
        Home,
        Office,
        Government,
        Kitchen,
        Bedroom,
        LivingRoom,
        Factory,
        Arcade,
        Library,
        Museum,
        Park,
        Beach,
        Forest,
        Desert,
        Mountain,
        Space,
        Underwater,
        Futuristic,
        Medieval
    }
    
    public static class Items
    {
        private static readonly Dictionary<string, ItemDefinition> _itemRegistry = new();
        
        static Items()
        {
            InitializeItems();
        }
        
        private static void InitializeItems()
        {
            // School theme
            AddItem("desk_school", "School Desk", ItemCategory.Furniture, MaterialType.Wood, "school", 15);
            AddItem("chair_school", "School Chair", ItemCategory.Furniture, MaterialType.Wood, "school", 15);
            AddItem("bookshelf", "Bookshelf", ItemCategory.Furniture, MaterialType.Wood, "school", 8);
            AddItem("blackboard", "Blackboard", ItemCategory.Structure, MaterialType.Stone, "school", 5);
            AddItem("pencil", "Pencil", ItemCategory.Tool, MaterialType.Wood, "school", 20);
            AddItem("apple", "Apple", ItemCategory.Food, MaterialType.Wood, "school", 12);
            AddItem("backpack", "Backpack", ItemCategory.Decoration, MaterialType.Wood, "school", 8);
            
            // Home theme
            AddItem("couch", "Couch", ItemCategory.Furniture, MaterialType.Wood, "home", 12);
            AddItem("tv", "TV", ItemCategory.Electronics, MaterialType.Stone, "home", 8);
            AddItem("lamp", "Lamp", ItemCategory.Decoration, MaterialType.Wood, "home", 10);
            AddItem("picture_frame", "Picture Frame", ItemCategory.Decoration, MaterialType.Wood, "home", 15);
            AddItem("fridge", "Refridgeator", ItemCategory.Electronics, MaterialType.Iron, "home", 6);
            AddItem("oven", "Oven", ItemCategory.Electronics, MaterialType.Iron, "home", 6);
            AddItem("kitchen_table", "Kitchen Table", ItemCategory.Furniture, MaterialType.Wood, "home", 10);
            AddItem("bed", "Bed", ItemCategory.Furniture, MaterialType.Wood, "home", 8);
            AddItem("dresser", "Dresser", ItemCategory.Furniture, MaterialType.Wood, "home", 8);
            
            // Office theme
            AddItem("desk_office", "Office Desk", ItemCategory.Furniture, MaterialType.Wood, "office", 15);
            AddItem("office_chair", "Office Chair", ItemCategory.Furniture, MaterialType.Wood, "office", 15);
            AddItem("filing_cabinet", "Filing Cabinet", ItemCategory.Furniture, MaterialType.Iron, "office", 8);
            AddItem("computer", "Computer", ItemCategory.Electronics, MaterialType.Stone, "office", 10);
            AddItem("phone", "Phone", ItemCategory.Electronics, MaterialType.Stone, "office", 12);
            AddItem("printer", "Printer", ItemCategory.Electronics, MaterialType.Stone, "office", 6);
            AddItem("paper_stack", "Paper Stack", ItemCategory.Decoration, MaterialType.Wood, "office", 20);
            AddItem("coffee_cup", "Coffee Cup", ItemCategory.Decoration, MaterialType.Stone, "office", 15);
            
            // Government theme
            AddItem("podium", "Podium", ItemCategory.Furniture, MaterialType.Wood, "government", 8);
            AddItem("flag_stand", "Flag Stand", ItemCategory.Decoration, MaterialType.Iron, "government", 10);
            AddItem("portrait", "Portrait", ItemCategory.Decoration, MaterialType.Wood, "government", 12);
            AddItem("desk_gov", "Government Desk", ItemCategory.Furniture, MaterialType.Wood, "government", 10);
            AddItem("government_seal", "Government Seal", ItemCategory.Decoration, MaterialType.Iron, "government", 5);
            AddItem("formal_chair", "Formal Chair", ItemCategory.Furniture, MaterialType.Wood, "government", 12);
            AddItem("conference_table", "Conference Table", ItemCategory.Furniture, MaterialType.Wood, "government", 6);
            AddItem("document_stack", "Document Stack", ItemCategory.Decoration, MaterialType.Wood, "government", 15);
            AddItem("file_cabinet", "File Cabinet", ItemCategory.Furniture, MaterialType.Iron, "government", 10);
            
            // Kitchen theme
            AddItem("microwave", "Microwave", ItemCategory.Electronics, MaterialType.Iron, "kitchen", 8);
            AddItem("toaster", "Toaster", ItemCategory.Electronics, MaterialType.Iron, "kitchen", 10);
            AddItem("blender", "Blender", ItemCategory.Electronics, MaterialType.Iron, "kitchen", 8);
            AddItem("coffee_maker", "Coffee Maker", ItemCategory.Electronics, MaterialType.Iron, "kitchen", 8);
            AddItem("dishwasher", "Dishwasher", ItemCategory.Electronics, MaterialType.Iron, "kitchen", 5);
            AddItem("cutting_board", "Cutting Board", ItemCategory.Tool, MaterialType.Wood, "kitchen", 15);
            AddItem("pot", "Pot", ItemCategory.Tool, MaterialType.Iron, "kitchen", 12);
            AddItem("pan", "Pan", ItemCategory.Tool, MaterialType.Iron, "kitchen", 12);
            
            // Bedroom theme
            AddItem("nightstand", "Nightstand", ItemCategory.Furniture, MaterialType.Wood, "bedroom", 10);
            AddItem("wardrobe", "Wardrobe", ItemCategory.Furniture, MaterialType.Wood, "bedroom", 6);
            AddItem("mirror", "Mirror", ItemCategory.Decoration, MaterialType.Stone, "bedroom", 12);
            AddItem("alarm_clock", "Alarm Clock", ItemCategory.Electronics, MaterialType.Stone, "bedroom", 15);
            AddItem("pillow", "Pillow", ItemCategory.Decoration, MaterialType.Wood, "bedroom", 20);
            AddItem("blanket", "Blanket", ItemCategory.Decoration, MaterialType.Wood, "bedroom", 15);
            
            // Living room theme
            AddItem("coffee_table", "Coffee Table", ItemCategory.Furniture, MaterialType.Wood, "livingroom", 10);
            AddItem("bookshelf_lr", "Bookshelf", ItemCategory.Furniture, MaterialType.Wood, "livingroom", 8);
            AddItem("tv_stand", "TV Stand", ItemCategory.Furniture, MaterialType.Wood, "livingroom", 10);
            AddItem("game_console", "Game Console", ItemCategory.Electronics, MaterialType.Stone, "livingroom", 12);
            AddItem("plant", "Plant", ItemCategory.Decoration, MaterialType.Wood, "livingroom", 15);
            AddItem("vase", "Vase", ItemCategory.Decoration, MaterialType.Stone, "livingroom", 12);
            
            // Explosives (cross-theme)
            AddItem("tnt_small", "TNT (Small)", ItemCategory.Explosive, MaterialType.Stone, "all", 8, true);
            AddItem("tnt_medium", "TNT (Medium)", ItemCategory.Explosive, MaterialType.Stone, "all", 6, true);
            AddItem("tnt_large", "TNT (Large)", ItemCategory.Explosive, MaterialType.Stone, "all", 4, true);
            AddItem("firecracker", "Firecracker", ItemCategory.Explosive, MaterialType.Wood, "all", 15, true);
            AddItem("dynamite", "Dynamite", ItemCategory.Explosive, MaterialType.Stone, "all", 5, true);
            
            // Generic items
            AddItem("box", "Box", ItemCategory.Structure, MaterialType.Wood, "all", 20);
            AddItem("crate", "Crate", ItemCategory.Structure, MaterialType.Wood, "all", 18);
            AddItem("barrel", "Barrel", ItemCategory.Structure, MaterialType.Iron, "all", 12);
            
            // Arcade theme
            AddItem("arcade_cabinet", "Arcade Cabinet", ItemCategory.Electronics, MaterialType.Iron, "arcade", 8);
            AddItem("pinball_machine", "Pinball Machine", ItemCategory.Electronics, MaterialType.Iron, "arcade", 6);
            AddItem("claw_machine", "Claw Machine", ItemCategory.Electronics, MaterialType.Iron, "arcade", 5);
            
            // Library theme
            AddItem("library_table", "Library Table", ItemCategory.Furniture, MaterialType.Wood, "library", 10);
            AddItem("reading_lamp", "Reading Lamp", ItemCategory.Decoration, MaterialType.Wood, "library", 12);
            AddItem("book_stack", "Book Stack", ItemCategory.Decoration, MaterialType.Wood, "library", 20);
            
            // Museum theme
            AddItem("display_case", "Display Case", ItemCategory.Furniture, MaterialType.Iron, "museum", 8);
            AddItem("statue", "Statue", ItemCategory.Decoration, MaterialType.Stone, "museum", 6);
            AddItem("painting", "Painting", ItemCategory.Decoration, MaterialType.Wood, "museum", 12);
            
            // Park theme
            AddItem("bench", "Bench", ItemCategory.Furniture, MaterialType.Wood, "park", 12);
            AddItem("trash_can", "Trash Can", ItemCategory.Structure, MaterialType.Iron, "park", 10);
            AddItem("fountain", "Fountain", ItemCategory.Structure, MaterialType.Stone, "park", 4);
            
            // Factory theme
            AddItem("conveyor_belt", "Conveyor Belt", ItemCategory.Structure, MaterialType.Iron, "factory", 8);
            AddItem("industrial_barrel", "Industrial Barrel", ItemCategory.Structure, MaterialType.Iron, "factory", 12);
            AddItem("machine_part", "Machine Part", ItemCategory.Structure, MaterialType.Iron, "factory", 15);
            
            // Unique items for variety
            AddItem("clock", "Clock", ItemCategory.Decoration, MaterialType.Iron, "all", 10);
            AddItem("globe", "Globe", ItemCategory.Decoration, MaterialType.Wood, "all", 10);
            AddItem("camera", "Camera", ItemCategory.Electronics, MaterialType.Stone, "all", 8);
            AddItem("guitar", "Guitar", ItemCategory.Decoration, MaterialType.Wood, "all", 6);
        }
        
        private static void AddItem(string itemId, string itemName, ItemCategory category, 
                                  MaterialType material, string theme, int spawnWeight = 10, bool isExplosive = false)
        {
            var item = new ItemDefinition(itemId, itemName, category, material, theme, spawnWeight)
            {
                IsExplosive = isExplosive
            };
            _itemRegistry[itemId] = item;
        }
        
        public static ItemDefinition GetItem(string itemId)
        {
            return _itemRegistry.TryGetValue(itemId, out var item) ? item : null;
        }
        
        public static ItemDefinition[] GetAllItems()
        {
            return _itemRegistry.Values.ToArray();
        }
        
        public static ItemDefinition[] GetItemsByTheme(string theme)
        {
            return _itemRegistry.Values.Where(i => i.Theme == theme || theme == "all").ToArray();
        }
        
        public static ItemDefinition[] GetItemsByCategory(ItemCategory category)
        {
            return _itemRegistry.Values.Where(i => i.Category == category).ToArray();
        }
        
        public static ItemDefinition[] GetItemsByType(ItemCategory category)
        {
            return GetItemsByCategory(category);
        }
    }
    
    public static class Themes
    {
        public static readonly Dictionary<LevelTheme, ThemeConfig> ThemeConfigs = new()
        {
            [LevelTheme.School] = new ThemeConfig("School", "Blue", 0.3f, 15),
            [LevelTheme.Home] = new ThemeConfig("Home", "Green", 0.4f, 15),
            [LevelTheme.Office] = new ThemeConfig("Office", "Gray", 0.5f, 15),
            [LevelTheme.Government] = new ThemeConfig("Government", "Red", 0.6f, 12),
            [LevelTheme.Kitchen] = new ThemeConfig("Kitchen", "Orange", 0.4f, 12),
            [LevelTheme.Bedroom] = new ThemeConfig("Bedroom", "Purple", 0.35f, 12),
            [LevelTheme.LivingRoom] = new ThemeConfig("Living Room", "Brown", 0.4f, 12),
            [LevelTheme.Factory] = new ThemeConfig("Factory", "Dark Gray", 0.7f, 10),
            [LevelTheme.Arcade] = new ThemeConfig("Arcade", "Neon", 0.5f, 10),
            [LevelTheme.Library] = new ThemeConfig("Library", "Wood", 0.3f, 10),
            [LevelTheme.Museum] = new ThemeConfig("Museum", "Gold", 0.6f, 8),
            [LevelTheme.Park] = new ThemeConfig("Park", "Green", 0.25f, 10),
            [LevelTheme.Beach] = new ThemeConfig("Beach", "Cyan", 0.3f, 8),
            [LevelTheme.Forest] = new ThemeConfig("Forest", "Dark Green", 0.35f, 10),
            [LevelTheme.Desert] = new ThemeConfig("Desert", "Yellow", 0.45f, 8),
            [LevelTheme.Mountain] = new ThemeConfig("Mountain", "Stone", 0.5f, 8),
            [LevelTheme.Space] = new ThemeConfig("Space", "Dark Blue", 0.6f, 6),
            [LevelTheme.Underwater] = new ThemeConfig("Underwater", "Blue", 0.4f, 8),
            [LevelTheme.Futuristic] = new ThemeConfig("Futuristic", "Silver", 0.7f, 8),
            [LevelTheme.Medieval] = new ThemeConfig("Medieval", "Medieval", 0.5f, 10)
        };
    }
    
    public class ThemeConfig
    {
        public string DisplayName { get; set; }
        public string ColorScheme { get; set; }
        public float DifficultyMultiplier { get; set; }
        public int LevelCount { get; set; }
        
        public ThemeConfig(string displayName, string colorScheme, float difficultyMultiplier, int levelCount)
        {
            DisplayName = displayName;
            ColorScheme = colorScheme;
            DifficultyMultiplier = difficultyMultiplier;
            LevelCount = levelCount;
        }
    }
}