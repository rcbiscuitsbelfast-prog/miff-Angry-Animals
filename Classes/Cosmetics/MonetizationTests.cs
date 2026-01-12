using System;
using System.Collections.Generic;
using Godot;
using NUnit.Framework;

/// <summary>
/// Automated testing framework for Cosmetics and Battle Pass systems.
/// Run these tests to verify core functionality.
/// </summary>
public class MonetizationTests
{
    private CosmeticsShop _shop = null!;
    private BattlePass _battlePass = null!;
    private CosmeticsDatabase _database = null!;
    private BattlePassSeason _season = null!;
    
    [SetUp]
    public void SetUp()
    {
        // Create test instances
        _database = new CosmeticsDatabase();
        _database.Cosmetics = CreateTestCosmetics();
        
        _shop = new CosmeticsShop();
        _shop.Database = _database;
        
        _season = new BattlePassSeason();
        _season.SeasonNumber = 1;
        _season.SeasonName = "Test Season";
        _season.TotalTiers = 10;
        _season.FreeTierCount = 5;
        _season.InitializeDefaults();
        
        _battlePass = new BattlePass();
        _battlePass.CurrentSeason = _season;
    }
    
    [TearDown]
    public void TearDown()
    {
        _shop?.QueueFree();
        _battlePass?.QueueFree();
    }
    
    // ==================== COSMETIC TESTS ====================
    
    [Test]
    public void TestCosmeticCreation()
    {
        var cosmetic = new CosmeticItem
        {
            Id = "test_hat",
            DisplayName = "Test Hat",
            Description = "A test hat",
            Rarity = CosmeticRarity.Common,
            Category = CosmeticCategory.Hat,
            PriceCoins = 100,
            PriceUsd = 0,
            AssetPath = "res://Assets/test.png",
            UnlockCondition = UnlockCondition.Always,
            IsActive = true
        };
        
        Assert.That(cosmetic.Id, Is.EqualTo("test_hat"));
        Assert.That(cosmetic.DisplayName, Is.EqualTo("Test Hat"));
        Assert.That(cosmetic.Rarity, Is.EqualTo(CosmeticRarity.Common));
        Assert.That(cosmetic.PriceCoins, Is.EqualTo(100));
        Assert.That(cosmetic.CanPurchaseWithCoins(), Is.True);
        Assert.That(cosmetic.IsFree(), Is.False);
    }
    
    [Test]
    public void TestCosmeticPriceString()
    {
        var coinCosmetic = new CosmeticItem { PriceCoins = 500, PriceUsd = 0 };
        var usdCosmetic = new CosmeticItem { PriceCoins = 0, PriceUsd = 2.99f };
        var freeCosmetic = new CosmeticItem { PriceCoins = 0, PriceUsd = 0 };
        
        Assert.That(coinCosmetic.GetPriceString(), Is.EqualTo("500 coins"));
        Assert.That(usdCosmetic.GetPriceString(), Is.EqualTo("$2.99"));
        Assert.That(freeCosmetic.GetPriceString(), Is.EqualTo("FREE"));
    }
    
    [Test]
    public void TestCosmeticRarityColors()
    {
        var common = new CosmeticItem { Rarity = CosmeticRarity.Common };
        var legendary = new CosmeticItem { Rarity = CosmeticRarity.Legendary };
        
        var commonColor = common.GetRarityColor();
        var legendaryColor = legendary.GetRarityColor();
        
        // Common should be gray-ish
        Assert.That(commonColor.R, Is.InRange(0.6f, 0.8f));
        Assert.That(commonColor.G, Is.InRange(0.6f, 0.8f));
        Assert.That(commonColor.B, Is.InRange(0.6f, 0.8f));
        
        // Legendary should be gold-ish
        Assert.That(legendaryColor.R, Is.GreaterThan(0.8f));
        Assert.That(legendaryColor.G, Is.GreaterThan(0.5f));
    }
    
    [Test]
    public void TestCosmeticClone()
    {
        var original = new CosmeticItem
        {
            Id = "original",
            DisplayName = "Original",
            Rarity = CosmeticRarity.Epic,
            PriceCoins = 1000
        };
        
        var clone = original.Clone();
        
        Assert.That(clone.Id, Is.EqualTo(original.Id));
        Assert.That(clone.DisplayName, Is.EqualTo(original.DisplayName));
        Assert.That(clone.Rarity, Is.EqualTo(original.Rarity));
        Assert.That(clone.PriceCoins, Is.EqualTo(original.PriceCoins));
        
        // Clone should be independent
        clone.PriceCoins = 2000;
        Assert.That(original.PriceCoins, Is.EqualTo(1000));
    }
    
