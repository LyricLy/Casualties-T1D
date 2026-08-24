using CUCoreLib.Data;
using CUCoreLib.Registries;
using System;

namespace Diabetes;

class ModSettings
{
    public static event EventHandler? MgDlChanged;

    public static void AddSettings()
    {
        ModOptionsRegistry.Register(ModOptionDefinition.Dropdown(
            "lyricly.diabetes.unit",
            "Blood glucose unit",
            "Most of the world uses mmol/l; Americans use mg/dl, which is 18×mmol/l.",
            Setting.SettingCategory.Game,
            0,
            [
                new ModDropdownChoice("mmol/l", "mmol/l"),
                new ModDropdownChoice("mg/dl", "mg/dl")
            ],
            _ => MgDlChanged?.Invoke(typeof(ModSettings), EventArgs.Empty)
        ));
    }

    public static bool UseMgDl => Settings.Get<SettingDropdown>("lyricly.diabetes.unit").value == 1;
}
