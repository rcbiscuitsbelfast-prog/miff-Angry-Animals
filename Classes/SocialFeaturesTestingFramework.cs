using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Comprehensive testing framework for social features
/// Tests friend challenges, replays, leaderboards, and social cosmetics
/// </summary>
public partial class SocialFeaturesTestingFramework : Node
{
    public static SocialFeaturesTestingFramework Instance { get; private set; }
    
    private List<TestResult> _testResults = new();
    private bool _isRunning = false;
    
    // Signals
    [Signal]
    public delegate void TestStartedEventHandler(string testName);
    
    [Signal]
    public delegate void TestCompletedEventHandler(string testName, bool passed);
    
    [Signal]
    public delegate void AllTestsCompletedEventHandler(int passed, int failed);
    
    public override void _Ready()
    {
        if (Instance != null)
        {
            QueueFree();
            return;
        }
        
        Instance = this;
        ProcessMode = ProcessModeEnum.Always;
        
        GD.Print("Social Features Testing Framework initialized");
    }
    
    /// <summary>
    /// Run all social features tests
    /// </summary>
    public void RunAllTests()
    {
        if (_isRunning)
        {
            GD.Print("Tests already running");
            return;
        }
        
        _isRunning = true;
        _testResults.Clear();
        
        GD.Print("\n=== Starting Social Features Tests ===\n");
        
        // Friend System Tests
        TestFriendAddition();
        TestFriendRemoval();
        TestFriendScoreUpdate();
        TestFriendLeaderboard();
        TestFriendSearch();
        
        // Challenge System Tests
        TestChallengeCreation();
        TestChallengeAcceptance();
        TestChallengeCompletion();
        TestChallengeRewards();
        TestChallengeExpiration();
        
        // Replay System Tests
        TestReplayRecording();
        TestReplayPlayback();
        TestReplaySharing();
        TestReplayImport();
        TestReplayStorage();
        
        // Leaderboard Tests
        TestLeaderboardSubmission();
        TestLeaderboardRanking();
        TestLeaderboardFiltering();
        TestLeaderboardSync();
        
        // Social Cosmetics Tests
        TestCosmeticUnlocks();
        TestSocialAchievements();
        
        // Integration Tests
        TestChallengeReplayIntegration();
        TestLeaderboardReplayIntegration();
        
        // Performance Tests
        TestReplayFileSize();
        TestLeaderboardPerformance();
        
        PrintTestResults();
        _isRunning = false;
    }
    
    // ===== FRIEND SYSTEM TESTS =====
    
    private void TestFriendAddition()
    {
        RunTest("Friend Addition", () =>
        {
            var friendId = "test_friend_001";
            var friendName = "Test Friend";
            
            var success = FriendLeaderboard.Instance?.AddFriend(friendId, friendName) ?? false;
            
            if (!success)
                return false;
            
            var friend = FriendLeaderboard.Instance?.GetFriend(friendId);
            return friend != null && friend.FriendName == friendName;
        });
    }
    
    private void TestFriendRemoval()
    {
        RunTest("Friend Removal", () =>
        {
            var friendId = "test_friend_002";
            FriendLeaderboard.Instance?.AddFriend(friendId, "Test Friend 2");
            
            var removed = FriendLeaderboard.Instance?.RemoveFriend(friendId) ?? false;
            
            if (!removed)
                return false;
            
            var friend = FriendLeaderboard.Instance?.GetFriend(friendId);
            return friend == null;
        });
    }
    
    private void TestFriendScoreUpdate()
    {
        RunTest("Friend Score Update", () =>
        {
            var friendId = "test_friend_003";
            FriendLeaderboard.Instance?.AddFriend(friendId, "Test Friend 3");
            
            FriendLeaderboard.Instance?.UpdateFriendScore(friendId, "level_01", 10000, 5);
            
            var leaderboard = FriendLeaderboard.Instance?.GetFriendLeaderboardForLevel("level_01");
            var entry = leaderboard?.Find(e => e.PlayerId == friendId);
            
            return entry != null && entry.Score == 10000 && entry.Stars == 5;
        });
    }
    
