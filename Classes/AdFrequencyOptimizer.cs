using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

/// <summary>
/// Intelligent ad placement optimizer using A/B testing to balance revenue and retention
/// Implements smart placement strategies based on player behavior and game state
/// </summary>
public class AdFrequencyOptimizer : Node
{
    public static AdFrequencyOptimizer Instance { get; private set; }

    // Ad strategy configurations
    private Dictionary<AdStrategyType, AdStrategyConfig> _adStrategies;
    
    // Current strategy being used
    private AdStrategyType _currentStrategy;
    private AdStrategyConfig _currentConfig;
    
    // Player behavior tracking
    private DateTime _lastAdShown;
    private int _adsShownThisSession;
    private List<DateTime> _recentAds = new List<DateTime>();
    private float _sessionPlayTime;
    private bool _playerIsFrustrated;
    
    // A/B Testing integration
    private ABTestingManager _abTestingManager;
    
    // Quiet hours configuration
    private TimeSpan _quietStart = new TimeSpan(22, 0, 0); // 10 PM
    private TimeSpan _quietEnd = new TimeSpan(8, 0, 0);   // 8 AM
    
    // Ad limits
    private const int MAX_ADS_PER_30_MIN = 3;
    private const float MIN_AD_INTERVAL_SECONDS = 60f;
    
    [Signal]
    public delegate void AdStrategyChangedEventHandler(AdStrategyType newStrategy);
    
    [Signal]
    public delegate void AdPlacementOptimizedEventHandler(string placementReason, bool adShown);
    
    [Signal]
    public delegate void AdFrequencyMetricsUpdatedEventHandler(Dictionary<string, object> metrics);

    public override void _Ready()
    {
        if (Instance != null)
        {
            QueueFree();
            return;
        }
        
        Instance = this;
        InitializeAdOptimizer();
    }

    /// <summary>
    /// Initialize ad frequency optimizer
    /// </summary>
    private void InitializeAdOptimizer()
    {
        InitializeAdStrategies();
        LoadCurrentStrategy();
        InitializeABTestingIntegration();
        ResetSessionMetrics();
        
        GD.Print("Ad Frequency Optimizer initialized");
    }

    /// <summary>
    /// Initialize ad strategy configurations
    /// </summary>
    private void InitializeAdStrategies()
    {
        _adStrategies = new Dictionary<AdStrategyType, AdStrategyConfig>
        {
            [AdStrategyType.Aggressive] = new AdStrategyConfig
            {
                StrategyName = "Aggressive",
                InterstitialFrequency = 2, // Every 2 levels
                RewardedAdProminence = 0.9f,
                BannerAdAlwaysVisible = true,
                ExpectedARPPU = 8.50f,
                ExpectedRetentionDrop = 0.15f,
                Description = "Maximum revenue, may hurt retention"
            },
            
            [AdStrategyType.Balanced] = new AdStrategyConfig
            {
                StrategyName = "Balanced",
                InterstitialFrequency = 5, // Every 5 levels
                RewardedAdProminence = 0.6f,
                BannerAdAlwaysVisible = false,
                ExpectedARPPU = 6.20f,
                ExpectedRetentionDrop = 0.05f,
                Description = "Good balance of revenue and retention"
            },
            
            [AdStrategyType.Conservative] = new AdStrategyConfig
            {
                StrategyName = "Conservative",
                InterstitialFrequency = 8, // Every 8 levels
                RewardedAdProminence = 0.3f,
                BannerAdAlwaysVisible = false,
                ExpectedARPPU = 4.80f,
                ExpectedRetentionDrop = 0.01f,
                Description = "Best retention, lower revenue"
            }
        };
    }

    /// <summary>
    /// Load current strategy (from player preferences or A/B testing)
    /// </summary>
    private void LoadCurrentStrategy()
    {
        // Check A/B testing assignment first
        var abVariant = ABTestingManager.Instance?.GetUserVariant("ad_frequency_test");
        
        if (abVariant != null)
        {
            switch (abVariant.ToLower())
            {
                case "control":
                    _currentStrategy = AdStrategyType.Balanced;
                    break;
                case "variant_1":
                    _currentStrategy = AdStrategyType.Aggressive;
                    break;
                case "variant_2":
                    _currentStrategy = AdStrategyType.Conservative;
                    break;
                default:
                    _currentStrategy = AdStrategyType.Balanced;
                    break;
            }
        }
        else
        {
            // Default to balanced strategy
            _currentStrategy = AdStrategyType.Balanced;
        }
        
        _currentConfig = _adStrategies[_currentStrategy];
        
        GD.Print($"Using ad strategy: {_currentConfig.StrategyName}");
    }

