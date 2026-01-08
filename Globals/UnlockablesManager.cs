using System;
using System.Collections.Generic;
using Godot;

/// <summary>
/// Manages unlockable gameplay modifiers that players can unlock through achievements.
/// These modifiers change gameplay mechanics for fun or challenge variations.
/// Examples: Extreme Physics, Big Heads, Double Explosions, etc.
/// </summary>
public partial class UnlockablesManager : Node
{
    public static UnlockablesManager Instance { get; private set; } = null!;

    [Signal] public delegate void ModifierUnlockedEventHandler(string modifierId, string modifierName);
    [Signal] public delegate void ModifierToggledEventHandler(string modifierId, bool enabled);
    [Signal] public delegate void HardcoreModeToggledEventHandler(bool enabled);

    [Header("🎮 Unlockable Modifiers")]
    [Tooltip("Enable/disable the Extreme Physics modifier (2x ragdoll intensity).")]
    [Export] public bool ExtremePhysicsUnlocked { get; set; } = false;
    
    [Tooltip("Enable/disable the Big Heads modifier (enlarged character heads).")]
    [Export] public bool BigHeadsUnlocked { get; set; } = false;
    
    [Tooltip("Enable/disable the Double Explosions modifier (2x explosions per hit).")]
    [Export] public bool DoubleExplosionsUnlocked { get; set; } = false;
    
    [Tooltip("Enable/disable the Slow Motion modifier (time control ability).")]
    [Export] public bool SlowMotionUnlocked { get; set; } = false;
    
    [Tooltip("Enable/disable the No Gravity modifier (floating ragdolls).")]
    [Export] public bool NoGravityUnlocked { get; set; } = false;
    
    [Tooltip("Enable/disable the Colorful Mode modifier (neon character colors).")]
    [Export] public bool ColorfulModeUnlocked { get; set; } = false;

    [Header("🔒 Achievement Tracking")]
    [Tooltip("Number of levels completed without using slingshot second time.")]
    [Export] public int NoSecondShotLevelsCompleted { get; private set; } = 0;
    
    [Tooltip("Number of perfect scores achieved (3-star levels).")]
    [Export] public int PerfectScoresAchieved { get; private set; } = 0;
    
    [Tooltip("Number of consecutive perfect scores.")]
    [Export] public int ConsecutivePerfectScores { get; private set; } = 0;
    
    [Tooltip("Number of tutorial levels completed perfectly.")]
    [Export] public int TutorialLevelsCompleted { get; private set; } = 0;
    
    [Tooltip("Total number of levels completed.")]
    [Export] public int TotalLevelsCompleted { get; private set; } = 0;
    
    [Tooltip("Total number of enemies destroyed.")]
    [Export] public int TotalEnemiesDestroyed { get; private set; } = 0;

    [Header("🏆 Unlock Conditions")]
    [Tooltip("Levels needed to unlock Extreme Physics (no slingshot second use).")]
    [Export] public int ExtremePhysicsUnlockRequirement { get; set; } = 20;
    
    [Tooltip("Perfect scores needed to unlock Big Heads.")]
    [Export] public int BigHeadsUnlockRequirement { get; set; } = 10;
    
    [Tooltip("Consecutive perfect scores needed to unlock Double Explosions.")]
    [Export] public int DoubleExplosionsUnlockRequirement { get; set; } = 5;
    
    [Tooltip("Levels needed to unlock Slow Motion.")]
    [Export] public int SlowMotionUnlockRequirement { get; set; } = 30;
    
    [Tooltip("Tutorial levels needed to unlock No Gravity.")]
    [Export] public int NoGravityUnlockRequirement { get; set; } = 5;
    
    [Tooltip("Total levels needed to unlock Colorful Mode.")]
    [Export] public int ColorfulModeUnlockRequirement { get; set; } = 50;

    public override void _Ready()
    {
        Instance = this;
        ProcessMode = ProcessModeEnum.Always;
        LoadUnlockablesData();
    }

    public override void _ExitTree()
    {
        SaveUnlockablesData();
    }

    #region Modifier Status

