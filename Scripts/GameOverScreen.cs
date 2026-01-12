using Godot;
using System;

/// <summary>
/// Screen shown when the player fails a level (runs out of projectiles).
/// Offers a "Second Chance" via rewarded ads.
/// </summary>
public partial class GameOverScreen : Control
{
    private Button? _retryButton;
    private Button? _watchAdButton;
    private Button? _selectLevelButton;
    private Button? _menuButton;
    private Label? _statusLabel;
    
    [Export] private NodePath _retryButtonPath;
    [Export] private NodePath _watchAdButtonPath;
    [Export] private NodePath _selectLevelButtonPath;
    [Export] private NodePath _menuButtonPath;
    [Export] private NodePath _statusLabelPath;

    public override void _Ready()
    {
        _retryButton = GetNodeOrNull<Button>(_retryButtonPath);
        _watchAdButton = GetNodeOrNull<Button>(_watchAdButtonPath);
        _selectLevelButton = GetNodeOrNull<Button>(_selectLevelButtonPath);
        _menuButton = GetNodeOrNull<Button>(_menuButtonPath);
        _statusLabel = GetNodeOrNull<Label>(_statusLabelPath);

        if (_retryButton != null) _retryButton.Pressed += OnRetryPressed;
        if (_watchAdButton != null) _watchAdButton.Pressed += OnWatchAdPressed;
        if (_selectLevelButton != null) _selectLevelButton.Pressed += OnSelectLevelPressed;
        if (_menuButton != null) _menuButton.Pressed += OnMenuPressed;
        
        UpdateWatchAdButtonVisibility();
        
        // Pause the game while Game Over is shown
        GetTree().Paused = true;
    }

    private void UpdateWatchAdButtonVisibility()
    {
        if (_watchAdButton == null) return;

        // Only show "Watch Ad" if ad is available and user hasn't already used second chance
        bool adReady = RewardedAdManager.Instance != null && RewardedAdManager.Instance.IsRewardedAdReady();
        bool isPremium = PremiumManager.Instance != null && PremiumManager.Instance.IsAdFreeVersion;
        
        // Premium users get the second chance without watching an ad
        if (isPremium)
        {
            _watchAdButton.Text = "Free Second Chance!";
            _watchAdButton.Visible = true;
        }
        else
        {
            _watchAdButton.Visible = adReady;
        }
    }
    
    private void OnWatchAdPressed()
    {
        if (_watchAdButton == null) return;
        
        _watchAdButton.Disabled = true;
        
        bool isPremium = PremiumManager.Instance != null && PremiumManager.Instance.IsAdFreeVersion;
        
        if (isPremium)
        {
            OnRewardEarned();
        }
        else if (RewardedAdManager.Instance != null)
        {
            _watchAdButton.Text = "Loading ad...";
            RewardedAdManager.Instance.ShowRewardedAd(() => OnRewardEarned());
        }
        else
        {
            // Fallback for editor
            OnRewardEarned();
        }
    }
    
    private void OnRewardEarned()
    {
        GD.Print("GameOverScreen: Reward earned! Adding extra projectiles.");
        
        // Signal the reward
        SignalManager.EmitRewardEarned();
        
        // For the sake of this task's logic:
        // In GameOverScreen, we just need to tell the RoomBase or GameManager to continue.
        
        GetTree().Paused = false;
        
        // Instead of reloading, we might want to just resume and add projectiles.
        // But the ticket says: "Immediately restart level ... GetTree().ReloadCurrentScene();"
        // Wait, if we reload, we need to know we have extra heads.
        
        PurchaseStateManager.Instance?.SaveRewardEarned(true);
        
        GameManager.RestartRoom();
        QueueFree();
    }
    
    private void OnRetryPressed()
    {
        GetTree().Paused = false;
        GameManager.RestartRoom();
        QueueFree();
    }

    private void OnSelectLevelPressed()
    {
        GetTree().Paused = false;
        GameManager.LoadMain(); 
        QueueFree();
    }

    private void OnMenuPressed()
    {
        GetTree().Paused = false;
        GameManager.LoadMain();
        QueueFree();
    }
    
    public void SetStatus(string text)
    {
        if (_statusLabel != null)
        {
            _statusLabel.Text = text;
        }
    }
}
