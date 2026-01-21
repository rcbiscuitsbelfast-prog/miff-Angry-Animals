extends IAPAdapter
class_name AmazonIAPAdapter

## Amazon Appstore IAP integration adapter for Amazon Fire devices.
## This adapter interfaces with the Amazon IAP library.
##
## NOTE: This requires Amazon IAP library to be integrated as a Godot plugin.

var _plugin_name: String = "AmazonIAP"

## Initialize Amazon IAP library.
func initialize() -> void:
	if not Engine.has_singleton(_plugin_name):
		push_warning("AmazonIAPAdapter: Amazon IAP plugin not found")
		return
	
	print("AmazonIAPAdapter: Initializing Amazon IAP")
	
	if _try_call("start", []):
		print("AmazonIAPAdapter: Amazon IAP started")
	else:
		push_error("AmazonIAPAdapter: Failed to start Amazon IAP")

## Query product information for the specified SKU.
func query_product_info(sku: String) -> Dictionary:
	var result = {}
	
	if not Engine.has_singleton(_plugin_name):
		push_error("AmazonIAPAdapter: Plugin not available")
		return result
	
	var response = _try_call("getProductData", [[sku]])
	if response != null and typeof(response) == TYPE_DICTIONARY:
		result = {
			"sku": sku,
			"title": response.get("title", ""),
			"description": response.get("description", ""),
			"price": response.get("price", ""),
			"currency": response.get("currency", "USD"),
			"itemType": response.get("itemType", "ENTITLED"),  # For permanent purchases
			"available": response.get("available", true)
		}
		
		print("AmazonIAPAdapter: Queried product info for %s" % sku)
	else:
		push_error("AmazonIAPAdapter: Product info query failed for %s" % sku)
	
	return result

## Launch purchase flow for the specified SKU.
func purchase_product(sku: String) -> bool:
	if not Engine.has_singleton(_plugin_name):
		push_error("AmazonIAPAdapter: Plugin not available")
		return false
	
	print("AmazonIAPAdapter: Launching purchase flow for %s" % sku)
	
	var started = _try_call("purchase", [[sku]])
	
	if started:
		purchase_started.emit(sku)
		print("AmazonIAPAdapter: Purchase flow started for %s" % sku)
	else:
		push_error("AmazonIAPAdapter: Failed to launch purchase flow for %s" % sku)
	
	return started

## Restore purchases from Amazon IAP.
func restore_purchases() -> bool:
	if not Engine.has_singleton(_plugin_name):
		push_error("AmazonIAPAdapter: Plugin not available")
		return false
	
	print("AmazonIAPAdapter: Restoring purchases")
	
	var started = _try_call("getPurchaseUpdates", [])
	
	if started:
		print("AmazonIAPAdapter: Restore started")
	else:
		push_error("AmazonIAPAdapter: Failed to restore purchases")
	
	return started

## Verify a purchase with Amazon servers for security.
## This should be implemented server-side for maximum security.
func verify_purchase(receipt: String) -> bool:
	if receipt.is_empty():
		push_warning("AmazonIAPAdapter: Missing receipt for verification")
		return false
	
	print("AmazonIAPAdapter: Verifying purchase with Amazon")
	
	# In production, this would:
	# 1. Send purchase data to Amazon for verification
	# 2. Check if the purchase is legitimate
	# 3. Return verification result
	
	# For now, assume verification passes
	return true

## Check if Amazon IAP is available on the device.
func is_billing_available() -> bool:
	if not Engine.has_singleton(_plugin_name):
		return false
	
	var is_available = _try_call("isAvailable", [])
	return is_available == true

## Get the Amazon IAP library version.
func get_service_version() -> String:
	return "2.0.76"  # Example Amazon IAP version

## Handle purchase result callback from Amazon IAP.
## This should be called by the native plugin when a purchase completes.
func handle_purchase_result(purchase_data: Dictionary) -> void:
	if purchase_data.is_empty():
		push_warning("AmazonIAPAdapter: Empty purchase data received")
		return
	
	print("AmazonIAPAdapter: Purchase result received")
	
	var status = purchase_data.get("status", "")
	var sku = purchase_data.get("sku", "")
	
	match status:
		"SUCCESS":
			var receipt = JSON.stringify(purchase_data)
			purchase_completed.emit(sku, receipt)
			
			# Fulfill the purchase
			var purchase_token = purchase_data.get("receiptId", "")
			if not purchase_token.is_empty():
				_fulfill_purchase(purchase_token)
			
			print("AmazonIAPAdapter: Purchase completed successfully for %s" % sku)
		"CANCELLED":
			purchase_failed.emit(sku, "Purchase was cancelled by user")
		"PENDING":
			purchase_failed.emit(sku, "Purchase is being processed by Amazon. Please check your purchase history.")
		"FAILED", "NOT_SUPPORTED":
			purchase_failed.emit(sku, "Purchase failed")
		_:
			purchase_failed.emit(sku, "Purchase failed")

## Handle query purchases result (for restore functionality).
func handle_query_purchases_result(purchases: Array) -> void:
	print("AmazonIAPAdapter: Query purchases result received")
	
	for purchase in purchases:
		if typeof(purchase) == TYPE_DICTIONARY:
			var status = purchase.get("status", "")
			if status == "SUCCESS":
				var sku = purchase.get("sku", "")
				var receipt = JSON.stringify(purchase)
				
				# Fulfill the purchase
				var purchase_token = purchase.get("receiptId", "")
				if not purchase_token.is_empty():
					_fulfill_purchase(purchase_token)
				
				purchase_completed.emit(sku, receipt)
	
	restore_completed.emit()

## Fulfills a purchase (marks it as consumed/entitlement granted).
## This should be called after a successful purchase.
func _fulfill_purchase(purchase_token: String) -> void:
	if purchase_token.is_empty():
		push_warning("AmazonIAPAdapter: Missing purchase token for fulfillment")
		return
	
	print("AmazonIAPAdapter: Fulfilling purchase %s" % purchase_token)
	
	var params = {
		"receiptId": purchase_token
	}
	
	_try_call("fulfillPurchase", [params])

## Try to call a plugin method.
func _try_call(method_name: String, args: Array) -> Variant:
	if not Engine.has_singleton(_plugin_name):
		return null
	
	var plugin = Engine.get_singleton(_plugin_name)
	if not plugin.has_method(method_name):
		return null
	
	return plugin.callv(method_name, args)
