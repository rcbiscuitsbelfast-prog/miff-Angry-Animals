# Procedural Levels - Non-Coder Guide

## 🎲 What Are Procedural Levels?

**In Simple Terms:**  
Instead of manually designing all 100 levels one-by-one, the game can now **automatically generate unlimited levels** using mathematical formulas. These generated levels are:

- ✅ **Unique** - Each level number has a different layout
- ✅ **Reproducible** - Same seed always creates the same level
- ✅ **Sharable** - Friends can play your exact same layout
- ✅ **Infinite** - You're not limited to 100 levels anymore!

**Think of it like:**
- **Manual Levels** = Hand-drawn maps (100 carefully designed puzzles)
- **Procedural Levels** = AI-generated maps (infinite computer-created layouts)

---

## 🎮 How to Enable Procedural Levels

### For Players

1. **Launch the game** and go to the main menu
2. Click **"Select Room"** button
3. At the top of the room list, you'll see a checkbox: **"Procedural Levels: OFF/ON"**
4. **Toggle it ON** to enable procedural generation
5. Now when you click any level, it will generate a random cup layout instead of loading the pre-made design
6. **Toggle it OFF** to return to the original 100 hand-designed levels

### Seed Controls

When procedural mode is ON, you'll see additional controls:

#### **Seed Input Field**
- Enter **0** (default) = Each level has a unique deterministic layout
- Enter **any number** = Use that exact seed to generate the level
- Example: Seed `12345` on Level 1 will always create the same cup positions

#### **Buttons**
- **Random** - Generates a completely new random seed
- **Deterministic** - Resets to 0 (default behavior)
- **Use Last** - Loads the most recent seed you played

---

## 🎨 Visual Theme Progression

Procedural levels automatically change colors as you progress:

### **Levels 1-30: Blue Theme**
- Light blue sky background
- Green-tinted floor
- 3 cups per level (free tier difficulty)
- **Best for:** Beginners, casual play

### **Levels 31-60: Purple Theme**
- Purple gradient background
- Purple-tinted floor
- 4 cups per level (medium difficulty)
- Premium visual effects (if enabled in code)
- **Best for:** Intermediate players

### **Levels 61-100+: Red/Orange Theme**
- Sunset orange background
- Red-brown floor
- 5-6 cups per level (hard difficulty)
- Premium effects enabled
- **Best for:** Advanced players, challenges

**Color Transitions:**  
Colors smoothly blend between themes (e.g., Levels 30-45 gradually shift from blue to purple).

---

## 📈 Difficulty Scaling

The game automatically adjusts difficulty based on level number:

| Level Range | Cup Count | Difficulty | Monetization |
|-------------|-----------|------------|--------------|
| 1-20        | 3 cups    | Easy       | Free         |
| 21-50       | 4 cups    | Medium     | Premium      |
| 51-75       | 5 cups    | Hard       | Premium      |
| 76-100+     | 6 cups    | Very Hard  | Premium      |

**Note:** Level 21 and above require the "Full Game Unlock" IAP (£1.50) in procedural mode, just like manual levels.

---

## 🔄 Sharing Levels with Friends

### How to Share

1. **Enable procedural mode**
2. **Select a level** (e.g., Level 5)
3. **Enter a custom seed** (e.g., 999999) or use Random
4. **Click the level** to start playing
5. **Your seed is automatically copied to your clipboard!**
6. Send your friend:
   - Level number: `5`
   - Seed: `999999`
7. Your friend enters the same seed and selects Level 5
8. **They'll play the EXACT SAME layout!**

### Community Challenges

**Example:**
- "Level 10 with Seed 123456 - Can you beat it in 2 shots?"
- "Level 50 with Seed 789000 - No one has beaten this yet!"
- "Daily Challenge: Level 25 Seed 20250115"

---

## ⚙️ What You Can Change

### As a Non-Coder

**You CAN change:**
- ✅ Projectile count (affects difficulty)
- ✅ Slingshot power
- ✅ Target score (but procedural levels auto-set this to cup count)
- ✅ Theme colors (edit LevelGenerator.cs theme definitions)
- ✅ Cup spawn zones (edit DefineSpawnZones function)
- ✅ Cup scale and rotation ranges
- ✅ Safe zone boundaries

