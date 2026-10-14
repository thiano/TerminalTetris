using System.Diagnostics;

namespace TerminalTetris.Input;

/*
 * Geavanceerde console input handler met softwarematige DAS (Delayed Auto-Shift)
 * en ARR (Auto-Repeat Rate) voor directe, responsieve laterale besturing.
 */
public class consoleInputHandler : iInputHandler
{
    // DAS: wachttijd voor het automatisch doorschuiven start (in milliseconden)
    private const int DasThresholdMs = 130;

    // ARR: snelheid van het doorschuiven zodra DAS actief is (in milliseconden per stap)
    private const int ArrIntervalMs = 30;

    // Drempel om te bepalen of een toets is losgelaten
    private const int ReleaseTimeoutMs = 110;

    private readonly Stopwatch _timer = Stopwatch.StartNew();

    private ConsoleKey? _activeHeldKey;
    private long _keyPressTimestamp;
    private long _lastRepeatTimestamp;
    private long _lastSeenTimestamp;

    public gameAction PollAction()
    {
        long now = _timer.ElapsedMilliseconds;

        // Controleer of de actieve toets is losgelaten (geen invoer meer binnen de time-out)
        if (_activeHeldKey != null && now - _lastSeenTimestamp > ReleaseTimeoutMs)
        {
            ResetHeldKey();
        }

        // Lees alle wachtende toetsaanslagen uit de buffer
        ConsoleKey? latestKey = null;
        while (Console.KeyAvailable)
        {
            ConsoleKeyInfo keyInfo = Console.ReadKey(intercept: true);
            latestKey = keyInfo.Key;
        }

        if (latestKey.HasValue)
        {
            ConsoleKey key = latestKey.Value;
            _lastSeenTimestamp = now;

            // Nieuwe laterale richting ingedrukt
            if (IsLateralKey(key))
            {
                if (_activeHeldKey != key)
                {
                    _activeHeldKey = key;
                    _keyPressTimestamp = now;
                    _lastRepeatTimestamp = now;
                    return MapKeyToAction(key); // Directe eerste verplaatsing
                }
            }
            else
            {
                // Niet-laterale toetsen (rotatie, drops, pauze) direct verwerken
                return MapKeyToAction(key);
            }
        }

        // DAS & ARR afhandeling voor ingedrukte richtingstoetsen
        if (_activeHeldKey.HasValue)
        {
            long heldDuration = now - _keyPressTimestamp;

            // Als DAS voorbij is en het ARR-interval verstreken is
            if (heldDuration >= DasThresholdMs && now - _lastRepeatTimestamp >= ArrIntervalMs)
            {
                _lastRepeatTimestamp = now;
                return MapKeyToAction(_activeHeldKey.Value);
            }
        }

        return gameAction.None;
    }

    private void ResetHeldKey()
    {
        _activeHeldKey = null;
        _keyPressTimestamp = 0;
        _lastRepeatTimestamp = 0;
        _lastSeenTimestamp = 0;
    }

    private static bool IsLateralKey(ConsoleKey key) =>
        key is ConsoleKey.LeftArrow or ConsoleKey.RightArrow or ConsoleKey.A or ConsoleKey.D;

    private static gameAction MapKeyToAction(ConsoleKey key) =>
        key switch
        {
            ConsoleKey.LeftArrow or ConsoleKey.A => gameAction.MoveLeft,
            ConsoleKey.RightArrow or ConsoleKey.D => gameAction.MoveRight,
            ConsoleKey.DownArrow or ConsoleKey.S => gameAction.SoftDrop,
            ConsoleKey.Spacebar => gameAction.HardDrop,
            ConsoleKey.UpArrow or ConsoleKey.W => gameAction.RotateClockwise,
            ConsoleKey.Z => gameAction.RotateCounterClockwise,
            ConsoleKey.C => gameAction.Hold,
            ConsoleKey.P => gameAction.TogglePause,
            ConsoleKey.Escape or ConsoleKey.Q => gameAction.Quit,
            _ => gameAction.None
        };
}