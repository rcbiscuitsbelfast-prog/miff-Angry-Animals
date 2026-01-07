using Godot;
using Godot.Collections;

/// <summary>
/// Utility for layering cosmetic overlay sprites (moustache, hair, glasses, etc.)
/// over a base face sprite.
/// </summary>
public static class CosmeticOverlay
{
    public static void ApplyOverlays(Sprite2D faceSprite, Array<string> overlayIds)
    {
        if (faceSprite == null)
            return;

        RemoveExistingOverlays(faceSprite);

        foreach (var overlayId in overlayIds)
        {
            if (string.IsNullOrWhiteSpace(overlayId))
                continue;

            var texture = ResolveOverlayTexture(overlayId);
            if (texture == null)
                continue;

            var overlaySprite = new Sprite2D
            {
                Name = $"Overlay_{overlayId}",
                Texture = texture,
                ZIndex = faceSprite.ZIndex + 1,
                Position = Vector2.Zero
            };

            faceSprite.AddChild(overlaySprite);
        }
    }

    private static void RemoveExistingOverlays(Sprite2D faceSprite)
    {
        foreach (Node child in faceSprite.GetChildren())
        {
            if (child.Name.ToString().StartsWith("Overlay_"))
                child.QueueFree();
        }
    }

    private static Texture2D? ResolveOverlayTexture(string overlayId)
    {
        // Convention: overlay textures live in res://Assets/Cosmetics/Overlays/
        var path = $"res://Assets/Cosmetics/Overlays/{overlayId}.png";
        if (!ResourceLoader.Exists(path))
            return null;

        return ResourceLoader.Load<Texture2D>(path);
    }
}
