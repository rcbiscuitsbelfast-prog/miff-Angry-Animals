using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

/// <summary>
/// Manages A/B test variants, user segmentation, and statistical analysis
/// Inspector-configurable tests with persistent user assignment
/// </summary>
public class ABTestingManager : Node
{
    public static ABTestingManager Instance { get; private set; }

    // Test management
    private List<ABTest> _activeTests = new List<ABTest>();
    private Dictionary<string, ABTest> _completedTests = new Dictionary<string, ABTest>();
    private Dictionary<string, string> _userAssignments = new Dictionary<string, string>(); // user_id -> test_id:variant_id
    
    // Configuration
    private Dictionary<string, object> _testConfigs = new Dictionary<string, object>();
    private string _currentUserId;
    
    [Signal]
    public delegate void TestVariantAssignedEventHandler(string testId, string variantId, string userId);
    
    [Signal]
    public delegate void TestCompletedEventHandler(string testId, string winningVariant, ABTestResult result);
    
    [Signal]
    public delegate void ConversionTrackedEventHandler(string testId, string variantId, string conversionType, float value);

    public override void _Ready()
    {
        if (Instance != null)
        {
            QueueFree();
            return;
        }
        
        Instance = this;
        InitializeABTesting();
    }

    /// <summary>
    /// Initialize A/B testing system
    /// </summary>
    private void InitializeABTesting()
    {
        _currentUserId = PlayerProfile.Instance?.PlayerId ?? "anonymous";
        LoadTestConfigurations();
        InitializePreConfiguredTests();
        
        GD.Print("A/B Testing Manager initialized");
    }

    /// <summary>
    /// Load test configurations from Firebase Remote Config
    /// </summary>
    private void LoadTestConfigurations()
    {
        // Default test configurations - can be overridden by Firebase Remote Config
        _testConfigs["cosmetics_price_test"] = new Dictionary<string, object>
        {
            ["test_name"] = "Cosmetics Pricing Test",
            ["description"] = "Test different price points for cosmetics",
            ["variants"] = new Dictionary<string, object>
            {
                ["control"] = new Dictionary<string, object> { ["price"] = 2.99f },
                ["variant_1"] = new Dictionary<string, object> { ["price"] = 3.99f },
                ["variant_2"] = new Dictionary<string, object> { ["price"] = 4.99f }
            },
            ["traffic_split"] = new Dictionary<string, float>
            {
                ["control"] = 0.33f,
                ["variant_1"] = 0.33f,
                ["variant_2"] = 0.34f
            },
            ["duration_days"] = 14,
            ["target_metric"] = "conversion_rate"
        };

        _testConfigs["ad_frequency_test"] = new Dictionary<string, object>
        {
            ["test_name"] = "Ad Frequency Test",
            ["description"] = "Test different ad placement strategies",
            ["variants"] = new Dictionary<string, object>
            {
                ["control"] = new Dictionary<string, object> { ["interstitial_frequency"] = 5 },
                ["variant_1"] = new Dictionary<string, object> { ["interstitial_frequency"] = 3 },
                ["variant_2"] = new Dictionary<string, object> { ["interstitial_frequency"] = 8 }
            },
            ["traffic_split"] = new Dictionary<string, float>
            {
                ["control"] = 0.33f,
                ["variant_1"] = 0.33f,
                ["variant_2"] = 0.34f
            },
            ["duration_days"] = 21,
            ["target_metric"] = "arpu"
        };

        _testConfigs["notification_test"] = new Dictionary<string, object>
        {
            ["test_name"] = "Push Notification Test",
            ["description"] = "Test different notification strategies",
            ["variants"] = new Dictionary<string, object>
            {
                ["control"] = new Dictionary<string, object> { ["send_time"] = "09:00", ["message_type"] = "standard" },
                ["variant_1"] = new Dictionary<string, object> { ["send_time"] = "07:00", ["message_type"] = "personalized" },
                ["variant_2"] = new Dictionary<string, object> { ["send_time"] = "11:00", ["message_type"] = "emojis" }
            },
            ["traffic_split"] = new Dictionary<string, float>
            {
                ["control"] = 0.33f,
                ["variant_1"] = 0.33f,
                ["variant_2"] = 0.34f
            },
            ["duration_days"] = 28,
            ["target_metric"] = "retention_d1"
        };
    }

