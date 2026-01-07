# Expression Assets Guide

Angry Animals uses a procedural rendering system for facial expressions, but it also supports sprite-swapping for custom art.

## Procedural Assets

Currently, the system uses code-defined shapes:
- **Eyes**: `DrawCircle`, `DrawArc`, `DrawLine`
- **Eyebrows**: `DrawLine`
- **Mouth**: `DrawArc`, `DrawLine`, `DrawRect`

## Custom Art Requirements (Optional)

If replacing procedural expressions with static sprites, each "Expression Pack" should contain:

| Feature | Variants | Examples |
|---------|----------|----------|
| **Eyes** | Open, Closed (Blink), Squint, Wide, Dizzy (X), Crescent | `eyes_wide.png`, `eyes_blink.png` |
| **Brows** | Neutral, Raised, Furrowed, Single-up | `brows_angry.png`, `brows_surprised.png` |
| **Mouth** | Line, Smile, Frown, O-shape, Grimace, Teeth | `mouth_happy.png`, `mouth_scared.png` |

## Asset Configuration

Assets should be 256x256 pixels with transparency (PNG). The face image itself is used as the base layer, with eyebrows, eyes, and mouth rendered as overlays in that order.
