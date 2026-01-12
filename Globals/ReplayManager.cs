using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

/// <summary>
/// Manages gameplay replay recording, playback, and sharing
/// </summary>
public partial class ReplayManager : Node
{
    public static ReplayManager Instance { get; private set; }
    
    private const string ReplaysDataPath = "user://replays/";
    private const int MaxReplaysPerDevice = 20;
    private const float SnapshotInterval = 0.1f; // 10 snapshots per second
    
    // Recording state
    private bool _isRecording = false;
    private ReplayData _currentReplay;
    private float _recordingStartTime;
    private float _lastSnapshotTime;
    
    // Playback state
    private bool _isPlaying = false;
    private ReplayData _playbackReplay;
    private int _currentInputIndex;
    private int _currentSnapshotIndex;
    private float _playbackStartTime;
    private float _playbackSpeed = 1.0f;
    
    // Replay library
    private Dictionary<string, ReplayData> _replays = new();
    
    // Signals
    [Signal]
    public delegate void RecordingStartedEventHandler();
    
    [Signal]
    public delegate void RecordingStoppedEventHandler(ReplayData replay);
    
    [Signal]
    public delegate void PlaybackStartedEventHandler(ReplayData replay);
    
    [Signal]
    public delegate void PlaybackStoppedEventHandler();
    
    [Signal]
    public delegate void ReplaySharedEventHandler(ShareableReplay shareable);
    
    public override void _Ready()
    {
        if (Instance != null)
        {
            QueueFree();
            return;
        }
        
        Instance = this;
        ProcessMode = ProcessModeEnum.Always;
        
        CreateReplaysDirectory();
        LoadReplays();
        
        GD.Print("Replay Manager initialized");
    }
    
    public override void _Process(double delta)
    {
        if (_isRecording)
        {
            UpdateRecording((float)delta);
        }
        
        if (_isPlaying)
        {
            UpdatePlayback((float)delta);
        }
    }
    
    public override void _ExitTree()
    {
        if (_isRecording)
        {
            StopRecording();
        }
        
        if (_isPlaying)
        {
            StopPlayback();
        }
    }
    
    /// <summary>
    /// Start recording a replay
    /// </summary>
    public bool StartRecording(string levelId, string levelName)
    {
        if (_isRecording)
        {
            GD.PrintErr("Cannot start recording: already recording");
            return false;
        }
        
        _currentReplay = new ReplayData
        {
            ReplayId = Guid.NewGuid().ToString(),
            PlayerId = GetCurrentPlayerId(),
            PlayerName = GetCurrentPlayerName(),
            LevelId = levelId,
            LevelName = levelName,
            RecordedDate = DateTime.UtcNow,
            PlayerCosmetics = GetCurrentPlayerCosmetics(),
            StartingConditions = new ReplayStartingConditions
            {
                SlingshotType = PlayerProfile.Instance?.SelectedSlingshotType ?? 0,
                ProjectileType = PlayerProfile.Instance?.SelectedProjectileSkinIndex ?? 0
            }
        };
        
        _isRecording = true;
        _recordingStartTime = Time.GetTicksMsec() / 1000.0f;
        _lastSnapshotTime = _recordingStartTime;
        
        EmitSignal(SignalName.RecordingStarted);
        
        GD.Print($"Started recording replay for {levelName}");
        return true;
    }
    
    /// <summary>
    /// Stop recording and save replay
    /// </summary>
    public ReplayData? StopRecording(int finalScore = 0, int stars = 0, float completionTime = 0)
    {
        if (!_isRecording)
        {
            GD.PrintErr("Cannot stop recording: not recording");
            return null;
        }
        
        _isRecording = false;
        
        _currentReplay.Score = finalScore;
        _currentReplay.Stars = stars;
        _currentReplay.CompletionTime = completionTime;
        _currentReplay.IsPerfect = stars >= 5;
        
        // Calculate file size
        var json = JsonConvert.SerializeObject(_currentReplay);
        _currentReplay.FileSizeBytes = Encoding.UTF8.GetByteCount(json);
        
        // Check size limit
        if (!_currentReplay.IsWithinSizeLimit())
        {
            GD.PrintErr($"Replay exceeds size limit: {_currentReplay.GetFileSizeKB():F2} KB");
        }
        
        // Save replay
        SaveReplay(_currentReplay);
        
        EmitSignal(SignalName.RecordingStopped, _currentReplay);
        
        // Track analytics
        TrackReplayRecorded(_currentReplay);
        
        GD.Print($"Stopped recording replay: {_currentReplay.ReplayId} ({_currentReplay.GetFileSizeKB():F2} KB)");
        
        var replay = _currentReplay;
        _currentReplay = null;
        
        return replay;
    }
    
