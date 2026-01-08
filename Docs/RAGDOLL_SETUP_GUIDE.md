# Ragdoll Physics System Setup Guide

## Overview
This guide will help you set up and tune the ragdoll physics system for Angry Animals. The system creates realistic stick figure ragdolls that spawn when StickClones are caught in explosions, providing satisfying physics-based character destruction.

## Quick Start

### 1. Basic Setup
1. **Add RagdollSpawner to your scene**:
   - Create a new Node2D called "RagdollSystem"
   - Attach `RagdollSpawner.cs` script
   - The spawner will automatically listen for explosions and spawn ragdolls

2. **Configure Basic Settings**:
   - **Max Simultaneous Ragdolls**: 3-4 recommended for performance
   - **Explosion Radius**: 80-120 pixels works well
   - **Physics Preset**: Start with "cartoon" for bouncy fun, "realistic" for serious physics

### 2. Integration Points
The system automatically integrates with:
- **StickClone.cs**: Modified to emit explosion signals
- **Projectile.cs**: Modified to emit explosion events on impact
- **EffectsManager**: Reuses existing explosion particles and screen shake
- **AudioManager**: Uses existing impact sound effects

## Ragdoll Physics Settings

### Joint Stiffness
Controls how rigidly limbs are connected together.

**Range**: 0.1 - 1.0
- **0.1 - 0.3**: Very loose, flaily limbs (realistic)
- **0.4 - 0.6**: Moderate stiffness (balanced)
- **0.7 - 1.0**: Very rigid, cartoon-like movement

**Recommended**: 0.5 (balanced)

### Limb Mass
The weight of each limb, affecting how it responds to forces.

**Range**: 0.5 - 2.0
- **0.5 - 0.8**: Light limbs, very bouncy
- **0.9 - 1.2**: Normal weight (recommended)
- **1.3 - 2.0**: Heavy limbs, slower movement

**Recommended**: 1.0 (default)

### Linear Damping
Air resistance - how quickly limbs slow down in flight.

**Range**: 1.0 - 10.0
- **1.0 - 2.0**: Very bouncy, slow to settle
- **3.0 - 5.0**: Normal air resistance (recommended)
- **6.0 - 10.0**: Heavy air resistance, quick settling

**Recommended**: 3.0

### Angular Damping
Spin resistance - how quickly limbs stop rotating.

**Range**: 1.0 - 10.0
- **1.0 - 3.0**: Lots of spinning and tumbling
- **4.0 - 6.0**: Normal spin resistance (recommended)
- **7.0 - 10.0**: Very quick stop to spinning

**Recommended**: 5.0

### Explosion Force Multiplier
How strongly explosions push ragdoll limbs.

**Range**: 1.0 - 5.0
- **1.0 - 2.0**: Gentle push, realistic
- **2.1 - 3.0**: Moderate force (recommended)
- **3.1 - 5.0**: Explosive, cartoon-like

**Recommended**: 2.0

### Lifetime
How long ragdolls persist before automatically disappearing.

**Range**: 5 - 15 seconds
- **5 - 7 seconds**: Quick cleanup, good for intense action
- **8 - 10 seconds**: Good balance (recommended)
- **11 - 15 seconds**: Longer for dramatic effect

**Recommended**: 8 seconds

## Physics Presets

### Cartoon Style (Fun and Bouncy)
Perfect for casual, lighthearted gameplay.

```csharp
Joint Stiffness: 0.7
Limb Mass: 0.8
Linear Damping: 2.0
Angular Damping: 3.0
```

**Best for**: Family-friendly gameplay, comedy moments

### Realistic Style (Serious Physics)
For players who prefer believable character movement.

```csharp
Joint Stiffness: 0.3
Limb Mass: 1.2
Linear Damping: 4.0
Angular Damping: 6.0
```

**Best for**: Realistic gameplay, simulation-style games

### Bouncy Style (Maximum Comedy)
For maximum silliness and fun visual effects.

```csharp
Joint Stiffness: 0.9
Limb Mass: 0.5
Linear Damping: 1.0
Angular Damping: 2.0
```

**Best for**: Comedy games, party games, casual mobile games

## Visual Polish

### Sprite Setup
1. **Head**: Should carry face customization from StickClone
2. **Torso**: Main body that anchors the ragdoll
3. **Arms**: Thin rectangles, rounded ends look good
4. **Legs**: Similar to arms but slightly thicker

### Impact Effects
The system automatically triggers:
- **Screen Shake**: Based on explosion force
- **Particles**: Explosion dust and debris
- **Audio**: Impact sounds and limb collisions
- **Face Changes**: Can change to "hurt" expression

