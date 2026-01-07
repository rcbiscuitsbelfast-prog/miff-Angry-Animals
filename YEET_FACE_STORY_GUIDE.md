# Yeet Face Story Guide (Non‑Coder)

This project includes a lightweight story framework built around **5 Chapters** and **100 Levels**.

Everything is driven by:
- **StoryData.cs** (chapter definitions + subtitles)
- **PlayerProfile** (chapter progress + cutscene flags)
- **RoomSelection** (shows subtitles)
- **GameHud** (shows the current subtitle + objectives)

## Chapters
Chapters are defined in `Globals/StoryData.cs` using **0-based room indices**:
- Chapter 1: roomIndex **0–5** (Levels 1–6)
- Chapter 2: **6–25**
- Chapter 3: **26–75**
- Chapter 4: **76–95**
- Chapter 5: **96–99** (Levels 97–100)

## Level subtitles
Each level has a subtitle. These are shown:
- In **RoomSelection** (level list)
- In the **HUD** during play

To change a subtitle:
1. Open `Globals/StoryData.cs`
2. Edit `BuildDefaultSubtitles()`
3. Update the string for the level you want

Key beats already included:
- Level 1: “The Bedroom Incident”
- Level 6: “Mom’s Final Warning”
- Level 26: “The House Falls”
- Level 27: “First Day of Chaos”
- Level 76: “Principal’s Office Showdown”
- Level 96: “Government Collapse”
- Level 100: “Earth Destruction Protocol”

## Chapter progression
When you complete the final room index of a chapter (5 / 25 / 75 / 95), the next chapter is marked unlocked in `PlayerProfile`.

## Cutscenes
Cutscenes are triggered automatically at chapter starts (roomIndex 0/6/26/76/96). See `CUTSCENE_ANIMATION_GUIDE.md`.
