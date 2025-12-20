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
    /// Triggered when a destructible prop is damaged.
    /// </summary>
    [Signal] public delegate void OnPropDamagedEventHandler(Node prop, int damage);

    /// <summary>
    /// Triggered when a destructible prop is destroyed.
    /// </summary>
    [Signal] public delegate void OnPropDestroyedEventHandler(Node prop, int scoreValue);

    /// <summary>
    /// Triggered when the destruction score is updated.
    /// </summary>
    [Signal] public delegate void OnDestructionScoreUpdatedEventHandler(int score);

	/// <summary>
	/// Triggered when a room is completed in the traversal phase.
	/// </summary>
	[Signal] public delegate void OnRoomCompletedEventHandler();

	/// <summary>
	/// Triggered when the traversal phase starts (after slingshot phase).
	/// </summary>
	[Signal] public delegate void OnTraversalStartedEventHandler();

	/// <summary>
	/// Triggered when a projectile is launched in the slingshot phase.
	/// </summary>
	[Signal] public delegate void OnProjectileLaunchedEventHandler();


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

    public static void EmitOnPropDamaged(Node prop, int damage) => Instance.EmitSignal(SignalName.OnPropDamaged, prop, damage);
    public static void EmitOnPropDestroyed(Node prop, int scoreValue) => Instance.EmitSignal(SignalName.OnPropDestroyed, prop, scoreValue);
    public static void EmitOnDestructionScoreUpdated(int score) => Instance.EmitSignal(SignalName.OnDestructionScoreUpdated, score);

	/// <summary>
	/// Emits the OnRoomCompleted signal.
	/// </summary>
	public static void EmitOnRoomCompleted() => Instance.EmitSignal(SignalName.OnRoomCompleted);

	/// <summary>
	/// Emits the OnTraversalStarted signal.
	/// </summary>
	public static void EmitOnTraversalStarted() => Instance.EmitSignal(SignalName.OnTraversalStarted);

	/// <summary>
	/// Emits the OnProjectileLaunched signal.
	/// </summary>
	public static void EmitOnProjectileLaunched() => Instance.EmitSignal(SignalName.OnProjectileLaunched);
}