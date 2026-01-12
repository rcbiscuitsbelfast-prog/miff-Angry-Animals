using System;
using System.Threading.Tasks;
using Godot;

/// <summary>
/// Global premium manager responsible for "Remove Ads" in-app purchases.
/// Unified cross-platform IAP management with platform detection and fallback handling.
/// Supports Google Play Billing (Android), StoreKit 2 (iOS), and Amazon IAP.
/// </summary>
public partial class PremiumManager : Node
{
    public static PremiumManager Instance { get; private set; } = null!;

    [Signal] public delegate void RemoveAdsPurchaseSucceededEventHandler();
    [Signal] public delegate void RemoveAdsPurchaseFailedEventHandler(string reason);
    [Signal] public delegate void RemoveAdsPurchaseRestoredEventHandler();

    /// <summary>
    /// Product ID for removing ads across all platforms.
    /// Configure this in respective app stores.
    /// </summary>
    [Export] public string RemoveAdsProductId { get; set; } = "remove_ads";

    /// <summary>
    /// Price display for the remove ads purchase (configurable per region).
    /// </summary>
    [Export] public string RemoveAdsPrice { get; set; } = "$0.99";

    /// <summary>
    /// Returns whether the user has purchased the "Remove Ads" upgrade.
    /// This value is persisted in local storage via PurchaseStateManager.
    /// </summary>
    public bool IsAdFreeVersion => PurchaseStateManager.Instance?.IsRemoveAdsPurchased ?? false;

    /// <summary>
    /// Returns whether any premium features are available (ad-free version or full game unlocked).
    /// </summary>
    public bool IsPremiumVersion => IsAdFreeVersion || (MonetizationManager.Instance?.IsFullGameUnlocked ?? false);

    private GodotObject? _billingPlugin;
    private bool _initialized;

    public override void _Ready()
    {
        Instance = this;
        ProcessMode = ProcessModeEnum.Always;

        CallDeferred(nameof(DeferredInitializeAndRestore));
    }

    /// <summary>
    /// Initializes the premium billing integration for the current platform.
    /// </summary>
    public void Initialize()
    {
        _billingPlugin = FindBillingPluginSingleton();
        _initialized = _billingPlugin != null && IsPlatformSupported();

        if (!_initialized)
        {
            _billingPlugin = null;
            return;
        }

        try
        {
            // Attempt common init methods.
            TryCallPlugin("initialize");
            TryCallPlugin("init");
            TryCallPlugin("connect");
        }
        catch (Exception ex)
        {
            GD.PushWarning($"PremiumManager: billing initialization failed: {ex.Message}");
            _billingPlugin = null;
            _initialized = false;
        }
    }

