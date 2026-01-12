using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

/// <summary>
/// Manages global leaderboards with Firebase integration
/// Tracks top scores per level and global rankings
/// </summary>
public partial class GlobalLeaderboard : Node
{
    public static GlobalLeaderboard Instance { get; private set; }
    
    private const string LeaderboardDataPath = "user://leaderboard_cache.json";
    private const int Top100Count = 100;
    private const float SyncIntervalMinutes = 5.0f;
    
    // Leaderboard data
    private GlobalLeaderboardCollection _leaderboards = new();
    private DateTime _lastSyncTime = DateTime.MinValue;
    private bool _isSyncing = false;
    
    // Signals
    [Signal]
    public delegate void LeaderboardUpdatedEventHandler(LeaderboardType type);
    
    [Signal]
    public delegate void LeaderboardSyncStartedEventHandler();
    
    [Signal]
    public delegate void LeaderboardSyncCompletedEventHandler(bool success);
    
    [Signal]
    public delegate void PlayerRankChangedEventHandler(LeaderboardType type, int oldRank, int newRank);
    
    public override void _Ready()
    {
        if (Instance != null)
        {
            QueueFree();
            return;
        }
        
        Instance = this;
        ProcessMode = ProcessModeEnum.Always;
        
        LoadCachedLeaderboards();
        
        // Start periodic sync
        var timer = GetTree().CreateTimer(SyncIntervalMinutes * 60);
        timer.Timeout += OnSyncTimerTimeout;
        
        GD.Print("Global Leaderboard initialized");
    }
    
    public override void _ExitTree()
    {
        SaveLeaderboardCache();
    }
    
    /// <summary>
    /// Submit score to global leaderboard
    /// </summary>
    public void SubmitScore(string levelId, string levelName, int score, int stars, float completionTime, string replayId = "")
    {
        var entry = new LeaderboardEntry
        {
            PlayerId = GetCurrentPlayerId(),
            PlayerName = GetCurrentPlayerName(),
            Score = score,
            Stars = stars,
            CompletionTime = completionTime,
            DateAchieved = DateTime.UtcNow,
            Cosmetics = GetCurrentPlayerCosmetics(),
            ReplayId = replayId,
            IsCurrentPlayer = true
        };
        
        // Update level-specific leaderboard
        UpdateLevelLeaderboard(levelId, levelName, entry);
        
        // Update total score leaderboard
        UpdateTotalScoreLeaderboard();
        
        // Update perfect levels count if applicable
        if (stars >= 5)
        {
            UpdatePerfectLevelsLeaderboard();
        }
        
        // Sync to server (async)
        SyncToServer(levelId, entry);
        
        GD.Print($"Submitted score: {score} on {levelName}");
    }
    
    /// <summary>
    /// Update level-specific leaderboard
    /// </summary>
    private void UpdateLevelLeaderboard(string levelId, string levelName, LeaderboardEntry entry)
    {
        if (!_leaderboards.ByLevel.ContainsKey(levelId))
        {
            _leaderboards.ByLevel[levelId] = new LeaderboardData
            {
                LeaderboardId = levelId,
                Type = LeaderboardType.ByLevel,
                LevelId = levelId
            };
        }
        
        var leaderboard = _leaderboards.ByLevel[levelId];
        
        // Check if player already has an entry
        var existingEntry = leaderboard.Entries.Find(e => e.PlayerId == entry.PlayerId);
        
        if (existingEntry != null)
        {
            // Update if score is higher
            if (entry.Score > existingEntry.Score)
            {
                leaderboard.Entries.Remove(existingEntry);
                leaderboard.Entries.Add(entry);
            }
        }
        else
        {
            leaderboard.Entries.Add(entry);
        }
        
        // Sort and limit to top 100
        leaderboard.Entries = leaderboard.Entries
            .OrderByDescending(e => e.Score)
            .Take(Top100Count)
            .ToList();
        
        // Assign ranks
        for (int i = 0; i < leaderboard.Entries.Count; i++)
        {
            leaderboard.Entries[i].Rank = i + 1;
        }
        
        leaderboard.LastUpdated = DateTime.UtcNow;
        SaveLeaderboardCache();
        
        EmitSignal(SignalName.LeaderboardUpdated, (int)LeaderboardType.ByLevel);
    }
    
