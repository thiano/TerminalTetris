namespace TerminalTetris.Input;

/*
 * Contract for non-blocking player input evaluation.
 * Decouples input device implementations from the simulation loop.
 */
public interface iInputHandler
{
    /*
     * Polls the pending input stream and returns the next mapped gameAction.
     * Returns gameAction.None immediately if no keystroke is pending.
     */
    gameAction PollAction();
}