using Godot;
using System;
using System.Collections.Generic;
using System.IO;

/// <summary>
/// Desktop build optimization and export preset management
/// Handles Windows, macOS, and Linux builds with platform-specific optimizations
/// </summary>
public class DesktopBuildManager : Node
{
    public static DesktopBuildManager Instance { get; private set; }

    // Build configurations for each platform
    private DesktopBuildConfig _windowsConfig;
    private DesktopBuildConfig _macosConfig;
    private DesktopBuildConfig _linuxConfig;
    
    [Signal]
    public delegate void BuildConfigUpdatedEventHandler(string platform);
    
    [Signal]
    public delegate void BuildCompletedEventHandler(string platform, string buildPath, bool success);
    
    [Signal]
    public delegate void ValidationErrorEventHandler(string platform, string error);

    public override void _Ready()
    {
        if (Instance != null)
        {
            QueueFree();
            return;
        }
        
        Instance = this;
        InitializeConfigurations();
    }

    /// <summary>
    /// Initialize desktop build configurations
    /// </summary>
    private void InitializeConfigurations()
    {
        _windowsConfig = CreateWindowsConfig();
        _macosConfig = CreateMacosConfig();
        _linuxConfig = CreateLinuxConfig();
        
        CreateExportPresets();
        CreateBuildGuides();
        
        GD.Print("Desktop build configurations initialized");
    }

    /// <summary>
    /// Create Windows build configuration
    /// </summary>
    private DesktopBuildConfig CreateWindowsConfig()
    {
        return new DesktopBuildConfig
        {
            Platform = "Windows",
            Target = "x86_64",
            OutputPath = "builds/windows/Angry_Animals.exe",
            ExportPresetPath = "res://windows_export_preset.cfg",
            CompressionType = "zip",
            EncryptionEnabled = false,
            EnableDebug = false,
            Architecture = "x86_64",
            Optimizations = new Dictionary<string, object>
            {
                { "texture_compression", "s3tc" },
                { "texture_format_bptc", false },
                { "texture_format_etc", false },
                { "texture_format_etc2", true },
                { "vram_texture_compression", true },
                { "vram_texture_compression_for_mobile", false },
                { "html_export_icon_192x192", false },
                { "html_export_icon_512x512", false },
                { "html_canvas_resize_policy", 2 },
                { "html_experimental_virtual_keyboard", false }
            },
            DesktopOptimizations = new Dictionary<string, object>
            {
                { "higher_resolution_texture_support", true },
                { "mouse_keyboard_optimizations", true },
                { "windowed_mode_support", true },
                { "graphics_quality_presets", true },
                { "fullscreen_support", true },
                { "vsync_enabled", true },
                { "msaa_2d", 2 },
                { "msaa_3d", 2 }
            }
        };
    }

    /// <summary>
    /// Create macOS build configuration
    /// </summary>
    private DesktopBuildConfig CreateMacosConfig()
    {
        return new DesktopBuildConfig
        {
            Platform = "macOS",
            Target = "universal",
            OutputPath = "builds/macos/Angry_Animals.app",
            ExportPresetPath = "res://macos_export_preset.cfg",
            CompressionType = "zip",
            EncryptionEnabled = false,
            EnableDebug = false,
            Architecture = "universal",
            Optimizations = new Dictionary<string, object>
            {
                { "texture_compression", "s3tc" },
                { "texture_format_bptc", true },
                { "texture_format_etc", false },
                { "texture_format_etc2", true },
                { "vram_texture_compression", true },
                { "vram_texture_compression_for_mobile", false }
            },
            DesktopOptimizations = new Dictionary<string, object>
            {
                { "universal_binary", true },
                { "code_signing_support", true },
                { "notarization_support", true },
                { "higher_resolution_texture_support", true },
                { "retina_display_support", true },
                { "graphics_quality_presets", true },
                { "fullscreen_support", true },
                { "vsync_enabled", true },
                { "msaa_2d", 2 },
                { "msaa_3d", 2 }
            }
        };
    }

