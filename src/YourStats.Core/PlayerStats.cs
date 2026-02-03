namespace YourStats.Core;

public class PlayerStats
{
    public string Name { get; set; }
    public ulong SteamID { get; set; }
    public int Kills { get; set; }
    public int Deaths { get; set; }
    public int Assists { get; set; }
    public int UtilDamage { get; set; }
    public int FirstKills { get; set; }
    public int Team { get; set; }

    public double YSRating { get; set; }
}