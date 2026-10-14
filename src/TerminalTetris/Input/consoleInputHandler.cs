namespace TerminalTetris.Input;

/*
 * Snelle, latency-vrije console input handler.
 * Verwerkt directe toetsaanslagen en terminal key-repeats zonder buffersperre.
 */
public class consoleInputHandler : iInputHandler
{
    public gameAction PollAction()
    {
        if (!Console.KeyAvailable)
        {
            return gameAction.None;
        }

        // Lees de oudste wachtende toets uit om geen frames te skippen
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
            ConsoleKey.C => gameAction.Hold,
            ConsoleKey.P => gameAction.TogglePause,
            ConsoleKey.Escape or ConsoleKey.Q => gameAction.Quit,
            _ => gameAction.None
        };
}