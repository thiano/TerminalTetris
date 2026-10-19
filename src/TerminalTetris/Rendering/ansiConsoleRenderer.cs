using System.Text;
using TerminalTetris.Core;
using TerminalTetris.Enums;
using TerminalTetris.Models;

namespace TerminalTetris.Rendering;

/*
 * Strakke, minimalistische retro ASCII-renderer.
 * Bevat een visuele Hold/Next previewbox en verwijdert overbodige .NET-banners.
 */
public class ansiConsoleRenderer : iRenderer
{
    private const string AnsiHideCursor = "\x1b[?25l";
    private const string AnsiShowCursor = "\x1b[?25h";
    private const string AnsiResetCursor = "\x1b[H";
    private const string AnsiResetFormat = "\x1b[0m";

    private const int LayoutWidth = 45;
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

        // Strakke, minimalistische titelbalk
        buffer.Append(pad);
        buffer.AppendLine("+---------------------+ +--------------------+");
        buffer.Append(pad);
        buffer.AppendLine("|     T E T R I S     | |        STATS       |");
        buffer.Append(pad);
        buffer.AppendLine("+---------------------+ +--------------------+");

        // Rijen opbouwen
        for (int y = 0; y < board.Rows; y++)
        {
            buffer.Append(pad);
            buffer.Append("|");

            for (int x = 0; x < board.Columns; x++)
            {
                var pos = new position(x, y);

                if (currentPiece != null && currentPiece.GetOccupiedPositions().Contains(pos))
                {
                    buffer.Append(GetPieceColoredBlock(currentPiece.Type));
                }
                else if (ghostPiece != null && ghostPiece.GetOccupiedPositions().Contains(pos))
                {
                    buffer.Append("\x1b[90m::\x1b[0m"); // Donkere schaduw
                }
                else if (board[x, y] is { } lockedType)
                {
                    buffer.Append(GetPieceColoredBlock(lockedType));
                }
                else
                {
                    buffer.Append("\x1b[90m .\x1b[0m"); // Subtiele donkergrijze lege cel
                }
            }

            buffer.Append("| | ");
            buffer.Append(GetSidebarLine(y, heldPieceType, factory.PeekNextType(), scoreManager, state));
            buffer.AppendLine(" |");
        }

        // Onderkant van bord en sidebar
        buffer.Append(pad);
        buffer.AppendLine("+---------------------+ +--------------------+");

