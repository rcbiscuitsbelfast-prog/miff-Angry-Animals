# Angry Animals - Troubleshooting Guide

🎮 **Game:** Angry Animals (Godot 4.4 C#)  
🔧 **Version:** 1.0  
❓ **Purpose:** Quick solutions to common problems

---

## 🔍 QUICK PROBLEM FINDER

### "Game Won't Start / Crashes Immediately"
**Jump to:** [Godot Project Issues](#godot-project-issues) → [Crash on Startup](#crash-on-startup)

### "Slingshot Doesn't Work"
**Jump to:** [Gameplay Issues](#gameplay-issues) → [Slingshot Problems](#slingshot-doesnt-respond-to-dragging)

### "Projectiles Won't Launch"
**Jump to:** [Gameplay Issues](#gameplay-issues) → [Projectiles Dont Launch](#red-projectiles-dont-launch)

### "No Sound / Audio Problems"
**Jump to:** [Audio Issues](#audio-issues)

### "Levels Won't Load / Missing"
**Jump to:** [Scene Issues](#scene-issues) → [Level Not Found](#level-scene-not-found)

### "IAP / Ads Not Working"
**Jump to:** [Monetization Issues](#monetization-issues)

---

## 🖥️ GODOT EDITOR ISSUES

### Project Won't Open in Godot

**Problem:** Double-clicking `project.godot` does nothing or shows errors.

**Solutions:**

1. **Check Godot Version**
   ```
   Problem: Godot 3.x or wrong version
   Solution: Install Godot 4.4+ Mono/.NET version from godotengine.org
   ```

2. **Check .NET Installation**
   ```
   Problem: Missing .NET SDK
   Solution: Install .NET 8.0 SDK from microsoft.com/net
   ```

3. **Corrupted .godot Folder**
   ```
   Problem: Cache corruption
   Solution: 
   - Close Godot
   - Delete `.godot/` folder (hidden in project root)
   - Reopen project
   ```

4. **Missing Dependencies**
   ```
   Error: "Could not load project.godot"
   Solution: 
   - Check file exists and not corrupted
   - Open project folder, not just .godot file
   - Verify project structure is intact
   ```

---

### C# Scripts Show Errors in Editor

**Problem:** Red errors in script editor, can't attach scripts.

**Solutions:**

1. **Build the Project**
   ```
   Solution: 
   - Open Godot
   - Bottom panel → "MSBuild" tab
   - Click "Build Project" button
   - Or: Project → Tools → C# → Build Project
   ```

2. **Restore NuGet Packages**
   ```
   Solution:
   - Project → Tools → C# → Restore NuGet Packages
   - Wait for completion (watch Output panel)
   ```

3. **Missing .csproj/.sln**
   ```
   Solution:
   - Close Godot
   - Delete `.godot/`, `*.sln`, `*.csproj`
   - Open project.godot in Godot
   - Godot recreates project files
   ```

4. **Wrong Namespace**
   ```
   Error: "The type or namespace name 'X' could not be found"
   Solution:
   - Make sure all scripts use `namespace AngryAnimals;`
   - Or remove namespace lines entirely
   ```

---

## 🎮 GAMEPLAY ISSUES

### Slingshot Doesn't Respond to Dragging

**Symptoms:** Nothing happens when clicking/dragging on slingshot area.

**Diagnosis Steps:**

1. **Check Input Area**
   ```
   Solution:
   - Open level scene (e.g., Room001.tscn)
   - Find "Slingshot" node
   - Expand it → find "InputArea" node
   - Select InputArea
   - Inspector → Collision → check "Shape"
   
   Problem: Shape missing or too small
   Fix: Add CollisionShape2D with RectangleShape2D
   - Set shape to cover dragging area (e.g., 400x400 pixels)
   ```

2. **Check Script Assignment**
   ```
   Solution:
   - Select InputArea
   - Inspector → "Node" tab → "Signals"
   - Check for "input_event" connected to script
   
   Problem: Signal not connected
   Fix: Connect input_event to InputArea's _on_input_event method
   ```

3. **Check Slingshot Exports**
   ```
   Solution:
   - Select Slingshot node
   - Inspector → Exports section
   - Check _inputArea export variable
   
   Problem: Export variable is null or wrong node
   Fix: Drag InputArea node into the export slot
   ```

4. **Check Current Projectile**
   ```
   Problem: Slingshot has no projectile loaded
   Symptoms: Can drag but nothing launches
   
   Solution:
   - Check ProjectilesLoader node exists
   - Check _faceProjectileScene is assigned
   - Check _projectileCount > 0
   ```

---

### Projectiles Don't Launch

**Symptoms:** Dragging works, but releasing does nothing.

**Solutions:**

1. **Check CameraBoundaries**
   ```
   Common error: "Cannot apply impulse to Body in STATIC or KINEMATIC mode"
   
   Solution:
   - Select Projectile node (under ProjectilesLoader)
   - Inspector → RigidBody2D → Mode
   - Set to: "Rigid" (not Static or Kinematic)
   - Check "Freeze" is true initially
   ```

2. **Check Impulse Calculation**
   ```
   Problem: IMPULSE_MULT too low or zero
   
   Solution:
   - Select Slingshot node
   - Inspector → IMPULSE_MULT
   - Should be 10-30 (default 20)
   - Set to 20 if it's 0 or empty
   ```

3. **Check Free State**
   ```
   Problem: Projectile not "unfrozen" on launch
   
   Solution:
   Edit Slingshot.cs LaunchProjectile() method:
   
   _currentProjectile.Freeze = false; ← Must be present
   _currentProjectile.ApplyCentralImpulse(impulse);
   ```

---

### Projectiles Pass Through Objects

**Symptoms:** Projectiles don't collide with cups/obstacles.

**Solutions:**

1. **Check Collision Layers**
   ```
   Solution:
   - Select Projectile node
   - Inspector → Collision
   - Check "Collision Layer" (Projectile should be on layer 1)
   - Check "Collision Mask" (should detect layer 2 for obstacles)
   
   - Select Obstacle/Cup nodes
   - Check they're on "Collision Layer" 2
   ```

2. **Check Physics Layers**
   ```
   Project Settings → Physics → 2D → Layer Names
   
   Layer 1: "Projectile" or "Animal"
   Layer 2: "Obstacle" or "Environment"
   
   Make sure:
   - Projectile Mask includes Obstacle layer
   - Obstacle Mask includes Projectile layer
   ```

3. **Check Collision Shapes**
   ```
   Problem: CollisionShape2D missing or too small
   
   Solution:
   - Select cups/obstacles
   - Check for CollisionShape2D child
   - Inspector → Shape must not be null
   - Increase shape size if too small
   ```

---

### Trajectory Line Not Showing

**Symptoms:** When dragging, no line appears showing aim.

**Solutions:**

1. **Check TrajectoryDrawer Node**
   ```
   Solution:
   - Verify TrajectoryDrawer node exists under Slingshot
   - Check it's a Node2D with TrajectoryDrawer.cs script
   - Check script shows no errors
   ```

2. **Check Node Assignment**
   ```
   Solution:
   - Select Slingshot node
   - Inspector → _trajectoryDrawer export
   - Must reference TrajectoryDrawer node
   - Drag TrajectoryDrawer node into the slot if empty
   ```

3. **Check Show/Hide Methods**
   ```
   Solution:
   In TrajectoryDrawer.cs:
   
   _line2D.Visible = true; ← Must be set when dragging
   
   Check that _line2D is assigned in _Ready()
   ```

---

### StickClone Doesn't Appear

**Symptoms:** After launching projectiles, walking character never shows up.

**Solutions:**

1. **Check Scene File Exists**
   ```
   Error: "Cannot load resource: res://Scenes/Characters/StickClone.tscn"
   
   Problem: Scene file missing
   
   Solution:
   - Create StickClone.tscn
   - Or change path in RoomBase.cs line 216
   ```

2. **Check Spawn Position**
   ```
   Solution:
   - Open level scene
   - Add Marker2D node named "StickCloneSpawn"
   - Position where you want character to appear
   ```

3. **Check Signal Connection**
   ```
   Solution:
   RoomBase.cs must connect to OnAnimalDied signal
   
   Verify in RoomBase.cs:
   SignalManager.Instance.OnAnimalDied += OnAnimalDied;
   ```

---

### StickClone Won't Move / Can't Reach Exit

**Symptoms:** Character spawns but doesn't move, or gets stuck.

**Solutions:**

1. **Check Exit Area**
   ```
   Solution:
   - Add Area2D named "ExitArea"
   - Add CollisionShape2D (large rectangle)
   - Position over exit door
   - RoomBase will find with: GetNodeOrNull<Area2D>("ExitArea")
   ```

2. **Check Pathfinding**
   ```
   Solution:
   - StickClone needs clear path to exit
   - Place obstacles so character can jump over
   - Check _moveSpeed is reasonable (100-200)
   - Check _jumpForce allows clearing obstacles (-300 to -500)
   ```

3. **Check Ground Detection**
   ```
   Problem: Character thinks it's always in air
   
   Solution:
   - Check floor collision layers
   - Add raycasts for ground detection
   - Ensure _isGrounded updates correctly
   ```

---

## 🎵 AUDIO ISSUES

### No Sound / Music Not Playing

**Solutions:**

1. **Check Audio Files Exist**
   ```
   Error in output: "Audio resource not found: ..."
   
   Solution:
   - Create Assets/Audio/Music/ folder
   - Add BackgroundMusic.ogg or .mp3
   - Add SlingshotSound.ogg, DestructionSound.ogg, etc.
   ``}

2. **Check AudioManager Node**
   ```
   Solution:
   - Verify AudioManager scene/node exists
   - Check script assigned and running
   - Check _backgroundMusicPlayer not null
   
   - Click AudioManager in running scene
   - In Remote tab, check StreamPlayer is playing
   ```

3. **Check Volumes**
   ```
   Solution:
   - Select AudioManager
   - Inspector: MusicVolume = 0.7, SfxVolume = 0.8
   - Check MuteMusic = false, MuteSfx = false
   - Check AudioStreamPlayer volumes not -infinity
   ``}

4. **Check Audio Bus Layout**
   ```
   Solution:
   - Audio → Audio Bus at bottom of editor
   - Verify "Music" and "SFX" buses exist
   - Check main bus volume = 0 dB
   - Check bus not muted/soloed incorrectly
   ```

---

### Sound Effects Too Loud/Quiet

**Solutions:**

1. **Adjust Individual Volume**
   ```
   Solution:
   - AudioManager → MusicVolume (0.0-1.0)
   - AudioManager → SfxVolume (0.0-1.0)
   
   Typical values:
   Music: 0.6-0.8
   SFX: 0.7-0.9
   ```

2. **Balance with dB**
   ```
   Alternative:
   Edit default_bus_layout.tres
   - Set Music bus: -5 dB to -10 dB
   - Set SFX bus: -3 dB to -8 dB
   ```

---

## 🖼️ SCENE ISSUES

### Level Scene Not Found

**Error:** "Cannot open file 'res://Scenes/Levels/RoomXYZ.tscn'"

**Solutions:**

1. **Check Scene Exists**
   ```
   Solution:
   - Open folder: Scenes/Levels/
   - Verify RoomXXX.tscn exists (001-100)
   - Note: RoomBase.tscn doesn't exist (it's a script template)
   ```

2. **Check GameManager Configuration**
   ```
   Error occurs on level load
   
   Solution:
   - Open Globals/GameManager.cs
   - Find CreateDefaultRooms() method
   - Check scene paths: $"res://Scenes/Levels/Room{levelNumber:D3}.tscn"
   - Verify this matches your folder structure
   ```

3. **Regenerate Corrupt Scene**
   ```
   Solution:
   - Find working level (e.g., Room001.tscn)
   - Make backup copy
   - Open and re-save the scene
   - In FileSystem dock → right-click → Reimport
   ```

---

### Scene Opens But Shows Red/Error Nodes

**Symptoms:** Nodes in Scene tree show as gray/red with warning icon.

**Solutions:**

1. **Missing Dependencies**
   ```
   Solution:
   - Click red node
   - Inspector → will show "Missing resource: [script/scene]"
   - Click the missing resource slot
   - Re-assign the correct script/scene file
   - Save scene
   ```

2. **Missing ExtResources**
   ```
   Problem: Scene file references missing resources at top
   
   [ext_resource type="Script" path="res://Scripts/Missing.cs"]
   
   Solution:
   - Delete that ext_resource line
   - Save scene
   - Re-attach correct script in Inspector
   ```

3. **NodePath Issues**
   ```
   Error: "Node not found: X" in export variables
   
   Solution:
   - Select parent node with export variable
   - Inspector → find broken NodePath
   - Drag correct node from Scene tree into slot
   - Or use dropdown to select node
   ```

---

### UI Elements Not Visible / Off-Screen

**Solutions:**

1. **Check Anchor Settings**
   ```
   Solution:
   - Select UI node (Label, Button, etc.)
   - Inspector → Layout → Layout Mode
   - Set to: "Anchors" not "Position"
   - Set anchors for screen position:
     - Top-left: Anchor 0,0
     - Top-right: Anchor 1,0
     - Center: Anchor 0.5,0.5
   ```

2. **Use Container Nodes**
   ```
   Best practice:
   - Use HBoxContainer/VBoxContainer/MarginContainer
   - These auto-position child nodes
   - Avoid absolute positioning for responsive UI
   ```

3. **CanvasLayer Layer**
   ```
   Solution:
   - UI should be child of CanvasLayer
   - Not child of regular Node2D
   - CanvasLayer renders on top, screen coordinates
   ```

---

## 💰 MONETIZATION ISSUES

### IAP (In-App Purchase) Not Working

**Symptoms:** "Unlock Full Game" button does nothing or crashes.

**Solutions:**

1. **Check Product ID Configuration**
   ```
   Solution:
   - Open project.godot
   - Find [monetization] section
   - iap/android_product_id = "full_game_unlock"
   - iap/ios_product_id = "full_game_unlock"
   - Must match exactly what's configured in store
   ```

2. **Check MonetizationManager Node**
   ```
   Solution:
   - Verify MonetizationManager autoload exists
   - Project → Project Settings → Autoload
   - Should show: MonetizationManager → res://Globals/MonetizationManager.cs
   - Enabled: Yes (checked)
   ```

3. **Check Store Configuration**
   ```
   Android:
   - Google Play Console → Your app → Monetize → Products
   - Create product: full_game_unlock
   - Set price: £1.50 (or your price)
   - Activate product
   
   iOS:
   - App Store Connect → Your app → In-App Purchases
   - Create IAP: full_game_unlock
   - Configure pricing
   - Submit for review with app
   ```

---

### Ads Not Showing

**Symptoms:** No ads appear, or "Ad failed to load" errors.

**Solutions:**

1. **Check AdMob Configuration**
   ```
   Solution:
   - project.godot → [monetization]
   - admob/app_id = "ca-app-pub-..." (must be YOUR ID)
   - admob/banner_ad_unit_id = "ca-app-pub-.../..."
   - admob/interstitial_ad_unit_id = "ca-app-pub-..."
   - admob/rewarded_ad_unit_id = "ca-app-pub-..."
   
   For testing, use Google's test IDs
   ```

2. **Check AdsManager Node**
   ```
   Solution:
   - Project Settings → Autoload
   - Verify AdsManager is enabled
   - Check ShowAds = true in inspector
   ```

3. **Review AdMob Policy**
   ```
   Common ad blocking reasons:
   - App not published yet (use test ads)
   - No ads in your region for your app
   - Ad unit recently created (wait 24 hours)
   - Policy violation
   
   Fix: Use test ad units during development
   ```

---

## 💾 SAVE/LOAD ISSUES

### Scores Not Saving Between Sessions

**Solutions:**

1. **Check File Paths**
   ```
   Solution:
   - ScoreManager saves to: user://animals.save
   - PlayerProfile saves to: user://player_profile.json
   
   These paths automatically resolve to:
   Windows: %APPDATA%/Godot/app_userdata/Angry Animals/
   Mac: ~/Library/Application Support/Godot/app_userdata/Angry Animals/
   Android: Internal storage/Android/data/com.rcbiscuits.angryanimals/files/
   iOS: Application sandbox/Documents/
   
   Check files exist after play session
   ```

2. **Check FileManager Permissions**
   ```)
   Solution:
   - On mobile, requires read/write external storage permission
   - Project Settings → Permissions → check appropriate boxes
   - For Android: "Write External Storage"
   - For iOS: Adjust Info.plist
   ```

3. **Check Save on Exit**
   ```
   Solution:
   ScoreManager saves in _ExitTree() only
   If game crashes, scores won't save
   
   Add manual save:
   - Call ScoreManager.SaveToFile() after level complete
   - Or after each level to be safer
   ```

---

## 🏃 PERFORMANCE ISSUES

### Game Running Slowly / Lagging

**Solutions:**

1. **Check Physics Bodies**
   ```
   Problem: Too many RigidBody2D objects active
   
   Solution:
   - Set sleeping to true when not needed
   - Use StaticBody2D for non-moving objects
   - Limit projectile count in ProjectilesLoader
   - Disable continuous collision for simple shapes
   ```

2. **Check Rendering**
   ```
   Solution:
   - Use Sprite2D, not individual draw calls
   - Atlasc textures into sprite sheets
   - Limit particles if using many
   - Set process priority for important nodes
   ```

3. **Check Draw Calls**
   ```
   Solution:
   Godot provides a "Frame Time" panel at bottom
   Monitor:
   - Physics frame time
   - Idle frame time
   - Draw calls per frame
   
   Target: 60 FPS, each frame < 16ms
   ```

---

### Memory Leaks / Growing Memory Usage

**Symptoms:** Memory (RAM) usage increases over time, never decreases.

**Solutions:**

1. **Check Signal Connections**
   ```
   Common leak: Signals not disconnected
   
   Solution:
   Every script with signal connections needs:
   
   public override void _ExitTree()
   {
       // Disconnect ALL signals before exit
       someNode.SignalName -= MethodName;
   }
   
   Check all _ExitTree() methods
   ```

2. **Check Node Cleanup**
   ```)
   Solution:
   - Projectiles should QueueFree() when done
   - Effects should time out and delete
   - Remove from parent before QueueFree()
   ```

3. **Use Godot Debugger**
   ```
   Solution:
   Debug → Debugger → Monitors
   Watch:
   - Object count (should stay stable)
   - Process time
   - Resources loaded
   
   Play for 5-10 minutes, should not continuously increase
   ```

---

## 🐛 DEBUGGING TECHNIQUES

### Using GD.Print() for Debugging

```csharp
// Add these to track what's happening:

public override void _Ready()
{
    GD.Print($"{Name} ready!");
    GD.Print($"Position: {GlobalPosition}");
}

private void OnSomeEvent()
{
    GD.Print($"Event triggered! Value: {someValue}");
}

// Shows in Output panel at bottom
```

### Using Breakpoints

```csharp
// In Godot script editor:
// Click left margin to add red breakpoint
// Play with Debug button (bug icon)
// Execution pauses at breakpoint
// Can inspect variable values
```

### Checking Node State

```csharp
// In _Process or _PhysicsProcess:

if (_currentProjectile == null)
    GD.Print("ERROR: No projectile loaded!");

if (!_currentProjectile.CanApplyImpulse)
    GD.Print("Projectile in wrong state");
```

---

## 🎯 COMMON ERROR MESSAGES

### "Attempted to call `Callable::call()` on invalid instance"
```
Meaning: Signal connected to method that doesn't exist
Fix: Check signal connections, ensure method named correctly
```

### "Invalid call. Nonexistent function 'X' in base 'Y'"
```
Meaning: Calling method that doesn't exist on that node type
Fix: Check node type, ensure method exists, use HasMethod() check before calling
```

### "Can't change this state while flushing queries"
```
Meaning: Trying to add/remove physics objects during physics processing
Fix: Use CallDeferred() to postpone operation:
CallDeferred("MethodName");
```

### "Node not found: X while resolving node path"
```
Meaning: NodePath export variable pointing to non-existent node
Fix: Check NodePath export, ensure referenced node exists with exact name
```

### "Parser Error: Unexpected token: Identifier"
```
Meaning: Syntax error in C# script
Fix: Check for missing semicolons, braces, check latest C# syntax for Godot 4
```

---

## 📞 GETTING MORE HELP

### Official Resources
- **Godot Docs:** https://docs.godotengine.org/en/stable/
- **C# Specific:** Search "Godot C# scripting"
- **Godot Q&A:** https://ask.godotengine.org/

### When Asking for Help
Include this information:
```
1. Error message (full text)
2. When it happens (startup, gameplay, on exit)
3. What you were trying to do
4. Your Godot version (Help → About)
5. Operating system (Windows, Mac, Linux)
6. Steps to reproduce (if repeatable)
```

### Common Solutions Summary

| Problem | First Try | Second Try | Last Resort |
|---------|-----------|------------|-------------|
| Won't start | Rebuild C# | Delete .godot folder | Reinstall Godot |
| Slingshot broken | Check InputArea | Check exports | Reconnect signals |
| No physics | Check collision layers | Check RigidBody mode | Adjust shapes |
| No audio | Check volumes | Check audio files | Check bus layout |
| Crash on level load | Check scene exists | Check dependencies | Check console errors |
| IAP fails | Check product IDs | Check store setup | Use test IDs |
| Ads fail | Check AdMob config | Test with test ads | Check internet |

---

## 💡 PRO TIPS FOR DEBUGGING

### 1. Use Remote Scene Tree
```
Run game → "Remote" tab appears next to "Local"
- Shows live scene hierarchy
- Can inspect values in real-time
- Check if nodes exist during gameplay
```

### 2. Use the Profiler
```
Debug → Profiler
- Check frame time spikes
- Find what's slow
- Memory leaks show as climbing object count
```

### 3. Save Often
```)
Version control (Git):
- Commit when something works
- Can roll back if you break it
- Use descriptive commit messages
```

### 4. Test Scenes Individually
```
Don't test whole game every time:
- Open specific level scene
- Click "Play Scene" (not "Play Project")
- Faster iteration
- Faster debugging
```

### 5. Use Scene Unique Names
```
Right-click node → Access As Scene Unique Name
- Can reference with %NodeName
- Prevents path issues
- More robust than NodePaths
```

---

**🎮 Need more specific help?** Check the other documentation files:
- `AUDIT_COMPLETE.md` - Full technical details
- `NON_CODER_GUIDE.md` - Step-by-step instructions
- `GAME_VALUES.md` - All adjustable parameters
- `BUILD_CHECKLIST.md` - Mobile submission guide

**Good luck debugging! Remember: Every problem has a solution. 🔧**