namespace TerminalTetris.Models;

/*
 * Represents an individual high score leaderboard record.
 * Holds player identification, performance metrics, and session timestamp.
 */
public class highScoreEntry
{
    private string _playerInitials = "AAA";

    // Three-character arcade initials, automatically clamped and capitalized
    public string PlayerInitials
    {
        get => _playerInitials;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                _playerInitials = "AAA";
                return;
            }

            string clean = value.Trim().ToUpperInvariant();
            _playerInitials = clean.Length > 3 ? clean[..3] : clean.PadRight(3, ' ');
        }
    }

    // Final score points achieved
    public int Score { get; set; }

    // Total line clears completed during the game session
    public int Lines { get; set; }

    // Highest level achieved in the session
    public int Level { get; set; }

    // Timestamp when the record was achieved (UTC)
    public DateTime Date { get; set; } = DateTime.UtcNow;
}