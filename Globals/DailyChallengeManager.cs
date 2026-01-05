using Godot;
using System;

public partial class DailyChallengeManager : Node
{
    public static DailyChallengeManager Instance { get; private set; } = null!;

    public override void _Ready()
    {
        Instance = this;
    }

    public int GetDailySeed()
    {
        // Use current date as seed
        DateTime now = DateTime.UtcNow;
        return now.Year * 10000 + now.Month * 100 + now.Day;
    }

    public void StartDailyChallenge()
    {
        int seed = GetDailySeed();
        // Set a random level number for variety, but deterministic for the day
        int levelNumber = (seed % 100) + 1;
        
        if (PlayerProfile.Instance != null)
        {
            PlayerProfile.Instance.UseProceduralLevels = true;
            PlayerProfile.Instance.LastProceduralSeed = seed;
            PlayerProfile.Instance.LastProceduralLevelNumber = levelNumber;
            PlayerProfile.Instance.Save();
        }

        GameManager.StartRoomByLevelNumber(levelNumber);
    }
}
