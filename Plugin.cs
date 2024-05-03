using DrakiaXYZ.VersionChecker;
using AutoSaveGearPreset.Patches;

namespace AutoSaveGearPreset
{
    // the version number here is generated on build and may have a warning if not yet built
    [BepInPlugin("com.mpstark.AutoSaveGearPreset", "AutoSaveGearPreset", BuildInfo.Version)]
    public class Plugin : BaseUnityPlugin
    {
        public const int TarkovVersion = 29197;
        public static Plugin Instance;
        public static ManualLogSource Log => Instance.Logger;
        public static Inventory PlayerInventory => ClientAppUtils.GetMainApp().GetClientBackEndSession().Profile.Inventory;
        public static string PluginFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        internal void Awake()
        {
            if (!VersionChecker.CheckEftVersion(Logger, Info, Config))
            {
                throw new Exception("Invalid EFT Version");
            }

            Instance = this;
            DontDestroyOnLoad(this);

            // patches
        }
    }
}