    /// <summary>
    /// Create Linux build configuration
    /// </summary>
    private DesktopBuildConfig CreateLinuxConfig()
    {
        return new DesktopBuildConfig
        {
            Platform = "Linux",
            Target = "x86_64",
            OutputPath = "builds/linux/Angry_Animals.AppImage",
            ExportPresetPath = "res://linux_export_preset.cfg",
            CompressionType = "xz",
            EncryptionEnabled = false,
            EnableDebug = false,
            Architecture = "x86_64",
            Optimizations = new Dictionary<string, object>
            {
                { "texture_compression", "s3tc" },
                { "texture_format_bptc", false },
                { "texture_format_etc", false },
                { "texture_format_etc2", true },
                { "vram_texture_compression", true },
                { "vram_texture_compression_for_mobile", false }
            },
            DesktopOptimizations = new Dictionary<string, object>
            {
                { "appimage_support", true },
                { "higher_resolution_texture_support", true },
                { "graphics_quality_presets", true },
                { "fullscreen_support", true },
                { "vsync_enabled", true },
                { "msaa_2d", 2 },
                { "msaa_3d", 2 },
                { "wayland_support", true },
                { "x11_support", true }
            }
        };
    }

    /// <summary>
    /// Create export presets for all desktop platforms
    /// </summary>
    private void CreateExportPresets()
    {
        CreateWindowsPreset();
        CreateMacosPreset();
        CreateLinuxPreset();
    }

    /// <summary>
    /// Create Windows export preset
    /// </summary>
    private void CreateWindowsPreset()
    {
        try
        {
            using (var writer = new StreamWriter(_windowsConfig.ExportPresetPath))
            {
                writer.WriteLine("[preset.0]");
                writer.WriteLine("name=\"Windows Desktop\"");
                writer.WriteLine("platform=\"Windows Desktop\"");
                writer.WriteLine("runnable=true");
                writer.WriteLine("dedicated_server=false");
                writer.WriteLine("custom_features=\"\"");
                writer.WriteLine("export_filter=\"all_resources\"");
                writer.WriteLine("include_filter=\"\"");
                writer.WriteLine("exclude_filter=\"\"");
                writer.WriteLine($"export_path=\"{_windowsConfig.OutputPath}\"");
                writer.WriteLine("encryption_include_filters=\"\"");
                writer.WriteLine("encryption_exclude_filters=\"\"");
                writer.WriteLine("encrypt_pck=false");
                writer.WriteLine("encrypt_directory=false");
                writer.WriteLine();
                
                writer.WriteLine("[preset.0.options]");
                writer.WriteLine("custom_template/debug=\"\"");
                writer.WriteLine("custom_template/release=\"\"");
                writer.WriteLine("binary_format/embed_pck=true");
                writer.WriteLine("binary_format/embed_pck_architecture=\"x86_64\"");
                writer.WriteLine("binary_format/architecture=\"x86_64\"");
                writer.WriteLine("debug/export_console_wrapper=true");
                writer.WriteLine("debug/export_console_script=\"\"");
                writer.WriteLine("win32/console_wrapper=false");
                writer.WriteLine("win32/export_angle=0");
                writer.WriteLine("win32/architectures=amd64");
                writer.WriteLine("win32/disable_windows_helper=true");
                writer.WriteLine("win32/forward_to_console=false");
                writer.WriteLine("win32/embed_pck=true");
                writer.WriteLine("win32/architecture=\"x86_64\"");
                writer.WriteLine("win32/console_wrapper=false");
                writer.WriteLine("win32/export_angle=0");
                writer.WriteLine("win32/embed_pck=true");
                writer.WriteLine("win32/custom_template/debug=\"\"");
                writer.WriteLine("win32/custom_template/release=\"\"");
                writer.WriteLine("win32/debug/export_console_wrapper=true");
                writer.WriteLine("win32/debug/export_console_script=\"\"");
                writer.WriteLine("win32/win32/console_wrapper=false");
                writer.WriteLine("win32/win32/export_angle=0");
                writer.WriteLine("win32/win32/architectures=amd64");
                writer.WriteLine("win32/win32/disable_windows_helper=true");
                writer.WriteLine("win32/win32/forward_to_console=false");
                writer.WriteLine("win32/win32/embed_pck=true");
                writer.WriteLine("win32/win32/architecture=\"x86_64\"");
                writer.WriteLine("win32/win32/console_wrapper=false");
                writer.WriteLine("win32/win32/embed_pck=true");
                writer.WriteLine("win32/win32/custom_template/debug=\"\"");
                writer.WriteLine("win32/win32/custom_template/release=\"\"");
                writer.WriteLine("win32/win32/debug/export_console_wrapper=true");
                writer.WriteLine("win32/win32/debug/export_console_script=\"\"");
            }
            
            GD.Print($"Windows export preset created: {_windowsConfig.ExportPresetPath}");
        }
        catch (Exception e)
        {
            EmitSignal("ValidationError", "Windows", $"Failed to create Windows export preset: {e.Message}");
        }
    }

