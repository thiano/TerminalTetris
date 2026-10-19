using TerminalTetris.Core;
using TerminalTetris.Input;
using TerminalTetris.Models;
using TerminalTetris.Rendering;
using TerminalTetris.Services;

namespace TerminalTetris;

/*
 * Entry point van de TerminalTetris applicatie.
 * Ondersteunt directe CLI-vlaggen (--scores, --help) en start anders direct de gameplay.
 */
public static class program
{
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
                    Console.WriteLine($"Onbekend argument: '{args[0]}'. Gebruik --help voor opties.");
                    return;
            }
        }

        // Geen argumenten: start direct de game
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

    private static void PrintLeaderboardCli(highScoreService service)
    {
        Console.WriteLine("\x1b[96m=== TERMINALTETRIS LEADERBOARD ===\x1b[0m\n");

        var scores = service.Data.HighScores;
        if (scores.Count == 0)
        {
            Console.WriteLine("Nog geen scores geregistreerd.");
        }
        else
        {
            Console.WriteLine(" #   INIT   SCORE      LVL   LINES   DATUM");
            Console.WriteLine("--------------------------------------------------");
            for (int i = 0; i < scores.Count; i++)
            {
                var entry = scores[i];
                string dateStr = entry.Date.ToLocalTime().ToString("yyyy-MM-dd");
                Console.WriteLine($" {i + 1,-3} {entry.PlayerInitials,-6} {entry.Score,7}    {entry.Level,-5} {entry.Lines,-7} {dateStr}");
            }
        }

        var stats = service.Data.Stats;
        int hours = stats.TotalSecondsPlayed / 3600;
        int minutes = (stats.TotalSecondsPlayed % 3600) / 60;
        int seconds = stats.TotalSecondsPlayed % 60;
        string timeFormatted = hours > 0 ? $"{hours}u {minutes}m {seconds}s" : $"{minutes}m {seconds}s";

        Console.WriteLine("\n\x1b[93m--- STATISTIEKEN ---\x1b[0m");
        Console.WriteLine($" Totaal gespeeld:     {stats.GamesPlayed} games");
        Console.WriteLine($" Totale speeltijd:    {timeFormatted}");
        Console.WriteLine($" Totaal gewiste rijen:{stats.TotalLinesCleared}");
        Console.WriteLine($" Aantal 'TETRIS' (4L):{stats.TotalTetrisClears}");
        Console.WriteLine($" Hoogste level ooit:  Lvl {stats.MaxLevelReached}\n");
    }

    private static void PrintHelpCli()
    {
        Console.WriteLine("\x1b[96mTerminalTetris\x1b[0m - Retro Tetris in je terminal\n");
        Console.WriteLine("Gebruik:");
        Console.WriteLine("  TerminalTetris            Start het spel direct");
        Console.WriteLine("  TerminalTetris -s, --scores  Toon de lokale top 5 en stats");
        Console.WriteLine("  TerminalTetris -h, --help    Toon dit helpscherm\n");
        Console.WriteLine("Controls tijdens spel:");
        Console.WriteLine("  A / D   : Links / Rechts bewegen");
        Console.WriteLine("  W       : Roteren (met SRS wall kicks)");
        Console.WriteLine("  S       : Soft drop");
        Console.WriteLine("  Spatie  : Hard drop");
        Console.WriteLine("  C       : Hold piece");
        Console.WriteLine("  P       : Pauzeren");
        Console.WriteLine("  Q       : Afsluiten\n");
    }
}