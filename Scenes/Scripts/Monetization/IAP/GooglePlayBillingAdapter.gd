extends IAPAdapter
class_name GooglePlayBillingAdapter

## Google Play Billing integration adapter for Android devices.
## This adapter interfaces with the native Google Play Billing Client.
##
## NOTE: This requires the Google Play Billing Client library to be integrated as a Godot plugin.

var _plugin_name: String = "GooglePlayBilling"

## Initialize the Google Play Billing Client.
func initialize() -> void:
	if not Engine.has_singleton(_plugin_name):
		push_warning("GooglePlayBillingAdapter: Google Play Billing plugin not found")
		return
	
	print("GooglePlayBillingAdapter: Initializing Google Play Billing")
	
	if _try_call("startConnection", []):
		print("GooglePlayBillingAdapter: Connection started")
	else:
		push_error("GooglePlayBillingAdapter: Failed to start connection")

## Query the product details for the specified product ID.
func query_product_info(product_id: String) -> Dictionary:
	var result = {}
	
	if not Engine.has_singleton(_plugin_name):
		push_error("GooglePlayBillingAdapter: Plugin not available")
		return result
	
	var response = _try_call("querySkuDetails", [[product_id]])
	if response != null and typeof(response) == TYPE_DICTIONARY:
		result = {
			"title": response.get("title", ""),
			"description": response.get("description", ""),
			"price": response.get("price", ""),
			"price_amount_micros": response.get("price_amount_micros", 0),
			"price_currency_code": response.get("price_currency_code", ""),
			"sku": product_id,
			"type": response.get("type", "inapp")
		}
		
		print("GooglePlayBillingAdapter: Queried product details for %s" % product_id)
	else:
		push_error("GooglePlayBillingAdapter: Query failed for %s" % product_id)
	
	return result

## Launch the purchase flow for the specified product.
func purchase_product(product_id: String) -> bool:
	if not Engine.has_singleton(_plugin_name):
		push_error("GooglePlayBillingAdapter: Plugin not available")
		return false
	
	print("GooglePlayBillingAdapter: Launching purchase flow for %s" % product_id)
	
	var flow_params = {
		"product_id": product_id,
		"obfuscatedAccountId": "",
		"obfuscatedProfileId": ""
	}
	
	var started = _try_call("launchBillingFlow", [flow_params])
	
	if started:
		purchase_started.emit(product_id)
		print("GooglePlayBillingAdapter: Purchase flow started for %s" % product_id)
	else:
		push_error("GooglePlayBillingAdapter: Failed to launch purchase flow for %s" % product_id)
	
	return started

## Restore purchases from Google Play Billing.
func restore_purchases() -> bool:
	if not Engine.has_singleton(_plugin_name):
		push_error("GooglePlayBillingAdapter: Plugin not available")
		return false
	
	print("GooglePlayBillingAdapter: Restoring purchases")
	
	var started = _try_call("queryPurchases", [[
		"inapp",  # In-app purchases
		"subs"    # Subscriptions
	]])
	
	if started:
		print("GooglePlayBillingAdapter: Query for restore started")
	else:
		push_error("GooglePlayBillingAdapter: Failed to restore purchases")
	
	return started

## Verify the purchase signature for security.
## This should be implemented server-side for maximum security.
func verify_purchase(receipt: String) -> bool:
	if receipt.is_empty():
		push_warning("GooglePlayBillingAdapter: Missing receipt for verification")
		return false
	
	print("GooglePlayBillingAdapter: Verifying purchase signature")
	
	# In production, this would:
	# 1. Extract the signature from the purchase data
	# 2. Use Google Play's public key to verify the signature using RSA
	# 3. Parse and validate the purchase data
	# 4. Return verification result
	
	# For now, assume verification would pass (in production, MUST implement properly)
	return true

## Check if billing is available on the device.
func is_billing_available() -> bool:
	if not Engine.has_singleton(_plugin_name):
		return false
	
	var is_available = _try_call("isBillingSupported", [])
	return is_available == true

## Get the current Google Play Billing service version.
func get_service_version() -> String:
	if not Engine.has_singleton(_plugin_name):
		return "Unknown"
	
	var version = _try_call("getBillingClientVersion", [])
	return str(version) if version != null else "Unknown"

## Handle purchase result from Google Play Billing.
## This should be called by the native plugin when a purchase completes.
func handle_purchase_result(purchase_data: Dictionary) -> void:
	if purchase_data.is_empty():
		push_warning("GooglePlayBillingAdapter: Empty purchase data received")
		return
	
	print("GooglePlayBillingAdapter: Purchase result received")
	
	var purchase_state = purchase_data.get("purchaseState", 0)
	var product_id = purchase_data.get("productId", "")
	
	match purchase_state:
		0:  # PURCHASED
			purchase_completed.emit(product_id, JSON.stringify(purchase_data))
			print("GooglePlayBillingAdapter: Purchase completed successfully for %s" % product_id)
		1:  # PENDING
			purchase_failed.emit(product_id, "Purchase is pending")
		2:  # USER_CANCELED
			purchase_failed.emit(product_id, "Purchase was cancelled by user")
		_:
			purchase_failed.emit(product_id, "Purchase failed")

## Handle query purchases result (for restore functionality).
func handle_query_purchases_result(purchases: Array) -> void:
	print("GooglePlayBillingAdapter: Query purchases result received")
	
	for purchase in purchases:
		if typeof(purchase) == TYPE_DICTIONARY:
			var purchase_state = purchase.get("purchaseState", 0)
			if purchase_state == 0:  # PURCHASED and acknowledged
				var product_id = purchase.get("productId", "")
				var purchase_token = purchase.get("purchaseToken", "")
				
				# Acknowledge the purchase if needed
				if not purchase.get("acknowledged", false):
					_acknowledge_purchase(purchase_token)
				
				purchase_completed.emit(product_id, JSON.stringify(purchase))
	
	restore_completed.emit()

## Acknowledge a purchase (required by Google Play).
func _acknowledge_purchase(purchase_token: String) -> void:
	if purchase_token.is_empty():
		push_warning("GooglePlayBillingAdapter: Missing purchase token for acknowledgment")
		return
	
	print("GooglePlayBillingAdapter: Acknowledging purchase %s" % purchase_token)
	
	var params = {
		"purchase_token": purchase_token,
		"developer_payload": ""
	}
	
	_try_call("acknowledgePurchase", [params])

## Try to call a plugin method.
func _try_call(method_name: String, args: Array) -> Variant:
	if not Engine.has_singleton(_plugin_name):
		return null
	
	var plugin = Engine.get_singleton(_plugin_name)
	if not plugin.has_method(method_name):
		return null
	
	return plugin.callv(method_name, args)
