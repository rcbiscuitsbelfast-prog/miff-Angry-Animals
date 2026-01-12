using System;
using System.Collections.Generic;
using Godot;
using Godot.Collections;
using Newtonsoft.Json;

/// <summary>
/// SeasonalEventDatabase manages all seasonal events
/// Provides Inspector interface for creating and configuring events
/// </summary>
public partial class SeasonalEventDatabase : Node
{
    public static SeasonalEventDatabase Instance { get; private set; }

    [Signal] public delegate void EventDatabaseUpdatedEventHandler();

    [Export] private string _eventsResourcePath = "res://Data/SeasonalEvents/";
    [Export] private string _eventsDataFile = "user://seasonal_events.json";

    private Dictionary<string, SeasonalEvent> _seasonalEvents = new();

    public override void _Ready()
    {
        if (Instance != null)
        {
            QueueFree();
            return;
        }
        
        Instance = this;
        LoadEventsFromDatabase();
        
        GD.Print("SeasonalEventDatabase initialized");
    }

    /// <summary>
    /// Load all seasonal events from database
    /// </summary>
    private void LoadEventsFromDatabase()
    {
        // Load built-in seasonal events if database is empty
        if (_seasonalEvents.Count == 0)
        {
            CreateDefaultSeasonalEvents();
        }

        // Load any saved events from file
        LoadEventsFromFile();
        
        GD.Print($"Loaded {_seasonalEvents.Count} seasonal events from database");
    }

    /// <summary>
    /// Create default seasonal event templates
    /// </summary>
    private void CreateDefaultSeasonalEvents()
    {
        CreateWinterWonderlandEvent();
        CreateSpringBloomEvent();
        CreateSummerSplashEvent();
        CreateFallFestivalEvent();
    }

    /// <summary>
    /// Winter Wonderland event (Dec-Jan)
    /// </summary>
    private void CreateWinterWonderlandEvent()
    {
        var winterEvent = new SeasonalEvent();
        winterEvent.EventId = "winter_wonderland_2024";
        winterEvent.EventName = "Winter Wonderland";
        winterEvent.EventDescription = "Bundle up for a frosty adventure! Unlock exclusive ice-themed cosmetics and special winter effects.";
        winterEvent.EventTheme = "Winter";
        winterEvent.ThemeColor = new Color(0.7f, 0.9f, 1.0f); // Ice blue
        winterEvent.StartDate = new DateTime(2024, 12, 1);
        winterEvent.EndDate = new DateTime(2025, 1, 31);
        winterEvent.IsRepeating = true;

        // Winter cosmetics
        winterEvent.EventCosmetics = new Array<string>
        {
            "winter_hat_santa",
            "winter_glasses_snow",
            "winter_moustache_frost",
            "winter_wig_ice_queen",
            "winter_projectile_snowball"
        };

        // Winter challenges
        winterEvent.EventChallenges = new Array<string>
        {
            "play_5_levels_in_snow_conditions",
            "complete_10_perfect_scores",
            "unlock_all_winter_cosmetics"
        };

        // Event rewards
        winterEvent.EventRewards = new Dictionary<string, Variant>
        {
            ["tier_1"] = new Dictionary<string, Variant>
            {
                ["coins"] = 500,
                ["cosmetic_id"] = "winter_hat_santa",
                ["description"] = "500 coins + Santa Hat"
            },
            ["tier_2"] = new Dictionary<string, Variant>
            {
                ["coins"] = 750,
                ["cosmetic_id"] = "winter_glasses_snow",
                ["description"] = "750 coins + Snow Glasses"
            },
            ["tier_3"] = new Dictionary<string, Variant>
            {
                ["coins"] = 1000,
                ["cosmetic_id"] = "winter_wig_ice_queen",
                ["description"] = "1000 coins + Ice Queen Wig"
            }
        };

        SaveEvent(winterEvent);
    }

