using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// Visualizes detected facial landmarks with circles/dots for debugging and manual adjustment.
/// Provides an interactive interface for correcting landmark positions.
/// </summary>
public class FaceLandmarkVisualizer : Control
{
    [Signal] public delegate void LandmarkAdjustedEventHandler(string landmarkName, Vector2 newPosition);
    [Signal] public delegate void DetectionConfirmedEventHandler();

    private FaceDetectionManager.FaceLandmarks _landmarks;
    private TextureRect _faceImage;
    private TextureRect _landmarkOverlay;
    private ColorRect _confidenceOverlay;
    private Dictionary<string, Color> _landmarkColors;
    private Dictionary<string, bool> _landmarkSelectable;
    
    // UI State
    private bool _isEditMode = false;
    private bool _isDragging = false;
    private string _selectedLandmark = "";
    private float _landmarkRadius = 8f;
    
    // Visual Settings
    private Color _goodConfidenceColor = Colors.Green;
    private Color _mediumConfidenceColor = Colors.Yellow;
    private Color _lowConfidenceColor = Colors.Red;
    private Color _selectedColor = Colors.Cyan;
    private float _alpha = 0.7f;

    public FaceLandmarks Landmarks
    {
        get => _landmarks;
        set
        {
            _landmarks = value;
            UpdateVisualization();
        }
    }

    public bool IsEditMode
    {
        get => _isEditMode;
        set
        {
            _isEditMode = value;
            UpdateInteractionMode();
        }
    }

    public override void _Ready()
    {
        SetupUI();
        InitializeLandmarkColors();
    }

    private void SetupUI()
    {
        SetAnchorsAndMarginsFull();
        
        // Face image
        _faceImage = new TextureRect();
        _faceImage.ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize;
        _faceImage.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
        _faceImage.SetAnchorsPreset(LayoutPreset.FullRect);
        AddChild(_faceImage);
        
        // Landmark overlay (transparent)
        _landmarkOverlay = new TextureRect();
        _landmarkOverlay.ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize;
        _landmarkOverlay.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
        _landmarkOverlay.SetAnchorsPreset(LayoutPreset.FullRect);
        _landmarkOverlay.MouseFilter = Control.MouseFilter.Stop;
        _landmarkOverlay.SelfModulate = new Color(1, 1, 1, 0); // Transparent
        AddChild(_landmarkOverlay);
        
        // Confidence visualization overlay
        _confidenceOverlay = new ColorRect();
        _confidenceOverlay.SetAnchorsPreset(LayoutPreset.FullRect);
        _confidenceOverlay.SelfModulate = new Color(0, 1, 0, 0); // Semi-transparent green
        _confidenceOverlay.Visible = false;
        AddChild(_confidenceOverlay);
    }

    private void InitializeLandmarkColors()
    {
        _landmarkColors = new Dictionary<string, Color>
        {
            { "LeftEye", Colors.Blue },
            { "RightEye", Colors.Blue },
            { "LeftEyebrow", Colors.Purple },
            { "RightEyebrow", Colors.Purple },
            { "LeftMouthCorner", Colors.Green },
            { "RightMouthCorner", Colors.Green },
            { "MouthCenter", Colors.Yellow },
            { "NoseTip", Colors.Orange },
            { "JawLeft", Colors.Red },
            { "JawRight", Colors.Red },
            { "JawBottom", Colors.Magenta },
            { "FaceCenter", Colors.White },
            { "UpperLip", Colors.CadetBlue },
            { "LowerLip", Colors.CadetBlue },
            { "LeftCheek", Colors.LightGreen },
            { "RightCheek", Colors.LightGreen },
            { "Forehead", Colors.LightBlue },
            { "Chin", Colors.LightCoral }
        };

        _landmarkSelectable = new Dictionary<string, bool>
        {
            { "LeftEye", true },
            { "RightEye", true },
            { "LeftEyebrow", true },
            { "RightEyebrow", true },
            { "LeftMouthCorner", true },
            { "RightMouthCorner", true },
            { "MouthCenter", true },
            { "NoseTip", true },
            { "JawLeft", true },
            { "JawRight", true },
            { "JawBottom", true },
            { "FaceCenter", false }, // Usually auto-calculated
            { "UpperLip", true },
            { "LowerLip", true },
            { "LeftCheek", true },
            { "RightCheek", true },
            { "Forehead", true },
            { "Chin", true }
        };
    }

