using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

/// <summary>
/// UI controller for the Battle Pass screen.
/// Shows seasonal rewards, progress, and purchase options.
/// </summary>
public partial class BattlePassScreenUI : Control
{
    [Header("References")]
    [Export] public BattlePass? BattlePassManager;
    [Export] public Control? MainContainer;
    [Export] public Label? SeasonNameLabel;
    [Export] public Label? SeasonDescriptionLabel;
    [Export] public TextureRect? SeasonBanner;
    [Export] public Label? DaysRemainingLabel;
    [Export] public ProgressBar? TierProgressBar;
    [Export] public Label? TierLabel;
    [Export] public Label? XpLabel;
    [Export] public Control? RewardsScrollContainer;
    [Export] public GridContainer? RewardsGrid;
    [Export] public Button? PurchaseButton;
    [Export] public Button? ClaimRewardButton;
    [Export] public Button? CloseButton;
    [Export] public TextureRect? PremiumBadge;
    [Export] public Label? PremiumCurrencyLabel;
    
    [Header("Tier Display")]
    [Export] public PackedScene? TierRewardNode;
    [Export] public Vector2 TierNodeSize = new Vector2(80, 100);
    [Export] public int TiersPerRow = 6;
    
    [Header("Theme Colors")]
    [Export] public Color FreeTierColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
    [Export] public Color PremiumTierColor = new Color(0.9f, 0.7f, 0.1f, 0.8f);
    [Export] public Color ClaimedColor = new Color(0.3f, 0.8f, 0.3f, 0.5f);
    
    // State
    private List<Control> _tierNodes = new();
    
    public override void _Ready()
    {
        if (BattlePassManager == null)
        {
            BattlePassManager = BattlePass.Instance;
        }
        
        // Connect signals
        CloseButton!.Pressed += OnClosePressed;
        PurchaseButton!.Pressed += OnPurchasePressed;
        ClaimRewardButton!.Pressed += OnClaimRewardPressed;
        
        // Initialize UI
        InitializeUI();
        UpdateUI();
        
        // Show screen
        Show();
    }
    
    /// <summary>
    /// Initialize the UI elements.
    /// </summary>
    private void InitializeUI()
    {
        if (BattlePassManager == null || RewardsGrid == null)
            return;
        
        var totalTiers = BattlePassManager.GetTotalTiers();
        
        // Create tier nodes for the rewards path
        for (int i = 1; i <= totalTiers; i++)
        {
            CreateTierNode(i);
        }
        
        UpdateUI();
    }
    
    /// <summary>
    /// Create a node for displaying a tier.
    /// </summary>
    private void CreateTierNode(int tier)
    {
        if (RewardsGrid == null || TierNodeSize == Vector2.Zero)
            return;
        
        var container = new Control();
        container.CustomMinimumSize = TierNodeSize;
        
        // Background panel
        var panel = new PanelContainer();
        panel.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        
        var style = new StyleBoxFlat();
        style.BgColor = new Color(0.2f, 0.2f, 0.3f, 0.8f);
        style.CornerRadiusTopLeft = 8;
        style.CornerRadiusTopRight = 8;
        style.CornerRadiusBottomLeft = 8;
        style.CornerRadiusBottomRight = 8;
        panel.AddThemeStyleboxOverride("panel", style);
        
        // Vertical layout
        var vbox = new VBoxContainer();
        vbox.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        vbox.MouseFilter = Control.MouseFilterEnum.Stop;
        
        // Tier number
        var tierLabel = new Label();
        tierLabel.Text = $"Tier {tier}";
        tierLabel.HorizontalAlignment = HorizontalAlignment.Center;
        tierLabel.AddThemeFontSizeOverride("font_size", 14);
        
        // Reward icon container
        var rewardContainer = new Control();
        rewardContainer.CustomMinimumSize = new Vector2(50, 50);
        
        var freeIcon = new TextureRect();
        freeIcon.Name = "FreeIcon";
        freeIcon.CustomMinimumSize = new Vector2(40, 40);
        freeIcon.ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize;
        freeIcon.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
        freeIcon.Position = new Vector2(5, 0);
        
        var premiumIcon = new TextureRect();
        premiumIcon.Name = "PremiumIcon";
        premiumIcon.CustomMinimumSize = new Vector2(40, 40);
        premiumIcon.ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize;
        premiumIcon.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
        premiumIcon.Position = new Vector2(35, 0);
        
        rewardContainer.AddChild(freeIcon);
        rewardContainer.AddChild(premiumIcon);
        
        // Premium indicator
        var premiumBadge = new TextureRect();
        premiumBadge.Name = "PremiumBadge";
        premiumBadge.Texture = PremiumBadge?.Texture;
        premiumBadge.Visible = false;
        premiumBadge.CustomMinimumSize = new Vector2(20, 20);
        premiumBadge.Position = new Vector2(55, -5);
        
        // Lock indicator
        var lockIcon = new TextureRect();
        lockIcon.Name = "LockIcon";
        lockIcon.Visible = false;
        lockIcon.CustomMinimumSize = new Vector2(30, 30);
        lockIcon.ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize;
        lockIcon.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
        
        vbox.AddChild(tierLabel);
        vbox.AddChild(rewardContainer);
        vbox.AddChild(premiumBadge);
        vbox.AddChild(lockIcon);
        panel.AddChild(vbox);
        container.AddChild(panel);
        
        RewardsGrid.AddChild(container);
        _tierNodes.Add(container);
        
        // Connect click to show reward details
        container.GuiInput += (inputEvent) => OnTierNodeClicked(inputEvent, tier);
    }
    
