using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

/// <summary>
/// Difficulty Heatmap Tracker - Analyzes level difficulty patterns and player frustration
/// Tracks which levels cause most failures, longest completion times, and rage-quit patterns
/// Data can be exported to CSV for spreadsheet analysis
/// </summary>
public class DifficultyHeatmapTracker : Node
{
    public static DifficultyHeatmapTracker Instance { get; private set; }

    // Heatmap data storage
    private Dictionary<int, LevelDifficultyData> _levelData = new Dictionary<int, LevelDifficultyData>();
    private string _dataFilePath = "user://difficulty_heatmap.json";
    
    // Session tracking
    private Dictionary<int, AttemptData> _currentSessionAttempts = new Dictionary<int, AttemptData>();
    private List<DateTime> _recentFailures = new List<DateTime>();
    
    // Rage quit detection settings
    private int _rageQuitThreshold = 3; // Number of failures in time window
    private TimeSpan _rageQuitTimeWindow = TimeSpan.FromMinutes(5); // Time window for rage quit detection
    private TimeSpan _minimumSessionTime = TimeSpan.FromSeconds(30); // Minimum time to count as session
    
    [Signal]
    public delegate void DifficultyAnomalyDetectedEventHandler(int levelNumber, string anomalyType, float severity);
    
    [Signal]
    public delegate void RageQuitDetectedEventHandler(int levelNumber, int failureCount, TimeSpan timeSpan);

    public override void _Ready()
    {
        if (Instance != null)
        {
            QueueFree();
            return;
        }
        
        Instance = this;
        InitializeTracker();
    }

    /// <summary>
    /// Initialize difficulty tracker
    /// </summary>
    private void InitializeTracker()
    {
        LoadHeatmapData();
        GD.Print("Difficulty Heatmap Tracker initialized");
    }

    /// <summary>
    /// Load heatmap data from storage
    /// </summary>
    private void LoadHeatmapData()
    {
        try
        {
            if (FileAccess.FileExists(_dataFilePath))
            {
                using (var file = FileAccess.Open(_dataFilePath, FileAccess.ModeFlags.Read))
                {
                    string json = file.GetAsText();
                    var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var data = System.Text.Json.JsonSerializer.Deserialize<Dictionary<int, LevelDifficultyData>>(json, options);
                    
                    if (data != null)
                    {
                        _levelData = data;
                        GD.Print($"Loaded difficulty data for {_levelData.Count} levels");
                    }
                }
            }
        }
        catch (Exception e)
        {
            GD.PrintErr($"Error loading heatmap data: {e.Message}");
        }
    }

    /// <summary>
    /// Save heatmap data to storage
    /// </summary>
    private void SaveHeatmapData()
    {
        try
        {
            using (var file = FileAccess.Open(_dataFilePath, FileAccess.ModeFlags.Write))
            {
                var options = new System.Text.Json.JsonSerializerOptions { WriteIndented = true };
                string json = System.Text.Json.JsonSerializer.Serialize(_levelData, options);
                file.StoreString(json);
            }
        }
        catch (Exception e)
        {
            GD.PrintErr($"Error saving heatmap data: {e.Message}");
        }
    }

    // ===============================================
    // CORE TRACKING METHODS
    // ===============================================

