using System;
using Godot;

/// <summary>
/// Centralized settings manager for all game configuration values.
/// This singleton provides inspector-tweakable parameters for physics, UI, difficulty, and gameplay settings.
/// Designed to be accessible to non-coders who can adjust values in the Inspector without touching code.
/// </summary>
public partial class GameSettingsManager : Node
{
    public static GameSettingsManager Instance { get; private set; } = null!;

    public override void _Ready()
    {
        Instance = this;
        ProcessMode = ProcessModeEnum.Always;
        
        // Load settings from file
        LoadSettings();
    }

    public override void _ExitTree()
    {
        // Save settings when exiting
        SaveSettings();
    }

    #region Physics Settings
    
    [Header("🔧 Physics Settings")]
    [Tooltip("Multiplier for slingshot impulse force. Higher = more powerful shots.")]
    [Export] public float SlingshotImpulseMultiplier { get; set; } = 20.0f;
    
    [Tooltip("Maximum slingshot impulse force. Caps the maximum shot power.")]
    [Export] public float SlingshotImpulseMax { get; set; } = 1200.0f;
    
    [Tooltip("Maximum drag distance for slingshot. Controls shot angle range.")]
    [Export] public float SlingshotDragMax { get; set; } = 60.0f;
    
    [Tooltip("Minimum drag distance to launch projectile. Prevents accidental shots.")]
    [Export] public float SlingshotDragMin { get; set; } = 10.0f;
    
    [Tooltip("Projectile speed threshold for 'almost stopped' detection.")]
    [Export] public float ProjectileStoppedThreshold { get; set; } = 0.1f;
    
    [Tooltip("Gravity scale applied to all projectiles.")]
    [Export] public float ProjectileGravityScale { get; set; } = 1.0f;
    
    [Tooltip("Bounce coefficient for projectile-wall collisions.")]
    [Export] public float ProjectileBounceCoefficient { get; set; } = 0.7f;
    
    [Tooltip("Character movement speed in pixels per second.")]
    [Export] public float CharacterMoveSpeed { get; set; } = 200.0f;
    
    [Tooltip("Character jump force in pixels per second.")]
    [Export] public float CharacterJumpForce { get; set; } = 400.0f;
    
    [Tooltip("Character movement acceleration.")]
    [Export] public float CharacterAcceleration { get; set; } = 1500.0f;
    
    #endregion

    #region Ragdoll Physics Settings
    
    [Header("🎭 Ragdoll Physics Settings")]
    [Tooltip("Joint stiffness for ragdoll limbs. 0.1 = very loose, 1.0 = very stiff.")]
    [Export] public float RagdollJointStiffness { get; set; } = 0.7f;
    
    [Tooltip("Angular damping for ragdoll limbs. Higher = less spinning.")]
    [Export] public float RagdollAngularDamping { get; set; } = 3.0f;
    
    [Tooltip("Linear damping for ragdoll limbs. Higher = slower movement.")]
    [Export] public float RagdollLinearDamping { get; set; } = 2.0f;
    
    [Tooltip("Ragdoll limb mass. Affects force response.")]
    [Export] public float RagdollLimbMass { get; set; } = 1.0f;
    
    [Tooltip("Time in seconds before ragdoll limbs are automatically cleaned up.")]
    [Export] public float RagdollLifetime { get; set; } = 8.0f;
    
    [Tooltip("Explosion force multiplier applied to ragdoll limbs.")]
    [Export] public float RagdollExplosionForceMultiplier { get; set; } = 1.0f;
    
    [Tooltip("Explosion radius in pixels. Higher = affects more limbs.")]
    [Export] public float RagdollExplosionRadius { get; set; } = 150.0f;
    
    [Tooltip("Enable/disable gravity for ragdoll limbs.")]
    [Export] public bool RagdollGravityEnabled { get; set; } = true;
    
    #endregion

    #region UI/Transition Settings
    
    [Header("🎨 UI & Transition Settings")]
    [Tooltip("Duration of level complete fade effect in seconds.")]
    [Export] public float LevelCompleteFadeDuration { get; set; } = 1.0f;
    
    [Tooltip("Color used for fade effects (R, G, B values 0-1).")]
    [Export] public Color LevelCompleteFadeColor { get; set; } = Colors.Black;
    
    [Tooltip("Duration of menu transition animations in seconds.")]
    [Export] public float MenuTransitionSpeed { get; set; } = 0.3f;
    
