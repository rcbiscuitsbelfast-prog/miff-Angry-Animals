using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

/// <summary>
/// Manages friend-based leaderboards and competitions
/// Handles friend relationships, scores, and challenges
/// </summary>
public partial class FriendLeaderboard : Node
{
    public static FriendLeaderboard Instance { get; private set; }
    
    private const string FriendsDataPath = "user://friends.json";
    private const int MaxFriends = 100;
    
    // Friend data
    private Dictionary<string, FriendData> _friends = new();
    private Dictionary<string, Dictionary<string, int>> _friendScoresByLevel = new();
    
    // Signals
    [Signal]
    public delegate void FriendAddedEventHandler(FriendData friend);
    
    [Signal]
    public delegate void FriendRemovedEventHandler(string friendId);
    
    [Signal]
    public delegate void FriendScoreUpdatedEventHandler(string friendId, string levelId, int score);
    
    [Signal]
    public delegate void FriendLeaderboardRefreshedEventHandler();
    
    public override void _Ready()
    {
        if (Instance != null)
        {
            QueueFree();
            return;
        }
        
        Instance = this;
        ProcessMode = ProcessModeEnum.Always;
        
        LoadFriends();
        GD.Print("Friend Leaderboard initialized");
    }
    
    public override void _ExitTree()
    {
        SaveFriends();
    }
    
    /// <summary>
    /// Add a friend to the friends list
    /// </summary>
    public bool AddFriend(string friendId, string friendName)
    {
        if (string.IsNullOrWhiteSpace(friendId))
        {
            GD.PrintErr("Cannot add friend: invalid friend ID");
            return false;
        }
        
        if (_friends.Count >= MaxFriends)
        {
            GD.PrintErr($"Cannot add friend: maximum limit of {MaxFriends} reached");
            return false;
        }
        
        if (_friends.ContainsKey(friendId))
        {
            GD.Print($"Friend {friendName} is already in your friends list");
            return false;
        }
        
        var friend = new FriendData
        {
            FriendId = friendId,
            FriendName = friendName,
            FriendshipDate = DateTime.UtcNow,
            LastInteractionDate = DateTime.UtcNow
        };
        
        _friends[friendId] = friend;
        SaveFriends();
        
        EmitSignal(SignalName.FriendAdded, friend);
        
        // Track analytics
        TrackFriendAdded(friendId);
        
        GD.Print($"Added friend: {friendName} ({friendId})");
        return true;
    }
    
    /// <summary>
    /// Remove a friend from the friends list
    /// </summary>
    public bool RemoveFriend(string friendId)
    {
        if (!_friends.ContainsKey(friendId))
        {
            GD.PrintErr($"Cannot remove friend: {friendId} not found");
            return false;
        }
        
        var friendName = _friends[friendId].FriendName;
        _friends.Remove(friendId);
        SaveFriends();
        
        EmitSignal(SignalName.FriendRemoved, friendId);
        
        // Track analytics
        TrackFriendRemoved(friendId);
        
        GD.Print($"Removed friend: {friendName} ({friendId})");
        return true;
    }
    
    /// <summary>
    /// Get all friends
    /// </summary>
    public List<FriendData> GetAllFriends()
    {
        return _friends.Values.ToList();
    }
    
    /// <summary>
    /// Get friend by ID
    /// </summary>
    public FriendData? GetFriend(string friendId)
    {
        return _friends.GetValueOrDefault(friendId);
    }
    
    /// <summary>
    /// Get friend count
    /// </summary>
    public int GetFriendCount()
    {
        return _friends.Count;
    }
    
    /// <summary>
    /// Update friend's score for a specific level
    /// </summary>
    public void UpdateFriendScore(string friendId, string levelId, int score, int stars)
    {
        if (!_friends.ContainsKey(friendId))
            return;
        
        if (!_friendScoresByLevel.ContainsKey(friendId))
            _friendScoresByLevel[friendId] = new Dictionary<string, int>();
        
        var currentScore = _friendScoresByLevel[friendId].GetValueOrDefault(levelId, 0);
        if (score > currentScore)
        {
            _friendScoresByLevel[friendId][levelId] = score;
            
            // Update friend's total score
            var friend = _friends[friendId];
            friend.TotalScore = _friendScoresByLevel[friendId].Values.Sum();
            friend.LastInteractionDate = DateTime.UtcNow;
            
            SaveFriends();
            
            EmitSignal(SignalName.FriendScoreUpdated, friendId, levelId, score);
        }
    }
    
    /// <summary>
    /// Get friend leaderboard for a specific level
    /// </summary>
    public List<LeaderboardEntry> GetFriendLeaderboardForLevel(string levelId)
    {
        var entries = new List<LeaderboardEntry>();
        
        foreach (var friend in _friends.Values)
        {
            var score = _friendScoresByLevel.GetValueOrDefault(friend.FriendId, new())
                .GetValueOrDefault(levelId, 0);
            
            if (score > 0)
            {
                entries.Add(new LeaderboardEntry
                {
                    PlayerId = friend.FriendId,
                    PlayerName = friend.FriendName,
                    Score = score,
                    DateAchieved = friend.LastInteractionDate,
                    Cosmetics = friend.FavoriteCosmetics,
                    IsFriend = true
                });
            }
        }
        
        // Sort by score descending
        entries = entries.OrderByDescending(e => e.Score).ToList();
        
        // Assign ranks
        for (int i = 0; i < entries.Count; i++)
        {
            entries[i].Rank = i + 1;
        }
        
        return entries;
    }
    
