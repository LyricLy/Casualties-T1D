using CUCoreLib.Helpers;
using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;

namespace Diabetes;

[HarmonyPatch(typeof(WaterContainerItem))]
static class WaterContainerItemPatches
{
    [HarmonyPatch(nameof(WaterContainerItem.Drink))]
    [HarmonyPrefix]
    static void AddDrinkCarbs(WaterContainerItem __instance, Body body, float amount)
    {
        var status = body.GetStatus<DiabetesStatus>();
        List<float> amounts = __instance.CalculateDrain(amount);
        for (int i = 0; i < amounts.Count; i++)
        {
            if (!Carbs.DrinkRatios.TryGetValue(__instance.stack[i].liquidId, out var drink)) continue;
            float carbs = amounts[i] * drink.ratio;
            status.EatFastFood(carbs * drink.ofWhichSugar);
            status.EatSlowFood(carbs * (1f - drink.ofWhichSugar));
        }
    }

    [HarmonyPatch(nameof(WaterContainerItem.UpdateCondition))]
    [HarmonyPrefix]
    static bool PreservePumpCondition(WaterContainerItem __instance)
    {
        return !Items.IsInsulinPump(__instance.item);
    }

    [HarmonyPatch(nameof(WaterContainerItem.Start))]
    [HarmonyPostfix]
    static void RotateInsulinFill(WaterContainerItem __instance)
    {
        if (Items.GetInsulinContainer(__instance.item) is not null)
        {
            __instance.fillRenderer.gameObject.transform.localEulerAngles = new Vector3(0f, 0f, -90f);
        }
    }

    [HarmonyPatch(nameof(WaterContainerItem.OnWillRenderObject))]
    [HarmonyPostfix]
    static void FixFill(WaterContainerItem __instance)
    {
        if (Items.GetInsulinContainer(__instance.item) is InsulinContainerInfo c)
        {
            Material mat = __instance.fillRenderer.sharedMaterial;
            mat.SetFloat("_FillAmount", c.FixFill(mat.GetFloat("_FillAmount")));
        }
    }
}
