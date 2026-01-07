using Godot;

/// <summary>
/// Test scene that demonstrates all 5 material types working together.
/// Spawns one obstacle for each material type in a horizontal layout.
/// Useful for testing and debugging the material hardness system.
/// </summary>
public partial class MaterialTestScene : Node2D
{
    /// <summary>
    /// PackedScene for the BreakableObstacle (Cup scene).
    /// </summary>
    [Export] public PackedScene ObstacleScene { get; set; }

    /// <summary>
    /// Horizontal spacing between test obstacles.
    /// </summary>
    [Export] public float ObstacleSpacing = 200f;

    /// <summary>
    /// Y position for all test obstacles.
    /// </summary>
    [Export] public float ObstacleY = 400f;

    /// <summary>
    /// Starting X position for the first obstacle.
    /// </summary>
    [Export] public float StartX = 300f;

    public override void _Ready()
    {
        // Spawn one obstacle for each material type
        SpawnTestObstacles();
        
        // Add a label explaining the test
        CreateTestLabel();
        
        GD.Print("Material test scene loaded. All 5 material types should be visible.");
    }

    /// <summary>
    /// Spawns test obstacles for each material type.
    /// </summary>
    private void SpawnTestObstacles()
    {
        if (ObstacleScene == null)
        {
            GD.PushError("MaterialTestScene: ObstacleScene not set.");
            return;
        }

        var materials = MaterialProperties.GetAllMaterials();

        for (int i = 0; i < materials.Length; i++)
        {
            var material = materials[i];
            var obstacle = SpawnObstacle(material, i);
            
            // Add to scene
            AddChild(obstacle);
            
            GD.Print($"Spawned {material.Material} obstacle: Hardness={material.Hardness}, " +
                    $"HitsToDestroy={material.HitsToDestroy}, Color={material.BaseColor}");
        }
    }

    /// <summary>
    /// Creates and configures a single test obstacle.
    /// </summary>
    /// <param name="material">Material properties to apply.</param>
    /// <param name="index">Index for positioning.</param>
    /// <returns>Configured obstacle node.</returns>
    private Node2D SpawnObstacle(MaterialProperties material, int index)
    {
        // Instantiate the obstacle
        var obstacle = ObstacleScene.Instantiate<Node2D>();
        
        // Set position based on index
        float x = StartX + (index * ObstacleSpacing);
        obstacle.Position = new Vector2(x, ObstacleY);
        
        // Set name for debugging
        obstacle.Name = $"TestObstacle_{material.Material}";
        
        // Apply material if the obstacle supports it
        if (obstacle is BreakableObstacle breakableObstacle)
        {
            breakableObstacle.SetMaterial(material);
            breakableObstacle.UseProceduralMaterial = false; // Use our specific material
            
            // Add debug info to score for tracking
            breakableObstacle.ScoreValue = 100 + (material.Hardness * 50);
        }
        
        return obstacle;
    }

    /// <summary>
    /// Creates a label explaining the test layout.
    /// </summary>
    private void CreateTestLabel()
    {
        var label = new Label();
        label.Name = "TestLabel";
        label.Position = new Vector2(50, 50);
        label.Text = "Material Hardness Test Scene\n" +
                     "Left to Right: Wood → Stone → Brick → Iron → Diamond\n" +
                     "Each obstacle shows:\n" +
                     "• Different base color\n" +
                     "• Varying hardness (1-5 hits)\n" +
                     "• Distinct visual feedback\n" +
                     "• Material-appropriate destruction\n" +
                     "\nClick or hit obstacles to test damage!";
        
        AddChild(label);
    }

    /// <summary>
    /// Handles input for testing damage to obstacles.
    /// Left click on any obstacle to apply damage.
    /// </summary>
    public override void _Input(event)
    {
        if (event is InputEventMouseButton mouseButton && 
            mouseButton.ButtonIndex == MouseButton.Left && 
            mouseButton.Pressed)
        {
            // Get the clicked position
            Vector2 clickPosition = mouseButton.Position;
            
            // Find obstacles under the click
            var obstacles = GetTree().GetNodesInGroup("obstacle");
            foreach (Node obstacle in obstacles)
            {
                if (obstacle is BreakableObstacle breakable)
                {
                    float distance = obstacle.GlobalPosition.DistanceTo(GlobalPosition + clickPosition);
                    if (distance < 50f) // Click tolerance
                    {
                        // Apply damage
                        breakable.Hit(10);
                        GD.Print($"Applied test damage to {breakable.Material.Material} obstacle. " +
                                $"Hits taken: {breakable.CurrentHitsTaken}/{breakable.Material.HitsToDestroy}");
                        break;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Gets a summary of all spawned obstacles for debugging.
    /// </summary>
    public string GetObstacleSummary()
    {
        var summary = new System.Text.StringBuilder();
        summary.AppendLine("Material Test Scene Obstacle Summary:");
        
        var obstacles = GetTree().GetNodesInGroup("obstacle");
        foreach (Node obstacle in obstacles)
        {
            if (obstacle is BreakableObstacle breakable)
            {
                summary.AppendLine($"• {breakable.Material.Material}: " +
                                $"HP={breakable.CurrentHp}/{breakable.MaxHp}, " +
                                $"Hits={breakable.CurrentHitsTaken}/{breakable.Material.HitsToDestroy}");
            }
        }
        
        return summary.ToString();
    }
}