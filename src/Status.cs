using CUCoreLib.Data;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Diabetes;

class DiabetesStatus : BodyStatus
{
    public static float ICR => WorldGeneration.GetRunSettingFloat("icr");
    public static float ISF => WorldGeneration.GetRunSettingFloat("isf");

    public float bloodSugar = 6.0f;
    public float rateOfChange;
    public float ketones;
    public float insulinSensitivity = 1f;
    public float healingRate = 1f;
    public float insulinSaturation = 4f;
    public float simSpeed = 1f;
    public float honeymoonTime;
    public List<EffectOverTime> boluses = [new EffectOverTime(0.5f, 0.015f, 0.0001f)];
    public List<EffectOverTime> meals = [];
    public bool hadSweetUrine;

    public float TotalInsulin =>
        meals.Select(e => e.effectRemaining).Sum()
        - boluses.Select(e => e.effectRemaining).Sum()
        * insulinSensitivity;

    public static float GlycogenolysisRate => 0.03f * WorldGeneration.GetRunSettingFloat("glycogenolysisrate");
    public float PancreasEffect
    {
        get
        {
            var val = 0.05f * WorldGeneration.GetRunSettingFloat("pancreaseffect");
            return honeymoonTime > 0f ? Mathf.Lerp(val, 1f, 0.25f) : val;
        }
    }

    public float SugarIn(float seconds = 90f) => bloodSugar + seconds * rateOfChange;
    public float SugarBelow(float level = 4.0f) => Mathf.Max(level - bloodSugar, 0f);
    public float SugarAbove(float level = 10.0f) => Mathf.Max(bloodSugar - level, 0f);
    public float KetonesAbove(float level) => Mathf.Max(ketones - level, 0f);

    public void LowerBloodSugar(float amount) => boluses.Add(new EffectOverTime(amount, 0.15f, 1.2f));
    public void TakeRapidInsulin(float amount) => boluses.Add(new EffectOverTime(amount * ISF, 0.02f, 9f));
    public void TakeLongInsulin(float amount) => boluses.Add(new EffectOverTime(amount * ISF, 0.002f, 110f));

    public void RaiseBloodSugar(float amount) => meals.Add(new EffectOverTime(amount, 0.15f, 1.2f));
    public void EatFastFood(float carbs) => meals.Add(new EffectOverTime(carbs / ICR * ISF, 0.03f, 5f));
    public void EatSlowFood(float carbs) => meals.Add(new EffectOverTime(carbs / ICR * ISF, 0.005f, 40f));
}
