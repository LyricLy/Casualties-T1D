using CUCoreLib.Helpers;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering.Universal;
using System.Collections.Generic;
using System.Linq;

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
    static void MarkClearPoint(PlayerCamera __instance)
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
}
