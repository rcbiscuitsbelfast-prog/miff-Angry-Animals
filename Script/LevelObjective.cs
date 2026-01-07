using Godot;

/// <summary>
/// Inspector-configurable objective definition.
/// RoomBase uses these to track completion progress.
/// </summary>
[GlobalClass]
public partial class LevelObjective : Resource
{
    public enum ObjectiveType
    {
        DestroyXCups,
        DestroySpecificNpcs,
        CageOrContainNpcs,
        KnockNpcIntoHazard,
        ReachExit,
        CollectItems
    }

    [Export] public ObjectiveType Type { get; set; } = ObjectiveType.DestroyXCups;

    /// <summary>
    /// For NPC objectives, this can be a node name or a group tag.
    /// For CollectItems, this can be an item id or group.
    /// </summary>
    [Export] public string Target { get; set; } = string.Empty;

    /// <summary>
    /// Target count for count-based objectives.
    /// </summary>
    [Export] public int Count { get; set; } = 0;

    [Export(PropertyHint.MultilineText)] public string OverrideText { get; set; } = string.Empty;

    public string GetDisplayText(int progressCount)
    {
        if (!string.IsNullOrWhiteSpace(OverrideText))
            return OverrideText;

        return Type switch
        {
            ObjectiveType.DestroyXCups => Count > 0 ? $"Destroy {progressCount}/{Count} cups" : $"Destroy cups ({progressCount})",
            ObjectiveType.DestroySpecificNpcs => string.IsNullOrWhiteSpace(Target) ? "Destroy the target NPC" : $"Destroy: {Target}",
            ObjectiveType.CageOrContainNpcs => string.IsNullOrWhiteSpace(Target) ? "Cage/contain NPCs" : $"Cage: {Target}",
            ObjectiveType.KnockNpcIntoHazard => string.IsNullOrWhiteSpace(Target) ? "Knock NPC into hazard" : $"Hazard: {Target}",
            ObjectiveType.ReachExit => "Reach the exit",
            ObjectiveType.CollectItems => Count > 0 ? $"Collect {progressCount}/{Count} items" : "Collect items",
            _ => "Objective"
        };
    }
}
