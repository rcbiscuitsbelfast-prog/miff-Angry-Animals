using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// Creates a skeletal bone rig from detected facial landmarks.
/// Defines bone relationships and allows for facial expression animations.
/// </summary>
public class FaceRiggerSystem : Node
{
    [Signal] public delegate void RigCompleteEventHandler();
    [Signal] public delegate void RigUpdatedEventHandler();

    public class Bone
    {
        public string Name;
        public Vector2 Position;
        public Vector2 RestPosition;
        public Vector2[] InfluencedVertices;
        public float[] VertexWeights;
        public Bone Parent;
        public List<Bone> Children = new List<Bone>();
        public float Rotation = 0f;
        public Vector2 Scale = Vector2.One;
        public Vector2 RestRotation = Vector2.Zero;
        public Vector2 RestScale = Vector2.One;
        public bool IsVisible = false; // For debugging
    }

    public class BoneRig
    {
        public Dictionary<string, Bone> Bones = new Dictionary<string, Bone>();
        public Vector2[] MeshVertices;
        public int[] MeshIndices;
        public Vector2[] DeformedVertices;
        public Texture2D SourceTexture;
        public Rect2 TextureRect;
        public float[,] VertexWeights; // [vertex_index, bone_index]
        public bool IsValid = false;
    }

    private BoneRig _currentRig;
    private FaceDetectionManager.FaceLandmarks _landmarks;
    private MeshInstance2D _debugMeshInstance;
    private bool _debugMode = false;

    public BoneRig CurrentRig => _currentRig;

    public override void _Ready()
    {
        CreateDebugMesh();
    }

    /// <summary>
    /// Creates a bone rig from detected facial landmarks
    /// </summary>
    public void CreateRig(FaceDetectionManager.FaceLandmarks landmarks, Texture2D sourceTexture)
    {
        _landmarks = landmarks;
        _currentRig = new BoneRig
        {
            SourceTexture = sourceTexture,
            TextureRect = new Rect2(0, 0, sourceTexture.GetWidth(), sourceTexture.GetHeight()),
            MeshVertices = GenerateMeshFromImage(sourceTexture),
            IsValid = false
        };

        CreateBonesFromLandmarks();
        CalculateVertexWeights();
        _currentRig.DeformedVertices = (Vector2[])_currentRig.MeshVertices.Clone();
        _currentRig.IsValid = true;

        EmitSignal(SignalName.RigComplete);
    }

    private void CreateBonesFromLandmarks()
    {
        var features = _landmarks.KeyFeatures;
        var points = _landmarks.AllPoints;

        // Create primary facial bones
        CreatePrimaryBones(features);
        
        // Create secondary bones for finer control
        CreateSecondaryBones(features, points);
        
        // Set up parent-child relationships
        SetupBoneHierarchy();
        
        GD.Print($"FaceRiggerSystem: Created rig with {_currentRig.Bones.Count} bones");
    }

    private void CreatePrimaryBones(FaceDetectionManager.KeyLandmarks features)
    {
        // Left Eye Bone
        CreateBone("LeftEye", features.LeftEye, new string[] { "LeftEyeLid", "LeftEyeSocket" });
        
        // Right Eye Bone
        CreateBone("RightEye", features.RightEye, new string[] { "RightEyeLid", "RightEyeSocket" });
        
        // Left Eyebrow Bone
        CreateBone("LeftEyebrow", features.LeftEyebrow, new string[] { "LeftBrowInner", "LeftBrowOuter" });
        
        // Right Eyebrow Bone
        CreateBone("RightEyebrow", features.RightEyebrow, new string[] { "RightBrowInner", "RightBrowOuter" });
        
        // Mouth Left Corner Bone
        CreateBone("MouthLeftCorner", features.LeftMouthCorner, new string[] { "MouthLeftCorner", "MouthLeftLip" });
        
        // Mouth Right Corner Bone
        CreateBone("MouthRightCorner", features.RightMouthCorner, new string[] { "MouthRightCorner", "MouthRightLip" });
        
        // Mouth Center Bone
        CreateBone("MouthCenter", features.MouthCenter, new string[] { "MouthCenter", "MouthInner" });
        
        // Nose Bone
        CreateBone("Nose", features.NoseTip, new string[] { "NoseTip", "NoseBridge" });
        
        // Jaw Left Bone
        CreateBone("JawLeft", features.JawLeft, new string[] { "JawLeft", "JawLeftCheek" });
        
        // Jaw Right Bone
        CreateBone("JawRight", features.JawRight, new string[] { "JawRight", "JawRightCheek" });
        
        // Jaw Bottom Bone
        CreateBone("JawBottom", features.JawBottom, new string[] { "JawBottom", "JawCenter" });
        
        // Face Center Bone (root)
        CreateBone("FaceCenter", features.FaceCenter, new string[] { "FaceCenter", "Forehead", "Cheeks" });
    }

