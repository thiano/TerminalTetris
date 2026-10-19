namespace TerminalTetris.Input;

/*
 * Represents discrete high-level player inputs and system commands.
 * Decouples raw hardware input events from the gameplay simulation engine.
 */
public enum gameAction
{
    // Default idle state when no input is polled
    None,

    // Lateral translation
    MoveLeft,
    MoveRight,

    // Vertical translation
    SoftDrop,
    HardDrop,

    // Piece rotations
    RotateClockwise,
    RotateCounterClockwise,

    // Inventory and hold mechanism
    Hold,

    // State flow control
    TogglePause,
    Quit
}