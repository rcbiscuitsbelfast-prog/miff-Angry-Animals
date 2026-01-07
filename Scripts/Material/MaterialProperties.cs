using Godot;

/// <summary>
/// Contains all material properties for breakable obstacles.
/// Defines hardness, durability, visual appearance, and damage feedback settings.
/// </summary>
public readonly struct MaterialProperties
{
    /// <summary>
    /// The type of material this represents.
    /// </summary>
    public readonly MaterialType Material { get; }

    /// <summary>
    /// Hardness rating from 1-5 (1 = softest, 5 = hardest).
    /// Higher hardness means more durability.
    /// </summary>
    public readonly int Hardness { get; }

    /// <summary>
    /// Number of hits required to destroy this material.
    /// Calculated from hardness with a base multiplier.
    /// </summary>
    public readonly int HitsToDestroy { get; }

    /// <summary>
    /// Base color for visual representation of this material.
    /// Used for placeholder coloring when no custom assets are available.
    /// </summary>
    public readonly Color BaseColor { get; }

    /// <summary>
    /// Visual modifier for difficulty indication.
    /// Controls scale and opacity adjustments for harder materials.
    /// </summary>
    public readonly Vector2 VisualModifier { get; }

    /// <summary>
    /// Constructor for creating material properties.
    /// </summary>
    /// <param name="material">The material type.</param>
    /// <param name="hardness">Hardness rating (1-5).</param>
    /// <param name="baseColor">Visual color for this material.</param>
    /// <param name="visualModifier">Scale/opacity modifier for difficulty indication.</param>
    public MaterialProperties(MaterialType material, int hardness, Color baseColor, Vector2 visualModifier)
    {
        Material = material;
        Hardness = Mathf.Clamp(hardness, 1, 5);
        
        // Calculate hits to destroy based on hardness
        // Formula: Base hits = hardness * 2, with minimum of 1 hit
        HitsToDestroy = Mathf.Max(1, hardness * 2);
        
        BaseColor = baseColor;
        VisualModifier = visualModifier;
    }

    /// <summary>
    /// Gets material properties for Wood material.
    /// Light brown color, soft and easy to break.
    /// </summary>
    public static MaterialProperties Wood => new MaterialProperties(
        MaterialType.Wood, 
        1, 
        new Color(0x8B, 0x45, 0x13), // #8B4513 - Light brown
        new Vector2(1.0f, 1.0f)      // Full scale, normal opacity
    );

    /// <summary>
    /// Gets material properties for Stone material.
    /// Medium gray color, moderate durability.
    /// </summary>
    public static MaterialProperties Stone => new MaterialProperties(
        MaterialType.Stone, 
        2, 
        new Color(0x80, 0x80, 0x80), // #808080 - Medium gray
        new Vector2(1.05f, 0.95f)    // Slightly larger, slightly transparent
    );

    /// <summary>
    /// Gets material properties for Brick material.
    /// Red color, high durability.
    /// </summary>
    public static MaterialProperties Brick => new MaterialProperties(
        MaterialType.Brick, 
        3, 
        new Color(0xC4, 0x1E, 0x3A), // #C41E3A - Red
        new Vector2(1.1f, 0.9f)      // Larger, more transparent
    );

    /// <summary>
    /// Gets material properties for Iron material.
    /// Dark gray color, very durable.
    /// </summary>
    public static MaterialProperties Iron => new MaterialProperties(
        MaterialType.Iron, 
        4, 
        new Color(0x36, 0x45, 0x4F), // #36454F - Dark gray
        new Vector2(1.15f, 0.85f)    // Even larger, more transparent
    );

    /// <summary>
    /// Gets material properties for Diamond material.
    /// Light cyan color, extremely durable.
    /// </summary>
    public static MaterialProperties Diamond => new MaterialProperties(
        MaterialType.Diamond, 
        5, 
        new Color(0x00, 0xFF, 0xFF), // #00FFFF - Light cyan
        new Vector2(1.2f, 0.8f)      // Largest, most transparent
    );

    /// <summary>
    /// Gets all available material properties as an array.
    /// Useful for random material selection.
    /// </summary>
    public static MaterialProperties[] GetAllMaterials()
    {
        return new MaterialProperties[]
        {
            Wood,
            Stone,
            Brick,
            Iron,
            Diamond
        };
    }

    /// <summary>
    /// Gets material properties for a specific material type.
    /// </summary>
    /// <param name="type">The material type to get properties for.</param>
    /// <returns>MaterialProperties for the specified type.</returns>
    /// <exception cref="ArgumentException">Thrown if the material type is not supported.</exception>
    public static MaterialProperties GetMaterialProperties(MaterialType type)
    {
        return type switch
        {
            MaterialType.Wood => Wood,
            MaterialType.Stone => Stone,
            MaterialType.Brick => Brick,
            MaterialType.Iron => Iron,
            MaterialType.Diamond => Diamond,
            _ => throw new ArgumentException($"Unsupported material type: {type}")
        };
    }

    /// <summary>
    /// Gets a random material from the available options.
    /// Useful for procedural generation.
    /// </summary>
    /// <param name="random">Random number generator to use.</param>
    /// <returns>Random material properties.</returns>
    public static MaterialProperties GetRandomMaterial(Random random)
    {
        var materials = GetAllMaterials();
        return materials[random.Next(materials.Length)];
    }

    /// <summary>
    /// Gets a material appropriate for the given difficulty level.
    /// Easier levels favor softer materials, harder levels include harder materials.
    /// </summary>
    /// <param name="roomNumber">The room/level number for difficulty scaling.</param>
    /// <param name="random">Random number generator to use.</param>
    /// <returns>Material appropriate for the difficulty level.</returns>
    public static MaterialProperties GetMaterialForDifficulty(int roomNumber, Random random)
    {
        // Define difficulty tiers based on room progression
        MaterialType[] easyMaterials = { MaterialType.Wood, MaterialType.Stone };
        MaterialType[] mediumMaterials = { MaterialType.Stone, MaterialType.Brick };
        MaterialType[] hardMaterials = { MaterialType.Brick, MaterialType.Iron };
        MaterialType[] extremeMaterials = { MaterialType.Iron, MaterialType.Diamond };

        MaterialType[] availableMaterials;

        if (roomNumber <= 20)
        {
            // Early levels: mostly wood and stone
            availableMaterials = easyMaterials;
        }
        else if (roomNumber <= 50)
        {
            // Mid-early levels: stone and brick
            availableMaterials = mediumMaterials;
        }
        else if (roomNumber <= 80)
        {
            // Mid-late levels: brick and iron
            availableMaterials = hardMaterials;
        }
        else
        {
            // Late levels: iron and diamond
            availableMaterials = extremeMaterials;
        }

        // Randomly select from available materials for this difficulty
        return GetMaterialProperties(availableMaterials[random.Next(availableMaterials.Length)]);
    }
}