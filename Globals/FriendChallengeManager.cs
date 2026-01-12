using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

/// <summary>
/// Manages friend challenges: creation, acceptance, completion, and rewards
/// </summary>
public partial class FriendChallengeManager : Node
{
    public static FriendChallengeManager Instance { get; private set; }
    
    private const string ChallengesDataPath = "user://challenges.json";
    private const int MaxActiveChallenges = 50;
    
    // Challenge data
    private Dictionary<string, FriendChallenge> _challenges = new();
    private List<string> _pendingChallengeIds = new();
    
    // Challenge rewards
    private const int LoserRewardCoins = 50;
    private const int WinnerRewardCoins = 200;
    private const int BothCompleteBonus = 100;
    
    // Signals
    [Signal]
    public delegate void ChallengeCreatedEventHandler(FriendChallenge challenge);
    
    [Signal]
    public delegate void ChallengeAcceptedEventHandler(FriendChallenge challenge);
    
    [Signal]
    public delegate void ChallengeCompletedEventHandler(FriendChallenge challenge, string winnerId);
    
    [Signal]
    public delegate void ChallengeExpiredEventHandler(FriendChallenge challenge);
    
    [Signal]
    public delegate void ChallengeNotificationEventHandler(string message, FriendChallenge challenge);
    
    public override void _Ready()
    {
        if (Instance != null)
        {
            QueueFree();
            return;
        }
        
        Instance = this;
        ProcessMode = ProcessModeEnum.Always;
        
        LoadChallenges();
        CleanupExpiredChallenges();
        
        GD.Print("Friend Challenge Manager initialized");
    }
    
    public override void _ExitTree()
    {
        SaveChallenges();
    }
    
    /// <summary>
    /// Create a new friend challenge
    /// </summary>
    public FriendChallenge? CreateChallenge(string challengeeId, string challengeeName, 
        string levelId, string levelName, int targetScore, string message = "")
    {
        if (_challenges.Count >= MaxActiveChallenges)
        {
            GD.PrintErr($"Cannot create challenge: maximum limit of {MaxActiveChallenges} reached");
            return null;
        }
        
        if (string.IsNullOrWhiteSpace(challengeeId))
        {
            GD.PrintErr("Cannot create challenge: invalid challengee ID");
            return null;
        }
        
        var challenge = new FriendChallenge
        {
            ChallengeId = Guid.NewGuid().ToString(),
            ChallengerId = GetCurrentPlayerId(),
            ChallengerName = GetCurrentPlayerName(),
            ChallengeeId = challengeeId,
            ChallengeeName = challengeeName,
            LevelId = levelId,
            LevelName = levelName,
            TargetScore = targetScore,
            ChallengerScore = targetScore,
            Message = message,
            CreatedDate = DateTime.UtcNow,
            ExpirationDate = DateTime.UtcNow.AddDays(7),
            Status = ChallengeStatus.Pending,
            ChallengerCosmetics = GetCurrentPlayerCosmetics()
        };
        
        _challenges[challenge.ChallengeId] = challenge;
        _pendingChallengeIds.Add(challenge.ChallengeId);
        
        SaveChallenges();
        
        // Update friend statistics
        if (FriendLeaderboard.Instance != null)
        {
            FriendLeaderboard.Instance.IncrementChallengesSent(challengeeId);
            FriendLeaderboard.Instance.UpdateFriendInteraction(challengeeId);
        }
        
        EmitSignal(SignalName.ChallengeCreated, challenge);
        
        // Track analytics
        TrackChallengeCreated(challenge);
        
        // Send notification
        SendChallengeNotification(challenge);
        
        GD.Print($"Created challenge: {challenge.ChallengerName} -> {challenge.ChallengeeName} on {levelName}");
        return challenge;
    }
    
