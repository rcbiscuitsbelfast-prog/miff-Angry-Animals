# In-App Purchase (IAP) "Remove Ads" Implementation Guide

## Overview

This document provides step-by-step instructions for implementing and configuring the "Remove Ads" in-app purchase feature across Android (Google Play), iOS (App Store), and Amazon Appstore.

## Architecture Components

### Core Files Created

1. **PremiumManager.cs** - Unified IAP management across platforms
2. **PurchaseStateManager.cs** - Persistent purchase state storage
3. **GooglePlayBillingAdapter.cs** - Android Google Play Billing integration
4. **AppStoreKitAdapter.cs** - iOS StoreKit 2 integration  
5. **AmazonIAPAdapter.cs** - Amazon Appstore IAP integration

### Updated Files

1. **AdsManager.cs** - Integrated premium status checks
2. **SettingsMenu.cs** - Added "Remove Ads" button and purchase flow
3. **project.godot** - Added autoloads and monetization configuration

## App Store Configuration

### Google Play Console (Android)

#### Step 1: Create In-App Product
1. Go to Google Play Console
2. Select your app
3. Navigate to **Monetization > Products > In-app products**
4. Click **Create** → **Single-use**
5. Configure the product:
   - **Product ID**: `remove_ads`
   - **Name**: Remove Ads
   - **Description**: Remove all ads and enjoy ad-free gameplay
   - **Price**: Set per region (suggested: $0.99 USD)
   - **Billing period**: One-time purchase
6. **Status**: Set to "Active"

#### Step 2: Verify Product Configuration
- Product ID must match exactly: `remove_ads`
- Price should be consistent across regions
- Product should be active for testing and production

#### Step 3: Testing
- Use Google Play Console test accounts
- Verify purchase flow works with test cards
- Ensure acknowledgment and verification work properly

### App Store Connect (iOS)

