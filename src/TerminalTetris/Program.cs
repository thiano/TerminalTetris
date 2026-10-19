using TerminalTetris.Core;
using TerminalTetris.Input;
using TerminalTetris.Models;
using TerminalTetris.Rendering;
using TerminalTetris.Services;

namespace TerminalTetris;

/*
 * Entry point for the TerminalTetris application.
 * Evaluates CLI arguments (--scores, --help) or launches interactive gameplay.
 */
public static class Program
{
    /*
     * Application entry routine.
     */
    public static void Main(string[] args)
    {
        var highScores = new highScoreService();

        if (args.Length > 0)
        {
            string flag = args[0].ToLowerInvariant();
            switch (flag)
            {
                case "--scores":
                case "-s":
                    PrintLeaderboardCli(highScores);
                    return;

                case "--help":
                case "-h":
                    PrintHelpCli();
                    return;

                default:
                    Console.WriteLine($"Unknown option: '{args[0]}'. Use --help for usage information.");
                    return;
            }
        }

        // Initialize core gameplay components
        var gameBoard = new board();
        var factory = new tetrominoFactory();
        var scoreTracker = new scoreManager();
        var inputService = new consoleInputHandler();
        var visualRenderer = new ansiConsoleRenderer();

        var engine = new gameEngine(
            gameBoard,
            factory,
            scoreTracker,
            inputService,
            visualRenderer,
            highScores);

        engine.Run();
    }

    /*
     * Prints local high scores and lifetime statistics to standard output.
     */
    private static void PrintLeaderboardCli(highScoreService service)
    {
        Console.WriteLine("\x1b[96m=== TERMINALTETRIS LEADERBOARD ===\x1b[0m\n");

        var scores = service.Data.HighScores;
        if (scores.Count == 0)
        {
            Console.WriteLine("No records registered yet.");
        }
        else
        {
            Console.WriteLine(" #   INIT   SCORE      LVL   LINES   DATE");
            Console.WriteLine("--------------------------------------------------");
            for (int i = 0; i < scores.Count; i++)
            {
                var entry = scores[i];
                string dateStr = entry.Date.ToLocalTime().ToString("yyyy-MM-dd");
                Console.WriteLine($" {i + 1,-3} {entry.PlayerInitials,-6} {entry.Score,7}     {entry.Level,-5} {entry.Lines,-7} {dateStr}");
            }
        }

        var stats = service.Data.Stats;
        int hours = stats.TotalSecondsPlayed / 3600;
        int minutes = (stats.TotalSecondsPlayed % 3600) / 60;
        int seconds = stats.TotalSecondsPlayed % 60;
        string timeFormatted = hours > 0 ? $"{hours}h {minutes}m {seconds}s" : $"{minutes}m {seconds}s";

        Console.WriteLine("\n\x1b[93m--- STATISTICS ---\x1b[0m");
        Console.WriteLine($" Games Played:        {stats.GamesPlayed}");
        Console.WriteLine($" Total Play Time:     {timeFormatted}");
        Console.WriteLine($" Total Lines Cleared: {stats.TotalLinesCleared}");
        Console.WriteLine($" Tetris Clears (4L):  {stats.TotalTetrisClears}");
        Console.WriteLine($" Max Level Reached:   Lvl {stats.MaxLevelReached}\n");
    }

    /*
     * Prints CLI command syntax and in-game key bindings to standard output.
     */
    private static void PrintHelpCli()
    {
        Console.WriteLine("\x1b[96mTerminalTetris\x1b[0m - Retro Tetris for your terminal\n");
        Console.WriteLine("Usage:");
        Console.WriteLine("  TerminalTetris            Launch interactive game");
        Console.WriteLine("  TerminalTetris -s, --scores  Display local leaderboard and stats");
        Console.WriteLine("  TerminalTetris -h, --help    Show this help message\n");
        Console.WriteLine("Controls:");
        Console.WriteLine("  A / D   : Move Left / Right");
        Console.WriteLine("  W       : Rotate Clockwise (SRS wall kicks)");
        Console.WriteLine("  Z       : Rotate Counter-Clockwise");
        Console.WriteLine("  S       : Soft Drop");
        Console.WriteLine("  Space   : Hard Drop");
        Console.WriteLine("  C       : Hold Piece");
        Console.WriteLine("  P       : Toggle Pause");
        Console.WriteLine("  Q / Esc : Quit Game\n");
    }
}