    /// <summary>
    /// Sets the face image to display with landmarks
    /// </summary>
    public void SetFaceImage(Texture2D faceTexture)
    {
        _faceImage.Texture = faceTexture;
    }

    /// <summary>
    /// Updates the landmark visualization
    /// </summary>
    public void UpdateVisualization()
    {
        if (_landmarks == null) return;
        
        // Create an image with landmarks drawn on it
        DrawLandmarksOnImage();
        
        // Update confidence visualization
        UpdateConfidenceVisualization();
    }

    private void DrawLandmarksOnImage()
    {
        // Create a transparent overlay image
        var overlayImage = Image.Create(512, 512, false, Image.Format.Rgba8);
        overlayImage.Fill(new Color(0, 0, 0, 0)); // Transparent

        var keyFeatures = _landmarks.KeyFeatures;
        
        // Draw all key landmarks
        DrawLandmarkCircle(overlayImage, keyFeatures.LeftEye, "LeftEye", GetConfidenceColor(_landmarks.ConfidenceScores[33]));
        DrawLandmarkCircle(overlayImage, keyFeatures.RightEye, "RightEye", GetConfidenceColor(_landmarks.ConfidenceScores[362]));
        DrawLandmarkCircle(overlayImage, keyFeatures.LeftEyebrow, "LeftEyebrow", GetConfidenceColor(_landmarks.ConfidenceScores[105]));
        DrawLandmarkCircle(overlayImage, keyFeatures.RightEyebrow, "RightEyebrow", GetConfidenceColor(_landmarks.ConfidenceScores[336]));
        DrawLandmarkCircle(overlayImage, keyFeatures.LeftMouthCorner, "LeftMouthCorner", GetConfidenceColor(_landmarks.ConfidenceScores[61]));
        DrawLandmarkCircle(overlayImage, keyFeatures.RightMouthCorner, "RightMouthCorner", GetConfidenceColor(_landmarks.ConfidenceScores[291]));
        DrawLandmarkCircle(overlayImage, keyFeatures.MouthCenter, "MouthCenter", GetConfidenceColor(_landmarks.ConfidenceScores[14]));
        DrawLandmarkCircle(overlayImage, keyFeatures.NoseTip, "NoseTip", GetConfidenceColor(_landmarks.ConfidenceScores[1]));
        DrawLandmarkCircle(overlayImage, keyFeatures.JawLeft, "JawLeft", GetConfidenceColor(_landmarks.ConfidenceScores[172]));
        DrawLandmarkCircle(overlayImage, keyFeatures.JawRight, "JawRight", GetConfidenceColor(_landmarks.ConfidenceScores[397]));
        DrawLandmarkCircle(overlayImage, keyFeatures.JawBottom, "JawBottom", GetConfidenceColor(_landmarks.ConfidenceScores[200]));
        DrawLandmarkCircle(overlayImage, keyFeatures.FaceCenter, "FaceCenter", Colors.White * _alpha);
        
        // Draw connecting lines to show facial structure
        DrawFacialStructureLines(overlayImage, keyFeatures);
        
        // Convert to texture and display
        var overlayTexture = ImageTexture.CreateFromImage(overlayImage);
        _landmarkOverlay.Texture = overlayTexture;
        _landmarkOverlay.Visible = true;
    }

