using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

/// <summary>
/// Centralized version management system
/// Stores app version, build number, and release notes consistently across platforms
/// </summary>
public class VersionInfo : Node
{
    public static VersionInfo Instance { get; private set; }

    // Version information
    private AppVersion _currentVersion;
    private string _versionFilePath = "res://version.json";
    
    [Signal]
    public delegate void VersionChangedEventHandler(AppVersion newVersion);
    
    [Signal]
    public delegate void BuildNumberIncrementedEventHandler(int newBuildNumber);

    public override void _Ready()
    {
        if (Instance != null)
        {
            QueueFree();
            return;
        }
        
        Instance = this;
        LoadVersionInfo();
        
        // Update project.godot version if needed
        UpdateProjectVersion();
        
        GD.Print($"Version system initialized: v{_currentVersion.Version} (build {_currentVersion.BuildNumber})");
    }

    /// <summary>
    /// Load version information from file
    /// </summary>
    private void LoadVersionInfo()
    {
        if (!File.Exists(_versionFilePath))
        {
            CreateDefaultVersion();
            SaveVersionInfo();
        }
        
        try
        {
            string jsonContent = File.ReadAllText(_versionFilePath);
            _currentVersion = JsonSerializer.Deserialize<AppVersion>(jsonContent) ?? CreateDefaultVersion();
        }
        catch (Exception e)
        {
            GD.PrintErr($"Failed to load version info: {e.Message}");
            _currentVersion = CreateDefaultVersion();
        }
    }

    /// <summary>
    /// Create default version information
    /// </summary>
    private AppVersion CreateDefaultVersion()
    {
        return new AppVersion
        {
            Version = "1.0.0",
            BuildNumber = 1,
            ReleaseName = "Initial Release",
            ReleaseNotes = new List<string>
            {
                "Initial launch of Angry Animals",
                "Physics-based puzzle gameplay",
                "Character customization with face photo integration",
                "100 hand-crafted levels",
                "Infinite procedural levels",
                "Multiple slingshot variants",
                "Speech bubble expressions",
                "Haptic feedback support",
                "Cross-platform support (iOS, Android, Desktop)"
            },
            PlatformVersions = new Dictionary<string, string>
            {
                { "android", "1.0.0" },
                { "ios", "1.0.0" },
                { "windows", "1.0.0" },
                { "macos", "1.0.0" },
                { "linux", "1.0.0" }
            },
            ReleaseDate = DateTime.Now,
            IsBeta = false,
            IsProduction = true
        };
    }

