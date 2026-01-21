# Phase 2: Monetization & Ads Systems (C# → GDScript)

This document describes the monetization and ads systems converted from C# to GDScript.

## Overview

All monetization and ads functionality has been converted from C# to GDScript while preserving full feature parity with the original implementation. The systems are now fully compatible with the existing Phase 1 GDScript codebase.

## Directory Structure

```
/Scenes/Scripts/Monetization/
├── Managers/
│   ├── AdsManager.gd              # Core ad display orchestration
│   ├── MonetizationManager.gd       # Central monetization controller
│   ├── RewardedAdManager.gd         # Rewarded video ad handler
│   ├── AdFrequencyOptimizer.gd      # Smart ad scheduling
│   ├── DailyChallengeManager.gd       # Daily rewards system
│   └── EnhancedAudioSystem.gd        # Audio muting during ads
├── IAP/
│   ├── IAPAdapter.gd               # Base class for IAP adapters
│   ├── GooglePlayBillingAdapter.gd   # Android IAP implementation
│   ├── AppStoreKitAdapter.gd        # iOS IAP implementation
│   └── AmazonIAPAdapter.gd           # Amazon AppStore IAP
├── Cosmetics/
│   ├── CosmeticLootTable.gd        # Cosmetic drop system
│   └── SocialCosmetics.gd          # Social unlock cosmetics
└── Data/
    ├── Promise.gd                  # Async/await utility
    ├── MaterialDistributor.gd        # Material distribution for levels
    └── cosmetics_data.json          # Cosmetic definitions
```

## Converted Files

### 1. Ad Management (3 files)

#### AdsManager.gd
**Purpose:** Core ad display orchestration for AdMob integration

**Key Features:**
- Banner ads (top/bottom placement, auto-refresh)
- Interstitial ads (with cooldown and preloading)
- Rewarded ads (with reward tracking)
- Platform detection and graceful fallback for unsupported platforms
- AdMob plugin detection (tries multiple plugin naming conventions)
- Placeholder banner support for editor/testing
- Premium status integration (respects remove ads purchases)
- Frequency capping (45s cooldown between interstitials)
- Signals: `ad_closed`, `ad_clicked`, `reward_earned`, `banner_inset_changed`

**Configuration from Project Settings:**
- `monetization/admob/app_id` - AdMob app ID
- `monetization/admob/banner_ad_unit_id` - Banner ad unit ID
- `monetization/admob/interstitial_ad_unit_id` - Interstitial ad unit ID
- `monetization/admob/rewarded_ad_unit_id` - Rewarded ad unit ID
- `monetization/admob/banner_position` - "top" or "bottom"
- `monetization/admob/persistent_banner` - Enable persistent banner
- `monetization/admob/banner_auto_refresh` - Enable auto-refresh
- `monetization/admob/banner_refresh_seconds` - Refresh interval
- `monetization/admob/banner_height_px` - Banner height for UI layout

**Usage:**
```gdscript
# Show banner
AdsManager.instance.show_banner_ad()

# Show interstitial
await AdsManager.instance.show_interstitial_ad()

# Show rewarded ad
await AdsManager.instance.show_rewarded_ad()

# Hide banner
AdsManager.instance.hide_banner_ad()

# Check if ads should be shown
if AdsManager.instance.should_show_ads():
    # Show ads
```

#### AdFrequencyOptimizer.gd
**Purpose:** Intelligent ad placement using A/B testing to balance revenue and retention

**Key Features:**
- Three strategies: Aggressive, Balanced, Conservative
- Smart placement based on player behavior
- Quiet hours support (configurable 10 PM - 8 AM default)
- Frequency capping (max 3 ads per 30 minutes)
- Minimum interval enforcement (60 seconds between ads)
- Player frustration detection (reduces ads when player struggling)
- A/B testing integration hooks
- Strategic frequency control (every 2/5/8 levels based on strategy)
- Signals: `ad_strategy_changed`, `ad_placement_optimized`, `ad_frequency_metrics_updated`

**Strategy Configuration:**
- **Aggressive:** Every 2 levels, high rewarded prominence, banner always visible
- **Balanced:** Every 5 levels, moderate rewarded prominence, banner optional
- **Conservative:** Every 8 levels, low rewarded prominence, banner hidden

