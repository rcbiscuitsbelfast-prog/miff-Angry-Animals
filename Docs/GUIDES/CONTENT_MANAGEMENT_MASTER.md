# 📚 CONTENT MANAGEMENT MASTER GUIDE
## Control Your Game Without Code!

> **TL;DR:** Everything in Yeet Face can be customized through the Inspector. This guide covers ALL content management in one place.

---

## 🎯 MASTER OVERVIEW

**What You Can Control:**
- ✅ **NPCs & Dialogue** (Add characters, write speech)
- ✅ **Cosmetics & Appearance** (Visual customization)
- ✅ **Level Objectives** (Win conditions)
- ✅ **Game Settings** (Difficulty, physics, audio)
- ✅ **Meme Mini-Games** (Copy & modify variants)
- ✅ **Marketing Content** (Streamer highlights)

**What You DON'T Need Code For:**
- ❌ Writing dialogue
- ❌ Placing NPCs
- ❌ Changing cosmetics
- ❌ Adjusting difficulty
- ❌ Creating new levels
- ❌ Modifying objectives

---

## 🎭 PART 1: NPC & DIALOGUE MANAGEMENT

### Add NPC to Any Level (1 minute)
```
1. Open: Scenes/Levels/RoomXXX.tscn
2. Drag: Scenes/Prefabs/NPCs/[AnyNPC].tscn into scene
3. Position: Drag with mouse to desired location
4. Save: Ctrl+S
5. Test: F5 to verify NPC appears
```

### Write Custom Dialogue (30 seconds)
```
1. Select NPC node in scene
2. Inspector → dialogue property
3. Add Element → Type your line
4. Save → Test
Example:
["WATCH IT!", "Not MY rules!", "You're grounded!"]
```

### Change NPC Appearance (1 minute)
```
1. Select NPC node
2. Inspector → cosmetic_overlays
3. Add Element → Choose cosmetic:
   - moustache (for Dad)
   - glasses (for Teacher)
   - crown (for Authority)
   - military_helmet (for Soldier)
4. Save → Test
```

### NPC Types & Behaviors
```
FAMILY (Mom, Dad):
- Static positioning
- Protective/commanding dialogue
- 100 HP

AUTHORITY (Teacher, Soldier, Principal):
- Command presence
- Disciplinary dialogue  
- 80-120 HP

SCHOOLMATE (Classmate):
- Social dynamics
- Peer-level dialogue
- 50 HP

BEHAVIORS:
- STATIC: Stays in place
- MOVING_PATROL: Walks back/forth
- CAGED: Trapped (bars visible)
```

---

## 🎨 PART 2: COSMETIC CUSTOMIZATION

### Use Existing Cosmetics (30 seconds)
```
Available Cosmetics:
- moustache (fatherly)
- glasses (smart)
- academic_hat (teacher)
- military_helmet (military)
- crown (authority)
- beanie (casual)
- bandana (rebel)
- scarf (weather)
- pirate_hat (special)

Method:
1. Select NPC
2. Inspector → cosmetic_overlays → Add
3. Choose from dropdown
4. Save → Test
```

### Create New Cosmetic (5 minutes)
```
1. Create 64x64 PNG with transparency
2. Save to: Assets/Sprites/Cosmetics/YOUR_COSMETIC.png
3. Open: Script/CosmeticOverlay.cs
4. Add to enum:
   YOUR_COSMETIC_NAME,
5. Add case:
   case CosmeticType.YOUR_COSMETIC_NAME:
       overlaySprite.Texture = GD.Load<Texture2D>("res://Assets/Sprites/Cosmetics/YOUR_COSMETIC_NAME.png");
       break;
6. Build → Test
```

---

## 🎯 PART 3: LEVEL OBJECTIVES

### Set Level Win Conditions (1 minute)
```
1. Open: Scenes/Levels/RoomXXX.tscn
2. Select: RoomBase node
3. Inspector → LevelObjective settings:
   - Type: DESTROY_ALL, COLLECT_ITEMS, REACH_EXIT
   - Target: "NPC1, NPC2" (comma-separated)
   - Required: 5 (number to complete)
4. Save → Test
```

### Objective Types Explained
```
DESTROY_ALL:
- Destroy every NPC in level
- Use for: Clear the room levels

DESTROY_NPCS:
- Destroy specific NPCs
- Target: "Mom, Dad, Teacher"
- Use for: Target elimination levels

COLLECT_ITEMS:
- Collect all scattered items
- Use for: Treasure/crystal levels

REACH_EXIT:
- Player reaches exit without dying
- Use for: Stealth/escape levels

ELIMINATE_TARGET:
- Destroy one specific high-value NPC
- Use for: Boss battle levels
```

---

## 🎮 PART 4: GAME SETTINGS CONTROL

