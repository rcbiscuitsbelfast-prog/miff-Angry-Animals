using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Singleton that determines material distribution for procedural levels based on room difficulty.
/// Allows for fine-tuning of material progression and ensure balanced gameplay.
/// </summary>
public partial class MaterialDistributor : Node
{
    public static MaterialDistributor Instance { get; private set; }

    [ExportGroup("Toughness Factors")]
    [Export] public float EasyModeToughnessFactor = 0.7f;
    [Export] public float MediumModeToughnessFactor = 1.0f;
    [Export] public float HardModeToughnessFactor = 1.3f;

    [ExportGroup("Debug")]
    [Export] public bool EnableDebugLogging = true;

    [ExportGroup("Difficulty Distributions")]
    [Export] public Vector3 EasyDistribution = new Vector3(70, 20, 10); // Wood, Stone, Brick
    [Export] public Vector4 MediumDistribution = new Vector4(30, 40, 20, 10); // Wood, Stone, Brick, Iron
    [Export] public Vector4 HardDistribution = new Vector4(20, 30, 40, 10); // Stone, Brick, Iron, Diamond
    [Export] public Vector3 ExtremeDistribution = new Vector3(10, 40, 50); // Brick, Iron, Diamond

    public override void _Ready()
    {
        Instance = this;
    }

    /// <summary>
    /// Gets an array of materials for a specific room and number of obstacles.
    /// </summary>
    public static MaterialType[] GetMaterialsForRoom(int roomNumber, int obstacleCount)
    {
        // Use a deterministic seed derived from the room number, but offset to avoid correlation with layout
        var random = new Random(LevelGenerator.CalculateSeed(roomNumber) + 555);
        var materials = new MaterialType[obstacleCount];
        var distribution = GetDetailedDistribution(roomNumber);

        for (int i = 0; i < obstacleCount; i++)
        {
            materials[i] = distribution.PickRandomMaterial(random);
        }

        // Ensure at least one varied material per room if count > 1
        if (obstacleCount > 1 && materials.All(m => m == materials[0]))
        {
            var available = distribution.Materials.Where(m => m != materials[0]).ToArray();
            if (available.Length > 0)
            {
                materials[random.Next(materials.Length)] = available[random.Next(available.Length)];
            }
        }

        return materials;
    }

    /// <summary>
    /// Returns a softness value from 0.0 (all hard) to 1.0 (all soft).
    /// </summary>
    public static float GetDifficultySoftness(int roomNumber)
    {
        var dist = GetDetailedDistribution(roomNumber);
        float weightedSum = 0;
        float totalWeight = 0;

        for (int i = 0; i < dist.Materials.Length; i++)
        {
            weightedSum += (int)dist.Materials[i] * dist.Weights[i];
            totalWeight += dist.Weights[i];
        }

        if (totalWeight == 0) return 1.0f;

        float avgHardness = weightedSum / totalWeight;
        // Hardness ranges from 1 (Wood) to 5 (Diamond)
        // Softness 1.0 at hardness 1, 0.0 at hardness 5
        return Mathf.Clamp((5f - avgHardness) / 4f, 0f, 1f);
    }

    public struct MaterialDistribution
    {
        public MaterialType[] Materials;
        public float[] Weights;

        public MaterialType PickRandomMaterial(Random random)
        {
            float totalWeight = Weights.Sum();
            if (totalWeight <= 0) return Materials[0];

            float r = (float)random.NextDouble() * totalWeight;
            float current = 0;
            for (int i = 0; i < Materials.Length; i++)
            {
                current += Weights[i];
                if (r <= current) return Materials[i];
            }
            return Materials[Materials.Length - 1];
        }
    }

    /// <summary>
    /// Returns the detailed material distribution for a specific room.
    /// </summary>
    public static MaterialDistribution GetDetailedDistribution(int roomNumber)
    {
        // Fallback if Instance is not yet initialized (e.g. in editor or during early init)
        var easy = Instance?.EasyDistribution ?? new Vector3(70, 20, 10);
        var medium = Instance?.MediumDistribution ?? new Vector4(30, 40, 20, 10);
        var hard = Instance?.HardDistribution ?? new Vector4(20, 30, 40, 10);
        var extreme = Instance?.ExtremeDistribution ?? new Vector3(10, 40, 50);

        if (roomNumber <= 20)
        {
            return new MaterialDistribution
            {
                Materials = new[] { MaterialType.Wood, MaterialType.Stone, MaterialType.Brick },
                Weights = new[] { easy.X, easy.Y, easy.Z }
            };
        }
        else if (roomNumber <= 40)
        {
            return new MaterialDistribution
            {
                Materials = new[] { MaterialType.Wood, MaterialType.Stone, MaterialType.Brick, MaterialType.Iron },
                Weights = new[] { medium.X, medium.Y, medium.Z, medium.W }
            };
        }
        else if (roomNumber <= 60)
        {
            return new MaterialDistribution
            {
                Materials = new[] { MaterialType.Stone, MaterialType.Brick, MaterialType.Iron, MaterialType.Diamond },
                Weights = new[] { hard.X, hard.Y, hard.Z, hard.W }
            };
        }
        else
        {
            return new MaterialDistribution
            {
                Materials = new[] { MaterialType.Brick, MaterialType.Iron, MaterialType.Diamond },
                Weights = new[] { extreme.X, extreme.Y, extreme.Z }
            };
        }
    }
}
