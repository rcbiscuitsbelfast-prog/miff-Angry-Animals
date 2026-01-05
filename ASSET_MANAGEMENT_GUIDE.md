# Asset Management Guide: Angry Animals

This guide explains how to manage, upload, and swap assets in Angry Animals without needing to modify the core game code.

## 1. Recommended Folder Structure
The project is organized to keep assets separate from code and scenes. Use the following structure:

- `res://Assets/Sprites/`: All image files (.png, .svg).
  - `/Characters/`: Bird faces, StickClone body parts.
  - `/Environment/`: Ground textures, backgrounds, clouds.
  - `/UI/`: Buttons, icons, panels.
  - `/Props/`: Cups, crates, obstacles.
- `res://Assets/Audio/`: All sound files (.ogg, .mp3, .wav).
  - `/Music/`: Background tracks.
  - `/SFX/`: Explosions, slingshot sounds, UI clicks.

## 2. How to Swap Assets
Most game objects currently use placeholders (ColorRects). To replace them with real art:

### Swapping Sprites
1. Upload your `.png` file to the appropriate folder in `res://Assets/Sprites/`.
2. Open the scene for the object (e.g., `res://Scenes/Obstacles/Cup.tscn`).
3. Delete the `ColorRect` node.
4. Add a `Sprite2D` node.
5. Drag your image from the FileSystem dock into the `Texture` property of the `Sprite2D`.
6. Adjust the scale and collision shape to match your new art.

### Swapping Audio
1. Upload your `.ogg` or `.mp3` file to `res://Assets/Audio/SFX/` or `res://Assets/Audio/Music/`.
2. Ensure the filename matches what the `AudioManager` expects, OR update the paths in `AudioManager.cs`:
   - `res://Assets/Audio/Music/BackgroundMusic.ogg`
   - `res://Assets/Audio/SFX/SlingshotSound.ogg`
   - `res://Assets/Audio/SFX/DestructionSound.ogg`

## 3. Supported Formats
| Asset Type | Supported Formats | Recommendation |
|------------|-------------------|----------------|
| **Sprites**| .png, .svg, .jpg | Use **.png** with transparency for gameplay objects. |
| **Audio**  | .ogg, .mp3, .wav | Use **.ogg** for music (loops better) and **.wav** for short SFX. |
| **Fonts**  | .ttf, .otf        | Use **.ttf** for best compatibility. |

## 4. Swapping Without Code
The `PlayerProfile` system is designed to allow players to upload their own faces. 
- **Face Images:** These are stored in the user's local gallery or captured via camera. 
- **Path:** The game stores the path to these images in `user://profile.json`. 

To change the default cosmetics (Hats, Glasses):
1. Navigate to the `StickClone.tscn` scene.
2. Locate the cosmetics nodes.
3. Replace their textures or visibility logic. The code uses indices (0, 1, 2) to toggle these, so as long as your new assets are in the scene tree, they will "just work."

## 5. Asset Preparation Tips
- **Resolution:** Aim for a base resolution of **1920x1080** (Full HD). Godot will scale the assets for mobile screens.
- **Power of Two:** While not strictly required for 2D, keeping textures in power-of-two sizes (256x256, 512x512) can improve performance on older mobile devices.
- **Compression:** Use Godot's built-in "VRAM Compressed" import setting for larger background images to save memory.
