using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Newtonsoft.Json;

/// <summary>
/// Main cosmetics shop manager handling all cosmetics commerce operations.
/// </summary>
public partial class CosmeticsShop : Node
{
    public static CosmeticsShop Instance { get; private set; } = null!;
    
    [Signal] public delegate void CosmeticPurchasedEventHandler(string cosmeticId, float price, string currency);
    [Signal] public delegate void CosmeticEquippedEventHandler(string cosmeticId, string category);
    [Signal] public delegate void LoadoutChangedEventHandler(int loadoutIndex);
    [Signal] public delegate void ShopOpenedEventHandler();
    [Signal] public delegate void ShopClosedEventHandler();
    
    [Header("Database")]
    [Export] public CosmeticsDatabase? Database;
    
    [Header("Settings")]
    [Export] public bool ShopEnabled = true;
    [Export] public bool AllowIAPPurchases = true;
    [Export] public int MaxLoadouts = 10;
    
    [Header("Economy")]
    [Export] public float IAPExchangeRate = 100f; // Coins per $1
    [Export] public int FreeTierCooldownHours = 24;
    
    // Player progress
    private PlayerCosmeticsProgress _progress = new();
    
    // Current filter/sort state
    private CosmeticCategory _currentCategoryFilter = CosmeticCategory.Hat;
    private string _currentSortBy = "rarity";
    private bool _sortAscending = true;
    private string _searchTerm = string.Empty;
    
    // Recently viewed
    private List<string> _recentlyViewed = new();
    
    public override void _Ready()
    {
        Instance = this;
        ProcessMode = ProcessModeEnum.Always;
        
        LoadProgress();
        
        // Initialize database if not set
        if (Database == null)
        {
            InitializeDefaultDatabase();
        }
        
        GD.Print("CosmeticsShop initialized");
    }
    
    /// <summary>
    /// Initialize with a default database if none provided.
    /// </summary>
    private void InitializeDefaultDatabase()
    {
        // Create a default database with sample cosmetics
        var defaultCosmetics = GenerateSampleCosmetics();
        var db = new CosmeticsDatabase
        {
            DatabaseName = "Default Cosmetics",
            DatabaseVersion = 1,
            Cosmetics = defaultCosmetics
        };
        Database = db;
    }
    