    /// <summary>
    /// Triggers the "Remove Ads" purchase flow.
    /// </summary>
    public async Task PurchaseRemoveAds()
    {
        if (IsAdFreeVersion)
        {
            EmitSignal(SignalName.RemoveAdsPurchaseSucceeded);
            return;
        }

        if (!IsPlatformSupported())
        {
            EmitSignal(SignalName.RemoveAdsPurchaseFailed, "In-app purchases are not supported on this platform.");
            return;
        }

        if (!_initialized)
            Initialize();

        if (!_initialized || _billingPlugin == null)
        {
            EmitSignal(SignalName.RemoveAdsPurchaseFailed, "Billing unavailable. Please try again later.");
            return;
        }

        try
        {
            string productId = GetPlatformProductId();
            if (string.IsNullOrWhiteSpace(productId))
            {
                EmitSignal(SignalName.RemoveAdsPurchaseFailed, "Product not configured.");
                return;
            }

            // Ask the plugin to purchase. Plugins vary wildly, so we attempt common method names.
            bool started =
                TryCallPlugin("purchase", productId) ||
                TryCallPlugin("purchase_product", productId) ||
                TryCallPlugin("purchaseProduct", productId) ||
                TryCallPlugin("buy", productId) ||
                TryCallPlugin("buy_product", productId);

            if (!started)
            {
                EmitSignal(SignalName.RemoveAdsPurchaseFailed, "Billing plugin does not support purchasing.");
                return;
            }

            // Without a plugin callback we cannot know the result reliably.
            // We wait briefly for a native callback to invoke NotifyRemoveAdsPurchaseSucceeded/Failed.
            await WaitForPurchaseResultOrTimeoutAsync(20.0);
        }
        catch (Exception ex)
        {
            EmitSignal(SignalName.RemoveAdsPurchaseFailed, $"Purchase failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Restores "Remove Ads" purchases on startup.
    /// </summary>
    public async Task RestoreRemoveAdsPurchases()
    {
        if (!IsPlatformSupported())
        {
            if (IsAdFreeVersion)
                EmitSignal(SignalName.RemoveAdsPurchaseRestored);
            return;
        }

        if (!_initialized)
            Initialize();

        if (!_initialized || _billingPlugin == null)
        {
            if (IsAdFreeVersion)
                EmitSignal(SignalName.RemoveAdsPurchaseRestored);
            return;
        }

        try
        {
            bool started =
                TryCallPlugin("restore") ||
                TryCallPlugin("restore_purchases") ||
                TryCallPlugin("restorePurchases") ||
                TryCallPlugin("query_purchases") ||
                TryCallPlugin("queryPurchases");

            if (!started)
            {
                if (IsAdFreeVersion)
                    EmitSignal(SignalName.RemoveAdsPurchaseRestored);
                return;
            }

            await WaitForRestoreOrTimeoutAsync(10.0);

            if (IsAdFreeVersion)
                EmitSignal(SignalName.RemoveAdsPurchaseRestored);
        }
        catch (Exception ex)
        {
            GD.PushWarning($"PremiumManager: restore failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Callback hook for platform plugins to mark the "Remove Ads" purchase as successful.
    /// </summary>
    public void NotifyRemoveAdsPurchaseSucceeded()
    {
        GrantRemoveAds();
        EmitSignal(SignalName.RemoveAdsPurchaseSucceeded);
    }

    /// <summary>
    /// Callback hook for platform plugins to report purchase failure/cancellation.
    /// </summary>
    /// <param name="reason">Failure reason.</param>
    public void NotifyRemoveAdsPurchaseFailed(string reason)
    {
        EmitSignal(SignalName.RemoveAdsPurchaseFailed, string.IsNullOrWhiteSpace(reason) ? "Purchase failed." : reason);
    }

    /// <summary>
    /// Grants the "Remove Ads" premium status locally and persists the state.
    /// </summary>
    public void GrantRemoveAds()
    {
        if (PurchaseStateManager.Instance == null)
        {
            GD.PushWarning("PremiumManager: PurchaseStateManager not ready; cannot persist premium state.");
            return;
        }

        if (PurchaseStateManager.Instance.IsRemoveAdsPurchased)
            return;

        PurchaseStateManager.Instance.SetRemoveAdsPurchased(true);
        PurchaseStateManager.Instance.SavePurchaseState();

        // Hide ads immediately when premium status is granted
        AdsManager.Instance?.HideBannerAd();
        
        GD.Print("Remove Ads purchase granted - ads disabled");
    }

    /// <summary>
    /// Gets the current platform name for logging and debugging.
    /// </summary>
    public string GetPlatform()
    {
        var os = OS.GetName();
        return os switch
        {
            "Android" => "Android (Google Play)",
            "iOS" => "iOS (App Store)",
            "FreeBSD" => "Amazon Appstore", // Amazon Fire devices often report as FreeBSD
            _ => os
        };
    }

    private void DeferredInitializeAndRestore()
    {
        Initialize();
        _ = RestoreRemoveAdsPurchases();
    }

    private static bool IsPlatformSupported()
    {
        var os = OS.GetName();
        return os == "Android" || os == "iOS" || os == "FreeBSD"; // FreeBSD for Amazon Fire
    }

    private string GetPlatformProductId()
    {
        // Use same product ID across all platforms for consistency
        // Each store will map this to their respective product configuration
        return RemoveAdsProductId;
    }

    private static GodotObject? FindBillingPluginSingleton()
    {
        // These names depend on the specific plugins installed.
        // We try a few common ones to keep this code resilient.
        var os = OS.GetName();

        string[] candidates = os switch
        {
            "iOS" => new[]
            {
                "StoreKit",
                "StoreKit2",
                "InAppPurchase",
                "InAppPurchases",
                "GodotInAppPurchase",
                "PremiumIAP"
            },
            "Android" => new[]
            {
                "GooglePlayBilling",
                "GodotGooglePlayBilling",
                "GodotGooglePlay",
                "InAppPurchase",
                "InAppPurchases",
                "PremiumIAP"
            },
            "FreeBSD" => new[] // Amazon Fire
            {
                "AmazonIAP",
                "AmazonDeviceMessaging",
                "InAppPurchase",
                "InAppPurchases",
                "PremiumIAP"
            },
            _ => Array.Empty<string>()
        };

        foreach (var name in candidates)
        {
            if (Engine.HasSingleton(name))
                return Engine.GetSingleton(name);
        }

        return null;
    }

    private bool TryCallPlugin(string method, params Variant[] args)
    {
        if (_billingPlugin == null)
            return false;

        if (!_billingPlugin.HasMethod(method))
            return false;

        _billingPlugin.Call(method, args);
        return true;
    }

    private async Task WaitForPurchaseResultOrTimeoutAsync(double timeoutSeconds)
    {
        var timer = new Timer
        {
            OneShot = true,
            WaitTime = timeoutSeconds,
            ProcessCallback = Timer.TimerProcessCallback.Idle
        };

        AddChild(timer);
        timer.Start();

        var successTask = ToSignal(this, SignalName.RemoveAdsPurchaseSucceeded);
        var failTask = ToSignal(this, SignalName.RemoveAdsPurchaseFailed);
        var timeoutTask = ToSignal(timer, Timer.SignalName.Timeout);

        await Task.WhenAny(successTask, failTask, timeoutTask);
        timer.QueueFree();

        if (timeoutTask.IsCompleted && !IsAdFreeVersion)
            EmitSignal(SignalName.RemoveAdsPurchaseFailed, "Purchase timed out. Please try again.");
    }

    private async Task WaitForRestoreOrTimeoutAsync(double timeoutSeconds)
    {
        var timer = new Timer
        {
            OneShot = true,
            WaitTime = timeoutSeconds,
            ProcessCallback = Timer.TimerProcessCallback.Idle
        };

        AddChild(timer);
        timer.Start();

        var restoredTask = ToSignal(this, SignalName.RemoveAdsPurchaseRestored);
        var timeoutTask = ToSignal(timer, Timer.SignalName.Timeout);

        await Task.WhenAny(restoredTask, timeoutTask);
        timer.QueueFree();

        if (!restoredTask.IsCompleted)
            EmitSignal(SignalName.RemoveAdsPurchaseRestored);
    }
}