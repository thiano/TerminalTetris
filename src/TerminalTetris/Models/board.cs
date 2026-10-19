using TerminalTetris.Enums;

namespace TerminalTetris.Models;

/*
 * Manages the playfield grid state (standard 10 columns by 20 rows).
 * Handles collision detection, piece placement locking, and full line clearing.
 */
public class board(int columns = board.DefaultColumns, int rows = board.DefaultRows)
{
    public const int DefaultRows = 20;
    public const int DefaultColumns = 10;

    private readonly tetrominoType?[,] _grid = new tetrominoType?[columns, rows];

    public int Rows { get; } = rows;
    public int Columns { get; } = columns;

    /*
     * Provides read-only indexer access to the cell at coordinate (x, y).
     */
    public tetrominoType? this[int x, int y] => _grid[x, y];

    /*
     * Validates whether a coordinate is within boundary constraints and unoccupied.
     */
    public bool IsPositionValid(position pos)
    {
        // Horizontal bounds check
        if (pos.X < 0 || pos.X >= Columns)
        {
            return false;
        }

        // Floor boundary check
        if (pos.Y >= Rows)
        {
            return false;
        }

        // Sky/spawn area above visible playfield is considered valid
        if (pos.Y < 0)
        {
            return true;
        }

        // Occupancy check for locked cells
        return _grid[pos.X, pos.Y] == null;
    }

    /*
     * Verifies that all 4 occupied cells of a tetromino can be positioned legally.
     */
    public bool CanPlace(tetromino piece)
    {
        foreach (position pos in piece.GetOccupiedPositions())
        {
            if (!IsPositionValid(pos))
            {
                return false;
            }
        }

        return true;
    }

    /*
     * Permanently commits piece blocks into the grid matrix.
     */
    public void LockPiece(tetromino piece)
    {
        foreach (position pos in piece.GetOccupiedPositions())
        {
            if (pos.Y >= 0 && pos.Y < Rows && pos.X >= 0 && pos.X < Columns)
            {
                _grid[pos.X, pos.Y] = piece.Type;
            }
        }
    }

    /*
     * Returns a list of all Y-row indices that are currently completely filled.
     */
    public List<int> GetFullRowIndices()
    {
        var fullRows = new List<int>();

        for (int y = 0; y < Rows; y++)
        {
            if (IsRowFull(y))
            {
                fullRows.Add(y);
            }
        }

        return fullRows;
    }

    /*
     * Clears specific rows and collapses higher rows downward.
     * Iterates bottom-up (descending) to prevent row index shifts during clearance.
     */
    public void ClearSpecificRows(IEnumerable<int> rowsToClear)
    {
        foreach (int y in rowsToClear.OrderByDescending(r => r))
        {
            ClearRow(y);
            DropRowsAbove(y);
        }
    }

    /*
     * Checks if all cells across a horizontal row contain locked blocks.
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
     * Clears all cells on a given row coordinate.
     */
    private void ClearRow(int y)
    {
        for (int x = 0; x < Columns; x++)
        {
            _grid[x, y] = null;
        }
    }

    /*
     * Shifts all rows above the cleared row downward by one cell.
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

        // Clear top-most row
        for (int x = 0; x < Columns; x++)
        {
            _grid[x, 0] = null;
        }
    }
}