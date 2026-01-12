# MediaPipe Facial Detection & Animation System

## Overview

The MediaPipe Facial Detection & Animation System transforms the Angry Animals face customization from static emotion overlays to sophisticated, animated facial expressions that actually deform the player's captured face photo.

## System Architecture

### Core Components

1. **FaceDetectionManager** - Handles facial landmark detection using MediaPipe or fallback methods
2. **FaceRiggerSystem** - Creates skeletal bone rig from detected landmarks  
3. **FaceAnimationController** - Maps expressions to bone transformations
4. **FaceDeformationMesh** - Renders the deformed face mesh
5. **FaceLandmarkVisualizer** - Provides UI for landmark adjustment

### Animation Flow

```
Photo Capture → Face Detection → Landmark Extraction → Rig Creation → Animation → Deformation
```

## Key Features

### ✅ **MediaPipe Integration**
- Detects 468 facial landmarks with confidence scores
- Extracts key facial features (eyes, eyebrows, mouth, nose, jaw)
- Fallback to simple detection when MediaPipe unavailable

### ✅ **Skeletal Animation System**
- Creates invisible bone skeleton from detected landmarks
- Supports 18+ facial bones for detailed control
- Parent-child relationships for natural movement

### ✅ **Expression Mapping**
- Maps all 14+ expressions to bone transformations:
  - **Smile**: Rotate jaw down, move mouth corners up, raise cheeks
  - **Blink**: Scale eye bones to close vertically
  - **Angry**: Raise eyebrows inward, narrow eyes, press mouth down
  - **Surprised**: Raise eyebrows high, open mouth, widen eyes
  - **Sad**: Lower inner eyebrows, droop cheeks
  - **Scared**: Wide eyes, O-shaped mouth
  - **Dizzy**: Cross eyes, squiggle mouth

### ✅ **Smooth Animation**
- Easing curves for natural transitions
- Intensity-based expression scaling
- Automatic blinking during flight

### ✅ **Interactive Landmark Adjustment**
- Visual circles/dots show detected landmarks
- Drag-to-correct manual adjustment mode
- Confidence-based color coding
- Confirmation workflow

### ✅ **Performance Optimization**
- CPU mesh deformation with 60 FPS target
- GPU shader deformation option
- Efficient vertex weight calculation
- Fallback to static emotions if needed

## User Workflow

### 1. Photo Capture
```
Camera/Gallery → Image Resized (256x256) → Auto Face Detection
```

### 2. Landmark Detection
```
MediaPipe Analysis → 468 Landmarks → Key Feature Extraction → Confidence Scoring
```

### 3. Detection Confirmation
```
Show Landmark Circles → "Looks Good?" / "Adjust Detection" → Manual Correction (if needed)
```

### 4. Save & Rig Creation
```
Save Image + JSON → Create Bone Rig → Generate Vertex Weights → Ready for Animation
```

### 5. In-Game Animation
```
Load Face + Rig → Expression Triggers → Bone Animation → Face Deformation → Render
```

## Technical Implementation

### FaceDetectionManager
```csharp
// Detect landmarks asynchronously
_faceDetectionManager.DetectLandmarksAsync(capturedImage);

// Returns: 468 landmarks + confidence scores
public class FaceLandmarks {
    public Vector2[] AllPoints;        // 468 MediaPipe points
    public float[] ConfidenceScores;   // Detection confidence
    public KeyLandmarks KeyFeatures;   // Extracted features
    public Rect2 FaceBounds;          // Bounding box
}
```

### FaceRiggerSystem
```csharp
// Create bone rig from landmarks
_faceRiggerSystem.CreateRig(landmarks, faceTexture);

// Generates:
// - 18+ facial bones (eyes, mouth, jaw, etc.)
// - Vertex weights for mesh deformation
// - Parent-child bone hierarchy
```

### FaceAnimationController
```csharp
// Set facial expression
_faceAnimationController.SetExpression(ExpressionType.Happy, intensity: 0.8f);

// Animates bones with smooth easing
// Handles blinking and expression blending
```

### FaceDeformationMesh
```csharp
// Updates mesh deformation based on bone positions
// Two methods:
// 1. CPU: Direct vertex manipulation
// 2. GPU: Shader-based texture deformation
```

## Configuration & Settings

### Expression Intensity Mapping
```csharp
// Speed-based intensity
case ExpressionType.Scared:
    return Mathf.Clamp(speed / 1500f, 0.5f, 1.0f);

// Acceleration-based intensity  
case ExpressionType.Dizzy:
    return Mathf.Clamp(acceleration / 10000f, 0.7f, 1.0f);
```

### Bone Transformation Presets
```csharp
// Happy expression mapping
{
    "JawBottom": 0.3f,      // Jaw drop
    "MouthLeftCorner": 0.4f, // Smile up
    "MouthRightCorner": 0.4f,
    "LeftCheek": 0.2f,      // Cheek raise
    "RightCheek": 0.2f
}
```

### Mesh Resolution Settings
```csharp
// Performance vs Quality
private int meshResolution = 32;  // 32x32 grid (1024 vertices)
// Alternative: 64x64 for ultra quality
// Alternative: 16x16 for mobile performance
```

## File Structure

