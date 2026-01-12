using System;
using Godot;
using Godot.Collections;

/// <summary>
/// Login bonus screen UI controller
/// Displays daily reward, streak progress, and celebration effects
/// </summary>
public partial class LoginBonusScreenUI : Control
{
    [Signal] public delegate void ScreenDismissedEventHandler();
    [Signal] public delegate void ViewSeasonalEventsEventHandler();

    [Export] private NodePath _mainContainer;
    [Export] private NodePath _rewardPreviewContainer;
    [Export] private NodePath _streakCounterLabel;
    [Export] private NodePath _rewardTitleLabel;
    [Export] private NodePath _rewardDescriptionLabel;
    [Export] private NodePath _nextRewardPreviewLabel;
    [Export] private NodePath _streakCalendarContainer;
    [Export] private NodePath _dismissButton;
    [Export] private NodePath _viewEventsButton;
    [Export] private NodePath _celebrationEffects;
    [Export] private NodePath _progressFill;

    private Control _mainContainerNode;
    private Control _rewardPreviewContainerNode;
    private Label _streakCounterLabelNode;
    private Label _rewardTitleLabelNode;
    private Label _rewardDescriptionLabelNode;
    private Label _nextRewardPreviewLabelNode;
    private Control _streakCalendarContainerNode;
    private Button _dismissButtonNode;
    private Button _viewEventsButtonNode;
    private Control _celebrationEffectsNode;
    private ProgressBar _progressFillNode;

    private StreakManager _streakManager;
    private SeasonalEventManager _eventManager;
    private Timer _autoDismissTimer;

    public override void _Ready()
    {
        InitializeUI();
        ConnectSignals();
        LoadStreakData();
        SetupAutoDismiss();
        ShowCelebrationIfNeeded();
    }

    /// <summary>
    /// Initialize UI references
    /// </summary>
    private void InitializeUI()
    {
        _mainContainerNode = GetNode<Control>(_mainContainer);
        _rewardPreviewContainerNode = GetNode<Control>(_rewardPreviewContainer);
        _streakCounterLabelNode = GetNode<Label>(_streakCounterLabel);
        _rewardTitleLabelNode = GetNode<Label>(_rewardTitleLabel);
        _rewardDescriptionLabelNode = GetNode<Label>(_rewardDescriptionLabel);
        _nextRewardPreviewLabelNode = GetNode<Label>(_nextRewardPreviewLabel);
        _streakCalendarContainerNode = GetNode<Control>(_streakCalendarContainer);
        _dismissButtonNode = GetNode<Button>(_dismissButton);
        _viewEventsButtonNode = GetNode<Button>(_viewEventsButton);
        _celebrationEffectsNode = GetNode<Control>(_celebrationEffects);
        _progressFillNode = GetNode<ProgressBar>(_progressFill);

        // Get managers
        _streakManager = StreakManager.Instance;
        _eventManager = SeasonalEventManager.Instance;
    }

    /// <summary>
    /// Connect signal handlers
    /// </summary>
    private void ConnectSignals()
    {
        if (_dismissButtonNode != null)
            _dismissButtonNode.Pressed += OnDismissButtonPressed;

        if (_viewEventsButtonNode != null)
            _viewEventsButtonNode.Pressed += OnViewEventsButtonPressed;

        // Connect streak manager signals
        if (_streakManager != null)
        {
            _streakManager.DailyRewardClaimed += OnDailyRewardClaimed;
            _streakManager.MilestoneCelebration += OnMilestoneCelebration;
        }

        // Connect event manager signals
        if (_eventManager != null)
        {
            _eventManager.EventStarted += OnEventStarted;
        }
    }

    /// <summary>
    /// Setup auto-dismiss timer
    /// </summary>
    private void SetupAutoDismiss()
    {
        _autoDismissTimer = new Timer();
        _autoDismissTimer.WaitTime = 30f; // 30 seconds
        _autoDismissTimer.OneShot = true;
        _autoDismissTimer.Timeout += OnAutoDismissTimeout;
        AddChild(_autoDismissTimer);
        _autoDismissTimer.Start();
    }

