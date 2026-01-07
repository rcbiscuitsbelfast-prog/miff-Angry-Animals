# Common Changes Guide 🔧
## Top 10 Things Non-Coders Want to Change

> **All of these can be done without touching code!** Just use the **Inspector** panel in Godot.

---

## 1. 🔊 Change Sound Volumes

### What: Make sounds louder or quieter

### Where to Change:
- Open: `Globals/AudioManager.cs`
- Look in Inspector panel (right side)

### Settings to Change:
- **Master Volume**: Overall game volume (0.0 = silent, 1.0 = full)
- **Music Volume**: Background music volume
- **Sfx Volume**: Sound effects volume (explosions, clicks, etc.)

### How:
1. Double-click `Globals/AudioManager.cs` in FileSystem
2. Find **@Export** labeled **General Settings** in Inspector
3. Adjust volume sliders
4. Press **F5** to test

---

## 2. 🎮 Adjust Slingshot Power

### What: Make projectiles fly faster or slower

### Where to Change:
- Open: `Script/Slingshot.cs`
- Look in Inspector panel

### Settings to Change:
- **IMPULSE_MAX**: Maximum launch speed (default: 1200.0)
- **IMPULSE_MULT**: Multiplier for launch force (default: 20.0)

### How:
1. Double-click `Script/Slingshot.cs`
2. In Inspector, find constants (they appear as variables)
3. Increase **IMPULSE_MAX** for more power
4. Decrease **IMPULSE_MAX** for less power
5. Test with F6

### Example Values:
- Easy mode: `IMPULSE_MAX = 1500.0` (more power, easier shots)
- Hard mode: `IMPULSE_MAX = 900.0` (less power, harder shots)
- Default: `IMPULSE_MAX = 1200.0`

---

## 3. 🎨 Change Game Colors/Theme

### What: Change colors for UI, menus, game elements

### Where to Change:
- Open: `Globals/AudioManager.cs` (yes, for colors too!)
- Or: Open UI scenes like `MainMenu.tscn`

### Settings to Change:
In UI scenes:
- **Modulate**: Overall color tint
- **SelfModulate**: Node's own color

### How:
1. Open `Scenes/Main/MainMenu.tscn`
2. Click a button in Scene panel
3. In Inspector, find **Modulate** under **CanvasItem** section
4. Click color picker to choose new color
5. Save (Ctrl+S) and test (F5)

### Tips:
- Use **H:** field for hue (0-360, rainbow colors)
- Use **S:** field for saturation (0-1, gray to vibrant)
- Use **V:** field for value (0-1, black to white)

---

## 4. 🎯 Change Level Difficulty

### What: Make levels easier or harder

### Where to Change:
- Open: `Globals/LevelGenerator.cs`
- Or: Open individual level files: `Scenes/Levels/Room###.tscn`

### Settings to Change (LevelGenerator.cs):
- **Difficulty Scale**: Multiplier for enemy count (1.0 = normal, 0.5 = easy, 2.0 = hard)
- **Cup Count**: Number of cups per level (3, 4, 5, or 6)

### How to Change a Specific Level:
1. Open `Scenes/Levels/Room010.tscn` (for level 10)
2. Click on cups in Scene panel
3. Delete cups to make level easier
4. Duplicate cups to make level harder
5. Move cups to change positions
6. Save (Ctrl+S) and test (F6)

---

## 5. 🚫 Turn Off Ads

### What: Disable all advertising in game

### Where to Change:
- Open: `Globals/MonetizationManager.cs`

### Settings to Change:
- **Show Ads**: Toggle to `false` (default: `true`)

### How:
1. Double-click `Globals/MonetizationManager.cs`
2. In Inspector, find **@Export** labeled **General Settings**
3. Uncheck **Show Ads** checkbox
4. Save (Ctrl+S) and test (F5)

### Why Would You Want This?
- Testing without ads
- Creating ad-free version
- Premium game release

---

## 6. 🏆 Change Star Rating System

### What: Adjust how many stars players earn