    /// <summary>
    /// Save version information to file
    /// </summary>
    public void SaveVersionInfo()
    {
        try
        {
            var options = new JsonSerializerOptions 
            { 
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            
            string json = JsonSerializer.Serialize(_currentVersion, options);
            File.WriteAllText(_versionFilePath, json);
            
            GD.Print($"Version info saved: {_versionFilePath}");
        }
        catch (Exception e)
        {
            GD.PrintErr($"Failed to save version info: {e.Message}");
        }
    }

    /// <summary>
    /// Update project.godot version to match current version
    /// </summary>
    private void UpdateProjectVersion()
    {
        string projectPath = "res://project.godot";
        
        try
        {
            if (File.Exists(projectPath))
            {
                string[] lines = File.ReadAllLines(projectPath);
                bool found = false;
                
                for (int i = 0; i < lines.Length; i++)
                {
                    if (lines[i].StartsWith("config/version="))
                    {
                        lines[i] = $"config/version=\"{_currentVersion.Version}\"";
                        found = true;
                        break;
                    }
                }
                
                if (found)
                {
                    File.WriteAllLines(projectPath, lines);
                    GD.Print("Updated project.godot version");
                }
            }
        }
        catch (Exception e)
        {
            GD.PrintErr($"Failed to update project.godot version: {e.Message}");
        }
    }

    /// <summary>
    /// Get current version information
    /// </summary>
    public AppVersion GetCurrentVersion()
    {
        return _currentVersion;
    }

    /// <summary>
    /// Get version as string (e.g., "1.0.0 (123)")
    /// </summary>
    public string GetVersionString()
    {
        return $"{_currentVersion.Version} ({_currentVersion.BuildNumber})";
    }

    /// <summary>
    /// Get version for specific platform
    /// </summary>
    public string GetPlatformVersion(string platform)
    {
        if (_currentVersion.PlatformVersions.TryGetValue(platform.ToLower(), out string version))
        {
            return version;
        }
        
        return _currentVersion.Version;
    }

    /// <summary>
    /// Increment build number
    /// </summary>
    public void IncrementBuildNumber()
    {
        _currentVersion.BuildNumber++;
        _currentVersion.ReleaseDate = DateTime.Now;
        SaveVersionInfo();
        UpdateProjectVersion();
        
        EmitSignal("BuildNumberIncremented", _currentVersion.BuildNumber);
        EmitSignal("VersionChanged", _currentVersion);
        
        GD.Print($"Build number incremented to {_currentVersion.BuildNumber}");
    }

    /// <summary>
    /// Set new version (major.minor.patch)
    /// </summary>
    public void SetVersion(string version)
    {
        if (IsValidVersion(version))
        {
            _currentVersion.Version = version;
            _currentVersion.BuildNumber = 1;
            _currentVersion.ReleaseDate = DateTime.Now;
            
            // Update platform versions
            foreach (var platform in _currentVersion.PlatformVersions.Keys.ToList())
            {
                _currentVersion.PlatformVersions[platform] = version;
            }
            
            SaveVersionInfo();
            UpdateProjectVersion();
            
            EmitSignal("VersionChanged", _currentVersion);
            
            GD.Print($"Version updated to {version}");
        }
        else
        {
            GD.PrintErr($"Invalid version format: {version}. Use semantic versioning (e.g., 1.0.0)");
        }
    }

    /// <summary>
    /// Set release name
    /// </summary>
    public void SetReleaseName(string releaseName)
    {
        _currentVersion.ReleaseName = releaseName;
        _currentVersion.ReleaseDate = DateTime.Now;
        SaveVersionInfo();
    }

    /// <summary>
    /// Add release note
    /// </summary>
    public void AddReleaseNote(string note)
    {
        _currentVersion.ReleaseNotes.Insert(0, note);
        _currentVersion.ReleaseDate = DateTime.Now;
        SaveVersionInfo();
    }

    /// <summary>
    /// Set release notes (replaces all existing notes)
    /// </summary>
    public void SetReleaseNotes(List<string> notes)
    {
        _currentVersion.ReleaseNotes = notes;
        _currentVersion.ReleaseDate = DateTime.Now;
        SaveVersionInfo();
    }

    /// <summary>
    /// Mark as beta release
    /// </summary>
    public void MarkAsBeta(bool isBeta = true)
    {
        _currentVersion.IsBeta = isBeta;
        _currentVersion.IsProduction = !isBeta;
        SaveVersionInfo();
    }

    /// <summary>
    /// Mark as production release
    /// </summary>
    public void MarkAsProduction(bool isProduction = true)
    {
        _currentVersion.IsProduction = isProduction;
        _currentVersion.IsBeta = !isProduction;
        SaveVersionInfo();
    }

    /// <summary>
    /// Update platform-specific version
    /// </summary>
    public void SetPlatformVersion(string platform, string version)
    {
        if (IsValidVersion(version))
        {
            _currentVersion.PlatformVersions[platform.ToLower()] = version;
            SaveVersionInfo();
            
            GD.Print($"Platform version updated for {platform}: {version}");
        }
        else
        {
            GD.PrintErr($"Invalid version format for {platform}: {version}");
        }
    }

    /// <summary>
    /// Check if version string is valid (semantic versioning)
    /// </summary>
    private bool IsValidVersion(string version)
    {
        var parts = version.Split('.');
        if (parts.Length != 3) return false;
        
        foreach (string part in parts)
        {
            if (!int.TryParse(part, out _)) return false;
        }
        
        return true;
    }

    /// <summary>
    /// Compare versions (returns -1 if current < other, 0 if equal, 1 if current > other)
    /// </summary>
    public int CompareVersion(string otherVersion)
    {
        if (!IsValidVersion(otherVersion) || !IsValidVersion(_currentVersion.Version))
        {
            return 0;
        }
        
        var current = _currentVersion.Version.Split('.').Select(int.Parse).ToArray();
        var other = otherVersion.Split('.').Select(int.Parse).ToArray();
        
        for (int i = 0; i < 3; i++)
        {
            if (current[i] < other[i]) return -1;
            if (current[i] > other[i]) return 1;
        }
        
        return 0;
    }

    /// <summary>
    /// Check if current version is newer than specified version
    /// </summary>
    public bool IsNewerThan(string version)
    {
        return CompareVersion(version) > 0;
    }

    /// <summary>
    /// Check if current version is older than specified version
    /// </summary>
    public bool IsOlderThan(string version)
    {
        return CompareVersion(version) < 0;
    }

    /// <summary>
    /// Generate release notes from current build
    /// </summary>
    public string GenerateReleaseNotes()
    {
        var notes = new System.Text.StringBuilder();
        notes.AppendLine($"Version {_currentVersion.Version} (Build {_currentVersion.BuildNumber})");
        notes.AppendLine($"Release Date: {_currentVersion.ReleaseDate:yyyy-MM-dd}");
        
        if (!string.IsNullOrEmpty(_currentVersion.ReleaseName))
        {
            notes.AppendLine($"Release Name: {_currentVersion.ReleaseName}");
        }
        
        if (_currentVersion.IsBeta)
        {
            notes.AppendLine("Status: Beta Release");
        }
        else if (_currentVersion.IsProduction)
        {
            notes.AppendLine("Status: Production Release");
        }
        
        notes.AppendLine();
        notes.AppendLine("Changes:");
        
        foreach (string note in _currentVersion.ReleaseNotes)
        {
            notes.AppendLine($"• {note}");
        }
        
        return notes.ToString();
    }

    /// <summary>
    /// Export version information for store listings
    /// </summary>
    public StoreVersionInfo ExportForStore(string platform)
    {
        return new StoreVersionInfo
        {
            Version = GetPlatformVersion(platform),
            BuildNumber = _currentVersion.BuildNumber,
            ReleaseDate = _currentVersion.ReleaseDate,
            ReleaseNotes = _currentVersion.ReleaseNotes,
            IsBeta = _currentVersion.IsBeta,
            Platform = platform.ToLower()
        };
    }

    /// <summary>
    /// Get version display text for UI
    /// </summary>
    public string GetDisplayVersion()
    {
        var parts = new List<string>();
        parts.Add($"v{_currentVersion.Version}");
        
        if (_currentVersion.BuildNumber > 1)
        {
            parts.Add($"b{_currentVersion.BuildNumber}");
        }
        
        if (_currentVersion.IsBeta)
        {
            parts.Add("Beta");
        }
        
        return string.Join(" ", parts);
    }

    /// <summary>
    /// Create version increment script for automation
    /// </summary>
    public void CreateVersionIncrementScript()
    {
        string scriptPath = "scripts/increment_version.sh";
        string scriptDir = Path.GetDirectoryName(scriptPath);
        
        if (!Directory.Exists(scriptDir))
        {
            Directory.CreateDirectory(scriptDir);
        }
        
        try
        {
            using (var writer = new StreamWriter(scriptPath))
            {
                writer.WriteLine("#!/bin/bash");
                writer.WriteLine("# Version Increment Script");
                writer.WriteLine("# Usage: ./scripts/increment_version.sh [major|minor|patch]");
                writer.WriteLine();
                writer.WriteLine("if [ $# -eq 0 ]; then");
                writer.WriteLine("    echo \"Usage: $0 [major|minor|patch]\"");
                writer.WriteLine("    exit 1");
                writer.WriteLine("fi");
                writer.WriteLine();
                writer.WriteLine("# This would integrate with Godot's version management");
                writer.WriteLine("# In a real implementation, this would call the VersionInfo API");
                writer.WriteLine("echo \"Incrementing version: $1\"");
                writer.WriteLine("# godot --script increment_version.gd $1");
            }
            
            GD.Print($"Version increment script created: {scriptPath}");
        }
        catch (Exception e)
        {
            GD.PrintErr($"Failed to create version increment script: {e.Message}");
        }
    }
}

/// <summary>
/// Application version data structure
/// </summary>
public class AppVersion
{
    public string Version { get; set; } = "1.0.0";
    public int BuildNumber { get; set; } = 1;
    public string ReleaseName { get; set; } = "";
    public List<string> ReleaseNotes { get; set; } = new List<string>();
    public Dictionary<string, string> PlatformVersions { get; set; } = new Dictionary<string, string>();
    public DateTime ReleaseDate { get; set; } = DateTime.Now;
    public bool IsBeta { get; set; } = false;
    public bool IsProduction { get; set; } = true;
}

/// <summary>
/// Store-specific version information
/// </summary>
public class StoreVersionInfo
{
    public string Version { get; set; }
    public int BuildNumber { get; set; }
    public DateTime ReleaseDate { get; set; }
    public List<string> ReleaseNotes { get; set; } = new List<string>();
    public bool IsBeta { get; set; }
    public string Platform { get; set; }
}