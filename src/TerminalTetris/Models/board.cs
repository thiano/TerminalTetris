using TerminalTetris.Enums;

namespace TerminalTetris.Models;

/*
 * Beheert de toestand van het speelveld (standaard 10 breed x 20 hoog).
 * Verantwoordelijk voor botsingsdetectie, het vastzetten van stukken
 * en het detecteren/verwijderen van volle rijen.
 */
public class board
{
    public const int DefaultRows = 20;
    public const int DefaultColumns = 10;

    private readonly tetrominoType?[,] _grid;

    public int Rows { get; }
    public int Columns { get; }

    /*
     * Biedt read-only indexertoegang tot een cel op positie (x, y).
     */
    public tetrominoType? this[int x, int y] => _grid[x, y];

    public board(int columns = DefaultColumns, int rows = DefaultRows)
    {
        Columns = columns;
        Rows = rows;
        _grid = new tetrominoType?[columns, rows];
    }

    /*
     * Controleert of een positie binnen de speelveldgrenzen valt en vrij is.
     */
    public bool IsPositionValid(position pos)
    {
        // Horizontaal buiten het veld
        if (pos.X < 0 || pos.X >= Columns)
        {
            return false;
        }

        // Onder de bodem
        if (pos.Y >= Rows)
        {
            return false;
        }

        // Boven het zichtbare veld (spawnen is toegestaan)
        if (pos.Y < 0)
        {
            return true;
        }

        // Controleer of de cel al bezet is
        return _grid[pos.X, pos.Y] == null;
    }

    /*
     * Controleert of alle 4 de blokken van een tetromino op geldige cellen staan.
     */
    public bool CanPlace(tetromino piece)
    {
        foreach (position pos in piece.OccupiedPositions())
        {
            if (!IsPositionValid(pos))
            {
                return false;
            }
        }
        return true;
    }

    /*
     * Zet een tetromino definitief vast op het speelveld.
     */
    public void LockPiece(tetromino piece)
    {
        foreach (position pos in piece.OccupiedPositions())
        {
            if (pos.Y >= 0 && pos.Y < Rows && pos.X >= 0 && pos.X < Columns)
            {
                _grid[pos.X, pos.Y] = piece.Type;
            }
        }
    }

    /*
     * Controleert het bord op volle rijen, verwijdert ze en laat bovenliggende rijen zakken.
     * Geeft het aantal gewiste rijen terug.
     */
    public int ClearFullRows()
    {
        int clearedCount = 0;

        for (int y = Rows - 1; y >= 0; y--)
        {
            if (IsRowFull(y))
            {
                ClearRow(y);
                DropRowsAbove(y);
                clearedCount++;
                y++; // Controleer dezelfde rij-index opnieuw omdat alles één stap gezakt is
            }
        }

        return clearedCount;
    }

    /*
     * Controleert of alle cellen in een specifieke rij bezet zijn.
     */
    private bool IsRowFull(int y)
    {
        for (int x = 0; x < Columns; x++)
        {
            if (_grid[x, y] == null)
            {
                return false;
            }
        }
        return true;
    }

    /*
     * Maakt een volledige rij leeg.
     */
    private void ClearRow(int y)
    {
        for (int x = 0; x < Columns; x++)
        {
            _grid[x, y] = null;
        }
    }

    /*
     * Laat alle rijen boven een verwijderde rij exact één stap omlaag zakken.
     */
    private void DropRowsAbove(int startY)
    {
        for (int y = startY; y > 0; y--)
        {
            for (int x = 0; x < Columns; x++)
            {
                _grid[x, y] = _grid[x, y - 1];
            }
        }

        // Bovenste rij (y = 0) leegmaken
        for (int x = 0; x < Columns; x++)
        {
            _grid[x, 0] = null;
        }
    }
}