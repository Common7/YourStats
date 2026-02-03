using Microsoft.Data.Sqlite;
using YourStats.Core;

namespace YourStats.Data;

public class DatabaseService
{
    private readonly string _dbPath;

    public DatabaseService(string dbPath)
    {
        _dbPath = dbPath;
        InitalizeDatabase();
    }
    
    public void InitalizeDatabase()
    {
        using (var connection = new SqliteConnection($"Data Source={_dbPath}"))
        {
            connection.Open();
            
            var createTableCommand = connection.CreateCommand();

            createTableCommand.CommandText = @"
                CREATE TABLE IF NOT EXISTS Matches (
                    MatchId INTEGER PRIMARY KEY AUTOINCREMENT,
                    MapName TEXT,
                    TScore INTEGER,
                    CTScore INTEGER
                );

                CREATE TABLE IF NOT EXISTS MatchStats (
                    StatsId INTEGER PRIMARY KEY AUTOINCREMENT,
                    MatchId INTEGER,
                    SteamId TEXT,
                    PlayerName TEXT,
                    Kills INTEGER,
                    Deaths INTEGER,
                    Assists INTEGER,
                    UtilDamage INTEGER,
                    FirstKills INTEGER,
                    Team INTEGER,
                    FOREIGN KEY (MatchId) REFERENCES Matches (MatchId)
                );";
            createTableCommand.ExecuteNonQuery();
        }
    }

    public void SaveMatchData(MatchStats matchStats)
    {
        using (var connection = new SqliteConnection($"Data Source={_dbPath}"))
        {
            connection.Open();
            
            var matchCommand = connection.CreateCommand();
            matchCommand.CommandText = @"
                INSERT INTO Matches (MapName, TScore, CTScore)
                VALUES (@map, @tScore, @ctScore);
                SELECT last_insert_rowid();
            ";
            
            matchCommand.Parameters.AddWithValue("@map", matchStats.MapName);
            matchCommand.Parameters.AddWithValue("@tScore", matchStats.tScore);
            matchCommand.Parameters.AddWithValue("@ctScore", matchStats.CTScore);
            
            long matchId = (long)matchCommand.ExecuteScalar();

            foreach (var player in matchStats.Players)
            {
                var playerStatsCommand = connection.CreateCommand();
                playerStatsCommand.CommandText = @"
                    INSERT INTO MatchStats (MatchId, SteamId, PlayerName, Kills, Deaths, Assists, UtilDamage, FirstKills, Team)
                    VALUES (@matchId, @steamId, @playerName, @kills, @deaths, @assists, @utilDamage, @firstKills, @team);
                ";
                
                playerStatsCommand.Parameters.AddWithValue("@matchId", matchId);
                playerStatsCommand.Parameters.AddWithValue("@steamId", player.Value.SteamID);
                playerStatsCommand.Parameters.AddWithValue("@playerName", player.Value.Name);
                playerStatsCommand.Parameters.AddWithValue("@kills", player.Value.Kills);
                playerStatsCommand.Parameters.AddWithValue("@deaths", player.Value.Deaths);
                playerStatsCommand.Parameters.AddWithValue("@assists", player.Value.Assists);
                playerStatsCommand.Parameters.AddWithValue("@utilDamage", player.Value.UtilDamage);
                playerStatsCommand.Parameters.AddWithValue("@firstKills", player.Value.FirstKills);
                playerStatsCommand.Parameters.AddWithValue("@team", player.Value.Team);

                playerStatsCommand.ExecuteNonQuery();
            }
        }
    }
}