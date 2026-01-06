using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

/// <summary>
/// Game feel tuning dashboard for real-time parameter adjustment
/// Exposes all game feel parameters for tuning with presets
/// </summary>
public class GameFeelTuningSystem : Node
{
    public static GameFeelTuningSystem Instance { get; private set; }

    // Tuning parameters
    private TuningParameters _currentParameters = new TuningParameters();
    private Dictionary<string, TuningPreset> _presets = new Dictionary<string, TuningPreset>();
    
    // UI components
    private CanvasLayer _tuningLayer;
    private Control _tuningPanel;
    private bool _tuningEnabled = false;
    
    // Parameter categories
    private Dictionary<string, List<string>> _parameterCategories = new Dictionary<string, List<string>>();
    
    [Signal]
    public delegate void TuningParameterChangedEventHandler(string parameter, float value);
    
    [Signal]
    public delegate void TuningPresetChangedEventHandler(string presetName);
    
    [Signal]
    public delegate void TuningResetEventHandler();

    public override void _Ready()
    {
        if (Instance != null)
        {
            QueueFree();
            return;
        }
        
        Instance = this;
        InitializeTuningSystem();
    }

    /// <summary>
    /// Initialize tuning system
    /// </summary>
    private void InitializeTuningSystem()
    {
        CreateTuningPresets();
        InitializeParameterCategories();
        LoadTuningParameters();
        
        // Only enable in debug builds or when explicitly toggled
        _tuningEnabled = OS.IsDebugBuild();
        
        if (_tuningEnabled)
        {
            CreateTuningPanel();
        }
        
        GD.Print("Game feel tuning system initialized");
    }

    /// <summary>
    /// Create tuning presets for different game feel styles
    /// </summary>
    private void CreateTuningPresets()
    {
        // Juicy Preset - High feedback, intense effects
        _presets["Juicy"] = new TuningPreset
        {
            Name = "Juicy",
            Description = "High feedback, intense effects, maximum satisfaction",
            Parameters = new TuningParameters
            {
                // Screen shake
                ScreenShakeIntensity = 1.0f,
                ScreenShakeDuration = 0.3f,
                ScreenShakeFrequency = 25f,
                
                // Particle effects
                ParticleCount = 1.0f,
                ParticleSize = 1.0f,
                ParticleLifetime = 1.0f,
                ParticleSpeed = 1.0f,
                
                // Haptic feedback
                HapticIntensity = 1.0f,
                HapticDuration = 0.2f,
                
                // Audio feedback
                AudioVolume = 1.0f,
                AudioPitchVariation = 0.1f,
                ImpactSoundVolume = 1.0f,
                
                // Physics feedback
                BounceMultiplier = 1.2f,
                ImpactForceMultiplier = 1.0f,
                VelocityDamping = 0.98f,
                
                // Visual feedback
                FlashIntensity = 1.0f,
                ColorChangeSpeed = 1.5f,
                ExpressionChangeSpeed = 1.2f
            }
        };

        // Subtle Preset - Minimal feedback, clean experience
        _presets["Subtle"] = new TuningPreset
        {
            Name = "Subtle",
            Description = "Minimal feedback, clean experience, subtle effects",
            Parameters = new TuningParameters
            {
                ScreenShakeIntensity = 0.3f,
                ScreenShakeDuration = 0.1f,
                ScreenShakeFrequency = 20f,
                
                ParticleCount = 0.5f,
                ParticleSize = 0.8f,
                ParticleLifetime = 0.8f,
                ParticleSpeed = 0.9f,
                
                HapticIntensity = 0.4f,
                HapticDuration = 0.1f,
                
                AudioVolume = 0.8f,
                AudioPitchVariation = 0.05f,
                ImpactSoundVolume = 0.8f,
                
                BounceMultiplier = 0.9f,
                ImpactForceMultiplier = 0.8f,
                VelocityDamping = 0.99f,
                
                FlashIntensity = 0.4f,
                ColorChangeSpeed = 0.8f,
                ExpressionChangeSpeed = 0.8f
            }
        };

        // Arcade Preset - Snappy, responsive, high energy
        _presets["Arcade"] = new TuningPreset
        {
            Name = "Arcade",
            Description = "Snappy, responsive, high energy arcade feel",
            Parameters = new TuningParameters
            {
                ScreenShakeIntensity = 0.8f,
                ScreenShakeDuration = 0.15f,
                ScreenShakeFrequency = 30f,
                
                ParticleCount = 0.8f,
                ParticleSize = 1.2f,
                ParticleLifetime = 0.6f,
                ParticleSpeed = 1.3f,
                
                HapticIntensity = 0.8f,
                HapticDuration = 0.05f,
                
                AudioVolume = 1.0f,
                AudioPitchVariation = 0.15f,
                ImpactSoundVolume = 1.0f,
                
                BounceMultiplier = 1.1f,
                ImpactForceMultiplier = 1.1f,
                VelocityDamping = 0.97f,
                
                FlashIntensity = 0.8f,
                ColorChangeSpeed = 2.0f,
                ExpressionChangeSpeed = 1.5f
            }
        };

        // Realistic Preset - Grounded, minimal effects
        _presets["Realistic"] = new TuningPreset
        {
            Name = "Realistic",
            Description = "Grounded, minimal effects, realistic physics",
            Parameters = new TuningParameters
            {
                ScreenShakeIntensity = 0.2f,
                ScreenShakeDuration = 0.08f,
                ScreenShakeFrequency = 15f,
                
                ParticleCount = 0.3f,
                ParticleSize = 0.7f,
                ParticleLifetime = 0.6f,
                ParticleSpeed = 0.8f,
                
                HapticIntensity = 0.2f,
                HapticDuration = 0.05f,
                
                AudioVolume = 0.7f,
                AudioPitchVariation = 0.02f,
                ImpactSoundVolume = 0.7f,
                
                BounceMultiplier = 0.8f,
                ImpactForceMultiplier = 0.7f,
                VelocityDamping = 0.995f,
                
                FlashIntensity = 0.2f,
                ColorChangeSpeed = 0.6f,
                ExpressionChangeSpeed = 0.6f
            }
        };
    }

