using System;
using System.Reflection;
using EFT;
using EFT.Builds;
using EFT.InventoryLogic;
using HarmonyLib;

namespace GearPresetTools.Utils
{
    public static class GearPresetUtils
    {
        private static PropertyInfo _sessionEquipmentBuildStorageProperty = AccessTools.Property(typeof(ISession), "EquipmentBuildsStorage");
        public static Type BuildStorageType = _sessionEquipmentBuildStorageProperty.PropertyType;
        private static MethodInfo _buildStorageFindCustomBuildByNameMethod = AccessTools.Method(BuildStorageType, "FindCustomBuildByName");
        public static MethodInfo BuildStorageSaveBuildMethod = AccessTools.Method(BuildStorageType, "SaveBuild");
        public static Type BuildType = BuildStorageSaveBuildMethod.GetParameters()[0].ParameterType;
        private static PropertyInfo _buildIdProperty = AccessTools.Property(BuildType, "Id");
        public static PropertyInfo BuildEquipmentProperty = AccessTools.Property(BuildType, "Equipment");

        public static object FindCustomBuildByName(string name)
        {
            var buildStorage = _sessionEquipmentBuildStorageProperty.GetValue(Plugin.Session);
            return _buildStorageFindCustomBuildByNameMethod.Invoke(buildStorage, new object[] { name });
        }

        public static void SaveBuild(object build)
        {
            var buildStorage = _sessionEquipmentBuildStorageProperty.GetValue(Plugin.Session);
            BuildStorageSaveBuildMethod.Invoke(buildStorage, new[] { build });
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
                            ? new MongoID(Plugin.PlayerProfile)
                            : (MongoID)_buildIdProperty.GetValue(oldKit);
            var newKit = Activator.CreateInstance(BuildType, new object[] { mongoID,
                                                                             name,
                                                                             equipment,
                                                                             EEquipmentBuildType.Custom }); 
            SaveBuild(newKit);
        }

        public static void RemoveSlotFromBuild(object build, EquipmentSlot slot)
        {
            var equipment = BuildEquipmentProperty.GetValue(build) as EquipmentClass;
            equipment.GetSlot(slot).RemoveItem();
        }
    }
}
