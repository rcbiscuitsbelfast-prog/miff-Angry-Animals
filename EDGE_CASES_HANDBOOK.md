# 🚨 EDGE CASES HANDBOOK
**Troubleshooting Guide for Weird Game Behaviors**

This handbook covers all the strange and unusual scenarios that can occur in Angry Animals, with clear solutions for non-coders who encounter these issues.

---

## 🎯 OVERVIEW

This guide helps you handle **weird game behaviors** that might confuse non-technical players. Each section covers:
- **What the problem looks like**
- **Why it happens** (technical background in simple terms)
- **How to fix it** (step-by-step solutions)
- **How to prevent it** (settings adjustments)

---

## 🏆 PERFECT SCORE & LOOT DROP EDGE CASES

### **"I got a perfect score but no loot drop appeared!"**

**What happens:**
- Player achieves 3-star completion
- Expected: Cosmetic loot drop with celebration
- Actual: No loot drop appears

**Why it happens:**
- Loot drop system has randomized probability
- Base chance: 100% but with bonuses
- Player might be on a "dry streak" 
- Sometimes luck just isn't with you

**How to fix:**
1. **Play another level to perfect score**
2. **Check CosmeticLootTable settings** in Inspector:
   - Set `BaseDropChance = 1.0` (100% guaranteed)
   - Increase `PerfectScoreBonusChance = 0.5`
   - Set `DrySpellBonusChance = 0.5`
3. **Verify CosmeticLootTable is enabled**:
   - Check `LootDropsEnabled = true`

**Prevention:**
- In Settings, increase drop chances for guaranteed rewards
- Use "Reset to Defaults" if loot drops stop working

---

### **"I earned the same cosmetic multiple times!"**

**What happens:**
- Player gets duplicate cosmetics from loot drops
- Cosmetic inventory shows multiple copies
- Expected: Each perfect score gives unique cosmetic

**Why it happens:**
- Duplicate prevention only kicks in after 5 dry spells
- Low-weight cosmetics can appear multiple times
- Drop system prioritizes variety but not exclusively

**How to fix:**
1. **Check PlayerProfile cosmetics list**
2. **Increase DuplicateDropMultiplier** in CosmeticLootTable:
   - Set to `0.1` (very low duplicate chance)
   - Or set to `0.0` (never duplicate)
3. **Manually remove duplicates** by:
   - Using PlayerProfile methods to clear inventory
   - Resetting cosmetic progress

**Prevention:**
- Set `DuplicateDropMultiplier = 0.0` in settings
- This ensures every drop is a new cosmetic

---

### **"Perfect score but I got 2 stars instead of 3!"**

**What happens:**
- Player feels they performed perfectly
- Game shows 2 stars instead of expected 3
- Scoring seems incorrect

**Why it happens:**
- Perfect score threshold might be set too high
- Score calculation includes penalties
- "Perfect" is relative to optimal play, not player perception

**How to fix:**
1. **Adjust PerfectScoreThreshold** in GameSettingsManager:
   - Default: `0.9` (90% of optimal)
   - Easier: `0.8` (80% of optimal)
   - Much easier: `0.7` (70% of optimal)
2. **Check GoodScoreThreshold** too:
   - Set to `0.5` for easier 2-star threshold
3. **Verify RoomTargetScoreOffset**:
   - Negative values make scoring easier
   - Try `-10` or `-15` for easier targets

**Prevention:**
- Use Easy difficulty preset for more forgiving scoring
- Adjust thresholds based on player feedback

---

## 🎮 MODIFIER & UNLOCKABLE EDGE CASES

### **"I unlocked a modifier but can't find where to turn it on!"**

**What happens:**
- Player sees unlock notification
- Goes to Settings but can't find modifier toggle
- Modifier appears unlocked but no controls visible

**Why it happens:**
- Settings menu not fully updated with modifier UI
- Modifier tab might not be visible
- UI integration incomplete

**How to fix:**
1. **Check SettingsMenu.tscn scene**
2. **Look for Modifiers tab** in Settings menu
3. **If missing, manually enable in GameSettingsManager**:
   - Set `ExtremePhysicsMode = true` directly
   - Set `BigHeadsMode = true` directly
   - etc.
