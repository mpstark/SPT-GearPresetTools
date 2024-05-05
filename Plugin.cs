using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using GearPresetTools.Config;
using GearPresetTools.Features;
using GearPresetTools.Patches;

namespace GearPresetTools
{
    // the version number here is generated on build and may have a warning if not yet built
    [BepInPlugin("com.mpstark.GearPresetTools", "GearPresetTools", BuildInfo.Version)]
    public class Plugin : BaseUnityPlugin
    {
        public static Plugin Instance;
        public static ManualLogSource Log => Instance.Logger;
        public static string Path = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        internal void Awake()
        {
            Settings.Init(Config);
            // Config.SettingChanged += (x, y) => PlayerEncumbranceBar.OnSettingChanged();

            Instance = this;
            DontDestroyOnLoad(this);

            // features
            AutoSaveGearPreset.Enable();
            PartialPresets.Enable();

            // patch for saving profile preset config
            new EquipmentBuildScreenClosePatch().Enable();
        }
    }
}
