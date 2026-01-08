# SETTINGS_GUIDE.md

**Non-Coder Guide: Adjusting Game Settings Without Code**

This guide shows you how to tweak game settings without editing any C# code. All settings can be adjusted through the Inspector in Godot Editor.

---

## 🎮 Quick Access Settings

### **Main Settings Categories:**

1. **Physics Settings** - How the game feels to play
2. **UI/Transition Settings** - Animation speeds and fade effects  
3. **Difficulty Settings** - Challenge balance
4. **Audio Settings** - Volume controls
5. **Visual Settings** - Effects and accessibility
6. **Unlockable Modifiers** - Fun gameplay variations

---

## 🔧 Physics Settings

These control how the slingshot, projectiles, characters, and ragdolls behave.

### **Slingshot Controls:**
- **Slingshot Impulse Multiplier**: Higher = more powerful shots (Default: 20.0)
  - Easy Mode: 25.0
  - Hard Mode: 16.0
- **Slingshot Impulse Max**: Maximum shot power (Default: 1200.0)
- **Slingshot Drag Max**: How far you can pull back (Default: 60.0)
- **Slingshot Drag Min**: Minimum pull to shoot (Default: 10.0)

### **Projectile Physics:**
- **Projectile Stopped Threshold**: When projectile is considered "stopped" (Default: 0.1)
- **Projectile Gravity Scale**: How much gravity affects shots (Default: 1.0)
- **Projectile Bounce Coefficient**: How bouncy projectiles are (Default: 0.7)

### **Character Movement:**
- **Character Move Speed**: How fast characters walk (Default: 200.0)
- **Character Jump Force**: How high characters jump (Default: 400.0)
- **Character Acceleration**: How quickly characters reach max speed (Default: 1500.0)

### **Ragdoll Physics:**
- **Ragdoll Joint Stiffness**: How rigid ragdolls are (Default: 0.7)
  - 0.1 = very loose and floppy
  - 1.0 = very stiff and realistic
- **Ragdoll Angular Damping**: How much ragdolls spin (Default: 3.0)
- **Ragdoll Linear Damping**: How fast ragdolls slow down (Default: 2.0)
- **Ragdoll Lifetime**: How long ragdolls exist (Default: 8.0 seconds)

---

## 🎨 UI & Transition Settings

These control animation speeds and visual effects.

### **Level Complete Effects:**
- **Level Complete Fade Duration**: How long fade takes (Default: 1.0 seconds)
- **Level Complete Fade Color**: Color of fade effect (Default: Black)
- **Score Screen Hold Duration**: How long score screen stays (Default: 3.0 seconds)

### **Menu Animations:**
- **Menu Transition Speed**: Speed of menu changes (Default: 0.3 seconds)
- **Settings Panel Fade In/Out**: How fast settings appear/disappear (Default: 0.3/0.2 seconds)

### **Star Animation:**
- **Star Animation Duration**: How long star animation takes (Default: 0.3 seconds)
- **Star Bounce Scale**: How much stars bounce (Default: 1.3x)

---

## ⚡ Difficulty Settings

Fine-tune the challenge level.

### **Overall Balance:**
- **Base Difficulty Multiplier**: Global difficulty scaling (Default: 1.0)
- **Enemy Health Multiplier**: How tough enemies are (Default: 1.0)
- **Enemy Damage Multiplier**: How much damage enemies deal (Default: 1.0)

### **Scoring:**
- **Room Target Score Offset**: Adjust target scores up/down (Default: 0)
  - Negative values = easier (e.g., -10)
  - Positive values = harder (e.g., +15)
- **Perfect Score Threshold**: % of optimal score for 3 stars (Default: 0.9 = 90%)
- **Good Score Threshold**: % of optimal score for 2 stars (Default: 0.6 = 60%)

### **Difficulty Presets:**
Instead of adjusting individual settings, use these quick presets:
- **Easy**: More powerful slingshot, fewer enemies, lower targets
- **Normal**: Balanced default settings
- **Hard**: Weaker slingshot, tougher enemies, higher targets  
- **Extreme**: Very challenging for hardcore players

---

## 🔊 Audio Settings

Volume controls for different sound types.

### **Volume Sliders:**
- **Master Volume**: Overall game volume (Default: 1.0)
- **Music Volume**: Background music (Default: 0.7)
- **SFX Volume**: Sound effects (Default: 1.0)
- **Voice Volume**: Character voices (Default: 0.8)

### **Audio Options:**
- **Enable Impact Vocals**: Character sounds on hits (Default: On)
- **Max Simultaneous Sounds**: Limit overlapping sounds (Default: 3)

---

## 👁️ Visual Settings

Effects and accessibility options.

### **Visual Effects:**
- **Screen Shake Intensity**: How much screen shakes on impact (Default: 1.0)
- **Particle Density**: Amount of visual particles (Default: 1.0)
- **UI Animation Speed**: Speed of interface animations (Default: 1.0)

