# Settings Reference Manual 📖
## What Every Setting Does (Inspector Panel Guide)

> **PRO TIP:** All settings marked with **@Export** in Godot's Inspector are safe to change. They're designed for you to customize!

---

## 📋 How to Use This Reference

1. Open a `.cs` file in Godot (e.g., `GameManager.cs`)
2. Look at the **Inspector** panel on the right side
3. Find the setting name in this reference
4. Read the description to understand what it does
5. Change the value and test!

---

## 🎮 Global Managers

### GameManager (`Globals/GameManager.cs`)

#### General Settings
| Setting | Type | Default | Description |
|---------|------|---------|-------------|
| `TotalLevels` | int (const) | 100 | Total number of levels in game. **Don't change unless you add more levels!** |
| `FreeLevels` | int (const) | 20 | Number of levels available for free (rest paywalled) |
| `MainScenePath` | string | `res://Scenes/Main/Main.tscn` | Path to main menu scene |
| `ProceduralRoomScenePath` | string | `res://Scenes/Levels/ProceduralRoom.tscn` | Path to procedural level template |

#### ⚠️ DANGER: Don't Change
- `State`: Game state (managed automatically)
- `CurrentRoomIndex`: Current level index (managed automatically)

---

### AudioManager (`Globals/AudioManager.cs`)

#### General Settings (Volume)
| Setting | Type | Default | Range | Description |
|---------|------|---------|-------|-------------|
| `MasterVolume` | float | 1.0 | 0.0-1.0 | Overall game volume. 0.0 = silent, 1.0 = full |
| `MusicVolume` | float | 0.7 | 0.0-1.0 | Background music volume (relative to master) |
| `SfxVolume` | float | 0.8 | 0.0-1.0 | Sound effects volume (relative to master) |

#### Music Settings
| Setting | Type | Default | Description |
|---------|------|---------|-------------|
| `BackgroundMusic` | AudioStream | null | Main background music track |
| `MenuMusic` | AudioStream | null | Music for main menu |
| `LevelMusic` | AudioStream | null | Music during gameplay |

#### Sound Effects Settings
| Setting | Type | Default | Description |
|---------|------|---------|-------------|
| `LaunchSound` | AudioStream | null | Slingshot launch sound |
| `ImpactSound` | AudioStream | null | Projectile impact sound |
| `ExplosionSound` | AudioStream | null | Destruction/explosion sound |
| `SuccessSound` | AudioStream | null | Level completion sound |
| `FailureSound` | AudioStream | null | Level failed sound |
| `ButtonClickSound` | AudioStream | null | UI button click sound |
| `UiHoverSound` | AudioStream | null | UI hover sound |

#### ✅ Beginner-Friendly Changes:
- Adjust `MasterVolume` to make game quieter/louder
- Replace `BackgroundMusic` with your own `.ogg` file
- Turn off sounds by setting volumes to `0.0`

---

### MonetizationManager (`Globals/MonetizationManager.cs`)

#### General Settings
| Setting | Type | Default | Description |
|---------|------|---------|-------------|
| `ShowAds` | bool | true | Enable/disable ads in game |
| `ShowIap` | bool | true | Enable/disable in-app purchases |
| `FullGamePrice` | float | 0.99 | Price of full game unlock (USD) |

#### ✅ Beginner-Friendly Changes:
- Set `ShowAds` to `false` to remove ads
- Set `ShowIap` to `false` to disable purchases
- Change `FullGamePrice` to adjust IAP price

#### ⚠️ DANGER: Don't Change
- Store IDs are in `project.godot`, not here
- `IsFullGameUnlocked`: Managed automatically

---

### LevelGenerator (`Globals/LevelGenerator.cs`)

#### General Settings
| Setting | Type | Default | Range | Description |
|---------|------|---------|-------|-------------|
| `UseRandomSeed` | bool | true | true/false | Use random seed vs. sequential seeds |
| `DifficultyScale` | float | 1.0 | 0.5-2.0 | Multiplier for difficulty. Higher = harder |
| `MinCups` | int | 3 | 1-6 | Minimum cups in generated level |
| `MaxCups` | int | 6 | 1-6 | Maximum cups in generated level |

#### Visual Themes
| Setting | Type | Default | Description |
|---------|------|---------|-------------|
| `UseThemes` | bool | true | Enable color themes based on level number |
| `ThemeTransitionRange` | int | 10 | Levels per theme color |

#### Difficulty Curve
| Setting | Type | Default | Description |
|---------|------|---------|-------------|
| `CupScalePerTier` | int | 1 | How many cups to add per difficulty tier |

#### ✅ Beginner-Friendly Changes:
- `DifficultyScale = 0.5` → Easy mode
- `DifficultyScale = 2.0` → Hard mode
- `UseThemes = false` → Disable color themes
- `MinCups = 2`, `MaxCups = 4` → Fewer cups, easier levels

---

### EffectsManager (`Script/EffectsManager.cs`)