    /// <summary>
    /// Spring Bloom event (Mar-Apr)
    /// </ </summary>
    private void CreateSpringBloomEvent()
    {
        var springEvent = new SeasonalEvent();
        springEvent.EventId = "spring_bloom_2024";
        springEvent.EventName = "Spring Bloom";
        springEvent.EventDescription = "Celebrate new beginnings! Discover nature-themed cosmetics and flower effects as you blossom through challenges.";
        springEvent.EventTheme = "Spring";
        springEvent.ThemeColor = new Color(0.6f, 0.8f, 0.4f); // Spring green
        springEvent.StartDate = new DateTime(2024, 3, 1);
        springEvent.EndDate = new DateTime(2024, 4, 30);
        springEvent.IsRepeating = true;

        // Spring cosmetics
        springEvent.EventCosmetics = new Array<string>
        {
            "spring_hat_flower_crown",
            "spring_glasses_butterfly",
            "spring_moustache_dandelion",
            "spring_wig_garden",
            "spring_projectile_petals"
        };

        // Spring challenges
        springEvent.EventChallenges = new Array<string>
        {
            "complete_3_daily_challenges",
            "play_with_3_different_animals",
            "achieve_5_combo_masteries"
        };

        // Event rewards
        springEvent.EventRewards = new Dictionary<string, Variant>
        {
            ["tier_1"] = new Dictionary<string, Variant>
            {
                ["coins"] = 400,
                ["cosmetic_id"] = "spring_hat_flower_crown",
                ["description"] = "400 coins + Flower Crown"
            },
            ["tier_2"] = new Dictionary<string, Variant>
            {
                ["coins"] = 600,
                ["cosmetic_id"] = "spring_glasses_butterfly",
                ["description"] = "600 coins + Butterfly Glasses"
            },
            ["tier_3"] = new Dictionary<string, Variant>
            {
                ["coins"] = 800,
                ["cosmetic_id"] = "spring_wig_garden",
                ["description"] = "800 coins + Garden Wig"
            }
        };

        SaveEvent(springEvent);
    }

    /// <summary>
    /// Summer Splash event (Jun-Jul)
    /// </summary>
    private void CreateSummerSplashEvent()
    {
        var summerEvent = new SeasonalEvent();
        summerEvent.EventId = "summer_splash_2024";
        summerEvent.EventName = "Summer Splash";
        summerEvent.EventDescription = "Dive into summer fun! Cool off with beach-themed cosmetics and water effects as you make waves with your skills.";
        summerEvent.EventTheme = "Summer";
        summerEvent.ThemeColor = new Color(0.2f, 0.7f, 1.0f); // Ocean blue
        summerEvent.StartDate = new DateTime(2024, 6, 1);
        summerEvent.EndDate = new DateTime(2024, 7, 31);
        summerEvent.IsRepeating = true;

        // Summer cosmetics
        summerEvent.EventCosmetics = new Array<string>
        {
            "summer_hat_beach",
            "summer_glasses_sunglasses",
            "summer_moustache_surf",
            "summer_wig_ocean",
            "summer_projectile_water"
        };

        // Summer challenges
        summerEvent.EventChallenges = new Array<string>
        {
            "score_10000_points_in_single_level",
            "complete_2_levels_without_missing",
            "unlock_all_summer_cosmetics"
        };

        // Event rewards
        summerEvent.EventRewards = new Dictionary<string, Variant>
        {
            ["tier_1"] = new Dictionary<string, Variant>
            {
                ["coins"] = 450,
                ["cosmetic_id"] = "summer_hat_beach",
                ["description"] = "450 coins + Beach Hat"
            },
            ["tier_2"] = new Dictionary<string, Variant>
            {
                ["coins"] = 650,
                ["cosmetic_id"] = "summer_glasses_sunglasses",
                ["description"] = "650 coins + Sunglasses"
            },
            ["tier_3"] = new Dictionary<string, Variant>
            {
                ["coins"] = 850,
                ["cosmetic_id"] = "summer_wig_ocean",
                ["description"] = "850 coins + Ocean Wig"
            }
        };

        SaveEvent(summerEvent);
    }

