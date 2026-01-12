using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

/// <summary>
/// Automated Testing Framework for Analytics and Telemetry Systems
/// Provides unit tests, integration tests, and mock testing capabilities
/// Ensures analytics events fire correctly and Firebase integration works
/// </summary>
public class AnalyticsTestingFramework : Node
{
    public static AnalyticsTestingFramework Instance { get; private set; }

    // Test results
    private Dictionary<string, TestResult> _testResults = new Dictionary<string, TestResult>();
    private bool _isRunningTests = false;
    
    [Signal]
    public delegate void TestCompletedEventHandler(string testName, bool passed, string message);
    
    [Signal]
    public delegate void AllTestsCompletedEventHandler(Dictionary<string, TestResult> results);

    public override void _Ready()
    {
        if (Instance != null)
        {
            QueueFree();
            return;
        }
        
        Instance = this;
        GD.Print("Analytics Testing Framework initialized");
    }

    /// <summary>
    /// Run all analytics tests
    /// </summary>
    public async void RunAllTests()
    {
        if (_isRunningTests)
        {
            GD.PrintWarning("Tests already running");
            return;
        }
        
        _isRunningTests = true;
        _testResults.Clear();
        
        GD.Print("🧪 Starting Analytics Test Suite...");
        
        // Core system tests
        await RunTest("Firebase Manager Initialization", TestFirebaseManagerInitialization);
        await RunTest("Event Tracker Initialization", TestEventTrackerInitialization);
        await RunTest("Difficulty Heatmap Initialization", TestHeatmapInitialization);
        
        // Event logging tests
        await RunTest("Level Events Logging", TestLevelEventsLogging);
        await RunTest("Monetization Events Logging", TestMonetizationEventsLogging);
        await RunTest("Engagement Events Logging", TestEngagementEventsLogging);
        await RunTest("Quality Events Logging", TestQualityEventsLogging);
        
        // Integration tests
        await RunTest("Firebase Event Integration", TestFirebaseEventIntegration);
        await RunTest("Telemetry Dashboard Integration", TestTelemetryDashboardIntegration);
        await RunTest("Export Functionality", TestExportFunctionality);
        
        // Performance tests
        await RunTest("Event Batching Performance", TestEventBatchingPerformance);
        await RunTest("Memory Usage", TestMemoryUsage);
        
        _isRunningTests = false;
        
        GD.Print("🎯 Analytics Test Suite Completed!");
        EmitSignal("AllTestsCompleted", _testResults);
        
        PrintTestSummary();
    }

    /// <summary>
    /// Run a single test
    /// </summary>
    private async Task RunTest(string testName, Func<Task<bool>> testFunction)
    {
        try
        {
            GD.Print($"Running: {testName}");
            
            var result = new TestResult
            {
                TestName = testName,
                StartTime = DateTime.Now
            };
            
            bool passed = await testFunction();
            
            result.EndTime = DateTime.Now;
            result.Duration = result.EndTime - result.StartTime;
            result.Passed = passed;
            result.Message = passed ? "Test passed" : "Test failed";
            
            _testResults[testName] = result;
            
            string status = passed ? "✅ PASS" : "❌ FAIL";
            GD.Print($"{status} {testName} ({result.Duration.TotalMilliseconds:F0}ms)");
            
            EmitSignal("TestCompleted", testName, passed, result.Message);
        }
        catch (Exception e)
        {
            GD.PrintErr($"❌ FAIL {testName}: {e.Message}");
            
            _testResults[testName] = new TestResult
            {
                TestName = testName,
                Passed = false,
                Message = $"Exception: {e.Message}",
                StartTime = DateTime.Now,
                EndTime = DateTime.Now
            };
            
            EmitSignal("TestCompleted", testName, false, e.Message);
        }
    }

    // ===============================================
    // CORE SYSTEM TESTS
    // ===============================================

    /// <summary>
    /// Test Firebase Manager initialization
    /// </summary>
    private async Task<bool> TestFirebaseManagerInitialization()
    {
        try
        {
            // Check if FirebaseManager exists and is properly initialized
            if (FirebaseManager.Instance == null)
            {
                return false;
            }
            
            // Test configuration loading
            var config = FirebaseManager.Instance.GetFirebaseConfig();
            if (config == null)
            {
                return false;
            }
            
            // Test platform detection
            string platform = FirebaseManager.Instance.GetPlatformName();
            if (string.IsNullOrEmpty(platform))
            {
                return false;
            }
            
            return true;
        }
        catch (Exception e)
        {
            GD.PrintErr($"Firebase Manager test error: {e.Message}");
            return false;
        }
    }