    /// <summary>
    /// Track level attempt
    /// </summary>
    public void TrackLevelAttempt(int levelNumber, bool completed, float completionTime, int attemptsSoFar, string failureReason = "")
    {
        try
        {
            // Initialize level data if needed
            if (!_levelData.ContainsKey(levelNumber))
            {
                _levelData[levelNumber] = new LevelDifficultyData
                {
                    LevelNumber = levelNumber,
                    TotalAttempts = 0,
                    TotalCompletions = 0,
                    TotalFailures = 0,
                    AverageCompletionTime = 0f,
                    FastestCompletionTime = float.MaxValue,
                    SlowestCompletionTime = 0f,
                    DifficultyScore = 0f,
                    CompletionRate = 0f,
                    RageQuitCount = 0,
                    FirstAttemptSuccess = 0,
                    TotalFirstAttempts = 0,
                    FailureReasons = new Dictionary<string, int>()
                };
            }

            var levelData = _levelData[levelNumber];
            
            // Update attempt count
            levelData.TotalAttempts++;
            
            if (completed)
            {
                levelData.TotalCompletions++;
                UpdateCompletionTimeStats(levelData, completionTime);
                
                // Track first attempt success
                if (attemptsSoFar == 1)
                {
                    levelData.FirstAttemptSuccess++;
                }
                levelData.TotalFirstAttempts++;
                
                // Calculate new difficulty score
                UpdateDifficultyScore(levelData);
                
                // Clear current session attempts for this level
                _currentSessionAttempts.Remove(levelNumber);
            }
            else
            {
                levelData.TotalFailures++;
                
                // Track failure reason
                if (!string.IsNullOrEmpty(failureReason))
                {
                    if (!levelData.FailureReasons.ContainsKey(failureReason))
                    {
                        levelData.FailureReasons[failureReason] = 0;
                    }
                    levelData.FailureReasons[failureReason]++;
                }
                
                // Track session attempt
                TrackSessionAttempt(levelNumber, attemptsSoFar);
                
                // Check for rage quit patterns
                CheckRageQuitPattern(levelNumber);
                
                // Add to recent failures for pattern detection
                _recentFailures.Add(DateTime.Now);
                PruneOldFailures();
            }
            
            // Recalculate completion rate
            levelData.CompletionRate = levelData.TotalAttempts > 0 ? 
                (float)levelData.TotalCompletions / levelData.TotalAttempts * 100f : 0f;
            
            // Save updated data
            SaveHeatmapData();
            
            // Check for difficulty anomalies
            CheckDifficultyAnomalies(levelNumber, levelData);
        }
        catch (Exception e)
        {
            GD.PrintErr($"Error tracking level attempt: {e.Message}");
        }
    }

    /// <summary>
    /// Update completion time statistics
    /// </summary>
    private void UpdateCompletionTimeStats(LevelDifficultyData levelData, float completionTime)
    {
        // Update average
        levelData.AverageCompletionTime = (levelData.AverageCompletionTime * (levelData.TotalCompletions - 1) + completionTime) / levelData.TotalCompletions;
        
        // Update fastest/slowest
        if (completionTime < levelData.FastestCompletionTime)
        {
            levelData.FastestCompletionTime = completionTime;
        }
        
        if (completionTime > levelData.SlowestCompletionTime)
        {
            levelData.SlowestCompletionTime = completionTime;
        }
    }

    /// <summary>
    /// Update difficulty score based on multiple factors
    /// </summary>
    private void UpdateDifficultyScore(LevelDifficultyData levelData)
    {
        if (levelData.TotalAttempts == 0) return;
        
        float difficultyScore = 0f;
        
        // Factor 1: Completion rate (lower = harder)
        float completionRateFactor = (100f - levelData.CompletionRate) / 100f;
        difficultyScore += completionRateFactor * 40f;
        
        // Factor 2: Average completion time vs expected
        float expectedTime = GetExpectedCompletionTime(levelData.LevelNumber);
        float timeFactor = levelData.AverageCompletionTime / expectedTime;
        difficultyScore += Math.Min(timeFactor, 2f) * 30f; // Cap at 2x expected time
        
        // Factor 3: First attempt success rate
        float firstAttemptSuccessRate = levelData.TotalFirstAttempts > 0 ? 
            (float)levelData.FirstAttemptSuccess / levelData.TotalFirstAttempts * 100f : 0f;
        float firstAttemptFactor = (100f - firstAttemptSuccessRate) / 100f;
        difficultyScore += firstAttemptFactor * 20f;
        
        // Factor 4: Rage quit frequency
        float rageQuitFactor = Math.Min(levelData.RageQuitCount / (float)levelData.TotalAttempts, 0.5f);
        difficultyScore += rageQuitFactor * 10f;
        
        levelData.DifficultyScore = Math.Max(0f, Math.Min(100f, difficultyScore));
    }

