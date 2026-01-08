using Godot;
using System.Collections.Generic;

/// <summary>
/// Manages the connections between ragdoll limbs using PinJoint2D constraints.
/// Acts like muscles and ligaments, connecting limbs while allowing realistic movement.
/// </summary>
public partial class RagdollLimbConnector : Node
{
    [Signal] public delegate void JointCreatedEventHandler(PinJoint2D joint, RagdollLimb limbA, RagdollLimb limbB);
    [Signal] public delegate void JointBrokenEventHandler(RagdollLimb limbA, RagdollLimb limbB);

    [ExportGroup("Joint Settings")]
    [Export] private float _defaultStiffness = 0.5f;
    [Export] private float _defaultDamping = 0.3f;
    [Export] private bool _enableDebugDrawing = false;

    private readonly Dictionary<RagdollLimb, Dictionary<RagdollLimb, PinJoint2D>> _joints = new Dictionary<RagdollLimb, Dictionary<RagdollLimb, PinJoint2D>>();

    public override void _Ready()
    {
        GD.Print("RagdollLimbConnector initialized");
    }

    /// <summary>
    /// Connects two limbs with a PinJoint2D constraint that acts like a muscle/ligament
    /// </summary>
    /// <param name="limbA">First limb to connect</param>
    /// <param name="limbB">Second limb to connect</param>
    /// <param name="stiffness">Joint stiffness (0.1 = loose, 1.0 = very rigid)</param>
    /// <param name="anchorOffsetA">Local position on limb A where joint connects (default: center)</param>
    /// <param name="anchorOffsetB">Local position on limb B where joint connects (default: center)</param>
    /// <returns>The created PinJoint2D node</returns>
    public PinJoint2D ConnectLimbs(RagdollLimb limbA, RagdollLimb limbB, float stiffness = -1f, Vector2? anchorOffsetA = null, Vector2? anchorOffsetB = null)
    {
        if (limbA == null || limbB == null)
        {
            GD.PushError("Cannot connect limbs: one or both limbs are null");
            return null;
        }

        if (limbA == limbB)
        {
            GD.PushError("Cannot connect limb to itself");
            return null;
        }

        // Check if joint already exists
        if (HasJoint(limbA, limbB))
        {
            GD.Print($"Joint already exists between {limbA.Name} and {limbB.Name}");
            return GetJoint(limbA, limbB);
        }

        // Use default stiffness if not specified
        if (stiffness < 0)
        {
            stiffness = _defaultStiffness;
        }

        // Default to center of each limb if no offset specified
        Vector2 defaultOffsetA = anchorOffsetA ?? Vector2.Zero;
        Vector2 defaultOffsetB = anchorOffsetB ?? Vector2.Zero;

        // Create the PinJoint2D
        var joint = new PinJoint2D
        {
            Name = $"Joint_{limbA.Name}_{limbB.Name}",
            NodeA = limbA.GetPath(),
            NodeB = limbB.GetPath(),
            Position = defaultOffsetA,
            // Note: PinJoint2D in Godot 4 uses different property names
            // We'll configure these after creation
        };

        // Add to scene
        AddChild(joint);

        // Configure joint properties
        ConfigureJoint(joint, stiffness, _defaultDamping);

        // Store joint in our tracking dictionary
        if (!_joints.ContainsKey(limbA))
        {
            _joints[limbA] = new Dictionary<RagdollLimb, PinJoint2D>();
        }
        _joints[limbA][limbB] = joint;

        if (!_joints.ContainsKey(limbB))
        {
            _joints[limbB] = new Dictionary<RagdollLimb, PinJoint2D>();
        }
        _joints[limbB][limbA] = joint; // Store bidirectional reference

        // Prevent the connected limbs from colliding with each other
        SetupCollisionException(limbA, limbB);

        GD.Print($"Created joint between {limbA.Name} and {limbB.Name} with stiffness {stiffness}");
        
        EmitSignal(SignalName.JointCreated, joint, limbA, limbB);

        return joint;
    }

