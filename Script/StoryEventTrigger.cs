using Godot;

/// <summary>
/// Orchestrates story beats like chapter intro cutscenes.
/// Designed to be used as an AutoLoad and queried by GameManager before scene changes.
/// </summary>
public partial class StoryEventTrigger : Node
{
    public static StoryEventTrigger Instance { get; private set; } = null!;

    public string PendingCutsceneScenePath { get; private set; } = string.Empty;
    public int PendingReturnRoomIndex { get; private set; } = -1;

    public override void _Ready()
    {
        Instance = this;
        ProcessMode = ProcessModeEnum.Always;
    }

    public void ClearPending()
    {
        PendingCutsceneScenePath = string.Empty;
        PendingReturnRoomIndex = -1;
    }

    public bool TryQueueChapterStartCutscene(int roomIndex)
    {
        var cutscenePath = GetChapterStartCutscenePath(roomIndex);
        if (string.IsNullOrWhiteSpace(cutscenePath))
            return false;

        if (PlayerProfile.Instance != null)
        {
            var id = $"cutscene:{roomIndex}";
            if (PlayerProfile.Instance.HasSeenStoryFlag(id))
                return false;

            PlayerProfile.Instance.MarkStoryFlagSeen(id);
        }

        PendingCutsceneScenePath = cutscenePath;
        PendingReturnRoomIndex = roomIndex;
        return true;
    }

    private static string GetChapterStartCutscenePath(int roomIndex)
    {
        // Indices are 0-based room indices.
        return roomIndex switch
        {
            0 => "res://Scenes/Cutscenes/BedroomIncident.tscn",
            6 => "res://Scenes/Cutscenes/Chapter2Intro.tscn",
            26 => "res://Scenes/Cutscenes/Chapter3Intro.tscn",
            76 => "res://Scenes/Cutscenes/Chapter4Intro.tscn",
            96 => "res://Scenes/Cutscenes/Chapter5Intro.tscn",
            _ => string.Empty
        };
    }
}
