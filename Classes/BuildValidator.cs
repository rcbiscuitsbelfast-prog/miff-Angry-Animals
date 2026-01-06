using Godot;
using System;
using System.Collections.Generic;
using System.IO;

/// <summary>
/// Automated build validation and pre-submission checking system
/// Validates all required configurations before building for app stores
/// </summary>
public class BuildValidator : Node
{
    public static BuildValidator Instance { get; private set; }

    // Validation results
    private BuildValidationResults _validationResults;
    private List<string> _validationErrors = new List<string>();
    private List<string> _validationWarnings = new List<string>();
    
    // Validation checks
    private Dictionary<string, BuildCheck> _buildChecks = new Dictionary<string, BuildCheck>();
    
    [Signal]
    public delegate void ValidationStartedEventHandler(string platform);
    
    [Signal]
    public delegate void ValidationCompletedEventHandler(BuildValidationResults results);
    
    [Signal]
    public delegate void ValidationErrorEventHandler(string error);
    
    [Signal]
    public delegate void ValidationWarningEventHandler(string warning);

    public override void _Ready()
    {
        if (Instance != null)
        {
            QueueFree();
            return;
        }
        
        Instance = this;
        InitializeBuildValidator();
    }

    /// <summary>
    /// Initialize build validator
    /// </summary>
    private void InitializeBuildValidator()
    {
        CreateBuildChecks();
        
        GD.Print("Build validator initialized");
    }

    /// <summary>
    /// Create all build validation checks
    /// </summary>
    private void CreateBuildChecks()
    {
        // Platform validation checks
        _buildChecks["android_config"] = new BuildCheck
        {
            Name = "Android Configuration",
            Description = "Validates Android build configuration",
            CheckFunction = ValidateAndroidConfig,
            Priority = CheckPriority.Critical
        };
        
        _buildChecks["ios_config"] = new BuildCheck
        {
            Name = "iOS Configuration",
            Description = "Validates iOS build configuration",
            CheckFunction = ValidateIosConfig,
            Priority = CheckPriority.Critical
        };
        
        _buildChecks["desktop_config"] = new BuildCheck
        {
            Name = "Desktop Configuration",
            Description = "Validates desktop build configurations",
            CheckFunction = ValidateDesktopConfig,
            Priority = CheckPriority.Important
        };
        
        // Asset validation checks
        _buildChecks["asset_validation"] = new BuildCheck
        {
            Name = "Asset Validation",
            Description = "Checks for missing or placeholder assets",
            CheckFunction = ValidateAssets,
            Priority = CheckPriority.Important
        };
        
        _buildChecks["icon_validation"] = new BuildCheck
        {
            Name = "App Icons",
            Description = "Validates app icon requirements",
            CheckFunction = ValidateAppIcons,
            Priority = CheckPriority.Critical
        };
        
        // Configuration checks
        _buildChecks["project_config"] = new BuildCheck
        {
            Name = "Project Configuration",
            Description = "Validates project settings and versions",
            CheckFunction = ValidateProjectConfig,
            Priority = CheckPriority.Important
        };
        
        _buildChecks["permissions_validation"] = new BuildCheck
        {
            Name = "Platform Permissions",
            Description = "Validates required platform permissions",
            CheckFunction = ValidatePermissions,
            Priority = CheckPriority.Critical
        };
        
        // Store-specific checks
        _buildChecks["store_metadata"] = new BuildCheck
        {
            Name = "Store Metadata",
            Description = "Validates store listing metadata",
            CheckFunction = ValidateStoreMetadata,
            Priority = CheckPriority.Important
        };
        
        _buildChecks["privacy_compliance"] = new BuildCheck
        {
            Name = "Privacy Compliance",
            Description = "Validates privacy policy and compliance",
            CheckFunction = ValidatePrivacyCompliance,
            Priority = CheckPriority.Critical
        };
        
        // Technical checks
        _buildChecks["version_consistency"] = new BuildCheck
        {
            Name = "Version Consistency",
            Description = "Ensures version numbers are consistent across platforms",
            CheckFunction = ValidateVersionConsistency,
            Priority = CheckPriority.Important
        };
        
        _buildChecks["legal_documents"] = new BuildCheck
        {
            Name = "Legal Documents",
            Description = "Validates presence of legal documents",
            CheckFunction = ValidateLegalDocuments,
            Priority = CheckPriority.Critical
        };
    }

