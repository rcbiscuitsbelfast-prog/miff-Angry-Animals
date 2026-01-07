using System.Threading.Tasks;
using Godot;

/// <summary>
/// Simple timeline implementation for cutscene scenes.
/// Attach this script to a cutscene .tscn and configure the dialogue arrays in the Inspector.
/// </summary>
public partial class CutsceneScene : Node2D, ICutscene
{
    [ExportGroup("Dialogue")]
    [Export] public PackedStringArray Speakers { get; set; } = new();
    [Export(PropertyHint.MultilineText)] public PackedStringArray Lines { get; set; } = new();
    [Export] public PackedFloat32Array Durations { get; set; } = new();
    [Export] public PackedStringArray PortraitVariants { get; set; } = new();

    [ExportGroup("Animation")]
    [Export] public NodePath AnimatedSpritePath { get; set; }
    [Export] public PackedStringArray SpriteAnimations { get; set; } = new();

    [ExportGroup("Fallback")]
    [Export] public float FallbackDuration { get; set; } = 2.5f;

    public async Task PlayAsync(CutscenePlayer player)
    {
        var sprite = GetNodeOrNull<AnimatedSprite2D>(AnimatedSpritePath);

        int count = Lines.Length;
        if (count == 0)
        {
            await ToSignal(GetTree().CreateTimer(FallbackDuration, true, false, false), SceneTreeTimer.SignalName.Timeout);
            return;
        }

        for (int i = 0; i < count; i++)
        {
            if (player.IsSkipping)
                return;

            if (sprite != null && i < SpriteAnimations.Length && !string.IsNullOrWhiteSpace(SpriteAnimations[i]))
            {
                if (sprite.SpriteFrames != null && sprite.SpriteFrames.HasAnimation(SpriteAnimations[i]))
                    sprite.Play(SpriteAnimations[i]);
            }

            var speaker = i < Speakers.Length ? Speakers[i] : string.Empty;
            var text = Lines[i];
            var duration = i < Durations.Length ? Durations[i] : 2.0f;
            var portrait = i < PortraitVariants.Length ? PortraitVariants[i] : string.Empty;

            DialogueManager.Instance?.EnqueueDialogue(speaker, text, duration, portrait);

            // Wait for the line duration (DialogueManager handles fade and click-to-advance).
            // We add a small guard timer so the cutscene progresses even if DialogueManager isn't present.
            if (DialogueManager.Instance == null)
                await ToSignal(GetTree().CreateTimer(duration, true, false, false), SceneTreeTimer.SignalName.Timeout);
            else
                await ToSignal(GetTree().CreateTimer(duration + 0.01f, true, false, false), SceneTreeTimer.SignalName.Timeout);
        }
    }
}