    [Test]
    public void TestCosmeticIsPremiumExclusive()
    {
        var freeCosmetic = new CosmeticItem { UnlockCondition = UnlockCondition.Always };
        var battlePassCosmetic = new CosmeticItem { UnlockCondition = UnlockCondition.BattlePassTier };
        var iapCosmetic = new CosmeticItem { UnlockCondition = UnlockCondition.IAP };
        
        Assert.That(freeCosmetic.IsPremiumExclusive(), Is.False);
        Assert.That(battlePassCosmetic.IsPremiumExclusive(), Is.True);
        Assert.That(iapCosmetic.IsPremiumExclusive(), Is.True);
    }
    
    // ==================== DATABASE TESTS ====================
    
    [Test]
    public void TestDatabaseGetAllCosmetics()
    {
        var cosmetics = _database.GetAllCosmetics();
        Assert.That(cosmetics.Length, Is.EqualTo(10));
    }
    
    [Test]
    public void TestDatabaseGetByCategory()
    {
        var hats = _database.GetCosmeticsByCategory(CosmeticCategory.Hat);
        Assert.That(hats.Length, Is.EqualTo(5));
        
        var glasses = _database.GetCosmeticsByCategory(CosmeticCategory.Glasses);
        Assert.That(glasses.Length, Is.EqualTo(5));
    }
    
    [Test]
    public void TestDatabaseGetByRarity()
    {
        var common = _database.GetCosmeticsByRarity(CosmeticRarity.Common);
        Assert.That(common.Length, Is.EqualTo(3));
        
        var rare = _database.GetCosmeticsByRarity(CosmeticRarity.Rare);
        Assert.That(rare.Length, Is.EqualTo(3));
    }
    
    [Test]
    public void TestDatabaseGetById()
    {
        var found = _database.GetCosmeticById("hat_1");
        Assert.That(found, Is.Not.Null);
        Assert.That(found!.DisplayName, Is.EqualTo("Test Hat 1"));
        
        var notFound = _database.GetCosmeticById("nonexistent");
        Assert.That(notFound, Is.Null);
    }
    
    [Test]
    public void TestDatabaseSearch()
    {
        var results = _database.SearchCosmetics("Hat");
        Assert.That(results.Length, Is.EqualTo(5));
        
        var noResults = _database.SearchCosmetics("xyz123");
        Assert.That(noResults.Length, Is.EqualTo(0));
    }
    
    [Test]
    public void TestDatabaseSortCosmetics()
    {
        var all = _database.GetAllCosmetics();
        var byRarity = _database.SortCosmetics(all, "rarity", true);
        
        // Should be sorted ascending by rarity
        for (int i = 1; i < byRarity.Length; i++)
        {
            Assert.That((int)byRarity[i - 1].Rarity, Is.LessThanOrEqualTo((int)byRarity[i].Rarity));
        }
    }
    
    [Test]
    public void TestDatabaseStatistics()
    {
        var counts = _database.GetCosmeticCountByRarity();
        Assert.That(counts.Count, Is.EqualTo(2)); // Common and Rare
        Assert.That(counts[CosmeticRarity.Common], Is.EqualTo(3));
        Assert.That(counts[CosmeticRarity.Rare], Is.EqualTo(3));
        
        var total = _database.GetTotalCosmeticCount();
        Assert.That(total, Is.EqualTo(10));
    }
    
    [Test]
    public void TestDatabaseAddRemoveCosmetics()
    {
        var initialCount = _database.GetTotalCosmeticCount();
        
        var newCosmetic = new CosmeticItem
        {
            Id = "new_cosmetic",
            DisplayName = "New Cosmetic",
            Category = CosmeticCategory.Hat
        };
        
        _database.AddCosmetic(newCosmetic);
        Assert.That(_database.GetTotalCosmeticCount(), Is.EqualTo(initialCount + 1));
        Assert.That(_database.GetCosmeticById("new_cosmetic"), Is.Not.Null);
        
        _database.RemoveCosmetic("new_cosmetic");
        Assert.That(_database.GetTotalCosmeticCount(), Is.EqualTo(initialCount));
        Assert.That(_database.GetCosmeticById("new_cosmetic"), Is.Null);
    }
    