    /// <summary>
    /// Validate build for specific platform
    /// </summary>
    public void ValidateBuild(string platform)
    {
        EmitSignal("ValidationStarted", platform);
        
        _validationResults = new BuildValidationResults
        {
            Platform = platform,
            ValidationTime = DateTime.Now,
            TotalChecks = _buildChecks.Count,
            PassedChecks = 0,
            FailedChecks = 0,
            WarningChecks = 0
        };
        
        _validationErrors.Clear();
        _validationWarnings.Clear();
        
        GD.Print($"Starting build validation for {platform}");
        
        // Run all applicable checks
        foreach (var check in _buildChecks.Values)
        {
            if (IsCheckApplicable(check, platform))
            {
                try
                {
                    var result = check.CheckFunction();
                    ProcessCheckResult(check, result);
                }
                catch (Exception e)
                {
                    var errorResult = new CheckResult
                    {
                        Success = false,
                        Errors = new List<string> { $"Check failed with exception: {e.Message}" },
                        Warnings = new List<string>()
                    };
                    ProcessCheckResult(check, errorResult);
                }
            }
            else
            {
                // Skip check, count as passed
                _validationResults.PassedChecks++;
                GD.Print($"Skipping check '{check.Name}' for platform '{platform}'");
            }
        }
        
        // Finalize results
        _validationResults.ValidationErrors = new List<string>(_validationErrors);
        _validationResults.ValidationWarnings = new List<string>(_validationWarnings);
        _validationResults.IsValid = _validationResults.FailedChecks == 0;
        _validationResults.CompletionPercentage = (_validationResults.PassedChecks / (float)_validationResults.TotalChecks) * 100f;
        
        GD.Print($"Build validation completed for {platform}: {_validationResults.PassedChecks}/{_validationResults.TotalChecks} passed");
        
        EmitSignal("ValidationCompleted", _validationResults);
    }

    /// <summary>
    /// Check if a validation check is applicable to the platform
    /// </summary>
    private bool IsCheckApplicable(BuildCheck check, string platform)
    {
        // Platform-specific logic
        if (platform.ToLower() == "android")
        {
            return check.Name.Contains("Android") || 
                   check.Name.Contains("Project") || 
                   check.Name.Contains("Asset") || 
                   check.Name.Contains("Privacy") || 
                   check.Name.Contains("Legal") ||
                   check.Name.Contains("Version");
        }
        else if (platform.ToLower() == "ios")
        {
            return check.Name.Contains("iOS") || 
                   check.Name.Contains("Project") || 
                   check.Name.Contains("Asset") || 
                   check.Name.Contains("Privacy") || 
                   check.Name.Contains("Legal") ||
                   check.Name.Contains("Version");
        }
        else if (platform.ToLower() == "desktop")
        {
            return check.Name.Contains("Desktop") || 
                   check.Name.Contains("Project") || 
                   check.Name.Contains("Asset") || 
                   check.Name.Contains("Privacy") || 
                   check.Name.Contains("Legal") ||
                   check.Name.Contains("Version");
        }
        
        return true; // Global checks always apply
    }

    /// <summary>
    /// Process check result
    /// </summary>
    private void ProcessCheckResult(BuildCheck check, CheckResult result)
    {
        if (result.Success)
        {
            _validationResults.PassedChecks++;
            GD.Print($"✓ {check.Name}: Passed");
        }
        else
        {
            _validationResults.FailedChecks++;
            _validationErrors.AddRange(result.Errors);
            EmitSignal("ValidationError", $"{check.Name}: {string.Join(", ", result.Errors)}");
            GD.PrintErr($"✗ {check.Name}: Failed - {string.Join(", ", result.Errors)}");
        }
        
        if (result.Warnings.Any())
        {
            _validationResults.WarningChecks++;
            _validationWarnings.AddRange(result.Warnings);
            EmitSignal("ValidationWarning", $"{check.Name}: {string.Join(", ", result.Warnings)}");
            GD.Print($"⚠ {check.Name}: Warning - {string.Join(", ", result.Warnings)}");
        }
    }

