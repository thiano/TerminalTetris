namespace TerminalTetris.Core;

/*
 * Beheert scores, combo-vermenigvuldigers, levels en de dynamische valsnelheid.
 * Gebaseerd op het klassieke Tetris-puntensysteem.
 */
public class scoreManager
{
    private const int LinesPerLevel = 10;
    private const int BaseDropIntervalMs = 800;
    private const int MinDropIntervalMs = 100;
    private const int SpeedDecrementPerLevelMs = 60;

    public int Score { get; private set; }
    public int LinesCleared { get; private set; }
    public int Level => LinesCleared / LinesPerLevel;

    /*
     * Geeft het actuele tijdsinterval in milliseconden tussen twee zwaartekrachtstappen.
     * Naarmate het level stijgt, daalt dit interval lineair tot een vast minimum.
     */
    public int CurrentDropIntervalMs
    {
        get
        {
            int calculatedInterval = BaseDropIntervalMs - (Level * SpeedDecrementPerLevelMs);
            return Math.Max(MinDropIntervalMs, calculatedInterval);
        }
    }

    /*
     * Verwerkt het aantal gelijktijdig gewiste lijnen en kent punten toe.
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
            1 => 40 * currentLevelMultiplier,    // Single
            2 => 100 * currentLevelMultiplier,   // Double
            3 => 300 * currentLevelMultiplier,   // Triple
            4 => 1200 * currentLevelMultiplier,  // Tetris
            _ => 0
        };

        Score += pointsAwarded;
        LinesCleared += lineCount;
    }

    /*
     * Voegt optioneel punten toe voor soft-drops (manueel omlaag versnellen).
     */
    public void AddSoftDropPoint()
    {
        Score += 1;
    }

    /*
     * Reset de score, lijnen en level voor een nieuwe speelsessie.
     */
    public void Reset()
    {
        Score = 0;
        LinesCleared = 0;
    }
}