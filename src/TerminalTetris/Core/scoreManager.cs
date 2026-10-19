namespace TerminalTetris.Core;

/*
 * Manages scoring, level progression, and dynamic gravity drop intervals.
 * Implements standard classic Tetris point multipliers and level scaling.
 */
public class scoreManager
{
    private const int LinesPerLevel = 10;
    private const int BaseDropIntervalMs = 800;
    private const int MinDropIntervalMs = 100;
    private const int SpeedDecrementPerLevelMs = 60;

    // Current player score tally
    public int Score { get; private set; }

    // Total lines cleared during the current run
    public int LinesCleared { get; private set; }

    // Derived level index based on lines cleared
    public int Level => LinesCleared / LinesPerLevel;

    /*
     * Returns the active gravity interval in milliseconds.
     * Decreases linearly with each level until reaching the minimum floor.
     */
    public int CurrentDropIntervalMs =>
        Math.Max(MinDropIntervalMs, BaseDropIntervalMs - (Level * SpeedDecrementPerLevelMs));

    /*
     * Evaluates simultaneously cleared lines and adds weighted score values.
     */
    public void RegisterLinesCleared(int lineCount)
    {
        if (lineCount <= 0)
        {
            return;
        }

        int currentLevelMultiplier = Level + 1;
        int pointsAwarded = lineCount switch
        {
            1 => 40 * currentLevelMultiplier,    // Single clear
            2 => 100 * currentLevelMultiplier,   // Double clear
            3 => 300 * currentLevelMultiplier,   // Triple clear
            4 => 1200 * currentLevelMultiplier,  // Tetris (quad clear)
            _ => 0
        };

        Score += pointsAwarded;
        LinesCleared += lineCount;
    }

    /*
     * Awards points for manual soft drops (1 point per downward cell moved).
     */
    public void AddSoftDropPoint()
    {
        Score += 1;
    }

    /*
     * Awards points for instantaneous hard drops (2 points per cell dropped).
     */
    public void AddHardDropPoints(int dropDistance)
    {
        if (dropDistance > 0)
        {
            Score += dropDistance * 2;
        }
    }

    /*
     * Resets score, line tally, and level progress for a fresh game session.
     */
    public void Reset()
    {
        Score = 0;
        LinesCleared = 0;
    }
}