    /// <summary>
    /// Checks if a specific modifier is unlocked and available to use.
    /// </summary>
    public bool IsModifierUnlocked(string modifierId)
    {
        return modifierId switch
        {
            "extreme_physics" => ExtremePhysicsUnlocked,
            "big_heads" => BigHeadsUnlocked,
            "double_explosions" => DoubleExplosionsUnlocked,
            "slow_motion" => SlowMotionUnlocked,
            "no_gravity" => NoGravityUnlocked,
            "colorful_mode" => ColorfulModeUnlocked,
            _ => false
        };
    }

    /// <summary>
    /// Gets the display name for a modifier.
    /// </summary>
    public string GetModifierDisplayName(string modifierId)
    {
        return modifierId switch
        {
            "extreme_physics" => "Extreme Physics",
            "big_heads" => "Big Heads",
            "double_explosions" => "Double Explosions",
            "slow_motion" => "Slow Motion",
            "no_gravity" => "No Gravity",
            "colorful_mode" => "Colorful Mode",
            _ => modifierId
        };
    }

    /// <summary>
    /// Gets the description for a modifier.
    /// </summary>
    public string GetModifierDescription(string modifierId)
    {
        return modifierId switch
        {
            "extreme_physics" => "Double ragdoll intensity and chaos physics. Things get wild!",
            "big_heads" => "Stick figures get huge heads. Pure comedic value.",
            "double_explosions" => "Each projectile creates two explosions. Double the chaos!",
            "slow_motion" => "Spend time tokens to slow down gameplay for precision shots.",
            "no_gravity" => "Ragdoll limbs float like balloons. Physics take a vacation.",
            "colorful_mode" => "Characters use bright neon colors. A visual treat!",
            _ => "Unknown modifier"
        };
    }

    /// <summary>
    /// Gets the current unlock progress for a modifier (0.0 to 1.0).
    /// </summary>
    public float GetModifierUnlockProgress(string modifierId)
    {
        var requirement = GetUnlockRequirement(modifierId);
        if (requirement <= 0) return 1.0f;

        var current = GetCurrentProgress(modifierId);
        return Mathf.Clamp((float)current / requirement, 0.0f, 1.0f);
    }

    private int GetUnlockRequirement(string modifierId)
    {
        return modifierId switch
        {
            "extreme_physics" => ExtremePhysicsUnlockRequirement,
            "big_heads" => BigHeadsUnlockRequirement,
            "double_explosions" => DoubleExplosionsUnlockRequirement,
            "slow_motion" => SlowMotionUnlockRequirement,
            "no_gravity" => NoGravityUnlockRequirement,
            "colorful_mode" => ColorfulModeUnlockRequirement,
            _ => 0
        };
    }

    private int GetCurrentProgress(string modifierId)
    {
        return modifierId switch
        {
            "extreme_physics" => NoSecondShotLevelsCompleted,
            "big_heads" => PerfectScoresAchieved,
            "double_explosions" => ConsecutivePerfectScores,
            "slow_motion" => TotalLevelsCompleted,
            "no_gravity" => TutorialLevelsCompleted,
            "colorful_mode" => TotalLevelsCompleted,
            _ => 0
        };
    }

    #endregion

    #region Progress Tracking

    /// <summary>
    /// Called when a level is completed without using the slingshot a second time.
    /// </summary>
    public void OnLevelCompletedNoSecondShot()
    {
        NoSecondShotLevelsCompleted++;
        TotalLevelsCompleted++;
        
        CheckForUnlocks();
        SaveUnlockablesData();
    }

    /// <summary>
    /// Called when a perfect score (3 stars) is achieved.
    /// </summary>
    public void OnPerfectScoreAchieved()
    {
        PerfectScoresAchieved++;
        ConsecutivePerfectScores++;
        TotalLevelsCompleted++;
        
        CheckForUnlocks();
        SaveUnlockablesData();
    }

    /// <summary>
    /// Called when a level is completed but not perfectly.
    /// Resets consecutive perfect score counter.
    /// </summary>
    public void OnNonPerfectScore()
    {
        ConsecutivePerfectScores = 0;
        TotalLevelsCompleted++;
        SaveUnlockablesData();
    }

    /// <summary>
    /// Called when an enemy is destroyed.
    /// </summary>
    public void OnEnemyDestroyed()
    {
        TotalEnemiesDestroyed++;
        SaveUnlockablesData();
    }

    /// <summary>
    /// Called when a tutorial level is completed perfectly.
    /// </summary>
    public void OnTutorialLevelCompleted()
    {
        TutorialLevelsCompleted++;
        CheckForUnlocks();
        SaveUnlockablesData();
    }