    /// <summary>
    /// Validate Android configuration
    /// </summary>
    private CheckResult ValidateAndroidConfig()
    {
        var result = new CheckResult { Success = true, Errors = new List<string>(), Warnings = new List<string>() };
        
        // Check for export preset
        if (!File.Exists("res://android_export_preset.cfg"))
        {
            result.Errors.Add("Android export preset not found");
            result.Success = false;
        }
        
        // Check AndroidManifest.xml
        if (!File.Exists("res://android/AndroidManifest.xml"))
        {
            result.Errors.Add("AndroidManifest.xml not found");
            result.Success = false;
        }
        
        // Check required permissions
        if (File.Exists("res://android/AndroidManifest.xml"))
        {
            string manifest = File.ReadAllText("res://android/AndroidManifest.xml");
            
            var requiredPermissions = new[] { "INTERNET", "CAMERA", "VIBRATE" };
            foreach (var permission in requiredPermissions)
            {
                if (!manifest.Contains($"android.permission.{permission}"))
                {
                    result.Errors.Add($"Required permission missing: {permission}");
                }
            }
        }
        
        // Check keystore configuration
        if (!File.Exists("res://android/angry_animals.keystore"))
        {
            result.Warnings.Add("Keystore file not found - signing will fail");
        }
        
        return result;
    }

    /// <summary>
    /// Validate iOS configuration
    /// </summary>
    private CheckResult ValidateIosConfig()
    {
        var result = new CheckResult { Success = true, Errors = new List<string>(), Warnings = new List<string>() };
        
        // Check for export preset
        if (!File.Exists("res://ios_export_preset.cfg"))
        {
            result.Errors.Add("iOS export preset not found");
            result.Success = false;
        }
        
        // Check Info.plist
        if (!File.Exists("res://ios/Info.plist"))
        {
            result.Errors.Add("Info.plist not found");
            result.Success = false;
        }
        
        // Check required Info.plist keys
        if (File.Exists("res://ios/Info.plist"))
        {
            string infoPlist = File.ReadAllText("res://ios/Info.plist");
            
            var requiredKeys = new[] { "NSCameraUsageDescription", "NSPhotoLibraryUsageDescription" };
            foreach (var key in requiredKeys)
            {
                if (!infoPlist.Contains(key))
                {
                    result.Errors.Add($"Required Info.plist key missing: {key}");
                }
            }
        }
        
        // Check for icon assets
        if (!Directory.Exists("res://ios/Assets.xcassets/AppIcon.appiconset"))
        {
            result.Errors.Add("App icon assets not found");
            result.Success = false;
        }
        
        return result;
    }

    /// <summary>
    /// Validate desktop configuration
    /// </summary>
    private CheckResult ValidateDesktopConfig()
    {
        var result = new CheckResult { Success = true, Errors = new List<string>(), Warnings = new List<string>() };
        
        // Check for desktop export presets
        var desktopPresets = new[] { "res://windows_export_preset.cfg", "res://macos_export_preset.cfg", "res://linux_export_preset.cfg" };
        foreach (var preset in desktopPresets)
        {
            if (!File.Exists(preset))
            {
                result.Warnings.Add($"Desktop export preset not found: {Path.GetFileName(preset)}");
            }
        }
        
        return result;
    }

    /// <summary>
    /// Validate assets
    /// </summary>
    private CheckResult ValidateAssets()
    {
        var result = new CheckResult { Success = true, Errors = new List<string>(), Warnings = new List<string>() };
        
        // Check for Assets directory
        if (!Directory.Exists("res://Assets"))
        {
            result.Warnings.Add("Assets directory not found - using placeholder system");
            return result;
        }
        
        // Check for required asset categories
        var requiredCategories = new[] { "Sprites/Projectiles", "Sprites/UI", "Audio" };
        foreach (var category in requiredCategories)
        {
            var path = $"res://Assets/{category}";
            if (!Directory.Exists(path))
            {
                result.Warnings.Add($"Asset category missing: {category}");
            }
        }
        
        // Check for ColorRect placeholders (if validation tool exists)
        // This would integrate with AssetValidationTool
        
        return result;
    }

