using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

/// <summary>
/// Release notes generator that creates formatted release notes for app store submissions
/// Automatically generates notes from git history and manual inputs
/// </summary>
public class ReleaseNotesGenerator : Node
{
    public static ReleaseNotesGenerator Instance { get; private set; }

    // Release notes storage
    private List<ReleaseNote> _releaseNotes = new List<ReleaseNote>();
    private string _releaseNotesFilePath = "res://release_notes.json";
    
    // Git integration
    private GitIntegration _gitIntegration;
    
    // Template system
    private Dictionary<string, ReleaseNoteTemplate> _templates = new Dictionary<string, ReleaseNoteTemplate>();
    
    [Signal]
    public delegate void ReleaseNotesGeneratedEventHandler(string platform, string releaseNotes);
    
    [Signal]
    public delegate void GitCommitAnalyzedEventHandler(string commit, string category);

    public override void _Ready()
    {
        if (Instance != null)
        {
            QueueFree();
            return;
        }
        
        Instance = this;
        InitializeReleaseNotesGenerator();
    }

    /// <summary>
    /// Initialize release notes generator
    /// </summary>
    private void InitializeReleaseNotesGenerator()
    {
        LoadReleaseNotes();
        InitializeGitIntegration();
        CreateTemplates();
        
        GD.Print("Release notes generator initialized");
    }

    /// <summary>
    /// Initialize git integration
    /// </summary>
    private void InitializeGitIntegration()
    {
        _gitIntegration = new GitIntegration();
    }

    /// <summary>
    /// Load existing release notes
    /// </summary>
    private void LoadReleaseNotes()
    {
        try
        {
            if (File.Exists(_releaseNotesFilePath))
            {
                string jsonContent = File.ReadAllText(_releaseNotesFilePath);
                var releaseData = JsonSerializer.Deserialize<ReleaseNotesData>(jsonContent);
                
                if (releaseData?.ReleaseNotes != null)
                {
                    _releaseNotes = releaseData.ReleaseNotes;
                }
            }
        }
        catch (Exception e)
        {
            GD.PrintErr($"Failed to load release notes: {e.Message}");
        }
    }

