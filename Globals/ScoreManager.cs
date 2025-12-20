using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

/// <summary>
/// Global manager for handling player scores across levels.
/// Stores best scores per level and handles saving/loading them from disk.
/// </summary>
public partial class ScoreManager : Node
{
    public static ScoreManager Instance { get; private set; } // Singleton for global access as an AutoLoad.

    [Signal] public delegate void ScoreChangedEventHandler(int score);
    [Signal] public delegate void AttemptsChangedEventHandler(int attempts);


    /// <summary>
    /// Default score used when no data exists for a level.
    /// </summary>
    private const int DEFAULT_SCORE = 0000;

    /// <summary>
    /// Path to the save file where scores are stored.
    /// Uses Godot's "user://" prefic to ensure cross-platform compatibility.
    /// </summary>
    private const string SCORE_FILE = "user://animals.save";

    /// <summary>
    /// Current runtime score (not directly tied to level best scores).
    /// </summary>
    private int _score = 0;

    /// <summary>
    /// The currently selected level number.
    /// </summary>
    private int _selectedLevel;


    /// <summary>
    /// List of all recorded level scores.
    /// Each entry contains the level number, best score, and the date it was set.
    /// </summary>
    List<LevelScore> _levelScores = new();


    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        Instance = this;

        _levelScores = FileManager.LoadLevelScoreFromFile(SCORE_FILE); // Loads scores from file.
    }


    /// <summary>
    /// Called when the node exits the scene tree.
    /// Saves scores to file before quitting.
    /// </summary>
    public override void _ExitTree() => FileManager.SaveLevelScoreToFile(SCORE_FILE, _levelScores);



    // -------------------------------------------------------------------------------
    // Level Selection
    // -------------------------------------------------------------------------------

    /// <summary>
    /// Gets the currently selected level number.
    /// </summary>
    /// <returns>Current level selected.</returns>
    public static int GetLevel() => Instance._selectedLevel;


    /// <summary>
    /// Sets the currently selected level number.
    /// </summary>
    /// <param name="newLevel"></param>
    /// <returns>A new level.</returns>
    public static int SetLevel(int newLevel) => Instance._selectedLevel = newLevel;



    // -------------------------------------------------------------------------------
    // Score Retrieval and Modification
    // -------------------------------------------------------------------------------

    /// <summary>
    /// Gets the LevelScore object for the specified level.
    /// </summary>
    /// <param name="levelNumber"></param>
    /// <returns>Null if no Score exists for that level.</returns>
    public static LevelScore GetLevelScore(int levelNumber) => Instance._levelScores.FirstOrDefault(ls => ls.LevelNumber == levelNumber);


    /// <summary>
    /// Gets the best score recorded for a given level.
    /// </summary>
    /// <param name="levelNumber"></param>
    /// <returns>DEFAULT_SCORE if no record exists.</returns>
    public static int GetLevelBestScore(int levelNumber)
    {
        LevelScore levelScore = GetLevelScore(levelNumber);

        if (levelScore != null) return levelScore.BestScore;

        return DEFAULT_SCORE;
    }


    /// <summary>
    /// Sets the score for a given level.
    /// If a score already exists, only updates it if the new score is better.
    /// Otherwise, adds a new LevelScore entry.
    /// </summary>
    /// <param name="levelNumber"></param>
    /// <param name="score"></param>
    public static void SetLevelScore(int levelNumber, int score)
    {
        LevelScore levelScore = GetLevelScore(levelNumber);

        if (levelScore != null)
        {
            // Replace score only if it's better (lower in this case).
            if (score < levelScore.BestScore)
            {
                levelScore.BestScore = score;
                levelScore.DateSet = DateTime.Now;
            }
        }

        // Add a new entry if the level has no score yet.
        else Instance._levelScores.Add(new LevelScore(levelNumber, score));
    }
}