using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// Handles mesh deformation and texture rendering for the animated face.
/// Converts bone transformations into visible facial deformations.
/// </summary>
public class FaceDeformationMesh : MeshInstance2D
{
    private FaceRiggerSystem _riggerSystem;
    private ArrayMesh _deformationMesh;
    private MeshDataTool _meshDataTool;
    private ShaderMaterial _deformationShader;
    
    // Texture and UV mapping
    private Texture2D _faceTexture;
    private Vector2[] _originalUVs;
    private Vector2[] _deformedUVs;
    
    // Deformation settings
    private float _deformationStrength = 1.0f;
    private bool _useGPUDeformation = false;
    
    // Performance optimization
    private bool _meshDirty = false;
    private float _lastUpdateTime = 0f;
    private const float UPDATE_INTERVAL = 1f / 60f; // 60 FPS max update rate

    public override void _Ready()
    {
        InitializeShader();
        InitializeMeshData();
    }

    public override void _Process(double delta)
    {
        if (_meshDirty && Time.GetUnixTimeFromSystem() - _lastUpdateTime > UPDATE_INTERVAL)
        {
            UpdateMeshDeformation();
            _lastUpdateTime = Time.GetUnixTimeFromSystem();
        }
    }

    /// <summary>
    /// Sets up the deformation system with the face texture and rig
    /// </summary>
    public void InitializeFace(Texture2D faceTexture, FaceRiggerSystem riggerSystem)
    {
        _faceTexture = faceTexture;
        _riggerSystem = riggerSystem;

        if (_faceTexture != null)
        {
            Texture = _faceTexture;
            SetupMeshFromTexture();
            ConnectRigSignals();
        }
    }

    private void InitializeShader()
    {
        // Create a custom shader for texture deformation
        var shader = new Shader();
        shader.Code = @"
        shader_type canvas_item;
        render_mode unshaded;

        uniform sampler2D deformation_map;
        uniform vec2 deformation_strength = vec2(1.0, 1.0);
        uniform bool use_gpu_deformation = false;

        void fragment() {
            vec4 color = texture(TEXTURE, UV);
            
            if (use_gpu_deformation) {
                vec2 deformation = texture(deformation_map, UV).rg;
                deformation = (deformation - 0.5) * 2.0 * deformation_strength;
                color = texture(TEXTURE, UV + deformation * 0.1);
            }
            
            COLOR = color;
        }
        ";

        _deformationShader = new ShaderMaterial();
        _deformationShader.Shader = shader;
        Material = _deformationShader;
    }

    private void InitializeMeshData()
    {
        _meshDataTool = new MeshDataTool();
        _deformationMesh = new ArrayMesh();
    }

    private void SetupMeshFromTexture()
    {
        if (_faceTexture == null) return;

        int width = _faceTexture.GetWidth();
        int height = _faceTexture.GetHeight();

        // Create a high-resolution mesh for smooth deformation
        int meshResolution = 32; // Higher = smoother but more performance cost
        
        var vertices = new List<Vector3>();
        var normals = new List<Vector3>();
        var uvs = new List<Vector2>();
        var indices = new List<int>();

        // Generate grid mesh
        for (int y = 0; y <= meshResolution; y++)
        {
            for (int x = 0; x <= meshResolution; x++)
            {
                float u = (float)x / meshResolution;
                float v = (float)y / meshResolution;
                
                Vector3 vertex = new Vector3(u * width, v * height, 0);
                vertices.Add(vertex);
                
                normals.Add(Vector3.Forward);
                uvs.Add(new Vector2(u, v));
            }
        }

        // Generate indices for triangle mesh
        for (int y = 0; y < meshResolution; y++)
        {
            for (int x = 0; x < meshResolution; x++)
            {
                int i0 = y * (meshResolution + 1) + x;
                int i1 = i0 + 1;
                int i2 = i0 + (meshResolution + 1);
                int i3 = i2 + 1;

                // First triangle
                indices.Add(i0);
                indices.Add(i2);
                indices.Add(i1);

                // Second triangle
                indices.Add(i1);
                indices.Add(i2);
                indices.Add(i3);
            }
        }

        // Create arrays for mesh data
        var arrays = new Godot.Collections.Array();
        arrays.Resize((int)Mesh.ArrayType.Max);
        arrays[(int)Mesh.ArrayType.Vertex] = vertices.ToArray();
        arrays[(int)Mesh.ArrayType.Normal] = normals.ToArray();
        arrays[(int)Mesh.ArrayType.TexUv] = uvs.ToArray();
        arrays[(int)Mesh.ArrayType.Index] = indices.ToArray();

        // Create the mesh
        _deformationMesh.AddSurfaceFromArrays(Mesh.Priority, arrays);
        
        // Store original UVs for deformation calculations
        _originalUVs = uvs.ToArray();
        _deformedUVs = new Vector2[_originalUVs.Length];
        
        // Apply mesh to this MeshInstance2D
        Mesh = _deformationMesh;
        
        // Prepare mesh data tool for editing
        _meshDataTool.CreateFromSurface(_deformationMesh, 0);
    }

    private void ConnectRigSignals()
    {
        if (_riggerSystem != null)
        {
            _riggerSystem.Connect(FaceRiggerSystem.SignalName.RigUpdated, new Callable(this, nameof(OnRigUpdated)));
        }
    }

    private void OnRigUpdated()
    {
        _meshDirty = true;
    }