### Adjust Difficulty (2 minutes)
```
1. Open: Scenes/Levels/RoomBase.tscn (template)
2. Select: Slingshot node
3. Inspector → Slingshot settings:
   - max_power: 800-1200 (easier = lower)
   - friction: 0.1-0.3 (higher = more drag)
   - launch_velocity: affects all levels
4. Save → Test

Easy Mode: max_power=800, friction=0.3
Normal Mode: max_power=1000, friction=0.2  
Hard Mode: max_power=1200, friction=0.1
```

### Audio Settings (1 minute)
```
1. Open: Globals/AudioManager.cs
2. Inspector → Audio settings:
   - Master Volume: 0.0-1.0
   - SFX Volume: 0.0-1.0
   - Music Volume: 0.0-1.0
3. Test changes immediately
```

### Physics Settings (3 minutes)
```
1. Open: Script/Projectile.cs
2. Inspector → Physics settings:
   - Gravity: affects projectile arc
   - Bounce: how much projectiles bounce
   - Friction: surface interaction
3. Test in level → Adjust until perfect
```

---

## 🎪 PART 5: MEME MINI-GAMES

### Create New Meme Variant (10 minutes)
```
1. Duplicate: Scenes/MemeGames/SixToSevenMinigame.tscn
2. Rename: SixToSevenMinigame_YOUR_NAME.tscn
3. Open new file:
   - Edit AnimatedSprite2D animations
   - Create 8-12 frames of your meme
   - Adjust timing (0.1-0.3 seconds per frame)
4. Open: Script/MemeGateway.cs
5. Add to variant list:
   variants.Add("SixToSevenMinigame_YOUR_NAME");
6. Save → Test
```

### Meme Game Trigger
```
Automatic trigger: Perfect score on any level
Random selection: Game picks variant randomly
Custom trigger: Modify MemeGateway.cs timing
```

### Meme Game Types
```
SixToSevenMinigame:
- Classic transformation
- 8-12 animation frames
- 2-3 second duration

Dance variants:
- Robot dance
- Floss dance  
- Dab moves
- TikTok trends

Transformation variants:
- Animal morphing
- Power-up sequences
- Character evolution
```

---

## 📱 PART 6: MONETIZATION SETTINGS

### Configure AdMob (5 minutes)
```
1. Open: project.godot
2. Find [monetization] section:
   admob/app_id="ca-app-pub-xxxxxxxxxx~xxxxxxxxxx"
   admob/banner_ad_unit_id="ca-app-pub-xxxxxxxxxx/xxxxxxxxxx"
   admob/interstitial_ad_unit_id="ca-app-pub-xxxxxxxxxx/xxxxxxxxxx"
   admob/rewarded_ad_unit_id="ca-app-pub-xxxxxxxxxx/xxxxxxxxxx"
3. Replace with your actual AdMob IDs
4. Save → Test
```

### Configure IAP (In-App Purchases)
```
1. Open: project.godot
2. Find [iap] section:
   iap/app_id="com.yourcompany.yourgame"
   iap/sku_remove_ads="remove_ads"
   iap/sku_unlock_all="unlock_all"
3. Replace with your actual IAP IDs
4. Save → Test
```

---

## 🎨 PART 7: ASSET REPLACEMENT

### Replace Character Sprites (5 minutes)
```
1. Create new 64x64 PNG sprites
2. Save to: Assets/Sprites/Characters/
3. Open relevant scene:
   - StickClone.tscn (player character)
   - MomNPC.tscn (Mom sprite)
   - DadNPC.tscn (Dad sprite)
4. Replace Sprite2D texture:
   - Select Sprite2D node
   - Inspector → Texture → Load new PNG
5. Save → Test
```

### Replace Audio (2 minutes)
```
1. Replace audio files in: Assets/Audio/
   - BackgroundMusic.ogg (main theme)
   - SoundEffects/ (impact, speech bubbles, etc.)
2. Files auto-load by name
3. No scene changes needed
4. Test → Adjust volume if needed
```

### Replace Backgrounds
```
1. Create 1920x1080 PNG backgrounds
2. Save to: Assets/Sprites/Backgrounds/
3. Open level scenes → Select ColorRect
4. Inspector → Texture → Load new background
5. Save → Test
```

---

## 🎯 PART 8: LEVEL DESIGN

### Create New Level (15 minutes)
```
1. Duplicate: Scenes/Levels/RoomTemplate.tscn
2. Rename: RoomXXX.tscn (Room101, Room102, etc.)
3. Edit level:
   - Add obstacles, cups, NPCs
   - Position slingshot, exit
   - Set objectives via RoomBase node
4. Update: Scenes/Rooms/RoomSelection.tscn
   - Add new level to level list
   - Set star requirements
5. Test → Save
```

