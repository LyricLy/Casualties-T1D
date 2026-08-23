using System.Collections.Generic;

static class Carbs
{
    public static Dictionary<string, (float ratio, float ofWhichSugar)> FoodRatios = new()
    {
        ["banana"] = (0.23f, 0.5f),
        ["cookies"] = (0.60f, 0.65f),
        ["bread"] = (0.50f, 0f),
        ["browncap"] = (0.01f, 0.5f),
        ["burger"] = (0.06f, 0f),
        ["cactusflesh"] = (0.10f, 0.75f),
        ["cake"] = (0.60f, 0.5f),
        ["hardcandy"] = (1.00f, 1f),
        ["candybar"] = (0.53f, 0.8f),
        ["cereal"] = (0.81f, 0.15f),
        ["chips"] = (0.55f, 0f),
        ["chocolatebar"] = (0.51f, 0.85f),
        ["dogfood"] = (0.50f, 0f),
        ["exposedcore"] = (0.05f, 0.5f),
        ["foliagemeal"] = (0.01f, 0.5f),
        ["frigiantfruit"] = (0.01f, 0.75f),
        ["funguschunk"] = (0.01f, 0.5f),
        ["geofruit"] = (0.10f, 0.75f),
        ["mushpear"] = (0.05f, 0.25f),
        ["nutrientbar"] = (0.50f, 0f),
        ["pancake"] = (0.47f, 0.5f),
        ["paprikash"] = (0.05f, 0f),
        ["pemmican"] = (0.03f, 0.5f),
        ["pizzaslice"] = (0.50f, 0.1f),
        ["popcorn"] = (0.75f, 0f),
        ["stonefruitopen"] = (0.15f, 0f)
    };

    public static Dictionary<string, (float ratio, float ofWhichSugar)> DrinkRatios = new()
    {
        ["cereal"] = (0.36f, 0.08f),
        ["ketchup"] = (0.26f, 0.98f),
        ["orangejuice"] = (0.10f, 1f),
        ["lemonade"] = (0.10f, 1f),
        ["yogurt"] = (0.03f, 1f),
        ["applejuice"] = (0.10f, 1f),
        ["icetea"] = (0.04f, 1f),
        ["icecream"] = (0.13f, 0.8f),
        ["chocolatemilk"] = (0.12f, 0.92f),
        ["soda"] = (0.10f, 1f),
        ["sportsdrink"] = (0.06f, 1f),
        ["energydrink"] = (0.12f, 1f),
        ["treesap"] = (0.67f, 1f),
        ["producejuice"] = (0.05f, 0.5f),
        ["refinedjuice"] = (0.10f, 0.5f),
        ["milk"] = (0.05f, 1f),
        ["hotsauce"] = (0.04f, 0.98f)
    };
}
