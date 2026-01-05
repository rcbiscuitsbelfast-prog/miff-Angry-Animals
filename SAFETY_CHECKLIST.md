# Safety Checklist ⚠️
## What NOT to Touch (Don't Break Your Game!)

> **IMPORTANT:** Follow this checklist to avoid breaking Angry Animals. If something breaks, use **Ctrl+Z** to undo!

---

## 🔴 NEVER DELETE These Files

### Core Game Files (Will Break Game)
- ❌ `project.godot` - Project configuration (edit values, don't delete)
- ❌ `Globals/GameManager.cs` - Game flow controller
- ❌ `Globals/SignalManager.cs` - Communication system
- ❌ `Globals/PlayerProfile.cs` - Save/load system
- ❌ `Globals/FileManager.cs` - File operations
- ❌ `Globals/Globals.cs` - Core system manager

### Build Files (Will Prevent Compilation)
- ❌ `AngryAnimals.csproj` - C# project file
- ❌ `AngryAnimals.sln` - Solution file
- ❌ `.git/` folder - Git version control

### Export Files
- ❌ `export_presets.cfg` - Export configuration (edit values, don't delete)

---

## ⚠️ NEVER CHANGE These Things

### Singleton References (Will Crash Game)
- ❌ `public static GameManager Instance { get; private set; }` - Singleton pattern
- ❌ Any `public static Instance` properties - Don't modify!
- ❌ Autoload paths in `project.godot` - Don't add/remove singletons

### Signal Connections (Will Break Communication)
- ❌ Don't remove signal connections in `_Ready()` methods
- ❌ Don't add signals to `[Signal]` declarations (unless you know C#)
- ❌ Don't change signal names (they're used across entire game)

### File Paths (Will Break Scene Loading)
- ❌ Don't change hardcoded scene paths like `res://Scenes/Levels/Room001.tscn`
- ❌ Don't change autoload paths in `project.godot`

### Constants (Will Break Game Balance)
- ⚠️ Be careful changing `const` values (marked with `const` keyword)
- ⚠️ Especially physics constants (gravity, impulse limits)

---

## ✅ SAFE TO EDIT (Beginner Friendly)

### Scene Files (.tscn)
- ✅ `Scenes/Levels/Room001.tscn` through `Room100.tscn` - Move objects, add/remove cups
- ✅ `Scenes/Main/MainMenu.tscn` - Change layout, colors, buttons
- ✅ `Scenes/Rooms/RoomSelection.tscn` - Adjust level grid
- ✅ `Scenes/UI/GameHud.tscn` - Change HUD layout
- ✅ `Scenes/Characters/StickClone.tscn` - Change sprite only
- ✅ `Scenes/Obstacles/Cup.tscn` - Change sprite only

### Asset Files
- ✅ All `.png`, `.jpg`, `.svg` in `Assets/Sprites/` - Replace sprites
- ✅ All `.ogg`, `.wav` in `Assets/Audio/` - Replace sounds/music

### Configuration Values (via Inspector)
- ✅ Volume settings in `AudioManager.cs`
- ✅ Slingshot power in `Slingshot.cs`
- ✅ Difficulty in `LevelGenerator.cs`
- ✅ Show Ads toggle in `MonetizationManager.cs`
- ✅ Screen shake settings in `EffectsManager.cs`
- ✅ Game feel settings in `GameFeelManager.cs`

### Project Settings
- ✅ Game name in `project.godot` under `[application]`
- ✅ Game version in `project.godot` under `[application]`
- ✅ AdMob IDs in `project.godot` under `[monetization]`
- ✅ Package name for Android/iOS exports

---

## 🟡 USE CAREFULLY (Intermediate)

### These Require Understanding
- ⚠️ Physics constants in scripts (`const float GRAVITY`, etc.)
- ⚠️ Level generation parameters (affects all procedural levels)
- ⚠️ Star rating logic (requires C# knowledge)
- ⚠️ UI layout files (can break if not careful)

### Tips for Careful Editing:
1. Make backup before editing
2. Test frequently with F5/F6
3. Use Ctrl+Z if something breaks
4. Read code comments before changing

---

## 🔍 How to Identify What's Safe

### Rule 1: Check for @Export
- ✅ Variables marked `[Export]` are safe to edit in Inspector
- ❌ Variables with `_` prefix (like `_internalVar`) are internal - don't touch

### Rule 2: Check File Extension
- ✅ `.tscn` (scenes) - Safe to open and edit
- ✅ `.png`, `.ogg`, `.wav` (assets) - Safe to replace
- ⚠️ `.cs` (scripts) - Only edit via Inspector, not text editor
- ❌ `.csproj`, `.sln` (build files) - Don't touch

### Rule 3: Check Documentation
- ✅ Refer to **SETTINGS_REFERENCE.md** for what settings do
- ✅ Follow **COMMON_CHANGES.md** for safe changes
- ✅ Use **INSPECTOR_TOUR.md** to navigate Inspector

---

## 🚨 DANGER ZONES (Will Break Game)

### Zone 1: Autoloaded Singletons
**Files:** All files in `Globals/` folder except `.cs` files

**Don't Touch:**
- Singleton pattern: `Instance = this;`
- Signal connections in `_Ready()`
- File paths and references

**If You Must Edit:**
1. Make backup copy of file
2. Only change `[Export]` variables via Inspector
3. Test extensively

### Zone 2: Game State Management
**Files:** `GameManager.cs`, `SignalManager.cs`, `PlayerProfile.cs`

**Don't Touch:**
- State machine logic
- Save/load system
- Signal dispatching

### Zone 3: Physics System
**Files:** `Slingshot.cs`, `Projectile.cs`, `RoomBase.cs`

**Don't Touch:**
- Physics integration methods (`_PhysicsProcess`, `_IntegrateForces`)
- Collision detection logic
- Impulse calculation (unless adjusting slingshot power)

### Zone 4: Build System
**Files:** `.csproj`, `.sln`, `.godot/` folder

**Don't Touch:**
- C# project configuration
- Godot's internal `.godot/` folder
- Git's `.git/` folder

---

## ⚡ Emergency Recovery

### If You Deleted Something Important:

1. **Don't panic!**
2. Use Git to restore: `git checkout -- <filename>`
3. Or restore from backup (if you made one)
4. Or re-download project from repository

### If You Broke Code:

1. Press **Ctrl+Z** to undo last change
2. Or close file without saving
3. Re-open file and try again

### If Game Won't Compile:

1. Check **Output** panel for errors
2. Look for red error messages
3. Check what you changed last
4. Undo (Ctrl+Z) and try again

### If Game Crashes:

1. Check if you edited a `.cs` file
2. Review your changes
3. Undo changes and test
4. Check for null references in Output panel

---

## 📋 Pre-Edit Checklist

Before making changes, check:

- [ ] **I know what this setting does** (read documentation)
- [ ] **I have a backup** (copy project folder)
- [ ] **I'm editing via Inspector** (not code editor, unless experienced)
- [ ] **I can test changes** (press F5 after editing)
- [ ] **I can undo if needed** (Ctrl+Z ready)

---

## ✅ Safe Editing Workflow

1. **Identify what you want to change**
   - Read **SETTINGS_REFERENCE.md** or **COMMON_CHANGES.md**

2. **Open the right file**
   - Scene files: Double-click `.tscn` in FileSystem
   - Scripts: Double-click `.cs` in FileSystem

3. **Edit via Inspector**
   - Don't open `.cs` files in code editor!
   - Use Inspector panel on the right side
   - Only change `[Export]` variables

4. **Save your changes**
   - Press **Ctrl+S** to save

5. **Test immediately**
   - Press **F5** (full game) or **F6** (current scene)
   - Verify it works as expected

6. **If something breaks:**
   - Press **Ctrl+Z** to undo
   - Or close without saving

---

## 🎓 Learning Path

### Level 1: Beginner (Safe)
- Change volumes in `AudioManager.cs`
- Adjust slingshot power in `Slingshot.cs`
- Move objects in level scenes
- Replace sprites and sounds

### Level 2: Intermediate (Careful)
- Adjust difficulty in `LevelGenerator.cs`
- Change UI layouts
- Modify physics constants slightly

### Level 3: Advanced (Danger)
- Edit game logic in `.cs` files
- Add new features
- Modify state machines
- Change physics system

**Advice:** Stay in Level 1 and 2 unless you're learning C#!

---

## 🆘 When to Ask for Help

### Seek Help If:
- You're unsure what a setting does
- You get an error message you don't understand
- Game behaves unexpectedly after changes
- You want to make a major modification

### Resources:
- **GODOT_BEGINNER_MAP.md** - General guidance
- **SETTINGS_REFERENCE.md** - Setting descriptions
- **Godot Docs** - [docs.godotengine.org](https://docs.godotengine.org)
- **Godot Community** - Discord, Reddit, forums

---

## 📊 Decision Tree

```
Do you want to change something?
│
├─ No → Don't touch anything! ✅
│
└─ Yes → What do you want to change?
    │
    ├─ Volume/Power/Difficulty?
    │   └─ Edit via Inspector in .cs files ✅ SAFE
    │
    ├─ Move objects in levels?
    │   └─ Open .tscn scene files ✅ SAFE
    │
    ├─ Replace sprites/sounds?
    │   └─ Replace asset files ✅ SAFE
    │
    ├─ Game logic/new features?
    │   └─ Requires C# knowledge ⚠️ CAREFUL
    │
    └─ Core system (GameManager, etc.)?
        └─ Don't touch! ❌ DANGER
```

---

## 🎯 Golden Rules

### Rule #1: Backup First
Before making big changes, copy your project folder as backup!

### Rule #2: Test Frequently
After every change, press F5 to test!

### Rule #3: Use Inspector
Edit `.cs` files via Inspector, not code editor!

### Rule #4: Read Documentation
Check **SETTINGS_REFERENCE.md** before changing unknown settings!

### Rule #5: Ctrl+Z is Your Friend
Don't be afraid to undo if something breaks!

---

## 🎉 Safe Customizing!

Follow this checklist and you'll be able to customize Angry Animals without breaking it!

> **Remember:** Better to make small changes and test than big changes and break the game!

Start with safe changes (volumes, slingshot power) and build your confidence! 🎮✨
