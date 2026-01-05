using Godot;
using System;

/// <summary>
/// Generic object pooling system for performance optimization.
/// Reduces instantiation overhead by reusing objects instead of creating/destroying them.
/// Ported from Angry Aliens Node2DPool.gd
/// </summary>
public partial class ObjectPool : Node
{
	[Export] public PackedScene ObjectScene { get; set; }
	[Export] public int PoolSize { get; set; } = 5;
	[Export] public float RefreshTimer { get; set; } = 1.0f;

	private Node _inactiveContainer;
	private Godot.Collections.Array<Node> _activeObjects = new();

	public override void _Ready()
	{
		_inactiveContainer = new Node() { Name = "InactiveNodes" };
		AddChild(_inactiveContainer);

		// Populate pool
		for (int i = 0; i < PoolSize; i++)
		{
			var obj = _CreateObject();
			_inactiveContainer.AddChild(obj);
		}

		// Schedule pool refresh
		var timer = new Timer() { WaitTime = RefreshTimer, OneShot = false };
		AddChild(timer);
		timer.Start();
		timer.Timeout += CheckUnusedObjects;
	}

	/// <summary>
	/// Returns an object to the pool for reuse.
	/// </summary>
	public void Pool(Node obj)
	{
		if (obj == null) return;

		var parent = obj.GetParent();
		if (parent != null)
		{
			parent.RemoveChild(obj);
		}
		_activeObjects.Remove(obj);

		// Reset object state
		obj.Modulate = Colors.White;

		if (obj is RigidBody2D rb)
		{
			rb.LinearVelocity = Vector2.Zero;
			rb.AngularVelocity = 0f;
			rb.Rotation = 0f;
			rb.Freeze = true;
		}

		_inactiveContainer.AddChild(obj);
	}

	/// <summary>
	/// Gets an object from the pool, creating a new one if pool is empty.
	/// </summary>
	public Node GetInstance()
	{
		Node obj;

		if (_inactiveContainer.GetChildCount() > 0)
		{
			obj = _inactiveContainer.GetChild(0);
			_inactiveContainer.RemoveChild(obj);
		}
		else
		{
			GD.Print("ObjectPool: Pool empty. Creating new object.");
			obj = _CreateObject();
		}

		obj.Modulate = Colors.White;
		_activeObjects.Add(obj);

		// Wake up rigid bodies
		if (obj is RigidBody2D rb)
		{
			rb.Freeze = false;
		}

		return obj;
	}

	/// <summary>
	/// Checks for objects marked as reusable and returns them to pool.
	/// </summary>
	private void CheckUnusedObjects()
	{
		for (int i = _activeObjects.Count - 1; i >= 0; i--)
		{
			var obj = _activeObjects[i];
			if (obj.HasMeta("can_be_pooled") && (bool)obj.GetMeta("can_be_pooled"))
			{
				Pool(obj);
			}
		}
	}

	private Node _CreateObject()
	{
		if (ObjectScene == null)
		{
			GD.PrintErr("ObjectPool: No ObjectScene set!");
			return null;
		}

		var obj = ObjectScene.Instantiate();
		obj.SetMeta("can_be_pooled", false);
		return obj;
	}

	public override void _ExitTree()
	{
		if (IsInGroup("pools"))
		{
			foreach (var obj in _inactiveContainer.GetChildren())
			{
				obj.QueueFree();
			}
			foreach (var obj in _activeObjects)
			{
				obj.QueueFree();
			}
		}
	}
}
