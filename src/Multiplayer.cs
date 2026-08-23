using CUCoreLib.Helpers;
using KrokoshaCasualtiesMP;
using HarmonyLib;
using UnityEngine;
using System;
using System.Collections.Generic;
using LiteNetLib;
using LiteNetLib.Utils;

namespace Diabetes;

struct DiabetesStatusSyncPacket : INetSerializeByMemcpy
{
    float bloodSugar;
    float rateOfChange;
    float ketones;
    float insulinSensitivity;
    float healingRate;
    float insulinSaturation;
    float simSpeed;
    float honeymoonTime;

    public DiabetesStatusSyncPacket(DiabetesStatus status)
    {
        bloodSugar = status.bloodSugar;
        rateOfChange = status.rateOfChange;
        ketones = status.ketones;
        insulinSensitivity = status.insulinSensitivity;
        healingRate = status.healingRate;
        insulinSaturation = status.insulinSaturation;
        simSpeed = status.simSpeed;
        honeymoonTime = status.honeymoonTime;
    }

    public void Apply(DiabetesStatus status)
    {
        status.bloodSugar = bloodSugar;
        status.rateOfChange = rateOfChange;
        status.ketones = ketones;
        status.insulinSensitivity = insulinSensitivity;
        status.healingRate = healingRate;
        status.insulinSaturation = insulinSaturation;
        status.simSpeed = simSpeed;
        status.honeymoonTime = honeymoonTime;
    }
}

[System.AttributeUsage(System.AttributeTargets.Class)]
class MultiplayerPatchAttribute : Attribute
{}

class Multiplayer
{
    public static void Init()
    {
        ClientMain.RegisterClientReceiver(50200, ReceiveDiabetesStatus, ignorehost: true);
    }

    static void ReceiveDiabetesStatus(knetid _, ref NetDataReader reader)
    {
        reader = reader.DecompressReader();
        reader.Get(out knetid bodyId);
        reader.Get(out DiabetesStatusSyncPacket packet);
        var boluses = reader.GetArray<EffectOverTime>();
        var meals = reader.GetArray<EffectOverTime>();
        if (
            NetBody.TryGetNetBodyFromId(bodyId, out var nb)
            && nb._last_sync_health_packet_receive_time <= Time.realtimeSinceStartupAsDouble
        )
        {
            var status = nb.body.GetStatus<DiabetesStatus>();
            packet.Apply(status);
            status.boluses = new List<EffectOverTime>(boluses);
            status.meals = new List<EffectOverTime>(meals);
        }
    }
}

[HarmonyPatch(typeof(MedicalSync))]
[MultiplayerPatch]
class MedicalSyncPatches
{
    [HarmonyPatch(nameof(MedicalSync.Server_SendCharacterHealth))]
    [HarmonyPostfix]
    static void SendDiabetesStatus(NetBody nb, bool force)
    {
        if (force || MedicalSync.CanSyncHealth())
        {
            NetDataWriter writer = Net.CreateWriter(50200);
            var status = nb.body.GetStatus<DiabetesStatus>();
            writer.Put(nb.netId);
            writer.Put(new DiabetesStatusSyncPacket(status));
            writer.PutArray(status.boluses.ToArray());
            writer.PutArray(status.meals.ToArray());
            writer.CompressWriter();
            Net.Server_SendToClients(DeliveryMethod.Unreliable, in writer, ServerMain.AllClientIdsExceptHost);
        }
    }
}

[HarmonyPatch(typeof(NetBody))]
[MultiplayerPatch]
class NetBodyPatches
{
    [HarmonyPatch(nameof(NetBody.CreateNewPlayerCharacter))]
    [HarmonyPrefix]
    static void IsThisPlayerNew(NetPlayer plr, out bool __state)
    {
        __state = plr.body is null;
    }

    [HarmonyPatch(nameof(NetBody.CreateNewPlayerCharacter))]
    [HarmonyPostfix]
    static void GiveSpawningItems(NetPlayer plr, bool __state)
    {
        if (__state)
        {
            StartingItems.Give(plr.body);
        }
    }
}
