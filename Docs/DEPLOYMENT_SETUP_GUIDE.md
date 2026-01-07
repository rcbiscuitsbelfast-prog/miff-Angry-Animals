# Deployment Setup Guide: Angry Animals

This guide provides step-by-step instructions for deploying Angry Animals to the Google Play Store and Apple App Store.

## 1. External Accounts Needed
Before you begin, ensure you have the following:
- **Google Play Console Account:** ($25 one-time fee) for Android deployment.
- **Apple Developer Program:** ($99/year) for iOS deployment.
- **AdMob Account:** (Free) for managing and earning from ads.

---

## 2. Android Deployment (.aab)

### Prerequisites
- Install **OpenJDK 17** or higher.
- Install **Android Studio** (specifically for the SDK and Build Tools).
- Create a **Release Keystore** for signing.

### Step-by-Step
1. **Generate Keystore:**
   - Use `keytool` or the Godot Editor (Editor -> Editor Settings -> Export -> Android) to create a `.keystore` file.
2. **Configure Godot:**
   - Open Godot and go to `Project -> Export`.
   - Add an **Android** preset.
   - **Options -> Keystore:** Select your release keystore and enter the alias/passwords.
   - **Options -> Package:** Set `Name` to `com.rcbiscuits.angryanimals`.
3. **Ads/IAP Configuration:**
   - Open `project.godot` (or Project Settings -> General -> Monetization).
   - Enter your AdMob Android IDs.
   - Ensure the Android Product ID for IAP matches your Play Console product ID (`full_game_unlock`).
4. **Export:**
   - Click **Export Project**. 
   - Ensure **Export With Debug** is **OFF**.
   - Save as `AngryAnimals.aab` (App Bundle).
5. **Upload:**
   - Upload the `.aab` file to the Google Play Console under **Production**.

---

## 3. iOS Deployment (.ipa)

### Prerequisites
- A **Mac** with the latest **Xcode** installed.
- Valid **Apple Distribution Certificate** and **App Store Provisioning Profile**.

### Step-by-Step
1. **Configure Godot:**
   - Open Godot and go to `Project -> Export`.
   - Add an **iOS** preset.
   - **App Store Team ID:** Enter your Apple Team ID.
   - **Bundle Identifier:** `com.rcbiscuits.angryanimals`.
2. **Ads/IAP Configuration:**
   - Enter your AdMob iOS IDs in Project Settings.
   - Ensure the iOS Product ID for IAP matches your App Store Connect product ID.
3. **Permissions (Info.plist):**
   - The preset already includes required strings for Camera and Photo Library access. Update the descriptions if you change how these are used.
4. **Export:**
   - Click **Export Project**. This will generate an Xcode project (`.xcodeproj`).
5. **Xcode Build:**
   - Open the generated project in Xcode.
   - Select your target device as "Any iOS Device".
   - Go to `Product -> Archive`.
   - Once archived, click **Distribute App** and follow the prompts to upload to App Store Connect.

---

## 4. Configuring Monetization

### AdMob Setup
1. Create an app in the AdMob console.
2. Create three Ad Units: **Banner**, **Interstitial**, and **Rewarded**.
3. Copy the **App ID** and the three **Ad Unit IDs**.
4. Paste them into `project.godot` under the `[monetization]` section:
   ```ini
   [monetization]
   admob/app_id="ca-app-pub-XXXXXXXXXXXXXXXX~XXXXXXXXXX"
   admob/banner_ad_unit_id="ca-app-pub-XXXXXXXXXXXXXXXX/XXXXXXXXXX"
   ...
   ```

### In-App Purchase Setup
1. In Google Play Console / App Store Connect, create a **Non-Consumable** product with the ID `full_game_unlock`.
2. Ensure the price and description are set.
3. The game will automatically handle the "Unlock" logic when a successful purchase signal is received.

---

## 5. Versioning
To update your app, always increment the versioning in the Export menu:
- **Android:** Increment the `Version Code` (must be an integer, e.g., 1, 2, 3).
- **iOS:** Increment the `Build` number (e.g., 1.0.1).