4. **Restart game** to see changes

**Prevention:**
- Use Settings menu to access modifiers when UI is complete
- Check GameSettingsManager properties directly in Inspector

---

### **"My unlock progress reset when I died!"**

**What happens:**
- Player was making progress toward unlock
- Died/resumed and progress counter went backward
- Achievement tracking seems broken

**Why it happens:**
- Progress saves at specific checkpoints
- Mid-level progress might not persist
- Some progress only saves on completion

**How to fix:**
1. **Complete the current level** to save progress
2. **Check UnlockablesManager settings**:
   - Verify all progress counters
   - Manual progress adjustment in Inspector if needed
3. **Reset progress intentionally**:
   - Use `ResetAllProgress()` method
   - Start fresh from beginning

**Prevention:**
- Always complete levels to save progress
- Progress saves on level completion, not during gameplay

---

### **"Multiple modifiers don't work together!"**

**What happens:**
- Player enables 2+ modifiers
- Only one modifier seems active
- Combined effects don't stack as expected

**Why it happens:**
- Modifier interaction rules not fully implemented
- Some modifiers conflict with each other
- Hardcore mode might be accidentally enabled

**How to fix:**
1. **Check HardcoreMode setting**:
   - Ensure `HardcoreMode = false`
2. **Test modifiers individually**:
   - Enable one at a time to verify each works
   - Then combine gradually
3. **Check modifier interaction rules**:
   - Some combinations intentionally disabled
   - See UNLOCKABLES_CATALOG.md for stacking info

**Prevention:**
- Test modifier combinations one by one
- Refer to interaction rules in unlockables catalog

---

## 🎬 TRANSITION & UI EDGE CASES

### **"Fade transitions are too fast/slow!"**

**What happens:**
- Screen fades happen too quickly or too slowly
- Game feels jarring or boring during transitions
- Transition timing doesn't match game feel

**Why it happens:**
- Fade durations set incorrectly in GameSettingsManager
- Different systems use different timing values
- TransitionManager might not be reading settings

**How to fix:**
1. **Adjust fade durations** in GameSettingsManager:
   - `LevelCompleteFadeDuration = 1.0` (slower)
   - `LevelCompleteFadeDuration = 0.3` (faster)
2. **Check TransitionManager settings**:
   - Verify it reads from GameSettingsManager
   - Test fade effects manually
3. **Use difficulty presets**:
   - Easy: slower transitions for dramatic effect
   - Hard: faster transitions for urgent feel

**Prevention:**
- Set transition speeds based on target audience
- Test with different player types

---

### **"Settings don't save when I restart the game!"**

**What happens:**
- Player adjusts settings in-game
- Restarts game and settings reverted to defaults
- Progress and modifications lost

**Why it happens:**
- GameSettingsManager save/load system might be broken
- File permissions issue preventing save
- JSON corruption or invalid data

**How to fix:**
1. **Check save file location**:
   - Look for `user://game_settings.json`
   - Verify file exists and isn't corrupted
2. **Reset settings to defaults**:
   - Use "Reset to Defaults" button
   - Restart game to verify reset works
3. **Manual settings adjustment**:
   - Reconfigure settings after reset
   - Test save by adjusting one setting at a time

**Prevention:**
- Always use Settings menu save functionality
- Avoid manually editing JSON files

---

### **"UI elements are too small/big on my screen!"**

**What happens:**
- Text and buttons hard to read or click
- Interface doesn't scale properly
- Game feels uncomfortable to play

**Why it happens:**
- UI scaling settings not adjusted
- TextScale setting at default value
- Resolution scaling issues

**How to fix:**
1. **Adjust TextScale** in GameSettingsManager:
   - `TextScale = 1.5` for larger text
   - `TextScale = 0.8` for smaller text
2. **Enable HighContrastMode** for better visibility:
   - `HighContrastMode = true`
3. **Adjust ScreenShakeIntensity** if it interferes:
   - `ScreenShakeIntensity = 0.5` for reduced motion

**Prevention:**
- Set appropriate text scale for target screen size
- Use accessibility settings for better visibility

---

## 🐛 PHYSICS & GAMEPLAY EDGE CASES

