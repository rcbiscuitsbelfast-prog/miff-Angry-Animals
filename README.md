# Angry Animals

Angry Animals is a 2D physics-based puzzle game developed in Godot 4.4 and C#. Players use various slingshots to launch animals at obstacles, followed by a traversal phase where they control a character to reach the exit.

## Features
- **100 Hand-Crafted Levels**: Progress through a variety of challenging puzzles.
- **Infinite Procedural Levels**: Seeded random generation for endless replayability.
- **Face Customization**: Personalize your character with hats, glasses, and even your own photo!
- **Monetization Ready**: Integrated AdMob and In-App Purchases (Freemium model).
- **Game Feel Systems**: Screen shake, haptic feedback, and particle effects for a polished experience.

## How to Run
1.  **Clone the Repository**:
    ```bash
    git clone https://github.com/cto-new/angry-animals.git
    ```
2.  **Open in Godot**:
    - Launch Godot 4.4+ (Mono/C# version).
    - Import the project by selecting `project.godot`.
3.  **Build C# Project**:
    - Build the solution within Godot or via command line:
    ```bash
    dotnet build
    ```
4.  **Press Play**:
    - The game starts from `res://Scenes/MainMenu.tscn`.

## Key Controls
- **Mouse Left Button**: Drag and launch animals from the slingshot.
- **Arrows / WASD**: Move character during traversal phase.
- **Space**: Continue to next level / Retry.
- **Escape**: Return to menu.

## Project Structure
- `res://Scenes/`: All game scenes (.tscn)
- `res://Script/`: Main gameplay C# scripts
- `res://Globals/`: Singleton managers
- `res://Assets/`: Placeholders (Note: High-quality sprites need to be added to replace ColorRects)

## Production Audit Status
- **Status**: ✅ Turnkey Ready (with placeholders)
- **Codebase**: 7,200+ lines of robust C#
- **Tests**: Game loop verified from Main Menu to Level Complete.

## License
MIT License - See LICENSE file for details.
