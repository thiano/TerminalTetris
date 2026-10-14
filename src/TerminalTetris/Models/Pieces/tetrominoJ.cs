using TerminalTetris.Enums;

namespace TerminalTetris.Models.Pieces;

/*
 * Concrete implementatie van het J-tetromino blok.
 */
public class tetrominoJ : tetromino
{
    private static readonly position[][] PieceTiles =
    [
        [new position(0, 0), new position(0, 1), new position(1, 1), new position(2, 1)],
        [new position(1, 0), new position(2, 0), new position(1, 1), new position(1, 2)],
        [new position(0, 1), new position(1, 1), new position(2, 1), new position(2, 2)],
        [new position(1, 0), new position(1, 1), new position(0, 2), new position(1, 2)]
    ];

    protected override position[][] Tiles => PieceTiles;

    public override tetrominoType Type => tetrominoType.J;

    public tetrominoJ(position startPosition) : base(startPosition)
    {
    }
}