    /// <summary>
    /// Get global friend leaderboard (by total score)
    /// </summary>
    public List<LeaderboardEntry> GetGlobalFriendLeaderboard()
    {
        var entries = new List<LeaderboardEntry>();
        
        foreach (var friend in _friends.Values)
        {
            entries.Add(new LeaderboardEntry
            {
                PlayerId = friend.FriendId,
                PlayerName = friend.FriendName,
                Score = friend.TotalScore,
                Stars = friend.PerfectLevelsCount,
                DateAchieved = friend.LastInteractionDate,
                Cosmetics = friend.FavoriteCosmetics,
                IsFriend = true
            });
        }
        
        // Sort by total score descending
        entries = entries.OrderByDescending(e => e.Score).ToList();
        
        // Assign ranks
        for (int i = 0; i < entries.Count; i++)
        {
            entries[i].Rank = i + 1;
        }
        
        return entries;
    }
    
    /// <summary>
    /// Update friend interaction timestamp
    /// </summary>
    public void UpdateFriendInteraction(string friendId)
    {
        if (_friends.ContainsKey(friendId))
        {
            _friends[friendId].LastInteractionDate = DateTime.UtcNow;
            SaveFriends();
        }
    }
    
    /// <summary>
    /// Increment friend challenge statistics
    /// </summary>
    public void IncrementChallengesSent(string friendId)
    {
        if (_friends.ContainsKey(friendId))
        {
            _friends[friendId].TotalChallengesSent++;
            SaveFriends();
        }
    }
    
    public void IncrementChallengesReceived(string friendId)
    {
        if (_friends.ContainsKey(friendId))
        {
            _friends[friendId].TotalChallengesReceived++;
            SaveFriends();
        }
    }
    
    public void IncrementChallengesWon(string friendId)
    {
        if (_friends.ContainsKey(friendId))
        {
            _friends[friendId].ChallengesWon++;
            SaveFriends();
        }
    }
    
    public void IncrementChallengesLost(string friendId)
    {
        if (_friends.ContainsKey(friendId))
        {
            _friends[friendId].ChallengesLost++;
            SaveFriends();
        }
    }
    
    /// <summary>
    /// Search friends by name
    /// </summary>
    public List<FriendData> SearchFriends(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return GetAllFriends();
        
        var lowerQuery = query.ToLower();
        return _friends.Values
            .Where(f => f.FriendName.ToLower().Contains(lowerQuery))
            .ToList();
    }
    
    /// <summary>
    /// Get top N friends by score
    /// </summary>
    public List<FriendData> GetTopFriends(int count = 10)
    {
        return _friends.Values
            .OrderByDescending(f => f.TotalScore)
            .Take(count)
            .ToList();
    }
    
    /// <summary>
    /// Save friends to disk
    /// </summary>
    private void SaveFriends()
    {
        try
        {
            var data = new Dictionary<string, object>
            {
                ["friends"] = _friends,
                ["scores_by_level"] = _friendScoresByLevel,
                ["version"] = "1.0"
            };
            
            var json = JsonConvert.SerializeObject(data, Formatting.Indented);
            using var file = FileAccess.Open(FriendsDataPath, FileAccess.ModeFlags.Write);
            file?.StoreString(json);
        }
        catch (Exception ex)
        {
            GD.PrintErr($"Failed to save friends data: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Load friends from disk
    /// </summary>
    private void LoadFriends()
    {
        try
        {
            if (!FileAccess.FileExists(FriendsDataPath))
            {
                _friends = new Dictionary<string, FriendData>();
                _friendScoresByLevel = new Dictionary<string, Dictionary<string, int>>();
                return;
            }
            
            using var file = FileAccess.Open(FriendsDataPath, FileAccess.ModeFlags.Read);
            var json = file?.GetAsText() ?? "";
            
            if (string.IsNullOrWhiteSpace(json))
                return;
            
            var data = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
            if (data == null)
                return;
            
            if (data.ContainsKey("friends"))
            {
                var friendsJson = JsonConvert.SerializeObject(data["friends"]);
                _friends = JsonConvert.DeserializeObject<Dictionary<string, FriendData>>(friendsJson) 
                    ?? new Dictionary<string, FriendData>();
            }
            
            if (data.ContainsKey("scores_by_level"))
            {
                var scoresJson = JsonConvert.SerializeObject(data["scores_by_level"]);
                _friendScoresByLevel = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, int>>>(scoresJson)
                    ?? new Dictionary<string, Dictionary<string, int>>();
            }
            
            GD.Print($"Loaded {_friends.Count} friends");
        }
        catch (Exception ex)
        {
            GD.PrintErr($"Failed to load friends data: {ex.Message}");
            _friends = new Dictionary<string, FriendData>();
            _friendScoresByLevel = new Dictionary<string, Dictionary<string, int>>();
        }
    }
    
    /// <summary>
    /// Track friend added analytics
    /// </summary>
    private void TrackFriendAdded(string friendId)
    {
        try
        {
            if (AnalyticsEventTracker.Instance != null)
            {
                var parameters = new Dictionary<string, object>
                {
                    ["friend_id"] = friendId,
                    ["total_friends"] = _friends.Count
                };
                AnalyticsEventTracker.Instance.LogEvent("friend_added", parameters);
            }
        }
        catch (Exception ex)
        {
            GD.PrintErr($"Failed to track friend_added: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Track friend removed analytics
    /// </summary>
    private void TrackFriendRemoved(string friendId)
    {
        try
        {
            if (AnalyticsEventTracker.Instance != null)
            {
                var parameters = new Dictionary<string, object>
                {
                    ["friend_id"] = friendId,
                    ["total_friends"] = _friends.Count
                };
                AnalyticsEventTracker.Instance.LogEvent("friend_removed", parameters);
            }
        }
        catch (Exception ex)
        {
            GD.PrintErr($"Failed to track friend_removed: {ex.Message}");
        }
    }
}
