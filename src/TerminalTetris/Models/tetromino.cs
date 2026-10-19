using TerminalTetris.Enums;

namespace TerminalTetris.Models;

/*
 * Abstract base representation of a 4-block tetromino piece.
 * Manages spatial coordinates, rotation states, and tile grid transformations.
 */
public abstract class tetromino(position startPosition, int initialRotationState = 0)
{
    // Relative local block coordinates mapped per rotation orientation index
    protected abstract position[][] Tiles { get; }

    // Geometric piece classification
    public abstract tetrominoType Type { get; }

    // Absolute top-left pivot coordinate within the playfield grid
    public position Position { get; set; } = startPosition;

    // Current clockwise rotation step index (0 to 3)
    public int RotationState { get; set; } = initialRotationState;

    /*
     * Calculates absolute world grid positions for all 4 component blocks.
     */
    public IEnumerable<position> GetOccupiedPositions()
    {
        position[] currentOffsets = Tiles[RotationState];
        foreach (position offset in currentOffsets)
        {
            yield return Position + offset;
        }
    }

    /*
     * Translates piece position by relative delta coordinates.
     */
    public void Move(int deltaX, int deltaY)
    {
        Position += new position(deltaX, deltaY);
    }

    /*
     * Increments piece orientation state by 90 degrees clockwise.
     */
    public void RotateClockwise()
    {
        RotationState = (RotationState + 1) % Tiles.Length;
    }

    /*
     * Decrements piece orientation state by 90 degrees counter-clockwise.
     */
    public void RotateCounterClockwise()
    {
        RotationState = (RotationState + Tiles.Length - 1) % Tiles.Length;
    }

    /*
     * Creates an independent deep clone preserving current coordinates and rotation.
     */
    public abstract tetromino Clone();
}