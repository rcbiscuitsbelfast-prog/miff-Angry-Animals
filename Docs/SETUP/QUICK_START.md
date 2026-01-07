# Quick Start Guide 🚀
## Get Angry Animals Running in 5 Minutes

> **For Non-Coders:** You don't need programming knowledge to play, test, or customize this game!

---

## ⏱️ 5-Minute Setup

### Step 1: Open Godot (1 minute)
1. Download and install **Godot Engine 4.4** or later from [godotengine.org](https://godotengine.org)
2. Open Godot Engine
3. Click **"Import"** button (bottom right)
4. Navigate to this project folder (`/home/engine/project`)
5. Select the folder and click **"Open"**
6. Godot will scan and import the project automatically

### Step 2: Test the Game (1 minute)
1. In Godot's top bar, click the **"Play"** button (▶️) or press **F5**
2. Game will launch in a new window
3. Try launching a projectile and completing a level
4. Press **ESC** to close game and return to Godot

### Step 3: Navigate Scenes (2 minutes)
1. Look at the **FileSystem** panel (bottom left)
2. Double-click `Scenes/Main/MainMenu.tscn` to open main menu
3. Double-click `Scenes/Levels/Room001.tscn` to open first level
4. Notice the **Scene** panel (top left) showing hierarchy
5. Notice the **Inspector** panel (right) showing properties

### Step 4: Make Your First Change (1 minute)
1. Open `Scenes/Levels/Room001.tscn`
2. In the **Scene** panel, click on a "Cup" node
3. In the **Inspector** panel (right), find **Position**
4. Change the **X** or **Y** values to move the cup
5. Press **Ctrl+S** to save
6. Press **F6** to play current scene
7. See your change! The cup moved! 🎉

---

## 🎯 Essential Godot Panels

### Scene Panel (Left)
Shows the hierarchy of objects in your scene. Like a family tree of game objects.

### Inspector Panel (Right)
Shows properties of the selected object. This is where you change values!

### FileSystem Panel (Bottom Left)
All your files and folders. Navigate to scenes and scripts.

### Output Panel (Bottom)
Shows messages, errors, and game logs (GD.Print).

---

## 🎮 Testing Your Changes

### Play Full Game
- **F5** → Plays from main scene (starts at main menu)

### Play Current Scene
- **F6** → Plays the scene you're editing (starts at that scene)

### Common Workflow:
1. Open a scene (e.g., `Room001.tscn`)
2. Make changes in Inspector
3. **Ctrl+S** to save
4. **F6** to test
5. Repeat!

---

## 🔧 10 Most Common Changes

### 1. Move Objects in Levels
- Open `Room###.tscn`
- Click object in Scene panel
- Change Position in Inspector

### 2. Change Game Colors
- Open `Globals/AudioManager.cs` (yes, even for colors!)
- Find **@Export** variables in Inspector
- Change color values

### 3. Adjust Slingshot Power
- Open `Script/Slingshot.cs`
- In Inspector, find **IMPULSE_MAX**
- Increase for more power

### 4. Change Volume
- Open `Globals/AudioManager.cs`
- Find **Master Volume** slider in Inspector
- Adjust to your liking

### 5. Add Your Own Sprites
- Put your `.png` files in `Assets/Sprites/`
- In Godot, drag them into a scene

### 6. Change Level Difficulty
- Open `Globals/LevelGenerator.cs`
- Find **Difficulty Scale** in Inspector
- Adjust numbers

### 7. Turn Off Ads
- Open `Globals/MonetizationManager.cs`
- Set **Show Ads** to `false` in Inspector

### 8. Change Game Name
- Open `project.godot` file in text editor
- Change `config/name="Angry Animals"` to your game name

### 9. Test Procedural Levels
- Open `Scenes/Rooms/RoomSelection.tscn`
- Click the "Procedural" toggle in Inspector
- Play game (F5) and try procedural levels

### 10. Export to Android/iOS
- Click **Project → Export** in top menu
- Select **Android** or **iOS**
- Configure settings and export!

---

## 📱 Testing on Mobile

### Export for Android:
1. Click **Project → Export**
2. Select **Android**
3. Configure package name and keystore
4. Click **Export Project**
5. Install `.aab` or `.apk` on device

### Export for iOS:
1. Click **Project → Export**
2. Select **iOS**
3. Configure team and bundle identifier
4. Click **Export Project**
5. Open in Xcode and build for device

---

## ⚠️ Safety Tips

### ✅ DO:
- Open `.tscn` scene files (visual editor)
- Change values in Inspector (right panel)
- Replace assets in `Assets/` folder
- Test frequently with F5/F6
- Save your work (Ctrl+S)

### ❌ DON'T:
- Edit `.cs` script files unless you know C#
- Delete files in `Globals/` folder
- Change autoloaded singletons in `project.godot`
- Modify `.git` folder

---

## 🎓 Learning Resources

If you want to learn more about Godot:

- **Godot Docs**: [docs.godotengine.org](https://docs.godotengine.org)
- **YouTube Tutorials**: Search "Godot 4 tutorial for beginners"
- **This Project's Docs**: See `NON_CODER_GUIDE.md` for detailed info

---

## 🔍 Finding Files

| What You Want | Where to Find |
|--------------|---------------|
| Change level 5 | `Scenes/Levels/Room005.tscn` |
| Change background music | Replace files in `Assets/Audio/` |
| Change character sprites | Replace files in `Assets/Sprites/` |
| Change game title | Edit `project.godot` |
| Add AdMob IDs | Edit `project.godot` under `[monetization]` |

---

## 🚨 Troubleshooting

### Game Won't Start
- Check that you're using Godot 4.x (not 3.x)
- Look at **Output** panel for errors
- Make sure `project.godot` exists in project folder

### Scene Looks Empty
- Make sure you selected the root node in Scene panel
- Check that nodes are visible (look for eye icon in Scene panel)

### Changes Don't Show
- Press **Ctrl+S** to save scene
- Press **F6** to reload scene

### Compilation Errors
- Usually means you edited a `.cs` file incorrectly
- Check Output panel for specific error
- Try Undo (Ctrl+Z) to revert changes

---

## 📚 Next Steps

- **Read GODOT_BEGINNER_MAP.md** - Complete beginner guide
- **Read INSPECTOR_TOUR.md** - Understand the Inspector panel
- **Read COMMON_CHANGES.md** - Top 10 things to change
- **Read SETTINGS_REFERENCE.md** - What each setting does

---

## 🎉 You're Ready!

You now know enough to:
- ✅ Open and play the game
- ✅ Make simple changes to levels
- ✅ Test your changes
- ✅ Export to mobile platforms

Have fun, and don't be afraid to experiment! 🎮✨

> **Remember:** If something breaks, you can always use **Ctrl+Z** to undo!
