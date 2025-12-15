using Godot;

/// <summary>
/// Invisible input area for detecting drag interactions on the slingshot.
/// Communicates with the parent Slingshot node.
/// </summary>
public partial class InputArea : Area2D
{
	[Signal] public delegate void DragStartedEventHandler();
	[Signal] public delegate void DragEndedEventHandler();
	
	private bool _isDragging = false;
	
	public override void _Ready()
	{
		InputEvent += OnInputEvent;
	}
	
	public override void _Process(double delta)
	{
		if (_isDragging && Input.IsActionJustReleased("drag"))
		{
			_isDragging = false;
			EmitSignal(SignalName.DragEnded);
		}
	}
	
	private void OnInputEvent(Node viewport, InputEvent @event, long shapeIdx)
	{
		if (@event.IsActionPressed("drag"))
		{
			_isDragging = true;
			EmitSignal(SignalName.DragStarted);
		}
	}
}