    /// <summary>
    /// Track session attempt for rage quit detection
    /// </summary>
    private void TrackSessionAttempt(int levelNumber, int attemptsSoFar)
    {
        if (!_currentSessionAttempts.ContainsKey(levelNumber))
        {
            _currentSessionAttempts[levelNumber] = new AttemptData
            {
                FirstAttemptTime = DateTime.Now,
                AttemptCount = 0,
                LastAttemptTime = DateTime.Now
            };
        }
        
        var attemptData = _currentSessionAttempts[levelNumber];
        attemptData.AttemptCount = attemptsSoFar;
        attemptData.LastAttemptTime = DateTime.Now;
    }

    /// <summary>
    /// Check for rage quit patterns
    /// </summary>
    private void CheckRageQuitPattern(int levelNumber)
    {
        if (!_currentSessionAttempts.ContainsKey(levelNumber)) return;
        
        var attemptData = _currentSessionAttempts[levelNumber];
        var sessionDuration = DateTime.Now - attemptData.FirstAttemptTime;
        
        // Check if this qualifies as a rage quit
        if (attemptData.AttemptCount >= _rageQuitThreshold && 
            sessionDuration <= _rageQuitTimeWindow &&
            sessionDuration >= _minimumSessionTime)
        {
            var levelData = _levelData[levelNumber];
            levelData.RageQuitCount++;
            
            EmitSignal("RageQuitDetected", levelNumber, attemptData.AttemptCount, sessionDuration);
            
            GD.Print($"Rage quit detected on level {levelNumber}: {attemptData.AttemptCount} attempts in {sessionDuration.TotalMinutes:F1} minutes");
            
            // Save updated data
            SaveHeatmapData();
        }
    }

    /// <summary>
    /// Check for difficulty anomalies
    /// </summary>
    private void CheckDifficultyAnomalies(int levelNumber, LevelDifficultyData levelData)
    {
        // Anomaly 1: Extremely low completion rate
        if (levelData.CompletionRate < 20f && levelData.TotalAttempts >= 10)
        {
            EmitSignal("DifficultyAnomalyDetected", levelNumber, "extremely_low_completion_rate", 
                (20f - levelData.CompletionRate) / 20f);
        }
        
        // Anomaly 2: Extremely high completion time
        float expectedTime = GetExpectedCompletionTime(levelNumber);
        if (levelData.AverageCompletionTime > expectedTime * 3f && levelData.TotalCompletions >= 5)
        {
            EmitSignal("DifficultyAnomalyDetected", levelNumber, "extremely_slow_completion", 
                (levelData.AverageCompletionTime / expectedTime - 2f) / 3f);
        }
        
        // Anomaly 3: High rage quit rate
        if (levelData.TotalAttempts >= 10 && levelData.RageQuitCount / (float)levelData.TotalAttempts > 0.3f)
        {
            EmitSignal("DifficultyAnomalyDetected", levelNumber, "high_rage_quit_rate", 
                (levelData.RageQuitCount / (float)levelData.TotalAttempts - 0.2f) / 0.3f);
        }
        
        // Anomaly 4: Difficulty spike compared to neighbors
        CheckDifficultySpike(levelNumber, levelData);
    }

    /// <summary>
    /// Check for difficulty spikes compared to adjacent levels
    /// </summary>
    private void CheckDifficultySpike(int levelNumber, LevelDifficultyData currentLevel)
    {
        var neighborLevels = new[] { levelNumber - 1, levelNumber + 1 };
        float neighborAverage = 0f;
        int neighborCount = 0;
        
        foreach (var neighborLevel in neighborLevels)
        {
            if (_levelData.ContainsKey(neighborLevel))
            {
                neighborAverage += _levelData[neighborLevel].DifficultyScore;
                neighborCount++;
            }
        }
        
        if (neighborCount > 0)
        {
            neighborAverage /= neighborCount;
            
            // If current level is significantly harder than neighbors
            if (currentLevel.DifficultyScore > neighborAverage + 20f)
            {
                float severity = (currentLevel.DifficultyScore - neighborAverage - 20f) / 30f;
                EmitSignal("DifficultyAnomalyDetected", levelNumber, "difficulty_spike", Math.Min(severity, 1f));
            }
        }
    }