**Usage:**
```gdscript
# Check if interstitial should be shown
if AdFrequencyOptimizer.instance.should_show_interstitial_ad("level_completed", 5):
    await AdsManager.instance.show_interstitial_ad()
    AdFrequencyOptimizer.instance.record_ad_shown(AdFrequencyOptimizer.AdType.INTERSTITIAL)

# Get rewarded ad prominence (0.0 - 1.0)
var prominence = AdFrequencyOptimizer.instance.get_rewarded_ad_prominence()

# Switch strategy
AdFrequencyOptimizer.instance.switch_strategy(AdFrequencyOptimizer.AdStrategyType.AGGRESSIVE)
```

#### EnhancedAudioSystem.gd
**Purpose:** Audio muting during ads to improve user experience

**Key Features:**
- Automatic mute on ad display/click
- Automatic unmute on ad close
- Volume preservation (remembers pre-ad volume)
- Signals: `audio_muted`
- Integration with AudioManager via reflection for compatibility

**Usage:**
```gdscript
# System automatically handles audio muting
# No manual intervention needed
# Connect to audio_muted signal for UI updates
EnhancedAudioSystem.instance.audio_muted.connect(func(is_muted):
    update_mute_icon(is_muted))
```

### 2. In-App Purchase (IAP) Adapters (4 files)

#### IAPAdapter.gd
**Purpose:** Base class defining the interface for all IAP implementations

**Methods to Override:**
- `initialize()` - Initialize the IAP system
- `query_product_info(product_id)` - Get product details from store
- `purchase_product(product_id)` - Start purchase flow
- `restore_purchases()` - Restore previous purchases
- `is_billing_available()` - Check if billing is available
- `verify_purchase(receipt)` - Verify purchase receipt
- `get_service_version()` - Get billing service version

**Signals:**
- `purchase_started(product_id)` - Purchase flow initiated
- `purchase_completed(product_id, receipt)` - Purchase succeeded
- `purchase_failed(product_id, reason)` - Purchase failed
- `restore_completed` - Restore completed
- `restore_failed(reason)` - Restore failed

#### GooglePlayBillingAdapter.gd
**Purpose:** Android IAP via Google Play Billing

**Key Features:**
- Product info querying
- Purchase flow launching
- Purchase result handling
- Acknowledgment (required by Google Play)
- Signature verification (placeholder for server-side implementation)
- Restore purchases
- Billing availability check

**Plugin Support:**
Tries multiple singleton names: `GooglePlayBilling`, `GodotGooglePlayBilling`, `GodotGooglePlay`, `InAppPurchase`, `InAppPurchases`

**Usage:**
```gdscript
var adapter = GooglePlayBillingAdapter.new()
adapter.initialize()

# Query product info
var product_info = adapter.query_product_info("remove_ads")

# Start purchase
if adapter.purchase_product("remove_ads"):
    print("Purchase started")
```

#### AppStoreKitAdapter.gd
**Purpose:** iOS IAP via StoreKit 2

**Key Features:**
- Product info querying
- Purchase flow launching
- Purchase result handling
- Transaction finishing (required by StoreKit)
- Receipt validation (placeholder for server-side implementation)
- Restore purchases
- Payment authorization check

**Plugin Support:**
Tries multiple singleton names: `StoreKit`, `StoreKit2`, `InAppPurchase`, `InAppPurchases`, `GodotInAppPurchase`

**Usage:**
```gdscript
var adapter = AppStoreKitAdapter.new()
adapter.initialize()

# Check if can make payments
if adapter.can_make_payments():
    # Proceed with purchase
```

#### AmazonIAPAdapter.gd
**Purpose:** Amazon Fire devices IAP via Amazon Appstore

**Key Features:**
- Product info querying (SKU-based)
- Purchase flow launching
- Purchase result handling
- Purchase fulfillment (entitlement granting)
- Receipt verification (placeholder for server-side implementation)
- Restore purchases
- Billing availability check

**Plugin Support:**
Tries singleton: `AmazonIAP`

**Usage:**
```gdscript
var adapter = AmazonIAPAdapter.new()
adapter.initialize()

# Get available products
var products = adapter.get_available_products()
```

### 3. Monetization Management (1 file)

