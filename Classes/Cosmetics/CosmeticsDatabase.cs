using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Godot;
using Newtonsoft.Json;

/// <summary>
/// Inspector-editable database of all cosmetics in the game.
/// Configure cosmetics in the Godot Editor without writing code.
/// </summary>
[CreateAssetMenu(fileName = "CosmeticsDatabase", menuName = "Cosmetics/Database")]
public class CosmeticsDatabase : Resource
{
    [Header("Database Info")]
    [Export] public string DatabaseName = "Cosmetics Database";
    [Export] public int DatabaseVersion = 1;
    [Export] public string LastModified = "";
    
    [Header("Cosmetics List")]
    [Export] public CosmeticItem[] Cosmetics = Array.Empty<CosmeticItem>();
    
    [Header("Shop Settings")]
    [Export] public int DefaultItemsPerPage = 20;
    [Export] public bool EnableLimitedTimeCosmetics = true;
    [Export] public int FeaturedCosmeticsCount = 6;
    
    /// <summary>
    /// Get all active cosmetics.
    /// </summary>
    public CosmeticItem[] GetAllCosmetics()
    {
        return Cosmetics.Where(c => c.IsActive).ToArray();
    }
    
    /// <summary>
    /// Get cosmetics by category.
    /// </summary>
    public CosmeticItem[] GetCosmeticsByCategory(CosmeticCategory category)
    {
        return GetAllCosmetics().Where(c => c.Category == category).ToArray();
    }
    
    /// <summary>
    /// Get cosmetics by rarity.
    /// </summary>
    public CosmeticItem[] GetCosmeticsByRarity(CosmeticRarity rarity)
    {
        return GetAllCosmetics().Where(c => c.Rarity == rarity).ToArray();
    }
    
    /// <summary>
    /// Get a cosmetic by its ID.
    /// </summary>
    public CosmeticItem? GetCosmeticById(string id)
    {
        return GetAllCosmetics().FirstOrDefault(c => c.Id == id);
    }
    
    /// <summary>
    /// Get featured cosmetics for the shop homepage.
    /// </summary>
    public CosmeticItem[] GetFeaturedCosmetics()
    {
        return GetAllCosmetics()
            .OrderByDescending(c => c.Rarity)
            .Take(FeaturedCosmeticsCount)
            .ToArray();
    }
    
    /// <summary>
    /// Get new cosmetics (added in last 7 days).
    /// </summary>
    public CosmeticItem[] GetNewCosmetics()
    {
        var oneWeekAgo = DateTime.Now.AddDays(-7);
        return GetAllCosmetics()
            .Where(c => c.SortOrder >= 1000) // New items have high sort order
            .ToArray();
    }
    
    /// <summary>
    /// Get limited-time cosmetics currently available.
    /// </summary>
    public CosmeticItem[] GetLimitedTimeCosmetics()
    {
        if (!EnableLimitedTimeCosmetics)
            return Array.Empty<CosmeticItem>();
            
        return GetAllCosmetics()
            .Where(c => c.IsLimitedTime)
            .Where(c => 
            {
                if (string.IsNullOrEmpty(c.SeasonalEndDate))
                    return true;
                    
                if (DateTime.TryParse(c.SeasonalEndDate, out var endDate))
                    return endDate > DateTime.Now;
                    
                return true;
            })
            .ToArray();
    }
    
    /// <summary>
    /// Get seasonal cosmetics for a specific season.
    /// </summary>
    public CosmeticItem[] GetSeasonalCosmetics(int seasonNumber)
    {
        return GetAllCosmetics()
            .Where(c => c.SeasonNumber == seasonNumber)
            .ToArray();
    }
    
    /// <summary>
    /// Get cosmetics that can be unlocked at a specific battle pass tier.
    /// </summary>
    public CosmeticItem[] GetBattlePassTierCosmetics(int tier)
    {
        return GetAllCosmetics()
            .Where(c => c.UnlockCondition == UnlockCondition.BattlePassTier && 
                       c.UnlockRequirement == tier)
            .ToArray();
    }
    
    /// <summary>
    /// Get all cosmetics that require a specific unlock condition.
    /// </summary>
    public CosmeticItem[] GetCosmeticsByUnlockCondition(UnlockCondition condition)
    {
        return GetAllCosmetics().Where(c => c.UnlockCondition == condition).ToArray();
    }
    
