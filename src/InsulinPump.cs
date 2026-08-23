using CUCoreLib.Helpers;
using UnityEngine;

namespace Diabetes;

abstract class Pump : MonoBehaviour
{
    protected Item item = null!;
    protected WaterContainerItem wat = null!;
    float wearCheckTime;
    bool worn;

    // can't use PlayerCamera.main.body because of multiplayer
    protected Body body => GetComponentInParent<Body>();

    void Start()
    {
        item = GetComponent<Item>();
        wat = GetComponent<WaterContainerItem>();
    }

    protected abstract void WhileActive();

    void Update()
    {
        wearCheckTime -= Time.deltaTime;
        if (wearCheckTime < 0f)
        {
            wearCheckTime += 1f;
            worn = body && body.HasWearable(item);
            if (worn && item.battery.hasCharge)
            {
                WhileActive();
            }
        }
    }

    public void GiveManually(Body body)
    {
        if (!worn || !item.battery.hasCharge) return;
        var wat = item.GetComponent<WaterContainerItem>();
        wat.Inject(body.limbs[9], 2f);
        Sound.Play("syringe", body.transform.position);
    }
}


class InsulinPump : Pump
{
    protected override void WhileActive()
    {
        item.battery.DrainCharge(1f / 3600f);
        var status = body.GetStatus<DiabetesStatus>();

        wat.Inject(body.limbs[9], DiabetesStatus.GlycogenolysisRate / DiabetesStatus.ICR);
    }
}

class SmartInsulinPump : Pump
{
    protected override void WhileActive()
    {
        var status = body.GetStatus<DiabetesStatus>();
        var mul = status.bloodSugar switch
        {
            < 4.0f => 0f,
            < 6.0f => 0.4f,
            < 8.0f => 1f,
            < 15.0f => 1.5f,
            < 20.0f => 2f,
            _ => 3f
        };

        item.battery.DrainCharge(1f / 2400f * Mathf.Lerp(mul, 1f, 0.5f));
        wat.Inject(body.limbs[9], DiabetesStatus.GlycogenolysisRate / DiabetesStatus.ICR * mul);
    }
}
