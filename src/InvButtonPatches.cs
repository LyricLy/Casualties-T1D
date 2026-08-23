using HarmonyLib;
using UnityEngine.UI;

namespace Diabetes;

[HarmonyPatch(typeof(InvButton))]
static class InvButtonPatches
{
    [HarmonyPatch(nameof(InvButton.UpdateGraphic))]
    [HarmonyPostfix]
    static void UpdateFillProperties(InvButton __instance)
    {
        Item? item = __instance.GetItem();
        if (!item)
        {
            return;
        }

        Items.FixInsulinContainerImage(
            __instance.waterFill,
            Items.GetInsulinContainer(item),
            3f,
            __instance.maxImageSize
        );
    }
}
