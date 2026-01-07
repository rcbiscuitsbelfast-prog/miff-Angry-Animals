using System;
using Godot;

/// <summary>
/// Procedural level generator that creates varied cup configurations and visual themes.
/// Uses seeded RNG for deterministic generation.
/// Registered as an autoload singleton for global access.
/// </summary>
public partial class LevelGenerator : Node
{
    public static LevelGenerator Instance { get; private set; } = null!;

    /// <summary>
    /// Theme configuration for visual progression.
    /// </summary>
    public readonly struct ThemeConfig
    {
        public readonly Color BackgroundColor;
        public readonly Color FloorColor;
        public readonly bool HasPremiumEffects;
        public readonly string ThemeName;

        public ThemeConfig(Color backgroundColor, Color floorColor, bool premiumEffects, string themeName)
        {
            BackgroundColor = backgroundColor;
            FloorColor = floorColor;
            HasPremiumEffects = premiumEffects;
            ThemeName = themeName;
        }
    }

    /// <summary>
    /// Cup configuration for procedural spawning with material properties.
    /// </summary>
    public readonly struct CupConfig
    {
        public readonly Vector2 Position;
        public readonly float Rotation;
        public readonly float Scale;
        public readonly bool IsPremium;
        public readonly MaterialProperties Material;

        public CupConfig(Vector2 position, float rotation, float scale, bool isPremium, MaterialProperties material)
        {
            Position = position;
            Rotation = rotation;
            Scale = scale;
            IsPremium = isPremium;
            Material = material;
        }
    }

    // Safe zone boundaries (keep clear for gameplay)
    private const float SlingshotSafeX = 300f;
    private const float ExitDoorSafeX = 900f;
    private const float FloorY = 530f;

    public override void _Ready()
    {
        Instance = this;
        ProcessMode = ProcessModeEnum.Always;
    }

    /// <summary>
    /// Creates a deterministic seed from the room number.
    /// </summary>
    public static int CalculateSeed(int roomNumber)
    {
        // Simple hash: room number * prime + offset
        // This ensures the same room always generates the same layout
        return roomNumber * 73856093 ^ 19349663;
    }

    /// <summary>
    /// Creates a time-based random seed for fresh layouts.
    /// </summary>
    public static int CreateRandomSeed()
    {
        unchecked
        {
            return (int)(Time.GetTicksMsec() ^ (uint)GD.Randi());
        }
    }

    /// <summary>
    /// Gets the visual theme for the specified room number.
    /// </summary>
    public static ThemeConfig GetThemeForRoom(int roomNumber)
    {
        // Level 1-30: Blue theme
        if (roomNumber <= 30)
        {
            return new ThemeConfig(
                new Color(0.3f, 0.6f, 0.9f),
                new Color(0.4f, 0.5f, 0.3f),
                false,
                "Blue"
            );
        }

        // Level 31-60: Purple theme
        if (roomNumber <= 60)
        {
            return new ThemeConfig(
                new Color(0.5f, 0.3f, 0.7f),
                new Color(0.4f, 0.3f, 0.5f),
                true,
                "Purple"
            );
        }

        // Level 61-100+: Red/Orange theme
        return new ThemeConfig(
            new Color(0.8f, 0.4f, 0.3f),
            new Color(0.5f, 0.3f, 0.2f),
            true,
            "Red"
        );
    }

    /// <summary>
    /// Gets the interpolated background color between themes.
    /// </summary>
    public static Color GetInterpolatedBackgroundColor(int roomNumber)
    {
        var blueTheme = new Color(0.3f, 0.6f, 0.9f);
        var purpleTheme = new Color(0.5f, 0.3f, 0.7f);
        var redTheme = new Color(0.8f, 0.4f, 0.3f);

        if (roomNumber <= 30)
            return blueTheme;

        if (roomNumber <= 45)
        {
            float t = (roomNumber - 30) / 15f;
            return blueTheme.Lerp(purpleTheme, t);
        }

        if (roomNumber <= 60)
            return purpleTheme;

        if (roomNumber <= 75)
        {
            float t = (roomNumber - 60) / 15f;
            return purpleTheme.Lerp(redTheme, t);
        }

        return redTheme;
    }

    /// <summary>
    /// Gets the number of cups for the specified room.
    /// </summary>
    public static int GetCupCountForRoom(int roomNumber)
    {
        if (roomNumber <= 20)
            return 3; // Free tier

        if (roomNumber <= 50)
            return 4; // Early premium

        if (roomNumber <= 75)
            return 5; // Mid premium

        return 6; // Late premium (challenge)
    }