    /// <summary>
    /// Record input event
    /// </summary>
    public void RecordInputEvent(ReplayEventType eventType, Vector2 position, float dragAngle = 0, float dragStrength = 0)
    {
        if (!_isRecording)
            return;
        
        var timestamp = (Time.GetTicksMsec() / 1000.0f) - _recordingStartTime;
        
        _currentReplay.InputEvents.Add(new ReplayInputEvent
        {
            Timestamp = timestamp,
            EventType = eventType,
            PositionX = position.X,
            PositionY = position.Y,
            DragAngle = dragAngle,
            DragStrength = dragStrength,
            SlingshotType = PlayerProfile.Instance?.SelectedSlingshotType ?? 0
        });
    }
    
    /// <summary>
    /// Record physics snapshot
    /// </summary>
    public void RecordPhysicsSnapshot(Vector2 projectilePos, Vector2 projectileVel, float projectileRot, List<string> destroyedObjects)
    {
        if (!_isRecording)
            return;
        
        var currentTime = Time.GetTicksMsec() / 1000.0f;
        if (currentTime - _lastSnapshotTime < SnapshotInterval)
            return;
        
        _lastSnapshotTime = currentTime;
        var timestamp = currentTime - _recordingStartTime;
        
        _currentReplay.PhysicsSnapshots.Add(new PhysicsSnapshot
        {
            Timestamp = timestamp,
            ProjectilePositionX = projectilePos.X,
            ProjectilePositionY = projectilePos.Y,
            ProjectileVelocityX = projectileVel.X,
            ProjectileVelocityY = projectileVel.Y,
            ProjectileRotation = projectileRot,
            DestroyedObjects = new List<string>(destroyedObjects)
        });
    }
    
    /// <summary>
    /// Update recording
    /// </summary>
    private void UpdateRecording(float delta)
    {
        // Recording is event-driven, nothing to update per frame
    }
    
    /// <summary>
    /// Start replay playback
    /// </summary>
    public bool StartPlayback(ReplayData replay, float speed = 1.0f)
    {
        if (_isPlaying)
        {
            GD.PrintErr("Cannot start playback: already playing");
            return false;
        }
        
        if (replay == null)
        {
            GD.PrintErr("Cannot start playback: replay is null");
            return false;
        }
        
        _playbackReplay = replay;
        _isPlaying = true;
        _currentInputIndex = 0;
        _currentSnapshotIndex = 0;
        _playbackStartTime = Time.GetTicksMsec() / 1000.0f;
        _playbackSpeed = speed;
        
        // Increment view count
        replay.ViewCount++;
        SaveReplay(replay);
        
        EmitSignal(SignalName.PlaybackStarted, replay);
        
        GD.Print($"Started replay playback: {replay.ReplayId}");
        return true;
    }
    
    /// <summary>
    /// Stop replay playback
    /// </summary>
    public void StopPlayback()
    {
        if (!_isPlaying)
            return;
        
        _isPlaying = false;
        _playbackReplay = null;
        _currentInputIndex = 0;
        _currentSnapshotIndex = 0;
        
        EmitSignal(SignalName.PlaybackStopped);
        
        GD.Print("Stopped replay playback");
    }
    
    /// <summary>
    /// Set playback speed
    /// </summary>
    public void SetPlaybackSpeed(float speed)
    {
        _playbackSpeed = Mathf.Clamp(speed, 0.25f, 2.0f);
    }
    
    /// <summary>
    /// Update playback
    /// </summary>
    private void UpdatePlayback(float delta)
    {
        if (_playbackReplay == null)
            return;
        
        var currentTime = (Time.GetTicksMsec() / 1000.0f) - _playbackStartTime;
        var playbackTime = currentTime * _playbackSpeed;
        
        // Process input events
        while (_currentInputIndex < _playbackReplay.InputEvents.Count)
        {
            var inputEvent = _playbackReplay.InputEvents[_currentInputIndex];
            if (inputEvent.Timestamp <= playbackTime)
            {
                // Replay this input event
                ProcessPlaybackInputEvent(inputEvent);
                _currentInputIndex++;
            }
            else
            {
                break;
            }
        }
        
        // Check if playback is complete
        if (_currentInputIndex >= _playbackReplay.InputEvents.Count)
        {
            StopPlayback();
        }
    }
    
    /// <summary>
    /// Process playback input event
    /// </summary>
    private void ProcessPlaybackInputEvent(ReplayInputEvent inputEvent)
    {
        // This would trigger the actual game logic based on the input event
        // For now, just log it
        GD.Print($"Playback event: {inputEvent.EventType} at {inputEvent.Timestamp:F2}s");
    }
    