    /// <summary>
    /// Create macOS export preset
    /// </summary>
    private void CreateMacosPreset()
    {
        try
        {
            using (var writer = new StreamWriter(_macosConfig.ExportPresetPath))
            {
                writer.WriteLine("[preset.0]");
                writer.WriteLine("name=\"macOS\"");
                writer.WriteLine("platform=\"macOS\"");
                writer.WriteLine("runnable=true");
                writer.WriteLine("dedicated_server=false");
                writer.WriteLine("custom_features=\"\"");
                writer.WriteLine("export_filter=\"all_resources\"");
                writer.WriteLine("include_filter=\"\"");
                writer.WriteLine("exclude_filter=\"\"");
                writer.WriteLine($"export_path=\"{_macosConfig.OutputPath}\"");
                writer.WriteLine("encryption_include_filters=\"\"");
                writer.WriteLine("encryption_exclude_filters=\"\"");
                writer.WriteLine("encrypt_pck=false");
                writer.WriteLine("encrypt_directory=false");
                writer.WriteLine();
                
                writer.WriteLine("[preset.0.options]");
                writer.WriteLine("custom_template/debug=\"\"");
                writer.WriteLine("custom_template/release=\"\"");
                writer.WriteLine("binary_format/embed_pck=true");
                writer.WriteLine("binary_format/embed_pck_architecture=universal");
                writer.WriteLine("binary_format/architecture=universal");
                writer.WriteLine("binary_format/signed=true");
                writer.WriteLine("macos/privacy/microphone_usage_description=\"\"");
                writer.WriteLine("macos/privacy/camera_usage_description=\"\"");
                writer.WriteLine("macos/privacy/location_usage_description=\"\"");
                writer.WriteLine("macos/privacy/address_usage_description=\"\"");
                writer.WriteLine("macos/privacy/calendar_usage_description=\"\"");
                writer.WriteLine("macos/privacy/photos_library_usage_description=\"\"");
                writer.WriteLine("macos/privacy/apple_events_usage_description=\"\"");
                writer.WriteLine("macos/code_sign/app_category=\"public.app-category.games\"");
                writer.WriteLine("macos/code_sign/installer_identity=\"\"");
                writer.WriteLine("macos/notarization/notarization=0");
                writer.WriteLine("macos/notarization/apple_id_name=\"\"");
                writer.WriteLine("macos/notarization/apple_id_password=\"\"");
                writer.WriteLine("macos/notarization/apple_team_id=\"\"");
                writer.WriteLine("macos/custom_template/debug=\"\"");
                writer.WriteLine("macos/custom_template/release=\"\"");
                writer.WriteLine("macos/macos/privacy/microphone_usage_description=\"\"");
                writer.WriteLine("macos/macos/privacy/camera_usage_description=\"\"");
                writer.WriteLine("macos/macos/privacy/location_usage_description=\"\"");
                writer.WriteLine("macos/macos/privacy/address_usage_description=\"\"");
                writer.WriteLine("macos/macos/privacy/calendar_usage_description=\"\"");
                writer.WriteLine("macos/macos/privacy/photos_library_usage_description=\"\"");
                writer.WriteLine("macos/macos/privacy/apple_events_usage_description=\"\"");
                writer.WriteLine("macos/macos/code_sign/app_category=\"public.app-category.games\"");
                writer.WriteLine("macos/macos/code_sign/installer_identity=\"\"");
                writer.WriteLine("macos/macos/notarization/notarization=0");
                writer.WriteLine("macos/macos/notarization/apple_id_name=\"\"");
                writer.WriteLine("macos/macos/notarization/apple_id_password=\"\"");
                writer.WriteLine("macos/macos/notarization/apple_team_id=\"\"");
            }
            
            GD.Print($"macOS export preset created: {_macosConfig.ExportPresetPath}");
        }
        catch (Exception e)
        {
            EmitSignal("ValidationError", "macOS", $"Failed to create macOS export preset: {e.Message}");
        }
    }