    /// <summary>
    /// Accept a challenge
    /// </summary>
    public bool AcceptChallenge(string challengeId)
    {
        if (!_challenges.ContainsKey(challengeId))
        {
            GD.PrintErr($"Cannot accept challenge: {challengeId} not found");
            return false;
        }
        
        var challenge = _challenges[challengeId];
        
        if (!challenge.CanBeAccepted())
        {
            GD.PrintErr("Cannot accept challenge: already accepted, completed, or expired");
            return false;
        }
        
        challenge.Status = ChallengeStatus.Accepted;
        challenge.AcceptedDate = DateTime.UtcNow;
        
        _pendingChallengeIds.Remove(challengeId);
        SaveChallenges();
        
        // Update friend statistics
        if (FriendLeaderboard.Instance != null)
        {
            FriendLeaderboard.Instance.IncrementChallengesReceived(challenge.ChallengerId);
            FriendLeaderboard.Instance.UpdateFriendInteraction(challenge.ChallengerId);
        }
        
        EmitSignal(SignalName.ChallengeAccepted, challenge);
        
        // Track analytics
        TrackChallengeAccepted(challenge);
        
        GD.Print($"Challenge accepted: {challengeId}");
        return true;
    }
    
    /// <summary>
    /// Complete a challenge with score
    /// </summary>
    public bool CompleteChallenge(string challengeId, int score, int stars)
    {
        if (!_challenges.ContainsKey(challengeId))
        {
            GD.PrintErr($"Cannot complete challenge: {challengeId} not found");
            return false;
        }
        
        var challenge = _challenges[challengeId];
        
        if (challenge.Status != ChallengeStatus.Accepted)
        {
            GD.PrintErr("Cannot complete challenge: must be accepted first");
            return false;
        }
        
        challenge.ChallengeeScore = score;
        challenge.ChallengeeStars = stars;
        challenge.Status = ChallengeStatus.Completed;
        challenge.CompletedDate = DateTime.UtcNow;
        challenge.ChallengeeCosmetics = GetCurrentPlayerCosmetics();
        
        // Determine winner
        var winnerId = challenge.DetermineWinner();
        challenge.WinnerId = winnerId;
        
        SaveChallenges();
        
        // Update friend statistics
        if (FriendLeaderboard.Instance != null)
        {
            if (winnerId == challenge.ChallengeeId)
            {
                FriendLeaderboard.Instance.IncrementChallengesWon(challenge.ChallengeeId);
                FriendLeaderboard.Instance.IncrementChallengesLost(challenge.ChallengerId);
            }
            else if (winnerId == challenge.ChallengerId)
            {
                FriendLeaderboard.Instance.IncrementChallengesWon(challenge.ChallengerId);
                FriendLeaderboard.Instance.IncrementChallengesLost(challenge.ChallengeeId);
            }
            
            FriendLeaderboard.Instance.UpdateFriendInteraction(challenge.ChallengerId);
        }
        
        // Award rewards
        AwardChallengeRewards(challenge);
        
        EmitSignal(SignalName.ChallengeCompleted, challenge, winnerId);
        
        // Track analytics
        TrackChallengeCompleted(challenge, winnerId);
        
        GD.Print($"Challenge completed: {challengeId}, Winner: {winnerId}");
        return true;
    }
    
    /// <summary>
    /// Decline a challenge
    /// </summary>
    public bool DeclineChallenge(string challengeId)
    {
        if (!_challenges.ContainsKey(challengeId))
        {
            GD.PrintErr($"Cannot decline challenge: {challengeId} not found");
            return false;
        }
        
        var challenge = _challenges[challengeId];
        challenge.Status = ChallengeStatus.Declined;
        
        _pendingChallengeIds.Remove(challengeId);
        SaveChallenges();
        
        GD.Print($"Challenge declined: {challengeId}");
        return true;
    }
    
    /// <summary>
    /// Get all pending challenges
    /// </summary>
    public List<FriendChallenge> GetPendingChallenges()
    {
        return _pendingChallengeIds
            .Select(id => _challenges.GetValueOrDefault(id))
            .Where(c => c != null && c.CanBeAccepted())
            .ToList();
    }
    
