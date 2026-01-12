using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Newtonsoft.Json;

/// <summary>
/// Player's cosmetics progress - owned cosmetics and loadouts.
/// Persisted in player profile.
/// </summary>
[Serializable]
public class PlayerCosmeticsProgress
{
    /// <summary>
    /// Set of owned cosmetic IDs.
    /// </summary>
    [JsonProperty("owned_cosmetics")]
    public HashSet<string> OwnedCosmetics { get; set; } = new();
    
    /// <summary>
    /// Currently equipped cosmetics by category.
    /// </summary>
    [JsonProperty("equipped_cosmetics")]
    public Dictionary<string, string> EquippedCosmetics { get; set; } = new();
    
    /// <summary>
    /// Saved loadouts (up to 10).
    /// </summary>
    [JsonProperty("loadouts")]
    public List<CosmeticLoadout> Loadouts { get; set; } = new();
    
    /// <summary>
    /// Currently active loadout index (-1 = no loadout).
    /// </summary>
    [JsonProperty("active_loadout_index")]
    public int ActiveLoadoutIndex { get; set; } = -1;
    
    /// <summary>
    /// Recently viewed cosmetics (for "Recently Added" filter).
    /// </summary>
    [JsonProperty("recent_cosmetics")]
    public List<string> RecentCosmetics { get; set; } = new();
    
    /// <summary>
    /// Favorite cosmetic IDs.
    /// </summary>
    [JsonProperty("favorite_cosmetics")]
    public HashSet<string> FavoriteCosmetics { get; set; } = new();
    
    /// <summary>
    /// Check if player owns a specific cosmetic.
    /// </summary>
    public bool OwnsCosmetic(string cosmeticId)
    {
        return OwnedCosmetics.Contains(cosmeticId);
    }
    
    /// <summary>
    /// Check if a cosmetic is equipped.
    /// </summary>
    public bool IsCosmeticEquipped(string cosmeticId)
    {
        return EquippedCosmetics.Values.Contains(cosmeticId);
    }
    
    /// <summary>
    /// Get the equipped cosmetic for a category.
    /// </summary>
    public string? GetEquippedCosmetic(string category)
    {
        return EquippedCosmetics.TryGetValue(category, out var cosmetic) ? cosmetic : null;
    }
    
    /// <summary>
    /// Equip a cosmetic.
    /// </summary>
    public void EquipCosmetic(string category, string cosmeticId)
    {
        EquippedCosmetics[category] = cosmeticId;
    }
    
    /// <summary>
    /// Unequip a cosmetic from a category.
    /// </summary>
    public void UnequipCosmetic(string category)
    {
        EquippedCosmetics.Remove(category);
    }
    
    /// <summary>
    /// Add a cosmetic to owned collection.
    /// </summary>
    public void AddOwnedCosmetic(string cosmeticId)
    {
        OwnedCosmetics.Add(cosmeticId);
    }
    
    /// <summary>
    /// Create a new loadout from currently equipped cosmetics.
    /// </summary>
    public CosmeticLoadout CreateLoadout(string name)
    {
        if (Loadouts.Count >= 10)
        {
            GD.PushWarning("Maximum loadouts reached (10)");
            return null;
        }
        
        var loadout = new CosmeticLoadout
        {
            Name = name,
            EquippedCosmetics = new Dictionary<string, string>(EquippedCosmetics),
            CreatedAt = DateTime.Now
        };
        
        Loadouts.Add(loadout);
        return loadout;
    }
    
    /// <summary>
    /// Apply a saved loadout.
    /// </summary>
    public void ApplyLoadout(int loadoutIndex)
    {
        if (loadoutIndex < 0 || loadoutIndex >= Loadouts.Count)
            return;
            
        var loadout = Loadouts[loadoutIndex];
        foreach (var kvp in loadout.EquippedCosmetics)
        {
            // Only equip if player owns the cosmetic
            if (OwnsCosmetic(kvp.Value))
            {
                EquippedCosmetics[kvp.Key] = kvp.Value;
            }
        }
        ActiveLoadoutIndex = loadoutIndex;
    }
    
    /// <summary>
    /// Delete a loadout.
    /// </summary>
    public void DeleteLoadout(int loadoutIndex)
    {
        if (loadoutIndex < 0 || loadoutIndex >= Loadouts.Count)
            return;
            
        Loadouts.RemoveAt(loadoutIndex);
        if (ActiveLoadoutIndex == loadoutIndex)
            ActiveLoadoutIndex = -1;
        else if (ActiveLoadoutIndex > loadoutIndex)
            ActiveLoadoutIndex--;
    }
    
    /// <summary>
    /// Add a cosmetic to recent list.
    /// </summary>
    public void AddRecentCosmetic(string cosmeticId)
    {
        RecentCosmetics.Remove(cosmeticId);
        RecentCosmetics.Insert(0, cosmeticId);
        
        // Keep only last 20
        while (RecentCosmetics.Count > 20)
        {
            RecentCosmetics.RemoveAt(RecentCosmetics.Count - 1);
        }
    }
    
    /// <summary>
    /// Toggle favorite status.
    /// </summary>
    public void ToggleFavorite(string cosmeticId)
    {
        if (FavoriteCosmetics.Contains(cosmeticId))
            FavoriteCosmetics.Remove(cosmeticId);
        else
            FavoriteCosmetics.Add(cosmeticId);
    }
    
    /// <summary>
    /// Get count of owned cosmetics by rarity.
    /// </summary>
    public Dictionary<CosmeticRarity, int> GetOwnedCountByRarity(CosmeticsDatabase database)
    {
        var counts = new Dictionary<CosmeticRarity, int>();
        foreach (var cosmeticId in OwnedCosmetics)
        {
            var cosmetic = database.GetCosmeticById(cosmeticId);
            if (cosmetic != null)
            {
                if (!counts.ContainsKey(cosmetic.Rarity))
                    counts[cosmetic.Rarity] = 0;
                counts[cosmetic.Rarity]++;
            }
        }
        return counts;
    }
}

/// <summary>
/// Represents a saved outfit/loadout of cosmetics.
/// </summary>
[Serializable]
public class CosmeticLoadout
{
    /// <summary>
    /// Unique name for the loadout.
    /// </summary>
    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// When this loadout was created.
    /// </summary>
    [JsonProperty("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    
    /// <summary>
    /// Cosmetics in this loadout by category.
    /// </summary>
    [JsonProperty("equipped_cosmetics")]
    public Dictionary<string, string> EquippedCosmetics { get; set; } = new();
    
    /// <summary>
    /// Optional icon/path for preview.
    /// </summary>
    [JsonProperty("icon_path")]
    public string IconPath { get; set; } = string.Empty;
}
