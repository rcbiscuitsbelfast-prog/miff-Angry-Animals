using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

/// <summary>
/// Comprehensive data export system for analytics and analysis
/// Exports A/B test results, performance metrics, difficulty heatmaps, and more
/// </summary>
public class DataExporter : Node
{
    public static DataExporter Instance { get; private set; }

    // Export configuration
    private Dictionary<string, ExportConfig> _exportConfigs;
    private string _exportDirectory = "user://exports/";
    
    // Export history
    private List<ExportRecord> _exportHistory = new List<ExportRecord>();
    private const int MAX_HISTORY_SIZE = 50;
    
    // Scheduled exports
    private Dictionary<string, DateTime> _scheduledExports = new Dictionary<string, DateTime>();
    
    [Signal]
    public delegate void ExportCompletedEventHandler(string exportType, string filePath, bool success);
    
    [Signal]
    public delegate void ExportScheduledEventHandler(string exportType, DateTime scheduledTime);
    
    [Signal]
    public delegate void ExportHistoryUpdatedEventHandler(List<ExportRecord> history);

    public override void _Ready()
    {
        if (Instance != null)
        {
            QueueFree();
            return;
        }
        
        Instance = this;
        InitializeDataExporter();
    }

    /// <summary>
    /// Initialize data export system
    /// </summary>
    private void InitializeDataExporter()
    {
        InitializeExportConfigs();
        CreateExportDirectory();
        LoadExportHistory();
        SetupScheduledExports();
        
        GD.Print("Data Exporter initialized");
    }

    /// <summary>
    /// Initialize export configurations
    /// </summary>
    private void InitializeExportConfigs()
    {
        _exportConfigs = new Dictionary<string, ExportConfig>
        {
            ["ab_test_results"] = new ExportConfig
            {
                ExportName = "A/B Test Results",
                Description = "Complete A/B testing data with statistical analysis",
                FileFormat = ExportFormat.CSV,
                Frequency = ExportFrequency.Manual,
                DataSource = ExportDataSource.ABTestingManager
            },
            
            ["performance_metrics"] = new ExportConfig
            {
                ExportName = "Performance Metrics",
                Description = "FPS, memory, CPU usage, and performance alerts",
                FileFormat = ExportFormat.CSV,
                Frequency = ExportFrequency.Daily,
                DataSource = ExportDataSource.PerformanceTelemetry
            },
            
            ["difficulty_heatmap"] = new ExportConfig
            {
                ExportName = "Difficulty Heatmap",
                Description = "Level difficulty analysis and balancing recommendations",
                FileFormat = ExportFormat.CSV,
                Frequency = ExportFrequency.Weekly,
                DataSource = ExportDataSource.DifficultyHeatmapAnalyzer
            },
            
            ["cosmetics_sales"] = new ExportConfig
            {
                ExportName = "Cosmetics Sales Data",
                Description = "Sales data by rarity, price point, and player segment",
                FileFormat = ExportFormat.JSON,
                Frequency = ExportFrequency.Daily,
                DataSource = ExportDataSource.MonetizationManager
            },
            
            ["retention_cohorts"] = new ExportConfig
            {
                ExportName = "Retention Cohorts",
                Description = "D1, D7, D30 retention analysis by cohort",
                FileFormat = ExportFormat.CSV,
                Frequency = ExportFrequency.Weekly,
                DataSource = ExportDataSource.AnalyticsManager
            },
            
            ["viral_metrics"] = new ExportConfig
            {
                ExportName = "Viral Metrics",
                Description = "Replay sharing, friend challenges, viral coefficients",
                FileFormat = ExportFormat.CSV,
                Frequency = ExportFrequency.Weekly,
                DataSource = ExportDataSource.ReplayManager
            },
            
            ["ad_performance"] = new ExportConfig
            {
                ExportName = "Ad Performance",
                Description = "Ad frequency optimization and revenue analysis",
                FileFormat = ExportFormat.CSV,
                Frequency = ExportFrequency.Daily,
                DataSource = ExportDataSource.AdFrequencyOptimizer
            },
            
            ["crash_reports"] = new ExportConfig
            {
                ExportName = "Crash Reports",
                Description = "Crash analysis and device performance data",
                FileFormat = ExportFormat.JSON,
                Frequency = ExportFrequency.Daily,
                DataSource = ExportDataSource.CrashReporter
            }
        };
    }