    /// <summary>
    /// Handle tier node click.
    /// </summary>
    private void OnTierNodeClicked(InputEvent @event, int tier)
    {
        if (@event is InputEventMouseButton mouseEvent && 
            mouseEvent.Pressed && 
            mouseEvent.ButtonIndex == MouseButton.Left)
        {
            ShowTierDetails(tier);
        }
    }
    
    /// <summary>
    /// Show details for a specific tier.
    /// </summary>
    private void ShowTierDetails(int tier)
    {
        if (BattlePassManager == null)
            return;
        
        var rewards = BattlePassManager.GetTierRewards(tier);
        
        // Create details popup
        var popup = new PopupPanel();
        popup.Title = $"Tier {tier} Rewards";
        popup.Size = new Vector2i(400, 300);
        
        var vbox = new VBoxContainer();
        vbox.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        vbox.Margin = new Margin(10, 10, 10, 10);
        
        // Free reward
        if (rewards.free != null)
        {
            var freeLabel = new Label();
            freeLabel.Text = "Free Reward:";
            freeLabel.Modulate = Colors.LightBlue;
            vbox.AddChild(freeLabel);
            
            var freeReward = new Label();
            freeReward.Text = rewards.free.DisplayName;
            vbox.AddChild(freeReward);
        }
        
        // Premium reward
        if (rewards.premium != null)
        {
            var premiumLabel = new Label();
            premiumLabel.Text = "Premium Reward:";
            premiumLabel.Modulate = Colors.Gold;
            vbox.AddChild(premiumLabel);
            
            var premiumReward = new Label();
            premiumReward.Text = rewards.premium.DisplayName;
            vbox.AddChild(premiumReward);
        }
        
        // XP required
        var xpLabel = new Label();
        xpLabel.Text = $"XP Required: {BattlePassManager.GetRemainingXp()}";
        vbox.AddChild(xpLabel);
        
        popup.AddChild(vbox);
        AddChild(popup);
        popup.PopupCentered();
        
        // Auto-close after 3 seconds
        var timer = new Timer();
        timer.WaitTime = 3;
        timer.OneShot = true;
        timer.Timeout += popup.QueueFree;
        AddChild(timer);
        timer.Start();
    }
    
    /// <summary>
    /// Update all UI elements.
    /// </summary>
    public void UpdateUI()
    {
        if (BattlePassManager == null)
            return;
        
        // Update season info
        if (SeasonNameLabel != null)
            SeasonNameLabel.Text = BattlePassManager.GetSeasonName();
        
        if (SeasonDescriptionLabel != null)
            SeasonDescriptionLabel.Text = BattlePassManager.GetSeasonDescription();
        
        if (DaysRemainingLabel != null)
        {
            var days = BattlePassManager.GetDaysRemaining();
            DaysRemainingLabel.Text = days > 0 ? $"{days} days remaining" : "Season ended";
        }
        
        // Update progress
        var currentTier = BattlePassManager.GetCurrentTier();
        var totalTiers = BattlePassManager.GetTotalTiers();
        
        if (TierLabel != null)
            TierLabel.Text = $"Tier {currentTier}/{totalTiers}";
        
        if (TierProgressBar != null)
        {
            TierProgressBar.MaxValue = totalTiers;
            TierProgressBar.Value = currentTier;
        }
        
        if (XpLabel != null)
        {
            var xp = BattlePassManager.GetTotalXp();
            var remaining = BattlePassManager.GetRemainingXp();
            XpLabel.Text = $"{xp} XP ({remaining} to next tier)";
        }
        
        // Update premium currency
        if (PremiumCurrencyLabel != null)
        {
            var currency = BattlePassManager.GetPremiumCurrencyBalance();
            PremiumCurrencyLabel.Text = $"{currency} Premium";
        }
        
        // Update battle pass ownership
        var hasPass = BattlePassManager.HasBattlePass();
        if (PurchaseButton != null)
            PurchaseButton.Visible = !hasPass;
        
        // Update premium badge
        if (PremiumBadge != null)
            PremiumBadge.Visible = hasPass;
        
        // Update tier nodes
        UpdateTierNodes();
        
        // Update claim button
        UpdateClaimButton();
    }
    
