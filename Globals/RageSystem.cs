using Godot;

/// <summary>
/// Global rage and combo system.
/// Manages the player's rage level and combo multiplier based on gameplay events.
/// </summary>
public partial class RageSystem : Node
{
    /// <summary>
    /// Singleton instance of the RageSystem.
    /// </summary>
    public static RageSystem Instance { get; private set; } = null!;

    /// <summary>
    /// Emitted when the rage level changes.
    /// </summary>
    /// <param name="rage">The new rage level.</param>
    [Signal] public delegate void RageChangedEventHandler(float rage);

    /// <summary>
    /// Emitted when the combo multiplier changes.
    /// </summary>
    /// <param name="combo">The new combo multiplier.</param>
    [Signal] public delegate void ComboChangedEventHandler(int combo);

    /// <summary>
    /// Emitted when a rage threshold is reached.
    /// </summary>
    /// <param name="thresholdIndex">The index of the reached threshold.</param>
    [Signal] public delegate void RageThresholdReachedEventHandler(int thresholdIndex);

    /// <summary>
    /// The maximum rage level.
    /// </summary>
    [Export] public float MaxRage { get; set; } = 100f;

    /// <summary>
    /// The duration of the combo window in seconds.
    /// </summary>
    [Export] public float ComboWindowSeconds { get; set; } = 2.0f;

    /// <summary>
    /// The amount of rage gained per combo hit.
    /// </summary>
    [Export] public float RagePerComboHit { get; set; } = 5f;

    /// <summary>
    /// The rage thresholds that trigger signals.
    /// </summary>
    [Export] public float[] RageThresholds { get; set; } = [25f, 50f, 75f, 100f];

    /// <summary>
    /// The current rage level.
    /// </summary>
    public float Rage { get; private set; }

    /// <summary>
    /// The current combo multiplier.
    /// </summary>
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

    /// <summary>
    /// Registers a hit and updates the combo and rage levels.
    /// </summary>
    /// <param name="rageGain">Optional specific rage gain amount.</param>
    public static void RegisterComboHit(float? rageGain = null)
    {
        Instance.Combo++;
        Instance.EmitSignal(SignalName.ComboChanged, Instance.Combo);

        Instance.RestartComboTimer();
        AddRage(rageGain ?? Instance.RagePerComboHit);
    }

    /// <summary>
    /// Adds a specified amount of rage.
    /// </summary>
    /// <param name="amount">The amount of rage to add.</param>
    public static void AddRage(float amount)
    {
        if (amount == 0)
            return;

        var old = Instance.Rage;
        Instance.Rage = Mathf.Clamp(old + amount, 0f, Instance.MaxRage);
        Instance.EmitSignal(SignalName.RageChanged, Instance.Rage);

        Instance.CheckThresholds(old, Instance.Rage);
    }

    /// <summary>
    /// Resets the rage level to zero.
    /// </summary>
    public static void ResetRage()
    {
        Instance.Rage = 0f;
        Instance._lastThresholdIndex = -1;
        Instance.EmitSignal(SignalName.RageChanged, Instance.Rage);
    }

    /// <summary>
    /// Resets the combo multiplier to zero.
    /// </summary>
    public static void ResetCombo()
    {
        if (Instance.Combo == 0)
            return;

        Instance.Combo = 0;
        Instance.EmitSignal(SignalName.ComboChanged, Instance.Combo);
    }

    /// <summary>
    /// Resets both combo and rage levels.
    /// </summary>
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