### **"Characters get stuck and won't move!"**

**What happens:**
- StickClone characters stop responding
- No movement despite player input
- Game progression blocked

**Why it happens:**
- Physics collision detection issues
- Character movement speed set to 0
- Ragdoll physics interfering with movement

**How to fix:**
1. **Check CharacterMoveSpeed** in GameSettingsManager:
   - Set to `200.0` (default value)
   - Increase to `300.0` for faster movement
2. **Verify character state**:
   - Ensure character is in TRAVERSAL phase
   - Check for collision detection issues
3. **Restart level** if stuck:
   - Exit and retry level
   - Progress should be preserved

**Prevention:**
- Keep movement speeds at reasonable values
- Test physics on different levels

---

### **"Ragdolls are too jiggly/realistic!"**

**What happens:**
- Ragdoll physics feel unrealistic
- Limbs spin too much or not enough
- Physics feel "off" compared to expectations

**Why it happens:**
- Ragdoll physics settings not tuned properly
- Joint stiffness and damping values incorrect
- Gravity settings affect ragdoll behavior

**How to fix:**
1. **Adjust RagdollJointStiffness**:
   - `0.7` = realistic (default)
   - `0.3` = very loose and jiggly
   - `0.9` = very stiff and robotic
2. **Adjust RagdollAngularDamping**:
   - `3.0` = normal spinning (default)
   - `1.0` = lots of spinning
   - `5.0` = minimal spinning
3. **Set RagdollGravityEnabled**:
   - `true` = normal gravity
   - `false` = floating ragdolls

**Prevention:**
- Use Extreme Physics modifier for maximum chaos
- Use No Gravity modifier for floaty physics
- Test settings with different character types

---

### **"Slingshot feels too weak/strong!"**

**What happens:**
- Projectiles don't travel far enough
- Shots feel underpowered or overpowered
- Game difficulty doesn't match expectations

**Why it happens:**
- Slingshot impulse settings incorrect
- Projectile gravity scale too high/low
- Drag limits restricting shot power

**How to fix:**
1. **Adjust SlingshotImpulseMultiplier**:
   - `25.0` = very powerful (Easy)
   - `20.0` = normal power (Normal)
   - `15.0` = weak shots (Hard)
2. **Adjust SlingshotImpulseMax**:
   - `1200.0` = maximum shot power
   - `800.0` = reduced maximum
3. **Use difficulty presets**:
   - Easy: increases slingshot power
   - Hard: decreases slingshot power

**Prevention:**
- Use difficulty presets for balanced gameplay
- Adjust based on player feedback

---

## 🔧 TECHNICAL TROUBLESHOOTING

### **"Console shows null reference errors!"**

**What happens:**
- Error messages appear in console
- Game might crash or behave strangely
- Features stop working unexpectedly

**Why it happens:**
- GameSettingsManager not properly initialized
- Missing node references in scenes
- Autoload configuration issues

**How to fix:**
1. **Verify GameSettingsManager autoload**:
   - Check project.godot [autoload] section
   - Add missing autoload entries
2. **Check scene node paths**:
   - Verify all NodePath references are correct
   - Test scene loading individually
3. **Restart Godot editor**:
   - Sometimes fixes initialization issues

**Prevention:**
- Always test after autoload changes
- Keep autoload entries organized and documented

---

### **"Game runs slowly with many modifiers enabled!"**

**What happens:**
- Frame rate drops when using multiple modifiers
- Game becomes unresponsive or laggy
- Performance issues during intense gameplay

**Why it happens:**
- Physics calculations multiply with modifier effects
- Particle effects stack and overload system
- No performance optimization for modifier combinations

**How to fix:**
1. **Reduce ParticleDensity**:
   - Set to `0.5` for lower particle count
   - Or `0.0` to disable particles entirely
2. **Disable some modifiers**:
   - Don't use all 6 modifiers simultaneously
   - Test combinations for performance
3. **Use Hardcore Mode**:
   - Disables all modifiers for maximum performance

**Prevention:**
- Balance visual effects with performance
- Use performance monitoring tools

---

### **"Save files are corrupted!"**

**What happens:**
- Settings revert to defaults unexpectedly
- Cosmetic progress lost
- Achievement progress disappears

