# Custom Level Editor Guide

Welcome to the Angry Animals Custom Level Editor! This guide will help you create, validate, and share your own custom levels.

## Table of Contents

1. [Getting Started](#getting-started)
2. [Creating a Level](#creating-a-level)
3. [Materials Guide](#materials-guide)
4. [Difficulty System](#difficulty-system)
5. [Validation](#validation)
6. [Sharing Levels](#sharing-levels)
7. [Playing Custom Levels](#playing-custom-levels)
8. [Tips for Balanced Levels](#tips-for-balanced-levels)

---

## Getting Started

### Opening the Editor

1. Launch Angry Animals
2. From the main menu, click **"Create Level"**
3. The level editor will open

### Editor Layout

The editor is divided into several sections:

- **Top Bar**: Level name, creator name, and back button
- **Left Panel**: Material selector and action buttons
- **Center Panel**: Placement area where you design your level
- **Right Panel**: List of placed obstacles
- **Bottom Bar**: Difficulty indicator and obstacle count

---

## Creating a Level

### Placing Obstacles

1. **Select a Material**: Click one of the material buttons on the left panel
   - Wood (Brown) - Easiest to destroy
   - Stone (Gray) - Moderate difficulty
   - Brick (Red) - Harder
   - Iron (Dark Gray) - Very hard
   - Diamond (Blue) - Extremely hard

2. **Place Obstacle**: Click in the placement area to add an obstacle
   - Obstacles snap to a grid for easier placement
   - You can place up to 20 obstacles per level
   - Minimum 3 obstacles required

3. **Move Obstacles**: Click and drag existing obstacles to reposition them

4. **Delete Obstacles**: Right-click on an obstacle to delete it

### Editing Level Info

- **Level Name**: Enter a creative name for your level at the top
- **Creator Name**: Enter your name to get credit for your creation

---

## Materials Guide

Each material has different properties that affect difficulty:

| Material | Hardness | Hits to Destroy | Best Used For |
|----------|----------|-----------------|---------------|
| **Wood** | 1 | 1-2 hits | Easy obstacles, outer structures |
| **Stone** | 2 | 2-3 hits | Moderate challenges |
| **Brick** | 3 | 3-4 hits | Hard obstacles, protective layers |
| **Iron** | 4 | 4-5 hits | Very tough obstacles |
| **Diamond** | 5 | 5-6 hits | Ultimate challenge, boss obstacles |

### Material Distribution Tips

- **Easy Levels**: Use mostly Wood and Stone
- **Medium Levels**: Mix of Wood, Stone, and Brick
- **Hard Levels**: Include Iron and Diamond
- **Variety is Key**: Use at least 2 different materials for visual and gameplay variety

---

## Difficulty System

The editor automatically calculates your level's difficulty based on:

1. **Material Hardness** (50% weight)
   - Average hardness of all obstacles
   - Diamond obstacles significantly increase difficulty

2. **Obstacle Count** (30% weight)
   - More obstacles = higher difficulty
   - Capped at 20 obstacles

3. **Layout Complexity** (20% weight)
   - Scattered obstacles are harder than clustered ones
   - Distance between obstacles affects difficulty

### Difficulty Ratings

- **Easy** (0.0-0.3): Great for beginners, mostly soft materials
- **Medium** (0.3-0.6): Balanced challenge, good mix of materials
- **Hard** (0.6-0.85): Challenging, includes hard materials
- **Extreme** (0.85-1.0): Very difficult, lots of hard materials

The difficulty indicator updates in real-time as you place obstacles!

---

## Validation

Before sharing your level, it must pass validation:

### Validation Checks

1. **Obstacle Count**: 3-20 obstacles required
2. **Material Variety**: At least 2 different materials
3. **Positioning**: All obstacles must be in the playable area
4. **Balance**: Not too easy or too hard (warnings only)
5. **Reachability**: At least 50% of obstacles should be reachable

### How to Validate

1. Click **"Validate Level"** button
2. Review the validation message
3. Fix any errors (marked with ❌)
4. Address warnings (marked with ⚠️) if desired

**Note**: Warnings don't prevent sharing, but errors do!

---

## Sharing Levels

Once your level is validated and saved:

1. Click **"Save & Share"** button
2. A dialog will appear with your share code
3. The share code is automatically copied to your clipboard
4. Share the code with friends via:
   - Discord
   - Twitter
   - Text message
   - Email
   - Any messaging platform

### Share Code Format

Share codes look like this: `AA1_eyJMZXZlbE5hbWUiOi...`

- Always starts with `AA1_` (Angry Animals version 1)
- Contains encoded level data
- Safe to share - contains no personal information

### Local Storage

Your levels are also saved locally in draft form:
- You can reload them later
- Edit and re-share
- Backup your favorites!

---

## Playing Custom Levels

### Loading a Friend's Level

1. From the main menu, click **"Play Custom Level"**
2. A dialog will appear
3. Paste the share code your friend sent you
4. Click **"OK"**
5. The level will load and you can play!

### What Happens

- The level is validated before loading
- Invalid or corrupted codes will show an error
- Valid levels load instantly
- Full game mechanics apply (slingshot, physics, scoring)

---

## Tips for Balanced Levels

### Do's ✅

- **Mix Materials**: Use 2-3 different material types
- **Create Paths**: Give players clear targets
- **Test Difficulty**: Aim for Medium-Hard for best experience
- **Use Space**: Spread obstacles across the area
- **Think 3D**: Stack obstacles vertically for variety
- **Progressive Hardness**: Put harder materials in protected positions

### Don'ts ❌

- **All Same Material**: Boring and predictable
- **Too Clustered**: Everything in one spot is confusing
- **Too Scattered**: Impossible to hit multiple targets
- **All Diamond**: Frustratingly hard
- **All Wood**: Too easy, no challenge
- **Out of Bounds**: Keep obstacles in the playable area

### Level Design Patterns

#### Tower Pattern
- Stack obstacles vertically
- Softer materials at bottom
- Harder materials on top
- Creates satisfying chain reactions

#### Wall Pattern
- Horizontal line of obstacles
- Mixed materials
- Forces strategic aiming
- Good for medium difficulty

#### Scattered Pattern
- Obstacles spread around
- Mix of heights and positions
- Hardest materials in center
- Highest difficulty, most strategic

### Playtesting

- Share with friends first
- Get feedback on difficulty
- Iterate based on what's fun
- Not too hard, not too easy!

---

## Troubleshooting

### "Level cannot be played" Error

- Check that you have 3-20 obstacles
- Ensure at least 2 different materials
- Verify all obstacles are in bounds

### "Invalid share code" Error

- Code must start with `AA1_`
- Copy the entire code
- No extra spaces or characters
- Try copying again

### Difficulty Too High/Low

- **Too High**: Replace some hard materials with softer ones
- **Too Low**: Add harder materials or more obstacles
- Aim for 0.4-0.7 difficulty score

### Obstacles Overlapping

- Right-click to delete one
- Drag to reposition
- Use grid snapping for better placement

---

## Advanced Tips

### Creating Themed Levels

- **Forest Theme**: Mostly wood obstacles
- **Castle Theme**: Stone and brick
- **Fortress Theme**: Iron and diamond
- **Mixed Theme**: Variety of all materials

### Challenge Modes

- **Speed Run**: Few but strategic obstacles
- **Destruction**: Many easy obstacles for satisfying destruction
- **Puzzle**: Specific order required to clear
- **Boss Battle**: One or two extremely hard obstacles

### Sharing Best Practices

- Give your level a descriptive name
- Include difficulty in the name ("Easy Tower", "Hard Puzzle")
- Add your creator name for recognition
- Share on community forums or Discord
- Create level series with themes

---

## Community

Share your best levels and play levels from other players:

- **Discord**: [Your Discord Link]
- **Twitter**: Use #AngryAnimalsLevels
- **Forums**: [Your Forum Link]

### Level of the Week

The best community levels may be featured in-game!

---

## FAQ

**Q: Can I edit a level after sharing?**
A: Yes! Load your saved draft, edit, and generate a new share code.

**Q: Is there a level limit?**
A: You can save unlimited drafts locally. Share as many as you want!

**Q: Can I play my own levels?**
A: Yes! Save your level, then use "Play Custom Level" with your own code.

**Q: What if I lose my share code?**
A: If you saved it as a draft, you can reload it and generate a new code.

**Q: Can I import levels from other games?**
A: No, share codes are specific to Angry Animals.

**Q: Are there rewards for creating levels?**
A: Check the game for seasonal events and creator challenges!

---

## Need Help?

If you encounter issues or have questions:

1. Check this guide first
2. Visit the [GitHub Issues page](https://github.com/yourusername/angry-animals)
3. Ask in the Discord community
4. Contact support at [your-email@example.com]

---

**Happy Creating!** 🎨🎮

The Angry Animals Team
