using System;
using System.Collections.Generic;

namespace NEXUS.WindowsForms;

// Deterministic 40x30 worlds: same layout after restart, so collected items remain collected.
public static class NexusWorld
{
    public const int Width = 40;
    public const int Height = 30;

    public static string Generate(int world, string oldMap)
    {
        var tiles = new char[Height, Width];
        var random = new Random(92026 + world * 131);
        for (int y = 0; y < Height; y++)
        for (int x = 0; x < Width; x++)
        {
            double waves = Math.Sin(x * .32 + world) + Math.Cos(y * .39) +
                           .55 * Math.Sin((x + y) * .19);
            bool water = world == 0 ? waves > 1.75 : waves < -1.85;
            tiles[y, x] = water ? '~' : '.';
            if (!water && random.NextDouble() < .075) tiles[y, x] = '#';
        }

        // Preserve the old 10x5 region at its original coordinates for save compatibility.
        var old = oldMap.Split('|');
        for (int y = 0; y < Math.Min(old.Length, 5); y++)
        for (int x = 0; x < Math.Min(old[y].Length, 10); x++)
            tiles[y, x] = old[y][x] == 'P' ? '.' : old[y][x];

        // Guaranteed safe, connected roads from spawn into the wider world.
        for (int x = 2; x < Width - 2; x++) tiles[2, x] = '.';
        for (int y = 2; y < Height - 2; y++) tiles[y, 5] = '.';
        for (int y = 2; y < Height - 2; y++) tiles[y, 23] = '.';
        for (int x = 5; x <= 23; x++) tiles[15, x] = '.';
        for (int x = 23; x < Width - 2; x++) tiles[25, x] = '.';

        // Populate the expanded region with discoverable objectives.
        for (int y = 5; y < Height - 1; y++)
        for (int x = 10; x < Width - 1; x++)
        {
            if (tiles[y, x] != '.') continue;
            double chance = random.NextDouble();
            if (chance < .028) tiles[y, x] = '◆';
            else if (chance < .046) tiles[y, x] = '⚠';
            else if (chance < .054) tiles[y, x] = '●';
        }
        tiles[2, 2] = '.';
        var result = new string[Height];
        for (int y = 0; y < Height; y++)
        {
            var line = new char[Width];
            for (int x = 0; x < Width; x++) line[x] = tiles[y, x];
            result[y] = new string(line);
        }
        return string.Join("|", result);
    }
}
