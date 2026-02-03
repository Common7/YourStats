using System.Text.Json.Serialization;

namespace YourStats.Bridge;

using YourStats.Core;
using System.Text.Json;

public class BridgeService
{
    public MatchStats GetPlayerStats(string jsonPlayerStats, string jsonMatchData)
    {
        string jsonStringPlayerData = File.ReadAllText(jsonPlayerStats);
        string jsonStringMatchData = File.ReadAllText(jsonMatchData);
        
        Dictionary<ulong, PlayerStats> playerData = JsonSerializer.Deserialize<Dictionary<ulong, PlayerStats>>(jsonStringPlayerData);
        var match = JsonSerializer.Deserialize<MatchStats> (jsonStringMatchData);

        match.Players = playerData;

        return match;
    }
}