#### Screen Shake Settings
| Setting | Type | Default | Description |
|---------|------|---------|-------------|
| `ShakeDefaultIntensity` | float | 5.0 | Shake strength for minor events |
| `ShakeDefaultDuration` | float | 0.3 | How long minor shake lasts (seconds) |
| `ShakeImpactIntensity` | float | 15.0 | Shake strength for major impacts |
| `ShakeImpactDuration` | float | 0.5 | How long major shake lasts (seconds) |

#### Particle Settings
| Setting | Type | Default | Description |
|---------|------|---------|-------------|
| `ConfettiParticleScene` | PackedScene | null | Custom confetti particle (or uses default) |
| `ExplosionParticleScene` | PackedScene | null | Custom explosion particle (or uses default) |
| `DustParticleScene` | PackedScene | null | Custom dust particle (or uses default) |
| `SparkleParticleScene` | PackedScene | null | Custom sparkle particle (or uses default) |

#### ✅ Beginner-Friendly Changes:
- Reduce `ShakeDefaultIntensity` to `2.0` for subtle shakes
- Increase `ShakeDefaultIntensity` to `10.0` for dramatic effects
- Leave particle scenes as `null` to use built-in particles (recommended!)

---

### GameFeelManager (`Script/GameFeelManager.cs`)

#### General Settings
| Setting | Type | Default | Description |
|---------|------|---------|-------------|
| `EnableScreenShake` | bool | true | Enable/disable all screen shake |
| `EnableParticles` | bool | true | Enable/disable all particle effects |
| `EnableSlowMotion` | bool | false | Enable slow-motion on heavy impacts |

#### Slingshot Feel
| Setting | Type | Default | Description |
|---------|------|---------|-------------|
| `SlingshotChargeDuration` | float | 0.3 | How long slingshot takes to charge (seconds) |
| `ShowTrajectory` | bool | true | Show projectile trajectory while aiming |

#### Impact Feel
| Setting | Type | Default | Description |
|---------|------|---------|-------------|
| `HeavyImpactThreshold` | float | 500.0 | Impact force needed for heavy effects |
| `SlowMotionOnImpact` | bool | false | Enable slow-motion on impacts |
| `SlowMotionDuration` | float | 0.2 | How long slow-motion lasts (seconds) |
| `SlowMotionScale` | float | 0.3 | Time scale during slow-motion (1.0 = normal, 0.5 = half speed) |

#### ✅ Beginner-Friendly Changes:
- `EnableScreenShake = false` → Disable all screen shake
- `EnableParticles = false` → Disable all particles
- `ShowTrajectory = false` → Hide aiming line
- `EnableSlowMotion = true`, `SlowMotionOnImpact = true` → Enable cinematic slow-mo!

---

### HapticFeedbackManager (`Script/HapticFeedbackManager.cs`)

#### General Settings
| Setting | Type | Default | Description |
|---------|------|---------|-------------|
| `EnableHaptics` | bool | true | Enable/disable vibration on mobile |
| `GlobalIntensity` | float | 1.0 | Overall vibration strength (0.0-2.0) |

#### Impact Feedback
| Setting | Type | Default | Description |
|---------|------|---------|-------------|
| `LightImpactDuration` | float | 0.03 | Vibration time for light impacts (seconds) |
| `MediumImpactDuration` | float | 0.05 | Vibration time for medium impacts (seconds) |
| `HeavyImpactDuration` | float | 0.1 | Vibration time for heavy impacts (seconds) |

#### UI Feedback
| Setting | Type | Default | Description |
|---------|------|---------|-------------|
| `ButtonTapDuration` | float | 0.02 | Vibration when tapping buttons (seconds) |
| `SelectionChangeDuration` | float | 0.015 | Vibration when scrolling/selecting (seconds) |

#### Success/Failure Feedback
| Setting | Type | Default | Description |
|---------|------|---------|-------------|
| `SuccessDuration` | float | 0.15 | Vibration for success (seconds) |
| `FailureDuration` | float | 0.1 | Vibration for failure (seconds) |

#### ✅ Beginner-Friendly Changes:
- `EnableHaptics = false` → Disable vibration
- `GlobalIntensity = 0.5` → Make vibration more subtle
- `GlobalIntensity = 1.5` → Make vibration more intense

---

## 🎯 Game Objects

### Slingshot (`Script/Slingshot.cs`)

#### Physics Constants
| Setting | Type | Default | Description |
|---------|------|---------|-------------|
| `IMPULSE_MULT` | float (const) | 20.0 | Multiplier for launch force |
| `IMPULSE_MAX` | float (const) | 1200.0 | Maximum launch speed |
| `DRAG_LIM_MAX` | Vector2 (const) | (0, 60) | Maximum drag distance (X, Y) |
| `DRAG_LIM_MIN` | Vector2 (const) | (-60, 0) | Minimum drag distance (X, Y) |

#### ✅ Beginner-Friendly Changes:
- `IMPULSE_MAX = 1500` → More powerful slingshot
- `IMPULSE_MAX = 900` → Less powerful slingshot
- These are constants, so you need to edit the C# code file

---

### RoomBase (`Script/RoomBase.cs`)