### Modify Existing Level
```
1. Open: Scenes/Levels/RoomXXX.tscn
2. Make changes:
   - Move NPCs (drag with mouse)
   - Add obstacles (drag from left panel)
   - Change positions
   - Modify objectives
3. Save → Test
4. No code changes needed!
```

---

## 📈 PART 9: MARKETING CONTENT

### Create Streamer Clips (10 minutes)
```
1. Record perfect score runs
2. Capture meme mini-games
3. Save clips as MP4
4. Use for:
   - TikTok videos (30-60 seconds)
   - Twitter clips (15-30 seconds)
   - YouTube highlights (2-5 minutes)

Best Clip Moments:
- Perfect score celebrations
- Meme game transformations
- Comedy dialogue exchanges
- Creative level completions
```

### Social Media Posts
```
TikTok Content:
- 30-second meme game clips
- "POV: You're Mom in Yeet Face"
- Before/after cosmetic changes

Twitter Content:
- Absurdist humor posts
- Meme game highlights
- Developer commentary

Instagram Content:
- Face customization showcase
- Level progression posts
- Behind-the-scenes content
```

---

## 🚀 PART 10: RAPID ITERATION

### Daily Content Updates (30 minutes)
```
Morning Routine:
1. Check community feedback
2. Identify 1-2 quick improvements
3. Update dialogue, cosmetics, or levels
4. Test immediately
5. Deploy update

Example Changes:
- "Mom should say X instead of Y"
- "Add glasses to Teacher for better look"  
- "Make Level 15 slightly easier"
- "Add new dialogue line to Dad"
```

### Weekly Features (2 hours)
```
Weekly Goals:
- New cosmetic item
- New meme game variant
- Level difficulty tweaks
- Community-requested features

Example Weekly Features:
- "Winter scarf cosmetic"
- "Robot dance meme variant"
- "5 new dialogue lines"
- "Easier boss levels"
```

### Monthly Major Updates (1 day)
```
Monthly Goals:
- New NPC prefab
- New level pack (10 levels)
- Major gameplay balance
- Seasonal events

Example Monthly Updates:
- "New PrincipalNPC with crown"
- "Christmas themed levels"
- "New character voice lines"
- "Improved physics feel"
```

---

## 📋 QUICK REFERENCE

### Most Common Tasks (30 seconds each)
```
Change Dialogue:
1. Select NPC → Inspector → dialogue → Add line

Add Cosmetic:
1. Select NPC → Inspector → cosmetic_overlays → Add

Change Level:
1. Open RoomXXX.tscn → Drag objects → Save

Adjust Difficulty:
1. Open RoomBase.tscn → Select Slingshot → Change power

Replace Sprite:
1. Open scene → Select Sprite2D → Load new texture

Play Test:
1. F5 to run game
2. Test specific level
3. Verify changes work
```

### File Locations Cheat Sheet
```
NPCs: Scenes/Prefabs/NPCs/
Levels: Scenes/Levels/RoomXXX.tscn
Cosmetics: Assets/Sprites/Cosmetics/
Dialogue: NPC Inspector → dialogue property
Settings: Globals/ folder (with comments)
Meme Games: Scenes/MemeGames/
Audio: Assets/Audio/
Sprites: Assets/Sprites/
```

---

## 🎉 SUCCESS METRICS

### Week 1 Goals:
- [ ] Add 5 NPCs to different levels
- [ ] Create 10 unique dialogue lines
- [ ] Apply 3 different cosmetics
- [ ] Adjust 1 level's difficulty
- [ ] Trigger 1 meme mini-game

### Week 2 Goals:
- [ ] Create 1 new cosmetic
- [ ] Design 1 new meme variant
- [ ] Modify 3 level objectives
- [ ] Record 3 streamer clips
- [ ] Make 1 content update

### Month 1 Goals:
- [ ] Add 20+ NPCs across levels
- [ ] Create 50+ dialogue lines
- [ ] Design 10+ cosmetics
- [ ] Create 5+ meme variants
- [ ] Build 10+ new levels

**Once you complete these goals, you own the game! 🏆**

---

## 🔗 EMERGENCY CONTACTS

### When You Get Stuck:
1. **Check existing levels** for examples
2. **Test small changes** first (F5 often)
3. **Save frequently** (Ctrl+S)
4. **Read NPC comments** in code for guidance
5. **Use Inspector dropdowns** instead of typing

### Backup Plan:
- All changes are saved in scene files
- You can always revert to previous versions
- Duplicate files before major changes
- Export project regularly

---

**🎮 YOU NOW OWN EVERY ASPECT OF THE GAME! 🎮**

This guide gives you complete control. Every feature, every character, every level - all customizable without touching code. Welcome to maximum viral potential! 🚀