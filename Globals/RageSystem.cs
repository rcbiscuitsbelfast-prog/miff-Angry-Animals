using Godot;

public partial class RageSystem : Node
{
	public static RageSystem Instance { get; private set; } = null!;

	[Signal] public delegate void RageChangedEventHandler(float rage);
	[Signal] public delegate void ComboChangedEventHandler(int combo);
	[Signal] public delegate void RageThresholdReachedEventHandler(int thresholdIndex);

	[Export] public float MaxRage { get; set; } = 100f;
	[Export] public float ComboWindowSeconds { get; set; } = 2.0f;
	[Export] public float RagePerComboHit { get; set; } = 5f;
	[Export] public float[] RageThresholds { get; set; } = [25f, 50f, 75f, 100f];

	public float Rage { get; private set; }
	public int Combo { get; private set; }

	private int _lastThresholdIndex = -1;
	private Timer? _comboTimer;

	public override void _Ready()
	{
		Instance = this;
		ProcessMode = ProcessModeEnum.Always;

		_comboTimer = new Timer
		{
			Name = "ComboTimer",
			OneShot = true,
			WaitTime = ComboWindowSeconds,
			Autostart = false,
			ProcessCallback = Timer.TimerProcessCallback.Idle
		};
		_comboTimer.Timeout += OnComboTimeout;
		AddChild(_comboTimer);
	}

	public static void RegisterComboHit(float? rageGain = null)
	{
		Instance.Combo++;
		Instance.EmitSignal(SignalName.ComboChanged, Instance.Combo);

		Instance.RestartComboTimer();
		AddRage(rageGain ?? Instance.RagePerComboHit);
	}

	public static void AddRage(float amount)
	{
		if (amount == 0)
			return;

		var old = Instance.Rage;
		Instance.Rage = Mathf.Clamp(old + amount, 0f, Instance.MaxRage);
		Instance.EmitSignal(SignalName.RageChanged, Instance.Rage);

		Instance.CheckThresholds(old, Instance.Rage);
	}

	public static void ResetRage()
	{
		Instance.Rage = 0f;
		Instance._lastThresholdIndex = -1;
		Instance.EmitSignal(SignalName.RageChanged, Instance.Rage);
	}

	public static void ResetCombo()
	{
		if (Instance.Combo == 0)
			return;

		Instance.Combo = 0;
		Instance.EmitSignal(SignalName.ComboChanged, Instance.Combo);
	}

	public static void ResetAll()
	{
		ResetCombo();
		ResetRage();
	}

	private void RestartComboTimer()
	{
		if (_comboTimer == null)
			return;

		_comboTimer.WaitTime = ComboWindowSeconds;
		_comboTimer.Stop();
		_comboTimer.Start();
	}

	private void OnComboTimeout() => ResetCombo();

	private void CheckThresholds(float oldRage, float newRage)
	{
		for (int i = 0; i < RageThresholds.Length; i++)
		{
			if (i <= _lastThresholdIndex)
				continue;

			float threshold = RageThresholds[i];
			if (oldRage < threshold && newRage >= threshold)
			{
				_lastThresholdIndex = i;
				EmitSignal(SignalName.RageThresholdReached, i);
			}
		}
	}
}
