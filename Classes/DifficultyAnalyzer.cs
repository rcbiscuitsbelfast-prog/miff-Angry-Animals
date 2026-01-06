using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

/// <summary>
/// Difficulty curve analyzer and tuning system
/// Analyzes level difficulty and suggests balance adjustments
/// </summary>
public class DifficultyAnalyzer : Node
{
    public static DifficultyAnalyzer Instance { get; private set; }

    // Difficulty tracking
    private List<LevelDifficultyData> _levelData = new List<LevelDifficultyData>();
    private Dictionary<int, DifficultyMetrics> _difficultyHistory = new Dictionary<int, DifficultyMetrics>();
    
    // Analysis configuration
    private DifficultyConfig _config;
    private string _analysisFilePath = "user://difficulty_analysis.json";
    
    // Difficulty curve visualization data
    private List<Vector2> _difficultyCurve = new List<Vector2>();
    private List<string> _difficultySpikes = new List<string>();
    
    [Signal]
    public delegate void DifficultyAnalyzedEventHandler(List<BalanceSuggestion> suggestions);
    
    [Signal]
    public delegate void DifficultySpikeDetectedEventHandler(int levelNumber, float spikeIntensity);
    
    [Signal]
    public delegate void LevelBalanceRecommendedEventHandler(int levelNumber, LevelBalanceRecommendations recommendations);

    public override void _Ready()
    {
        if (Instance != null)
        {
            QueueFree();
            return;
        }
        
        Instance = this;
        InitializeAnalyzer();
    }

    /// <summary>
    /// Initialize difficulty analyzer
    /// </summary>
    private void InitializeAnalyzer()
    {
        LoadConfiguration();
        LoadHistoricalData();
        
        GD.Print("Difficulty analyzer initialized");
    }

    /// <summary>
    /// Load analysis configuration
    /// </summary>
    private void LoadConfiguration()
    {
        _config = new DifficultyConfig
        {
            SampleSize = 10, // Number of attempts to analyze
            DifficultyThreshold = 3.0f, // Spike threshold
            TargetCompletionRate = 0.7f, // 70% target success rate
            OptimalAttemptsRange = new Vector2(1.5f, 3.0f),
            TargetPlayTimeRange = new Vector2(30f, 120f), // seconds
            SpikeDetectionSensitivity = 0.5f,
            BalanceCheckInterval = 10, // levels
            EnableRealTimeAnalysis = true
        };
    }

    /// <summary>
    /// Load historical difficulty data
    /// </summary>
    private void LoadHistoricalData()
    {
        try
        {
            if (File.Exists(_analysisFilePath))
            {
                string jsonContent = File.ReadAllText(_analysisFilePath);
                var analysisData = JsonSerializer.Deserialize<DifficultyAnalysisData>(jsonContent);
                
                if (analysisData?.LevelData != null)
                {
                    _levelData = analysisData.LevelData;
                }
                
                if (analysisData?.DifficultyHistory != null)
                {
                    _difficultyHistory = analysisData.DifficultyHistory
                        .ToDictionary(d => d.LevelNumber, d => d);
                }
            }
        }
        catch (Exception e)
        {
            GD.PrintErr($"Failed to load difficulty data: {e.Message}");
        }
    }

    /// <summary>
    /// Record level attempt data
    /// </summary>
    public void RecordLevelAttempt(int levelNumber, bool completed, int attempts, float timeSpent, int hintsUsed = 0)
    {
        var existingData = _levelData.FirstOrDefault(d => d.LevelNumber == levelNumber);
        
        if (existingData == null)
        {
            existingData = new LevelDifficultyData
            {
                LevelNumber = levelNumber,
                TotalAttempts = 0,
                SuccessfulAttempts = 0,
                TotalTimeSpent = 0f,
                HintsUsed = 0,
                AttemptsHistory = new List<AttemptData>(),
                DifficultyScore = 0f
            };
            _levelData.Add(existingData);
        }
        
        // Update statistics
        existingData.TotalAttempts++;
        if (completed) existingData.SuccessfulAttempts++;
        existingData.TotalTimeSpent += timeSpent;
        existingData.HintsUsed += hintsUsed;
        
        // Add attempt history
        existingData.AttemptsHistory.Add(new AttemptData
        {
            Completed = completed,
            Attempts = attempts,
            TimeSpent = timeSpent,
            HintsUsed = hintsUsed,
            Timestamp = DateTime.Now
        });
        
        // Keep only recent attempts
        if (existingData.AttemptsHistory.Count > _config.SampleSize)
        {
            existingData.AttemptsHistory.RemoveAt(0);
        }
        
        // Recalculate difficulty score
        CalculateDifficultyScore(existingData);
        
        // Save data
        SaveAnalysisData();
        
        // Real-time analysis
        if (_config.EnableRealTimeAnalysis)
        {
            AnalyzeDifficulty(levelNumber);
        }
    }