    /// <summary>
    /// Initialize parameter categories for organization
    /// </summary>
    private void InitializeParameterCategories()
    {
        _parameterCategories["Screen Shake"] = new List<string>
        {
            "ScreenShakeIntensity",
            "ScreenShakeDuration", 
            "ScreenShakeFrequency"
        };
        
        _parameterCategories["Particle Effects"] = new List<string>
        {
            "ParticleCount",
            "ParticleSize",
            "ParticleLifetime",
            "ParticleSpeed"
        };
        
        _parameterCategories["Haptic Feedback"] = new List<string>
        {
            "HapticIntensity",
            "HapticDuration"
        };
        
        _parameterCategories["Audio"] = new List<string>
        {
            "AudioVolume",
            "AudioPitchVariation",
            "ImpactSoundVolume"
        };
        
        _parameterCategories["Physics"] = new List<string>
        {
            "BounceMultiplier",
            "ImpactForceMultiplier",
            "VelocityDamping"
        };
        
        _parameterCategories["Visual Feedback"] = new List<string>
        {
            "FlashIntensity",
            "ColorChangeSpeed",
            "ExpressionChangeSpeed"
        };
    }

    /// <summary>
    /// Create tuning panel UI
    /// </summary>
    private void CreateTuningPanel()
    {
        _tuningLayer = new CanvasLayer();
        AddChild(_tuningLayer);
        
        _tuningPanel = new PanelContainer();
        _tuningPanel.Name = "GameFeelTuningPanel";
        _tuningPanel.Visible = false;
        _tuningPanel.AnchorLeft = 1;
        _tuningPanel.AnchorTop = 0;
        _tuningPanel.AnchorRight = 1;
        _tuningPanel.AnchorBottom = 1;
        _tuningPanel.OffsetLeft = -400;
        _tuningPanel.OffsetTop = 20;
        _tuningPanel.OffsetRight = -20;
        _tuningPanel.OffsetBottom = -20;
        
        var mainVBox = new VBoxContainer();
        mainVBox.SizeFlagsVertical = Control.SizeFlags.Fill;
        mainVBox.SizeFlagsHorizontal = Control.SizeFlags.Fill;
        mainVBox.AddThemeConstantOverride("separation", 10);
        
        // Title
        var titleLabel = new Label();
        titleLabel.Text = "Game Feel Tuning";
        titleLabel.AddThemeFontSizeOverride("font_size", 18);
        titleLabel.AddThemeColorOverride("font_color", Color.Yellow);
        mainVBox.AddChild(titleLabel);
        
        // Preset selection
        var presetHBox = new HBoxContainer();
        presetHBox.AddThemeConstantOverride("separation", 10);
        
        var presetLabel = new Label();
        presetLabel.Text = "Preset:";
        presetLabel.SizeFlagsHorizontal = Control.SizeFlags.ShrinkEnd;
        presetHBox.AddChild(presetLabel);
        
        var presetOptionButton = new OptionButton();
        foreach (var preset in _presets.Keys)
        {
            presetOptionButton.AddItem(preset);
        }
        presetOptionButton.Select(0);
        presetOptionButton.ItemSelected += OnPresetSelected;
        presetHBox.AddChild(presetOptionButton);
        mainVBox.AddChild(presetHBox);
        
        // Parameter categories with scroll
        var scrollContainer = new ScrollContainer();
        scrollContainer.SizeFlagsVertical = Control.SizeFlags.Fill;
        scrollContainer.SizeFlagsHorizontal = Control.SizeFlags.Fill;
        
        var paramVBox = new VBoxContainer();
        paramVBox.SizeFlagsVertical = Control.SizeFlags.Fill;
        paramVBox.SizeFlagsHorizontal = Control.SizeFlags.Fill;
        paramVBox.AddThemeConstantOverride("separation", 15);
        
        foreach (var category in _parameterCategories.Keys)
        {
            var categoryFrame = new Frame();
            categoryFrame.AddThemeConstantOverride("margin_left", 10);
            categoryFrame.AddThemeConstantOverride("margin_top", 10);
            categoryFrame.AddThemeConstantOverride("margin_right", 10);
            categoryFrame.AddThemeConstantOverride("margin_bottom", 10);
            
            var categoryVBox = new VBoxContainer();
            categoryVBox.AddThemeConstantOverride("separation", 8);
            
            var categoryLabel = new Label();
            categoryLabel.Text = category;
            categoryLabel.AddThemeFontSizeOverride("font_size", 14);
            categoryLabel.AddThemeColorOverride("font_color", Color.Cyan);
            categoryVBox.AddChild(categoryLabel);
            
            // Parameter sliders
            foreach (var paramName in _parameterCategories[category])
            {
                var paramHBox = new HBoxContainer();
                paramHBox.AddThemeConstantOverride("separation", 10);
                
                var paramLabel = new Label();
                paramLabel.Text = GetParameterDisplayName(paramName);
                paramLabel.SizeFlagsHorizontal = Control.SizeFlags.Fill;
                paramHBox.AddChild(paramLabel);
                
                var slider = new HSlider();
                slider.MinValue = GetParameterMinValue(paramName);
                slider.MaxValue = GetParameterMaxValue(paramName);
                slider.Step = GetParameterStepValue(paramName);
                slider.Value = GetParameterValue(paramName);
                slider.SizeFlagsHorizontal = Control.SizeFlags.Fill;
                slider.ValueChanged += (value) => OnParameterChanged(paramName, value);
                paramHBox.AddChild(slider);
                
                var valueLabel = new Label();
                valueLabel.Text = slider.Value.ToString("F2");
                valueLabel.SizeFlagsHorizontal = Control.SizeFlags.ShrinkEnd;
                valueLabel.CustomMinimumSize = new Vector2(50, 0);
                paramHBox.AddChild(valueLabel);
                
                categoryVBox.AddChild(paramHBox);
            }
            
            categoryFrame.AddChild(categoryVBox);
            paramVBox.AddChild(categoryFrame);
        }
        
        scrollContainer.AddChild(paramVBox);
        mainVBox.AddChild(scrollContainer);
        
        // Control buttons
        var controlHBox = new HBoxContainer();
        controlHBox.AddThemeConstantOverride("separation", 10);
        
        var resetButton = new Button();
        resetButton.Text = "Reset to Default";
        resetButton.Pressed += ResetToDefault;
        controlHBox.AddChild(resetButton);
        
        var saveButton = new Button();
        saveButton.Text = "Save Preset";
        saveButton.Pressed += SaveCurrentPreset;
        controlHBox.AddChild(saveButton);
        
        var closeButton = new Button();
        closeButton.Text = "Close";
        closeButton.Pressed += ToggleTuningPanel;
        controlHBox.AddChild(closeButton);
        
        mainVBox.AddChild(controlHBox);
        
        _tuningPanel.AddChild(mainVBox);
        _tuningLayer.AddChild(_tuningPanel);
    }