    /// <summary>
    /// Save release notes to file
    /// </summary>
    private void SaveReleaseNotes()
    {
        try
        {
            var data = new ReleaseNotesData
            {
                ReleaseNotes = _releaseNotes,
                LastUpdated = DateTime.Now
            };
            
            var options = new JsonSerializerOptions 
            { 
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            
            string json = JsonSerializer.Serialize(data, options);
            File.WriteAllText(_releaseNotesFilePath, json);
        }
        catch (Exception e)
        {
            GD.PrintErr($"Failed to save release notes: {e.Message}");
        }
    }

    /// <summary>
    /// Create release note templates for different platforms
    /// </summary>
    private void CreateTemplates()
    {
        // Google Play Store template
        _templates["google_play"] = new ReleaseNoteTemplate
        {
            Platform = "Google Play Store",
            MaxLength = 500,
            Format = ReleaseNoteFormat.PlainText,
            Requirements = new List<string>
            {
                "What's New section",
                "Keep it under 500 characters",
                "Use plain text only",
                "Focus on user benefits",
                "Use past tense"
            }
        };

        // Apple App Store template
        _templates["app_store"] = new ReleaseNoteTemplate
        {
            Platform = "Apple App Store",
            MaxLength = 4000,
            Format = ReleaseNoteFormat.PlainText,
            Requirements = new List<string>
            {
                "Version description",
                "Can be up to 4000 characters",
                "Use plain text",
                "Include new features",
                "Bug fixes section"
            }
        };

        // Steam template
        _templates["steam"] = new ReleaseNoteTemplate
        {
            Platform = "Steam",
            MaxLength = 5000,
            Format = ReleaseNoteFormat.Markdown,
            Requirements = new List<string>
            {
                "Update notes",
                "Supports markdown formatting",
                "Can be quite detailed",
                "Include screenshots or video links",
                "Developer comments"
            }
        };

        // Generic template
        _templates["generic"] = new ReleaseNoteTemplate
        {
            Platform = "Generic",
            MaxLength = 1000,
            Format = ReleaseNoteFormat.PlainText,
            Requirements = new List<string>
            {
                "Clear and concise",
                "User-focused language",
                "Version number",
                "Key improvements",
                "Bug fixes"
            }
        };
    }

    /// <summary>
    /// Generate release notes for specific version
    /// </summary>
    public string GenerateReleaseNotes(string version, string platform, bool includeGitHistory = true)
    {
        var template = _templates.ContainsKey(platform.ToLower()) ? _templates[platform.ToLower()] : _templates["generic"];
        
        // Get release notes for this version
        var versionNotes = _releaseNotes.Where(note => note.Version == version).ToList();
        
        if (!versionNotes.Any())
        {
            GD.Print($"No release notes found for version {version}");
            return GenerateFallbackNotes(version, platform);
        }
        
        // Generate notes based on template
        return platform.ToLower() switch
        {
            "google_play" => GenerateGooglePlayNotes(versionNotes, template),
            "app_store" => GenerateAppStoreNotes(versionNotes, template),
            "steam" => GenerateSteamNotes(versionNotes, template),
            _ => GenerateGenericNotes(versionNotes, template)
        };
    }

    /// <summary>
    /// Generate Google Play Store release notes
    /// </summary>
    private string GenerateGooglePlayNotes(List<ReleaseNote> notes, ReleaseNoteTemplate template)
    {
        var sb = new System.Text.StringBuilder();
        
        // Get recent features and fixes
        var features = notes.Where(n => n.Category == ReleaseNoteCategory.Feature).ToList();
        var fixes = notes.Where(n => n.Category == ReleaseNoteCategory.BugFix).ToList();
        var improvements = notes.Where(n => n.Category == ReleaseNoteCategory.Improvement).ToList();
        
        sb.AppendLine("🎮 NEW FEATURES");
        foreach (var feature in features.Take(3)) // Limit to 3 main features
        {
            sb.AppendLine($"• {feature.Title}");
        }
        
        if (improvements.Any())
        {
            sb.AppendLine();
            sb.AppendLine("⚡ IMPROVEMENTS");
            foreach (var improvement in improvements.Take(2))
            {
                sb.AppendLine($"• {improvement.Title}");
            }
        }
        
        if (fixes.Any())
        {
            sb.AppendLine();
            sb.AppendLine("🔧 BUG FIXES");
            sb.AppendLine($"• Fixed {fixes.Count} issues for smoother gameplay");
        }
        
        string result = sb.ToString();
        
        // Truncate if necessary (Google Play has 500 char limit)
        if (result.Length > template.MaxLength)
        {
            result = result.Substring(0, template.MaxLength - 3) + "...";
        }
        
        EmitSignal("ReleaseNotesGenerated", "google_play", result);
        return result;
    }

    /// <summary>
    /// Generate Apple App Store release notes
    /// </summary>
    private string GenerateAppStoreNotes(List<ReleaseNote> notes, ReleaseNoteTemplate template)
    {
        var sb = new System.Text.StringBuilder();
        
        sb.AppendLine($"Version {notes.FirstOrDefault()?.Version}");
        sb.AppendLine();
        
        // Detailed features section
        var features = notes.Where(n => n.Category == ReleaseNoteCategory.Feature).ToList();
        if (features.Any())
        {
            sb.AppendLine("✨ NEW FEATURES");
            foreach (var feature in features)
            {
                sb.AppendLine($"• {feature.Title}");
                if (!string.IsNullOrEmpty(feature.Description))
                {
                    sb.AppendLine($"  {feature.Description}");
                }
            }
            sb.AppendLine();
        }
        
        // Improvements
        var improvements = notes.Where(n => n.Category == ReleaseNoteCategory.Improvement).ToList();
        if (improvements.Any())
        {
            sb.AppendLine("🚀 IMPROVEMENTS");
            foreach (var improvement in improvements)
            {
                sb.AppendLine($"• {improvement.Title}");
            }
            sb.AppendLine();
        }
        
        // Bug fixes
        var fixes = notes.Where(n => n.Category == ReleaseNoteCategory.BugFix).ToList();
        if (fixes.Any())
        {
            sb.AppendLine("🐛 BUG FIXES");
            foreach (var fix in fixes)
            {
                sb.AppendLine($"• {fix.Title}");
            }
        }
        
        string result = sb.ToString();
        
        // Truncate if necessary (App Store allows 4000 chars)
        if (result.Length > template.MaxLength)
        {
            result = result.Substring(0, template.MaxLength - 3) + "...";
        }
        
        EmitSignal("ReleaseNotesGenerated", "app_store", result);
        return result;
    }

    /// <summary>
    /// Generate Steam release notes
    /// </summary>
    private string GenerateSteamNotes(List<ReleaseNote> notes, ReleaseNoteTemplate template)
    {
        var sb = new System.Text.StringBuilder();
        
        sb.AppendLine($"# Version {notes.FirstOrDefault()?.Version}");
        sb.AppendLine();
        
        // Add detailed changelog in markdown
        var features = notes.Where(n => n.Category == ReleaseNoteCategory.Feature).ToList();
        var fixes = notes.Where(n => n.Category == ReleaseNoteCategory.BugFix).ToList();
        var improvements = notes.Where(n => n.Category == ReleaseNoteCategory.Improvement).ToList();
        
        if (features.Any())
        {
            sb.AppendLine("## 🎮 New Features");
            foreach (var feature in features)
            {
                sb.AppendLine($"- **{feature.Title}**");
                if (!string.IsNullOrEmpty(feature.Description))
                {
                    sb.AppendLine($"  - {feature.Description}");
                }
            }
            sb.AppendLine();
        }
        
        if (improvements.Any())
        {
            sb.AppendLine("## ⚡ Improvements");
            foreach (var improvement in improvements)
            {
                sb.AppendLine($"- {improvement.Title}");
            }
            sb.AppendLine();
        }
        
        if (fixes.Any())
        {
            sb.AppendLine("## 🐛 Bug Fixes");
            foreach (var fix in fixes)
            {
                sb.AppendLine($"- {fix.Title}");
            }
        }
        
        // Add developer note section
        sb.AppendLine();
        sb.AppendLine("## 💬 Developer Notes");
        sb.AppendLine("Thanks for playing Angry Animals! Your feedback helps us improve the game.");
        sb.AppendLine("Join our community for updates and discussions!");
        
        string result = sb.ToString();
        
        // Truncate if necessary
        if (result.Length > template.MaxLength)
        {
            result = result.Substring(0, template.MaxLength - 3) + "...";
        }
        
        EmitSignal("ReleaseNotesGenerated", "steam", result);
        return result;
    }

    /// <summary>
    /// Generate generic release notes
    /// </summary>
    private string GenerateGenericNotes(List<ReleaseNote> notes, ReleaseNoteTemplate template)
    {
        var sb = new System.Text.StringBuilder();
        
        sb.AppendLine($"Version {notes.FirstOrDefault()?.Version}");
        sb.AppendLine();
        
        var features = notes.Where(n => n.Category == ReleaseNoteCategory.Feature).ToList();
        var fixes = notes.Where(n => n.Category == ReleaseNoteCategory.BugFix).ToList();
        var improvements = notes.Where(n => n.Category == ReleaseNoteCategory.Improvement).ToList();
        
        if (features.Any())
        {
            sb.AppendLine("New Features:");
            foreach (var feature in features)
            {
                sb.AppendLine($"- {feature.Title}");
            }
            sb.AppendLine();
        }
        
        if (improvements.Any())
        {
            sb.AppendLine("Improvements:");
            foreach (var improvement in improvements)
            {
                sb.AppendLine($"- {improvement.Title}");
            }
            sb.AppendLine();
        }
        
        if (fixes.Any())
        {
            sb.AppendLine("Bug Fixes:");
            foreach (var fix in fixes)
            {
                sb.AppendLine($"- {fix.Title}");
            }
        }
        
        string result = sb.ToString();
        
        if (result.Length > template.MaxLength)
        {
            result = result.Substring(0, template.MaxLength - 3) + "...";
        }
        
        EmitSignal("ReleaseNotesGenerated", "generic", result);
        return result;
    }

    /// <summary>
    /// Generate fallback release notes when no manual notes exist
    /// </summary>
    private string GenerateFallbackNotes(string version, string platform)
    {
        var template = _templates.ContainsKey(platform.ToLower()) ? _templates[platform.ToLower()] : _templates["generic"];
        
        var sb = new System.Text.StringBuilder();
        
        sb.AppendLine($"Version {version}");
        sb.AppendLine();
        sb.AppendLine("New features and improvements based on your feedback!");
        sb.AppendLine("Enhanced gameplay experience");
        sb.AppendLine("Bug fixes and performance optimizations");
        
        string result = sb.ToString();
        
        if (result.Length > template.MaxLength)
        {
            result = result.Substring(0, template.MaxLength - 3) + "...";
        }
        
        return result;
    }

    /// <summary>
    /// Add manual release note
    /// </summary>
    public void AddReleaseNote(string version, string title, string description, ReleaseNoteCategory category)
    {
        var note = new ReleaseNote
        {
            Id = Guid.NewGuid().ToString(),
            Version = version,
            Title = title,
            Description = description,
            Category = category,
            CreatedAt = DateTime.Now
        };
        
        _releaseNotes.Add(note);
        SaveReleaseNotes();
        
        GD.Print($"Added release note: {title} for version {version}");
    }

    /// <summary>
    /// Analyze git commits and suggest release notes
    /// </summary>
    public List<GitCommitSuggestion> AnalyzeGitCommits(string sinceCommit = null)
    {
        var suggestions = new List<GitCommitSuggestion>();
        
        try
        {
            // This would integrate with actual git commands
            // For demonstration, we'll simulate git analysis
            var simulatedCommits = new[]
            {
                new { Message = "Add new slingshot variant", Category = "feature" },
                new { Message = "Fix collision detection bug", Category = "bugfix" },
                new { Message = "Improve particle effects", Category = "improvement" },
                new { Message = "Update UI layout", Category = "feature" }
            };
            
            foreach (var commit in simulatedCommits)
            {
                var category = commit.Category switch
                {
                    "feature" => ReleaseNoteCategory.Feature,
                    "bugfix" => ReleaseNoteCategory.BugFix,
                    "improvement" => ReleaseNoteCategory.Improvement,
                    _ => ReleaseNoteCategory.Other
                };
                
                suggestions.Add(new GitCommitSuggestion
                {
                    CommitMessage = commit.Message,
                    SuggestedTitle = FormatCommitMessage(commit.Message),
                    SuggestedCategory = category,
                    Confidence = 0.8f
                });
                
                EmitSignal("GitCommitAnalyzed", commit.Message, commit.Category);
            }
        }
        catch (Exception e)
        {
            GD.PrintErr($"Failed to analyze git commits: {e.Message}");
        }
        
        return suggestions;
    }

    /// <summary>
    /// Format commit message for release notes
    /// </summary>
    private string FormatCommitMessage(string commitMessage)
    {
        // Remove common prefixes
        string formatted = commitMessage
            .Replace("Add ", "")
            .Replace("Fix ", "")
            .Replace("Update ", "")
            .Replace("Improve ", "")
            .Replace("Remove ", "");
        
        // Capitalize first letter
        if (!string.IsNullOrEmpty(formatted))
        {
            formatted = char.ToUpper(formatted[0]) + formatted.Substring(1);
        }
        
        return formatted;
    }

    /// <summary>
    /// Generate release notes from git commits
    /// </summary>
    public string GenerateFromGit(string version, string platform)
    {
        var suggestions = AnalyzeGitCommits();
        var template = _templates.ContainsKey(platform.ToLower()) ? _templates[platform.ToLower()] : _templates["generic"];
        
        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"Version {version}");
        sb.AppendLine();
        
        var features = suggestions.Where(s => s.SuggestedCategory == ReleaseNoteCategory.Feature).ToList();
        var fixes = suggestions.Where(s => s.SuggestedCategory == ReleaseNoteCategory.BugFix).ToList();
        var improvements = suggestions.Where(s => s.SuggestedCategory == ReleaseNoteCategory.Improvement).ToList();
        
        if (features.Any())
        {
            sb.AppendLine("New Features:");
            foreach (var feature in features.Take(3))
            {
                sb.AppendLine($"• {feature.SuggestedTitle}");
            }
            sb.AppendLine();
        }
        
        if (improvements.Any())
        {
            sb.AppendLine("Improvements:");
            foreach (var improvement in improvements.Take(2))
            {
                sb.AppendLine($"• {improvement.SuggestedTitle}");
            }
            sb.AppendLine();
        }
        
        if (fixes.Any())
        {
            sb.AppendLine("Bug Fixes:");
            sb.AppendLine($"• Fixed {fixes.Count} issues");
        }
        
        string result = sb.ToString();
        
        if (result.Length > template.MaxLength)
        {
            result = result.Substring(0, template.MaxLength - 3) + "...";
        }
        
        return result;
    }

