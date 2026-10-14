using TerminalTetris.Enums;

namespace TerminalTetris.Models.Pieces;

/*
 * Concrete implementatie van het I-tetromino blok (4x4 bounding box).
 */
public class tetrominoI : tetromino
{
    private static readonly position[][] PieceTiles =
    [
        [new position(0, 1), new position(1, 1), new position(2, 1), new position(3, 1)], // Horizontaal
        [new position(2, 0), new position(2, 1), new position(2, 2), new position(2, 3)], // Verticaal
        [new position(0, 2), new position(1, 2), new position(2, 2), new position(3, 2)], // Horizontaal
        [new position(1, 0), new position(1, 1), new position(1, 2), new position(1, 3)]  // Verticaal
    ];

    protected override position[][] Tiles => PieceTiles;

    public override tetrominoType Type => tetrominoType.I;

    public tetrominoI(position startPosition) : base(startPosition)
    {
    }
    
    public override tetromino Clone()
    {
        var clone = new tetrominoI(Position);
        while (clone.RotationState != RotationState) clone.RotateClockwise();
        return clone;
    }
}