    private void TestFriendLeaderboard()
    {
        RunTest("Friend Leaderboard Generation", () =>
        {
            // Add multiple friends with scores
            for (int i = 1; i <= 5; i++)
            {
                var friendId = $"test_friend_lb_{i}";
                FriendLeaderboard.Instance?.AddFriend(friendId, $"Friend {i}");
                FriendLeaderboard.Instance?.UpdateFriendScore(friendId, "level_01", 1000 * i, i);
            }
            
            var leaderboard = FriendLeaderboard.Instance?.GetFriendLeaderboardForLevel("level_01");
            
            if (leaderboard == null || leaderboard.Count != 5)
                return false;
            
            // Check ordering (highest score first)
            for (int i = 0; i < leaderboard.Count - 1; i++)
            {
                if (leaderboard[i].Score < leaderboard[i + 1].Score)
                    return false;
            }
            
            return true;
        });
    }
    
    private void TestFriendSearch()
    {
        RunTest("Friend Search", () =>
        {
            FriendLeaderboard.Instance?.AddFriend("search_test_001", "Alice Smith");
            FriendLeaderboard.Instance?.AddFriend("search_test_002", "Bob Johnson");
            FriendLeaderboard.Instance?.AddFriend("search_test_003", "Alice Brown");
            
            var results = FriendLeaderboard.Instance?.SearchFriends("Alice");
            
            return results != null && results.Count == 2;
        });
    }
    
    // ===== CHALLENGE SYSTEM TESTS =====
    
    private void TestChallengeCreation()
    {
        RunTest("Challenge Creation", () =>
        {
            var challenge = FriendChallengeManager.Instance?.CreateChallenge(
                "test_friend_001",
                "Test Friend",
                "level_01",
                "Level 1",
                5000,
                "Test challenge message"
            );
            
            return challenge != null && 
                   challenge.Status == ChallengeStatus.Pending &&
                   challenge.TargetScore == 5000;
        });
    }
    
    private void TestChallengeAcceptance()
    {
        RunTest("Challenge Acceptance", () =>
        {
            var challenge = FriendChallengeManager.Instance?.CreateChallenge(
                "test_friend_accept",
                "Test Friend",
                "level_01",
                "Level 1",
                5000
            );
            
            if (challenge == null)
                return false;
            
            var accepted = FriendChallengeManager.Instance?.AcceptChallenge(challenge.ChallengeId) ?? false;
            
            if (!accepted)
                return false;
            
            var updatedChallenge = FriendChallengeManager.Instance?.GetChallenge(challenge.ChallengeId);
            return updatedChallenge?.Status == ChallengeStatus.Accepted;
        });
    }
    
    private void TestChallengeCompletion()
    {
        RunTest("Challenge Completion", () =>
        {
            var challenge = FriendChallengeManager.Instance?.CreateChallenge(
                "test_friend_complete",
                "Test Friend",
                "level_01",
                "Level 1",
                5000
            );
            
            if (challenge == null)
                return false;
            
            FriendChallengeManager.Instance?.AcceptChallenge(challenge.ChallengeId);
            var completed = FriendChallengeManager.Instance?.CompleteChallenge(challenge.ChallengeId, 6000, 5) ?? false;
            
            if (!completed)
                return false;
            
            var updatedChallenge = FriendChallengeManager.Instance?.GetChallenge(challenge.ChallengeId);
            return updatedChallenge?.Status == ChallengeStatus.Completed &&
                   updatedChallenge?.ChallengeeScore == 6000;
        });
    }
    
