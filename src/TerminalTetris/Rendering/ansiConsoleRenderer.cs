using System.Text;
using TerminalTetris.Core;
using TerminalTetris.Enums;
using TerminalTetris.Models;

namespace TerminalTetris.Rendering;

/*
 * ANSI terminal console renderer.
 * Handles double-buffered layout rendering, piece previews, animations, and high score screens.
 */
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

    /*
     * Configures the console encoding, hides the hardware cursor, and clears the buffer.
     */
    public void Initialize()
    {
        Console.OutputEncoding = Encoding.UTF8;
        Console.CursorVisible = false;
        Console.Clear();
        Console.Write(AnsiHideCursor);

        _lastWindowWidth = Console.WindowWidth;
        _lastWindowHeight = Console.WindowHeight;
    }

    /*
     * Restores terminal cursor visibility and default character attributes upon exit.
     */
    public void Cleanup()
    {
        Console.Write(AnsiShowCursor + AnsiResetFormat);
        Console.CursorVisible = true;
        Console.Clear();
    }

    /*
     * Renders the complete game interface including playfield, ghost piece, and sidebar stats.
     */
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

        // Precompute occupied coordinates to avoid repeated enumeration in the 200-cell loop
        var currentPositions = currentPiece?.GetOccupiedPositions().ToArray() ?? [];
        var ghostPositions = ghostPiece?.GetOccupiedPositions().ToArray() ?? [];

        var buffer = new StringBuilder(4096);
        buffer.Append(AnsiResetCursor);

        for (int i = 0; i < topPadding; i++)
        {
            buffer.AppendLine();
        }

        buffer.Append(pad);
        buffer.AppendLine("+---------------------+ +------------------+");
        buffer.Append(pad);
        buffer.AppendLine("|     T E T R I S     | |      STATS       |");
        buffer.Append(pad);
        buffer.AppendLine("+---------------------+ +------------------+");

        for (int y = 0; y < board.Rows; y++)
        {
            buffer.Append(pad);
            buffer.Append('|');

            for (int x = 0; x < board.Columns; x++)
            {
                var pos = new position(x, y);

                if (currentPiece != null && currentPositions.Contains(pos))
                {
                    buffer.Append(GetPieceColoredBlock(currentPiece.Type));
                }
                else if (ghostPiece != null && ghostPositions.Contains(pos))
                {
                    buffer.Append("\x1b[90m::\x1b[0m");
                }
                else if (board[x, y] is { } lockedType)
                {
                    buffer.Append(GetPieceColoredBlock(lockedType));
                }
                else
                {
                    buffer.Append("\x1b[90m .\x1b[0m");
                }
            }

            buffer.Append("| | ");
            buffer.Append(GetSidebarLine(y, heldPieceType, factory.PeekNextType(), scoreManager, state));
            buffer.AppendLine(" |");
        }

        buffer.Append(pad);
        buffer.AppendLine("+---------------------+ +------------------+");
        buffer.Append(pad);
        buffer.AppendLine(" [A/D] Move  [W] Rotate  [S] Soft  [Space] Drop ");
        buffer.Append(pad);
        buffer.AppendLine(" [C]   Hold  [P] Pause   [Q] Quit                ");

        Console.Write(buffer.ToString());
    }

    /*
     * Displays a flash animation sequence when rows are cleared.
     */
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
            buffer.AppendLine("+---------------------+ +------------------+");
            buffer.Append(pad);
            buffer.AppendLine("|     T E T R I S     | |      STATS       |");
            buffer.Append(pad);
            buffer.AppendLine("+---------------------+ +------------------+");

            for (int y = 0; y < board.Rows; y++)
            {
                buffer.Append(pad);
                buffer.Append('|');

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
                        buffer.Append("\x1b[90m .\x1b[0m");
                    }
                }

                buffer.Append("| | ");
                buffer.Append(GetSidebarLine(y, heldPieceType, factory.PeekNextType(), scoreManager, state));
                buffer.AppendLine(" |");
            }

            buffer.Append(pad);
            buffer.AppendLine("+---------------------+ +------------------+");
            buffer.Append(pad);
            buffer.AppendLine(" [A/D] Move  [W] Rotate  [S] Soft  [Space] Drop ");
            buffer.Append(pad);
            buffer.AppendLine(" [C]   Hold  [P] Pause   [Q] Quit                ");

            Console.Write(buffer.ToString());
            Thread.Sleep(35);
        }
    }

    /*
     * Re-clears terminal buffer if dimensions changed during execution.
     */
    private void CheckAndHandleWindowResize()
    {
        if (Console.WindowWidth != _lastWindowWidth || Console.WindowHeight != _lastWindowHeight)
        {
            _lastWindowWidth = Console.WindowWidth;
            _lastWindowHeight = Console.WindowHeight;
            Console.Clear();
        }
    }

    /*
     * Displays a boundary warning message if the terminal window falls below the minimum grid layout.
     */
    private static void RenderTooSmallWarning(int currentWidth, int currentHeight)
    {
        Console.SetCursorPosition(0, 0);
        Console.Write($"Terminal size: {currentWidth}x{currentHeight}. Minimum required: {LayoutWidth}x{LayoutHeight}  ");
    }

    /*
     * Constructs row-specific information for the right-hand statistics and preview panel.
     * Guarantees an exact visible width of 18 characters.
     */
    private static string GetSidebarLine(int row, tetrominoType? held, tetrominoType next, scoreManager score, gameState state)
    {
        string text = row switch
        {
            0 => $"SCORE  {score.Score,11}",
            1 => $"LINES  {score.LinesCleared,11}",
            2 => $"LEVEL  {score.Level,11}",
            3 => "------------------",
            4 => "HOLD:",
            5 => $"  {GetMiniPieceRow(held, 0)}",
            6 => $"  {GetMiniPieceRow(held, 1)}",
            7 => "------------------",
            8 => "NEXT:",
            9 => $"  {GetMiniPieceRow(next, 0)}",
            10 => $"  {GetMiniPieceRow(next, 1)}",
            11 => "------------------",
            13 => $"STATE  {FormatGameState(state)}",
            _ => string.Empty
        };

        int visible = GetVisibleLength(text);
        int padNeeded = Math.Max(0, 18 - visible);
        return text + new string(' ', padNeeded);
    }

    /*
     * Formats 2-row mini matrix representations for the hold and next piece preview slots.
     */
    private static string GetMiniPieceRow(tetrominoType? type, int row)
    {
        if (!type.HasValue) return "        ";

        string color = GetAnsiColor(type.Value);
        string reset = AnsiResetFormat;

        return (type.Value, row) switch
        {
            (tetrominoType.I, 0) => $"{color}[][][][]{reset}",
            (tetrominoType.I, 1) => "        ",

            (tetrominoType.O, 0) => $"  {color}[][]{reset}  ",
            (tetrominoType.O, 1) => $"  {color}[][]{reset}  ",

            (tetrominoType.T, 0) => $"  {color}[][][]{reset}",
            (tetrominoType.T, 1) => $"    {color}[]{reset}  ",

            (tetrominoType.L, 0) => $"  {color}[][][]{reset}",
            (tetrominoType.L, 1) => $"  {color}[]{reset}    ",

            (tetrominoType.J, 0) => $"  {color}[][][]{reset}",
            (tetrominoType.J, 1) => $"      {color}[]{reset}",

            (tetrominoType.S, 0) => $"    {color}[][]{reset}",
            (tetrominoType.S, 1) => $"  {color}[][]{reset}  ",

            (tetrominoType.Z, 0) => $"  {color}[][]{reset}  ",
            (tetrominoType.Z, 1) => $"    {color}[][]{reset}",

            _ => "        "
        };
    }

    /*
     * Formats ANSI-colored status tags for the active game engine state.
     */
    private static string FormatGameState(gameState state) =>
        state switch
        {
            gameState.Playing => "\x1b[92mPLAY\x1b[0m     ",
            gameState.Paused => "\x1b[93mPAUSE\x1b[0m    ",
            gameState.GameOver => "\x1b[91mOVER\x1b[0m     ",
            _ => "UNKNOWN  "
        };

    /*
     * Resolves the official standard ANSI 256/16 color code for each tetromino geometry.
     */
    private static string GetAnsiColor(tetrominoType type) =>
        type switch
        {
            tetrominoType.I => "\x1b[96m",       // Bright Cyan
            tetrominoType.J => "\x1b[94m",       // Bright Blue
            tetrominoType.L => "\x1b[38;5;208m", // Vibrant Orange
            tetrominoType.O => "\x1b[93m",       // Bright Yellow
            tetrominoType.S => "\x1b[92m",       // Bright Green
            tetrominoType.T => "\x1b[95m",       // Bright Magenta
            tetrominoType.Z => "\x1b[91m",       // Bright Red
            _ => "\x1b[0m"
        };

    private static string GetPieceColoredBlock(tetrominoType type) =>
        $"{GetAnsiColor(type)}[]{AnsiResetFormat}";

    /*
     * Plays an arcade splash effect celebrating a 4-line Tetris clear.
     */
    public void AnimateTetrisSplash(
        board board,
        tetrominoType? heldPieceType,
        tetrominoFactory factory,
        scoreManager scoreManager,
        gameState state)
    {
        int windowWidth = Math.Max(Console.WindowWidth, LayoutWidth);
        int windowHeight = Math.Max(Console.WindowHeight, LayoutHeight);
        int leftPadding = Math.Max(0, (windowWidth - LayoutWidth) / 2);
        int topPadding = Math.Max(0, (windowHeight - LayoutHeight) / 2);
        string pad = new(' ', leftPadding);

        string[] tetrisColors = [
            "\x1b[96m",
            "\x1b[93m",
            "\x1b[95m",
            "\x1b[92m",
            "\x1b[38;5;208m",
            "\x1b[91m"
        ];

        for (int frame = 0; frame < 8; frame++)
        {
            var buffer = new StringBuilder(4096);
            buffer.Append(AnsiResetCursor);

            for (int i = 0; i < topPadding; i++)
            {
                buffer.AppendLine();
            }

            string outerBorderCol = tetrisColors[frame % tetrisColors.Length];

            buffer.Append(pad);
            buffer.AppendLine($"{outerBorderCol}+---------------------+ +------------------+{AnsiResetFormat}");
            buffer.Append(pad);
            buffer.AppendLine($"{outerBorderCol}|{AnsiResetFormat}     \x1b[97m* * * * * *\x1b[0m     {outerBorderCol}| |{AnsiResetFormat}      STATS       {outerBorderCol}|{AnsiResetFormat}");
            buffer.Append(pad);
            buffer.AppendLine($"{outerBorderCol}+---------------------+ +------------------+{AnsiResetFormat}");

            string c0 = tetrisColors[(frame + 0) % 6];
            string c1 = tetrisColors[(frame + 1) % 6];
            string c2 = tetrisColors[(frame + 2) % 6];
            string c3 = tetrisColors[(frame + 3) % 6];
            string c4 = tetrisColors[(frame + 4) % 6];
            string c5 = tetrisColors[(frame + 5) % 6];

            string bannerBorder = (frame % 2 == 0) ? "\x1b[97m" : "\x1b[93m";

            for (int y = 0; y < board.Rows; y++)
            {
                buffer.Append(pad);
                buffer.Append($"{outerBorderCol}|{AnsiResetFormat}");

                if (y == 6)
                {
                    buffer.Append($"{bannerBorder}+------------------+{AnsiResetFormat}");
                }
                else if (y == 7)
                {
                    buffer.Append($"{bannerBorder}|{AnsiResetFormat}  {c0}* *{AnsiResetFormat}  \x1b[97mPERFECT\x1b[0m  {c3}* *{AnsiResetFormat} {bannerBorder}|{AnsiResetFormat}");
                }
                else if (y == 8)
                {
                    buffer.Append($"{bannerBorder}|{AnsiResetFormat} {c0}TT{AnsiResetFormat} {c1}EE{AnsiResetFormat} {c2}TT{AnsiResetFormat} {c3}RR{AnsiResetFormat} {c4}II{AnsiResetFormat} {c5}SS{AnsiResetFormat} {bannerBorder}|{AnsiResetFormat}");
                }
                else if (y == 9)
                {
                    buffer.Append($"{bannerBorder}|{AnsiResetFormat} {c0}##{AnsiResetFormat} {c1}##{AnsiResetFormat} {c2}##{AnsiResetFormat} {c3}##{AnsiResetFormat} {c4}##{AnsiResetFormat} {c5}##{AnsiResetFormat} {bannerBorder}|{AnsiResetFormat}");
                }
                else if (y == 10)
                {
                    buffer.Append($"{bannerBorder}|{AnsiResetFormat}   \x1b[93m>>> 800 PTS <<<\x1b[0m  {bannerBorder}|{AnsiResetFormat}");
                }
                else if (y == 11)
                {
                    buffer.Append($"{bannerBorder}+------------------+{AnsiResetFormat}");
                }
                else
                {
                    for (int x = 0; x < board.Columns; x++)
                    {
                        if (board[x, y] is { } lockedType)
                        {
                            buffer.Append(GetPieceColoredBlock(lockedType));
                        }
                        else
                        {
                            buffer.Append("\x1b[90m .\x1b[0m");
                        }
                    }
                }

                buffer.Append($"{outerBorderCol}| |{AnsiResetFormat} ");
                buffer.Append(GetSidebarLine(y, heldPieceType, factory.PeekNextType(), scoreManager, state));
                buffer.AppendLine($" {outerBorderCol}|{AnsiResetFormat}");
            }

            buffer.Append(pad);
            buffer.AppendLine($"{outerBorderCol}+---------------------+ +------------------+{AnsiResetFormat}");
            buffer.Append(pad);
            buffer.AppendLine(" [A/D] Move  [W] Rotate  [S] Soft  [Space] Drop ");
            buffer.Append(pad);
            buffer.AppendLine(" [C]   Hold  [P] Pause   [Q] Quit                ");

            Console.Write(buffer.ToString());
            Thread.Sleep(75);
        }
    }

    /*
     * Renders the compact Game Over box and Top 5 leaderboard.
     * Box width is fixed at 43 columns with 41 inner printable characters.
     */
    public void RenderGameOverScreen(int finalScore, bool isNewHighScore, List<highScoreEntry> topScores)
    {
        Console.Clear();
        Console.Write(AnsiResetCursor);

        int windowWidth = Math.Max(Console.WindowWidth, LayoutWidth);
        int windowHeight = Math.Max(Console.WindowHeight, LayoutHeight);
        int leftPadding = Math.Max(0, (windowWidth - LayoutWidth) / 2);
        int topPadding = Math.Min(4, Math.Max(0, (windowHeight - 14) / 2));
        string pad = new(' ', leftPadding);

        const int innerWidth = 41;
        const string horizontalBorder = "+-----------------------------------------+";
        const string dividerLine = "|-----------------------------------------|";

        var buffer = new StringBuilder(2048);
        buffer.Append(AnsiResetCursor);

        for (int i = 0; i < topPadding; i++)
        {
            buffer.AppendLine();
        }

        buffer.Append(pad).AppendLine(horizontalBorder);

        // 11 spaces + 19 chars banner + 11 spaces = 41 chars (New Record)
        // 16 spaces + 9 chars + 16 spaces = 41 chars (Game Over)
        string titleContent = isNewHighScore
            ? "           \x1b[93m★ NEW HIGH SCORE! ★\x1b[0m           "
            : "                \x1b[91mGAME OVER\x1b[0m                ";
        buffer.Append(pad).AppendLine(FormatBoxRow(titleContent, innerWidth));

        string scoreContent = $" SCORE: \x1b[96m{finalScore}\x1b[0m";
        string rightTag = "TOP 5 LEADERBOARD ";
        buffer.Append(pad).AppendLine(FormatSplitBoxRow(scoreContent, rightTag, innerWidth));

        buffer.Append(pad).AppendLine(dividerLine);

        for (int i = 0; i < 5; i++)
        {
            string rowContent;
            if (i < topScores.Count)
            {
                var entry = topScores[i];
                rowContent = $" #{i + 1}  {entry.PlayerInitials,-4} {entry.Score,7} PTS   (LVL {entry.Level,-2} {entry.Lines,3}L)";
            }
            else
            {
                rowContent = $" #{i + 1}  ---        0 PTS   (LVL  0   0L)";
            }

            buffer.Append(pad).AppendLine(FormatBoxRow(rowContent, innerWidth));
        }

        buffer.Append(pad).AppendLine(horizontalBorder);
        buffer.AppendLine();
        buffer.Append(pad).AppendLine("     PRESS ANY KEY TO CONTINUE...     ");

        Console.Write(buffer.ToString());
    }

    /*
     * Prompts the player for three arcade initials using an interactive console read.
     */
    public string PromptPlayerInitials()
    {
        int windowWidth = Math.Max(Console.WindowWidth, LayoutWidth);
        int leftPadding = Math.Max(0, (windowWidth - LayoutWidth) / 2);
        string pad = new(' ', leftPadding);

        Console.CursorVisible = true;
        Console.Write(AnsiShowCursor);

        Console.Write($"{pad}>> INITIALS (3 CHARS): ");
        string? input = Console.ReadLine();

        Console.CursorVisible = false;
        Console.Write(AnsiHideCursor);

        if (string.IsNullOrWhiteSpace(input))
        {
            return "AAA";
        }

        string clean = input.Trim().ToUpperInvariant();
        return clean.Length > 3 ? clean[..3] : clean.PadRight(3, ' ');
    }

    /*
     * Pads a line with trailing spaces up to exact visible length, ignoring ANSI color escapes.
     */
    private static string FormatBoxRow(string content, int targetWidth)
    {
        int visibleLength = GetVisibleLength(content);
        int paddingNeeded = Math.Max(0, targetWidth - visibleLength);
        return $"|{content}{new string(' ', paddingNeeded)}|";
    }

    /*
     * Formats split-aligned left and right contents within a bounding box row.
     */
    private static string FormatSplitBoxRow(string leftContent, string rightContent, int targetWidth)
    {
        int leftVisible = GetVisibleLength(leftContent);
        int rightVisible = GetVisibleLength(rightContent);
        int spaceBetween = Math.Max(1, targetWidth - (leftVisible + rightVisible));
        return $"|{leftContent}{new string(' ', spaceBetween)}{rightContent}|";
    }

    /*
     * Calculates visible character length by stripping non-printing ANSI escape sequences.
     */
    private static int GetVisibleLength(string text)
    {
        int length = 0;
        bool inEscape = false;

        foreach (char c in text)
        {
            if (c == '\x1b')
            {
                inEscape = true;
                continue;
            }

            if (inEscape)
            {
                if (c == 'm') inEscape = false;
                continue;
            }

            length++;
        }

        return length;
    }

    /*
     * Plays a downward curtain transition wiping the matrix when game over occurs.
     */
    public void AnimateGameOverCurtain(
        board board,
        tetrominoType? heldPieceType,
        tetrominoFactory factory,
        scoreManager scoreManager,
        gameState state)
    {
        int windowWidth = Math.Max(Console.WindowWidth, LayoutWidth);
        int windowHeight = Math.Max(Console.WindowHeight, LayoutHeight);
        int leftPadding = Math.Max(0, (windowWidth - LayoutWidth) / 2);
        int topPadding = Math.Max(0, (windowHeight - LayoutHeight) / 2);
        string pad = new(' ', leftPadding);

        for (int wipeY = board.Rows - 1; wipeY >= 0; wipeY--)
        {
            var buffer = new StringBuilder(4096);
            buffer.Append(AnsiResetCursor);

            for (int i = 0; i < topPadding; i++)
            {
                buffer.AppendLine();
            }

            buffer.Append(pad);
            buffer.AppendLine("+---------------------+ +------------------+");
            buffer.Append(pad);
            buffer.AppendLine("|     T E T R I S     | |      STATS       |");
            buffer.Append(pad);
            buffer.AppendLine("+---------------------+ +------------------+");

            for (int y = 0; y < board.Rows; y++)
            {
                buffer.Append(pad);
                buffer.Append('|');

                bool isWipedRow = y >= wipeY;

                for (int x = 0; x < board.Columns; x++)
                {
                    if (isWipedRow)
                    {
                        buffer.Append("\x1b[90m[X]\x1b[0m");
                    }
                    else if (board[x, y] is { } lockedType)
                    {
                        buffer.Append(GetPieceColoredBlock(lockedType));
                    }
                    else
                    {
                        buffer.Append("\x1b[90m .\x1b[0m");
                    }
                }

                buffer.Append("| | ");
                buffer.Append(GetSidebarLine(y, heldPieceType, factory.PeekNextType(), scoreManager, state));
                buffer.AppendLine(" |");
            }

            buffer.Append(pad);
            buffer.AppendLine("+---------------------+ +------------------+");
            buffer.Append(pad);
            buffer.AppendLine(" [A/D] Move  [W] Rotate  [S] Soft  [Space] Drop ");
            buffer.Append(pad);
            buffer.AppendLine(" [C]   Hold  [P] Pause   [Q] Quit                ");

            Console.Write(buffer.ToString());
            Thread.Sleep(30);
        }
    }
}