    /// <summary>
    /// Generate sample cosmetics for testing.
    /// </summary>
    private CosmeticItem[] GenerateSampleCosmetics()
    {
        var cosmetics = new List<CosmeticItem>();
        var id = 1;
        
        // Common cosmetics (50+) - Free or very cheap
        var commonHats = new[] { "Basic Cap", "Beanie", "Top Hat", "Cowboy Hat", "Beret", "Party Hat", "Santa Hat", "Winter Hat", "Baseball Cap", "Visor" };
        foreach (var name in commonHats)
        {
            cosmetics.Add(new CosmeticItem
            {
                Id = $"common_hat_{id}",
                DisplayName = name,
                Description = $"A {name.ToLower()} for your character.",
                Rarity = CosmeticRarity.Common,
                Category = CosmeticCategory.Hat,
                PriceCoins = id <= 5 ? 0 : 100, // First 5 are free
                UnlockCondition = UnlockCondition.Always,
                SortOrder = id
            });
            id++;
        }
        
        var commonGlasses = new[] { "Round Glasses", "Sunglasses", "Nerd Glasses", "Monocle", "3D Glasses", "Aviators", "Safety Goggles", "Mask Glasses" };
        foreach (var name in commonGlasses)
        {
            cosmetics.Add(new CosmeticItem
            {
                Id = $"common_glasses_{id}",
                DisplayName = name,
                Description = $"Stylish {name.ToLower()}.",
                Rarity = CosmeticRarity.Common,
                Category = CosmeticCategory.Glasses,
                PriceCoins = id <= 5 ? 0 : 100,
                UnlockCondition = UnlockCondition.Always,
                SortOrder = id
            });
            id++;
        }
        
        var commonMustaches = new[] { "Handlebar", "Pencil Mustache", "Walrus", "Fu Manchu", " Chaplin", "Saloon", "Imperial", "Dali" };
        foreach (var name in commonMustaches)
        {
            cosmetics.Add(new CosmeticItem
            {
                Id = $"common_mustache_{id}",
                DisplayName = name + " Mustache",
                Description = $"A distinguished {name.ToLower()} mustache.",
                Rarity = CosmeticRarity.Common,
                Category = CosmeticCategory.Mustache,
                PriceCoins = id <= 5 ? 0 : 100,
                UnlockCondition = UnlockCondition.Always,
                SortOrder = id
            });
            id++;
        }
        
        var commonWigs = new[] { "Afro", "Mohawk", "Ponytail", "Long Hair", "Spiky", "Bob", "Bald Cap", "Clown Wig" };
        foreach (var name in commonWigs)
        {
            cosmetics.Add(new CosmeticItem
            {
                Id = $"common_wig_{id}",
                DisplayName = name + " Wig",
                Description = $"A fun {name.ToLower()} wig.",
                Rarity = CosmeticRarity.Common,
                Category = CosmeticCategory.Wig,
                PriceCoins = id <= 5 ? 0 : 100,
                UnlockCondition = UnlockCondition.Always,
                SortOrder = id
            });
            id++;
        }
        
        // Uncommon cosmetics (30+) - 1000 coins or 2 star levels
        var uncommonItems = new[] { "Fancy Hat", "Golden Crown", "Pirate Hat", "Viking Helmet", "Wizard Hat", "Robot Glasses", "Laser Visor", "Holo Lens", "Chef Hat", "Vintage Glasses" };
        foreach (var name in uncommonItems)
        {
            cosmetics.Add(new CosmeticItem
            {
                Id = $"uncommon_{id}",
                DisplayName = name,
                Description = $"A rare {name.ToLower()}.",
                Rarity = CosmeticRarity.Uncommon,
                Category = CosmeticCategory.Hat,
                PriceCoins = 1000,
                PriceUsd = 0.99f,
                UnlockCondition = UnlockCondition.LevelUnlock,
                UnlockRequirement = 2,
                SortOrder = id
            });
            id++;
        }
        
        // Rare cosmetics (20+) - 2000 coins or 3 star levels
        var rareItems = new[] { "Golden Crown", "Diamond Tiara", "Royal Scepter", "Champagne Glass", "Martini Glass", "Crown of Thorns", "Angel Halo", "Devil Horns" };
        foreach (var name in rareItems)
        {
            cosmetics.Add(new CosmeticItem
            {
                Id = $"rare_{id}",
                DisplayName = name,
                Description = $"A prestigious {name.ToLower()}.",
                Rarity = CosmeticRarity.Rare,
                Category = CosmeticCategory.Hat,
                PriceCoins = 2000,
                PriceUsd = 1.99f,
                UnlockCondition = UnlockCondition.LevelUnlock,
                UnlockRequirement = 3,
                SortOrder = id
            });
            id++;
        }
        
        // Epic cosmetics (10+) - 5000 coins or IAP
        var epicItems = new[] { "Dragon Wings", "Angel Wings", "Demon Wings", "Shadow Cloak", "Light Aura", "Flame Aura", "Ice Crown", "Thunder Helm" };
        foreach (var name in epicItems)
        {
            cosmetics.Add(new CosmeticItem
            {
                Id = $"epic_{id}",
                DisplayName = name,
                Description = $"An epic {name.ToLower()}.",
                Rarity = CosmeticRarity.Epic,
                Category = CosmeticCategory.Hat,
                PriceCoins = 5000,
                PriceUsd = 2.99f,
                UnlockCondition = UnlockCondition.IAP,
                SortOrder = id
            });
            id++;
        }
        
        // Legendary cosmetics (5+) - IAP only, seasonal
        var legendaryItems = new[] { "World Champion Belt", "Legendary Crown", "Mythic Aura", "Eternal Flame", "Cosmic Crown" };
        foreach (var name in legendaryItems)
        {
            cosmetics.Add(new CosmeticItem
            {
                Id = $"legendary_{id}",
                DisplayName = name,
                Description = $"The legendary {name.ToLower()}.",
                Rarity = CosmeticRarity.Legendary,
                Category = CosmeticCategory.Hat,
                PriceUsd = 4.99f,
                UnlockCondition = UnlockCondition.IAP,
                IsLimitedTime = true,
                SeasonNumber = (id % 4) + 1,
                SeasonalEndDate = DateTime.Now.AddDays(28).ToString("yyyy-MM-dd"),
                SortOrder = id
            });
            id++;
        }
        
        return cosmetics.ToArray();
    }
    
