using TerminalTetris.Enums;

namespace TerminalTetris.Models;

/*
 * Abstracte basisklasse voor een vallend Tetris-stuk (tetromino).
 * Beheert de ankerpositie op het speelveld, de rotatietoestand en het berekenen
 * van de absolute veldposities van de 4 individuele blokjes.
 */
public abstract class tetromino
{
    private int _rotationState;

    /*
     * Matrix met 4 rotatiestanden. Elke stand bevat een array van exact 4 relatieve tegel-offsets.
     */
    protected abstract position[][] Tiles { get; }

    /*
     * Het specifieke type van dit blok (I, J, L, O, S, T, Z).
     */
    public abstract tetrominoType Type { get; }

    /*
     * Het ankerpunt (linksboven van de bounding box) op het bord.
     */
    public position Position { get; set; }

    /*
     * De huidige rotatie-index (0, 1, 2 of 3).
     */
    public int RotationState => _rotationState;

    /*
     * Initialiseert een tetromino met een startpositie op het bord.
     */
    protected tetromino(position startPosition)
    {
        Position = startPosition;
    }

    /*
     * Berekent de absolute coördinaten van de 4 blokjes op het speelveld.
     */
    public IEnumerable<position> OccupiedPositions()
    {
        foreach (position tileOffset in Tiles[_rotationState])
        {
            yield return Position + tileOffset;
        }
    }

    /*
     * Roteert het stuk 90 graden met de klok mee.
     */
    public void RotateClockwise()
    {
        _rotationState = (_rotationState + 1) % Tiles.Length;
    }

    /*
     * Roteert het stuk 90 graden tegen de klok in.
     */
    public void RotateCounterClockwise()
    {
        _rotationState = (_rotationState == 0) ? Tiles.Length - 1 : _rotationState - 1;
    }

    /*
     * Verplaatst het ankerpunt met de opgegeven delta op de X- en Y-as.
     */
    public void Move(int deltaX, int deltaY)
    {
        Position = new position(Position.X + deltaX, Position.Y + deltaY);
    }
    
    /*
     * Maakt een exacte functionele kopie van het huidige stuk inclusief rotatie en positie.
     */
    public abstract tetromino Clone();
}