using TerminalTetris.Core;
using TerminalTetris.Enums;
using TerminalTetris.Models;

namespace TerminalTetris.Rendering;

/*
 * Visual rendering contract for terminal output.
 * Decouples game loop state presentation, animations, and arcade overlays.
 */
public interface iRenderer
{
    /*
     * Sets up output encoding, adjusts terminal cursor visibility, and resets screen buffers.
     */
    void Initialize();

    /*
     * Restores default terminal attributes and hardware cursor state before application exit.
     */
    void Cleanup();

    /*
     * Renders an active frame including the playfield grid, falling piece, ghost projection, and sidebar stats.
     */
    void Render(
        board board,
        tetromino? currentPiece,
        tetromino? ghostPiece,
        tetrominoType? heldPieceType,
        tetrominoFactory factory,
        scoreManager scoreManager,
        gameState state);

    /*
     * Plays a flashing line-clear animation across completed rows before they collapse.
     */
    void AnimateLineClear(
        board board,
        List<int> fullRowIndices,
        tetrominoType? heldPieceType,
        tetrominoFactory factory,
        scoreManager scoreManager,
        gameState state);

    /*
     * Triggers a colorful arcade banner celebrating a 4-line clear (Tetris clear).
     */
    void AnimateTetrisSplash(
        board board,
        tetrominoType? heldPieceType,
        tetrominoFactory factory,
        scoreManager scoreManager,
        gameState state);

    /*
     * Renders the compact Game Over box containing the session score and Top 5 leaderboard.
     */
    void RenderGameOverScreen(int finalScore, bool isNewHighScore, List<highScoreEntry> topScores);

    /*
     * Displays an interactive prompt requesting 3 uppercase arcade initials from the player.
     */
    string PromptPlayerInitials();

    /*
     * Plays a vertical curtain sweep animation wiping the board blocks on game termination.
     */
    void AnimateGameOverCurtain(
        board board,
        tetrominoType? heldPieceType,
        tetrominoFactory factory,
        scoreManager scoreManager,
        gameState state);
}