    [Tooltip("Time to hold score screen before auto-advance in seconds.")]
    [Export] public float ScoreScreenHoldDuration { get; set; } = 3.0f;
    
    [Tooltip("Duration of star animation in level complete screen.")]
    [Export] public float StarAnimationDuration { get; set; } = 0.3f;
    
    [Tooltip("Scale factor for star appearance animation.")]
    [Export] public float StarBounceScale { get; set; } = 1.3f;
    
    [Tooltip("Settings panel fade-in duration.")]
    [Export] public float SettingsPanelFadeInDuration { get; set; } = 0.3f;
    
    [Tooltip("Settings panel fade-out duration.")]
    [Export] public float SettingsPanelFadeOutDuration { get; set; } = 0.2f;
    
    #endregion

    #region Difficulty Settings
    
    [Header("⚡ Difficulty Settings")]
    [Tooltip("Base difficulty multiplier applied to all challenges.")]
    [Export] public float BaseDifficultyMultiplier { get; set; } = 1.0f;
    
    [Tooltip("Multiplier for enemy health across all enemy types.")]
    [Export] public float EnemyHealthMultiplier { get; set; } = 1.0f;
    
    [Tooltip("Multiplier for enemy damage output.")]
    [Export] public float EnemyDamageMultiplier { get; set; } = 1.0f;
    
    [Tooltip("Offset added to room target scores. Can be negative to make easier.")]
    [Export] public int RoomTargetScoreOffset { get; set; } = 0;
    
    [Tooltip("Percentage of optimal score required for 3-star rating.")]
    [Export] public float PerfectScoreThreshold { get; set; } = 0.9f;
    
    [Tooltip("Percentage of optimal score required for 2-star rating.")]
    [Export] public float GoodScoreThreshold { get; set; } = 0.6f;
    
    [Tooltip("Bonus points awarded for watching rewarded ads.")]
    [Export] public int RewardedAdBonusPoints { get; set; } = 5;
    
    #endregion

    #region Audio Settings
    
    [Header("🔊 Audio Settings")]
    [Tooltip("Master volume multiplier (0.0 = silent, 1.0 = full volume).")]
    [Export] public float MasterVolume { get; set; } = 1.0f;
    
    [Tooltip("Music volume multiplier.")]
    [Export] public float MusicVolume { get; set; } = 0.7f;
    
    [Tooltip("Sound effects volume multiplier.")]
    [Export] public float SfxVolume { get; set; } = 1.0f;
    
    [Tooltip("Voice/vocal sound effects volume multiplier.")]
    [Export] public float VoiceVolume { get; set; } = 0.8f;
    
    [Tooltip("Enable/disable impact vocal sounds.")]
    [Export] public bool EnableImpactVocals { get; set; } = true;
    
    [Tooltip("Maximum number of simultaneous sound effects.")]
    [Export] public int MaxSimultaneousSounds { get; set; } = 3;
    
    #endregion

    #region Visual Settings
    
    [Header("👁️ Visual Settings")]
    [Tooltip("Screen shake intensity multiplier (0.0 = no shake, 1.0 = full intensity).")]
    [Export] public float ScreenShakeIntensity { get; set; } = 1.0f;
    
    [Tooltip("Particle effect density multiplier (0.0 = no particles, 1.0 = normal, 2.0 = double).")]
    [Export] public float ParticleDensity { get; set; } = 1.0f;
    
    [Tooltip("UI animation speed multiplier.")]
    [Export] public float UiAnimationSpeed { get; set; } = 1.0f;
    
    [Tooltip("Enable/disable colorblind mode for accessibility.")]
    [Export] public bool ColorblindMode { get; set; } = false;
    
    [Tooltip("Enable/disable high contrast mode for accessibility.")]
    [Export] public bool HighContrastMode { get; set; } = false;
    
    [Tooltip("Reduce motion effects for accessibility (lower shake, slower animations).")]
    [Export] public bool ReduceMotion { get; set; } = false;
    
    [Tooltip("Scale factor for UI text.")]
    [Export] public float TextScale { get; set; } = 1.0f;
    
    [Tooltip("Enable/disable haptic feedback on supported devices.")]
    [Export] public bool HapticFeedbackEnabled { get; set; } = true;
    
    #endregion

    #region Unlockable Gameplay Modifiers
    
    [Header("🎮 Unlockable Gameplay Modifiers")]
    [Tooltip("Double ragdoll intensity and chaos physics.")]
    [Export] public bool ExtremePhysicsMode { get; set; } = false;
    
