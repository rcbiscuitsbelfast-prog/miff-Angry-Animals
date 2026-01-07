# Angry Animals - Adjustable Game Parameters

🎮 **Complete reference of all values non-programmers can modify**  
📊 **Version:** 1.0  
⚙️ **Last Updated:** $(date)

---

## 🔧 How to Use This Document

This spreadsheet lists every adjustable parameter in Angry Animals, organized by system:

- **Parameter Name:** The setting name you see in Godot
- **Location:** Where to find it (Scene → Node → Property)
- **Type:** What kind of value (Number, Text, Checkbox, etc.)
- **Default:** The current value
- **Range:** Valid values (if applicable)
- **Impact:** What changing it affects
- **Safety:** ⚠️ = Adjust carefully, ✅ = Safe to change freely

---

## 🎮 CORE GAMEPLAY VALUES

### Slingshot Physics
| Parameter | Location | Type | Default | Range | Impact | Safety |
|-----------|----------|------|---------|-------|--------|--------|
| **IMPULSE_MULT** | [Scene] → Slingshot → Inspector | Float | 20.0 | 1.0-50.0 | Launch power multiplier. Higher = stronger shots | ⚠️ |
| **IMPULSE_MAX** | [Scene] → Slingshot → Inspector | Float | 1200.0 | 100-2000 | Maximum launch force cap | ⚠️ |
| **DRAG_LIM_MIN** | Code constant | Vector2 | (-60, 0) | Any | Minimum drag boundary (left/down) | ⚠️ |
| **DRAG_LIM_MAX** | Code constant | Vector2 | (0, 60) | Any | Maximum drag boundary (right/up) | ⚠️ |

**💡 Impact Description:**
- **IMPULSE_MULT** directly controls how powerful shots are. 20 = normal, 10 = weak, 30 = very strong
- **IMPULSE_MAX** prevents shots from being TOO powerful. If your level has close targets, lower this
- **DRAG_LIM** values control how far back players can pull. Wider range = more control, but also harder to aim

---

### Level Difficulty
| Parameter | Location | Type | Default | Range | Impact | Safety |
|-----------|----------|------|---------|-------|--------|--------|
| **_projectileCount** | [Scene] → ProjectilesLoader → Inspector | Int | 3 | 1-10 | Number of animals to launch | ✅ |
| **_targetScore** | [Scene] → RoomBase → Inspector | Int | 3 | 1-20 | Cups to destroy to unlock exit | ✅ |
| **_moveSpeed** | [Scene] → StickClone → Inspector | Float | 150.0 | 50-500 | Character traversal speed | ✅ |
| **_jumpForce** | [Scene] → StickClone → Inspector | Float | -400.0 | -1000 to -200 | Character jump height | ✅ |

**💡 Impact Description:**
- **_projectileCount**: More projectiles = easier level. 3 = challenging, 5 = easy, 1 = hard/nearly impossible
- **_targetScore**: Number of cups/points needed. 2 = easy, 5 = hard, 10 = very hard
- **_moveSpeed**: Faster traversal = less time for player to see obstacles, but feels more responsive
- **_jumpForce**: Higher jumps = easier to clear obstacles, but may look unrealistic

---

## 🎨 VISUAL & GRAPHICS

### Background Colors
| Parameter | Location | Type | Default | Range | Impact | Safety |
|-----------|----------|------|---------|-------|--------|--------|
| **ColorRect.color** | [Scene] → ColorRect → Inspector | Color | Sky blue (0.7,0.8,0.9) | Any RGB | Background fill color | ✅ |

**💡 Impact:** Controls the sky/background color of levels. Change to:
- Sunset: (0.9, 0.6, 0.3) - orange
- Night: (0.1, 0.1, 0.3) - dark blue
- Alien planet: (0.4, 0.2, 0.4) - purple

---

