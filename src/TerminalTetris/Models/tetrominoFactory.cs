using TerminalTetris.Enums;
using TerminalTetris.Models.Pieces;

namespace TerminalTetris.Models;

/*
 * Fabrieksklasse voor het genereren van nieuwe tetromino's.
 * Hanteert het officiële '7-Bag' randomizer algoritme zodat stukken
 * evenredig verdeeld worden zonder oneindige droogtes.
 */
public class tetrominoFactory
{
    private readonly Random _random;
    private readonly Queue<tetrominoType> _bag = new();

    public tetrominoFactory(int? seed = null)
    {
        _random = seed.HasValue ? new Random(seed.Value) : new Random();
    }

    /*
     * Haalt het volgende willekeurige stuk uit de huidige 7-bag
     * en plaatst het op de standaard spawnpositie bovenaan het bord.
     */
    public tetromino CreateNextPiece(position spawnPosition)
    {
        if (_bag.Count == 0)
        {
            RefillBag();
        }

        tetrominoType nextType = _bag.Dequeue();
        return InstantiatePiece(nextType, spawnPosition);
    }

    /*
     * Kijkt welk type als volgende aan de beurt is zonder de zak te legen.
     * Handig voor de 'Next Piece' preview in de UI.
     */
    public tetrominoType PeekNextType()
    {
        if (_bag.Count == 0)
        {
            RefillBag();
        }

        return _bag.Peek();
    }

    /*
     * Vult de zak met alle 7 types en schudt ze willekeurig (Fisher-Yates shuffle).
     */
    private void RefillBag()
    {
        List<tetrominoType> pieces =
        [
            tetrominoType.I,
            tetrominoType.J,
            tetrominoType.L,
            tetrominoType.O,
            tetrominoType.S,
            tetrominoType.T,
            tetrominoType.Z
        ];

        // Fisher-Yates shuffle
        int n = pieces.Count;
        while (n > 1)
        {
            n--;
            int k = _random.Next(n + 1);
            (pieces[k], pieces[n]) = (pieces[n], pieces[k]);
        }

        foreach (tetrominoType type in pieces)
        {
            _bag.Enqueue(type);
        }
    }

    /*
     * Mapt een tetrominoType naar de bijbehorende concrete klasse-instantie.
     */
    private static tetromino InstantiatePiece(tetrominoType type, position spawnPosition) =>
        type switch
        {
            tetrominoType.I => new tetrominoI(spawnPosition),
            tetrominoType.J => new tetrominoJ(spawnPosition),
            tetrominoType.L => new tetrominoL(spawnPosition),
            tetrominoType.O => new tetrominoO(spawnPosition),
            tetrominoType.S => new tetrominoS(spawnPosition),
            tetrominoType.T => new tetrominoT(spawnPosition),
            tetrominoType.Z => new tetrominoZ(spawnPosition),
            _ => throw new ArgumentOutOfRangeException(nameof(type), $"Onbekend tetromino type: {type}")
        };
    
    /*
     * Maakt een specifiek stuk op basis van tetrominoType (gebruikt voor Hold swap).
     */
    public tetromino CreatePieceByType(tetrominoType type, position spawnPos) =>
        type switch
        {
            tetrominoType.I => new Pieces.tetrominoI(spawnPos),
            tetrominoType.J => new Pieces.tetrominoJ(spawnPos),
            tetrominoType.L => new Pieces.tetrominoL(spawnPos),
            tetrominoType.O => new Pieces.tetrominoO(spawnPos),
            tetrominoType.S => new Pieces.tetrominoS(spawnPos),
            tetrominoType.T => new Pieces.tetrominoT(spawnPos),
            tetrominoType.Z => new Pieces.tetrominoZ(spawnPos),
            _ => throw new ArgumentOutOfRangeException(nameof(type), "Unknown tetromino type")
        };
}