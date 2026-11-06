using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using BepInEx;

namespace RenameWorlds;

/// Custom world names, by WorldGuid, in BepInEx/config/FabuleosWorldNamer.json.
internal static class WorldNames
{
    public const int MaxLength = 24;
    static readonly string FilePath = Path.Combine(Paths.ConfigPath, "FabuleosWorldNamer.json");
    static Dictionary<string, string> names = new();

    public static int Count => names.Count;

    public static void Load()
    {
        try
        {
            if (File.Exists(FilePath))
                names = JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(FilePath)) ?? new();
        }
        catch (Exception e)
        {
            Plugin.Logger.LogWarning($"Couldn't read {FilePath}: {e.Message}. Starting with no names.");
            names = new();
        }
    }

    public static bool TryGet(string worldGuid, out string name)
    {
        name = null;
        return !string.IsNullOrEmpty(worldGuid) && names.TryGetValue(worldGuid, out name) && !string.IsNullOrWhiteSpace(name);
    }

    /// An empty name removes the custom name (the slot shows its map again).
    public static void Set(string worldGuid, string name)
    {
        if (string.IsNullOrEmpty(worldGuid)) return;
        name = Clean(name);
        if (name.Length == 0) names.Remove(worldGuid); else names[worldGuid] = name;
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
            var tmp = FilePath + ".tmp";
            File.WriteAllText(tmp, JsonSerializer.Serialize(names, new JsonSerializerOptions { WriteIndented = true }));
            File.Move(tmp, FilePath, true);
        }
        catch (Exception e)
        {
            Plugin.Logger.LogError($"Couldn't save world names: {e.Message}");
        }
    }

    public static string Clean(string name)
    {
        name = (name ?? "").Replace("\n", " ").Replace("\r", " ").Trim();
        return name.Length > MaxLength ? name.Substring(0, MaxLength).Trim() : name;
    }
}