### Collision Layers
- **Ragdoll Limbs**: Layer 3 (they don't collide with each other)
- **Environment**: Layer 2 (collide with world objects)
- **Other Ragdolls**: Can optionally collide for pile-ups

## Performance Tuning

### Ragdoll Limits
- **Maximum Simultaneous**: 3-4 ragdolls for good performance
- **Object Pooling**: Enabled by default to reuse ragdolls
- **Auto-Cleanup**: Ragdolls disappear after lifetime expires

### Optimization Tips
1. **Lower Quality**: Reduce limb mass and increase damping for simpler physics
2. **Higher Quality**: Increase joint stiffness and reduce damping for complex movement
3. **Mobile Devices**: Use Cartoon preset with shorter lifetimes (5-7 seconds)
4. **Desktop/Console**: Use Realistic preset with longer lifetimes

## Troubleshooting

### Common Issues

**Ragdolls Not Spawning**:
- Check that RagdollSpawner is added to the scene
- Verify explosions are being detected (check console logs)
- Ensure StickClones are in the correct collision layer

**Ragdolls Too Bouncy/Not Bouncy Enough**:
- Adjust Linear/Angular Damping values
- Try different physics presets
- Modify Explosion Force Multiplier

**Performance Issues**:
- Reduce maximum simultaneous ragdolls
- Shorter lifetimes
- Enable object pooling (already default)
- Use Cartoon preset for simpler physics

**Limbs Falling Off**:
- Increase Joint Stiffness
- Check that collision layers are correct
- Verify PinJoint2D connections are working

### Console Debug Info
The system provides helpful debug information:
- Ragdoll creation and destruction
- Explosion detection
- Physics parameter changes
- Performance statistics

## Advanced Configuration

### Custom Explosion Detection
You can modify the explosion detection radius in StickClone.cs:
```csharp
Radius = 60.0f // Detection radius for explosions
```

### Adding New Explosion Sources
Extend the `OnPotentialExplosionDetected` method to detect other explosion sources:
```csharp
// Example for explosive props
if (body is ExplosiveProp prop)
{
    // Handle explosive prop detection
}
```

### Custom Face Customization
The system transfers face customization automatically, but you can customize this by modifying the `ApplyFaceCustomization` method in `RagdollStickClone.cs`.

### Pooling Configuration
Adjust object pooling in RagdollSpawner:
```csharp
Enable Ragdoll Pooling: true
Pre-warmed Pool Size: 2
Maximum Pool Size: 5
```

## Game Feel Integration

### Screen Shake
Ragdoll impacts trigger screen shake through the existing GameFeelManager:
- **Small Impacts**: Light shake
- **Large Explosions**: Heavy shake
- **Limb Collisions**: Moderate shake

### Audio
Uses existing AudioManager impact sounds:
- **Explosion**: Main explosion sound
- **Limb Impacts**: Thud/collision sounds
- **Settling**: Quieter settling sounds

### Particles
Leverages existing EffectsManager:
- **Explosion**: Explosion particle effects
- **Dust**: Settling dust when ragdolls expire
- **Impact**: Particle bursts on strong impacts

## Level Design Tips

### Explosion Placement
- Place explosive objects near paths where StickClones walk
- Consider explosion radius when positioning obstacles
- Create interesting physics scenarios with strategic placement

### Ragdoll-Friendly Environments
- Ensure ragdolls have space to fall and settle
- Avoid placing important objects where ragdolls might interfere
- Consider ragdoll lifetimes when designing level timing

### Performance Considerations
- Use ragdoll limits to prevent overwhelming the player
- Consider your target platform when choosing physics presets
- Test performance with multiple simultaneous ragdolls

## Reference

### Script Locations
- **Main Ragdoll Controller**: `Scripts/RagdollStickClone.cs`
- **Individual Limbs**: `Scripts/RagdollLimb.cs`
- **Joint Management**: `Scripts/RagdollLimbConnector.cs`
- **Spawner System**: `Scripts/RagdollSpawner.cs`
- **Modified StickClone**: `Scripts/StickClone.cs`
- **Modified Projectile**: `Scripts/Projectile.cs`

### Key Methods
- `Initialize(spawnPosition, faceCustomization)`: Set up new ragdoll
- `ApplyExplosionForce(epicenter, force, radius)`: Main explosion handling
- `SetDampingLevel(damping)`: Adjust physics behavior
- `ApplyPhysicsPreset(preset)`: Quick preset application

### Signals
- `RagdollCreated`: Emitted when ragdoll is fully initialized
- `RagdollDespawned`: Emitted when ragdoll lifetime expires
- `LimbImpacted`: Emitted when limbs experience impacts
- `ExplosionDetected`: Emitted when explosion is detected

---

**Need Help?** Check the console for debug messages, and adjust physics parameters gradually for the best results!