    /// <summary>
    /// Prune old failures from recent failures list
    /// </summary>
    private void PruneOldFailures()
    {
        var cutoffTime = DateTime.Now - _rageQuitTimeWindow;
        _recentFailures.RemoveAll(failure => failure < cutoffTime);
    }

    /// <summary>
    /// Get expected completion time for a level
    /// </summary>
    private float GetExpectedCompletionTime(int levelNumber)
    {
        // Simple progression: 30 seconds for level 1, +2 seconds per level
        return 30f + (levelNumber - 1) * 2f;
    }

    // ===============================================
    // DATA ANALYSIS & EXPORT
    // ===============================================

    /// <summary>
    /// Get level difficulty data
    /// </summary>
    public LevelDifficultyData GetLevelData(int levelNumber)
    {
        return _levelData.ContainsKey(levelNumber) ? _levelData[levelNumber] : null;
    }

    /// <summary>
    /// Get all difficulty data
    /// </summary>
    public Dictionary<int, LevelDifficultyData> GetAllLevelData()
    {
        return new Dictionary<int, LevelDifficultyData>(_levelData);
    }

    /// <summary>
    /// Get most problematic levels
    /// </summary>
    public List<LevelDifficultyData> GetMostProblematicLevels(int count = 10)
    {
        return _levelData.Values
            .Where(data => data.TotalAttempts >= 5) // Only levels with sufficient data
            .OrderByDescending(data => data.DifficultyScore)
            .Take(count)
            .ToList();
    }

    /// <summary>
    /// Get easiest levels
    /// </summary>
    public List<LevelDifficultyData> GetEasiestLevels(int count = 10)
    {
        return _levelData.Values
            .Where(data => data.TotalAttempts >= 5) // Only levels with sufficient data
            .OrderBy(data => data.DifficultyScore)
            .Take(count)
            .ToList();
    }

    /// <summary>
    /// Get levels with highest rage quit rates
    /// </summary>
    public List<LevelDifficultyData> GetHighestRageQuitLevels(int count = 10)
    {
        return _levelData.Values
            .Where(data => data.TotalAttempts >= 10)
            .Where(data => data.RageQuitCount > 0)
            .OrderByDescending(data => data.RageQuitCount / (float)data.TotalAttempts)
            .Take(count)
            .ToList();
    }

    /// <summary>
    /// Generate difficulty summary report
    /// </summary>
    public DifficultySummary GenerateSummary()
    {
        var levels = _levelData.Values.ToList();
        
        return new DifficultySummary
        {
            TotalLevels = _levelData.Count,
            TotalAttempts = levels.Sum(l => l.TotalAttempts),
            AverageCompletionRate = levels.Average(l => l.CompletionRate),
            AverageDifficultyScore = levels.Average(l => l.DifficultyScore),
            TotalRageQuits = levels.Sum(l => l.RageQuitCount),
            MostProblematicLevel = levels.OrderByDescending(l => l.DifficultyScore).FirstOrDefault()?.LevelNumber ?? 0,
            EasiestLevel = levels.OrderBy(l => l.DifficultyScore).FirstOrDefault()?.LevelNumber ?? 0,
            LevelsNeedingAttention = GetMostProblematicLevels(10).Select(l => l.LevelNumber).ToList(),
            AverageRageQuitRate = levels.Average(l => l.TotalAttempts > 0 ? (float)l.RageQuitCount / l.TotalAttempts : 0f)
        };
    }