    /// <summary>
    /// Update the visual state of tier nodes.
    /// </summary>
    private void UpdateTierNodes()
    {
        if (BattlePassManager == null)
            return;
        
        var currentTier = BattlePassManager.GetCurrentTier();
        var hasPass = BattlePassManager.HasBattlePass();
        
        for (int i = 0; i < _tierNodes.Count; i++)
        {
            var node = _tierNodes[i];
            var tier = i + 1;
            
            var panel = node.GetChild<PanelContainer>(0);
            var style = panel?.GetThemeStylebox("panel") as StyleBoxFlat;
            
            if (style == null)
            {
                style = new StyleBoxFlat();
                panel?.AddThemeStyleboxOverride("panel", style);
            }
            
            // Determine color based on tier state
            if (tier <= currentTier)
            {
                // Completed tier
                style.BgColor = ClaimedColor;
                style.BorderColor = Colors.Green;
                style.BorderWidthLeft = 2;
                style.BorderWidthTop = 2;
                style.BorderWidthRight = 2;
                style.BorderWidthBottom = 2;
            }
            else if (tier == currentTier + 1)
            {
                // Current tier
                style.BgColor = new Color(0.3f, 0.5f, 0.8f, 0.8f);
                style.BorderColor = Colors.Blue;
                style.BorderWidthLeft = 3;
                style.BorderWidthTop = 3;
                style.BorderWidthRight = 3;
                style.BorderWidthBottom = 3;
            }
            else if (tier <= currentTier + 5)
            {
                // Upcoming tier
                style.BgColor = new Color(0.3f, 0.3f, 0.4f, 0.6f);
                style.BorderColor = Colors.Gray;
                style.BorderWidthLeft = 1;
                style.BorderWidthTop = 1;
                style.BorderWidthRight = 1;
                style.BorderWidthBottom = 1;
            }
            else
            {
                // Future tier
                style.BgColor = new Color(0.2f, 0.2f, 0.3f, 0.4f);
                style.BorderColor = Colors.DarkGray;
                style.BorderWidthLeft = 1;
                style.BorderWidthTop = 1;
                style.BorderWidthRight = 1;
                style.BorderWidthBottom = 1;
            }
            
            // Update icons
            var rewards = BattlePassManager.GetTierRewards(tier);
            UpdateTierIcons(node, rewards, hasPass);
        }
    }
    
    /// <summary>
    /// Update icons on a tier node.
    /// </summary>
    private void UpdateTierIcons(Control node, (BattlePassReward? free, BattlePassReward? premium) rewards, bool hasPass)
    {
        var panel = node.GetChild<PanelContainer>(0);
        var vbox = panel?.GetChild<VBoxContainer>(0);
        
        var rewardContainer = vbox?.GetChild<Control>(1);
        var freeIcon = rewardContainer?.GetNode<TextureRect>("FreeIcon");
        var premiumIcon = rewardContainer?.GetNode<TextureRect>("PremiumIcon");
        var premiumBadge = vbox?.GetNode<TextureRect>("PremiumBadge");
        var lockIcon = vbox?.GetNode<TextureRect>("LockIcon");
        
        // Free reward
        if (freeIcon != null)
        {
            freeIcon.Visible = rewards.free != null;
            if (rewards.free != null && !string.IsNullOrEmpty(rewards.free.IconPath))
            {
                if (ResourceLoader.Exists(rewards.free.IconPath))
                {
                    freeIcon.Texture = ResourceLoader.Load<Texture2D>(rewards.free.IconPath);
                }
            }
        }
        
        // Premium reward
        if (premiumIcon != null)
        {
            premiumIcon.Visible = rewards.premium != null && hasPass;
            if (rewards.premium != null && hasPass && !string.IsNullOrEmpty(rewards.premium.IconPath))
            {
                if (ResourceLoader.Exists(rewards.premium.IconPath))
                {
                    premiumIcon.Texture = ResourceLoader.Load<Texture2D>(rewards.premium.IconPath);
                }
            }
        }
        
        // Premium badge (shows lock status)
        if (premiumBadge != null)
        {
            premiumBadge.Visible = rewards.premium != null && !hasPass;
        }
        
        // Lock icon
        if (lockIcon != null)
        {
            lockIcon.Visible = rewards.premium != null && !hasPass;
        }
    }
    
