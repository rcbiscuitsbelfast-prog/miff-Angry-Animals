extends Node
class_name IAPAdapter

## Base class for all IAP adapters.
## Platform-specific implementations should inherit from this.

signal purchase_started(product_id: String)
signal purchase_completed(product_id: String, receipt: String)
signal purchase_failed(product_id: String, reason: String)
signal restore_completed
signal restore_failed(reason: String)

## Initialize the IAP adapter for the platform.
func initialize() -> void:
	push_error("IAPAdapter.initialize() not implemented. Override in subclass.")

## Query product information from the store.
func query_product_info(product_id: String) -> Dictionary:
	return {}

## Start a purchase flow for a product.
func purchase_product(product_id: String) -> bool:
	return false

## Restore previous purchases.
func restore_purchases() -> bool:
	return false

## Check if billing is available on this platform.
func is_billing_available() -> bool:
	return false

## Verify a purchase receipt with server (optional).
func verify_purchase(receipt: String) -> bool:
	return false

## Get the billing service version.
func get_service_version() -> String:
	return "Unknown"
