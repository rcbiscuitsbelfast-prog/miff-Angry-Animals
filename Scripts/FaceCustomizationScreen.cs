using Godot;
using System;
using System.Threading.Tasks;

public partial class FaceCustomizationScreen : Control
{
    [Signal] public delegate void OnCloseEventHandler();

    private TextureRect _cameraPreview;
    private TextureRect _facePreview; // The captured/selected face
    private Control _previewContainer; // Holds face + cosmetics
    
    // Cosmetic Overlays
    private TextureRect _hatOverlay;
    private TextureRect _glassesOverlay;
    private TextureRect _emotionOverlay; // Or filter

    private Button _captureButton;
    private Button _galleryButton;
    private Button _saveButton;
    private Button _cancelButton;
    private Button _retakeButton;
    private Button _adjustButton;
    private Button _confirmButton;
    private Label _statusLabel;
    private TabContainer _cosmeticsTabs;

    // Face Detection & Rigging Components
    private FaceDetectionManager _faceDetectionManager;
    private FaceLandmarkVisualizer _landmarkVisualizer;
    private Control _detectionConfirmationPanel;

    private Image _capturedImage;
    private bool _isCameraActive = false;

    // Current selection
    private int _selectedHatIndex;
    private int _selectedGlassesIndex;
    private int _selectedEmotionIndex;

    // Face detection state
    private bool _isDetecting = false;
    private bool _hasDetectedLandmarks = false;
    private FaceDetectionManager.FaceLandmarks _detectedLandmarks;