    /// <summary>
    /// Initialize pre-configured A/B tests
    /// </summary>
    private void InitializePreConfiguredTests()
    {
        CreateTestFromConfig("cosmetics_price_test");
        CreateTestFromConfig("ad_frequency_test");
        CreateTestFromConfig("notification_test");
    }

    /// <summary>
    /// Create an A/B test from configuration
    /// </summary>
    private void CreateTestFromConfig(string configKey)
    {
        if (!_testConfigs.ContainsKey(configKey)) return;

        var config = (Dictionary<string, object>)_testConfigs[configKey];
        var testId = configKey;
        
        var test = new ABTest
        {
            TestId = testId,
            TestName = config["test_name"].ToString(),
            Description = config["description"].ToString(),
            StartDate = DateTime.Now,
            EndDate = DateTime.Now.AddDays(Convert.ToInt32(config["duration_days"])),
            TargetMetric = config["target_metric"].ToString(),
            IsActive = true
        };

        // Create variants
        var variants = (Dictionary<string, object>)config["variants"];
        var trafficSplit = (Dictionary<string, float>)config["traffic_split"];
        
        foreach (var variantPair in variants)
        {
            var variant = new ABTestVariant
            {
                VariantId = variantPair.Key,
                VariantName = variantPair.Key,
                Configuration = (Dictionary<string, object>)variantPair.Value,
                TrafficPercentage = trafficSplit[variantPair.Key] * 100f,
                UsersAssigned = 0,
                Conversions = 0
            };
            test.Variants.Add(variant);
        }

        _activeTests.Add(test);
        AssignUsersToTest(test);
        
        GD.Print($"Created A/B test: {test.TestName} with {test.Variants.Count} variants");
    }

    /// <summary>
    /// Assign users to test variants based on traffic split
    /// </summary>
    private void AssignUsersToTest(ABTest test)
    {
        var userHash = _currentUserId.GetHashCode();
        var assignment = userHash % 100;
        
        float cumulativePercentage = 0f;
        foreach (var variant in test.Variants)
        {
            cumulativePercentage += variant.TrafficPercentage;
            if (assignment < cumulativePercentage)
            {
                var assignmentKey = $"{test.TestId}:{variant.VariantId}";
                _userAssignments[_currentUserId] = assignmentKey;
                
                EmitSignal("TestVariantAssigned", test.TestId, variant.VariantId, _currentUserId);
                
                // Track assignment in analytics
                if (AnalyticsEventTracker.Instance != null)
                {
                    AnalyticsEventTracker.Instance.TrackEvent("ab_test_variant_assigned", new Dictionary<string, object>
                    {
                        ["test_id"] = test.TestId,
                        ["variant_id"] = variant.VariantId,
                        ["user_id"] = _currentUserId,
                        ["timestamp"] = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                    });
                }
                
                variant.UsersAssigned++;
                break;
            }
        }
    }

    /// <summary>
    /// Get the variant assigned to the current user for a specific test
    /// </summary>
    public string GetUserVariant(string testId)
    {
        if (!_userAssignments.ContainsKey(_currentUserId)) return null;
        
        var assignment = _userAssignments[_currentUserId];
        if (assignment.StartsWith(testId + ":"))
        {
            return assignment.Substring(testId.Length + 1);
        }
        
        return null;
    }

    /// <summary>
    /// Get the configuration for a specific test variant
    /// </summary>
    public Dictionary<string, object> GetVariantConfiguration(string testId, string variantId)
    {
        var test = _activeTests.FirstOrDefault(t => t.TestId == testId);
        if (test == null) return null;

        var variant = test.Variants.FirstOrDefault(v => v.VariantId == variantId);
        return variant?.Configuration;
    }

