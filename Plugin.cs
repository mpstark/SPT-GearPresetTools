using System;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using EFT;
using Aki.Reflection.Utils;
using GearPresetTools.Patches;
using GearPresetTools.Utils;
using HarmonyLib;
using System.Linq;

namespace GearPresetTools
{
    // the version number here is generated on build and may have a warning if not yet built
    [BepInPlugin("com.mpstark.GearPresetTools", "GearPresetTools", BuildInfo.Version)]
    public class Plugin : BaseUnityPlugin
    {
        public static Plugin Instance;
        public static ManualLogSource Log => Instance.Logger;

        public static ISession Session => ClientAppUtils.GetMainApp().GetClientBackEndSession();
        public static Profile SlayerProfile => _sessionProfileProperty.GetValue(Session) as Profile;

        private static Type _profileInterface= typeof(ISession).GetInterfaces().First(x => x.GetProperties().Length == 2 && AccessTools.Property(x, "Profile") != null);
        private static PropertyInfo _sessionProfileProperty = AccessTools.Property(_profileInterface, "Profile");

        public static string AutoSaveKitName = "Last Kit";

        internal void Awake()
        {
            Instance = this;
            DontDestroyOnLoad(this);

            // patches
            new MatchMakerTimeHasComeShowPatch().Enable();
        }

        public static void TrySaveCurrentGear()
        {
            BuildTools.SaveEquipmentAsBuild(AutoSaveKitName, SlayerProfile.Inventory.Equipment);
        }
    }
}
