using CUCoreLib.Helpers;
using HarmonyLib;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;

namespace Diabetes;

[HarmonyPatch(typeof(Body))]
static class BodyPatches
{
    static float timeSincePancreas;

    extension(Body body)
    {
        float TotalInfection => body.limbs.Where(l => !l.dismembered).Select(l => l.infectionAmount).Sum();
    }

    [HarmonyPatch(nameof(Body.Update))]
    [HarmonyPrefix]
    static void UpdateBloodSugar(Body __instance)
    {
        var status = __instance.GetStatus<DiabetesStatus>();

        status.simSpeed = Utils.Approach(
            status.simSpeed,
            status.SugarIn(60f) >= 4.0f ? 1f : WorldGeneration.GetRunSettingFloat("hypotimescale"),
            5f
        );
        float simTime = status.simSpeed * Time.deltaTime;

        float startingSugar = status.bloodSugar;
        status.bloodSugar += DiabetesStatus.GlycogenolysisRate * simTime;

        timeSincePancreas += simTime;
        if (timeSincePancreas >= 30f)
        {
            timeSincePancreas = 0f;

            float predictedBloodSugar = status.bloodSugar + status.TotalInsulin;
            if (predictedBloodSugar > 5.6f)
            {
                var toGive = (predictedBloodSugar - 4.0f) * status.PancreasEffect / 2f;
                status.LowerBloodSugar(toGive);
            }
            else if (predictedBloodSugar < 4.4f)
            {
                status.RaiseBloodSugar((6.0f - predictedBloodSugar) * status.PancreasEffect / 2f);
            }
        }

        foreach (EffectOverTime effect in status.boluses)
        {
            float amount = status.insulinSensitivity * effect.Tick(simTime);
            status.insulinSaturation += amount;
            status.bloodSugar -= amount;
        }
        status.boluses.RemoveAll(e => !e.Active);

        foreach (EffectOverTime effect in status.meals)
        {
            status.bloodSugar += effect.Tick(simTime);
        }
        status.meals.RemoveAll(e => !e.Active);

        status.insulinSaturation = Mathf.Clamp(
            status.insulinSaturation - 0.75f * DiabetesStatus.GlycogenolysisRate * simTime,
            0f,
            4f
        );
        if (__instance.hunger <= 5f)
        {
            status.ketones += 0.0035f * Time.deltaTime;
        }
        else if (status.insulinSaturation <= 1f)
        {
            status.ketones += 0.0035f * Time.deltaTime;
            __instance.weightOffset -= 0.006f
                * WorldGeneration.GetRunSettingFloat("metabolismrate")
                * Time.deltaTime;
        }
        else
        {
            status.ketones -= 0.005f * Time.deltaTime;
        }

        status.bloodSugar = Mathf.Clamp(status.bloodSugar, 0f, 50f);
        status.ketones = Mathf.Clamp(status.ketones, 0f, 6f);
        status.rateOfChange = Utils.Approach(
            status.rateOfChange,
            (status.bloodSugar - startingSugar) / Time.deltaTime,
            6f
        );
        status.honeymoonTime -= Time.deltaTime;

        __instance.sicknessAmount = Utils.RaiseToTarget(
            __instance.sicknessAmount,
            25f * status.ketones,
            0.3f
        );
        if (status.ketones >= 4.0)
        {
            __instance.adrenaline -= 2f * Time.deltaTime;
        }
        if (status.ketones >= 5.0)
        {
            __instance.consciousness -= 3.5f * Time.deltaTime;
            __instance.brainHealth -= 0.5f * Time.deltaTime;
            __instance.TryStartFibrillation(forced: true);
        }

        status.insulinSensitivity = Utils.Approach(
            status.insulinSensitivity,
            1f
                * (1f - 0.001f * Mathf.Max(100f - __instance.thirst, 0f))
                * (1f - 0.001f * Mathf.Max(-__instance.happiness, 0f))
                * (1f - 0.0015f * __instance.averagePain)
                * (1f - 0.0025f * Mathf.Clamp(__instance.TotalInfection, 0f, 100f))
                * (1f - 0.0025f * __instance.septicShock)
                * (1f - 0.003f * __instance.adrenaline)
                * (1f - 0.006f * __instance.sicknessAmount)
                * (1f - 0.333f * Mathf.Max(2.5f - status.bloodSugar, 0f))
                * (1f + 0.02f * (100f - __instance.stamina))
                * (1f - 0.008f * __instance.weightOffset)
                * (1f + 0.01f * __instance.tempDiffFromNormal)
            ,
            10f
        );
        status.healingRate = Utils.Approach(status.healingRate, 1f - 0.025f * status.SugarAbove(), 5f);

        __instance.thirst -= 0.2f
            * __instance.BaseThirstRate(__instance.tempDiffFromNormal)
            * status.SugarAbove();
        __instance.hunger -= 0.035f
            * __instance.BaseHungerRate
            * status.SugarAbove();
        __instance.bloodViscosity = Utils.RaiseToTarget(
            __instance.bloodViscosity,
            status.SugarAbove(20f),
            0.1f
        );

        __instance.adrenaline = Utils.RaiseToTarget(
            __instance.adrenaline,
            50f * status.SugarBelow(),
            10f
        );
        __instance.energy -= 0.035f
            * WorldGeneration.GetRunSettingFloat("sleepcyclespeed")
            * status.SugarBelow()
            * Time.deltaTime
        ;
        __instance.happiness -= status.SugarBelow() * 0.02f * Time.deltaTime;
        if (status.bloodSugar <= 0.6f)
        {
            __instance.consciousness -= Time.deltaTime;
            __instance.brainHealth -= 0.2f * Time.deltaTime;
        }
        if (status.bloodSugar <= 0.25f)
        {
            __instance.TryStartFibrillation(forced: true);
        }

        if (
            __instance.sleeping
            && (__instance.energy >= 20f && status.bloodSugar < 3.0f)
            && WorldGeneration.GetRunSettingBool("nosleeprestrictions")
        )
        {
            __instance.WakeUp();
        }
    }