    /// <summary>
    /// Calculate difficulty score for a level
    /// </summary>
    private void CalculateDifficultyScore(LevelDifficultyData data)
    {
        if (data.AttemptsHistory.Count == 0)
        {
            data.DifficultyScore = 1.0f;
            return;
        }
        
        float successRate = data.SuccessfulAttempts / (float)data.TotalAttempts;
        float avgAttempts = data.AttemptsHistory.Average(a => a.Attempts);
        float avgTime = data.AttemptsHistory.Average(a => a.TimeSpent);
        float hintUsageRate = data.HintsUsed / (float)data.TotalAttempts;
        
        // Difficulty score components
        float attemptFactor = Mathf.InverseLerp(_config.OptimalAttemptsRange.X, _config.OptimalAttemptsRange.Y * 2, avgAttempts);
        float timeFactor = Mathf.InverseLerp(_config.TargetPlayTimeRange.X, _config.TargetPlayTimeRange.Y * 2, avgTime);
        float hintFactor = Mathf.InverseLerp(0f, 0.5f, hintUsageRate);
        float failureFactor = Mathf.InverseLerp(1.0f, 0.3f, successRate);
        
        // Weighted difficulty score (0.0 = very easy, 5.0 = very hard)
        data.DifficultyScore = (attemptFactor * 0.3f + timeFactor * 0.25f + hintFactor * 0.2f + failureFactor * 0.25f) * 5.0f;
    }

    /// <summary>
    /// Analyze difficulty for a specific level
    /// </summary>
    public void AnalyzeDifficulty(int levelNumber)
    {
        var levelData = _levelData.FirstOrDefault(d => d.LevelNumber == levelNumber);
        if (levelData == null || levelData.AttemptsHistory.Count == 0) return;
        
        var analysis = new LevelDifficultyAnalysis
        {
            LevelNumber = levelNumber,
            CurrentDifficulty = levelData.DifficultyScore,
            SuccessRate = levelData.SuccessfulAttempts / (float)levelData.TotalAttempts,
            AverageAttempts = levelData.AttemptsHistory.Average(a => a.Attempts),
            AverageTimeSpent = levelData.AttemptsHistory.Average(a => a.TimeSpent),
            HintUsageRate = levelData.HintsUsed / (float)levelData.TotalAttempts,
            TrendDirection = CalculateTrendDirection(levelData.AttemptsHistory)
        };
        
        // Detect difficulty spikes
        DetectDifficultySpikes(analysis);
        
        // Generate balance suggestions
        var suggestions = GenerateBalanceSuggestions(analysis);
        
        EmitSignal("DifficultyAnalyzed", suggestions);
        
        if (suggestions.Any())
        {
            foreach (var suggestion in suggestions)
            {
                GD.Print($"Difficulty analysis for level {levelNumber}: {suggestion.Type} - {suggestion.Description}");
            }
        }
    }

    /// <summary>
    /// Analyze difficulty curve across all levels
    /// </summary>
    public void AnalyzeDifficultyCurve()
    {
        _difficultyCurve.Clear();
        _difficultySpikes.Clear();
        
        // Sort level data by level number
        var sortedLevels = _levelData.OrderBy(d => d.LevelNumber).ToList();
        
        foreach (var levelData in sortedLevels)
        {
            _difficultyCurve.Add(new Vector2(levelData.LevelNumber, levelData.DifficultyScore));
        }
        
        // Detect spikes in the curve
        DetectCurveSpikes();
        
        // Generate overall analysis
        var overallAnalysis = GenerateOverallAnalysis();
        
        GD.Print($"Difficulty curve analyzed: {_difficultyCurve.Count} levels, {_difficultySpikes.Count} spikes detected");
    }

