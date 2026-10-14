using TerminalTetris.Core;
using TerminalTetris.Input;
using TerminalTetris.Models;
using TerminalTetris.Rendering;

namespace TerminalTetris;

/*
 * Entry point van de TerminalTetris applicatie.
 * Initialiseert alle subdomeinen en start de gameEngine.
 */
public static class program
{
    public static void Main(string[] args)
    {
        // 1. Initialiseer modellen en services
        var gameBoard = new board();
        var factory = new tetrominoFactory();
        var scoreTracker = new scoreManager();
        var inputService = new consoleInputHandler();
        var visualRenderer = new ansiConsoleRenderer();

        // 2. Koppel alles via de centrale GameEngine
        var engine = new gameEngine(
            gameBoard,
            factory,
            scoreTracker,
            inputService,
            visualRenderer);

        // 3. Start het spel
        engine.Run();
    }
}