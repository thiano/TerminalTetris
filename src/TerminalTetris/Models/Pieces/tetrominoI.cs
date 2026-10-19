using TerminalTetris.Enums;

namespace TerminalTetris.Models.Pieces;

/*
 * Implementation of the I-tetromino (cyan straight line piece).
 * Rotates around the center of a 4x4 bounding box per standard SRS rules.
 */
public class tetrominoI(position startPosition, int initialRotationState = 0)
    : tetromino(startPosition, initialRotationState)
{
    private static readonly position[][] PieceTiles =
    [
        [new position(0, 1), new position(1, 1), new position(2, 1), new position(3, 1)], // State 0 (Horizontal)
        [new position(2, 0), new position(2, 1), new position(2, 2), new position(2, 3)], // State 1 (Vertical)
        [new position(0, 2), new position(1, 2), new position(2, 2), new position(3, 2)], // State 2 (Horizontal)
        [new position(1, 0), new position(1, 1), new position(1, 2), new position(1, 3)]  // State 3 (Vertical)
    ];

    protected override position[][] Tiles => PieceTiles;

    public override tetrominoType Type => tetrominoType.I;

    public override tetromino Clone() => new tetrominoI(Position, RotationState);
}