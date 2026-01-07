using System;
<<<<<<< HEAD
using System.Collections.Generic;
=======
>>>>>>> origin/feat/launch-readiness-repo-docs-non-coder-cms-npc-prefabs
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
    [Export] protected int _targetScore = 3;
    [Export] private bool _isBonusRoom = false;
    [Export] private NodePath _nextRoomPath; // For bonus room transitions
    [Export] private NodePath _enemySpawnerPath; // For enemy spawning

<<<<<<< HEAD
    [ExportGroup("Objectives")]
    [Export] private Godot.Collections.Array<LevelObjective> _objectives = new();

=======
>>>>>>> origin/feat/launch-readiness-repo-docs-non-coder-cms-npc-prefabs
    private Slingshot? _slingshot;
    private Node2D? _exitDoor;
    private ProjectilesLoader? _projectilesLoader;
    private Node2D? _nextRoomMarker;
    private EnemySpawner? _enemySpawner;

    private ConfirmationDialog? _rewardedDialog;

    private enum RoomPhase { SLINGSHOT, TRAVERSAL, COMPLETE }
    private RoomPhase _currentPhase = RoomPhase.SLINGSHOT;

    private int _destructionScore;
    private bool _exitUnlocked;

    private bool _handlingFailure;

<<<<<<< HEAD
    // Objective tracking
    private readonly List<LevelObjective> _activeObjectives = new();
    private readonly List<int> _objectiveProgress = new();
    private readonly List<bool> _objectiveCompleted = new();
    private int _cupsDestroyed;
    private int _totalCups;
    private int _npcsDestroyed;
    private bool _exitReached;

