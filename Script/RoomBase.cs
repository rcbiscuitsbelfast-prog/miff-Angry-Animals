using System;
using System.Threading.Tasks;
using Godot;

/// <summary>
/// Base class for all room/level scenes.
/// Handles the flow between slingshot phase and traversal phase,
/// manages room completion and exit door unlocking.
/// Supports procedural level generation and visual theming.
/// </summary>
public partial class RoomBase : Node2D
{
    [Signal] public delegate void SlingshotPhaseStartedEventHandler();
    [Signal] public delegate void TraversalPhaseStartedEventHandler();
    [Signal] public delegate void RoomTargetReachedEventHandler();
    [Signal] public delegate void ExitDoorUnlockedEventHandler();

    [Export] private NodePath _slingshotPath;
    [Export] private NodePath _exitDoorPath;
    [Export] private NodePath _projectilesLoaderPath;
    [Export] private int _targetScore = 3;
    [Export] private bool _isBonusRoom = false;
    [Export] private NodePath _nextRoomPath; // For bonus room transitions

    // Procedural generation exports
    [Export] private bool _enableProceduralGeneration = true;
    [Export] private NodePath _cupsContainerPath;
    [Export] private NodePath _backgroundPath;
    [Export] private NodePath _floorPath;
    [Export] private PackedScene _cupScene;

    private Slingshot? _slingshot;
    private Node2D? _exitDoor;
    private ProjectilesLoader? _projectilesLoader;
    private Node2D? _nextRoomMarker;
    private Node2D? _cupsContainer;
    private Sprite2D? _background;
    private Sprite2D? _floor;

    private ConfirmationDialog? _rewardedDialog;

    private enum RoomPhase { SLINGSHOT, TRAVERSAL, COMPLETE }
    private RoomPhase _currentPhase = RoomPhase.SLINGSHOT;

    private int _destructionScore;
    private bool _exitUnlocked;

    private bool _handlingFailure;
    private LevelGenerator? _levelGenerator;

    public override void _Ready()
    {
        InitializeRoom();
        ApplyProceduralGeneration();
        EnsureRewardDialog();
        ConnectSignals();
    }

    private void InitializeRoom()
    {
        _slingshot = GetNodeOrNull<Slingshot>(_slingshotPath);
        _exitDoor = GetNodeOrNull<Node2D>(_exitDoorPath);
        _projectilesLoader = GetNodeOrNull<ProjectilesLoader>(_projectilesLoaderPath);
        _nextRoomMarker = GetNodeOrNull<Node2D>(_nextRoomPath);
        _cupsContainer = GetNodeOrNull<Node2D>(_cupsContainerPath);
        _background = GetNodeOrNull<Sprite2D>(_backgroundPath);
        _floor = GetNodeOrNull<Sprite2D>(_floorPath);

        if (_exitDoor != null)
        {
            _exitDoor.SetProcess(false);
        }

        var currentRoomIndex = GameManager.Instance?.CurrentRoomIndex ?? 0;
        if (GameManager.Instance != null && currentRoomIndex >= 0 && currentRoomIndex < GameManager.Instance.Rooms.Length)
            _targetScore = GameManager.Instance.Rooms[currentRoomIndex].TargetScore;

        // Initialize level generator for procedural content
        if (_enableProceduralGeneration && GameManager.Instance != null)
        {
            _levelGenerator = new LevelGenerator(currentRoomIndex + 1);
        }
    }

    private void ApplyProceduralGeneration()
    {
        if (!_enableProceduralGeneration || _levelGenerator == null)
            return;

        int roomNumber = GameManager.Instance?.CurrentRoomIndex + 1 ?? 1;

        // Apply visual theme
        ApplyVisualTheme(roomNumber);

        // Generate cups procedurally
        GenerateCups();
    }

    private void ApplyVisualTheme(int roomNumber)
    {
        // Get theme configuration
        var theme = LevelGenerator.GetThemeForRoom(roomNumber);

        // Apply background color via modulate
        if (_background != null)
        {
            _background.Modulate = theme.BackgroundColor;
        }

        // Apply floor color
        if (_floor != null)
        {
            _floor.Modulate = theme.FloorColor;
        }

        // Apply premium effects if available
        if (theme.HasPremiumEffects)
        {
            // Add subtle particle effects or visual enhancements
            ApplyPremiumEffects();
        }
    }