    /// <summary>
    /// Update total score leaderboard
    /// </summary>
    private void UpdateTotalScoreLeaderboard()
    {
        var totalScore = CalculatePlayerTotalScore();
        
        var entry = new LeaderboardEntry
        {
            PlayerId = GetCurrentPlayerId(),
            PlayerName = GetCurrentPlayerName(),
            Score = totalScore,
            DateAchieved = DateTime.UtcNow,
            Cosmetics = GetCurrentPlayerCosmetics(),
            IsCurrentPlayer = true
        };
        
        var leaderboard = _leaderboards.TotalScore;
        leaderboard.Type = LeaderboardType.TotalScore;
        
        // Remove existing entry
        var existingEntry = leaderboard.Entries.Find(e => e.PlayerId == entry.PlayerId);
        if (existingEntry != null)
        {
            leaderboard.Entries.Remove(existingEntry);
        }
        
        leaderboard.Entries.Add(entry);
        
        // Sort and limit
        leaderboard.Entries = leaderboard.Entries
            .OrderByDescending(e => e.Score)
            .Take(Top100Count)
            .ToList();
        
        // Assign ranks
        for (int i = 0; i < leaderboard.Entries.Count; i++)
        {
            leaderboard.Entries[i].Rank = i + 1;
        }
        
        leaderboard.LastUpdated = DateTime.UtcNow;
        SaveLeaderboardCache();
        
        EmitSignal(SignalName.LeaderboardUpdated, (int)LeaderboardType.TotalScore);
    }
    
    /// <summary>
    /// Update perfect levels leaderboard
    /// </summary>
    private void UpdatePerfectLevelsLeaderboard()
    {
        var perfectCount = CalculatePlayerPerfectLevels();
        
        var entry = new LeaderboardEntry
        {
            PlayerId = GetCurrentPlayerId(),
            PlayerName = GetCurrentPlayerName(),
            Score = perfectCount,
            DateAchieved = DateTime.UtcNow,
            Cosmetics = GetCurrentPlayerCosmetics(),
            IsCurrentPlayer = true
        };
        
        var leaderboard = _leaderboards.PerfectLevels;
        leaderboard.Type = LeaderboardType.PerfectLevels;
        
        // Remove existing entry
        var existingEntry = leaderboard.Entries.Find(e => e.PlayerId == entry.PlayerId);
        if (existingEntry != null)
        {
            leaderboard.Entries.Remove(existingEntry);
        }
        
        leaderboard.Entries.Add(entry);
        
        // Sort and limit
        leaderboard.Entries = leaderboard.Entries
            .OrderByDescending(e => e.Score)
            .Take(Top100Count)
            .ToList();
        
        // Assign ranks
        for (int i = 0; i < leaderboard.Entries.Count; i++)
        {
            leaderboard.Entries[i].Rank = i + 1;
        }
        
        leaderboard.LastUpdated = DateTime.UtcNow;
        SaveLeaderboardCache();
        
        EmitSignal(SignalName.LeaderboardUpdated, (int)LeaderboardType.PerfectLevels);
    }
    
    /// <summary>
    /// Get leaderboard for specific level
    /// </summary>
    public LeaderboardData? GetLevelLeaderboard(string levelId)
    {
        return _leaderboards.ByLevel.GetValueOrDefault(levelId);
    }
    
    /// <summary>
    /// Get total score leaderboard
    /// </summary>
    public LeaderboardData GetTotalScoreLeaderboard()
    {
        return _leaderboards.TotalScore;
    }
    
    /// <summary>
    /// Get perfect levels leaderboard
    /// </summary>
    public LeaderboardData GetPerfectLevelsLeaderboard()
    {
        return _leaderboards.PerfectLevels;
    }
    
    /// <summary>
    /// Get replay views leaderboard
    /// </summary>
    public LeaderboardData GetReplayViewsLeaderboard()
    {
        return _leaderboards.ReplayViews;
    }
    