### **Accessibility:**
- **Colorblind Mode**: Color adjustments for colorblind players (Default: Off)
- **High Contrast Mode**: Higher contrast for visibility (Default: Off)
- **Reduce Motion**: Less intense animations (Default: Off)
- **Text Scale**: Size of UI text (Default: 1.0)

### **Feedback:**
- **Haptic Feedback**: Vibration on supported devices (Default: On)

---

## 🎮 Unlockable Gameplay Modifiers

Fun modifiers you unlock through achievements. These are disabled by default.

### **Available Modifiers:**

#### **Extreme Physics** 🌀
- **Effect**: Double ragdoll intensity, chaotic physics
- **Unlock**: Complete 20 levels without using slingshot second time
- **Toggle**: Enable/disable in Settings → Modifiers tab

#### **Big Heads** 😄
- **Effect**: Comically enlarged character heads
- **Unlock**: Achieve 10 perfect scores (3-star levels)
- **Visual**: Purely cosmetic but hilarious

#### **Double Explosions** 💥
- **Effect**: Each hit creates two explosions
- **Unlock**: Get 5 consecutive perfect scores
- **Gameplay**: Significantly more chaotic

#### **Slow Motion** ⏰
- **Effect**: Spend "time tokens" to slow gameplay
- **Unlock**: Complete 30 total levels
- **Usage**: Press special button during gameplay

#### **No Gravity** 🎈
- **Effect**: Ragdoll limbs float like balloons
- **Unlock**: Complete 5 tutorial levels perfectly
- **Visual**: Physics take a vacation

#### **Colorful Mode** 🌈
- **Effect**: Bright neon character colors
- **Unlock**: Complete 50 total levels
- **Visual**: Makes everything more vibrant

#### **Hardcore Mode** 🔥
- **Effect**: Disables all other modifiers
- **Purpose**: For serious speedrunning
- **Toggle**: Enable to play "pure" version

---

## 🛠️ How to Change Settings

### **Method 1: In-Game Settings Menu**
1. Start the game
2. Go to Main Menu → Settings
3. Adjust sliders and toggles
4. Changes apply immediately
5. Settings save automatically

### **Method 2: Inspector (Advanced)**
1. Open Godot Editor
2. Select `GameSettingsManager` node in the scene tree
3. In Inspector panel, find exported properties
4. Adjust values and save
5. Settings save automatically

### **Method 3: JSON File (Power Users)**
Settings are saved to: `user://game_settings.json`
- **Location**: Project folder → user:// directory
- **Format**: Human-readable JSON
- **Caution**: Only edit if you understand JSON format

---

## ⚠️ Important Notes

### **Settings That Require Restart:**
- Some physics changes work immediately
- Others need level restart to take effect
- Gravity-related settings usually need restart

### **Settings That Work Immediately:**
- All volume controls
- UI animation speeds
- Visual effect intensities
- Most accessibility options

### **Recommended Settings by Play Style:**

#### **Casual Players:**
- Easy difficulty preset
- Higher particle density
- Screen shake enabled
- All cosmetics unlocked

#### **Competitive Players:**
- Hard or Extreme difficulty
- Reduced motion effects
- Faster animations
- Hardcore mode enabled

#### **Accessibility:**
- High contrast mode
- Reduced motion
- Larger text scale
- Haptic feedback disabled

---

## 🔧 Troubleshooting

### **"Game feels too easy/hard"**
**Solution**: Adjust these 3 settings:
1. Slingshot Impulse Multiplier (±5 for noticeable change)
2. Enemy Health Multiplier (0.7 = easier, 1.3 = harder)
3. Room Target Score Offset (-10 = easier, +15 = harder)

### **"Transitions are too slow/fast"**
**Solution**: Adjust these 2 settings:
1. Menu Transition Speed (0.2 = fast, 0.5 = slow)
2. Level Complete Fade Duration (0.5 = quick, 2.0 = slow)

### **"Physics don't feel right"**
**Solution**: Try these presets:
- **Too floaty**: Increase Ragdoll Linear Damping to 3.0
- **Too stiff**: Decrease Ragdoll Joint Stiffness to 0.5
- **Too weak**: Increase Slingshot Impulse Multiplier to 25.0
- **Too strong**: Decrease Slingshot Impulse Multiplier to 16.0

### **"Audio is too loud/quiet"**
**Solution**: Adjust volume sliders:
- **Master Volume**: Overall game volume
- **Individual tracks**: Fine-tune specific sound types

---

## 📝 Custom Presets

Create your own difficulty presets by combining settings:

### **"Super Easy"**
- Slingshot Impulse: 30.0
- Enemy Health: 0.5
- Target Score Offset: -20
- Perfect Score Threshold: 0.8

### **"Challenging"**
- Slingshot Impulse: 14.0
- Enemy Health: 1.6
- Target Score Offset: +20
- Perfect Score Threshold: 0.95

### **"Physics Playground"**
- Extreme Physics: Enabled
- No Gravity: Enabled
- Double Explosions: Enabled
- High particle density

---

**Settings are automatically saved and persist between game sessions!**
