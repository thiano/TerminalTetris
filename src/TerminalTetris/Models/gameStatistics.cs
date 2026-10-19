namespace TerminalTetris.Models;

/*
 * Data transfer object aggregating cumulative lifetime player statistics.
 * Persisted to disk via high score serialization.
 */
public class gameStatistics
{
    // Total number of completed game sessions
    public int GamesPlayed { get; set; }

    // Cumulative sum of all lines cleared across all sessions
    public int TotalLinesCleared { get; set; }

    // Total count of 4-line clears (Tetris clears) achieved
    public int TotalTetrisClears { get; set; }

    // Highest level reached in any individual session
    public int MaxLevelReached { get; set; }

    // Cumulative gameplay time recorded across all sessions in seconds
    public int TotalSecondsPlayed { get; set; }
}