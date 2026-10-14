using System.Text;
using TerminalTetris.Core;
using TerminalTetris.Enums;
using TerminalTetris.Models;

namespace TerminalTetris.Rendering;

public class ansiConsoleRenderer : iRenderer
{
    private const string AnsiHideCursor = "\x1b[?25l";
    private const string AnsiShowCursor = "\x1b[?25h";
    private const string AnsiResetCursor = "\x1b[H";
    private const string AnsiResetFormat = "\x1b[0m";

    private const int LayoutWidth = 43;
    private const int LayoutHeight = 25;

    private int _lastWindowWidth;
    private int _lastWindowHeight;

    public void Initialize()
    {
        Console.OutputEncoding = Encoding.UTF8;
        Console.CursorVisible = false;
        Console.Clear();
        Console.Write(AnsiHideCursor);

        _lastWindowWidth = Console.WindowWidth;
        _lastWindowHeight = Console.WindowHeight;
    }

    public void Cleanup()
    {
        Console.Write(AnsiShowCursor + AnsiResetFormat);
        Console.CursorVisible = true;
        Console.Clear();
    }

    public void Render(
        board board,
        tetromino? currentPiece,
        tetromino? ghostPiece,
        tetrominoType? heldPieceType,
        tetrominoFactory factory,
        scoreManager scoreManager,
        gameState state)
    {
        CheckAndHandleWindowResize();

        if (Console.WindowWidth < LayoutWidth || Console.WindowHeight < LayoutHeight)
        {
            RenderTooSmallWarning(Console.WindowWidth, Console.WindowHeight);
            return;
        }

        int windowWidth = Math.Max(Console.WindowWidth, LayoutWidth);
        int windowHeight = Math.Max(Console.WindowHeight, LayoutHeight);
        int leftPadding = Math.Max(0, (windowWidth - LayoutWidth) / 2);
        int topPadding = Math.Max(0, (windowHeight - LayoutHeight) / 2);
        string pad = new(' ', leftPadding);

        var buffer = new StringBuilder(4096);
        buffer.Append(AnsiResetCursor);

        for (int i = 0; i < topPadding; i++)
        {
            buffer.AppendLine();
        }

        buffer.Append(pad);
        buffer.AppendLine("+-----------------------------------------+");
        buffer.Append(pad);
        buffer.AppendLine("|         TERMINAL TETRIS                 |");
        buffer.Append(pad);
        buffer.AppendLine("+-----------------------------------------+");

        buffer.Append(pad);
        buffer.Append("+");
        buffer.Append(new string('-', board.Columns * 2));
        buffer.AppendLine("+   +---------------------------+");

        for (int y = 0; y < board.Rows; y++)
        {
            buffer.Append(pad);
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
                    buffer.Append("\x1b[90m::\x1b[0m");
                }
                else if (board[x, y] is { } lockedType)
                {
                    buffer.Append(GetPieceColoredBlock(lockedType));
                }
                else
                {
                    buffer.Append(" .");
                }
            }

