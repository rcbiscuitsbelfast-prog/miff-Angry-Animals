using System;
using System.Collections.Generic;
using Godot;

/// <summary>
/// Manages cosmetic loot drops for perfect score achievements.
/// Provides a weighted random drop system for hats, glasses, emotions, and other cosmetics.
/// Designed to encourage replayability through organic unlock progression.
/// </summary>
public partial class CosmeticLootTable : Node
{
    public static CosmeticLootTable Instance { get; private set; } = null!;

    [Signal] public delegate void CosmeticEarnedEventHandler(string cosmeticId, string cosmeticType);

    [Header("🎁 Loot Drop Configuration")]
    [Tooltip("Enable/disable cosmetic loot drops entirely.")]
    [Export] public bool LootDropsEnabled { get; set; } = true;
    
    [Tooltip("Base chance for any cosmetic drop (0.0 = never, 1.0 = always).")]
    [Export] public float BaseDropChance { get; set; } = 1.0f;
    
    [Tooltip("Multiplied by drop chance if player already has cosmetic (prevents duplicates).")]
    [Export] public float DuplicateDropMultiplier { get; set; } = 0.3f;

    [Header("🏆 Perfect Score Bonuses")]
    [Tooltip("Additional drop chance bonus for achieving perfect score.")]
    [Export] public float PerfectScoreBonusChance { get; set; } = 0.2f;
    
    [Tooltip("Bonus chance if player hasn't earned any cosmetic recently.")]
    [Export] public float DrySpellBonusChance { get; set; } = 0.15f;

    #region Cosmetic Definitions

    [Header("👒 Hat Loot Table")]
    [Tooltip("Chance weight for Cap drops (0.0 = never, 1.0 = normal, >1.0 = more likely).")]
    [Export] public float CapDropWeight { get; set; } = 1.0f;
    
    [Tooltip("Chance weight for Crown drops.")]
    [Export] public float CrownDropWeight { get; set; } = 0.8f;
    
    [Tooltip("Chance weight for Beanie drops.")]
    [Export] public float BeanieDropWeight { get; set; } = 1.2f;
    
    [Tooltip("Chance weight for Top Hat drops.")]
    [Export] public float TopHatDropWeight { get; set; } = 0.6f;
    
    [Tooltip("Chance weight for Cowboy Hat drops.")]
    [Export] public float CowboyHatDropWeight { get; set; } = 0.7f;
    
    [Tooltip("Chance weight for Beret drops.")]
    [Export] public float BeretDropWeight { get; set; } = 0.9f;

    [Header("🕶️ Glasses Loot Table")]
    [Tooltip("Chance weight for Round Glasses drops.")]
    [Export] public float RoundGlassesDropWeight { get; set; } = 1.0f;
    
    [Tooltip("Chance weight for Aviator Glasses drops.")]
    [Export] public float AviatorGlassesDropWeight { get; set; } = 0.9f;
    
    [Tooltip("Chance weight for Sunglasses drops.")]
    [Export] public float SunglassesDropWeight { get; set; } = 1.1f;
    
    [Tooltip("Chance weight for Nerd Glasses drops.")]
    [Export] public float NerdGlassesDropWeight { get; set; } = 0.8f;
    
    [Tooltip("Chance weight for Monocle drops.")]
    [Export] public float MonocleDropWeight { get; set; } = 0.6f;
    
    [Tooltip("Chance weight for 3D Glasses drops.")]
    [Export] public float ThreeDGlassesDropWeight { get; set; } = 0.7f;

    [Header("😊 Emotion Loot Table")]
    [Tooltip("Chance weight for Happy Emotion drops.")]
    [Export] public float HappyEmotionDropWeight { get; set; } = 1.0f;
    
    [Tooltip("Chance weight for Angry Emotion drops.")]
    [Export] public float AngryEmotionDropWeight { get; set; } = 0.9f;
    
    [Tooltip("Chance weight for Sad Emotion drops.")]
    [Export] public float SadEmotionDropWeight { get; set; } = 0.8f;
    
    [Tooltip("Chance weight for Excited Emotion drops.")]
    [Export] public float ExcitedEmotionDropWeight { get; set; } = 1.1f;
    
