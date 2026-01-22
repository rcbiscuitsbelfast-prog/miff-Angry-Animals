extends Control

## SeasonalEventScreen - UI for displaying and managing seasonal events.
## Shows active events, cosmetics, progress, and allows interaction.

signal event_selected(event_id: String)
signal cosmetics_unlocked()
signal dismissed()

@export var main_container: NodePath
@export var event_carousel_container: NodePath
@export var event_details_container: NodePath
@export var current_event_title: NodePath
@export var event_description: NodePath
@export var event_countdown: NodePath
@export var event_progress_bar: NodePath
@export var cosmetics_grid_container: NodePath
@export var unlock_cosmetics_button: NodePath
@export var dismiss_button: NodePath
@export var event_background: NodePath

var _main_container: VBoxContainer
var _event_carousel_container: ScrollContainer
var _event_details_container: VBoxContainer
var _current_event_title: Label
var _event_description: RichTextLabel
var _event_countdown: Label
var _event_progress_bar: ProgressBar
var _cosmetics_grid_container: GridContainer
var _unlock_cosmetics_button: Button
var _dismiss_button: Button
var _event_background: TextureRect

var _current_events: Array = []
var _selected_event_index: int = 0
var _event_data: Dictionary = {}

func _ready():
	initialize_seasonal_screen()
	load_events()
	display_events()

func initialize_seasonal_screen():
	# Initialize UI elements
	_main_container = get_node_or_null(main_container) as VBoxContainer
	_event_carousel_container = get_node_or_null(event_carousel_container) as ScrollContainer
	_event_details_container = get_node_or_null(event_details_container) as VBoxContainer
	_current_event_title = get_node_or_null(current_event_title) as Label
	_event_description = get_node_or_null(event_description) as RichTextLabel
	_event_countdown = get_node_or_null(event_countdown) as Label
	_event_progress_bar = get_node_or_null(event_progress_bar) as ProgressBar
	_cosmetics_grid_container = get_node_or_null(cosmetics_grid_container) as GridContainer
	_unlock_cosmetics_button = get_node_or_null(unlock_cosmetics_button) as Button
	_dismiss_button = get_node_or_null(dismiss_button) as Button
	_event_background = get_node_or_null(event_background) as TextureRect
	
	# Connect button signals
	if _unlock_cosmetics_button:
		_unlock_cosmetics_button.pressed.connect(_on_unlock_cosmetics_pressed)
	
	if _dismiss_button:
		_dismiss_button.pressed.connect(_on_dismiss_pressed)

func load_events():
	# Load available seasonal events
	_current_events = [
		{
			"id": "winter_wonderland",
			"name": "Winter Wonderland",
			"description": "Bundle up for a frosty adventure! Unlock exclusive ice-themed cosmetics and special winter effects.",
			"start_date": "2024-12-01",
			"end_date": "2025-01-15",
			"progress": 65,
			"cosmetics_unlocked": 2,
			"total_cosmetics": 5,
			"background_color": Color(0.2, 0.6, 1.0, 0.3)
		},
		{
			"id": "spring_celebration",
			"name": "Spring Celebration",
			"description": "Welcome spring with beautiful floral cosmetics and nature-themed effects!",
			"start_date": "2025-03-20",
			"end_date": "2025-04-30",
			"progress": 25,
			"cosmetics_unlocked": 1,
			"total_cosmetics": 4,
			"background_color": Color(0.3, 1.0, 0.3, 0.3)
		},
		{
			"id": "summer_fun",
			"name": "Summer Fun",
			"description": "Beat the heat with sunny beach cosmetics and tropical effects!",
			"start_date": "2025-06-21",
			"end_date": "2025-08-31",
			"progress": 0,
			"cosmetics_unlocked": 0,
			"total_cosmetics": 6,
			"background_color": Color(1.0, 0.8, 0.2, 0.3)
		}
	]
	
	# Select first active event or first event
	var active_index = get_active_event_index()
	if active_index >= 0:
		_selected_event_index = active_index
	else:
		_selected_event_index = 0
	
	print("Loaded %d seasonal events" % _current_events.size())

