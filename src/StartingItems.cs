using UnityEngine;

class StartingItems
{
    public static void Give(Body body)
    {
        WorldGeneration world = WorldGeneration.world;

        if (
            world.totalTraveled <= 0
            && world.biomeOverride == WorldGeneration.OverrideSceneType.None
            && !SaveSystem.loadedRun
            && world.debugStartDepth == 0
        )
        {
            GameObject rapidInsulin = Utils.Create("rapidinsulinpen", body.transform.position, 0f);
            rapidInsulin.GetComponent<WaterContainerItem>().AddLiquid("rapidinsulin", 100f);
            GameObject longInsulin = Utils.Create("longinsulinpen", body.transform.position, 0f);
            longInsulin.GetComponent<WaterContainerItem>().AddLiquid("longinsulin", 100f);
            
            body.PickUpItem(rapidInsulin.GetComponent<Item>(), 0, force: true);
            body.PickUpItem(longInsulin.GetComponent<Item>(), 2, force: true);
        }
    }
}
