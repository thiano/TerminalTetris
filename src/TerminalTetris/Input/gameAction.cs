namespace TerminalTetris.Input;

/*
 * Definieert de abstracte acties die een speler kan uitvoeren
 */
public enum gameAction
{
    None,
    MoveLeft,
    MoveRight,
    SoftDrop,
    HardDrop,
    RotateClockwise,
    RotateCounterClockwise,
    TogglePause,
    Quit
}