using System;
using System.Reflection;
using EFT;
using EFT.Builds;
using EFT.InventoryLogic;
using HarmonyLib;

namespace GearPresetTools.Wrappers
{
    public class GearPreset
    {
        // reflection
        public static Type WrappedType = GearPresetStorage.SaveBuildMethod.GetParameters()[0].ParameterType;
        private static PropertyInfo _idProperty = AccessTools.Property(WrappedType, "Id");
        private static PropertyInfo _equipmentProperty = AccessTools.Property(WrappedType, "Equipment");

        // properties
        public object Value { get; }
        public MongoID Id => (MongoID)_idProperty.GetValue(Value);
        public EquipmentClass Equipment => (EquipmentClass)_equipmentProperty.GetValue(Value);

        public GearPreset(object value)
        {
            if (value.GetType() != WrappedType)
            {
                throw new ArgumentException("Tried to construct with wrong type value");
            }

            Value = value;
        }

        public GearPreset(MongoID mongoID, string name, EquipmentClass equipment, EEquipmentBuildType buildType = EEquipmentBuildType.Custom)
        {
            Value = Activator.CreateInstance(WrappedType, new object[] { mongoID, name, equipment, EEquipmentBuildType.Custom }); 
        }

        public void RemoveItemFromSlot(EquipmentSlot slot)
        {
            Equipment.GetSlot(slot).RemoveItem();
        }
    }
}