    /// <summary>
    /// Export release notes for all platforms
    /// </summary>
    public void ExportForAllPlatforms(string version)
    {
        var platforms = new[] { "google_play", "app_store", "steam", "generic" };
        var exportDir = "res://release_notes";
        
        if (!Directory.Exists(exportDir))
        {
            Directory.CreateDirectory(exportDir);
        }
        
        foreach (var platform in platforms)
        {
            string releaseNotes = GenerateReleaseNotes(version, platform);
            string fileName = $"{version}_{platform}_release_notes.txt";
            string filePath = Path.Combine(exportDir, fileName);
            
            try
            {
                File.WriteAllText(filePath, releaseNotes);
                GD.Print($"Exported release notes for {platform}: {filePath}");
            }
            catch (Exception e)
            {
                GD.PrintErr($"Failed to export release notes for {platform}: {e.Message}");
            }
        }
    }

    /// <summary>
    /// Get release notes for specific version
    /// </summary>
    public List<ReleaseNote> GetReleaseNotes(string version)
    {
        return _releaseNotes.Where(note => note.Version == version).ToList();
    }

    /// <summary>
    /// Get all release notes
    /// </summary>
    public List<ReleaseNote> GetAllReleaseNotes()
    {
        return _releaseNotes;
    }

    /// <summary>
    /// Remove release note
    /// </summary>
    public void RemoveReleaseNote(string noteId)
    {
        var note = _releaseNotes.FirstOrDefault(n => n.Id == noteId);
        if (note != null)
        {
            _releaseNotes.Remove(note);
            SaveReleaseNotes();
            GD.Print($"Removed release note: {noteId}");
        }
    }