    /// <summary>
    /// Get all challenges
    /// </summary>
    public List<FriendChallenge> GetAllChallenges()
    {
        return _challenges.Values.ToList();
    }
    
    /// <summary>
    /// Get challenge by ID
    /// </summary>
    public FriendChallenge? GetChallenge(string challengeId)
    {
        return _challenges.GetValueOrDefault(challengeId);
    }
    
    /// <summary>
    /// Get completed challenges
    /// </summary>
    public List<FriendChallenge> GetCompletedChallenges()
    {
        return _challenges.Values
            .Where(c => c.Status == ChallengeStatus.Completed)
            .OrderByDescending(c => c.CompletedDate)
            .ToList();
    }
    
    /// <summary>
    /// Get challenges for a specific friend
    /// </summary>
    public List<FriendChallenge> GetChallengesWithFriend(string friendId)
    {
        return _challenges.Values
            .Where(c => c.ChallengerId == friendId || c.ChallengeeId == friendId)
            .OrderByDescending(c => c.CreatedDate)
            .ToList();
    }
    
    /// <summary>
    /// Award challenge rewards
    /// </summary>
    private void AwardChallengeRewards(FriendChallenge challenge)
    {
        if (challenge.RewardsClaimed)
            return;
        
        var winnerId = challenge.WinnerId;
        var currentPlayerId = GetCurrentPlayerId();
        
        // Award coins to current player
        if (currentPlayerId == challenge.ChallengeeId)
        {
            if (winnerId == currentPlayerId)
            {
                // Winner gets medium reward
                AwardCoins(WinnerRewardCoins);
                GD.Print($"Challenge won! Awarded {WinnerRewardCoins} coins");
            }
            else
            {
                // Loser gets small reward
                AwardCoins(LoserRewardCoins);
                GD.Print($"Challenge lost. Awarded {LoserRewardCoins} coins");
            }
            
            // Both complete bonus
            if (!string.IsNullOrEmpty(winnerId))
            {
                AwardCoins(BothCompleteBonus);
                GD.Print($"Both players completed! Bonus {BothCompleteBonus} coins");
            }
        }
        
        challenge.RewardsClaimed = true;
        SaveChallenges();
    }
    
