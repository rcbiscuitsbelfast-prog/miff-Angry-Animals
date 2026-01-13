using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

/// <summary>
/// Processes telemetry data to identify problem levels and generate difficulty heatmaps
/// Analyzes failure rates, completion times, rage-quit patterns, and player progression
/// </summary>
public class DifficultyHeatmapAnalyzer : Node
{
    public static DifficultyHeatmapAnalyzer Instance { get; private set; }

    // Level difficulty data
    private Dictionary<string, LevelDifficultyData> _levelData = new Dictionary<string, LevelDifficultyData>();
    
    // Rage-quit detection
    private Dictionary<string, List<DateTime>> _playerFailures = new Dictionary<string, List<DateTime>>();
    private const float RAGE_QUIT_THRESHOLD_MINUTES = 2.0f;
    private const int RAGE_QUIT_FAILURE_COUNT = 3;
    
    // Heatmap generation
    private List<HeatmapDataPoint> _heatmapPoints = new List<HeatmapDataPoint>();
    
    [Signal]
    public delegate void DifficultyDataUpdatedEventHandler(string levelId, LevelDifficultyData data);
    
    [Signal]
    public delegate void HeatmapGeneratedEventHandler(List<HeatmapDataPoint> heatmap);
    
    [Signal]
    public delegate void ProblemLevelDetectedEventHandler(string levelId, string issue, float severity);

    public override void _Ready()
    {
        if (Instance != null)
        {
            QueueFree();
            return;
        }
        
        Instance = this;
        InitializeDifficultyAnalyzer();
    }

    /// <summary>
    /// Initialize difficulty analyzer
    /// </summary>
    private void InitializeDifficultyAnalyzer()
    {
        LoadExistingDifficultyData();
        GenerateHeatmap();
        
        GD.Print("Difficulty Heatmap Analyzer initialized");
    }

    /// <summary>
    /// Record level attempt and outcome
    /// </summary>
    public void RecordLevelAttempt(string levelId, bool success, float completionTime, string playerId, int attemptNumber)
    {
        if (!_levelData.ContainsKey(levelId))
        {
            _levelData[levelId] = new LevelDifficultyData { LevelId = levelId };
        }
        
        var data = _levelData[levelId];
        data.TotalAttempts++;
        
        if (success)
        {
            data.SuccessfulAttempts++;
            data.TotalCompletionTime += completionTime;
            data.FirstAttemptSuccessRate = data.SuccessfulAttempts == 1 && attemptNumber == 1;
        }
        else
        {
            data.FailedAttempts++;
        }
        
        data.LastAttemptTime = DateTime.Now;
        
        // Update rage-quit tracking
        UpdateRageQuitTracking(playerId, levelId, success);
        
        // Update analytics
        TrackLevelAttempt(levelId, success, completionTime, attemptNumber);
        
        EmitSignal("DifficultyDataUpdated", levelId, data);
        
        // Check for problem indicators
        CheckForProblemLevel(levelId, data);
    }

    /// <summary>
    /// Update rage-quit tracking for a player
    /// </summary>
    private void UpdateRageQuitTracking(string playerId, string levelId, bool success)
    {
        if (success) return; // Only track failures for rage-quit detection
        
        if (!_playerFailures.ContainsKey(playerId))
        {
            _playerFailures[playerId] = new List<DateTime>();
        }
        
        var failures = _playerFailures[playerId];
        failures.Add(DateTime.Now);
        
        // Remove old failures (older than 10 minutes)
        failures.RemoveAll(f => (DateTime.Now - f).TotalMinutes > 10);
        
        // Check for rage-quit pattern
        var recentFailures = failures.Where(f => (DateTime.Now - f).TotalMinutes <= RAGE_QUIT_THRESHOLD_MINUTES).ToList();
        
        if (recentFailures.Count >= RAGE_QUIT_FAILURE_COUNT)
        {
            var levelData = _levelData[levelId];
            levelData.RageQuits++;
            
            EmitSignal("ProblemLevelDetected", levelId, "Rage Quit Pattern", 0.8f);
            
            GD.Print($"Rage quit detected: Player {playerId} on level {levelId} - {recentFailures.Count} failures in {RAGE_QUIT_THRESHOLD_MINUTES} minutes");
            
            // Track rage-quit event
            if (AnalyticsEventTracker.Instance != null)
            {
                AnalyticsEventTracker.Instance.TrackEvent("rage_quit_detected", new Dictionary<string, object>
                {
                    ["level_id"] = levelId,
                    ["player_id"] = playerId,
                    ["failure_count"] = recentFailures.Count,
                    ["time_window"] = RAGE_QUIT_THRESHOLD_MINUTES
                });
            }
        }
    }

