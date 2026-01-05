using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

public class LeaderboardEntry
{
    public string PlayerName { get; set; }
    public int Score { get; set; }
    public DateTime Date { get; set; }

    public LeaderboardEntry(string playerName, int score)
    {
        PlayerName = playerName;
        Score = score;
        Date = DateTime.Now;
    }
}

public partial class LeaderboardManager : Node
{
    public static LeaderboardManager Instance { get; private set; } = null!;
    private const string LeaderboardPath = "user://leaderboard.json";
    
    // Key is level number, value is list of top entries
    private Dictionary<int, List<LeaderboardEntry>> _leaderboards = new();

    public override void _Ready()
    {
        Instance = this;
        Load();
    }

    public void AddEntry(int levelNumber, string playerName, int score)
    {
        if (!_leaderboards.ContainsKey(levelNumber))
        {
            _leaderboards[levelNumber] = new List<LeaderboardEntry>();
        }

        _leaderboards[levelNumber].Add(new LeaderboardEntry(playerName, score));
        
        // Sort by score descending and keep top 10
        _leaderboards[levelNumber] = _leaderboards[levelNumber]
            .OrderByDescending(e => e.Score)
            .Take(10)
            .ToList();
            
        Save();
    }

    public List<LeaderboardEntry> GetTopEntries(int levelNumber)
    {
        if (_leaderboards.ContainsKey(levelNumber))
        {
            return _leaderboards[levelNumber];
        }
        return new List<LeaderboardEntry>();
    }

    private void Save()
    {
        try
        {
            string json = JsonConvert.SerializeObject(_leaderboards, Formatting.Indented);
            using var file = FileAccess.Open(LeaderboardPath, FileAccess.ModeFlags.Write);
            file?.StoreString(json);
        }
        catch (Exception ex)
        {
            GD.PushWarning($"Failed to save leaderboard: {ex.Message}");
        }
    }

    private void Load()
    {
        if (!FileAccess.FileExists(LeaderboardPath)) return;

        try
        {
            using var file = FileAccess.Open(LeaderboardPath, FileAccess.ModeFlags.Read);
            string json = file?.GetAsText() ?? "";
            if (string.IsNullOrEmpty(json)) return;

            _leaderboards = JsonConvert.DeserializeObject<Dictionary<int, List<LeaderboardEntry>>>(json) 
                            ?? new Dictionary<int, List<LeaderboardEntry>>();
        }
        catch (Exception ex)
        {
            GD.PushWarning($"Failed to load leaderboard: {ex.Message}");
        }
    }
}
