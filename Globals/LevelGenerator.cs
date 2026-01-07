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
        public readonly MaterialType Material;
        public readonly ObstaclePattern Pattern;
        public readonly float DifficultyCoefficient;

        public CupConfig(Vector2 position, float rotation, float scale, bool isPremium, MaterialType material, ObstaclePattern pattern, float difficultyCoefficient)
        {
            Position = position;
            Rotation = rotation;
            Scale = scale;
            IsPremium = isPremium;
            Material = material;
            Pattern = pattern;
            DifficultyCoefficient = difficultyCoefficient;
        }
    }

    public enum ObstaclePattern { Tower, Wall, Scattered }

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
        return CreateSeedFromParameters(roomNumber);
    }

    /// <summary>
    /// Creates a deterministic seed from parameters.
    /// </summary>
    public static int CreateSeedFromParameters(int roomNumber, int customLayout = -1, int materialVariant = -1)
    {
        int layout = customLayout != -1 ? customLayout : (roomNumber % 3);
        int variant = materialVariant != -1 ? materialVariant : (roomNumber % 100);
        
        // Encode into 32-bit int
        // Bits 0-15: Room Number
        // Bits 16-17: Layout (Pattern)
        // Bits 18-27: Variant
        int seed = (roomNumber & 0xFFFF) | ((layout & 0x3) << 16) | ((variant & 0x3FF) << 18);
        return seed;
    }

    /// <summary>
    /// Decodes a seed into its component parameters.
    /// </summary>
    public static bool TryDecodeSeedToParameters(int seed, out int roomNumber, out int layout, out int variant)
    {
        roomNumber = seed & 0xFFFF;
        layout = (seed >> 16) & 0x3;
        variant = (seed >> 18) & 0x3FF;
        return true;
    }

    /// <summary>
    /// Encodes a room as a shareable code (Base64 preparation).
    /// </summary>
    public static string EncodeRoomAsShareCode(int roomNumber)
    {
        int seed = CreateSeedFromParameters(roomNumber);
        byte[] bytes = BitConverter.GetBytes(seed);
        return Convert.ToBase64String(bytes).Replace("=", "");
    }

    /// <summary>
    /// Gets the obstacle pattern for a room based on its seed.
    /// </summary>
    public static ObstaclePattern GetPatternForRoom(int roomNumber)
    {
        int seed = CreateSeedFromParameters(roomNumber);
        TryDecodeSeedToParameters(seed, out _, out int layout, out _);
        return (ObstaclePattern)layout;
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
    /// Gets the number of cups for the specified room, balanced by material hardness.
    /// </summary>
    public static int GetCupCountForRoom(int roomNumber)
    {
        int maxObstacles = DifficultyBalancer.GetRecommendedMaxObstacles(roomNumber);
        
        // Base count scales with room progression
        int baseCount = 3 + (roomNumber / 10);
        
        // Ensure it doesn't exceed recommended max for hardness
        return Math.Clamp(baseCount, 1, maxObstacles);
    }

    /// <summary>
    /// Generates cup configurations with materials and difficulty balancing.
    /// </summary>
    public static CupConfig[] GenerateCupsWithMaterials(int roomNumber, int cupCount, int seed)
    {
        var random = new Random(seed);
        TryDecodeSeedToParameters(seed, out _, out int layoutIndex, out int variant);
        
        ObstaclePattern pattern = (ObstaclePattern)layoutIndex;
        MaterialType[] materials = MaterialDistributor.GetMaterialsForRoom(roomNumber, cupCount);
        
        // Sort materials by hardness for patterns that benefit from it
        var sortedByHardness = materials.OrderBy(m => (int)m).ToArray();
        
        var cups = new CupConfig[cupCount];
        float difficultyCoeff = DifficultyBalancer.CalculateRoomDifficulty(roomNumber).OverallScore;

        switch (pattern)
        {
            case ObstaclePattern.Tower:
                GenerateTowerPattern(cups, roomNumber, cupCount, sortedByHardness, random, difficultyCoeff);
                break;
            case ObstaclePattern.Wall:
                GenerateWallPattern(cups, roomNumber, cupCount, materials, random, difficultyCoeff);
                break;
            case ObstaclePattern.Scattered:
                GenerateScatteredPattern(cups, roomNumber, cupCount, materials, random, difficultyCoeff);
                break;
        }

        return cups;
    }

    private static void GenerateTowerPattern(CupConfig[] cups, int roomNumber, int cupCount, MaterialType[] materials, Random random, float difficulty)
    {
        float centerX = (SlingshotSafeX + ExitDoorSafeX) / 2f + (float)(random.NextDouble() - 0.5) * 100f;
        float baseY = FloorY - 40f;
        float verticalSpacing = 60f;

        for (int i = 0; i < cupCount; i++)
        {
            // Tower pattern: single tall stack, mostly soft materials at bottom (if sorted)
            // Note: materials here are sorted by hardness, so materials[0] is softest.
            Vector2 pos = new Vector2(centerX + (float)(random.NextDouble() - 0.5) * 20f, baseY - (i * verticalSpacing));
            cups[i] = new CupConfig(pos, (float)(random.NextDouble() * 0.1 - 0.05), 1.0f, roomNumber > 20, materials[i], ObstaclePattern.Tower, difficulty);
        }
    }

    private static void GenerateWallPattern(CupConfig[] cups, int roomNumber, int cupCount, MaterialType[] materials, Random random, float difficulty)
    {
        float startX = SlingshotSafeX + 100f;
        float endX = ExitDoorSafeX - 100f;
        float width = endX - startX;
        float spacing = width / (cupCount + 1);

        for (int i = 0; i < cupCount; i++)
        {
            // Wall pattern: horizontal barrier, mixed hardness
            float x = startX + (i + 1) * spacing + (float)(random.NextDouble() - 0.5) * 30f;
            float softness = MaterialDistributor.GetDifficultySoftness(roomNumber);
            float y = FloorY - 40f - (1.0f - softness) * 50f + (float)(random.NextDouble() - 0.5) * 40f;
            
            cups[i] = new CupConfig(new Vector2(x, y), (float)(random.NextDouble() * 0.2 - 0.1), 1.0f, roomNumber > 20, materials[i], ObstaclePattern.Wall, difficulty);
        }
    }

    private static void GenerateScatteredPattern(CupConfig[] cups, int roomNumber, int cupCount, MaterialType[] materials, Random random, float difficulty)
    {
        for (int i = 0; i < cupCount; i++)
        {
            MaterialType mat = materials[i];
            Vector2 pos;
            bool isHard = (int)mat >= (int)MaterialType.Iron;
            
            if (isHard)
            {
                // Hard materials: cluster near center, slightly elevated
                float centerX = (SlingshotSafeX + ExitDoorSafeX) / 2f;
                pos = new Vector2(centerX + (float)(random.NextDouble() - 0.5) * 200f, FloorY - 150f - (float)random.NextDouble() * 100f);
            }
            else
            {
                // Soft materials: spread across room, varied heights
                pos = new Vector2(
                    Mathf.Clamp(SlingshotSafeX + 50f + (float)random.NextDouble() * (ExitDoorSafeX - SlingshotSafeX - 150f), SlingshotSafeX + 50f, ExitDoorSafeX - 100f),
                    FloorY - 80f - (float)random.NextDouble() * 200f
                );
            }
            
            cups[i] = new CupConfig(pos, (float)(random.NextDouble() * 0.4 - 0.2), 1.0f, roomNumber > 20, mat, ObstaclePattern.Scattered, difficulty);
        }
    }

    /// <summary>
    /// Generates cup configurations for procedural spawning.
    /// Uses the provided seed to ensure deterministic replay.
    /// </summary>
    public static CupConfig[] GenerateCups(int roomNumber, int targetCupCount, int seed)
    {
        return GenerateCupsWithMaterials(roomNumber, targetCupCount, seed);
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