    /// <summary>
    /// Track level attempt in analytics
    /// </summary>
    private void TrackLevelAttempt(string levelId, bool success, float completionTime, int attemptNumber)
    {
        if (AnalyticsEventTracker.Instance != null)
        {
            AnalyticsEventTracker.Instance.TrackEvent("level_attempt", new Dictionary<string, object>
            {
                ["level_id"] = levelId,
                ["success"] = success,
                ["completion_time"] = completionTime,
                ["attempt_number"] = attemptNumber,
                ["timestamp"] = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            });
        }
    }

    /// <summary>
    /// Check if a level has problem indicators
    /// </summary>
    private void CheckForProblemLevel(string levelId, LevelDifficultyData data)
    {
        var failureRate = data.FailureRate;
        var avgCompletionTime = data.AverageCompletionTime;
        
        // High failure rate (> 70%)
        if (failureRate > 0.7f)
        {
            EmitSignal("ProblemLevelDetected", levelId, "High Failure Rate", failureRate);
            GD.Print($"Problem level detected: {levelId} - {failureRate:P1} failure rate");
        }
        
        // Very long completion time (> 5 minutes average)
        if (avgCompletionTime > 300f)
        {
            EmitSignal("ProblemLevelDetected", levelId, "Too Long", avgCompletionTime / 300f);
            GD.Print($"Problem level detected: {levelId} - {avgCompletionTime / 60f:F1} min average completion");
        }
        
        // High rage-quit rate (> 10% of attempts)
        if (data.RageQuitRate > 0.1f)
        {
            EmitSignal("ProblemLevelDetected", levelId, "High Rage-Quit Rate", data.RageQuitRate);
            GD.Print($"Problem level detected: {levelId} - {data.RageQuitRate:P1} rage-quit rate");
        }
        
        // Very low first-attempt success rate (< 20%)
        if (data.FirstAttemptSuccessRate < 0.2f && data.TotalAttempts >= 50)
        {
            EmitSignal("ProblemLevelDetected", levelId, "Poor First Attempt Rate", 1f - data.FirstAttemptSuccessRate);
            GD.Print($"Problem level detected: {levelId} - {data.FirstAttemptSuccessRate:P1} first-attempt success");
        }
    }

    /// <summary>
    /// Generate difficulty heatmap from collected data
    /// </summary>
    public void GenerateHeatmap()
    {
        _heatmapPoints.Clear();
        
        foreach (var levelPair in _levelData)
        {
            var levelId = levelPair.Key;
            var data = levelPair.Value;
            
            var heatmapPoint = new HeatmapDataPoint
            {
                LevelId = levelId,
                DifficultyScore = CalculateDifficultyScore(data),
                FailureRate = data.FailureRate,
                AverageCompletionTime = data.AverageCompletionTime,
                RageQuitRate = data.RageQuitRate,
                TotalAttempts = data.TotalAttempts,
                ColorCode = GetDifficultyColorCode(data)
            };
            
            _heatmapPoints.Add(heatmapPoint);
        }
        
        // Sort by difficulty score (highest difficulty first)
        _heatmapPoints = _heatmapPoints.OrderByDescending(p => p.DifficultyScore).ToList();
        
        EmitSignal("HeatmapGenerated", _heatmapPoints);
        
        GD.Print($"Generated heatmap with {_heatmapPoints.Count} levels");
    }

    /// <summary>
    /// Calculate overall difficulty score for a level (0-100, higher = more difficult)
    /// </summary>
    private float CalculateDifficultyScore(LevelDifficultyData data)
    {
        if (data.TotalAttempts == 0) return 0f;
        
        var failureWeight = data.FailureRate * 40f;
        var timeWeight = Mathf.Clamp(data.AverageCompletionTime / 300f * 30f, 0f, 30f); // Normalize to 5 minutes
        var rageQuitWeight = data.RageQuitRate * 20f;
        var firstAttemptWeight = (1f - data.FirstAttemptSuccessRate) * 10f;
        
        return Mathf.Clamp(failureWeight + timeWeight + rageQuitWeight + firstAttemptWeight, 0f, 100f);
    }