    /// <summary>
    /// Toggle tuning panel visibility
    /// </summary>
    public void ToggleTuningPanel()
    {
        _tuningPanel.Visible = !_tuningPanel.Visible;
    }

    /// <summary>
    /// Load tuning parameters from file
    /// </summary>
    private void LoadTuningParameters()
    {
        string paramPath = "user://tuning_parameters.json";
        
        try
        {
            if (File.Exists(paramPath))
            {
                string jsonContent = File.ReadAllText(paramPath);
                var savedParams = JsonSerializer.Deserialize<TuningParameters>(jsonContent);
                
                if (savedParams != null)
                {
                    _currentParameters = savedParams;
                    ApplyParameters();
                }
            }
        }
        catch (Exception e)
        {
            GD.PrintErr($"Failed to load tuning parameters: {e.Message}");
        }
    }

    /// <summary>
    /// Save tuning parameters to file
    /// </summary>
    public void SaveTuningParameters()
    {
        string paramPath = "user://tuning_parameters.json";
        
        try
        {
            var options = new JsonSerializerOptions 
            { 
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            
            string json = JsonSerializer.Serialize(_currentParameters, options);
            File.WriteAllText(paramPath, json);
            
            GD.Print("Tuning parameters saved");
        }
        catch (Exception e)
        {
            GD.PrintErr($"Failed to save tuning parameters: {e.Message}");
        }
    }

    /// <summary>
    /// Apply current parameters to game systems
    /// </summary>
    private void ApplyParameters()
    {
        // Apply to GameFeelManager
        ApplyToGameFeelManager();
        
        // Apply to EffectsManager
        ApplyToEffectsManager();
        
        // Apply to HapticFeedbackManager
        ApplyToHapticFeedbackManager();
        
        // Apply to AudioManager
        ApplyToAudioManager();
    }

    /// <summary>
    /// Apply parameters to GameFeelManager
    /// </summary>
    private void ApplyToGameFeelManager()
    {
        // This would integrate with the actual GameFeelManager
        GD.Print($"Applied tuning: Screen shake intensity {_currentParameters.ScreenShakeIntensity}");
    }

    /// <summary>
    /// Apply parameters to EffectsManager
    /// </summary>
    private void ApplyToEffectsManager()
    {
        // This would integrate with the actual EffectsManager
        GD.Print($"Applied tuning: Particle count {_currentParameters.ParticleCount}");
    }

    /// <summary>
    /// Apply parameters to HapticFeedbackManager
    /// </summary>
    private void ApplyToHapticFeedbackManager()
    {
        // This would integrate with the actual HapticFeedbackManager
        GD.Print($"Applied tuning: Haptic intensity {_currentParameters.HapticIntensity}");
    }

    /// <summary>
    /// Apply parameters to AudioManager
    /// </summary>
    private void ApplyToAudioManager()
    {
        // This would integrate with the actual AudioManager
        GD.Print($"Applied tuning: Audio volume {_currentParameters.AudioVolume}");
    }

    /// <summary>
    /// Handle preset selection
    /// </summary>
    private void OnPresetSelected(int index)
    {
        string presetName = _presets.Keys.ElementAt(index);
        ApplyPreset(presetName);
    }

    /// <summary>
    /// Apply a tuning preset
    /// </summary>
    public void ApplyPreset(string presetName)
    {
        if (_presets.TryGetValue(presetName, out TuningPreset preset))
        {
            _currentParameters = JsonSerializer.Deserialize<TuningParameters>(JsonSerializer.Serialize(preset.Parameters));
            ApplyParameters();
            EmitSignal("TuningPresetChanged", presetName);
            
            GD.Print($"Applied tuning preset: {presetName}");
        }
        else
        {
            GD.PrintErr($"Preset not found: {presetName}");
        }
    }

    /// <summary>
    /// Handle parameter value change
    /// </summary>
    private void OnParameterChanged(string parameterName, float value)
    {
        SetParameterValue(parameterName, value);
        EmitSignal("TuningParameterChanged", parameterName, value);
    }

    /// <summary>
    /// Set parameter value
    /// </summary>
    public void SetParameterValue(string parameterName, float value)
    {
        var property = typeof(TuningParameters).GetProperty(parameterName);
        if (property != null && property.CanWrite)
        {
            property.SetValue(_currentParameters, value);
            ApplyParameters();
        }
    }

    /// <parameter name="name">Get parameter value</parameter>
    public float GetParameterValue(string parameterName)
    {
        var property = typeof(TuningParameters).GetProperty(parameterName);
        if (property != null && property.CanRead)
        {
            return (float)(property.GetValue(_currentParameters) ?? 0f);
        }
        
        return 0f;
    }

    /// <summary>
    /// Get parameter display name
    /// </summary>
    private string GetParameterDisplayName(string parameterName)
    {
        return parameterName switch
        {
            "ScreenShakeIntensity" => "Shake Intensity",
            "ScreenShakeDuration" => "Shake Duration",
            "ScreenShakeFrequency" => "Shake Frequency",
            "ParticleCount" => "Particle Count",
            "ParticleSize" => "Particle Size",
            "ParticleLifetime" => "Particle Lifetime",
            "ParticleSpeed" => "Particle Speed",
            "HapticIntensity" => "Haptic Intensity",
            "HapticDuration" => "Haptic Duration",
            "AudioVolume" => "Audio Volume",
            "AudioPitchVariation" => "Pitch Variation",
            "ImpactSoundVolume" => "Impact Volume",
            "BounceMultiplier" => "Bounce Multiplier",
            "ImpactForceMultiplier" => "Force Multiplier",
            "VelocityDamping" => "Velocity Damping",
            "FlashIntensity" => "Flash Intensity",
            "ColorChangeSpeed" => "Color Change Speed",
            "ExpressionChangeSpeed" => "Expression Speed",
            _ => parameterName
        };
    }

    /// <summary>
    /// Get parameter minimum value
    /// </summary>
    private float GetParameterMinValue(string parameterName)
    {
        return parameterName switch
        {
            "ScreenShakeIntensity" => 0f,
            "ScreenShakeDuration" => 0f,
            "ScreenShakeFrequency" => 10f,
            "ParticleCount" => 0f,
            "ParticleSize" => 0.1f,
            "ParticleLifetime" => 0.1f,
            "ParticleSpeed" => 0.1f,
            "HapticIntensity" => 0f,
            "HapticDuration" => 0.01f,
            "AudioVolume" => 0f,
            "AudioPitchVariation" => 0f,
            "ImpactSoundVolume" => 0f,
            "BounceMultiplier" => 0.1f,
            "ImpactForceMultiplier" => 0.1f,
            "VelocityDamping" => 0.9f,
            "FlashIntensity" => 0f,
            "ColorChangeSpeed" => 0.1f,
            "ExpressionChangeSpeed" => 0.1f,
            _ => 0f
        };
    }

    /// <summary>
    /// Get parameter maximum value
    /// </summary>
    private float GetParameterMaxValue(string parameterName)
    {
        return parameterName switch
        {
            "ScreenShakeIntensity" => 2f,
            "ScreenShakeDuration" => 1f,
            "ScreenShakeFrequency" => 60f,
            "ParticleCount" => 3f,
            "ParticleSize" => 3f,
            "ParticleLifetime" => 3f,
            "ParticleSpeed" => 3f,
            "HapticIntensity" => 1f,
            "HapticDuration" => 0.5f,
            "AudioVolume" => 2f,
            "AudioPitchVariation" => 0.3f,
            "ImpactSoundVolume" => 2f,
            "BounceMultiplier" => 2f,
            "ImpactForceMultiplier" => 2f,
            "VelocityDamping" => 1f,
            "FlashIntensity" => 2f,
            "ColorChangeSpeed" => 3f,
            "ExpressionChangeSpeed" => 3f,
            _ => 1f
        };
    }

    /// <summary>
    /// Get parameter step value
    /// </summary>
    private float GetParameterStepValue(string parameterName)
    {
        return parameterName switch
        {
            "ScreenShakeFrequency" => 1f,
            "HapticDuration" => 0.01f,
            "VelocityDamping" => 0.001f,
            _ => 0.1f
        };
    }

    /// <summary>
    /// Reset to default parameters
    /// </summary>
    public void ResetToDefault()
    {
        _currentParameters = new TuningParameters();
        ApplyParameters();
        EmitSignal("TuningReset");
        
        GD.Print("Tuning parameters reset to default");
    }

    /// <summary>
    /// Save current parameters as custom preset
    /// </summary>
    public void SaveCurrentPreset()
    {
        var dialog = new AcceptDialog();
        dialog.Title = "Save Preset";
        dialog.Size = new Vector2(400, 150);
        
        var vbox = new VBoxContainer();
        vbox.SizeFlagsVertical = Control.SizeFlags.Fill;
        vbox.SizeFlagsHorizontal = Control.SizeFlags.Fill;
        vbox.AddThemeConstantOverride("separation", 10);
        
        var label = new Label();
        label.Text = "Enter preset name:";
        vbox.AddChild(label);
        
        var lineEdit = new LineEdit();
        lineEdit.PlaceholderText = "Custom Preset";
        vbox.AddChild(lineEdit);
        
        dialog.AddChild(vbox);
        
        dialog.Confirmed += () => {
            string presetName = lineEdit.Text.Trim();
            if (!string.IsNullOrEmpty(presetName))
            {
                var preset = new TuningPreset
                {
                    Name = presetName,
                    Description = "Custom preset",
                    Parameters = JsonSerializer.Deserialize<TuningParameters>(JsonSerializer.Serialize(_currentParameters))
                };
                
                _presets[presetName] = preset;
                GD.Print($"Saved custom preset: {presetName}");
            }
        };
        
        var viewport = GetTree().Root;
        viewport.AddChild(dialog);
        dialog.PopupCentered();
    }

    /// <summary>
    /// Get current tuning parameters
    /// </summary>
    public TuningParameters GetCurrentParameters()
    {
        return _currentParameters;
    }

    /// <summary>
    /// Get available presets
    /// </summary>
    public Dictionary<string, TuningPreset> GetPresets()
    {
        return _presets;
    }

    /// <summary>
    /// Enable/disable tuning system
    /// </summary>
    public void SetEnabled(bool enabled)
    {
        _tuningEnabled = enabled;
        
        if (!_tuningEnabled && _tuningPanel?.Visible == true)
        {
            _tuningPanel.Visible = false;
        }
    }

    /// <summary>
    /// Check if tuning system is enabled
    /// </summary>
    public bool IsEnabled()
    {
        return _tuningEnabled;
    }
}

/// <summary>
/// Tuning parameters data structure
/// </summary>
public class TuningParameters
{
    // Screen shake parameters
    public float ScreenShakeIntensity { get; set; } = 0.5f;
    public float ScreenShakeDuration { get; set; } = 0.2f;
    public float ScreenShakeFrequency { get; set; } = 25f;
    