    public override void _Ready()
    {
        _selectedHatIndex = PlayerProfile.Instance.SelectedHatIndex;
        _selectedGlassesIndex = PlayerProfile.Instance.SelectedGlassesIndex;
        _selectedEmotionIndex = PlayerProfile.Instance.SelectedEmotionIndex;

        AnchorRight = 1;
        AnchorBottom = 1;
        
        var background = new ColorRect();
        background.Color = new Color(0.1f, 0.1f, 0.1f, 1.0f);
        background.SetAnchorsPreset(LayoutPreset.FullRect);
        AddChild(background);

        var mainHBox = new HBoxContainer();
        mainHBox.SetAnchorsPreset(LayoutPreset.FullRect);
        mainHBox.Alignment = BoxContainer.AlignmentMode.Center;
        AddChild(mainHBox);

        // LEFT SIDE: Preview & Camera
        var leftVBox = new VBoxContainer();
        leftVBox.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        leftVBox.CustomMinimumSize = new Vector2(400, 0);
        leftVBox.Alignment = BoxContainer.AlignmentMode.Center;
        mainHBox.AddChild(leftVBox);

        _statusLabel = new Label();
        _statusLabel.Text = "Face Customization";
        _statusLabel.HorizontalAlignment = HorizontalAlignment.Center;
        leftVBox.AddChild(_statusLabel);

        // Preview Container
        var frameContainer = new AspectRatioContainer();
        frameContainer.CustomMinimumSize = new Vector2(300, 300);
        frameContainer.StretchMode = AspectRatioContainer.StretchModeEnum.Keep;
        leftVBox.AddChild(frameContainer);

        _previewContainer = new Control(); // Holds layers
        frameContainer.AddChild(_previewContainer);

        // Layer 0: Camera Feed (hidden when image captured)
        _cameraPreview = new TextureRect();
        _cameraPreview.ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize;
        _cameraPreview.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
        _cameraPreview.SetAnchorsPreset(LayoutPreset.FullRect);
        _previewContainer.AddChild(_cameraPreview);

        // Layer 1: Captured/Selected Face
        _facePreview = new TextureRect();
        _facePreview.ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize;
        _facePreview.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
        _facePreview.SetAnchorsPreset(LayoutPreset.FullRect);
        _facePreview.Visible = false;
        _previewContainer.AddChild(_facePreview);

        // Layer 2: Emotion Overlay
        _emotionOverlay = new TextureRect();
        _emotionOverlay.ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize;
        _emotionOverlay.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
        _emotionOverlay.SetAnchorsPreset(LayoutPreset.FullRect);
        _previewContainer.AddChild(_emotionOverlay);

        // Layer 3: Glasses Overlay
        _glassesOverlay = new TextureRect();
        _glassesOverlay.ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize;
        _glassesOverlay.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
        _glassesOverlay.SetAnchorsPreset(LayoutPreset.FullRect);
        _previewContainer.AddChild(_glassesOverlay);

        // Layer 4: Hat Overlay
        _hatOverlay = new TextureRect();
        _hatOverlay.ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize;
        _hatOverlay.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
        _hatOverlay.SetAnchorsPreset(LayoutPreset.FullRect);
        _hatOverlay.Position = new Vector2(0, -50); // Offset hat slightly up
        _previewContainer.AddChild(_hatOverlay);


        // Camera Controls
        var camControlsBox = new HBoxContainer();
        camControlsBox.Alignment = BoxContainer.AlignmentMode.Center;
        leftVBox.AddChild(camControlsBox);

        _captureButton = new Button();
        _captureButton.Text = "Take Photo";
        _captureButton.Pressed += OnCaptureButtonPressed;
        camControlsBox.AddChild(_captureButton);

        _galleryButton = new Button();
        _galleryButton.Text = "Gallery";
        _galleryButton.Pressed += OnGalleryButtonPressed;
        camControlsBox.AddChild(_galleryButton);

        _retakeButton = new Button();
        _retakeButton.Text = "Retake";
        _retakeButton.Visible = false;
        _retakeButton.Pressed += OnRetakeButtonPressed;
        camControlsBox.AddChild(_retakeButton);

        _adjustButton = new Button();
        _adjustButton.Text = "Adjust Detection";
        _adjustButton.Visible = false;
        _adjustButton.Pressed += OnAdjustButtonPressed;
        camControlsBox.AddChild(_adjustButton);

        _confirmButton = new Button();
        _confirmButton.Text = "Confirm";
        _confirmButton.Visible = false;
        _confirmButton.Pressed += OnConfirmButtonPressed;
        camControlsBox.AddChild(_confirmButton);


        // RIGHT SIDE: Cosmetics
        var rightVBox = new VBoxContainer();
        rightVBox.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        rightVBox.CustomMinimumSize = new Vector2(300, 0);
        mainHBox.AddChild(rightVBox);

        _cosmeticsTabs = new TabContainer();
        _cosmeticsTabs.SizeFlagsVertical = SizeFlags.ExpandFill;
        rightVBox.AddChild(_cosmeticsTabs);

        AddCosmeticTab("Hats", PlayerProfile.GetHats(), (idx) => { _selectedHatIndex = idx; UpdatePreview(); });
        AddCosmeticTab("Glasses", PlayerProfile.GetGlasses(), (idx) => { _selectedGlassesIndex = idx; UpdatePreview(); });
        AddCosmeticTab("Emotions", PlayerProfile.GetEmotions(), (idx) => { _selectedEmotionIndex = idx; UpdatePreview(); });

        // Bottom Controls
        var bottomBox = new HBoxContainer();
        bottomBox.Alignment = BoxContainer.AlignmentMode.End;
        rightVBox.AddChild(bottomBox);

        _saveButton = new Button();
        _saveButton.Text = "Save & Close";
        _saveButton.Pressed += OnSaveButtonPressed;
        bottomBox.AddChild(_saveButton);

        _cancelButton = new Button();
        _cancelButton.Text = "Cancel";
        _cancelButton.Pressed += OnCancelButtonPressed;
        bottomBox.AddChild(_cancelButton);
        
        // Initialize face detection system
        InitializeFaceDetectionSystem();

        // Initial setup
        StartCamera();
        
        // If there is already a saved face, load it
        if (!string.IsNullOrEmpty(PlayerProfile.Instance.FaceImagePath))
        {
             LoadExistingFace(PlayerProfile.Instance.FaceImagePath);
        }
        
        UpdatePreview();
    }
    
    private void AddCosmeticTab(string title, string[] items, Action<int> onSelect)
    {
        var list = new ItemList();
        list.Name = title;
        list.FixedIconSize = new Vector2i(64, 64);
        list.MaxColumns = 3;
        list.SameColumnWidth = true;
        list.IconMode = ItemList.IconModeEnum.Top;
        
        for (int i = 0; i < items.Length; i++)
        {
             list.AddItem(items[i]);
             // TODO: Add icons if available
        }
        
        list.ItemSelected += (long index) => onSelect((int)index);
        _cosmeticsTabs.AddChild(list);
    }

