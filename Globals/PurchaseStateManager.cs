using System;
using System.IO;
using System.Text.Json;
using Godot;

/// <summary>
/// Manages persistent storage of purchase state for "Remove Ads" premium feature.
/// Handles save/load operations with error handling and data validation.
/// </summary>
public partial class PurchaseStateManager : Node
{
    public static PurchaseStateManager Instance { get; private set; } = null!;

    [Signal] public delegate void PurchaseStateChangedEventHandler(bool isPurchased);

    private const string PURCHASE_STATE_FILE = "user://premium/purchase_state.json";

    [Serializable]
    private class PurchaseStateData
    {
        public bool IsRemoveAdsPurchased { get; set; } = false;
        public long PurchaseTimestamp { get; set; } = 0;
        public string Platform { get; set; } = "";
        public string ProductId { get; set; } = "";
        public string Version { get; set; } = "1.0";
    }

    private PurchaseStateData _currentState = new PurchaseStateData();
    private bool _isLoaded = false;

    /// <summary>
    /// Returns whether the user has purchased the "Remove Ads" premium feature.
    /// </summary>
    public bool IsRemoveAdsPurchased => _currentState.IsRemoveAdsPurchased;

    /// <summary>
    /// Returns the timestamp of when the "Remove Ads" purchase was made.
    /// </summary>
    public DateTime PurchaseTimestamp => _currentState.PurchaseTimestamp > 0 
        ? DateTimeOffset.FromUnixTimeSeconds(_currentState.PurchaseTimestamp).DateTime
        : DateTime.MinValue;

    /// <summary>
    /// Returns the platform where the purchase was made.
    /// </summary>
    public string Platform => _currentState.Platform;

    /// <summary>
    /// Returns the product ID that was purchased.
    /// </summary>
    public string ProductId => _currentState.ProductId;

    public override void _Ready()
    {
        Instance = this;
        ProcessMode = ProcessModeEnum.Always;

        LoadPurchaseState();
    }

    /// <summary>
    /// Sets the "Remove Ads" purchase status.
    /// </summary>
    /// <param name="purchased">True if purchased, false otherwise.</param>
    public void SetRemoveAdsPurchased(bool purchased)
    {
        var previousState = _currentState.IsRemoveAdsPurchased;
        _currentState.IsRemoveAdsPurchased = purchased;
        
        if (purchased)
        {
            _currentState.PurchaseTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            _currentState.Platform = OS.GetName();
            _currentState.ProductId = PremiumManager.Instance?.RemoveAdsProductId ?? "remove_ads";
        }
        
        // Only emit signal if state actually changed
        if (previousState != purchased)
        {
            EmitSignal(SignalName.PurchaseStateChanged, purchased);
        }
    }