    /// <summary>
    /// Initialize A/B testing integration
    /// </summary>
    private void InitializeABTestingIntegration()
    {
        _abTestingManager = ABTestingManager.Instance;
        
        // Listen for A/B test variant changes
        if (_abTestingManager != null)
        {
            _abTestingManager.Connect("TestVariantAssigned", new Callable(this, nameof(OnABTestVariantChanged)));
        }
    }

    /// <summary>
    /// Handle A/B test variant changes
    /// </summary>
    private void OnABTestVariantChanged(string testId, string variantId, string userId)
    {
        if (testId != "ad_frequency_test") return;
        
        LoadCurrentStrategy();
        EmitSignal("AdStrategyChanged", _currentStrategy);
        
        GD.Print($"A/B test variant changed to {variantId}, new strategy: {_currentConfig.StrategyName}");
    }

    /// <summary>
    /// Reset session metrics
    /// </summary>
    private void ResetSessionMetrics()
    {
        _lastAdShown = DateTime.MinValue;
        _adsShownThisSession = 0;
        _recentAds.Clear();
        _sessionPlayTime = 0f;
        _playerIsFrustrated = false;
    }

    /// <summary>
    /// Update session metrics
    /// </summary>
    public override void _Process(float delta)
    {
        _sessionPlayTime += delta;
        
        // Clean up old ad timestamps (older than 30 minutes)
        _recentAds.RemoveAll(adTime => (DateTime.Now - adTime).TotalMinutes > 30);
        
        // Check for player frustration (simplified heuristic)
        CheckPlayerFrustration();
        
        // Update metrics periodically
        if (Time.GetTicksMsec() % 30000 < 16) // Every 30 seconds
        {
            UpdateAdFrequencyMetrics();
        }
    }

    /// <summary>
    /// Check if player is frustrated based on recent behavior
    /// </summary>
    private void CheckPlayerFrustration()
    {
        // Simple frustration indicators:
        // - Rapid level failures
        // - Short session times
        // - Quick app switching
        
        var currentTime = DateTime.Now;
        
        // If player has failed 3+ levels in last 5 minutes, they're frustrated
        var recentFailures = DifficultyHeatmapAnalyzer.Instance?
            .GetAllDifficultyData()
            .Where(kvp => (currentTime - kvp.Value.LastAttemptTime).TotalMinutes < 5)
            .Where(kvp => kvp.Value.FailedAttempts > 0)
            .Count() ?? 0;
            
        _playerIsFrustrated = recentFailures >= 3;
    }

    /// <summary>
    /// Determine if an interstitial ad should be shown
    /// </summary>
    public bool ShouldShowInterstitialAd(string gameState, int levelsCompleted)
    {
        // Check quiet hours
        if (IsQuietHours()) return false;
        
        // Check ad frequency limits
        if (IsAdLimitReached()) return false;
        
        // Check interval since last ad
        if ((DateTime.Now - _lastAdShown).TotalSeconds < MIN_AD_INTERVAL_SECONDS) return false;
        
        // Don't show ads when player is frustrated
        if (_playerIsFrustrated && gameState == "level_failed") return false;
        
        // Check strategic frequency
        if (levelsCompleted % _currentConfig.InterstitialFrequency != 0) return false;
        
        // Don't show ads immediately after starting the game
        if (_sessionPlayTime < 60f) return false; // First minute
        
        // A/B test: some variants may disable certain ad types
        var adVariant = _abTestingManager?.GetUserVariant("ad_frequency_test");
        if (adVariant == "variant_2" && _currentStrategy == AdStrategyType.Conservative && levelsCompleted < 10)
        {
            return false; // Conservative variant delays early ads
        }
        
        return true;
    }

    /// <summary>
    /// Determine if rewarded ad should be prominently displayed
    /// </summary>
    public float GetRewardedAdProminence()
    {
        // Base prominence from strategy
        var prominence = _currentConfig.RewardedAdProminence;
        
        // Reduce prominence if player is frustrated
        if (_playerIsFrustrated)
        {
            prominence *= 0.5f;
        }
        
        // Reduce prominence during quiet hours
        if (IsQuietHours())
        {
            prominence *= 0.3f;
        }
        
        // A/B test modifier
        var adVariant = _abTestingManager?.GetUserVariant("ad_frequency_test");
        if (adVariant == "variant_1" && _currentStrategy == AdStrategyType.Aggressive)
        {
            prominence = Mathf.Min(prominence * 1.2f, 1.0f); // Boost in aggressive variant
        }
        
        return Mathf.Clamp(prominence, 0f, 1f);
    }

