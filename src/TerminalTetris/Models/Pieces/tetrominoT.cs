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

    public tetrominoT(position startPosition) : base(startPosition)
    {
    }
    
    public override tetromino Clone()
    {
        var clone = new tetrominoT(Position);
        while (clone.RotationState != RotationState) clone.RotateClockwise();
        return clone;
    }
}