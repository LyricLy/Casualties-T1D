using UnityEngine;

namespace Diabetes;

class EffectOverTime {
    const float MIN_RATE = 0.00001f;

    public float effectRemaining;
    public float targetRate;
    public float accel;
    public float currentRate;

    public bool Active => currentRate != 0f;

    public EffectOverTime(float effect, float targetRate, float accel)
    {
        effectRemaining = effect;
        this.targetRate = targetRate;
        this.accel = accel;
    }

    public float Tick(float dt)
    {
        currentRate = Mathf.Max(
            Utils.Approach(currentRate, effectRemaining * targetRate, accel, dt),
            MIN_RATE
        );

        float toRemove = Mathf.Min(currentRate * dt, effectRemaining);
        effectRemaining -= toRemove;
        return toRemove;
    }
}
