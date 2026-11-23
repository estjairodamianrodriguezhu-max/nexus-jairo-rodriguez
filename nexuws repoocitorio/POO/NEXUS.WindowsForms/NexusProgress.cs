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
    public int Crystals { get; set; }
    public int EnergyUpgrade { get; set; }
    public int StabilityUpgrade { get; set; }
    public bool MusicEnabled { get; set; } = true;
    public int MusicVolume { get; set; } = 30;
    public HashSet<string> CollectedCrystals { get; set; } = new();
    public HashSet<string> ResolvedRisks { get; set; } = new();
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

    private static string ConnectionString =>
        Environment.GetEnvironmentVariable("NEXUS_SQL_CONNECTION")
        ?? @"Server=DESKTOP-KU1KKQB\MSSQLSERVER03;Database=NEXUS;Integrated Security=True;Encrypt=True;TrustServerCertificate=True;Connection Timeout=5";

    private static void EnsureDatabase()
    {
        var builder = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(ConnectionString);
        string database = builder.InitialCatalog;
        builder.InitialCatalog = "master";
        using (var master = new Microsoft.Data.SqlClient.SqlConnection(builder.ConnectionString))
        {
            master.Open();
            using var createDb = master.CreateCommand();
            createDb.CommandText = $"IF DB_ID(N'{database.Replace("'", "''")}') IS NULL CREATE DATABASE [{database.Replace("]", "]]")}]";
            createDb.ExecuteNonQuery();
        }

        using var connection = new Microsoft.Data.SqlClient.SqlConnection(ConnectionString);
        connection.Open();
        using var createTable = connection.CreateCommand();
        createTable.CommandText = @"
IF OBJECT_ID(N'dbo.Partidas', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Partidas
    (
        IdPartida INT IDENTITY(1,1) PRIMARY KEY,
        Nombre NVARCHAR(100) NOT NULL,
        Edad INT NOT NULL,
        Energia INT NOT NULL,
        Estabilidad INT NOT NULL,
        Interferencia INT NOT NULL,
        Cristales INT NOT NULL DEFAULT 0,
        Experiencia INT NOT NULL DEFAULT 0,
        Misiones INT NOT NULL DEFAULT 0,
        ProgresoJson NVARCHAR(MAX) NOT NULL
    );
END";
        createTable.ExecuteNonQuery();
    }

    public static NexusSave? Load()
    {
        try
        {
            EnsureDatabase();
            using var connection = new Microsoft.Data.SqlClient.SqlConnection(ConnectionString);
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT TOP (1) ProgresoJson FROM dbo.Partidas ORDER BY IdPartida DESC";
            var json = command.ExecuteScalar() as string;
            if (!string.IsNullOrWhiteSpace(json))
            {
                var fromSql = JsonSerializer.Deserialize<NexusSave>(json);
                if (fromSql != null && !string.IsNullOrWhiteSpace(fromSql.PlayerName))
                    return Normalize(fromSql);
            }
        }
        catch
        {
            // If SQL Server is temporarily unavailable, keep the local backup usable.
        }

        if (!File.Exists(SavePath)) return null;
        var local = JsonSerializer.Deserialize<NexusSave>(File.ReadAllText(SavePath));
        return local == null || string.IsNullOrWhiteSpace(local.PlayerName) ? null : Normalize(local);
    }

    private static NexusSave Normalize(NexusSave data)
    {
        data.Age = Math.Clamp(data.Age, 18, 80);
        data.Energy = Math.Clamp(data.Energy, 0, 100);
        data.Stability = Math.Clamp(data.Stability, 0, 100);
        data.Interference = Math.Clamp(data.Interference, 0, 100);
        data.Experience = Math.Max(0, data.Experience);
        data.Missions = Math.Max(0, data.Missions);
        data.Scans = Math.Max(0, data.Scans);
        data.Interventions = Math.Max(0, data.Interventions);
        data.RechargeCount = Math.Max(0, data.RechargeCount);
        data.Crystals = Math.Max(0, data.Crystals);
        data.EnergyUpgrade = Math.Clamp(data.EnergyUpgrade, 0, 5);
        data.StabilityUpgrade = Math.Clamp(data.StabilityUpgrade, 0, 5);
        data.MusicVolume = Math.Clamp(data.MusicVolume, 0, 100);
        data.CollectedCrystals ??= new();
        data.ResolvedRisks ??= new();
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
        string json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(temp, json);
        File.Move(temp, SavePath, true);

        EnsureDatabase();
        using var connection = new Microsoft.Data.SqlClient.SqlConnection(ConnectionString);
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = @"
DECLARE @Id INT;
SELECT TOP (1) @Id = IdPartida
FROM dbo.Partidas
WHERE Nombre = @Nombre
ORDER BY IdPartida DESC;

IF @Id IS NULL
BEGIN
    INSERT INTO dbo.Partidas
        (Nombre, Edad, Energia, Estabilidad, Interferencia, Cristales, Experiencia, Misiones, ProgresoJson)
    VALUES
        (@Nombre, @Edad, @Energia, @Estabilidad, @Interferencia, @Cristales, @Experiencia, @Misiones, @ProgresoJson);
END
ELSE
BEGIN
    UPDATE dbo.Partidas
    SET Edad = @Edad,
        Energia = @Energia,
        Estabilidad = @Estabilidad,
        Interferencia = @Interferencia,
        Cristales = @Cristales,
        Experiencia = @Experiencia,
        Misiones = @Misiones,
        ProgresoJson = @ProgresoJson
    WHERE IdPartida = @Id;
END";
        command.Parameters.AddWithValue("@Nombre", data.PlayerName);
        command.Parameters.AddWithValue("@Edad", data.Age);
        command.Parameters.AddWithValue("@Energia", data.Energy);
        command.Parameters.AddWithValue("@Estabilidad", data.Stability);
        command.Parameters.AddWithValue("@Interferencia", data.Interference);
        command.Parameters.AddWithValue("@Cristales", data.Crystals);
        command.Parameters.AddWithValue("@Experiencia", data.Experience);
        command.Parameters.AddWithValue("@Misiones", data.Missions);
        command.Parameters.AddWithValue("@ProgresoJson", json);
        command.ExecuteNonQuery();
    }

    public static void Reset()
    {
        var current = Load();
        EnsureDatabase();
        using (var connection = new Microsoft.Data.SqlClient.SqlConnection(ConnectionString))
        {
            connection.Open();
            using var command = connection.CreateCommand();
            if (current != null && !string.IsNullOrWhiteSpace(current.PlayerName))
            {
                command.CommandText = "DELETE FROM dbo.Partidas WHERE Nombre = @Nombre";
                command.Parameters.AddWithValue("@Nombre", current.PlayerName);
            }
            else
            {
                command.CommandText = @"DELETE FROM dbo.Partidas WHERE IdPartida =
                    (SELECT TOP (1) IdPartida FROM dbo.Partidas ORDER BY IdPartida DESC)";
            }
            command.ExecuteNonQuery();
        }
        if (File.Exists(SavePath)) File.Delete(SavePath);
    }
}