#### MonetizationManager.gd
**Purpose:** Central monetization controller for in-app purchases

**Key Features:**
- Platform detection (Android/iOS/others)
- Product ID management (iOS and Android)
- Purchase flow orchestration
- Restore purchases functionality
- Full game unlock state persistence (via PlayerProfile)
- Auto-hide ads on purchase
- Signals: `purchase_succeeded`, `purchase_failed`, `purchase_restored`

**Configuration:**
- `ios_product_id` - iOS product ID (default: "full_game_unlock")
- `android_product_id` - Android product ID (default: "full_game_unlock")

**Usage:**
```gdscript
# Purchase full game unlock
await MonetizationManager.instance.purchase_full_game()

# Restore purchases
await MonetizationManager.instance.restore_purchases()

# Check if full game is unlocked
if MonetizationManager.instance.get_is_full_game_unlocked():
    print("Full game unlocked!")
```

#### RewardedAdManager.gd
**Purpose:** Core handler for rewarded video ads with lifecycle management

**Key Features:**
- Ad preloading on startup
- Ad ready state tracking
- Callback-based reward granting
- Integration with AdsManager
- Analytics tracking
- Auto-preload next ad after show
- Signals: `reward_granted`, `reward_failed`

**Usage:**
```gdscript
# Show rewarded ad with callback
RewardedAdManager.instance.show_rewarded_ad(func():
    print("Reward granted! Give player coins/boost.")
)

# Check if ad is ready
if RewardedAdManager.instance.is_rewarded_ad_ready():
    # Show ad
```

### 4. Cosmetics System (2 files)

#### CosmeticLootTable.gd
**Purpose:** Weighted random drop system for cosmetic unlocks on perfect scores

**Key Features:**
- Weighted loot table for hats, glasses, emotions, wigs, skins, effects
- Perfect score trigger (3 stars required)
- Base drop chance with bonuses
- Dry spell bonus (increases drop chance after multiple perfect scores without drops)
- Duplicate prevention (reduces weight of already-owned cosmetics)
- Progressive drop rate (better chance over time)
- Persistence of drop history
- Celebration UI hooks
- Signals: `cosmetic_earned(cosmetic_id, cosmetic_type)`

**Drop Categories:**
- **Hats (6):** Cap, Crown, Beanie, Top Hat, Cowboy Hat, Beret
- **Glasses (6):** Round, Aviator, Sunglasses, Nerd Glasses, Monocle, 3D Glasses
- **Emotions (5):** Happy, Angry, Sad, Excited, Surprised
- **Special (8):** Moustache, Wig, Slingshot Skin, Projectile Skin, Trail Effect, Hit Effect, Victory Effect

**Weights:**
- Common: 0.8-1.2
- Uncommon: 0.6-0.9
- Rare: 0.3-0.7
- Epic: 0.2-0.3
- Legendary: 0.1

**Usage:**
```gdscript
# Award cosmetic drop on perfect score (3 stars)
if CosmeticLootTable.instance.try_award_cosmetic_drop(3, score, level_number):
    print("Cosmetic dropped!")

# Force award cosmetic (for testing)
CosmeticLootTable.instance.force_award_cosmetic("crown", level_number)

# Get current drop chance percentage
var drop_chance = CosmeticLootTable.instance.get_current_drop_chance_percentage()
```

#### SocialCosmetics.gd
**Purpose:** Cosmetic unlocks based on social actions and achievements

**Key Features:**
- Social unlock conditions (friends, challenges, replays, leaderboards)
- Progress tracking toward unlocks
- Auto-unlock when threshold reached
- Rarity system (rare, epic, legendary)
- Integration with social systems (FriendChallengeManager, ReplayManager, GlobalLeaderboard)
- Signals: `social_cosmetic_unlocked(cosmetic_id)`

**Social Cosmetics:**
1. **Friendship Hat** - Add 5 friends (rare)
2. **Challenge Champion Crown** - Win 10 friend challenges (epic)
3. **Viral Legend Glasses** - Get 100 replay views (legendary)
4. **Team Player Wig** - Participate in 50 challenges (rare)
5. **Leaderboard Elite Moustache** - Rank in top 100 (epic)

