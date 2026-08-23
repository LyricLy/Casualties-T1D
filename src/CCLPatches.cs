using CUCoreLib.Patches;
using UnityEngine;
using HarmonyLib;

namespace Diabetes;

[HarmonyPatch(typeof(Item))]
static class ItemPatches
{
    [HarmonyPatch(nameof(Item.totalWeight), MethodType.Getter)]
    [HarmonyPostfix]
    static void WhyIsThisAFeature(Item __instance, ref float __result)
    {
        if (Items.GetInsulinContainer(__instance) is not null)
        {
            __result = Mathf.Lerp(0.1f, __instance.Stats.weight, __instance.condition);
        }
    }
}

[HarmonyPatch(typeof(ItemRegistryPatches))]
static class ItemRegistryPatchesPatches
{
    // this is so bad
    // CCL wants to change the condition of my item when pretty much anything happens because it wants the
    // condition to reflect the liquid level like other liquid containers
    // so stop it from doing that by saving the condition beforehand and restoring it after

    [HarmonyPatch(nameof(ItemRegistryPatches.ApplyCustomItemComponents))]
    [HarmonyPrefix]
    static void SaveCondition(Item item, out float __state)
    {
        __state = item.condition;
    }

    [HarmonyPatch(nameof(ItemRegistryPatches.ApplyCustomItemComponents))]
    [HarmonyPostfix]
    static void RestorePumpCondition(Item item, float __state)
    {
        if (Items.IsInsulinPump(item))
        {
            item.condition = __state;
        }
    }
}