#### Level Settings
| Setting | Type | Default | Description |
|---------|------|---------|-------------|
| `_targetScore` | int (protected) | 3 | Score needed to unlock exit door |
| `_isBonusRoom` | bool | false | Is this a bonus room? |
| `_nextRoomPath` | NodePath | null | Path to next room (for bonus rooms) |

#### ✅ Beginner-Friendly Changes:
- Edit individual level scenes (`Room###.tscn`) instead of this base class
- In the scene, find RoomBase node and adjust `_targetScore` in Inspector

---

## 🎨 UI Elements

### UI Buttons (General)

#### Size & Layout
| Setting | Type | Description |
|---------|------|-------------|
| `CustomMinimumSize` | Vector2 | Minimum button size (good for mobile) |
| `Size` | Vector2 | Actual button size |

#### Visual
| Setting | Type | Description |
|---------|------|-------------|
| `Modulate` | Color | Overall color tint of button |
| `SelfModulate` | Color | Color of button itself (not children) |

#### Text
| Setting | Type | Description |
|---------|------|-------------|
| `Text` | string | Button text |
| `LabelSettings` | Resource | Font and text style settings |

#### ✅ Beginner-Friendly Changes:
- Set `CustomMinimumSize = (100, 100)` for better mobile touch targets
- Change `Text` to customize button labels
- Change `Modulate` to recolor buttons

---

## 📱 Export Settings (project.godot)

### Application Settings
```ini
[application]
config/name="Angry Animals"          # Game name
config/icon="res://icon.svg"          # App icon
config/version="1.0"                   # Version number
run/main_scene="res://Scenes/Main/Main.tscn"  # First scene
```

### Monetization Settings
```ini
[monetization]
admob/app_id=""                        # AdMob app ID
admob/banner_ad_unit_id=""             # Banner ad ID
admob/interstitial_ad_unit_id=""       # Interstitial ad ID
admob/rewarded_ad_unit_id=""          # Rewarded ad ID
iap/ios_product_id="full_game_unlock" # iOS IAP ID
iap/android_product_id="full_game_unlock"  # Android IAP ID
```

### ✅ Beginner-Friendly Changes:
- Change `config/name` to your game name
- Change `config/version` for new releases
- Add your AdMob IDs when ready for release

---

## 🎯 Common Setting Presets

### Easy Mode
```yaml
GameFeelManager:
  EnableScreenShake: false
  EnableSlowMotion: false

Slingshot:
  IMPULSE_MAX: 1500

LevelGenerator:
  DifficultyScale: 0.5
  MinCups: 2
  MaxCups: 4
```

### Normal Mode (Default)
```yaml
GameFeelManager:
  EnableScreenShake: true
  EnableSlowMotion: false

Slingshot:
  IMPULSE_MAX: 1200

LevelGenerator:
  DifficultyScale: 1.0
  MinCups: 3
  MaxCups: 6
```

### Hard Mode
```yaml
GameFeelManager:
  EnableScreenShake: true
  EnableSlowMotion: true

Slingshot:
  IMPULSE_MAX: 900

LevelGenerator:
  DifficultyScale: 2.0
  MinCups: 4
  MaxCups: 6
```

### Cinematic Mode (Lots of Effects)
```yaml
GameFeelManager:
  EnableScreenShake: true
  EnableParticles: true
  EnableSlowMotion: true
  SlowMotionOnImpact: true
  SlowMotionDuration: 0.3
  SlowMotionScale: 0.2

EffectsManager:
  ShakeImpactIntensity: 20.0
  ShakeImpactDuration: 0.6

HapticFeedbackManager:
  GlobalIntensity: 1.5
```

---

## 📓 How to Apply Presets

1. Create a `.tres` resource file (e.g., `EasyMode.tres`)
2. Add settings as key-value pairs
3. In your scene, create a new `Resource` node
4. Set its resource to your `.tres` file
5. Scripts can read from this resource

**Note:** This requires some C# knowledge. For beginners, just change settings directly in Inspector!

---

## 🎓 Next Steps

- **GODOT_BEGINNER_MAP.md**: Complete beginner guide
- **INSPECTOR_TOUR.md**: Visual guide to Inspector panel
- **COMMON_CHANGES.md**: Top 10 things to change

---

## ⚠️ Safety Guidelines

### ✅ SAFE to Change:
- Any setting labeled `@Export` in Inspector
- Audio volume settings
- Slingshot power settings
- UI size/color settings
- Monetization toggles

### ⚠️ USE CARE:
- Physics constants (IMPULSE_MAX, etc.)
- Level generation parameters
- Export settings

### ❌ NEVER CHANGE:
- `_state`, `_index`, `_count` (internal variables)
- `Instance` properties (singleton references)
- File paths that end in `.cs` (script paths)

---

## 🆘 Need Help?

1. Open the `.cs` file in Godot
2. Look at the code comments for more details
3. Check `NON_CODER_GUIDE.md` for context
4. Start with safe settings (volume, colors) before trying advanced ones

---

## 🎉 You're Informed!

Now you know what every major setting does. Start customizing! 🎮✨

> **Remember:** Always test changes with F5/F6!