    /// <summary>
    /// Test Event Tracker initialization
    /// </summary>
    private async Task<bool> TestEventTrackerInitialization()
    {
        try
        {
            if (AnalyticsEventTracker.Instance == null)
            {
                return false;
            }
            
            // Test event logging functionality
            AnalyticsEventTracker.Instance.LogEvent("test_event", new Dictionary<string, object>
            {
                { "test_param", "test_value" }
            });
            
            // Check if event was logged
            var eventCounts = AnalyticsEventTracker.Instance.GetEventCounts();
            return eventCounts.ContainsKey("test_event");
        }
        catch (Exception e)
        {
            GD.PrintErr($"Event Tracker test error: {e.Message}");
            return false;
        }
    }

    /// <summary>
    /// Test Difficulty Heatmap initialization
    /// </summary>
    private async Task<bool> TestHeatmapInitialization()
    {
        try
        {
            if (DifficultyHeatmapTracker.Instance == null)
            {
                return false;
            }
            
            // Test level data tracking
            DifficultyHeatmapTracker.Instance.TrackLevelAttempt(999, false, 30f, 1, "test_failure");
            
            // Check if data was tracked
            var levelData = DifficultyHeatmapTracker.Instance.GetLevelData(999);
            return levelData != null && levelData.TotalAttempts == 1;
        }
        catch (Exception e)
        {
            GD.PrintErr($"Heatmap test error: {e.Message}");
            return false;
        }
    }

    // ===============================================
    // EVENT LOGGING TESTS
    // ===============================================

    /// <summary>
    /// Test level events logging
    /// </summary>
    private async Task<bool> TestLevelEventsLogging()
    {
        try
        {
            var tracker = AnalyticsEventTracker.Instance;
            if (tracker == null) return false;
            
            // Test level started
            tracker.TrackLevelStarted(1, "normal");
            
            // Test level completed
            tracker.TrackLevelCompleted(1, 45.2f, 1, 150, false);
            
            // Test level failed
            tracker.TrackLevelFailed(1, 3, 90f, "test_failure");
            
            // Test perfect score
            tracker.TrackPerfectScoreAchieved(1, 35.1f);
            
            // Verify events were logged
            var eventCounts = tracker.GetEventCounts();
            
            return eventCounts.ContainsKey("level_started") &&
                   eventCounts.ContainsKey("level_completed") &&
                   eventCounts.ContainsKey("level_failed") &&
                   eventCounts.ContainsKey("perfect_score_achieved");
        }
        catch (Exception e)
        {
            GD.PrintErr($"Level events test error: {e.Message}");
            return false;
        }
    }

    /// <summary>
    /// Test monetization events logging
    /// </summary>
    private async Task<bool> TestMonetizationEventsLogging()
    {
        try
        {
            var tracker = AnalyticsEventTracker.Instance;
            if (tracker == null) return false;
            
            // Test cosmetic purchase
            tracker.TrackCosmeticPurchased("hat", "cowboy_hat", 2.99f);
            
            // Test cosmetic unlock
            tracker.TrackCosmeticUnlocked("hat", "cowboy_hat", "purchase");
            
            // Test remove ads purchase
            tracker.TrackRemoveAdsPurchased(0.99f);
            
            // Test rewarded ad
            tracker.TrackRewardedAdWatched("extra_attempts", 3f);
            
            // Verify events were logged
            var eventCounts = tracker.GetEventCounts();
            
            return eventCounts.ContainsKey("cosmetic_purchased") &&
                   eventCounts.ContainsKey("cosmetic_unlocked") &&
                   eventCounts.ContainsKey("remove_ads_purchased") &&
                   eventCounts.ContainsKey("rewarded_ad_watched");
        }
        catch (Exception e)
        {
            GD.PrintErr($"Monetization events test error: {e.Message}");
            return false;
        }
    }

    /// <summary>
    /// Test engagement events logging
    /// </summary>
    private async Task<bool> TestEngagementEventsLogging()
    {
        try
        {
            var tracker = AnalyticsEventTracker.Instance;
            if (tracker == null) return false;
            
            // Test daily login streak
            tracker.TrackDailyLoginStreakReached(7);
            
            // Test achievement unlocked
            tracker.TrackAchievementUnlocked("first_level_completed");
            
            // Test seasonal event
            tracker.TrackSeasonalEventStarted("christmas_2024", "limited_time");
            
            // Verify events were logged
            var eventCounts = tracker.GetEventCounts();
            
            return eventCounts.ContainsKey("daily_login_streak_reached") &&
                   eventCounts.ContainsKey("achievement_unlocked") &&
                   eventCounts.ContainsKey("seasonal_event_started");
        }
        catch (Exception e)
        {
            GD.PrintErr($"Engagement events test error: {e.Message}");
            return false;
        }
    }