    [Tooltip("Enlarge character heads for comedic effect.")]
    [Export] public bool BigHeadsMode { get; set; } = false;
    
    [Tooltip("Each projectile triggers two explosions.")]
    [Export] public bool DoubleExplosionsMode { get; set; } = false;
    
    [Tooltip("Enable slow motion time control ability.")]
    [Export] public bool SlowMotionMode { get; set; } = false;
    
    [Tooltip("Ragdoll limbs float without gravity.")]
    [Export] public bool NoGravityMode { get; set; } = false;
    
    [Tooltip("Characters use bright neon colors.")]
    [Export] public bool ColorfulMode { get; set; } = false;
    
    [Tooltip("Disables all modifiers for hardcore/speedrun mode.")]
    [Export] public bool HardcoreMode { get; set; } = false;
    
    #endregion

    #region Settings Persistence
    
    private const string SettingsFilePath = "user://game_settings.json";
    
    public void SaveSettings()
    {
        try
        {
            using var file = FileAccess.Open(SettingsFilePath, FileAccess.ModeFlags.Write);
            if (file != null)
            {
                var json = Json.Stringify(BuildSettingsJson());
                file.StoreString(json);
            }
        }
        catch (Exception ex)
        {
            GD.PushWarning($"Failed to save settings: {ex.Message}");
        }
    }
    
    public void LoadSettings()
    {
        try
        {
            if (!FileAccess.FileExists(SettingsFilePath))
                return;
                
            using var file = FileAccess.Open(SettingsFilePath, FileAccess.ModeFlags.Read);
            if (file != null)
            {
                var json = file.GetAsText();
                if (!string.IsNullOrEmpty(json))
                {
                    ApplySettingsJson(Json.ParseString(json));
                }
            }
        }
        catch (Exception ex)
        {
            GD.PushWarning($"Failed to load settings: {ex.Message}");
        }
    }
    
    private Dictionary<string, Variant> BuildSettingsJson()
    {
        var settings = new Dictionary<string, Variant>();
        
        // Physics Settings
        settings["slingshot_impulse_multiplier"] = SlingshotImpulseMultiplier;
        settings["slingshot_impulse_max"] = SlingshotImpulseMax;
        settings["slingshot_drag_max"] = SlingshotDragMax;
        settings["slingshot_drag_min"] = SlingshotDragMin;
        settings["projectile_stopped_threshold"] = ProjectileStoppedThreshold;
        settings["projectile_gravity_scale"] = ProjectileGravityScale;
        settings["projectile_bounce_coefficient"] = ProjectileBounceCoefficient;
        settings["character_move_speed"] = CharacterMoveSpeed;
        settings["character_jump_force"] = CharacterJumpForce;
        settings["character_acceleration"] = CharacterAcceleration;
        
        // Ragdoll Settings
        settings["ragdoll_joint_stiffness"] = RagdollJointStiffness;
        settings["ragdoll_angular_damping"] = RagdollAngularDamping;
        settings["ragdoll_linear_damping"] = RagdollLinearDamping;
        settings["ragdoll_limb_mass"] = RagdollLimbMass;
        settings["ragdoll_lifetime"] = RagdollLifetime;
        settings["ragdoll_explosion_force_multiplier"] = RagdollExplosionForceMultiplier;
        settings["ragdoll_explosion_radius"] = RagdollExplosionRadius;
        settings["ragdoll_gravity_enabled"] = RagdollGravityEnabled;
        
        // UI Settings
        settings["level_complete_fade_duration"] = LevelCompleteFadeDuration;
        settings["level_complete_fade_color"] = new Color(LevelCompleteFadeColor.R, LevelCompleteFadeColor.G, LevelCompleteFadeColor.B, 1.0);
        settings["menu_transition_speed"] = MenuTransitionSpeed;
        settings["score_screen_hold_duration"] = ScoreScreenHoldDuration;
        settings["star_animation_duration"] = StarAnimationDuration;
        settings["star_bounce_scale"] = StarBounceScale;
        settings["settings_panel_fade_in_duration"] = SettingsPanelFadeInDuration;
        settings["settings_panel_fade_out_duration"] = SettingsPanelFadeOutDuration;
        
        // Difficulty Settings
        settings["base_difficulty_multiplier"] = BaseDifficultyMultiplier;
        settings["enemy_health_multiplier"] = EnemyHealthMultiplier;
        settings["enemy_damage_multiplier"] = EnemyDamageMultiplier;
        settings["room_target_score_offset"] = RoomTargetScoreOffset;
        settings["perfect_score_threshold"] = PerfectScoreThreshold;
        settings["good_score_threshold"] = GoodScoreThreshold;
        settings["rewarded_ad_bonus_points"] = RewardedAdBonusPoints;
        
        // Audio Settings
        settings["master_volume"] = MasterVolume;
        settings["music_volume"] = MusicVolume;
        settings["sfx_volume"] = SfxVolume;
        settings["voice_volume"] = VoiceVolume;
        settings["enable_impact_vocals"] = EnableImpactVocals;
        settings["max_simultaneous_sounds"] = MaxSimultaneousSounds;
        
        // Visual Settings
        settings["screen_shake_intensity"] = ScreenShakeIntensity;
        settings["particle_density"] = ParticleDensity;
        settings["ui_animation_speed"] = UiAnimationSpeed;
        settings["colorblind_mode"] = ColorblindMode;
        settings["high_contrast_mode"] = HighContrastMode;
        settings["reduce_motion"] = ReduceMotion;
        settings["text_scale"] = TextScale;
        settings["haptic_feedback_enabled"] = HapticFeedbackEnabled;
        
        // Unlockable Modifiers
        settings["extreme_physics_mode"] = ExtremePhysicsMode;
        settings["big_heads_mode"] = BigHeadsMode;
        settings["double_explosions_mode"] = DoubleExplosionsMode;
        settings["slow_motion_mode"] = SlowMotionMode;
        settings["no_gravity_mode"] = NoGravityMode;
        settings["colorful_mode"] = ColorfulMode;
        settings["hardcore_mode"] = HardcoreMode;
        
        return settings;
    }
    
