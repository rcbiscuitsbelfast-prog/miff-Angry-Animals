using System;
using System.Threading.Tasks;
using Godot;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

/// <summary>
/// Global manager for handling the player's profile, including customization and progression.
/// Persists data to a JSON file on disk.
/// </summary>
public partial class PlayerProfile : Node
{
    /// <summary>
    /// Singleton instance of the PlayerProfile.
    /// </summary>
    public static PlayerProfile Instance { get; private set; } = null!;

    private const string ProfilePath = "user://profile.json";

    private static readonly string[] DefaultHats =
    [
        "none",
        "cap",
        "crown",
        "beanie"
    ];

    private static readonly string[] DefaultGlasses =
    [
        "none",
        "round",
        "aviator"
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

    /// <summary>
    /// The player's display name.
    /// </summary>
    public string PlayerName { get; private set; } = "Player";

    /// <summary>
    /// Whether the full game has been unlocked via in-app purchase.
    /// </summary>
    public bool IsFullGameUnlocked { get; set; } = false;

    /// <summary>
    /// Index of the currently selected hat.
    /// </summary>
    public int SelectedHatIndex { get; private set; }

    /// <summary>
    /// Index of the currently selected glasses.
    /// </summary>
    public int SelectedGlassesIndex { get; private set; }

    /// <summary>
    /// Index of the currently selected filter.
    /// </summary>
    public int SelectedFilterIndex { get; private set; }

    /// <summary>
    /// Index of the currently selected emotion.
    /// </summary>
    public int SelectedEmotionIndex { get; private set; }

    /// <summary>
    /// Path to a custom face image, if set.
    /// </summary>
    public string FaceImagePath { get; private set; } = "";

    /// <summary>
    /// The index of the highest room the player has unlocked.
    /// </summary>
    public int HighestUnlockedRoomIndex { get; private set; }

    /// <summary>
    /// The current rage level from the global RageSystem.
    /// </summary>
    public float CurrentRage { get; private set; }

    /// <summary>
    /// The current combo multiplier from the global RageSystem.
    /// </summary>
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
    public static string[] GetFilters() => DefaultFilters;
    public static string[] GetEmotions() => DefaultEmotions;

    public static void SetCosmetics(int hatIndex, int glassesIndex, int filterIndex, int emotionIndex)
    {
        Instance.SelectedHatIndex = Mathf.Clamp(hatIndex, 0, DefaultHats.Length - 1);
        Instance.SelectedGlassesIndex = Mathf.Clamp(glassesIndex, 0, DefaultGlasses.Length - 1);
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

    public void Save()
    {
        var root = new JObject
        {
            ["version"] = 2,
            ["profile_name"] = PlayerName,
            ["is_full_game_unlocked"] = IsFullGameUnlocked,
            ["face_image_path"] = FaceImagePath,
            ["highest_unlocked_room_index"] = HighestUnlockedRoomIndex,
            ["cosmetics"] = new JObject
            {
                ["hat_index"] = SelectedHatIndex,
                ["glasses_index"] = SelectedGlassesIndex,
                ["filter_index"] = SelectedFilterIndex,
                ["emotion_index"] = SelectedEmotionIndex
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

            FaceImagePath = ReadString(root, "face_image_path")
                ?? ReadString(root, "FaceImagePath")
                ?? "";

            HighestUnlockedRoomIndex = Math.Max(0,
                ReadInt(root, "highest_unlocked_room_index")
                ?? ReadInt(root, "HighestUnlockedRoomIndex")
                ?? 0);

            var cosmeticsToken = root["cosmetics"];
            if (cosmeticsToken is JObject cosmetics)
            {
                SelectedHatIndex = Mathf.Clamp(ReadInt(cosmetics, "hat_index") ?? 0, 0, DefaultHats.Length - 1);
                SelectedGlassesIndex = Mathf.Clamp(ReadInt(cosmetics, "glasses_index") ?? 0, 0, DefaultGlasses.Length - 1);
                SelectedFilterIndex = Mathf.Clamp(ReadInt(cosmetics, "filter_index") ?? 0, 0, DefaultFilters.Length - 1);
                SelectedEmotionIndex = Mathf.Clamp(ReadInt(cosmetics, "emotion_index") ?? 0, 0, DefaultEmotions.Length - 1);
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