    /// <summary>
    /// Load streak data and update UI
    /// </summary>
    private void LoadStreakData()
    {
        if (_streakManager == null) return;

        var streakData = _streakManager.GetStreakDisplayData();
        UpdateStreakDisplay(streakData);
        UpdateRewardDisplay(streakData);
        UpdateProgressDisplay(streakData);
        UpdateCalendarDisplay(streakData);
        UpdateNextRewardPreview(streakData);
        UpdateViewEventsButton();
    }

    /// <summary>
    /// Update streak counter display
    /// </summary>
    private void UpdateStreakDisplay(Dictionary<string, Variant> streakData)
    {
        var currentStreak = (int)streakData.GetValueOrDefault("current_streak", 0);
        var bestStreak = (int)streakData.GetValueOrDefault("best_streak", 0);
        var streakStatus = streakData.GetValueOrDefault("streak_status", "").ToString();

        if (_streakCounterLabelNode != null)
        {
            if (currentStreak > 0)
            {
                _streakCounterLabelNode.Text = $"🔥 Day {currentStreak} of 30!";
                _streakCounterLabelNode.Modulate = currentStreak switch
                {
                    >= 22 => Colors.Gold,
                    >= 15 => Colors.Purple,
                    >= 8 => Colors.Blue,
                    _ => Colors.Green
                };
            }
            else
            {
                _streakCounterLabelNode.Text = "🌟 Start your streak today!";
                _streakCounterLabelNode.Modulate = Colors.White;
            }
        }
    }

    /// <summary>
    /// Update reward preview display
    /// </summary>
    private void UpdateRewardDisplay(Dictionary<string, Variant> streakData)
    {
        var currentReward = streakData.GetValueOrDefault("current_reward", "").ToString();
        var isEligible = (bool)streakData.GetValueOrDefault("is_eligible_for_reward", false);

        if (_rewardTitleLabelNode != null)
        {
            if (isEligible)
            {
                _rewardTitleLabelNode.Text = $"🎁 Today's Reward: {currentReward}";
                _rewardTitleLabelNode.Modulate = Colors.Yellow;
            }
            else
            {
                _rewardTitleLabelNode.Text = "✅ Reward Already Claimed!";
                _rewardTitleLabelNode.Modulate = Colors.Gray;
            }
        }

        if (_rewardDescriptionLabelNode != null)
        {
            var rewardDescription = GetRewardDescription();
            _rewardDescriptionLabelNode.Text = rewardDescription;
        }
    }

    /// <summary>
    /// Update progress bar display
    /// </summary>
    private void UpdateProgressDisplay(Dictionary<string, Variant> streakData)
    {
        var currentStreak = (int)streakData.GetValueOrDefault("current_streak", 0);
        var progress = (float)streakData.GetValueOrDefault("progress_to_next_milestone", 0f);

        if (_progressFillNode != null)
        {
            _progressFillNode.Value = progress * 100f;
            
            // Set color based on milestone progress
            if (progress >= 0.8f)
                _progressFillNode.Modulate = Colors.Gold;
            else if (progress >= 0.5f)
                _progressFillNode.Modulate = Colors.Blue;
            else
                _progressFillNode.Modulate = Colors.Green;
        }
    }

    /// <summary>
    /// Update streak calendar display
    /// </summary>
    private void UpdateCalendarDisplay(Dictionary<string, Variant> streakData)
    {
        if (_streakCalendarContainerNode == null) return;

        // Clear existing calendar
        foreach (var child in _streakCalendarContainerNode.GetChildren())
        {
            child.QueueFree();
        }

        // Create 30-day calendar grid
        CreateCalendarGrid();
    }