    private void DrawLandmarkCircle(Image image, Vector2 position, string landmarkName, Color color)
    {
        if (!_landmarkColors.ContainsKey(landmarkName))
        {
            _landmarkColors[landmarkName] = Colors.White;
        }
        
        Color drawColor = _landmarkColors[landmarkName];
        if (landmarkName == _selectedLandmark)
        {
            drawColor = _selectedColor;
        }
        
        drawColor.A = _alpha;
        
        // Draw circle outline
        int radius = (int)_landmarkRadius;
        int centerX = (int)position.X;
        int centerY = (int)position.Y;
        
        for (int angle = 0; angle < 360; angle += 5)
        {
            float radians = Mathf.DegToRad(angle);
            int x = centerX + (int)(radius * Mathf.Cos(radians));
            int y = centerY + (int)(radius * Mathf.Sin(radians));
            
            if (x >= 0 && x < image.GetWidth() && y >= 0 && y < image.GetHeight())
            {
                image.SetPixel(x, y, drawColor);
            }
        }
        
        // Draw center dot
        if (centerX >= 0 && centerX < image.GetWidth() && centerY >= 0 && centerY < image.GetHeight())
        {
            image.SetPixel(centerX, centerY, drawColor);
        }
        
        // Draw landmark name label
        DrawLandmarkLabel(image, position, landmarkName, drawColor);
    }

    private void DrawLandmarkLabel(Image image, Vector2 position, string landmarkName, Color color)
    {
        // For simplicity, just draw a small square next to each landmark
        // In a real implementation, you'd use a font to render text
        
        int labelSize = 4;
        int offsetX = 15;
        int offsetY = -5;
        
        for (int y = 0; y < labelSize; y++)
        {
            for (int x = 0; x < labelSize; x++)
            {
                int pixelX = (int)position.X + offsetX + x;
                int pixelY = (int)position.Y + offsetY + y;
                
                if (pixelX >= 0 && pixelX < image.GetWidth() && pixelY >= 0 && pixelY < image.GetHeight())
                {
                    image.SetPixel(pixelX, pixelY, color * 0.5f);
                }
            }
        }
    }

    private void DrawFacialStructureLines(Image image, FaceDetectionManager.KeyLandmarks features)
    {
        // Draw lines connecting key facial features
        
        // Eye outline
        DrawLine(image, features.LeftEye + new Vector2(-20, -5), features.RightEye + new Vector2(20, -5), Colors.Gray * 0.3f);
        DrawLine(image, features.LeftEye + new Vector2(-20, 5), features.RightEye + new Vector2(20, 5), Colors.Gray * 0.3f);
        
        // Eyebrows
        DrawLine(image, features.LeftEyebrow + new Vector2(-15, 0), features.LeftEyebrow + new Vector2(15, 0), Colors.Gray * 0.3f);
        DrawLine(image, features.RightEyebrow + new Vector2(-15, 0), features.RightEyebrow + new Vector2(15, 0), Colors.Gray * 0.3f);
        
        // Mouth
        DrawLine(image, features.LeftMouthCorner, features.MouthCenter, Colors.Gray * 0.3f);
        DrawLine(image, features.MouthCenter, features.RightMouthCorner, Colors.Gray * 0.3f);
        
        // Jaw
        DrawLine(image, features.JawLeft, features.FaceCenter, Colors.Gray * 0.2f);
        DrawLine(image, features.FaceCenter, features.JawRight, Colors.Gray * 0.2f);
        DrawLine(image, features.JawLeft, features.JawBottom, Colors.Gray * 0.2f);
        DrawLine(image, features.JawBottom, features.JawRight, Colors.Gray * 0.2f);
    }

    private void DrawLine(Image image, Vector2 start, Vector2 end, Color color)
    {
        float distance = start.DistanceTo(end);
        int steps = Mathf.Max(1, (int)(distance / 2));
        
        for (int i = 0; i <= steps; i++)
        {
            float t = (float)i / steps;
            Vector2 point = start.Lerp(end, t);
            
            int x = (int)point.X;
            int y = (int)point.Y;
            
            if (x >= 0 && x < image.GetWidth() && y >= 0 && y < image.GetHeight())
            {
                image.SetPixel(x, y, color);
            }
        }
    }

    private void UpdateConfidenceVisualization()
    {
        if (_landmarks == null) return;
        
        // Create a confidence heatmap
        var confidenceImage = Image.Create(512, 512, false, Image.Format.Rgba8);
        confidenceImage.Fill(new Color(0, 0, 0, 0));
        
        // Apply confidence-based color overlay
        for (int i = 0; i < _landmarks.ConfidenceScores.Length && i < _landmarks.AllPoints.Length; i++)
        {
            float confidence = _landmarks.ConfidenceScores[i];
            Vector2 point = _landmarks.AllPoints[i];
            
            Color confidenceColor = GetConfidenceColor(confidence);
            
            // Draw confidence circle
            DrawConfidenceCircle(confidenceImage, point, confidenceColor, confidence);
        }
        
        var confidenceTexture = ImageTexture.CreateFromImage(confidenceImage);
        _confidenceOverlay.Texture = confidenceTexture;
        _confidenceOverlay.Visible = true;
    }