    private void ApplyPremiumEffects()
    {
        // Add visual enhancements for premium rooms
        // This can include:
        // - Additional particle systems
        // - Enhanced floor texture
        // - Special lighting effects
        GD.Print("Applying premium visual effects");
    }

    private void GenerateCups()
    {
        if (_levelGenerator == null || _cupsContainer == null || _cupScene == null)
            return;

        // Clear existing cups
        foreach (var child in _cupsContainer.GetChildren())
        {
            if (child is Node2D node)
                node.QueueFree();
        }

        // Generate new cup configurations
        int cupCount = LevelGenerator.GetCupCountForRoom(GameManager.Instance?.CurrentRoomIndex + 1 ?? 1);
        var cupConfigs = _levelGenerator.GenerateCupConfigs(cupCount);

        // Spawn cups
        foreach (var config in cupConfigs)
        {
            SpawnCup(config);
        }

        GD.Print($"Generated {cupCount} cups for room {GameManager.Instance?.CurrentRoomIndex + 1}");
    }

    private void SpawnCup(LevelGenerator.CupConfig config)
    {
        if (_cupScene == null || _cupsContainer == null)
            return;

        var cup = _cupScene.Instantiate();
        if (cup is Node2D cupNode)
        {
            cupNode.GlobalPosition = config.Position;
            cupNode.Rotation = config.Rotation;
            cupNode.Scale = new Vector2(config.Scale, config.Scale);

            // Apply premium styling if needed
            if (config.IsPremium)
            {
                ApplyPremiumCupStyle(cupNode);
            }

            _cupsContainer.AddChild(cupNode);
        }
    }

    private void ApplyPremiumCupStyle(Node2D cup)
    {
        // Apply visual enhancement to premium room cups
        // This could include different colors, glow effects, etc.
        if (cup is Sprite2D sprite)
        {
            // Add slight glow or color variation for premium cups
            sprite.Modulate = sprite.Modulate.Lightened(0.1f);
        }
    }

    private void EnsureRewardDialog()
    {
        _rewardedDialog = new ConfirmationDialog
        {
            Name = "RewardedDialog",
            Title = "Bonus",
            DialogText = "Watch an ad to get 5 bonus points?",
            ProcessMode = ProcessModeEnum.Always
        };
        _rewardedDialog.GetOkButton().Text = "Watch";
        _rewardedDialog.GetCancelButton().Text = "Retry";
        _rewardedDialog.Confirmed += OnRewardedAccepted;
        _rewardedDialog.Canceled += OnRewardedCanceled;
        AddChild(_rewardedDialog);
    }

    private void ConnectSignals()
    {
        if (SignalManager.Instance != null)
        {
            SignalManager.Instance.OnDestructionScoreUpdated += OnDestructionScoreUpdated;
            SignalManager.Instance.OnCupDestroyed += OnCupDestroyed;
            SignalManager.Instance.OnPropDestroyed += OnPropDestroyed;
            SignalManager.Instance.OnAnimalDied += OnAnimalDied;
        }

        if (_projectilesLoader != null)
        {
            _projectilesLoader.ProjectileLaunched += OnProjectileLaunched;
            _projectilesLoader.AllProjectilesUsed += OnAllProjectilesUsed;
        }

        if (_slingshot != null)
        {
            _slingshot.ProjectileLaunched += OnSlingshotProjectileLaunched;
        }

        if (AdsManager.Instance != null)
        {
            AdsManager.Instance.RewardEarned += OnRewardEarned;
        }
    }

