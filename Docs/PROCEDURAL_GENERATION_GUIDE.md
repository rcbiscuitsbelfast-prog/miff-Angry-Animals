# Procedural Generation & Material Distribution Guide

This document explains the difficulty-aware procedural generation system, material distribution, and seeding logic implemented in Phase 2.

## 1. Material Distribution Algorithm

The `MaterialDistributor` singleton manages how materials are distributed across rooms based on difficulty tiers.

### Difficulty Tiers:
- **Rooms 1-20** (Easy): 70% Wood, 20% Stone, 10% Brick
- **Rooms 21-40** (Medium): 30% Wood, 40% Stone, 20% Brick, 10% Iron
- **Rooms 41-60** (Hard): 20% Stone, 30% Brick, 40% Iron, 10% Diamond
- **Rooms 61+** (Extreme): 10% Brick, 40% Iron, 50% Diamond

### Tuning:
Non-coders can adjust these percentages in the Inspector on the `MaterialDistributor` node.
- `EasyModeToughnessFactor`
- `MediumModeToughnessFactor`
- `HardModeToughnessFactor`

## 2. Obstacle Placement Patterns

The `LevelGenerator` now uses three distinct structural patterns, chosen deterministically by the room seed:

1.  **Tower**: A single tall stack of obstacles. Softer materials are placed at the bottom to ensure structural stability (conceptually) and easier initial hits.
2.  **Wall**: A horizontal barrier of obstacles. Materials have mixed hardness.
3.  **Scattered**: Random clusters. Hard materials (Iron, Diamond) are clustered near the center, while softer materials are spread out.

### Material Clustering & Height:
- **Soft Materials**: Spread across the room with varied heights.
- **Hard Materials**: Clustered near the center and slightly elevated to require more precise shots.

## 3. Difficulty Balancing

The `DifficultyBalancer` calculates a room's difficulty score (0.0 to 1.0) based on:
- **Material Hardness**: Weighted average of materials in the room.
- **Obstacle Count**: Scaled by room number and capped based on hardness.
- **Layout Pattern**: Tower (0.3), Wall (0.6), Scattered (0.9).

### Balance Rules:
- Harder rooms (more Diamond/Iron) have a lower maximum obstacle count (max 8).
- Softer rooms can have up to 15 obstacles.

## 4. Deterministic Seeding System

Levels can be shared and reconstructed using 32-bit integer seeds.

### Seed Composition:
- **Bits 0-15**: Room Number
- **Bits 16-17**: Layout Variant (Pattern)
- **Bits 18-27**: Material Variant

Use `LevelGenerator.CreateSeedFromParameters()` to generate a seed and `LevelGenerator.TryDecodeSeedToParameters()` to retrieve parameters from a seed.

## 5. Test Scene

The `Scenes/Tests/ProceduralDifficultyTest.tscn` scene allows for instant visualization and tuning. It displays multiple rooms side-by-side with their difficulty metrics and material distributions.
