extends Node
class_name MonetizationManager

## Global monetization manager responsible for in-app purchases and monetization state.
## Intended to integrate with platform billing plugins (StoreKit2 / Google Play Billing).

signal purchase_succeeded
signal purchase_failed(reason: String)
signal purchase_restored

var ios_product_id: String = "full_game_unlock"
var android_product_id: String = "full_game_unlock"
var _billing_plugin: Object = null
var _initialized: bool = false

static var instance: MonetizationManager = null

## Returns whether the full game is unlocked.
## This value is persisted in user://profile.json via PlayerProfile.
func get_is_full_game_unlocked() -> bool:
	var player_profile = get_node_or_null("/root/PlayerProfile")
	if player_profile:
		return player_profile.is_full_game_unlocked
	return false

## Returns whether ads should be shown for the current player.
func get_show_ads() -> bool:
	return not get_is_full_game_unlocked()

func _ready() -> void:
	instance = self
	process_mode = Node.PROCESS_MODE_ALWAYS
	call_deferred("_deferred_initialize_and_restore")

## Initializes the billing integration.
func initialize(ios_product_id_param: String, android_product_id_param: String) -> void:
	ios_product_id = ios_product_id_param if ios_product_id_param else ios_product_id
	android_product_id = android_product_id_param if android_product_id_param else android_product_id
	
	_billing_plugin = _find_billing_plugin_singleton()
	_initialized = _billing_plugin != null and _is_platform_supported()
	
	if not _initialized:
		_billing_plugin = null
		return
	
	_try_call_plugin("initialize")
	_try_call_plugin("init")
	_try_call_plugin("connect")

## Triggers the full-game purchase flow.
func purchase_full_game() -> void:
	if get_is_full_game_unlocked():
		purchase_succeeded.emit()
		return
	
	if not _is_platform_supported():
		purchase_failed.emit("In-app purchases are not supported on this platform.")
		return
	
	if not _initialized:
		initialize(ios_product_id, android_product_id)
	
	if not _initialized or _billing_plugin == null:
		purchase_failed.emit("Billing unavailable. Please try again later.")
		return
	
	var product_id = _get_platform_product_id()
	if product_id.is_empty():
		purchase_failed.emit("Product not configured.")
		return
	
	var started = (_try_call_plugin("purchase", [product_id]) or
				_try_call_plugin("purchase_product", [product_id]) or
				_try_call_plugin("purchaseProduct", [product_id]) or
				_try_call_plugin("buy", [product_id]) or
				_try_call_plugin("buy_product", [product_id]))
	
	if not started:
		purchase_failed.emit("Billing plugin does not support purchasing.")
		return
	
	await _wait_for_purchase_result_or_timeout_async(20.0)

## Restores purchases on startup.
func restore_purchases() -> void:
	if not _is_platform_supported():
		if get_is_full_game_unlocked():
			purchase_restored.emit()
		return
	
	if not _initialized:
		initialize(ios_product_id, android_product_id)
	
	if not _initialized or _billing_plugin == null:
		if get_is_full_game_unlocked():
			purchase_restored.emit()
		return
	
	var started = (_try_call_plugin("restore") or
				_try_call_plugin("restore_purchases") or
				_try_call_plugin("restorePurchases") or
				_try_call_plugin("query_purchases") or
				_try_call_plugin("queryPurchases"))
	
	if not started:
		if get_is_full_game_unlocked():
			purchase_restored.emit()
		return
	
	await _wait_for_restore_or_timeout_async(10.0)
	
	if get_is_full_game_unlocked():
		purchase_restored.emit()

## Callback hook for platform plugins to mark the purchase as successful.
func notify_purchase_succeeded() -> void:
	unlock_full_game()
	purchase_succeeded.emit()

## Callback hook for platform plugins to report purchase failure/cancellation.
func notify_purchase_failed(reason: String) -> void:
	var failure_reason = reason if reason else "Purchase failed."
	purchase_failed.emit(failure_reason)

## Unlocks the full game locally and persists the state in the player profile.
func unlock_full_game() -> void:
	var player_profile = get_node_or_null("/root/PlayerProfile")
	if player_profile == null:
		push_warning("MonetizationManager: PlayerProfile not ready; cannot persist unlock state.")
		return
	
	if player_profile.is_full_game_unlocked:
		return
	
	player_profile.is_full_game_unlocked = true
	player_profile.save_profile()
	
	var ads_manager = get_node_or_null("/root/AdsManager")
	if ads_manager:
		ads_manager.hide_banner_ad()

## Private methods

func _deferred_initialize_and_restore() -> void:
	initialize(ios_product_id, android_product_id)
	restore_purchases()

func _is_platform_supported() -> bool:
	var os_name = OS.get_name()
	return os_name == "Android" or os_name == "iOS"

func _get_platform_product_id() -> String:
	var os_name = OS.get_name()
	return ios_product_id if os_name == "iOS" else android_product_id

func _find_billing_plugin_singleton() -> Object:
	var os_name = OS.get_name()
	var candidates = []
	
	if os_name == "iOS":
		candidates = ["StoreKit", "StoreKit2", "InAppPurchase", "InAppPurchases", "GodotInAppPurchase"]
	else:
		candidates = ["GooglePlayBilling", "GodotGooglePlayBilling", "GodotGooglePlay", "InAppPurchase", "InAppPurchases"]
	
	for name in candidates:
		if Engine.has_singleton(name):
			return Engine.get_singleton(name)
	
	return null

func _try_call_plugin(method_name: String, args: Array = []) -> bool:
	if _billing_plugin == null:
		return false
	
	if not _billing_plugin.has_method(method_name):
		return false
	
	_billing_plugin.callv(method_name, args)
	return true

func _wait_for_purchase_result_or_timeout_async(timeout_seconds: float) -> void:
	var timer = Timer.new()
	timer.wait_time = timeout_seconds
	timer.one_shot = true
	timer.autostart = true
	add_child(timer)
	
	await Promise.any([
		Promise.from_signal(self, "purchase_succeeded"),
		Promise.from_signal(self, "purchase_failed"),
		Promise.from_signal(timer, "timeout")
	])
	
	timer.queue_free()
	
	if not get_is_full_game_unlocked():
		purchase_failed.emit("Purchase timed out. Please try again.")

func _wait_for_restore_or_timeout_async(timeout_seconds: float) -> void:
	var timer = Timer.new()
	timer.wait_time = timeout_seconds
	timer.one_shot = true
	timer.autostart = true
	add_child(timer)
	
	var restored_promise = Promise.from_signal(self, "purchase_restored")
	await Promise.any([restored_promise, Promise.from_signal(timer, "timeout")])
	
	timer.queue_free()
	
	if not restored_promise.is_completed():
		purchase_restored.emit()
