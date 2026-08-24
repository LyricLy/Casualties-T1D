using BepInEx;
using BepInEx.Logging;
using CUCoreLib.ContentReload;
using CUCoreLib.Networking;
using HarmonyLib;
using System;
using System.Reflection;

namespace Diabetes;

[BepInPlugin(GUID, Name, Version)]
[BepInDependency("net.cucorelib", BepInDependency.DependencyFlags.HardDependency)] 
public class Diabetes : BaseUnityPlugin
{
    public const string Name = "Diabetes";
    public const string Version = "1.1.1";
    public const string GUID = "com.lyricly.diabetes";

    internal static new ManualLogSource Logger = null!;
    readonly Harmony _harmony = new(GUID);

    public void Awake()
    {
        Logger = base.Logger;

        foreach (Type type in AccessTools.GetTypesFromAssembly(Assembly.GetExecutingAssembly()))
        {
            var isMulti = Attribute.GetCustomAttribute(type, typeof(MultiplayerPatchAttribute)) is not null;
            if (!isMulti || MultiplayerApi.IsAvailable)
            {
                _harmony.CreateClassProcessor(type).Patch();
            }
        }
        Logger.LogInfo($"Plugin {Name} is loaded!");

        if (MultiplayerApi.IsAvailable)
        {
            Multiplayer.Init();
            Logger.LogInfo("KrokMP detected, multiplayer patch applied");
        }

        ContentReloadManager.EnableHotReload(GUID);
        Items.AddLiquids();
        Items.AddItems();
        Items.AddRecipes();
        ModSettings.AddSettings();
        CustomRunSettings.AddRunSettings();
        CCLHooks.AddHooks();
    }
}
