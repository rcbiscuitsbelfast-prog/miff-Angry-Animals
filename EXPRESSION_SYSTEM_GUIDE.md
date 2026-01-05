# Facial Expression System Guide

The Angry Animals facial expression system brings the character to life with 14 distinct emotions that respond dynamically to gameplay events and physics.

## Expression Types

- **Neutral**: Default state when stationary.
- **Determined**: Triggered during launch.
- **Excited**: Triggered during high-speed movement.
- **Scared**: Triggered when moving extremely fast or falling.
- **Dizzy**: Triggered after a hard impact.
- **Curious**: Randomly triggered during flight.
- **Frightened**: Triggered during rapid acceleration/deceleration.
- **Happy**: Triggered on level success or high destruction.
- **Bored**: Triggered when moving slowly.
- **Angry**: Triggered when taking damage or near enemies.
- **Nauseous**: Randomly triggered during spins.
- **Melting**: Triggered in hot environments (planned).
- **Cold**: Triggered in ice environments (planned).
- **Disgusted**: Triggered when hitting water or mud.

## Technical Implementation

The system uses procedural drawing via Godot's `_Draw` method in `ExpressionManager.cs`. This ensures that expressions always look crisp at any resolution and can be overlaid on any face image (camera-captured or gallery-selected).

### Key Components:
- **Eyebrows**: Tilt and shift based on emotion intensity.
- **Eyes**: Change shape (circles, crescents, Xs) and support natural blinking.
- **Mouth**: Changes shape (lines, arcs, Os, rectangles) to match the emotion.

## Integration

Expressions are automatically managed by the `FaceProjectile` class, which monitors physics properties (velocity, acceleration) to trigger the appropriate reaction.

- **Launch** -> Determined
- **Speed > 1000** -> Scared
- **Speed < 50** -> Bored
- **Impact** -> Dizzy
- **Random (1%)** -> Variety of flight emotions