**Usage:**
```gdscript
# Check all social unlocks
SocialCosmetics.instance.check_all_unlocks()

# Get progress for a cosmetic
var progress = SocialCosmetics.instance.get_progress("challenge_champion_crown")
print(f"Progress: {progress}/10 wins")

# Get all social cosmetics
var cosmetics = SocialCosmetics.instance.get_all_cosmetics()
```

### 5. Supporting Infrastructure (2 files)

#### DailyChallengeManager.gd
**Purpose:** Daily challenge system with deterministic seed-based levels

**Key Features:**
- Date-based seed (same challenge for all players each day)
- Procedural level generation
- Daily level number selection (deterministic via seed)
- Player profile persistence

**Seed Formula:**
```gdscript
seed = year * 10000 + month * 100 + day
```

**Usage:**
```gdscript
# Start today's daily challenge
DailyChallengeManager.instance.start_daily_challenge()

# Get today's seed
var seed = DailyChallengeManager.instance.get_daily_seed()
```

#### MaterialDistributor.gd
**Purpose:** Material distribution for procedurally generated levels based on difficulty

**Key Features:**
- Difficulty-based distribution (Easy/Medium/Hard/Extreme)
- Material types: Wood, Stone, Brick, Iron, Diamond
- Weighted random selection
- Deterministic seeding (uses LevelGenerator seed + offset)
- Material variety enforcement (prevents all-same materials)
- Softness calculation for gameplay balance

**Difficulty Distributions:**
- **Easy (rooms 1-20):** 70% Wood, 20% Stone, 10% Brick
- **Medium (rooms 21-40):** 30% Wood, 40% Stone, 20% Brick, 10% Iron
- **Hard (rooms 41-60):** 20% Stone, 30% Brick, 40% Iron, 10% Diamond
- **Extreme (rooms 61+):** 10% Brick, 40% Iron, 50% Diamond

**Usage:**
```gdscript
# Get materials for a room
var materials = MaterialDistributor.get_materials_for_room(room_number, obstacle_count)

# Get difficulty softness (0.0 = all hard, 1.0 = all soft)
var softness = MaterialDistributor.get_difficulty_softness(room_number)
```

#### cosmetics_data.json
**Purpose:** Data file containing all cosmetic definitions

**Structure:**
```json
{
  "cosmetics": {
    "hats": [...],
    "glasses": [...],
    "emotions": [...],
    "moustaches": [...],
    "wigs": [...],
    "slingshot_skins": [...],
    "projectile_skins": [...],
    "trail_effects": [...],
    "hit_effects": [...],
    "victory_effects": [...]
  }
}
```

Each cosmetic includes:
- `id` - Unique identifier
- `name` - Display name
- `rarity` - "common", "uncommon", "rare", "epic", "legendary"

### 6. Utilities (1 file)

#### Promise.gd
**Purpose:** Async/await utility for GDScript to handle multiple signals

**Key Features:**
- Promise-like API (`then`, `catch`)
- Signal-based promises (`Promise.from_signal()`)
- Immediate promises (`Promise.resolved()`, `Promise.rejected()`)
- Promise composition (`Promise.any()`)
- Delay promises (`Promise.delay()`)
- Chaining support

**Usage:**
```gdscript
# Wait for signal
var promise = Promise.from_signal(node, "signal_name")
await promise

# Wait for multiple signals
var results = await Promise.any([
    Promise.from_signal(ads_manager, "ad_closed"),
    Promise.from_signal(timer, "timeout")
])

# Delay
await Promise.delay(2.0)  # 2 seconds

# Chain
Promise.from_signal(node, "signal_name").then(func(result):
    print("Result:", result))
```

## Integration with Phase 1

### PlayerProfile Changes
Added method `unlock_cosmetic(cosmetic_id: String)` to support cosmetic unlocking:

```gdscript
# In PlayerProfile.gd
func unlock_cosmetic(cosmetic_id: String) -> void:
    if not cosmetic_id in unlocked_cosmetics:
        unlocked_cosmetics.append(cosmetic_id)
        save_profile()
```

### Autoload Configuration
Updated `project.godot` to register new autoloaded singletons:

