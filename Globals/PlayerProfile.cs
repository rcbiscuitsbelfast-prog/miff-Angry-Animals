using System;
using System.Threading.Tasks;
using Godot;
using Newtonsoft.Json;

public partial class PlayerProfile : Node
{
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

    public string PlayerName { get; private set; } = "Player";

    public int SelectedHatIndex { get; private set; }
    public int SelectedGlassesIndex { get; private set; }
    public int SelectedFilterIndex { get; private set; }
    public int SelectedEmotionIndex { get; private set; }

    public string FaceImagePath { get; private set; } = "";

    public int HighestUnlockedRoomIndex { get; private set; }

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
        var data = new SaveData
        {
            PlayerName = PlayerName,
            SelectedHatIndex = SelectedHatIndex,
            SelectedGlassesIndex = SelectedGlassesIndex,
            SelectedFilterIndex = SelectedFilterIndex,
            SelectedEmotionIndex = SelectedEmotionIndex,
            FaceImagePath = FaceImagePath,
            HighestUnlockedRoomIndex = HighestUnlockedRoomIndex
        };

        try
        {
            using var file = FileAccess.Open(ProfilePath, FileAccess.ModeFlags.Write);
            file?.StoreString(JsonConvert.SerializeObject(data, Formatting.Indented));
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

            var data = JsonConvert.DeserializeObject<SaveData>(json);
            if (data == null)
                return;

            PlayerName = string.IsNullOrWhiteSpace(data.PlayerName) ? "Player" : data.PlayerName;
            SelectedHatIndex = Mathf.Clamp(data.SelectedHatIndex, 0, DefaultHats.Length - 1);
            SelectedGlassesIndex = Mathf.Clamp(data.SelectedGlassesIndex, 0, DefaultGlasses.Length - 1);
            SelectedFilterIndex = Mathf.Clamp(data.SelectedFilterIndex, 0, DefaultFilters.Length - 1);
            SelectedEmotionIndex = Mathf.Clamp(data.SelectedEmotionIndex, 0, DefaultEmotions.Length - 1);
            FaceImagePath = data.FaceImagePath ?? "";
            HighestUnlockedRoomIndex = Math.Max(0, data.HighestUnlockedRoomIndex);
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

    private sealed class SaveData
    {
        public int Version { get; set; } = 1;
        public string PlayerName { get; set; } = "Player";
        public int SelectedHatIndex { get; set; }
        public int SelectedGlassesIndex { get; set; }
        public int SelectedFilterIndex { get; set; }
        public int SelectedEmotionIndex { get; set; }
        public string FaceImagePath { get; set; } = "";
        public int HighestUnlockedRoomIndex { get; set; }
    }
}
