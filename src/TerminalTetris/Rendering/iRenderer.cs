using TerminalTetris.Core;
using TerminalTetris.Enums;
using TerminalTetris.Models;

namespace TerminalTetris.Rendering;

public interface iRenderer
{
    void Initialize();
    void Cleanup();
    void Render(
        board board,
        tetromino? currentPiece,
        tetromino? ghostPiece,
        tetrominoType? heldPieceType,
        tetrominoFactory factory,
        scoreManager scoreManager,
        gameState state);

    void AnimateLineClear(
        board board,
        List<int> fullRowIndices,
        tetrominoType? heldPieceType,
        tetrominoFactory factory,
        scoreManager scoreManager,
        gameState state);
    
    /*
     * Speelt een flitsende arcade TETRIS splash-animatie af in het midden van het speelveld
     * wanneer de speler 4 rijen tegelijk wist.
     */
    void AnimateTetrisSplash(
        board board,
        tetrominoType? heldPieceType,
        tetrominoFactory factory,
        scoreManager scoreManager,
        gameState state);
}

