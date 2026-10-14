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

            if (_state == gameState.Playing && gravityStopwatch.ElapsedMilliseconds >= _scoreManager.CurrentDropIntervalMs)
            {
                StepGravity();
                gravityStopwatch.Restart();
            }

            tetromino? ghostPiece = CalculateGhostPiece();
            _renderer.Render(_board, _currentPiece, ghostPiece, _heldPieceType, _factory, _scoreManager, _state);

            Thread.Sleep(16);
        }

        _renderer.Cleanup();
    }

    /*
     * Verwerkt ALLE direct beschikbare invoer binnen het huidige frame.
     * Voorkomt dat toetsaanslagen zich opstapelen en vertraging veroorzaken.
     */
    private void HandleInput()
    {
        // Zolang er toetsaanslagen in de buffer klaarstaan, verwerken we ze direct
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
                    TryMovePiece(-1, 0);
                    break;
                case gameAction.MoveRight:
                    TryMovePiece(1, 0);
                    break;
                case gameAction.SoftDrop:
                    if (TryMovePiece(0, 1))
                    {
                        _scoreManager.AddSoftDropPoint();
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
            // Swap held piece with current piece
            tetrominoType toSpawn = _heldPieceType.Value;
            _heldPieceType = currentType;
            _currentPiece = _factory.CreatePieceByType(toSpawn, DefaultSpawnPosition);
        }
        else
        {
            // First time holding
            _heldPieceType = currentType;
            SpawnNextPiece();
        }

        _canHold = false; // Mag pas weer na een succesvolle lock

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

        if (!TryMovePiece(0, 1))
        {
            LockCurrentPieceAndContinue();
        }
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

    private bool TryRotatePiece(bool clockwise)
    {
        if (_currentPiece == null)
        {
            return false;
        }

        if (clockwise)
        {
            _currentPiece.RotateClockwise();
        }
        else
        {
            _currentPiece.RotateCounterClockwise();
        }

        if (!_board.CanPlace(_currentPiece))
        {
            if (clockwise)
            {
                _currentPiece.RotateCounterClockwise();
            }
            else
            {
                _currentPiece.RotateClockwise();
            }

            return false;
        }

        return true;
    }

    private void LockCurrentPieceAndContinue()
    {
        if (_currentPiece == null)
        {
            return;
        }

        _board.LockPiece(_currentPiece);

        List<int> fullRows = _board.GetFullRowIndices();

        if (fullRows.Count > 0)
        {
            // 1. Eerst de gewone rij-animatie () -> >< -> || -> -- -> ..
            _renderer.AnimateLineClear(_board, fullRows, _heldPieceType, _factory, _scoreManager, _state);

            // 2. Was het een 'Tetris' (4 rijen tegelijk)? Toon de meerkleurige splash!
            if (fullRows.Count == 4)
            {
                _renderer.AnimateTetrisSplash(_board, _heldPieceType, _factory, _scoreManager, _state);
            }

            // 3. Wis de rijen en registreer de score
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