    /// <summary>
    /// Create shareable replay
    /// </summary>
    public ShareableReplay CreateShareableReplay(ReplayData replay)
    {
        if (replay == null)
        {
            GD.PrintErr("Cannot create shareable replay: replay is null");
            return null;
        }
        
        var shareable = new ShareableReplay
        {
            ReplayData = replay
        };
        
        // Encode replay to base64
        var json = JsonConvert.SerializeObject(replay);
        var bytes = Encoding.UTF8.GetBytes(json);
        shareable.EncodedString = Convert.ToBase64String(bytes);
        
        // Generate share URL
        shareable.GenerateShareUrl();
        
        GD.Print($"Created shareable replay: {shareable.EncodedString.Length} characters");
        return shareable;
    }
    
    /// <summary>
    /// Import replay from encoded string
    /// </summary>
    public ReplayData? ImportReplay(string encodedString)
    {
        try
        {
            var bytes = Convert.FromBase64String(encodedString);
            var json = Encoding.UTF8.GetString(bytes);
            var replay = JsonConvert.DeserializeObject<ReplayData>(json);
            
            if (replay != null)
            {
                SaveReplay(replay);
                GD.Print($"Imported replay: {replay.ReplayId}");
            }
            
            return replay;
        }
        catch (Exception ex)
        {
            GD.PrintErr($"Failed to import replay: {ex.Message}");
            return null;
        }
    }
    
    /// <summary>
    /// Share replay to social media
    /// </summary>
    public void ShareReplay(ReplayData replay, string platform)
    {
        if (replay == null)
        {
            GD.PrintErr("Cannot share replay: replay is null");
            return;
        }
        
        var shareable = CreateShareableReplay(replay);
        if (shareable == null)
            return;
        
        shareable.ShareMessage = shareable.GenerateShareMessage(platform);
        
        // Increment share count
        replay.ShareCount++;
        SaveReplay(replay);
        
        EmitSignal(SignalName.ReplayShared, shareable);
        
        // Track analytics
        TrackReplayShared(replay, platform);
        
        // Open share dialog (platform-specific)
        OpenShareDialog(shareable, platform);
        
        GD.Print($"Shared replay to {platform}");
    }
    
    /// <summary>
    /// Open platform-specific share dialog
    /// </summary>
    private void OpenShareDialog(ShareableReplay shareable, string platform)
    {
        // This would open native share dialogs on mobile
        // For now, copy to clipboard
        DisplayServer.ClipboardSet(shareable.ShareMessage);
        GD.Print($"Share message copied to clipboard: {shareable.ShareMessage}");
    }
    
    /// <summary>
    /// Get all replays
    /// </summary>
    public List<ReplayData> GetAllReplays()
    {
        return _replays.Values.OrderByDescending(r => r.RecordedDate).ToList();
    }
    
    /// <summary>
    /// Get replay by ID
    /// </summary>
    public ReplayData? GetReplay(string replayId)
    {
        return _replays.GetValueOrDefault(replayId);
    }
    
    /// <summary>
    /// Get replays for level
    /// </summary>
    public List<ReplayData> GetReplaysForLevel(string levelId)
    {
        return _replays.Values
            .Where(r => r.LevelId == levelId)
            .OrderByDescending(r => r.Score)
            .ToList();
    }
    
    /// <summary>
    /// Delete replay
    /// </summary>
    public bool DeleteReplay(string replayId)
    {
        if (!_replays.ContainsKey(replayId))
        {
            GD.PrintErr($"Cannot delete replay: {replayId} not found");
            return false;
        }
        
        _replays.Remove(replayId);
        
        // Delete file
        var filePath = $"{ReplaysDataPath}{replayId}.json";
        if (FileAccess.FileExists(filePath))
        {
            DirAccess.RemoveAbsolute(filePath);
        }
        
        GD.Print($"Deleted replay: {replayId}");
        return true;
    }
    
