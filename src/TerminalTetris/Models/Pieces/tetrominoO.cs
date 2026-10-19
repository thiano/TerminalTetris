using TerminalTetris.Enums;

namespace TerminalTetris.Models.Pieces;

/*
 * Implementation of the O-tetromino (yellow square piece).
 * Positioned in a 2x2 grid with invariant tile coordinates across all rotation states.
 */
public class tetrominoO(position startPosition, int initialRotationState = 0)
    : tetromino(startPosition, initialRotationState)
{
    private static readonly position[][] PieceTiles =
    [
        [new position(0, 0), new position(1, 0), new position(0, 1), new position(1, 1)], // State 0
        [new position(0, 0), new position(1, 0), new position(0, 1), new position(1, 1)], // State 1
        [new position(0, 0), new position(1, 0), new position(0, 1), new position(1, 1)], // State 2
        [new position(0, 0), new position(1, 0), new position(0, 1), new position(1, 1)]  // State 3
    ];

    protected override position[][] Tiles => PieceTiles;

    public override tetrominoType Type => tetrominoType.O;

    public override tetromino Clone() => new tetrominoO(Position, RotationState);
}