        // Subtiele helpbalk
        buffer.Append(pad);
        buffer.AppendLine(" [A/D] Move  [W] Rotate  [S] Soft  [Space] Drop ");
        buffer.Append(pad);
        buffer.AppendLine(" [C]   Hold  [P] Pause   [Q] Quit                ");

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
            buffer.AppendLine("+---------------------+ +--------------------+");
            buffer.Append(pad);
            buffer.AppendLine("|     T E T R I S     | |        STATS       |");
            buffer.Append(pad);
            buffer.AppendLine("+---------------------+ +--------------------+");

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
                        buffer.Append("\x1b[90m .\x1b[0m");
                    }
                }

                buffer.Append("| | ");
                buffer.Append(GetSidebarLine(y, heldPieceType, factory.PeekNextType(), scoreManager, state));
                buffer.AppendLine(" |");
            }

            buffer.Append(pad);
            buffer.AppendLine("+---------------------+ +--------------------+");
            buffer.Append(pad);
            buffer.AppendLine(" [A/D] Move  [W] Rotate  [S] Soft  [Space] Drop ");
            buffer.Append(pad);
            buffer.AppendLine(" [C]   Hold  [P] Pause   [Q] Quit                ");

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
        Console.Write($"Terminal size: {currentWidth}x{currentHeight}. Minimum required: {LayoutWidth}x{LayoutHeight}  ");
    }

    /*
     * Vult de regels van het rechterpaneel aan (18 tekens breed).
     * Bevat nu een visuele 4x2 preview voor HOLD en NEXT!
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

        return text.PadRight(18);
    }

    /*
     * Rendert een 4x2 mini ASCII weergave van een blokje in de sidebar.
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

    private static string FormatGameState(gameState state) =>
        state switch
        {
            gameState.Playing => "\x1b[92mPLAY\x1b[0m     ",
            gameState.Paused => "\x1b[93mPAUSE\x1b[0m    ",
            gameState.GameOver => "\x1b[91mOVER\x1b[0m     ",
            _ => "UNKNOWN  "
        };

    private static string GetAnsiColor(tetrominoType type) =>
        type switch
        {
            tetrominoType.I => "\x1b[96m",
            tetrominoType.J => "\x1b[94m",
            tetrominoType.L => "\x1b[38;5;208m",
            tetrominoType.O => "\x1b[93m",
            tetrominoType.S => "\x1b[92m",
            tetrominoType.T => "\x1b[95m",
            tetrominoType.Z => "\x1b[91m",
            _ => "\x1b[0m"
        };

    private static string GetPieceColoredBlock(tetrominoType type) =>
        $"{GetAnsiColor(type)}[]{AnsiResetFormat}";

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

        // De 6 officiële tetromino-kleurcodes
        string[] tetrisColors = [
            "\x1b[96m",       // Cyaan (I)
            "\x1b[93m",       // Geel (O)
            "\x1b[95m",       // Magenta (T)
            "\x1b[92m",       // Groen (S)
            "\x1b[38;5;208m", // Oranje (L)
            "\x1b[91m"        // Rood (Z)
        ];

        // 8 frames lange golfcyclus (~600ms totale animatietijd)
        for (int frame = 0; frame < 8; frame++)
        {
            var buffer = new StringBuilder(4096);
            buffer.Append(AnsiResetCursor);

            for (int i = 0; i < topPadding; i++)
            {
                buffer.AppendLine();
            }

            // Pulseer de buitenste speelveldrand mee met de explosie
            string outerBorderCol = tetrisColors[frame % tetrisColors.Length];

            buffer.Append(pad);
            buffer.AppendLine($"{outerBorderCol}+---------------------+ +--------------------+{AnsiResetFormat}");
            buffer.Append(pad);
            buffer.AppendLine($"{outerBorderCol}|{AnsiResetFormat}     \x1b[97m* * * * * *\x1b[0m     {outerBorderCol}| |{AnsiResetFormat}        STATS        {outerBorderCol}|{AnsiResetFormat}");
            buffer.Append(pad);
            buffer.AppendLine($"{outerBorderCol}+---------------------+ +--------------------+{AnsiResetFormat}");

            // Kleuren verschuiven per frame voor een vloeiende regenbooggolf
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

                // Grote impact-zone in het midden van het veld (rij 6 t/m 11)
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
                    // Brede, dikke arcade TETRIS letters
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
                    // Normale bordinhoud
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
            buffer.AppendLine($"{outerBorderCol}+---------------------+ +--------------------+{AnsiResetFormat}");
            buffer.Append(pad);
            buffer.AppendLine(" [A/D] Move  [W] Rotate  [S] Soft  [Space] Drop ");
            buffer.Append(pad);
            buffer.AppendLine(" [C]   Hold  [P] Pause   [Q] Quit                ");

            Console.Write(buffer.ToString());

            // 75ms per golf-frame (8 * 75ms = 600ms pure arcade hype)
            Thread.Sleep(75);
        }
    }
    
    /*
     * Rendert het klassieke arcade Game Over scherm met optionele High Score banner.
     */
    public void RenderGameOverScreen(int finalScore, bool isNewHighScore, List<highScoreEntry> topScores)
    {
        Console.Clear();
        Console.Write(AnsiResetCursor);

        int leftPadding = Math.Max(0, (Console.WindowWidth - LayoutWidth) / 2);
        string pad = new(' ', leftPadding);

        var buffer = new StringBuilder(2048);
        buffer.AppendLine();
        buffer.AppendLine();
        buffer.Append(pad).AppendLine("\x1b[91m+-------------------------------------------+\x1b[0m");
        buffer.Append(pad).AppendLine("\x1b[91m|                GAME OVER                  |\x1b[0m");
        buffer.Append(pad).AppendLine("\x1b[91m+-------------------------------------------+\x1b[0m");
        buffer.AppendLine();

        if (isNewHighScore)
        {
            buffer.Append(pad).AppendLine("\x1b[93m★ ★ ★   N E W   H I G H   S C O R E !   ★ ★ ★\x1b[0m");
            buffer.AppendLine();
        }

        buffer.Append(pad).AppendLine($"  YOUR SCORE: \x1b[96m{finalScore}\x1b[0m");
        buffer.AppendLine();
        buffer.Append(pad).AppendLine("\x1b[90m----------------- TOP 5 --------------------\x1b[0m");

        for (int i = 0; i < topScores.Count; i++)
        {
            var entry = topScores[i];
            buffer.Append(pad).AppendLine($"  #{i + 1}  {entry.PlayerInitials,-4}  {entry.Score,7} PTS  (LVL {entry.Level}, {entry.Lines} L)");
        }

        buffer.Append(pad).AppendLine("\x1b[90m--------------------------------------------\x1b[0m");
        buffer.AppendLine();

        Console.Write(buffer.ToString());
    }

    /*
     * Vraagt 3 arcade-initialen op met cursor zichtbaar.
     */
    public string PromptPlayerInitials()
    {
        Console.CursorVisible = true;
        Console.Write("\x1b[?25h"); // Cursor aanzetten

        int leftPadding = Math.Max(0, (Console.WindowWidth - LayoutWidth) / 2);
        string pad = new(' ', leftPadding);

        Console.Write($"{pad}\x1b[93mENTER INITIALS (3 CHARS): \x1b[0m");
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
     * Vult het speelveld rij voor rij van onder naar boven met grijze [X] blokken.
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

        // Loop van de onderste rij (Rows - 1) tot de bovenste rij (0)
        for (int wipeY = board.Rows - 1; wipeY >= 0; wipeY--)
        {
            var buffer = new StringBuilder(4096);
            buffer.Append(AnsiResetCursor);

            for (int i = 0; i < topPadding; i++)
            {
                buffer.AppendLine();
            }

            buffer.Append(pad);
            buffer.AppendLine("+---------------------+ +--------------------+");
            buffer.Append(pad);
            buffer.AppendLine("|     T E T R I S     | |        STATS       |");
            buffer.Append(pad);
            buffer.AppendLine("+---------------------+ +--------------------+");

            for (int y = 0; y < board.Rows; y++)
            {
                buffer.Append(pad);
                buffer.Append("|");

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
            buffer.AppendLine("+---------------------+ +--------------------+");
            buffer.Append(pad);
            buffer.AppendLine(" [A/D] Move  [W] Rotate  [S] Soft  [Space] Drop ");
            buffer.Append(pad);
            buffer.AppendLine(" [C]   Hold  [P] Pause   [Q] Quit                ");

            Console.Write(buffer.ToString());
            Thread.Sleep(30);
        }
    }
}