### Sprite Textures
| Parameter | Location | Type | Default | Range | Impact | Safety |
|-----------|----------|------|---------|-------|--------|--------|
| **Sprite2D.texture** | [Node] → Inspector | Texture | null/empty | Any Texture2D | Visual appearance | ✅ |
| **Modulate** | [Node] → Inspector | Color | White (1,1,1) | Any RGB | Color tint overlay | ✅ |
| **FlipH/FlipV** | [Node] → Inspector | Bool | false | true/false | Mirror horizontally/vertically | ✅ |

**💡 Impact:**
- Change textures to completely new graphics
- Modulate tints sprites (e.g., red overlay for damage)
- Flip sprites to face different directions

---

### Character Customization
| Parameter | Location | Path | Default | Impact | Safety |
|-----------|----------|------|---------|--------|--------|
| **Face Emotions** | Asset files | `Assets/Sprites/Face/face_*.png` | Missing | Character expressions | ✅ |
| **Hat Sprites** | Asset files | `Assets/Sprites/Face/Hats/*.png` | Missing | Hat accessories | ✅ |
| **Glasses Sprites** | Asset files | `Assets/Sprites/Face/Glasses/*.png` | Missing | Glasses accessories | ✅ |
| **Custom Face Image** | Player data | `user://custom_face.png` | null | User uploaded photo | ✅ |

**💡 Impact:** These are cosmetic only - players customize their character's appearance

---

## 🔊 AUDIO VALUES

### Audio Manager
| Parameter | Location | Type | Default | Range | Impact | Safety |
|-----------|----------|------|---------|-------|--------|--------|
| **MusicVolume** | [Scene] → AudioManager → Inspector | Float | 0.7 | 0.0-1.0 | Background music volume | ✅ |
| **SfxVolume** | [Scene] → AudioManager → Inspector | Float | 0.8 | 0.0-1.0 | Sound effects volume | ✅ |
| **MuteMusic** | [Scene] → AudioManager → Inspector | Bool | false | true/false | Toggle music on/off | ✅ |
| **MuteSfx** | [Scene] → AudioManager → Inspector | Bool | false | true/false | Toggle SFX on/off | ✅ |

**💡 Impact:** Master volume controls for all audio. 0.0 = silent, 1.0 = maximum

---

### Audio Files
| Parameter | Location | Path | Default | Impact | Safety |
|-----------|----------|------|---------|--------|--------|
| **Background Music** | Asset files | `Assets/Audio/Music/BackgroundMusic.ogg` | null | Menu/game BGM | ✅ |
| **Slingshot Sound** | Asset files | `Assets/Audio/SFX/SlingshotSound.ogg` | null | Launch sound | ✅ |
| **Destruction Sound** | Asset files | `Assets/Audio/SFX/DestructionSound.ogg` | null | Hit/collision sound | ✅ |
| **UI Click Sound** | Asset files | `Assets/Audio/SFX/UiClickSound.ogg` | null | Button press sound | ✅ |
| **Combo Sound** | Asset files | `Assets/Audio/SFX/ComboSound.ogg` | null | Multiple hits | ✅ |
| **Rage Sound** | Asset files | `Assets/Audio/SFX/RageSound.ogg` | null | Rage/killstreak sound | ✅ |

**💡 Impact:** Each sound effect plays during specific game events

---

## 🔔 I/O & FILES

### Save Data
| Parameter | Location | Path | Default | Impact | Safety |
|-----------|----------|------|---------|--------|--------|
| **Score File** | FileManager | `user://animals.save` | animals.save | Level scores | ⚠️ |
| **Profile File** | FileManager | `user://player_profile.json` | player_profile.json | Customization | ⚠️ |

**⚠️ Caution:** Don't change these paths unless you understand file systems. `user://` is a cross-platform path that works on all devices.

---

## 💰 MONETIZATION SETTINGS

### In-App Purchase (IAP)
| Parameter | Location | Type | Default | Range | Impact | Safety |
|-----------|----------|------|---------|-------|--------|--------|
| **ShowAds** | MonetizationManager → Inspector | Bool | true | true/false | Enable/disable all ads | ✅ |
| **IAP Price** | MainMenu.cs (line 73) | String | "£1.50" | Any text | Display price text | ✅ |
| **Product ID** | project.godot | String | "full_game_unlock" | Any ID | IAP product identifier | ⚠️ |