    // ==================== SHOP OPERATIONS ====================
    
    /// <summary>
    /// Get all available cosmetics for the shop view.
    /// </summary>
    public CosmeticItem[] GetShopCosmetics()
    {
        if (Database == null)
            return Array.Empty<CosmeticItem>();
            
        var cosmetics = Database.GetAllCosmetics();
        
        // Apply category filter
        if (_currentCategoryFilter != CosmeticCategory.Hat) // "All" is index 0 but we use Hat as default
        {
            // Special handling for "All" filter
        }
        else
        {
            // "All" includes all categories
        }
        
        // Apply search filter
        if (!string.IsNullOrEmpty(_searchTerm))
        {
            cosmetics = cosmetics.Where(c => 
                c.DisplayName.ToLowerInvariant().Contains(_searchTerm.ToLowerInvariant()) ||
                c.Description.ToLowerInvariant().Contains(_searchTerm.ToLowerInvariant())
            ).ToArray();
        }
        
        // Apply sorting
        cosmetics = Database.SortCosmetics(cosmetics.ToArray(), _currentSortBy, _sortAscending);
        
        return cosmetics;
    }
    
    /// <summary>
    /// Get cosmetics filtered by category.
    /// </summary>
    public CosmeticItem[] GetCosmeticsByCategory(CosmeticCategory category)
    {
        if (Database == null)
            return Array.Empty<CosmeticItem>();
            
        return Database.GetCosmeticsByCategory(category);
    }
    
    /// <summary>
    /// Set the category filter.
    /// </summary>
    public void SetCategoryFilter(CosmeticCategory category)
    {
        _currentCategoryFilter = category;
    }
    
    /// <summary>
    /// Set sort options.
    /// </summary>
    public void SetSortOptions(string sortBy, bool ascending = true)
    {
        _currentSortBy = sortBy;
        _sortAscending = ascending;
    }
    
    /// <summary>
    /// Set search term.
    /// </summary>
    public void SetSearchTerm(string term)
    {
        _searchTerm = term;
    }
    
    /// <summary>
    /// Get featured cosmetics for the shop homepage.
    /// </summary>
    public CosmeticItem[] GetFeaturedCosmetics()
    {
        if (Database == null)
            return Array.Empty<CosmeticItem>();
            
        return Database.GetFeaturedCosmetics();
    }
    
    /// <summary>
    /// Get limited-time cosmetics.
    /// </summary>
    public CosmeticItem[] GetLimitedTimeCosmetics()
    {
        if (Database == null)
            return Array.Empty<CosmeticItem>();
            
        return Database.GetLimitedTimeCosmetics();
    }
    
    // ==================== PURCHASE OPERATIONS ====================
    
    /// <summary>
    /// Check if player can afford a cosmetic with coins.
    /// </summary>
    public bool CanAffordWithCoins(string cosmeticId)
    {
        var cosmetic = GetCosmetic(cosmeticId);
        if (cosmetic == null)
            return false;
            
        if (!cosmetic.CanPurchaseWithCoins())
            return false;
            
        var playerCoins = PlayerProfile.Instance?.GetCoins() ?? 0;
        return playerCoins >= cosmetic.PriceCoins;
    }
    
