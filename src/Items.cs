using CUCoreLib.Data;
using CUCoreLib.Helpers;
using CUCoreLib.Registries;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace Diabetes;

class InsulinContainerInfo
{
    public readonly Sprite inner;
    readonly float fillStart;
    readonly float fillEnd;

    public InsulinContainerInfo(Sprite inner, float fillStart, float fillEnd)
    {
        this.inner = inner;
        this.fillStart = fillStart;
        this.fillEnd = fillEnd;
    }

    public float FixFill(float fill)
    {
        float length = inner.rect.width;
        return fillStart / length + (fillEnd - fillStart) / length * fill;
    }
}

static class Items
{
    public static Sprite penInner { get; private set; } = null!;
    public static Sprite inner { get; private set; } = null!;

    public static InsulinContainerInfo? GetInsulinContainerByID(string id)
    {
        if (id == "rapidinsulinpen" || id == "longinsulinpen")
        {
            return new InsulinContainerInfo(penInner, 2f, 5f);
        }
        else if (id == "rapidinsulin" || id == "longinsulin")
        {
            return new InsulinContainerInfo(inner, 2f, 9f);
        }
        return null;
    }

    public static InsulinContainerInfo? GetInsulinContainer(Item item)
    {
        return GetInsulinContainerByID(item.id);
    }

    public static void FixInsulinContainerImage(Image image, InsulinContainerInfo? c, float ps, float ms)
    {
        if (c is not null)
        {
            image.sprite = c.inner;
            image.rectTransform.sizeDelta = PlayerCamera.ImageSizeDelta(
                c.inner.texture,
                ps,
                ms
            );
            image.fillMethod = Image.FillMethod.Horizontal;
            image.fillAmount = c.FixFill(image.fillAmount);
        }
        else
        {
            image.fillMethod = Image.FillMethod.Vertical;
        }
    }

    public static bool IsInsulinPump(Item item)
    {
        return item.id == "insulinpump" || item.id == "smartinsulinpump";
    }

