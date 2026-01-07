using System;
using System.Collections.Generic;
using Godot;

/// <summary>
/// Static story metadata for Yeet Face.
/// Uses 0-based room indices (GameManager roomIndex) for ranges.
/// </summary>
public static class StoryData
{
    public readonly record struct ChapterInfo(
        int ChapterIndex,
        string Name,
        string Description,
        int StartRoomIndex,
        int EndRoomIndex,
        Color ThemeColor,
        string BackgroundTheme);

    public static readonly ChapterInfo[] Chapters =
    [
        new ChapterInfo(
            0,
            "Chapter 1: The Bedroom Incident",
            "Waking up angry, destroying room chaos",
            0,
            5,
            new Color(0.95f, 0.45f, 0.45f),
            "bedroom"),
        new ChapterInfo(
            1,
            "Chapter 2: House Takeover",
            "Spreading destruction through home",
            6,
            25,
            new Color(0.95f, 0.8f, 0.45f),
            "house"),
        new ChapterInfo(
            2,
            "Chapter 3: School Daze",
            "Mayhem at school, classmates in the blast zone",
            26,
            75,
            new Color(0.55f, 0.85f, 0.95f),
            "school"),
        new ChapterInfo(
            3,
            "Chapter 4: Government Chaos",
            "Destroying authority structures",
            76,
            95,
            new Color(0.7f, 0.7f, 0.75f),
            "government"),
        new ChapterInfo(
            4,
            "Chapter 5: Space Apocalypse",
            "Final boss: destroy Earth itself",
            96,
            99,
            new Color(0.6f, 0.55f, 0.95f),
            "space")
    ];

    private static readonly string[] LevelSubtitles = BuildDefaultSubtitles();

    public static ChapterInfo GetChapterForRoomIndex(int roomIndex)
    {
        foreach (var chapter in Chapters)
        {
            if (roomIndex >= chapter.StartRoomIndex && roomIndex <= chapter.EndRoomIndex)
                return chapter;
        }

        return Chapters[0];
    }

    public static int GetChapterIndexForRoomIndex(int roomIndex) => GetChapterForRoomIndex(roomIndex).ChapterIndex;

    public static bool IsChapterUnlocked(int chapterIndex)
    {
        if (PlayerProfile.Instance == null)
            return chapterIndex == 0;

        return PlayerProfile.Instance.HighestUnlockedChapterIndex >= chapterIndex;
    }

    public static bool IsRoomInUnlockedChapter(int roomIndex)
    {
        var chapterIndex = GetChapterIndexForRoomIndex(roomIndex);
        return IsChapterUnlocked(chapterIndex);
    }

    public static string GetLevelSubtitle(int roomIndex)
    {
        if (roomIndex < 0 || roomIndex >= LevelSubtitles.Length)
            return string.Empty;

        return LevelSubtitles[roomIndex];
    }

    public static void MarkRoomCompleted(int roomIndex)
    {
        if (PlayerProfile.Instance == null)
            return;

        var chapterIndex = GetChapterIndexForRoomIndex(roomIndex);
        var chapter = Chapters[chapterIndex];

        if (roomIndex == chapter.EndRoomIndex)
        {
            PlayerProfile.Instance.MarkChapterCompleted(chapterIndex);
            PlayerProfile.Instance.UnlockChapter(chapterIndex + 1);
        }

        PlayerProfile.Instance.Save();
    }

    public static string GetChapterHeaderText(int chapterIndex)
    {
        if (chapterIndex < 0 || chapterIndex >= Chapters.Length)
            return string.Empty;

        return Chapters[chapterIndex].Name;
    }

    private static string[] BuildDefaultSubtitles()
    {
        var subtitles = new string[GameManager.TotalLevels];

        for (int roomIndex = 0; roomIndex < subtitles.Length; roomIndex++)
        {
            var chapter = GetChapterForRoomIndex(roomIndex);
            int chapterLevel = roomIndex - chapter.StartRoomIndex + 1;
            subtitles[roomIndex] = $"{chapter.Name} - Beat {chapterLevel}";
        }

        // Key story beats from the ticket
        subtitles[0] = "The Bedroom Incident";
        subtitles[5] = "Mom's Final Warning";
        subtitles[25] = "The House Falls";
        subtitles[26] = "First Day of Chaos";
        subtitles[75] = "Principal's Office Showdown";
        subtitles[95] = "Government Collapse";
        subtitles[99] = "Earth Destruction Protocol";

        return subtitles;
    }
}