func get_active_event_index() -> int:
	# Find the first active event
	var current_time = Time.get_unix_time_from_system()
	
	for i in range(_current_events.size()):
		var event = _current_events[i]
		var end_date = event.get("end_date", "")
		if is_event_active(event):
			return i
	
	return -1

func is_event_active(event: Dictionary) -> bool:
	# Check if an event is currently active
	var current_time = Time.get_unix_time_from_system()
	
	# Parse event dates (simplified - in real implementation would parse actual dates)
	var event_name = event.get("id", "")
	
	match event_name:
		"winter_wonderland":
			return true  # Active during winter months
		"spring_celebration":
			return false  # Not active yet
		"summer_fun":
			return false  # Not active yet
		_:
			return false

func display_events():
	# Display events in the carousel
	if _event_carousel_container:
		var carousel_hbox = _event_carousel_container.get_child(0)
		if carousel_hbox:
			carousel_hbox.queue_free()
		
		var new_hbox = HBoxContainer.new()
		new_hbox.name = "CarouselHBox"
		_event_carousel_container.add_child(new_hbox)
		
		# Create event cards
		for i in range(_current_events.size()):
			var event = _current_events[i]
			create_event_card(new_hbox, event, i)

func create_event_card(parent: Node, event: Dictionary, index: int):
	# Create an event card for the carousel
	var card_button = Button.new()
	card_button.custom_minimum_size = Vector2(200, 120)
	card_button.pressed.connect(func(): select_event(index))
	
	# Style the card based on event status
	var is_active = is_event_active(event)
	if is_active:
		card_button.add_theme_color_override("font_color", Color.White)
		card_button.add_theme_color_override("font_color_pressed", Color.Yellow)
	else:
		card_button.add_theme_color_override("font_color", Color.Gray)
	
	# Create card content
	var card_container = VBoxContainer.new()
	card_container.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	card_container.size_flags_vertical = Control.SIZE_EXPAND_FILL
	
	var event_name = Label.new()
	event_name.text = event.get("name", "Unknown Event")
	event_name.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	event_name.add_theme_font_size_override("font_size", 16)
	card_container.add_child(event_name)
	
	var event_status = Label.new()
	event_status.text = "ACTIVE" if is_active else "COMING SOON"
	event_status.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	event_status.add_theme_font_size_override("font_size", 12)
	card_container.add_child(event_status)
	
	var progress_info = Label.new()
	progress_info.text = "%d/%d" % [event.get("cosmetics_unlocked", 0), event.get("total_cosmetics", 0)]
	progress_info.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	progress_info.add_theme_font_size_override("font_size", 10)
	card_container.add_child(progress_info)
	
	card_button.add_child(card_container)
	parent.add_child(card_button)

func select_event(index: int):
	# Select a specific event
	if index < 0 or index >= _current_events.size():
		return
	
	_selected_event_index = index
	_event_data = _current_events[index]
	display_event_details()
	event_selected.emit(_event_data.get("id", ""))

func display_event_details():
	# Display details for the selected event
	if not _event_data:
		return
	
	# Update title
	if _current_event_title:
		_current_event_title.text = _event_data.get("name", "Unknown Event")
	
	# Update description
	if _event_description:
		_event_description.text = _event_data.get("description", "")
	
	# Update countdown
	if _event_countdown:
		var time_remaining = calculate_time_remaining(_event_data)
		_event_countdown.text = "⏰ Ends in %s" % format_time_remaining(time_remaining)
	
	# Update progress bar
	if _event_progress_bar:
		var progress = float(_event_data.get("progress", 0))
		_event_progress_bar.value = progress
		_event_progress_bar.tooltip_text = "Event Progress: %d%%" % int(progress)
	
	# Update unlock button
	if _unlock_cosmetics_button:
		var cosmetics_unlocked = _event_data.get("cosmetics_unlocked", 0)
		var total_cosmetics = _event_data.get("total_cosmetics", 0)
		_unlock_cosmetics_button.text = "Unlock Event Cosmetics (%d/%d)" % [cosmetics_unlocked, total_cosmetics]
		
		# Disable button if all cosmetics are unlocked
		_unlock_cosmetics_button.disabled = cosmetics_unlocked >= total_cosmetics
	
	# Display cosmetics
	display_cosmetics()
	
	# Update background
	if _event_background:
		var background_color = _event_data.get("background_color", Color(0.2, 0.2, 0.2, 0.3))
		_event_background.modulate = background_color