    /// <summary>
    /// Determine if banner ad should be visible
    /// </summary>
    public bool ShouldShowBannerAd()
    {
        // Always check quiet hours
        if (IsQuietHours()) return false;
        
        // Strategy-based visibility
        if (!_currentConfig.BannerAdAlwaysVisible) return false;
        
        // Don't show banners when frustrated
        if (_playerIsFrustrated) return false;
        
        // A/B test consideration
        var adVariant = _abTestingManager?.GetUserVariant("ad_frequency_test");
        if (adVariant == "control" && _currentStrategy == AdStrategyType.Balanced)
        {
            return false; // Control group hides banners
        }
        
        return true;
    }

    /// <summary>
    /// Record that an ad was shown
    /// </summary>
    public void RecordAdShown(AdType adType)
    {
        _lastAdShown = DateTime.Now;
        _adsShownThisSession++;
        _recentAds.Add(DateTime.Now);
        
        // Track in A/B testing
        if (_abTestingManager != null)
        {
            _abTestingManager.TrackConversion("ad_frequency_test", "ad_shown");
        }
        
        // Track in analytics
        if (AnalyticsEventTracker.Instance != null)
        {
            AnalyticsEventTracker.Instance.TrackEvent("ad_shown", new Dictionary<string, object>
            {
                ["ad_type"] = adType.ToString(),
                ["strategy"] = _currentStrategy.ToString(),
                ["prominence"] = GetRewardedAdProminence(),
                ["player_frustrated"] = _playerIsFrustrated,
                ["session_time"] = _sessionPlayTime,
                ["ads_this_session"] = _adsShownThisSession
            });
        }
        
        EmitSignal("AdPlacementOptimized", "ad_shown", true);
    }

    /// <summary>
    /// Record ad completion (for revenue optimization)
    /// </summary>
    public void RecordAdCompleted(AdType adType)
    {
        // Track in A/B testing
        if (_abTestingManager != null)
        {
            _abTestingManager.TrackConversion("ad_frequency_test", "ad_completed");
        }
        
        // Track in analytics
        if (AnalyticsEventTracker.Instance != null)
        {
            AnalyticsEventTracker.Instance.TrackEvent("ad_completed", new Dictionary<string, object>
            {
                ["ad_type"] = adType.ToString(),
                ["strategy"] = _currentStrategy.ToString(),
                ["completion_rate"] = 1.0f
            });
        }
    }

    /// <summary>
    /// Record ad skip (for placement optimization)
    /// </summary>
    public void RecordAdSkipped(AdType adType)
    {
        // Track in analytics
        if (AnalyticsEventTracker.Instance != null)
        {
            AnalyticsEventTracker.Instance.TrackEvent("ad_skipped", new Dictionary<string, object>
            {
                ["ad_type"] = adType.ToString(),
                ["strategy"] = _currentStrategy.ToString(),
                ["skip_rate"] = 1.0f
            });
        }
    }

    /// <summary>
    /// Check if current time is within quiet hours
    /// </summary>
    private bool IsQuietHours()
    {
        var now = DateTime.Now.TimeOfDay;
        
        if (_quietStart > _quietEnd)
        {
            // Quiet hours span midnight
            return now >= _quietStart || now <= _quietEnd;
        }
        else
        {
            // Normal quiet hours
            return now >= _quietStart && now <= _quietEnd;
        }
    }

    /// <summary>
    /// Check if ad frequency limit is reached
    /// </summary>
    private bool IsAdLimitReached()
    {
        return _recentAds.Count >= MAX_ADS_PER_30_MIN;
    }

