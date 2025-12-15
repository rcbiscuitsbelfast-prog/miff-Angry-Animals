using Godot;

/// <summary>
/// Global manager for rage/combo system.
/// Handles rage meter and combo tracking during gameplay.
/// </summary>
public partial class RageSystem : Node
{
	public static RageSystem Instance { get; private set; }

	[Signal] public delegate void OnRageUpdatedEventHandler(float rage);
	[Signal] public delegate void OnComboUpdatedEventHandler(int combo);

	private float _rage = 0f;
	private int _combo = 0;
	private const float MAX_RAGE = 100f;

	public override void _Ready() => Instance = this;

	public static float GetRage() => Instance._rage;

	public static void SetRage(float rage)
	{
		Instance._rage = Mathf.Clamp(rage, 0, Instance.MAX_RAGE);
		Instance.EmitSignal(SignalName.OnRageUpdated, Instance._rage);
	}

	public static void AddRage(float amount) => SetRage(Instance._rage + amount);

	public static int GetCombo() => Instance._combo;

	public static void SetCombo(int combo)
	{
		Instance._combo = combo;
		Instance.EmitSignal(SignalName.OnComboUpdated, Instance._combo);
	}

	public static void AddCombo() => SetCombo(Instance._combo + 1);

	public static void ResetRageAndCombo()
	{
		Instance._rage = 0f;
		Instance._combo = 0;
		Instance.EmitSignal(SignalName.OnRageUpdated, 0f);
		Instance.EmitSignal(SignalName.OnComboUpdated, 0);
	}
}