    private void DrawConfidenceCircle(Image image, Vector2 position, Color color, float confidence)
    {
        int radius = (int)(10f * confidence);
        int centerX = (int)position.X;
        int centerY = (int)position.Y;
        
        for (int angle = 0; angle < 360; angle += 10)
        {
            float radians = Mathf.DegToRad(angle);
            int x = centerX + (int)(radius * Mathf.Cos(radians));
            int y = centerY + (int)(radius * Mathf.Sin(radians));
            
            if (x >= 0 && x < image.GetWidth() && y >= 0 && y < image.GetHeight())
            {
                image.SetPixel(x, y, color);
            }
        }
    }

    private Color GetConfidenceColor(float confidence)
    {
        if (confidence > 0.7f)
            return _goodConfidenceColor * confidence;
        else if (confidence > 0.4f)
            return _mediumConfidenceColor * confidence;
        else
            return _lowConfidenceColor * confidence;
    }

    private void UpdateInteractionMode()
    {
        // Update visual appearance based on edit mode
        if (_isEditMode)
        {
            _landmarkOverlay.MouseFilter = Control.MouseFilter.Stop;
            _landmarkOverlay.SelfModulate = new Color(1, 1, 1, 1);
        }
        else
        {
            _landmarkOverlay.MouseFilter = Control.MouseFilter.Ignore;
            _landmarkOverlay.SelfModulate = new Color(1, 1, 1, 0.7f);
        }
    }

    public override void _GuiInput(InputEvent @event)
    {
        if (!_isEditMode) return;
        
        if (@event is InputEventMouseButton mouseButton)
        {
            if (mouseButton.ButtonIndex == MouseButton.Left)
            {
                if (mouseButton.Pressed)
                {
                    HandleMouseDown(mouseButton.Position);
                }
                else
                {
                    HandleMouseUp(mouseButton.Position);
                }
            }
        }
        else if (@event is InputEventMouseMotion mouseMotion)
        {
            if (_isDragging)
            {
                HandleDrag(mouseMotion.Position);
            }
        }
    }

    private void HandleMouseDown(Vector2 mousePos)
    {
        // Find the closest landmark
        string closestLandmark = FindClosestLandmark(mousePos);
        
        if (!string.IsNullOrEmpty(closestLandmark) && _landmarkSelectable.GetValueOrDefault(closestLandmark, false))
        {
            _selectedLandmark = closestLandmark;
            _isDragging = true;
            UpdateVisualization();
        }
    }

    private void HandleMouseUp(Vector2 mousePos)
    {
        if (_isDragging)
        {
            _isDragging = false;
            _selectedLandmark = "";
            UpdateVisualization();
        }
    }

    private void HandleDrag(Vector2 mousePos)
    {
        if (string.IsNullOrEmpty(_selectedLandmark)) return;
        
        // Convert screen position to image coordinates
        Vector2 imagePos = ScreenToImagePosition(mousePos);
        
        // Update the landmark position
        UpdateLandmarkPosition(_selectedLandmark, imagePos);
        EmitSignal(SignalName.LandmarkAdjusted, _selectedLandmark, imagePos);
    }