    private void CreateSecondaryBones(FaceDetectionManager.KeyLandmarks features, Vector2[] points)
    {
        // Create additional bones for more detailed control
        
        // Upper lip bone
        CreateBone("UpperLip", CalculateUpperLipPosition(features), new string[] { "UpperLip", "UpperLipInner" });
        
        // Lower lip bone
        CreateBone("LowerLip", CalculateLowerLipPosition(features), new string[] { "LowerLip", "LowerLipInner" });
        
        // Left cheek bone
        CreateBone("LeftCheek", CalculateLeftCheekPosition(features), new string[] { "LeftCheek", "LeftCheekBone" });
        
        // Right cheek bone
        CreateBone("RightCheek", CalculateRightCheekPosition(features), new string[] { "RightCheek", "RightCheekBone" });
        
        // Forehead bone
        CreateBone("Forehead", CalculateForeheadPosition(features), new string[] { "Forehead", "ForeheadCenter" });
        
        // Chin bone
        CreateBone("Chin", CalculateChinPosition(features), new string[] { "Chin", "ChinCenter" });
        
        // Eye iris bones (for pupil movement)
        CreateBone("LeftIris", CalculateLeftIrisPosition(features), new string[] { "LeftIris", "LeftPupil" });
        CreateBone("RightIris", CalculateRightIrisPosition(features), new string[] { "RightIris", "RightPupil" });
    }

    private void CreateBone(string name, Vector2 position, string[] childNames)
    {
        var bone = new Bone
        {
            Name = name,
            Position = position,
            RestPosition = position,
            Rotation = 0f,
            Scale = Vector2.One
        };

        _currentRig.Bones[name] = bone;

        // Create child bones
        foreach (var childName in childNames)
        {
            var childBone = new Bone
            {
                Name = $"{name}_{childName}",
                Position = position,
                RestPosition = position,
                Parent = bone
            };
            _currentRig.Bones[childBone.Name] = childBone;
            bone.Children.Add(childBone);
        }
    }

    private void SetupBoneHierarchy()
    {
        // Set up specific parent-child relationships based on facial anatomy
        
        // FaceCenter is the root
        if (_currentRig.Bones.TryGetValue("FaceCenter", out var faceCenter))
        {
            SetParent("LeftEye", faceCenter);
            SetParent("RightEye", faceCenter);
            SetParent("LeftEyebrow", faceCenter);
            SetParent("RightEyebrow", faceCenter);
            SetParent("Nose", faceCenter);
            SetParent("Forehead", faceCenter);
        }

        // Jaw hierarchy
        if (_currentRig.Bones.TryGetValue("JawBottom", out var jawBottom))
        {
            SetParent("JawLeft", jawBottom);
            SetParent("JawRight", jawBottom);
            SetParent("Chin", jawBottom);
        }

        // Mouth hierarchy
        if (_currentRig.Bones.TryGetValue("MouthCenter", out var mouthCenter))
        {
            SetParent("MouthLeftCorner", mouthCenter);
            SetParent("MouthRightCorner", mouthCenter);
            SetParent("UpperLip", mouthCenter);
            SetParent("LowerLip", mouthCenter);
        }

        // Eye hierarchy
        if (_currentRig.Bones.TryGetValue("LeftEye", out var leftEye))
        {
            SetParent("LeftIris", leftEye);
        }
        
        if (_currentRig.Bones.TryGetValue("RightEye", out var rightEye))
        {
            SetParent("RightIris", rightEye);
        }
    }

    private void SetParent(string childName, Bone parent)
    {
        if (_currentRig.Bones.TryGetValue(childName, out var child))
        {
            child.Parent = parent;
            if (!parent.Children.Contains(child))
            {
                parent.Children.Add(child);
            }
        }
    }

    private Vector2[] GenerateMeshFromImage(Texture2D texture)
    {
        int width = texture.GetWidth();
        int height = texture.GetHeight();
        int meshDensity = 20; // Higher = more vertices, smoother deformation

        var vertices = new List<Vector2>();
        
        // Create a grid mesh
        for (int y = 0; y <= meshDensity; y++)
        {
            for (int x = 0; x <= meshDensity; x++)
            {
                float u = (float)x / meshDensity;
                float v = (float)y / meshDensity;
                
                Vector2 vertex = new Vector2(u * width, v * height);
                vertices.Add(vertex);
            }
        }

        return vertices.ToArray();
    }

    private void CalculateVertexWeights()
    {
        if (_currentRig.MeshVertices == null || _currentRig.Bones.Count == 0) return;

        _currentRig.VertexWeights = new float[_currentRig.MeshVertices.Length, _currentRig.Bones.Count];
        
        int boneIndex = 0;
        foreach (var kvp in _currentRig.Bones)
        {
            var bone = kvp.Value;
            
            for (int vertexIndex = 0; vertexIndex < _currentRig.MeshVertices.Length; vertexIndex++)
            {
                var vertex = _currentRig.MeshVertices[vertexIndex];
                float distance = vertex.DistanceTo(bone.Position);
                float maxInfluence = 50f; // Maximum distance for influence
                
                if (distance < maxInfluence)
                {
                    float weight = Mathf.Clamp(1f - (distance / maxInfluence), 0f, 1f);
                    // Smooth falloff
                    weight = weight * weight;
                    _currentRig.VertexWeights[vertexIndex, boneIndex] = weight;
                }
            }
            
            boneIndex++;
        }
    }

