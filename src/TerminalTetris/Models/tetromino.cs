using TerminalTetris.Enums;

namespace TerminalTetris.Models;

public abstract class tetromino
{
    // Aansluitend op jouw implementatie: Tiles in plaats van Rotations
    protected abstract position[][] Tiles { get; }

    public abstract tetrominoType Type { get; }

    public position Position { get; set; }

    public int RotationState { get; set; }

    protected tetromino(position startPosition, int initialRotationState = 0)
    {
        Position = startPosition;
        RotationState = initialRotationState;
    }

    public IEnumerable<position> GetOccupiedPositions()
    {
        position[] currentOffsets = Tiles[RotationState];
        foreach (position offset in currentOffsets)
        {
            yield return new position(Position.X + offset.X, Position.Y + offset.Y);
        }
    }

    public void Move(int deltaX, int deltaY)
    {
        Position = new position(Position.X + deltaX, Position.Y + deltaY);
    }

    public void RotateClockwise()
    {
        RotationState = (RotationState + 1) % Tiles.Length;
    }

    public void RotateCounterClockwise()
    {
        RotationState = (RotationState + Tiles.Length - 1) % Tiles.Length;
    }

    public abstract tetromino Clone();
}