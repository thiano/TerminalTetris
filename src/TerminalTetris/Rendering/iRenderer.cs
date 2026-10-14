using TerminalTetris.Core;
using TerminalTetris.Models;

namespace TerminalTetris.Rendering;

/*
 * Contract voor weergave-implementaties.
 * Ontkoppelt de visuele presentatie van de logische spelstatus.
 */
public interface iRenderer
{
    /*
     * Initialiseert het consolescherm (cursor verbergen, window instellen).
     */
    void Initialize();

    /*
     * Herstelt de console-instellingen bij afsluiten van het spel.
     */
    void Cleanup();

    /*
     * Rendert de complete speltoestand: het speelveld, het actieve stuk,
     * de ghost piece preview, score, level en het volgende stuk.
     */
    void Render(board board, tetromino? currentPiece, tetromino? ghostPiece, tetrominoFactory factory, scoreManager scoreManager, gameState state);
}