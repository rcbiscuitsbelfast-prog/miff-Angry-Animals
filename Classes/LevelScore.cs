using System;

/// <summary>
/// Represents the score of a game level.
/// Stores the level number, the best score achieved, and the date when that score was recorded.
/// </summary>
public class LevelScore
{
    /// <summary>
    /// The number that represents the level played.
    /// </summary>
    public int LevelNumber { get; set; }

    /// <summary>
    /// The best score recorded for the level.
    /// Note: In this game's context, a lower value is always the better since it represents the number of attempts needed to succeed.
    /// E.G.: If a level has 3 cups, the best score possible is '0003'.
    /// </summary>
    public int BestScore { get; set; }

    /// <summary>
    /// The star rating achieved for this level (1-3).
    /// </summary>
    public int StarRating { get; set; }

    /// <summary>
    /// The date and time when the score was set.
    /// </summary>
    public DateTime DateSet { get; set; }

    /// <summary>
    /// Constructor that initializes a LevelScore record.
    /// The DateSet is automatically assigned to the current time upon creation.
    /// </summary>
    /// <param name="levelNumber">The number of the level.</param>
    /// <param name="bestScore">The best score achieved.</param>
    /// <param name="starRating">The star rating achieved.</param>
    public LevelScore(int levelNumber, int bestScore, int starRating = 0)
    {
        DateSet = DateTime.Now; // Record the current date and time.
        LevelNumber = levelNumber;
        BestScore = bestScore;
        StarRating = starRating;
    }
}
