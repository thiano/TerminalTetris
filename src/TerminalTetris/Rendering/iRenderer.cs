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

    void AnimateTetrisSplash(
        board board,
        tetrominoType? heldPieceType,
        tetrominoFactory factory,
        scoreManager scoreManager,
        gameState state);

    // Nieuwe methodes voor Game Over & High Score:
    void RenderGameOverScreen(int finalScore, bool isNewHighScore, List<highScoreEntry> topScores);
    string PromptPlayerInitials();
    
    void AnimateGameOverCurtain(board board, tetrominoType? heldPieceType, tetrominoFactory factory, scoreManager scoreManager, gameState state); 
}