    /// <summary>
    /// Get color code for difficulty visualization
    /// </summary>
    private string GetDifficultyColorCode(LevelDifficultyData data)
    {
        var score = CalculateDifficultyScore(data);
        
        if (score >= 80f) return "red";      // Very hard
        if (score >= 60f) return "orange";   // Hard
        if (score >= 40f) return "yellow";   // Medium
        if (score >= 20f) return "lightgreen"; // Easy
        return "green";                       // Very easy
    }

    /// <summary>
    /// Get difficulty data for a specific level
    /// </summary>
    public LevelDifficultyData GetLevelDifficultyData(string levelId)
    {
        return _levelData.GetValueOrDefault(levelId);
    }

    /// <summary>
    /// Get all difficulty data
    /// </summary>
    public Dictionary<string, LevelDifficultyData> GetAllDifficultyData()
    {
        return _levelData;
    }

    /// <summary>
    /// Get heatmap data for visualization
    /// </summary>
    public List<HeatmapDataPoint> GetHeatmapData()
    {
        return _heatmapPoints;
    }

    /// <summary>
    /// Get top problem levels (highest difficulty scores)
    /// </summary>
    public List<HeatmapDataPoint> GetTopProblemLevels(int count = 5)
    {
        return _heatmapPoints.Take(count).ToList();
    }

    /// <summary>
    /// Get recommendations for level balancing
    /// </summary>
    public List<string> GetBalancingRecommendations(string levelId)
    {
        var recommendations = new List<string>();
        
        if (!_levelData.ContainsKey(levelId)) return recommendations;
        
        var data = _levelData[levelId];
        
        // Failure rate recommendations
        if (data.FailureRate > 0.7f)
        {
            recommendations.Add("TOO HARD - Reduce enemy count or obstacle difficulty");
            recommendations.Add("Consider adding more tutorial hints or guidance");
        }
        else if (data.FailureRate > 0.5f)
        {
            recommendations.Add("HARD - Slightly reduce difficulty or add checkpoints");
        }
        
        // Completion time recommendations
        if (data.AverageCompletionTime > 300f)
        {
            recommendations.Add("TOO LONG - Reduce level length or add time bonuses");
            recommendations.Add("Consider breaking into smaller segments");
        }
        else if (data.AverageCompletionTime > 180f)
        {
            recommendations.Add("LONG - May benefit from performance optimizations");
        }
        
        // Rage-quit recommendations
        if (data.RageQuitRate > 0.15f)
        {
            recommendations.Add("HIGH FRUSTRATION - Add more positive feedback");
            recommendations.Add("Consider adding difficulty scaling or help systems");
        }
        
        // First attempt success recommendations
        if (!data.FirstAttemptSuccessRate && data.TotalAttempts > 20)
        {
            recommendations.Add("POOR FIRST IMPRESSION - Add better onboarding");
        }
        
        return recommendations;
    }

    /// <summary>
    /// Export heatmap data to CSV
    /// </summary>
    public string ExportHeatmapToCSV()
    {
        var csv = new System.Text.StringBuilder();
        csv.AppendLine("Level ID,Failure Rate,Avg Completion (s),Rage Quit Rate,Total Attempts,Difficulty Score,Color Code,Recommendation");
        
        foreach (var point in _heatmapPoints)
        {
            var recommendation = GetBalancingRecommendations(point.LevelId).FirstOrDefault() ?? "Balanced";
            csv.AppendLine($"{point.LevelId},{point.FailureRate:P2},{point.AverageCompletionTime:F1},{point.RageQuitRate:P2},{point.TotalAttempts},{point.DifficultyScore:F1},{point.ColorCode},{recommendation}");
        }
        
        return csv.ToString();
    }

