# Angry Animals - Asset Guide

This guide details the specifications for assets required to replace the `ColorRect` placeholders currently used in the project.

## Sprite Specifications

### Slingshots
- **Path**: `res://Assets/Sprites/Infrastructure/`
- **Variants**: Classic, Metal, Magic, Modern
- **Resolution**: 512x512 pixels
- **Pivot**: Bottom Center
- **Format**: PNG (32-bit RGBA)

### Animals (Projectiles)
- **Path**: `res://Assets/Sprites/Characters/Animals/`
- **Resolution**: 128x128 pixels
- **Format**: PNG (32-bit RGBA)

### Obstacles (Cups)
- **Path**: `res://Assets/Sprites/Obstacles/`
- **Variants**: Basic, Metal, Glass, Special
- **Resolution**: 128x128 pixels
- **Format**: PNG (32-bit RGBA)

### Character (StickClone)
- **Path**: `res://Assets/Sprites/Characters/StickClone/`
- **Body**: 256x256 pixels
- **Expressions**: 64x64 pixels (residing in `res://Assets/Sprites/Face/`)
- **Accessories**: 128x128 pixels (Hats, Glasses)

### UI Elements
- **Path**: `res://Assets/Sprites/UI/`
- **Buttons**: 200x80 pixels (Normal, Hover, Pressed, Disabled)
- **Panels**: 9-patch sprites for responsive dialogs
- **Icons**: 64x64 pixels

## Audio Specifications

### Music
- **Format**: OGG (preferred) or MP3
- **Looping**: Enabled for background tracks
- **Path**: `res://Assets/Audio/Music/`

### Sound Effects (SFX)
- **Format**: WAV (preferred for low latency)
- **Path**: `res://Assets/Audio/SFX/`
- **Key Sounds**: 
  - `ui_click`: Menu navigation
  - `slingshot_stretch`: Dragging animal
  - `slingshot_release`: Launching animal
  - `impact_wood`: Animal hitting cup
  - `level_win`: Level completion celebration

## Naming Conventions
- Files should be lowercase with underscores (e.g., `slingshot_classic.png`).
- Group similar assets in subdirectories.

## Current Placeholder Count
- 110 `ColorRect` nodes identified for replacement.
- Use `res://Classes/AssetValidationTool.cs` to track progress.