    /// <summary>
    /// Fall Festival event (Sep-Oct)
    /// </summary>
    private void CreateFallFestivalEvent()
    {
        var fallEvent = new SeasonalEvent();
        fallEvent.EventId = "fall_festival_2024";
        fallEvent.EventName = "Fall Festival";
        fallEvent.EventDescription = "Harvest season is here! Gather autumn-themed cosmetics and pumpkin effects as you fall into challenging fun.";
        fallEvent.EventTheme = "Fall";
        fallEvent.ThemeColor = new Color(0.8f, 0.4f, 0.2f); // Autumn orange
        fallEvent.StartDate = new DateTime(2024, 9, 1);
        fallEvent.EndDate = new DateTime(2024, 10, 31);
        fallEvent.IsRepeating = true;

        // Fall cosmetics
        fallEvent.EventCosmetics = new Array<string>
        {
            "fall_hat_pumpkin",
            "fall_glasses_acorn",
            "fall_moustache_walnut",
            "fall_wig_harvest",
            "fall_projectile_leaf"
        };

        // Fall challenges
        fallEvent.EventChallenges = new Array<string>
        {
            "collect_100_coins_in_single_level",
            "complete_10_levels_with_perfect_accuracy",
            "achieve_3_day_streak"
        };

        // Event rewards
        fallEvent.EventRewards = new Dictionary<string, Variant>
        {
            ["tier_1"] = new Dictionary<string, Variant>
            {
                ["coins"] = 500,
                ["cosmetic_id"] = "fall_hat_pumpkin",
                ["description"] = "500 coins + Pumpkin Hat"
            },
            ["tier_2"] = new Dictionary<string, Variant>
            {
                ["coins"] = 700,
                ["cosmetic_id"] = "fall_glasses_acorn",
                ["description"] = "700 coins + Acorn Glasses"
            },
            ["tier_3"] = new Dictionary<string, Variant>
            {
                ["coins"] = 900,
                ["cosmetic_id"] = "fall_wig_harvest",
                ["description"] = "900 coins + Harvest Wig"
            }
        };

        SaveEvent(fallEvent);
    }

    /// <summary>
    /// Save event to database
    /// </summary>
    public void SaveEvent(SeasonalEvent seasonalEvent)
    {
        _seasonalEvents[seasonalEvent.EventId] = seasonalEvent;
        SaveEventsToFile();
        EmitSignal("EventDatabaseUpdated");
        
        GD.Print($"Saved event: {seasonalEvent.EventName}");
    }

    /// <summary>
    /// Remove event from database
    /// </summary>
    public void RemoveEvent(string eventId)
    {
        if (_seasonalEvents.Remove(eventId))
        {
            SaveEventsToFile();
            EmitSignal("EventDatabaseUpdated");
            GD.Print($"Removed event: {eventId}");
        }
    }

    /// <summary>
    /// Get all events
    /// </summary>
    public Dictionary<string, SeasonalEvent> GetAllEvents()
    {
        return new Dictionary<string, SeasonalEvent>(_seasonalEvents);
    }

    /// <summary>
    /// Get event by ID
    /// </summary>
    public SeasonalEvent? GetEvent(string eventId)
    {
        return _seasonalEvents.GetValueOrDefault(eventId);
    }

    /// <summary>
    /// Get active events
    /// </summary>
    public List<SeasonalEvent> GetActiveEvents()
    {
        var activeEvents = new List<SeasonalEvent>();
        var now = DateTime.UtcNow;
        
        foreach (var kvp in _seasonalEvents)
        {
            if (kvp.Value.IsEventActive())
            {
                activeEvents.Add(kvp.Value);
            }
        }
        
        return activeEvents;
    }

    /// <summary>
    /// Get upcoming events
    /// </summary>
    public List<SeasonalEvent> GetUpcomingEvents()
    {
        var upcomingEvents = new List<SeasonalEvent>();
        var now = DateTime.UtcNow;
        
        foreach (var kvp in _seasonalEvents)
        {
            if (kvp.Value.StartDate > now)
            {
                upcomingEvents.Add(kvp.Value);
            }
        }
        
        return upcomingEvents;
    }

    /// <summary>
    /// Clone existing event to create new variant
    /// </summary>
    public SeasonalEvent CloneEvent(string sourceEventId, string newEventName, DateTime newStartDate, DateTime newEndDate)
    {
        if (!_seasonalEvents.TryGetValue(sourceEventId, out var sourceEvent))
        {
            GD.PrintErr($"Source event not found: {sourceEventId}");
            return null;
        }

        var clonedEvent = new SeasonalEvent();
        clonedEvent.EventId = GenerateEventId(newEventName);
        clonedEvent.EventName = newEventName;
        clonedEvent.EventDescription = sourceEvent.EventDescription;
        clonedEvent.EventTheme = sourceEvent.EventTheme;
        clonedEvent.ThemeColor = sourceEvent.ThemeColor;
        clonedEvent.StartDate = newStartDate;
        clonedEvent.EndDate = newEndDate;
        clonedEvent.IsRepeating = sourceEvent.IsRepeating;
        clonedEvent.EventCosmetics = new Array<string>(sourceEvent.EventCosmetics);
        clonedEvent.EventChallenges = new Array<string>(sourceEvent.EventChallenges);
        clonedEvent.EventRewards = new Dictionary<string, Variant>(sourceEvent.EventRewards);

        SaveEvent(clonedEvent);
        return clonedEvent;
    }

