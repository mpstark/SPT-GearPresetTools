using System;
using System.Collections.Generic;
using System.Reflection;
using EFT.InventoryLogic;
using HarmonyLib;

namespace GearPresetTools.Utils
{
    public static class SlotUtils
    {
        private static MethodInfo _slotRemoveItemMethod = AccessTools.Method(typeof(Slot), "RemoveItem");

        public static readonly HashSet<EquipmentSlot> Slots = new HashSet<EquipmentSlot>()
        {
            EquipmentSlot.FirstPrimaryWeapon,
            EquipmentSlot.SecondPrimaryWeapon,
            EquipmentSlot.Holster,
            EquipmentSlot.Scabbard,
            EquipmentSlot.Backpack,
            EquipmentSlot.SecuredContainer,
            EquipmentSlot.TacticalVest,
            EquipmentSlot.ArmorVest,
            EquipmentSlot.Eyewear,
            EquipmentSlot.FaceCover,
            EquipmentSlot.Headwear,
            EquipmentSlot.Earpiece,
            EquipmentSlot.ArmBand
        };

        // for some reason, slots are not in predictable spots
        // nor do they contain direct reference to what equipment slot they are
        public static Dictionary<EquipmentSlot, Slot> GetSlotsForItem(LootItemClass item)
        {
            var mapping = new Dictionary<EquipmentSlot, Slot>();
            foreach (var slot in item.AllSlots)
            {
                if (!Enum.TryParse(slot.Id, out EquipmentSlot parsedSlot))
                {
                    continue;
                }

                mapping[parsedSlot] = slot;
            }

            return mapping;
        }

        public static void RemoveItemFromSlot(Slot slot)
        {
            _slotRemoveItemMethod.Invoke(slot, new object[] { false });
        }
    }
}