    /// <summary>
    /// Create 30-day calendar grid
    /// </summary>
    private void CreateCalendarGrid()
    {
        var currentStreak = _streakManager?.GetStreakDisplayData().GetValueOrDefault("current_streak", 0) ?? 0;

        for (int day = 1; day <= 30; day++)
        {
            var dayCell = CreateDayCell(day, day <= currentStreak);
            _streakCalendarContainerNode.AddChild(dayCell);
        }
    }

    /// <summary>
    /// Create a single day cell for calendar
    /// </summary>
    private Control CreateDayCell(int day, bool isCompleted)
    {
        var container = new Control();
        container.SetAnchorsAndOffsetsPreset(Control.PresetMode.TopLeft);
        container.Size = new Vector2(32, 32);
        
        var colorRect = new ColorRect();
        colorRect.Color = isCompleted ? Colors.Green : Colors.DarkGray;
        colorRect.Color.A = isCompleted ? 0.8f : 0.3f;
        colorRect.Size = new Vector2(28, 28);
        colorRect.Position = new Vector2(2, 2);
        container.AddChild(colorRect);

        var label = new Label();
        label.Text = day.ToString();
        label.HorizontalAlignment = HorizontalAlignment.Center;
        label.VerticalAlignment = VerticalAlignment.Center;
        label.Size = new Vector2(28, 28);
        label.Position = new Vector2(2, 2);
        label.Modulate = isCompleted ? Colors.White : Colors.Gray;
        container.AddChild(label);

        // Add milestone markers
        if (day == 7 || day == 14 || day == 21 || day == 30)
        {
            var milestoneIcon = new Label();
            milestoneIcon.Text = day == 30 ? "👑" : "⭐";
            milestoneIcon.Position = new Vector2(20, -5);
            milestoneIcon.Scale = new Vector2(0.7f, 0.7f);
            container.AddChild(milestoneIcon);
        }

        return container;
    }

    /// <summary>
    /// Update next reward preview
    /// </summary>
    private void UpdateNextRewardPreview(Dictionary<string, Variant> streakData)
    {
        if (_nextRewardPreviewLabelNode == null) return;

        var nextMilestoneDay = (int)streakData.GetValueOrDefault("next_milestone_day", 7);
        var daysUntilMilestone = (int)streakData.GetValueOrDefault("days_until_next_milestone", 0);
        var nextReward = GetNextMilestoneReward(nextMilestoneDay);

        if (daysUntilMilestone > 0)
        {
            _nextRewardPreviewLabelNode.Text = $"🎯 Next milestone ({nextMilestoneDay}): {nextReward}";
            _nextRewardPreviewLabelNode.Modulate = Colors.LightBlue;
        }
        else
        {
            _nextRewardPreviewLabelNode.Text = $"🏆 Milestone reached! {nextReward}";
            _nextRewardPreviewLabelNode.Modulate = Colors.Gold;
        }
    }

    /// <summary>
    /// Update view events button
    /// </summary>
    private void UpdateViewEventsButton()
    {
        if (_viewEventsButtonNode == null) return;

        var hasActiveEvents = _eventManager?.GetActiveEvents().Count > 0;
        _viewEventsButtonNode.Visible = hasActiveEvents;
        
        if (hasActiveEvents)
        {
            _viewEventsButtonNode.Text = "🎉 View Active Events";
        }
    }

    /// <summary>
    /// Show celebration effects if milestone reached
    /// </summary>
    private void ShowCelebrationIfNeeded()
    {
        if (_streakManager == null) return;

        var streakData = _streakManager.GetStreakDisplayData();
        var isMilestoneDay = (bool)streakData.GetValueOrDefault("is_milestone_day", false);

        if (isMilestoneDay)
        {
            ShowCelebrationEffects();
        }
    }