    /// <summary>
    /// Award coins to player
    /// </summary>
    private void AwardCoins(int amount)
    {
        try
        {
            if (MonetizationManager.Instance != null)
            {
                MonetizationManager.Instance.AddCoins(amount);
            }
        }
        catch (Exception ex)
        {
            GD.PrintErr($"Failed to award coins: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Clean up expired challenges
    /// </summary>
    private void CleanupExpiredChallenges()
    {
        var expiredIds = _challenges.Values
            .Where(c => c.IsExpired() && c.Status == ChallengeStatus.Pending)
            .Select(c => c.ChallengeId)
            .ToList();
        
        foreach (var id in expiredIds)
        {
            var challenge = _challenges[id];
            challenge.Status = ChallengeStatus.Expired;
            _pendingChallengeIds.Remove(id);
            
            EmitSignal(SignalName.ChallengeExpired, challenge);
        }
        
        if (expiredIds.Count > 0)
        {
            SaveChallenges();
            GD.Print($"Cleaned up {expiredIds.Count} expired challenges");
        }
    }
    
    /// <summary>
    /// Send challenge notification
    /// </summary>
    private void SendChallengeNotification(FriendChallenge challenge)
    {
        var message = $"{challenge.ChallengerName} challenged you to beat {challenge.TargetScore} on {challenge.LevelName}!";
        EmitSignal(SignalName.ChallengeNotification, message, challenge);
        
        // Play notification sound
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySfx("challenge_notification");
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
    
    /// <summary>
    /// Save challenges to disk
    /// </summary>
    private void SaveChallenges()
    {
        try
        {
            var data = new Dictionary<string, object>
            {
                ["challenges"] = _challenges,
                ["pending_ids"] = _pendingChallengeIds,
                ["version"] = "1.0"
            };
            
            var json = JsonConvert.SerializeObject(data, Formatting.Indented);
            using var file = FileAccess.Open(ChallengesDataPath, FileAccess.ModeFlags.Write);
            file?.StoreString(json);
        }
        catch (Exception ex)
        {
            GD.PrintErr($"Failed to save challenges data: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Load challenges from disk
    /// </summary>
    private void LoadChallenges()
    {
        try
        {
            if (!FileAccess.FileExists(ChallengesDataPath))
            {
                _challenges = new Dictionary<string, FriendChallenge>();
                _pendingChallengeIds = new List<string>();
                return;
            }
            
            using var file = FileAccess.Open(ChallengesDataPath, FileAccess.ModeFlags.Read);
            var json = file?.GetAsText() ?? "";
            
            if (string.IsNullOrWhiteSpace(json))
                return;
            
            var data = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
            if (data == null)
                return;
            
            if (data.ContainsKey("challenges"))
            {
                var challengesJson = JsonConvert.SerializeObject(data["challenges"]);
                _challenges = JsonConvert.DeserializeObject<Dictionary<string, FriendChallenge>>(challengesJson)
                    ?? new Dictionary<string, FriendChallenge>();
            }
            
            if (data.ContainsKey("pending_ids"))
            {
                var pendingJson = JsonConvert.SerializeObject(data["pending_ids"]);
                _pendingChallengeIds = JsonConvert.DeserializeObject<List<string>>(pendingJson)
                    ?? new List<string>();
            }
            
            GD.Print($"Loaded {_challenges.Count} challenges");
        }
        catch (Exception ex)
        {
            GD.PrintErr($"Failed to load challenges data: {ex.Message}");
            _challenges = new Dictionary<string, FriendChallenge>();
            _pendingChallengeIds = new List<string>();
        }
    }
    
    /// <summary>
    /// Track challenge created analytics
    /// </summary>
    private void TrackChallengeCreated(FriendChallenge challenge)
    {
        try
        {
            if (AnalyticsEventTracker.Instance != null)
            {
                var parameters = new Dictionary<string, object>
                {
                    ["challenge_id"] = challenge.ChallengeId,
                    ["challengee_id"] = challenge.ChallengeeId,
                    ["level_id"] = challenge.LevelId,
                    ["target_score"] = challenge.TargetScore
                };
                AnalyticsEventTracker.Instance.LogEvent("challenge_created", parameters);
            }
        }
        catch (Exception ex)
        {
            GD.PrintErr($"Failed to track challenge_created: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Track challenge accepted analytics
    /// </summary>
    private void TrackChallengeAccepted(FriendChallenge challenge)
    {
        try
        {
            if (AnalyticsEventTracker.Instance != null)
            {
                var parameters = new Dictionary<string, object>
                {
                    ["challenge_id"] = challenge.ChallengeId,
                    ["challenger_id"] = challenge.ChallengerId,
                    ["level_id"] = challenge.LevelId
                };
                AnalyticsEventTracker.Instance.LogEvent("challenge_accepted", parameters);
            }
        }
        catch (Exception ex)
        {
            GD.PrintErr($"Failed to track challenge_accepted: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Track challenge completed analytics
    /// </summary>
    private void TrackChallengeCompleted(FriendChallenge challenge, string winnerId)
    {
        try
        {
            if (AnalyticsEventTracker.Instance != null)
            {
                var parameters = new Dictionary<string, object>
                {
                    ["challenge_id"] = challenge.ChallengeId,
                    ["winner_id"] = winnerId,
                    ["challenger_score"] = challenge.ChallengerScore,
                    ["challengee_score"] = challenge.ChallengeeScore,
                    ["level_id"] = challenge.LevelId
                };
                
                var eventName = winnerId == GetCurrentPlayerId() ? "challenge_won" : "challenge_lost";
                AnalyticsEventTracker.Instance.LogEvent(eventName, parameters);
            }
        }
        catch (Exception ex)
        {
            GD.PrintErr($"Failed to track challenge completion: {ex.Message}");
        }
    }
}