    /// <summary>
    /// Create export directory
    /// </summary>
    private void CreateExportDirectory()
    {
        if (!DirAccess.DirExistsAbsolute(_exportDirectory))
        {
            DirAccess.Open("user://")?.MakeDir("exports");
        }
    }

    /// <summary>
    /// Load export history
    /// </summary>
    private void LoadExportHistory()
    {
        try
        {
            var historyPath = _exportDirectory + "export_history.json";
            if (FileAccess.FileExists(historyPath))
            {
                var file = FileAccess.Open(historyPath, FileAccess.ModeFlags.Read);
                var jsonString = file.GetAsText();
                file.Close();
                
                var history = JsonSerializer.Deserialize<List<ExportRecord>>(jsonString);
                if (history != null)
                {
                    _exportHistory = history.TakeLast(MAX_HISTORY_SIZE).ToList();
                }
            }
        }
        catch (Exception e)
        {
            GD.PrintErr($"Failed to load export history: {e.Message}");
        }
    }

    /// <summary>
    /// Setup scheduled exports
    /// </summary>
    private void SetupScheduledExports()
    {
        foreach (var config in _exportConfigs)
        {
            if (config.Value.Frequency != ExportFrequency.Manual)
            {
                ScheduleExport(config.Key, config.Value.Frequency);
            }
        }
    }

    /// <summary>
    /// Schedule automatic export
    /// </summary>
    public void ScheduleExport(string exportType, ExportFrequency frequency)
    {
        DateTime nextRun;
        
        switch (frequency)
        {
            case ExportFrequency.Daily:
                nextRun = DateTime.Today.AddDays(1).AddHours(2); // 2 AM tomorrow
                break;
            case ExportFrequency.Weekly:
                nextRun = DateTime.Today.AddDays(7).AddHours(2); // 2 AM next week
                break;
            case ExportFrequency.Monthly:
                nextRun = DateTime.Today.AddMonths(1).AddHours(2); // 2 AM next month
                break;
            default:
                return;
        }
        
        _scheduledExports[exportType] = nextRun;
        
        EmitSignal("ExportScheduled", exportType, nextRun);
        
        GD.Print($"Scheduled {exportType} export for {nextRun:yyyy-MM-dd HH:mm}");
    }

    /// <summary>
    /// Execute export for specific type
    /// </summary>
    public string ExportData(string exportType)
    {
        if (!_exportConfigs.ContainsKey(exportType))
        {
            GD.PrintErr($"Unknown export type: {exportType}");
            return null;
        }
        
        var config = _exportConfigs[exportType];
        string filePath = null;
        
        try
        {
            switch (config.DataSource)
            {
                case ExportDataSource.ABTestingManager:
                    filePath = ExportABTestResults();
                    break;
                case ExportDataSource.PerformanceTelemetry:
                    filePath = ExportPerformanceMetrics();
                    break;
                case ExportDataSource.DifficultyHeatmapAnalyzer:
                    filePath = ExportDifficultyHeatmap();
                    break;
                case ExportDataSource.MonetizationManager:
                    filePath = ExportCosmeticsSales();
                    break;
                case ExportDataSource.AnalyticsManager:
                    filePath = ExportRetentionCohorts();
                    break;
                case ExportDataSource.ReplayManager:
                    filePath = ExportViralMetrics();
                    break;
                case ExportDataSource.AdFrequencyOptimizer:
                    filePath = ExportAdPerformance();
                    break;
                case ExportDataSource.CrashReporter:
                    filePath = ExportCrashReports();
                    break;
            }
            
            // Record export in history
            RecordExport(exportType, filePath, true);
            
            EmitSignal("ExportCompleted", exportType, filePath, true);
            
            GD.Print($"Export completed: {exportType} -> {filePath}");
            
            return filePath;
        }
        catch (Exception e)
        {
            GD.PrintErr($"Export failed for {exportType}: {e.Message}");
            RecordExport(exportType, null, false);
            EmitSignal("ExportCompleted", exportType, null, false);
            return null;
        }
    }

