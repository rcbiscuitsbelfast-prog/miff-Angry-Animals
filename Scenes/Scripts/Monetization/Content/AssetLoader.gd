extends Node
class_name AssetLoader

## Asset loading utility for cosmetics and content.
## Provides efficient loading, error handling, and progress tracking.

signal asset_loaded(asset_path: String, asset: Resource)
signal asset_failed(asset_path: String, reason: String)
signal loading_progress(asset_path: String, percent: float)

var _loaded_assets: Dictionary = {}
var _loading_assets: Dictionary = {}
var _enable_threaded_loading: bool = true
var _max_concurrent_loads: int = 3

static var instance: AssetLoader = null

func _ready() -> void:
	instance = self
	process_mode = Node.PROCESS_MODE_ALWAYS

## Load an asset asynchronously
func load_asset_async(asset_path: String) -> void:
	if asset_path.is_empty():
		asset_failed.emit(asset_path, "Asset path is empty")
		return
	
	if _loaded_assets.has(asset_path):
		print("AssetLoader: %s already loaded" % asset_path)
		asset_loaded.emit(asset_path, _loaded_assets[asset_path])
		return
	
	if _loading_assets.has(asset_path):
		print("AssetLoader: %s already loading" % asset_path)
		return
	
	# Check loading limit
	if _loading_assets.size() >= _max_concurrent_loads:
		print("AssetLoader: Concurrent load limit reached, queueing: %s" % asset_path)
		await _wait_for_load_slot()
	
	_loading_assets[asset_path] = true
	print("AssetLoader: Loading %s..." % asset_path)
	
	if _enable_threaded_loading:
		await _load_threaded_async(asset_path)
	else:
		await _load_sync_async(asset_path)

## Load an asset synchronously (blocks thread)
func load_asset_sync(asset_path: String) -> Resource:
	if asset_path.is_empty():
		return null
	
	if _loaded_assets.has(asset_path):
		return _loaded_assets[asset_path]
	
	print("AssetLoader: Loading %s (sync)..." % asset_path)
	
	var asset = ResourceLoader.load(asset_path)
	
	if asset == null:
		push_error("AssetLoader: Failed to load %s" % asset_path)
		asset_failed.emit(asset_path, "Failed to load resource")
		return null
	
	_loaded_assets[asset_path] = asset
	print("AssetLoader: Loaded %s (sync)" % asset_path)
	return asset

## Load multiple assets asynchronously
func load_assets_async(asset_paths: Array) -> Array:
	var results = []
	
	for asset_path in asset_paths:
		var asset = await _load_and_track(asset_path)
		results.append(asset)
	
	return results

## Load and track asset with progress
func _load_and_track(asset_path: String) -> void:
	var start_time = Time.get_ticks_msec()
	
	_loading_assets[asset_path] = true
	loading_progress.emit(asset_path, 0.0)
	
	# Simulate progress updates
	var progress_steps = [0.1, 0.25, 0.5, 0.75, 1.0]
	for progress in progress_steps:
		loading_progress.emit(asset_path, progress)
		await get_tree().create_timer(0.05).timeout  # 50ms per step
	
	var asset = ResourceLoader.load(asset_path)
	
	var elapsed_ms = Time.get_ticks_msec() - start_time
	_loading_assets.erase(asset_path)
	
	if asset == null:
		push_error("AssetLoader: Failed to load %s" % asset_path)
		asset_failed.emit(asset_path, "Failed to load resource")
		return
	
	_loaded_assets[asset_path] = asset
	print("AssetLoader: Loaded %s in %dms" % [asset_path, elapsed_ms])
	asset_loaded.emit(asset_path, asset)

## Unload an asset to free memory
func unload_asset(asset_path: String) -> void:
	if not _loaded_assets.has(asset_path):
		return
	
	_loaded_assets.erase(asset_path)
	print("AssetLoader: Unloaded %s" % asset_path)

## Unload all assets
func unload_all() -> void:
	var count = _loaded_assets.size()
	_loaded_assets.clear()
	print("AssetLoader: Unloaded %d assets" % count)

## Check if asset is loaded
func is_asset_loaded(asset_path: String) -> bool:
	return _loaded_assets.has(asset_path)

## Check if asset is currently loading
func is_asset_loading(asset_path: String) -> bool:
	return _loading_assets.has(asset_path)

## Get loaded asset
func get_loaded_asset(asset_path: String) -> Resource:
	return _loaded_assets.get(asset_path, null)

## Get all loaded assets
func get_loaded_assets() -> Dictionary:
	return _loaded_assets.duplicate()

## Preload assets (blocking)
func preload_assets(asset_paths: Array) -> void:
	for asset_path in asset_paths:
		ResourceLoader.load_interactive(asset_path, ResourceLoader.CACHE_MODE_REUSE)

## Validate an asset path
func validate_asset_path(asset_path: String) -> bool:
	if asset_path.is_empty():
		return false
	
	if not asset_path.contains("://"):
		return false
	
	var extension = asset_path.get_extension()
	var valid_extensions = ["tscn", "res", "gd", "png", "jpg", "svg", "json"]
	
	return extension.to_lower() in valid_extensions

## Wait for a load slot to become available
func _wait_for_load_slot() -> void:
	while _loading_assets.size() >= _max_concurrent_loads:
		await get_tree().process_frame

## Load asset with threaded loading
func _load_threaded_async(asset_path: String) -> void:
	if not ResourceLoader.exists(asset_path):
		asset_failed.emit(asset_path, "Asset does not exist")
		_loading_assets.erase(asset_path)
		return
	
	var asset = ResourceLoader.load_threaded(asset_path, ResourceLoader.CACHE_MODE_REUSE)
	
	if asset == null:
		push_error("AssetLoader: Failed to load %s (threaded)" % asset_path)
		asset_failed.emit(asset_path, "Failed to load resource")
		_loading_assets.erase(asset_path)
		return
	
	_loaded_assets[asset_path] = asset
	asset_loaded.emit(asset_path, asset)

## Load asset synchronously (wrapped in async for consistency)
func _load_sync_async(asset_path: String) -> void:
	if not ResourceLoader.exists(asset_path):
		asset_failed.emit(asset_path, "Asset does not exist")
		_loading_assets.erase(asset_path)
		return
	
	var asset = ResourceLoader.load(asset_path)
	
	if asset == null:
		push_error("AssetLoader: Failed to load %s (sync)" % asset_path)
		asset_failed.emit(asset_path, "Failed to load resource")
		_loading_assets.erase(asset_path)
		return
	
	_loaded_assets[asset_path] = asset
	asset_loaded.emit(asset_path, asset)

## Get asset info without loading
func get_asset_info(asset_path: String) -> Dictionary:
	var info = {
		"path": asset_path,
		"exists": ResourceLoader.exists(asset_path),
		"loaded": _loaded_assets.has(asset_path),
		"loading": _loading_assets.has(asset_path),
		"valid": validate_asset_path(asset_path)
	}
	
	if info["loaded"] and _loaded_assets[asset_path] != null:
		var asset = _loaded_assets[asset_path]
		if asset:
			info["type"] = asset.get_class()
			info["resource_path"] = asset.resource_path
	
	return info
