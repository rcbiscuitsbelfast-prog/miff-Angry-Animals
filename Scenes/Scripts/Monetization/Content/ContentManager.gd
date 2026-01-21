extends Node
class_name ContentManager

## Content delivery and caching manager for cosmetics and downloadable content.
## Handles remote content fetching, local caching, and asset management.

signal content_loaded(content_id: String)
signal content_failed(content_id: String, reason: String)
signal cache_updated(total_size_mb: float)

var _content_cache: Dictionary = {}
var _cache_max_size_mb: float = 100.0
var _use_remote_content: bool = true
var _content_base_url: String = ""
var _enable_caching: bool = true

static var instance: ContentManager = null

const CACHE_PATH = "user://content_cache/"
const METADATA_PATH = "user://content_metadata.json"

func _ready() -> void:
	instance = self
	process_mode = Node.PROCESS_MODE_ALWAYS
	
	_load_cache_metadata()
	_cleanup_old_cache()

## Fetch content from remote server or local cache
func fetch_content(content_id: String) -> void:
	if _content_cache.has(content_id):
		print("ContentManager: %s found in cache" % content_id)
		content_loaded.emit(content_id)
		return
	
	if not _use_remote_content:
		push_warning("ContentManager: Remote content disabled, %s not available" % content_id)
		content_failed.emit(content_id, "Remote content disabled")
		return
	
	print("ContentManager: Fetching %s from remote..." % content_id)
	await _fetch_remote_content(content_id)

## Load content from local cache
func load_cached_content(content_id: String) -> Dictionary:
	if not _content_cache.has(content_id):
		return {}
	
	var cache_entry = _content_cache[content_id]
	return cache_entry.duplicate()

## Check if content is available locally
func is_content_cached(content_id: String) -> bool:
	return _content_cache.has(content_id)

## Preload content for faster access
func preload_content(content_id: String) -> void:
	if _content_cache.has(content_id):
		var cache_entry = _content_cache[content_id]
		if cache_entry.has("asset_path"):
			var asset_path = cache_entry["asset_path"]
			print("ContentManager: Preloading %s from %s" % [content_id, asset_path])
			
			# Preload the asset
			if ResourceLoader.exists(asset_path):
				ResourceLoader.load_threaded(asset_path)

## Clear content cache
func clear_cache() -> void:
	_content_cache.clear()
	_save_cache_metadata()
	
	# Delete cached files
	var dir = DirAccess.open(CACHE_PATH)
	if dir:
		dir.list_dir_begin()
		var file_name = dir.get_next()
		while file_name != "":
			dir.remove(file_name)
			file_name = dir.get_next()
		dir.list_dir_end()
	
	cache_updated.emit(0.0)
	print("ContentManager: Cache cleared")

## Get cache statistics
func get_cache_stats() -> Dictionary:
	var total_size = 0
	var file_count = 0
	
	for content_id in _content_cache:
		var cache_entry = _content_cache[content_id]
		if cache_entry.has("size_mb"):
			total_size += cache_entry["size_mb"]
			file_count += 1
	
	return {
		"total_size_mb": total_size,
		"file_count": file_count,
		"max_size_mb": _cache_max_size_mb,
		"usage_percent": (total_size / _cache_max_size_mb) * 100 if _cache_max_size_mb > 0 else 0
	}

## Set remote content base URL
func set_content_base_url(url: String) -> void:
	_content_base_url = url

## Enable/disable remote content fetching
func set_use_remote_content(enabled: bool) -> void:
	_use_remote_content = enabled

## Enable/disable caching
func set_enable_caching(enabled: bool) -> void:
	_enable_caching = enabled

## Private methods

func _fetch_remote_content(content_id: String) -> void:
	if _content_base_url.is_empty():
		content_failed.emit(content_id, "Content base URL not configured")
		return
	
	var url = "%s/%s" % [_content_base_url, content_id]
	print("ContentManager: Fetching from %s" % url)
	
	# In production, this would use HTTPRequest to fetch content
	# For now, simulate the fetch
	await get_tree().create_timer(1.0).timeout
	
	# Simulate successful fetch
	_add_to_cache(content_id, {
		"content_id": content_id,
		"url": url,
		"downloaded_at": Time.get_datetime_dict_from_system(),
		"size_mb": 1.0  # Simulated size
	})
	
	content_loaded.emit(content_id)

func _add_to_cache(content_id: String, metadata: Dictionary) -> void:
	_content_cache[content_id] = metadata
	_save_cache_metadata()
	
	if _enable_caching:
		var stats = get_cache_stats()
		cache_updated.emit(stats.get("total_size_mb", 0))

func _load_cache_metadata() -> void:
	if not FileAccess.file_exists(METADATA_PATH):
		return
	
	var file = FileAccess.open(METADATA_PATH, FileAccess.READ)
	if file:
		var json_string = file.get_as_text()
		file.close()
		
		if not json_string.is_empty():
			var json = JSON.new()
			var error = json.parse(json_string)
			if error == OK and typeof(json.data) == TYPE_DICTIONARY:
				_content_cache = json.data

func _save_cache_metadata() -> void:
	var file = FileAccess.open(METADATA_PATH, FileAccess.WRITE)
	if file:
		file.store_string(JSON.stringify(_content_cache, "\t"))
		file.close()

func _cleanup_old_cache() -> void:
	# Remove cache entries older than 30 days
	var now = Time.get_unix_time_from_system()
	var thirty_days_seconds = 30 * 24 * 60 * 60
	
	var expired_ids = []
	for content_id in _content_cache:
		var cache_entry = _content_cache[content_id]
		if cache_entry.has("downloaded_at"):
			var downloaded_time = _parse_datetime(cache_entry["downloaded_at"])
			var cache_age = now - downloaded_time
			
			if cache_age > thirty_days_seconds:
				expired_ids.append(content_id)
	
	# Remove expired entries from cache
	for content_id in expired_ids:
		_remove_from_cache(content_id)
	
	if expired_ids.size() > 0:
		print("ContentManager: Cleaned up %d expired cache entries" % expired_ids.size())

func _remove_from_cache(content_id: String) -> void:
	_content_cache.erase(content_id)
	_save_cache_metadata()
	
	# Delete cached files
	var cache_entry = _content_cache.get(content_id, {})
	if cache_entry.has("asset_path"):
		var asset_path = cache_entry["asset_path"]
		if FileAccess.file_exists(asset_path):
			DirAccess.remove_absolute(asset_path)

func _parse_datetime(datetime_dict: Dictionary) -> int:
	if typeof(datetime_dict) != TYPE_DICTIONARY:
		return 0
	
	var year = datetime_dict.get("year", 1970)
	var month = datetime_dict.get("month", 1)
	var day = datetime_dict.get("day", 1)
	var hour = datetime_dict.get("hour", 0)
	var minute = datetime_dict.get("minute", 0)
	var second = datetime_dict.get("second", 0)
	
	var time = Time.get_unix_time_from_datetime_dict({
		"year": year,
		"month": month,
		"day": day,
		"hour": hour,
		"minute": minute,
		"second": second
	})
	
	return time
