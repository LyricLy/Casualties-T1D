using CUCoreLib.Helpers;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

namespace Diabetes;

class EndHook : MonoBehaviour
{
    public GlucoseMeterMinigame? game;

    void OnDestroy()
    {
        if (game is null)
        {
            return;
        }

        game.End();
    }
}

public class GlucoseMeterMinigame : Minigame
{
    const float Delay = 3.5f;

    static Sprite glucoseMeter = null!;
    static Sprite applyBloodOn = null!;
    static Sprite applyBloodOff = null!;
    static Sprite bloodApplied = null!;
    static Sprite bloodDrop = null!;
    static Sprite bloodSquare = null!;
    static AudioClip lancetClick = null!;
    static AudioClip glucoseMeterStart = null!;
    static AudioClip glucoseMeterEnd = null!;
    static TMP_FontAsset? font;

	Image applyBlood = null!;
	Image droppedBlood = null!;
	Image bloodFill = null!;
	TextMeshProUGUI display = null!;
	TextMeshProUGUI unitDisplay = null!;
	TextMeshProUGUI countdown = null!;

    Limb limb;
    Body body => limb.body;
    float delayToBleeding = 0.8f;
    float leftToBleed;
    float bloodOnFinger = 1f;
    float bloodSaturation;
	float flashCounter;
	float progression;
	bool done;
	int oldHandSlot;

    Button handButton = null!;

	public override HandSpriteType HandType()
	{
		return HandSpriteType.Point;
	}

	public override string GuideLocaleString()
	{
		return "glucoseMeterMinigameGuide";
	}

	public override bool NeedsItem()
	{
		return true;
	}

    public override float HandRotOffset()
    {
    	return -3f;
    }

	public GlucoseMeterMinigame(Limb limb)
	{
	    this.limb = limb;
	}

    public static void LoadAssets()
    {
        glucoseMeter = AssetLoader.LoadEmbeddedSprite("minigameglucosemeter.png");
        applyBloodOn = AssetLoader.LoadEmbeddedSprite("minigameapplyblood.png");
        applyBloodOff = AssetLoader.LoadEmbeddedSprite("minigameapplynoblood.png");
        bloodApplied = AssetLoader.LoadEmbeddedSprite("minigamebloodapplied.png");
        bloodDrop = AssetLoader.LoadEmbeddedSprite("minigameblooddrop.png");
        bloodSquare = AssetLoader.LoadEmbeddedSprite("minigamebloodfill.png");

        lancetClick = AssetLoader.LoadEmbeddedAudio("lancetclick.wav");
        glucoseMeterStart = AssetLoader.LoadEmbeddedAudio("glucosemeterstart.wav");
        glucoseMeterEnd = AssetLoader.LoadEmbeddedAudio("glucosemeterend.wav");
    }

    GameObject FormUIObject(string name, Transform parent)
    {
        var obj = new GameObject(name, typeof(RectTransform));
        obj.transform.SetParent(parent);
        var rt = obj.GetComponent<RectTransform>();
        rt.pivot = new(0.5f, 0.5f);
        rt.anchorMin = new(0.5f, 0.5f);
        rt.anchorMax = new(0.5f, 0.5f);
        rt.sizeDelta = new(0f, 0f);
        rt.localPosition = new(0f, 0f, 0f);
        rt.localScale = new(1f, 1f, 1f);
        return obj;
    }

    Image FormImageUIObject(string name, Transform parent, Sprite sprite, Vector2 position, float size)
    {
        var im = FormUIObject(name, parent).AddComponent<Image>();
        im.sprite = sprite;
        im.preserveAspect = true;
        im.rectTransform.anchoredPosition += position;
        im.rectTransform.sizeDelta = new(size, size);
        return im;
    }

    TextMeshProUGUI FormTextUIObject(string name, Transform parent, Vector2 position)
    {
        var obj = FormUIObject(name, parent);
        var rt = obj.GetComponent<RectTransform>();
        rt.pivot = new(1f, 0.5f);
        rt.anchoredPosition += position;
        rt.sizeDelta = new(500f, 0f);
        var text = obj.AddComponent<TextMeshProUGUI>();

        if (!font)
        {
            // I don't know what the good way to do this is
            font = Object.FindObjectsByType<TextMeshProUGUI>(FindObjectsSortMode.None)
                .First(t => t.font.name == "Retro GamingPix")
                .font;
        }
        text.font = font;
        text.alignment = TextAlignmentOptions.BaselineRight;

        return text;
    }

