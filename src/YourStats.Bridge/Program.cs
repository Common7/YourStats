// See https://aka.ms/new-console-template for more information

using System.Diagnostics;
using System.Runtime.InteropServices;
using YourStats.Bridge;
using YourStats.Data;

class Program
{
    static async Task Main()
    {
        //if (args.Length == 0)
        //{
            //Console.WriteLine("Error: No demo file provided");
            //return;
        //}
        //string demoPath = args[0];
        
        Console.WriteLine("Enter the path of the demo file.");
        string demoPath = Console.ReadLine();

        var db = new DatabaseService("stats.db");
        var bridge = new BridgeService();
        
        string platform = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "main.exe" : "./main";

        var parser = new ProcessStartInfo
        {
            FileName = platform,
            Arguments = demoPath,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            CreateNoWindow = true
        };

        using (Process p = Process.Start(parser))
        {
            p.WaitForExit();
        }
        
        var completedMatch = bridge.GetPlayerStats("result.json", "match.json");
        
        db.SaveMatchData(completedMatch);
    }
}