    /// <summary>
    /// Checks if any modifiers should be unlocked based on current progress.
    /// </summary>
    private void CheckForUnlocks()
    {
        // Check each modifier unlock condition
        if (!ExtremePhysicsUnlocked && NoSecondShotLevelsCompleted >= ExtremePhysicsUnlockRequirement)
        {
            UnlockModifier("extreme_physics", "Extreme Physics");
        }

        if (!BigHeadsUnlocked && PerfectScoresAchieved >= BigHeadsUnlockRequirement)
        {
            UnlockModifier("big_heads", "Big Heads");
        }

        if (!DoubleExplosionsUnlocked && ConsecutivePerfectScores >= DoubleExplosionsUnlockRequirement)
        {
            UnlockModifier("double_explosions", "Double Explosions");
        }

        if (!SlowMotionUnlocked && TotalLevelsCompleted >= SlowMotionUnlockRequirement)
        {
            UnlockModifier("slow_motion", "Slow Motion");
        }

        if (!NoGravityUnlocked && TutorialLevelsCompleted >= NoGravityUnlockRequirement)
        {
            UnlockModifier("no_gravity", "No Gravity");
        }

        if (!ColorfulModeUnlocked && TotalLevelsCompleted >= ColorfulModeUnlockRequirement)
        {
            UnlockModifier("colorful_mode", "Colorful Mode");
        }
    }

    /// <summary>
    /// Unlocks a specific modifier and shows celebration.
    /// </summary>
    private void UnlockModifier(string modifierId, string displayName)
    {
        SetModifierUnlocked(modifierId, true);
        
        GD.Print($"🏆 MODIFIER UNLOCKED: {displayName} ({modifierId})");
        EmitSignal(SignalName.ModifierUnlocked, modifierId, displayName);
        
        // Show unlock celebration UI
        ShowUnlockCelebration(modifierId, displayName);
    }

    /// <summary>
    /// Shows celebration UI when a modifier is unlocked.
    /// </summary>
    private void ShowUnlockCelebration(string modifierId, string displayName)
    {
        // TODO: Create actual celebration UI
        GD.Print($"🎉 CELEBRATION: You unlocked {displayName}!");
        
        // Could show:
        // - Unlock notification popup
        // - Modifier icon animation
        // - "How to enable" tutorial prompt
    }

    #endregion

    #region Modifier Control

    /// <summary>
    /// Sets whether a modifier is unlocked.
    /// </summary>
    public void SetModifierUnlocked(string modifierId, bool unlocked)
    {
        switch (modifierId)
        {
            case "extreme_physics":
                ExtremePhysicsUnlocked = unlocked;
                break;
            case "big_heads":
                BigHeadsUnlocked = unlocked;
                break;
            case "double_explosions":
                DoubleExplosionsUnlocked = unlocked;
                break;
            case "slow_motion":
                SlowMotionUnlocked = unlocked;
                break;
            case "no_gravity":
                NoGravityUnlocked = unlocked;
                break;
            case "colorful_mode":
                ColorfulModeUnlocked = unlocked;
                break;
        }
        
        // Update GameSettingsManager
        var settings = GameSettingsManager.Instance;
        if (settings != null)
        {
            settings.ExtremePhysicsMode = unlocked && modifierId == "extreme_physics";
            settings.BigHeadsMode = unlocked && modifierId == "big_heads";
            settings.DoubleExplosionsMode = unlocked && modifierId == "double_explosions";
            settings.SlowMotionMode = unlocked && modifierId == "slow_motion";
            settings.NoGravityMode = unlocked && modifierId == "no_gravity";
            settings.ColorfulMode = unlocked && modifierId == "colorful_mode";
        }
    }

    /// <summary>
    /// Toggles a modifier on/off (only if unlocked).
    /// </summary>
    public bool ToggleModifier(string modifierId)
    {
        if (!IsModifierUnlocked(modifierId))
            return false;

        var settings = GameSettingsManager.Instance;
        if (settings == null)
            return false;

        bool newState = !GetModifierEnabled(modifierId);
        SetModifierEnabled(modifierId, newState);
        
        EmitSignal(SignalName.ModifierToggled, modifierId, newState);
        return true;
    }

