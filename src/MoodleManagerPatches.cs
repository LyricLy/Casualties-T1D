using CUCoreLib.Helpers;
using CUCoreLib.Registries;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace Diabetes;

[HarmonyPatch(typeof(MoodleManager))]
static class MoodleManagerPatches
{
    [HarmonyPatch(nameof(MoodleManager.Awake))]
    [HarmonyPostfix]
    public static void AddIcons(MoodleManager __instance)
    {
        foreach (var key in (List<string>) ["ketoacidosis", "hypoglycemia", "hyperglycemia", "honeymoon"])
        {
            __instance.icons[key] = MoodleRegistry.NormalizeIconSprite(
                AssetLoader.LoadEmbeddedSprite($"{key}.png")
            );
        }
    }

    static void AddMoodle(
        MoodleManager manager,
        int intensity,
        string key,
        string level,
        bool critical = false,
        bool chippedOnly = false
    )
    {
        manager.AddMoodle(
            intensity,
            key,
            LocaleRegistry.Get("moodle", $"{key}{level}", null),
            LocaleRegistry.Get("moodle", $"{key}{level}dsc", null),
            critical,
            chippedOnly
        );
    }

    public static void AddMoodles(MoodleManager manager)
    {
        var status = manager.body.GetStatus<DiabetesStatus>();

        if (status.honeymoonTime > 0f)
        {
            AddMoodle(manager, 7, "honeymoon", "", chippedOnly: true);
        }

        if (status.ketones >= 4.5f)
        {
            AddMoodle(manager, 3, "ketoacidosis", "4", critical: true);
        }
        else if (status.ketones >= 1.5f)
        {
            AddMoodle(manager, 2, "ketoacidosis", "3", critical: status.ketones >= 3.0);
        }
        else if (status.ketones >= 0.6f)
        {
            AddMoodle(manager, 1, "ketoacidosis", "2", chippedOnly: true);
        }
        else if (status.ketones >= 0.2f)
        {
            AddMoodle(manager, 0, "ketoacidosis", "1", chippedOnly: true);
        }

        if (status.bloodSugar <= 0.8f)
        {
            AddMoodle(manager, 3, "hypoglycemia", "4", critical: true);
        }
        else if (status.bloodSugar <= 1.9f)
        {
            AddMoodle(manager, 2, "hypoglycemia", "3", critical: status.bloodSugar <= 1.1);
        }
        else if (status.bloodSugar <= 2.5f)
        {
            AddMoodle(manager, 1, "hypoglycemia", "2");
        }
        else if (status.bloodSugar <= 3.9f)
        {
            AddMoodle(manager, 0, "hypoglycemia", "1");
        }

        if (status.bloodSugar >= 40.0f)
        {
            AddMoodle(manager, 3, "hyperglycemia", "4", chippedOnly: true, critical: true);
        }
        else if (status.bloodSugar >= 24.0f)
        {
            AddMoodle(manager, 2, "hyperglycemia", "3", chippedOnly: true);
        }
        else if (status.bloodSugar >= 16.7f)
        {
            AddMoodle(manager, 1, "hyperglycemia", "2", chippedOnly: true);
        }
        else if (status.bloodSugar >= 10.0f)
        {
            AddMoodle(manager, 0, "hyperglycemia", "1", chippedOnly: true);
        }
    }

    [HarmonyPatch(nameof(MoodleManager.AddAllMoodles))]
    [HarmonyTranspiler]
    static IEnumerable<CodeInstruction> InsertOurMoodles(IEnumerable<CodeInstruction> instructions)
    {
        int stage = 0;

        foreach (CodeInstruction inst in instructions)
        {
            if (stage == 0 && inst.opcode == OpCodes.Ldstr && (string) inst.operand == "hypertension1")
            {
                stage++;
            }
            else if (stage == 1 && inst.Calls(
                typeof(MoodleManager).GetMethod(nameof(MoodleManager.AddMoodle)
            )))
            {
                stage++;
            }
            else if (stage == 2)
            {
                stage++;

                // make sure the jump over the blood pressure moodles doesn't jump over this
                var start = new CodeInstruction(OpCodes.Ldarg_0);
                start.labels = inst.labels;
                inst.labels = [];
                yield return start;

                yield return CodeInstruction.Call(
                    typeof(MoodleManagerPatches),
                    nameof(AddMoodles)
                );
            }

            yield return inst;
        }
    }
}
