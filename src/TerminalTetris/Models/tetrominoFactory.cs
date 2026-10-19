using TerminalTetris.Enums;
using TerminalTetris.Models.Pieces;

namespace TerminalTetris.Models;

/*
 * Factory and randomizer service for tetromino generation.
 * Implements the official Tetris Guideline '7-Bag' algorithm
 * to ensure an even distribution without drought streaks.
 */
public class tetrominoFactory(int? seed = null)
{
    private readonly Random _random = seed.HasValue ? new Random(seed.Value) : Random.Shared;
    private readonly Queue<tetrominoType> _bag = new();

    /*
     * Pulls the next tetromino from the current 7-bag and instantiates it at the spawn position.
     */
    public tetromino CreateNextPiece(position spawnPosition)
    {
        EnsureBagFilled();
        tetrominoType nextType = _bag.Dequeue();
        return CreatePieceByType(nextType, spawnPosition);
    }

    /*
     * Inspects the upcoming tetromino type without removing it from the queue.
     * Used by the renderer to display the 'Next Piece' preview panel.
     */
    public tetrominoType PeekNextType()
    {
        EnsureBagFilled();
        return _bag.Peek();
    }

    /*
     * Instantiates a concrete tetromino implementation mapped from a given type.
     * Serves both the random spawn flow and the hold-piece retrieval mechanic.
     */
    public tetromino CreatePieceByType(tetrominoType type, position spawnPosition) =>
        type switch
        {
            tetrominoType.I => new tetrominoI(spawnPosition),
            tetrominoType.J => new tetrominoJ(spawnPosition),
            tetrominoType.L => new tetrominoL(spawnPosition),
            tetrominoType.O => new tetrominoO(spawnPosition),
            tetrominoType.S => new tetrominoS(spawnPosition),
            tetrominoType.T => new tetrominoT(spawnPosition),
            tetrominoType.Z => new tetrominoZ(spawnPosition),
            _ => throw new ArgumentOutOfRangeException(nameof(type), $"Unknown tetromino type: {type}")
        };

    /*
     * Verifies remaining items and restocks a shuffled set of all 7 tetromino types when empty.
     */
    private void EnsureBagFilled()
    {
        if (_bag.Count > 0)
        {
            return;
        }

        tetrominoType[] pieces =
        [
            tetrominoType.I,
            tetrominoType.J,
            tetrominoType.L,
            tetrominoType.O,
            tetrominoType.S,
            tetrominoType.T,
            tetrominoType.Z
        ];

        // Perform in-place Fisher-Yates shuffle
        _random.Shuffle(pieces);

        foreach (tetrominoType piece in pieces)
        {
            _bag.Enqueue(piece);
        }
    }
}