    public override void _ExitTree()
    {
        if (SignalManager.Instance != null)
        {
            SignalManager.Instance.OnDestructionScoreUpdated -= OnDestructionScoreUpdated;
            SignalManager.Instance.OnCupDestroyed -= OnCupDestroyed;
            SignalManager.Instance.OnPropDestroyed -= OnPropDestroyed;
            SignalManager.Instance.OnAnimalDied -= OnAnimalDied;
        }

        if (_projectilesLoader != null)
        {
            _projectilesLoader.ProjectileLaunched -= OnProjectileLaunched;
            _projectilesLoader.AllProjectilesUsed -= OnAllProjectilesUsed;
        }

        if (_slingshot != null)
        {
            _slingshot.ProjectileLaunched -= OnSlingshotProjectileLaunched;
        }

        if (AdsManager.Instance != null)
        {
            AdsManager.Instance.RewardEarned -= OnRewardEarned;
        }

        if (_rewardedDialog != null)
        {
            _rewardedDialog.Confirmed -= OnRewardedAccepted;
            _rewardedDialog.Canceled -= OnRewardedCanceled;
        }
    }

    private void OnDestructionScoreUpdated(int score)
    {
        _destructionScore = score;

        if (_destructionScore >= _targetScore && !_exitUnlocked)
            UnlockExitDoor();
    }

    private void OnCupDestroyed()
    {
        GD.Print("Cup destroyed in room");
    }

    private void OnPropDestroyed(Node prop, int scoreValue)
    {
        GD.Print($"Prop destroyed with score value: {scoreValue}");
    }

    private void OnAnimalDied()
    {
        if (_currentPhase == RoomPhase.SLINGSHOT && _projectilesLoader != null)
        {
            if (_projectilesLoader.HasMoreProjectiles)
                StartTraversalPhase();
            else
                HandleAttemptsFailed();
        }
    }

    private void OnProjectileLaunched(Projectile projectile)
    {
    }

    private void OnSlingshotProjectileLaunched(Projectile projectile)
    {
        projectile.AlmostStopped += () => OnProjectileAlmostStopped(projectile);
    }

    private void OnProjectileAlmostStopped(Projectile projectile)
    {
        if (_currentPhase == RoomPhase.SLINGSHOT)
            StartTraversalPhase();
    }

    private void OnAllProjectilesUsed()
    {
        if (_exitUnlocked)
            CompleteRoom();
        else
            HandleAttemptsFailed();
    }

    private void StartTraversalPhase()
    {
        if (_currentPhase != RoomPhase.SLINGSHOT)
            return;

        GD.Print("Starting traversal phase");
        _currentPhase = RoomPhase.TRAVERSAL;
        EmitSignal(SignalName.TraversalPhaseStarted);

        SpawnStickClone();
    }

    private void SpawnStickClone()
    {
        var hat = PlayerProfile.GetHats()[PlayerProfile.Instance.SelectedHatIndex];
        var glasses = PlayerProfile.GetGlasses()[PlayerProfile.Instance.SelectedGlassesIndex];
        var emotion = PlayerProfile.GetEmotions()[PlayerProfile.Instance.SelectedEmotionIndex];

        var spawnPosition = FindStickCloneSpawnPosition();
        if (spawnPosition == Vector2.Zero)
        {
            GD.PushWarning("Could not find spawn position for StickClone");
            return;
        }

        var stickCloneScene = ResourceLoader.Load<PackedScene>("res://Scenes/Characters/StickClone.tscn");
        if (stickCloneScene != null)
        {
            var stickClone = stickCloneScene.Instantiate<StickClone>();
            stickClone.GlobalPosition = spawnPosition;
            AddChild(stickClone);

            GD.Print($"Spawning StickClone at {spawnPosition} with: Hat={hat}, Glasses={glasses}, Emotion={emotion}");
        }
        else
        {
            GD.PushWarning("StickClone scene not found: res://Scenes/Characters/StickClone.tscn");
        }
    }

    private void UnlockExitDoor()
    {
        _exitUnlocked = true;

        if (_exitDoor != null)
            _exitDoor.SetProcess(true);

        GD.Print($"Exit door unlocked! Score: {_destructionScore}/{_targetScore}");
        EmitSignal(SignalName.ExitDoorUnlocked);
    }

