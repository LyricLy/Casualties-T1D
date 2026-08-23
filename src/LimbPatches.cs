using CUCoreLib.Helpers;
using HarmonyLib;
using UnityEngine;

namespace Diabetes;

[HarmonyPatch(typeof(Limb))]
static class LimbPatches
{
    extension(Limb limb)
    {
        float HealingRate => limb.body.GetStatus<DiabetesStatus>().healingRate;
    }

    [HarmonyPatch(nameof(Limb.MuscleHealRate), MethodType.Getter)]
    [HarmonyPostfix]
    static void HealMuscleSlower(Limb __instance, ref float __result)
    {
        __result *= __instance.HealingRate;
    }

    [HarmonyPatch(nameof(Limb.SkinHealRate), MethodType.Getter)]
    [HarmonyPostfix]
    static void HealSkinSlower(Limb __instance, ref float __result)
    {
        __result *= __instance.HealingRate;
    }

    [HarmonyPatch(nameof(Limb.injuryHealTime), MethodType.Getter)]
    [HarmonyPostfix]
    static void ShowSlowerHealRate(Limb __instance, ref float __result)
    {
        __result /= __instance.HealingRate;
    }

    [HarmonyPatch(nameof(Limb.Update))]
    [HarmonyPrefix]
    static void HealBoneSlower(Limb __instance)
    {
        // it's about to be decreased by the original code, so increase it enough to offset that
        __instance.dislocationTimer +=
            (1f - __instance.HealingRate)
            * Limb.dislocationHealSpeed
            * WorldGeneration.GetRunSettingFloat("healingrate")
            * Time.deltaTime;
        __instance.boneHealTimer +=
            (1f - __instance.HealingRate)
            * Limb.boneHealSpeed
            * WorldGeneration.GetRunSettingFloat("healingrate")
            * Time.deltaTime;
    }

    [HarmonyPatch(nameof(Limb.totalForce), MethodType.Getter)]
    [HarmonyPostfix]
    static void WeakenArms(Limb __instance, ref float __result)
    {
        if (__instance.isArm)
        {
            __result /= 1f + __instance.body.GetStatus<DiabetesStatus>().SugarBelow();
        }
    }
}