**Why it happens:**
- JSON file format changes between versions
- File permissions prevent saving
- Disk space or file system issues

**How to fix:**
1. **Delete corrupted save files**:
   - Remove `user://game_settings.json`
   - Remove `user://cosmetic_drop_history.json`
   - Remove `user://unlockables_data.json`
2. **Restart game**:
   - Fresh start with default settings
   - Progress will be lost but game will work
3. **Check file permissions**:
   - Ensure game can write to user directory

**Prevention:**
- Regularly backup save files
- Don't manually edit JSON files
- Use built-in save/load functionality

---

## 📱 PLATFORM-SPECIFIC ISSUES

### **"Game crashes on mobile when..."**

**What happens:**
- App closes unexpectedly on phones/tablets
- Crashes during specific gameplay moments
- Performance issues on mobile hardware

**Why it happens:**
- Memory usage too high for mobile devices
- Physics calculations too intensive
- Screen size or orientation issues

**How to fix:**
1. **Reduce graphics settings**:
   - Set `ParticleDensity = 0.0`
   - Disable `ScreenShakeIntensity`
   - Use `ReduceMotion = true`
2. **Lower physics complexity**:
   - Reduce `RagdollLifetime` to cleanup faster
   - Decrease `MaxSimultaneousSounds`
3. **Use accessibility settings**:
   - Enable `HighContrastMode` for simpler visuals
   - Increase `TextScale` for better visibility

**Prevention:**
- Test on target mobile devices
- Use mobile-optimized settings presets

---

### **"Audio doesn't work on some devices!"**

**What happens:**
- No sound effects or music
- Audio works in some parts but not others
- Volume controls have no effect

**Why it happens:**
- Audio driver compatibility issues
- Device-specific audio settings
- Game audio system not properly configured

**How to fix:**
1. **Check AudioManager settings**:
   - Verify `MasterVolume > 0`
   - Check `SfxVolume` and `MusicVolume`
   - Ensure `MaxSimultaneousSounds > 0`
2. **Test device audio**:
   - Verify device volume is up
   - Test with other applications
   - Check device-specific audio settings
3. **Restart audio system**:
   - Restart game completely
   - Check Godot audio driver settings

**Prevention:**
- Test audio on target devices
- Provide audio troubleshooting in user guide

---

## 🎯 PREVENTIVE MEASURES

### **Regular Maintenance:**

1. **Weekly Settings Check**:
   - Review all GameSettingsManager values
   - Ensure reasonable ranges for all settings
   - Test difficulty presets

2. **Monthly Performance Review**:
   - Monitor frame rate with all modifiers
   - Check memory usage over time
   - Test save/load functionality

3. **Quarterly Content Review**:
   - Update unlock requirements if too easy/hard
   - Adjust cosmetic drop rates
   - Review modifier balance

### **User Communication:**

1. **Clear Error Messages**:
   - When settings fail to save
   - When modifiers conflict
   - When performance issues occur

2. **Helpful Documentation**:
   - This handbook for edge cases
   - UNLOCKABLES_CATALOG.md for modifier details
   - SETTINGS_GUIDE.md for basic configuration

3. **Easy Reset Options**:
   - "Reset to Defaults" button always available
   - Clear progression reset options
   - Backup/restore functionality

---

## 🆘 EMERGENCY PROCEDURES

### **"Game is completely broken!"**

**Nuclear Option - Complete Reset:**
1. Delete all save files:
   - `user://game_settings.json`
   - `user://cosmetic_drop_history.json` 
   - `user://unlockables_data.json`
2. Restart game with default settings
3. Reconfigure preferences from scratch

### **"Can't access Settings menu!"**

**Alternative Access:**
1. Use Inspector to modify GameSettingsManager directly
2. Reset project.godot autoload settings
3. Check scene hierarchy for SettingsMenu node

### **"Performance is unplayable!"**

**Performance Mode:**
1. Enable Hardcore Mode (disables all modifiers)
2. Set `ParticleDensity = 0.0`
3. Set `ScreenShakeIntensity = 0.0`
4. Set `RagdollLifetime = 3.0`

---

This handbook ensures that no matter what weird situation arises, there's always a clear path to resolution!