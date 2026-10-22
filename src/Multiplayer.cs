using Together;
using CasualtiesTogetherUtils;
using HarmonyLib;
using System;

namespace Diabetes;

[System.AttributeUsage(System.AttributeTargets.Class)]
class MultiplayerPatchAttribute : Attribute
{}

[HarmonyPatch(typeof(NetBody))]
[MultiplayerPatch]
static class NetBodyPatches
{
    [HarmonyPatch(nameof(NetBody.CreateNewPlayerCharacter))]
    [HarmonyPrefix]
    static void IsThisPlayerNew(ScavPlayer plr, out bool __state)
    {
        __state = plr.body is null;
    }

    [HarmonyPatch(nameof(NetBody.CreateNewPlayerCharacter))]
    [HarmonyPostfix]
    static void GiveSpawningItems(ScavPlayer plr, bool __state)
    {
        if (__state)
        {
            StartingItems.Give(plr.body);
        }
    }
}

[HarmonyPatch(typeof(Util_BodyExtensions))]
[MultiplayerPatch]
static class Util_BodyExtensionsPatches
{
    [HarmonyPatch(nameof(Util_BodyExtensions.ResetHealth))]
    [HarmonyPostfix]
    static void ResetDiabetes(Body body)
    {
        body.DiabetesHeal();
    }
}