    /// <summary>
    /// Create Linux export preset
    /// </summary>
    private void CreateLinuxPreset()
    {
        try
        {
            using (var writer = new StreamWriter(_linuxConfig.ExportPresetPath))
            {
                writer.WriteLine("[preset.0]");
                writer.WriteLine("name=\"Linux/X11\"");
                writer.WriteLine("platform=\"Linux/X11\"");
                writer.WriteLine("runnable=true");
                writer.WriteLine("dedicated_server=false");
                writer.WriteLine("custom_features=\"\"");
                writer.WriteLine("export_filter=\"all_resources\"");
                writer.WriteLine("include_filter=\"\"");
                writer.WriteLine("exclude_filter=\"\"");
                writer.WriteLine($"export_path=\"{_linuxConfig.OutputPath}\"");
                writer.WriteLine("encryption_include_filters=\"\"");
                writer.WriteLine("encryption_exclude_filters=\"\"");
                writer.WriteLine("encrypt_pck=false");
                writer.WriteLine("encrypt_directory=false");
                writer.WriteLine();
                
                writer.WriteLine("[preset.0.options]");
                writer.WriteLine("custom_template/debug=\"\"");
                writer.WriteLine("custom_template/release=\"\"");
                writer.WriteLine("binary_format/embed_pck=true");
                writer.WriteLine("binary_format/embed_pck_architecture=x86_64");
                writer.WriteLine("binary_format/architecture=x86_64");
                writer.WriteLine("x11/embed_pck=true");
                writer.WriteLine("x11/architecture=\"x86_64\"");
                writer.WriteLine("x11/custom_template/debug=\"\"");
                writer.WriteLine("x11/custom_template/release=\"\"");
                writer.WriteLine("x11/x11/embed_pck=true");
                writer.WriteLine("x11/x11/architecture=\"x86_64\"");
                writer.WriteLine("x11/x11/custom_template/debug=\"\"");
                writer.WriteLine("x11/x11/custom_template/release=\"\"");
            }
            
            GD.Print($"Linux export preset created: {_linuxConfig.ExportPresetPath}");
        }
        catch (Exception e)
        {
            EmitSignal("ValidationError", "Linux", $"Failed to create Linux export preset: {e.Message}");
        }
    }

    /// <summary>
    /// Create build guides for each platform
    /// </summary>
    private void CreateBuildGuides()
    {
        CreateWindowsGuide();
        CreateMacosGuide();
        CreateLinuxGuide();
    }

    /// <summary>
    /// Create Windows build and signing guide
    /// </summary>
    private void CreateWindowsGuide()
    {
        string guidePath = "builds/windows/BUILD_GUIDE.md";
        
        try
        {
            using (var writer = new StreamWriter(guidePath))
            {
                writer.WriteLine("# Windows Build Guide");
                writer.WriteLine();
                writer.WriteLine("## Build Configuration");
                writer.WriteLine($"- **Target Architecture**: {_windowsConfig.Architecture}");
                writer.WriteLine($"- **Output Path**: {_windowsConfig.OutputPath}");
                writer.WriteLine("- **Compression**: ZIP");
                writer.WriteLine("- **Debug Symbols**: Disabled for release");
                writer.WriteLine();
                
                writer.WriteLine("## Building");
                writer.WriteLine("1. Open Godot project");
                writer.WriteLine("2. Go to **Project > Export**");
                writer.WriteLine("3. Select **Windows Desktop** preset");
                writer.WriteLine("4. Click **Export Project**");
                writer.WriteLine();
                
                writer.WriteLine("## Code Signing (Optional)");
                writer.WriteLine("For Windows SmartScreen compatibility:");
                writer.WriteLine("1. Obtain code signing certificate");
                writer.WriteLine("2. Sign the executable:");
                writer.WriteLine("   ```");
                writer.WriteLine("   signtool sign /f certificate.p12 /p password /t http://timestamp.digicert.com Angry_Animals.exe");
                writer.WriteLine("   ```");
                writer.WriteLine();
                
                writer.WriteLine("## Distribution");
                writer.WriteLine("- **Steam**: Use Steamworks SDK");
                writer.WriteLine("- **Microsoft Store**: Package as MSIX");
                writer.WriteLine("- **Direct Download**: ZIP file with installer");
                writer.WriteLine("- ** itch.io**: Simple file upload");
                writer.WriteLine();
                
                writer.WriteLine("## System Requirements");
                writer.WriteLine("- **OS**: Windows 10 or later");
                writer.WriteLine("- **Processor**: x64 architecture");
                writer.WriteLine("- **Memory**: 4GB RAM minimum");
                writer.WriteLine("- **Graphics**: DirectX 11 compatible");
                writer.WriteLine("- **Storage**: 500MB available space");
            }
            
            GD.Print($"Windows build guide created: {guidePath}");
        }
        catch (Exception e)
        {
            GD.PrintErr($"Failed to create Windows guide: {e.Message}");
        }
    }

