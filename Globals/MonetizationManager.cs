using System;
using System.Threading.Tasks;
using Godot;

/// <summary>
/// Global monetization manager responsible for in-app purchases and monetization state.
/// Intended to integrate with platform billing plugins (StoreKit2 / Google Play Billing).
/// </summary>
public partial class MonetizationManager : Node
{
    public static MonetizationManager Instance { get; private set; } = null!;

    [Signal] public delegate void PurchaseSucceededEventHandler();
    [Signal] public delegate void PurchaseFailedEventHandler(string reason);
    [Signal] public delegate void PurchaseRestoredEventHandler();

    /// <summary>
    /// Default iOS product ID. You can override this at runtime via <see cref="Initialize"/>.
    /// </summary>
    [Export] public string IosProductId { get; set; } = "full_game_unlock";

    /// <summary>
    /// Default Android product ID. You can override this at runtime via <see cref="Initialize"/>.
    /// </summary>
    [Export] public string AndroidProductId { get; set; } = "full_game_unlock";

    private GodotObject? _billingPlugin;
    private bool _initialized;

    /// <summary>
    /// Returns whether the full game is unlocked.
    /// This value is persisted in <c>user://profile.json</c> via <see cref="PlayerProfile"/>.
    /// </summary>
    public bool IsFullGameUnlocked => PlayerProfile.Instance?.IsFullGameUnlocked ?? false;

    /// <summary>
    /// Returns whether ads should be shown for the current player.
    /// </summary>
    public bool ShowAds => !IsFullGameUnlocked;

    public override void _Ready()
    {
        Instance = this;
        ProcessMode = ProcessModeEnum.Always;

        CallDeferred(nameof(DeferredInitializeAndRestore));
    }

    /// <summary>
    /// Initializes the billing integration.
    /// </summary>
    /// <param name="iosProductId">iOS product ID.</param>
    /// <param name="androidProductId">Android product ID.</param>
    public void Initialize(string iosProductId, string androidProductId)
    {
        IosProductId = string.IsNullOrWhiteSpace(iosProductId) ? IosProductId : iosProductId.Trim();
        AndroidProductId = string.IsNullOrWhiteSpace(androidProductId) ? AndroidProductId : androidProductId.Trim();

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
            GD.PushWarning($"MonetizationManager: billing initialization failed: {ex.Message}");
            _billingPlugin = null;
            _initialized = false;
        }
    }

    /// <summary>
    /// Triggers the full-game purchase flow.
    /// </summary>
    public async Task PurchaseFullGame()
    {
        if (IsFullGameUnlocked)
        {
            EmitSignal(SignalName.PurchaseSucceeded);
            return;
        }

        if (!IsPlatformSupported())
        {
            EmitSignal(SignalName.PurchaseFailed, "In-app purchases are not supported on this platform.");
            return;
        }

        if (!_initialized)
            Initialize(IosProductId, AndroidProductId);

        if (!_initialized || _billingPlugin == null)
        {
            EmitSignal(SignalName.PurchaseFailed, "Billing unavailable. Please try again later.");
            return;
        }

        try
        {
            string productId = GetPlatformProductId();
            if (string.IsNullOrWhiteSpace(productId))
            {
                EmitSignal(SignalName.PurchaseFailed, "Product not configured.");
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
                EmitSignal(SignalName.PurchaseFailed, "Billing plugin does not support purchasing.");
                return;
            }

            // Without a plugin callback we cannot know the result reliably.
            // We wait briefly for a native callback to invoke NotifyPurchaseSucceeded/Failed.
            await WaitForPurchaseResultOrTimeoutAsync(20.0);
        }
        catch (Exception ex)
        {
            EmitSignal(SignalName.PurchaseFailed, $"Purchase failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Restores purchases on startup.
    /// </summary>
    public async Task RestorePurchases()
    {
        if (!IsPlatformSupported())
        {
            if (IsFullGameUnlocked)
                EmitSignal(SignalName.PurchaseRestored);
            return;
        }

        if (!_initialized)
            Initialize(IosProductId, AndroidProductId);

        if (!_initialized || _billingPlugin == null)
        {
            if (IsFullGameUnlocked)
                EmitSignal(SignalName.PurchaseRestored);
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
                if (IsFullGameUnlocked)
                    EmitSignal(SignalName.PurchaseRestored);
                return;
            }

            await WaitForRestoreOrTimeoutAsync(10.0);

            if (IsFullGameUnlocked)
                EmitSignal(SignalName.PurchaseRestored);
        }
        catch (Exception ex)
        {
            GD.PushWarning($"MonetizationManager: restore failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Callback hook for platform plugins to mark the purchase as successful.
    /// </summary>
    public void NotifyPurchaseSucceeded()
    {
        UnlockFullGame();
        EmitSignal(SignalName.PurchaseSucceeded);
    }

    /// <summary>
    /// Callback hook for platform plugins to report purchase failure/cancellation.
    /// </summary>
    /// <param name="reason">Failure reason.</param>
    public void NotifyPurchaseFailed(string reason)
    {
        EmitSignal(SignalName.PurchaseFailed, string.IsNullOrWhiteSpace(reason) ? "Purchase failed." : reason);
    }

    /// <summary>
    /// Unlocks the full game locally and persists the state in the player profile.
    /// </summary>
    public void UnlockFullGame()
    {
        if (PlayerProfile.Instance == null)
        {
            GD.PushWarning("MonetizationManager: PlayerProfile not ready; cannot persist unlock state.");
            return;
        }

        if (PlayerProfile.Instance.IsFullGameUnlocked)
            return;

        PlayerProfile.Instance.IsFullGameUnlocked = true;
        PlayerProfile.Instance.Save();

        AdsManager.Instance?.HideBannerAd();
    }

    private void DeferredInitializeAndRestore()
    {
        Initialize(IosProductId, AndroidProductId);
        _ = RestorePurchases();
    }

    private static bool IsPlatformSupported()
    {
        var os = OS.GetName();
        return os == "Android" || os == "iOS";
    }

    private string GetPlatformProductId()
    {
        var os = OS.GetName();
        return os == "iOS" ? IosProductId : AndroidProductId;
    }

    private static GodotObject? FindBillingPluginSingleton()
    {
        // These names depend on the specific plugins installed.
        // We try a few common ones to keep this code resilient.
        var os = OS.GetName();

        string[] candidates = os == "iOS"
            ?
            [
                "StoreKit",
                "StoreKit2",
                "InAppPurchase",
                "InAppPurchases",
                "GodotInAppPurchase"
            ]
            :
            [
                "GooglePlayBilling",
                "GodotGooglePlayBilling",
                "GodotGooglePlay",
                "InAppPurchase",
                "InAppPurchases"
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

        var successTask = ToSignal(this, SignalName.PurchaseSucceeded);
        var failTask = ToSignal(this, SignalName.PurchaseFailed);
        var timeoutTask = ToSignal(timer, Timer.SignalName.Timeout);

        await Task.WhenAny(successTask, failTask, timeoutTask);
        timer.QueueFree();

        if (timeoutTask.IsCompleted && !IsFullGameUnlocked)
            EmitSignal(SignalName.PurchaseFailed, "Purchase timed out. Please try again.");
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

        var restoredTask = ToSignal(this, SignalName.PurchaseRestored);
        var timeoutTask = ToSignal(timer, Timer.SignalName.Timeout);

        await Task.WhenAny(restoredTask, timeoutTask);
        timer.QueueFree();

        if (!restoredTask.IsCompleted)
            EmitSignal(SignalName.PurchaseRestored);
    }
}