    [Tooltip("Chance weight for Surprised Emotion drops.")]
    [Export] public float SurprisedEmotionDropWeight { get; set; } = 0.7f;

    [Header("🦸‍♂️ Special Cosmetics")]
    [Tooltip("Chance weight for Moustache drops.")]
    [Export] public float MoustacheDropWeight { get; set; } = 0.5f;
    
    [Tooltip("Chance weight for Wig drops.")]
    [Export] public float WigDropWeight { get; set; } = 0.4f;
    
    [Tooltip("Chance weight for Slingshot Skin drops.")]
    [Export] public float SlingshotSkinDropWeight { get; set; } = 0.3f;
    
    [Tooltip("Chance weight for Projectile Skin drops.")]
    [Export] public float ProjectileSkinDropWeight { get; set; } = 0.3f;
    
    [Tooltip("Chance weight for Trail Effect drops.")]
    [Export] public float TrailEffectDropWeight { get; set; } = 0.2f;
    
    [Tooltip("Chance weight for Hit Effect drops.")]
    [Export] public float HitEffectDropWeight { get; set; } = 0.2f;
    
    [Tooltip("Chance weight for Victory Effect drops.")]
    [Export] public float VictoryEffectDropWeight { get; set; } = 0.1f;

    #endregion

    #region Drop History Tracking

    private int _perfectScoresSinceLastDrop = 0;
    private int _totalPerfectScores = 0;
    private DateTime _lastDropTime = DateTime.MinValue;
    
    [Header("📊 Drop History")]
    [Tooltip("Number of perfect scores since last cosmetic drop.")]
    [Export] public int PerfectScoresSinceLastDrop { get; private set; }
    
    [Tooltip("Total number of perfect scores achieved.")]
    [Export] public int TotalPerfectScores { get; private set; }
    
    [Tooltip("Last cosmetic earned (ID).")]
    [Export] public string LastEarnedCosmetic { get; private set; } = "";
    
    [Tooltip("Time of last cosmetic drop.")]
    [Export] public DateTime LastDropTime { get; private set; }

    #endregion

    public override void _Ready()
    {
        Instance = this;
        ProcessMode = ProcessModeEnum.Always;
        LoadDropHistory();
    }

    public override void _ExitTree()
    {
        SaveDropHistory();
    }

    #region Loot Drop Logic