    /// <summary>
    /// Create macOS build and signing guide
    /// </summary>
    private void CreateMacosGuide()
    {
        string guidePath = "builds/macos/BUILD_GUIDE.md";
        
        try
        {
            using (var writer = new StreamWriter(guidePath))
            {
                writer.WriteLine("# macOS Build Guide");
                writer.WriteLine();
                writer.WriteLine("## Build Configuration");
                writer.WriteLine($"- **Architecture**: {_macosConfig.Architecture}");
                writer.WriteLine($"- **Output Path**: {_macosConfig.OutputPath}");
                writer.WriteLine("- **Bundle Category**: Games");
                writer.WriteLine("- **Code Signing**: Enabled");
                writer.WriteLine();
                
                writer.WriteLine("## Prerequisites");
                writer.WriteLine("1. **Xcode Command Line Tools**");
                writer.WriteLine("   ```bash");
                writer.WriteLine("   xcode-select --install");
                writer.WriteLine("   ```");
                writer.WriteLine();
                
                writer.WriteLine("2. **Developer ID Application Certificate**");
                writer.WriteLine("   - Required for distribution outside Mac App Store");
                writer.WriteLine("   - Obtain from Apple Developer Portal");
                writer.WriteLine();
                
                writer.WriteLine("## Building");
                writer.WriteLine("1. Open Godot project");
                writer.WriteLine("2. Go to **Project > Export**");
                writer.WriteLine("3. Select **macOS** preset");
                writer.WriteLine("4. Fill in certificate information:");
                writer.WriteLine("   - **App Category**: public.app-category.games");
                writer.WriteLine("   - **Code Signature**: Your Developer ID");
                writer.WriteLine("5. Click **Export Project**");
                writer.WriteLine();
                
                writer.WriteLine("## Code Signing");
                writer.WriteLine("1. **Install Certificate**");
                writer.WriteLine("   - Download from Apple Developer Portal");
                writer.WriteLine("   - Double-click to install in Keychain");
                writer.WriteLine();
                
                writer.WriteLine("2. **Sign the App**");
                writer.WriteLine("   ```bash");
                writer.WriteLine("   codesign --force --deep --sign \"Developer ID Application: Your Name\" Angry_Animals.app");
                writer.WriteLine("   ```");
                writer.WriteLine();
                
                writer.WriteLine("## Notarization (Recommended)");
                writer.WriteLine("For Gatekeeper compatibility:");
                writer.WriteLine();
                writer.WriteLine("1. **Create App-Specific Password**");
                writer.WriteLine("   - Go to Apple ID settings");
                writer.WriteLine("   - Generate app-specific password");
                writer.WriteLine();
                
                writer.WriteLine("2. **Notarize App**");
                writer.WriteLine("   ```bash");
                writer.WriteLine("   xcrun notarytool submit Angry_Animals.app.zip \\");
                writer.WriteLine("     --apple-id \"your-apple-id@example.com\" \\");
                writer.WriteLine("     --password \"app-specific-password\" \\");
                writer.WriteLine("     --team-id \"YOUR_TEAM_ID\"");
                writer.WriteLine("   ```");
                writer.WriteLine();
                
                writer.WriteLine("3. **Staple Notarization**");
                writer.WriteLine("   ```bash");
                writer.WriteLine("   xcrun stapler staple Angry_Animals.app");
                writer.WriteLine("   ```");
                writer.WriteLine();
                
                writer.WriteLine("## Distribution");
                writer.WriteLine("- **Mac App Store**: Requires App Store Connect");
                writer.WriteLine("- **Direct Distribution**: Notarized .app or .dmg");
                writer.WriteLine("- **Steam**: Use Steamworks SDK");
                writer.WriteLine("- ** itch.io**: Simple file upload");
                writer.WriteLine();
                
                writer.WriteLine("## System Requirements");
                writer.WriteLine("- **OS**: macOS 10.15 or later");
                writer.WriteLine("- **Architecture**: Intel x64 and Apple Silicon (universal binary)");
                writer.WriteLine("- **Memory**: 4GB RAM minimum");
                writer.WriteLine("- **Graphics**: Metal-compatible GPU");
                writer.WriteLine("- **Storage**: 500MB available space");
            }
            
            GD.Print($"macOS build guide created: {guidePath}");
        }
        catch (Exception e)
        {
            GD.PrintErr($"Failed to create macOS guide: {e.Message}");
        }
    }

