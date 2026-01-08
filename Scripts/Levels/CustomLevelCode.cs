using Godot;
using System;
using System.Text;

/// <summary>
/// Handles encoding and decoding of custom levels to/from shareable base64 codes.
/// </summary>
public static class CustomLevelCode
{
    /// <summary>
    /// Encodes a custom level into a shareable base64 string.
    /// Format: AA1_[base64_encoded_json]
    /// </summary>
    public static string EncodeLevel(CustomLevelData level)
    {
        if (level == null)
        {
            GD.PrintErr("Cannot encode null level");
            return null;
        }

        try
        {
            return level.ToBase64();
        }
        catch (Exception ex)
        {
            GD.PrintErr($"Failed to encode level: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Attempts to decode a custom level from a base64 share code.
    /// </summary>
    public static bool TryDecodeLevel(string code, out CustomLevelData level)
    {
        level = null;

        if (string.IsNullOrWhiteSpace(code))
        {
            GD.PrintErr("Share code is empty");
            return false;
        }

        try
        {
            level = CustomLevelData.FromBase64(code.Trim());
            return level != null;
        }
        catch (Exception ex)
        {
            GD.PrintErr($"Failed to decode share code: {ex.Message}");
            return false;
        }
    }
}
