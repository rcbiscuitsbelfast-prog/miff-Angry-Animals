using Godot;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

/// <summary>
/// Firebase Manager - Handles all Firebase operations for Angry Animals
/// Provides cross-platform Firebase integration with fallback support
/// Supports Analytics, Crashlytics, and Remote Config
/// </summary>
public class FirebaseManager : Node
{
    public static FirebaseManager Instance { get; private set; }

    // Firebase components
    private FirebaseAnalyticsWrapper _analytics;
    private FirebaseCrashlyticsWrapper _crashlytics;
    private FirebaseRemoteConfigWrapper _remoteConfig;
    
    // Configuration
    private FirebaseConfig _config;
    private bool _isInitialized = false;
    private bool _isAvailable = false;
    
    // Platform detection
    private bool _isMobile = false;
    private bool _isEditor = false;
    
    // Event queue for offline support
    private Queue<FirebaseEvent> _eventQueue = new Queue<FirebaseEvent>();
    private System.Timers.Timer _flushTimer;
    
    [Signal]
    public delegate void FirebaseInitializedEventHandler(bool success);
    
    [Signal]
    public delegate void EventLoggedEventHandler(string eventName, Dictionary<string, object> parameters);
    
    [Signal]
    public delegate void CrashReportedEventHandler(string crashType, string message);

    public override void _Ready()
    {
        if (Instance != null)
        {
            QueueFree();
            return;
        }
        
        Instance = this;
        InitializeFirebase();
    }

    /// <summary>
    /// Initialize Firebase with platform detection
    /// </summary>
    private void InitializeFirebase()
    {
        DetectPlatform();
        LoadConfiguration();
        
        // Try to initialize Firebase
        TryInitializeFirebase();
        
        // Start event flush timer
        StartEventFlushTimer();
        
        GD.Print($"Firebase Manager initialized - Platform: {GetPlatformName()}, Available: {_isAvailable}");
    }

    /// <summary>
    /// Detect current platform and Firebase availability
    /// </summary>
    private void DetectPlatform()
    {
        string platform = OS.GetName();
        
        _isEditor = EngineEditorInterface.IsEditorHint();
        _isMobile = platform == "Android" || platform == "iOS";
        
        // Check for Firebase plugins
        _isAvailable = CheckFirebasePlugins();
        
        if (_isEditor)
        {
            GD.Print("Running in editor - Firebase features will be simulated");
        }
        else if (!_isMobile)
        {
            GD.Print("Desktop platform detected - Firebase features will be simulated");
        }
    }

    /// <summary>
    /// Check if Firebase plugins are available
    /// </summary>
    private bool CheckFirebasePlugins()
    {
        try
        {
            // Try multiple possible Firebase singleton names
            var possibleNames = new[] { "Firebase", "FirebaseApp", "FirebaseAnalytics", "FirebaseCrashlytics" };
            
            foreach (var name in possibleNames)
            {
                if (Engine.HasSingleton(name))
                {
                    GD.Print($"Found Firebase plugin: {name}");
                    return true;
                }
            }
            
            return false;
        }
        catch (Exception e)
        {
            GD.PrintErr($"Error checking Firebase plugins: {e.Message}");
            return false;
        }
    }

    /// <summary>
    /// Load Firebase configuration
    /// </summary>
    private void LoadConfiguration()
    {
        _config = new FirebaseConfig
        {
            // Project configuration - replace with your actual Firebase project details
            ProjectId = "angry-animals-analytics", // Replace with your Firebase project ID
            ApiKey = "AIzaSyYourApiKeyHere", // Replace with your Web API Key
            AppId = "1:123456789:web:abcdef123456", // Replace with your app ID
            
            // Feature toggles
            AnalyticsEnabled = true,
            CrashlyticsEnabled = true,
            RemoteConfigEnabled = false, // Enable when needed
            
            // Performance settings
            BatchSize = 10,
            FlushInterval = 30, // seconds
            MaxQueueSize = 100,
            
            // Privacy settings
            UserConsent = CheckUserConsent(),
            DataCollectionEnabled = true
        };
    }

