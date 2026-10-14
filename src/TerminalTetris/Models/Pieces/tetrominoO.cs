using TerminalTetris.Enums;

namespace TerminalTetris.Models.Pieces;

/*
 * Concrete implementatie van het O-tetromino blok (2x2 vierkant).
 */
public class tetrominoO : tetromino
{
    private static readonly position[][] PieceTiles =
    [
        [new position(0, 0), new position(1, 0), new position(0, 1), new position(1, 1)],
        [new position(0, 0), new position(1, 0), new position(0, 1), new position(1, 1)],
        [new position(0, 0), new position(1, 0), new position(0, 1), new position(1, 1)],
        [new position(0, 0), new position(1, 0), new position(0, 1), new position(1, 1)]
    ];

    protected override position[][] Tiles => PieceTiles;

    public override tetrominoType Type => tetrominoType.O;

    public tetrominoO(position startPosition) : base(startPosition)
    {
    }
    
    public override tetromino Clone()
    {
        var clone = new tetrominoO(Position);
        while (clone.RotationState != RotationState) clone.RotateClockwise();
        return clone;
    }
}