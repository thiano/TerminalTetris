namespace TerminalTetris.Models;

/*
 * Root data model for persisting player profile information to disk.
 * Encapsulates both leaderboard rankings and lifetime game statistics.
 */
public class playerData
{
    // Ordered collection of top high score records
    public List<highScoreEntry> HighScores { get; set; } = [];

    // Aggregated lifetime metrics and session counts
    public gameStatistics Stats { get; set; } = new();
}