    /// <summary>
    /// Try to initialize Firebase services
    /// </summary>
    private void TryInitializeFirebase()
    {
        try
        {
            if (_isAvailable)
            {
                InitializeFirebaseServices();
            }
            else
            {
                InitializeMockFirebase();
            }
            
            _isInitialized = true;
            EmitSignal("FirebaseInitialized", true);
        }
        catch (Exception e)
        {
            GD.PrintErr($"Failed to initialize Firebase: {e.Message}");
            InitializeMockFirebase();
            EmitSignal("FirebaseInitialized", false);
        }
    }

    /// <summary>
    /// Initialize actual Firebase services
    /// </summary>
    private void InitializeFirebaseServices()
    {
        if (_config.AnalyticsEnabled)
        {
            _analytics = new FirebaseAnalyticsWrapper(_config);
        }
        
        if (_config.CrashlyticsEnabled)
        {
            _crashlytics = new FirebaseCrashlyticsWrapper(_config);
        }
        
        if (_config.RemoteConfigEnabled)
        {
            _remoteConfig = new FirebaseRemoteConfigWrapper(_config);
        }
    }

    /// <summary>
    /// Initialize mock Firebase for testing/editor
    /// </summary>
    private void InitializeMockFirebase()
    {
        _analytics = new FirebaseAnalyticsWrapper(_config, isMock: true);
        _crashlytics = new FirebaseCrashlyticsWrapper(_config, isMock: true);
        _remoteConfig = new FirebaseRemoteConfigWrapper(_config, isMock: true);
        
        GD.Print("Mock Firebase initialized - events will be logged locally");
    }

    /// <summary>
    /// Start event flush timer
    /// </summary>
    private void StartEventFlushTimer()
    {
        _flushTimer = new System.Timers.Timer(_config.FlushInterval * 1000);
        _flushTimer.Elapsed += OnFlushTimer;
        _flushTimer.Start();
    }

    /// <summary>
    /// Flush event queue
    /// </summary>
    private void OnFlushTimer(object sender, System.Timers.ElapsedEventArgs e)
    {
        FlushEventQueue();
    }

    /// <summary>
    /// Flush queued events
    /// </summary>
    private void FlushEventQueue()
    {
        while (_eventQueue.Count > 0 && _eventQueue.Count >= _config.BatchSize)
        {
            var batch = new List<FirebaseEvent>();
            
            for (int i = 0; i < _config.BatchSize && _eventQueue.Count > 0; i++)
            {
                batch.Add(_eventQueue.Dequeue());
            }
            
            ProcessEventBatch(batch);
        }
    }

    /// <summary>
    /// Process a batch of events
    /// </summary>
    private void ProcessEventBatch(List<FirebaseEvent> events)
    {
        try
        {
            if (_analytics != null)
            {
                foreach (var evt in events)
                {
                    _analytics.LogEvent(evt.EventName, evt.Parameters);
                }
            }
        }
        catch (Exception e)
        {
            GD.PrintErr($"Error processing event batch: {e.Message}");
        }
    }

    /// <summary>
    /// Log analytics event
    /// </summary>
    public void LogEvent(string eventName, Dictionary<string, object> parameters = null)
    {
        var firebaseEvent = new FirebaseEvent
        {
            EventName = eventName,
            Parameters = parameters ?? new Dictionary<string, object>(),
            Timestamp = DateTime.Now
        };
        
        if (_isInitialized)
        {
            if (_analytics != null)
            {
                _analytics.LogEvent(eventName, parameters);
            }
            
            EmitSignal("EventLogged", eventName, parameters);
        }
        else
        {
            // Queue event for later
            _eventQueue.Enqueue(firebaseEvent);
            
            if (_eventQueue.Count > _config.MaxQueueSize)
            {
                // Remove oldest event if queue is full
                _eventQueue.Dequeue();
            }
        }
    }

    /// <summary>
    /// Report crash to Firebase Crashlytics
    /// </summary>
    public void ReportCrash(string crashType, string message, Dictionary<string, object> additionalData = null)
    {
        try
        {
            if (_crashlytics != null)
            {
                _crashlytics.RecordException(crashType, message, additionalData);
            }
            
            // Also log as analytics event
            LogEvent("crash_reported", new Dictionary<string, object>
            {
                { "crash_type", crashType },
                { "message", message },
                { "platform", GetPlatformName() },
                { "timestamp", DateTime.Now.ToString("O") }
            });
            
            EmitSignal("CrashReported", crashType, message);
        }
        catch (Exception e)
        {
            GD.PrintErr($"Error reporting crash: {e.Message}");
        }
    }

