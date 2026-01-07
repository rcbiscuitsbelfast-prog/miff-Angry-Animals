# Inspector Panel Tour 🔍
## Your Control Center for Customizing Angry Animals

> **The Inspector is your best friend!** Everything marked with **@Export** is designed for you to change.

---

## 🎯 What is the Inspector Panel?

The Inspector panel is on the **right side** of Godot. It shows all properties and settings for the selected object.

### When to Use It:
- ✅ Changing game settings (volume, difficulty, etc.)
- ✅ Adjusting object positions and sizes
- ✅ Changing colors and visual properties
- ✅ Connecting signals (events)
- ✅ Adding/removing components

---

## 📍 Where to Find It

```
┌─────────────────────────────────────────────┐
│  Godot Editor Window                    │
│                                         │
│  ┌──────────┐        ┌──────────────┐ │
│  │ Scene    │        │   INSPECTOR  │ │ ← THIS!
│  │ Panel    │        │   PANEL      │ │
│  │ (Left)   │        │   (Right)    │ │
│  └──────────┘        └──────────────┘ │
│                                         │
│  ┌──────────────────────────────────┐     │
│  │ FileSystem                      │     │
│  │ Panel (Bottom Left)            │     │
│  └──────────────────────────────────┘     │
│                                         │
└─────────────────────────────────────────────┘
```

---

## 🔍 How to Open Something in Inspector

1. **Open a Scene File:**
   - Go to **FileSystem** (bottom left)
   - Double-click any `.tscn` or `.cs` file
   - Scene opens in center view
   - Inspector shows properties

2. **Open a Script:**
   - Double-click `.cs` file in FileSystem
   - Inspector shows `@Export` variables

---

## 🎨 Inspector Panel Layout

```
┌─────────────────────────────────────────────────┐
│  Inspector Panel                             │
├─────────────────────────────────────────────────┤
│                                             │
│  🔵 Node: GameManager                      │ ← Selected object
│                                             │
│  ┌─ ▶ Inspector ─────────────────────────┐  │
│  │                                     │  │
│  │  [General Settings]                  │  │ ← Category/Header
│  │                                     │  │
│  │  📊 TotalLevels: 100               │  │ ← Setting
│  │  📊 FreeLevels: 20                │  │
│  │  📊 MainScenePath: res://...       │  │
│  │                                     │  │
│  │  [Monetization Settings]             │  │
│  │                                     │  │
│  │  ✅ ShowAds: ☑️ true               │  │ ← Checkbox
│  │  ✅ ShowIap: ☑️ true               │  │
│  │  💰 FullGamePrice: 0.99            │  │ ← Number field
│  │                                     │  │
│  └─────────────────────────────────────┘  │
│                                             │
│  ▶ Node  ▶ Groups  ▶ Signals             │  │ ← Tabs
└─────────────────────────────────────────────────┘
```

---

## 🏷️ Understanding Icons

| Icon | Meaning | Example |
|------|---------|---------|
| 📊 | Number input field | `100`, `0.99` |
| 🔤 | Text input field | `"Angry Animals"` |
| ✅ | Checkbox (true/false) | `true`, `false` |
| 🎨 | Color picker | Colors, tints |
| 📁 | File path selector | `res://Scenes/Main.tscn` |
| 🎵 | Audio stream selector | Music files, SFX |
| 🖼️ | Texture selector | Sprites, images |
| 🔄 | Vector2 (X, Y) | Position, size |
| 🔢 | Array/List | Multiple items |
| ⚙️ | Resource | Config files, presets |

---

## 📋 Inspector Tabs

### 1. Inspector (Default)
Shows all properties and `@Export` variables. **Most used tab!**

### 2. Node
Shows node hierarchy and connections. Rarely needed.

### 3. Groups
Shows groups this node belongs to. For advanced users.

### 4. Signals
Shows events this node can send/receive. For advanced users.

**Tip:** Stay in **Inspector** tab most of the time!

---

## 🎯 Common Inspector Sections

### Section: [General Settings]
Most scripts have this section with core settings.

**Example from GameManager:**
```
┌─ [General Settings] ──────────────────────┐
│                                           │
│  TotalLevels: [100] 📊                   │
│  FreeLevels: [20] 📊                      │
│  MainScenePath: [res://...] 📁            │
│                                           │
└───────────────────────────────────────────┘
```

### Section: [Volume Settings]
Audio-related settings.

**Example from AudioManager:**
```
┌─ [Volume Settings] ───────────────────────┐
│                                           │
│  MasterVolume: [1.0] 📊                 │
│  MusicVolume: [0.7] 📊                  │
│  SfxVolume: [0.8] 📊                    │
│                                           │
└───────────────────────────────────────────┘
```

