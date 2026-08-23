using CUCoreLib.Networking;
using HarmonyLib;
using System.Collections;
using UnityEngine;

[HarmonyPatch(typeof(WorldGeneration))]
static class WorldGenerationPatches
{
    [HarmonyPatch(nameof(WorldGeneration.WorldPlacePlayer))]
    [HarmonyPostfix]
    static IEnumerator DiabetesSpawningItems(IEnumerator original, WorldGeneration __instance)
    {
        while (original.MoveNext())
        {
            yield return original.Current;
        }

        StartingItems.Give(__instance.body);
    }
}