    private void StartCamera()
    {
        // Request permissions for Mobile
        if (OS.GetName() == "Android" || OS.GetName() == "iOS")
        {
             OS.RequestPermissions();
        }

        var feeds = CameraServer.Feeds();
        if (feeds.Count > 0)
        {
            var feed = feeds[0];
            _isCameraActive = true;
            _statusLabel.Text = "Camera Active";
            
            var cameraTexture = new CameraTexture();
            cameraTexture.CameraFeedId = feed.Id;
            cameraTexture.CameraIsActive = true;
            _cameraPreview.Texture = cameraTexture;
        }
        else
        {
            _statusLabel.Text = "No Camera Found";
            _cameraPreview.Texture = GetPlaceholderImage();
        }
    }

    private Texture2D GetPlaceholderImage()
    {
        var image = Image.Create(256, 256, false, Image.Format.Rgba8);
        image.Fill(Colors.Gray);
        return ImageTexture.CreateFromImage(image);
    }

    private void InitializeFaceDetectionSystem()
    {
        // Initialize face detection manager
        _faceDetectionManager = new FaceDetectionManager();
        AddChild(_faceDetectionManager);
        
        // Connect signals
        _faceDetectionManager.Connect(FaceDetectionManager.SignalName.DetectionComplete, new Callable(this, nameof(OnFaceDetectionComplete)));
        _faceDetectionManager.Connect(FaceDetectionManager.SignalName.DetectionFailed, new Callable(this, nameof(OnFaceDetectionFailed)));
        
        // Initialize landmark visualizer
        _landmarkVisualizer = new FaceLandmarkVisualizer();
        _landmarkVisualizer.IsEditMode = false;
        _landmarkVisualizer.Connect(FaceLandmarkVisualizer.SignalName.LandmarkAdjusted, new Callable(this, nameof(OnLandmarkAdjusted)));
        _landmarkVisualizer.Connect(FaceLandmarkVisualizer.SignalName.DetectionConfirmed, new Callable(this, nameof(OnDetectionConfirmed)));
        
        // Add to preview container (above face image)
        _previewContainer.AddChild(_landmarkVisualizer);
        _landmarkVisualizer.Visible = false;
    }

    private void OnCaptureButtonPressed()
    {
        Image? captured = null;

        try
        {
            if (_cameraPreview.Texture != null)
                captured = _cameraPreview.Texture.GetImage();
        }
        catch (Exception ex)
        {
            GD.PushWarning($"FaceCustomizationScreen: failed to capture image: {ex.Message}");
        }

        _capturedImage = captured ?? GetPlaceholderImage().GetImage();

        if (_capturedImage != null)
            ProcessCapturedImage(_capturedImage);
    }

    private void OnGalleryButtonPressed()
    {
        var fileDialog = new FileDialog();
        fileDialog.FileMode = FileDialog.FileModeEnum.OpenFile;
        fileDialog.Access = FileDialog.AccessEnum.Filesystem;
        fileDialog.Filters = new string[] { "*.png", "*.jpg", "*.jpeg" };
        fileDialog.FileSelected += OnFileSelected;
        fileDialog.MinSize = new Vector2(400, 300);
        fileDialog.Visible = true;
        AddChild(fileDialog);
        fileDialog.PopupCentered();
    }

    private void OnFileSelected(string path)
    {
        var image = new Image();
        if (image.Load(path) == Error.Ok)
        {
            ProcessCapturedImage(image);
        }
    }
    
    private void LoadExistingFace(string path)
    {
        var image = new Image();
        if (image.Load(path) == Error.Ok)
        {
            ProcessCapturedImage(image);
        }
    }

    private void ProcessCapturedImage(Image image)
    {
        _capturedImage = image;
        _capturedImage.Resize(256, 256);
        
        var texture = ImageTexture.CreateFromImage(_capturedImage);
        _facePreview.Texture = texture;
        _facePreview.Visible = true;
        _cameraPreview.Visible = false;
        
        _captureButton.Visible = false;
        _galleryButton.Visible = false;
        _retakeButton.Visible = true;
        
        // Start face detection
        StartFaceDetection();
    }