    private void ApplySettingsJson(Variant jsonData)
    {
        if (jsonData.VariantType != Variant.Type.Dictionary)
            return;
            
        var settings = jsonData.AsDictionary<string, Variant>();
        
        // Helper method to safely get and convert values
        void SetIfExists<T>(string key, Action<T> setter)
        {
            if (settings.TryGetValue(key, out var value) && value.CanConvert<T>())
            {
                setter(value.As<T>());
            }
        }
        
        // Apply physics settings
        SetIfExists("slingshot_impulse_multiplier", v => SlingshotImpulseMultiplier = v);
        SetIfExists("slingshot_impulse_max", v => SlingshotImpulseMax = v);
        SetIfExists("slingshot_drag_max", v => SlingshotDragMax = v);
        SetIfExists("slingshot_drag_min", v => SlingshotDragMin = v);
        SetIfExists("projectile_stopped_threshold", v => ProjectileStoppedThreshold = v);
        SetIfExists("projectile_gravity_scale", v => ProjectileGravityScale = v);
        SetIfExists("projectile_bounce_coefficient", v => ProjectileBounceCoefficient = v);
        SetIfExists("character_move_speed", v => CharacterMoveSpeed = v);
        SetIfExists("character_jump_force", v => CharacterJumpForce = v);
        SetIfExists("character_acceleration", v => CharacterAcceleration = v);
        
        // Apply ragdoll settings
        SetIfExists("ragdoll_joint_stiffness", v => RagdollJointStiffness = v);
        SetIfExists("ragdoll_angular_damping", v => RagdollAngularDamping = v);
        SetIfExists("ragdoll_linear_damping", v => RagdollLinearDamping = v);
        SetIfExists("ragdoll_limb_mass", v => RagdollLimbMass = v);
        SetIfExists("ragdoll_lifetime", v => RagdollLifetime = v);
        SetIfExists("ragdoll_explosion_force_multiplier", v => RagdollExplosionForceMultiplier = v);
        SetIfExists("ragdoll_explosion_radius", v => RagdollExplosionRadius = v);
        SetIfExists("ragdoll_gravity_enabled", v => RagdollGravityEnabled = v);
        
        // Apply UI settings
        SetIfExists("level_complete_fade_duration", v => LevelCompleteFadeDuration = v);
        SetIfExists("level_complete_fade_color", v => LevelCompleteFadeColor = v.AsColor());
        SetIfExists("menu_transition_speed", v => MenuTransitionSpeed = v);
        SetIfExists("score_screen_hold_duration", v => ScoreScreenHoldDuration = v);
        SetIfExists("star_animation_duration", v => StarAnimationDuration = v);
        SetIfExists("star_bounce_scale", v => StarBounceScale = v);
        SetIfExists("settings_panel_fade_in_duration", v => SettingsPanelFadeInDuration = v);
        SetIfExists("settings_panel_fade_out_duration", v => SettingsPanelFadeOutDuration = v);
        
        // Apply difficulty settings
        SetIfExists("base_difficulty_multiplier", v => BaseDifficultyMultiplier = v);
        SetIfExists("enemy_health_multiplier", v => EnemyHealthMultiplier = v);
        SetIfExists("enemy_damage_multiplier", v => EnemyDamageMultiplier = v);
        SetIfExists("room_target_score_offset", v => RoomTargetScoreOffset = v);
        SetIfExists("perfect_score_threshold", v => PerfectScoreThreshold = v);
        SetIfExists("good_score_threshold", v => GoodScoreThreshold = v);
        SetIfExists("rewarded_ad_bonus_points", v => RewardedAdBonusPoints = v);
        
        // Apply audio settings
        SetIfExists("master_volume", v => MasterVolume = v);
        SetIfExists("music_volume", v => MusicVolume = v);
        SetIfExists("sfx_volume", v => SfxVolume = v);
        SetIfExists("voice_volume", v => VoiceVolume = v);
        SetIfExists("enable_impact_vocals", v => EnableImpactVocals = v);
        SetIfExists("max_simultaneous_sounds", v => MaxSimultaneousSounds = v);
        
        // Apply visual settings
        SetIfExists("screen_shake_intensity", v => ScreenShakeIntensity = v);
        SetIfExists("particle_density", v => ParticleDensity = v);
        SetIfExists("ui_animation_speed", v => UiAnimationSpeed = v);
        SetIfExists("colorblind_mode", v => ColorblindMode = v);
        SetIfExists("high_contrast_mode", v => HighContrastMode = v);
        SetIfExists("reduce_motion", v => ReduceMotion = v);
        SetIfExists("text_scale", v => TextScale = v);
        SetIfExists("haptic_feedback_enabled", v => HapticFeedbackEnabled = v);
        
        // Apply unlockable modifiers
        SetIfExists("extreme_physics_mode", v => ExtremePhysicsMode = v);
        SetIfExists("big_heads_mode", v => BigHeadsMode = v);
        SetIfExists("double_explosions_mode", v => DoubleExplosionsMode = v);
        SetIfExists("slow_motion_mode", v => SlowMotionMode = v);
        SetIfExists("no_gravity_mode", v => NoGravityMode = v);
        SetIfExists("colorful_mode", v => ColorfulMode = v);
        SetIfExists("hardcore_mode", v => HardcoreMode = v);
        
        GD.Print("Settings loaded successfully from file");
    }
    
