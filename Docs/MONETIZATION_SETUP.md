# Monetization Setup (Ads + Full Game Unlock)

This project uses a **free + paid unlock** model:

- **Free tier:** Levels 1–20, ads shown during gameplay.
- **Paid unlock (£1.50):** Unlocks all 100 levels and disables ads.
- Unlock is persisted in `user://profile.json` via `PlayerProfile.IsFullGameUnlocked`.

> Note: The code is written to integrate with **Godot mobile plugins** (Android/iOS). On desktop platforms ads/IAP are disabled gracefully.

---

## 1) AdMob Setup

### 1.1 Create an AdMob account
1. Go to https://admob.google.com/
2. Create an account (free).

### 1.2 Create your AdMob app entry
1. In the AdMob console, add a new app.
2. Choose the correct platform (Android / iOS).
3. Copy the **AdMob App ID**.

### 1.3 Create ad units
Create three ad units:

- **Banner** (shown during gameplay at the bottom)
- **Interstitial** (shown between levels)
- **Rewarded** (optional: offered after failure to gain a small bonus)

Copy each **Ad Unit ID**.

### 1.4 Configure IDs in the Godot project
The game reads AdMob configuration from **ProjectSettings** keys (recommended for production builds):

- `monetization/admob/app_id`
- `monetization/admob/banner_ad_unit_id`
- `monetization/admob/interstitial_ad_unit_id`
- `monetization/admob/rewarded_ad_unit_id`

You can set these in the Godot Editor:
1. **Project → Project Settings**
2. Add the keys above (type: String)

At runtime these settings are passed into `AdsManager.Initialize(...)` from `Globals.cs`.

### 1.5 Plugin integration notes
`AdsManager` expects an AdMob plugin to be installed and exposed as an **Engine Singleton**.

Different plugins use different singleton names and method names. `AdsManager` attempts multiple common names/methods to be resilient.

If no plugin is installed (or the singleton name differs), ads are automatically disabled and the game continues.

---

## 2) Apple IAP Setup (StoreKit / App Store Connect)

### 2.1 Create the IAP product
1. Go to **App Store Connect** → your app.
2. Create an **In-App Purchase**:
   - **Product ID:** `full_game_unlock`
   - **Type:** Non-consumable
   - **Price:** £1.50
3. Fill out required metadata and submit the IAP for review alongside the app.

### 2.2 Configure the game
The product IDs are configured via ProjectSettings (recommended):

- `monetization/iap/ios_product_id` → `full_game_unlock`
- `monetization/iap/android_product_id` → `full_game_unlock`

`Globals.cs` passes these values to `MonetizationManager.Initialize(...)`.

### 2.3 Restore purchases
`MonetizationManager` calls `RestorePurchases()` automatically on startup.

If the platform plugin reports the non-consumable as owned, `MonetizationManager` should call:
- `MonetizationManager.NotifyPurchaseSucceeded()`

Which persists:
- `PlayerProfile.IsFullGameUnlocked = true`
- `PlayerProfile.Save()`

---

## 3) Google Play Billing Setup

### 3.1 Create the in-app product
1. Go to **Google Play Console** → your app.
2. Create a **Managed product**:
   - **SKU / Product ID:** `full_game_unlock`
   - **Price:** £1.50
3. Activate the product.

### 3.2 Configure merchant account
Ensure your Play Console merchant account and payment profile are configured.

---

## 4) Testing

### iOS
- Use sandbox testers / TestFlight.
- Verify purchase flow:
  - Purchase succeeds
  - App restart still shows unlocked state
  - Restore purchases works on a new install

### Android
- Add license testers in Play Console.
- Use internal testing track.
- Verify purchase + restore.

### Ads
- Use AdMob test ad unit IDs while developing.
- Verify:
  - Banner appears during gameplay (free tier)
  - Interstitial appears between levels
  - Rewarded appears on failure and grants reward

---

## 5) Code Integration Overview

### Autoloads
Two new autoload singletons are added in `project.godot`:

- `AdsManager` → `res://Globals/AdsManager.cs`
- `MonetizationManager` → `res://Globals/MonetizationManager.cs`

### Initialization
`Globals.cs` calls:

- `MonetizationManager.Initialize(iosProductId, androidProductId)`
- `AdsManager.Initialize(appId, bannerId, interstitialId, rewardedId)`

Using ProjectSettings values.

### Unlock persistence
- `PlayerProfile.IsFullGameUnlocked` is saved in `user://profile.json` under `is_full_game_unlocked`.

---

## 6) Stripe Integration (Future)

If a **web build** or direct payments are added later:

- Stripe can be used to take payments on web/PC with a backend.
- Mobile apps **must** use Apple/Google IAP for digital goods (store policy).
- If you later introduce an account system, Stripe webhooks can be used to sync web purchases to your backend and then unlock in-game on other platforms.