    /// <summary>
    /// Sets whether a modifier is enabled (only if unlocked).
    /// </summary>
    public void SetModifierEnabled(string modifierId, bool enabled)
    {
        var settings = GameSettingsManager.Instance;
        if (settings == null)
            return;

        switch (modifierId)
        {
            case "extreme_physics":
                if (ExtremePhysicsUnlocked) settings.ExtremePhysicsMode = enabled;
                break;
            case "big_heads":
                if (BigHeadsUnlocked) settings.BigHeadsMode = enabled;
                break;
            case "double_explosions":
                if (DoubleExplosionsUnlocked) settings.DoubleExplosionsMode = enabled;
                break;
            case "slow_motion":
                if (SlowMotionUnlocked) settings.SlowMotionMode = enabled;
                break;
            case "no_gravity":
                if (NoGravityUnlocked) settings.NoGravityMode = enabled;
                break;
            case "colorful_mode":
                if (ColorfulModeUnlocked) settings.ColorfulMode = enabled;
                break;
        }
    }

    /// <summary>
    /// Gets whether a modifier is currently enabled.
    /// </summary>
    public bool GetModifierEnabled(string modifierId)
    {
        var settings = GameSettingsManager.Instance;
        if (settings == null)
            return false;

        return modifierId switch
        {
            "extreme_physics" => settings.ExtremePhysicsMode,
            "big_heads" => settings.BigHeadsMode,
            "double_explosions" => settings.DoubleExplosionsMode,
            "slow_motion" => settings.SlowMotionMode,
            "no_gravity" => settings.NoGravityMode,
            "colorful_mode" => settings.ColorfulMode,
            _ => false
        };
    }

    /// <summary>
    /// Enables or disables hardcore mode (disables all modifiers).
    /// </summary>
    public void SetHardcoreMode(bool enabled)
    {
        var settings = GameSettingsManager.Instance;
        if (settings != null)
        {
            settings.HardcoreMode = enabled;
        }
        
        EmitSignal(SignalName.HardcoreModeToggled, enabled);
        
        if (enabled)
        {
            GD.Print("🔥 HARDCORE MODE: All modifiers disabled");
        }
        else
        {
            GD.Print("✨ HARDCORE MODE: All modifiers re-enabled");
        }
    }

    /// <summary>
    /// Gets all unlocked modifiers with their current state.
    /// </summary>
    public Dictionary<string, bool> GetAllUnlockedModifiers()
    {
        var modifiers = new Dictionary<string, bool>();
        
        if (ExtremePhysicsUnlocked)
            modifiers["extreme_physics"] = GetModifierEnabled("extreme_physics");
        if (BigHeadsUnlocked)
            modifiers["big_heads"] = GetModifierEnabled("big_heads");
        if (DoubleExplosionsUnlocked)
            modifiers["double_explosions"] = GetModifierEnabled("double_explosions");
        if (SlowMotionUnlocked)
            modifiers["slow_motion"] = GetModifierEnabled("slow_motion");
        if (NoGravityUnlocked)
            modifiers["no_gravity"] = GetModifierEnabled("no_gravity");
        if (ColorfulModeUnlocked)
            modifiers["colorful_mode"] = GetModifierEnabled("colorful_mode");
            
        return modifiers;
    }

    #endregion

    #region Persistence

    private const string UnlockablesDataPath = "user://unlockables_data.json";

    private void LoadUnlockablesData()
    {
        try
        {
            if (!FileAccess.FileExists(UnlockablesDataPath))
                return;

            using var file = FileAccess.Open(UnlockablesDataPath, FileAccess.ModeFlags.Read);
            if (file != null)
            {
                var json = file.GetAsText();
                if (!string.IsNullOrEmpty(json))
                {
                    ApplyUnlockablesData(Json.ParseString(json));
                }
            }
        }
        catch (Exception ex)
        {
            GD.PushWarning($"Failed to load unlockables data: {ex.Message}");
        }
    }

    private void SaveUnlockablesData()
    {
        try
        {
            var data = BuildUnlockablesDataJson();
            using var file = FileAccess.Open(UnlockablesDataPath, FileAccess.ModeFlags.Write);
            if (file != null)
            {
                file.StoreString(Json.Stringify(data));
            }
        }
        catch (Exception ex)
        {
            GD.PushWarning($"Failed to save unlockables data: {ex.Message}");
        }
    }

