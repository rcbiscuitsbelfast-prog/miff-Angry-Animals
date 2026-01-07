using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;

public partial class PlayerProfile : Node
{
    public static PlayerProfile Instance { get; private set; } = null!;

    private const string ProfilePath = "user://profile.json";

    private static readonly string[] DefaultHats =
    [
        "none",
        "cap",
        "crown",
        "beanie",
        "tophat",
        "cowboy",
        "beret"
    ];

    private static readonly string[] DefaultGlasses =
    [
        "none",
        "round",
        "aviator",
        "sunglasses",
        "nerd_glasses",
        "monocle",
        "3d_glasses"
    ];

    private static readonly string[] DefaultMoustaches =
    [
        "none",
        "normal",
        "fancy",
        "handlebar",
        "pencil",
        "walrus"
    ];

    private static readonly string[] DefaultWigs =
    [
        "none",
        "afro",
        "long_hair",
        "ponytail",
        "mohawk"
    ];

    private static readonly string[] DefaultFilters =
    [
        "none",
        "sepia",
        "bw"
    ];

    private static readonly string[] DefaultEmotions =
    [
        "neutral",
        "happy",
        "angry",
        "sad"
    ];

    public string PlayerName { get; private set; } = "Player";

    /// <summary>
    /// Whether the full game has been unlocked via in-app purchase.
    /// </summary>
    public bool IsFullGameUnlocked { get; set; } = false;

    /// <summary>
    /// Whether to use procedurally generated levels instead of manually designed ones.
    /// </summary>
    public bool UseProceduralLevels { get; set; } = false;

    /// <summary>
    /// The last procedural seed used (for quick replay/sharing).
    /// </summary>
    public int LastProceduralSeed { get; set; }

    /// <summary>
    /// The last level number used with procedural generation.
    /// </summary>
    public int LastProceduralLevelNumber { get; set; } = 1;

    public int SelectedHatIndex { get; private set; }
    public int SelectedGlassesIndex { get; private set; }
    public int SelectedMoustacheIndex { get; private set; }
    public int SelectedWigIndex { get; private set; }
    public int SelectedFilterIndex { get; private set; }
    public int SelectedEmotionIndex { get; private set; }

    // New Cosmetics
    public int SelectedSlingshotSkinIndex { get; set; }
    public int SelectedProjectileSkinIndex { get; set; }
    public int SelectedTrailEffectIndex { get; set; }
    public int SelectedHitEffectIndex { get; set; }
    public int SelectedVictoryEffectIndex { get; set; }

    /// <summary>
    /// The selected slingshot type (Catapult, GiantHand, Trebuchet, Spring).
    /// Defaults to Catapult.
    /// </summary>
    public int SelectedSlingshotType { get; set; } = 0;

    // Unlocked Cosmetics (via IAP)
    public HashSet<string> UnlockedCosmetics { get; private set; } = new();

    // Accessibility
    public bool ColorblindMode { get; set; } = false;
    public float TextScale { get; set; } = 1.0f;
    public bool HighContrastMode { get; set; } = false;
    public bool ReduceMotion { get; set; } = false;
    public string DifficultyPreset { get; set; } = "Normal";

    public string FaceImagePath { get; private set; } = "";

    public int HighestUnlockedRoomIndex { get; private set; }

    public int HighestUnlockedChapterIndex { get; private set; }
    private HashSet<int> _completedChapters = new();
    private HashSet<string> _storyFlagsSeen = new();

    public float CurrentRage { get; private set; }
    public int CurrentCombo { get; private set; }

    private RageSystem? _rageSystem;

    public override void _Ready()
    {
        Instance = this;
        ProcessMode = ProcessModeEnum.Always;

        Load();
        CallDeferred(nameof(ConnectRageSystem));
    }

    public override void _ExitTree()
    {
        Save();

        if (_rageSystem != null)
        {
            _rageSystem.RageChanged -= OnRageChanged;
            _rageSystem.ComboChanged -= OnComboChanged;
            _rageSystem = null;
        }
    }

    private void ConnectRageSystem()
    {
        if (_rageSystem != null)
            return;

        _rageSystem = GetNodeOrNull<RageSystem>("/root/RageSystem");
        if (_rageSystem == null)
            return;

        _rageSystem.RageChanged += OnRageChanged;
        _rageSystem.ComboChanged += OnComboChanged;
    }

    private void OnRageChanged(float value) => CurrentRage = value;
    private void OnComboChanged(int value) => CurrentCombo = value;

