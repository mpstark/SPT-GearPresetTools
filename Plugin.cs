using System;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using EFT;
using Aki.Reflection.Utils;
using AutoSaveGearPreset.Patches;
using HarmonyLib;
using EFT.Builds;

namespace AutoSaveGearPreset
{
    // the version number here is generated on build and may have a warning if not yet built
    [BepInPlugin("com.mpstark.AutoSaveGearPreset", "AutoSaveGearPreset", BuildInfo.Version)]
    public class Plugin : BaseUnityPlugin
    {
        public static Plugin Instance;
        public static ManualLogSource Log => Instance.Logger;

        public static FieldInfo SessionEquipmentBuildStorageField = AccessTools.Field(typeof(ISession), "EquipmentBuildsStorage");
        public static Type BuildStorageType = SessionEquipmentBuildStorageField.GetType();
        public static MethodInfo BuildStorageFindCustomBuildByNameMethod = AccessTools.Method(BuildStorageType, "FindCustomBuildByName");
        public static MethodInfo BuildStorageSaveBuildMethod = AccessTools.Method(BuildStorageType, "SaveBuild");
        public static Type BuildType = BuildStorageSaveBuildMethod.GetParameters()[0].ParameterType;
        public static PropertyInfo BuildIdProperty = AccessTools.Property(BuildType, "Id");
        public static PropertyInfo SessionProfileProperty = AccessTools.Property(typeof(ISession), "Profile");
        public static ISession Session => ClientAppUtils.GetMainApp().GetClientBackEndSession();
        public static Profile PlayerProfile => SessionProfileProperty.GetValue(Session) as Profile;

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
            SaveCurrentEquipmentAsBuild(AutoSaveKitName);
        }

        private static object FindCustomBuildByName(string name)
        {
            var buildStorage = SessionEquipmentBuildStorageField.GetValue(Session);
            return BuildStorageFindCustomBuildByNameMethod.Invoke(buildStorage, new object[] { name });
        }

        private static void SaveBuild(object build)
        {
            var buildStorage = SessionEquipmentBuildStorageField.GetValue(Session);
            BuildStorageSaveBuildMethod.Invoke(buildStorage, new[] { build });
        }

        private static void SaveCurrentEquipmentAsBuild(string name)
        {
            // INSTEAD OF THIS
            // var oldKit = Session.EquipmentBuildsStorage.FindCustomBuildByName(AutoSaveKitName);
            // var mongoID = oldKit == null ? new MongoID(Session.Profile) : oldKit.Id;
            // var newKit = new GClass3182(mongoID, AutoSaveKitName, session.Profile.Inventory.Equipment, EEquipmentBuildType.Custom);
            // session.EquipmentBuildsStorage.SaveBuild(newKit);

            var oldKit = FindCustomBuildByName(name);
            var mongoID = (oldKit == null)
                            ? new MongoID(PlayerProfile)
                            : (MongoID)BuildIdProperty.GetValue(oldKit);
            var newKit = Activator.CreateInstance(BuildType, new object[] { mongoID,
                                                                            AutoSaveKitName,
                                                                            PlayerProfile.Inventory.Equipment,
                                                                            EEquipmentBuildType.Custom }); 
            SaveBuild(newKit);
        }
    }
}