    /// <summary>
    /// Detect difficulty spikes in the curve
    /// </summary>
    private void DetectCurveSpikes()
    {
        for (int i = 1; i < _difficultyCurve.Count - 1; i++)
        {
            var prev = _difficultyCurve[i - 1];
            var current = _difficultyCurve[i];
            var next = _difficultyCurve[i + 1];
            
            // Calculate spike intensity
            float spikeIntensity = 0f;
            
            // Upward spike
            if (current.y > prev.y + _config.DifficultyThreshold && current.y > next.y + _config.DifficultyThreshold)
            {
                spikeIntensity = (current.y - prev.y) / prev.y;
                _difficultySpikes.Add($"Level {current.x}: Upward spike (+{spikeIntensity:P1})");
                EmitSignal("DifficultySpikeDetected", (int)current.x, spikeIntensity);
            }
            
            // Downward spike (too easy)
            if (current.y < prev.y - _config.DifficultyThreshold && current.y < next.y - _config.DifficultyThreshold)
            {
                spikeIntensity = (prev.y - current.y) / prev.y;
                _difficultySpikes.Add($"Level {current.x}: Downward spike (-{spikeIntensity:P1})");
                EmitSignal("DifficultySpikeDetected", (int)current.x, -spikeIntensity);
            }
        }
    }

    /// <summary>
    /// Calculate trend direction for attempt history
    /// </summary>
    private TrendDirection CalculateTrendDirection(List<AttemptData> attempts)
    {
        if (attempts.Count < 3) return TrendDirection.Stable;
        
        var recent = attempts.TakeLast(5).ToList();
        var older = attempts.Take(5).ToList();
        
        float recentAvgAttempts = recent.Average(a => a.Attempts);
        float olderAvgAttempts = older.Average(a => a.Attempts);
        
        float difference = recentAvgAttempts - olderAvgAttempts;
        
        if (difference > 0.5f) return TrendDirection.Increasing;
        if (difference < -0.5f) return TrendDirection.Decreasing;
        return TrendDirection.Stable;
    }

