using Godot;

/// <summary>
/// Interface for objects that can be pooled for performance optimization.
/// Objects implementing this interface can be reused instead of destroyed.
/// </summary>
public interface IPoolable
{
	/// <summary>
	/// Resets the object state before it's returned to the pool.
	/// </summary>
	void ResetForPool();

	/// <summary>
	/// Marks the object as ready to be returned to the pool.
	/// </summary>
	void MarkForPooling();
}
