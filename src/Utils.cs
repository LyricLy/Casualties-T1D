using UnityEngine;

namespace Diabetes;

static class Utils
{
    public static float Approach(float a, float b, float hl, float? dt = null) =>
        Mathf.Lerp(a, b, 1.0f - Mathf.Pow(2, -(dt ?? Time.deltaTime) / hl));

    public static float RaiseToTarget(float a, float target, float rate, float? dt = null)
    {
        return a < target ? Mathf.MoveTowards(a, target, rate * (dt ?? Time.deltaTime)) : a;
    }
}