    public static void AddItems()
    {
        penInner = AssetLoader.LoadEmbeddedSprite("peninner.png");
        inner = AssetLoader.LoadEmbeddedSprite("inner.png");
        Sprite inner90 = AssetLoader.LoadEmbeddedSprite("inner90.png");
        Sprite penInner90 = AssetLoader.LoadEmbeddedSprite("peninner90.png");

        ItemRegistry.Register("rapidinsulinpen", new CustomItemInfo
        {
            fullName = "stub",
            description = "stub",
            category = "custom",
            slotRotation = -45f,
            usable = false,
            usableOnLimb = true,
            destroyAtZeroCondition = false,
            combineable = true,
            weight = 0.3f,
            scaleWeightWithCondition = true,
            capacity = 100f,
            defaultContents = [],
            autoFill = false,
            useLimbAction = (Limb limb, Item item) =>
            {
                var wat = item.GetComponent<WaterContainerItem>();
                wat.Inject(limb, 2f);
                Sound.Play("syringe", limb.body.transform.position);
            },
            value = 5,
            tags = "medicine",
            rec = new Recognition(5),
            qualities = [
                new CraftingQuality("insulinpen", 1f)
            ],
            LiquidMask = penInner90
        }, AssetLoader.LoadEmbeddedSprite("rapidinsulinpen.png"));

        ItemRegistry.Register("longinsulinpen", new CustomItemInfo
        {
            fullName = "stub",
            description = "stub",
            category = "custom",
            slotRotation = -45f,
            usable = false,
            usableOnLimb = true,
            destroyAtZeroCondition = false,
            combineable = true,
            weight = 0.3f,
            scaleWeightWithCondition = true,
            capacity = 100f,
            defaultContents = [],
            autoFill = false,
            useLimbAction = (Limb limb, Item item) =>
            {
                var wat = item.GetComponent<WaterContainerItem>();
                wat.Inject(limb, 10f);
                Sound.Play("syringe", limb.body.transform.position);
            },
            value = 5,
            tags = "medicine",
            rec = new Recognition(5),
            qualities = [
                new CraftingQuality("insulinpen", 1f)
            ],
            LiquidMask = penInner90
        }, AssetLoader.LoadEmbeddedSprite("longinsulinpen.png"));

        ItemRegistry.Register("rapidinsulin", new CustomItemInfo
        {
            fullName = "stub",
            description = "stub",
            category = "drug",
            slotRotation = -45f,
            usable = false,
            usableOnLimb = false,
            destroyAtZeroCondition = false,
            combineable = true,
            weight = 0.2f,
            scaleWeightWithCondition = true,
            capacity = 100f,
            defaultContents = [
                new LiquidStack("rapidinsulin", 100f)
            ],
            autoFill = false,
            value = 1,
            tags = "medicine",
            rec = new Recognition(5),
            LiquidMask = inner90
        }, AssetLoader.LoadEmbeddedSprite("rapidinsulin.png"));

        ItemRegistry.Register("longinsulin", new CustomItemInfo
        {
            fullName = "stub",
            description = "stub",
            category = "drug",
            slotRotation = -45f,
            usable = false,
            usableOnLimb = false,
            destroyAtZeroCondition = false,
            combineable = true,
            weight = 0.2f,
            scaleWeightWithCondition = true,
            capacity = 100f,
            defaultContents = [
                new LiquidStack("longinsulin", 100f)
            ],
            autoFill = false,
            value = 1,
            tags = "medicine",
            rec = new Recognition(5),
            LiquidMask = inner90
        }, AssetLoader.LoadEmbeddedSprite("longinsulin.png"));

        ItemRegistry.Register(
            "insulinpump",
            new CustomItemInfo
            {
                fullName = "stub",
                description = "stub",
                category = "medical",
                slotRotation = -90f,
                usable = true,
                usableOnLimb = false,
                destroyAtZeroCondition = false,
                combineable = true,
                weight = 1f,
                capacity = 100f,
                defaultContents = [],
                autoFill = false,
                wearable = true,
                wearableCanBeHeld = true,
                wearableHitDurabilityLossMultiplier = 0f,
                desiredWearLimb = "ThighF",
                wearSlotId = "insulinpump",
                wearableVisualOffset = 9,
                useAction = (Body body, Item item) =>
                {
                    var pump = item.GetComponent<InsulinPump>();
                    pump.GiveManually(body);
                },
                value = 18,
                tags = "medicine",
                rec = new Recognition(8),
                Battery = new BatteryProperties
                {
                    Preset = BatteryItem.BatteryPreset.Medium
                }
            }.AddSpawnComponent<InsulinPump>(),
            AssetLoader.LoadEmbeddedSprite("insulinpump.png")
        );

        ItemRegistry.Register(
            "smartinsulinpump",
            new CustomItemInfo
            {
                fullName = "stub",
                description = "stub",
                category = "medical",
                slotRotation = -90f,
                usable = true,
                usableOnLimb = false,
                destroyAtZeroCondition = false,
                combineable = true,
                weight = 1f,
                capacity = 100f,
                defaultContents = [],
                autoFill = false,
                wearable = true,
                wearableCanBeHeld = true,
                wearableHitDurabilityLossMultiplier = 0f,
                desiredWearLimb = "ThighF",
                wearSlotId = "insulinpump",
                wearableVisualOffset = 9,
                useAction = (Body body, Item item) =>
                {
                    var pump = item.GetComponent<InsulinPump>();
                    pump.GiveManually(body);
                },
                value = 25,
                tags = "medicine",
                rec = new Recognition(8),
                Battery = new BatteryProperties
                {
                    Preset = BatteryItem.BatteryPreset.Medium
                }
            }.AddSpawnComponent<SmartInsulinPump>(),
            AssetLoader.LoadEmbeddedSprite("smartinsulinpump.png")
        );

        ItemRegistry.Register("metformin", new CustomItemInfo
        {
            fullName = "stub",
            description = "stub",
            category = "drug",
            slotRotation = -45f,
            usable = true,
            usableOnLimb = false,
            destroyAtZeroCondition = false,
            combineable = true,
            weight = 0.3f,
            scaleWeightWithCondition = true,
            capacity = 100f,
            defaultContents = [
                new LiquidStack("metformin", 100f)
            ],
            autoFill = false,
            useAction = (Body body, Item item) =>
            {
                var wat = item.GetComponent<WaterContainerItem>();
                wat.Drink(body, 20f, "pills");
            },
            value = 1,
            tags = "medicine",
            rec = new Recognition(10)
        }, AssetLoader.LoadEmbeddedSprite("metformin.png"));

        ItemRegistry.Register("glucagon", new CustomItemInfo
        {
            fullName = "stub",
            description = "stub",
            category = "drug",
            slotRotation = -45f,
            usable = false,
            usableOnLimb = true,
            destroyAtZeroCondition = false,
            combineable = true,
            weight = 0.3f,
            scaleWeightWithCondition = true,
            capacity = 100f,
            defaultContents = [
                new LiquidStack("glucagon", 100f)
            ],
            autoFill = false,
            useLimbAction = (Limb limb, Item item) =>
            {
                var wat = item.GetComponent<WaterContainerItem>();
                MinigameBase.main.StartMinigame(new SyringeMinigame((float mult) =>
                {
                    wat.Inject(limb, mult * 100f);
                }, limb, wat.AverageColor()), item);
            },
            value = 1,
            tags = "medicine",
            rec = new Recognition(6)
        }, AssetLoader.LoadEmbeddedSprite("glucagon.png"));

        ItemRegistry.Register("glucosemeter", new CustomItemInfo
        {
            fullName = "stub",
            description = "stub",
            category = "medical",
            slotRotation = 0f,
            usable = false,
            usableOnLimb = true,
            destroyAtZeroCondition = false,
            weight = 0.3f,
            useLimbAction = (Limb limb, Item item) =>
            {
                if (limb != limb.body.limbs[5] && limb != limb.body.limbs[8] || !item.battery.hasCharge)
                {
                    return;
                }
                MinigameBase.main.StartMinigame(new GlucoseMeterMinigame(limb), item);
            },
            value = 9,
            tags = "medicine",
            rec = new Recognition(8),
            Battery = new BatteryProperties
            {
                Preset = BatteryItem.BatteryPreset.Small
            }
        }, AssetLoader.LoadEmbeddedSprite("glucosemeter.png"));
    }