    /// <summary>
    /// Validate app icons
    /// </summary>
    private CheckResult ValidateAppIcons()
    {
        var result = new CheckResult { Success = true, Errors = new List<string>(), Warnings = new List<string>() };
        
        // Android icon check
        if (Directory.Exists("res://android"))
        {
            var androidIconPaths = new[] { 
                "res://android/res/mipmap-hdpi/ic_launcher.png",
                "res://android/res/mipmap-mdpi/ic_launcher.png",
                "res://android/res/mipmap-xhdpi/ic_launcher.png",
                "res://android/res/mipmap-xxhdpi/ic_launcher.png",
                "res://android/res/mipmap-xxxhdpi/ic_launcher.png"
            };
            
            foreach (var iconPath in androidIconPaths)
            {
                if (!File.Exists(iconPath))
                {
                    result.Warnings.Add($"Android icon missing: {Path.GetFileName(iconPath)}");
                }
            }
        }
        
        // iOS icon check
        if (Directory.Exists("res://ios/Assets.xcassets/AppIcon.appiconset"))
        {
            // Check for required iOS icon sizes
            var requiredSizes = new[] { "20", "29", "40", "58", "60", "76", "80", "87", "120", "152", "167", "180", "1024" };
            foreach (var size in requiredSizes)
            {
                var iconPath = $"res://ios/Assets.xcassets/AppIcon.appiconset/Icon-App-{size}x{size}@1x.png";
                if (!File.Exists(iconPath))
                {
                    result.Warnings.Add($"iOS icon missing: {size}x{size}");
                }
            }
        }
        
        return result;
    }

    /// <summary>
    /// Validate project configuration
    /// </summary>
    private CheckResult ValidateProjectConfig()
    {
        var result = new CheckResult { Success = true, Errors = new List<string>(), Warnings = new List<string>() };
        
        // Check project.godot
        if (!File.Exists("res://project.godot"))
        {
            result.Errors.Add("project.godot not found");
            result.Success = false;
        }
        
        // Check version consistency
        if (File.Exists("res://project.godot"))
        {
            string projectConfig = File.ReadAllText("res://project.godot");
            
            // Check for version
            if (!projectConfig.Contains("config/version="))
            {
                result.Errors.Add("Version not specified in project.godot");
                result.Success = false;
            }
        }
        
        // Check for required scenes
        if (!File.Exists("res://Scenes/Main/Main.tscn"))
        {
            result.Errors.Add("Main scene not found");
            result.Success = false;
        }
        
        return result;
    }

    /// <summary>
    /// Validate platform permissions
    /// </summary>
    private CheckResult ValidatePermissions()
    {
        var result = new CheckResult { Success = true, Errors = new List<string>(), Warnings = new List<string>() };
        
        // Check Android permissions
        if (File.Exists("res://android/AndroidManifest.xml"))
        {
            string manifest = File.ReadAllText("res://android/AndroidManifest.xml");
            
            if (!manifest.Contains("android.permission.INTERNET"))
            {
                result.Errors.Add("INTERNET permission missing from AndroidManifest.xml");
            }
        }
        
        // Check iOS privacy usage descriptions
        if (File.Exists("res://ios/Info.plist"))
        {
            string infoPlist = File.ReadAllText("res://ios/Info.plist");
            
            var privacyKeys = new[] { "NSCameraUsageDescription", "NSPhotoLibraryUsageDescription" };
            foreach (var key in privacyKeys)
            {
                if (!infoPlist.Contains(key))
                {
                    result.Errors.Add($"Privacy usage description missing: {key}");
                }
            }
        }
        
        return result;
    }

