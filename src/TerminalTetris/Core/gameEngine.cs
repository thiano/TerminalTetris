using System.Diagnostics;
using TerminalTetris.Enums;
using TerminalTetris.Input;
using TerminalTetris.Models;
using TerminalTetris.Rendering;

namespace TerminalTetris.Core;

public class gameEngine
{
    private static readonly position DefaultSpawnPosition = new(3, 0);

    private readonly board _board;
    private readonly tetrominoFactory _factory;
    private readonly scoreManager _scoreManager;
    private readonly iInputHandler _inputHandler;
    private readonly iRenderer _renderer;

    private tetromino? _currentPiece;
    private tetrominoType? _heldPieceType;
    private bool _canHold = true;

    private gameState _state;
    private bool _isRunning;

    // Lock Delay instellingen (officiële 500ms regel met max 15 resets)
    private const int MaxLockResets = 15;
    private static readonly TimeSpan LockDelayDuration = TimeSpan.FromMilliseconds(500);
    private Stopwatch? _lockStopwatch;
    private int _lockResetCount;

    public gameEngine(
        board board,
        tetrominoFactory factory,
        scoreManager scoreManager,
        iInputHandler inputHandler,
        iRenderer renderer)
    {
        _board = board;
        _factory = factory;
        _scoreManager = scoreManager;
        _inputHandler = inputHandler;
        _renderer = renderer;
        _state = gameState.Playing;
    }

    public void Run()
    {
        _renderer.Initialize();
        _isRunning = true;

        SpawnNextPiece();

        var gravityStopwatch = Stopwatch.StartNew();

        while (_isRunning)
        {
            HandleInput();

            if (_state == gameState.Playing)
            {
                // 1. Zwaartekracht afhandelen
                if (gravityStopwatch.ElapsedMilliseconds >= _scoreManager.CurrentDropIntervalMs)
                {
                    StepGravity();
                    gravityStopwatch.Restart();
                }

                // 2. Lock Delay timer controleren als het blok op de grond rust
                CheckLockDelay();
            }

            tetromino? ghostPiece = CalculateGhostPiece();
            _renderer.Render(_board, _currentPiece, ghostPiece, _heldPieceType, _factory, _scoreManager, _state);

            Thread.Sleep(16);
        }

        _renderer.Cleanup();
    }

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

        if (_currentPiece != null && !_board.CanPlace(_currentPiece))
        {
            _state = gameState.GameOver;
        }
    }

    private void StepGravity()
    {
        if (_currentPiece == null)
        {
            return;
        }

        // Als het blok nog omlaag kan, beweegt het en reset de lock delay
        if (TryMovePiece(0, 1))
        {
            if (!IsPieceOnGround())
            {
                CancelLockDelay();
            }
        }
        else
        {
            // Blok raakt de bodem: start de lock delay als die nog niet liep
            StartLockDelayIfNeeded();
        }
    }

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

    public bool TryRotatePiece(bool clockwise = true)
    {
        if (_currentPiece == null || _state != gameState.Playing)
        {
            return false;
        }

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
                _renderer.AnimateTetrisSplash(_board, _heldPieceType, _factory, _scoreManager, _state);
            }

            _board.ClearSpecificRows(fullRows);
            _scoreManager.RegisterLinesCleared(fullRows.Count);
        }

        _canHold = true;
        SpawnNextPiece();
    }

    private void SpawnNextPiece()
    {
        _currentPiece = _factory.CreateNextPiece(DefaultSpawnPosition);

        if (!_board.CanPlace(_currentPiece))
        {
            _state = gameState.GameOver;
        }
    }

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