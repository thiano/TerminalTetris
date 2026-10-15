namespace TerminalTetris.Models;

public class gameStatistics
{
    public int GamesPlayed { get; set; }
    public int TotalLinesCleared { get; set; }
    public int TotalTetrisClears { get; set; } //  4-rij clears tegelijk
    public int MaxLevelReached { get; set; }
    public int TotalSecondsPlayed { get; set; }
}