    /// <summary>
    /// Update release note
    /// </summary>
    public void UpdateReleaseNote(string noteId, string title, string description)
    {
        var note = _releaseNotes.FirstOrDefault(n => n.Id == noteId);
        if (note != null)
        {
            note.Title = title;
            note.Description = description;
            note.UpdatedAt = DateTime.Now;
            SaveReleaseNotes();
            GD.Print($"Updated release note: {noteId}");
        }
    }

    /// <summary>
    /// Get available templates
    /// </summary>
    public Dictionary<string, ReleaseNoteTemplate> GetTemplates()
    {
        return _templates;
    }

    /// <summary>
    /// Create automated release notes template
    /// </summary>
    public string CreateAutoReleaseNotes(string version, List<string> features, List<string> fixes, List<string> improvements)
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"Version {version}");
        sb.AppendLine();
        
        if (features.Any())
        {
            sb.AppendLine("New Features:");
            foreach (var feature in features)
            {
                sb.AppendLine($"• {feature}");
            }
            sb.AppendLine();
        }
        
        if (improvements.Any())
        {
            sb.AppendLine("Improvements:");
            foreach (var improvement in improvements)
            {
                sb.AppendLine($"• {improvement}");
            }
            sb.AppendLine();
        }
        
        if (fixes.Any())
        {
            sb.AppendLine("Bug Fixes:");
            foreach (var fix in fixes)
            {
                sb.AppendLine($"• {fix}");
            }
        }
        
        return sb.ToString();
    }
}