```ini
AdsManager="*res://Scenes/Scripts/Monetization/Managers/AdsManager.gd"
RewardedAdManager="*res://Scenes/Scripts/Monetization/Managers/RewardedAdManager.gd"
MonetizationManager="*res://Scenes/Scripts/Monetization/Managers/MonetizationManager.gd"
DailyChallengeManager="*res://Scenes/Scripts/Monetization/Managers/DailyChallengeManager.gd"
CosmeticLootTable="*res://Scenes/Scripts/Monetization/Cosmetics/CosmeticLootTable.gd"
SocialCosmetics="*res://Scenes/Scripts/Monetization/Cosmetics/SocialCosmetics.gd"
EnhancedAudioSystem="*res://Scenes/Scripts/Monetization/Managers/EnhancedAudioSystem.gd"
MaterialDistributor="*res://Scenes/Scripts/Monetization/Data/MaterialDistributor.gd"
```

### Signal Integration

All managers emit Godot signals for easy integration with UI and other systems:

**AdsManager:**
- `ad_closed` - Ad was closed
- `ad_clicked` - Ad was clicked
- `reward_earned` - Reward granted
- `banner_inset_changed(inset_px)` - Banner size changed

**MonetizationManager:**
- `purchase_succeeded` - Purchase completed
- `purchase_failed(reason)` - Purchase failed
- `purchase_restored` - Purchases restored

**RewardedAdManager:**
- `reward_granted` - User earned reward
- `reward_failed(reason)` - Reward failed

**CosmeticLootTable:**
- `cosmetic_earned(cosmetic_id, cosmetic_type)` - Cosmetic unlocked

**SocialCosmetics:**
- `social_cosmetic_unlocked(cosmetic_id)` - Social cosmetic unlocked

**AdFrequencyOptimizer:**
- `ad_strategy_changed(new_strategy)` - Strategy changed
- `ad_placement_optimized(placement_reason, ad_shown)` - Ad shown
- `ad_frequency_metrics_updated(metrics)` - Metrics updated

**EnhancedAudioSystem:**
- `audio_muted(is_muted)` - Audio mute state changed

## Key Conversions from C# to GDScript

### C# → GDScript Mapping

| C# Feature | GDScript Equivalent |
|------------|-------------------|
| `Task<T>` / `async/await` | `Promise` class with `await` |
| `Signal` with parameters | Godot `signal` with parameters |
| `Enum` | GDScript `enum` |
| `Dictionary<string, object>` | `Dictionary` |
| `Action<T>` | `Callable` |
| `DateTime.Now` | `Time.get_ticks_msec()` |
| `TimeSpan` | Manual calculation |
| `[Export]` properties | `export var` |
| `[Signal]` delegates | `signal name(parameters)` |
| `Instance` singleton pattern | `static var instance: ClassName` |
| `GodotObject` | `Object` |
| `Node.CallDeferred()` | `call_deferred()` |
| `TryCallPlugin()` | `_try_call_plugin()` |
| `ToSignal()` | `Promise.from_signal()` |
| `Task.WhenAny()` | `Promise.any()` |
| `await Task.Delay()` | `await Promise.delay()` |
| `Math.Clamp()` | `clampf()` |
| `Math.Max()` | `maxf()` |
| `Random.NextDouble()` | `RandomNumberGenerator.randf()` |
| `Random.Next()` | `RandomNumberGenerator.randi()` |

## Platform Support

### Android
- **Ads:** AdMob via plugin detection
- **IAP:** GooglePlayBillingAdapter (requires Google Play Billing plugin)
- **Gradle:** `com.google.android.gms:play-services-ads:23.5.0`
- **Permissions:** Internet, Network State

### iOS
- **Ads:** AdMob via plugin detection
- **IAP:** AppStoreKitAdapter (requires StoreKit 2 plugin)
- **Frameworks:** Google-Mobile-Ads-SDK

### Amazon
- **Ads:** AdMob via plugin detection (if Amazon supports AdMob)
- **IAP:** AmazonIAPAdapter (requires Amazon IAP plugin)

### Desktop/Web
- **Ads:** Placeholder banner (simulated in editor)
- **IAP:** Mock/test mode only
- **Graceful Fallback:** All systems handle missing plugins

## Data Persistence

### PlayerProfile Integration
Cosmetic unlocks persisted in `PlayerProfile.unlocked_cosmetics` array:

```json
{
  "cosmetics": {
    "unlocked_list": ["crown", "aviator", "happy"]
  }
}
```

