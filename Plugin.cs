using System;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using EFT;
using Aki.Reflection.Utils;
using DrakiaXYZ.VersionChecker;
using AutoSaveGearPreset.Patches;
using HarmonyLib;
using EFT.Builds;

namespace AutoSaveGearPreset
{
    // the version number here is generated on build and may have a warning if not yet built
    [BepInPlugin("com.mpstark.AutoSaveGearPreset", "AutoSaveGearPreset", BuildInfo.Version)]
    public class Plugin : BaseUnityPlugin
    {
        public const int TarkovVersion = 29197;
        public static Plugin Instance;
        public static ManualLogSource Log => Instance.Logger;

        public static ConstructorInfo BuildConstructor;
        public static MethodInfo SaveBuildMethod;

        public static string AutoSaveKitName = "Last Kit";

        internal void Awake()
        {
            if (!VersionChecker.CheckEftVersion(Logger, Info, Config))
            {
                throw new Exception("Invalid EFT Version");
            }

            Instance = this;
            DontDestroyOnLoad(this);

            // patches
            new MatchMakerTimeHasComeShowPatch().Enable();
        }

        internal void TrySaveCurrentGear()
        {
            var session = ClientAppUtils.GetMainApp().GetClientBackEndSession();

            // find methodinfo and constructorinfo for out methods to avoid using GClass3182 directly
            if (BuildConstructor == null)
            {
                SaveBuildMethod = AccessTools.Method(session.EquipmentBuildsStorage.GetType(), "SaveBuild");
                BuildConstructor = AccessTools.Constructor(SaveBuildMethod.GetParameters()[0].ParameterType, new[] { typeof(MongoID), typeof(string), typeof(EquipmentClass), typeof(EEquipmentBuildType) });
            }

            // try to get the last preset with that name
            var oldKit = session.EquipmentBuildsStorage.FindCustomBuildByName(AutoSaveKitName);
            var mongoID = oldKit == null ? new MongoID(session.Profile) : oldKit.Id;

            // var newKit = new GClass3182(mongoID, AutoSaveKitName, session.Profile.Inventory.Equipment, EEquipmentBuildType.Custom);
            // session.EquipmentBuildsStorage.SaveBuild(newKit);

            // cumbersome to do without using the class directly
            var newKit = BuildConstructor.Invoke(new object[] { mongoID, AutoSaveKitName, session.Profile.Inventory.Equipment, EEquipmentBuildType.Custom });
            SaveBuildMethod.Invoke(session.EquipmentBuildsStorage, new[] { newKit });
        }
    }
}