    #endregion

    #region Difficulty Presets
    
    public void ApplyDifficultyPreset(string presetName)
    {
        switch (presetName.ToLower())
        {
            case "easy":
                ApplyEasyPreset();
                break;
            case "normal":
                ApplyNormalPreset();
                break;
            case "hard":
                ApplyHardPreset();
                break;
            case "extreme":
                ApplyExtremePreset();
                break;
        }
        
        SaveSettings();
        GD.Print($"Applied difficulty preset: {presetName}");
    }
    
    private void ApplyEasyPreset()
    {
        // More powerful slingshot, easier targets
        SlingshotImpulseMultiplier = 25.0f;
        BaseDifficultyMultiplier = 0.8f;
        EnemyHealthMultiplier = 0.7f;
        EnemyDamageMultiplier = 0.7f;
        RoomTargetScoreOffset = -10;
        PerfectScoreThreshold = 0.85f;
        GoodScoreThreshold = 0.5f;
    }
    
    private void ApplyNormalPreset()
    {
        // Default balanced settings
        SlingshotImpulseMultiplier = 20.0f;
        BaseDifficultyMultiplier = 1.0f;
        EnemyHealthMultiplier = 1.0f;
        EnemyDamageMultiplier = 1.0f;
        RoomTargetScoreOffset = 0;
        PerfectScoreThreshold = 0.9f;
        GoodScoreThreshold = 0.6f;
    }
    
    private void ApplyHardPreset()
    {
        // Less powerful slingshot, tougher enemies
        SlingshotImpulseMultiplier = 16.0f;
        BaseDifficultyMultiplier = 1.3f;
        EnemyHealthMultiplier = 1.4f;
        EnemyDamageMultiplier = 1.3f;
        RoomTargetScoreOffset = 15;
        PerfectScoreThreshold = 0.95f;
        GoodScoreThreshold = 0.7f;
    }
    