### Where to Change:
- Open: `Script/LevelCompleted.cs`
- Look at code (requires understanding a bit of C#)

### Settings to Change:
- In `CalculateStarCount()` method

### Current Logic (from LevelCompleted.cs):
```csharp
private int CalculateStarCount()
{
    // Simple star calculation
    if (_finalScore >= 3 * _bestScore)
        return 1;
    else if (_finalScore >= 2 * _bestScore)
        return 2;
    else if (_finalScore == _bestScore)
        return 3;
    else if (_finalScore <= _bestScore * 0.5)
        return 3; // Perfect score

    return 2; // Default
}
```

### How to Adjust:
This requires editing C# code. If you're comfortable:
- Change the multiplier values to adjust star thresholds
- `3 * _bestScore` means 3x the best score gets 1 star
- Lower multipliers = easier to get more stars

### Beginner Alternative:
Don't edit code! Just adjust level difficulty so players naturally get better scores.

---

## 7. 📱 Adjust Mobile Button Sizes

### What: Make buttons larger/smaller for mobile devices

### Where to Change:
- Open: `Scenes/UI/GameHud.tscn`
- Or: `Scenes/Main/MainMenu.tscn`

### Settings to Change:
- **Custom Minimum Size**: Minimum button size
- **Size**: Actual button size

### How:
1. Open `Scenes/UI/GameHud.tscn`
2. Click a button in Scene panel
3. In Inspector, find **Control** section
4. Expand **Layout** subsection
5. Change **Custom Minimum Size** values
6. Recommended: `X=100, Y=100` for better mobile touch targets
7. Save and test on mobile

---

## 8. 🎵 Change Music Tracks

### What: Replace background music with your own

### Where to Change:
- Files in: `Assets/Audio/`

### How:
1. Prepare your music file (must be `.ogg` format)
2. Place in `Assets/Audio/` folder
3. Open `Globals/AudioManager.cs`
4. In Inspector, find **@Export** labeled **Music**
5. Click to browse and select your `.ogg` file
6. Save and test (F5)

### Requirements:
- Format: OGG (Vorbis)
- Sample rate: 44.1kHz or 48kHz
- Channels: Stereo
- Bitrate: 128-192 kbps

---

## 9. 🚀 Add AdMob Ad IDs

### What: Configure real ads for release

### Where to Change:
- Open: `project.godot` file in a text editor

### Settings to Change:
Under `[monetization]` section:
```ini
admob/app_id="ca-app-pub-XXXXXXXXXXXXXXXX~YYYYYYYYYY"
admob/banner_ad_unit_id="ca-app-pub-XXXXXXXXXXXXXXXX/ZZZZZZZZZZ"
admob/interstitial_ad_unit_id="ca-app-pub-XXXXXXXXXXXXXXXX/AAAAAAAAAA"
admob/rewarded_ad_unit_id="ca-app-pub-XXXXXXXXXXXXXXXX/BBBBBBBBBB"
```

### How to Get IDs:
1. Go to [AdMob console](https://apps.admob.com)
2. Create an app
3. Create ad units (Banner, Interstitial, Rewarded)
4. Copy the IDs into `project.godot`
5. Save file

### Testing IDs:
Use test IDs while developing:
```ini
admob/app_id="ca-app-pub-3940256099942544~3347511713"
admob/banner_ad_unit_id="ca-app-pub-3940256099942544/6300978111"
admob/interstitial_ad_unit_id="ca-app-pub-3940256099942544/1033173712"
admob/rewarded_ad_unit_id="ca-app-pub-3940256099942544/5224354917"
```

---

## 10. 🎭 Change Character Sprites

### What: Use your own artwork for StickClone, enemies, etc.

### Where to Change:
- Files in: `Assets/Sprites/`

### How:
1. Create your sprite as a `.png` file
2. Place in `Assets/Sprites/` folder
3. Open the scene using that sprite (e.g., `Scenes/Characters/StickClone.tscn`)
4. In Scene panel, find the sprite node
5. In Inspector, click the sprite preview
6. Browse to select your new `.png` file
7. Save and test (F6)

### Requirements:
- Format: PNG (with transparency recommended)
- Size: Depends on your design
- Resolution: For 2D games, sprites are usually 32x32 to 128x128 pixels

### Tips:
- Use same name as original file to auto-replace
- Test sprite looks good in game
- Consider multiple frames for animations

---

## 📊 Quick Reference Table

| Change | File | Difficulty | Time Required |
|--------|-------|------------|---------------|
| Volume | `AudioManager.cs` | ⭐ Easy | 1 min |
| Slingshot Power | `Slingshot.cs` | ⭐ Easy | 1 min |
| Colors | Scene files | ⭐ Easy | 2 min |
| Level Difficulty | `LevelGenerator.cs` or level files | ⭐⭐ Medium | 5 min |
| Turn Off Ads | `MonetizationManager.cs` | ⭐ Easy | 30 sec |
| Star Ratings | `LevelCompleted.cs` | ⭐⭐⭐ Hard (code) | 10 min |
| Button Sizes | UI scenes | ⭐⭐ Medium | 3 min |
| Music | `Assets/Audio/` | ⭐ Easy | 5 min |
| AdMob IDs | `project.godot` | ⭐ Easy | 5 min |
| Character Sprites | `Assets/Sprites/` | ⭐⭐ Medium | 5 min |

---

## 🎓 Want to Learn More?

- **INSPECTOR_TOUR.md**: Detailed guide to Inspector panel
- **SETTINGS_REFERENCE.md**: What every setting does
- **GODOT_BEGINNER_MAP.md**: Complete beginner guide

---

## ⚠️ Important Reminders

### Always Test After Changes:
1. Save (Ctrl+S)
2. Play (F5 or F6)
3. Verify it works as expected

### If Something Breaks:
1. Use **Ctrl+Z** to undo
2. Or close without saving
3. Re-open the file

### Backup First:
Before making big changes, copy your project folder as backup!

---

## 🚀 Advanced Changes (Requires Coding)

These require C# knowledge:
- Adding new game modes
- Creating new power-ups
- Implementing multiplayer
- Changing physics behavior
- Adding new enemy types

If you want to do these, start learning C# and Godot GDScript!

---

## 🎉 You've Got This!

These 10 changes cover 90% of what non-coders want to do. Start with the easy ones (volume, slingshot power) and work your way up!

Good luck customizing your game! 🎮✨