    [Test]
    public void TestDatabaseValidation()
    {
        var issues = _database.ValidateDatabase();
        Assert.That(issues, Does.Contain("Database validation passed"));
    }
    
    [Test]
    public void TestDatabaseJsonExport()
    {
        var json = _database.ExportToJson();
        Assert.That(json, Does.Contain("Test Cosmetics"));
        Assert.That(json, Does.Contain("cosmetics"));
        Assert.That(json, Does.Contain("hat_1"));
    }
    
    // ==================== SHOP TESTS ====================
    
    [Test]
    public void TestShopOwnsCosmetic()
    {
        Assert.That(_shop.OwnsCosmetic("hat_1"), Is.False);
        
        // Simulate purchasing
        var progress = new PlayerCosmeticsProgress();
        progress.AddOwnedCosmetic("hat_1");
        
        Assert.That(progress.OwnsCosmetic("hat_1"), Is.True);
    }
    
    [Test]
    public void TestShopEquipCosmetic()
    {
        var progress = new PlayerCosmeticsProgress();
        
        progress.EquipCosmetic("Hat", "hat_1");
        
        Assert.That(progress.GetEquippedCosmetic("Hat"), Is.EqualTo("hat_1"));
        Assert.That(progress.IsCosmeticEquipped("hat_1"), Is.True);
    }
    
    [Test]
    public void TestShopUnequipCosmetic()
    {
        var progress = new PlayerCosmeticsProgress();
        
        progress.EquipCosmetic("Hat", "hat_1");
        progress.UnequipCosmetic("Hat");
        
        Assert.That(progress.GetEquippedCosmetic("Hat"), Is.Null);
    }
    
    [Test]
    public void TestShopLoadoutCreation()
    {
        var progress = new PlayerCosmeticsProgress();
        
        progress.EquipCosmetic("Hat", "hat_1");
        progress.EquipCosmetic("Glasses", "glasses_1");
        
        var loadout = progress.CreateLoadout("Test Loadout");
        
        Assert.That(loadout, Is.Not.Null);
        Assert.That(loadout.Name, Is.EqualTo("Test Loadout"));
        Assert.That(loadout.EquippedCosmetics.Count, Is.EqualTo(2));
    }
    
    [Test]
    public void TestShopLoadoutApply()
    {
        var progress = new PlayerCosmeticsProgress();
        progress.AddOwnedCosmetic("hat_1");
        progress.AddOwnedCosmetic("glasses_1");
        
        var loadout = progress.CreateLoadout("My Loadout");
        loadout.EquippedCosmetics["Hat"] = "hat_1";
        loadout.EquippedCosmetics["Glasses"] = "glasses_1";
        
        progress.ApplyLoadout(0);
        
        Assert.That(progress.GetEquippedCosmetic("Hat"), Is.EqualTo("hat_1"));
        Assert.That(progress.GetEquippedCosmetic("Glasses"), Is.EqualTo("glasses_1"));
    }
    
    [Test]
    public void TestShopLoadoutDelete()
    {
        var progress = new PlayerCosmeticsProgress();
        
        progress.CreateLoadout("Loadout 1");
        progress.CreateLoadout("Loadout 2");
        
        Assert.That(progress.Loadouts.Count, Is.EqualTo(2));
        
        progress.DeleteLoadout(0);
        
        Assert.That(progress.Loadouts.Count, Is.EqualTo(1));
        Assert.That(progress.Loadouts[0].Name, Is.EqualTo("Loadout 2"));
    }
    
    [Test]
    public void TestShopMaxLoadouts()
    {
        var progress = new PlayerCosmeticsProgress();
        
        // Create 10 loadouts
        for (int i = 0; i < 10; i++)
        {
            progress.CreateLoadout($"Loadout {i}");
        }
        
        Assert.That(progress.Loadouts.Count, Is.EqualTo(10));
        
        // 11th should fail
        var eleventh = progress.CreateLoadout("Extra");
        Assert.That(eleventh, Is.Null);
    }
    
    // ==================== BATTLE PASS TESTS ====================
    
