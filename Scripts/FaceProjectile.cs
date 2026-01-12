using Godot;

/// <summary>
/// A projectile with an animated face sprite, representing an animal character.
/// Extends the base Projectile class with sophisticated facial animation using detected landmarks.
/// </summary>
public partial class FaceProjectile : Projectile
{
    [Export] private Sprite2D _faceSprite;
    [Export] private bool _useAdvancedAnimation = true; // Toggle for new animation system
    
    // Animation components
    private FaceDetectionManager _faceDetectionManager;
    private FaceRiggerSystem _faceRiggerSystem;
    private FaceAnimationController _faceAnimationController;
    private FaceDeformationMesh _deformationMesh;
    
    // Legacy animation system (fallback)
    private ExpressionManager _legacyExpressionManager;
    
    private Vector2 _lastVelocity;
    private ExpressionType _lastExpression = ExpressionType.Neutral;
    private bool _hasTriggeredLaunchBubble = false;
    
    // Animation state
    private bool _hasLoadedFaceRig = false;
    private float _animationBlinkTimer = 0f;
    private float _animationUpdateTimer = 0f;
    private const float BLINK_INTERVAL_MIN = 2.0f;
    private const float BLINK_INTERVAL_MAX = 5.0f;

    public override void _Ready()
    {
        base._Ready();

        if (_useAdvancedAnimation && _faceSprite != null)
        {
            InitializeAdvancedAnimation();
        }
        else
        {
            InitializeLegacyAnimation();
        }

        LoadFaceImage();
    }

    private void InitializeAdvancedAnimation()
    {
        // Create face detection manager
        _faceDetectionManager = new FaceDetectionManager();
        AddChild(_faceDetectionManager);
        
        // Create face rig system
        _faceRiggerSystem = new FaceRiggerSystem();
        AddChild(_faceRiggerSystem);
        
        // Create animation controller
        _faceAnimationController = new FaceAnimationController(_faceRiggerSystem);
        AddChild(_faceAnimationController);
        
        // Create deformation mesh
        _deformationMesh = new FaceDeformationMesh();
        AddChild(_deformationMesh);
        
        // Set up deformation mesh with the face sprite
        if (_faceSprite != null)
        {
            _deformationMesh.Position = _faceSprite.Position;
            _deformationMesh.Scale = _faceSprite.Scale;
        }
        
        // Connect signals
        _faceAnimationController.Connect(FaceAnimationController.SignalName.ExpressionChanged, new Callable(this, nameof(OnExpressionChanged)));
        
        GD.Print("FaceProjectile: Advanced animation system initialized");
    }

    private void InitializeLegacyAnimation()
    {
        // Fallback to the original ExpressionManager system
        _legacyExpressionManager = new ExpressionManager();
        AddChild(_legacyExpressionManager);

        // If we have a face sprite, center the expressions on it
        if (_faceSprite != null)
        {
            _legacyExpressionManager.Position = _faceSprite.Position;
        }
        
        GD.Print("FaceProjectile: Legacy animation system initialized");
    }

    private void LoadFaceImage()
    {
        string path = PlayerProfile.Instance.FaceImagePath;
        if (string.IsNullOrEmpty(path)) return;

        if (FileAccess.FileExists(path))
        {
            var image = new Image();
            if (image.Load(path) == Error.Ok)
            {
                var texture = ImageTexture.CreateFromImage(image);
                if (_faceSprite != null)
                {
                    _faceSprite.Texture = texture;
                }
                
                // Initialize advanced animation with the loaded face
                if (_useAdvancedAnimation && _deformationMesh != null)
                {
                    _deformationMesh.InitializeFace(texture, _faceRiggerSystem);
                    LoadLandmarksAndCreateRig(texture);
                }
            }
        }
    }