    /// <summary>
    /// Set user property
    /// </summary>
    public void SetUserProperty(string propertyName, string value)
    {
        if (_analytics != null)
        {
            _analytics.SetUserProperty(propertyName, value);
        }
        
        // Also store locally for fallback
        SetUserPropertyLocally(propertyName, value);
    }

    /// <summary>
    /// Set user ID
    /// </summary>
    public void SetUserId(string userId)
    {
        if (_analytics != null)
        {
            _analytics.SetUserId(userId);
        }
        
        SetUserIdLocally(userId);
    }

    /// <summary>
    /// Get remote config value
    /// </summary>
    public object GetRemoteConfigValue(string key, object defaultValue = null)
    {
        if (_remoteConfig != null)
        {
            return _remoteConfig.GetValue(key, defaultValue);
        }
        
        return defaultValue;
    }

    /// <summary>
    /// Fetch remote config
    /// </summary>
    public void FetchRemoteConfig(Action<bool> callback = null)
    {
        if (_remoteConfig != null)
        {
            _remoteConfig.Fetch(callback);
        }
        else if (callback != null)
        {
            callback(false);
        }
    }

    /// <summary>
    /// Check user consent for data collection
    /// </summary>
    private bool CheckUserConsent()
    {
        // This would integrate with PrivacyPolicyManager
        // For now, check if analytics consent was given
        return true;
    }

    /// <summary>
    /// Get platform name
    /// </summary>
    public string GetPlatformName()
    {
        if (_isEditor) return "Editor";
        if (OS.GetName() == "Android") return "Android";
        if (OS.GetName() == "iOS") return "iOS";
        return OS.GetName();
    }

    /// <summary>
    /// Check if Firebase is available
    /// </summary>
    public bool IsFirebaseAvailable()
    {
        return _isAvailable && _isInitialized;
    }

    /// <summary>
    /// Get queued event count
    /// </summary>
    public int GetQueuedEventCount()
    {
        return _eventQueue.Count;
    }

    /// <summary>
    /// Get Firebase configuration
    /// </summary>
    public FirebaseConfig GetFirebaseConfig()
    {
        return _config;
    }

    /// <summary>
    /// Force flush all queued events
    /// </summary>
    public void FlushEvents()
    {
        FlushEventQueue();
    }

    /// <summary>
    /// Clear all queued events
    /// </summary>
    public void ClearEventQueue()
    {
        _eventQueue.Clear();
    }

    /// <summary>
    /// Set user property locally
    /// </summary>
    private void SetUserPropertyLocally(string propertyName, string value)
    {
        // Store in player preferences for local fallback
        var prefs = new ConfigFile();
        prefs.SetValue("firebase_user_properties", propertyName, value);
        prefs.Save("user://firebase_properties.cfg");
    }

    /// <summary>
    /// Set user ID locally
    /// </summary>
    private void SetUserIdLocally(string userId)
    {
        // Store in player preferences for local fallback
        var prefs = new ConfigFile();
        prefs.SetValue("firebase_user_properties", "user_id", userId);
        prefs.Save("user://firebase_properties.cfg");
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _flushTimer?.Dispose();
            
            // Flush remaining events
            if (_eventQueue.Count > 0)
            {
                ProcessEventBatch(new List<FirebaseEvent>(_eventQueue));
                _eventQueue.Clear();
            }
        }
        base.Dispose(disposing);
    }
}

/// <summary>
/// Firebase configuration
/// </summary>
public class FirebaseConfig
{
    public string ProjectId { get; set; }
    public string ApiKey { get; set; }
    public string AppId { get; set; }
    public bool AnalyticsEnabled { get; set; }
    public bool CrashlyticsEnabled { get; set; }
    public bool RemoteConfigEnabled { get; set; }
    public int BatchSize { get; set; }
    public int FlushInterval { get; set; }
    public int MaxQueueSize { get; set; }
    public bool UserConsent { get; set; }
    public bool DataCollectionEnabled { get; set; }
}

/// <summary>
/// Firebase event structure
/// </summary>
public class FirebaseEvent
{
    public string EventName { get; set; }
    public Dictionary<string, object> Parameters { get; set; } = new Dictionary<string, object>();
    public DateTime Timestamp { get; set; }
}