    /// <summary>
    /// Generate balance suggestions for a level
    /// </summary>
    private List<BalanceSuggestion> GenerateBalanceSuggestions(LevelDifficultyAnalysis analysis)
    {
        var suggestions = new List<BalanceSuggestion>();
        
        // Success rate analysis
        if (analysis.SuccessRate < 0.3f)
        {
            suggestions.Add(new BalanceSuggestion
            {
                Type = SuggestionType.ReduceDifficulty,
                Priority = SuggestionPriority.High,
                Description = "Success rate too low - consider reducing difficulty",
                SpecificChanges = new List<string>
                {
                    "Reduce target requirements",
                    "Add more hints",
                    "Simplify level layout",
                    "Increase projectile power"
                }
            });
        }
        else if (analysis.SuccessRate > 0.9f)
        {
            suggestions.Add(new BalanceSuggestion
            {
                Type = SuggestionType.IncreaseDifficulty,
                Priority = SuggestionPriority.Medium,
                Description = "Success rate too high - consider increasing challenge",
                SpecificChanges = new List<string>
                {
                    "Add more obstacles",
                    "Require precision shots",
                    "Increase target requirements",
                    "Reduce available attempts"
                }
            });
        }
        
        // Attempts analysis
        if (analysis.AverageAttempts > _config.OptimalAttemptsRange.Y)
        {
            suggestions.Add(new BalanceSuggestion
            {
                Type = SuggestionType.ReduceDifficulty,
                Priority = SuggestionPriority.High,
                Description = "Average attempts too high - level may be too difficult",
                SpecificChanges = new List<string>
                {
                    "Wider targeting area",
                    "Clearer line of sight",
                    "More forgiving physics",
                    "Better visual cues"
                }
            });
        }
        
        // Time analysis
        if (analysis.AverageTimeSpent > _config.TargetPlayTimeRange.Y)
        {
            suggestions.Add(new BalanceSuggestion
            {
                Type = SuggestionType.ReduceDifficulty,
                Priority = SuggestionPriority.Medium,
                Description = "Level takes too long to complete",
                SpecificChanges = new List<string>
                {
                    "Simplify target placement",
                    "Reduce required precision",
                    "Add visual guidance",
                    "Optimize level flow"
                }
            });
        }
        else if (analysis.AverageTimeSpent < _config.TargetPlayTimeRange.X)
        {
            suggestions.Add(new BalanceSuggestion
            {
                Type = SuggestionType.IncreaseDifficulty,
                Priority = SuggestionPriority.Low,
                Description = "Level completed too quickly - may lack engagement",
                SpecificChanges = new List<string>
                {
                    "Add more complex mechanics",
                    "Increase target requirements",
                    "Add bonus challenges",
                    "Extend level length"
                }
            });
        }
        
        // Hint usage analysis
        if (analysis.HintUsageRate > 0.5f)
        {
            suggestions.Add(new BalanceSuggestion
            {
                Type = SuggestionType.ImproveClarity,
                Priority = SuggestionPriority.Medium,
                Description = "High hint usage - level may need clearer objectives",
                SpecificChanges = new List<string>
                {
                    "Improve visual communication",
                    "Add clearer objectives",
                    "Better tutorial integration",
                    "Enhanced feedback systems"
                }
            });
        }
        
        // Trend analysis
        if (analysis.TrendDirection == TrendDirection.Increasing)
        {
            suggestions.Add(new BalanceSuggestion
            {
                Type = SuggestionType.MonitorDifficulty,
                Priority = SuggestionPriority.Low,
                Description = "Difficulty trending upward - monitor for balance issues",
                SpecificChanges = new List<string>
                {
                    "Collect more data",
                    "Watch for player frustration",
                    "Consider gradual difficulty curve",
                    "Monitor completion rates"
                }
            });
        }
        
        return suggestions;
    }

    /// <summary>
    /// Generate overall difficulty curve analysis
    /// </summary>
    private OverallDifficultyAnalysis GenerateOverallAnalysis()
    {
        var analysis = new OverallDifficultyAnalysis
        {
            TotalLevels = _difficultyCurve.Count,
            AverageDifficulty = _difficultyCurve.Count > 0 ? _difficultyCurve.Average(p => p.y) : 0f,
            DifficultyVariance = CalculateVariance(_difficultyCurve.Select(p => p.y).ToList()),
            SpikeCount = _difficultySpikes.Count,
            SmoothnessScore = CalculateSmoothnessScore(),
            BalanceScore = CalculateBalanceScore()
        };
        
        // Categorize levels
        analysis.EasyLevels = _difficultyCurve.Count(p => p.y < 1.5f);
        analysis.MediumLevels = _difficultyCurve.Count(p => p.y >= 1.5f && p.y < 3.0f);
        analysis.HardLevels = _difficultyCurve.Count(p => p.y >= 3.0f);
        
        // Generate overall recommendations
        analysis.OverallRecommendations = GenerateOverallRecommendations(analysis);
        
        return analysis;
    }

    /// <summary>
    /// Calculate variance of difficulty scores
    /// </summary>
    private float CalculateVariance(List<float> values)
    {
        if (values.Count < 2) return 0f;
        
        float mean = values.Average();
        return values.Sum(v => (v - mean) * (v - mean)) / values.Count;
    }

    /// <summary>
    /// Calculate difficulty curve smoothness score
    /// </summary>
    private float CalculateSmoothnessScore()
    {
        if (_difficultyCurve.Count < 3) return 1.0f;
        
        float totalVariation = 0f;
        for (int i = 1; i < _difficultyCurve.Count; i++)
        {
            totalVariation += Mathf.Abs(_difficultyCurve[i].y - _difficultyCurve[i - 1].y);
        }
        
        float maxPossibleVariation = _difficultyCurve.Count * 5.0f; // Assuming max difficulty of 5
        return 1.0f - (totalVariation / maxPossibleVariation);
    }