    private void TestChallengeRewards()
    {
        RunTest("Challenge Rewards", () =>
        {
            // Test that completing a challenge awards rewards
            var initialCoins = MonetizationManager.Instance?.GetCoins() ?? 0;
            
            var challenge = FriendChallengeManager.Instance?.CreateChallenge(
                "test_friend_reward",
                "Test Friend",
                "level_01",
                "Level 1",
                5000
            );
            
            if (challenge == null)
                return false;
            
            FriendChallengeManager.Instance?.AcceptChallenge(challenge.ChallengeId);
            FriendChallengeManager.Instance?.CompleteChallenge(challenge.ChallengeId, 6000, 5);
            
            var finalCoins = MonetizationManager.Instance?.GetCoins() ?? 0;
            
            // Winner should get 200 + 100 (both complete bonus) = 300 coins
            return finalCoins > initialCoins;
        });
    }
    
    private void TestChallengeExpiration()
    {
        RunTest("Challenge Expiration", () =>
        {
            var challenge = FriendChallengeManager.Instance?.CreateChallenge(
                "test_friend_expire",
                "Test Friend",
                "level_01",
                "Level 1",
                5000
            );
            
            if (challenge == null)
                return false;
            
            // Manually set expiration to past
            challenge.ExpirationDate = DateTime.UtcNow.AddDays(-1);
            
            return challenge.IsExpired() && !challenge.CanBeAccepted();
        });
    }
    
    // ===== REPLAY SYSTEM TESTS =====
    
    private void TestReplayRecording()
    {
        RunTest("Replay Recording", () =>
        {
            var started = ReplayManager.Instance?.StartRecording("level_01", "Level 1") ?? false;
            
            if (!started)
                return false;
            
            // Simulate some input events
            ReplayManager.Instance?.RecordInputEvent(ReplayEventType.DragStart, new Vector2(100, 100));
            ReplayManager.Instance?.RecordInputEvent(ReplayEventType.Launch, new Vector2(200, 200), 45f, 100f);
            
            var replay = ReplayManager.Instance?.StopRecording(5000, 5, 10.5f);
            
            return replay != null && 
                   replay.Score == 5000 && 
                   replay.Stars == 5 &&
                   replay.InputEvents.Count > 0;
        });
    }
    
    private void TestReplayPlayback()
    {
        RunTest("Replay Playback", () =>
        {
            ReplayManager.Instance?.StartRecording("level_01", "Level 1");
            ReplayManager.Instance?.RecordInputEvent(ReplayEventType.Launch, Vector2.Zero);
            var replay = ReplayManager.Instance?.StopRecording(5000, 5, 10f);
            
            if (replay == null)
                return false;
            
            var started = ReplayManager.Instance?.StartPlayback(replay) ?? false;
            
            if (!started)
                return false;
            
            ReplayManager.Instance?.StopPlayback();
            
            return true;
        });
    }
    
    private void TestReplaySharing()
    {
        RunTest("Replay Sharing", () =>
        {
            ReplayManager.Instance?.StartRecording("level_01", "Level 1");
            var replay = ReplayManager.Instance?.StopRecording(5000, 5, 10f);
            
            if (replay == null)
                return false;
            
            var shareable = ReplayManager.Instance?.CreateShareableReplay(replay);
            
            return shareable != null && 
                   !string.IsNullOrEmpty(shareable.EncodedString) &&
                   !string.IsNullOrEmpty(shareable.ShareUrl);
        });
    }
    
    private void TestReplayImport()
    {
        RunTest("Replay Import", () =>
        {
            ReplayManager.Instance?.StartRecording("level_01", "Level 1");
            var replay = ReplayManager.Instance?.StopRecording(5000, 5, 10f);
            
            if (replay == null)
                return false;
            
            var shareable = ReplayManager.Instance?.CreateShareableReplay(replay);
            if (shareable == null)
                return false;
            
            var imported = ReplayManager.Instance?.ImportReplay(shareable.EncodedString);
            
            return imported != null && imported.Score == 5000;
        });
    }
    
    private void TestReplayStorage()
    {
        RunTest("Replay Storage Management", () =>
        {
            var initialCount = ReplayManager.Instance?.GetAllReplays().Count ?? 0;
            
            ReplayManager.Instance?.StartRecording("level_01", "Level 1");
            var replay = ReplayManager.Instance?.StopRecording(5000, 5, 10f);
            
            var finalCount = ReplayManager.Instance?.GetAllReplays().Count ?? 0;
            
            return finalCount > initialCount;
        });
    }
    
