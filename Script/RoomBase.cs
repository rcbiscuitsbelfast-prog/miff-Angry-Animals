using System;
using System.Threading.Tasks;
using Godot;

/// <summary>
/// Base class for all room/level scenes.
/// Handles the flow between slingshot phase and traversal phase,
/// manages room completion and exit door unlocking.
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

    private Slingshot? _slingshot;
    private Node2D? _exitDoor;
    private ProjectilesLoader? _projectilesLoader;
    private Node2D? _nextRoomMarker;

    private ConfirmationDialog? _rewardedDialog;

    private enum RoomPhase { SLINGSHOT, TRAVERSAL, COMPLETE }
    private RoomPhase _currentPhase = RoomPhase.SLINGSHOT;

    private int _destructionScore;
    private bool _exitUnlocked;

    private bool _handlingFailure;

    public override void _Ready()
    {
        InitializeRoom();
        EnsureRewardDialog();
        ConnectSignals();
    }

    private void InitializeRoom()
    {
        _slingshot = GetNodeOrNull<Slingshot>(_slingshotPath);
        _exitDoor = GetNodeOrNull<Node2D>(_exitDoorPath);
        _projectilesLoader = GetNodeOrNull<ProjectilesLoader>(_projectilesLoaderPath);
        _nextRoomMarker = GetNodeOrNull<Node2D>(_nextRoomPath);

        if (_exitDoor != null)
        {
            _exitDoor.SetProcess(false);
        }

        var currentRoomIndex = GameManager.Instance?.CurrentRoomIndex ?? 0;
        if (GameManager.Instance != null && currentRoomIndex >= 0 && currentRoomIndex < GameManager.Instance.Rooms.Length)
            _targetScore = GameManager.Instance.Rooms[currentRoomIndex].TargetScore;
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