    private void StartFaceDetection()
    {
        if (_faceDetectionManager == null || _capturedImage == null) return;
        
        _isDetecting = true;
        _statusLabel.Text = "Detecting facial landmarks...";
        
        // Hide UI buttons during detection
        _adjustButton.Visible = false;
        _confirmButton.Visible = false;
        
        _faceDetectionManager.DetectLandmarksAsync(_capturedImage);
    }

    private void OnFaceDetectionComplete(FaceDetectionManager.FaceLandmarks landmarks, bool success)
    {
        _isDetecting = false;
        _hasDetectedLandmarks = true;
        _detectedLandmarks = landmarks;
        
        if (success)
        {
            _statusLabel.Text = "Face detected! Review and confirm landmarks.";
            ShowDetectionConfirmationUI();
        }
        else
        {
            _statusLabel.Text = "Detection complete (using fallback method). Review landmarks.";
            ShowDetectionConfirmationUI();
        }
    }

    private void OnFaceDetectionFailed(string error)
    {
        _isDetecting = false;
        _statusLabel.Text = $"Detection failed: {error}";
        
        // Show fallback options
        _adjustButton.Visible = false;
        _confirmButton.Visible = true;
        _confirmButton.Text = "Continue without landmarks";
    }

    private void ShowDetectionConfirmationUI()
    {
        // Update landmark visualizer
        if (_landmarkVisualizer != null && _detectedLandmarks != null)
        {
            _landmarkVisualizer.Landmarks = _detectedLandmarks;
            _landmarkVisualizer.SetFaceImage(_facePreview.Texture);
            _landmarkVisualizer.Visible = true;
        }
        
        // Show appropriate buttons
        _adjustButton.Visible = true;
        _confirmButton.Visible = true;
    }

    private void OnAdjustButtonPressed()
    {
        if (_landmarkVisualizer == null) return;
        
        bool isEditMode = _landmarkVisualizer.IsEditMode;
        _landmarkVisualizer.IsEditMode = !isEditMode;
        
        _adjustButton.Text = _landmarkVisualizer.IsEditMode ? "Exit Edit Mode" : "Adjust Detection";
        
        if (_landmarkVisualizer.IsEditMode)
        {
            _statusLabel.Text = "Drag landmark circles to adjust positions";
        }
        else
        {
            _statusLabel.Text = "Review detected landmarks";
        }
    }

    private void OnConfirmButtonPressed()
    {
        if (_hasDetectedLandmarks && _detectedLandmarks != null)
        {
            // Save face and landmarks
            SaveFaceWithLandmarks();
            _statusLabel.Text = "Face saved with landmark data!";
        }
        else
        {
            // Save without landmarks (fallback)
            _statusLabel.Text = "Face saved without landmark data.";
        }
        
        // Hide landmark visualization
        if (_landmarkVisualizer != null)
        {
            _landmarkVisualizer.Visible = false;
        }
        
        _adjustButton.Visible = false;
        _confirmButton.Visible = false;
    }

    private void OnLandmarkAdjusted(string landmarkName, Vector2 newPosition)
    {
        // Update the landmark position and rig system
        if (_faceDetectionManager != null && _detectedLandmarks != null)
        {
            // Update the specific landmark in detectedLandmarks
            UpdateLandmarkPosition(landmarkName, newPosition);
        }
    }

    private void OnDetectionConfirmed()
    {
        OnConfirmButtonPressed();
    }