    /// <summary>
    /// Updates the bone rig based on new positions (for manual adjustment)
    /// </summary>
    public void UpdateBonePosition(string boneName, Vector2 newPosition)
    {
        if (_currentRig?.Bones.TryGetValue(boneName, out var bone) == true)
        {
            bone.Position = newPosition;
            UpdateMeshVertices();
            EmitSignal(SignalName.RigUpdated);
        }
    }

    /// <summary>
    /// Animates a bone with rotation and scale transformations
    /// </summary>
    public void AnimateBone(string boneName, Vector2 rotation, Vector2 scale, float blendWeight = 1f)
    {
        if (_currentRig?.Bones.TryGetValue(boneName, out var bone) == true)
        {
            bone.Rotation = Mathf.Lerp(bone.Rotation, rotation.X, blendWeight);
            bone.Scale = Vector2.Lerp(bone.Scale, scale, blendWeight);
            UpdateMeshVertices();
        }
    }

    /// <summary>
    /// Resets all bones to their rest positions
    /// </summary>
    public void ResetRig()
    {
        foreach (var bone in _currentRig.Bones.Values)
        {
            bone.Position = bone.RestPosition;
            bone.Rotation = 0f;
            bone.Scale = Vector2.One;
        }
        UpdateMeshVertices();
    }

    private void UpdateMeshVertices()
    {
        if (_currentRig?.MeshVertices == null) return;

        // Clear previous deformed vertices
        Array.Clear(_currentRig.DeformedVertices, 0, _currentRig.DeformedVertices.Length);
        
        int boneIndex = 0;
        foreach (var kvp in _currentRig.Bones)
        {
            var bone = kvp.Value;
            
            for (int vertexIndex = 0; vertexIndex < _currentRig.MeshVertices.Length; vertexIndex++)
            {
                float weight = _currentRig.VertexWeights[vertexIndex, boneIndex];
                if (weight > 0.001f) // Small threshold to skip insignificant weights
                {
                    Vector2 originalVertex = _currentRig.MeshVertices[vertexIndex];
                    Vector2 offset = bone.Position - bone.RestPosition;
                    Vector2 rotatedOffset = offset.Rotated(bone.Rotation);
                    Vector2 scaledOffset = rotatedOffset * bone.Scale;
                    
                    _currentRig.DeformedVertices[vertexIndex] += (originalVertex + scaledOffset) * weight;
                }
            }
            
            boneIndex++;
        }

        if (_debugMode)
        {
            UpdateDebugMesh();
        }
    }

    // Helper methods for calculating bone positions
    private Vector2 CalculateUpperLipPosition(FaceDetectionManager.KeyLandmarks features)
    {
        return (features.MouthCenter + features.LeftMouthCorner + features.RightMouthCorner) / 3f + new Vector2(0, -8);
    }

    private Vector2 CalculateLowerLipPosition(FaceDetectionManager.KeyLandmarks features)
    {
        return (features.MouthCenter + features.LeftMouthCorner + features.RightMouthCorner) / 3f + new Vector2(0, 8);
    }

    private Vector2 CalculateLeftCheekPosition(FaceDetectionManager.KeyLandmarks features)
    {
        return (features.LeftEye + features.LeftEyebrow + features.JawLeft) / 3f;
    }

    private Vector2 CalculateRightCheekPosition(FaceDetectionManager.KeyLandmarks features)
    {
        return (features.RightEye + features.RightEyebrow + features.JawRight) / 3f;
    }

    private Vector2 CalculateForeheadPosition(FaceDetectionManager.KeyLandmarks features)
    {
        return features.FaceCenter + new Vector2(0, -30);
    }

    private Vector2 CalculateChinPosition(FaceDetectionManager.KeyLandmarks features)
    {
        return features.JawBottom + new Vector2(0, 15);
    }

    private Vector2 CalculateLeftIrisPosition(FaceDetectionManager.KeyLandmarks features)
    {
        return features.LeftEye;
    }

    private Vector2 CalculateRightIrisPosition(FaceDetectionManager.KeyLandmarks features)
    {
        return features.RightEye;
    }

    // Debug visualization
    private void CreateDebugMesh()
    {
        _debugMeshInstance = new MeshInstance2D();
        AddChild(_debugMeshInstance);
    }

    private void UpdateDebugMesh()
    {
        // TODO: Create debug visualization mesh
        // This would show the bone positions and mesh deformation for debugging
    }

    /// <summary>
    /// Toggles debug mode to visualize bones and mesh
    /// </summary>
    public void SetDebugMode(bool enabled)
    {
        _debugMode = enabled;
        foreach (var bone in _currentRig.Bones.Values)
        {
            bone.IsVisible = enabled;
        }
    }

    /// <summary>
    /// Gets the current deformed mesh vertices for rendering
    /// </summary>
    public Vector2[] GetDeformedVertices()
    {
        return _currentRig?.DeformedVertices;
    }
}