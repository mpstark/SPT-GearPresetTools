using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using EFT;
using GearPresetTools.Utils;
using HarmonyLib;

namespace GearPresetTools.Wrappers
{
    public class GearPresetStorage
    {
        // lazy singleton
        private static readonly Lazy<GearPresetStorage> lazy = new Lazy<GearPresetStorage>(() =>
            new GearPresetStorage(_sessionEquipmentBuildStorageProperty.GetValue(ClientUtils.Session)));
        public static GearPresetStorage Instance { get { return lazy.Value; } }

        // reflection
        private static PropertyInfo _sessionEquipmentBuildStorageProperty = AccessTools.Property(typeof(ISession), "EquipmentBuildsStorage");
        internal static Type WrappedType = _sessionEquipmentBuildStorageProperty.PropertyType;
        private static MethodInfo _findCustomBuildByNameMethod = AccessTools.Method(WrappedType, "FindCustomBuildByName");
        private static PropertyInfo _equipmentBuildsProperty = AccessTools.Property(WrappedType, "EquipmentBuilds");
        internal static MethodInfo SaveBuildMethod = AccessTools.Method(WrappedType, "SaveBuild");

        // properties
        public object Value { get; }

        public GearPresetStorage(object value)
        {
            if (value.GetType() != WrappedType)
            {
                throw new ArgumentException("Tried to construct with wrong type value");
            }

            Value = value;
        }

        public void SaveBuild(GearPreset preset)
        {
            SaveBuildMethod.Invoke(Value, new[] { preset.Value });
        }

        public GearPreset FindCustomBuildByName(string name)
        {
            var build = _findCustomBuildByNameMethod.Invoke(Value, new object[] { name });
            if (build == null)
            {
                return null;
            }

            return new GearPreset(build);
        }

        public void SaveEquipmentAsBuild(string name, EquipmentClass equipment)
        {
            var oldKit = FindCustomBuildByName(name);
            var mongoID = (oldKit == null) ? new MongoID(ClientUtils.SessionProfile) : oldKit.Id;

            SaveBuild(new GearPreset(mongoID, name, equipment));
        }

        public IEnumerable<string> GetAllGearPresetIds()
        {
            var builds = _equipmentBuildsProperty.GetValue(Value) as IDictionary;
            foreach (DictionaryEntry entry in builds)
            {
                yield return ((MongoID)entry.Key).ToString();
            }
        }
    }
}
