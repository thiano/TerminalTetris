namespace TerminalTetris.Models;

/*
 * Representeert een tweedimensionale positie of verplaatsingsvector op het speelveld.
 * X = horizontale kolomindex (0-based)
 * Y = verticale rij-index (0-based)
 */
public readonly record struct position(int X, int Y)
{
    /*
     * Telt twee posities vectorieel bij elkaar op.
     */
    public static position operator +(position a, position b) =>
        new(a.X + b.X, a.Y + b.Y);

    /*
     * Trekt een vector af van een positie.
     */
    public static position operator -(position a, position b) =>
        new(a.X - b.X, a.Y - b.Y);
}