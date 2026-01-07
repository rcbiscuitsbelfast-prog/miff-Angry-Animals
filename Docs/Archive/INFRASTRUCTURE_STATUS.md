# Infrastructure Status: Angry Animals

This document provides a comprehensive audit of the backend, database, and infrastructure readiness for Angry Animals.

## 1. Backend & Server Requirements
- **Does the game need a backend server?** **NO.**
- **Reasoning:** Angry Animals is designed as a standalone, client-side game. All logic, progression, and state management are handled locally on the device.
- **Cloud/Database Dependencies:** None.
- **Server-side Systems (Leaderboards, Cloud Save, Multiplayer):** None implemented. The game uses local storage for all persistent data.
- **Verdict:** STANDALONE GAME. Deployment is simplified as there are no servers to maintain, no database costs, and no backend scaling concerns.

## 2. Current Infrastructure Status
The following manager scripts are implemented and ready for production use:

- **AdsManager.cs:** Handles AdMob integration. Designed as a wrapper for platform-specific plugins (no-op on desktop).
- **MonetizationManager.cs:** Handles In-App Purchases (Full Game Unlock). Ready for StoreKit (iOS) and Google Play Billing (Android).
- **FileManager.cs:** Handles local JSON serialization for level scores.
- **PlayerProfile.cs:** Manages local player data (name, unlocks, cosmetics, face image paths).
- **GameManager.cs:** Manages game state and level progression.
- **Globals.cs:** Handles global initialization, including reading monetization settings from `project.godot`.

### Configuration Status
- **project.godot:** Contains a `[monetization]` section with placeholders for AdMob App IDs and Ad Unit IDs.
- **export_presets.example.cfg:** Provides a complete template for Android and iOS export settings, including canonical bundle identifiers and required permissions.

## 3. Ads System Integration
- **Status:** **CODE READY.** The `AdsManager` is fully implemented and hooked into the game flow.
- **Supported Ad Types:** 
  - Banner ads (bottom of screen)
  - Interstitial ads (full-screen transitions)
  - Rewarded ads (video ads for rewards)
- **Configuration Required:** 
  - Install a Godot AdMob plugin (e.g., Poing Studios or similar).
  - Paste real AdMob App ID and Ad Unit IDs into `project.godot`.
- **Blocking:** No. The game runs perfectly without ads configured; the manager gracefully handles missing plugins or IDs.

## 4. Data & Save System
- **Mechanism:** Local JSON files.
- **Save Location:** `user://` directory (platform-specific persistent storage).
  - **Windows:** `%APPDATA%\Godot\app_userdata\Angry Animals\`
  - **Android/iOS:** Internal app storage (protected).
- **Files:**
  - `profile.json`: Stores player name, unlock status, and cosmetic preferences.
  - `level_scores.json`: Stores high scores and star ratings for each level.
- **Persistence:** Full persistence across sessions is implemented. No cloud backend is required for saving progress.

## 5. Deployment Readiness Summary
| Feature | Status | Action Required |
|---------|--------|-----------------|
| Standalone Flow | Ready | None |
| Save System | Ready | None |
| Ads Code | Ready | Add IDs & Plugin |
| IAP Code | Ready | Add Product IDs |
| Export Presets | Ready | Configure Certificates |
| App Icons/Splash | Missing | Upload Assets |

**Conclusion:** The game is architecturally ready for deployment. The remaining steps are purely configuration and asset-based.
