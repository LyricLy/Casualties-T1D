using CUCoreLib.Helpers;
using UnityEngine;

namespace Diabetes;

static class CCLHooks
{
    public static void DiabetesHeal(this Body body)
    {
        var status = body.GetStatus<DiabetesStatus>();
        status.bloodSugar = 5.0f;
        status.rateOfChange = 0f;
        status.ketones = 0.0f;
        status.insulinSensitivity = 1f;
        status.insulinSaturation = 4f;
        status.boluses.Clear();
        status.meals.Clear();
    }

    public static void AddHooks()
    {
        CUCoreUtils.OnHeal += () =>
        {
            CUCoreUtils.EventPlayer.DiabetesHeal();
        };

        CUCoreUtils.OnLastStand += () =>
        {
            var status = CUCoreUtils.EventPlayer.GetStatus<DiabetesStatus>();
            status.bloodSugar = Mathf.Lerp(status.bloodSugar, 5.0f, 0.85f);
            status.ketones *= 0.15f;
            status.insulinSaturation = 4f;
            status.honeymoonTime = 600f;
        };
    }
}