    /// <summary>
    /// Generate unique event ID
    /// </summary>
    private string GenerateEventId(string eventName)
    {
        var baseId = eventName.Replace(" ", "_").ToLower();
        var timestamp = DateTime.UtcNow.Ticks;
        return $"{baseId}_{timestamp}";
    }

    /// <summary>
    /// Save events to file
    /// </summary>
    private void SaveEventsToFile()
    {
        try
        {
            var eventsData = new Dictionary<string, Dictionary<string, Variant>>();
            
            foreach (var kvp in _seasonalEvents)
            {
                eventsData[kvp.Key] = kvp.Value.SerializeEventData();
            }

            var jsonData = JsonConvert.SerializeObject(eventsData, Formatting.Indented);
            
            using var file = FileAccess.Open(_eventsDataFile, FileAccess.ModeFlags.Write);
            file?.StoreString(jsonData);
            
            GD.Print($"Saved {_seasonalEvents.Count} events to file");
        }
        catch (Exception ex)
        {
            GD.PrintErr($"Failed to save events to file: {ex.Message}");
        }
    }

    /// <summary>
    /// Load events from file
    /// </summary>
    private void LoadEventsFromFile()
    {
        try
        {
            if (!FileAccess.FileExists(_eventsDataFile))
                return;

            using var file = FileAccess.Open(_eventsDataFile, FileAccess.ModeFlags.Read);
            var jsonData = file?.GetAsText();
            
            if (string.IsNullOrEmpty(jsonData))
                return;

            var eventsData = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, Variant>>>(jsonData);
            if (eventsData == null) return;

            foreach (var kvp in eventsData)
            {
                var eventId = kvp.Key;
                var eventData = kvp.Value;
                
                var seasonalEvent = new SeasonalEvent();
                seasonalEvent.DeserializeEventData(eventData);
                _seasonalEvents[eventId] = seasonalEvent;
            }
            
            GD.Print($"Loaded {_seasonalEvents.Count} events from file");
        }
        catch (Exception ex)
        {
            GD.PrintErr($"Failed to load events from file: {ex.Message}");
        }
    }

    /// <summary>
    /// Get events by theme
    /// </summary>
    public List<SeasonalEvent> GetEventsByTheme(string theme)
    {
        var themedEvents = new List<SeasonalEvent>();
        
        foreach (var kvp in _seasonalEvents)
        {
            if (kvp.Value.EventTheme.Equals(theme, StringComparison.OrdinalIgnoreCase))
            {
                themedEvents.Add(kvp.Value);
            }
        }
        
        return themedEvents;
    }

    /// <summary>
    /// Get events by date range
    /// </summary>
    public List<SeasonalEvent> GetEventsByDateRange(DateTime startDate, DateTime endDate)
    {
        var eventsInRange = new List<SeasonalEvent>();
        
        foreach (var kvp in _seasonalEvents)
        {
            var seasonalEvent = kvp.Value;
            if (seasonalEvent.StartDate <= endDate && seasonalEvent.EndDate >= startDate)
            {
                eventsInRange.Add(seasonalEvent);
            }
        }
        
        return eventsInRange;
    }

    /// <summary>
    /// Check if event name is available
    /// </summary>
    public bool IsEventNameAvailable(string eventName)
    {
        foreach (var kvp in _seasonalEvents)
        {
            if (kvp.Value.EventName.Equals(eventName, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Get event statistics
    /// </summary>
    public Dictionary<string, int> GetEventStatistics()
    {
        var stats = new Dictionary<string, int>
        {
            ["total_events"] = _seasonalEvents.Count,
            ["active_events"] = GetActiveEvents().Count,
            ["upcoming_events"] = GetUpcomingEvents().Count,
            ["completed_events"] = _seasonalEvents.Count - GetActiveEvents().Count - GetUpcomingEvents().Count
        };

        // Count by theme
        var themes = new HashSet<string>();
        foreach (var kvp in _seasonalEvents)
        {
            themes.Add(kvp.Value.EventTheme);
        }
        stats["unique_themes"] = themes.Count;

        return stats;
    }
}