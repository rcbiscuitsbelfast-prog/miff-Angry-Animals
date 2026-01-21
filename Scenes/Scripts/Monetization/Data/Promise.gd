## Promise utility class for async operations in GDScript.
## Provides Promise-like functionality for combining multiple signals.

extends RefCounted

class_name Promise

var _completed: bool = false
var _result: Variant = null
var _error: String = ""
var _target: Object = null
var _signal_name: String = ""
var _callbacks: Array = []

## Create a promise from a signal.
static func from_signal(target: Object, signal_name: String) -> Promise:
	var promise = Promise.new()
	promise._target = target
	promise._signal_name = signal_name
	
	target.connect(signal_name, func(args):
		promise._completed = true
		promise._result = args
		promise._trigger_callbacks()
	, Object.CONNECT_ONE_SHOT)
	
	return promise

## Create a promise that completes immediately with a value.
static func resolved(value: Variant = null) -> Promise:
	var promise = Promise.new()
	promise._completed = true
	promise._result = value
	return promise

## Create a promise that fails immediately with an error.
static func rejected(error: String) -> Promise:
	var promise = Promise.new()
	promise._completed = true
	promise._error = error
	return promise

## Wait for any of the promises/signal promises to complete.
static func any(promises: Array) -> Promise:
	var combined = Promise.new()
	
	for item in promises:
		if typeof(item) == TYPE_OBJECT and item.get_script() == Promise:
			var promise = item as Promise
			if promise._completed:
				combined._completed = true
				combined._result = promise._result
				return combined
			else:
				var callback = func():
					if not combined._completed:
						combined._completed = true
						combined._result = promise._result
						combined._trigger_callbacks()
				
				promise._callbacks.append(callback)
	
	return combined

## Create a promise that completes after a delay.
static func delay(seconds: float) -> Promise:
	var promise = Promise.new()
	var timer = Timer.new()
	timer.wait_time = seconds
	timer.one_shot = true
	timer.autostart = true
	
	var main_loop = Engine.get_main_loop()
	if main_loop and main_loop is SceneTree:
		main_loop.root.add_child(timer)
		
		timer.timeout.connect(func():
			promise._completed = true
			promise._trigger_callbacks()
			timer.queue_free()
		)
	else:
			promise._completed = true
	
	return promise

## Check if promise is completed.
func is_completed() -> bool:
	return _completed

## Get the result value.
func get_result() -> Variant:
	return _result

## Get the error if promise failed.
func get_error() -> String:
	return _error

## Then-like chaining.
func then(callback: Callable) -> Promise:
	var chained = Promise.new()
	
	if _completed:
		chained._result = callback.call(_result)
		chained._completed = true
	else:
		var callback_wrapper = func():
			chained._result = callback.call(_result)
			chained._completed = true
			chained._trigger_callbacks()
		
		_callbacks.append(callback_wrapper)
	
	return chained

## Catch-like chaining for errors.
func catch(callback: Callable) -> Promise:
	var chained = Promise.new()
	
	if _completed:
		chained._result = callback.call(_error)
		chained._completed = true
	else:
		var callback_wrapper = func():
			chained._result = callback.call(_error)
			chained._completed = true
			chained._trigger_callbacks()
		
		_callbacks.append(callback_wrapper)
	
	return chained

func _trigger_callbacks() -> void:
	for callback in _callbacks:
		callback.call()
	_callbacks.clear()
