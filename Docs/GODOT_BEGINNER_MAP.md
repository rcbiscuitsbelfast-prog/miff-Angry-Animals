# Godot Beginner's Map 🗺️
## Navigate Angry Animals Without Fear!

> **⚠️ IMPORTANT FOR NON-CODERS:** You can change 90% of the game settings without touching a single line of code! Everything is in the **Inspector** panel on the right side of Godot.

---

## 🎯 Quick Start (2 Minutes)

1. Open Godot Engine 4.x
2. Click "Import" and select this project folder
3. Double-click `project.godot` to open
4. Press **F5** to test the game immediately!
5. When you want to change something, follow the sections below.

---

## 📁 Folder Structure Explained

```
AngryAnimals/
├── 🔴 Globals/          ← ⚠️ DANGER ZONE - Don't edit unless you know C#!
│   ├── GameManager.cs     ← Game flow, level loading, state management
│   ├── AudioManager.cs   ← Sound effects, music
│   ├── LevelGenerator.cs ← Procedural level generation
│   └── ... (11 managers total)
│
├── 🟢 Script/           ← ✅ SAFE ZONE - Contains game logic scripts
│   ├── Animal.cs         ← Player character (StickClone)
│   ├── Slingshot.cs     ← Slingshot behavior
│   ├── RoomBase.cs      ← Room/level template
│   └── ... (30+ scripts)
│
├── 🔵 Scenes/           ← ✅ SAFE ZONE - Game scenes you can open
│   ├── Main/            ← Main menu, level selection
│   ├── Levels/          ← All 100 level rooms (Room001.tscn - Room100.tscn)
│   ├── Characters/      ← Player, enemies, animals
│   ├── Obstacles/       ← Cups, props, destructibles
│   └── UI/              ← Buttons, HUD, panels
│
├── 🟡 Assets/           ← ✅ SAFE ZONE - Your images, sounds, music
│   ├── Sprites/         ← PNG images for characters, objects
│   ├── Audio/           ← OGG (music) and WAV (sound effects)
│   └── ... (add your own!)
│
└── 📄 Documentation/    ← ✅ SAFE ZONE - Guides for you!
    ├── NON_CODER_GUIDE.md
    ├── QUICK_START.md
    └── ... (this file)
```

---

## 🎨 File Color Legend

| Color | Type | Safety | What It Is |
|-------|------|--------|------------|
| 🔴 **Red** | `.cs` scripts | ⚠️ BE CAREFUL | C# code - contains game logic |
| 🔵 **Blue** | `.tscn` scenes | ✅ SAFE | Godot scenes - levels, menus, objects |
| 🟢 **Green** | `.png`, `.jpg`, `.wav`, `.ogg` | ✅ SAFE | Your artwork, sounds, music |
| 🟡 **Yellow** | `.tres` | ⚠️ USE CAREFULLY | Game settings/presets |
| ⚪ **White** | `.md` | ✅ SAFE | Documentation |

---

## ✅ SAFE TO MODIFY (Beginner Friendly)

### Scenes You Can Open & Edit:
1. **`Scenes/Levels/Room001.tscn` through `Room100.tscn`**
   - Move objects around
   - Add/remove cups and obstacles
   - Change positions

2. **`Scenes/Main/MainMenu.tscn`**
   - Change menu layout
   - Change colors, fonts
   - Add/remove buttons

3. **`Scenes/Rooms/RoomSelection.tscn`**
   - Change level selection screen
   - Adjust level grid layout

4. **`Scenes/Characters/StickClone.tscn`**
   - Change player character
   - Swap sprites

### Assets You Can Replace:
- All files in `Assets/Sprites/` → Replace with your own PNGs
- All files in `Assets/Audio/` → Replace with your own sounds

### Settings Files You Can Edit:
- **`project.godot`** → Game name, version, monetization IDs
- **`export_presets.cfg`** → Android/iOS export settings

---

## ⚠️ DANGER ZONE - NEVER TOUCH THESE

Unless you know C# programming, **DO NOT EDIT**:

### Core Game Scripts (Globals/):
- `GameManager.cs` - Game will break if you edit this
- `SignalManager.cs` - Communication between systems
- `PlayerProfile.cs` - Save/load system
- `FileManager.cs` - File operations

### Complex Game Logic (Script/):
- `RoomBase.cs` - Room/level flow
- `Slingshot.cs` - Physics and input
- `Projectile.cs` - Projectile physics
- `StickClone.cs` - Player controller

