using System;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using EFT;
using Aki.Reflection.Utils;
using AutoSaveGearPreset.Patches;
using HarmonyLib;
using EFT.Builds;
using System.Linq;

namespace AutoSaveGearPreset
{
    // the version number here is generated on build and may have a warning if not yet built
    [BepInPlugin("com.mpstark.AutoSaveGearPreset", "AutoSaveGearPreset", BuildInfo.Version)]
    public class Plugin : BaseUnityPlugin
    {
        public static Plugin Instance;
        public static ManualLogSource Log => Instance.Logger;

        private static PropertyInfo _sessionEquipmentBuildStorageProperty;
        private static Type _buildStorageType;
        private static MethodInfo _buildStorageFindCustomBuildByNameMethod;
        private static MethodInfo _buildStorageSaveBuildMethod;
        private static Type _buildType;
        private static PropertyInfo _buildIdProperty;
        private static Type _profileInterface;
        private static PropertyInfo _sessionProfileProperty;

        private static ISession _session => ClientAppUtils.GetMainApp().GetClientBackEndSession();
        private static Profile _playerProfile => _sessionProfileProperty.GetValue(_session) as Profile;

        public static string AutoSaveKitName = "Last Kit";

        internal void Awake()
        {
            Instance = this;
            DontDestroyOnLoad(this);

            // reflection to make generic
            _sessionEquipmentBuildStorageProperty = AccessTools.Property(typeof(ISession), "EquipmentBuildsStorage");
            _buildStorageType = _sessionEquipmentBuildStorageProperty.PropertyType;
            _buildStorageFindCustomBuildByNameMethod = AccessTools.Method(_buildStorageType, "FindCustomBuildByName");
            _buildStorageSaveBuildMethod = AccessTools.Method(_buildStorageType, "SaveBuild");
            _buildType = _buildStorageSaveBuildMethod.GetParameters()[0].ParameterType;
            _buildIdProperty = AccessTools.Property(_buildType, "Id");
            _profileInterface = typeof(ISession).GetInterfaces().First(x => x.GetProperties().Length == 2 && AccessTools.Property(x, "Profile") != null);
            _sessionProfileProperty = AccessTools.Property(_profileInterface, "Profile");

            // patches
            new MatchMakerTimeHasComeShowPatch().Enable();
        }

        public static void TrySaveCurrentGear()
        {
            SaveEquipmentAsBuild(AutoSaveKitName, _playerProfile.Inventory.Equipment);
        }

        private static object FindCustomBuildByName(string name)
        {
            var buildStorage = _sessionEquipmentBuildStorageProperty.GetValue(_session);
            return _buildStorageFindCustomBuildByNameMethod.Invoke(buildStorage, new object[] { name });
        }

        private static void SaveBuild(object build)
        {
            var buildStorage = _sessionEquipmentBuildStorageProperty.GetValue(_session);
            _buildStorageSaveBuildMethod.Invoke(buildStorage, new[] { build });
        }

        private static void SaveEquipmentAsBuild(string name, EquipmentClass equipment)
        {
            // INSTEAD OF THIS
            // var oldKit = Session.EquipmentBuildsStorage.FindCustomBuildByName(AutoSaveKitName);
            // var mongoID = oldKit == null ? new MongoID(Session.Profile) : oldKit.Id;
            // var newKit = new GClass3182(mongoID, AutoSaveKitName, session.Profile.Inventory.Equipment, EEquipmentBuildType.Custom);
            // session.EquipmentBuildsStorage.SaveBuild(newKit);

            var oldKit = FindCustomBuildByName(name);
            var mongoID = (oldKit == null)
                            ? new MongoID(_playerProfile)
                            : (MongoID)_buildIdProperty.GetValue(oldKit);
            var newKit = Activator.CreateInstance(_buildType, new object[] { mongoID,
                                                                             AutoSaveKitName,
                                                                             equipment,
                                                                             EEquipmentBuildType.Custom }); 
            SaveBuild(newKit);
        }
    }
}