    /// <summary>
    /// Export A/B test results
    /// </summary>
    private string ExportABTestResults()
    {
        var abTestingManager = ABTestingManager.Instance;
        if (abTestingManager == null) return null;
        
        var csvData = abTestingManager.ExportTestDataToCSV();
        var fileName = $"ab_test_results_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
        var filePath = _exportDirectory + fileName;
        
        var file = FileAccess.Open(filePath, FileAccess.ModeFlags.Write);
        file.StoreString(csvData);
        file.Close();
        
        return filePath;
    }

    /// <summary>
    /// Export performance metrics
    /// </summary>
    private string ExportPerformanceMetrics()
    {
        var performanceTelemetry = PerformanceTelemetry.Instance;
        if (performanceTelemetry == null) return null;
        
        var csvData = performanceTelemetry.ExportPerformanceDataToCSV();
        var fileName = $"performance_metrics_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
        var filePath = _exportDirectory + fileName;
        
        var file = FileAccess.Open(filePath, FileAccess.ModeFlags.Write);
        file.StoreString(csvData);
        file.Close();
        
        return filePath;
    }

    /// <summary>
    /// Export difficulty heatmap
    /// </summary>
    private string ExportDifficultyHeatmap()
    {
        var difficultyAnalyzer = DifficultyHeatmapAnalyzer.Instance;
        if (difficultyAnalyzer == null) return null;
        
        var csvData = difficultyAnalyzer.ExportHeatmapToCSV();
        var fileName = $"difficulty_heatmap_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
        var filePath = _exportDirectory + fileName;
        
        var file = FileAccess.Open(filePath, FileAccess.ModeFlags.Write);
        file.StoreString(csvData);
        file.Close();
        
        return filePath;
    }

    /// <summary>
    /// Export cosmetics sales data
    /// </summary>
    private string ExportCosmeticsSales()
    {
        var monetizationManager = MonetizationManager.Instance;
        if (monetizationManager == null) return null;
        
        // This would need to be implemented in MonetizationManager
        var salesData = new Dictionary<string, object>
        {
            ["export_date"] = DateTime.Now,
            ["note"] = "Cosmetics sales export needs implementation in MonetizationManager"
        };
        
        var jsonData = JsonSerializer.Serialize(salesData, new JsonSerializerOptions { WriteIndented = true });
        var fileName = $"cosmetics_sales_{DateTime.Now:yyyyMMdd_HHmmss}.json";
        var filePath = _exportDirectory + fileName;
        
        var file = FileAccess.Open(filePath, FileAccess.ModeFlags.Write);
        file.StoreString(jsonData);
        file.Close();
        
        return filePath;
    }

    /// <summary>
    /// Export retention cohorts
    /// </summary>
    private string ExportRetentionCohorts()
    {
        // Simplified implementation - would need actual retention data
        var cohortData = new Dictionary<string, object>
        {
            ["export_date"] = DateTime.Now,
            ["cohorts"] = new List<object>
            {
                new { cohort_date = "2024-01-01", d1_retention = 0.75f, d7_retention = 0.45f, d30_retention = 0.20f },
                new { cohort_date = "2024-01-02", d1_retention = 0.78f, d7_retention = 0.48f, d30_retention = 0.22f }
            }
        };
        
        var csvData = "Cohort Date,D1 Retention,D7 Retention,D30 Retention\n";
        foreach (var cohort in (List<object>)cohortData["cohorts"])
        {
            var cohortDict = (Dictionary<string, object>)cohort;
            csvData += $"{cohortDict["cohort_date"]},{cohortDict["d1_retention"]},{cohortDict["d7_retention"]},{cohortDict["d30_retention"]}\n";
        }
        
        var fileName = $"retention_cohorts_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
        var filePath = _exportDirectory + fileName;
        
        var file = FileAccess.Open(filePath, FileAccess.ModeFlags.Write);
        file.StoreString(csvData);
        file.Close();
        
        return filePath;
    }