    // Particle effect parameters
    public float ParticleCount { get; set; } = 1.0f;
    public float ParticleSize { get; set; } = 1.0f;
    public float ParticleLifetime { get; set; } = 1.0f;
    public float ParticleSpeed { get; set; } = 1.0f;
    
    // Haptic feedback parameters
    public float HapticIntensity { get; set; } = 0.5f;
    public float HapticDuration { get; set; } = 0.1f;
    
    // Audio parameters
    public float AudioVolume { get; set; } = 1.0f;
    public float AudioPitchVariation { get; set; } = 0.1f;
    public float ImpactSoundVolume { get; set; } = 1.0f;
    
    // Physics parameters
    public float BounceMultiplier { get; set; } = 1.0f;
    public float ImpactForceMultiplier { get; set; } = 1.0f;
    public float VelocityDamping { get; set; } = 0.98f;
    
    // Visual feedback parameters
    public float FlashIntensity { get; set; } = 0.5f;
    public float ColorChangeSpeed { get; set; } = 1.0f;
    public float ExpressionChangeSpeed { get; set; } = 1.0f;
}

/// <summary>
/// Tuning preset data structure
/// </summary>
public class TuningPreset
{
    public string Name { get; set; }
    public string Description { get; set; }
    public TuningParameters Parameters { get; set; }
}