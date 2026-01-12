# COSMETICS SHOP CONFIGURATION GUIDE
## Customizing the Shop UI and Experience

This guide covers shop layout adjustments, promotional features, and visual customization without code changes.

---

## Table of Contents
1. [Shop Layout Overview](#shop-layout-overview)
2. [Adjusting Grid Layout](#adjusting-grid-layout)
3. [Customizing Filter Tabs](#customizing-filter-tabs)
4. [Promotional Features](#promotional-features)
5. [Price Psychology](#price-psychology)
6. [Visual Styling](#visual-styling)
7. [Creating Bundles](#creating-bundles)

---

## Shop Layout Overview

```
┌─────────────────────────────────────────────────────────┐
│  [Header: Cosmetics Shop]                      [Close]  │
├─────────────────────────────────────────────────────────┤
│  [All][Hats][Glasses][Mustaches][Wigs][New][Limited]   │
├─────────────────────────────────────────────────────────┤
│  [Search cosmetics...]                                    │
├─────────────────────────────────────────────────────────┤
│  Sort: [By Rarity ▼]  [↑]                                │
├─────────────────────────────────────────────────────────┤
│  ┌─────┐ ┌─────┐ ┌─────┐ ┌─────┐ ┌─────┐                │
│  │     │ │     │ │     │ │     │ │     │                │
│  │ IMG │ │ IMG │ │ IMG │ │ IMG │ │ IMG │                │
│  │     │ │     │ │     │ │     │ │     │                │
│  └─────┘ └─────┘ └─────┘ └─────┘ └─────┘                │
│  ┌─────┐ ┌─────┐ ┌─────┐ ┌─────┐ ┌─────┐                │
│  ...                                                    │
├─────────────────────────────────────────────────────────┤
│  [Loadout Panel]                           [Preview]    │
│  [Save][Load]                              [Buy][Equip] │
└─────────────────────────────────────────────────────────┘
```

---

## Adjusting Grid Layout

### Changing Tile Size

1. Open `CosmeticsShop.tscn` scene
2. Select the `CosmeticsShop` root node
3. Find `TileSize` property in Inspector:
   ```
   Default: Vector2(120, 150)
   Larger tiles: Vector2(150, 180)
   Compact tiles: Vector2(100, 130)
   ```

### Changing Items Per Row

1. Open `CosmeticsShop.tscn` scene
2. Find `CosmeticsGrid` node
3. Change `Columns` property:
   ```
   Desktop: 5-6 columns
   Mobile: 3-4 columns
   Tablet: 4-5 columns
   ```

### Adjusting Spacing

1. Select `CosmeticsGrid` node
2. Find `Theme Override > Constants`
3. Adjust:
   ```
   H Separation: 10 (horizontal gap)
   V Separation: 10 (vertical gap)
   ```

---

## Customizing Filter Tabs

### Reordering Tabs

The filter tabs are HBoxContainer children. To reorder:

1. Open `CosmeticsShop.tscn` scene
2. Find `FilterTabs` HBoxContainer
3. Drag and drop to reorder:
   ```
   Recommended order:
   All → Hats → Glasses → Mustaches → Wigs → Emotions → New → Limited
   ```

### Renaming Tabs

1. Expand `FilterTabs`
2. Select each `Button` node
3. Change `Text` property in Inspector

### Adding New Filter

1. Add new Button to `FilterTabs`:
   ```
   Name: FilterTrending
   Text: "Trending"
   Custom Minimum Size: (100, 40)
   ```

2. Connect signal in `CosmeticsShopUI.cs`:
   ```csharp
   // Add to _Ready():
   FilterTrending.Pressed += () => SetCategoryFilter("trending");
   ```

3. Implement filter in `GetShopCosmetics()`:
   ```csharp
   if (_currentCategoryFilter == "trending")
       cosmetics = GetTrendingCosmetics();
   ```

---

## Promotional Features

### Featured Cosmetics Section

To add a "Featured" section at the top:

1. Add new HBoxContainer above the grid
2. Set `Custom Minimum Size` for featured area
3. Populate with `CosmeticsShop.GetFeaturedCosmetics()`

### Limited-Time Badges

Automatic badges appear when:
- `Is Limited Time` is checked in cosmetic
- `Seasonal End Date` is in the future

Badge styling:
```csharp
// In CosmeticsShopUI.cs - CreateCosmeticTile()
var badge = new Label();
badge.Text = "LIMITED!";
badge.Modulate = Colors.Red;
badge.Position = new Vector2(5, 5);
```

### "New" Badge

New cosmetics automatically show "NEW" badge when:
- `Sort Order` >= 1000 (indicates recent addition)
- Or within 7 days of adding to database

---

## Price Psychology

### Recommended Price Points

| Price Point | Psychology | When to Use |
|-------------|------------|-------------|
| Free | No barrier | Common items |
| 100 coins | "Spending money" feel | Entry-level |
| 500 coins | Medium commitment | Uncommon |
| 1000 coins | "Significant" spend | Rare threshold |
| $0.99 | Impulse buy | Premium cosmetics |
| $2.99 | Premium value | Epic cosmetics |
| $4.99 | Substantial purchase | Battle Pass, Legendary |

### Price Display Formatting

| Format | Example | Use Case |
|--------|---------|----------|
| "{N} coins" | "500 coins" | Coin prices |
| "${N}.99" | "$2.99" | USD prices |
| "FREE" | "FREE" | Free items |
| "UNLOCK" | "UNLOCK" | Achievement unlocks |

### Conversion Rate Tips

1. **Anchor high prices** - Show expensive items first
2. **Create tiers** - $0.99 → $2.99 → $4.99
3. **Discount psychology** - "Was $4.99, now $2.99"
4. **Bundle deals** - 3 for 2 discounts

---

## Visual Styling

### Changing Shop Background

1. Select `Background` ColorRect in scene
2. Change `Color` property:
   ```
   Default: Color(0, 0, 0, 0.8) // Dark overlay
   Alternative: Color(0.1, 0.1, 0.2, 0.95)
   ```

### Rarity Color Customization

Edit `CosmeticItem.GetRarityColor()` in code:

```csharp
public Color GetRarityColor()
{
    return Rarity switch
    {
        CosmeticRarity.Common => new Color(0.7f, 0.7f, 0.7f),       // Gray
        CosmeticRarity.Uncommon => new Color(0.3f, 0.8f, 0.3f),     // Green
        CosmeticRarity.Rare => new Color(0.3f, 0.5f, 0.9f),         // Blue
        CosmeticRarity.Epic => new Color(0.6f, 0.3f, 0.8f),         // Purple
        CosmeticRarity.Legendary => new Color(1f, 0.6f, 0f),        // Gold
        _ => Colors.White
    };
}
```

### Button Styling

1. Select purchase/equip buttons
2. Find `Theme Overrides > Styles`
3. Create new StyleBoxFlat:
   ```
   BG Color: Green (purchase) / Blue (equip)
   Corner Radius: 8
   Content Margin: 10px all sides
   ```

### Custom Fonts

1. Select Label nodes
2. Find `Theme Overrides > Font Sizes`
3. Adjust `Font Size`:
   ```
   Title: 32
   Name: 18
   Price: 16
   Rarity: 14
   ```

---

## Creating Bundles

### What Are Bundles?

Bundles sell multiple cosmetics at a discount, increasing average order value.

### Bundle Types

| Type | Discount | Example |
|------|----------|---------|
| Thematic | 15-20% | "Winter Collection" |
| Rarity | 20-25% | "Epic Pack" |
| Complete | 30-40% | "All Glasses" |
| Starter | 25% | "New Player Pack" |

### Implementing Bundles

1. Create bundle definition:
   ```csharp
   public class CosmeticBundle
   {
       public string Name;
       public string[] CosmeticIds;
       public int DiscountPercent;
       public Texture2D? Icon;
   }
   ```

2. Display in shop:
   ```
   Add BundlePanel to shop scene
   Populate with available bundles
   Show discount vs individual prices
   ```

3. Purchase flow:
   ```
   Click bundle → Confirm purchase →
   Grant all cosmetics → Apply discount
   ```

### Example: Winter Bundle

```
Bundle: "Winter Collection"
Cosmetics: ice_crown, snow_glasses, scarf_wig
Individual: 3000 coins
Bundle Price: 2500 coins (17% off)
```

---

## Advanced: Custom Sorting

### Available Sort Options

| Sort Key | Description |
|----------|-------------|
| "rarity" | By rarity tier (ascending) |
| "price" | By coin price (ascending) |
| "name" | Alphabetically |
| "newest" | By sort order (descending) |
| "category" | Group by category |

### Adding Custom Sort

1. Add to Sort Option dropdown in Inspector
2. Handle in `OnSortItemSelected()`:
   ```csharp
   case 4: // "My Custom Sort"
       cosmetics = SortByMyCustomLogic(cosmetics);
       break;
   ```

---

## Troubleshooting

### Grid Not Updating

**Problem**: Changes to cosmetics don't appear

**Solution**: Call `RefreshCosmeticsGrid()` after changes

### Tiles Misaligned

**Problem**: Tiles have wrong spacing/size

**Solution**: 
1. Check `TileSize` property
2. Verify `Columns` count
3. Check H/V Separation values

### Filters Not Working

**Problem**: Category filter shows all items

**Solution**:
1. Verify `SetCategory()` is called
2. Check `GetCosmeticsByCategory()` implementation
3. Ensure cosmetic categories match

### Prices Displaying Wrong

**Problem**: Prices show as "NaN" or wrong format

**Solution**:
1. Check both PriceCoins and PriceUSD are valid
2. Verify `GetPriceString()` formatting
3. Ensure default values are set

---

## Performance Tips

### Object Pooling

For large cosmetic catalogs:
1. Pre-create cosmetic tile scenes
2. Reuse tiles when scrolling
3. Only update changed content

### Lazy Loading

```csharp
// Load textures only when visible
private void OnTileBecameVisible(CosmeticItem cosmetic)
{
    if (!string.IsNullOrEmpty(cosmetic.AssetPath))
    {
        textureRect.Texture = ResourceLoader.Load<Texture2D>(cosmetic.AssetPath);
    }
}
```

---

## Next Steps

After configuring your shop:
1. Test on target devices (mobile vs desktop)
2. A/B test different layouts
3. Monitor conversion metrics
4. Iterate based on player feedback

For analytics and monetization strategy, see:
- [MONETIZATION_STRATEGY_GUIDE.md](MONETIZATION_STRATEGY_GUIDE.md)
- [BATTLE_PASS_SETUP_GUIDE.md](BATTLE_PASS_SETUP_GUIDE.md)
