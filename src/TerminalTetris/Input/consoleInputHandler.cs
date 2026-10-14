namespace TerminalTetris.Input;

/*
 * Concrete implementatie van iInputhandler die de System.Console-invoerbuffer uitleest
 */
public class consoleInputHandler : iInputHandler
{
    /*
     * Leest de consolebuffer uit zonder de uitvoerthread te blokkeren
     */
    public gameAction PollAction()
    {
        if (!Console.KeyAvailable)
        {
            return gameAction.None;
        }
        
        ConsoleKeyInfo keyInfo = Console.ReadKey(intercept: true);
        
        // Consumeer eventuele overtollige toetsen in de buffer om input-lag te voorkomen
        while (Console.KeyAvailable){}
        {
            Console.ReadKey(intercept: true);
        }
        
        return MapKeyToAction(keyInfo.Key);
    }
    
    /*
     * Vertaalt een fysieke toets naar een logische spelactie
     * Ondersteunt zowel de pijltjestoetsen als alternatieve WASD-bediening
     */
    private static gameAction MapKeyToAction(ConsoleKey key) =>
        key switch
        {
            ConsoleKey.LeftArrow or ConsoleKey.A => gameAction.MoveLeft,
            ConsoleKey.RightArrow or ConsoleKey.D => gameAction.MoveRight,
            ConsoleKey.DownArrow or ConsoleKey.S => gameAction.SoftDrop,
            ConsoleKey.Spacebar => gameAction.HardDrop,
            ConsoleKey.UpArrow or ConsoleKey.W => gameAction.RotateClockwise,
            ConsoleKey.Z => gameAction.RotateCounterClockwise,
            ConsoleKey.P => gameAction.TogglePause,
            ConsoleKey.Escape or ConsoleKey.Q => gameAction.Quit,
            _ => gameAction.None
        };
}