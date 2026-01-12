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
    [Signal] public delegate void BannerInsetChangedEventHandler(int insetPx);

    public enum BannerPlacement
    {
        Bottom = 0,
        Top = 1
    }

    /// <summary>
    /// AdMob app ID used for initialization.
    /// If <see cref="Initialize"/> is called with an empty string, this value will be used.
    /// </summary>
    [Export] public string AdMobAppId { get; set; } = "";

    /// <summary>
    /// Platform-specific AdMob app ID overrides.
    /// If set, these take precedence over <see cref="AdMobAppId"/> when running on that platform.
    /// </summary>
    [Export] public string AndroidAdMobAppId { get; set; } = "";

    [Export] public string IosAdMobAppId { get; set; } = "";

    /// <summary>
    /// Banner ad unit ID.
    /// </summary>
    [Export] public string BannerAdUnitId { get; set; } = "ca-app-pub-6675121744131727/8033303534";

    /// <summary>
    /// Interstitial ad unit ID.
    /// </summary>
    [Export] public string InterstitialAdUnitId { get; set; } = "ca-app-pub-6675121744131727/8410569879";

    /// <summary>
    /// Rewarded video ad unit ID.
    /// </summary>
    [Export] public string RewardedAdUnitId { get; set; } = "";

    /// <summary>
    /// Where the banner should be anchored.
    /// </summary>
    [Export] public BannerPlacement BannerPosition { get; set; } = BannerPlacement.Bottom;

    /// <summary>
    /// If enabled, the banner is shown once at startup and stays visible for the entire app session
    /// (unless ads are disabled via IAP).
    /// </summary>
    [Export] public bool PersistentBannerEnabled { get; set; } = true;

    /// <summary>
    /// If enabled, the manager will attempt to refresh/reload the banner at a fixed interval.
    /// Some SDK/plugin combinations auto-refresh; this is an additional safety net.
    /// </summary>
    [Export] public bool EnableBannerAutoRefresh { get; set; } = true;

    /// <summary>
    /// Banner refresh interval in seconds.
    /// </summary>
    [Export] public int BannerRefreshSeconds { get; set; } = 30;

    /// <summary>
    /// Expected banner height in pixels for safe-area/UI adjustments.
    /// Standard banners are 320x50 on phones.
    /// </summary>
    [Export] public int BannerHeightPx { get; set; } = 50;

    /// <summary>
    /// On non-mobile platforms, show a small placeholder bar to simulate the banner.
    /// </summary>
    [Export] public bool ShowEditorPlaceholderBanner { get; set; } = true;

    private GodotObject? _adPlugin;
    private bool _initialized;

    /// <summary>
    /// Minimum time between interstitial ads in seconds.
    /// Prevents ad spam and improves user experience.
    /// </summary>
    [Export] public float InterstitialCooldownSeconds { get; set; } = 45.0f;

    /// <summary>
    /// Whether to automatically preload the next interstitial ad after showing one.
    /// </summary>
    [Export] public bool EnableInterstitialPreloading { get; set; } = true;

    private bool _bannerVisible;
    private bool _interstitialReady;
    private bool _rewardedReady;

    private Timer? _bannerRefreshTimer;
    private Timer? _interstitialCooldownTimer;

    private CanvasLayer? _placeholderLayer;
    private Control? _placeholderBanner;

    private int _lastBannerInset;
    private DateTime _lastInterstitialShownTime;

    public int CurrentBannerInsetPx => _bannerVisible ? BannerHeightPx : 0;

    public override void _Ready()
    {
        Instance = this;
        ProcessMode = ProcessModeEnum.Always;

        ApplyBannerSettingsFromProjectSettings();
        SetupPlaceholderBannerIfNeeded();

        CallDeferred(nameof(EnsurePersistentBannerIfPossible));
    }

    public override void _ExitTree()
    {
        StopBannerRefreshTimer();
        StopInterstitialCooldownTimer();
        DestroyBanner();
    }

    /// <summary>
    /// Initializes the underlying AdMob plugin (if available) with the provided IDs.
    /// This is safe to call multiple times.
    /// </summary>
    /// <param name="adMobAppId">AdMob app ID (optional if provided via platform configs/manifest).</param>
    /// <param name="bannerAdUnitId">Banner ad unit ID.</param>
    /// <param name="interstitialAdUnitId">Interstitial ad unit ID.</param>
    /// <param name="rewardedAdUnitId">Rewarded ad unit ID.</param>
    public void Initialize(string adMobAppId, string bannerAdUnitId, string interstitialAdUnitId, string rewardedAdUnitId)
    {
        var resolvedAppId = ResolveAdMobAppId(adMobAppId);

        AdMobAppId = string.IsNullOrWhiteSpace(resolvedAppId) ? AdMobAppId : resolvedAppId.Trim();
        BannerAdUnitId = string.IsNullOrWhiteSpace(bannerAdUnitId) ? BannerAdUnitId : bannerAdUnitId.Trim();
        InterstitialAdUnitId = string.IsNullOrWhiteSpace(interstitialAdUnitId) ? InterstitialAdUnitId : interstitialAdUnitId.Trim();
        RewardedAdUnitId = string.IsNullOrWhiteSpace(rewardedAdUnitId) ? RewardedAdUnitId : rewardedAdUnitId.Trim();

        ApplyBannerSettingsFromProjectSettings();

        if (!IsPlatformSupported())
        {
            _initialized = false;
            _adPlugin = null;
            UpdatePlaceholderVisibility();
            return;
        }

        _adPlugin = FindAdPluginSingleton();
        if (_adPlugin == null)
        {
            GD.PushWarning("AdsManager: No AdMob plugin singleton found. Ads are disabled.");
            _initialized = false;
            UpdatePlaceholderVisibility();
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
                TryCallPlugin("setAppId", AdMobAppId);
            }
            else
            {
                // Some plugins/exports rely on manifest/Info.plist app IDs.
                TryCallPlugin("initialize");
                TryCallPlugin("init");
            }

            _initialized = true;

            _ = LoadAdsAsync();
            CallDeferred(nameof(EnsurePersistentBannerIfPossible));
        }
        catch (Exception ex)
        {
            GD.PushWarning($"AdsManager: initialization failed: {ex.Message}");
            _initialized = false;
            _adPlugin = null;
            UpdatePlaceholderVisibility();
        }
    }

    /// <summary>
    /// Shows a banner ad.
    /// Does nothing when ads are unavailable.
    /// </summary>
    public void ShowBannerAd()
    {
        if (!IsReadyForShowingAds() || !ShouldShowAds())
            return;

        if (_bannerVisible)
            return;

        try
        {
            ConfigureBannerPositionAndSize();

            bool shown;
            if (!string.IsNullOrWhiteSpace(BannerAdUnitId))
            {
                shown =
                    TryCallPlugin("show_banner", BannerAdUnitId) ||
                    TryCallPlugin("showBanner", BannerAdUnitId) ||
                    TryCallPlugin("show_banner_ad", BannerAdUnitId) ||
                    TryCallPlugin("show_banner_ad_unit", BannerAdUnitId);
            }
            else
            {
                shown =
                    TryCallPlugin("show_banner") ||
                    TryCallPlugin("showBanner") ||
                    TryCallPlugin("show_banner_ad");
            }

            _bannerVisible = shown;
            UpdateBannerInset();

            if (_bannerVisible)
                StartBannerRefreshTimerIfNeeded();
        }
        catch (Exception ex)
        {
            GD.PushWarning($"AdsManager: ShowBannerAd failed: {ex.Message}");
            _bannerVisible = false;
            UpdateBannerInset();
        }
    }

    /// <summary>
    /// Hides the banner ad.
    /// </summary>
    public void HideBannerAd()
    {
        _bannerVisible = false;
        StopBannerRefreshTimer();
        UpdateBannerInset();

        if (!IsPlatformSupported())
        {
            UpdatePlaceholderVisibility();
            return;
        }

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
    /// Pauses/resumes manual banner refresh. (Optional optimization for pause menus.)
    /// </summary>
    public void SetBannerRefreshPaused(bool paused)
    {
        if (_bannerRefreshTimer == null)
            return;

        _bannerRefreshTimer.Paused = paused;
    }

    /// <summary>
    /// Shows an interstitial ad. If no ad is available, emits <see cref="SignalName.AdClosed"/> on the next frame.
    /// </summary>
    public async Task ShowInterstitialAd()
    {
        if (!IsReadyForShowingAds() || !ShouldShowAds())
        {
            await EmitAdClosedNextFrameAsync();
            return;
        }

        // Check cooldown
        if (!CanShowInterstitial())
        {
            GD.Print($"Interstitial ad skipped - cooldown active ({GetRemainingCooldownSeconds():F1}s remaining)");
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
            bool shown =
                TryCallPlugin("show_interstitial") ||
                TryCallPlugin("showInterstitial") ||
                TryCallPlugin("show_interstitial_ad");

            if (!shown)
            {
                await EmitAdClosedNextFrameAsync();
                return;
            }

            // Record that we showed an interstitial
            _lastInterstitialShownTime = DateTime.Now;
            StartInterstitialCooldownTimer();

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
            if (EnableInterstitialPreloading)
            {
                _ = LoadInterstitialAsync();
            }
        }
    }

    /// <summary>
    /// Manually load an interstitial ad in the background.
    /// </summary>
    public async Task LoadInterstitialAd()
    {
        await LoadInterstitialAsync();
    }

    /// <summary>
    /// Check if an interstitial ad is currently loaded and ready to show.
    /// </summary>
    public bool IsInterstitialReady()
    {
        return _interstitialReady && CanShowInterstitial();
    }

    /// <summary>
    /// Force reset the interstitial cooldown timer. Use with caution - only for testing.
    /// </summary>
    public void ResetInterstitialCooldown()
    {
        _lastInterstitialShownTime = DateTime.MinValue;
        if (_interstitialCooldownTimer != null)
        {
            _interstitialCooldownTimer.Stop();
            _interstitialCooldownTimer.QueueFree();
            _interstitialCooldownTimer = null;
        }
    }

    /// <summary>
    /// Get remaining cooldown time in seconds for interstitial ads.
    /// </summary>
    public float GetRemainingCooldownSeconds()
    {
        var elapsed = (DateTime.Now - _lastInterstitialShownTime).TotalSeconds;
        return Mathf.Max(0, InterstitialCooldownSeconds - (float)elapsed);
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
    /// Returns whether ads should be shown based on premium status.
    /// Ads are hidden if user has purchased either full game unlock or remove ads.
    /// </summary>
    public bool ShouldShowAds()
    {
        // Don't show ads if user is premium (full game unlocked OR remove ads purchased)
        return !(MonetizationManager.Instance?.IsFullGameUnlocked ?? false) && 
               !(PremiumManager.Instance?.IsAdFreeVersion ?? false);
    }

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

    private void EnsurePersistentBannerIfPossible()
    {
        if (!PersistentBannerEnabled)
        {
            HideBannerAd();
            return;
        }

        // Check if user is premium (either full game unlocked or remove ads purchased)
        if (!ShouldShowAds())
        {
            HideBannerAd();
            return;
        }

        UpdatePlaceholderVisibility();

        if (!IsReadyForShowingAds())
            return;

        _ = EnsureBannerLoadedAndShownAsync();
    }

    private async Task EnsureBannerLoadedAndShownAsync()
    {
        try
        {
            await LoadBannerAsync();
            ShowBannerAd();
        }
        catch (Exception ex)
        {
            GD.PushWarning($"AdsManager: EnsureBannerLoadedAndShownAsync failed: {ex.Message}");
        }
    }

    private async Task LoadAdsAsync()
    {
        await LoadBannerAsync();
        await LoadInterstitialAsync();
        await LoadRewardedAsync();
    }

    private bool CanShowInterstitial()
    {
        // Check if enough time has passed since last interstitial
        var elapsed = (DateTime.Now - _lastInterstitialShownTime).TotalSeconds;
        return elapsed >= InterstitialCooldownSeconds;
    }

    private void StartInterstitialCooldownTimer()
    {
        if (_interstitialCooldownTimer != null)
        {
            _interstitialCooldownTimer.Stop();
            _interstitialCooldownTimer.QueueFree();
        }

        _interstitialCooldownTimer = new Timer
        {
            OneShot = true,
            WaitTime = InterstitialCooldownSeconds,
            ProcessCallback = Timer.TimerProcessCallback.Idle
        };

        _interstitialCooldownTimer.Timeout += OnInterstitialCooldownTimerTimeout;
        AddChild(_interstitialCooldownTimer);
        _interstitialCooldownTimer.Start();
    }

    private void OnInterstitialCooldownTimerTimeout()
    {
        if (_interstitialCooldownTimer != null)
        {
            _interstitialCooldownTimer.Timeout -= OnInterstitialCooldownTimerTimeout;
            _interstitialCooldownTimer.QueueFree();
            _interstitialCooldownTimer = null;
        }

        GD.Print("Interstitial cooldown expired - ads can be shown again");
    }

    private void StopInterstitialCooldownTimer()
    {
        if (_interstitialCooldownTimer != null)
        {
            _interstitialCooldownTimer.Timeout -= OnInterstitialCooldownTimerTimeout;
            _interstitialCooldownTimer.Stop();
            _interstitialCooldownTimer.QueueFree();
            _interstitialCooldownTimer = null;
        }
    }

    private async Task LoadBannerAsync()
    {
        if (!IsReadyForShowingAds())
            return;

        try
        {
            ConfigureBannerPositionAndSize();

            bool called;
            var positionString = BannerPosition == BannerPlacement.Top ? "top" : "bottom";
            var positionInt = BannerPosition == BannerPlacement.Top ? 1 : 0;

            if (!string.IsNullOrWhiteSpace(BannerAdUnitId))
            {
                called =
                    TryCallPlugin("load_banner", BannerAdUnitId, "BANNER", positionString) ||
                    TryCallPlugin("load_banner", BannerAdUnitId, "BANNER", positionInt) ||
                    TryCallPlugin("load_banner", BannerAdUnitId, positionString) ||
                    TryCallPlugin("load_banner", BannerAdUnitId) ||
                    TryCallPlugin("loadBanner", BannerAdUnitId, "BANNER", positionString) ||
                    TryCallPlugin("loadBanner", BannerAdUnitId, "BANNER", positionInt) ||
                    TryCallPlugin("loadBanner", BannerAdUnitId, positionString) ||
                    TryCallPlugin("loadBanner", BannerAdUnitId) ||
                    TryCallPlugin("create_banner", BannerAdUnitId, "BANNER", positionString) ||
                    TryCallPlugin("createBanner", BannerAdUnitId, "BANNER", positionString);
            }
            else
            {
                called =
                    TryCallPlugin("load_banner") ||
                    TryCallPlugin("loadBanner") ||
                    TryCallPlugin("create_banner") ||
                    TryCallPlugin("createBanner");
            }

            if (!called)
                return;

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

    private void ConfigureBannerPositionAndSize()
    {
        if (_adPlugin == null)
            return;

        var positionString = BannerPosition == BannerPlacement.Top ? "top" : "bottom";
        var positionInt = BannerPosition == BannerPlacement.Top ? 1 : 0;

        // Position
        TryCallPlugin("set_banner_position", positionString);
        TryCallPlugin("setBannerPosition", positionString);
        TryCallPlugin("set_banner_position", positionInt);
        TryCallPlugin("setBannerPosition", positionInt);
        TryCallPlugin("set_banner_anchor", positionString);
        TryCallPlugin("setBannerAnchor", positionString);

        // Size
        TryCallPlugin("set_banner_size", "BANNER");
        TryCallPlugin("setBannerSize", "BANNER");
        TryCallPlugin("set_banner_size", 320, 50);
        TryCallPlugin("setBannerSize", 320, 50);
    }

    private string ResolveAdMobAppId(string fromInitializeCall)
    {
        if (!string.IsNullOrWhiteSpace(fromInitializeCall))
            return fromInitializeCall;

        var os = OS.GetName();
        var platformOverride = os == "Android" ? AndroidAdMobAppId : IosAdMobAppId;
        if (!string.IsNullOrWhiteSpace(platformOverride))
            return platformOverride;

        // Optional project settings overrides.
        // These keys are not required but allow build-time configuration without touching scenes/scripts.
        var settingKey = os == "Android" ? "monetization/admob/app_id_android" : "monetization/admob/app_id_ios";
        if (ProjectSettings.HasSetting(settingKey))
        {
            var value = ProjectSettings.GetSetting(settingKey).AsString();
            if (!string.IsNullOrWhiteSpace(value))
                return value;
        }

        if (ProjectSettings.HasSetting("monetization/admob/app_id"))
        {
            var value = ProjectSettings.GetSetting("monetization/admob/app_id").AsString();
            if (!string.IsNullOrWhiteSpace(value))
                return value;
        }

        return AdMobAppId;
    }

    private void ApplyBannerSettingsFromProjectSettings()
    {
        if (ProjectSettings.HasSetting("monetization/admob/persistent_banner"))
            PersistentBannerEnabled = ProjectSettings.GetSetting("monetization/admob/persistent_banner").AsBool();

        if (ProjectSettings.HasSetting("monetization/admob/banner_auto_refresh"))
            EnableBannerAutoRefresh = ProjectSettings.GetSetting("monetization/admob/banner_auto_refresh").AsBool();

        if (ProjectSettings.HasSetting("monetization/admob/banner_refresh_seconds"))
        {
            var seconds = (int)ProjectSettings.GetSetting("monetization/admob/banner_refresh_seconds").AsInt32();
            if (seconds > 0)
                BannerRefreshSeconds = seconds;
        }

        if (ProjectSettings.HasSetting("monetization/admob/banner_height_px"))
        {
            var height = (int)ProjectSettings.GetSetting("monetization/admob/banner_height_px").AsInt32();
            if (height > 0)
                BannerHeightPx = height;
        }

        if (ProjectSettings.HasSetting("monetization/admob/banner_position"))
        {
            var pos = ProjectSettings.GetSetting("monetization/admob/banner_position").AsString().Trim().ToLowerInvariant();
            if (pos == "top")
                BannerPosition = BannerPlacement.Top;
            else if (pos == "bottom")
                BannerPosition = BannerPlacement.Bottom;
        }
    }

    private void StartBannerRefreshTimerIfNeeded()
    {
        if (!EnableBannerAutoRefresh)
            return;

        if (BannerRefreshSeconds <= 0)
            return;

        if (_bannerRefreshTimer != null)
            return;

        _bannerRefreshTimer = new Timer
        {
            OneShot = false,
            WaitTime = BannerRefreshSeconds,
            ProcessCallback = Timer.TimerProcessCallback.Idle
        };

        _bannerRefreshTimer.Timeout += OnBannerRefreshTimerTimeout;
        AddChild(_bannerRefreshTimer);
        _bannerRefreshTimer.Start();
    }

    private void StopBannerRefreshTimer()
    {
        if (_bannerRefreshTimer == null)
            return;

        _bannerRefreshTimer.Timeout -= OnBannerRefreshTimerTimeout;
        _bannerRefreshTimer.QueueFree();
        _bannerRefreshTimer = null;
    }

    private void OnBannerRefreshTimerTimeout()
    {
        if (!_bannerVisible)
            return;

        if (MonetizationManager.Instance?.ShowAds == false)
        {
            HideBannerAd();
            return;
        }

        if (!IsReadyForShowingAds())
            return;

        _ = RefreshBannerAsync();
    }

    private async Task RefreshBannerAsync()
    {
        try
        {
            // Reloading the banner is plugin-dependent.
            // We try a few common method names, falling back to re-loading.
            bool called =
                TryCallPlugin("refresh_banner") ||
                TryCallPlugin("refreshBanner") ||
                TryCallPlugin("reload_banner") ||
                TryCallPlugin("reloadBanner");

            if (!called)
                await LoadBannerAsync();

            // Some plugins require explicitly showing after reload.
            if (_bannerVisible)
            {
                TryCallPlugin("show_banner");
                TryCallPlugin("showBanner");
            }
        }
        catch (Exception ex)
        {
            GD.PushWarning($"AdsManager: banner refresh failed: {ex.Message}");
        }
    }

    private void DestroyBanner()
    {
        if (!IsPlatformSupported())
            return;

        try
        {
            TryCallPlugin("destroy_banner");
            TryCallPlugin("destroyBanner");
            TryCallPlugin("remove_banner");
            TryCallPlugin("removeBanner");
        }
        catch (Exception ex)
        {
            GD.PushWarning($"AdsManager: DestroyBanner failed: {ex.Message}");
        }
    }

    private void SetupPlaceholderBannerIfNeeded()
    {
        if (IsPlatformSupported())
            return;

        if (!ShowEditorPlaceholderBanner)
            return;

        _placeholderLayer = new CanvasLayer { Name = "AdBannerPlaceholderLayer", Layer = 1000 };

        _placeholderBanner = new PanelContainer
        {
            Name = "AdBannerPlaceholder",
            MouseFilter = Control.MouseFilterEnum.Ignore,
            AnchorsPreset = LayoutPreset.BottomWide,
            AnchorLeft = 0,
            AnchorRight = 1,
            AnchorTop = 1,
            AnchorBottom = 1,
            OffsetTop = -BannerHeightPx,
            OffsetBottom = 0
        };

        var label = new Label
        {
            Text = "Banner Ad (placeholder)",
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
        _placeholderBanner.AddChild(label);
        _placeholderLayer.AddChild(_placeholderBanner);
        AddChild(_placeholderLayer);

        UpdatePlaceholderVisibility();
    }

    private void UpdatePlaceholderVisibility()
    {
        if (_placeholderBanner == null)
            return;

        var show = !IsPlatformSupported() && ShowEditorPlaceholderBanner && (MonetizationManager.Instance?.ShowAds != false) && PersistentBannerEnabled;
        _placeholderBanner.Visible = show;

        _bannerVisible = show;
        UpdateBannerInset();
    }

    private void UpdateBannerInset()
    {
        var inset = CurrentBannerInsetPx;
        if (inset == _lastBannerInset)
            return;

        _lastBannerInset = inset;
        EmitSignal(SignalName.BannerInsetChanged, inset);
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