    public static string[] GetHats() => DefaultHats;
    public static string[] GetGlasses() => DefaultGlasses;
    public static string[] GetMoustaches() => DefaultMoustaches;
    public static string[] GetWigs() => DefaultWigs;
    public static string[] GetFilters() => DefaultFilters;
    public static string[] GetEmotions() => DefaultEmotions;

    public static void SetCosmetics(int hatIndex, int glassesIndex, int moustacheIndex, int wigIndex, int filterIndex, int emotionIndex)
    {
        Instance.SelectedHatIndex = Mathf.Clamp(hatIndex, 0, DefaultHats.Length - 1);
        Instance.SelectedGlassesIndex = Mathf.Clamp(glassesIndex, 0, DefaultGlasses.Length - 1);
        Instance.SelectedMoustacheIndex = Mathf.Clamp(moustacheIndex, 0, DefaultMoustaches.Length - 1);
        Instance.SelectedWigIndex = Mathf.Clamp(wigIndex, 0, DefaultWigs.Length - 1);
        Instance.SelectedFilterIndex = Mathf.Clamp(filterIndex, 0, DefaultFilters.Length - 1);
        Instance.SelectedEmotionIndex = Mathf.Clamp(emotionIndex, 0, DefaultEmotions.Length - 1);
        Instance.Save();
    }

    public static void SetPlayerName(string name)
    {
        Instance.PlayerName = string.IsNullOrWhiteSpace(name) ? "Player" : name.Trim();
        Instance.Save();
    }

    public static void UnlockRoom(int roomIndex)
    {
        if (roomIndex <= Instance.HighestUnlockedRoomIndex)
            return;

        Instance.HighestUnlockedRoomIndex = roomIndex;
        Instance.Save();
    }

    public static bool IsRoomUnlocked(int roomIndex) => roomIndex <= Instance.HighestUnlockedRoomIndex;

    public void UnlockChapter(int chapterIndex)
    {
        if (chapterIndex <= HighestUnlockedChapterIndex)
            return;

        HighestUnlockedChapterIndex = chapterIndex;
    }

    public void MarkChapterCompleted(int chapterIndex)
    {
        _completedChapters.Add(chapterIndex);
    }

    public bool IsChapterCompleted(int chapterIndex) => _completedChapters.Contains(chapterIndex);

    public bool HasSeenStoryFlag(string flagId) => _storyFlagsSeen.Contains(flagId);

    public void MarkStoryFlagSeen(string flagId)
    {
        if (string.IsNullOrWhiteSpace(flagId))
            return;

        _storyFlagsSeen.Add(flagId);
        Save();
    }

    public void UnlockCosmetic(string cosmeticId)
    {
        if (string.IsNullOrWhiteSpace(cosmeticId))
            return;

        if (UnlockedCosmetics.Add(cosmeticId))
            Save();
    }

    public void Save()
    {
        var root = new JObject
        {
            ["version"] = 3,
            ["profile_name"] = PlayerName,
            ["is_full_game_unlocked"] = IsFullGameUnlocked,
            ["use_procedural_levels"] = UseProceduralLevels,
            ["last_procedural_seed"] = LastProceduralSeed,
            ["last_procedural_level_number"] = LastProceduralLevelNumber,
            ["face_image_path"] = FaceImagePath,
            ["highest_unlocked_room_index"] = HighestUnlockedRoomIndex,
            ["story"] = new JObject
            {
                ["highest_unlocked_chapter_index"] = HighestUnlockedChapterIndex,
                ["completed_chapters"] = JArray.FromObject(_completedChapters.ToList()),
                ["seen_flags"] = JArray.FromObject(_storyFlagsSeen.ToList())
            },
            ["accessibility"] = new JObject
            {
                ["colorblind_mode"] = ColorblindMode,
                ["text_scale"] = TextScale,
                ["high_contrast_mode"] = HighContrastMode,
                ["reduce_motion"] = ReduceMotion,
                ["difficulty_preset"] = DifficultyPreset
            },
            ["cosmetics"] = new JObject
            {
                ["hat_index"] = SelectedHatIndex,
                ["glasses_index"] = SelectedGlassesIndex,
                ["moustache_index"] = SelectedMoustacheIndex,
                ["wig_index"] = SelectedWigIndex,
                ["filter_index"] = SelectedFilterIndex,
                ["emotion_index"] = SelectedEmotionIndex,
                ["slingshot_skin_index"] = SelectedSlingshotSkinIndex,
                ["projectile_skin_index"] = SelectedProjectileSkinIndex,
                ["trail_effect_index"] = SelectedTrailEffectIndex,
                ["hit_effect_index"] = SelectedHitEffectIndex,
                ["victory_effect_index"] = SelectedVictoryEffectIndex,
                ["slingshot_type"] = SelectedSlingshotType,
                ["unlocked_list"] = JArray.FromObject(UnlockedCosmetics.ToList())
            }
        };

        try
        {
            using var file = FileAccess.Open(ProfilePath, FileAccess.ModeFlags.Write);
            file?.StoreString(JsonConvert.SerializeObject(root, Formatting.Indented));
        }
        catch (Exception ex)
        {
            GD.PushWarning($"Failed to save profile: {ex.Message}");
        }
    }