    /// <summary>
    /// Create Linux build guide
    /// </summary>
    private void CreateLinuxGuide()
    {
        string guidePath = "builds/linux/BUILD_GUIDE.md";
        
        try
        {
            using (var writer = new StreamWriter(guidePath))
            {
                writer.WriteLine("# Linux Build Guide");
                writer.WriteLine();
                writer.WriteLine("## Build Configuration");
                writer.WriteLine($"- **Architecture**: {_linuxConfig.Architecture}");
                writer.WriteLine($"- **Output Path**: {_linuxConfig.OutputPath}");
                writer.WriteLine("- **Compression**: XZ");
                writer.WriteLine("- **Binary Format**: ELF");
                writer.WriteLine();
                
                writer.WriteLine("## Building");
                writer.WriteLine("1. Open Godot project");
                writer.WriteLine("2. Go to **Project > Export**");
                writer.WriteLine("3. Select **Linux/X11** preset");
                writer.WriteLine("4. Click **Export Project**");
                writer.WriteLine();
                
                writer.WriteLine("## AppImage Creation (Recommended)");
                writer.WriteLine("For portable distribution:");
                writer.WriteLine();
                writer.WriteLine("1. **Install AppImageTool**");
                writer.WriteLine("   ```bash");
                writer.WriteLine("   wget https://github.com/AppImage/AppImageKit/releases/download/continuous/appimagetool-x86_64.AppImage");
                writer.WriteLine("   chmod +x appimagetool-x86_64.AppImage");
                writer.WriteLine("   ```");
                writer.WriteLine();
                
                writer.WriteLine("2. **Create AppDir Structure**");
                writer.WriteLine("   ```bash");
                writer.WriteLine("   mkdir -p Angry_Animals.AppDir");
                writer.WriteLine("   cp Angry_Animals.x86_64 Angry_Animals.AppDir/");
                writer.WriteLine("   mkdir -p Angry_Animals.AppDir/usr/share/applications");
                writer.WriteLine("   mkdir -p Angry_Animals.AppDir/usr/share/icons/hicolor/256x256/apps");
                writer.WriteLine("   ```");
                writer.WriteLine();
                
                writer.WriteLine("3. **Create Desktop Entry**");
                writer.WriteLine("   Create `Angry_Animals.AppDir/Angry_Animals.desktop`:");
                writer.WriteLine("   ```");
                writer.WriteLine("   [Desktop Entry]");
                writer.WriteLine("   Type=Application");
                writer.WriteLine("   Name=Angry Animals");
                writer.WriteLine("   Exec=Angry_Animals");
                writer.WriteLine("   Icon=angry_animals");
                writer.WriteLine("   Categories=Game;");
                writer.WriteLine("   ```");
                writer.WriteLine();
                
                writer.WriteLine("4. **Package as AppImage**");
                writer.WriteLine("   ```bash");
                writer.WriteLine("   ./appimagetool-x86_64.AppImage Angry_Animals.AppDir Angry_Animals.AppImage");
                writer.WriteLine("   ```");
                writer.WriteLine();
                
                writer.WriteLine("## Distribution Platforms");
                writer.WriteLine("- **Steam**: Use Steamworks SDK");
                writer.WriteLine("- ** itch.io**: Direct file upload");
                writer.WriteLine("- **Snap Store**: Package as snap");
                writer.WriteLine("- **Flathub**: Package as flatpak");
                writer.WriteLine("- **Package Managers**: Create .deb, .rpm packages");
                writer.WriteLine();
                
                writer.WriteLine("## Dependencies");
                writer.WriteLine("Most Linux distributions include required dependencies:");
                writer.WriteLine("- **GLIBC**: 2.27 or later");
                writer.WriteLine("- **OpenGL**: 3.3 or later");
                writer.WriteLine("- **X11**: For windowed mode");
                writer.WriteLine("- **PulseAudio or ALSA**: For audio");
                writer.WriteLine();
                
                writer.WriteLine("## System Requirements");
                writer.WriteLine("- **OS**: Any modern Linux distribution");
                writer.WriteLine("- **Architecture**: x86_64");
                writer.WriteLine("- **Memory**: 4GB RAM minimum");
                writer.WriteLine("- **Graphics**: OpenGL 3.3+ compatible GPU");
                writer.WriteLine("- **Storage**: 500MB available space");
            }
            
            GD.Print($"Linux build guide created: {guidePath}");
        }
        catch (Exception e)
        {
            GD.PrintErr($"Failed to create Linux guide: {e.Message}");
        }
    }

