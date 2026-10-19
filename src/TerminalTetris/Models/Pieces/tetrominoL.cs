using TerminalTetris.Enums;

namespace TerminalTetris.Models.Pieces;

/*
 * Implementation of the L-tetromino (orange hook piece).
 * Rotates within a 3x3 bounding box.
 */
public class tetrominoL(position startPosition, int initialRotationState = 0)
    : tetromino(startPosition, initialRotationState)
{
    private static readonly position[][] PieceTiles =
    [
        [new position(2, 0), new position(0, 1), new position(1, 1), new position(2, 1)], // State 0
        [new position(1, 0), new position(1, 1), new position(1, 2), new position(2, 2)], // State 1
        [new position(0, 1), new position(1, 1), new position(2, 1), new position(0, 2)], // State 2
        [new position(0, 0), new position(1, 0), new position(1, 1), new position(1, 2)]  // State 3
    ];

    protected override position[][] Tiles => PieceTiles;

    public override tetrominoType Type => tetrominoType.L;

    public override tetromino Clone() => new tetrominoL(Position, RotationState);
}