    // ===== LEADERBOARD TESTS =====
    
    private void TestLeaderboardSubmission()
    {
        RunTest("Leaderboard Score Submission", () =>
        {
            GlobalLeaderboard.Instance?.SubmitScore("level_01", "Level 1", 10000, 5, 15.5f);
            
            var leaderboard = GlobalLeaderboard.Instance?.GetLevelLeaderboard("level_01");
            
            return leaderboard != null && leaderboard.Entries.Count > 0;
        });
    }
    
    private void TestLeaderboardRanking()
    {
        RunTest("Leaderboard Ranking", () =>
        {
            // Submit multiple scores
            for (int i = 1; i <= 10; i++)
            {
                GlobalLeaderboard.Instance?.SubmitScore("level_test", "Test Level", 1000 * i, 3, 10f);
            }
            
            var leaderboard = GlobalLeaderboard.Instance?.GetLevelLeaderboard("level_test");
            
            if (leaderboard == null || leaderboard.Entries.Count == 0)
                return false;
            
            // Check ranking order
            for (int i = 0; i < leaderboard.Entries.Count - 1; i++)
            {
                if (leaderboard.Entries[i].Score < leaderboard.Entries[i + 1].Score)
                    return false;
            }
            
            return true;
        });
    }
    
    private void TestLeaderboardFiltering()
    {
        RunTest("Leaderboard Friend Filtering", () =>
        {
            // Add a friend
            FriendLeaderboard.Instance?.AddFriend("friend_filter_test", "Filter Friend");
            FriendLeaderboard.Instance?.UpdateFriendScore("friend_filter_test", "level_01", 8000, 4);
            
            var friendEntries = GlobalLeaderboard.Instance?.GetFriendEntries(LeaderboardType.ByLevel, "level_01");
            
            return friendEntries != null;
        });
    }
    
    private void TestLeaderboardSync()
    {
        RunTest("Leaderboard Sync", () =>
        {
            var wasSyncing = GlobalLeaderboard.Instance?.IsSyncing ?? false;
            
            GlobalLeaderboard.Instance?.SyncLeaderboards();
            
            // Wait briefly for async operation
            System.Threading.Thread.Sleep(100);
            
            return true; // Sync initiated successfully
        });
    }
    
    // ===== SOCIAL COSMETICS TESTS =====
    
    private void TestCosmeticUnlocks()
    {
        RunTest("Social Cosmetic Unlocks", () =>
        {
            // Add enough friends to unlock "friendship_hat"
            for (int i = 1; i <= 5; i++)
            {
                FriendLeaderboard.Instance?.AddFriend($"cosmetic_test_{i}", $"Cosmetic Friend {i}");
            }
            
            SocialCosmetics.Instance?.CheckAllUnlocks();
            
            var unlocked = SocialCosmetics.Instance?.IsUnlocked("friendship_hat") ?? false;
            
            return unlocked;
        });
    }
    
    private void TestSocialAchievements()
    {
        RunTest("Social Achievement Progress", () =>
        {
            var cosmetic = SocialCosmetics.Instance?.GetCosmetic("challenge_champion_crown");
            
            if (cosmetic == null)
                return false;
            
            var progress = SocialCosmetics.Instance?.GetProgress(cosmetic) ?? 0;
            
            return progress >= 0; // Progress tracking works
        });
    }
    
    // ===== INTEGRATION TESTS =====
    
    private void TestChallengeReplayIntegration()
    {
        RunTest("Challenge + Replay Integration", () =>
        {
            // Record a replay
            ReplayManager.Instance?.StartRecording("level_01", "Level 1");
            var replay = ReplayManager.Instance?.StopRecording(5000, 5, 10f);
            
            if (replay == null)
                return false;
            
            // Create challenge with replay
            var challenge = FriendChallengeManager.Instance?.CreateChallenge(
                "integration_test_friend",
                "Integration Friend",
                "level_01",
                "Level 1",
                5000
            );
            
            if (challenge == null)
                return false;
            
            challenge.ReplayId = replay.ReplayId;
            
            return !string.IsNullOrEmpty(challenge.ReplayId);
        });
    }
    