    /// <summary>
    /// Disconnects two previously connected limbs
    /// </summary>
    /// <param name="limbA">First limb</param>
    /// <param name="limbB">Second limb</param>
    /// <returns>True if joint was successfully disconnected</returns>
    public bool DisconnectLimbs(RagdollLimb limbA, RagdollLimb limbB)
    {
        var joint = GetJoint(limbA, limbB);
        if (joint == null)
        {
            GD.Print($"No joint found between {limbA?.Name} and {limbB?.Name}");
            return false;
        }

        // Remove from tracking dictionaries
        if (_joints.ContainsKey(limbA) && _joints[limbA].ContainsKey(limbB))
        {
            _joints[limbA].Remove(limbB);
        }
        if (_joints.ContainsKey(limbB) && _joints[limbB].ContainsKey(limbA))
        {
            _joints[limbB].Remove(limbA);
        }

        // Clean up empty dictionaries
        if (_joints.ContainsKey(limbA) && _joints[limbA].Count == 0)
        {
            _joints.Remove(limbA);
        }
        if (_joints.ContainsKey(limbB) && _joints[limbB].Count == 0)
        {
            _joints.Remove(limbB);
        }

        // Queue the joint for deletion
        joint.QueueFree();

        GD.Print($"Disconnected {limbA.Name} from {limbB.Name}");
        EmitSignal(SignalName.JointBroken, limbA, limbB);

        return true;
    }

    /// <summary>
    /// Gets the joint connecting two limbs
    /// </summary>
    /// <param name="limbA">First limb</param>
    /// <param name="limbB">Second limb</param>
    /// <returns>The PinJoint2D connecting the limbs, or null if none exists</returns>
    public PinJoint2D? GetJoint(RagdollLimb limbA, RagdollLimb limbB)
    {
        if (_joints.ContainsKey(limbA) && _joints[limbA].ContainsKey(limbB))
        {
            return _joints[limbA][limbB];
        }
        return null;
    }

    /// <summary>
    /// Checks if two limbs are currently connected by a joint
    /// </summary>
    /// <param name="limbA">First limb</param>
    /// <param name="limbB">Second limb</param>
    /// <returns>True if a joint exists between the limbs</returns>
    public bool HasJoint(RagdollLimb limbA, RagdollLimb limbB)
    {
        return GetJoint(limbA, limbB) != null;
    }

    /// <summary>
    /// Gets all joints connected to a specific limb
    /// </summary>
    /// <param name="limb">The limb to get joints for</param>
    /// <returns>Array of PinJoint2D nodes connected to this limb</returns>
    public PinJoint2D[] GetJointsForLimb(RagdollLimb limb)
    {
        if (!_joints.ContainsKey(limb))
        {
            return new PinJoint2D[0];
        }

        var jointList = new List<PinJoint2D>();
        foreach (var joint in _joints[limb].Values)
        {
            if (IsInstanceValid(joint))
            {
                jointList.Add(joint);
            }
        }
        return jointList.ToArray();
    }

    /// <summary>
    /// Sets the stiffness of an existing joint
    /// </summary>
    /// <param name="limbA">First limb in the joint</param>
    /// <param name="limbB">Second limb in the joint</param>
    /// <param name="stiffness">New stiffness value (0.1 = loose, 1.0 = rigid)</param>
    public void SetJointStiffness(RagdollLimb limbA, RagdollLimb limbB, float stiffness)
    {
        var joint = GetJoint(limbA, limbB);
        if (joint != null)
        {
            ConfigureJoint(joint, stiffness, _defaultDamping);
            GD.Print($"Set joint stiffness between {limbA.Name} and {limbB.Name} to {stiffness}");
        }
    }

    /// <summary>
    /// Breaks all joints connected to a specific limb (useful for extreme damage)
    /// </summary>
    /// <param name="limb">The limb whose joints should be broken</param>
    public void BreakAllJointsForLimb(RagdollLimb limb)
    {
        var joints = GetJointsForLimb(limb);
        foreach (var joint in joints)
        {
            // Find the other limb connected to this joint
            RagdollLimb otherLimb = null;
            if (joint.NodeA == limb.GetPath())
            {
                otherLimb = GetNodeOrNull<RagdollLimb>(joint.NodeB);
            }
            else if (joint.NodeB == limb.GetPath())
            {
                otherLimb = GetNodeOrNull<RagdollLimb>(joint.NodeA);
            }

            if (otherLimb != null)
            {
                DisconnectLimbs(limb, otherLimb);
            }
        }
    }