    public static void AddLiquids()
    {
        LiquidRegistry.Register("rapidinsulin", new CustomLiquidInfo
        {
            color = new Color32(203, 238, 255, 255),
            valuePerLiter = 245f,
            injectable = true,
            injectionSickness = 0f,
            onDrink = (float ml, Body body) =>
            {
                var status = body.GetStatus<DiabetesStatus>();
                status.TakeRapidInsulin(ml * 0.01f);
                body.sicknessAmount += ml * 0.01f;
                body.talker.EatBad();
            },
            onHealthUse = (float ml, Limb limb) =>
            {
                var status = limb.body.GetStatus<DiabetesStatus>();
                status.TakeRapidInsulin(ml);
            },
            unobtainable = true
        });

        LiquidRegistry.Register("longinsulin", new CustomLiquidInfo
        {
            color = new Color32(237, 255, 241, 255),
            valuePerLiter = 200f,
            injectable = true,
            injectionSickness = 0f,
            onDrink = (float ml, Body body) =>
            {
                var status = body.GetStatus<DiabetesStatus>();
                status.TakeLongInsulin(ml * 0.01f);
                body.sicknessAmount += ml * 0.01f;
                body.talker.EatBad();
            },
            onHealthUse = (float ml, Limb limb) =>
            {
                var status = limb.body.GetStatus<DiabetesStatus>();
                status.TakeLongInsulin(ml);
            },
            unobtainable = true
        });

        LiquidRegistry.Register("metformin", new CustomLiquidInfo
        {
            color = new Color32(198, 255, 179, 255),
            valuePerLiter = 100f,
            onDrink = (float ml, Body body) =>
            {
                float doses = ml / 20f;
                var status = body.GetStatus<DiabetesStatus>();
                CoUtils.instance.DoTimedOp("metformin", () =>
                {
                    status.insulinSensitivity = Utils.RaiseToTarget(status.insulinSensitivity, 1f, 0.04f, 1f);
                    if (CoUtils.instance.DurationOf("metformin") > 1200f)
                    {
                        status.ketones += 0.01f;
                        body.overdoseIndex = 3;
                    }
                }, 300f * doses);
                Sound.Play("pills", body.transform.position);
            }
        });

        LiquidRegistry.Register("glucagon", new CustomLiquidInfo
        {
            color = new Color32(252, 213, 174, 255),
            valuePerLiter = 80f,
            injectable = true,
            injectionSickness = 0f,
            onHealthUse = (float ml, Limb limb) =>
            {
                var status = limb.body.GetStatus<DiabetesStatus>();
                status.RaiseBloodSugar(ml / 10f);
            }
        });

        LiquidRegistry.Register("sweeturine", new CustomLiquidInfo
        {
            color = new Color32(255, 218, 84, 255),
            valuePerLiter = 2f,
            injectionSickness = 2f,
            onDrink = (float ml, Body body) =>
            {
                var status = body.GetStatus<DiabetesStatus>();
                float litres = ml * 0.001f;
                body.Drink(70f * litres);
                body.temperature -= 3f * litres;
                body.happiness -= 15f * litres;
                body.sicknessAmount += 65f * litres;
                body.weightOffset += 2f * litres;
                body.talker.EatBad();
                if (!status.hadSweetUrine)
                {
                    status.hadSweetUrine = true;
                    body.StartCoroutine(SweetUrineDoubleTake(body));
                }
            },
            qualities = [
                new CraftingQuality("water", 0.1f)
            ]
        });
    }

    static IEnumerator SweetUrineDoubleTake(Body body)
    {
        yield return new WaitForSeconds(2f);
        body.talker.Talk(LocaleRegistry.Get("other", "sweeturinedoubletake", null));
    }