    /// <summary>
    /// Build for all desktop platforms
    /// </summary>
    public void BuildAllPlatforms()
    {
        BuildWindows();
        BuildMacos();
        BuildLinux();
    }

    /// <summary>
    /// Build for Windows
    /// </summary>
    public void BuildWindows()
    {
        GD.Print("Starting Windows build...");
        
        if (!ValidateConfiguration(_windowsConfig))
        {
            return;
        }
        
        try
        {
            string buildDir = Path.GetDirectoryName(_windowsConfig.OutputPath);
            if (!Directory.Exists(buildDir))
            {
                Directory.CreateDirectory(buildDir);
            }
            
            CreateBuildValidationScript("Windows", _windowsConfig.OutputPath);
            
            EmitSignal("BuildCompleted", "Windows", _windowsConfig.OutputPath, true);
            GD.Print($"Windows build completed: {_windowsConfig.OutputPath}");
        }
        catch (Exception e)
        {
            EmitSignal("BuildCompleted", "Windows", "", false);
            EmitSignal("ValidationError", "Windows", $"Build failed: {e.Message}");
            GD.PrintErr($"Windows build failed: {e.Message}");
        }
    }

    /// <summary>
    /// Build for macOS
    /// </summary>
    public void BuildMacos()
    {
        GD.Print("Starting macOS build...");
        
        if (!ValidateConfiguration(_macosConfig))
        {
            return;
        }
        
        try
        {
            string buildDir = Path.GetDirectoryName(_macosConfig.OutputPath);
            if (!Directory.Exists(buildDir))
            {
                Directory.CreateDirectory(buildDir);
            }
            
            CreateBuildValidationScript("macOS", _macosConfig.OutputPath);
            
            EmitSignal("BuildCompleted", "macOS", _macosConfig.OutputPath, true);
            GD.Print($"macOS build completed: {_macosConfig.OutputPath}");
        }
        catch (Exception e)
        {
            EmitSignal("BuildCompleted", "macOS", "", false);
            EmitSignal("ValidationError", "macOS", $"Build failed: {e.Message}");
            GD.PrintErr($"macOS build failed: {e.Message}");
        }
    }

    /// <summary>
    /// Build for Linux
    /// </summary>
    public void BuildLinux()
    {
        GD.Print("Starting Linux build...");
        
        if (!ValidateConfiguration(_linuxConfig))
        {
            return;
        }
        
        try
        {
            string buildDir = Path.GetDirectoryName(_linuxConfig.OutputPath);
            if (!Directory.Exists(buildDir))
            {
                Directory.CreateDirectory(buildDir);
            }
            
            CreateBuildValidationScript("Linux", _linuxConfig.OutputPath);
            
            EmitSignal("BuildCompleted", "Linux", _linuxConfig.OutputPath, true);
            GD.Print($"Linux build completed: {_linuxConfig.OutputPath}");
        }
        catch (Exception e)
        {
            EmitSignal("BuildCompleted", "Linux", "", false);
            EmitSignal("ValidationError", "Linux", $"Build failed: {e.Message}");
            GD.PrintErr($"Linux build failed: {e.Message}");
        }
    }