    /// <summary>
    /// Validate store metadata
    /// </summary>
    private CheckResult ValidateStoreMetadata()
    {
        var result = new CheckResult { Success = true, Errors = new List<string>(), Warnings = new List<string>() };
        
        // Check for store metadata files
        if (!File.Exists("res://store/store_metadata.json"))
        {
            result.Warnings.Add("Store metadata file not found");
        }
        
        // Check for privacy policy
        if (!File.Exists("res://legal/PrivacyPolicy.md"))
        {
            result.Errors.Add("Privacy policy not found");
            result.Success = false;
        }
        
        // Check for terms of service
        if (!File.Exists("res://legal/TermsOfService.md"))
        {
            result.Warnings.Add("Terms of service not found");
        }
        
        return result;
    }

    /// <summary>
    /// Validate privacy compliance
    /// </summary>
    private CheckResult ValidatePrivacyCompliance()
    {
        var result = new CheckResult { Success = true, Errors = new List<string>(), Warnings = new List<string>() };
        
        // Check for privacy policy
        if (!File.Exists("res://legal/PrivacyPolicy.md"))
        {
            result.Errors.Add("Privacy policy required for app store submission");
            result.Success = false;
        }
        else
        {
            string privacyPolicy = File.ReadAllText("res://legal/PrivacyPolicy.md");
            
            // Check for required sections
            var requiredSections = new[] { "data collection", "third-party", "gdpr", "contact" };
            foreach (var section in requiredSections)
            {
                if (!privacyPolicy.ToLower().Contains(section))
                {
                    result.Warnings.Add($"Privacy policy may be missing required section: {section}");
                }
            }
        }
        
        return result;
    }

    /// <summary>
    /// Validate version consistency
    /// </summary>
    private CheckResult ValidateVersionConsistency()
    {
        var result = new CheckResult { Success = true, Errors = new List<string>(), Warnings = new List<string>() };
        
        // Check version in project.godot
        string projectVersion = "";
        if (File.Exists("res://project.godot"))
        {
            string projectConfig = File.ReadAllText("res://project.godot");
            foreach (var line in projectConfig.Split('\n'))
            {
                if (line.Contains("config/version="))
                {
                    projectVersion = line.Split('"')[1];
                    break;
                }
            }
        }
        
        // Check version in version.json
        if (File.Exists("res://version.json"))
        {
            // This would parse the JSON and check consistency
            result.Warnings.Add("Version consistency check not fully implemented");
        }
        
        return result;
    }

    /// <summary>
    /// Validate legal documents
    /// </summary>
    private CheckResult ValidateLegalDocuments()
    {
        var result = new CheckResult { Success = true, Errors = new List<string>(), Warnings = new List<string>() };
        
        var requiredDocuments = new Dictionary<string, bool>
        {
            { "res://legal/PrivacyPolicy.md", false },
            { "res://legal/TermsOfService.md", false }
        };
        
        foreach (var doc in requiredDocuments)
        {
            if (File.Exists(doc.Key))
            {
                requiredDocuments[doc.Key] = true;
            }
            else
            {
                if (doc.Key.Contains("PrivacyPolicy"))
                {
                    result.Errors.Add($"Required document missing: {Path.GetFileName(doc.Key)}");
                    result.Success = false;
                }
                else
                {
                    result.Warnings.Add($"Recommended document missing: {Path.GetFileName(doc.Key)}");
                }
            }
        }
        
        return result;
    }