    [HarmonyPatch(nameof(Body.totalHappiness), MethodType.Getter)]
    [HarmonyPostfix]
    static void TempHappiness(Body __instance, ref float __result)
    {
        var status = __instance.GetStatus<DiabetesStatus>();
        __result = Mathf.Clamp(
            __result -
                5f
                * status.SugarBelow()
                * (__instance.mindWipe ? 0f : 1f)
                * (1f - __instance.horrifiedLevel * 0.005f)
            ,
            -100f,
            100f
        );
    }

    [HarmonyPatch(nameof(Body.UseItem))]
    [HarmonyPrefix]
    static void RememberCondition(out float __state, Item item)
    {
        __state = item.condition;
    }

    [HarmonyPatch(nameof(Body.UseItem))]
    [HarmonyPostfix]
    static void AddFoodCarbs(float __state, Body __instance, Item item)
    {
        if (!Carbs.FoodRatios.TryGetValue(item.id, out var food)) return;
        var status = __instance.GetStatus<DiabetesStatus>();
        float carbs = (__state - item.condition) * item.Stats.weight * 450f * food.ratio;
        status.EatFastFood(carbs * food.ofWhichSugar);
        status.EatSlowFood(carbs * (1f - food.ofWhichSugar));
    }

    [HarmonyPatch(nameof(Body.HandleBody))]
    [HarmonyTranspiler]
    static IEnumerable<CodeInstruction> CapConsciousness(IEnumerable<CodeInstruction> instructions)
    {
        foreach (CodeInstruction inst in instructions)
        {
            if (inst.opcode == OpCodes.Stloc_2)
            {
                yield return new CodeInstruction(OpCodes.Ldarg_0);
                yield return CodeInstruction.Call(
                    typeof(BodyPatches),
                    nameof(DoCapConsciousness)
                );
            }

            yield return inst;
        }
    }

    static float DoCapConsciousness(float targetConsciousness, Body body)
    {
        var status = body.GetStatus<DiabetesStatus>();
        return Mathf.Min(
            targetConsciousness,
            100f - 35f * status.KetonesAbove(3.0f),
            12.5f + 21.875f * status.bloodSugar
        );
    }

    [HarmonyPatch(nameof(Body.HandleCirculation))]
    [HarmonyTranspiler]
    static IEnumerable<CodeInstruction> BoostRespiratoryRate(IEnumerable<CodeInstruction> instructions)
    {
        bool done = false;
        int clampStage = 0;
        foreach (CodeInstruction inst in instructions)
        {
            if (!done && inst.opcode == OpCodes.Stloc_2)
            {
                yield return new CodeInstruction(OpCodes.Ldarg_0);
                yield return CodeInstruction.Call(
                    typeof(BodyPatches),
                    nameof(DoBoostRespiratoryRate)
                );
                done = true;
            }

            // increase max respiratory rate
            if (
                clampStage == 0 && inst.LoadsField(typeof(Body).GetField("respiratoryRate"))
                || clampStage == 1 && inst.LoadsConstant(0.0)
            )
            {
                clampStage++;
            }
            else if (clampStage == 2 && inst.LoadsConstant(100.0))
            {
                clampStage++;
                yield return new CodeInstruction(OpCodes.Ldc_R4, 180f);
                continue;
            }
            else
            {
                clampStage = 0;
            }

            yield return inst;
        }
    }

    static float DoBoostRespiratoryRate(float targetRespiratoryRate, Body body)
    {
        var status = body.GetStatus<DiabetesStatus>();
        return targetRespiratoryRate + 32f * status.KetonesAbove(2.5f);
    }

    [HarmonyPatch(nameof(Body.isDying), MethodType.Getter)]
    [HarmonyPostfix]
    static void ConsiderDiabetesDying(Body __instance, ref bool __result)
    {
        if (!__result)
        {
            var status = __instance.GetStatus<DiabetesStatus>();
            __result =
                status.bloodSugar <= 1.1f
                || status.bloodSugar <= 4.5f && status.SugarIn() <= 0.7f
                || status.ketones >= 3.0f;
        }
    }

    [HarmonyPatch(nameof(Body.isCriticallyDying), MethodType.Getter)]
    [HarmonyPostfix]
    static void ConsiderDiabetesCriticalDying(Body __instance, ref bool __result)
    {
        if (!__result)
        {
            var status = __instance.GetStatus<DiabetesStatus>();
            __result =
                status.bloodSugar <= 0.8f
                || status.bloodSugar <= 3.0f && status.SugarIn() <= 0.4f
                || status.ketones >= 4.0f;
        }
    }
}
