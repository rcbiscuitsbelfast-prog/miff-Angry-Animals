# Cutscene Animation Guide (Godot)

Cutscenes are regular `.tscn` scenes stored in:
- `Scenes/Cutscenes/`

They are played by:
- `Script/CutscenePlayer.cs`

## How chapter cutscenes trigger
`StoryEventTrigger` triggers cutscenes automatically at chapter starts:
- roomIndex 0 → `BedroomIncident.tscn`
- roomIndex 6 → `Chapter2Intro.tscn`
- roomIndex 26 → `Chapter3Intro.tscn`
- roomIndex 76 → `Chapter4Intro.tscn`
- roomIndex 96 → `Chapter5Intro.tscn`

Each cutscene is only shown once per save profile (stored as a story flag).

## Editing a cutscene
Open any cutscene scene (example: `BedroomIncident.tscn`).

Each cutscene uses the `CutsceneScene` script:
- **Speakers**: array of speaker names (ex: `MOM`, `DAD`, `STICK`)
- **Lines**: array of text lines
- **Durations**: how long each line is shown

Optional:
- Add an **AnimatedSprite2D** and set **AnimatedSpritePath** to it.
- Fill **SpriteAnimations** with animation names to play per line.

## Skipping
During a cutscene:
- Tap/click anywhere OR press **ESC** to skip.

## Adding a new cutscene
1. Duplicate an existing cutscene scene in `Scenes/Cutscenes/`
2. Edit dialogue / visuals
3. Add the trigger mapping in `Script/StoryEventTrigger.cs`