### Config Files:
- **`.git/` folder** - Git version control (don't touch)
- **`*.csproj` files** - C# project files (don't touch)
- **`*.sln` files** - Solution files (don't touch)

---

## 🎮 How to Change Common Things

### Change Game Colors/Theme
1. Open `Globals/GameManager.cs` in Godot
2. Look at the Inspector panel (right side)
3. Find **@Export** variables (they have export icon)
4. Click to change values!

### Change Sound Volume
1. Open `Globals/AudioManager.cs` in Godot
2. In Inspector, find **Master Volume** setting
3. Adjust the slider

### Change Monetization (AdMob IDs)
1. Open `project.godot` file in a text editor
2. Find the `[monetization]` section
3. Replace empty strings with your IDs:
   ```
   admob/app_id="YOUR_APP_ID"
   admob/banner_ad_unit_id="YOUR_BANNER_ID"
   admob/interstitial_ad_unit_id="YOUR_INTERSTITIAL_ID"
   admob/rewarded_ad_unit_id="YOUR_REWARDED_ID"
   ```

### Change Difficulty
1. Open `Globals/LevelGenerator.cs` in Godot
2. In Inspector, find **Difficulty Scale** settings
3. Adjust the numbers

---

## 🔍 How to Find What You Need

### "I want to change the main menu"
→ Open: `Scenes/Main/MainMenu.tscn`

### "I want to change level 5"
→ Open: `Scenes/Levels/Room005.tscn`

### "I want to change the slingshot power"
→ Open: `Script/Slingshot.cs` → Look at Inspector → Find **IMPULSE_MAX**

### "I want to add my own character sprite"
→ Replace: `Assets/Sprites/YourSprite.png` → Update in scene

### "I want to change background music"
→ Replace: `Assets/Audio/BackgroundMusic.ogg`

---

## 📋 What Each Scene Does

| Scene | What It Is | Safe to Edit? |
|-------|------------|---------------|
| `MainMenu.tscn` | Title screen, start buttons | ✅ Yes |
| `RoomSelection.tscn` | Level picker grid | ✅ Yes |
| `Room001.tscn` ... `Room100.tscn` | The 100 game levels | ✅ Yes |
| `ProceduralRoom.tscn` | Procedurally generated levels | ⚠️ Template only |
| `LevelCompleted.tscn` | Victory screen | ✅ Yes |
| `GameHud.tscn` | In-game HUD (score, rage) | ✅ Yes |
| `PausePanel.tscn` | Pause menu | ✅ Yes |
| `StickClone.tscn` | Player character | ✅ Yes (sprites only) |
| `Cup.tscn` | Destructible cup | ✅ Yes (sprites only) |

---

## 🔧 How the Game Works (High-Level)

### Game Flow:
1. **MainMenu** → Player clicks Play
2. **RoomSelection** → Player selects level
3. **RoomBase** (a level room)
   - Slingshot phase: Player drags, launches projectile
   - Traversal phase: Player moves StickClone to door
4. **LevelCompleted** → Shows stars, score, next level button
5. Back to next room or room selection

### Key Systems:
- **GameManager**: Controls what scene is loaded
- **SignalManager**: Lets objects talk to each other
- **AudioManager**: Plays all sounds and music
- **EffectsManager**: Particle effects, screen shake
- **GameFeelManager**: Combines all polish (shake + particles + haptics)
- **HapticFeedbackManager**: Vibration on mobile devices

---

## 🚀 Quick Reference Card

```
┌─────────────────────────────────────────────┐
│  GODOT BEGINNER CHEAT SHEET                │
├─────────────────────────────────────────────┤
│                                             │
│  F5  → Play game                           │
│  F6  → Play current scene                   │
│  F11 → Fullscreen editor                    │
│                                             │
│  Ctrl+S → Save scene                        │
│  Ctrl+D → Duplicate node                    │
│  Ctrl+Z → Undo                              │
│                                             │
│  🎨 Change colors? → Edit scene, modify nodes │
│  🔊 Change sounds? → Replace audio files     │
│  🎮 Change levels? → Open Room###.tscn       │
│                                             │
│  ⚠️ NEVER touch: GameManager, SignalManager │
│  ✅ SAFE to edit: Scenes, Assets           │
│                                             │
└─────────────────────────────────────────────┘
```

---

## 📚 Next Steps

- Read **QUICK_START.md** for getting started in 5 minutes
- Read **INSPECTOR_TOUR.md** to understand the Inspector panel
- Read **COMMON_CHANGES.md** for the top 10 things beginners change
- Read **SETTINGS_REFERENCE.md** for what each setting does

---

## 🆘 When You Get Stuck

1. Check the **NON_CODER_GUIDE.md** for detailed explanations
2. Look at existing levels to see how they're set up
3. Try small changes first (like moving a cup)
4. Always test with **F5** after changes

---

## ⭐ Golden Rule

> **If you didn't write the code, and you don't know what it does, DON'T TOUCH THE `.cs` FILES!**

Only edit:
- ✅ `.tscn` scenes (visual editor)
- ✅ `.png`, `.wav`, `.ogg` files (replace assets)
- ✅ `.md` files (documentation)

Good luck, and have fun! 🎮✨