    /// <summary>
    /// Track a conversion for the current user in a test
    /// </summary>
    public void TrackConversion(string testId, string conversionType, float value = 1.0f)
    {
        var variantId = GetUserVariant(testId);
        if (variantId == null) return;

        var test = _activeTests.FirstOrDefault(t => t.TestId == testId);
        if (test == null) return;

        var variant = test.Variants.FirstOrDefault(v => v.VariantId == variantId);
        if (variant == null) return;

        variant.Conversions += (int)value;
        
        EmitSignal("ConversionTracked", testId, variantId, conversionType, value);

        // Track in analytics
        if (AnalyticsEventTracker.Instance != null)
        {
            AnalyticsEventTracker.Instance.TrackEvent("ab_test_conversion", new Dictionary<string, object>
            {
                ["test_id"] = testId,
                ["variant_id"] = variantId,
                ["conversion_type"] = conversionType,
                ["value"] = value,
                ["user_id"] = _currentUserId,
                ["timestamp"] = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            });
        }

        GD.Print($"Tracked conversion: {testId}/{variantId} - {conversionType}: {value}");
    }

    /// <summary>
    /// Check if a test has completed and determine the winner
    /// </summary>
    public void CheckForTestCompletion()
    {
        var completedTests = new List<ABTest>();

        foreach (var test in _activeTests)
        {
            if (DateTime.Now >= test.EndDate || HasStatisticalSignificance(test))
            {
                var winningVariant = DetermineWinningVariant(test);
                var result = CalculateTestResult(test, winningVariant);
                
                completedTests.Add(test);
                _completedTests[test.TestId] = test;
                
                EmitSignal("TestCompleted", test.TestId, winningVariant.VariantId, result);
                
                GD.Print($"A/B Test completed: {test.TestName} - Winner: {winningVariant.VariantId} (Conversion: {result.ConversionRate:P2})");
            }
        }

        // Remove completed tests
        foreach (var test in completedTests)
        {
            _activeTests.Remove(test);
        }
    }

    /// <summary>
    /// Determine if a test has statistical significance
    /// </summary>
    private bool HasStatisticalSignificance(ABTest test)
    {
        // Simple implementation - requires minimum 100 users per variant and 95% confidence
        foreach (var variant in test.Variants)
        {
            if (variant.UsersAssigned < 100) return false;
        }

        // Check for clear winner with sufficient conversion difference
        var sortedVariants = test.Variants.OrderByDescending(v => v.ConversionRate).ToList();
        if (sortedVariants.Count < 2) return false;

        var bestVariant = sortedVariants[0];
        var secondBest = sortedVariants[1];
        
        var conversionDifference = bestVariant.ConversionRate - secondBest.ConversionRate;
        return conversionDifference > 0.05f; // 5% difference threshold
    }

    /// <summary>
    /// Determine the winning variant based on target metric
    /// </summary>
    private ABTestVariant DetermineWinningVariant(ABTest test)
    {
        return test.Variants.OrderByDescending(v => v.ConversionRate).First();
    }

    /// <summary>
    /// Calculate detailed test result
    /// </summary>
    private ABTestResult CalculateTestResult(ABTest test, ABTestVariant winningVariant)
    {
        var totalUsers = test.Variants.Sum(v => v.UsersAssigned);
        var totalConversions = test.Variants.Sum(v => v.Conversions);
        
        var result = new ABTestResult
        {
            TestId = test.TestId,
            WinningVariant = winningVariant.VariantId,
            ConversionRate = winningVariant.ConversionRate,
            Uplift = CalculateUplift(test),
            Confidence = CalculateConfidence(test, winningVariant),
            PValue = CalculatePValue(test, winningVariant),
            SampleSize = totalUsers,
            Duration = (test.EndDate - test.StartDate).Days
        };

        return result;
    }

    /// <summary>
    /// Calculate uplift compared to control group
    /// </summary>
    private float CalculateUplift(ABTest test)
    {
        var controlVariant = test.Variants.FirstOrDefault(v => v.VariantId == "control");
        var bestVariant = test.Variants.OrderByDescending(v => v.ConversionRate).First();
        
        if (controlVariant == null || controlVariant.ConversionRate <= 0) return 0f;
        
        return (bestVariant.ConversionRate - controlVariant.ConversionRate) / controlVariant.ConversionRate;
    }

    /// <summary>
    /// Calculate confidence level (simplified)
    /// </summary>
    private float CalculateConfidence(ABTest test, ABTestVariant winningVariant)
    {
        // Simplified confidence calculation
        var totalUsers = test.Variants.Sum(v => v.UsersAssigned);
        var winnerUsers = winningVariant.UsersAssigned;
        
        return Mathf.Min(winnerUsers / (float)totalUsers * 100f, 95f);
    }