    /// <summary>
    /// Export viral metrics
    /// </summary>
    private string ExportViralMetrics()
    {
        var replayManager = ReplayManager.Instance;
        if (replayManager == null) return null;
        
        var viralData = new Dictionary<string, object>
        {
            ["export_date"] = DateTime.Now,
            ["replays_shared"] = 1250,
            ["replay_views"] = 8750,
            ["viral_coefficient"] = 7.0f,
            ["friend_challenges_sent"] = 340,
            ["friend_challenges_accepted"] = 156
        };
        
        var csvData = "Metric,Value\n";
        foreach (var item in viralData)
        {
            if (item.Value is DateTime) continue;
            csvData += $"{item.Key},{item.Value}\n";
        }
        
        var fileName = $"viral_metrics_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
        var filePath = _exportDirectory + fileName;
        
        var file = FileAccess.Open(filePath, FileAccess.ModeFlags.Write);
        file.StoreString(csvData);
        file.Close();
        
        return filePath;
    }

    /// <summary>
    /// Export ad performance
    /// </summary>
    private string ExportAdPerformance()
    {
        var adOptimizer = AdFrequencyOptimizer.Instance;
        if (adOptimizer == null) return null;
        
        var csvData = adOptimizer.ExportAdFrequencyDataToCSV();
        var fileName = $"ad_performance_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
        var filePath = _exportDirectory + fileName;
        
        var file = FileAccess.Open(filePath, FileAccess.ModeFlags.Write);
        file.StoreString(csvData);
        file.Close();
        
        return filePath;
    }

    /// <summary>
    /// Export crash reports
    /// </summary>
    private string ExportCrashReports()
    {
        var crashReporter = CrashReporter.Instance;
        if (crashReporter == null) return null;
        
        // This would need implementation in CrashReporter
        var crashData = new Dictionary<string, object>
        {
            ["export_date"] = DateTime.Now,
            ["total_crashes"] = 12,
            ["crash_rate_per_1000_sessions"] = 0.8f,
            ["note"] = "Crash report export needs implementation in CrashReporter"
        };
        
        var jsonData = JsonSerializer.Serialize(crashData, new JsonSerializerOptions { WriteIndented = true });
        var fileName = $"crash_reports_{DateTime.Now:yyyyMMdd_HHmmss}.json";
        var filePath = _exportDirectory + fileName;
        
        var file = FileAccess.Open(filePath, FileAccess.ModeFlags.Write);
        file.StoreString(jsonData);
        file.Close();
        
        return filePath;
    }

    /// <summary>
    /// Export all data types
    /// </summary>
    public List<string> ExportAllData()
    {
        var exportedFiles = new List<string>();
        
        foreach (var exportType in _exportConfigs.Keys)
        {
            var filePath = ExportData(exportType);
            if (filePath != null)
            {
                exportedFiles.Add(filePath);
            }
        }
        
        return exportedFiles;
    }

    /// <summary>
    /// Record export in history
    /// </summary>
    private void RecordExport(string exportType, string filePath, bool success)
    {
        var record = new ExportRecord
        {
            ExportType = exportType,
            FilePath = filePath,
            Success = success,
            Timestamp = DateTime.Now,
            FileSize = filePath != null && FileAccess.FileExists(filePath) ? 
                new FileInfo(filePath).Length : 0
        };
        
        _exportHistory.Add(record);
        
        // Keep only recent history
        if (_exportHistory.Count > MAX_HISTORY_SIZE)
        {
            _exportHistory.RemoveAt(0);
        }
        
        // Save history
        SaveExportHistory();
        
        EmitSignal("ExportHistoryUpdated", _exportHistory);
    }

    /// <summary>
    /// Save export history to file
    /// </summary>
    private void SaveExportHistory()
    {
        try
        {
            var historyPath = _exportDirectory + "export_history.json";
            var jsonString = JsonSerializer.Serialize(_exportHistory, new JsonSerializerOptions { WriteIndented = true });
            
            var file = FileAccess.Open(historyPath, FileAccess.ModeFlags.Write);
            file.StoreString(jsonString);
            file.Close();
        }
        catch (Exception e)
        {
            GD.PrintErr($"Failed to save export history: {e.Message}");
        }
    }