    private void UpdateLandmarkPosition(string landmarkName, Vector2 newPosition)
    {
        if (_detectedLandmarks == null) return;
        
        // Update the specific landmark position
        switch (landmarkName)
        {
            case "LeftEye":
                _detectedLandmarks.KeyFeatures.LeftEye = newPosition;
                break;
            case "RightEye":
                _detectedLandmarks.KeyFeatures.RightEye = newPosition;
                break;
            case "LeftEyebrow":
                _detectedLandmarks.KeyFeatures.LeftEyebrow = newPosition;
                break;
            case "RightEyebrow":
                _detectedLandmarks.KeyFeatures.RightEyebrow = newPosition;
                break;
            case "LeftMouthCorner":
                _detectedLandmarks.KeyFeatures.LeftMouthCorner = newPosition;
                break;
            case "RightMouthCorner":
                _detectedLandmarks.KeyFeatures.RightMouthCorner = newPosition;
                break;
            case "MouthCenter":
                _detectedLandmarks.KeyFeatures.MouthCenter = newPosition;
                break;
            case "NoseTip":
                _detectedLandmarks.KeyFeatures.NoseTip = newPosition;
                break;
            case "JawLeft":
                _detectedLandmarks.KeyFeatures.JawLeft = newPosition;
                break;
            case "JawRight":
                _detectedLandmarks.KeyFeatures.JawRight = newPosition;
                break;
            case "JawBottom":
                _detectedLandmarks.KeyFeatures.JawBottom = newPosition;
                break;
            case "FaceCenter":
                _detectedLandmarks.KeyFeatures.FaceCenter = newPosition;
                break;
        }
    }

    private void SaveFaceWithLandmarks()
    {
        // Save the face image
        if (_capturedImage != null)
        {
            var dir = DirAccess.Open("user://");
            if (!dir.DirExists("faces"))
            {
                dir.MakeDir("faces");
            }

            string playerName = PlayerProfile.Instance.PlayerName;
            string fileName = $"faces/{playerName}_face.png";
            string fullPath = "user://" + fileName;
            
            _capturedImage.SavePng(fullPath);
            PlayerProfile.SetFaceImage(fullPath);
        }

        // Save landmark data
        if (_detectedLandmarks != null)
        {
            SaveLandmarkData(_detectedLandmarks);
        }

        // Save Cosmetics
        PlayerProfile.SetCosmetics(_selectedHatIndex, _selectedGlassesIndex, moustacheIndex: 0, wigIndex: 0, filterIndex: 0, emotionIndex: _selectedEmotionIndex);
        
        GD.Print($"Saved face with landmarks for player: {PlayerProfile.Instance.PlayerName}");
    }

    private void SaveLandmarkData(FaceDetectionManager.FaceLandmarks landmarks)
    {
        // Save landmark data as JSON for later use in animation
        var landmarkData = new
        {
            DetectionTime = landmarks.DetectionTime.ToString("yyyy-MM-dd HH:mm:ss"),
            AverageConfidence = landmarks.AverageConfidence,
            FaceBounds = new
            {
                X = landmarks.FaceBounds.Position.X,
                Y = landmarks.FaceBounds.Position.Y,
                Width = landmarks.FaceBounds.Size.X,
                Height = landmarks.FaceBounds.Size.Y
            },
            KeyFeatures = new
            {
                LeftEye = new { X = landmarks.KeyFeatures.LeftEye.X, Y = landmarks.KeyFeatures.LeftEye.Y },
                RightEye = new { X = landmarks.KeyFeatures.RightEye.X, Y = landmarks.KeyFeatures.RightEye.Y },
                LeftEyebrow = new { X = landmarks.KeyFeatures.LeftEyebrow.X, Y = landmarks.KeyFeatures.LeftEyebrow.Y },
                RightEyebrow = new { X = landmarks.KeyFeatures.RightEyebrow.X, Y = landmarks.KeyFeatures.RightEyebrow.Y },
                LeftMouthCorner = new { X = landmarks.KeyFeatures.LeftMouthCorner.X, Y = landmarks.KeyFeatures.LeftMouthCorner.Y },
                RightMouthCorner = new { X = landmarks.KeyFeatures.RightMouthCorner.X, Y = landmarks.KeyFeatures.RightMouthCorner.Y },
                MouthCenter = new { X = landmarks.KeyFeatures.MouthCenter.X, Y = landmarks.KeyFeatures.MouthCenter.Y },
                NoseTip = new { X = landmarks.KeyFeatures.NoseTip.X, Y = landmarks.KeyFeatures.NoseTip.Y },
                JawLeft = new { X = landmarks.KeyFeatures.JawLeft.X, Y = landmarks.KeyFeatures.JawLeft.Y },
                JawRight = new { X = landmarks.KeyFeatures.JawRight.X, Y = landmarks.KeyFeatures.JawRight.Y },
                JawBottom = new { X = landmarks.KeyFeatures.JawBottom.X, Y = landmarks.KeyFeatures.JawBottom.Y },
                FaceCenter = new { X = landmarks.KeyFeatures.FaceCenter.X, Y = landmarks.KeyFeatures.FaceCenter.Y }
            },
            AllLandmarks = landmarks.AllPoints,
            ConfidenceScores = landmarks.ConfidenceScores
        };

        string json = Newtonsoft.Json.JsonConvert.SerializeObject(landmarkData, Newtonsoft.Json.Formatting.Indented);
        string playerName = PlayerProfile.Instance.PlayerName;
        string landmarksPath = $"user://faces/{playerName}_landmarks.json";
        
        FileAccess file = FileAccess.Open(landmarksPath, FileAccess.ModeFlags.Write);
        if (file != null)
        {
            file.StoreString(json);
            file.Close();
        }
    }