=======
>>>>>>> origin/feat/launch-readiness-repo-docs-non-coder-cms-npc-prefabs
    public override void _Ready()
    {
        InitializeRoom();
        EnsureRewardDialog();
<<<<<<< HEAD
        InitializeObjectives();
        ConnectSignals();
        EmitObjectivesToHud();
=======
        ConnectSignals();
>>>>>>> origin/feat/launch-readiness-repo-docs-non-coder-cms-npc-prefabs
    }

    private void InitializeRoom()
    {
        _slingshot = GetNodeOrNull<Slingshot>(_slingshotPath);
        _exitDoor = GetNodeOrNull<Node2D>(_exitDoorPath);
        _projectilesLoader = GetNodeOrNull<ProjectilesLoader>(_projectilesLoaderPath);
        _nextRoomMarker = GetNodeOrNull<Node2D>(_nextRoomPath);
        _enemySpawner = GetNodeOrNull<EnemySpawner>(_enemySpawnerPath);

        if (_exitDoor != null)
        {
            _exitDoor.SetProcess(false);
        }

        var currentRoomIndex = GameManager.Instance?.CurrentRoomIndex ?? 0;
        if (GameManager.Instance != null && currentRoomIndex >= 0 && currentRoomIndex < GameManager.Instance.Rooms.Length)
        {
            int optimalScore = GameManager.Instance.Rooms[currentRoomIndex].OptimalScore;
            // Target to unlock door is 30% of optimal score
            _targetScore = (int)(optimalScore * 0.3f);
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

<<<<<<< HEAD
    private void InitializeObjectives()
    {
        _cupsDestroyed = 0;
        _npcsDestroyed = 0;
        _exitReached = false;

        _totalCups = GetTree().GetNodesInGroup(Cup.GROUP_NAME).Count;

        _activeObjectives.Clear();
        _objectiveProgress.Clear();
        _objectiveCompleted.Clear();

        if (_objectives != null && _objectives.Count > 0)
        {
            foreach (var obj in _objectives)
            {
                if (obj == null)
                    continue;

                _activeObjectives.Add(obj);
                _objectiveProgress.Add(0);
                _objectiveCompleted.Add(false);
            }
        }
        else
        {
            // Default objective: destroy all cups if present, otherwise reach the exit.
            var defaultObj = new LevelObjective
            {
                Type = _totalCups > 0 ? LevelObjective.ObjectiveType.DestroyXCups : LevelObjective.ObjectiveType.ReachExit,
                Count = _totalCups
            };

            _activeObjectives.Add(defaultObj);
            _objectiveProgress.Add(0);
            _objectiveCompleted.Add(false);
        }

        UpdateObjectiveState();
    }

    private void EmitObjectivesToHud()
    {
        SignalManager.EmitOnObjectivesUpdated(BuildObjectivesText());
    }

    private string BuildObjectivesText()
    {
        if (_activeObjectives.Count == 0)
            return string.Empty;

        string text = "";
        for (int i = 0; i < _activeObjectives.Count; i++)
        {
            var obj = _activeObjectives[i];
            var done = _objectiveCompleted[i];
            var progress = _objectiveProgress[i];

            var line = obj.GetDisplayText(progress);
            if (done)
                line = $"✓ {line}";

            text += (i == 0 ? "" : "\n") + line;
        }

        return text;
    }

    private void UpdateObjectiveState()
    {
        for (int i = 0; i < _activeObjectives.Count; i++)
        {
            var obj = _activeObjectives[i];
            bool complete = false;
            int progress = 0;

            switch (obj.Type)
            {
                case LevelObjective.ObjectiveType.DestroyXCups:
                    int required = obj.Count > 0 ? obj.Count : _totalCups;
                    progress = _cupsDestroyed;
                    complete = required > 0 && _cupsDestroyed >= required;
                    break;

                case LevelObjective.ObjectiveType.ReachExit:
                    progress = _exitReached ? 1 : 0;
                    complete = _exitReached;
                    break;

                case LevelObjective.ObjectiveType.DestroySpecificNpcs:
                    progress = _npcsDestroyed;
                    complete = obj.Count > 0 ? _npcsDestroyed >= obj.Count : _npcsDestroyed > 0;
                    break;

                default:
                    // Framework placeholder.
                    progress = 0;
                    complete = false;
                    break;
            }

            _objectiveProgress[i] = progress;
            _objectiveCompleted[i] = complete;
        }

        EmitObjectivesToHud();
    }

    private bool AreAllObjectivesComplete()
    {
        for (int i = 0; i < _objectiveCompleted.Count; i++)
        {
            if (!_objectiveCompleted[i])
                return false;
        }

        return _objectiveCompleted.Count > 0;
    }

=======
>>>>>>> origin/feat/launch-readiness-repo-docs-non-coder-cms-npc-prefabs
    private void ConnectSignals()
    {
        if (SignalManager.Instance != null)
        {
            SignalManager.Instance.OnDestructionScoreUpdated += OnDestructionScoreUpdated;
            SignalManager.Instance.OnCupDestroyed += OnCupDestroyed;
            SignalManager.Instance.OnPropDestroyed += OnPropDestroyed;
            SignalManager.Instance.OnAnimalDied += OnAnimalDied;
<<<<<<< HEAD
            SignalManager.Instance.OnNpcDestroyed += OnNpcDestroyed;
=======
>>>>>>> origin/feat/launch-readiness-repo-docs-non-coder-cms-npc-prefabs
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
<<<<<<< HEAD
            SignalManager.Instance.OnNpcDestroyed -= OnNpcDestroyed;
=======
>>>>>>> origin/feat/launch-readiness-repo-docs-non-coder-cms-npc-prefabs
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
<<<<<<< HEAD
        _cupsDestroyed++;
        UpdateObjectiveState();
=======
        GD.Print("Cup destroyed in room");
>>>>>>> origin/feat/launch-readiness-repo-docs-non-coder-cms-npc-prefabs
    }

    private void OnPropDestroyed(Node prop, int scoreValue)
    {
        GD.Print($"Prop destroyed with score value: {scoreValue}");
    }

<<<<<<< HEAD
    private void OnNpcDestroyed(Node npc)
    {
        _npcsDestroyed++;
        UpdateObjectiveState();
    }

=======
>>>>>>> origin/feat/launch-readiness-repo-docs-non-coder-cms-npc-prefabs
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

        // Add game feel feedback for door unlock
        if (GameFeelManager.Instance != null && _exitDoor != null)
        {
            GameFeelManager.Instance.OnDoorUnlocked(_exitDoor.GlobalPosition);
        }
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
<<<<<<< HEAD
        _exitReached = true;
        UpdateObjectiveState();
=======
        GD.Print("Player reached exit door");
>>>>>>> origin/feat/launch-readiness-repo-docs-non-coder-cms-npc-prefabs

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
