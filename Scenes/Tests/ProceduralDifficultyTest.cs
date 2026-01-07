using Godot;
using System;
using System.Linq;

/// <summary>
/// Test scene script for visualizing procedural generation difficulty and material distribution.
/// </summary>
public partial class ProceduralDifficultyTest : Control
{
    private GridContainer _grid;
    private float _timer = 0f;
    private const float RefreshInterval = 1.0f; // Refresh every second for "instant" feedback

    public override void _Ready()
    {
        _grid = GetNode<GridContainer>("GridContainer");
        RefreshTest();
    }

    public override void _Process(double delta)
    {
        _timer += (float)delta;
        if (_timer >= RefreshInterval)
        {
            _timer = 0f;
            RefreshTest();
        }
    }

    public void RefreshTest()
    {
        // Clear existing info
        foreach (Node child in _grid.GetChildren())
        {
            child.QueueFree();
        }

        // Test rooms as specified in the ticket
        int[] testRooms = { 1, 20, 40, 60, 80 };

        foreach (int roomNum in testRooms)
        {
            var roomInfo = CreateRoomInfo(roomNum);
            _grid.AddChild(roomInfo);
        }
    }

    private VBoxContainer CreateRoomInfo(int roomNumber)
    {
        var vbox = new VBoxContainer();
        vbox.CustomMinimumSize = new Vector2(180, 350);

        // Panel for better visibility
        var panel = new Panel();
        panel.CustomMinimumSize = new Vector2(180, 350);
        vbox.AddChild(panel);

        var innerVbox = new VBoxContainer();
        innerVbox.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect, LayoutPresetMode.Minsize, 10);
        panel.AddChild(innerVbox);

        var title = new Label { 
            Text = $"Room {roomNumber}",
            HorizontalAlignment = HorizontalAlignment.Center
        };
        innerVbox.AddChild(title);

        innerVbox.AddChild(new HSeparator());

        var difficulty = DifficultyBalancer.CalculateRoomDifficulty(roomNumber);
        
        var diffLabel = new Label { 
            Text = $"Score: {difficulty.OverallScore:F2}",
            TooltipText = $"Mat: {difficulty.MaterialDifficulty:F2}, Count: {difficulty.ObstacleCountDifficulty:F2}, Layout: {difficulty.LayoutDifficulty:F2}"
        };
        innerVbox.AddChild(diffLabel);

        var descLabel = new Label { 
            Text = $"Desc: {difficulty.Description}",
            ThemeOverrideColorsFontColor = GetDifficultyColor(difficulty.OverallScore)
        };
        innerVbox.AddChild(descLabel);

        var pattern = LevelGenerator.GetPatternForRoom(roomNumber);
        var patternLabel = new Label { Text = $"Pattern: {pattern}" };
        innerVbox.AddChild(patternLabel);

        innerVbox.AddChild(new HSeparator());
        innerVbox.AddChild(new Label { Text = "Materials:" });

        var dist = MaterialDistributor.GetDetailedDistribution(roomNumber);
        for (int i = 0; i < dist.Materials.Length; i++)
        {
            var mat = dist.Materials[i];
            var weight = dist.Weights[i];
            var props = MaterialProperties.GetMaterialProperties(mat);
            
            var hBox = new HBoxContainer();
            var colorRect = new ColorRect { 
                CustomMinimumSize = new Vector2(15, 15), 
                Color = props.BaseColor,
                SizeFlagsVertical = SizeFlags.ShrinkCenter
            };
            var matLabel = new Label { 
                Text = $"{mat}: {weight}%",
                FontSize = 12
            };
            hBox.AddChild(colorRect);
            hBox.AddChild(matLabel);
            innerVbox.AddChild(hBox);

            // Bar visualization
            var bar = new ProgressBar {
                Value = weight,
                MaxValue = 100,
                CustomMinimumSize = new Vector2(0, 10),
                ShowPercentage = false
            };
            innerVbox.AddChild(bar);
        }

        return vbox;
    }

    private Color GetDifficultyColor(float score)
    {
        if (score < 0.3f) return Colors.Green;
        if (score < 0.6f) return Colors.Yellow;
        if (score < 0.85f) return Colors.Orange;
        return Colors.Red;
    }
}