    private void ApplyExtremePreset()
    {
        // Very challenging for hardcore players
        SlingshotImpulseMultiplier = 12.0f;
        BaseDifficultyMultiplier = 1.6f;
        EnemyHealthMultiplier = 1.8f;
        EnemyDamageMultiplier = 1.6f;
        RoomTargetScoreOffset = 25;
        PerfectScoreThreshold = 0.98f;
        GoodScoreThreshold = 0.8f;
    }
    
    #endregion

    #region Utility Methods
    
    /// <summary>
    /// Gets the effective value after applying modifiers (like Hardcore Mode disabling others).
    /// </summary>
    public bool IsModifierEnabled(string modifierName)
    {
        // If hardcore mode is on, disable all modifiers
        if (HardcoreMode && modifierName != "hardcore_mode")
            return false;
            
        return modifierName switch
        {
            "extreme_physics_mode" => ExtremePhysicsMode,
            "big_heads_mode" => BigHeadsMode,
            "double_explosions_mode" => DoubleExplosionsMode,
            "slow_motion_mode" => SlowMotionMode,
            "no_gravity_mode" => NoGravityMode,
            "colorful_mode" => ColorfulMode,
            _ => false
        };
    }
    
    /// <summary>
    /// Gets physics multiplier for ragdoll effects based on Extreme Physics mode.
    /// </summary>
    public float GetRagdollPhysicsMultiplier()
    {
        return ExtremePhysicsMode ? 2.0f : 1.0f;
    }
    
    /// <summary>
    /// Gets explosion count multiplier based on Double Explosions mode.
    /// </summary>
    public int GetExplosionCountMultiplier()
    {
        return DoubleExplosionsMode ? 2 : 1;
    }
    
    /// <summary>
    /// Gets gravity scale based on No Gravity mode.
    /// </summary>
    public float GetGravityScale()
    {
        return NoGravityMode ? 0.0f : 1.0f;
    }
    
    /// <summary>
    /// Resets all settings to default values.
    /// </summary>
    public void ResetToDefaults()
    {
        ApplyDifficultyPreset("normal");
        
        // Reset individual settings
        SlingshotImpulseMultiplier = 20.0f;
        SlingshotImpulseMax = 1200.0f;
        SlingshotDragMax = 60.0f;
        SlingshotDragMin = 10.0f;
        ProjectileStoppedThreshold = 0.1f;
        ProjectileGravityScale = 1.0f;
        ProjectileBounceCoefficient = 0.7f;
        CharacterMoveSpeed = 200.0f;
        CharacterJumpForce = 400.0f;
        CharacterAcceleration = 1500.0f;
        
        // Reset ragdoll settings
        RagdollJointStiffness = 0.7f;
        RagdollAngularDamping = 3.0f;
        RagdollLinearDamping = 2.0f;
        RagdollLimbMass = 1.0f;
        RagdollLifetime = 8.0f;
        RagdollExplosionForceMultiplier = 1.0f;
        RagdollExplosionRadius = 150.0f;
        RagdollGravityEnabled = true;
        
        // Reset UI settings
        LevelCompleteFadeColor = Colors.Black;
        MenuTransitionSpeed = 0.3f;
        ScoreScreenHoldDuration = 3.0f;
        StarAnimationDuration = 0.3f;
        StarBounceScale = 1.3f;
        SettingsPanelFadeInDuration = 0.3f;
        SettingsPanelFadeOutDuration = 0.2f;
        
        // Reset audio settings
        MasterVolume = 1.0f;
        MusicVolume = 0.7f;
        SfxVolume = 1.0f;
        VoiceVolume = 0.8f;
        EnableImpactVocals = true;
        MaxSimultaneousSounds = 3;
        
        // Reset visual settings
        ScreenShakeIntensity = 1.0f;
        ParticleDensity = 1.0f;
        UiAnimationSpeed = 1.0f;
        ColorblindMode = false;
        HighContrastMode = false;
        ReduceMotion = false;
        TextScale = 1.0f;
        HapticFeedbackEnabled = true;
        
        // Disable all modifiers
        ExtremePhysicsMode = false;
        BigHeadsMode = false;
        DoubleExplosionsMode = false;
        SlowMotionMode = false;
        NoGravityMode = false;
        ColorfulMode = false;
        HardcoreMode = false;
        
        SaveSettings();
        GD.Print("All settings reset to defaults");
    }
    
    #endregion
}
