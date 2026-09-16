using CUCoreLib.Registries;
using CUCoreLib.Helpers;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Diabetes;

[HarmonyPatch(typeof(WoundView))]
static class WoundViewPatches
{
    static TextMeshProUGUI bloodSugarText = null!;
    static TextMeshProUGUI ketonesText = null!;
    static TextMeshProUGUI insulinSensText = null!;

    static Image bloodSugarIcon = null!;
    static Sprite lowBloodSugar = null!;
    static Sprite normalBloodSugar = null!;
    static Sprite highBloodSugar = null!;

    static TextMeshProUGUI ApeRespiratoryText(GameObject toCopy, string name, Vector2 pos, string key)
    {
        GameObject obj = Object.Instantiate(toCopy, toCopy.transform.parent);
        obj.name = name;
        obj.AddComponent<ContentSizeFitter>().horizontalFit = ContentSizeFitter.FitMode.PreferredSize;

        var rectTransform = obj.GetComponent<RectTransform>();
        rectTransform.pivot = new Vector2(0f, 0.5f);
        rectTransform.anchoredPosition += pos;

        var tooltip = obj.GetComponent<UITooltip>();
        tooltip.localeName = key;
        tooltip.localeDesc = $"{key}dsc";

        return obj.GetComponent<TextMeshProUGUI>();
    }

    static void SetMgDlTooltip()
    {
        var tooltip = bloodSugarText.gameObject.GetComponent<UITooltip>();
        tooltip.localeDesc = ModSettings.UseMgDl ? "hpbloodsugarmgdldsc" : "hpbloodsugardsc";
        tooltip.tipDesc = LocaleRegistry.Get("other", tooltip.localeDesc, null);
    }

    static Image CreateIcon(GameObject toCopy, string name, Component before, Sprite sprite, Vector2 pos)
    {
        var obj = Object.Instantiate(toCopy, before.transform.parent);

        var rectTransform = obj.GetComponent<RectTransform>();
        obj.transform.position = before.gameObject.transform.position;
        rectTransform.anchoredPosition += pos;
        rectTransform.sizeDelta = sprite.rect.size * 3f;

        var image = obj.GetComponent<Image>();
        image.sprite = sprite;
        return image;
    }

    public static void ToggleInsulinSens()
    {
        GameObject obj = insulinSensText.gameObject;
        obj.SetActive(!obj.activeSelf);
    }

    [HarmonyPatch(nameof(WoundView.Awake))]
    [HarmonyPostfix]
    static void AddDiabetesInfo(WoundView __instance)
    {
        GameObject toCopy = __instance.respiratoryText.gameObject;
        GameObject iconToCopy = __instance.happinessIcon.gameObject;
    
        bloodSugarText = ApeRespiratoryText(
            toCopy,
            "BloodSugarText",
            new Vector2(74f, 34f),
            "hpbloodsugar"
        );
        bloodSugarText.fontSize = 28;
        SetMgDlTooltip();
        ModSettings.MgDlChanged += (_, _) => SetMgDlTooltip();

        lowBloodSugar = AssetLoader.LoadEmbeddedSprite("hpbglow.png");
        normalBloodSugar = AssetLoader.LoadEmbeddedSprite("hpbgnormal.png");
        highBloodSugar = AssetLoader.LoadEmbeddedSprite("hpbghigh.png");
        bloodSugarIcon = CreateIcon(
            iconToCopy,
            "BloodSugarIcon",
            bloodSugarText,
            normalBloodSugar,
            new Vector2(-20f, 0f)
        );

        ketonesText = ApeRespiratoryText(
            toCopy,
            "KetonesText",
            new Vector2(74f, -4f),
            "hpketones"
        );
        ketonesText.fontSize = 22;

        CreateIcon(
            iconToCopy,
            "KetonesIcon",
            ketonesText,
            AssetLoader.LoadEmbeddedSprite("hpketones.png"),
            new Vector2(-18f, 0f)
        );

        insulinSensText = ApeRespiratoryText(
            toCopy,
            "InsulinSensText",
            new Vector2(74f, 60f),
            "hpinsulinsens"
        );
        insulinSensText.gameObject.SetActive(false);
    }

    [HarmonyPatch(nameof(WoundView.UpdateView))]
    [HarmonyPostfix]
    static void UpdateDiabetesInfo(WoundView __instance)
    {
        var status = __instance.body.GetStatus<DiabetesStatus>();

        bloodSugarText.text = status.DisplaySugar();

        __instance.FlashText(bloodSugarText, status.bloodSugar < 3.0f || status.bloodSugar > 20.0f);
        if (status.bloodSugar <= 4.0f)
        {
            bloodSugarIcon.sprite = lowBloodSugar;
        }
        else if (status.bloodSugar <= 10.0f)
        {
            bloodSugarIcon.sprite = normalBloodSugar;
        }
        else
        {
            bloodSugarIcon.sprite = highBloodSugar;
        }

        ketonesText.text = $"{status.ketones:F1}mmol/l";
        __instance.FlashText(ketonesText, status.ketones >= 1.0f);

        insulinSensText.text = $"{status.insulinSensitivity:F2}";
    }
}