    [Test]
    public void TestBattlePassInitialization()
    {
        Assert.That(_battlePass.GetCurrentTier(), Is.EqualTo(1));
        Assert.That(_battlePass.GetTotalTiers(), Is.EqualTo(10));
        Assert.That(_battlePass.HasBattlePass(), Is.False);
        Assert.That(_battlePass.GetDaysRemaining(), Is.GreaterThanOrEqualTo(27));
    }
    
    [Test]
    public void TestBattlePassAddXp()
    {
        var initialTier = _battlePass.GetCurrentTier();
        var tiersGained = _battlePass.AddXp(150);
        
        Assert.That(tiersGained, Is.EqualTo(1));
        Assert.That(_battlePass.GetCurrentTier(), Is.EqualTo(initialTier + 1));
    }
    
    [Test]
    public void TestBattlePassMultipleTiersXp()
    {
        // Add enough XP for multiple tiers
        var tiersGained = _battlePass.AddXp(1000);
        
        // With 100 XP per tier, should gain ~10 tiers
        Assert.That(tiersGained, Is.GreaterThanOrEqualTo(1));
        Assert.That(tiersGained, Is.LessThanOrEqualTo(10));
    }
    
    [Test]
    public void TestBattlePassPurchase()
    {
        Assert.That(_battlePass.HasBattlePass(), Is.False);
        
        // Simulate purchase
        var progress = new BattlePassProgress();
        progress.PurchaseBattlePass();
        
        Assert.That(progress.HasBattlePass, Is.True);
    }
    
    [Test]
    public void TestBattlePassClaimReward()
    {
        var progress = new BattlePassProgress();
        progress.PurchaseBattlePass();
        progress.CurrentTier = 5;
        
        // Should be able to claim tier 3
        var canClaim = progress.CanClaimTier(3, true);
        Assert.That(canClaim, Is.True);
        
        // Should not be able to claim tier 6 (not reached)
        var cannotClaim = progress.CanClaimTier(6, true);
        Assert.That(cannotClaim, Is.False);
    }
    
    [Test]
    public void TestBattlePassProgressTracking()
    {
        var progress = new BattlePassProgress();
        
        progress.AddXp(150, _season);
        
        Assert.That(progress.CurrentTier, Is.EqualTo(2));
        Assert.That(progress.TotalXpEarned, Is.EqualTo(150));
    }
    
    [Test]
    public void TestBattlePassPremiumCurrency()
    {
        var progress = new BattlePassProgress();
        
        progress.AddPremiumCurrency(100);
        Assert.That(progress.GetPremiumCurrencyBalance(), Is.EqualTo(100));
        
        var spent = progress.SpendPremiumCurrency(50);
        Assert.That(spent, Is.True);
        Assert.That(progress.GetPremiumCurrencyBalance(), Is.EqualTo(50));
        
        // Can't spend more than balance
        spent = progress.SpendPremiumCurrency(100);
        Assert.That(spent, Is.False);
    }
    
    [Test]
    public void TestBattlePassXpMultiplier()
    {
        var progress = new BattlePassProgress();
        var initialXp = progress.TotalXpEarned;
        
        progress.SetXpMultiplier(2.0f);
        progress.AddXp(100, _season);
        
        // Should have earned 200 XP (2x multiplier)
        Assert.That(progress.TotalXpEarned, Is.EqualTo(initialXp + 200));
    }
    
    [Test]
    public void TestBattlePassSeasonProgress()
    {
        var progress = new BattlePassProgress();
        progress.CurrentTier = 5;
        progress.TotalXpEarned = 500;
        
        var completion = progress.GetSeasonCompletion(_season);
        
        // Should be around 50% complete for 10 tiers
        Assert.That(completion, Is.GreaterThan(0.4f));
        Assert.That(completion, Is.LessThanOrEqualTo(0.6f));
    }
    
    [Test]
    public void TestBattlePassResetForNewSeason()
    {
        var progress = new BattlePassProgress();
        progress.CurrentTier = 10;
        progress.TotalXpEarned = 1000;
        progress.HasBattlePass = true;
        progress.EarnedPremiumCurrency = 500;
        
        progress.ResetForNewSeason(2, _season);
        
        Assert.That(progress.CurrentSeason, Is.EqualTo(2));
        Assert.That(progress.CurrentTier, Is.EqualTo(1));
        Assert.That(progress.TotalXpEarned, Is.EqualTo(0));
        Assert.That(progress.HasBattlePass, Is.False); // Reset on new season
        Assert.That(progress.EarnedPremiumCurrency, Is.EqualTo(0)); // Reset
    }
    
