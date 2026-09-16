using CUCoreLib.Registries;
using CUCoreLib.Helpers;
using HarmonyLib;
using UnityEngine;
using System.Runtime.CompilerServices;

namespace Diabetes;

class DiabetesWatchData
{
    public float sugarCheckTime;
    public float lastReportedSugar;
}

[HarmonyPatch(typeof(WatchScript))]
class WatchScriptPatches
{
    static ConditionalWeakTable<WatchScript, DiabetesWatchData> extraData = new();

    [HarmonyPatch(nameof(WatchScript.Update))]
    [HarmonyPostfix]
    static void ReportBloodSugar(WatchScript __instance)
    {
        // this is correct here, MP doesn't patch the logic to anything else
        Body body = PlayerCamera.main.body;

        if (
            !__instance.batt.hasCharge
            || Vector2.Distance(__instance.transform.position, body.transform.position) > 30f
        )
        {
        	return;
        }

        DiabetesWatchData data = extraData.GetOrCreateValue(__instance);
        data.sugarCheckTime += Time.deltaTime;

        if (!body.conscious)
        {
        	return;
        }
        var status = body.GetStatus<DiabetesStatus>();

        if (
            data.sugarCheckTime >= 85f
            || data.sugarCheckTime >= 10f && Mathf.Abs(status.bloodSugar - data.lastReportedSugar) > 4f
            || data.sugarCheckTime >= 15f && status.bloodSugar <= 4.0f
        )
        {
            data.sugarCheckTime = 0f;
            data.lastReportedSugar = status.bloodSugar;
            __instance.talker.Talk(LocaleRegistry.Get("other", "watchbloodsugar", null) + status.DisplaySugar());
        }
    }
}
