# Store Submission Prep (iOS + Android)

This repository contains **code + templates** to get Angry Animals ready for App Store / Google Play submission.

> Notes
> - Account creation, certificates, and store-console setup must be done in Apple/Google portals.
> - **Do not commit signing files** (keystore, .p12, provisioning profiles). `.gitignore` already blocks common formats.
> - `export_presets.cfg` remains **gitignored** because Godot may store sensitive signing data in it. Use the provided example template instead.

---

## 1) Bundle IDs / Product IDs (canonical)

- **Bundle / Package ID:** `com.rcbiscuits.angryanimals`
- **IAP Product ID (iOS + Android):** `full_game_unlock` (non-consumable)

These defaults are already wired into the game:
- `Globals.cs` reads project settings:
  - `monetization/iap/ios_product_id`
  - `monetization/iap/android_product_id`
- `project.godot` defines defaults for these settings.

---

## 2) Godot export presets (local-only)

1. In Godot: **Project → Export…**
2. Create two presets:
   - **iOS** (Release)
   - **Android** (Release, AAB)
3. Use `export_presets.example.cfg` as a checklist of which fields must be set.

> Keep `export_presets.cfg` untracked.

---

## 3) iOS (Xcode / TestFlight / App Store)

### Required capabilities
- In-App Purchase
- Camera (face customization)
- Photo Library (gallery selection)
- Internet (ads)

### Export / build flow
1. Install Godot **iOS export templates**.
2. Export to an **Xcode project**.
3. In Xcode:
   - Set Signing & Capabilities (Team, App ID, provisioning)
   - Add missing `Info.plist` usage strings (see below)
   - Archive → Distribute → App Store Connect (or upload via Transporter)

### Info.plist usage strings (required)
Add the following (exact text can be adjusted):
- `NSCameraUsageDescription`: "Used to capture your face for in-game character customization."
- `NSPhotoLibraryUsageDescription`: "Used to select an image for in-game character customization."

If using AdMob personalized ads / IDFA:
- `NSUserTrackingUsageDescription`: "Used to deliver relevant ads."

### Privacy manifest
If your exported Xcode project needs an `PrivacyInfo.xcprivacy`, add one in Xcode (Apple requirement varies by SDK / APIs). Keep declarations accurate.

---

## 4) Android (Play Console / Internal testing / Production)

### Required items
- **Keystore** (upload key) stored securely + backed up
- Target SDK: 34+
- Min SDK: 28+
- App Bundle (**AAB**) for Play Store

### Export / build flow
1. Install Godot **Android build templates**.
2. Generate a keystore (see `scripts/android/generate_keystore.sh`).
3. In Godot Android export preset:
   - Package: `com.rcbiscuits.angryanimals`
   - Version name: `1.0`
   - Version code: `1`
   - ARM64 enabled (required)
   - Configure signing with your keystore
4. Export **Android App Bundle (.aab)** for upload.

### Permissions
Depending on the final implementation of face/gallery and ads, you likely need:
- `INTERNET` (ads)
- `CAMERA` (face capture)
- Android 13+: `READ_MEDIA_IMAGES` (gallery)
- Pre-Android 13: `READ_EXTERNAL_STORAGE` (gallery)

---

## 5) Store listing text & assets

Draft metadata lives in `store/metadata/`:
- `store/metadata/ios/*`
- `store/metadata/android/*`

A privacy policy template is provided in `PRIVACY_POLICY.md`.

---

## 6) Build QA checklist (practical)

Before submitting builds:
- Verify **IAP** unlock toggles `PlayerProfile.IsFullGameUnlocked` and persists after restart.
- Verify **ads** are not shown after unlock.
- Verify face customization:
  - permission prompts show
  - camera works
  - selecting a face saves to `user://faces/*` and persists
- Test background/resume.

See also:
- `LAUNCH_CHECKLIST.md`
- `MONETIZATION_SETUP.md`
