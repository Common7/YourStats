using System.Reflection.Emit;

namespace YourStats.Core;

public class MatchStats
{
    public string MapName { get; set; }
    public int CTScore { get; set; }
    public int tScore { get; set; }
    
    public Dictionary<ulong, PlayerStats> Players { get; set; }
}