    /// <summary>
    /// Load existing difficulty data from save file
    /// </summary>
    private void LoadExistingDifficultyData()
    {
        try
        {
            var savePath = "user://difficulty_data.json";
            if (FileAccess.FileExists(savePath))
            {
                var file = FileAccess.Open(savePath, FileAccess.ModeFlags.Read);
                var jsonString = file.GetAsText();
                file.Close();
                
                var data = JsonSerializer.Deserialize<Dictionary<string, LevelDifficultyData>>(jsonString);
                if (data != null)
                {
                    _levelData = data;
                    GD.Print($"Loaded difficulty data for {_levelData.Count} levels");
                }
            }
        }
        catch (Exception e)
        {
            GD.PrintErr($"Failed to load difficulty data: {e.Message}");
        }
    }

    /// <summary>
    /// Save difficulty data to file
    /// </summary>
    public void SaveDifficultyData()
    {
        try
        {
            var savePath = "user://difficulty_data.json";
            var file = FileAccess.Open(savePath, FileAccess.ModeFlags.Write);
            
            var jsonString = JsonSerializer.Serialize(_levelData, new JsonSerializerOptions { WriteIndented = true });
            file.StoreString(jsonString);
            file.Close();
            
            GD.Print($"Saved difficulty data for {_levelData.Count} levels");
        }
        catch (Exception e)
        {
            GD.PrintErr($"Failed to save difficulty data: {e.Message}");
        }
    }

    /// <summary>
    /// Reset all difficulty data
    /// </summary>
    public void ResetDifficultyData()
    {
        _levelData.Clear();
        _heatmapPoints.Clear();
        _playerFailures.Clear();
        
        GD.Print("Difficulty data reset");
    }

    /// <summary>
    /// Get difficulty statistics summary
    /// </summary>
    public Dictionary<string, object> GetDifficultyStatistics()
    {
        if (_levelData.Count == 0)
        {
            return new Dictionary<string, object>
            {
                ["total_levels"] = 0,
                ["average_failure_rate"] = 0f,
                ["problem_levels_count"] = 0,
                ["most_difficult_level"] = "None",
                ["easiest_level"] = "None"
            };
        }
        
        var allData = _levelData.Values.ToList();
        var averageFailureRate = allData.Average(d => d.FailureRate);
        var problemLevels = allData.Count(d => d.FailureRate > 0.7f || d.AverageCompletionTime > 300f);
        var mostDifficult = _heatmapPoints.FirstOrDefault();
        var easiest = _heatmapPoints.LastOrDefault();
        
        return new Dictionary<string, object>
        {
            ["total_levels"] = _levelData.Count,
            ["average_failure_rate"] = averageFailureRate,
            ["problem_levels_count"] = problemLevels,
            ["most_difficult_level"] = mostDifficult?.LevelId ?? "None",
            ["easiest_level"] = easiest?.LevelId ?? "None",
            ["total_attempts_tracked"] = allData.Sum(d => d.TotalAttempts),
            ["total_rage_quits"] = allData.Sum(d => d.RageQuits)
        };
    }

    public override void _ExitTree()
    {
        // Save data on exit
        SaveDifficultyData();
    }
}

/// <summary>
/// Difficulty data for a specific level
/// </summary>
public class LevelDifficultyData
{
    public string LevelId { get; set; }
    public int TotalAttempts { get; set; }
    public int SuccessfulAttempts { get; set; }
    public int FailedAttempts { get; set; }
    public float TotalCompletionTime { get; set; }
    public int RageQuits { get; set; }
    public bool FirstAttemptSuccessRate { get; set; }
    public DateTime LastAttemptTime { get; set; }
    
    public float FailureRate => TotalAttempts > 0 ? (float)FailedAttempts / TotalAttempts : 0f;
    public float AverageCompletionTime => SuccessfulAttempts > 0 ? TotalCompletionTime / SuccessfulAttempts : 0f;
    public float RageQuitRate => TotalAttempts > 0 ? (float)RageQuits / TotalAttempts : 0f;
}

/// <summary>
/// Heatmap data point for visualization
/// </summary>
public class HeatmapDataPoint
{
    public string LevelId { get; set; }
    public float DifficultyScore { get; set; }
    public float FailureRate { get; set; }
    public float AverageCompletionTime { get; set; }
    public float RageQuitRate { get; set; }
    public int TotalAttempts { get; set; }
    public string ColorCode { get; set; }
}