    /// <summary>
    /// Export heatmap data to CSV
    /// </summary>
    public void ExportToCsv(string filePath = "user://difficulty_heatmap_export.csv")
    {
        try
        {
            using (var file = FileAccess.Open(filePath, FileAccess.ModeFlags.Write))
            {
                // CSV Header
                file.StoreString("Level Number,Total Attempts,Completions,Failures,Completion Rate %,Average Time (s),Fastest Time (s),Slowest Time (s),Difficulty Score,Rage Quits,First Attempt Success Rate %,Failure Reasons\n");
                
                // Data rows
                foreach (var levelData in _levelData.Values.OrderBy(l => l.LevelNumber))
                {
                    float firstAttemptSuccessRate = levelData.TotalFirstAttempts > 0 ? 
                        (float)levelData.FirstAttemptSuccess / levelData.TotalFirstAttempts * 100f : 0f;
                    
                    string failureReasons = string.Join("; ", levelData.FailureReasons.Select(fr => $"{fr.Key}({fr.Value})"));
                    
                    string row = $"{levelData.LevelNumber}," +
                                $"{levelData.TotalAttempts}," +
                                $"{levelData.TotalCompletions}," +
                                $"{levelData.TotalFailures}," +
                                $"{levelData.CompletionRate:F1}," +
                                $"{levelData.AverageCompletionTime:F1}," +
                                $"{(levelData.FastestCompletionTime == float.MaxValue ? 0 : levelData.FastestCompletionTime):F1}," +
                                $"{levelData.SlowestCompletionTime:F1}," +
                                $"{levelData.DifficultyScore:F1}," +
                                $"{levelData.RageQuitCount}," +
                                $"{firstAttemptSuccessRate:F1}," +
                                $"\"{failureReasons}\"\n";
                    
                    file.StoreString(row);
                }
            }
            
            GD.Print($"Difficulty heatmap exported to: {filePath}");
        }
        catch (Exception e)
        {
            GD.PrintErr($"Error exporting heatmap to CSV: {e.Message}");
        }
    }

    /// <summary>
    /// Clear all heatmap data
    /// </summary>
    public void ClearData()
    {
        _levelData.Clear();
        _currentSessionAttempts.Clear();
        _recentFailures.Clear();
        
        try
        {
            if (FileAccess.FileExists(_dataFilePath))
            {
                DirAccess.RemoveAbsolute(_dataFilePath);
            }
        }
        catch (Exception e)
        {
            GD.PrintErr($"Error clearing heatmap data: {e.Message}");
        }
        
        GD.Print("Difficulty heatmap data cleared");
    }

    /// <summary>
    /// Get statistics for current session
    /// </summary>
    public Dictionary<string, object> GetSessionStats()
    {
        var activeLevels = _currentSessionAttempts.Keys.ToList();
        
        return new Dictionary<string, object>
        {
            { "active_levels", activeLevels.Count },
            { "total_attempts_session", _currentSessionAttempts.Values.Sum(a => a.AttemptCount) },
            { "recent_failures", _recentFailures.Count },
            { "rage_quit_candidates", _currentSessionAttempts.Values.Count(a => a.AttemptCount >= _rageQuitThreshold) }
        };
    }
}

// ===============================================
// DATA STRUCTURES
// ===============================================

/// <summary>
/// Level difficulty data structure
/// </summary>
public class LevelDifficultyData
{
    public int LevelNumber { get; set; }
    public int TotalAttempts { get; set; }
    public int TotalCompletions { get; set; }
    public int TotalFailures { get; set; }
    public float AverageCompletionTime { get; set; }
    public float FastestCompletionTime { get; set; }
    public float SlowestCompletionTime { get; set; }
    public float DifficultyScore { get; set; }
    public float CompletionRate { get; set; }
    public int RageQuitCount { get; set; }
    public int FirstAttemptSuccess { get; set; }
    public int TotalFirstAttempts { get; set; }
    public Dictionary<string, int> FailureReasons { get; set; } = new Dictionary<string, int>();
}

/// <summary>
/// Session attempt data
/// </summary>
public class AttemptData
{
    public DateTime FirstAttemptTime { get; set; }
    public int AttemptCount { get; set; }
    public DateTime LastAttemptTime { get; set; }
}

/// <summary>
/// Difficulty summary report
/// </summary>
public class DifficultySummary
{
    public int TotalLevels { get; set; }
    public int TotalAttempts { get; set; }
    public float AverageCompletionRate { get; set; }
    public float AverageDifficultyScore { get; set; }
    public int TotalRageQuits { get; set; }
    public int MostProblematicLevel { get; set; }
    public int EasiestLevel { get; set; }
    public List<int> LevelsNeedingAttention { get; set; } = new List<int>();
    public float AverageRageQuitRate { get; set; }
}