    private void OnRetakeButtonPressed()
    {
        _capturedImage = null;
        _facePreview.Visible = false;
        _cameraPreview.Visible = true;
        
        _captureButton.Visible = true;
        _galleryButton.Visible = true;
        _retakeButton.Visible = false;
        
        // Reset face detection state
        _hasDetectedLandmarks = false;
        _detectedLandmarks = null;
        _isDetecting = false;
        
        // Hide landmark visualizer
        if (_landmarkVisualizer != null)
        {
            _landmarkVisualizer.Visible = false;
        }
        
        // Hide detection buttons
        _adjustButton.Visible = false;
        _confirmButton.Visible = false;
        
        // Reset status
        _statusLabel.Text = "Camera Active";
    }
    
    private void UpdatePreview()
    {
        // Update overlay textures based on selection
        // In a real app, these would be loaded from resources
        
        // Hat
        string hatName = PlayerProfile.GetHats()[_selectedHatIndex];
        // _hatOverlay.Texture = ResourceLoader.Load<Texture2D>($"res://Assets/Hats/{hatName}.png");
        
        // Glasses
        string glassesName = PlayerProfile.GetGlasses()[_selectedGlassesIndex];
        // _glassesOverlay.Texture = ResourceLoader.Load<Texture2D>($"res://Assets/Glasses/{glassesName}.png");
        
        // Emotion - if we have a custom face, emotion might be skipped or overlaid
        string emotionName = PlayerProfile.GetEmotions()[_selectedEmotionIndex];
        // _emotionOverlay.Texture = ResourceLoader.Load<Texture2D>($"res://Assets/Face/face_{emotionName}.png");
    }

    private void OnSaveButtonPressed()
    {
        // Check if we have landmarks to save
        if (_hasDetectedLandmarks && _detectedLandmarks != null)
        {
            SaveFaceWithLandmarks();
        }
        else
        {
            // Save without landmarks (original method)
            SaveFaceWithoutLandmarks();
        }
        
        GD.Print($"Saved: Hat={_selectedHatIndex}, Glasses={_selectedGlassesIndex}, Face saved.");
        EmitSignal(SignalName.OnClose);
        QueueFree();
    }

    private void SaveFaceWithoutLandmarks()
    {
        // Original save method for backward compatibility
        if (_capturedImage != null)
        {
            var dir = DirAccess.Open("user://");
            if (!dir.DirExists("faces"))
            {
                dir.MakeDir("faces");
            }

            string playerName = PlayerProfile.Instance.PlayerName;
            string fileName = $"faces/{playerName}_face.png";
            string fullPath = "user://" + fileName;
            
            _capturedImage.SavePng(fullPath);
            PlayerProfile.SetFaceImage(fullPath);
        }

        // Save Cosmetics
        PlayerProfile.SetCosmetics(_selectedHatIndex, _selectedGlassesIndex, moustacheIndex: 0, wigIndex: 0, filterIndex: 0, emotionIndex: _selectedEmotionIndex);
    }

    private void OnCancelButtonPressed()
    {
        EmitSignal(SignalName.OnClose);
        QueueFree();
    }
}
