
using System;
using System.Reflection;
using EFT;
using EFT.Builds;
using HarmonyLib;

namespace GearPresetTools.Utils
{
    public static class BuildTools
    {
        private static PropertyInfo _sessionEquipmentBuildStorageProperty = AccessTools.Property(typeof(ISession), "EquipmentBuildsStorage");
        public static Type BuildStorageType = _sessionEquipmentBuildStorageProperty.PropertyType;
        private static MethodInfo _buildStorageFindCustomBuildByNameMethod = AccessTools.Method(BuildStorageType, "FindCustomBuildByName");
        private static MethodInfo _buildStorageSaveBuildMethod = AccessTools.Method(BuildStorageType, "SaveBuild");
        public static Type BuildType = _buildStorageSaveBuildMethod.GetParameters()[0].ParameterType;
        private static PropertyInfo _buildIdProperty = AccessTools.Property(BuildType, "Id");

        public static object FindCustomBuildByName(string name)
        {
            var buildStorage = _sessionEquipmentBuildStorageProperty.GetValue(Plugin.Session);
            return _buildStorageFindCustomBuildByNameMethod.Invoke(buildStorage, new object[] { name });
        }

        public static void SaveBuild(object build)
        {
            var buildStorage = _sessionEquipmentBuildStorageProperty.GetValue(Plugin.Session);
            _buildStorageSaveBuildMethod.Invoke(buildStorage, new[] { build });
        }

        public static void SaveEquipmentAsBuild(string name, EquipmentClass equipment)
        {
            // INSTEAD OF THIS
            // var oldKit = Session.EquipmentBuildsStorage.FindCustomBuildByName(AutoSaveKitName);
            // var mongoID = oldKit == null ? new MongoID(Session.Profile) : oldKit.Id;
            // var newKit = new GClass3182(mongoID, AutoSaveKitName, session.Profile.Inventory.Equipment, EEquipmentBuildType.Custom);
            // session.EquipmentBuildsStorage.SaveBuild(newKit);

            var oldKit = FindCustomBuildByName(name);
            var mongoID = (oldKit == null)
                            ? new MongoID(Plugin.SlayerProfile)
                            : (MongoID)_buildIdProperty.GetValue(oldKit);
            var newKit = Activator.CreateInstance(BuildType, new object[] { mongoID,
                                                                             name,
                                                                             equipment,
                                                                             EEquipmentBuildType.Custom }); 
            SaveBuild(newKit);
        }
    }
}