func calculate_time_remaining(event: Dictionary) -> Dictionary:
	# Calculate time remaining for the event (simplified)
	# In a real implementation, this would calculate actual time difference
	return {
		"days": 5,
		"hours": 12,
		"minutes": 30
	}

func format_time_remaining(time_data: Dictionary) -> String:
	# Format time remaining for display
	var days = time_data.get("days", 0)
	var hours = time_data.get("hours", 0)
	var minutes = time_data.get("minutes", 0)
	
	if days > 0:
		return "%dd %dh" % [days, hours]
	elif hours > 0:
		return "%dh %dm" % [hours, minutes]
	else:
		return "%dm" % minutes

func display_cosmetics():
	# Display available cosmetics for the current event
	if _cosmetics_grid_container:
		_cosmetics_grid_container.clear()
		
		var total_cosmetics = _event_data.get("total_cosmetics", 0)
		var unlocked_cosmetics = _event_data.get("cosmetics_unlocked", 0)
		
		# Create cosmetic slots
		for i in range(total_cosmetics):
			var cosmetic_slot = create_cosmetic_slot(i, i < unlocked_cosmetics)
			_cosmetics_grid_container.add_child(cosmetic_slot)

func create_cosmetic_slot(index: int, is_unlocked: bool) -> Control:
	# Create a cosmetic slot (icon + lock indicator)
	var slot = Panel.new()
	slot.custom_minimum_size = Vector2(80, 80)
	
	# Cosmetic icon (simplified)
	var icon = Label.new()
	icon.text = "🎁" if is_unlocked else "🔒"
	icon.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	icon.vertical_alignment = VERTICAL_ALIGNMENT_CENTER
	icon.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	icon.size_flags_vertical = Control.SIZE_EXPAND_FILL
	icon.add_theme_font_size_override("font_size", 24)
	slot.add_child(icon)
	
	# Set slot appearance
	if is_unlocked:
		slot.add_theme_color_override("font_color", Color.Gold)
		slot.add_theme_color_override("font_color_hover", Color.Yellow)
	else:
		slot.add_theme_color_override("font_color", Color.Gray)
		slot.add_theme_color_override("font_color_hover", Color.Gray)
	
	return slot

func unlock_cosmetics():
	# Unlock cosmetics for the current event
	if not _event_data:
		return
	
	var cosmetics_unlocked = _event_data.get("cosmetics_unlocked", 0)
	var total_cosmetics = _event_data.get("total_cosmetics", 0)
	
	if cosmetics_unlocked < total_cosmetics:
		_event_data["cosmetics_unlocked"] = cosmetics_unlocked + 1
		_event_data["progress"] = int((_event_data["cosmetics_unlocked"] * 100.0) / total_cosmetics)
		
		# Update the event in the array
		_current_events[_selected_event_index] = _event_data
		
		# Update UI
		display_event_details()
		
		# Emit signal
		cosmetics_unlocked.emit()
		
		print("Unlocked cosmetic for %s event (%d/%d)" % [
			_event_data.get("name", "Unknown"),
			_event_data["cosmetics_unlocked"],
			total_cosmetics
		])

func show_screen():
	# Show the seasonal events screen
	visible = true
	select_event(_selected_event_index)

func hide_screen():
	# Hide the seasonal events screen
	visible = false

func _on_unlock_cosmetics_pressed():
	print("Unlock cosmetics button pressed")
	unlock_cosmetics()

func _on_dismiss_pressed():
	print("Seasonal events dismissed")
	dismissed.emit()
	hide_screen()

func _process(delta):
	# Update countdown timer every second
	if _event_countdown and visible:
		var time_remaining = calculate_time_remaining(_event_data)
		_event_countdown.text = "⏰ Ends in %s" % format_time_remaining(time_remaining)

func _exit_tree():
	# Clean up connections
	if _unlock_cosmetics_button:
		_unlock_cosmetics_button.pressed.disconnect(_on_unlock_cosmetics_pressed)
	
	if _dismiss_button:
		_dismiss_button.pressed.disconnect(_on_dismiss_pressed)