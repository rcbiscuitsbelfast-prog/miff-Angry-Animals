using System;
using Godot;

/// <summary>
/// Detects the legendary (X6 perfect) condition and inserts the SixToSeven minigame before X7.
/// Designed to be used as an AutoLoad.
/// </summary>
public partial class MemeGateway : Node
{
    public static MemeGateway Instance { get; private set; } = null!;

    [Export] public string MinigameScenePath { get; set; } = "res://Scenes/MemeGames/SixToSevenMinigame.tscn";

    public override void _Ready()
    {
        Instance = this;
        ProcessMode = ProcessModeEnum.Always;
    }

    public bool ShouldTriggerForLevel(int levelNumber, int finalScore)
    {
        if (GameManager.Instance == null)
            return false;

        if (levelNumber < 1 || levelNumber > GameManager.TotalLevels)
            return false;

        if (levelNumber % 10 != 6)
            return false;

        int roomIndex = levelNumber - 1;
        int optimal = GameManager.Instance.Rooms[roomIndex].OptimalScore;

        // "Perfect" maps to a 3-star threshold.
        return finalScore >= optimal * 0.9f;
    }

    /// <summary>
    /// Called by LevelCompleted when the user presses Next.
    /// Returns true if the gateway consumed the transition.
    /// </summary>
    public bool TryPlayMinigameThenLoadNext(int currentLevelNumber)
    {
        if (GameManager.Instance == null)
            return false;

        int currentRoomIndex = currentLevelNumber - 1;
        int nextRoomIndex = currentRoomIndex + 1;

        if (nextRoomIndex >= GameManager.TotalLevels)
            return false;

        int finalScore = ScoreManager.GetScore();

        if (!ShouldTriggerForLevel(currentLevelNumber, finalScore))
            return false;

        NotificationManager.Instance?.SendInstantNotification(
            "6 PERFECT??",
            "7 SPEEDRUN MINI-GAME INCOMING!");

        PlayMinigame(() => GameManager.StartRoom(nextRoomIndex));
        return true;
    }

    private void PlayMinigame(Action onFinished)
    {
        if (!ResourceLoader.Exists(MinigameScenePath))
        {
            onFinished?.Invoke();
            return;
        }

        var scene = ResourceLoader.Load<PackedScene>(MinigameScenePath);
        if (scene == null)
        {
            onFinished?.Invoke();
            return;
        }

        var node = scene.Instantiate<Node>();

        if (node is SixToSevenMinigame minigame)
        {
            minigame.Finished += () =>
            {
                AwardMemeReward();
                minigame.QueueFree();
                onFinished?.Invoke();
            };
        }
        else
        {
            node.TreeExited += () =>
            {
                AwardMemeReward();
                onFinished?.Invoke();
            };
        }

        GetTree().Root.AddChild(node);
    }

    private void AwardMemeReward()
    {
        // Reward is a cosmetic unlock.
        // These are string ids; actual visuals can be wired later.
        string[] rewards =
        {
            "meme_hat_speedrun",
            "meme_hat_brain",
            "meme_badge_perfect",
            "meme_expression_perfect??",
            "meme_hat_yeet_crown"
        };

        if (PlayerProfile.Instance == null)
            return;

        foreach (var reward in rewards)
        {
            if (!PlayerProfile.Instance.UnlockedCosmetics.Contains(reward))
            {
                PlayerProfile.Instance.UnlockCosmetic(reward);
                return;
            }
        }

        // If everything is already unlocked, re-award a random one.
        PlayerProfile.Instance.UnlockCosmetic(rewards[(int)GD.RandRange(0, rewards.Length - 1)]);
    }
}