    /// <summary>
    /// Save replay to disk
    /// </summary>
    private void SaveReplay(ReplayData replay)
    {
        try
        {
            // Add to library
            _replays[replay.ReplayId] = replay;
            
            // Enforce max replays limit
            if (_replays.Count > MaxReplaysPerDevice)
            {
                var oldestReplay = _replays.Values.OrderBy(r => r.RecordedDate).First();
                DeleteReplay(oldestReplay.ReplayId);
            }
            
            // Save to file
            var json = JsonConvert.SerializeObject(replay, Formatting.Indented);
            var filePath = $"{ReplaysDataPath}{replay.ReplayId}.json";
            
            using var file = FileAccess.Open(filePath, FileAccess.ModeFlags.Write);
            file?.StoreString(json);
        }
        catch (Exception ex)
        {
            GD.PrintErr($"Failed to save replay: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Load all replays from disk
    /// </summary>
    private void LoadReplays()
    {
        try
        {
            if (!DirAccess.DirExistsAbsolute(ReplaysDataPath))
                return;
            
            using var dir = DirAccess.Open(ReplaysDataPath);
            if (dir == null)
                return;
            
            dir.ListDirBegin();
            var fileName = dir.GetNext();
            
            while (!string.IsNullOrEmpty(fileName))
            {
                if (!dir.CurrentIsDir() && fileName.EndsWith(".json"))
                {
                    var filePath = $"{ReplaysDataPath}{fileName}";
                    
                    using var file = FileAccess.Open(filePath, FileAccess.ModeFlags.Read);
                    var json = file?.GetAsText() ?? "";
                    
                    if (!string.IsNullOrWhiteSpace(json))
                    {
                        var replay = JsonConvert.DeserializeObject<ReplayData>(json);
                        if (replay != null)
                        {
                            _replays[replay.ReplayId] = replay;
                        }
                    }
                }
                
                fileName = dir.GetNext();
            }
            
            dir.ListDirEnd();
            
            GD.Print($"Loaded {_replays.Count} replays");
        }
        catch (Exception ex)
        {
            GD.PrintErr($"Failed to load replays: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Create replays directory
    /// </summary>
    private void CreateReplaysDirectory()
    {
        try
        {
            if (!DirAccess.DirExistsAbsolute(ReplaysDataPath))
            {
                DirAccess.MakeDirAbsolute(ReplaysDataPath);
                GD.Print($"Created replays directory: {ReplaysDataPath}");
            }
        }
        catch (Exception ex)
        {
            GD.PrintErr($"Failed to create replays directory: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Get current player ID
    /// </summary>
    private string GetCurrentPlayerId()
    {
        return PlayerProfile.Instance?.PlayerName ?? "Player";
    }
    
    /// <summary>
    /// Get current player name
    /// </summary>
    private string GetCurrentPlayerName()
    {
        return PlayerProfile.Instance?.PlayerName ?? "Player";
    }
    
    /// <summary>
    /// Get current player cosmetics
    /// </summary>
    private FriendCosmetics GetCurrentPlayerCosmetics()
    {
        if (PlayerProfile.Instance == null)
            return new FriendCosmetics();
        
        return new FriendCosmetics
        {
            HatIndex = PlayerProfile.Instance.SelectedHatIndex,
            GlassesIndex = PlayerProfile.Instance.SelectedGlassesIndex,
            MoustacheIndex = PlayerProfile.Instance.SelectedMoustacheIndex,
            WigIndex = PlayerProfile.Instance.SelectedWigIndex,
            SlingshotSkinIndex = PlayerProfile.Instance.SelectedSlingshotSkinIndex,
            ProjectileSkinIndex = PlayerProfile.Instance.SelectedProjectileSkinIndex
        };
    }
    
    /// <summary>
    /// Track replay recorded analytics
    /// </summary>
    private void TrackReplayRecorded(ReplayData replay)
    {
        try
        {
            if (AnalyticsEventTracker.Instance != null)
            {
                var parameters = new Dictionary<string, object>
                {
                    ["replay_id"] = replay.ReplayId,
                    ["level_id"] = replay.LevelId,
                    ["score"] = replay.Score,
                    ["stars"] = replay.Stars,
                    ["file_size_kb"] = replay.GetFileSizeKB()
                };
                AnalyticsEventTracker.Instance.LogEvent("replay_recorded", parameters);
            }
        }
        catch (Exception ex)
        {
            GD.PrintErr($"Failed to track replay_recorded: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Track replay shared analytics
    /// </summary>
    private void TrackReplayShared(ReplayData replay, string platform)
    {
        try
        {
            if (AnalyticsEventTracker.Instance != null)
            {
                var parameters = new Dictionary<string, object>
                {
                    ["replay_id"] = replay.ReplayId,
                    ["level_id"] = replay.LevelId,
                    ["score"] = replay.Score,
                    ["platform"] = platform
                };
                AnalyticsEventTracker.Instance.LogEvent("replay_shared", parameters);
            }
        }
        catch (Exception ex)
        {
            GD.PrintErr($"Failed to track replay_shared: {ex.Message}");
        }
    }
    
    public bool IsRecording => _isRecording;
    public bool IsPlaying => _isPlaying;
    public float PlaybackSpeed => _playbackSpeed;
}
