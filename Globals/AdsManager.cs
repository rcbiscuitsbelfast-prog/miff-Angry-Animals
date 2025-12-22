using System;
using System.Threading.Tasks;
using Godot;

/// <summary>
/// Global ads manager responsible for initializing and showing ads.
/// Designed to integrate with AdMob via platform-specific Godot plugins.
/// On unsupported platforms (Windows/macOS/Linux/Web) this manager becomes a no-op.
/// </summary>
public partial class AdsManager : Node
{
    public static AdsManager Instance { get; private set; } = null!;

    [Signal] public delegate void AdClosedEventHandler();
    [Signal] public delegate void AdClickedEventHandler();
    [Signal] public delegate void RewardEarnedEventHandler();

    /// <summary>
    /// AdMob app ID used for initialization.
    /// If <see cref="Initialize"/> is called with an empty string, this value will be used.
    /// </summary>
    [Export] public string AdMobAppId { get; set; } = "";

    /// <summary>
    /// Banner ad unit ID.
    /// </summary>
    [Export] public string BannerAdUnitId { get; set; } = "";

    /// <summary>
    /// Interstitial ad unit ID.
    /// </summary>
    [Export] public string InterstitialAdUnitId { get; set; } = "";

    /// <summary>
    /// Rewarded video ad unit ID.
    /// </summary>
    [Export] public string RewardedAdUnitId { get; set; } = "";

    private GodotObject? _adPlugin;
    private bool _initialized;

    private bool _bannerVisible;
    private bool _interstitialReady;
    private bool _rewardedReady;

    public override void _Ready()
    {
        Instance = this;
        ProcessMode = ProcessModeEnum.Always;
    }

    /// <summary>
    /// Initializes the underlying AdMob plugin (if available) with the provided IDs.
    /// This is safe to call multiple times.
    /// </summary>
    /// <param name="adMobAppId">AdMob app ID.</param>
    /// <param name="bannerAdUnitId">Banner ad unit ID.</param>
    /// <param name="interstitialAdUnitId">Interstitial ad unit ID.</param>
    /// <param name="rewardedAdUnitId">Rewarded ad unit ID.</param>
    public void Initialize(string adMobAppId, string bannerAdUnitId, string interstitialAdUnitId, string rewardedAdUnitId)
    {
        AdMobAppId = string.IsNullOrWhiteSpace(adMobAppId) ? AdMobAppId : adMobAppId.Trim();
        BannerAdUnitId = string.IsNullOrWhiteSpace(bannerAdUnitId) ? BannerAdUnitId : bannerAdUnitId.Trim();
        InterstitialAdUnitId = string.IsNullOrWhiteSpace(interstitialAdUnitId) ? InterstitialAdUnitId : interstitialAdUnitId.Trim();
        RewardedAdUnitId = string.IsNullOrWhiteSpace(rewardedAdUnitId) ? RewardedAdUnitId : rewardedAdUnitId.Trim();

        if (!IsPlatformSupported())
        {
            _initialized = false;
            _adPlugin = null;
            return;
        }

        _adPlugin = FindAdPluginSingleton();
        if (_adPlugin == null)
        {
            GD.PushWarning("AdsManager: No AdMob plugin singleton found. Ads are disabled.");
            _initialized = false;
            return;
        }

        try
        {
            // Different plugins use different method names. We attempt a few common ones.
            if (!string.IsNullOrWhiteSpace(AdMobAppId))
            {
                TryCallPlugin("initialize", AdMobAppId);
                TryCallPlugin("init", AdMobAppId);
                TryCallPlugin("set_app_id", AdMobAppId);
            }

            _initialized = true;

            _ = LoadAdsAsync();
        }
        catch (Exception ex)
        {
            GD.PushWarning($"AdsManager: initialization failed: {ex.Message}");
            _initialized = false;
            _adPlugin = null;
        }
    }