    private async void CompleteRoom()
    {
        if (_currentPhase == RoomPhase.COMPLETE)
            return;

        GD.Print("Room completed!");
        _currentPhase = RoomPhase.COMPLETE;
        EmitSignal(SignalName.RoomTargetReached);

        await MaybeShowInterstitialBeforeCompletionAsync();
        OnLevelCompleted();
    }

    /// <summary>
    /// Called when the room is completed and the completion flow should proceed.
    /// </summary>
    private void OnLevelCompleted()
    {
        if (_isBonusRoom && _nextRoomMarker != null)
            HandleBonusRoomTransition();
        else
            SignalManager.EmitOnLevelCompleted();
    }

    private void HandleBonusRoomTransition()
    {
        GD.Print("Bonus room completed, handling transition...");
        SignalManager.EmitOnLevelCompleted();
    }

    private async Task MaybeShowInterstitialBeforeCompletionAsync()
    {
        if (MonetizationManager.Instance?.ShowAds != true)
            return;

        if (AdsManager.Instance == null)
            return;

        try
        {
            await AdsManager.Instance.ShowInterstitialAd();
        }
        catch (Exception ex)
        {
            GD.PushWarning($"Interstitial ad failed: {ex.Message}");
        }
    }

    private void HandleAttemptsFailed()
    {
        if (_handlingFailure || _currentPhase == RoomPhase.COMPLETE)
            return;

        _handlingFailure = true;
        OnAttemptsFailed();
    }

    /// <summary>
    /// Called when the player runs out of attempts / all projectiles are used without meeting the target.
    /// Offers an optional rewarded ad for a small score boost.
    /// </summary>
    private void OnAttemptsFailed()
    {
        if (_rewardedDialog == null)
        {
            GameManager.RestartRoom();
            return;
        }

        if (MonetizationManager.Instance?.ShowAds != true || AdsManager.Instance == null)
        {
            GameManager.RestartRoom();
            return;
        }

        _rewardedDialog.PopupCentered();
    }

    private async void OnRewardedAccepted()
    {
        if (AdsManager.Instance == null)
        {
            GameManager.RestartRoom();
            return;
        }

        try
        {
            var rewardTask = ToSignal(AdsManager.Instance, AdsManager.SignalName.RewardEarned);
            await AdsManager.Instance.ShowRewardedAd();

            if (rewardTask.IsCompleted)
                ApplyRewardPoints(5);
        }
        finally
        {
            _handlingFailure = false;

            if (_exitUnlocked)
                CompleteRoom();
            else
                GameManager.RestartRoom();
        }
    }

    private void OnRewardedCanceled()
    {
        _handlingFailure = false;
        GameManager.RestartRoom();
    }

    private void OnRewardEarned()
    {
        // Bonus points are applied in OnRewardedAccepted after the ad flow.
    }

    private void ApplyRewardPoints(int points)
    {
        _destructionScore += points;
        OnDestructionScoreUpdated(_destructionScore);
        SignalManager.EmitOnDestructionScoreUpdated(_destructionScore);
    }

    /// <summary>
    /// Called when player reaches the exit door.
    /// </summary>
    public void OnExitReached()
    {
        GD.Print("Player reached exit door");

        if (_exitUnlocked)
            CompleteRoom();
    }

    /// <summary>
    /// Gets the current destruction score.
    /// </summary>
    public int GetDestructionScore() => _destructionScore;

    /// <summary>
    /// Gets the target score for this room.
    /// </summary>
    public int GetTargetScore() => _targetScore;

    /// <summary>
    /// Checks if the exit door is unlocked.
    /// </summary>
    public bool IsExitUnlocked() => _exitUnlocked;

    /// <summary>
    /// Finds a suitable spawn position for StickClone.
    /// </summary>
    /// <returns>Spawn position or Vector2.Zero if not found</returns>
    private Vector2 FindStickCloneSpawnPosition()
    {
        var spawnMarker = GetNodeOrNull<Node2D>("StickCloneSpawn");
        if (spawnMarker != null)
            return spawnMarker.GlobalPosition;

        if (_slingshot != null)
            return _slingshot.GlobalPosition + new Vector2(100, 0);

        return Vector2.Zero;
    }
}