    /// <summary>
    /// Attempts to award a cosmetic drop based on current performance.
    /// Called automatically when perfect score is achieved.
    /// </summary>
    public bool TryAwardCosmeticDrop(int starCount, int score, int levelNumber)
    {
        if (!LootDropsEnabled)
            return false;

        // Only award drops on perfect scores (3 stars)
        if (starCount < 3)
            return false;

        _totalPerfectScores++;
        PerfectScoresSinceLastDrop++;
        _perfectScoresSinceLastDrop++;

        // Calculate drop chance with bonuses
        float dropChance = CalculateDropChance();
        
        // Random roll
        var rng = new Random();
        bool dropOccurred = rng.NextDouble() < dropChance;

        if (dropOccurred)
        {
            var cosmeticId = RollRandomCosmetic();
            if (!string.IsNullOrEmpty(cosmeticId))
            {
                AwardCosmetic(cosmeticId, levelNumber);
                _perfectScoresSinceLastDrop = 0;
                _lastDropTime = DateTime.Now;
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Calculates the current drop chance with all bonuses applied.
    /// </summary>
    private float CalculateDropChance()
    {
        float chance = BaseDropChance;

        // Perfect score bonus
        chance += PerfectScoreBonusChance;

        // Dry spell bonus (if no drops for multiple perfect scores)
        if (_perfectScoresSinceLastDrop >= 3)
        {
            chance += DrySpellBonusChance * (_perfectScoresSinceLastDrop - 2);
        }

        // Cap at 100%
        return Mathf.Clamp(chance, 0.0f, 1.0f);
    }

    /// <summary>
    /// Rolls a random cosmetic from the weighted loot table.
    /// </summary>
    private string RollRandomCosmetic()
    {
        var allCosmetics = BuildWeightedCosmeticList();
        if (allCosmetics.Count == 0)
            return "";

        var rng = new Random();
        double totalWeight = 0;
        foreach (var cosmetic in allCosmetics)
        {
            totalWeight += cosmetic.Weight;
        }

        double roll = rng.NextDouble() * totalWeight;
        foreach (var cosmetic in allCosmetics)
        {
            if (roll < cosmetic.Weight)
                return cosmetic.CosmeticId;
            roll -= cosmetic.Weight;
        }

        return allCosmetics[^1].CosmeticId; // Fallback
    }

    /// <summary>
    /// Builds a weighted list of all available cosmetics.
    /// </summary>
    private List<WeightedCosmetic> BuildWeightedCosmeticList()
    {
        var cosmetics = new List<WeightedCosmetic>();

        // Hats
        cosmetics.Add(new WeightedCosmetic { CosmeticId = "cap", Type = "hat", Weight = CapDropWeight });
        cosmetics.Add(new WeightedCosmetic { CosmeticId = "crown", Type = "hat", Weight = CrownDropWeight });
        cosmetics.Add(new WeightedCosmetic { CosmeticId = "beanie", Type = "hat", Weight = BeanieDropWeight });
        cosmetics.Add(new WeightedCosmetic { CosmeticId = "tophat", Type = "hat", Weight = TopHatDropWeight });
        cosmetics.Add(new WeightedCosmetic { CosmeticId = "cowboy", Type = "hat", Weight = CowboyHatDropWeight });
        cosmetics.Add(new WeightedCosmetic { CosmeticId = "beret", Type = "hat", Weight = BeretDropWeight });

        // Glasses
        cosmetics.Add(new WeightedCosmetic { CosmeticId = "round", Type = "glasses", Weight = RoundGlassesDropWeight });
        cosmetics.Add(new WeightedCosmetic { CosmeticId = "aviator", Type = "glasses", Weight = AviatorGlassesDropWeight });
        cosmetics.Add(new WeightedCosmetic { CosmeticId = "sunglasses", Type = "glasses", Weight = SunglassesDropWeight });
        cosmetics.Add(new WeightedCosmetic { CosmeticId = "nerd_glasses", Type = "glasses", Weight = NerdGlassesDropWeight });
        cosmetics.Add(new WeightedCosmetic { CosmeticId = "monocle", Type = "glasses", Weight = MonocleDropWeight });
        cosmetics.Add(new WeightedCosmetic { CosmeticId = "3d_glasses", Type = "glasses", Weight = ThreeDGlassesDropWeight });

        // Emotions
        cosmetics.Add(new WeightedCosmetic { CosmeticId = "happy", Type = "emotion", Weight = HappyEmotionDropWeight });
        cosmetics.Add(new WeightedCosmetic { CosmeticId = "angry", Type = "emotion", Weight = AngryEmotionDropWeight });
        cosmetics.Add(new WeightedCosmetic { CosmeticId = "sad", Type = "emotion", Weight = SadEmotionDropWeight });
        cosmetics.Add(new WeightedCosmetic { CosmeticId = "excited", Type = "emotion", Weight = ExcitedEmotionDropWeight });
        cosmetics.Add(new WeightedCosmetic { CosmeticId = "surprised", Type = "emotion", Weight = SurprisedEmotionDropWeight });

        // Special cosmetics (lower drop rates)
        cosmetics.Add(new WeightedCosmetic { CosmeticId = "normal", Type = "moustache", Weight = MoustacheDropWeight });
        cosmetics.Add(new WeightedCosmetic { CosmeticId = "afro", Type = "wig", Weight = WigDropWeight });
        cosmetics.Add(new WeightedCosmetic { CosmeticId = "golden_slingshot", Type = "slingshot_skin", Weight = SlingshotSkinDropWeight });
        cosmetics.Add(new WeightedCosmetic { CosmeticId = "rainbow_projectile", Type = "projectile_skin", Weight = ProjectileSkinDropWeight });
        cosmetics.Add(new WeightedCosmetic { CosmeticId = "sparkle_trail", Type = "trail_effect", Weight = TrailEffectDropWeight });
        cosmetics.Add(new WeightedCosmetic { CosmeticId = "explosion_hit", Type = "hit_effect", Weight = HitEffectDropWeight });
        cosmetics.Add(new WeightedCosmetic { CosmeticId = "victory_fireworks", Type = "victory_effect", Weight = VictoryEffectDropWeight });

        // Filter out cosmetics player already owns (unless dry spell bonus applies)
        if (_perfectScoresSinceLastDrop < 5)
        {
            cosmetics.RemoveAll(c => PlayerProfile.Instance?.UnlockedCosmetics.Contains(c.CosmeticId) ?? false);
        }

        return cosmetics;
    }

    /// <summary>
    /// Awards a cosmetic to the player and shows celebration UI.
    /// </summary>
    private void AwardCosmetic(string cosmeticId, int levelNumber)
    {
        if (PlayerProfile.Instance != null)
        {
            PlayerProfile.Instance.UnlockCosmetic(cosmeticId);
        }

        LastEarnedCosmetic = cosmeticId;
        LastDropTime = DateTime.Now;

        GD.Print($"🎁 COSMETIC EARNED! Level {levelNumber}: {cosmeticId}");

        // Emit signal for UI to show celebration
        EmitSignal(SignalName.CosmeticEarned, cosmeticId, GetCosmeticType(cosmeticId));

        // Play celebration sound
        var audioManager = GetNodeOrNull<AudioManager>("/root/AudioManager");
        audioManager?.PlayComboSound(); // Reuse combo sound for now

        // Show loot drop animation/UI
        ShowLootDropCelebration(cosmeticId);
    }

    /// <summary>
    /// Shows the loot drop celebration UI.
    /// </summary>
    private void ShowLootDropCelebration(string cosmeticId)
    {
        // Create floating cosmetic icon with particles
        var celebration = CreateLootDropCelebration(cosmeticId);
        if (celebration != null)
        {
            GetTree().CurrentScene.AddChild(celebration);
            
            // Auto-remove after animation
            var timer = new Timer();
            timer.WaitTime = 4.0;
            timer.OneShot = true;
            timer.Timeout += () => celebration.QueueFree();
            AddChild(timer);
            timer.Start();
        }
    }

    /// <summary>
    /// Creates a visual celebration for the earned cosmetic.
    /// </summary>
    private Node CreateLootDropCelebration(string cosmeticId)
    {
        // This would be implemented with actual UI elements
        // For now, just log the celebration
        GD.Print($"🎉 CELEBRATION: You earned {cosmeticId}!");
        
        // TODO: Create actual UI with:
        // - Floating cosmetic icon
        // - Particle effects
        // - "You earned [Cosmetic]!" text
        // - Animation scaling up and fading out
        
        return null; // Placeholder
    }

    /// <summary>
    /// Gets the type category of a cosmetic ID.
    /// </summary>
    private string GetCosmeticType(string cosmeticId)
    {
        return cosmeticId switch
        {
            "cap" or "crown" or "beanie" or "tophat" or "cowboy" or "beret" => "hat",
            "round" or "aviator" or "sunglasses" or "nerd_glasses" or "monocle" or "3d_glasses" => "glasses",
            "happy" or "angry" or "sad" or "excited" or "surprised" => "emotion",
            "normal" or "fancy" or "handlebar" or "pencil" or "walrus" => "moustache",
            "afro" or "long_hair" or "ponytail" or "mohawk" => "wig",
            "golden_slingshot" or "rainbow_slingshot" => "slingshot_skin",
            "rainbow_projectile" or "fire_projectile" => "projectile_skin",
            "sparkle_trail" or "smoke_trail" => "trail_effect",
            "explosion_hit" or "flash_hit" => "hit_effect",
            "victory_fireworks" or "confetti_victory" => "victory_effect",
            _ => "cosmetic"
        };
    }

    #endregion

    #region Persistence

    private const string DropHistoryPath = "user://cosmetic_drop_history.json";

    private void LoadDropHistory()
    {
        try
        {
            if (!FileAccess.FileExists(DropHistoryPath))
                return;

            using var file = FileAccess.Open(DropHistoryPath, FileAccess.ModeFlags.Read);
            if (file != null)
            {
                var json = file.GetAsText();
                if (!string.IsNullOrEmpty(json))
                {
                    ApplyDropHistoryJson(Json.ParseString(json));
                }
            }
        }
        catch (Exception ex)
        {
            GD.PushWarning($"Failed to load drop history: {ex.Message}");
        }
    }

    private void SaveDropHistory()
    {
        try
        {
            var history = BuildDropHistoryJson();
            using var file = FileAccess.Open(DropHistoryPath, FileAccess.ModeFlags.Write);
            if (file != null)
            {
                file.StoreString(Json.Stringify(history));
            }
        }
        catch (Exception ex)
        {
            GD.PushWarning($"Failed to save drop history: {ex.Message}");
        }
    }

    private Dictionary<string, Variant> BuildDropHistoryJson()
    {
        var history = new Dictionary<string, Variant>();
        history["perfect_scores_since_last_drop"] = PerfectScoresSinceLastDrop;
        history["total_perfect_scores"] = TotalPerfectScores;
        history["last_earned_cosmetic"] = LastEarnedCosmetic;
        history["last_drop_time"] = LastDropTime.ToBinary();
        return history;
    }

    private void ApplyDropHistoryJson(Variant jsonData)
    {
        if (jsonData.VariantType != Variant.Type.Dictionary)
            return;

        var history = jsonData.AsDictionary<string, Variant>();

        if (history.TryGetValue("perfect_scores_since_last_drop", out var scoresSince))
            PerfectScoresSinceLastDrop = scoresSince.AsInt32();
            
        if (history.TryGetValue("total_perfect_scores", out var totalScores))
            TotalPerfectScores = totalScores.AsInt32();
            
        if (history.TryGetValue("last_earned_cosmetic", out var lastCosmetic))
            LastEarnedCosmetic = lastCosmetic.AsString();
            
        if (history.TryGetValue("last_drop_time", out var lastTime))
            LastDropTime = DateTime.FromBinary(lastTime.AsInt64());

        _perfectScoresSinceLastDrop = PerfectScoresSinceLastDrop;
        _totalPerfectScores = TotalPerfectScores;
        _lastDropTime = LastDropTime;
    }

    #endregion

    #region Utility Methods

    /// <summary>
    /// Manually awards a cosmetic (for testing or admin commands).
    /// </summary>
    public void ForceAwardCosmetic(string cosmeticId, int levelNumber = 0)
    {
        if (PlayerProfile.Instance != null)
        {
            PlayerProfile.Instance.UnlockCosmetic(cosmeticId);
        }

        LastEarnedCosmetic = cosmeticId;
        LastDropTime = DateTime.Now;
        PerfectScoresSinceLastDrop = 0;
        _perfectScoresSinceLastDrop = 0;

        GD.Print($"🛠️ FORCE AWARD: {cosmeticId} (Level {levelNumber})");
        EmitSignal(SignalName.CosmeticEarned, cosmeticId, GetCosmeticType(cosmeticId));
    }

    /// <summary>
    /// Resets all drop history (for testing).
    /// </summary>
    public void ResetDropHistory()
    {
        PerfectScoresSinceLastDrop = 0;
        TotalPerfectScores = 0;
        LastEarnedCosmetic = "";
        LastDropTime = DateTime.MinValue;
        
        _perfectScoresSinceLastDrop = 0;
        _totalPerfectScores = 0;
        _lastDropTime = DateTime.MinValue;

        SaveDropHistory();
        GD.Print("Drop history reset");
    }

    /// <summary>
    /// Gets the current drop chance percentage for UI display.
    /// </summary>
    public float GetCurrentDropChancePercentage()
    {
        return CalculateDropChance() * 100.0f;
    }

    /// <summary>
    /// Checks if a specific cosmetic type should have increased drop rate.
    /// </summary>
    public bool ShouldBoostCosmeticTypeDropRate(string cosmeticType)
    {
        // Boost rare types if player hasn't gotten them recently
        if (cosmeticType == "wig" || cosmeticType == "moustache" || cosmeticType == "special_effects")
        {
            return _perfectScoresSinceLastDrop >= 10;
        }
        return false;
    }

    #endregion

    #region Helper Classes

    private class WeightedCosmetic
    {
        public string CosmeticId { get; set; } = "";
        public string Type { get; set; } = "";
        public float Weight { get; set; } = 1.0f;
    }

    #endregion
}