    private Dictionary<string, Variant> BuildUnlockablesDataJson()
    {
        var data = new Dictionary<string, Variant>();
        
        // Unlock states
        data["extreme_physics_unlocked"] = ExtremePhysicsUnlocked;
        data["big_heads_unlocked"] = BigHeadsUnlocked;
        data["double_explosions_unlocked"] = DoubleExplosionsUnlocked;
        data["slow_motion_unlocked"] = SlowMotionUnlocked;
        data["no_gravity_unlocked"] = NoGravityUnlocked;
        data["colorful_mode_unlocked"] = ColorfulModeUnlocked;
        
        // Progress tracking
        data["no_second_shot_levels"] = NoSecondShotLevelsCompleted;
        data["perfect_scores_achieved"] = PerfectScoresAchieved;
        data["consecutive_perfect_scores"] = ConsecutivePerfectScores;
        data["tutorial_levels_completed"] = TutorialLevelsCompleted;
        data["total_levels_completed"] = TotalLevelsCompleted;
        data["total_enemies_destroyed"] = TotalEnemiesDestroyed;
        
        return data;
    }

    private void ApplyUnlockablesData(Variant jsonData)
    {
        if (jsonData.VariantType != Variant.Type.Dictionary)
            return;

        var data = jsonData.AsDictionary<string, Variant>();

        // Load unlock states
        ExtremePhysicsUnlocked = data.GetValueOrDefault("extreme_physics_unlocked", ExtremePhysicsUnlocked).AsBool();
        BigHeadsUnlocked = data.GetValueOrDefault("big_heads_unlocked", BigHeadsUnlocked).AsBool();
        DoubleExplosionsUnlocked = data.GetValueOrDefault("double_explosions_unlocked", DoubleExplosionsUnlocked).AsBool();
        SlowMotionUnlocked = data.GetValueOrDefault("slow_motion_unlocked", SlowMotionUnlocked).AsBool();
        NoGravityUnlocked = data.GetValueOrDefault("no_gravity_unlocked", NoGravityUnlocked).AsBool();
        ColorfulModeUnlocked = data.GetValueOrDefault("colorful_mode_unlocked", ColorfulModeUnlocked).AsBool();
        
        // Load progress
        NoSecondShotLevelsCompleted = data.GetValueOrDefault("no_second_shot_levels", NoSecondShotLevelsCompleted).AsInt32();
        PerfectScoresAchieved = data.GetValueOrDefault("perfect_scores_achieved", PerfectScoresAchieved).AsInt32();
        ConsecutivePerfectScores = data.GetValueOrDefault("consecutive_perfect_scores", ConsecutivePerfectScores).AsInt32();
        TutorialLevelsCompleted = data.GetValueOrDefault("tutorial_levels_completed", TutorialLevelsCompleted).AsInt32();
        TotalLevelsCompleted = data.GetValueOrDefault("total_levels_completed", TotalLevelsCompleted).AsInt32();
        TotalEnemiesDestroyed = data.GetValueOrDefault("total_enemies_destroyed", TotalEnemiesDestroyed).AsInt32();
    }

    #endregion

    #region Utility Methods

    /// <summary>
    /// Resets all progress (for testing).
    /// </summary>
    public void ResetAllProgress()
    {
        ExtremePhysicsUnlocked = false;
        BigHeadsUnlocked = false;
        DoubleExplosionsUnlocked = false;
        SlowMotionUnlocked = false;
        NoGravityUnlocked = false;
        ColorfulModeUnlocked = false;
        
        NoSecondShotLevelsCompleted = 0;
        PerfectScoresAchieved = 0;
        ConsecutivePerfectScores = 0;
        TutorialLevelsCompleted = 0;
        TotalLevelsCompleted = 0;
        TotalEnemiesDestroyed = 0;
        
        SaveUnlockablesData();
        GD.Print("All unlockables progress reset");
    }

    /// <summary>
    /// Unlocks all modifiers immediately (for testing).
    /// </summary>
    public void UnlockAllModifiers()
    {
        ExtremePhysicsUnlocked = true;
        BigHeadsUnlocked = true;
        DoubleExplosionsUnlocked = true;
        SlowMotionUnlocked = true;
        NoGravityUnlocked = true;
        ColorfulModeUnlocked = true;
        
        CheckForUnlocks();
        SaveUnlockablesData();
        GD.Print("All modifiers unlocked for testing");
    }

    #endregion
}