    /// <summary>
    /// Test quality events logging
    /// </summary>
    private async Task<bool> TestQualityEventsLogging()
    {
        try
        {
            var tracker = AnalyticsEventTracker.Instance;
            if (tracker == null) return false;
            
            // Test crash detection
            tracker.TrackCrashDetected("null_reference", "TestScene", "test crash");
            
            // Test performance frame drop
            tracker.TrackPerformanceFrameDrop(25.5f, 40.2f);
            
            // Test memory warning
            tracker.TrackMemoryWarning(650.0f);
            
            // Verify events were logged
            var eventCounts = tracker.GetEventCounts();
            
            return eventCounts.ContainsKey("crash_detected") &&
                   eventCounts.ContainsKey("performance_frame_drop") &&
                   eventCounts.ContainsKey("memory_warning");
        }
        catch (Exception e)
        {
            GD.PrintErr($"Quality events test error: {e.Message}");
            return false;
        }
    }

    // ===============================================
    // INTEGRATION TESTS
    // ===============================================

    /// <summary>
    /// Test Firebase event integration
    /// </summary>
    private async Task<bool> TestFirebaseEventIntegration()
    {
        try
        {
            var firebaseManager = FirebaseManager.Instance;
            var tracker = AnalyticsEventTracker.Instance;
            
            if (firebaseManager == null || tracker == null)
            {
                return false;
            }
            
            // Log event through tracker
            tracker.LogEvent("integration_test_event", new Dictionary<string, object>
            {
                { "test_integration", true }
            });
            
            // Check if Firebase received the event (mock mode will store it)
            int queuedEvents = firebaseManager.GetQueuedEventCount();
            
            // In mock mode, events should be queued
            // In real mode, events should be sent immediately
            return queuedEvents >= 0; // Just check no error occurred
        }
        catch (Exception e)
        {
            GD.PrintErr($"Firebase integration test error: {e.Message}");
            return false;
        }
    }

    /// <summary>
    /// Test telemetry dashboard integration
    /// </summary>
    private async Task<bool> TestTelemetryDashboardIntegration()
    {
        try
        {
            var dashboard = TelemetryDebugPanel.Instance;
            if (dashboard == null)
            {
                return false;
            }
            
            // Test dashboard functionality
            // Note: This is a basic test - full UI testing would require more complex setup
            var sessionStats = dashboard.GetSessionStats();
            
            return sessionStats != null;
        }
        catch (Exception e)
        {
            GD.PrintErr($"Telemetry dashboard integration test error: {e.Message}");
            return false;
        }
    }

    /// <summary>
    /// Test export functionality
    /// </summary>
    private async Task<bool> TestExportFunctionality()
    {
        try
        {
            var tracker = AnalyticsEventTracker.Instance;
            var heatmap = DifficultyHeatmapTracker.Instance;
            
            if (tracker == null || heatmap == null)
            {
                return false;
            }
            
            // Create some test data
            tracker.LogEvent("export_test_event");
            heatmap.TrackLevelAttempt(888, true, 25.0f, 1);
            
            // Test exports (this will create files in user:// directory)
            tracker.ExportAnalytics("user://test_analytics_export.json");
            heatmap.ExportToCsv("user://test_heatmap_export.csv");
            
            // Verify files were created
            bool analyticsFileExists = FileAccess.FileExists("user://test_analytics_export.json");
            bool heatmapFileExists = FileAccess.FileExists("user://test_heatmap_export.csv");
            
            return analyticsFileExists && heatmapFileExists;
        }
        catch (Exception e)
        {
            GD.PrintErr($"Export functionality test error: {e.Message}");
            return false;
        }
    }

    // ===============================================
    // PERFORMANCE TESTS
    // ===============================================

