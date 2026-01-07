using Godot;

/// <summary>
/// Defines the different material types available for breakable obstacles.
/// Each material has a specific hardness rating that determines durability.
/// </summary>
public enum MaterialType
{
    /// <summary>
    /// Wood material with low hardness (1).
    /// Represents soft obstacles that break easily.
    /// </summary>
    Wood = 1,

    /// <summary>
    /// Stone material with medium hardness (2).
    /// Provides moderate resistance to damage.
    /// </summary>
    Stone = 2,

    /// <summary>
    /// Brick material with high hardness (3).
    /// Requires multiple hits to destroy.
    /// </summary>
    Brick = 3,

    /// <summary>
    /// Iron material with very high hardness (4).
    /// Very durable and resistant to damage.
    /// </summary>
    Iron = 4,

    /// <summary>
    /// Diamond material with maximum hardness (5).
    /// Extremely durable and hardest to destroy.
    /// </summary>
    Diamond = 5
}