            buffer.Append("|   | ");
            buffer.Append(GetSidebarLine(y, heldPieceType, factory, scoreManager, state));
            buffer.AppendLine(" |");
        }

        buffer.Append(pad);
        buffer.Append("+");
        buffer.Append(new string('-', board.Columns * 2));
        buffer.AppendLine("+   +---------------------------+");

        // Beknopte Engelse controls
        buffer.Append(pad);
        buffer.AppendLine("Controls: [A/D] Move | [W] Rotate | [S] Soft | [Space] Hard");
        buffer.Append(pad);
        buffer.AppendLine("          [C] Hold   | [P] Pause  | [Q/ESC] Quit");

        Console.Write(buffer.ToString());
    }

    public void AnimateLineClear(
        board board,
        List<int> fullRowIndices,
        tetrominoType? heldPieceType,
        tetrominoFactory factory,
        scoreManager scoreManager,
        gameState state)
    {
        string[] animationFrames = ["()", "><", "||", "--", "..", "  "];

        int windowWidth = Math.Max(Console.WindowWidth, LayoutWidth);
        int windowHeight = Math.Max(Console.WindowHeight, LayoutHeight);
        int leftPadding = Math.Max(0, (windowWidth - LayoutWidth) / 2);
        int topPadding = Math.Max(0, (windowHeight - LayoutHeight) / 2);
        string pad = new(' ', leftPadding);

        foreach (string frameGlyph in animationFrames)
        {
            var buffer = new StringBuilder(4096);
            buffer.Append(AnsiResetCursor);

            for (int i = 0; i < topPadding; i++)
            {
                buffer.AppendLine();
            }

            buffer.Append(pad);
            buffer.AppendLine("+-----------------------------------------+");
            buffer.Append(pad);
            buffer.AppendLine("|         TERMINAL TETRIS                 |");
            buffer.Append(pad);
            buffer.AppendLine("+-----------------------------------------+");

            buffer.Append(pad);
            buffer.Append("+");
            buffer.Append(new string('-', board.Columns * 2));
            buffer.AppendLine("+   +---------------------------+");

            for (int y = 0; y < board.Rows; y++)
            {
                buffer.Append(pad);
                buffer.Append("|");

                bool isClearingRow = fullRowIndices.Contains(y);

                for (int x = 0; x < board.Columns; x++)
                {
                    if (isClearingRow)
                    {
                        buffer.Append($"\x1b[97m{frameGlyph}\x1b[0m");
                    }
                    else if (board[x, y] is { } lockedType)
                    {
                        buffer.Append(GetPieceColoredBlock(lockedType));
                    }
                    else
                    {
                        buffer.Append(" .");
                    }
                }

                buffer.Append("|   | ");
                buffer.Append(GetSidebarLine(y, heldPieceType, factory, scoreManager, state));
                buffer.AppendLine(" |");
            }

            buffer.Append(pad);
            buffer.Append("+");
            buffer.Append(new string('-', board.Columns * 2));
            buffer.AppendLine("+   +---------------------------+");
            buffer.Append(pad);
            buffer.AppendLine("Controls: [A/D] Move | [W] Rotate | [S] Soft | [Space] Hard");
            buffer.Append(pad);
            buffer.AppendLine("          [C] Hold   | [P] Pause  | [Q/ESC] Quit");

            Console.Write(buffer.ToString());
            Thread.Sleep(35);
        }
    }

    private void CheckAndHandleWindowResize()
    {
        if (Console.WindowWidth != _lastWindowWidth || Console.WindowHeight != _lastWindowHeight)
        {
            _lastWindowWidth = Console.WindowWidth;
            _lastWindowHeight = Console.WindowHeight;
            Console.Clear();
        }
    }

    private static void RenderTooSmallWarning(int currentWidth, int currentHeight)
    {
        Console.SetCursorPosition(0, 0);
        Console.Write($"Terminal too small: {currentWidth}x{currentHeight}. Min required: {LayoutWidth}x{LayoutHeight}     ");
    }

    private static string GetSidebarLine(int row, tetrominoType? heldPieceType, tetrominoFactory factory, scoreManager scoreManager, gameState state)
    {
        string text = row switch
        {
            1 => $"Status : {FormatGameState(state)}",
            3 => $"Score  : {scoreManager.Score,-10}",
            4 => $"Lines  : {scoreManager.LinesCleared,-10}",
            5 => $"Level  : {scoreManager.Level,-10}",
            7 => "Hold   :",
            8 => heldPieceType.HasValue ? $"  >> [ {heldPieceType.Value} ]" : "  >> [ - ]",
            10 => "Next   :",
            11 => $"  >> [ {factory.PeekNextType()} ]",
            14 => "Speed up every",
            15 => "10 cleared lines!",
            _ => string.Empty
        };

        return text.PadRight(25);
    }

    private static string FormatGameState(gameState state) =>
        state switch
        {
            gameState.Playing => "\x1b[92mPLAYING\x1b[0m  ",
            gameState.Paused => "\x1b[93mPAUSED\x1b[0m   ",
            gameState.GameOver => "\x1b[91mGAME OVER\x1b[0m",
            _ => "UNKNOWN  "
        };

    private static string GetPieceColoredBlock(tetrominoType type) =>
        type switch
        {
            tetrominoType.I => "\x1b[96m[]\x1b[0m",
            tetrominoType.J => "\x1b[94m[]\x1b[0m",
            tetrominoType.L => "\x1b[38;5;208m[]\x1b[0m",
            tetrominoType.O => "\x1b[93m[]\x1b[0m",
            tetrominoType.S => "\x1b[92m[]\x1b[0m",
            tetrominoType.T => "\x1b[95m[]\x1b[0m",
            tetrominoType.Z => "\x1b[91m[]\x1b[0m",
            _ => "[]"
        };
}