using System.Diagnostics;
using TerminalTetris.Enums;
using TerminalTetris.Input;
using TerminalTetris.Models;
using TerminalTetris.Rendering;

namespace TerminalTetris.Core;

/*
 * Centrale game loop engine.
 * Beheert de timing van de zwaartekracht, verwerkt spelersinvoer,
 * voert botsingsdetectie uit en stuurt de renderer aan.
 */
public class gameEngine
{
    private static readonly position DefaultSpawnPosition = new(3, 0);

    private readonly board _board;
    private readonly tetrominoFactory _factory;
    private readonly scoreManager _scoreManager;
    private readonly iInputHandler _inputHandler;
    private readonly iRenderer _renderer;

    private tetromino? _currentPiece;
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

    /*
     * Start en onderhoudt de hoofd-gameloop totdat de speler afsluit.
     */
    public void Run()
    {
        _renderer.Initialize();
        _isRunning = true;

        SpawnNextPiece();

        var gravityStopwatch = Stopwatch.StartNew();

        while (_isRunning)
        {
            // 1. Input verwerken
            HandleInput();

            // 2. Logica updaten (zwaartekracht)
            if (_state == gameState.Playing && gravityStopwatch.ElapsedMilliseconds >= _scoreManager.CurrentDropIntervalMs)
            {
                StepGravity();
                gravityStopwatch.Restart();
            }

            // 3. Renderen
            tetromino? ghostPiece = CalculateGhostPiece();
            _renderer.Render(_board, _currentPiece, ghostPiece, _factory, _scoreManager, _state);

            // 4. Frame-limiter (~60 FPS / 16ms pauze per tick)
            Thread.Sleep(16);
        }

        _renderer.Cleanup();
    }

    /*
     * Leest de niet-blokkerende invoer uit en voert acties uit.
     */
    private void HandleInput()
    {
        gameAction action = _inputHandler.PollAction();

        if (action == gameAction.None)
        {
            return;
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
            return;
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
        }
    }

    /*
     * Laat het actieve stuk 1 stap zakken door de zwaartekracht.
     * Als het niet kan zakken, wordt het vastgezet (gelockt).
     */
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

    /*
     * Plaatst het stuk direct op de bodem (Hard Drop).
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
     * Probeert het stuk te verplaatsen met deltaX en deltaY.
     * Rolt de verplaatsing terug bij botsing.
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
     * Probeert het stuk te roteren.
     * Rolt de rotatie terug als er een botsing optreedt.
     */
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
            // Draai terug bij botsing
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

    /*
     * Zet het huidige stuk vast, wist volle rijen en spawnt het volgende stuk.
     */
    private void LockCurrentPieceAndContinue()
    {
        if (_currentPiece == null)
        {
            return;
        }

        _board.LockPiece(_currentPiece);
        int clearedRows = _board.ClearFullRows();
        _scoreManager.RegisterLinesCleared(clearedRows);

        SpawnNextPiece();
    }

    /*
     * Haalt een nieuw stuk op via de factory en controleert op Game Over.
     */
    private void SpawnNextPiece()
    {
        _currentPiece = _factory.CreateNextPiece(DefaultSpawnPosition);

        if (!_board.CanPlace(_currentPiece))
        {
            _state = gameState.GameOver;
        }
    }

    /*
     * Berekent een schaduw (ghost piece) op de bodem zonder de factory te belasten.
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