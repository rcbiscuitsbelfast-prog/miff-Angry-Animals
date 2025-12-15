using Godot;

/// <summary>
/// Displays the rage meter during gameplay.
/// Shows visual feedback for player combo/destruction.
/// </summary>
public partial class RageDisplay : Control
{
	[Export] ProgressBar _rageBar;
	[Export] Label _rageLabel;

	public override void _Ready()
	{
		if (_rageBar != null)
		{
			_rageBar.MaxValue = 100;
			_rageBar.Value = 0;
		}

		if (_rageLabel != null)
			_rageLabel.Text = "Rage: 0%";

		if (RageSystem.Instance != null)
			RageSystem.Instance.OnRageUpdated += OnRageUpdated;
	}

	private void OnRageUpdated(float rage)
	{
		if (_rageBar != null)
			_rageBar.Value = rage;

		if (_rageLabel != null)
			_rageLabel.Text = $"Rage: {(int)rage}%";
	}

	public override void _ExitTree()
	{
		if (RageSystem.Instance != null)
			RageSystem.Instance.OnRageUpdated -= OnRageUpdated;
	}
}
