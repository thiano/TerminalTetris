using System.Diagnostics;
using TerminalTetris.Enums;
using TerminalTetris.Input;
using TerminalTetris.Models;
using TerminalTetris.Rendering;
using TerminalTetris.Services;

namespace TerminalTetris.Core;

/*
 * Core game loop and state coordinator.
 * Manages player input, gravity tick rate, SRS rotation logic, and lock delay mechanics.
 */
public class gameEngine(
    board board,
    tetrominoFactory factory,
    scoreManager scoreManager,
    iInputHandler inputHandler,
    iRenderer renderer,
    highScoreService highScoreService)
{
    private static readonly position DefaultSpawnPosition = new(3, 0);

    // Gameplay sub-services and state models
    private readonly board _board = board;
    private readonly tetrominoFactory _factory = factory;
    private readonly scoreManager _scoreManager = scoreManager;
    private readonly iInputHandler _inputHandler = inputHandler;
    private readonly iRenderer _renderer = renderer;
    private readonly highScoreService _highScoreService = highScoreService;

    // Active piece and hold tracking
    private tetromino? _currentPiece;
    private tetrominoType? _heldPieceType;
    private bool _canHold = true;

    // Engine loop lifecycle
    private gameState _state = gameState.Playing;
    private bool _isRunning;

    // Session duration and Tetris (4-line clear) tracking
    private readonly Stopwatch _sessionStopwatch = new();
    private int _tetrisClearsCount;

    // Guideline lock delay settings (500ms duration, max 15 movement resets)
    private const int MaxLockResets = 15;
    private static readonly TimeSpan LockDelayDuration = TimeSpan.FromMilliseconds(500);
    private Stopwatch? _lockStopwatch;
    private int _lockResetCount;

    /*
     * Starts the main game loop, timers, and initial piece spawn.
     */
    public void Run()
    {
        _renderer.Initialize();
        _isRunning = true;
        _sessionStopwatch.Start();

        SpawnNextPiece();

        var gravityStopwatch = Stopwatch.StartNew();

        while (_isRunning)
        {
            HandleInput();

            if (_state == gameState.Playing)
            {
                // 1. Process vertical gravity step
                if (gravityStopwatch.ElapsedMilliseconds >= _scoreManager.CurrentDropIntervalMs)
                {
                    StepGravity();
                    gravityStopwatch.Restart();
                }

                // 2. Evaluate piece ground contact and lock delay timeout
                CheckLockDelay();
            }

            tetromino? ghostPiece = CalculateGhostPiece();
            _renderer.Render(_board, _currentPiece, ghostPiece, _heldPieceType, _factory, _scoreManager, _state);

            // Target frame cap (~60 FPS)
            Thread.Sleep(16);
        }

        _sessionStopwatch.Stop();
        _renderer.Cleanup();

        if (_state == gameState.GameOver)
        {
            HandleGameOver();
        }
    }

    /*
     * Executes the Game Over sequence: curtain animation, session stats, and high score input.
     */
    private void HandleGameOver()
    {
        _renderer.AnimateGameOverCurtain(_board, _heldPieceType, _factory, _scoreManager, _state);
        Thread.Sleep(200);

        int finalScore = _scoreManager.Score;
        int lines = _scoreManager.LinesCleared;
        int level = _scoreManager.Level;

        // Persist session metrics to local high score storage
        _highScoreService.RecordGameSession(lines, level, _tetrisClearsCount, _sessionStopwatch.Elapsed);

        bool isHighScore = _highScoreService.IsHighScore(finalScore);

        // Display leaderboard overlay
        _renderer.RenderGameOverScreen(finalScore, isHighScore, _highScoreService.Data.HighScores);

        // Request initials if player achieved a top 5 rank
        if (isHighScore)
        {
            string initials = _renderer.PromptPlayerInitials();
            _highScoreService.AddHighScore(initials, finalScore, lines, level);
            _renderer.RenderGameOverScreen(finalScore, false, _highScoreService.Data.HighScores);
        }

        // Wait for player acknowledgement before terminal exit
        Console.ReadKey(intercept: true);
    }

    /*
     * Polls and processes all pending keyboard input events from the input queue.
     */
    private void HandleInput()
    {
        while (true)
        {
            gameAction action = _inputHandler.PollAction();

            if (action == gameAction.None)
            {
                break;
            }

            if (action == gameAction.Quit)
            {
                _isRunning = false;
                return;
            }

            if (action == gameAction.TogglePause && _state != gameState.GameOver)
            {
                _state = (_state == gameState.Playing) ? gameState.Paused : gameState.Playing;
                return;
            }

            if (_state != gameState.Playing || _currentPiece == null)
            {
                continue;
            }

            switch (action)
            {
                case gameAction.MoveLeft:
                    if (TryMovePiece(-1, 0))
                    {
                        ResetLockDelay();
                    }
                    break;

                case gameAction.MoveRight:
                    if (TryMovePiece(1, 0))
                    {
                        ResetLockDelay();
                    }
                    break;

                case gameAction.SoftDrop:
                    if (TryMovePiece(0, 1))
                    {
                        _scoreManager.AddSoftDropPoint();
                        ResetLockDelay();
                    }
                    break;

                case gameAction.HardDrop:
                    HardDropPiece();
                    break;

                case gameAction.RotateClockwise:
                    TryRotatePiece(clockwise: true);
                    break;

                case gameAction.RotateCounterClockwise:
                    TryRotatePiece(clockwise: false);
                    break;

                case gameAction.Hold:
                    HoldPiece();
                    break;
            }
        }
    }

    /*
     * Swaps the active piece into the hold slot or spawns next piece on initial hold.
     */
    private void HoldPiece()
    {
        if (!_canHold || _currentPiece == null)
        {
            return;
        }

        tetrominoType currentType = _currentPiece.Type;

        if (_heldPieceType.HasValue)
        {
            tetrominoType toSpawn = _heldPieceType.Value;
            _heldPieceType = currentType;
            _currentPiece = _factory.CreatePieceByType(toSpawn, DefaultSpawnPosition);
        }
        else
        {
            _heldPieceType = currentType;
            SpawnNextPiece();
        }

        _canHold = false;
        CancelLockDelay();

        // Check if held piece spawns directly into occupied blocks (top out)
        if (_currentPiece != null && !_board.CanPlace(_currentPiece))
        {
            _state = gameState.GameOver;
            _isRunning = false;
        }
    }

    /*
     * Advances piece downwards by one cell due to automatic gravity.
     */
    private void StepGravity()
    {
        if (_currentPiece == null)
        {
            return;
        }

        if (TryMovePiece(0, 1))
        {
            if (!IsPieceOnGround())
            {
                CancelLockDelay();
            }
        }
        else
        {
            StartLockDelayIfNeeded();
        }
    }

    /*
     * Checks if current piece rests on surface and locks it once duration expires.
     */
    private void CheckLockDelay()
    {
        if (_currentPiece == null)
        {
            return;
        }

        if (IsPieceOnGround())
        {
            StartLockDelayIfNeeded();

            if (_lockStopwatch != null && _lockStopwatch.Elapsed >= LockDelayDuration)
            {
                LockCurrentPieceAndContinue();
            }
        }
        else
        {
            CancelLockDelay();
        }
    }

    /*
     * Checks whether piece has ground or locked tile contact directly underneath.
     */
    private bool IsPieceOnGround()
    {
        if (_currentPiece == null) return false;

        tetromino clone = _currentPiece.Clone();
        clone.Move(0, 1);
        return !_board.CanPlace(clone);
    }

    private void StartLockDelayIfNeeded()
    {
        _lockStopwatch ??= Stopwatch.StartNew();
    }

    /*
     * Resets the lock delay stopwatch upon valid horizontal movement or rotation.
     */
    private void ResetLockDelay()
    {
        if (IsPieceOnGround() && _lockResetCount < MaxLockResets)
        {
            _lockStopwatch?.Restart();
            _lockResetCount++;
        }
    }

    private void CancelLockDelay()
    {
        _lockStopwatch = null;
        _lockResetCount = 0;
    }

    /*
     * Drops the piece instantly to the bottom, adds drop points, and locks placement.
     */
    private void HardDropPiece()
    {
        if (_currentPiece == null)
        {
            return;
        }

        while (TryMovePiece(0, 1))
        {
            _scoreManager.AddSoftDropPoint();
        }

        LockCurrentPieceAndContinue();
    }

    /*
     * Attempts to translate active piece position by delta coordinates.
     */
    private bool TryMovePiece(int deltaX, int deltaY)
    {
        if (_currentPiece == null)
        {
            return false;
        }

        _currentPiece.Move(deltaX, deltaY);

        if (!_board.CanPlace(_currentPiece))
        {
            _currentPiece.Move(-deltaX, -deltaY);
            return false;
        }

        return true;
    }

    /*
     * Executes SRS (Super Rotation System) tests for clockwise or counter-clockwise rotation.
     */
    public bool TryRotatePiece(bool clockwise = true)
    {
        if (_currentPiece == null || _state != gameState.Playing)
        {
            return false;
        }

        // O-piece possesses 4-way radial symmetry, rotation tests can be skipped
        if (_currentPiece.Type == tetrominoType.O)
        {
            return true;
        }

        int fromState = _currentPiece.RotationState;
        int toState = clockwise 
            ? (fromState + 1) % 4 
            : (fromState + 3) % 4;

        int kickIndex = wallKickData.GetKickTestIndex(fromState, toState);
        var kickTable = _currentPiece.Type == tetrominoType.I 
            ? wallKickData.IKicks 
            : wallKickData.StandardKicks;

        // Perform the 5 standard SRS kick tests
        for (int test = 0; test < 5; test++)
        {
            var (deltaX, deltaY) = kickTable[kickIndex, test];

            tetromino clone = _currentPiece.Clone();
            if (clockwise)
            {
                clone.RotateClockwise();
            }
            else
            {
                clone.RotateCounterClockwise();
            }

            clone.Move(deltaX, deltaY);

            if (_board.CanPlace(clone))
            {
                _currentPiece = clone;
                ResetLockDelay();
                return true;
            }
        }

        return false;
    }

    /*
     * Locks piece into grid matrix, evaluates completed rows, triggers clear effects, and spawns next piece.
     */
    private void LockCurrentPieceAndContinue()
    {
        if (_currentPiece == null)
        {
            return;
        }

        CancelLockDelay();
        _board.LockPiece(_currentPiece);

        List<int> fullRows = _board.GetFullRowIndices();

        if (fullRows.Count > 0)
        {
            _renderer.AnimateLineClear(_board, fullRows, _heldPieceType, _factory, _scoreManager, _state);

            if (fullRows.Count == 4)
            {
                _tetrisClearsCount++;
                _renderer.AnimateTetrisSplash(_board, _heldPieceType, _factory, _scoreManager, _state);
            }

            _board.ClearSpecificRows(fullRows);
            _scoreManager.RegisterLinesCleared(fullRows.Count);
        }

        _canHold = true;
        SpawnNextPiece();
    }

    /*
     * Pulls the next tetromino from the random generator and checks for spawn collisions.
     */
    private void SpawnNextPiece()
    {
        _currentPiece = _factory.CreateNextPiece(DefaultSpawnPosition);

        if (!_board.CanPlace(_currentPiece))
        {
            _state = gameState.GameOver;
            _isRunning = false;
        }
    }

    /*
     * Calculates the projected landing location (ghost piece) of the currently falling piece.
     */
    private tetromino? CalculateGhostPiece()
    {
        if (_currentPiece == null || _state != gameState.Playing)
        {
            return null;
        }

        tetromino ghost = _currentPiece.Clone();

        while (_board.CanPlace(ghost))
        {
            ghost.Move(0, 1);
        }

        ghost.Move(0, -1);
        return ghost;
    }
}