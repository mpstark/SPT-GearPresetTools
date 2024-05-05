using GearPresetTools.Config;
using GearPresetTools.Patches;
using GearPresetTools.Utils;
using GearPresetTools.Wrappers;

namespace GearPresetTools.Features
{
    public static class PartialPresets
    {
        public static GearPreset SelectedGearPreset { get; set; }

        public static void Enable()
        {
            new SaveBuildPatch().Enable();
            new InteractionsHandlerClassTransferContentPatch().Enable();
            new EquipmentBuildScreenSelectBuildPatch().Enable();
        }

        /// <summary>
        /// If configured, remove slots from a build being saved 
        /// </summary>
        public static void TryRemoveSlotsFromSavingBuild(GearPreset preset)
        {
            foreach (var (slot, setting) in Settings.DontSaveSlots)
            {
                if (!setting.Value) 
                {
                    continue;
                }

                preset.RemoveItemFromSlot(slot);
            }
        }

        /// <summary>
        /// If configured, prevent empty slots in a preset from applying to inventory
        /// </summary>
        internal static void TryPreventEmptySlotOverwrite(LootItemClass presetItemCopy, LootItemClass inventoryItem)
        {
            // only affect if top level
            if (inventoryItem.Id != ClientUtils.SessionProfile.Inventory.Equipment.Id)
            {
                return;
            }

            // get mapping of equipment slot enum to actual slot, since bsg is weird
            var presetSlots = SlotUtils.GetSlotsForItem(presetItemCopy);
            var inventorySlots = SlotUtils.GetSlotsForItem(inventoryItem);

            // go through slots
            foreach (var (slot, setting) in Settings.DontUnequipSlots)
            {
                // sanity check, should never have to continue here, because we're always at top level
                if (!presetSlots.ContainsKey(slot) || !inventorySlots.ContainsKey(slot))
                {
                    continue;
                }

                if (!setting.Value || presetSlots[slot].ContainedItem != null || inventorySlots[slot].ContainedItem == null) 
                {
                    continue;
                }
                
                // after that check, we know that the setting is set for this slot
                // that the preset doesn't have an item in the slot
                // and the current inventory does have an item in the slot

                // set the copy of the preset to have what the inventory slot already has in it
                presetSlots[slot].ContainedItem = inventorySlots[slot].ContainedItem.CloneItem();
            }
        }
    }
}
