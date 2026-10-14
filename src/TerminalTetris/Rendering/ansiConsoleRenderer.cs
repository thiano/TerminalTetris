using System.Text;
using TerminalTetris.Core;
using TerminalTetris.Enums;
using TerminalTetris.Models;

namespace TerminalTetris.Rendering;

/*
 * Rendert het speelveld met pure ASCII-karakters (toetsenbord-only).
 * Gebruikt ANSI escape codes voor positionering en kleurcodering zonder console flicker.
 */
public class ansiConsoleRenderer : iRenderer
{
    private const string AnsiHideCursor = "\x1b[?25l";
    private const string AnsiShowCursor = "\x1b[?25h";
    private const string AnsiResetCursor = "\x1b[H";
    private const string AnsiResetFormat = "\x1b[0m";

    public void Initialize()
    {
        Console.OutputEncoding = Encoding.UTF8;
        Console.CursorVisible = false;
        Console.Clear();
        Console.Write(AnsiHideCursor);
    }

    public void Cleanup()
    {
        Console.Write(AnsiShowCursor + AnsiResetFormat);
        Console.CursorVisible = true;
        Console.SetCursorPosition(0, board.DefaultRows + 6);
    }

    public void Render(
        board board,
        tetromino? currentPiece,
        tetromino? ghostPiece,
        tetrominoFactory factory,
        scoreManager scoreManager,
        gameState state)
    {
        var buffer = new StringBuilder(2048);

        buffer.Append(AnsiResetCursor);

        // Header
        buffer.AppendLine("+-----------------------------------------+");
        buffer.AppendLine("|         TERMINAL TETRIS (.NET)          |");
        buffer.AppendLine("+-----------------------------------------+");

        // Bovenrand speelveld en zijpaneel
        buffer.Append("+");
        buffer.Append(new string('-', board.Columns * 2));
        buffer.AppendLine("+   +---------------------------+");

        // Speelveld rijen
        for (int y = 0; y < board.Rows; y++)
        {
            buffer.Append("|");

            for (int x = 0; x < board.Columns; x++)
            {
                var pos = new position(x, y);

                if (currentPiece != null && currentPiece.OccupiedPositions().Contains(pos))
                {
                    buffer.Append(GetPieceColoredBlock(currentPiece.Type));
                }
                else if (ghostPiece != null && ghostPiece.OccupiedPositions().Contains(pos))
                {
                    buffer.Append("\x1b[90m::\x1b[0m"); // Schaduw / projectie met dubbele dubbele punt
                }
                else if (board[x, y] is { } lockedType)
                {
                    buffer.Append(GetPieceColoredBlock(lockedType));
                }
                else
                {
                    buffer.Append(" ."); // Lege cel
                }
            }

            buffer.Append("|   | ");
            buffer.Append(GetSidebarLine(y, factory, scoreManager, state));
            buffer.AppendLine(" |");
        }

        // Onderrand
        buffer.Append("+");
        buffer.Append(new string('-', board.Columns * 2));
        buffer.AppendLine("+   +---------------------------+");

        // Instructies
        buffer.AppendLine("Controls: [A/D] Links/Rechts | [W] Roteer | [S] Val | [Spatie] Drop");
        buffer.AppendLine("          [P] Pauze | [Q/ESC] Afsluiten");

        Console.Write(buffer.ToString());
    }

    private static string GetSidebarLine(int row, tetrominoFactory factory, scoreManager scoreManager, gameState state)
    {
        string text = row switch
        {
            1 => $"Status: {FormatGameState(state)}",
            3 => $"Score : {scoreManager.Score,-10}",
            4 => $"Lines : {scoreManager.LinesCleared,-10}",
            5 => $"Level : {scoreManager.Level,-10}",
            7 => "Volgend stuk:",
            8 => $"  >> [ {factory.PeekNextType()} ]",
            12 => "Snelheid stijgt per",
            13 => "10 gewiste lijnen!",
            _ => string.Empty
        };

        return text.PadRight(25);
    }

    private static string FormatGameState(gameState state) =>
        state switch
        {
            gameState.Playing => "\x1b[92mSPEELT\x1b[0m   ",
            gameState.Paused => "\x1b[93mPAUZE\x1b[0m    ",
            gameState.GameOver => "\x1b[91mGAME OVER\x1b[0m",
            _ => "UNKNOWN  "
        };

    /*
     * Geeft een gekleurd ASCII-blokje terug: [#] of []
     */
    private static string GetPieceColoredBlock(tetrominoType type) =>
        type switch
        {
            tetrominoType.I => "\x1b[96m[]\x1b[0m", // Cyaan
            tetrominoType.J => "\x1b[94m[]\x1b[0m", // Blauw
            tetrominoType.L => "\x1b[38;5;208m[]\x1b[0m", // Oranje
            tetrominoType.O => "\x1b[93m[]\x1b[0m", // Geel
            tetrominoType.S => "\x1b[92m[]\x1b[0m", // Groen
            tetrominoType.T => "\x1b[95m[]\x1b[0m", // Magenta
            tetrominoType.Z => "\x1b[91m[]\x1b[0m", // Rood
            _ => "[]"
        };
}