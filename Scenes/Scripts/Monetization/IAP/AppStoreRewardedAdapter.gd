extends Node
class_name AppStoreRewardedAdapter

## iOS adapter for App Store rewarded ads.
## Handles SKAdNetwork rewarded video integration.

var ad_unit_id: String = "ca-app-pub-6675121744131727/8406522837"
var _ad_mob_plugin: Object = null

func _ready() -> void:
	_ad_mob_plugin = null
	if Engine.has_singleton("AdMob"):
		_ad_mob_plugin = Engine.get_singleton("AdMob")

## Load rewarded ad from AdMob.
func load_rewarded_ad() -> void:
	if _ad_mob_plugin == null:
		return
	
	print("AppStoreRewardedAdapter: Loading rewarded ad %s" % ad_unit_id)
	_ad_mob_plugin.call("load_rewarded_ad", [ad_unit_id])

## Show rewarded ad from AdMob.
func show_rewarded_ad() -> void:
	if _ad_mob_plugin == null:
		return
	
	print("AppStoreRewardedAdapter: Showing rewarded ad")
	_ad_mob_plugin.call("show_rewarded_ad", [ad_unit_id])

## Check if rewarded ad is loaded.
func is_ad_loaded() -> bool:
	if _ad_mob_plugin == null:
		return false
	
	var loaded = _ad_mob_plugin.call("is_rewarded_ad_loaded")
	return bool(loaded) if loaded != null else false

## Callbacks from native code would be handled here or via AdsManager.