    /// <summary>
    /// Update the claim reward button state.
    /// </summary>
    private void UpdateClaimButton()
    {
        if (BattlePassManager == null || ClaimRewardButton == null)
            return;
        
        var availableRewards = BattlePassManager.GetAvailableRewards();
        
        if (availableRewards.Count > 0)
        {
            ClaimRewardButton.Visible = true;
            ClaimRewardButton.Text = $"Claim {availableRewards.Count} Reward(s)";
            ClaimRewardButton.Disabled = false;
        }
        else
        {
            ClaimRewardButton.Visible = false;
        }
    }
    
    /// <summary>
    /// Handle purchase button click.
    /// </summary>
    private async void OnPurchasePressed()
    {
        if (BattlePassManager == null)
            return;
        
        var price = BattlePassManager.GetBattlePassPrice();
        
        // Show confirmation dialog
        var dialog = new ConfirmationDialog();
        dialog.Title = "Purchase Battle Pass";
        dialog.DialogText = $"Purchase the Battle Pass for ${price:F2}? You'll unlock all premium rewards!";
        dialog.Size = new Vector2i(400, 200);
        
        AddChild(dialog);
        dialog.PopupCentered();
        
        var tcs = new TaskCompletionSource<bool>();
        dialog.Confirmed += () => tcs.SetResult(true);
        dialog.Canceled += () => tcs.SetResult(false);
        
        var confirmed = await tcs.Task;
        dialog.QueueFree();
        
        if (confirmed)
        {
            var success = await BattlePassManager.PurchaseBattlePass();
            if (success)
            {
                ShowMessage("Battle Pass purchased!");
                UpdateUI();
            }
            else
            {
                ShowMessage("Purchase failed. Please try again.");
            }
        }
    }
    
    /// <summary>
    /// Handle claim reward button click.
    /// </summary>
    private void OnClaimRewardPressed()
    {
        if (BattlePassManager == null)
            return;
        
        var availableRewards = BattlePassManager.GetAvailableRewards();
        
        foreach (var reward in availableRewards)
        {
            BattlePassManager.ClaimReward(reward.tier, reward.isPremium);
        }
        
        ShowMessage($"Claimed {availableRewards.Count} reward(s)!");
        UpdateUI();
    }
    
    /// <summary>
    /// Handle close button click.
    /// </summary>
    private void OnClosePressed()
    {
        QueueFree();
    }
    
    /// <summary>
    /// Show a message to the player.
    /// </summary>
    private void ShowMessage(string message)
    {
        var notification = new Label();
        notification.Text = message;
        notification.HorizontalAlignment = HorizontalAlignment.Center;
        notification.AddThemeFontSizeOverride("font_size", 24);
        notification.Modulate = Colors.White;
        
        notification.Position = (Size / 2) - (notification.GetMinimumSize() / 2);
        notification.AnchorLeft = 0.5f;
        notification.AnchorRight = 0.5f;
        
        AddChild(notification);
        
        // Animate out
        var tween = CreateTween();
        tween.TweenProperty(notification, "modulate:a", 0f, 1.5f);
        tween.TweenCallback(notification.QueueFree);
    }
    
    /// <summary>
    /// Show the battle pass screen.
    /// </summary>
    public void Show()
    {
        Visible = true;
        UpdateUI();
    }
    
    /// <summary>
    /// Hide the battle pass screen.
    /// </summary>
    public void Hide()
    {
        Visible = false;
    }
}

/// <summary>
/// Simple popup panel for showing details.
/// </summary>
public partial class PopupPanel : Window
{
    public PopupPanel()
    {
        Title = "Popup";
        Size = new Vector2i(300, 200);
        Exclusive = false;
    }
}
