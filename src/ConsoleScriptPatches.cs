using CUCoreLib.Helpers;
using HarmonyLib;
using System.ComponentModel;
using System.Reflection;
using System.Linq;

namespace Diabetes;

[HarmonyPatch(typeof(ConsoleScript))]
static class ConsoleScriptPatches
{
    static bool diabetesDetailsRegistered;

    [HarmonyPatch(nameof(ConsoleScript.RegisterAllCommands))]
    [HarmonyPostfix]
    static void DoCommandChanges(ConsoleScript __instance)
    {
        Command setbodyfield = ConsoleScript.SearchExact("setbodyfield");
        setbodyfield.action = (string[] args) =>
        {
            __instance.CheckForWorld();
            __instance.CheckArgumentCount(args, 2);
            string text = args[1];

            FieldInfo field = typeof(Body).GetField(text);
            object target = PlayerCamera.main.body;
            if (field is null)
            {
                field = typeof(DiabetesStatus).GetField(text);
                target = PlayerCamera.main.body.GetStatus<DiabetesStatus>();
            }

            object obj = TypeDescriptor.GetConverter(field.FieldType).ConvertFromInvariantString(args[2]);
            field.SetValue(target, obj);
            __instance.LogToConsole($"Set player body field \"{text}\" to \"{obj.ToString()}\".");
        };

        ConsoleScript.Commands.Add(new Command(
            "toggleinsulinsensitivity",
            "Show/hide the display in the health panel of how effective insulin is, for debugging purposes.",
            (string[] args) =>
            {
                WoundViewPatches.ToggleInsulinSens();
            },
            null
        ));
    }

    [HarmonyPatch(nameof(ConsoleScript.RegisterPlayerDetails))]
    [HarmonyPostfix]
    static void RegisterDiabetesDetails()
    {
        if (diabetesDetailsRegistered) return;
        diabetesDetailsRegistered = true;

        Command setbodyfield = ConsoleScript.SearchExact("setbodyfield");
        FieldInfo[] fields = typeof(DiabetesStatus).GetFields(BindingFlags.Instance | BindingFlags.Public);
        setbodyfield.argAutofill[0].AddRange(fields.Where(f => f.FieldType.IsPrimitive).Select(f => f.Name));
    }
}