### Section: [Export]
Variables marked with `[Export]` in code. These are safe to change!

**Example from GameFeelManager:**
```
┌─ [General Settings] ──────────────────────┐
│                                           │
│  ✅ EnableScreenShake: ☑️ true          │
│  ✅ EnableParticles: ☑️ true             │
│  ✅ EnableSlowMotion: ☐ false           │
│                                           │
│  SlingshotChargeDuration: [0.3] 📊         │
│  HeavyImpactThreshold: [500.0] 📊          │
│                                           │
└───────────────────────────────────────────┘
```

---

## 🎨 Working with Colors

### Color Picker in Inspector:

```
┌─ Color Picker ─────────────────────────────┐
│                                           │
│  ◯ Color Preview                        │
│                                           │
│  H: [0]   🌈 Hue (0-360 rainbow)      │
│  S: [1.0] 🌈 Saturation (0-1)          │
│  V: [1.0] 🌈 Value/Brightness (0-1)     │
│  A: [1.0] 🌈 Alpha/Opacity (0-1)      │
│                                           │
│  R: [255] G: [0] B: [0]              │
│                                           │
│  🎨 Preset Colors:                      │
│  [🔴] [🟠] [🟡] [🟢] [🔵] [🟣]  │
│                                           │
└───────────────────────────────────────────┘
```

### How to Change a Color:
1. Click color preview circle (◯)
2. Color picker opens
3. Adjust H/S/V or R/G/B sliders
4. Click **OK** or click outside

### Quick Color Tips:
- **H (Hue)** = Color on rainbow (0=red, 120=green, 240=blue)
- **S (Saturation)** = How colorful (0=gray, 1=vibrant)
- **V (Value)** = Brightness (0=black, 1=white)
- **A (Alpha)** = Opacity (0=invisible, 1=solid)

---

## 📁 Working with File Paths

### File Path Selector:

```
┌─ File Path ───────────────────────────────┐
│                                           │
│  res://Scenes/Main/Main.tscn           │
│  [📁] Browse...  [🔄] Clear           │
│                                           │
└───────────────────────────────────────────┘
```

### How to Change a File Path:
1. Click **[📁] Browse...** button
2. File dialog opens
3. Navigate to file
4. Select and click **Open**

### Examples:
- **Audio files**: Select `.ogg` or `.wav` files
- **Scenes**: Select `.tscn` files
- **Sprites**: Select `.png` or `.jpg` files
- **Resources**: Select `.tres` or `.res` files

---

## 🎵 Working with Audio Streams

### Audio Stream Selector:

```
┌─ Audio Stream ────────────────────────────┐
│                                           │
│  [New AudioStream]                        │
│  Type: ▼ [AudioStreamOggVorbis]          │
│                                           │
│  📁 Load...  [🔄] Clear                 │
│                                           │
│  ▼ Volume: 1.0                           │
│  ▼ Pitch Scale: 1.0                       │
│                                           │
└───────────────────────────────────────────┘
```

### How to Add Audio:
1. Click **[📁] Load...** button
2. Navigate to `Assets/Audio/` folder
3. Select `.ogg` or `.wav` file
4. Adjust Volume and Pitch Scale if needed

### Supported Formats:
- **Music**: `.ogg` (Vorbis) recommended
- **SFX**: `.wav` or `.ogg`

---

## 🔢 Working with Numbers

### Number Input Fields:

```
┌─ Number Input ────────────────────────────┐
│                                           │
│  MasterVolume: [1.0] 📊                │
│              ↑↓                           │
│  [0.5]  [0.75]  [1.0]  [1.25]       │ ← Quick options
│                                           │
└───────────────────────────────────────────┘
```

### How to Change Numbers:
1. Click number field
2. Type new value
3. Press **Enter** or click outside

### Tips:
- **↑↓** arrows: Fine-tune value
- **Drag**: Click and drag slider for quick adjustment
- **Quick options**: Click preset buttons for common values

### Common Ranges:
- **Volumes**: 0.0 to 1.0
- **Durations**: 0.0 to 10.0 seconds
- **Counts**: 1 to 100
- **Multipliers**: 0.1 to 2.0

---

## ✅ Working with Checkboxes

### Checkbox Fields:

```
┌─ Checkbox ────────────────────────────────┐
│                                           │
│  ✅ EnableScreenShake: ☑️ true        │
│  ✅ EnableParticles: ☐ false          │
│                                           │
└───────────────────────────────────────────┘
```

### How to Toggle:
1. Click checkbox
2. ☑️ = enabled (true)
3. ☐ = disabled (false)