    /// <summary>
    /// Updates the mesh deformation based on bone positions
    /// </summary>
    private void UpdateMeshDeformation()
    {
        if (_riggerSystem?.CurrentRig == null || _meshDataTool == null) return;

        var deformedVertices = _riggerSystem.GetDeformedVertices();
        if (deformedVertices == null) return;

        // Update mesh vertices based on deformed positions
        for (int i = 0; i < _meshDataTool.GetVertexCount() && i < deformedVertices.Length; i++)
        {
            var originalVertex = _meshDataTool.GetVertex(i);
            var deformedPosition = deformedVertices[i];
            
            // Convert world positions to local mesh coordinates
            var localPosition = toLocal(deformedPosition);
            _meshDataTool.SetVertex(i, new Vector3(localPosition.X, localPosition.Y, originalVertex.Z));
        }

        // Recalculate normals for proper lighting (though we're unshaded)
        for (int i = 0; i < _meshDataTool.GetVertexCount(); i++)
        {
            _meshDataTool.SetNormal(i, Vector3.Forward);
        }

        // Clear the mesh and re-add the updated surface
        _deformationMesh.ClearSurfaces();
        _meshDataTool.CommitToSurface(_deformationMesh);
        Mesh = _deformationMesh;

        _meshDirty = false;
    }

    /// <summary>
    /// Alternative GPU-based deformation using shader
    /// </summary>
    private void UpdateGPUDeformation()
    {
        if (_faceTexture == null || _riggerSystem?.CurrentRig == null) return;

        // Create a deformation map texture from bone positions
        var deformationMap = CreateDeformationMap();
        
        if (_deformationShader != null)
        {
            _deformationShader.SetShaderParameter("deformation_map", deformationMap);
            _deformationShader.SetShaderParameter("deformation_strength", new Vector2(_deformationStrength, _deformationStrength));
            _deformationShader.SetShaderParameter("use_gpu_deformation", true);
        }
    }

    private Texture2D CreateDeformationMap()
    {
        // Create a texture that stores deformation vectors for each UV coordinate
        int width = 256;
        int height = 256;
        var image = Image.Create(width, height, false, Image.Format.Rgba8);
        
        var deformedVertices = _riggerSystem.GetDeformedVertices();
        if (deformedVertices == null) return _faceTexture;

        // Calculate deformation for each pixel
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float u = (float)x / (width - 1);
                float v = (float)y / (height - 1);
                
                // Find the closest mesh vertex
                Vector2 uv = new Vector2(u, v);
                Vector2 deformation = CalculateVertexDeformation(uv, deformedVertices);
                
                // Store deformation in RG channels
                Color pixel = new Color(
                    (deformation.X + 1f) * 0.5f, // Map from [-1,1] to [0,1]
                    (deformation.Y + 1f) * 0.5f,
                    0f, 1f
                );
                
                image.SetPixel(x, y, pixel);
            }
        }
        
        return ImageTexture.CreateFromImage(image);
    }

    private Vector2 CalculateVertexDeformation(Vector2 uv, Vector2[] deformedVertices)
    {
        // Find the closest vertex in the mesh
        float minDistance = float.MaxValue;
        Vector2 closestDeformation = Vector2.Zero;

        // This is a simplified approach - in practice, you'd want to interpolate
        // between nearby vertices for smoother deformation
        for (int i = 0; i < _originalUVs.Length && i < deformedVertices.Length; i++)
        {
            float distance = uv.DistanceTo(_originalUVs[i]);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestDeformation = deformedVertices[i] - _originalUVs[i];
            }
        }

        return closestDeformation;
    }

    /// <summary>
    /// Sets the deformation strength multiplier
    /// </summary>
    public void SetDeformationStrength(float strength)
    {
        _deformationStrength = Mathf.Clamp(strength, 0f, 2f);
        
        if (_deformationShader != null)
        {
            _deformationShader.SetShaderParameter("deformation_strength", new Vector2(_deformationStrength, _deformationStrength));
        }
    }

    /// <summary>
    /// Toggles between CPU and GPU deformation methods
    /// </summary>
    public void SetUseGPUDeformation(bool useGPU)
    {
        _useGPUDeformation = useGPU;
        
        if (_deformationShader != null)
        {
            _deformationShader.SetShaderParameter("use_gpu_deformation", useGPU);
        }
    }

    /// <summary>
    /// Resets the mesh to its original undeformed state
    /// </summary>
    public void ResetDeformation()
    {
        if (_meshDataTool == null) return;

        for (int i = 0; i < _meshDataTool.GetVertexCount(); i++)
        {
            var originalVertex = _meshDataTool.GetVertex(i);
            _meshDataTool.SetVertex(i, new Vector3(originalVertex.X, originalVertex.Y, 0));
        }

        _deformationMesh.ClearSurfaces();
        _meshDataTool.CommitToSurface(_deformationMesh);
        Mesh = _deformationMesh;
        
        _meshDirty = false;
    }

    /// <summary>
    /// Gets the current deformation strength
    /// </summary>
    public float DeformationStrength => _deformationStrength;
    
    /// <summary>
    /// Gets whether GPU deformation is enabled
    /// </summary>
    public bool UseGPUDeformation => _useGPUDeformation;
    
    /// <summary>
    /// Gets the mesh resolution for performance monitoring
    /// </summary>
    public int MeshResolution => _meshDataTool?.GetVertexCount() ?? 0;
}