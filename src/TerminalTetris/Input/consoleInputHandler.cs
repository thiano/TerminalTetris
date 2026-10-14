namespace TerminalTetris.Input;

/*
 * Concrete implementatie van iInputHandler die de System.Console-invoerbuffer uitleest.
 * Laat herhaalde toetsaanslagen soepel door voor vloeiende besturing bij het ingedrukt houden.
 */
public class consoleInputHandler : iInputHandler
{
    public gameAction PollAction()
    {
        if (!Console.KeyAvailable)
        {
            return gameAction.None;
        }

        ConsoleKeyInfo keyInfo = Console.ReadKey(intercept: true);
        return MapKeyToAction(keyInfo.Key);
    }

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