    /// <summary>
    /// Get free cosmetics (no cost).
    /// </summary>
    public CosmeticItem[] GetFreeCosmetics()
    {
        return GetAllCosmetics().Where(c => c.IsFree()).ToArray();
    }
    
    /// <summary>
    /// Get purchasable cosmetics (can be bought with coins).
    /// </summary>
    public CosmeticItem[] GetPurchasableCosmetics()
    {
        return GetAllCosmetics()
            .Where(c => c.PriceCoins > 0 && !c.IsPremiumExclusive())
            .ToArray();
    }
    
    /// <summary>
    /// Get IAP-only cosmetics.
    /// </summary>
    public CosmeticItem[] GetIAPCosmetics()
    {
        return GetAllCosmetics()
            .Where(c => c.UnlockCondition == UnlockCondition.IAP || 
                       (c.PriceUsd > 0 && c.UnlockCondition == UnlockCondition.Always))
            .ToArray();
    }
    
    /// <summary>
    /// Get the count of cosmetics by rarity.
    /// </summary>
    public Dictionary<CosmeticRarity, int> GetCosmeticCountByRarity()
    {
        return GetAllCosmetics()
            .GroupBy(c => c.Rarity)
            .ToDictionary(g => g.Key, g => g.Count());
    }
    
    /// <summary>
    /// Get the total count of cosmetics.
    /// </summary>
    public int GetTotalCosmeticCount()
    {
        return GetAllCosmetics().Length;
    }
    
    /// <summary>
    /// Search cosmetics by name or description.
    /// </summary>
    public CosmeticItem[] SearchCosmetics(string searchTerm)
    {
        var term = searchTerm.ToLowerInvariant();
        return GetAllCosmetics()
            .Where(c => c.DisplayName.ToLowerInvariant().Contains(term) ||
                       c.Description.ToLowerInvariant().Contains(term))
            .ToArray();
    }
    
    /// <summary>
    /// Sort cosmetics by the specified criteria.
    /// </summary>
    public CosmeticItem[] SortCosmetics(CosmeticItem[] cosmetics, string sortBy, bool ascending = true)
    {
        var sorted = sortBy.ToLowerInvariant() switch
        {
            "rarity" => cosmetics.OrderBy(c => c.Rarity),
            "price" => cosmetics.OrderBy(c => c.PriceCoins),
            "name" => cosmetics.OrderBy(c => c.DisplayName),
            "newest" => cosmetics.OrderByDescending(c => c.SortOrder),
            "category" => cosmetics.OrderBy(c => c.Category),
            _ => cosmetics.OrderBy(c => c.SortOrder)
        };
        
        return ascending ? sorted.ToArray() : sorted.Reverse().ToArray();
    }
    
    /// <summary>
    /// Export the database to JSON for version control.
    /// </summary>
    public string ExportToJson()
    {
        var exportData = new
        {
            database_name = DatabaseName,
            version = DatabaseVersion,
            last_modified = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            cosmetics_count = Cosmetics.Length,
            cosmetics = Cosmetics
        };
        
        return JsonConvert.SerializeObject(exportData, Formatting.Indented);
    }
    
    /// <summary>
    /// Export database to a file.
    /// </summary>
    public void ExportToFile(string filePath)
    {
        var json = ExportToJson();
        File.WriteAllText(filePath, json);
        GD.Print($"Cosmetics database exported to {filePath}");
    }
    
