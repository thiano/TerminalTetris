namespace TerminalTetris.Enums;

/*
 * Represents the current operational state of the game loop.
 */
public enum gameState
{
    // Active gameplay with gravity and input processing enabled
    Playing,

    // Loop running but piece gravity and movements suspended
    Paused,

    // Game terminated; displays final score overlay and leaderboard entry
    GameOver
}