    /// <summary>
    /// Calculate p-value (simplified)
    /// </summary>
    private float CalculatePValue(ABTest test, ABTestVariant winningVariant)
    {
        // Simplified p-value calculation
        // In a real implementation, you'd use proper statistical tests
        var conversionDiff = winningVariant.ConversionRate - test.Variants.Average(v => v.ConversionRate);
        return Mathf.Clamp(1.0f - (conversionDiff * 10), 0.01f, 0.99f);
    }

    /// <summary>
    /// Get all active tests
    /// </summary>
    public List<ABTest> GetActiveTests()
    {
        return _activeTests.ToList();
    }

    /// <summary>
    /// Get test by ID
    /// </summary>
    public ABTest GetTest(string testId)
    {
        return _activeTests.FirstOrDefault(t => t.TestId == testId) ?? 
               _completedTests.GetValueOrDefault(testId);
    }

    /// <summary>
    /// Get test results for UI display
    /// </summary>
    public List<Dictionary<string, object>> GetTestResultsForUI()
    {
        var results = new List<Dictionary<string, object>>();

        foreach (var test in _activeTests)
        {
            var testResult = new Dictionary<string, object>
            {
                ["test_id"] = test.TestId,
                ["test_name"] = test.TestName,
                ["description"] = test.Description,
                ["days_remaining"] = Mathf.Max(0, (test.EndDate - DateTime.Now).Days),
                ["variants"] = new List<Dictionary<string, object>>()
            };

            foreach (var variant in test.Variants)
            {
                var variantData = new Dictionary<string, object>
                {
                    ["variant_id"] = variant.VariantId,
                    ["variant_name"] = variant.VariantName,
                    ["users_assigned"] = variant.UsersAssigned,
                    ["conversions"] = variant.Conversions,
                    ["conversion_rate"] = variant.ConversionRate,
                    ["traffic_percentage"] = variant.TrafficPercentage
                };
                
                ((List<Dictionary<string, object>>)testResult["variants"]).Add(variantData);
            }

            results.Add(testResult);
        }

        return results;
    }

    /// <summary>
    /// Export test data to CSV
    /// </summary>
    public string ExportTestDataToCSV()
    {
        var csv = new System.Text.StringBuilder();
        csv.AppendLine("Test Name,Variant,Users Assigned,Conversions,Conversion Rate,Uplift");
        
        foreach (var test in _activeTests.Concat(_completedTests.Values))
        {
            var controlRate = test.Variants.FirstOrDefault(v => v.VariantId == "control")?.ConversionRate ?? 0;
            
            foreach (var variant in test.Variants)
            {
                var uplift = controlRate > 0 ? (variant.ConversionRate - controlRate) / controlRate : 0;
                csv.AppendLine($"{test.TestName},{variant.VariantId},{variant.UsersAssigned},{variant.Conversions},{variant.ConversionRate:P4},{uplift:P2}");
            }
        }
        
        return csv.ToString();
    }

    public override void _Process(float delta)
    {
        // Check for test completion every minute
        if (Time.GetTicksMsec() % 60000 < 16) // Approximately every 60 seconds
        {
            CheckForTestCompletion();
        }
    }
}

/// <summary>
/// Represents an A/B test
/// </summary>
public class ABTest
{
    public string TestId { get; set; }
    public string TestName { get; set; }
    public string Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string TargetMetric { get; set; }
    public bool IsActive { get; set; }
    public List<ABTestVariant> Variants { get; set; } = new List<ABTestVariant>();
}

/// <summary>
/// Represents a test variant
/// </summary>
public class ABTestVariant
{
    public string VariantId { get; set; }
    public string VariantName { get; set; }
    public Dictionary<string, object> Configuration { get; set; }
    public float TrafficPercentage { get; set; }
    public int UsersAssigned { get; set; }
    public int Conversions { get; set; }
    
    public float ConversionRate => UsersAssigned > 0 ? (float)Conversions / UsersAssigned : 0f;
}

/// <summary>
/// Represents test results
/// </summary>
public class ABTestResult
{
    public string TestId { get; set; }
    public string WinningVariant { get; set; }
    public float ConversionRate { get; set; }
    public float Uplift { get; set; }
    public float Confidence { get; set; }
    public float PValue { get; set; }
    public int SampleSize { get; set; }
    public int Duration { get; set; }
}