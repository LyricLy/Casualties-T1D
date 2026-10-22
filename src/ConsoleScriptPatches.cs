using CUCoreLib.Helpers;
using HarmonyLib;
using System.ComponentModel;
using System.Reflection;
using System.Linq;

namespace Diabetes;

[HarmonyPatch(typeof(ConsoleScript))]
static class ConsoleScriptPatches
{
    [HarmonyPatch(nameof(ConsoleScript.RegisterAllCommands))]
    [HarmonyPostfix]
    static void DoCommandChanges(ConsoleScript __instance)
    {
        ConsoleScript.Commands.Add(new Command(
            "toggleinsulinsensitivity",
            "Show/hide the display in the health panel of how effective insulin is, for debugging purposes.",
            (string[] args) =>
            {
                bool r = WoundViewPatches.ToggleInsulinSens();
                __instance.LogToConsole($"{(r ? "Enabled" : "Disabled")} insulin sensitivity display.");
            },
            null
        ));
    }
}