    /// <summary>
    /// Get player's rank in leaderboard
    /// </summary>
    public int GetPlayerRank(LeaderboardType type, string levelId = "")
    {
        LeaderboardData leaderboard = type switch
        {
            LeaderboardType.ByLevel => _leaderboards.ByLevel.GetValueOrDefault(levelId),
            LeaderboardType.TotalScore => _leaderboards.TotalScore,
            LeaderboardType.PerfectLevels => _leaderboards.PerfectLevels,
            LeaderboardType.ReplayViews => _leaderboards.ReplayViews,
            _ => null
        };
        
        if (leaderboard == null)
            return -1;
        
        var playerId = GetCurrentPlayerId();
        var entry = leaderboard.Entries.Find(e => e.PlayerId == playerId);
        
        return entry?.Rank ?? -1;
    }
    
    /// <summary>
    /// Search player in leaderboard
    /// </summary>
    public LeaderboardEntry? SearchPlayer(string playerName, LeaderboardType type, string levelId = "")
    {
        LeaderboardData leaderboard = type switch
        {
            LeaderboardType.ByLevel => _leaderboards.ByLevel.GetValueOrDefault(levelId),
            LeaderboardType.TotalScore => _leaderboards.TotalScore,
            LeaderboardType.PerfectLevels => _leaderboards.PerfectLevels,
            LeaderboardType.ReplayViews => _leaderboards.ReplayViews,
            _ => null
        };
        
        if (leaderboard == null)
            return null;
        
        return leaderboard.Entries.Find(e => 
            e.PlayerName.Contains(playerName, StringComparison.OrdinalIgnoreCase));
    }
    
    /// <summary>
    /// Get friend entries from leaderboard
    /// </summary>
    public List<LeaderboardEntry> GetFriendEntries(LeaderboardType type, string levelId = "")
    {
        LeaderboardData leaderboard = type switch
        {
            LeaderboardType.ByLevel => _leaderboards.ByLevel.GetValueOrDefault(levelId),
            LeaderboardType.TotalScore => _leaderboards.TotalScore,
            LeaderboardType.PerfectLevels => _leaderboards.PerfectLevels,
            LeaderboardType.ReplayViews => _leaderboards.ReplayViews,
            _ => null
        };
        
        if (leaderboard == null)
            return new List<LeaderboardEntry>();
        
        // Mark friends
        if (FriendLeaderboard.Instance != null)
        {
            var friendIds = FriendLeaderboard.Instance.GetAllFriends().Select(f => f.FriendId).ToHashSet();
            
            foreach (var entry in leaderboard.Entries)
            {
                entry.IsFriend = friendIds.Contains(entry.PlayerId);
            }
        }
        
        return leaderboard.Entries.Where(e => e.IsFriend).ToList();
    }
    
    /// <summary>
    /// Sync leaderboard with server
    /// </summary>
    public async void SyncLeaderboards()
    {
        if (_isSyncing)
        {
            GD.Print("Leaderboard sync already in progress");
            return;
        }
        
        _isSyncing = true;
        EmitSignal(SignalName.LeaderboardSyncStarted);
        
        GD.Print("Starting leaderboard sync...");
        
        // Simulate server sync (in production, this would use Firebase)
        await System.Threading.Tasks.Task.Delay(1000);
        
        _lastSyncTime = DateTime.UtcNow;
        _leaderboards.LastSync = _lastSyncTime;
        
        SaveLeaderboardCache();
        
        _isSyncing = false;
        EmitSignal(SignalName.LeaderboardSyncCompleted, true);
        
        GD.Print("Leaderboard sync completed");
    }
    
    /// <summary>
    /// Sync to server (async)
    /// </summary>
    private async void SyncToServer(string levelId, LeaderboardEntry entry)
    {
        // In production, this would sync to Firebase Realtime Database
        // For now, just log
        await System.Threading.Tasks.Task.Delay(100);
        GD.Print($"Synced entry to server: {entry.PlayerName} - {entry.Score}");
    }
    
