namespace TerminalTetris.Models;

/*
 * Provides offset test tables for the Super Rotation System (SRS).
 * Determines wall and floor kick translation vectors when standard rotation collides.
 */
public static class wallKickData
{
    /*
     * Standard kick translation table for J, L, S, T, and Z tetromino pieces.
     * Dimensions: [8 transition states, 5 kick tests per transition].
     */
    public static readonly (int x, int y)[,] StandardKicks = new[,]
    {
        // Index 0: State 0 -> 1 (Clockwise from spawn)
        { (0, 0), (-1, 0), (-1, -1), (0, 2), (-1, 2) },

        // Index 1: State 1 -> 0 (Counter-clockwise to spawn)
        { (0, 0), (1, 0), (1, 1), (0, -2), (1, -2) },

        // Index 2: State 1 -> 2 (Clockwise to inverted)
        { (0, 0), (1, 0), (1, 1), (0, -2), (1, -2) },

        // Index 3: State 2 -> 1 (Counter-clockwise to right-facing)
        { (0, 0), (-1, 0), (-1, -1), (0, 2), (-1, 2) },

        // Index 4: State 2 -> 3 (Clockwise to left-facing)
        { (0, 0), (1, 0), (1, -1), (0, 2), (1, 2) },

        // Index 5: State 3 -> 2 (Counter-clockwise to inverted)
        { (0, 0), (-1, 0), (-1, 1), (0, -2), (-1, -2) },

        // Index 6: State 3 -> 0 (Clockwise to spawn)
        { (0, 0), (-1, 0), (-1, 1), (0, -2), (-1, -2) },

        // Index 7: State 0 -> 3 (Counter-clockwise from spawn)
        { (0, 0), (1, 0), (1, -1), (0, 2), (1, 2) }
    };

    /*
     * Specialized kick translation table for the 4x4 I-tetromino piece.
     * Dimensions: [8 transition states, 5 kick tests per transition].
     */
    public static readonly (int x, int y)[,] IKicks = new[,]
    {
        // Index 0: State 0 -> 1
        { (0, 0), (-2, 0), (1, 0), (-2, 1), (1, -2) },

        // Index 1: State 1 -> 0
        { (0, 0), (2, 0), (-1, 0), (2, -1), (-1, 2) },

        // Index 2: State 1 -> 2
        { (0, 0), (-1, 0), (2, 0), (-1, -2), (2, 1) },

        // Index 3: State 2 -> 1
        { (0, 0), (1, 0), (-2, 0), (1, 2), (-2, -1) },

        // Index 4: State 2 -> 3
        { (0, 0), (2, 0), (-1, 0), (2, -1), (-1, 2) },

        // Index 5: State 3 -> 2
        { (0, 0), (-2, 0), (1, 0), (-2, 1), (1, -2) },

        // Index 6: State 3 -> 0
        { (0, 0), (1, 0), (-2, 0), (1, 2), (-2, -1) },

        // Index 7: State 0 -> 3
        { (0, 0), (-1, 0), (2, 0), (-1, -2), (2, 1) }
    };

    /*
     * Maps rotation transition pairs (fromState -> toState) to their kick table row index.
     */
    public static int GetKickTestIndex(int fromState, int toState) => (fromState, toState) switch
    {
        (0, 1) => 0,
        (1, 0) => 1,
        (1, 2) => 2,
        (2, 1) => 3,
        (2, 3) => 4,
        (3, 2) => 5,
        (3, 0) => 6,
        (0, 3) => 7,
        _ => 0
    };
}