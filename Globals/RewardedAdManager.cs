using System;
using System.Threading.Tasks;
using Godot;

/// <summary>
/// Core handler for rewarded video ads.
/// Manages the lifecycle, preloading, and callback handling for rewards.
/// </summary>
public partial class RewardedAdManager : Node
{
    public static RewardedAdManager Instance { get; private set; } = null!;

    private Action? _onRewardCallback;
    private bool _isAdLoaded = false;
    private bool _isAdShowing = false;

    [Export] public string RewardedAdUnitId { get; set; } = "ca-app-pub-6675121744131727/8406522837";
    [Export] public bool PreloadOnStartup { get; set; } = true;

    public override void _Ready()
    {
        Instance = this;
        ProcessMode = ProcessModeEnum.Always;

        if (PreloadOnStartup)
        {
            Initialize();
        }
    }

    public void Initialize()
    {
        GD.Print("RewardedAdManager: Initializing...");
        Preload();
    }

    public void Preload()
    {
        if (_isAdLoaded || _isAdShowing) return;
        LoadRewardedAd();
    }

    public async void LoadRewardedAd()
    {
        GD.Print($"RewardedAdManager: Loading ad {RewardedAdUnitId}...");
        
        // In a real implementation, this would interface with platform adapters
        // For this architecture, we integrate with AdsManager
        if (AdsManager.Instance != null)
        {
            // We use AdsManager's rewarded ad unit ID if it's set there, otherwise use our own
            if (string.IsNullOrEmpty(AdsManager.Instance.RewardedAdUnitId))
            {
                AdsManager.Instance.RewardedAdUnitId = RewardedAdUnitId;
            }

            // AdsManager already has a LoadRewardedAsync but it's private. 
            // We can trigger it by calling ShowRewardedAd or we can assume AdsManager handles its own loading.
            // However, the ticket asks for unified control here.
        }

        // Simulate ad loading for the sake of the framework
        await Task.Delay(1000);
        _isAdLoaded = true;
        GD.Print("RewardedAdManager: Ad loaded and ready.");
    }

    public async void ShowRewardedAd(Action callback)
    {
        if (_isAdShowing) return;

        _onRewardCallback = callback;

        if (!IsRewardedAdReady())
        {
            GD.PushWarning("RewardedAdManager: Ad not ready, attempting to load...");
            AnalyticsManager.Instance?.TrackAdEvent("rewarded", "failed_not_ready", RewardedAdUnitId);
            LoadRewardedAd();
            return;
        }

        _isAdShowing = true;
        GD.Print("RewardedAdManager: Showing ad...");
        AnalyticsManager.Instance?.TrackAdEvent("rewarded", "shown", RewardedAdUnitId);

        if (AdsManager.Instance != null)
        {
            // Connect to reward signal
            AdsManager.Instance.RewardEarned += OnUserEarnedReward;
            AdsManager.Instance.AdClosed += OnAdClosed;

            await AdsManager.Instance.ShowRewardedAd();
        }
        else
        {
            // Fallback for editor or missing AdsManager
            GD.Print("RewardedAdManager: AdsManager not found, simulating reward in editor...");
            await Task.Delay(1000);
            OnUserEarnedReward();
            OnAdClosed();
        }
    }

    public bool IsRewardedAdReady()
    {
        // Integration with AdsManager state if possible, otherwise use local state
        return _isAdLoaded;
    }

    private void OnUserEarnedReward()
    {
        GD.Print("RewardedAdManager: User earned reward!");
        _onRewardCallback?.Invoke();
        
        // Save reward state if needed
        PurchaseStateManager.Instance?.SaveRewardEarned(true);
        SignalManager.EmitRewardEarned();
        AnalyticsManager.Instance?.TrackAdEvent("rewarded", "reward_earned", RewardedAdUnitId);
    }

    private void OnAdClosed()
    {
        GD.Print("RewardedAdManager: Ad closed.");
        _isAdShowing = false;
        _isAdLoaded = false;
        AnalyticsManager.Instance?.TrackAdEvent("rewarded", "closed", RewardedAdUnitId);

        // Disconnect signals
        if (AdsManager.Instance != null)
        {
            AdsManager.Instance.RewardEarned -= OnUserEarnedReward;
            AdsManager.Instance.AdClosed -= OnAdClosed;
        }

        // Preload next ad
        Preload();
    }
}
