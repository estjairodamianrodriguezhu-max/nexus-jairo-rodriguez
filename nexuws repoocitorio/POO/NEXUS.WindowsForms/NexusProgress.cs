using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace NEXUS.WindowsForms;

// The save file is local to the current Windows user, never committed to GitHub.
public sealed class NexusSave
{
    public int Version { get; set; } = 1;
    public string PlayerName { get; set; } = "";
    public int Age { get; set; } = 20;
    public int Energy { get; set; } = 80;
    public int Stability { get; set; } = 90;
    public int Interference { get; set; } = 40;
    public int Experience { get; set; }
    public int Missions { get; set; }
    public int Scans { get; set; }
    public int Interventions { get; set; }
    public int RechargeCount { get; set; }
    public List<string> Log { get; set; } = new();
    public HashSet<string> ScannedCells { get; set; } = new();
    public HashSet<string> Achievements { get; set; } = new();
}

public static class NexusProgress
{
    public static string SavePath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "NEXUS", "partida.json");

    public static int Level(int experience) => Math.Max(1, experience / 100 + 1);
    public static int LevelProgress(int experience) => Math.Clamp(experience % 100, 0, 99);

    public static (string id, string title, string detail, int goal, int current, int reward)[] Missions(NexusSave data)
        => new[]
        {
            ("first", "Primer salto", "Completa 1 inmersión", 1, data.Missions, 60),
            ("scout", "Cartógrafo IRIS", "Escanea 5 sectores diferentes", 5, data.ScannedCells.Count, 80),
            ("stabilize", "Estabilizador", "Interviene 3 veces en anomalías", 3, data.Interventions, 100),
            ("veteran", "Veterano dimensional", "Completa 5 inmersiones", 5, data.Missions, 140)
        };

    public static string[] NewAchievements(NexusSave data)
    {
        var unlocked = new List<string>();
        foreach (var mission in Missions(data))
        {
            if (mission.current >= mission.goal && data.Achievements.Add(mission.id))
            {
                data.Experience += mission.reward;
                unlocked.Add(mission.title + "  (+" + mission.reward + " XP)");
            }
        }
        return unlocked.ToArray();
    }

    public static NexusSave? Load()
    {
        if (!File.Exists(SavePath)) return null;
        var data = JsonSerializer.Deserialize<NexusSave>(File.ReadAllText(SavePath));
        if (data == null || string.IsNullOrWhiteSpace(data.PlayerName)) return null;
        data.Age = Math.Clamp(data.Age, 18, 80);
        data.Energy = Math.Clamp(data.Energy, 0, 100);
        data.Stability = Math.Clamp(data.Stability, 0, 100);
        data.Interference = Math.Clamp(data.Interference, 0, 100);
        data.Experience = Math.Max(0, data.Experience);
        data.Missions = Math.Max(0, data.Missions);
        data.Scans = Math.Max(0, data.Scans);
        data.Interventions = Math.Max(0, data.Interventions);
        data.RechargeCount = Math.Max(0, data.RechargeCount);
        data.Log ??= new();
        data.ScannedCells ??= new();
        data.Achievements ??= new();
        return data;
    }

    public static void Save(NexusSave data)
    {
        string folder = Path.GetDirectoryName(SavePath)!;
        Directory.CreateDirectory(folder);
        string temp = SavePath + ".tmp";
        File.WriteAllText(temp, JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true }));
        File.Move(temp, SavePath, true);
    }
}
