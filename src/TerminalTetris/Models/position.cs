namespace TerminalTetris.Models;

/*
 * Represents a 2D integer coordinate or displacement vector on the playfield.
 * Uses a zero-based coordinate system (X: column index, Y: row index).
 */
public readonly record struct position(int X, int Y)
{
    // Origin coordinate (0, 0)
    public static readonly position Zero = new(0, 0);

    /*
     * Adds two coordinate vectors together component-wise.
     */
    public static position operator +(position a, position b) =>
        new(a.X + b.X, a.Y + b.Y);

    /*
     * Subtracts displacement vector b from coordinate vector a.
     */
    public static position operator -(position a, position b) =>
        new(a.X - b.X, a.Y - b.Y);
}