**💡 Impact:** Controls free vs. paid content. Fully unlocking gives access to all 100 levels and removes ads.

---

### Ad Configuration
| Parameter | Location | Type | Default | Impact | Safety |
|-----------|----------|------|---------|--------|--------|
| **AdMob App ID** | project.godot → monetization | String | "" (empty) | Your AdMob ID | ⚠️ |
| **Banner Ad Unit** | project.godot → monetization | String | "" (empty) | Banner ad identifier | ⚠️ |
| **Interstitial Ad Unit** | project.godot → monetization | String | "" (empty) | Full-screen ad identifier | ⚠️ |
| **Rewarded Ad Unit** | project.godot → monetization | String | "" (empty) | Rewarded video ad ID | ⚠️ |

**⚠️ Caution:** These require AdMob account setup. Leave empty to disable ads entirely.

---

## 👾 TESTING & DEBUGGING

### Developer Shortcuts
| Parameter | Location | Type | Default | Impact | Safety |
|-----------|----------|------|---------|--------|--------|
| **Q Key** | Level.cs (line 15) | Key | Q | - | Returns to main menu in-game | ⚠️ |

**💡 Impact:** Pressing Q during gameplay returns to main menu (debug-only feature)

---

## 📊 PERFORMANCE SETTINGS

### Physics
| Parameter | Location | Type | Default | Range | Impact | Safety |
|-----------|----------|------|---------|-------|--------|--------|
| **STOPPED_THRESHOLD** | Projectile.cs (line 14) | Float | 0.1 | 0.0-1.0 | Velocity to trigger "almost stopped" | ⚠️ |

**💡 Impact:** Lower values = projectile considered "stopped" sooner. Affects when traversal phase begins.

---

### Trajectory Visuals
**Note:** These are in TrajectoryDrawer.cs but may not be exported

| Parameter | Location | Type | Default | Impact | Safety |
|-----------|----------|------|---------|--------|--------|
| **Trajectory Line Width** | Code constant | Int | 3 | 1-10 | Visual thickness of aim line | ✅ |
| **Trajectory Line Color** | Code constant | Color | Gray | Any RGB | Color of aim line | ✅ |
| **Arrow Size** | Code constant | Int | 15 | 5-30 | Size of direction arrow | ✅ |

---

## 🎨 LEVEL DESIGN BEST PRACTICES

### Difficulty Guidelines

**Easy Level:**
```
_projectileCount = 5 (many animals)
_targetScore = 2 (few cups to destroy)
Obstacles = scattered, easy angles
```

**Medium Level:**
```
_projectileCount = 3
_targetScore = 4
Obstacles = layered, need planning
```

**Hard Level:**
```
_projectileCount = 2 (few animals)
_targetScore = 6 (many cups)
Obstacles = complex structures, precise shots needed
```

### Suggested Ranges by Difficulty

| Difficulty | _projectileCount | _targetScore | Notes |
|------------|------------------|--------------|--------|
| Very Easy | 5-7 | 1-3 | Tutorial/first levels |
| Easy | 4-5 | 2-4 | Early game |
| Medium | 3-4 | 3-5 | Mid game |
| Hard | 2-3 | 4-6 | Late game |
| Very Hard | 1-2 | 5-8 | Challenge levels |
| Expert | 1 | 6-10 | For skilled players |

---

## 🎯 QUICK REFERENCE: MOST COMMON CHANGES

### Change These to Adjust Difficulty:
1. `_projectileCount` (ProjectilesLoader) - How many animals to launch
2. `_targetScore` (RoomBase) - Cups needed to unlock exit
3. `IMPULSE_MULT` (Slingshot) - Launch power
4. Obstacle positions - Make shots easier/harder

### Change These to Adjust Audio:
1. `MusicVolume` (AudioManager) - Background music
2. `SfxVolume` (AudioManager) - Sound effects
3. Audio file paths in AudioManager.cs