    public void Load()
    {
        try
        {
            if (!FileAccess.FileExists(ProfilePath))
            {
                HighestUnlockedRoomIndex = 0;
                HighestUnlockedChapterIndex = 0;
                _completedChapters = new HashSet<int>();
                _storyFlagsSeen = new HashSet<string>();
                Save();
                return;
            }

            using var file = FileAccess.Open(ProfilePath, FileAccess.ModeFlags.Read);
            var json = file?.GetAsText() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(json))
                return;

            var root = JObject.Parse(json);

            PlayerName = ReadString(root, "profile_name")
                ?? ReadString(root, "PlayerName")
                ?? "Player";

            IsFullGameUnlocked = ReadBool(root, "is_full_game_unlocked")
                ?? ReadBool(root, "IsFullGameUnlocked")
                ?? false;

            UseProceduralLevels = ReadBool(root, "use_procedural_levels")
                ?? ReadBool(root, "UseProceduralLevels")
                ?? false;

            LastProceduralSeed = ReadInt(root, "last_procedural_seed")
                ?? ReadInt(root, "LastProceduralSeed")
                ?? 0;

            LastProceduralLevelNumber = ReadInt(root, "last_procedural_level_number")
                ?? ReadInt(root, "LastProceduralLevelNumber")
                ?? 1;

            FaceImagePath = ReadString(root, "face_image_path")
                ?? ReadString(root, "FaceImagePath")
                ?? "";

            HighestUnlockedRoomIndex = Math.Max(0,
                ReadInt(root, "highest_unlocked_room_index")
                ?? ReadInt(root, "HighestUnlockedRoomIndex")
                ?? 0);

            var storyToken = root["story"];
            if (storyToken is JObject story)
            {
                HighestUnlockedChapterIndex = Math.Max(0, ReadInt(story, "highest_unlocked_chapter_index") ?? 0);

                var completedToken = story["completed_chapters"];
                if (completedToken is JArray completed)
                    _completedChapters = new HashSet<int>(completed.Select(t => (int)(t.Type == JTokenType.Integer ? t.Value<int>() : int.TryParse(t.ToString(), out var v) ? v : 0)));

                var flagsToken = story["seen_flags"];
                if (flagsToken is JArray flags)
                    _storyFlagsSeen = new HashSet<string>(flags.Select(t => t.ToString()));
            }
            else
            {
                // Back-compat: derive chapter unlock from level progression.
                HighestUnlockedChapterIndex = StoryData.GetChapterIndexForRoomIndex(HighestUnlockedRoomIndex);
                _completedChapters = new HashSet<int>();
                _storyFlagsSeen = new HashSet<string>();
            }

            var accessibilityToken = root["accessibility"];
            if (accessibilityToken is JObject accessibility)
            {
                ColorblindMode = ReadBool(accessibility, "colorblind_mode") ?? false;
                TextScale = (float)(ReadDouble(accessibility, "text_scale") ?? 1.0);
                HighContrastMode = ReadBool(accessibility, "high_contrast_mode") ?? false;
                ReduceMotion = ReadBool(accessibility, "reduce_motion") ?? false;
                DifficultyPreset = ReadString(accessibility, "difficulty_preset") ?? "Normal";
            }

            var cosmeticsToken = root["cosmetics"];
            if (cosmeticsToken is JObject cosmetics)
            {
                SelectedHatIndex = Mathf.Clamp(ReadInt(cosmetics, "hat_index") ?? 0, 0, DefaultHats.Length - 1);
                SelectedGlassesIndex = Mathf.Clamp(ReadInt(cosmetics, "glasses_index") ?? 0, 0, DefaultGlasses.Length - 1);
                SelectedFilterIndex = Mathf.Clamp(ReadInt(cosmetics, "filter_index") ?? 0, 0, DefaultFilters.Length - 1);
                SelectedEmotionIndex = Mathf.Clamp(ReadInt(cosmetics, "emotion_index") ?? 0, 0, DefaultEmotions.Length - 1);
                SelectedSlingshotSkinIndex = ReadInt(cosmetics, "slingshot_skin_index") ?? 0;
                SelectedProjectileSkinIndex = ReadInt(cosmetics, "projectile_skin_index") ?? 0;
                SelectedTrailEffectIndex = ReadInt(cosmetics, "trail_effect_index") ?? 0;
                SelectedHitEffectIndex = ReadInt(cosmetics, "hit_effect_index") ?? 0;
                SelectedVictoryEffectIndex = ReadInt(cosmetics, "victory_effect_index") ?? 0;
                SelectedSlingshotType = ReadInt(cosmetics, "slingshot_type") ?? 0;
                
                var unlockedToken = cosmetics["unlocked_list"];
                if (unlockedToken is JArray unlockedList)
                {
                    UnlockedCosmetics = new HashSet<string>(unlockedList.Select(t => t.ToString()));
                }
            }
            else
            {
                SelectedHatIndex = Mathf.Clamp(ReadInt(root, "SelectedHatIndex") ?? 0, 0, DefaultHats.Length - 1);
                SelectedGlassesIndex = Mathf.Clamp(ReadInt(root, "SelectedGlassesIndex") ?? 0, 0, DefaultGlasses.Length - 1);
                SelectedFilterIndex = Mathf.Clamp(ReadInt(root, "SelectedFilterIndex") ?? 0, 0, DefaultFilters.Length - 1);
                SelectedEmotionIndex = Mathf.Clamp(ReadInt(root, "SelectedEmotionIndex") ?? 0, 0, DefaultEmotions.Length - 1);
            }
        }
        catch (Exception ex)
        {
            GD.PushWarning($"Failed to load profile: {ex.Message}");
        }
    }

    public static Task<string?> CapturePhotoAsync()
    {
        // This will be handled by the UI layer, but we provide the API point.
        // In a real implementation, this might signal the UI to open the camera.
        GD.Print("CapturePhotoAsync called - Waiting for UI implementation");
        return Task.FromResult<string?>(null);
    }

    public static Task<string?> SelectFromGalleryAsync()
    {
        GD.Print("SelectFromGalleryAsync called - Waiting for UI implementation");
        return Task.FromResult<string?>(null);
    }

    public static void SetFaceImage(string path)
    {
        Instance.FaceImagePath = path ?? "";
        Instance.Save();
    }

    public static void SaveCosmetics()
    {
        Instance.Save();
    }

    public static void SetProceduralMode(bool enabled)
    {
        Instance.UseProceduralLevels = enabled;
        Instance.Save();
    }

    public static void SetSlingshotType(int typeIndex)
    {
        Instance.SelectedSlingshotType = Mathf.Clamp(typeIndex, 0, 3);
        Instance.Save();
    }

    public static int GetSlingshotType() => Instance.SelectedSlingshotType;

    private static string? ReadString(JObject root, string key)
    {
        if (!root.TryGetValue(key, out var token))
            return null;

        var value = token.Type == JTokenType.String ? token.Value<string>() : token.ToString();
        return string.IsNullOrWhiteSpace(value) ? null : value;
    }

    private static int? ReadInt(JObject root, string key)
    {
        if (!root.TryGetValue(key, out var token))
            return null;

        if (token.Type == JTokenType.Integer)
            return token.Value<int>();

        if (int.TryParse(token.ToString(), out int value))
            return value;

        return null;
    }

    private static double? ReadDouble(JObject root, string key)
    {
        if (!root.TryGetValue(key, out var token))
            return null;

        if (token.Type == JTokenType.Float || token.Type == JTokenType.Integer)
            return token.Value<double>();

        if (double.TryParse(token.ToString(), out double value))
            return value;

        return null;
    }

    private static bool? ReadBool(JObject root, string key)
    {
        if (!root.TryGetValue(key, out var token))
            return null;

        if (token.Type == JTokenType.Boolean)
            return token.Value<bool>();

        if (bool.TryParse(token.ToString(), out bool value))
            return value;

        return null;
    }
}
