using System;
using System.Collections.Generic;
using EFT.InventoryLogic;

namespace GearPresetTools.Utils
{
    public static class SlotUtils
    {
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
            EquipmentSlot.Pockets,
            EquipmentSlot.Eyewear,
            EquipmentSlot.FaceCover,
            EquipmentSlot.Headwear,
            EquipmentSlot.Earpiece,
            EquipmentSlot.ArmBand
        };

        // for some reason, slots are not in predictable spots
        // nor do they contain direct reference to what equipmentslot they are
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
    }
}