    /// <summary>
    /// Import cosmetics from JSON data.
    /// </summary>
    public void ImportFromJson(string json)
    {
        try
        {
            var importData = Newtonsoft.Json.Linq.JObject.Parse(json);
            var cosmeticsArray = importData["cosmetics"] as Newtonsoft.Json.Linq.JArray;
            
            if (cosmeticsArray != null)
            {
                var importedCosmetics = cosmeticsArray.ToObject<CosmeticItem[]>();
                if (importedCosmetics != null)
                {
                    Cosmetics = importedCosmetics;
                    LastModified = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    GD.Print($"Imported {importedCosmetics.Length} cosmetics from JSON");
                }
            }
        }
        catch (Exception ex)
        {
            GD.PushError($"Failed to import cosmetics: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Import database from a file.
    /// </summary>
    public void ImportFromFile(string filePath)
    {
        if (File.Exists(filePath))
        {
            var json = File.ReadAllText(filePath);
            ImportFromJson(json);
        }
        else
        {
            GD.PushError($"Cosmetics database file not found: {filePath}");
        }
    }
    
    /// <summary>
    /// Add a new cosmetic to the database.
    /// </summary>
    public void AddCosmetic(CosmeticItem cosmetic)
    {
        var list = Cosmetics.ToList();
        cosmetic.Id = GenerateUniqueId(cosmetic.DisplayName);
        list.Add(cosmetic);
        Cosmetics = list.ToArray();
        LastModified = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
    }
    
    /// <summary>
    /// Remove a cosmetic from the database.
    /// </summary>
    public void RemoveCosmetic(string cosmeticId)
    {
        Cosmetics = Cosmetics.Where(c => c.Id != cosmeticId).ToArray();
        LastModified = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
    }
    
    /// <summary>
    /// Update an existing cosmetic.
    /// </summary>
    public void UpdateCosmetic(CosmeticItem updatedCosmetic)
    {
        var index = Array.FindIndex(Cosmetics, c => c.Id == updatedCosmetic.Id);
        if (index >= 0)
        {
            Cosmetics[index] = updatedCosmetic;
            LastModified = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }
    }
    
    /// <summary>
    /// Generate a unique ID for a new cosmetic.
    /// </summary>
    private string GenerateUniqueId(string displayName)
    {
        var baseId = displayName.ToLower().Replace(" ", "_").Replace("-", "_");
        var id = baseId;
        var counter = 1;
        
        while (GetCosmeticById(id) != null)
        {
            id = $"{baseId}_{counter}";
            counter++;
        }
        
        return id;
    }
    
    /// <summary>
    /// Validate the database for common issues.
    /// </summary>
    public string ValidateDatabase()
    {
        var issues = new List<string>();
        var usedIds = new HashSet<string>();
        
        for (int i = 0; i < Cosmetics.Length; i++)
        {
            var cosmetic = Cosmetics[i];
            
            // Check for duplicate IDs
            if (!string.IsNullOrEmpty(cosmetic.Id))
            {
                if (usedIds.Contains(cosmetic.Id))
                {
                    issues.Add($"Duplicate ID found: {cosmetic.Id}");
                }
                usedIds.Add(cosmetic.Id);
            }
            else
            {
                issues.Add($"Missing ID at index {i}: {cosmetic.DisplayName}");
            }
            
            // Check for empty names
            if (string.IsNullOrEmpty(cosmetic.DisplayName))
            {
                issues.Add($"Empty display name at index {i}");
            }
            
            // Check for invalid prices
            if (cosmetic.PriceCoins < 0)
            {
                issues.Add($"Negative coin price for {cosmetic.Id}");
            }
            
            if (cosmetic.PriceUsd < 0)
            {
                issues.Add($"Negative USD price for {cosmetic.Id}");
            }
        }
        
        if (issues.Count == 0)
        {
            return "Database validation passed!";
        }
        
        return string.Join("\n", issues);
    }
    
    /// <summary>
    /// Get a statistics summary of the database.
    /// </summary>
    public string GetStatisticsSummary()
    {
        var byRarity = GetCosmeticCountByRarity();
        var sb = new System.Text.StringBuilder();
        
        sb.AppendLine("=== Cosmetics Database Statistics ===");
        sb.AppendLine($"Total Cosmetics: {GetTotalCosmeticCount()}");
        sb.AppendLine($"Last Modified: {LastModified}");
        sb.AppendLine();
        
        sb.AppendLine("By Rarity:");
        foreach (var rarity in Enum.GetValues<CosmeticRarity>())
        {
            var count = byRarity.TryGetValue(rarity, out var c) ? c : 0;
            sb.AppendLine($"  {rarity}: {count}");
        }
        
        sb.AppendLine();
        sb.AppendLine("By Unlock Condition:");
        foreach (var condition in Enum.GetValues<UnlockCondition>())
        {
            var count = GetCosmeticsByUnlockCondition(condition).Length;
            sb.AppendLine($"  {condition}: {count}");
        }
        
        return sb.ToString();
    }
}