    /// <summary>
    /// Shows a banner ad at the bottom of the screen.
    /// Does nothing when ads are unavailable.
    /// </summary>
    public void ShowBannerAd()
    {
        if (!IsReadyForShowingAds())
            return;

        if (_bannerVisible)
            return;

        _bannerVisible = true;

        try
        {
            if (!string.IsNullOrWhiteSpace(BannerAdUnitId))
            {
                TryCallPlugin("show_banner", BannerAdUnitId);
                TryCallPlugin("showBanner", BannerAdUnitId);
                TryCallPlugin("show_banner_ad", BannerAdUnitId);
            }
            else
            {
                TryCallPlugin("show_banner");
                TryCallPlugin("showBanner");
                TryCallPlugin("show_banner_ad");
            }
        }
        catch (Exception ex)
        {
            GD.PushWarning($"AdsManager: ShowBannerAd failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Hides the banner ad.
    /// </summary>
    public void HideBannerAd()
    {
        _bannerVisible = false;

        if (!IsPlatformSupported())
            return;

        try
        {
            TryCallPlugin("hide_banner");
            TryCallPlugin("hideBanner");
            TryCallPlugin("hide_banner_ad");
        }
        catch (Exception ex)
        {
            GD.PushWarning($"AdsManager: HideBannerAd failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Shows an interstitial ad. If no ad is available, emits <see cref="SignalName.AdClosed"/> on the next frame.
    /// </summary>
    public async Task ShowInterstitialAd()
    {
        if (!IsReadyForShowingAds())
        {
            await EmitAdClosedNextFrameAsync();
            return;
        }

        if (!_interstitialReady)
            await LoadInterstitialAsync();

        if (!_interstitialReady)
        {
            await EmitAdClosedNextFrameAsync();
            return;
        }

        try
        {
            // Attempt to show. If plugin supports callbacks it should call back into NotifyAdClosed/NotifyAdClicked.
            bool shown =
                TryCallPlugin("show_interstitial") ||
                TryCallPlugin("showInterstitial") ||
                TryCallPlugin("show_interstitial_ad");

            if (!shown)
            {
                await EmitAdClosedNextFrameAsync();
                return;
            }

            await WaitForAdClosedOrTimeoutAsync(10.0);
        }
        catch (Exception ex)
        {
            GD.PushWarning($"AdsManager: ShowInterstitialAd failed: {ex.Message}");
            await EmitAdClosedNextFrameAsync();
        }
        finally
        {
            _interstitialReady = false;
            _ = LoadInterstitialAsync();
        }
    }

    /// <summary>
    /// Shows a rewarded video ad. Emits <see cref="SignalName.RewardEarned"/> when a reward is granted by the ad network.
    /// If rewarded ads are unavailable, the call completes without throwing.
    /// </summary>
    public async Task ShowRewardedAd()
    {
        if (!IsReadyForShowingAds())
        {
            await EmitAdClosedNextFrameAsync();
            return;
        }

        if (!_rewardedReady)
            await LoadRewardedAsync();

        if (!_rewardedReady)
        {
            await EmitAdClosedNextFrameAsync();
            return;
        }

        try
        {
            bool shown =
                TryCallPlugin("show_rewarded") ||
                TryCallPlugin("showRewarded") ||
                TryCallPlugin("show_rewarded_ad");

            if (!shown)
            {
                await EmitAdClosedNextFrameAsync();
                return;
            }

            await WaitForAdClosedOrTimeoutAsync(15.0);
        }
        catch (Exception ex)
        {
            GD.PushWarning($"AdsManager: ShowRewardedAd failed: {ex.Message}");
            await EmitAdClosedNextFrameAsync();
        }
        finally
        {
            _rewardedReady = false;
            _ = LoadRewardedAsync();
        }
    }

    /// <summary>
    /// Returns whether any full-screen ad is currently ready (interstitial or rewarded).
    /// </summary>
    public bool IsAdReady() => _interstitialReady || _rewardedReady;

    /// <summary>
    /// Callback hook for plugins to notify that an ad was closed.
    /// This method is safe to call from platform code via <c>Callable</c>/<c>Call</c>.
    /// </summary>
    public void NotifyAdClosed()
    {
        EmitSignal(SignalName.AdClosed);
    }

    /// <summary>
    /// Callback hook for plugins to notify that an ad was clicked.
    /// </summary>
    public void NotifyAdClicked()
    {
        EmitSignal(SignalName.AdClicked);
    }

    /// <summary>
    /// Callback hook for plugins to notify that a reward has been earned.
    /// </summary>
    public void NotifyRewardEarned()
    {
        EmitSignal(SignalName.RewardEarned);
    }

    private async Task LoadAdsAsync()
    {
        await LoadBannerAsync();
        await LoadInterstitialAsync();
        await LoadRewardedAsync();
    }

    private async Task LoadBannerAsync()
    {
        if (!IsReadyForShowingAds())
            return;

        try
        {
            if (!string.IsNullOrWhiteSpace(BannerAdUnitId))
            {
                TryCallPlugin("load_banner", BannerAdUnitId);
                TryCallPlugin("loadBanner", BannerAdUnitId);
            }
            else
            {
                TryCallPlugin("load_banner");
                TryCallPlugin("loadBanner");
            }

            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        }
        catch (Exception ex)
        {
            GD.PushWarning($"AdsManager: banner load failed: {ex.Message}");
        }
    }

    private async Task LoadInterstitialAsync()
    {
        _interstitialReady = false;

        if (!IsReadyForShowingAds())
            return;

        try
        {
            bool called;
            if (!string.IsNullOrWhiteSpace(InterstitialAdUnitId))
            {
                called =
                    TryCallPlugin("load_interstitial", InterstitialAdUnitId) ||
                    TryCallPlugin("loadInterstitial", InterstitialAdUnitId) ||
                    TryCallPlugin("load_interstitial_ad", InterstitialAdUnitId);
            }
            else
            {
                called =
                    TryCallPlugin("load_interstitial") ||
                    TryCallPlugin("loadInterstitial") ||
                    TryCallPlugin("load_interstitial_ad");
            }

            if (!called)
                return;

            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
            _interstitialReady = true;
        }
        catch (Exception ex)
        {
            GD.PushWarning($"AdsManager: interstitial load failed: {ex.Message}");
            _interstitialReady = false;
        }
    }

    private async Task LoadRewardedAsync()
    {
        _rewardedReady = false;

        if (!IsReadyForShowingAds())
            return;

        try
        {
            bool called;
            if (!string.IsNullOrWhiteSpace(RewardedAdUnitId))
            {
                called =
                    TryCallPlugin("load_rewarded", RewardedAdUnitId) ||
                    TryCallPlugin("loadRewarded", RewardedAdUnitId) ||
                    TryCallPlugin("load_rewarded_ad", RewardedAdUnitId);
            }
            else
            {
                called =
                    TryCallPlugin("load_rewarded") ||
                    TryCallPlugin("loadRewarded") ||
                    TryCallPlugin("load_rewarded_ad");
            }

            if (!called)
                return;

            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
            _rewardedReady = true;
        }
        catch (Exception ex)
        {
            GD.PushWarning($"AdsManager: rewarded load failed: {ex.Message}");
            _rewardedReady = false;
        }
    }

    private static bool IsPlatformSupported()
    {
        var os = OS.GetName();
        return os == "Android" || os == "iOS";
    }

    private bool IsReadyForShowingAds() => IsPlatformSupported() && _initialized && _adPlugin != null;

    private static GodotObject? FindAdPluginSingleton()
    {
        // The singleton name depends on the specific AdMob plugin.
        // We try a few common ones.
        string[] candidates =
        [
            "AdMob",
            "Admob",
            "GodotAdMob",
            "AdMobPlugin",
            "AdMobSingleton",
            "AdmobSingleton"
        ];

        foreach (var name in candidates)
        {
            if (Engine.HasSingleton(name))
                return Engine.GetSingleton(name);
        }

        return null;
    }

    private bool TryCallPlugin(string method, params Variant[] args)
    {
        if (_adPlugin == null)
            return false;

        if (!_adPlugin.HasMethod(method))
            return false;

        _adPlugin.Call(method, args);
        return true;
    }

    private async Task EmitAdClosedNextFrameAsync()
    {
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        EmitSignal(SignalName.AdClosed);
    }

    private async Task WaitForAdClosedOrTimeoutAsync(double timeoutSeconds)
    {
        var timeoutTimer = new Timer
        {
            OneShot = true,
            WaitTime = timeoutSeconds,
            ProcessCallback = Timer.TimerProcessCallback.Idle
        };

        AddChild(timeoutTimer);
        timeoutTimer.Start();

        var closedTask = ToSignal(this, SignalName.AdClosed);
        var timeoutTask = ToSignal(timeoutTimer, Timer.SignalName.Timeout);

        await Task.WhenAny(closedTask, timeoutTask);

        timeoutTimer.QueueFree();

        // If the ad system never emitted, still emit AdClosed to unblock flow.
        if (!closedTask.IsCompleted)
            EmitSignal(SignalName.AdClosed);
    }
}
