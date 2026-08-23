using UnityEngine;
using LiteNetLib.Utils;

namespace Diabetes;

class EffectOverTime : INetSerializable {
    const float MIN_RATE = 0.00001f;

    public float effectRemaining { get; private set; }
    float targetRate;
    float accel;
    float currentRate;

    public bool Active => currentRate != 0f;

    public EffectOverTime()
    {}

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

    public void Deserialize(NetDataReader reader)
    {
        reader.Get(out float e);
        reader.Get(out float t);
        reader.Get(out float a);
        reader.Get(out float c);
        effectRemaining = e;
        targetRate = t;
        accel = a;
        currentRate = c;
    }

    public void Serialize(NetDataWriter writer)
    {
        writer.Put(effectRemaining);
        writer.Put(targetRate);
        writer.Put(accel);
        writer.Put(currentRate);
    }
}