### Change These to Adjust Visuals:
1. Sprite textures - Replace PNG files
2. ColorRect colors - Background skies
3. UI fonts - Typography
4. Character face/hat/glasses sprites

### Change These for Monetization:
1. `ShowAds` - Enable/disable ads entirely
2. Ad unit IDs in project.godot
3. IAP price in MainMenu.cs

---

## ⚠️ IMPORTANT NOTES

### Values That Require Code Changes (Not Exported):
Some values are hardcoded in scripts and require editing the C# code:

- `DRAG_LIM_MIN` / `DRAG_LIM_MAX` (Vector2 constants)
- `STOPPED_THRESHOLD` (float constant)
- Physics layer numbers (int constants)
- Signal names
- Product IDs for IAP

**Recommendation:** If you need these changed, work with a programmer or follow the "Safe Code Changes" section in NON_CODER_GUIDE.md

### Values That Reset on Scene Reload:
Some runtime values reset every time scene loads:
- Current score
- Attempts count
- Projectiles fired
- Destruction score

**These are meant to reset** - don't try to make them persist unless you understand save systems.

### Asset Resolution:
All paths are relative to `res://` (project root). Common paths:
```
res:// = project folder root
res://Assets/Audio/ = audio files
res://Assets/Sprites/ = graphics
res://Scenes/Levels/ = level scenes
res://Scripts/ = C# scripts
user:// = user data (save files, per device)
```

---

## 📚 ADDITIONAL PARAMETER CATEGORIES

### UI/UX Settings
| Parameter | Location | Type | Default | Impact |
|-----------|----------|------|---------|--------|
| Button Colors | Theme | Color | Gray/Blue | UI button appearance |
| Font Size | Labels | Int | Variable | Text size |
| UI Scale | Project Settings | Float | 1.0 | Overall UI size |

### Mobile-Specific
| Parameter | Location | Type | Default | Impact |
|-----------|----------|------|---------|--------|
| Orientation | project.godot | Enum | Landscape | Screen orientation |
| Touch Sensitivity | Code constant | Float | 1.0 | Touch input responsiveness |
| Virtual Joystick | Asset | Texture | null | On-screen controls |

### Platform-Specific
| Parameter | Location | Type | Default | Impact |
|-----------|----------|------|---------|--------|
| Icon | Project Settings | Texture | icon.svg | App icon |
| Version | Project Settings | String | "1.0.0" | App version |
| Package Name | Export Settings | String | com.company.app | App ID |

---

## 🛠️ TROUBLESHOOTING VALUES

### "My changes don't show up in game"

**Check:**
1. Did you save the scene (Ctrl+S)?
2. Did you change the value on the right node?
3. Is it an exported variable (wrench icon in Inspector)?
4. Try restarting Godot or reloading the project

### "I broke something and don't know what"

**Recovery:**
1. Check Output panel for error messages
2. If you edited a scene: Revert the file (git checkout if using version control)
3. If you edited a script: Revert the file
4. If you changed export variables: Reset to default in Inspector (revert arrow icon)

### "Values look different than this document"

**Possible causes:**
- You're looking at a different scene/node
- Default values were changed in code
- Document needs updating
- You're in the wrong mode (2D vs Script)

**Solution:** Trust what you see in the Inspector over this document. Default values may vary by scene.

---

## 📊 DOCUMENT VERSION HISTORY

| Version | Date | Changes |
|---------|------|---------|
| 1.0 | Initial | First complete parameter list |

---

## 🎯 SUMMARY: CHANGES BY IMPACT

### Easiest (No risk):
- Colors, textures, fonts
- Volume levels
- Sprite positions
- Background colors

### Moderate (Test after changing):
- Projectile counts
- Target scores
- Launch power
- Audio file paths

### Advanced (Consult guide first):
- Ad unit IDs
- IAP product IDs
- Save file paths
- Physics constants

### Expert (Code knowledge required):
- Signal names
- Class structures
- Physics calculations
- Core game logic

---

**END OF PARAMETERS LIST**

**Remember:** When in doubt, TEST YOUR CHANGES! Godot makes it easy to try different values and immediately see results.