    private string FindClosestLandmark(Vector2 mousePos)
    {
        if (_landmarks == null) return "";
        
        float minDistance = float.MaxValue;
        string closestLandmark = "";
        
        Vector2 imagePos = ScreenToImagePosition(mousePos);
        
        // Check key features
        var features = _landmarks.KeyFeatures;
        CheckLandmarkDistance("LeftEye", features.LeftEye, ref minDistance, ref closestLandmark, imagePos);
        CheckLandmarkDistance("RightEye", features.RightEye, ref minDistance, ref closestLandmark, imagePos);
        CheckLandmarkDistance("LeftEyebrow", features.LeftEyebrow, ref minDistance, ref closestLandmark, imagePos);
        CheckLandmarkDistance("RightEyebrow", features.RightEyebrow, ref minDistance, ref closestLandmark, imagePos);
        CheckLandmarkDistance("LeftMouthCorner", features.LeftMouthCorner, ref minDistance, ref closestLandmark, imagePos);
        CheckLandmarkDistance("RightMouthCorner", features.RightMouthCorner, ref minDistance, ref closestLandmark, imagePos);
        CheckLandmarkDistance("MouthCenter", features.MouthCenter, ref minDistance, ref closestLandmark, imagePos);
        CheckLandmarkDistance("NoseTip", features.NoseTip, ref minDistance, ref closestLandmark, imagePos);
        CheckLandmarkDistance("JawLeft", features.JawLeft, ref minDistance, ref closestLandmark, imagePos);
        CheckLandmarkDistance("JawRight", features.JawRight, ref minDistance, ref closestLandmark, imagePos);
        CheckLandmarkDistance("JawBottom", features.JawBottom, ref minDistance, ref closestLandmark, imagePos);
        CheckLandmarkDistance("FaceCenter", features.FaceCenter, ref minDistance, ref closestLandmark, imagePos);
        
        return minDistance < 20f ? closestLandmark : "";
    }

    private void CheckLandmarkDistance(string landmarkName, Vector2 landmarkPos, ref float minDistance, ref string closestLandmark, Vector2 mousePos)
    {
        float distance = landmarkPos.DistanceTo(mousePos);
        if (distance < minDistance && _landmarkSelectable.GetValueOrDefault(landmarkName, false))
        {
            minDistance = distance;
            closestLandmark = landmarkName;
        }
    }

    private Vector2 ScreenToImagePosition(Vector2 screenPos)
    {
        // Convert screen coordinates to image coordinates
        // This is a simplified version - in practice you'd need to account for aspect ratio and scaling
        
        Vector2 imageSize = new Vector2(512, 512); // Assuming 512x512 image
        Vector2 controlSize = GetSize();
        
        Vector2 normalizedPos = screenPos / controlSize;
        return normalizedPos * imageSize;
    }

    private void UpdateLandmarkPosition(string landmarkName, Vector2 newPosition)
    {
        if (_landmarks == null) return;
        
        // Update the specific landmark in KeyFeatures
        switch (landmarkName)
        {
            case "LeftEye":
                _landmarks.KeyFeatures.LeftEye = newPosition;
                break;
            case "RightEye":
                _landmarks.KeyFeatures.RightEye = newPosition;
                break;
            case "LeftEyebrow":
                _landmarks.KeyFeatures.LeftEyebrow = newPosition;
                break;
            case "RightEyebrow":
                _landmarks.KeyFeatures.RightEyebrow = newPosition;
                break;
            case "LeftMouthCorner":
                _landmarks.KeyFeatures.LeftMouthCorner = newPosition;
                break;
            case "RightMouthCorner":
                _landmarks.KeyFeatures.RightMouthCorner = newPosition;
                break;
            case "MouthCenter":
                _landmarks.KeyFeatures.MouthCenter = newPosition;
                break;
            case "NoseTip":
                _landmarks.KeyFeatures.NoseTip = newPosition;
                break;
            case "JawLeft":
                _landmarks.KeyFeatures.JawLeft = newPosition;
                break;
            case "JawRight":
                _landmarks.KeyFeatures.JawRight = newPosition;
                break;
            case "JawBottom":
                _landmarks.KeyFeatures.JawBottom = newPosition;
                break;
            case "FaceCenter":
                _landmarks.KeyFeatures.FaceCenter = newPosition;
                break;
        }
    }

    /// <summary>
    /// Shows or hides the landmark visualization
    /// </summary>
    public void SetLandmarksVisible(bool visible)
    {
        _landmarkOverlay.Visible = visible;
        _confidenceOverlay.Visible = visible;
    }

    /// <summary>
    /// Confirms the current landmark detection
    /// </summary>
    public void ConfirmDetection()
    {
        EmitSignal(SignalName.DetectionConfirmed);
    }
}