    /// <summary>
    /// Purchase a cosmetic with coins.
    /// </summary>
    public bool PurchaseCosmeticWithCoins(string cosmeticId)
    {
        var cosmetic = GetCosmetic(cosmeticId);
        if (cosmetic == null)
        {
            GD.PushError($"CosmeticsShop: Cosmetic not found: {cosmeticId}");
            return false;
        }
        
        if (!cosmetic.CanPurchaseWithCoins())
        {
            GD.PushError($"CosmeticsShop: Cannot purchase {cosmeticId} with coins");
            return false;
        }
        
        var playerCoins = PlayerProfile.Instance?.GetCoins() ?? 0;
        if (playerCoins < cosmetic.PriceCoins)
        {
            GD.PushWarning($"CosmeticsShop: Not enough coins for {cosmeticId}");
            return false;
        }
        
        // Deduct coins
        PlayerProfile.Instance?.AddCoins(-cosmetic.PriceCoins);
        
        // Add to owned cosmetics
        _progress.AddOwnedCosmetic(cosmeticId);
        SaveProgress();
        
        // Track analytics
        AnalyticsEventTracker.Instance?.TrackCosmeticPurchased(
            cosmetic.Category.ToString(),
            cosmeticId,
            cosmetic.PriceCoins,
            "coins"
        );
        
        EmitSignal(SignalName.CosmeticPurchased, cosmeticId, cosmetic.PriceCoins, "coins");
        
        return true;
    }
    