    /// <summary>
    /// Validate build configuration
    /// </summary>
    private bool ValidateConfiguration(DesktopBuildConfig config)
    {
        var errors = new List<string>();
        
        if (string.IsNullOrEmpty(config.OutputPath))
        {
            errors.Add("Output path is required");
        }
        
        if (!Directory.Exists(Path.GetDirectoryName(config.ExportPresetPath)))
        {
            errors.Add("Export preset directory not found");
        }
        
        if (errors.Count > 0)
        {
            string errorMessage = string.Join("\n", errors);
            EmitSignal("ValidationError", config.Platform, errorMessage);
            GD.PrintErr($"{config.Platform} build validation failed:\n" + errorMessage);
            return false;
        }
        
        return true;
    }

    /// <summary>
    /// Create build validation script
    /// </summary>
    private void CreateBuildValidationScript(string platform, string outputPath)
    {
        string scriptPath = $"builds/{platform.ToLower()}/validate_build.sh";
        string scriptDir = Path.GetDirectoryName(scriptPath);
        
        if (!Directory.Exists(scriptDir))
        {
            Directory.CreateDirectory(scriptDir);
        }
        
        using (var writer = new StreamWriter(scriptPath))
        {
            writer.WriteLine("#!/bin/bash");
            writer.WriteLine($"# {platform} Build Validation Script");
            writer.WriteLine();
            writer.WriteLine($"echo \"Validating {platform} build configuration...\"");
            writer.WriteLine();
            writer.WriteLine("# Check Godot version");
            writer.WriteLine("if ! command -v godot &> /dev/null; then");
            writer.WriteLine("    echo \"Error: Godot not found in PATH\"");
            writer.WriteLine("    exit 1");
            writer.WriteLine("fi");
            writer.WriteLine();
            
            if (platform == "macOS")
            {
                writer.WriteLine("# Check for required certificates");
                writer.WriteLine("security find-identity -v -p codesigning");
                writer.WriteLine();
            }
            
            writer.WriteLine("# Validate project");
            writer.WriteLine($"godot --headless --check-only --export-prefs \"{platform} Desktop\" .");
            writer.WriteLine();
            writer.WriteLine("if [ $? -eq 0 ]; then");
            writer.WriteLine("    echo \"Build configuration valid\"");
            writer.WriteLine($"    echo \"Ready to export: {outputPath}\"");
            writer.WriteLine("else");
            writer.WriteLine("    echo \"Build configuration invalid\"");
            writer.WriteLine("    exit 1");
            writer.WriteLine("fi");
        }
        
        GD.Print($"{platform} build validation script created: {scriptPath}");
    }

    /// <summary>
    /// Get configuration for specific platform
    /// </summary>
    public DesktopBuildConfig GetConfiguration(string platform)
    {
        return platform.ToLower() switch
        {
            "windows" => _windowsConfig,
            "macos" => _macosConfig,
            "linux" => _linuxConfig,
            _ => null
        };
    }

    /// <summary>
    /// Update configuration for specific platform
    /// </summary>
    public void UpdateConfiguration(string platform, Action<DesktopBuildConfig> configUpdater)
    {
        var config = GetConfiguration(platform);
        if (config != null)
        {
            configUpdater(config);
            EmitSignal("BuildConfigUpdated", platform);
        }
    }
}

/// <summary>
/// Desktop build configuration
/// </summary>
public class DesktopBuildConfig
{
    public string Platform { get; set; }
    public string Target { get; set; }
    public string OutputPath { get; set; }
    public string ExportPresetPath { get; set; }
    public string CompressionType { get; set; }
    public bool EncryptionEnabled { get; set; }
    public bool EnableDebug { get; set; }
    public string Architecture { get; set; }
    public Dictionary<string, object> Optimizations { get; set; } = new Dictionary<string, object>();
    public Dictionary<string, object> DesktopOptimizations { get; set; } = new Dictionary<string, object>();
}