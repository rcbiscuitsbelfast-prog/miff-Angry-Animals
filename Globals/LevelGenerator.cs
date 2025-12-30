using System;
using Godot;

/// <summary>
/// Procedural level generator that creates varied cup configurations and visual themes.
/// Uses seeded RNG based on room number for consistent replay.
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
    /// Cup configuration for procedural spawning.
    /// </summary>
    public readonly struct CupConfig
    {
        public readonly Vector2 Position;
        public readonly float Rotation;
        public readonly float Scale;
        public readonly bool IsPremium;

        public CupConfig(Vector2 position, float rotation, float scale, bool isPremium)
        {
            Position = position;
            Rotation = rotation;
            Scale = scale;
            IsPremium = isPremium;
        }
    }

    // Safe zone boundaries (keep clear for gameplay)
    private const float SlingshotSafeX = 300f;
    private const float ExitDoorSafeX = 900f;
    private const float FloorY = 530f;

    // Slingshot position reference
    private static readonly Vector2 SlingshotPosition = new Vector2(200f, 500f);

    // Exit door position reference
    private static readonly Vector2 ExitDoorPosition = new Vector2(1000f, 530f);

    public override void _Ready()
    {
        Instance = this;
        ProcessMode = ProcessModeEnum.Always;
    }

    // Seed offset for randomization
    private int _roomNumber;

    // Cached seed for this room
    private int _cachedSeed;

    /// <summary>
    /// Creates a level generator for the specified room number.
    /// </summary>
    public LevelGenerator(int roomNumber)
    {
        _roomNumber = roomNumber;
        _cachedSeed = CalculateSeed(roomNumber);
    }

    /// <summary>
    /// Creates a deterministic seed from the room number.
    /// </summary>
    private static int CalculateSeed(int roomNumber)
    {
        // Simple hash: room number * prime + offset
        // This ensures the same room always generates the same layout
        return roomNumber * 73856093 ^ 19349663;
    }

    /// <summary>
    /// Gets the seeded Random instance for this room.
    /// </summary>
    private Random GetRandom()
    {
        // Create a new Random with the cached seed
        // Using a simple seed-based approach
        return new Random(_cachedSeed);
    }

    /// <summary>
    /// Regenerates the room seed for variety (optional, called when fresh layout needed).
    /// </summary>
    public void RegenerateSeed()
    {
        _cachedSeed = CalculateSeed(_roomNumber) + (int)Time.GetTicksMsec();
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
                new Color(0.3f, 0.6f, 0.9f),  // Blue background
                new Color(0.4f, 0.5f, 0.3f),  // Green-tinted floor
                false,                         // No premium effects
                "Blue"
            );
        }
        // Level 31-60: Purple theme
        else if (roomNumber <= 60)
        {
            return new ThemeConfig(
                new Color(0.5f, 0.3f, 0.7f),  // Purple background
                new Color(0.4f, 0.3f, 0.5f),  // Purple-tinted floor
                true,                          // Premium effects
                "Purple"
            );
        }
        // Level 61-100: Red/Orange theme
        else
        {
            return new ThemeConfig(
                new Color(0.8f, 0.4f, 0.3f),  // Red/Orange background
                new Color(0.5f, 0.3f, 0.2f),  // Red-tinted floor
                true,                          // Premium effects
                "Red"
            );
        }
    }

    /// <summary>
    /// Gets the interpolated background color between themes.
    /// </summary>
    public static Color GetInterpolatedBackgroundColor(int roomNumber)
    {
        // Get base colors
        var blueTheme = new Color(0.3f, 0.6f, 0.9f);
        var purpleTheme = new Color(0.5f, 0.3f, 0.7f);
        var redTheme = new Color(0.8f, 0.4f, 0.3f);

        if (roomNumber <= 30)
        {
            // Pure blue theme
            return blueTheme;
        }
        else if (roomNumber <= 45)
        {
            // Interpolate blue to purple
            float t = (roomNumber - 30) / 15f;
            return blueTheme.Lerp(purpleTheme, t);
        }
        else if (roomNumber <= 60)
        {
            // Pure purple theme
            return purpleTheme;
        }
        else if (roomNumber <= 75)
        {
            // Interpolate purple to red
            float t = (roomNumber - 60) / 15f;
            return purpleTheme.Lerp(redTheme, t);
        }
        else
        {
            // Pure red theme
            return redTheme;
        }
    }

    /// <summary>
    /// Generates cup configurations for the specified room.
    /// Uses seeded RNG for consistent replay while providing variety.
    /// </summary>
    public CupConfig[] GenerateCupConfigs(int targetCupCount)
    {
        var random = GetRandom();
        var cups = new CupConfig[targetCupCount];

        // Define spawn zones (areas where cups can be placed)
        var spawnZones = DefineSpawnZones(targetCupCount);

        for (int i = 0; i < targetCupCount; i++)
        {
            var zone = spawnZones[i];
            Vector2 position;
            float rotation;
            float scale;

            if (zone.Count > 1)
            {
                // Place multiple cups in this zone with variation
                float offsetX = (float)(random.NextDouble() - 0.5) * zone.Spread;
                float offsetY = (float)(random.NextDouble() - 0.5) * zone.Spread;
                position = zone.Center + new Vector2(offsetX, offsetY);
                rotation = (float)(random.NextDouble() * 0.3 - 0.15); // Small rotation variation
                scale = 0.9f + (float)random.NextDouble() * 0.2f; // Scale between 0.9 and 1.1
            }
            else
            {
                // Single cup in zone
                float offsetX = (float)(random.NextDouble() - 0.5) * zone.Spread;
                float offsetY = (float)(random.NextDouble() - 0.5) * zone.Spread;
                position = zone.Center + new Vector2(offsetX, offsetY);
                rotation = (float)(random.NextDouble() * 0.2 - 0.1);
                scale = 0.95f + (float)random.NextDouble() * 0.1f;
            }

            // Ensure position is above floor and within bounds
            position.Y = Mathf.Max(position.Y, FloorY - 80f);
            position.X = Mathf.Clamp(position.X, SlingshotSafeX + 50f, ExitDoorSafeX - 100f);

            bool isPremium = _roomNumber > 20;
            cups[i] = new CupConfig(position, rotation, scale, isPremium);
        }

        return cups;
    }

    /// <summary>
    /// Defines spawn zones for cups based on target count.
    /// </summary>
    private static (Vector2 Center, float Spread, int Count)[] DefineSpawnZones(int cupCount)
    {
        return cupCount switch
        {
            3 => new[]
            {
                (new Vector2(450f, 480f), 40f, 1),
                (new Vector2(600f, 480f), 40f, 1),
                (new Vector2(750f, 480f), 40f, 1)
            },
            4 => new[]
            {
                (new Vector2(450f, 480f), 35f, 1),
                (new Vector2(550f, 480f), 35f, 1),
                (new Vector2(650f, 480f), 35f, 1),
                (new Vector2(750f, 480f), 35f, 1)
            },
            5 => new[]
            {
                (new Vector2(400f, 480f), 30f, 1),
                (new Vector2(500f, 480f), 30f, 1),
                (new Vector2(600f, 480f), 30f, 1),
                (new Vector2(700f, 480f), 30f, 1),
                (new Vector2(800f, 480f), 30f, 1)
            },
            6 => new[]
            {
                (new Vector2(400f, 480f), 50f, 2),
                (new Vector2(550f, 480f), 30f, 1),
                (new Vector2(650f, 480f), 30f, 1),
                (new Vector2(800f, 480f), 50f, 2)
            },
            _ => GenerateDynamicZones(cupCount)
        };
    }

    /// <summary>
    /// Generates dynamic spawn zones for non-standard cup counts.
    /// </summary>
    private static (Vector2 Center, float Spread, int Count)[] GenerateDynamicZones(int cupCount)
    {
        var zones = new (Vector2, float, int)[cupCount];
        float spacing = 300f / cupCount;
        float startX = 450f;

        for (int i = 0; i < cupCount; i++)
        {
            zones[i] = (new Vector2(startX + i * spacing, 480f), 30f, 1);
        }

        return zones;
    }

    /// <summary>
    /// Gets the number of cups for the specified room.
    /// </summary>
    public static int GetCupCountForRoom(int roomNumber)
    {
        if (roomNumber <= 20)
            return 3; // Free tier
        else if (roomNumber <= 50)
            return 4; // Early premium
        else if (roomNumber <= 75)
            return 5; // Mid premium
        else
            return 6; // Late premium (challenge)
    }

    /// <summary>
    /// Checks if a position is within the safe zone (not blocking gameplay).
    /// </summary>
    public static bool IsPositionSafe(Vector2 position)
    {
        // Check slingshot area
        if (position.X < SlingshotSafeX && position.Y > 450f)
            return false;

        // Check exit door area
        if (position.X > ExitDoorSafeX - 100f && position.Y > 450f)
            return false;

        // Check floor boundary
        if (position.Y > FloorY - 20f)
            return false;

        return true;
    }

    /// <summary>
    /// Gets the room number this generator is configured for.
    /// </summary>
    public int RoomNumber => _roomNumber;

    /// <summary>
    /// Gets whether this is a premium room (levels 21+).
    /// </summary>
    public bool IsPremiumRoom => _roomNumber > 20;

    // ============================================================================
    // Static Helper Methods for Autoload Usage
    // ============================================================================

    /// <summary>
    /// Gets the visual theme for the specified room number (static helper).
    /// </summary>
    public static ThemeConfig GetTheme(int roomNumber) => GetThemeForRoom(roomNumber);

    /// <summary>
    /// Gets the interpolated background color for the specified room (static helper).
    /// </summary>
    public static Color GetBackgroundColor(int roomNumber) => GetInterpolatedBackgroundColor(roomNumber);

    /// <summary>
    /// Gets the number of cups for the specified room (static helper).
    /// </summary>
    public static int GetCupCount(int roomNumber) => GetCupCountForRoom(roomNumber);

    /// <summary>
    /// Generates cup configurations for procedural spawning (static helper).
    /// Uses the room number to seed the RNG for consistent replay.
    /// </summary>
    public static CupConfig[] GenerateCups(int roomNumber, int targetCupCount)
    {
        var generator = new LevelGenerator(roomNumber);
        return generator.GenerateCupConfigs(targetCupCount);
    }

    /// <summary>
    /// Checks if a position is safe for cup placement (static helper).
    /// </summary>
    public static bool IsPositionSafe(Vector2 position)
    {
        // Check slingshot area
        if (position.X < SlingshotSafeX && position.Y > 450f)
            return false;

        // Check exit door area
        if (position.X > ExitDoorSafeX - 100f && position.Y > 450f)
            return false;

        // Check floor boundary
        if (position.Y > FloorY - 20f)
            return false;

        return true;
    }
}
