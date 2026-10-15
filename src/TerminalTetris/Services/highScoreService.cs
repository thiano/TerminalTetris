using System.Text.Json;
using TerminalTetris.Models;

namespace TerminalTetris.Services;

public class highScoreService
{
    private const int MaxLeaderboardEntries = 5;
    private readonly string _filePath;
    private readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };

    public playerData Data { get; private set; }

    public highScoreService(string? customPath = null)
    {
        if (!string.IsNullOrWhiteSpace(customPath))
        {
            _filePath = customPath;
        }
        else
        {
            string configDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                ".config",
                "TerminalTetris");
            
            Directory.CreateDirectory(configDir);
            _filePath = Path.Combine(configDir, "highscores.json");
        }

        Data = Load();
    }

    public bool IsHighScore(int score)
    {
        if (score <= 0) return false;
        if (Data.HighScores.Count < MaxLeaderboardEntries) return true;
        return score > Data.HighScores.Last().Score;
    }

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

        // Sorteer van hoog naar laag en behoud alleen de top 5
        Data.HighScores = Data.HighScores
            .OrderByDescending(h => h.Score)
            .Take(MaxLeaderboardEntries)
            .ToList();

        Save();
    }

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
                    // Zorg voor een nette sortering
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
            // Fallback bij corrupte JSON
        }

        // Default top scores als het bestand nieuw is
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

    private void Save()
    {
        try
        {
            string json = JsonSerializer.Serialize(Data, _jsonOptions);
            File.WriteAllText(_filePath, json);
        }
        catch
        {
            // Stille afhandeling om gameplay niet te onderbreken bij write permissions
        }
    }
}