### Cosmetic Drop History
Saved to `user://cosmetic_drop_history.json`:

```json
{
  "perfect_scores_since_last_drop": 0,
  "total_perfect_scores": 15,
  "last_drop_time": 1640995200000
}
```

## Testing

### Mock IAP
For testing without real purchases, systems gracefully handle missing plugins by:
1. Checking `Engine.has_singleton(plugin_name)`
2. Returning `false` for billing operations
3. Using placeholder data for product info

### Ad Testing
In editor/desktop environments, ads use placeholder functionality:
1. Editor placeholder banner (gray bar)
2. Simulated ad loading with timers
3. No-op for all ad operations

### Cosmetic Testing
```gdscript
# Force award a cosmetic (bypasses all checks)
CosmeticLootTable.instance.force_award_cosmetic("crown", 1)

# Reset drop history
CosmeticLootTable.instance.reset_drop_history()

# Check all social unlocks
SocialCosmetics.instance.check_all_unlocks()
```

## Analytics Integration

All monetization events track to `AnalyticsEventTracker` if available:

- `ad_shown` - Ad displayed to user
- `ad_completed` - User watched full ad
- `ad_skipped` - User skipped ad
- `reward_earned` - Rewarded ad completed
- `rewarded_ad_closed` - Rewarded ad closed
- `purchase_started` - Purchase initiated
- `purchase_completed` - Purchase successful
- `social_cosmetic_unlocked` - Social cosmetic earned

## Next Steps

### Phase 3: Analytics, Social & Firebase
- Firebase integration
- A/B testing system
- Friends/Challenges system
- Leaderboards

### Platform Plugin Development
For production use, you'll need to integrate:
1. **AdMob Plugin** for banner/interstitial/rewarded ads
2. **Google Play Billing Plugin** for Android IAP
3. **StoreKit 2 Plugin** for iOS IAP
4. **Amazon IAP Plugin** for Amazon Fire devices

Recommended plugins:
- [Godot AdMob](https://github.com/silentlogiccode/godot-admob) - Cross-platform AdMob
- [Godot Billing](https://github.com/moises-silva/godot-billing) - Google Play Billing
- Custom iOS plugin with StoreKit 2 framework

## Migration Notes

### Removed Files (C#)
The following C# files have been replaced by GDScript equivalents:
- `Globals/AdsManager.cs` → `Monetization/Managers/AdsManager.gd`
- `Globals/MonetizationManager.cs` → `Monetization/Managers/MonetizationManager.gd`
- `Globals/RewardedAdManager.cs` → `Monetization/Managers/RewardedAdManager.gd`
- `Globals/DailyChallengeManager.cs` → `Monetization/Managers/DailyChallengeManager.gd`
- `Globals/CosmeticLootTable.cs` → `Monetization/Cosmetics/CosmeticLootTable.gd`
- `Globals/SocialCosmetics.cs` → `Monetization/Cosmetics/SocialCosmetics.gd`
- `Classes/AdFrequencyOptimizer.cs` → `Monetization/Managers/AdFrequencyOptimizer.gd`
- `Classes/EnhancedAudioSystem.cs` → `Monetization/Managers/EnhancedAudioSystem.gd`
- `Globals/MaterialDistributor.cs` → `Monetization/Data/MaterialDistributor.gd`

### Preserved Files (C#)
These C# files remain (not yet converted to GDScript in this phase):
- Platform-specific IAP adapters that call native plugins
- Some infrastructure systems that don't directly interact with monetization

### Feature Parity
All monetization features from the C# version have been preserved:
✅ Banner ads with positioning
✅ Interstitial ads with cooldown
✅ Rewarded ads with callbacks
✅ Platform detection and fallback
✅ Premium status integration
✅ AdMob plugin detection
✅ IAP adapter base class
✅ Google Play Billing adapter
✅ App Store (StoreKit 2) adapter
✅ Amazon IAP adapter
✅ Cosmetic loot table with weights
✅ Social cosmetics with unlock conditions
✅ Daily challenge system
✅ Material distribution
✅ Enhanced audio system
✅ Ad frequency optimization
✅ Analytics tracking hooks
✅ Data persistence
✅ Signal-based architecture

## License

All converted code maintains the same license as the original project.