    /// <summary>
    /// Purchase a cosmetic with IAP.
    /// </summary>
    public async Task<bool> PurchaseCosmeticWithIAP(string cosmeticId)
    {
        var cosmetic = GetCosmetic(cosmeticId);
        if (cosmetic == null)
        {
            GD.PushError($"CosmeticsShop: Cosmetic not found: {cosmeticId}");
            return false;
        }
        
        if (!AllowIAPPurchases || !cosmetic.CanPurchaseWithMoney())
        {
            GD.PushError($"CosmeticsShop: Cannot purchase {cosmeticId} with IAP");
            return false;
        }
        
        try
        {
            // In a real implementation, this would call MonetizationManager
            // For now, we simulate a successful purchase
            
            // Simulate purchase delay
            await Task.Delay(1000);
            
            // Add to owned cosmetics
            _progress.AddOwnedCosmetic(cosmeticId);
            SaveProgress();
            
            // Track analytics
            AnalyticsEventTracker.Instance?.TrackCosmeticPurchased(
                cosmetic.Category.ToString(),
                cosmeticId,
                cosmetic.PriceUsd,
                "USD"
            );
            
            EmitSignal(SignalName.CosmeticPurchased, cosmeticId, cosmetic.PriceUsd, "USD");
            
            return true;
        }
        catch (Exception ex)
        {
            GD.PushError($"CosmeticsShop: IAP purchase failed: {ex.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// Unlock a cosmetic (free or achievement unlock).
    /// </summary>
    public void UnlockCosmetic(string cosmeticId, string unlockMethod = "achievement")
    {
        var cosmetic = GetCosmetic(cosmeticId);
        if (cosmetic == null)
            return;
        
        if (_progress.OwnsCosmetic(cosmeticId))
            return;
        
        _progress.AddOwnedCosmetic(cosmeticId);
        SaveProgress();
        
        AnalyticsEventTracker.Instance?.TrackCosmeticUnlocked(
            cosmetic.Category.ToString(),
            cosmeticId,
            unlockMethod
        );
    }
    
    /// <summary>
    /// Check if player owns a cosmetic.
    /// </summary>
    public bool OwnsCosmetic(string cosmeticId)
    {
        return _progress.OwnsCosmetic(cosmeticId);
    }
    
    /// <summary>
    /// Get a cosmetic by ID.
    /// </summary>
    public CosmeticItem? GetCosmetic(string cosmeticId)
    {
        return Database?.GetCosmeticById(cosmeticId);
    }
    
    // ==================== EQUIP OPERATIONS ====================
    
    /// <summary>
    /// Equip a cosmetic to a category slot.
    /// </summary>
    public void EquipCosmetic(string cosmeticId)
    {
        var cosmetic = GetCosmetic(cosmeticId);
        if (cosmetic == null)
            return;
        
        if (!_progress.OwnsCosmetic(cosmeticId))
        {
            GD.PushWarning($"CosmeticsShop: Cannot equip unowned cosmetic: {cosmeticId}");
            return;
        }
        
        _progress.EquipCosmetic(cosmetic.Category.ToString(), cosmeticId);
        SaveProgress();
        
        // Update player profile
        UpdatePlayerProfileFromLoadout();
        
        EmitSignal(SignalName.CosmeticEquipped, cosmeticId, cosmetic.Category.ToString());
    }
    
    /// <summary>
    /// Unequip a cosmetic from a category.
    /// </summary>
    public void UnequipCosmetic(CosmeticCategory category)
    {
        _progress.UnequipCosmetic(category.ToString());
        SaveProgress();
        UpdatePlayerProfileFromLoadout();
    }
    
    /// <summary>
    /// Get currently equipped cosmetic for a category.
    /// </summary>
    public string? GetEquippedCosmetic(CosmeticCategory category)
    {
        return _progress.GetEquippedCosmetic(category.ToString());
    }
    
    /// <summary>
    /// Get all equipped cosmetics.
    /// </summary>
    public Dictionary<string, string> GetAllEquippedCosmetics()
    {
        return new Dictionary<string, string>(_progress.EquippedCosmetics);
    }
    
    /// <summary>
    /// Apply a full loadout of cosmetics.
    /// </summary>
    public void ApplyLoadout(Dictionary<string, string> loadout)
    {
        foreach (var kvp in loadout)
        {
            if (_progress.OwnsCosmetic(kvp.Value))
            {
                _progress.EquipCosmetic(kvp.Key, kvp.Value);
            }
        }
        SaveProgress();
        UpdatePlayerProfileFromLoadout();
    }
    
    // ==================== LOADOUT OPERATIONS ====================
    
    /// <summary>
    /// Create a new loadout from currently equipped cosmetics.
    /// </summary>
    public int CreateLoadout(string name)
    {
        if (_progress.Loadouts.Count >= MaxLoadouts)
        {
            GD.PushWarning("CosmeticsShop: Maximum loadouts reached");
            return -1;
        }
        
        var loadout = _progress.CreateLoadout(name);
        SaveProgress();
        
        var index = _progress.Loadouts.Count - 1;
        EmitSignal(SignalName.LoadoutChanged, index);
        
        return index;
    }
    
    /// <summary>
    /// Apply a saved loadout.
    /// </summary>
    public void ApplyLoadout(int index)
    {
        if (index < 0 || index >= _progress.Loadouts.Count)
            return;
        
        _progress.ApplyLoadout(index);
        SaveProgress();
        UpdatePlayerProfileFromLoadout();
        
        EmitSignal(SignalName.LoadoutChanged, index);
    }
    
    /// <summary>
    /// Delete a loadout.
    /// </summary>
    public void DeleteLoadout(int index)
    {
        if (index < 0 || index >= _progress.Loadouts.Count)
            return;
        
        _progress.DeleteLoadout(index);
        SaveProgress();
    }
    
    /// <summary>
    /// Get all saved loadouts.
    /// </summary>
    public List<CosmeticLoadout> GetLoadouts()
    {
        return _progress.Loadouts;
    }
    
    /// <summary>
    /// Get a specific loadout.
    /// </summary>
    public CosmeticLoadout? GetLoadout(int index)
    {
        if (index < 0 || index >= _progress.Loadouts.Count)
            return null;
        
        return _progress.Loadouts[index];
    }
    
    // ==================== DATA PERSISTENCE ====================
    
    /// <summary>
    /// Load player cosmetics progress.
    /// </summary>
    private void LoadProgress()
    {
        var savePath = "user://cosmetics_progress.json";
        
        try
        {
            if (FileAccess.FileExists(savePath))
            {
                using var file = FileAccess.Open(savePath, FileAccess.ModeFlags.Read);
                var json = file?.GetAsText() ?? string.Empty;
                
                if (!string.IsNullOrEmpty(json))
                {
                    var loaded = JsonConvert.DeserializeObject<PlayerCosmeticsProgress>(json);
                    if (loaded != null)
                    {
                        _progress = loaded;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            GD.PushWarning($"CosmeticsShop: Failed to load progress: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Save player cosmetics progress.
    /// </summary>
    public void SaveProgress()
    {
        var savePath = "user://cosmetics_progress.json";
        
        try
        {
            var json = JsonConvert.SerializeObject(_progress, Formatting.Indented);
            using var file = FileAccess.Open(savePath, FileAccess.ModeFlags.Write);
            file?.StoreString(json);
        }
        catch (Exception ex)
        {
            GD.PushWarning($"CosmeticsShop: Failed to save progress: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Update player profile with equipped cosmetics.
    /// </summary>
    private void UpdatePlayerProfileFromLoadout()
    {
        if (PlayerProfile.Instance == null)
            return;
        
        // Map cosmetic categories to player profile indices
        if (_progress.EquippedCosmetics.TryGetValue("Hat", out var hatId))
        {
            var hatIndex = GetHatIndexFromId(hatId);
            PlayerProfile.SelectedHatIndex = hatIndex;
        }
        
        if (_progress.EquippedCosmetics.TryGetValue("Glasses", out var glassesId))
        {
            var glassesIndex = GetGlassesIndexFromId(glassesId);
            PlayerProfile.SelectedGlassesIndex = glassesIndex;
        }
        
        if (_progress.EquippedCosmetics.TryGetValue("Mustache", out var mustacheId))
        {
            var mustacheIndex = GetMustacheIndexFromId(mustacheId);
            PlayerProfile.SelectedMoustacheIndex = mustacheIndex;
        }
        
        if (_progress.EquippedCosmetics.TryGetValue("Wig", out var wigId))
        {
            var wigIndex = GetWigIndexFromId(wigId);
            PlayerProfile.SelectedWigIndex = wigIndex;
        }
        
        PlayerProfile.SaveCosmetics();
    }
    
    private int GetHatIndexFromId(string id) => 0;
    private int GetGlassesIndexFromId(string id) => 0;
    private int GetMustacheIndexFromId(string id) => 0;
    private int GetWigIndexFromId(string id) => 0;
    
    // ==================== ANALYTICS ====================
    
    /// <summary>
    /// Record that a cosmetic was viewed in the shop.
    /// </summary>
    public void RecordCosmeticViewed(string cosmeticId)
    {
        _progress.AddRecentCosmetic(cosmeticId);
        _recentlyViewed.Remove(cosmeticId);
        _recentlyViewed.Insert(0, cosmeticId);
        
        if (_recentlyViewed.Count > 10)
            _recentlyViewed.RemoveAt(_recentlyViewed.Count - 1);
        
        AnalyticsEventTracker.Instance?.LogEvent("cosmetic_viewed", new Dictionary<string, object>
        {
            { "cosmetic_id", cosmeticId },
            { "source", "shop" }
        });
    }
    
    /// <summary>
    /// Get statistics about owned cosmetics.
    /// </summary>
    public Dictionary<CosmeticRarity, int> GetOwnedCountByRarity()
    {
        if (Database == null)
            return new Dictionary<CosmeticRarity, int>();
        
        return _progress.GetOwnedCountByRarity(Database);
    }
    
    /// <summary>
    /// Get total number of owned cosmetics.
    /// </summary>
    public int GetTotalOwnedCount()
    {
        return _progress.OwnedCosmetics.Count;
    }
    
    // ==================== SHOP LIFECYCLE ====================
    
    /// <summary>
    /// Open the shop.
    /// </summary>
    public void OpenShop()
    {
        if (!ShopEnabled)
            return;
            
        EmitSignal(SignalName.ShopOpened);
        GD.Print("CosmeticsShop opened");
    }
    
    /// <summary>
    /// Close the shop.
    /// </summary>
    public void CloseShop()
    {
        SaveProgress();
        EmitSignal(SignalName.ShopClosed);
        GD.Print("CosmeticsShop closed");
    }
}
