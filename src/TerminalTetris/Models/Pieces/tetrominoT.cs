using TerminalTetris.Enums;

namespace TerminalTetris.Models.Pieces;

/*
 * Concrete implementatie van het T-tetromino blok.
 */
public class tetrominoT : tetromino
{
    private static readonly position[][] PieceTiles =
    [
        [new position(1, 0), new position(0, 1), new position(1, 1), new position(2, 1)], // Stand 0
        [new position(1, 0), new position(1, 1), new position(2, 1), new position(1, 2)], // Stand 1
        [new position(0, 1), new position(1, 1), new position(2, 1), new position(1, 2)], // Stand 2
        [new position(1, 0), new position(0, 1), new position(1, 1), new position(1, 2)]  // Stand 3
    ];

    protected override position[][] Tiles => PieceTiles;

    public override tetrominoType Type => tetrominoType.T;

    public tetrominoT(position startPosition, int initialRotationState = 0) 
        : base(startPosition, initialRotationState)
    {
    }

    public override tetromino Clone()
    {
        return new tetrominoT(Position, RotationState);
    }
}