    /// <summary>
    /// Update ad frequency metrics
    /// </summary>
    private void UpdateAdFrequencyMetrics()
    {
        var metrics = new Dictionary<string, object>
        {
            ["current_strategy"] = _currentStrategy.ToString(),
            ["ads_shown_this_session"] = _adsShownThisSession,
            ["ads_per_hour"] = _sessionPlayTime > 0 ? (_adsShownThisSession / (_sessionPlayTime / 3600f)) : 0f,
            ["player_frustrated"] = _playerIsFrustrated,
            ["quiet_hours"] = IsQuietHours(),
            ["last_ad_ago_minutes"] = _lastAdShown == DateTime.MinValue ? -1 : (int)(DateTime.Now - _lastAdShown).TotalMinutes,
            ["recent_ads_count"] = _recentAds.Count,
            ["interstitial_frequency"] = _currentConfig.InterstitialFrequency,
            ["rewarded_prominence"] = GetRewardedAdProminence()
        };
        
        EmitSignal("AdFrequencyMetricsUpdated", metrics);
    }

    /// <summary>
    /// Get current ad strategy configuration
    /// </summary>
    public AdStrategyConfig GetCurrentStrategyConfig()
    {
        return _currentConfig;
    }

    /// <summary>
    /// Get all available strategies
    /// </summary>
    public Dictionary<AdStrategyType, AdStrategyConfig> GetAllStrategies()
    {
        return _adStrategies;
    }

    /// <summary>
    /// Switch to a different ad strategy (for testing)
    /// </summary>
    public void SwitchStrategy(AdStrategyType newStrategy)
    {
        if (!_adStrategies.ContainsKey(newStrategy)) return;
        
        _currentStrategy = newStrategy;
        _currentConfig = _adStrategies[newStrategy];
        
        GD.Print($"Switched to ad strategy: {_currentConfig.StrategyName}");
        EmitSignal("AdStrategyChanged", _currentStrategy);
    }

    /// <summary>
    /// Get optimal ad placement recommendations
    /// </summary>
    public List<string> GetAdPlacementRecommendations()
    {
        var recommendations = new List<string>();
        
        // Strategy-specific recommendations
        switch (_currentStrategy)
        {
            case AdStrategyType.Aggressive:
                recommendations.Add("Consider reducing interstitial frequency if retention drops");
                recommendations.Add("Monitor user feedback for ad fatigue");
                break;
                
            case AdStrategyType.Balanced:
                recommendations.Add("Good baseline strategy - monitor metrics for optimization");
                recommendations.Add("Test banner ad visibility in future A/B tests");
                break;
                
            case AdStrategyType.Conservative:
                recommendations.Add("Consider testing slightly higher frequency for revenue");
                recommendations.Add("Monitor if players complete more levels with fewer ads");
                break;
        }
        
        // Player behavior recommendations
        if (_playerIsFrustrated)
        {
            recommendations.Add("Player appears frustrated - reduce ad frequency temporarily");
            recommendations.Add("Consider showing helpful tips during ad breaks");
        }
        
        if (_adsShownThisSession > 5)
        {
            recommendations.Add("High ad frequency this session - consider player experience");
        }
        
        if (IsQuietHours())
        {
            recommendations.Add("Currently in quiet hours - ads suppressed for player comfort");
        }
        
        return recommendations;
    }

    /// <summary>
    /// Export ad frequency data for analysis
    /// </summary>
    public string ExportAdFrequencyDataToCSV()
    {
        var csv = new System.Text.StringBuilder();
        csv.AppendLine("Timestamp,Strategy,Ads Shown,Session Time (min),Player Frustrated,Quiet Hours,Recommendations");
        
        var metrics = new Dictionary<string, object>();
        EmitSignal("AdFrequencyMetricsUpdated", metrics);
        
        var recommendations = GetAdPlacementRecommendations();
        var recString = string.Join("; ", recommendations);
        
        csv.AppendLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss},{_currentStrategy},{_adsShownThisSession},{_sessionPlayTime/60f:F1},{_playerIsFrustrated},{IsQuietHours()},\"{recString}\"");
        
        return csv.ToString();
    }
}

/// <summary>
/// Ad strategy types
/// </summary>
public enum AdStrategyType
{
    Aggressive,
    Balanced,
    Conservative
}

/// <summary>
/// Ad strategy configuration
/// </summary>
public class AdStrategyConfig
{
    public string StrategyName { get; set; }
    public int InterstitialFrequency { get; set; } // Every N levels
    public float RewardedAdProminence { get; set; } // 0-1 scale
    public bool BannerAdAlwaysVisible { get; set; }
    public float ExpectedARPPU { get; set; } // Expected revenue per paying user
    public float ExpectedRetentionDrop { get; set; } // Expected retention impact
    public string Description { get; set; }
}

/// <summary>
/// Ad types
/// </summary>
public enum AdType
{
    Interstitial,
    Rewarded,
    Banner
}