    /// <summary>
    /// Disconnects all joints in the ragdoll system
    /// </summary>
    public void DisconnectAllJoints()
    {
        var allJoints = new List<PinJoint2D>();
        
        // Collect all joints
        foreach (var limbDict in _joints.Values)
        {
            foreach (var joint in limbDict.Values)
            {
                if (IsInstanceValid(joint) && !allJoints.Contains(joint))
                {
                    allJoints.Add(joint);
                }
            }
        }

        // Disconnect all
        foreach (var joint in allJoints)
        {
            var limbA = GetNodeOrNull<RagdollLimb>(joint.NodeA);
            var limbB = GetNodeOrNull<RagdollLimb>(joint.NodeB);
            
            if (limbA != null && limbB != null)
            {
                DisconnectLimbs(limbA, limbB);
            }
        }

        _joints.Clear();
        GD.Print("All ragdoll joints disconnected");
    }

    /// <summary>
    /// Configures a PinJoint2D with the specified properties
    /// </summary>
    /// <param name="joint">The joint to configure</param>
    /// <param name="stiffness">Joint stiffness</param>
    /// <param name="damping">Joint damping</param>
    private void ConfigureJoint(PinJoint2D joint, float stiffness, float damping)
    {
        // Clamp values to reasonable ranges
        stiffness = Mathf.Clamp(stiffness, 0.1f, 1.0f);
        damping = Mathf.Clamp(damping, 0.0f, 1.0f);

        // PinJoint2D properties in Godot 4
        // These may need adjustment based on the actual Godot version
        joint.ExcludeFromParent = true;
        
        // Set bias to maintain connection strength over time
        joint.Bias = stiffness * 0.5f;
        
        // Note: Actual stiffness/damping properties may vary by Godot version
        // This is a best-effort implementation
        GD.Print($"Configured joint with stiffness {stiffness}, damping {damping}");
    }

    /// <summary>
    /// Sets up collision exception between two limbs so they don't collide with each other
    /// This prevents unrealistic self-collision while still allowing interaction with environment
    /// </summary>
    /// <param name="limbA">First limb</param>
    /// <param name="limbB">Second limb</param>
    private void SetupCollisionException(RagdollLimb limbA, RagdollLimb limbB)
    {
        if (limbA == null || limbB == null) return;

        // Add collision exception so limbs don't collide with each other
        limbA.AddCollisionExceptionWith(limbB);
        limbB.AddCollisionExceptionWith(limbA);

        GD.Print($"Set up collision exception between {limbA.Name} and {limbB.Name}");
    }

    /// <summary>
    /// Removes collision exception between two limbs
    /// </summary>
    /// <param name="limbA">First limb</param>
    /// <param name="limbB">Second limb</param>
    public void RemoveCollisionException(RagdollLimb limbA, RagdollLimb limbB)
    {
        if (limbA == null || limbB == null) return;

        // Remove collision exception
        limbA.RemoveCollisionExceptionWith(limbB);
        limbB.RemoveCollisionExceptionWith(limbA);

        GD.Print($"Removed collision exception between {limbA.Name} and {limbB.Name}");
    }

    public override void _Draw()
    {
        // Draw debug lines showing joint connections
        if (_enableDebugDrawing)
        {
            foreach (var limbDict in _joints.Values)
            {
                foreach (var joint in limbDict.Values)
                {
                    if (IsInstanceValid(joint))
                    {
                        var limbA = GetNodeOrNull<RagdollLimb>(joint.NodeA);
                        var limbB = GetNodeOrNull<RagdollLimb>(joint.NodeB);
                        
                        if (limbA != null && limbB != null)
                        {
                            DrawLine(limbA.GlobalPosition, limbB.GlobalPosition, Colors.Yellow, 2.0f);
                        }
                    }
                }
            }
        }
    }

    public override void _Process(double delta)
    {
        // Redraw debug visualization if enabled
        if (_enableDebugDrawing)
        {
            QueueRedraw();
        }
    }
}