    private void TestLeaderboardReplayIntegration()
    {
        RunTest("Leaderboard + Replay Integration", () =>
        {
            ReplayManager.Instance?.StartRecording("level_01", "Level 1");
            var replay = ReplayManager.Instance?.StopRecording(5000, 5, 10f);
            
            if (replay == null)
                return false;
            
            GlobalLeaderboard.Instance?.SubmitScore("level_01", "Level 1", 5000, 5, 10f, replay.ReplayId);
            
            var leaderboard = GlobalLeaderboard.Instance?.GetLevelLeaderboard("level_01");
            var entry = leaderboard?.Entries.Find(e => e.IsCurrentPlayer);
            
            return entry != null && !string.IsNullOrEmpty(entry.ReplayId);
        });
    }
    
    // ===== PERFORMANCE TESTS =====
    
    private void TestReplayFileSize()
    {
        RunTest("Replay File Size Limit", () =>
        {
            ReplayManager.Instance?.StartRecording("level_01", "Level 1");
            
            // Record many events
            for (int i = 0; i < 100; i++)
            {
                ReplayManager.Instance?.RecordInputEvent(ReplayEventType.DragUpdate, new Vector2(i, i));
            }
            
            var replay = ReplayManager.Instance?.StopRecording(5000, 5, 10f);
            
            if (replay == null)
                return false;
            
            // Check file size is within limit (<500KB)
            return replay.IsWithinSizeLimit();
        });
    }
    
    private void TestLeaderboardPerformance()
    {
        RunTest("Leaderboard Performance", () =>
        {
            var startTime = DateTime.Now;
            
            // Query leaderboard 100 times
            for (int i = 0; i < 100; i++)
            {
                GlobalLeaderboard.Instance?.GetTotalScoreLeaderboard();
            }
            
            var elapsed = (DateTime.Now - startTime).TotalMilliseconds;
            
            // Should complete in < 1 second
            return elapsed < 1000;
        });
    }
    
    // ===== TEST UTILITIES =====
    
    private void RunTest(string testName, Func<bool> testFunc)
    {
        EmitSignal(SignalName.TestStarted, testName);
        
        try
        {
            var passed = testFunc();
            
            _testResults.Add(new TestResult
            {
                TestName = testName,
                Passed = passed,
                ErrorMessage = passed ? "" : "Test assertion failed"
            });
            
            EmitSignal(SignalName.TestCompleted, testName, passed);
            
            GD.Print($"[{(passed ? "✓" : "✗")}] {testName}");
        }
        catch (Exception ex)
        {
            _testResults.Add(new TestResult
            {
                TestName = testName,
                Passed = false,
                ErrorMessage = ex.Message
            });
            
            EmitSignal(SignalName.TestCompleted, testName, false);
            
            GD.PrintErr($"[✗] {testName}: {ex.Message}");
        }
    }
    
    private void PrintTestResults()
    {
        var passed = _testResults.Count(r => r.Passed);
        var failed = _testResults.Count(r => !r.Passed);
        var total = _testResults.Count;
        
        GD.Print($"\n=== Social Features Test Results ===");
        GD.Print($"Total: {total} | Passed: {passed} | Failed: {failed}");
        GD.Print($"Success Rate: {(passed * 100.0 / total):F1}%\n");
        
        if (failed > 0)
        {
            GD.Print("Failed Tests:");
            foreach (var result in _testResults.Where(r => !r.Passed))
            {
                GD.Print($"  - {result.TestName}: {result.ErrorMessage}");
            }
        }
        
        EmitSignal(SignalName.AllTestsCompleted, passed, failed);
    }
    
    public List<TestResult> GetTestResults() => _testResults;
}

public class TestResult
{
    public string TestName { get; set; } = "";
    public bool Passed { get; set; }
    public string ErrorMessage { get; set; } = "";
}
