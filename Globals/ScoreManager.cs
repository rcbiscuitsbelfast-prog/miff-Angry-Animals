using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

/// <summary>
/// Global manager for handling player scores across levels.
/// Stores best scores per level and handles saving/loading them from disk.
/// Also keeps runtime score/attempt counters for UI.
/// </summary>
public partial class ScoreManager : Node
{
    /// <summary>
    /// Singleton for global access as an AutoLoad.
    /// </summary>
    public static ScoreManager Instance { get; private set; } = null!;

    [Signal] public delegate void ScoreChangedEventHandler(int score);
    [Signal] public delegate void AttemptsChangedEventHandler(int attempts);

    private const int DEFAULT_SCORE = 0;
    private const string SCORE_FILE = "user://animals.save";

    private int _score;
    private int _attempts;
    private int _selectedLevel;

    private List<LevelScore> _levelScores = new();

    public override void _Ready()
    {
        Instance = this;
        ProcessMode = ProcessModeEnum.Always;

        _levelScores = FileManager.LoadLevelScoreFromFile(SCORE_FILE);
        ResetRun();

        CallDeferred(nameof(DeferredConnectSignals));
    }

    public override void _ExitTree() => FileManager.SaveLevelScoreToFile(SCORE_FILE, _levelScores);

    private void DeferredConnectSignals()
    {
        if (SignalManager.Instance == null)
            return;

        SignalManager.Instance.OnAttemptMade += OnAttemptMade;
        SignalManager.Instance.OnScoreUpdated += OnScoreUpdated;
        SignalManager.Instance.OnDestructionScoreUpdated += OnDestructionScoreUpdated;

        if (GameManager.Instance != null)
            GameManager.Instance.RoomStarted += OnRoomStarted;
    }

    private void OnRoomStarted(int roomIndex)
    {
        ResetRun();
    }

    private void OnAttemptMade()
    {
        _attempts++;
        EmitSignal(SignalName.AttemptsChanged, _attempts);
    }

    private void OnScoreUpdated(int score)
    {
        _score = score;
        EmitSignal(SignalName.ScoreChanged, _score);
    }

    private void OnDestructionScoreUpdated(int score)
    {
        _score = score;
        EmitSignal(SignalName.ScoreChanged, _score);
    }

    /// <summary>
    /// Resets runtime score and attempts.
    /// </summary>
    public static void ResetRun() => Instance.ResetRunInternal();

    private void ResetRunInternal()
    {
        _score = 0;
        _attempts = 0;
        EmitSignal(SignalName.ScoreChanged, _score);
        EmitSignal(SignalName.AttemptsChanged, _attempts);
    }

    // -------------------------------------------------------------------------------
    // Level Selection
    // -------------------------------------------------------------------------------

    /// <summary>
    /// Gets the currently selected level number.
    /// </summary>
    public static int GetLevel() => Instance._selectedLevel;

    /// <summary>
    /// Sets the currently selected level number.
    /// </summary>
    public static int SetLevel(int newLevel) => Instance._selectedLevel = newLevel;

    // -------------------------------------------------------------------------------
    // Runtime score access
    // -------------------------------------------------------------------------------

    /// <summary>
    /// Gets the current runtime score.
    /// </summary>
    public static int GetScore() => Instance._score;

    /// <summary>
    /// Gets the current runtime attempts.
    /// </summary>
    public static int GetAttempts() => Instance._attempts;

    /// <summary>
    /// Adds to the runtime score.
    /// </summary>
    public static void AddScore(int amount)
    {
        if (amount == 0)
            return;

        Instance._score += amount;
        Instance.EmitSignal(SignalName.ScoreChanged, Instance._score);
        SignalManager.EmitOnDestructionScoreUpdated(Instance._score);
    }

    // -------------------------------------------------------------------------------
    // Best scores per level
    // -------------------------------------------------------------------------------

    /// <summary>
    /// Gets the LevelScore object for the specified level.
    /// </summary>
    public static LevelScore? GetLevelScore(int levelNumber) => Instance._levelScores.FirstOrDefault(ls => ls.LevelNumber == levelNumber);

    /// <summary>
    /// Gets the best score recorded for a given level.
    /// </summary>
    public static int GetLevelBestScore(int levelNumber)
    {
        var levelScore = GetLevelScore(levelNumber);
        return levelScore != null ? levelScore.BestScore : DEFAULT_SCORE;
    }

    /// <summary>
    /// Sets the score for a given level.
    /// If a score already exists, only updates it if the new score is better.
    /// </summary>
    public static void SetLevelScore(int levelNumber, int score)
    {
        var levelScore = GetLevelScore(levelNumber);

        if (levelScore != null)
        {
            if (score < levelScore.BestScore)
            {
                levelScore.BestScore = score;
                levelScore.DateSet = DateTime.Now;
            }
        }
        else
        {
            Instance._levelScores.Add(new LevelScore(levelNumber, score));
        }
    }
}