    /// <summary>
    /// Calculate player's total score across all levels
    /// </summary>
    private int CalculatePlayerTotalScore()
    {
        var totalScore = 0;
        var playerId = GetCurrentPlayerId();
        
        foreach (var leaderboard in _leaderboards.ByLevel.Values)
        {
            var entry = leaderboard.Entries.Find(e => e.PlayerId == playerId);
            if (entry != null)
            {
                totalScore += entry.Score;
            }
        }
        
        return totalScore;
    }
    
    /// <summary>
    /// Calculate player's perfect levels count
    /// </summary>
    private int CalculatePlayerPerfectLevels()
    {
        var perfectCount = 0;
        var playerId = GetCurrentPlayerId();
        
        foreach (var leaderboard in _leaderboards.ByLevel.Values)
        {
            var entry = leaderboard.Entries.Find(e => e.PlayerId == playerId);
            if (entry != null && entry.Stars >= 5)
            {
                perfectCount++;
            }
        }
        
        return perfectCount;
    }
    
    /// <summary>
    /// Sync timer callback
    /// </summary>
    private void OnSyncTimerTimeout()
    {
        if (_leaderboards.NeedsSync())
        {
            SyncLeaderboards();
        }
        
        // Restart timer
        var timer = GetTree().CreateTimer(SyncIntervalMinutes * 60);
        timer.Timeout += OnSyncTimerTimeout;
    }
    
    /// <summary>
    /// Save leaderboard cache to disk
    /// </summary>
    private void SaveLeaderboardCache()
    {
        try
        {
            var json = JsonConvert.SerializeObject(_leaderboards, Formatting.Indented);
            using var file = FileAccess.Open(LeaderboardDataPath, FileAccess.ModeFlags.Write);
            file?.StoreString(json);
        }
        catch (Exception ex)
        {
            GD.PrintErr($"Failed to save leaderboard cache: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Load cached leaderboards from disk
    /// </summary>
    private void LoadCachedLeaderboards()
    {
        try
        {
            if (!FileAccess.FileExists(LeaderboardDataPath))
            {
                _leaderboards = new GlobalLeaderboardCollection();
                return;
            }
            
            using var file = FileAccess.Open(LeaderboardDataPath, FileAccess.ModeFlags.Read);
            var json = file?.GetAsText() ?? "";
            
            if (string.IsNullOrWhiteSpace(json))
                return;
            
            _leaderboards = JsonConvert.DeserializeObject<GlobalLeaderboardCollection>(json)
                ?? new GlobalLeaderboardCollection();
            
            GD.Print("Loaded cached leaderboards");
        }
        catch (Exception ex)
        {
            GD.PrintErr($"Failed to load leaderboard cache: {ex.Message}");
            _leaderboards = new GlobalLeaderboardCollection();
        }
    }
    
    /// <summary>
    /// Get current player ID
    /// </summary>
    private string GetCurrentPlayerId()
    {
        return PlayerProfile.Instance?.PlayerName ?? "Player";
    }
    
    /// <summary>
    /// Get current player name
    /// </summary>
    private string GetCurrentPlayerName()
    {
        return PlayerProfile.Instance?.PlayerName ?? "Player";
    }
    
    /// <summary>
    /// Get current player cosmetics
    /// </summary>
    private FriendCosmetics GetCurrentPlayerCosmetics()
    {
        if (PlayerProfile.Instance == null)
            return new FriendCosmetics();
        
        return new FriendCosmetics
        {
            HatIndex = PlayerProfile.Instance.SelectedHatIndex,
            GlassesIndex = PlayerProfile.Instance.SelectedGlassesIndex,
            MoustacheIndex = PlayerProfile.Instance.SelectedMoustacheIndex,
            WigIndex = PlayerProfile.Instance.SelectedWigIndex,
            SlingshotSkinIndex = PlayerProfile.Instance.SelectedSlingshotSkinIndex,
            ProjectileSkinIndex = PlayerProfile.Instance.SelectedProjectileSkinIndex
        };
    }
    
    public bool IsSyncing => _isSyncing;
    public DateTime LastSyncTime => _lastSyncTime;
}
