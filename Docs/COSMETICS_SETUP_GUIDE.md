# COSMETICS SETUP GUIDE
## Adding and Managing Cosmetics Without Code

This guide walks you through adding new cosmetics to the game using the Godot Editor Inspector. No coding required!

---

## Table of Contents
1. [Quick Start (2-minute workflow)](#quick-start-2-minute-workflow)
2. [Understanding Cosmetic Properties](#understanding-cosmetic-properties)
3. [Adding a New Cosmetic](#adding-a-new-cosmetic)
4. [Bulk Import/Export](#bulk-importexport)
5. [Setting Up Rarity Tiers](#setting-up-rarity-tiers)
6. [Managing Unlock Conditions](#managing-unlock-conditions)
7. [Seasonal Cosmetics](#seasonal-cosmetics)
8. [Troubleshooting](#troubleshooting)

---

## Quick Start (2-minute workflow)

### Step 1: Create your asset
Place your cosmetic sprite in the `Assets/Cosmetics/` folder. Recommended size: 256x256 pixels, PNG format with transparency.

### Step 2: Select the database
1. In the Godot Editor, find the `CosmeticsDatabase` resource in the FileSystem
2. Click on it to select it in the Inspector

### Step 3: Add new cosmetic
1. In the Inspector, find the `Cosmetics` array
2. Click the dropdown arrow
3. Click "New Element" to add a new cosmetic
4. Fill in the properties (see below)

### Step 4: Configure and save
1. Set the Display Name, Rarity, Category, and Price
2. Drag your sprite to the Asset Path field
3. Save the scene (Ctrl+S)

**Done!** Your cosmetic is now in the game.

---

## Understanding Cosmetic Properties

### Basic Properties

| Property | Description | Example |
|----------|-------------|---------|
| **ID** | Unique identifier (auto-generated) | `cool_hat_001` |
| **Display Name** | What players see | "Cool Cowboy Hat" |
| **Description** | Flavor text shown in shop | "A stylish hat for adventures" |
| **Rarity** | Quality tier | Common, Uncommon, Rare, Epic, Legendary |
| **Category** | Where it appears in shop | Hat, Glasses, Mustache, Wig, Emotion |
| **Price (Coins)** | Cost in in-game currency | 0 = free, 500 = 500 coins |
| **Price (USD)** | Real money cost | 0.99 = 99 cents |
| **Asset Path** | Path to sprite file | `res://Assets/Cosmetics/hat_cowboy.png` |

### Rarity Color Guide

| Rarity | Color (RGB) | Typical Price |
|--------|-------------|---------------|
| Common | Gray (180,180,180) | Free or 100 coins |
| Uncommon | Green (80,200,80) | 500-1000 coins |
| Rare | Blue (80,130,230) | 1500-2500 coins |
| Epic | Purple (150,80,200) | 3000-5000 coins |
| Legendary | Gold (255,150,0) | $2.99-$4.99 |

---

## Adding a New Cosmetic

### Step-by-step example: Adding a "Ninja Headband"

1. **Create the asset**
   - Draw or import your headband sprite
   - Save as `Assets/Cosmetics/headband_ninja.png`

2. **Open the database**
   ```
   FileSystem → Resources → CosmeticsDatabase
   ```

3. **Add new cosmetic**
   - Click the `Cosmetics` array in Inspector
   - Click "Add New Element" at the bottom
   
4. **Configure properties**
   ```
   Display Name: "Ninja Headband"
   Description: "For stealthy operations"
   Rarity: Rare (dropdown)
   Category: Hat (dropdown)
   Price Coins: 2000
   Price USD: 0
   Asset Path: res://Assets/Cosmetics/headband_ninja.png
   Unlock Condition: Always
   ```

5. **Save**
   - Press Ctrl+S to save the database

### Tips for Good Shop Presentation

- **Display Name**: Keep under 25 characters
- **Description**: 1-2 sentences, engaging
- **Asset**: Use consistent size for all cosmetics
- **Price**: Match the rarity guidelines above

---

## Bulk Import/Export

### Exporting Cosmetics (for version control)

1. Select `CosmeticsDatabase` in Inspector
2. Look for `Export` button in the toolbar
3. Click "Export to JSON"
4. Save to `Assets/Cosmetics/cosmetics_export.json`

### Importing Cosmetics (from JSON)

1. Select `CosmeticsDatabase` in Inspector
2. Look for `Import` button in the toolbar
3. Select your JSON file
4. Click "Import"

### Manual Bulk Editing (Advanced)

You can edit the JSON directly for bulk changes:

```json
{
  "cosmetics": [
    {
      "display_name": "Hat 1",
      "rarity": "Common",
      "price_coins": 100,
      "category": "Hat"
    },
    {
      "display_name": "Hat 2", 
      "rarity": "Common",
      "price_coins": 100,
      "category": "Hat"
    }
  ]
}
```

---

## Setting Up Rarity Tiers

### How Rarity Affects Gameplay

1. **Visual distinction** - Rarity colors in shop
2. **Price scaling** - Higher rarity = higher prices
3. **Exclusivity** - Rare cosmetics feel more valuable
4. **Progression goals** - Players aim for better rarities

### Recommended Distribution

| Rarity | Count | % of Total | Price Range |
|--------|-------|------------|-------------|
| Common | 50+ | 50% | Free-500 coins |
| Uncommon | 30+ | 30% | 500-1500 coins |
| Rare | 20+ | 15% | 1500-3000 coins |
| Epic | 10+ | 5% | 3000-5000 coins |
| Legendary | 5+ | <1% | $2.99-$4.99 |

### Changing Rarity for Existing Cosmetics

1. Find the cosmetic in `Cosmetics` array
2. Change the `Rarity` dropdown
3. Save the database
4. Colors update automatically in shop

---

## Managing Unlock Conditions

### Available Unlock Conditions

| Condition | Description | When Available |
|-----------|-------------|----------------|
| **Always** | Available from start | Immediately |
| **Level Unlock** | Requires X stars | After completing X levels |
| **Perfect Score** | 3-star all levels | After mastery |
| **Battle Pass Tier** | BP Tier X reward | When reaching tier |
| **Seasonal** | Limited time only | During event |
| **IAP** | Real money purchase | Always |
| **Achievement** | Achievement reward | When unlocked |

### Setting Up Level Unlock Cosmetics

1. Set `Unlock Condition` to "Level Unlock"
2. Set `Unlock Requirement` to the star level needed (e.g., 3 for 3-star levels)
3. Players will see the cosmetic but can't buy it until they meet the requirement

### Setting Up Battle Pass Rewards

1. Set `Unlock Condition` to "Battle Pass Tier"
2. Set `Unlock Requirement` to the tier number
3. Cosmetic automatically unlocks when player reaches that tier

---

## Seasonal Cosmetics

### What Are Seasonal Cosmetics?

Limited-time items that create urgency and FOMO (fear of missing out). They disappear after the season ends.

### Creating a Seasonal Cosmetic

1. Add new cosmetic to database
2. Set properties:
   ```
   Unlock Condition: Seasonal
   Is Limited Time: Checked
   Season Number: 1 (for Season 1)
   Seasonal End Date: 2024-02-28
   ```
3. Cosmetic automatically appears during season
4. Shows "Limited Time!" badge in shop

### Seasonal Planning Template

| Season | Theme | Months | Cosmetic Count |
|--------|-------|--------|----------------|
| 1 | Ice/Winter | Jan-Mar | 10 |
| 2 | Fire/Summer | Apr-Jun | 10 |
| 3 | Nature/Spring | Jul-Sep | 10 |
| 4 | Dark/Autumn | Oct-Dec | 10 |

---

## Troubleshooting

### Cosmetic Not Showing in Shop

**Problem**: Added a cosmetic but it doesn't appear

**Solutions**:
1. Check `Is Active` is checked
2. Verify `Asset Path` is correct
3. Ensure `Price Coins` is not negative
4. Restart the editor if it still doesn't show

### Wrong Category

**Problem**: Cosmetic appears in wrong category

**Solution**: Check the `Category` property dropdown is set correctly

### Price Not Displaying

**Problem**: Price shows as "NaN" or empty

**Solution**: Make sure either `Price Coins` or `Price USD` has a value (not both zero)

### Asset Not Loading

**Problem**: Cosmetic shows as blank/empty

**Solutions**:
1. Verify file exists at the asset path
2. Check file is a valid image (PNG recommended)
3. Try re-importing the asset

### Rarity Colors Wrong

**Problem**: Colors don't match rarity

**Solution**: Colors are automatic - just set the rarity, color follows

---

## Quick Reference: Keyboard Shortcuts

| Action | Shortcut |
|--------|----------|
| Save Database | Ctrl+S |
| Undo | Ctrl+Z |
| Redo | Ctrl+Y |
| Search in list | Ctrl+F |
| Duplicate item | Ctrl+D |

---

## Next Steps

After adding cosmetics:
1. Test purchasing in the shop
2. Verify unlock conditions work
3. Check analytics tracking
4. Consider promotional events

For shop UI customization, see [COSMETICS_SHOP_CONFIGURATION_GUIDE.md](COSMETICS_SHOP_CONFIGURATION_GUIDE.md)