    /// <summary>
    /// Calculate overall balance score
    /// </summary>
    private float CalculateBalanceScore()
    {
        if (_difficultyCurve.Count == 0) return 0f;
        
        float smoothness = CalculateSmoothnessScore();
        float targetDistribution = CalculateTargetDistributionScore();
        float spikePenalty = Mathf.Clamp(1.0f - (_difficultySpikes.Count / 10f), 0f, 1f);
        
        return (smoothness * 0.4f + targetDistribution * 0.4f + spikePenalty * 0.2f) * 100f;
    }

    /// <summary>
    /// Calculate target distribution score
    /// </summary>
    private float CalculateTargetDistributionScore()
    {
        if (_difficultyCurve.Count == 0) return 0f;
        
        int easyCount = _difficultyCurve.Count(p => p.y < 1.5f);
        int mediumCount = _difficultyCurve.Count(p => p.y >= 1.5f && p.y < 3.0f);
        int hardCount = _difficultyCurve.Count(p => p.y >= 3.0f);
        
        float easyTarget = 0.3f; // 30% easy
        float mediumTarget = 0.5f; // 50% medium
        float hardTarget = 0.2f; // 20% hard
        
        float easyScore = 1.0f - Mathf.Abs((easyCount / (float)_difficultyCurve.Count) - easyTarget);
        float mediumScore = 1.0f - Mathf.Abs((mediumCount / (float)_difficultyCurve.Count) - mediumTarget);
        float hardScore = 1.0f - Mathf.Abs((hardCount / (float)_difficultyCurve.Count) - hardTarget);
        
        return (easyScore + mediumScore + hardScore) / 3f;
    }

    /// <summary>
    /// Generate overall recommendations
    /// </summary>
    private List<OverallRecommendation> GenerateOverallRecommendations(OverallDifficultyAnalysis analysis)
    {
        var recommendations = new List<OverallRecommendation>();
        
        // Difficulty curve smoothness
        if (analysis.SmoothnessScore < 0.7f)
        {
            recommendations.Add(new OverallRecommendation
            {
                Type = OverallRecommendationType.ImproveCurveSmoothness,
                Priority = RecommendationPriority.High,
                Description = "Difficulty curve has rough transitions",
                Impact = "May cause player frustration or boredom",
                SuggestedActions = new List<string>
                {
                    "Adjust level difficulty gradually",
                    "Smooth out sudden difficulty spikes",
                    "Add transition levels between difficulty jumps",
                    "Review level sequence order"
                }
            });
        }
        
        // Balance score
        if (analysis.BalanceScore < 70f)
        {
            recommendations.Add(new OverallRecommendation
            {
                Type = OverallRecommendationType.ImproveOverallBalance,
                Priority = RecommendationPriority.High,
                Description = "Overall level balance needs improvement",
                Impact = "May affect player retention and enjoyment",
                SuggestedActions = new List<string>
                {
                    "Redistribute difficulty across levels",
                    "Add more medium-difficulty levels",
                    "Reduce extreme difficulty spikes",
                    "Improve level progression curve"
                }
            });
        }
        
        // Level distribution
        if (analysis.EasyLevels < 10 || analysis.HardLevels < 5)
        {
            recommendations.Add(new OverallRecommendation
            {
                Type = OverallRecommendationType.AdjustLevelDistribution,
                Priority = RecommendationPriority.Medium,
                Description = "Unbalanced distribution of difficulty levels",
                Impact = "May not provide appropriate challenge progression",
                SuggestedActions = new List<string>
                {
                    "Add more easy levels for onboarding",
                    "Include challenging levels for experienced players",
                    "Ensure smooth difficulty progression",
                    "Balance tutorial and advanced content"
                }
            });
        }
        
        return recommendations;
    }

