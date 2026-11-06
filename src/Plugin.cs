using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using Il2CppInterop.Runtime.Injection;
using UnityEngine;

namespace RenameWorlds;

/// Fabuleo's World Namer for Flipping is Hard: a pencil next to each world's delete button opens a Rename box.
/// The game itself has no world names (a slot shows its map), so names are kept by this mod in
/// BepInEx/config/FabuleosWorldNamer.json, keyed by each world's WorldGuid. Saves are never touched.
[BepInPlugin(Guid, "Fabuleo's World Namer", "1.0.0")]
public class Plugin : BasePlugin
{
    public const string Guid = "com.fabuleo8796.worldnamer";
    internal static ManualLogSource Logger;

    public override void Load()
    {
        Logger = Log;
        WorldNames.Load();
        ClassInjector.RegisterTypeInIl2Cpp<RenameDriver>();
        var host = new GameObject("RenameWorlds");
        Object.DontDestroyOnLoad(host);
        host.hideFlags = HideFlags.HideAndDontSave;
        host.AddComponent<RenameDriver>();
        new Harmony(Guid).PatchAll(typeof(SlotPatches));
        Log.LogInfo($"Fabuleo's World Namer loaded ({WorldNames.Count} custom name(s))");
    }
}
