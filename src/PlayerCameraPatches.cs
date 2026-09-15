using CUCoreLib.Registries;
using CUCoreLib.Helpers;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering.Universal;
using System.Collections.Generic;

namespace Diabetes;

[HarmonyPatch(typeof(PlayerCamera))]
static class PlayerCameraPatches
{
    [HarmonyPatch(nameof(PlayerCamera.HandleScreenShaders))]
    [HarmonyPostfix]
    static void HandleHypoglycemiaShader(PlayerCamera __instance)
    {
        var status = __instance.body.GetStatus<DiabetesStatus>();

        if (__instance.volume.profile.TryGet<Bloom>(out var bloom))
        {
            bloom.intensity.value = 1f + 30f * status.SugarBelow(3.5f);
        }
    }

    [HarmonyPatch(nameof(PlayerCamera.ClearTraderInventories))]
    [HarmonyPostfix]
    static void DetachClearedChildren(PlayerCamera __instance)
    {
        __instance.traderInventory.DetachChildren();
    }

    [HarmonyPatch(nameof(PlayerCamera.RefreshTraderInventories))]
    [HarmonyPostfix]
    static void FixFill(PlayerCamera __instance)
    {
        // this is so awful but so is the original code
        int i = 0;
        foreach (Transform child in __instance.traderInventory)
        {
            GameObject obj = child.gameObject;
            if (obj.name != "TraderItemPanel(Clone)")
            {
                continue;
            }

            TraderItem item = __instance.currentTrader.items[i];
            Items.FixInsulinContainerImage(
                obj.transform.GetChild(0).GetComponent<Image>(),
                Items.GetInsulinContainerByID(item.id),
                8f,
                64f
            );

            i++;
        }
    }

    [HarmonyPatch(nameof(PlayerCamera.ItemHoverDescription))]
    [HarmonyPrefix]
    static void SetRapidInsulinMgDl(Item item)
    {
        if (item.id == "rapidinsulin")
        {
            Liquids.Registry[item.id].localeName =
                ModSettings.UseMgDl ? "rapidinsulinmgdl" : "rapidinsulin";
        }
    }

    static RangeF EstimateCarbs(DiabetesStatus status, (string, bool) key, float truth)
    {
        if (!status.carbEstimations.TryGetValue(key, out RangeF curEst))
        {
            curEst = new RangeF(0f, 1f);
        }

        float estDiameter = 1f / Mathf.Pow(1.3f, (float) PlayerCamera.main.body.skills.INT);
        float toNarrow = curEst.max - curEst.min - estDiameter;
        if (toNarrow > 0.001f)
        {
            float balance = Random.Range(
                Mathf.Max(1f - (truth - curEst.min) / toNarrow, 0f),
                Mathf.Min((curEst.max - truth) / toNarrow, 1f)
            );
            curEst.min += (1f - balance) * toNarrow;
            curEst.max -= balance * toNarrow;
            status.carbEstimations[key] = curEst;
        }

        return curEst;
    }

    [HarmonyPatch(nameof(PlayerCamera.ItemHoverDescription))]
    [HarmonyPostfix]
    static void DisplayCarbs(Item item, ref (string, string) __result)
    {
        if (
            !item
            || !item.Stats.rec.recognizable
            || !Input.GetKey(KeyBinds.GetBind("expanddesc")) && !PlayerCamera.alwaysExpandDescriptions
        )
        {
            return;
        }

        var status = PlayerCamera.main.body.GetStatus<DiabetesStatus>();

        RangeF finalEst;

        if (Carbs.FoodRatios.TryGetValue(item.id, out var food))
        {
            finalEst =
                EstimateCarbs(status, (item.id, false), food.ratio)
                * item.condition
                * item.Stats.weight
                * Carbs.GramsPerUnit;
        }
        else if (item.TryGetComponent<WaterContainerItem>(out var wat))
        {
            finalEst = new(0f, 0f);
            foreach (LiquidStack stack in wat.stack)
            {
                if (!Carbs.DrinkRatios.TryGetValue(stack.liquidId, out var drink)) continue;
                // + for RangeF is wrong lol
                var toAdd = EstimateCarbs(status, (stack.liquidId, true), drink.ratio) * stack.amount;
                finalEst = new(finalEst.min + toAdd.min, finalEst.max + toAdd.max);
            }
        }
        else
        {
            return;
        }

        int lowEst = Mathf.RoundToInt(finalEst.min);
        int highEst = Mathf.RoundToInt(finalEst.max);
        string carbsAre = LocaleRegistry.Get("other", "carbs", null);
        string carbCount = lowEst == highEst ? $"{lowEst}" : $"{lowEst}-{highEst}";
        __result.Item2 += $"<color=#ff8fb0><sprite index=2 tint=1>{carbsAre}{carbCount}g";
    }
}