/// <summary>
/// Git integration helper
/// </summary>
public class GitIntegration
{
    public List<string> GetCommitsSince(string commitHash)
    {
        // This would execute actual git commands
        // For now, return simulated commits
        return new List<string>
        {
            "Add new slingshot variant",
            "Fix collision detection bug",
            "Improve particle effects",
            "Update UI layout"
        };
    }
}

/// <summary>
/// Git commit suggestion
/// </summary>
public class GitCommitSuggestion
{
    public string CommitMessage { get; set; }
    public string SuggestedTitle { get; set; }
    public ReleaseNoteCategory SuggestedCategory { get; set; }
    public float Confidence { get; set; }
}

/// <summary>
/// Release note data structure
/// </summary>
public class ReleaseNote
{
    public string Id { get; set; }
    public string Version { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public ReleaseNoteCategory Category { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Release note template
/// </summary>
public class ReleaseNoteTemplate
{
    public string Platform { get; set; }
    public int MaxLength { get; set; }
    public ReleaseNoteFormat Format { get; set; }
    public List<string> Requirements { get; set; } = new List<string>();
}

/// <summary>
/// Release notes data storage
/// </summary>
public class ReleaseNotesData
{
    public List<ReleaseNote> ReleaseNotes { get; set; } = new List<ReleaseNote>();
    public DateTime LastUpdated { get; set; }
}

/// <summary>
/// Release note categories
/// </summary>
public enum ReleaseNoteCategory
{
    Feature,
    BugFix,
    Improvement,
    Security,
    Performance,
    Other
}

/// <summary>
/// Release note formats
/// </summary>
public enum ReleaseNoteFormat
{
    PlainText,
    Markdown,
    Html,
    Json
}