    public static void AddRecipes()
    {
        RecipeRegistry.Register(new Recipe
        {
            INT = 10,
            result = new RecipeResult { id = "rapidinsulinpen" },
            items = [
                new RecipeItem(0f) { specificId = "flexiglass" },
                new RecipeItem(0f) { specificId = "autoinjector" },
                new RecipeItem(0f) { specificId = "plasticchunk" },
                new RecipeItem(0f) { specificId = "plasticchunk" },
                new RecipeItem(30f) { specificId = "biochem", isLiquid = true },
                new RecipeItem(0f) { quality = "nails", destroyItem = false },
                new RecipeItem(0f) { quality = "hammering", destroyItem = false }
            ],
            category = Recipes.RecipeCategory.Medicine
        });

        RecipeRegistry.Register(new Recipe
        {
            INT = 10,
            result = new RecipeResult { id = "longinsulinpen" },
            items = [
                new RecipeItem(0f) { specificId = "flexiglass" },
                new RecipeItem(0f) { specificId = "autoinjector" },
                new RecipeItem(0f) { specificId = "plasticchunk" },
                new RecipeItem(0f) { specificId = "plasticchunk" },
                new RecipeItem(30f) { specificId = "biochem", isLiquid = true },
                new RecipeItem(0f) { quality = "nails", destroyItem = false },
                new RecipeItem(0f) { quality = "hammering", destroyItem = false }
            ],
            category = Recipes.RecipeCategory.Medicine
        });

        RecipeRegistry.Register(new Recipe
        {
            INT = 11,
            result = new RecipeResult { id = "glucosemeter" },
            items = [
                new RecipeItem(0f) { specificId = "lcdscreen" },
                new RecipeItem(0f) { specificId = "bundleofwires" },
                new RecipeItem(0f) { specificId = "plasticchunk" },
                new RecipeItem(0f) { specificId = "plasticchunk" },
                new RecipeItem(0f) { specificId = "scraptube" },
                new RecipeItem { specificId = "circuitboard" },
                new RecipeItem(10f) { specificId = "biochem", isLiquid = true },
                new RecipeItem(0f) { quality = "hammering", destroyItem = false }
            ],
            category = Recipes.RecipeCategory.Medicine
        });

        RecipeRegistry.Register(new Recipe
        {
            INT = 12,
            result = new RecipeResult { id = "insulinpump" },
            items = [
                new RecipeItem(0f) { quality = "insulinpen" },
                new RecipeItem(0f) { specificId = "autoinjector" },
                new RecipeItem(0f) { specificId = "lcdscreen" },
                new RecipeItem(0f) { specificId = "bundleofwires" },
                new RecipeItem { specificId = "circuitboard" },
                new RecipeItem(0f) { specificId = "scrappanel" },
                new RecipeItem(50f) { specificId = "biochem", isLiquid = true },
                new RecipeItem(0f) { quality = "nails", destroyItem = false },
                new RecipeItem(0f) { quality = "hammering", destroyItem = false }
            ],
            category = Recipes.RecipeCategory.Medicine
        });

        RecipeRegistry.Register(new Recipe
        {
            INT = 16,
            result = new RecipeResult { id = "smartinsulinpump" },
            items = [
                new RecipeItem(0f) { specificId = "insulinpump" },
                new RecipeItem(0f) { specificId = "glucosemeter" },
                new RecipeItem(0f) { specificId = "bundleofwires" },
                new RecipeItem { specificId = "circuitboard" },
                new RecipeItem(0f) { specificId = "titaniumsheet" },
                new RecipeItem(10f) { specificId = "biochem", isLiquid = true },
                new RecipeItem(0f) { quality = "hammering", destroyItem = false }
            ],
            category = Recipes.RecipeCategory.Medicine
        });

        RecipeRegistry.Register(new Recipe
        {
            INT = 7,
            result = new RecipeResult { id = "rapidinsulin", isLiquid = true, resultCondition = 10f },
            items = [
                new RecipeItem(10f) { specificId = "biochem", isLiquid = true },
                new RecipeItem(10f) { specificId = "longinsulin", isLiquid = true }
            ],
            category = Recipes.RecipeCategory.Medicine
        });

        RecipeRegistry.Register(new Recipe
        {
            INT = 8,
            result = new RecipeResult { id = "longinsulin", isLiquid = true, resultCondition = 10f },
            items = [
                new RecipeItem(10f) { specificId = "biochem", isLiquid = true },
                new RecipeItem { quality = new CraftingQuality("blood", 40f), isLiquid = true }
            ],
            category = Recipes.RecipeCategory.Medicine
        });
    }
}