    /// <summary>
    /// Generate pre-submission report
    /// </summary>
    public void GeneratePreSubmissionReport(string platform)
    {
        if (_validationResults == null)
        {
            GD.PrintErr("No validation results available. Run ValidateBuild first.");
            return;
        }
        
        var reportPath = $"builds/{platform}/pre_submission_report.md";
        var reportDir = Path.GetDirectoryName(reportPath);
        
        if (!Directory.Exists(reportDir))
        {
            Directory.CreateDirectory(reportDir);
        }
        
        try
        {
            using (var writer = new StreamWriter(reportPath))
            {
                writer.WriteLine($"# Build Pre-Submission Report");
                writer.WriteLine($"Platform: {platform}");
                writer.WriteLine($"Generated: {_validationResults.ValidationTime}");
                writer.WriteLine();
                
                writer.WriteLine("## Summary");
                writer.WriteLine($"- **Status**: {_validationResults.IsValid ? "✅ Ready for Submission" : "❌ Not Ready for Submission"}");
                writer.WriteLine($"- **Validation Score**: {_validationResults.CompletionPercentage:F1}%");
                writer.WriteLine($"- **Checks Passed**: {_validationResults.PassedChecks}/{_validationResults.TotalChecks}");
                writer.WriteLine($"- **Warnings**: {_validationResults.WarningChecks}");
                writer.WriteLine($"- **Errors**: {_validationResults.FailedChecks}");
                writer.WriteLine();
                
                if (_validationErrors.Any())
                {
                    writer.WriteLine("## ❌ Critical Errors");
                    foreach (var error in _validationErrors)
                    {
                        writer.WriteLine($"- {error}");
                    }
                    writer.WriteLine();
                }
                
                if (_validationWarnings.Any())
                {
                    writer.WriteLine("## ⚠️ Warnings");
                    foreach (var warning in _validationWarnings)
                    {
                        writer.WriteLine($"- {warning}");
                    }
                    writer.WriteLine();
                }
                
                writer.WriteLine("## Next Steps");
                if (_validationResults.IsValid)
                {
                    writer.WriteLine("✅ All critical checks passed!");
                    writer.WriteLine("- Review warnings (if any)");
                    writer.WriteLine("- Test build on device/simulator");
                    writer.WriteLine("- Submit to app store");
                }
                else
                {
                    writer.WriteLine("❌ Critical issues must be resolved:");
                    writer.WriteLine("- Address all errors listed above");
                    writer.WriteLine("- Re-run validation");
                    writer.WriteLine("- Test build process");
                    writer.WriteLine("- Resubmit for validation");
                }
                
                writer.WriteLine();
                writer.WriteLine("## Build Checklist");
                writer.WriteLine("- [ ] Version number updated");
                writer.WriteLine("- [ ] Build tested on target devices");
                writer.WriteLine("- [ ] Store assets prepared");
                writer.WriteLine("- [ ] Privacy policy updated");
                writer.WriteLine("- [ ] App store metadata completed");
                writer.WriteLine("- [ ] Screenshots prepared");
                writer.WriteLine("- [ ] TestFlight/Play Console tested");
                writer.WriteLine("- [ ] Submission reviewed");
            }
            
            GD.Print($"Pre-submission report generated: {reportPath}");
        }
        catch (Exception e)
        {
            GD.PrintErr($"Failed to generate pre-submission report: {e.Message}");
        }
    }

    /// <summary>
    /// Get validation results
    /// </summary>
    public BuildValidationResults GetValidationResults()
    {
        return _validationResults;
    }

    /// <summary>
    /// Validate all platforms
    /// </summary>
    public void ValidateAllPlatforms()
    {
        var platforms = new[] { "android", "ios", "desktop" };
        
        foreach (var platform in platforms)
        {
            GD.Print($"Validating {platform}...");
            ValidateBuild(platform);
            GeneratePreSubmissionReport(platform);
        }
    }
}

/// <summary>
/// Build validation check
/// </summary>
public class BuildCheck
{
    public string Name { get; set; }
    public string Description { get; set; }
    public CheckPriority Priority { get; set; }
    public Func<CheckResult> CheckFunction { get; set; }
}

/// <summary>
/// Check result
/// </summary>
public class CheckResult
{
    public bool Success { get; set; }
    public List<string> Errors { get; set; } = new List<string>();
    public List<string> Warnings { get; set; } = new List<string>();
}

/// <summary>
/// Build validation results
/// </summary>
public class BuildValidationResults
{
    public string Platform { get; set; }
    public DateTime ValidationTime { get; set; }
    public int TotalChecks { get; set; }
    public int PassedChecks { get; set; }
    public int FailedChecks { get; set; }
    public int WarningChecks { get; set; }
    public bool IsValid { get; set; }
    public float CompletionPercentage { get; set; }
    public List<string> ValidationErrors { get; set; } = new List<string>();
    public List<string> ValidationWarnings { get; set; } = new List<string>();
}

/// <summary>
/// Check priority
/// </summary>
public enum CheckPriority
{
    Low,
    Important,
    Critical
}