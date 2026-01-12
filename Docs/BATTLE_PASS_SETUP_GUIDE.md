# BATTLE PASS SETUP GUIDE
## Creating and Managing Battle Pass Seasons Without Code

This guide covers creating battle pass seasons, configuring rewards, and managing seasonal progression using the Inspector.

---

## Table of Contents
1. [Quick Start](#quick-start)
2. [Understanding Battle Pass Structure](#understanding-battle-pass-structure)
3. [Creating a New Season](#creating-a-new-season)
4. [Configuring Tier Rewards](#configuring-tier-rewards)
5. [Setting Up Cosmetics for Battle Pass](#setting-up-cosmetics-for-battle-pass)
6. [Pricing and Economics](#pricing-and-economics)
7. [Season Duration and Scheduling](#season-duration-and-scheduling)
8. [Managing Multiple Seasons](#managing-multiple-seasons)
9. [Troubleshooting](#troubleshooting)

---

## Quick Start

### Creating Your First Season (5 minutes)

1. **Create Season Resource**
   ```
   Right-click in FileSystem → New Resource → BattlePassSeason
   Name: BattlePassSeason_S1
   ```

2. **Configure Basic Info**
   ```
   Season Number: 1
   Season Name: "Season 1: Ice Theme"
   Season Description: "Cool down with icy cosmetics!"
   Theme Color: #3498db (blue)
   ```

3. **Set Duration**
   ```
   Start Date: [Today]
   Duration Days: 28
   ```

4. **Configure Pricing**
   ```
   Battle Pass Price: $4.99
   Currency Code: USD
   ```

5. **Generate Rewards**
   - Click "Generate Default Tiers" button
   - Review rewards in the Tiers array

6. **Save and Test**
   - Save the resource
   - Test in game

---

## Understanding Battle Pass Structure

### Season Components

```
Season (28 days)
├── Free Track (20 tiers)
│   ├── Tier 1-5: Coins rewards
│   ├── Tier 6-15: Mixed rewards
│   └── Tier 16-20: Big coin rewards
│
└── Premium Track (30 tiers, requires purchase)
    ├── Tier 1-5: Bonus coins
    ├── Tier 10: First exclusive cosmetic
    ├── Tier 15: More coins
    ├── Tier 20: Exclusive cosmetic
    ├── Tier 25: Premium currency
    ├── Tier 30: Ultimate reward
    └── Tiers 6-9, 11-14, 16-19, 21-24, 26-29: Premium coins
```

### Tier Configuration

Each tier has:
- **Tier Number**: 1-30
- **XP Required**: How much XP to reach this tier (auto-calculated)
- **Free Reward**: What free players get
- **Premium Reward**: What battle pass owners get

### Default XP Scaling

| Tier Range | XP Per Tier |
|------------|-------------|
| 1-5 | 100 |
| 6-10 | 110 |
| 11-15 | 120 |
| 16-20 | 130 |
| 21-25 | 140 |
| 26-30 | 150 |

---

## Creating a New Season

### Step-by-Step: Season 2 (Fire Theme)

1. **Duplicate Existing Season**
   ```
   Right-click BattlePassSeason_S1 → Duplicate
   Rename to: BattlePassSeason_S2
   ```

2. **Update Season Info**
   ```
   Season Number: 2
   Season Name: "Season 2: Fire Theme"
   Season Description: "Heat up with fiery cosmetics!"
   Theme Color: #e74c3c (red)
   ```

3. **Set New Dates**
   ```
   Start Date: [End of Season 1]
   Duration Days: 28
   ```

4. **Configure Premium Rewards**
   ```
   Go to each Premium Reward in Tiers array
   Update cosmetic IDs to new Season 2 cosmetics
   ```

5. **Save**

### Example: Custom Free Reward

```csharp
// In the Inspector for Tier 5 Free Reward:
Type: Coins
Reward ID: season2_tier5_coins
Amount: 500
Display Name: "500 Coins"
Description: "Flame fuel for your adventures!"
```

### Example: Custom Premium Reward (Exclusive Cosmetic)

```csharp
// In the Inspector for Tier 10 Premium Reward:
Type: Cosmetic
Reward ID: fire_crown_001
Display Name: "Fire Crown"
Description: "A crown of eternal flames!"
Icon Path: res://Assets/Cosmetics/fire_crown.png
```

---

## Configuring Tier Rewards

### Reward Types Available

| Type | Description | Fields |
|------|-------------|--------|
| **Cosmetic** | Unlock a cosmetic | ID, Display Name, Icon |
| **Coins** | Award in-game currency | Amount |
| **Premium Currency** | Award premium coins | Amount |
| **XP Bonus** | Multiplier for future XP | Multiplier value |
| **Title** | Player title reward | Title string |
| **Profile Frame** | Profile border reward | Frame ID |

### Setting Free Rewards

1. Expand the `Tiers` array
2. Find the tier you want to modify
3. Expand `Free Reward`
4. Set `Type` to desired reward type
5. Fill in the required fields

### Setting Premium Rewards

1. Expand the `Tiers` array
2. Find the tier you want to modify
3. Expand `Premium Reward`
4. Set `Type` to desired reward type
5. Fill in the required fields

### Recommended Free Track

| Tiers | Rewards | Purpose |
|-------|---------|---------|
| 1-5 | 100-500 coins each | Early engagement |
| 6-10 | 200 coins + 2x XP | Variety |
| 11-15 | 300 coins + XP boost | Mid-game value |
| 16-20 | 400-500 coins each | Late game push |

### Recommended Premium Track

| Tiers | Rewards | Purpose |
|-------|---------|---------|
| 1-5 | Bonus coins | Immediate value |
| 6-9 | Premium currency | Small currency |
| 10 | **EXCLUSIVE COSMETIC** | Big reward |
| 11-14 | Premium currency | Keep rewarding |
| 15 | Coins + premium | Mid-season bonus |
| 16-19 | Premium currency | Consistency |
| 20 | **EXCLUSIVE COSMETIC** | Major milestone |
| 21-24 | Premium currency | Build up |
| 25 | Big premium amount | Mid-late value |
| 26-29 | Premium currency | Finish strong |
| 30 | **ULTIMATE COSMETIC** | Grand finale |

---

## Setting Up Cosmetics for Battle Pass

### Step 1: Create Seasonal Cosmetics

Create new cosmetics in `CosmeticsDatabase` with:
```
Unlock Condition: Battle Pass Tier
Season Number: [Season #]
```

### Step 2: Assign to Battle Pass Rewards

1. Open your `BattlePassSeason` resource
2. Find the tier where you want the cosmetic as reward
3. Set the Premium Reward:
   ```
   Type: Cosmetic
   Reward ID: [Cosmetic ID from CosmeticsDatabase]
   Display Name: [Cosmetic Name]
   Description: [Cosmetic Description]
   ```

### Example: Season 1 Ice Cosmetics

**Cosmetic Database Entry:**
```
ID: ice_crown_001
Display Name: "Ice Crown"
Rarity: Epic
Unlock Condition: Battle Pass Tier
Unlock Requirement: 10
Season Number: 1
```

**Battle Pass Tier 10 Premium Reward:**
```
Type: Cosmetic
Reward ID: ice_crown_001
Display Name: "Ice Crown"
Description: "A crown forged from eternal ice!"
```

---

## Pricing and Economics

### Recommended Price Points

| Price | Conversion Impact | Best For |
|-------|-------------------|----------|
| $2.99 | Lower barrier, higher volume | Launch/first seasons |
| $4.99 | Standard, good conversion | Established game |
| $7.99 | Premium positioning | Whales focus |
| $9.99 | High-value exclusive | VIP bundles |

### Price Psychology Tips

1. **$4.99 > $4.99** - $4.99 feels like a "treat" not an investment
2. **End-of-season discount** - Consider $2.99 for last week
3. **Bundle with coins** - "Battle Pass + 1000 coins" increases perceived value

### Currency Economics

| Earn from BP | Approximate Value |
|--------------|-------------------|
| 500 Premium Coins | ~$5.00 |
| 10 Exclusive Cosmetics | ~$30.00 value |
| Total BP Value | ~$40.00 |

This makes $4.99 feel like a great deal!

---

## Season Duration and Scheduling

### Fixed 28-Day Season

```
Week 1: Early engagement, easy rewards
Week 2: Challenge ramps up
Week 3: FOMO kicks in, push to finish
Week 4: Final push, last chances
```

### Scheduling Multiple Seasons

**Recommended Pattern:**
- Season 1: Jan 1 - Jan 28
- Season 2: Feb 1 - Feb 28
- Season 3: Mar 1 - Mar 28
- Season 4: Apr 1 - Apr 28

This creates a rhythm players can anticipate!

### Extending/Shortening a Season

1. Open `BattlePassSeason` resource
2. Change `Duration Days`
3. Update `End Date` if needed
4. Save

**Warning**: Shortening a season may frustrate players who were on track to complete it!

---

## Managing Multiple Seasons

### How Seasons Work in Code

```csharp
// Seasons are stored in BattlePass.AvailableSeasons
// Current season is BattlePass.CurrentSeason
// Progress auto-resets when season changes
```

### Pre-Configuring All 4 Seasons

1. **Season 1 (Ice - Blue)**
   ```
   Theme Color: #3498db
   Premium Rewards: Ice-themed cosmetics
   ```

2. **Season 2 (Fire - Red)**
   ```
   Theme Color: #e74c3c
   Premium Rewards: Fire-themed cosmetics
   ```

3. **Season 3 (Nature - Green)**
   ```
   Theme Color: #27ae60
   Premium Rewards: Nature-themed cosmetics
   ```

4. **Season 4 (Dark - Purple)**
   ```
   Theme Color: #8e44ad
   Premium Rewards: Dark/mysterious cosmetics
   ```

### Seasonal Transition

When a season ends:
1. All unclaimed rewards are lost
2. Progress resets for new season
3. New season becomes active
4. Players keep owned cosmetics

---

## Troubleshooting

### Battle Pass Not Showing in Game

**Problem**: Battle Pass menu option is missing

**Solutions**:
1. Check `BattlePass` is in autoload (project.godot)
2. Verify `CurrentSeason` is assigned
3. Check `ShopEnabled` is true in CosmeticsShop

### Players Can't Purchase

**Problem**: Purchase button does nothing

**Solutions**:
1. Check `AllowSeasonPurchase` is true
2. Verify `BattlePassPrice` > 0
3. Check IAP integration is working

### Rewards Not Unlocking

**Problem**: Player reached tier but no reward

**Solutions**:
1. Check `ClaimTier()` is being called
2. Verify reward data is properly configured
3. Check player has battle pass (for premium rewards)

### XP Not Progressing

**Problem**: Players gain XP but tiers don't advance

**Solutions**:
1. Check `AddXp()` is being called on level complete
2. Verify `XpForCurrentTier` calculation
3. Check season total tiers configuration

---

## Quick Reference: Inspector Fields

### BattlePassSeason Fields

| Field | Type | Description |
|-------|------|-------------|
| SeasonNumber | int | 1-4 for each season |
| SeasonName | string | Display name |
| SeasonDescription | string | Flavor text |
| ThemeColor | string | Hex color (#RRGGBB) |
| StartDate | datetime | Season start |
| EndDate | datetime | Season end |
| DurationDays | int | Usually 28 |
| BattlePassPrice | float | USD price |
| TotalTiers | int | Usually 30 |
| FreeTierCount | int | Usually 20 |
| Tiers | array | All tier data |

### Tier Reward Fields

| Field | Type | Description |
|-------|------|-------------|
| Type | enum | Cosmetic, Coins, Premium, etc. |
| RewardId | string | Reference ID |
| Amount | int | For currency rewards |
| DisplayName | string | Reward name |
| Description | string | Reward description |
| IconPath | string | Sprite path |

---

## Next Steps

After setting up battle pass:
1. Create promotional graphics for each theme
2. Set up analytics tracking for BP purchases
3. Plan launch marketing for first season
4. Consider early-bird discounts

For monetization strategy, see [MONETIZATION_STRATEGY_GUIDE.md](MONETIZATION_STRATEGY_GUIDE.md)
