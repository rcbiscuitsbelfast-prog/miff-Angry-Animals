# Angry Animals - Known Limitations

This document lists the current known issues, constraints, and pending items for the Angry Animals project as of January 7, 2025.

## Visuals
- **Placeholder Art**: The game currently uses `ColorRect` nodes for 90% of visual elements. High-quality PNG assets are required for a commercial release.
- **Animations**: Character animations are currently driven by code and simple transforms; skeletal animations (e.g., via Spine or Godot's Skeleton2D) are not yet implemented.

## Monetization
- **Account Configuration**: While the AdMob and IAP code is integrated, it requires actual AdMob App IDs and Store IDs to function in a production environment.
- **Currency System**: There is currently no soft or hard currency system (coins/gems).

## Platforms
- **iOS/Android Signing**: Exporting to mobile requires valid developer certificates and signing keys not included in the repository.
- **Input**: Gamepad support is partially implemented via standard Godot UI navigation but hasn't been fully tuned for gameplay.

## Code
- **Asset Paths**: Some scripts use hardcoded paths in `res://Assets/` which may need adjustment if the folder structure changes.
- **Inconsistent Folder Names**: The project contains both `Script/` and `scripts/` directories.

## Content
- **100 Levels**: While 100 levels exist, they vary in complexity and may require additional balancing.
- **Settings Menu**: The settings menu (volume, difficulty, etc.) is partially implemented but needs visual polish.

## Performance
- **Physics Load**: Levels with 50+ rigid bodies may experience frame drops on low-end mobile devices.