    /// <summary>
    /// Get level balance recommendations
    /// </summary>
    public LevelBalanceRecommendations GetLevelBalanceRecommendations(int levelNumber)
    {
        var levelData = _levelData.FirstOrDefault(d => d.LevelNumber == levelNumber);
        if (levelData == null) return null;
        
        AnalyzeDifficulty(levelNumber);
        
        var suggestions = GenerateBalanceSuggestions(new LevelDifficultyAnalysis
        {
            LevelNumber = levelNumber,
            CurrentDifficulty = levelData.DifficultyScore,
            SuccessRate = levelData.SuccessfulAttempts / (float)levelData.TotalAttempts,
            AverageAttempts = levelData.AttemptsHistory.Average(a => a.Attempts),
            AverageTimeSpent = levelData.AttemptsHistory.Average(a => a.TimeSpent),
            HintUsageRate = levelData.HintsUsed / (float)levelData.TotalAttempts,
            TrendDirection = CalculateTrendDirection(levelData.AttemptsHistory)
        });
        
        return new LevelBalanceRecommendations
        {
            LevelNumber = levelNumber,
            CurrentDifficulty = levelData.DifficultyScore,
            Recommendations = suggestions,
            AnalysisData = levelData
        };
    }

    /// <summary>
    /// Get difficulty curve data for visualization
    /// </summary>
    public List<Vector2> GetDifficultyCurve()
    {
        return new List<Vector2>(_difficultyCurve);
    }

    /// <summary>
    /// Get detected difficulty spikes
    /// </summary>
    public List<string> GetDifficultySpikes()
    {
        return new List<string>(_difficultySpikes);
    }

