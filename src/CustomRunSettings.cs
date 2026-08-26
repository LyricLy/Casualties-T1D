namespace Diabetes;

public static class CustomRunSettings
{
    public static void AddRunSettings()
    {
        RunSettings.settingTypes.Add(new RunSettingFloat("glycogenolysisrate")
        {
            limits = new RangeF(0.1f, 10f),
            postfix = "x"
        });
        RunSettings.settingTypes.Add(new RunSettingFloat("pancreaseffect")
        {
            limits = new RangeF(0f, 10f),
            postfix = "x"
        });
        RunSettings.settingTypes.Add(new RunSettingFloat("icr")
        {
            limits = new RangeF(1f, 30f),
            postfix = "g"
        });
        RunSettings.settingTypes.Add(new RunSettingFloat("isf")
        {
            limits = new RangeF(1f, 10f),
            postfix = "\nmmol/l"
        });
        RunSettings.settingTypes.Add(new RunSettingFloat("hypotimescale")
        {
            limits = new RangeF(0.1f, 1f),
            postfix = "x"
        });

        RunSettingsPreset normal = RunSettings.GetPreset("normal");
        normal.presetValues["glycogenolysisrate"] = 1f;
        normal.presetValues["pancreaseffect"] = 1f;
        normal.presetValues["icr"] = 5f;
        normal.presetValues["isf"] = 2f;
        normal.presetValues["hypotimescale"] = 0.3f;

        RunSettingsPreset relaxed = RunSettings.GetPreset("relaxed");
        relaxed.presetValues["glycogenolysisrate"] = 0.9f;
        relaxed.presetValues["pancreaseffect"] = 1.2f;
        relaxed.presetValues["icr"] = 10f;
        relaxed.presetValues["isf"] = 4f;
        relaxed.presetValues["hypotimescale"] = 0.2f;

        RunSettingsPreset paradise = RunSettings.GetPreset("paradise");
        paradise.presetValues["glycogenolysisrate"] = 0.5f;
        paradise.presetValues["pancreaseffect"] = 2f;
        paradise.presetValues["icr"] = 15f;
        paradise.presetValues["isf"] = 6f;
        paradise.presetValues["hypotimescale"] = 0.1f;

        RunSettingsPreset desolate = RunSettings.GetPreset("desolate");
        desolate.presetValues["glycogenolysisrate"] = 1.2f;
        desolate.presetValues["pancreaseffect"] = 0.5f;
        desolate.presetValues["icr"] = 4f;
        desolate.presetValues["isf"] = 1.6f;
    }
}