    /// <summary>
    /// Generates cup configurations for procedural spawning.
    /// Uses the provided seed to ensure deterministic replay.
    /// Now includes material assignment based on difficulty progression.
    /// </summary>
    public static CupConfig[] GenerateCups(int roomNumber, int targetCupCount, int seed)
    {
        var random = new Random(seed);
        var cups = new CupConfig[targetCupCount];

        var spawnZones = DefineSpawnZones(targetCupCount);

        for (int i = 0; i < targetCupCount; i++)
        {
            var zone = spawnZones[i];
            Vector2 position;
            float rotation;
            float scale;

            float offsetX = (float)(random.NextDouble() - 0.5) * zone.Spread;
            float offsetY = (float)(random.NextDouble() - 0.5) * zone.Spread;
            position = zone.Center + new Vector2(offsetX, offsetY);

            if (zone.Count > 1)
            {
                rotation = (float)(random.NextDouble() * 0.3 - 0.15);
                scale = 0.9f + (float)random.NextDouble() * 0.2f;
            }
            else
            {
                rotation = (float)(random.NextDouble() * 0.2 - 0.1);
                scale = 0.95f + (float)random.NextDouble() * 0.1f;
            }

            position.Y = Mathf.Max(position.Y, FloorY - 80f);
            position.X = Mathf.Clamp(position.X, SlingshotSafeX + 50f, ExitDoorSafeX - 100f);

            bool isPremium = roomNumber > 20;
            
            // Get material appropriate for difficulty level
            MaterialProperties material = MaterialProperties.GetMaterialForDifficulty(roomNumber, random);
            
            cups[i] = new CupConfig(position, rotation, scale, isPremium, material);
            
            // Debug logging for material assignment
            GD.Print($"Generated cup {i + 1}/{targetCupCount} for room {roomNumber}: " +
                    $"Position={position}, Material={material.Material}, Hardness={material.Hardness}, " +
                    $"HitsToDestroy={material.HitsToDestroy}");
        }

        return cups;
    }

    /// <summary>
    /// Generates cup configurations for a room using a deterministic default seed
    /// unless an explicit override is supplied.
    /// </summary>
    public static CupConfig[] GenerateCups(int roomNumber, int targetCupCount, int? seedOverride = null)
    {
        int seed = seedOverride ?? CalculateSeed(roomNumber);
        return GenerateCups(roomNumber, targetCupCount, seed);
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
            // Early levels (1-20): Mostly wood and stone for gentle introduction
            // Distribution: 70% Wood, 30% Stone
            availableMaterials = (random.NextDouble() < 0.7) ? easyMaterials : new[] { MaterialType.Stone };
        }
        else if (roomNumber <= 50)
        {
            // Mid-early levels (21-50): Stone and brick for skill progression
            // Distribution: 40% Stone, 60% Brick
            availableMaterials = (random.NextDouble() < 0.4) ? new[] { MaterialType.Stone } : mediumMaterials;
        }
        else if (roomNumber <= 80)
        {
            // Mid-late levels (51-80): Brick and iron for challenge
            // Distribution: 30% Brick, 70% Iron
            availableMaterials = (random.NextDouble() < 0.3) ? new[] { MaterialType.Brick } : hardMaterials;
        }
        else
        {
            // Late levels (81+): Iron and diamond for expert players
            // Distribution: 50% Iron, 50% Diamond
            availableMaterials = extremeMaterials;
        }

        // Randomly select from available materials for this difficulty
        return MaterialProperties.GetMaterialProperties(availableMaterials[random.Next(availableMaterials.Length)]);
    }

    private static (Vector2 Center, float Spread, int Count)[] DefineSpawnZones(int cupCount)
    {
        return cupCount switch
        {
            3 =>
            [
                (new Vector2(450f, 480f), 40f, 1),
                (new Vector2(600f, 480f), 40f, 1),
                (new Vector2(750f, 480f), 40f, 1)
            ],
            4 =>
            [
                (new Vector2(450f, 480f), 35f, 1),
                (new Vector2(550f, 480f), 35f, 1),
                (new Vector2(650f, 480f), 35f, 1),
                (new Vector2(750f, 480f), 35f, 1)
            ],
            5 =>
            [
                (new Vector2(400f, 480f), 30f, 1),
                (new Vector2(500f, 480f), 30f, 1),
                (new Vector2(600f, 480f), 30f, 1),
                (new Vector2(700f, 480f), 30f, 1),
                (new Vector2(800f, 480f), 30f, 1)
            ],
            6 =>
            [
                (new Vector2(400f, 480f), 50f, 2),
                (new Vector2(550f, 480f), 30f, 1),
                (new Vector2(650f, 480f), 30f, 1),
                (new Vector2(800f, 480f), 50f, 2)
            ],
            _ => GenerateDynamicZones(cupCount)
        };
    }

    private static (Vector2 Center, float Spread, int Count)[] GenerateDynamicZones(int cupCount)
    {
        var zones = new (Vector2, float, int)[cupCount];
        float spacing = 300f / cupCount;
        float startX = 450f;

        for (int i = 0; i < cupCount; i++)
            zones[i] = (new Vector2(startX + i * spacing, 480f), 30f, 1);

        return zones;
    }

    /// <summary>
    /// Checks if a position is within the safe zone (not blocking gameplay).
    /// </summary>
    public static bool IsPositionSafe(Vector2 position)
    {
        if (position.X < SlingshotSafeX && position.Y > 450f)
            return false;

        if (position.X > ExitDoorSafeX - 100f && position.Y > 450f)
            return false;

        if (position.Y > FloorY - 20f)
            return false;

        return true;
    }

    // Static helper aliases for autoload usage.
    public static ThemeConfig GetTheme(int roomNumber) => GetThemeForRoom(roomNumber);
    public static Color GetBackgroundColor(int roomNumber) => GetInterpolatedBackgroundColor(roomNumber);
    public static int GetCupCount(int roomNumber) => GetCupCountForRoom(roomNumber);
}