**You CANNOT easily change:**
- ❌ Number of cups per difficulty tier (requires code edit)
- ❌ Spawn logic algorithms (requires C# knowledge)
- ❌ RNG seed formulas (requires math/programming)

---

## 🛠️ Customizing Procedural Generation (Simple)

### Change Theme Colors

1. Open `Globals/LevelGenerator.cs` in a text editor (or Godot script editor)
2. Find the function `GetThemeForRoom(int roomNumber)`
3. Look for color values like:
   ```csharp
   new Color(0.3f, 0.6f, 0.9f)  // Blue theme
   ```
4. Change the numbers (RGB values from 0.0 to 1.0):
   - **Red:** `Color(1.0f, 0.0f, 0.0f)`
   - **Green:** `Color(0.0f, 1.0f, 0.0f)`
   - **Yellow:** `Color(1.0f, 1.0f, 0.0f)`
   - **Pink:** `Color(1.0f, 0.5f, 0.8f)`

### Change Cup Count

1. Open `Globals/LevelGenerator.cs`
2. Find the function `GetCupCountForRoom(int roomNumber)`
3. Change the return values:
   ```csharp
   if (roomNumber <= 20)
       return 3;  // ← Change to 4 for easier free levels
   ```

### Change Level Ranges

Change when themes switch:
```csharp
if (roomNumber <= 30)  // ← Change to 50 to extend blue theme
```

---

## 🐛 Troubleshooting

### "Procedural toggle doesn't save"
- Check that `user://profile.json` is writable
- Try toggling OFF then ON again
- Restart the game

### "Levels still look hand-designed"
- Make sure toggle is **ON** (green checkbox)
- Verify you're not on Level 1-20 (which look similar by design)
- Try Level 50+ for more obvious differences

### "Can't reproduce friend's level"
- Confirm you're using the **EXACT same seed AND level number**
- Example: Seed `12345` on Level 5 ≠ Seed `12345` on Level 6
- Check for typos in seed number

### "Cups spawn inside walls/floor"
- This is a bug in spawn zone logic
- Report with level number and seed
- Temporary fix: Try a different seed

### "Procedural levels too easy/hard"
- Adjust slingshot power in room scene
- Increase/decrease projectile count
- Or edit cup count in LevelGenerator.cs (see above)

---

## 📊 Comparison: Manual vs. Procedural

| Feature | Manual Levels | Procedural Levels |
|---------|---------------|-------------------|
| **Level Count** | 100 fixed | Infinite |
| **Design** | Hand-crafted puzzles | Algorithm-generated |
| **Variety** | Carefully tuned | Random but consistent |
| **Sharing** | Level number only | Level + seed |
| **Difficulty** | Gradual curve | Consistent per tier |
| **Storage** | 100 .tscn files | 1 template + code |
| **Customization** | Scene-by-scene | Global rules |
| **Best For** | Story mode, campaigns | Endless mode, challenges |

---

## 🎯 Recommended Usage

### **Use Manual Levels When:**
- You want a curated experience
- Building a story/campaign
- Need specific puzzle mechanics
- Testing new gameplay features

### **Use Procedural Levels When:**
- You want endless replayability
- Running community challenges
- Testing game balance at scale
- Sharing custom seeds with friends

### **Hybrid Approach:**
- Manual for Levels 1-20 (teach mechanics)
- Procedural for 21-100+ (endless challenge)
- Toggle available for player choice

---

## 📝 Technical Notes

**Files Involved:**
- `Globals/LevelGenerator.cs` - Core generation logic
- `Script/ProceduralRoom.cs` - Room scene script
- `Scenes/Levels/ProceduralRoom.tscn` - Reusable template
- `Script/RoomSelection.cs` - UI toggle controls
- `Globals/PlayerProfile.cs` - Saves preference

**How Seeds Work:**
- Seed = Number that initializes random number generator
- Same seed → Same random sequence → Identical level
- Deterministic (seed = 0) → Uses level number to calculate seed
- Random seed → Uses current time + random value

**Performance:**
- Generation time: < 1 millisecond
- No lag or stutter
- Memory usage: Negligible
- Compatible with all platforms

---

## ✅ Quick Reference

### Enable Procedural Mode
```
Main Menu → Select Room → Toggle "Procedural Levels: ON"
```

### Play with Custom Seed
```
Enable Procedural → Enter Seed → Select Level → Play
```

### Share a Challenge
```
"Try Level 10 with Seed 123456!"
```

### Reset to Default
```
Toggle "Procedural Levels: OFF"
```

---

## 🚀 Future Enhancements

**Planned Features:**
- Daily challenge seeds
- Leaderboards per seed
- More obstacle types (not just cups)
- Custom difficulty modifiers
- Community seed database
- Seed validation/rating system

**Wishlist:**
- Procedural enemy spawning
- Multi-layer cup stacks
- Moving platforms
- Timed challenges
- Co-op seed sharing

---

## 📞 Support

**Need Help?**
- Check TROUBLESHOOTING.md
- Review PROCEDURAL_LEVELS.md (technical guide)
- Ask in community Discord/forum
- Report bugs with level number + seed

**For Developers:**
- See PROCEDURAL_LEVELS.md for API documentation
- Check LevelGenerator.cs comments for implementation details
- Integration guide in INTEGRATION_COMPLETE.md

---

**Last Updated:** January 2025  
**Version:** 1.0  
**Status:** Production-Ready ✅