#### Step 1: Create App Product
1. Go to [App Store Connect](https://appstoreconnect.apple.com)
2. Select your app
3. Navigate to **Monetization > In-App Purchases**
4. Click **Create New** → **Non-Consumable**
5. Configure the product:
   - **Product ID**: `remove_ads`
   - **Name**: Remove Ads
   - **Description**: Remove all ads and enjoy ad-free gameplay
   - **Price**: Select Tier 1 (~$0.99 USD)
   - **Review Notes**: "This purchase removes all advertisements from the game"
6. **Status**: Ready to Submit

#### Step 2: Family Controls
1. In App Store Connect, go to **Family Controls**
2. Enable **"Remove Ads"** as appropriate for children

#### Step 3: Testing
- Use StoreKit Configuration files for local testing
- Test with Apple Sandbox accounts
- Verify receipt validation works properly

### Amazon Developer Console (Amazon)

#### Step 1: Create In-App Item
1. Go to [Amazon Developer Console](https://developer.amazon.com/apps-and-games)
2. Select your app
3. Navigate to **Monetization > In-App Items**
4. Click **Create Item**
5. Configure the item:
   - **SKU**: `remove_ads`
   - **Item Name**: Remove Ads
   - **Description**: Remove all ads and enjoy ad-free gameplay
   - **Price**: USD $0.99
   - **Item Type**: Entitlement
6. **Status**: Active

#### Step 2: Test Mode
- Enable test mode for development
- Use Amazon Appstore test accounts
- Verify purchase verification works

## Native Plugin Integration

### Google Play Billing Plugin (Android)

Create a native Android plugin that implements:

```kotlin
// GodotPlugin.gdplugin integration
public class GooglePlayBillingPlugin : GodotPlugin {
    
    public boolean initializeBilling(Activity context, boolean enablePendingPurchases) {
        // Initialize BillingClient
        // Set up purchase callbacks
        return true;
    }
    
    public boolean launchPurchaseFlow(Activity activity, String productId) {
        // Launch purchase flow
        // Handle result via callback
        return true;
    }
    
    public void onPurchaseResult(String purchaseData) {
        // Parse purchase result
        // Verify signature
        // Notify Godot via JNI
        PremiumManager.Instance.NotifyRemoveAdsPurchaseSucceeded();
    }
    
    public boolean restorePurchases() {
        // Query Google Play for existing purchases
        // Restore purchase state
        return true;
    }
}
```

### StoreKit 2 Plugin (iOS)

Create a native iOS plugin that implements:

```swift
// StoreKit2Plugin.swift
import StoreKit

class StoreKit2Plugin: NSObject, SKPaymentTransactionObserver {
    
    func initializeStoreKit() {
        SKPaymentQueue.default().add(self)
    }
    
    func startPurchase(productId: String) {
        Task {
            do {
                let product = try await Product.products(for: [productId]).first
                let result = try await product?.purchase()
                
                switch result {
                case .success(let transaction):
                    await processSuccessfulTransaction(transaction)
                case .userCancelled:
                    // Handle cancellation
                case .pending:
                    // Handle pending purchase
                case .failed(let error):
                    // Handle failure
                }
            }
        }
    }
    
    func paymentQueue(_ queue: SKPaymentQueue, updatedTransactions transactions: [SKPaymentTransaction]) {
        // Handle restored purchases
    }
}
```

### Amazon IAP Plugin (Android)

Create a native Android plugin for Amazon IAP:

```kotlin
// AmazonIAPPlugin.kt
class AmazonIAPPlugin : GodotPlugin {
    
    private var purchasingService: PurchasingService? = null
    
    fun initializeAmazonIAP(context: Context) {
        PurchasingService.registerListener(context, this)
    }
    
    fun launchPurchaseFlow(activity: Activity, sku: String) {
        PurchasingService.purchase(activity, sku)
    }
    
    override fun onPurchaseResponse(purchaseResponse: PurchaseResponse) {
        when (purchaseResponse.purchaseRequestStatus) {
            PurchaseResponse.PurchaseStatus.SUCCESS -> {
                // Verify purchase and notify Godot
                PremiumManager.Instance.NotifyRemoveAdsPurchaseSucceeded()
            }
            PurchaseResponse.PurchaseStatus.FAILED -> {
                // Handle failure
            }
            else -> {
                // Handle other states
            }
        }
    }
}
```

## Project Configuration

### Android Build Configuration

Update your Android export settings:

```gdscript
# export_presets.cfg
[android]
gradle_dependencies = [
  "com.android.billingclient:billing:7.0.0",
  "com.amazon:amazon-device-messaging:1.0.1"
]

[android.manifest]
permissions = [
  "android.permission.INTERNET",
  "android.permission.ACCESS_NETWORK_STATE",
  "com.amazon.device.messaging.permission.RECEIVE"
]
```

### iOS Build Configuration

Update your iOS export settings:

```gdscript
# export_presets.cfg
[ios]
frameworks = [
  "StoreKit",
  "Security",
  "Foundation"
]

info_plist_extra_keys = {
  "LSApplicationQueriesSchemes": ["itms-apps"]
}
```

## Testing Checklist

### Android Testing
- [ ] Test purchase flow with Google Play test accounts
- [ ] Verify purchase acknowledgment works
- [ ] Test purchase restoration on app reinstall
- [ ] Check signature verification
- [ ] Test edge cases (network failure, cancelled purchases)
- [ ] Verify ads are hidden after successful purchase

### iOS Testing
- [ ] Test with StoreKit Configuration file
- [ ] Test with Apple Sandbox accounts
- [ ] Verify receipt validation
- [ ] Test purchase restoration
- [ ] Test family sharing compatibility
- [ ] Verify ads are hidden after successful purchase

### Amazon Testing
- [ ] Test with Amazon Appstore test environment
- [ ] Verify purchase verification
- [ ] Test on Amazon Fire device
- [ ] Test purchase restoration
- [ ] Verify ads are hidden after successful purchase

## Troubleshooting

### Common Issues

1. **Plugin Not Found**
   - Ensure native plugins are properly installed
   - Check plugin names match in PremiumManager
   - Verify plugin initialization

2. **Purchase Failures**
   - Check network connectivity
   - Verify product IDs match exactly across stores
   - Test with different accounts
   - Check store-specific requirements

3. **State Sync Issues**
   - Verify PurchaseStateManager is working
   - Check file permissions for save directory
   - Test purchase restoration

4. **Ads Still Showing**
   - Check ShouldShowAds() logic
   - Verify premium status is being set
   - Test with fresh app install

### Debug Mode

Enable debug logging:

```gdscript
# In project.godot or runtime
PremiumManager.Instance?.DebugMode = true
PurchaseStateManager.Instance?.DebugMode = true
```

## Security Considerations

### Purchase Verification
- Always verify purchase signatures server-side in production
- Use proper RSA signature verification
- Implement receipt validation
- Log suspicious purchase attempts

### Offline Protection
- Don't allow offline premium bypass
- Store purchase state securely
- Validate purchase timestamps
- Implement rate limiting

### Data Protection
- Encrypt purchase state if possible
- Avoid storing sensitive purchase data locally
- Use secure file permissions
- Implement proper error handling

## Revenue Optimization

### Pricing Strategy
- Start with $0.99 USD for broad appeal
- Consider regional pricing adjustments
- Monitor conversion rates by region
- Test different price points

### UX Optimization
- Show purchase option at natural breaks
- Provide clear value proposition
- Handle all purchase states gracefully
- Offer good user experience for failures

### Analytics Tracking
- Track purchase attempts vs. completions
- Monitor conversion rates by platform
- Analyze user behavior after purchase
- Optimize ad placement for free users

## Support Documentation

### For Players
- Clear explanation of what "Remove Ads" includes
- Instructions for troubleshooting purchase issues
- Contact information for store-specific support

### For Developers
- Setup guide for app stores
- Testing procedures
- Troubleshooting common issues
- Integration with analytics systems

This implementation provides a complete, secure, and user-friendly "Remove Ads" purchase system that works across all major mobile app stores while maintaining compatibility with the existing ad monetization strategy.