    private void LoadLandmarksAndCreateRig(Texture2D faceTexture)
    {
        string playerName = PlayerProfile.Instance.PlayerName;
        string landmarksPath = $"user://faces/{playerName}_landmarks.json";
        
        if (FileAccess.FileExists(landmarksPath))
        {
            try
            {
                FileAccess file = FileAccess.Open(landmarksPath, FileAccess.ModeFlags.Read);
                if (file != null)
                {
                    string jsonData = file.GetAsText();
                    file.Close();
                    
                    // Parse landmark data
                    var landmarkData = Newtonsoft.Json.JsonConvert.DeserializeObject<LandmarkData>(jsonData);
                    if (landmarkData != null)
                    {
                        // Convert JSON data back to FaceLandmarks
                        var landmarks = ConvertJsonToLandmarks(landmarkData);
                        if (landmarks != null)
                        {
                            // Create rig with loaded landmarks
                            _faceRiggerSystem.CreateRig(landmarks, faceTexture);
                            _hasLoadedFaceRig = true;
                            
                            // Set initial expression
                            _faceAnimationController.SetExpression(ExpressionType.Neutral, 0f);
                            
                            GD.Print($"FaceProjectile: Loaded face rig for {playerName}");
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                GD.PushWarning($"FaceProjectile: Failed to load landmarks: {ex.Message}");
                // Fall back to default landmark detection if available
                CreateFallbackRig(faceTexture);
            }
        }
        else
        {
            // No landmarks file, create a basic rig from scratch
            CreateFallbackRig(faceTexture);
        }
    }

    private void CreateFallbackRig(Texture2D faceTexture)
    {
        // Create basic landmarks if none are available
        var basicLandmarks = CreateBasicLandmarks(faceTexture);
        if (basicLandmarks != null)
        {
            _faceRiggerSystem.CreateRig(basicLandmarks, faceTexture);
            _hasLoadedFaceRig = true;
            _faceAnimationController.SetExpression(ExpressionType.Neutral, 0f);
            
            GD.Print("FaceProjectile: Created basic fallback rig");
        }
    }

    private FaceDetectionManager.FaceLandmarks CreateBasicLandmarks(Texture2D texture)
    {
        if (texture == null) return null;
        
        var landmarks = new FaceDetectionManager.FaceLandmarks
        {
            DetectionTime = System.DateTime.Now
        };
        
        int width = texture.GetWidth();
        int height = texture.GetHeight();
        
        // Create basic landmark positions for a centered face
        landmarks.KeyFeatures = new FaceDetectionManager.KeyLandmarks
        {
            FaceCenter = new Vector2(width * 0.5f, height * 0.5f),
            LeftEye = new Vector2(width * 0.35f, height * 0.4f),
            RightEye = new Vector2(width * 0.65f, height * 0.4f),
            LeftEyebrow = new Vector2(width * 0.35f, height * 0.35f),
            RightEyebrow = new Vector2(width * 0.65f, height * 0.35f),
            LeftMouthCorner = new Vector2(width * 0.4f, height * 0.7f),
            RightMouthCorner = new Vector2(width * 0.6f, height * 0.7f),
            MouthCenter = new Vector2(width * 0.5f, height * 0.75f),
            NoseTip = new Vector2(width * 0.5f, height * 0.55f),
            JawLeft = new Vector2(width * 0.3f, height * 0.6f),
            JawRight = new Vector2(width * 0.7f, height * 0.6f),
            JawBottom = new Vector2(width * 0.5f, height * 0.85f)
        };
        
        // Fill in all landmark points with basic distribution
        for (int i = 0; i < landmarks.AllPoints.Length; i++)
        {
            landmarks.AllPoints[i] = new Vector2(
                Mathf.Lerp(width * 0.2f, width * 0.8f, (float)i / landmarks.AllPoints.Length),
                Mathf.Lerp(height * 0.2f, height * 0.8f, (float)(i % 10) / 10f)
            );
            landmarks.ConfidenceScores[i] = 0.5f;
        }
        
        landmarks.FaceBounds = new Rect2(new Vector2(width * 0.1f, height * 0.1f), new Vector2(width * 0.8f, height * 0.8f));
        landmarks.AverageConfidence = 0.5f;
        
        return landmarks;
    }

    private FaceDetectionManager.FaceLandmarks ConvertJsonToLandmarks(LandmarkData data)
    {
        if (data?.KeyFeatures == null) return null;
        
        var landmarks = new FaceDetectionManager.FaceLandmarks
        {
            DetectionTime = System.DateTime.Parse(data.DetectionTime),
            AverageConfidence = data.AverageConfidence,
            FaceBounds = new Rect2(new Vector2(data.FaceBounds.X, data.FaceBounds.Y), new Vector2(data.FaceBounds.Width, data.FaceBounds.Height))
        };
        
        // Convert key features
        landmarks.KeyFeatures = new FaceDetectionManager.KeyLandmarks
        {
            LeftEye = new Vector2(data.KeyFeatures.LeftEye.X, data.KeyFeatures.LeftEye.Y),
            RightEye = new Vector2(data.KeyFeatures.RightEye.X, data.KeyFeatures.RightEye.Y),
            LeftEyebrow = new Vector2(data.KeyFeatures.LeftEyebrow.X, data.KeyFeatures.LeftEyebrow.Y),
            RightEyebrow = new Vector2(data.KeyFeatures.RightEyebrow.X, data.KeyFeatures.RightEyebrow.Y),
            LeftMouthCorner = new Vector2(data.KeyFeatures.LeftMouthCorner.X, data.KeyFeatures.LeftMouthCorner.Y),
            RightMouthCorner = new Vector2(data.KeyFeatures.RightMouthCorner.X, data.KeyFeatures.RightMouthCorner.Y),
            MouthCenter = new Vector2(data.KeyFeatures.MouthCenter.X, data.KeyFeatures.MouthCenter.Y),
            NoseTip = new Vector2(data.KeyFeatures.NoseTip.X, data.KeyFeatures.NoseTip.Y),
            JawLeft = new Vector2(data.KeyFeatures.JawLeft.X, data.KeyFeatures.JawLeft.Y),
            JawRight = new Vector2(data.KeyFeatures.JawRight.X, data.KeyFeatures.JawRight.Y),
            JawBottom = new Vector2(data.KeyFeatures.JawBottom.X, data.KeyFeatures.JawBottom.Y),
            FaceCenter = new Vector2(data.KeyFeatures.FaceCenter.X, data.KeyFeatures.FaceCenter.Y)
        };
        
        // Copy all landmarks and confidence scores
        if (data.AllLandmarks != null && data.AllLandmarks.Length == landmarks.AllPoints.Length)
        {
            for (int i = 0; i < landmarks.AllPoints.Length; i++)
            {
                landmarks.AllPoints[i] = new Vector2(data.AllLandmarks[i].X, data.AllLandmarks[i].Y);
                landmarks.ConfidenceScores[i] = data.ConfidenceScores[i];
            }
        }
        
        return landmarks;
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        
        if (_useAdvancedAnimation)
        {
            UpdateAdvancedAnimations((float)delta);
        }
        else
        {
            UpdateLegacyAnimations((float)delta);
        }
    }

    private void UpdateAdvancedAnimations(float delta)
    {
        if (_faceAnimationController == null) return;

        float speed = LinearVelocity.Length();
        float acceleration = (LinearVelocity - _lastVelocity).Length() / delta;
        _lastVelocity = LinearVelocity;

        ExpressionType newExpression = _lastExpression;

        // Expression logic (same as before)
        if (speed > 1000f)
        {
            newExpression = ExpressionType.Scared;
        }
        else if (speed < 50f && speed > 5f)
        {
            newExpression = ExpressionType.Bored;
        }
        else if (acceleration > 5000f) // Impact or Launch
        {
            if (speed < 500f)
                newExpression = ExpressionType.Dizzy;
            else
                newExpression = ExpressionType.Determined;
        }

        // Random expressions during flight
        if (speed > 100f && GD.Randf() < 0.01f)
        {
            newExpression = (ExpressionType)GD.RandRange(0, 13);
        }

        // Apply expression change
        if (newExpression != _lastExpression)
        {
            float intensity = CalculateExpressionIntensity(newExpression, speed, acceleration);
            _faceAnimationController.SetExpression(newExpression, intensity);
            
            // Play vocal feedback
            AudioManager.PlayExpressionVocalSfx(newExpression);
            
            // Update speech bubble
            if (SpeechBubbleManager.Instance != null)
            {
                SpeechBubbleManager.Instance.OnExpressionChanged(newExpression);
            }
            
            _lastExpression = newExpression;
        }

        // Handle blinking
        UpdateBlinking(delta);
    }

    private float CalculateExpressionIntensity(ExpressionType expression, float speed, float acceleration)
    {
        switch (expression)
        {
            case ExpressionType.Scared:
                return Mathf.Clamp(speed / 1500f, 0.5f, 1.0f);
            case ExpressionType.Dizzy:
                return Mathf.Clamp(acceleration / 10000f, 0.7f, 1.0f);
            case ExpressionType.Determined:
                return Mathf.Clamp(speed / 800f, 0.3f, 0.8f);
            case ExpressionType.Bored:
                return 0.3f;
            default:
                return 0.6f;
        }
    }

    private void UpdateBlinking(float delta)
    {
        _animationBlinkTimer -= delta;
        if (_animationBlinkTimer <= 0)
        {
            // Trigger a blink
            if (_faceAnimationController != null)
            {
                _faceAnimationController.Blink(0.15f);
            }
            
            // Reset timer with random interval
            _animationBlinkTimer = (float)GD.RandRange(BLINK_INTERVAL_MIN, BLINK_INTERVAL_MAX);
        }
    }

    private void UpdateLegacyAnimations(float delta)
    {
        if (_legacyExpressionManager == null) return;

        float speed = LinearVelocity.Length();
        float acceleration = (LinearVelocity - _lastVelocity).Length() / delta;
        _lastVelocity = LinearVelocity;

        ExpressionType newExpression = _lastExpression;

        // Original expression logic
        if (speed > 1000f)
        {
            newExpression = ExpressionType.Scared;
        }
        else if (speed < 50f && speed > 5f)
        {
            newExpression = ExpressionType.Bored;
        }
        else if (acceleration > 5000f) // Impact or Launch
        {
            if (speed < 500f)
                newExpression = ExpressionType.Dizzy;
            else
                newExpression = ExpressionType.Determined;
        }

        // Random during flight
        if (speed > 100f && GD.Randf() < 0.01f)
        {
            newExpression = (ExpressionType)GD.RandRange(0, 13);
        }

        // Apply expression change and trigger feedback
        if (newExpression != _lastExpression)
        {
            _legacyExpressionManager.SetExpression(newExpression, 1.0f);

            // Play expression vocal
            AudioManager.PlayExpressionVocalSfx(newExpression);

            // Spawn speech bubble on expression change
            if (SpeechBubbleManager.Instance != null)
            {
                SpeechBubbleManager.Instance.OnExpressionChanged(newExpression);
            }

            _lastExpression = newExpression;
        }
    }

    private void OnExpressionChanged(ExpressionType expression, float intensity)
    {
        // Additional feedback when expression changes
        GD.Print($"FaceProjectile: Expression changed to {expression} with intensity {intensity}");
    }

    public override void _ExitTree()
    {
        base._ExitTree();

        // Clear speech bubble tracking
        if (SpeechBubbleManager.Instance != null)
        {
            SpeechBubbleManager.Instance.SetCurrentProjectile(null);
        }
    }

    public void OnImpact()
    {
        if (_faceAnimationController != null)
        {
            _faceAnimationController.SetExpression(ExpressionType.Dizzy, 1.0f, 2.0f);
        }
        else if (_legacyExpressionManager != null)
        {
            _legacyExpressionManager.SetExpression(ExpressionType.Dizzy, 2.0f);
        }
    }

    public void OnSuccess()
    {
        if (_faceAnimationController != null)
        {
            _faceAnimationController.SetExpression(ExpressionType.Happy, 1.0f, 3.0f);
        }
        else if (_legacyExpressionManager != null)
        {
            _legacyExpressionManager.SetExpression(ExpressionType.Happy, 3.0f);
        }
    }
}
