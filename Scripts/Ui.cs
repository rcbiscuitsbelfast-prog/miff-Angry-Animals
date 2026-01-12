using System;
using System.Threading.Tasks;
using Godot;

/// <summary>
/// Handles the user interface of the game.
/// Displays the current level, attempts made by the player, and shows the Game Over panel when the level is completed.
/// </summary>
public partial class Ui : MarginContainer
{
	[Export] Label _levelLabel;
	[Export] Label _attemptLabel;
	[Export] BoxContainer _gameOverVB;

	/// <summary>
	/// Whether to show interstitial ads after repeated game over failures.
	/// </summary>
	[Export] public bool ShowInterstitialOnGameOver { get; set; } = true;

	/// <summary>
	/// Number of failed attempts before showing interstitial ad.
	/// </summary>
	[Export] public int FailedAttemptsBeforeInterstitial { get; set; } = 3;

	private int _consecutiveFailures = 0;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		// Initializes UI labels.
		_gameOverVB.Hide();
		_levelLabel.Text = $"Level: {ScoreManager.GetLevel()}";

		// Connects relevant signals.
		SignalManager.Instance.OnScoreUpdated += OnUpdateAttemptsLabel;
		SignalManager.Instance.OnLevelCompleted += OnLevelFinished;
		
		// Reset failure counter when starting new level
		if (GameManager.Instance != null)
		{
			GameManager.Instance.GameStateChanged += OnGameStateChanged;
		}
	}

    private void OnGameStateChanged(GameManager.GameState state)
    {
        if (state == GameManager.GameState.Playing)
        {
            // Reset consecutive failures when starting a new level
            _consecutiveFailures = 0;
        }
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        // Checks if the Game Over panel is visible and listens for the restar input.
        if (_gameOverVB.Visible && Input.IsActionJustPressed("level_completed"))
        {
			GameManager.LoadMain();
        }
    }

	// Called when the node is removed from the scene tree.
	public override void _ExitTree()
	{
		// Disconects signals to prevent memory leaks.
		SignalManager.Instance.OnScoreUpdated -= OnUpdateAttemptsLabel;
        SignalManager.Instance.OnLevelCompleted -= OnLevelFinished;
		
		if (GameManager.Instance != null)
		{
			GameManager.Instance.GameStateChanged -= OnGameStateChanged;
		}
    }


	/// <summary>
	/// Updates the attempts label whenever the player makes a new attempt.
	/// </summary>
	/// <param name="attempts">The total number of attempts made by the player.</param>
	private void OnUpdateAttemptsLabel(int attempts) => _attemptLabel.Text = $"Attempts: {attempts}";


	/// <summary>
	/// Displays the Game Over panel when the level is completed.
	/// </summary>
    private void OnLevelFinished()
	{
		_gameOverVB.Show();
		
		// Check if we should show interstitial after repeated failures
		_consecutiveFailures++;
		
		if (ShowInterstitialOnGameOver && _consecutiveFailures >= FailedAttemptsBeforeInterstitial)
		{
			_consecutiveFailures = 0; // Reset counter
			_ = ShowInterstitialAfterFailuresAsync();
		}
	}

	private async Task ShowInterstitialAfterFailuresAsync()
	{
		// Check monetization settings
		if (MonetizationManager.Instance?.ShowAds == false)
			return;

		// Check if AdsManager is available
		if (AdsManager.Instance == null)
			return;

		// Wait a moment before showing ad
		await Task.Delay(1000);

		// Check if interstitial is ready
		if (!AdsManager.Instance.IsInterstitialReady())
		{
			GD.Print("Interstitial not ready - preloading for next failure");
			await AdsManager.Instance.LoadInterstitialAd();
			return;
		}

		GD.Print("Showing interstitial after repeated game over failures");
		await AdsManager.Instance.ShowInterstitialAd();
	}
}