```
/Scripts/
├── FaceDetectionManager.cs          # MediaPipe wrapper
├── FaceRiggerSystem.cs             # Bone creation & management  
├── FaceAnimationController.cs      # Expression-to-bone mapping
├── FaceDeformationMesh.cs          # Mesh deformation rendering
├── FaceLandmarkVisualizer.cs       # Landmark visualization UI
├── LandmarkData.cs                 # JSON serialization
├── FaceCustomizationScreen.cs      # Enhanced capture UI
└── FaceProjectile.cs               # Animated face projectile
```

### Saved Data Structure
```
/user://faces/
├── player_face.png              # Captured face image
└── player_landmarks.json       # Landmark data

{
    "DetectionTime": "2025-01-12 10:29:15",
    "AverageConfidence": 0.85,
    "KeyFeatures": {
        "LeftEye": {"X": 89.2, "Y": 45.1},
        "RightEye": {"X": 166.8, "Y": 45.1},
        // ... all 12 key features
    },
    "AllLandmarks": [ /* 468 landmark points */ ],
    "ConfidenceScores": [ /* 468 confidence values */ ]
}
```

## Fallback Strategy

### Detection Failure Levels
1. **MediaPipe Available** → Full 468-point detection
2. **Simple Detection** → Basic 18 landmark approximation  
3. **No Detection** → Default landmark positions
4. **No Landmarks** → Legacy ExpressionManager system

### Automatic Fallback
```csharp
// FaceProjectile initialization
if (_useAdvancedAnimation && HasLandmarks())
{
    InitializeAdvancedAnimation();
}
else
{
    InitializeLegacyAnimation(); // Original ExpressionManager
}
```

## Performance Considerations

### Mobile Optimization
- **Mesh Resolution**: Start with 16x16, increase if performance allows
- **Update Rate**: Limit to 60 FPS maximum
- **Memory**: Cache landmark data, avoid re-detection
- **GPU Fallback**: Use shader deformation on capable devices

### Desktop Enhancement  
- **High Resolution**: 32x32 or 64x64 mesh
- **GPU Deformation**: Enable shader-based approach
- **Advanced Easing**: Elastic and bounce curves
- **Debug Mode**: Visual bone/landmark overlay

## Debug & Testing

### Visual Debugging
```csharp
// Enable landmark visualization
_faceRiggerSystem.SetDebugMode(true);

// Show bone positions and mesh
_faceLandmarkVisualizer.SetLandmarksVisible(true);
```

### Expression Testing
```csharp
// Test all expressions programmatically
foreach (ExpressionType expr in Enum.GetValues<ExpressionType>())
{
    _faceAnimationController.SetExpression(expr, 1.0f);
    await ToSignal(GetTree().CreateTimer(1.0f), SceneTreeTimer.SignalName.Timeout);
}
```

## Non-Coder Configuration

### Inspector Exposed Settings
- **FaceProjectile**: `_useAdvancedAnimation` toggle
- **Mesh Resolution**: Performance vs quality slider
- **Animation Speed**: Expression transition timing
- **Debug Mode**: Show/hide bones and landmarks

### JSON Customization
```json
{
    "ExpressionPresets": {
        "Happy": {
            "Duration": 0.5,
            "Curve": "EaseInOut"
        }
    },
    "MeshSettings": {
        "Resolution": 32,
        "DeformationStrength": 1.0
    }
}
```

## Troubleshooting

### Common Issues

**❌ "Face detection failed"**
- Check camera permissions
- Verify image quality (clear, front-facing)
- Use fallback manual adjustment

**❌ "Animation looks weird"**
- Enable debug mode to check landmark positions
- Adjust individual landmarks manually
- Check bone weight assignments

**❌ "Poor performance on mobile"**
- Reduce mesh resolution to 16x16
- Disable GPU deformation
- Check frame rate in debug overlay

**❌ "Expressions not animating"**
- Verify landmarks file exists
- Check rig creation success
- Ensure animation controller initialized

### Performance Monitoring
```csharp
// Monitor deformation performance
GD.Print($"Mesh vertices: {_deformationMesh.MeshResolution}");
GD.Print($"Deformation FPS: {1.0f / delta}");
```

## Future Enhancements

### Planned Features
- **Real-time Face Tracking**: Video-based expression detection
- **Voice-Driven Animation**: Lip-sync with audio
- **AR Integration**: Live camera face overlay
- **Advanced Blending**: Multiple expression mixing
- **Physics Simulation**: Hair/clothing physics
- **GPU Compute**: HLSL shader deformation
- **Machine Learning**: Custom expression training

### Integration Points
- **Analytics**: Track expression usage patterns
- **Social Features**: Share animated expressions
- **Accessibility**: Eye-tracking for motor impairments
- **VR/AR**: Spatial face tracking
- **Multiplayer**: Synchronized expressions

## Migration Guide

### From Static to Animated
1. **Existing Saves**: Automatically use fallback system
2. **New Captures**: Trigger detection automatically  
3. **Performance**: Graceful degradation on weak devices
4. **Compatibility**: 100% backward compatible

### User Impact
- **No Change Required**: Existing functionality preserved
- **Enhanced Experience**: More realistic expressions
- **Optional Features**: Manual landmark adjustment
- **Performance Choice**: Quality vs speed settings

---

*This system transforms Angry Animals from a simple slingshot game into an engaging, personalized experience where players see their own faces come alive with emotions during gameplay.*