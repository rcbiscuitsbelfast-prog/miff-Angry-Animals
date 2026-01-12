using Godot;
using System;

/// <summary>
/// Factory for creating item instances in the level editor and gameplay
/// </summary>
public static class ItemFactory
{
    private const string ITEM_SCENE_PATH = "res://Scenes/Objects/BreakableObstacle.tscn";
    private const string EXPLOSIVE_SCENE_PATH = "res://Scenes/Objects/Explosive.tscn";
    
    /// <summary>
    /// Creates an item node from a definition
    /// </summary>
    public static Node2D CreateItem(ItemDefinition definition, Vector2 position)
    {
        if (definition == null)
        {
            GD.PushWarning("ItemFactory.CreateItem: Null definition provided");
            return null;
        }
        
        // Determine which scene to use
        string scenePath = definition.IsExplosive ? EXPLOSIVE_SCENE_PATH : ITEM_SCENE_PATH;
        var scene = GD.Load<PackedScene>(scenePath);
        
        if (scene == null)
        {
            GD.PushWarning($"ItemFactory.CreateItem: Scene not found at {scenePath}");
            return CreatePlaceholderItem(definition, position);
        }
        
        var itemNode = scene.Instantiate<Node2D>();
        itemNode.Position = position;
        itemNode.Name = definition.ItemId;
        
        // Configure the item based on definition
        ConfigureItem(itemNode, definition);
        
        return itemNode;
    }
    
    /// <summary>
    /// Creates an item instance from metadata
    /// </summary>
    public static Node2D CreateItemInstance(ItemInstance instance)
    {
        var definition = ContentManager.Items.GetItem(instance.ItemId);
        if (definition == null)
        {
            GD.PushWarning($"ItemFactory.CreateItemInstance: Unknown item ID {instance.ItemId}");
            return null;
        }
        
        var itemNode = CreateItem(definition, instance.Position);
        if (itemNode != null)
        {
            itemNode.Rotation = instance.Rotation;
            itemNode.Scale = new Vector2(instance.Scale, instance.Scale);
        }
        
        return itemNode;
    }
    
    /// <summary>
    /// Creates a placeholder visual item for the editor
    /// </summary>
    public static Node2D CreateEditorItem(ItemDefinition definition, Vector2 position)
    {
        var container = new Node2D();
        container.Position = position;
        container.Name = definition.ItemId;
        
        // Create colored rectangle based on material
        var rect = new ColorRect();
        rect.Size = new Vector2(40, 40);
        rect.Position = new Vector2(-20, -20);
        rect.Color = GetMaterialColor(definition.Material);
        container.AddChild(rect);
        
        // Add icon/text label
        var label = new Label();
        label.Text = GetItemIcon(definition);
        label.Position = new Vector2(-10, -5);
        label.Size = new Vector2(20, 10);
        container.AddChild(label);
        
        // Add name tooltip
        var tooltip = new Tooltip();
        tooltip.Text = definition.ItemName;
        container.AddChild(tooltip);
        
        return container;
    }
    
    /// <summary>
    /// Creates a simple placeholder when scenes are unavailable
    /// </summary>
    private static Node2D CreatePlaceholderItem(ItemDefinition definition, Vector2 position)
    {
        var container = new Node2D();
        container.Position = position;
        
        var sprite = new Sprite2D();
        sprite.Texture = CreatePlaceholderTexture(definition);
        container.AddChild(sprite);
        
        return container;
    }
    
    /// <summary>
    /// Configures an item node with material and properties
    /// </summary>
    private static void ConfigureItem(Node2D itemNode, ItemDefinition definition)
    {
        // Try to find BreakableObstacle component
        var breakable = itemNode.GetNodeOrNull<BreakableObstacle>(".") ?? 
                       itemNode.GetNodeOrNull<BreakableObstacle>("BreakableObstacle");
        
        if (breakable != null)
        {
            breakable.Material = definition.Material;
        }
        
        // Set custom data for item identification
        itemNode.SetMeta("item_id", definition.ItemId);
        itemNode.SetMeta("item_name", definition.ItemName);
        itemNode.SetMeta("is_explosive", definition.IsExplosive);
    }
    
    /// <summary>
    /// Gets the appropriate color for a material
    /// </summary>
    private static Color GetMaterialColor(MaterialType material)
    {
        return material switch
        {
            MaterialType.Wood => new Color(0.6f, 0.4f, 0.2f),
            MaterialType.Stone => new Color(0.5f, 0.5f, 0.5f),
            MaterialType.Brick => new Color(0.7f, 0.3f, 0.2f),
            MaterialType.Iron => new Color(0.3f, 0.3f, 0.3f),
            MaterialType.Diamond => new Color(0.4f, 0.7f, 0.9f),
            _ => Colors.White
        };
    }
    
    /// <summary>
    /// Gets a simple icon/text representation for an item
    /// </summary>
    private static string GetItemIcon(ItemDefinition definition)
    {
        return definition.Category switch
        {
            ItemCategory.Furniture => "🪑",
            ItemCategory.Electronics => "📺",
            ItemCategory.Food => "🍎",
            ItemCategory.Decoration => "🖼️",
            ItemCategory.Structure => "📦",
            ItemCategory.Tool => "🔧",
            ItemCategory.Explosive => "💣",
            _ => "❓"
        };
    }
    
    /// <summary>
    /// Creates a simple colored texture for placeholders
    /// </summary>
    private static Texture2D CreatePlaceholderTexture(ItemDefinition definition)
    {
        var image = Image.Create(32, 32, false, Image.Format.Rgba8);
        var color = GetMaterialColor(definition.Material);
        
        for (int x = 0; x < 32; x++)
        {
            for (int y = 0; y < 32; y++)
            {
                image.SetPixel(x, y, color);
            }
        }
        
        var texture = ImageTexture.CreateFromImage(image);
        return texture;
    }
}

/// <summary>
/// Simple tooltip helper for editor items
/// </summary>
public partial class Tooltip : Node
{
    public string Text { get; set; } = "";
}