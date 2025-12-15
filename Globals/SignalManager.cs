using Godot;

/// <summary>
/// Global manager for handling Signals (Events).
/// Provides a centralized way to emit and handle events, avoiding repetitive signal connections across multiple scripts.
/// </summary>
public partial class SignalManager : Node
{
	public static SignalManager Instance { get; private set; } // Singleton for global access as an AutoLoad.


    /// <summary>
    /// Triggered when an animal dies in the game.
    /// </summary>
    [Signal] public delegate void OnAnimalDiedEventHandler();

    /// <summary>
    /// Triggered when a cup is destroyed in the game.
    /// </summary>
    [Signal] public delegate void OnCupDestroyedEventHandler();
    
    /// <summary>
    /// Triggered when the player completes a level.
    /// </summary>
    [Signal] public delegate void OnLevelCompletedEventHandler();
    
    /// <summary>
	/// Triggered when the player makes an attempt (like shooting or failing).
	/// </summary>
	[Signal] public delegate void OnAttemptMadeEventHandler();
	
    /// <summary>
	/// Triggered when the score is updated.
	/// The score value is passed as a parameter.
	/// </summary>
	/// <param name="score">The new score value.</param>
	[Signal] public delegate void OnScoreUpdatedEventHandler(int score);

	/// <summary>
	/// Triggered when the game is paused.
	/// </summary>
	[Signal] public delegate void OnPausedEventHandler();

	/// <summary>
	/// Triggered when the game is resumed.
	/// </summary>
	[Signal] public delegate void OnResumedEventHandler();



    // Called when the node enters the scene tree for the first time.
    public override void _Ready() => Instance = this;



    /// <summary>
    /// Emits the OnAnimalDied.
    /// </summary>
    public static void EmitOnAnimalDied() => Instance.EmitSignal(SignalName.OnAnimalDied);

    /// <summary>
    /// Emits the OnCupDestroyed.
    /// </summary>
	public static void EmitOnCupDestroyed() => Instance.EmitSignal(SignalName.OnCupDestroyed);

    /// <summary>
    /// Emits the OnLevelCompleted signal.
    /// </summary>
	public static void EmitOnLevelCompleted() => Instance.EmitSignal(SignalName.OnLevelCompleted);

    /// <summary>
    /// Emits the OnAttemptMade signal.
    /// </summary>
    public static void EmitOnAttemptMade() => Instance.EmitSignal(SignalName.OnAttemptMade);

    /// <summary>
    /// Emits the OnScoreUpdated signal with the current score.
    /// </summary>
    /// <param name="score">The updated score to send with the signa.</param>
    public static void EmitOnScoreUpdated(int score) => Instance.EmitSignal(SignalName.OnScoreUpdated, score);

	/// <summary>
	/// Emits the OnPaused signal.
	/// </summary>
	public static void EmitOnPaused() => Instance.EmitSignal(SignalName.OnPaused);

	/// <summary>
	/// Emits the OnResumed signal.
	/// </summary>
	public static void EmitOnResumed() => Instance.EmitSignal(SignalName.OnResumed);
}
