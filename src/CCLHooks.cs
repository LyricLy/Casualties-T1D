using CUCoreLib.Helpers;
using UnityEngine;

namespace Diabetes;

static class CCLHooks
{
    public static void AddHooks()
    {
        CUCoreUtils.OnHeal += () =>
        {
            var status = CUCoreUtils.EventPlayer.GetStatus<DiabetesStatus>();
            status.bloodSugar = 6.0f;
            status.rateOfChange = 0f;
            status.ketones = 0.0f;
            status.insulinSensitivity = 1f;
            status.insulinSaturation = 4f;
            status.boluses.Clear();
            status.meals.Clear();
        };

        CUCoreUtils.OnLastStand += () =>
        {
            var status = CUCoreUtils.EventPlayer.GetStatus<DiabetesStatus>();
            status.bloodSugar = Mathf.Lerp(status.bloodSugar, 6.0f, 0.85f);
            status.ketones *= 0.15f;
            status.insulinSaturation = 4f;
            status.honeymoonTime = 600f;
        };
    }
}