### Common Toggles:
- **Enable...**: Turn features on/off
- **Use...**: Enable/disable features
- **Show...**: Display/hide elements

---

## 🔄 Working with Vector2

### Vector2 Input (X, Y):

```
┌─ Vector2 Input ──────────────────────────┐
│                                           │
│  Position: X: [100]  Y: [200]          │
│            ↓         ↓                     │
│  ┌──────────────┬──────────────┐        │
│  │              │              │        │
│  └──────────────┴──────────────┘        │
│                                           │
└───────────────────────────────────────────┘
```

### How to Change Vector2:
1. Click X or Y field
2. Type new value
3. Press **Enter**

### What Vector2 Represents:
- **Position**: Location in 2D space
- **Size**: Width and height
- **Scale**: X and Y scale multiplier
- **Offset**: X and Y offset

### Tips:
- **Position (0, 0)** = Top-left corner
- **Positive X** = Right
- **Positive Y** = Down (Godot coordinate system)

---

## 🎯 Finding Specific Settings

### Method 1: Search
1. In Inspector, use keyboard shortcut **Ctrl+F**
2. Type setting name
3. Inspector highlights matching settings

### Method 2: Expand/Collapse
1. Click section headers (▶ / ▼) to expand/collapse
2. Find section you want
3. Expand it

### Method 3: Read Code Comments
1. Open `.cs` file in a text editor
2. Read comments above `[Export]` variables
3. Find same variable name in Inspector

---

## 📱 Inspector for Mobile Optimization

### Button Size Settings:
```
┌─ Control ───────────────────────────────┐
│                                           │
│  CustomMinimumSize: X: [100]  Y: [100]  │
│                    ↑          ↑           │
│              Width (px)  Height (px)      │
│                                           │
└───────────────────────────────────────────┘
```

**Recommended Mobile Sizes:**
- Buttons: 100x100 minimum
- Touch targets: 80x80 minimum
- Spacing: 20-40 pixels between elements

---

## 🔍 Real Examples

### Example 1: Change Music Volume
1. Double-click `Globals/AudioManager.cs`
2. In Inspector, find **[Volume Settings]**
3. Click **MusicVolume** field
4. Type `0.5` (50% volume)
5. Press **Enter**
6. Test with F5!

### Example 2: Make Slingshot More Powerful
1. Double-click `Script/Slingshot.cs`
2. In Inspector, scroll to find constants
3. Click **IMPULSE_MAX** field
4. Type `1500` (was 1200)
5. Press **Enter**
6. Save and test with F6!

### Example 3: Disable Screen Shake
1. Double-click `Script/GameFeelManager.cs`
2. In Inspector, find **[General Settings]**
3. Uncheck **EnableScreenShake** checkbox
4. Save and test with F5!

### Example 4: Change Button Color
1. Double-click `Scenes/Main/MainMenu.tscn`
2. In Scene panel, click a button
3. In Inspector, find **Modulate** under **CanvasItem**
4. Click color circle
5. Pick new color (e.g., blue)
6. Click **OK**
7. Save and test with F5!

---

## ⚠️ Common Mistakes to Avoid

### Mistake 1: Editing Non-Export Variables
❌ Changing `_internalVariable` (underscore prefix)
✅ Changing `ExportVariable` (no underscore)

### Mistake 2: Wrong File Type
❌ Trying to load `.mp3` for music (use `.ogg`)
✅ Using `.ogg` or `.wav` files

### Mistake 3: Invalid Values
❌ Setting volume to `5.0` (max is 1.0)
✅ Setting volume to `0.5` or `0.8`

### Mistake 4: Forgetting to Save
❌ Changing settings, closing file, losing changes
✅ Always press **Ctrl+S** after changes

---

## 🎓 Tips and Tricks

### Tip 1: Use Preset Buttons
Many number fields have quick preset buttons. Click them for common values!

### Tip 2: Read Tooltips
Hover over setting names to see tooltips with descriptions.

### Tip 3: Use Drag Sliders
For number fields, click and drag left/right for quick adjustments.

### Tip 4: Reset to Default
Right-click a setting → **Reset to Default** to undo changes.

### Tip 5: Copy/Paste Values
Ctrl+C on a setting, select another, Ctrl+V to paste.

---

## 📚 Next Steps

- **SETTINGS_REFERENCE.md**: What every setting does
- **COMMON_CHANGES.md**: Top 10 things to change
- **GODOT_BEGINNER_MAP.md**: Complete beginner guide

---

## 🎉 You're an Inspector Pro!

Now you can navigate and customize Angry Animals like a pro. The Inspector is your control center - don't be afraid to experiment!

> **Remember:** Always test changes with F5/F5! 🎮✨
