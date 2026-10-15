namespace TerminalTetris.Models;

public class highScoreEntry
{
    public string PlayerInitials { get; set; } = "AAA";
    public int Score { get; set; }
    public int Lines { get; set; }
    public int Level { get; set; }
    public DateTime Date { get; set; } =  DateTime.UtcNow;
}