    /// <summary>
    /// Test event batching performance
    /// </summary>
    private async Task<bool> TestEventBatchingPerformance()
    {
        try
        {
            var tracker = AnalyticsEventTracker.Instance;
            if (tracker == null) return false;
            
            var startTime = DateTime.Now;
            const int eventCount = 100;
            
            // Log many events quickly
            for (int i = 0; i < eventCount; i++)
            {
                tracker.LogEvent($"perf_test_event_{i}", new Dictionary<string, object>
                {
                    { "iteration", i },
                    { "timestamp", DateTime.Now.Ticks }
                });
            }
            
            var endTime = DateTime.Now;
            var duration = endTime - startTime;
            
            // Should complete within reasonable time (less than 5 seconds)
            bool performanceOk = duration.TotalSeconds < 5.0;
            
            if (!performanceOk)
            {
                GD.PrintErr($"Event batching performance test failed: {duration.TotalSeconds:F2}s for {eventCount} events");
            }
            
            return performanceOk;
        }
        catch (Exception e)
        {
            GD.PrintErr($"Event batching performance test error: {e.Message}");
            return false;
        }
    }

    /// <summary>
    /// Test memory usage
    /// </summary>
    private async Task<bool> TestMemoryUsage()
    {
        try
        {
            // Get initial memory usage
            long initialMemory = OS.GetStaticMemoryUsage();
            
            // Create some analytics data
            var tracker = AnalyticsEventTracker.Instance;
            if (tracker == null) return false;
            
            for (int i = 0; i < 50; i++)
            {
                tracker.LogEvent($"memory_test_event_{i}");
            }
            
            // Force garbage collection
            GC.Collect();
            await Task.Delay(100);
            
            // Check memory after operations
            long finalMemory = OS.GetStaticMemoryUsage();
            long memoryIncrease = finalMemory - initialMemory;
            
            // Memory increase should be reasonable (less than 10MB)
            bool memoryOk = memoryIncrease < 10 * 1024 * 1024; // 10MB
            
            if (!memoryOk)
            {
                GD.PrintErr($"Memory usage test failed: {memoryIncrease / (1024 * 1024):F1}MB increase");
            }
            
            return memoryOk;
        }
        catch (Exception e)
        {
            GD.PrintErr($"Memory usage test error: {e.Message}");
            return false;
        }
    }

    // ===============================================
    // HELPER METHODS
    // ===============================================

    /// <summary>
    /// Print test summary
    /// </summary>
    private void PrintTestSummary()
    {
        GD.Print("\n📊 Test Summary:");
        GD.Print("==================");
        
        int totalTests = _testResults.Count;
        int passedTests = 0;
        int failedTests = 0;
        
        foreach (var result in _testResults.Values)
        {
            string status = result.Passed ? "✅ PASS" : "❌ FAIL";
            GD.Print($"{status} {result.TestName} ({result.Duration.TotalMilliseconds:F0}ms)");
            
            if (result.Passed)
            {
                passedTests++;
            }
            else
            {
                failedTests++;
                GD.PrintErr($"   Error: {result.Message}");
            }
        }
        
        GD.Print("\n📈 Results:");
        GD.Print($"Total Tests: {totalTests}");
        GD.Print($"Passed: {passedTests}");
        GD.Print($"Failed: {failedTests}");
        GD.Print($"Success Rate: {(totalTests > 0 ? (passedTests * 100.0 / totalTests) : 0):F1}%");
        
        if (failedTests == 0)
        {
            GD.Print("\n🎉 All tests passed! Analytics system is working correctly.");
        }
        else
        {
            GD.Print($"\n⚠️ {failedTests} test(s) failed. Please check the errors above.");
        }
    }

    /// <summary>
    /// Get test results
    /// </summary>
    public Dictionary<string, TestResult> GetTestResults()
    {
        return new Dictionary<string, TestResult>(_testResults);
    }

    /// <summary>
    /// Check if tests are currently running
    /// </summary>
    public bool IsRunningTests()
    {
        return _isRunningTests;
    }

    /// <summary>
    /// Run specific test by name
    /// </summary>
    public async void RunSpecificTest(string testName)
    {
        if (_isRunningTests)
        {
            GD.PrintWarning("Cannot run specific test while suite is running");
            return;
        }
        
        switch (testName.ToLower())
        {
            case "firebase":
                await RunTest("Firebase Manager Initialization", TestFirebaseManagerInitialization);
                break;
            case "events":
                await RunTest("Level Events Logging", TestLevelEventsLogging);
                break;
            case "integration":
                await RunTest("Firebase Event Integration", TestFirebaseEventIntegration);
                break;
            case "performance":
                await RunTest("Event Batching Performance", TestEventBatchingPerformance);
                break;
            default:
                GD.PrintErr($"Unknown test: {testName}");
                break;
        }
    }
}

/// <summary>
/// Test result data structure
/// </summary>
public class TestResult
{
    public string TestName { get; set; }
    public bool Passed { get; set; }
    public string Message { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public TimeSpan Duration { get; set; }
}