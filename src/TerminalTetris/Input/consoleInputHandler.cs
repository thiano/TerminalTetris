namespace TerminalTetris.Input;

/*
 * Low-latency console input polling provider.
 * Intercepts keyboard events directly from standard input without echo buffering.
 */
public class consoleInputHandler : iInputHandler
{
    /*
     * Polls the next queued key press from standard input and maps it to a gameAction.
     * Returns gameAction.None immediately if no input is pending.
     */
    public gameAction PollAction()
    {
        if (!Console.KeyAvailable)
        {
            return gameAction.None;
        }

        // Intercept pending keystroke to prevent standard output echo
        ConsoleKeyInfo keyInfo = Console.ReadKey(intercept: true);
        return MapKeyToAction(keyInfo.Key);
    }

    /*
     * Maps physical console key codes to high-level game action commands.
     */
    private static gameAction MapKeyToAction(ConsoleKey key) =>
        key switch
        {
            // Lateral movement (WASD and arrow keys)
            ConsoleKey.LeftArrow or ConsoleKey.A => gameAction.MoveLeft,
            ConsoleKey.RightArrow or ConsoleKey.D => gameAction.MoveRight,

            // Downward movement
            ConsoleKey.DownArrow or ConsoleKey.S => gameAction.SoftDrop,
            ConsoleKey.Spacebar => gameAction.HardDrop,

            // Rotations
            ConsoleKey.UpArrow or ConsoleKey.W => gameAction.RotateClockwise,
            ConsoleKey.Z => gameAction.RotateCounterClockwise,

            // Mechanics and state control
            ConsoleKey.C => gameAction.Hold,
            ConsoleKey.P => gameAction.TogglePause,
            ConsoleKey.Escape or ConsoleKey.Q => gameAction.Quit,

            _ => gameAction.None
        };
}