    /// <summary>
    /// Show celebration effects
    /// </summary>
    private void ShowCelebrationEffects()
    {
        if (_celebrationEffectsNode == null) return;

        // Show confetti effect
        var confettiTimer = new Timer();
        confettiTimer.WaitTime = 0.1f;
        confettiTimer.OneShot = false;
        confettiTimer.Timeout += () => SpawnConfettiParticle();
        AddChild(confettiTimer);
        confettiTimer.Start();

        // Stop after 3 seconds
        var stopTimer = new Timer();
        stopTimer.WaitTime = 3f;
        stopTimer.OneShot = true;
        stopTimer.Timeout += () => confettiTimer.Stop();
        AddChild(stopTimer);
        stopTimer.Start();

        // Play celebration sound
        AudioManager.Instance?.PlaySound("celebration_milestone");
    }

    /// <summary>
    /// Spawn confetti particle effect
    /// </summary>
    private void SpawnConfettiParticle()
    {
        // This would integrate with your particle system
        // For now, we'll log the effect
        GD.Print("🎊 Confetti effect spawned!");
    }

    /// <summary>
    /// Get reward description based on current streak
    /// </summary>
    private string GetRewardDescription()
    {
        var currentStreak = _streakManager?.GetStreakDisplayData().GetValueOrDefault("current_streak", 0) ?? 0;
        
        return currentStreak switch
        {
            1 => "Welcome to the game! Here's a starter reward to get you going!",
            2 => "Great job coming back! You're building a great habit!",
            3 => "Three days in a row! You're showing dedication!",
            4 => "Consistency is key! Keep up the great work!",
            5 => "Halfway through the week! You're doing amazing!",
            6 => "Almost there! One more day until your first milestone!",
            7 => "WEEK 1 COMPLETE! 🎉 You're on fire!",
            8 => "Starting week 2! The rewards are getting even better!",
            14 => "TWO WEEKS! 🔥 You're a true champion!",
            21 => "THREE WEEKS! 🏆 You're absolutely incredible!",
            30 => "MONTH MASTER! 👑 You're a legendary player!",
            _ => "Keep up the streak! Each day brings better rewards!"
        };
    }

    /// <summary>
    /// Get next milestone reward
    /// </summary>
    private string GetNextMilestoneReward(int milestoneDay)
    {
        return milestoneDay switch
        {
            7 => "Week 1 Complete Hat + 200 coins",
            14 => "Legendary Glasses + 400 coins", 
            21 => "Legendary Moustache + 800 coins",
            30 => "Legendary Crown + 2000 coins",
            _ => "Special milestone reward!"
        };
    }

    /// <summary>
    /// Handle dismiss button pressed
    /// </summary>
    private void OnDismissButtonPressed()
    {
        GD.Print("Login bonus screen dismissed");
        _autoDismissTimer?.Stop();
        EmitSignal("ScreenDismissed");
    }

    /// <summary>
    /// Handle view events button pressed
    /// </summary>
    private void OnViewEventsButtonPressed()
    {
        GD.Print("View events button pressed");
        EmitSignal("ViewSeasonalEvents");
    }

    /// <summary>
    /// Handle daily reward claimed
    /// </summary>
    private void OnDailyRewardClaimed(StreakReward reward)
    {
        GD.Print($"Daily reward claimed: {reward.Title}");
        LoadStreakData(); // Refresh UI
    }

    /// <summary>
    /// Handle milestone celebration
    /// </summary>
    private void OnMilestoneCelebration(int milestoneDay)
    {
        GD.Print($"Milestone celebration: {milestoneDay} days!");
        ShowCelebrationEffects();
    }

    /// <summary>
    /// Handle event started
    /// </summary>
    private void OnEventStarted(string eventId)
    {
        GD.Print($"Event started: {eventId}");
        UpdateViewEventsButton();
    }

    /// <summary>
    /// Handle auto dismiss timeout
    /// </summary>
    private void OnAutoDismissTimeout()
    {
        GD.Print("Login bonus screen auto-dismissed");
        EmitSignal("ScreenDismissed");
    }

    /// <summary>
    /// Clean up resources
    /// </summary>
    public override void _ExitTree()
    {
        _autoDismissTimer?.Stop();
        _autoDismissTimer?.QueueFree();
    }
}