# Angry Animals - Complete Codebase Documentation

## Project Overview

**Angry Animals** is a Godot 4 C# game featuring a slingshot physics mechanic similar to Angry Birds. Players launch projectiles to destroy cups and obstacles, progressing through levels with increasingly complex challenges. The game includes face customization, monetization features, scoring systems, and sound management.

---

## Core Architecture

### Game Flow
```
MainMenu → RoomSelection → Room (Slingshot Phase) → Room (Traversal Phase) → LevelCompleted
```

### Key Systems
- **Physics & Launching**: Slingshot + Projectile system
- **Gameplay Logic**: RoomBase manages level flow and completion
- **Scoring**: ScoreManager + RageSystem + Scorer
- **UI & HUD**: GameHud + LevelCompleted + MainMenu
- **Audio**: AudioManager (referenced but not provided)
- **Monetization**: MonetizationManager + AdsManager (referenced but not provided)

---

## Script Reference

### Player Systems

#### **Projectile.cs**
Base class for all launched objects from the slingshot.

**Responsibilities:**
- Physics interaction and collision detection
- Launch mechanics with impulse application
- Lifecycle management (launch → almost stopped → death)
- Sound effects on impact

**Key Methods:**
- `Launch(Vector2 impulse)` - Apply launch force
- `CheckIfAlmostStopped()` - Detect when projectile has nearly stopped
- Signal: `AlmostStopped` - Emitted when velocity drops below threshold

**Related:** `FaceProjectile.cs` extends this with sprite visuals

---

#### **FaceProjectile.cs**
Visual variant of Projectile with face sprite for character representation.

**Features:**
- Extends Projectile base class
- Displays sprite during flight
- Minimal custom logic (inherits most from parent)

---

#### **Slingshot.cs**
Main controller for launching mechanics from the slingshot.

**Responsibilities:**
- Drag input detection and processing
- Impulse calculation with constraints
- Projectile positioning and launching
- Trajectory visualization coordination
- Sound effect triggering

**Key Components:**
- `InputArea` - Detects drag start/end
- `TrajectoryDrawer` - Shows launch preview
- `Marker2D _projectileHolder` - Projectile rest position
- `Marker2D _restPosition` - Target return position

**Constants:**
- `IMPULSE_MULT = 20.0f` - Drag-to-impulse multiplier
- `IMPULSE_MAX = 1200.0f` - Maximum launch force
- `DRAG_LIM_MAX/MIN` - Drag boundary constraints

**Key Methods:**
- `LoadProjectile(Projectile)` - Set active projectile
- `UpdateDragging()` - Handle drag physics
- `LaunchProjectile()` - Execute launch with impulse

---

#### **InputArea.cs**
Invisible Area2D for detecting drag interactions.

**Signals:**
- `DragStarted` - User began dragging
- `DragEnded` - User released drag

**Notes:** Simple event dispatcher; actual drag mechanics in