    /// <summary>
    /// Export difficulty analysis report
    /// </summary>
    public void ExportAnalysisReport(string filePath)
    {
        try
        {
            var report = new DifficultyAnalysisReport
            {
                GeneratedAt = DateTime.Now,
                TotalLevels = _difficultyCurve.Count,
                AverageDifficulty = _difficultyCurve.Count > 0 ? _difficultyCurve.Average(p => p.y) : 0f,
                DifficultySpikes = new List<string>(_difficultySpikes),
                LevelData = _levelData,
                OverallAnalysis = GenerateOverallAnalysis()
            };
            
            var options = new JsonSerializerOptions 
            { 
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            
            string json = JsonSerializer.Serialize(report, options);
            File.WriteAllText(filePath, json);
            
            GD.Print($"Difficulty analysis report exported: {filePath}");
        }
        catch (Exception e)
        {
            GD.PrintErr($"Failed to export analysis report: {e.Message}");
        }
    }

    /// <summary>
    /// Save analysis data to file
    /// </summary>
    private void SaveAnalysisData()
    {
        try
        {
            var data = new DifficultyAnalysisData
            {
                LevelData = _levelData,
                DifficultyHistory = _difficultyHistory.Values.ToList(),
                LastUpdated = DateTime.Now
            };
            
            var options = new JsonSerializerOptions 
            { 
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            
            string json = JsonSerializer.Serialize(data, options);
            File.WriteAllText(_analysisFilePath, json);
        }
        catch (Exception e)
        {
            GD.PrintErr($"Failed to save analysis data: {e.Message}");
        }
    }

    /// <summary>
    /// Reset analysis data
    /// </summary>
    public void ResetAnalysisData()
    {
        _levelData.Clear();
        _difficultyHistory.Clear();
        _difficultyCurve.Clear();
        _difficultySpikes.Clear();
        
        SaveAnalysisData();
        
        GD.Print("Difficulty analysis data reset");
    }

    /// <summary>
    /// Get configuration
    /// </summary>
    public DifficultyConfig GetConfig()
    {
        return _config;
    }

    /// <summary>
    /// Update configuration
    /// </summary>
    public void UpdateConfig(Action<DifficultyConfig> configUpdater)
    {
        configUpdater(_config);
    }
}

/// <summary>
/// Level difficulty data
/// </summary>
public class LevelDifficultyData
{
    public int LevelNumber { get; set; }
    public int TotalAttempts { get; set; }
    public int SuccessfulAttempts { get; set; }
    public float TotalTimeSpent { get; set; }
    public int HintsUsed { get; set; }
    public float DifficultyScore { get; set; }
    public List<AttemptData> AttemptsHistory { get; set; } = new List<AttemptData>();
}

/// <summary>
/// Individual attempt data
/// </summary>
public class AttemptData
{
    public bool Completed { get; set; }
    public int Attempts { get; set; }
    public float TimeSpent { get; set; }
    public int HintsUsed { get; set; }
    public DateTime Timestamp { get; set; }
}

/// <summary>
/// Difficulty analysis configuration
/// </summary>
public class DifficultyConfig
{
    public int SampleSize { get; set; }
    public float DifficultyThreshold { get; set; }
    public float TargetCompletionRate { get; set; }
    public Vector2 OptimalAttemptsRange { get; set; }
    public Vector2 TargetPlayTimeRange { get; set; }
    public float SpikeDetectionSensitivity { get; set; }
    public int BalanceCheckInterval { get; set; }
    public bool EnableRealTimeAnalysis { get; set; }
}

/// <summary>
/// Level difficulty analysis
/// </summary>
public class LevelDifficultyAnalysis
{
    public int LevelNumber { get; set; }
    public float CurrentDifficulty { get; set; }
    public float SuccessRate { get; set; }
    public float AverageAttempts { get; set; }
    public float AverageTimeSpent { get; set; }
    public float HintUsageRate { get; set; }
    public TrendDirection TrendDirection { get; set; }
}

/// <summary>
/// Balance suggestion
/// </summary>
public class BalanceSuggestion
{
    public SuggestionType Type { get; set; }
    public SuggestionPriority Priority { get; set; }
    public string Description { get; set; }
    public List<string> SpecificChanges { get; set; } = new List<string>();
}

/// <summary>
/// Overall difficulty analysis
/// </summary>
public class OverallDifficultyAnalysis
{
    public int TotalLevels { get; set; }
    public float AverageDifficulty { get; set; }
    public float DifficultyVariance { get; set; }
    public int SpikeCount { get; set; }
    public float SmoothnessScore { get; set; }
    public float BalanceScore { get; set; }
    public int EasyLevels { get; set; }
    public int MediumLevels { get; set; }
    public int HardLevels { get; set; }
    public List<OverallRecommendation> OverallRecommendations { get; set; } = new List<OverallRecommendation>();
}

/// <summary>
/// Overall recommendation
/// </summary>
public class OverallRecommendation
{
    public OverallRecommendationType Type { get; set; }
    public RecommendationPriority Priority { get; set; }
    public string Description { get; set; }
    public string Impact { get; set; }
    public List<string> SuggestedActions { get; set; } = new List<string>();
}

/// <summary>
/// Level balance recommendations
/// </summary>
public class LevelBalanceRecommendations
{
    public int LevelNumber { get; set; }
    public float CurrentDifficulty { get; set; }
    public List<BalanceSuggestion> Recommendations { get; set; } = new List<BalanceSuggestion>();
    public LevelDifficultyData AnalysisData { get; set; }
}

/// <summary>
/// Difficulty metrics for historical tracking
/// </summary>
public class DifficultyMetrics
{
    public int LevelNumber { get; set; }
    public float DifficultyScore { get; set; }
    public DateTime LastUpdated { get; set; }
}

/// <summary>
/// Difficulty analysis data
/// </summary>
public class DifficultyAnalysisData
{
    public List<LevelDifficultyData> LevelData { get; set; } = new List<LevelDifficultyData>();
    public List<DifficultyMetrics> DifficultyHistory { get; set; } = new List<DifficultyMetrics>();
    public DateTime LastUpdated { get; set; }
}

/// <summary>
/// Complete difficulty analysis report
/// </summary>
public class DifficultyAnalysisReport
{
    public DateTime GeneratedAt { get; set; }
    public int TotalLevels { get; set; }
    public float AverageDifficulty { get; set; }
    public List<string> DifficultySpikes { get; set; } = new List<string>();
    public List<LevelDifficultyData> LevelData { get; set; } = new List<LevelDifficultyData>();
    public OverallDifficultyAnalysis OverallAnalysis { get; set; }
}

/// <summary>
/// Enums
/// </summary>
public enum TrendDirection
{
    Increasing,
    Decreasing,
    Stable
}

public enum SuggestionType
{
    ReduceDifficulty,
    IncreaseDifficulty,
    ImproveClarity,
    MonitorDifficulty
}

public enum SuggestionPriority
{
    Low,
    Medium,
    High
}

public enum OverallRecommendationType
{
    ImproveCurveSmoothness,
    ImproveOverallBalance,
    AdjustLevelDistribution
}

public enum RecommendationPriority
{
    Low,
    Medium,
    High
}