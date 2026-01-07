# Angry Animals - Non-Coder Guide

🎮 **Game:** Angry Animals (Godot 4.4 C#)
📋 **Version:** 1.0
👤 **Audience:** Designers, Artists, Producers, Non-Programmers
⏱️ **Reading Time:** ~15 minutes
✨ **Status:** POLISHED & BEGINNER-FRIENDLY (January 2025)

---

## 🆕 What's New (January 2025)

This guide has been updated with:
- ✅ **Game Feel System**: Particles, screen shake, haptic feedback
- ✅ **Settings Menu**: Easy customization of volume, difficulty, game feel
- ✅ **New Documentation**: 7 beginner-friendly guides
- ✅ **Safety Checklists**: Clear guidance on what to avoid
- ✅ **Mobile Optimization**: Touch-friendly controls, haptics

### New Documentation Suite:
1. **QUICK_START.md** - 5-minute setup guide
2. **GODOT_BEGINNER_MAP.md** - Complete folder structure guide
3. **INSPECTOR_TOUR.md** - Visual guide to Inspector panel
4. **COMMON_CHANGES.md** - Top 10 things non-coders change
5. **SETTINGS_REFERENCE.md** - What every setting does
6. **SAFETY_CHECKLIST.md** - What NOT to touch
7. **FINAL_POLISH_REPORT.md** - All polish features documented

### Game Feel Features Added:
- 🎨 **Particle Effects**: Confetti on wins, explosions on impacts, dust on hits
- 📳 **Screen Shake**: Minor/major/intense shake levels
- 📱 **Haptic Feedback**: Vibration on all game events (mobile)
- 🎬 **Slow Motion**: Cinematic moments on heavy impacts
- 🎵 **Audio Polish**: Volume controls, balanced sound levels

---

## 🎯 Quick Start for Non-Coders

This guide will teach you how to modify **Angry Animals** without writing any code. You'll learn:

- ✅ How to change game difficulty and values
- ✅ How to add new levels
- ✅ How to swap out graphics and sounds
- ✅ What you can safely change vs. what to avoid
- ✅ How to test your changes
- ✅ How to build for mobile devices

**Before You Start:**
- Install [Godot 4.4+ Mono version](https://godotengine.org/download)
- Download this project folder
- Open `project.godot` in Godot Editor

---

## 📁 Project Structure for Non-Coders

Think of the project like a filing cabinet:

```
Angry Animals/
├── Scenes/              ← WHERE YOU'LL SPEND MOST TIME
│   ├── MainMenu.tscn    ← Main menu screen
│   ├── Levels/          ← All game levels (001-100)
│   └── Rooms/           ← Room selection grid
├── Script/              ← CODE - DO NOT EDIT (unless you know C#)
├── Globals/             ← CODE - DO NOT EDIT
├── Assets/              ← YOUR ART AND AUDIO GO HERE (create this!)
└── project.godot        ← Project settings
```

---

## 🎮 Understanding the Game Flow

Before making changes, understand how the game works:

### 1. **Main Menu**
- First screen players see
- Has "Play", "Customize Face", "Unlock Full Game" buttons
- **File:** `Scenes/MainMenu.tscn`

### 2. **Room Selection**
- Grid showing all 100 levels (20 free, 80 premium)
- Players can see locked/unlocked levels
- **File:** `Scenes/Rooms/RoomSelection.tscn`

#### Procedural Levels (New)
The Room Selection screen now includes a toggle for **Procedural Levels**.

- **Procedural OFF**: Plays the original hand-built `Room001.tscn` → `Room100.tscn`
- **Procedural ON**: Plays a generated version of the selected room using `ProceduralRoom.tscn`

When Procedural is ON, you’ll also see a **Seed** field:
- **Seed = 0** (default): deterministic per-level generation (same room number always produces the same layout)
- **Seed = any number**: forces that exact layout (useful for sharing)
- The effective seed is copied to clipboard when starting a procedural level.

For more detail, see `PROCEDURAL_LEVELS.md`.

### 3. **Slingshot Phase** (Gameplay Part 1)
- Player drags projectile back to aim
- Shows trajectory arc
- Release to launch
- Destroys cups and obstacles
- **Main files:** 
  - `Scenes/Levels/RoomXXX.tscn` (individual levels)
  - Has Slingshot, TrajectoryDrawer, ProjectilesLoader nodes

### 4. **Traversal Phase** (Gameplay Part 2)
- StickClone character appears at spawn point
- Walks/jumps toward exit door
- Must reach exit to complete level
- **Main files:** 
  - In each level scene: Look for StickCloneSpawn node

### 5. **Level Complete**
- Shows score (how many attempts)
- Shows stars
- "Next Level" or "Retry" options
- **File:** `Scenes/LevelCompleted.tscn`

If destruction score > target, exit door unlocks.
If StickClone reaches exit door, level complete.

---

## 🔧 How to Change Game Values

### Method 1: Change in Godot Editor (Recommended)

**For SLINGSHOT POWER:**
1. Open any level (e.g., `Scenes/Levels/Room001.tscn`)
2. Find the "Slingshot" node in the Scene tree (left panel)
3. Click the Slingshot node
4. Look at the Inspector (right panel) - see export variables
5. **IMPULSE_MULT** - Controls launch power (default: 20.0)
   - Lower = weaker shots
   - Higher = stronger shots
6. **IMPULSE_MAX** - Maximum power cap (default: 1200)
7. **DRAG_LIM_MIN/MAX** - How far back player can drag

**For LEVEL DIFFICULTY:**
1. Open any level (e.g., Room001.tscn)
2. Find "RoomBase" node (usually root node of scene)
3. In Inspector:
   - **_targetScore** - How many cups to destroy (default: 3)
   - Higher number = harder level
4. Find "ProjectilesLoader" node
   - **_projectileCount** - How many animals you get (default: 3)
   - Fewer projectiles = harder

**For AUDIO VOLUME:**
1. Open MainMenu.tscn
2. Find "AudioManager" node (in scene hierarchy)
3. In Inspector:
   - **MusicVolume** - 0.0 to 1.0 (default: 0.7)
   - **SfxVolume** - Sound effects volume (default: 0.8)
   - **MuteMusic** / **MuteSfx** - Checkboxes to mute

### Method 2: Change Via Resource Files (Advanced)

Some values are stored in .tres files (you can edit these in Godot):
- Theme files for UI appearance
- Physics materials for bounciness
- Audio bus layouts

**To edit a .tres file:**
1. Double-click the file
2. Modify values in Inspector
3. Save

---

## 🎨 How to Add/Edit Levels

### Adding a New Level (Method 1: Copy Existing)

**FASTEST WAY:**
1. Open folder `Scenes/Levels/`
2. Find `Room030.tscn` (or any working level)
3. Right-click → Copy
4. Right-click → Paste
5. Rename to `Room031.tscn`
6. Open in Godot
7. MOVE things around (drag cups, obstacles)
8. DON'T delete or rename nodes unless you know what they do
9. Save
10. Launch game - your new level appears!

**What you can safely move:**
- Cup positions (nodes named "Cup" or similar)
- Obstacle positions (wooden blocks, stones, etc.)
- Background elements

**What you SHOULDN'T move:**
- Slingshot node (or game won't have a launcher)
- Camera node (or view will be wrong)
- Exit door (or level can't be completed)

### Adding a New Level (Method 2: Build From Template)

**For more control:**
1. Open a working level like Room001.tscn
2. File → Save Scene As → RoomXXX.tscn
3. Now edit freely - it's a new scene
4. Add obstacles by dragging from FileSystem into scene
5. Position them where you want
6. Test by playing

### Editing Existing Levels

**To change obstacle positions:**
1. Open the level (e.g., Room005.tscn)
2. Click any obstacle in the scene
3. Drag to move (or use Inspector for precise coordinates)
4. Test your changes

**To add more obstacles:**
1. Find similar obstacle in the scene
2. Right-click → Duplicate (or Ctrl+D)
3. Move the duplicate to new position
4. Adjust rotation if needed (Inspector → Transform → Rotation)

**To adjust target score:**
1. Select RoomBase node
2. Inspector → _targetScore
3. Change from 3 to desired number
4. More cups/obstacles = higher target score

---

## 🎵 How to Change Audio/Music

### Adding Background Music

**Before you start:** Create the `Assets` folder structure:
```
res://Assets/
├── Audio/
│   ├── Music/
│   └── SFX/
└── Sprites/
    ├── Face/
    └── Obstacles/
```

**Steps:**
1. Prepare your music file: `.ogg` or `.mp3` format
2. Copy to `Assets/Audio/Music/`
3. Open MainMenu.tscn
4. Find AudioManager node
5. Inspector → _backgroundMusic property
6. Click "[empty]" → Load → Select your music file
7. Check Autoplay
8. Adjust Volume

**Alternative via Code Edit (Easy):**
1. Open `Globals/AudioManager.cs`
2. Find line: `_backgroundMusic = LoadAudioResource("res://Assets/Audio/Music/BackgroundMusic.ogg");`
3. Change filename to your music file

### Adding Sound Effects

**Slingshot Sound:**
1. Put `.ogg` file in `Assets/Audio/SFX/`
2. Open AudioManager.cs
3. Find: `_slingshotSound = LoadAudioResource(...);`
4. Change path to your sound file

**Destruction Sound:**
1. Add destruction.ogg to `Assets/Audio/SFX/`
2. Open AudioManager.cs
3. Find: `_destructionSound = LoadAudioResource(...);`
4. Update path

**UI Click Sound:**
1. Add ui_click.ogg to `Assets/Audio/SFX/`
2. Open AudioManager.cs
3. Find: `_uiClickSound = LoadAudioResource(...);`
4. Update path

---

## 🖼️ How to Change Visuals

### Changing Background Colors

**Easy:**
1. Open any level scene
2. Find "ColorRect" node (usually named Background or similar)
3. Inspector → Color → click color picker
4. Choose new color
5. All levels with this node change color!

### Adding/Changing Sprites

**Creating the asset:**
1. Create or find PNG image for your sprite
2. Save to `Assets/Sprites/` folder (create subfolders to organize)
   - e.g., `Assets/Sprites/Obstacles/my_rock.png`
   - e.g., `Assets/Sprites/Face/face_happy.png`

**Using the sprite in a level:**
1. Open a level scene
2. Select the node you want to change (e.g., a cup)
3. Inspector → Texture → click [empty]
4. Click "Load" → navigate to your PNG
5. Sprite appears in scene!

**For Face Customization:**
*Face emotions:*
- Save as: `Assets/Sprites/Face/face_neutral.png`
- Save as: `Assets/Sprites/Face/face_happy.png`
- Save as: `Assets/Sprites/Face/face_angry.png`
- etc.

*Hats:*
- `Assets/Sprites/Face/Hats/cap.png`
- `Assets/Sprites/Face/Hats/crown.png`
- `Assets/Sprites/Face/Hats/beanie.png`

*Glasses:*
- `Assets/Sprites/Face/Glasses/round.png`
- `Assets/Sprites/Face/Glasses/aviator.png`

### Changing UI Fonts

1. Find font file (.ttf or .otf)
2. Copy to `Assets/Fonts/`
3. Open MainMenu.tscn
4. Select any Label or Button
5. Inspector → Theme Overrides → Fonts → Font
6. Click [empty] → Load → select your font
7. Adjust Font Size if needed

Or create a global theme:
1. Project → Project Settings → GUI Theme
2. Create New Theme
3. Set default font for all UI

---

## 💰 How to Adjust Monetization

### Enabling/Disabling Ads

**Quick Toggle:**
1. Open MainMenu.tscn
2. Find MonetizationManager node (autoload)
3. Inspector → **ShowAds** checkbox
   - Checked = ads enabled
   - Unchecked = ads disabled

**Per-Platform:**
Open `project.godot` in a text editor:
```ini
[monetization]
admob/app_id="ca-app-pub-XXXXXXXXXXXXXXXX~YYYYYYYYYY"  ← Your AdMob App ID
admob/banner_ad_unit_id="ca-app-pub-.../..."              ← Your banner ad
admob/interstitial_ad_unit_id="ca-app-pub-..."            ← Your interstitial
admob/rewarded_ad_unit_id="ca-app-pub-.../..."            ← Your rewarded ad
```

### Changing IAP Price

**Method 1 - In Code:**
1. Open `Globals/MonetizationManager.cs`
2. Find: `"Unlock Full Game - £1.50"`
3. Change £1.50 to your price (e.g., $1.99, €2.00)

**Method 2 - Dynamic:**
The price comes from app store - text is just display. Update:
- App Store Connect (iOS)
- Google Play Console (Android)

### Disabling IAP (Free Game)

If you want the game completely free:

1. Open `Script/MainMenu.cs`
2. Find method: `AddUnlockFullGameButton()`
3. Add this at the start:
```csharp
// Add this line to hide unlock button
return;
```

Or simpler: Just don't configure IAP in app stores, and players won't see purchase option.

---

## 🧪 How to Test Your Changes

### Testing in Godot Editor

**Method 1 - Play Button:**
1. Open any scene (level, menu, etc.)
2. Click "Play Scene" button (► icon) in top-right
3. Scene runs immediately
4. Test your changes
5. Press F8 or click Stop to exit

**Method 2 - Play Project:**
1. Set main scene in Project → Project Settings → Run → Main Scene
2. Should be: `res://Scenes/Main/Main.tscn`
3. Click "Play Project" button (bigger ► icon)
4. Test full game flow from main menu

**While Testing:**
- Press F5 to reload current scene quickly
- Use Scene → Reload Saved Scene if changes don't appear
- Check Output/Debugger panels for errors (bottom of screen)

### Testing on Mobile

**For Android:**
1. Connect Android device via USB
2. Enable USB debugging (Settings → Developer Options)
3. In Godot: Project → Export
4. Select Android preset
5. Check "Export With Debug"
6. Click "Export All" or
7. Click "Remote Debug" → "Remote Debug"
8. Game installs and launches on device

**For iOS:**
1. Need macOS with Xcode
2. Export project for iOS
3. Open in Xcode
4. Set up signing certificates
5. Build and run on device

---

## 🎬 Scene Hierarchy Explanation

### The 4 Main Scene Types

#### 1. Main Menu Scenes
**Files:** `Scenes/MainMenu.tscn`

Structure:
```
MainMenu (CanvasLayer)
├── ColorRect (background)
└── VBoxContainer (holds buttons)
    ├── TitleLabel
    ├── PlayButton
    ├── CustomizeFaceButton
    ├── RoomSelectionButton
    └── SettingsButton
```

**Scripts:** MainMenu.cs

**What it controls:**
- All main menu buttons and navigation
- IAP purchase flow ("Unlock Full Game")
- Face customization screen launch
- Scene transitions to game

---

#### 2. Level/Scene Scenes
**Files:** `Scenes/Levels/RoomXXX.tscn` (001-100)

Typical Structure:
```
RoomBase (Node2D) ← Main script that controls the level
├── ColorRect (background)
├── Slingshot (Node2D) ← Launches animals
│   ├── InputArea (Area2D) ← Detects mouse/touch
│   └── TrajectoryDrawer ← Shows aiming line
├── ProjectilesLoader (Node2D) ← Manages animal queue
│   └── FaceProjectile (RigidBody2D) ← The animal that gets launched
├── Various Obstacles (StaticBody2D)
│   └── Sprite2D
├── Cups (StaticBody2D) ← Targets to destroy
│   └── Sprite2D
├── ExitDoor (Node2D) ← Goal position
└── StickCloneSpawn (Marker2D) ← Where walking character appears
```

**Scripts:**
- RoomBase.cs (controls the whole level flow)
- Slingshot.cs (drag/launch mechanics)
- Projectile.cs (physics and collisions)
- ProjectilesLoader.cs (manages projectile queue)
- (Plus others for specific objects)

**What you can modify:**
- Obstacle positions
- Cup positions  
- Background colors
- Number of projectiles (via ProjectilesLoader export)
- Target score (via RoomBase export)
- Any Sprite2D textures

---

#### 3. UI/HUD Scenes
**Files:** `Scenes/UI/GameHud.tscn`, `Scenes/LevelCompleted.tscn`

Structure:
```
GameHud (CanvasLayer)
├── AttemptsLabel (shows 3 left)
├── ScoreLabel
├── RageBar (if implemented)
└── PauseButton
```

**Scripts:** GameHud.cs

**What it controls:**
- In-game UI display
- Pause menu
- Score and attempt counters

---

#### 4. Character Scenes
**Files:** Missing but should be:
- `Scenes/Characters/StickClone.tscn`
- `Scenes/Characters/FaceProjectile.tscn`

Typical Structure:
```
StickClone (CharacterBody2D)
├── Sprite2D (body)
├── Sprite2D (face)
├── Sprite2D (hat) ← Optional
└── Sprite2D (glasses) ← Optional
```

**Scripts:** StickClone.cs

**What it is:**
- The character that appears AFTER you launch all animals
- Walks from spawn point to exit door
- Can have custom faces, hats, glasses

---

## ⚠️ What You SHOULD NEVER Touch vs. What You CAN Modify

### ❌ NEVER TOUCH (Unless You're a Programmer)

**These are the core scripts. Breaking them breaks the whole game.**

#### Global Scripts (Autoload Singletons):
```
Globals/
├── GameManager.cs      ← Controls entire game flow
├── SignalManager.cs    ← All event communication
├── AudioManager.cs     ← Sound system
├── ScoreManager.cs     ← Score tracking and save files
├── FileManager.cs      ← File reading/writing
├── AdsManager.cs       ← Advertisement system
├── MonetizationManager.cs ← IAP purchases
├── PlayerProfile.cs    ← User saved data
└── RageSystem.cs       ← Combo/rage mechanics
```

**Why not touch these:**
- They run the entire game
- Changes can affect every level
- Require understanding of C# and Godot
- Breaking changes are hard to debug

#### Core Gameplay Scripts:
```
Script/
├── Projectile.cs       ← Physics and collisions
├── ProjectilesLoader.cs ← Controls projectile queue
└── RoomBase.cs         ← Parent class for all levels
```

**Why not touch these:**
- They're used by every level
- Physics calculations are delicate
- Changes break all levels at once

---

### ✅ SAFE TO MODIFY

**These you can edit freely without breaking core systems.**

#### Level-Specific Values (Via Inspector):
- Slingshot drag limits
- Number of projectiles  
- Target score per level
- Obstacle positions
- Cup positions
- Spawn point locations

#### Visual Assets:
- Sprite images (PNG files)
- Background colors
- Font files
- UI graphics
- Particles (if added)

#### Audio:
- Background music files
- Sound effect files
- Volume levels

#### Configuration:
- Game title in Project Settings
- App icon
- Version number
- Build settings

### 🟡 MODIFY WITH CAUTION

**These can be changed, but test thoroughly:**

#### User-Facing Scripts:
```
Script/
├── MainMenu.cs         ← Menu behavior (but it's complex)
├── StickClone.cs       ← Character controls (simpler)
├── LevelCompleted.cs   ← Victory screen
├── GameHud.cs          ← In-game UI
├── RoomSelection.cs    ← Level select grid
└── FaceCustomizationScreen.cs ← Character creator
```

**If you modify these:**
- Test the specific screen thoroughly
- Check all buttons still work
- Make sure navigation still flows correctly
- Keep backup copies before changing

#### Scene Structure:
You CAN add/remove nodes, but:
- Don't delete required nodes (Slingshot, Camera, etc.)
- Don't rename exported node path references
- Don't change node types (e.g., don't change RigidBody2D to StaticBody2D)

---

## 🐛 Common Issues & Quick Fixes

### Issue: "Scene file is invalid" or Won't Open

**Fix:**
1. Close Godot
2. Delete `.godot/` hidden folder (it's a cache)
3. Re-open project
4. Godot rebuilds cache automatically

### Issue: Game Runs But Nothing Happens

**Likely cause:** Missing autoload singletons

**Fix:**
1. Project → Project Settings → Autoload
2. Check all these have green checkmarks:
   - GameManager
   - SignalManager
   - AudioManager
   - ScoreManager
   - Etc.

### Issue: "Could not find node: X"

**Likely cause:** Node name changed or moved in scene

**Fix:**
1. Open scene mentioned in error
2. Find node with correct name
3. Check node path matches what script expects
4. Or re-export node path in Inspector

### Issue: Slingshot Doesn't Work

**Check:**
1. Is InputArea node under Slingshot?
2. Does InputArea collision shape cover the drag area?
3. Are there any errors in Output panel?
4. Is the Slingshot script assigned to the node?

### Issue: Projectiles Don't Launch

**Check:**  
1. Is ProjectilesLoader node in scene?
2. Does it have reference to Slingshot node?
3. Does it have FaceProjectile scene assigned?
4. Check _projectileCount > 0

### Issue: Audio Not Playing

**Check:**
1. Are audio files in correct `Assets/Audio/` folder?
2. Check Volume isn't 0 in AudioManager
3. Check MuteMusic/MuteSfx aren't checked
4. Check speakers/headphones work

### Issue: Custom Face Not Showing

**Check:**
1. Is face image in correct folder?
2. Is filepath correct in code/scene?
3. Is image PNG format?
4. Check Output for loading errors

---

## 📝 Your First Non-Coder Task

Let's walk through adding a simple change:

### Task: Make level 5 easier by giving more animals

1. **Open Godot** and load the project
2. **Double-click** `Scenes/Levels/Room005.tscn`
3. **Find** "ProjectilesLoader" node (maybe called "Projectiles" or similar)
4. **Click** it to select
5. **Look** at Inspector panel (right side)
6. **Find** `_projectileCount` property
7. **Change** from 3 to 5
8. **Save** scene (Ctrl+S or Scene → Save)
9. **Click** Play button (►)
10. **Test** - you now have 5 animals instead of 3!

**Congratulations!** You just modified a game value without code. This same method works for:
- Slingshot power
- Target score
- Audio volumes
- And many other export variables

---

## 🚀 Next Steps for Non-Coders

### Level 1: Visual Changes
1. Change background colors in levels
2. Swap sprite textures
3. Add your own images
4. Change UI colors and fonts

### Level 2: Game Values
1. Adjust slingshot power
2. Change number of projectiles
3. Modify target scores
4. Adjust audio levels

### Level 3: Level Design
1. Copy existing levels
2. Move obstacles around
3. Create new configurations
4. Test playability

### Level 4: Advanced Changes
1. Add new customization assets
2. Create particle effects
3. Modify UI layouts
4. Add new sound effects

### Level 5: Export to Mobile
1. Set up export templates
2. Configure signing
3. Build for Android/iOS
4. Test on real devices

---

## 📚 Resources & Support

**Godot Documentation:**
- Official: https://docs.godotengine.org/en/stable/
- C# Specific: https:
- Tutorials: https://docs.godotengine.org/en/stable/getting_started/step_by_step/index.html

**Common Terms:**
- **Node:** An object in the scene (like a cup or slingshot)
- **Scene:** A collection of nodes (like a level or menu)
- **Script:** Code that makes nodes do things
- **Export Variable:** A setting you can change in the Inspector
- **Inspector:** Right panel showing node properties
- **Scene Tree:** Left panel showing all nodes in hierarchy

**Keyboard Shortcuts:**
- Ctrl+S: Save scene
- F5: Run project
- F6: Run current scene  
- Ctrl+D: Duplicate selected node
- Delete: Delete selected node
- Ctrl+Z: Undo
- Q/W/E/R: Switch manipulate tools (select/move/rotate/scale)

---

## 🎓 Key Takeaways for Non-Coders

✅ **You CAN:**
- Change game values via Inspector
- Swap graphics and audio
- Move obstacles and design levels
- Adjust colors and fonts
- Modify configuration settings

❌ **You SHOULDN'T:**
- Edit core script files
- Change autoload singletons
- Modify physics calculations
- Delete critical nodes
- Rename important nodes without updating references

🎯 **Pro Tip:** Always test your changes, and keep backups before modifying files!

---

**Need Help?**
- Check `TROUBLESHOOTING.md` for common issues
- Review `GAME_VALUES.md` for adjustable parameters
- See `BUILD_CHECKLIST.md` before mobile submission

Happy designing! 🎮✨