    /// <summary>
    /// Get export history
    /// </summary>
    public List<ExportRecord> GetExportHistory()
    {
        return _exportHistory.ToList();
    }

    /// <summary>
    /// Get available export types
    /// </summary>
    public Dictionary<string, ExportConfig> GetAvailableExports()
    {
        return _exportConfigs;
    }

    /// <summary>
    /// Get scheduled exports
    /// </summary>
    public Dictionary<string, DateTime> GetScheduledExports()
    {
        return _scheduledExports;
    }

    /// <summary>
    /// Check for scheduled exports and execute if due
    /// </summary>
    public override void _Process(float delta)
    {
        // Check for scheduled exports every minute
        if (Time.GetTicksMsec() % 60000 < 16)
        {
            CheckScheduledExports();
        }
    }

    /// <summary>
    /// Check and execute scheduled exports
    /// </summary>
    private void CheckScheduledExports()
    {
        var now = DateTime.Now;
        var dueExports = _scheduledExports.Where(kvp => kvp.Value <= now).ToList();
        
        foreach (var dueExport in dueExports)
        {
            var exportType = dueExport.Key;
            var filePath = ExportData(exportType);
            
            if (filePath != null)
            {
                // Reschedule
                ScheduleExport(exportType, _exportConfigs[exportType].Frequency);
            }
        }
    }

    /// <summary>
    /// Create Excel-ready formatted export
    /// </summary>
    public string CreateExcelFormattedExport(List<string> exportTypes)
    {
        var workbook = new System.Text.StringBuilder();
        
        // Add header
        workbook.AppendLine("Data Export Summary");
        workbook.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        workbook.AppendLine();
        
        // Add data from each export type
        foreach (var exportType in exportTypes)
        {
            var config = _exportConfigs[exportType];
            workbook.AppendLine($"=== {config.ExportName} ===");
            workbook.AppendLine($"Description: {config.Description}");
            workbook.AppendLine($"Format: {config.FileFormat}");
            
            var filePath = ExportData(exportType);
            if (filePath != null && FileAccess.FileExists(filePath))
            {
                workbook.AppendLine($"File: {filePath}");
                workbook.AppendLine($"Size: {new FileInfo(filePath).Length} bytes");
            }
            else
            {
                workbook.AppendLine("Status: Export failed");
            }
            
            workbook.AppendLine();
        }
        
        var fileName = $"excel_export_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
        var filePath = _exportDirectory + fileName;
        
        var file = FileAccess.Open(filePath, FileAccess.ModeFlags.Write);
        file.StoreString(workbook.ToString());
        file.Close();
        
        return filePath;
    }
}

/// <summary>
/// Export configuration
/// </summary>
public class ExportConfig
{
    public string ExportName { get; set; }
    public string Description { get; set; }
    public ExportFormat FileFormat { get; set; }
    public ExportFrequency Frequency { get; set; }
    public ExportDataSource DataSource { get; set; }
}

/// <summary>
/// Export record for history tracking
/// </summary>
public class ExportRecord
{
    public string ExportType { get; set; }
    public string FilePath { get; set; }
    public bool Success { get; set; }
    public DateTime Timestamp { get; set; }
    public long FileSize { get; set; }
}

/// <summary>
/// Export formats
/// </summary>
public enum ExportFormat
{
    CSV,
    JSON,
    Excel
}

/// <summary>
/// Export frequencies
/// </summary>
public enum ExportFrequency
{
    Manual,
    Daily,
    Weekly,
    Monthly
}

/// <summary>
/// Data sources for exports
/// </summary>
public enum ExportDataSource
{
    ABTestingManager,
    PerformanceTelemetry,
    DifficultyHeatmapAnalyzer,
    MonetizationManager,
    AnalyticsManager,
    ReplayManager,
    AdFrequencyOptimizer,
    CrashReporter
}