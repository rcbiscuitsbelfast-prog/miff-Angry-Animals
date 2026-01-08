using Godot;
using System.Collections.Generic;
using System.IO;

/// <summary>
/// Handles local storage of custom level drafts.
/// </summary>
public static class LocalLevelStorage
{
    private const string SAVE_DIR = "user://custom_levels/";

    static LocalLevelStorage()
    {
        EnsureDirectoryExists();
    }

    private static void EnsureDirectoryExists()
    {
        if (!DirAccess.DirExistsAbsolute(SAVE_DIR))
        {
            DirAccess.MakeDirAbsolute(SAVE_DIR);
        }
    }

    /// <summary>
    /// Saves a level draft to local storage.
    /// </summary>
    public static bool SaveDraft(CustomLevelData level)
    {
        if (level == null)
        {
            GD.PrintErr("Cannot save null level");
            return false;
        }

        try
        {
            EnsureDirectoryExists();

            string fileName = SanitizeFileName(level.LevelName);
            string filePath = SAVE_DIR + fileName + ".json";
            
            string json = level.ToJson();
            
            using var file = FileAccess.Open(filePath, FileAccess.ModeFlags.Write);
            if (file == null)
            {
                GD.PrintErr($"Failed to open file for writing: {filePath}");
                return false;
            }

            file.StoreString(json);
            GD.Print($"Level draft saved: {filePath}");
            return true;
        }
        catch (System.Exception ex)
        {
            GD.PrintErr($"Failed to save level draft: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Loads all level drafts from local storage.
    /// </summary>
    public static List<CustomLevelData> LoadDrafts()
    {
        var drafts = new List<CustomLevelData>();

        try
        {
            EnsureDirectoryExists();

            var dir = DirAccess.Open(SAVE_DIR);
            if (dir == null)
            {
                GD.PrintErr($"Failed to open directory: {SAVE_DIR}");
                return drafts;
            }

            dir.ListDirBegin();
            string fileName = dir.GetNext();

            while (!string.IsNullOrEmpty(fileName))
            {
                if (!dir.CurrentIsDir() && fileName.EndsWith(".json"))
                {
                    string filePath = SAVE_DIR + fileName;
                    var level = LoadDraft(filePath);
                    if (level != null)
                    {
                        drafts.Add(level);
                    }
                }
                fileName = dir.GetNext();
            }

            dir.ListDirEnd();
            GD.Print($"Loaded {drafts.Count} level drafts");
        }
        catch (System.Exception ex)
        {
            GD.PrintErr($"Failed to load level drafts: {ex.Message}");
        }

        return drafts;
    }

    /// <summary>
    /// Loads a specific level draft by file path.
    /// </summary>
    public static CustomLevelData LoadDraft(string filePath)
    {
        try
        {
            using var file = FileAccess.Open(filePath, FileAccess.ModeFlags.Read);
            if (file == null)
            {
                GD.PrintErr($"Failed to open file for reading: {filePath}");
                return null;
            }

            string json = file.GetAsText();
            return CustomLevelData.FromJson(json);
        }
        catch (System.Exception ex)
        {
            GD.PrintErr($"Failed to load level draft from {filePath}: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Deletes a level draft from local storage.
    /// </summary>
    public static bool DeleteDraft(string levelName)
    {
        try
        {
            string fileName = SanitizeFileName(levelName);
            string filePath = SAVE_DIR + fileName + ".json";

            if (FileAccess.FileExists(filePath))
            {
                DirAccess.RemoveAbsolute(filePath);
                GD.Print($"Level draft deleted: {filePath}");
                return true;
            }

            return false;
        }
        catch (System.Exception ex)
        {
            GD.PrintErr($"Failed to delete level draft: {ex.Message}");
            return false;
        }
    }

    private static string SanitizeFileName(string fileName)
    {
        // Remove invalid filename characters
        char[] invalidChars = Path.GetInvalidFileNameChars();
        string sanitized = fileName;
        
        foreach (char c in invalidChars)
        {
            sanitized = sanitized.Replace(c.ToString(), "_");
        }

        // Limit length
        if (sanitized.Length > 50)
        {
            sanitized = sanitized.Substring(0, 50);
        }

        return sanitized;
    }
}
