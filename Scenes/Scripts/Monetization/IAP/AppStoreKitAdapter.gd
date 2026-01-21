extends IAPAdapter
class_name AppStoreKitAdapter

## App Store (StoreKit 2) integration adapter for iOS devices.
## This adapter interfaces with StoreKit 2 framework.
##
## NOTE: This requires the StoreKit2 framework to be integrated as a Godot plugin.

var _plugin_name: String = "StoreKit"

## Initialize StoreKit 2 for in-app purchases.
func initialize() -> void:
	if not Engine.has_singleton(_plugin_name):
		push_warning("AppStoreKitAdapter: StoreKit plugin not found")
		return
	
	print("AppStoreKitAdapter: Initializing StoreKit 2")
	
	if _try_call("start", []):
		print("AppStoreKitAdapter: StoreKit started")
	else:
		push_error("AppStoreKitAdapter: Failed to start StoreKit")

## Request product information for specified product ID.
func query_product_info(product_id: String) -> Dictionary:
	var result = {}
	
	if not Engine.has_singleton(_plugin_name):
		push_error("AppStoreKitAdapter: Plugin not available")
		return result
	
	var response = _try_call("get_product_info", [[product_id]])
	if response != null and typeof(response) == TYPE_DICTIONARY:
		result = {
			"id": product_id,
			"displayName": response.get("localizedTitle", ""),
			"description": response.get("localizedDescription", ""),
			"displayPrice": response.get("localizedPriceString", ""),
			"priceLocale": response.get("priceLocale", ""),
			"type": "NonConsumable"
		}
		
		print("AppStoreKitAdapter: Requested product info for %s" % product_id)
	else:
		push_error("AppStoreKitAdapter: Product info request failed for %s" % product_id)
	
	return result

## Launches purchase flow using StoreKit 2.
func purchase_product(product_id: String) -> bool:
	if not Engine.has_singleton(_plugin_name):
		push_error("AppStoreKitAdapter: Plugin not available")
		return false
	
	print("AppStoreKitAdapter: Starting purchase for %s" % product_id)
	
	var started = _try_call("purchase", [[product_id]])
	
	if started:
		purchase_started.emit(product_id)
		print("AppStoreKitAdapter: Purchase flow started for %s" % product_id)
	else:
		push_error("AppStoreKitAdapter: Failed to start purchase flow for %s" % product_id)
	
	return started

## Restore purchases from the App Store.
func restore_purchases() -> bool:
	if not Engine.has_singleton(_plugin_name):
		push_error("AppStoreKitAdapter: Plugin not available")
		return false
	
	print("AppStoreKitAdapter: Restoring purchases")
	
	var started = _try_call("restore", [])
	
	if started:
		print("AppStoreKitAdapter: Restore started")
	else:
		push_error("AppStoreKitAdapter: Failed to restore purchases")
	
	return started

## Verify a purchase receipt for security.
## In production, this should be done server-side with Apple's validation servers.
func verify_purchase(receipt: String) -> bool:
	if receipt.is_empty():
		push_warning("AppStoreKitAdapter: Missing receipt for verification")
		return false
	
	print("AppStoreKitAdapter: Verifying purchase with App Store")
	
	# In production, this would:
	# 1. Send the receipt to Apple's validation server
	# 2. Get verification response
	# 3. Check if the receipt is valid and not expired
	# 4. Return verification result
	
	# For now, assume verification would pass (in production, MUST implement properly)
	return true

## Check if user is authorized to make payments.
func can_make_payments() -> bool:
	if not Engine.has_singleton(_plugin_name):
		return false
	
	var can_pay = _try_call("canMakePayments", [])
	return can_pay == true

## Get the current StoreKit version.
func get_service_version() -> String:
	return "2.0"  # StoreKit 2

## Handle purchase result callback from StoreKit 2.
## This should be called by the native plugin when a purchase completes.
func handle_purchase_result(result: Dictionary) -> void:
	if result.is_empty():
		push_warning("AppStoreKitAdapter: Empty purchase result received")
		return
	
	print("AppStoreKitAdapter: Purchase result received")
	
	var product_id = result.get("productId", "")
	var status = result.get("status", "")
	
	match status:
		"success", "purchased":
			purchase_completed.emit(product_id, JSON.stringify(result))
			print("AppStoreKitAdapter: Purchase completed successfully for %s" % product_id)
		"pending":
			purchase_failed.emit(product_id, "Purchase is being processed by Apple. Please check your purchase history.")
		"userCancelled", "cancelled":
			purchase_failed.emit(product_id, "Purchase was cancelled by user")
		"failed":
			purchase_failed.emit(product_id, "Purchase failed")
		_:
			purchase_failed.emit(product_id, "Purchase failed")

## Handle restore purchases result.
func handle_restore_result(purchases: Array) -> void:
	print("AppStoreKitAdapter: Restore result received")
	
	for purchase in purchases:
		if typeof(purchase) == TYPE_DICTIONARY:
			var product_id = purchase.get("productId", "")
			var transaction_id = purchase.get("transactionId", "")
			
			# Finish the transaction
			if not purchase.get("finished", false):
				_finish_transaction(transaction_id)
			
			purchase_completed.emit(product_id, JSON.stringify(purchase))
	
	restore_completed.emit()

## Finish a transaction after successful purchase.
func _finish_transaction(transaction_id: String) -> void:
	if transaction_id.is_empty():
		push_warning("AppStoreKitAdapter: Missing transaction ID for finishing")
		return
	
	print("AppStoreKitAdapter: Finishing transaction %s" % transaction_id)
	
	_try_call("finishTransaction", [[transaction_id]])

## Try to call a plugin method.
func _try_call(method_name: String, args: Array) -> Variant:
	if not Engine.has_singleton(_plugin_name):
		return null
	
	var plugin = Engine.get_singleton(_plugin_name)
	if not plugin.has_method(method_name):
		return null
	
	return plugin.callv(method_name, args)