    /// <summary>
    /// Loads the purchase state from persistent storage.
    /// Called automatically on startup.
    /// </summary>
    public void LoadPurchaseState()
    {
        try
        {
            if (!FileAccess.FileExists(PURCHASE_STATE_FILE))
            {
                GD.Print("PurchaseStateManager: No existing purchase state found, starting fresh");
                _currentState = new PurchaseStateData();
                _isLoaded = true;
                return;
            }

            using var file = FileAccess.Open(PURCHASE_STATE_FILE, FileAccess.ModeFlags.Read);
            if (file == null)
            {
                GD.PushWarning("PurchaseStateManager: Could not open purchase state file");
                _currentState = new PurchaseStateData();
                _isLoaded = true;
                return;
            }

            var jsonString = file.GetAsText();
            file.Close();

            if (string.IsNullOrWhiteSpace(jsonString))
            {
                GD.PushWarning("PurchaseStateManager: Purchase state file is empty");
                _currentState = new PurchaseStateData();
                _isLoaded = true;
                return;
            }

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                AllowTrailingCommas = true
            };

            var loadedState = JsonSerializer.Deserialize<PurchaseStateData>(jsonString, options);
            if (loadedState != null)
            {
                _currentState = loadedState;
                GD.Print($"PurchaseStateManager: Loaded purchase state - Remove Ads: {_currentState.IsRemoveAdsPurchased}");
            }
            else
            {
                GD.PushWarning("PurchaseStateManager: Failed to deserialize purchase state");
                _currentState = new PurchaseStateData();
            }
        }
        catch (Exception ex)
        {
            GD.PushError($"PurchaseStateManager: Error loading purchase state: {ex.Message}");
            _currentState = new PurchaseStateData();
        }
        finally
        {
            _isLoaded = true;
        }
    }

    /// <summary>
    /// Saves the current purchase state to persistent storage.
    /// Should be called after making changes to the purchase state.
    /// </summary>
    public void SavePurchaseState()
    {
        try
        {
            // Ensure the directory exists
            var dirPath = PURCHASE_STATE_FILE.GetBaseDir();
            if (!DirAccess.DirExists(dirPath))
            {
                DirAccess.MakeDirRecursive(dirPath);
            }

            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
            };

            var jsonString = JsonSerializer.Serialize(_currentState, options);

            using var file = FileAccess.Open(PURCHASE_STATE_FILE, FileAccess.ModeFlags.Write);
            if (file == null)
            {
                GD.PushError("PurchaseStateManager: Could not open purchase state file for writing");
                return;
            }

            file.StoreString(jsonString);
            file.Close();

            GD.Print("PurchaseStateManager: Purchase state saved successfully");
        }
        catch (Exception ex)
        {
            GD.PushError($"PurchaseStateManager: Error saving purchase state: {ex.Message}");
        }
    }

    /// <summary>
    /// Clears the purchase state (useful for testing or account reset).
    /// </summary>
    public void ClearPurchaseState()
    {
        try
        {
            _currentState = new PurchaseStateData();
            
            if (FileAccess.FileExists(PURCHASE_STATE_FILE))
            {
                DirAccess.Remove(PURCHASE_STATE_FILE);
            }
            
            GD.Print("PurchaseStateManager: Purchase state cleared");
            EmitSignal(SignalName.PurchaseStateChanged, false);
        }
        catch (Exception ex)
        {
            GD.PushError($"PurchaseStateManager: Error clearing purchase state: {ex.Message}");
        }
    }

    /// <summary>
    /// Validates the current purchase state for consistency and security.
    /// Checks for anomalies like future timestamps or invalid platform names.
    /// </summary>
    /// <returns>True if the state appears valid, false otherwise.</returns>
    public bool ValidatePurchaseState()
    {
        try
        {
            // Check if timestamp is reasonable (not in the future, not too old)
            if (_currentState.PurchaseTimestamp > 0)
            {
                var purchaseTime = DateTimeOffset.FromUnixTimeSeconds(_currentState.PurchaseTimestamp);
                var now = DateTimeOffset.UtcNow;
                
                // Reject purchases made more than 10 years ago (likely data corruption)
                if (purchaseTime < now.AddYears(-10))
                {
                    GD.PushWarning("PurchaseStateManager: Purchase timestamp too old, possible data corruption");
                    return false;
                }
                
                // Allow recent purchases but warn about future timestamps
                if (purchaseTime > now.AddMinutes(5))
                {
                    GD.PushWarning("PurchaseStateManager: Purchase timestamp in future, possible data corruption");
                    return false;
                }
            }

            // Check if platform is recognized
            var os = OS.GetName();
            if (!string.IsNullOrEmpty(_currentState.Platform) && _currentState.Platform != os)
            {
                GD.PushWarning($"PurchaseStateManager: Platform mismatch - stored: {_currentState.Platform}, current: {os}");
                // Don't fail validation for this, as users might have transferred devices
            }

            // Check if product ID is reasonable
            if (!string.IsNullOrEmpty(_currentState.ProductId) && _currentState.ProductId != "remove_ads")
            {
                GD.PushWarning($"PurchaseStateManager: Unexpected product ID: {_currentState.ProductId}");
                // Don't fail validation for this, as it might be a different SKU
            }

            return true;
        }
        catch (Exception ex)
        {
            GD.PushError($"PurchaseStateManager: Error validating purchase state: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Gets a formatted string describing the current purchase state for debugging.
    /// </summary>
    /// <returns>A human-readable string describing the purchase state.</returns>
    public string GetPurchaseStateDescription()
    {
        if (!_isLoaded)
            return "Purchase state not loaded yet";

        var status = _currentState.IsRemoveAdsPurchased ? "Purchased" : "Not purchased";
        var timestamp = _currentState.PurchaseTimestamp > 0 
            ? PurchaseTimestamp.ToString("yyyy-MM-dd HH:mm:ss UTC")
            : "Never";
        
        return $"Remove Ads: {status} | Timestamp: {timestamp} | Platform: {_currentState.Platform} | Product: {_currentState.ProductId}";
    }

    /// <summary>
    /// Exports the purchase state for debugging or support purposes.
    /// </summary>
    /// <returns>A JSON string containing the purchase state data.</returns>
    public string ExportPurchaseState()
    {
        try
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
            };

            return JsonSerializer.Serialize(_currentState, options);
        }
        catch (Exception ex)
        {
            GD.PushError($"PurchaseStateManager: Error exporting purchase state: {ex.Message}");
            return "{}";
        }
    }

    private bool _hasPendingReward = false;

    /// <summary>
    /// Saves whether a reward (like second chance) has been earned and is pending.
    /// </summary>
    public void SaveRewardEarned(bool earned)
    {
        _hasPendingReward = earned;
        GD.Print($"PurchaseStateManager: Pending reward set to {earned}");
    }

    /// <summary>
    /// Checks if there is a pending reward to be applied.
    /// </summary>
    public bool HasPendingReward()
    {
        return _hasPendingReward;
    }

    /// <summary>
    /// Clears any pending rewards.
    /// </summary>
    public void ClearPendingReward()
    {
        _hasPendingReward = false;
        GD.Print("PurchaseStateManager: Pending reward cleared");
    }
}