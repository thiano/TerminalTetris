namespace TerminalTetris.Models;

public class playerData
{
    public List<highScoreEntry> HighScores { get; set; } = [];
    public gameStatistics Stats { get; set; } = new();
}