    GameObject CreateScreen(Transform parent)
    {
        GameObject screen = FormUIObject("GlucoseMeterMinigame", parent);

        Image meter = FormImageUIObject(
            "GlucoseMeter",
            screen.transform,
            glucoseMeter,
            new(0f, 100f),
            500f
        );

        bloodFill = FormImageUIObject(
            "BloodFill",
            meter.transform,
            bloodSquare,
            new(0f, -235f - 35f / 69f),
            14f + 34f / 69f
        );
        bloodFill.type = Image.Type.Filled;
        bloodFill.fillMethod = Image.FillMethod.Vertical;
        bloodFill.fillOrigin = (int)Image.OriginVertical.Bottom;

        applyBlood = FormImageUIObject(
            "ApplyBlood",
            meter.transform,
            applyBloodOn,
            new(0f, 65f),
            170f
        );

        display = FormTextUIObject("Display", meter.transform, new(73f, 35f));
        display.fontSize = 57;

        unitDisplay = FormTextUIObject("UnitDisplay", meter.transform, new(73f, 98f));
        unitDisplay.fontSize = 20;

        countdown = FormTextUIObject("Countdown", meter.transform, new(-20f, -48f));
        countdown.fontSize = 22;
        countdown.text = ":05";

        FormTextUIObject("GlucoseMeterDisplay", meter.transform, new(55f, 50f));

        return screen;
    }

	public override void Start()
	{
		GameObject screen = CreateScreen(game.minigameScreen);
		screen.AddComponent<EndHook>().game = this;
		screen.transform.SetSiblingIndex(0);
		game.spawnedMiniGame = screen.transform;

        oldHandSlot = body.handSlot;
        body.handSlot = limb == body.limbs[5] ? 0 : 1;
        handButton = WoundView.view.transform.Find("HandButton").GetComponent<Button>();
        handButton.interactable = false;

        if (limb.bleedAmount <= 0.05f)
        {
            limb.bleedAmount += 0.25f;
            limb.skinHealth -= 0.5f;
            if (Random.value <= 0.1f)
            {
                limb.pain += 20f;
            }
            leftToBleed = 1f;
            bloodOnFinger = 0f;
            Sound.Play(lancetClick, Vector2.zero, twoDimensional: true, pitchShift: false);
        }

        droppedBlood = FormImageUIObject("BloodDrop", game.handTransform, bloodDrop, new(), 0f);
        droppedBlood.rectTransform.pivot = new(1f, 0.5f);
	}

    public void End()
    {
        body.handSlot = oldHandSlot;
        handButton.interactable = true;

        GameObject.Destroy(droppedBlood.gameObject);
    }

	public override void Update(List<RaycastResult> uiCasts)
	{
	    delayToBleeding -= Time.deltaTime;
        if (delayToBleeding <= 0f)
        {
            float toBleed = Mathf.Min(Time.deltaTime / 0.75f, leftToBleed);
            leftToBleed -= toBleed;
            bloodOnFinger += toBleed;
        }

        if (Vector3.Distance(droppedBlood.transform.position, bloodFill.transform.position) < 30f)
        {
            bloodOnFinger = Mathf.Max(bloodOnFinger - Time.deltaTime / 1.2f, 0f);
            bloodSaturation = Mathf.Min(bloodSaturation + Time.deltaTime / 0.84f, 1f);
        }

        if (bloodSaturation == 1f) {
            if (progression == 0f)
            {
                Sound.Play(glucoseMeterStart, Vector2.zero, twoDimensional: true, pitchShift: false);
            }
            progression += Time.deltaTime;
        }

        if (progression >= Delay && !done)
        {
            var status = body.GetStatus<DiabetesStatus>();
            float range = Mathf.Max(status.bloodSugar * 0.1f, 0.5f);
            float bg = status.bloodSugar + Random.Range(-range, range);
            display.text =
                bg > 33.3f ? "HI"
                : bg < 1.1f ? "LO"
                : ModSettings.UseMgDl ? $"{bg * 18:F0}" : $"{bg:F1}";
            unitDisplay.text = ModSettings.UseMgDl ? $"mg/dl" : $"mmol/l";
            game.currentItem.battery.DrainCharge(1f / 90f);
            applyBlood.enabled = false;
            Sound.Play(glucoseMeterEnd, Vector2.zero, twoDimensional: true, pitchShift: false);
            done = true;
        }

	    flashCounter += Time.deltaTime;
        applyBlood.sprite =
            progression > 0f ? bloodApplied
            : flashCounter % 1.5f < 0.75f ? applyBloodOn
            : applyBloodOff;

        countdown.text =
            progression > 0f && progression < Delay
            ? $":{Mathf.CeilToInt(5f * (1f - progression / Delay)):D2}"
            : "";

        bloodFill.fillAmount = bloodSaturation;

        var size = 40f * bloodOnFinger;
        droppedBlood.rectTransform.sizeDelta = new(size, size);

        int spriteIndex = System.Array.IndexOf(game.handSprites, game.handSprite.sprite);
        droppedBlood.rectTransform.anchoredPosition = spriteIndex switch
        {
            3 => new(-203f, 720f),
            4 => new(-203f, 695f),
            _ => new(-227f, 686f),
        };
        droppedBlood.transform.localEulerAngles = new(
            0f,
            0f,
            spriteIndex switch
            {
                3 => 0f,
                4 => 6f,
                _ => 8.5f,
            }
        );
	}
}
