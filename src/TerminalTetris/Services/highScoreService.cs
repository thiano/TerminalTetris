using System.Text.Json;
using TerminalTetris.Models;

namespace TerminalTetris.Services;

/*
 * Persistence service managing leaderboard rankings and lifetime player metrics.
 * Handles JSON serialization, default profile seeding, and atomic disk writes.
 */
public class highScoreService
{
    private const int MaxLeaderboardEntries = 5;
    private readonly string _filePath;
    private readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };

    // Active player records and lifetime gameplay metrics
    public playerData Data { get; private set; }

    public highScoreService(string? customPath = null)
    {
        if (!string.IsNullOrWhiteSpace(customPath))
        {
            _filePath = customPath;
        }
        else
        {
            // Resolve platform-appropriate configuration and data directory
            string basePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            if (string.IsNullOrEmpty(basePath))
            {
                basePath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                    ".config");
            }

            string configDir = Path.Combine(basePath, "TerminalTetris");
            Directory.CreateDirectory(configDir);
            _filePath = Path.Combine(configDir, "highscores.json");
        }

        Data = Load();
    }

    /*
     * Checks if a given score qualifies for entry into the top leaderboard.
     */
    public bool IsHighScore(int score)
    {
        if (score <= 0)
        {
            return false;
        }

        // Automatically qualifies if leaderboard contains open slots
        if (Data.HighScores.Count < MaxLeaderboardEntries)
        {
            return true;
        }

        return score > Data.HighScores.Last().Score;
    }

    /*
     * Inserts a new score entry, sorts leaderboard rankings, and truncates to top 5.
     */
    public void AddHighScore(string initials, int score, int lines, int level)
    {
        string cleanInitials = string.IsNullOrWhiteSpace(initials)
            ? "AAA"
            : initials.Trim().ToUpperInvariant();

        if (cleanInitials.Length > 3)
        {
            cleanInitials = cleanInitials[..3];
        }

        Data.HighScores.Add(new highScoreEntry
        {
            PlayerInitials = cleanInitials,
            Score = score,
            Lines = lines,
            Level = level,
            Date = DateTime.UtcNow
        });

        // Maintain descending order and clamp to maximum leaderboard capacity
        Data.HighScores = Data.HighScores
            .OrderByDescending(h => h.Score)
            .Take(MaxLeaderboardEntries)
            .ToList();

        Save();
    }

    /*
     * Updates cumulative lifetime metrics upon game session completion.
     */
    public void RecordGameSession(int lines, int level, int tetrisClears, TimeSpan playTime)
    {
        Data.Stats.GamesPlayed++;
        Data.Stats.TotalLinesCleared += lines;
        Data.Stats.TotalTetrisClears += tetrisClears;
        Data.Stats.TotalSecondsPlayed += (int)playTime.TotalSeconds;

        if (level > Data.Stats.MaxLevelReached)
        {
            Data.Stats.MaxLevelReached = level;
        }

        Save();
    }

    /*
     * Loads existing player data from disk or seeds default records if file is missing/corrupted.
     */
    private playerData Load()
    {
        try
        {
            if (File.Exists(_filePath))
            {
                string json = File.ReadAllText(_filePath);
                var loaded = JsonSerializer.Deserialize<playerData>(json);
                if (loaded != null)
                {
                    // Ensure entries remain correctly sorted and constrained
                    loaded.HighScores = loaded.HighScores
                        .OrderByDescending(h => h.Score)
                        .Take(MaxLeaderboardEntries)
                        .ToList();

                    return loaded;
                }
            }
        }
        catch
        {
            // Fall back to default seeded profile on deserialization failure
        }

        // Seed default arcade leaderboard records for initial startup
        return new playerData
        {
            HighScores =
            [
                new highScoreEntry { PlayerInitials = "TET", Score = 10000, Lines = 20, Level = 2 },
                new highScoreEntry { PlayerInitials = "ARC", Score = 7500, Lines = 15, Level = 1 },
                new highScoreEntry { PlayerInitials = "THI", Score = 5000, Lines = 10, Level = 1 }
            ],
            Stats = new gameStatistics()
        };
    }

    /*
     * Persists active player data to disk using atomic file replacement.
     */
    private void Save()
    {
        try
        {
            string json = JsonSerializer.Serialize(Data, _jsonOptions);
            string tempPath = $"{_filePath}.tmp";

            // Write to temporary buffer before replacement to avoid file corruption
            File.WriteAllText(tempPath, json);
            File.Move(tempPath, _filePath, overwrite: true);
        }
        catch
        {
            // Silently suppress disk write exceptions to preserve continuous game loop
        }
    }
}