    [Test]
    public void TestBattlePassTierRewards()
    {
        var rewards = _battlePass.GetTierRewards(5);
        
        Assert.That(rewards.free, Is.Not.Null);
        Assert.That(rewards.premium, Is.Not.Null);
    }
    
    [Test]
    public void TestBattlePassAvailableRewards()
    {
        var progress = new BattlePassProgress();
        progress.PurchaseBattlePass();
        progress.CurrentTier = 3;
        
        var available = _battlePass.GetAvailableRewards();
        
        // Should have rewards available
        Assert.That(available.Count, Is.GreaterThan(0));
    }
    
    [Test]
    public void TestBattlePassSeasonColors()
    {
        var color = _battlePass.GetSeasonThemeColor();
        
        Assert.That(color.R, Is.InRange(0f, 1f));
        Assert.That(color.G, Is.InRange(0f, 1f));
        Assert.That(color.B, Is.InRange(0f, 1f));
    }
    
    [Test]
    public void TestBattlePassStatistics()
    {
        var stats = _battlePass.GetStatistics();
        
        Assert.That(stats, Does.Contain("Battle Pass Statistics"));
        Assert.That(stats, Does.Contain("Current Tier:"));
        Assert.That(stats, Does.Contain("Season Progress:"));
    }
    
    // ==================== INTEGRATION TESTS ====================
    
    [Test]
    public void TestFullPurchaseFlow()
    {
        // Player has enough coins
        var coins = 2000;
        var progress = new PlayerCosmeticsProgress();
        
        var cosmetic = _database.GetCosmeticById("hat_1");
        Assert.That(cosmetic, Is.Not.Null);
        Assert.That(cosmetic!.PriceCoins, Is.LessThanOrEqualTo(coins));
        
        // Purchase
        if (cosmetic.PriceCoins <= coins)
        {
            progress.AddOwnedCosmetic("hat_1");
        }
        
        // Verify owned
        Assert.That(progress.OwnsCosmetic("hat_1"), Is.True);
    }
    
    [Test]
    public void TestFullEquipFlow()
    {
        var progress = new PlayerCosmeticsProgress();
        
        // Own cosmetic
        progress.AddOwnedCosmetic("hat_1");
        
        // Equip
        progress.EquipCosmetic("Hat", "hat_1");
        
        // Verify equipped
        Assert.That(progress.GetEquippedCosmetic("Hat"), Is.EqualTo("hat_1"));
    }
    
    [Test]
    public void TestBattlePassXpFromLevels()
    {
        var progress = new BattlePassProgress();
        var initialTier = progress.CurrentTier;
        
        // Simulate completing 5 levels
        for (int i = 0; i < 5; i++)
        {
            progress.AddXp(100, _season);
        }
        
        // Should have advanced at least 4-5 tiers
        Assert.That(progress.CurrentTier, Is.GreaterThan(initialTier + 3));
    }
    
    // ==================== HELPER METHODS ====================
    
    private CosmeticItem[] CreateTestCosmetics()
    {
        var cosmetics = new List<CosmeticItem>();
        
        // 5 Hats (Common)
        for (int i = 1; i <= 5; i++)
        {
            cosmetics.Add(new CosmeticItem
            {
                Id = $"hat_{i}",
                DisplayName = $"Test Hat {i}",
                Description = $"A test hat number {i}",
                Rarity = CosmeticRarity.Common,
                Category = CosmeticCategory.Hat,
                PriceCoins = 100 * i,
                UnlockCondition = UnlockCondition.Always,
                SortOrder = i,
                IsActive = true
            });
        }
        
        // 5 Glasses (Rare)
        for (int i = 1; i <= 5; i++)
        {
            cosmetics.Add(new CosmeticItem
            {
                Id = $"glasses_{i}",
                DisplayName = $"Test Glasses {i}",
                Description = $"Test glasses number {i}",
                Rarity = CosmeticRarity.Rare,
                Category = CosmeticCategory.Glasses,
                PriceCoins = 2000,
                UnlockCondition = UnlockCondition.LevelUnlock,
                UnlockRequirement = 3,
                SortOrder = 100 + i,
                IsActive = true
            });
        }
        
        return cosmetics.ToArray();
    }
}
