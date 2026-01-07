# App Store Submission Checklist: Angry Animals

Use this checklist to ensure everything is ready before you hit "Submit" on Google Play or the App Store.

## 1. Store Metadata
- [ ] **App Name:** Angry Animals (Check for availability).
- [ ] **Short Description:** Catchy 80-character hook.
- [ ] **Long Description:** Full feature list, keywords for ASO.
- [ ] **Privacy Policy:** Required for both stores. (A template is available in `PRIVACY_POLICY.md`).
- [ ] **Support URL:** A website or email where users can contact you.

## 2. Visual Assets
- [ ] **App Icon:** 
  - Android: 512x512 PNG.
  - iOS: 1024x1024 PNG.
- [ ] **Feature Graphic (Android):** 1024x500 PNG.
- [ ] **Screenshots:** 
  - Phone (6.5 inch).
  - Tablet (11 inch and 12.9 inch).
  - Need at least 2-3 high-quality gameplay screenshots.

## 3. Configuration (Godot)
- [ ] **Version Number:** Matches previous version + 1.
- [ ] **Package Name:** `com.rcbiscuits.angryanimals`.
- [ ] **AdMob IDs:** Real Production IDs (NOT Test IDs).
- [ ] **IAP Product IDs:** Match exactly between Godot and Store Consoles.
- [ ] **Main Scene:** Set to `res://Scenes/Main/Main.tscn`.

## 4. Technical Verification
- [ ] **Release Build:** "Export With Debug" is unchecked.
- [ ] **Signing:** 
  - Android: Signed with Release Keystore.
  - iOS: Signed with Distribution Certificate.
- [ ] **Permissions:** `Internet` (Android) and `Camera/Photos` usage descriptions (iOS) are present.
- [ ] **Testing:** Game tested on at least one real Android and one real iOS device.
- [ ] **Save Data:** Verified that closing and reopening the app keeps your progress.

## 5. Monetization Verification
- [ ] **Ads:** Banners appear, and Rewarded/Interstitial ads trigger correctly.
- [ ] **IAP:** "Unlock Full Game" button triggers the store prompt.
- [ ] **Paywall:** Levels 21+ are locked until purchase is made.

## 6. External Accounts
- [ ] **AdMob:** Payments profile completed (so you can get paid).
- [ ] **Google Play:** Merchant account linked (for IAP).
- [ ] **Apple:** Paid Applications Agreement signed in App Store Connect.
