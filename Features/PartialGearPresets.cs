using System.Collections.Generic;
using System.Reflection;
using EFT.InventoryLogic;
using EFT.UI;
using EFT.UI.DragAndDrop;
using GearPresetTools.Config;
using GearPresetTools.Patches;
using GearPresetTools.Utils;
using GearPresetTools.Wrappers;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace GearPresetTools.Features
{
    public static class PartialGearPresets
    {
        public static GearPreset SelectedGearPreset { get; set; }

        // reflection
        private static FieldInfo _equipmentTabSlotViewsField = AccessTools.Field(typeof(EquipmentBuildGear), "_slotViews");
        private static FieldInfo _slotViewSlotHeaderField = AccessTools.Field(typeof(SlotView), "_slotHeader");
        private static FieldInfo _slotViewHeaderSelectActionField = AccessTools.Field(typeof(SlotViewHeader), "action_0");

        private static Color _ignoreColor = new Color(0, 0.5f, 0.5f, 1f);

        public static void Enable()
        {
            new SaveBuildPatch().Enable();
            new InteractionsHandlerClassTransferContentPatch().Enable();
            new EquipmentBuildScreenSelectBuildPatch().Enable();
            new EquipmentBuildGearShowPatch().Enable();
        }

        /// <summary>
        /// If configured, set default value of ignore slot to config
        /// </summary>
        public static void TrySetDefaultIgnoreOnSavingBuild(GearPreset preset)
        {
            foreach (var (slot, setting) in Settings.IgnoreSlotsByDefault)
            {
                var hasIgnoreValue = ProfilePresetConfig.Instance.SlotHasIgnoreValue(ClientUtils.ProfileId, preset.IdString, slot);
                if (!hasIgnoreValue) 
                {
                    ProfilePresetConfig.Instance.SetIgnoreSlot(ClientUtils.ProfileId, preset.IdString, slot, setting.Value);
                }
            }
        }

        /// <summary>
        /// If configured, prevent slots in a preset from applying to inventory
        /// </summary>
        public static void TryApplyIgnoredSlots(LootItemClass presetItemCopy, LootItemClass inventoryItem)
        {
            // only affect if top level
            if (inventoryItem.Id != ClientUtils.SessionProfile.Inventory.Equipment.Id)
            {
                return;
            }

            // get which slots that the currently selected profile has ignored
            var ignoredSlots = ProfilePresetConfig.Instance.GetIgnoredSlots(ClientUtils.ProfileId,
                                                                            SelectedGearPreset.IdString);

            // get mapping of equipment slot enum to actual slot, since bsg is weird
            var presetSlots = SlotUtils.GetSlotsForItem(presetItemCopy);
            var inventorySlots = SlotUtils.GetSlotsForItem(inventoryItem);

            // ignore the slots
            foreach (var slot in ignoredSlots)
            {
                // sanity check, should never have to continue here, because we should be at top level
                if (!presetSlots.ContainsKey(slot) || !inventorySlots.ContainsKey(slot))
                {
                    continue;
                }

                // set the copy of the preset to have what the inventory slot already has in it
                presetSlots[slot].ContainedItem = inventorySlots[slot].ContainedItem?.CloneItem();
            }
        }

        internal static void TryAdjustEquipmentBuildGear(EquipmentBuildGear gear)
        {
            var slotViews = _equipmentTabSlotViewsField.GetValue(gear) as Dictionary<EquipmentSlot, SlotView>;
            var ignoredSlots = ProfilePresetConfig.Instance.GetIgnoredSlots(ClientUtils.ProfileId, SelectedGearPreset.IdString);

            // this isn't the jankiest thing you've ever seen
            // but it's close
            var buildsScreen = gear.gameObject.transform.parent.parent.parent.parent.parent.gameObject.GetComponent<EquipmentBuildsScreen>();

            Plugin.Log.LogInfo($"{slotViews}");
            Plugin.Log.LogInfo($"{SelectedGearPreset}");

            // go through all slots on buildGear and add our click handler
            // and setup our ignored slots
            foreach (var (slot, slotView) in slotViews)
            {
                var slotHeader = _slotViewSlotHeaderField.GetValue(slotView) as SlotViewHeader;

                // clear selection action from header
                _slotViewHeaderSelectActionField.SetValue(slotHeader, null);

                // set interactable, so we can click it, add our click handler
                slotHeader.Interactable = true;
                slotHeader.SetSelected(false, false);
                slotHeader.OnSelected += (selected) =>
                {
                    ProfilePresetConfig.Instance.SetIgnoreSlot(ClientUtils.ProfileId, SelectedGearPreset.IdString, slot, selected);
                    buildsScreen.method_9(SelectedGearPreset.Value as GClass3182);
                };

                var transform = slotView.gameObject.transform;
                transform.Find("SlotViewHeader/BackgroundSelected/Left").GetComponent<Image>().color = _ignoreColor;
                transform.Find("SlotViewHeader/BackgroundSelected/Mid").GetComponent<Image>().color = _ignoreColor;
                transform.Find("SlotViewHeader/BackgroundSelected/Right").GetComponent<Image>().color = _ignoreColor;

                // if this slot is ignored, set a few things 
                if (ignoredSlots.Contains(slot))
                {
                    slotHeader.SetSelected(true, false);
                    // hide background and other pieces of the slotview
                    transform.Find("BackImage").gameObject.SetActive(false);
                    transform.Find("Background").gameObject.SetActive(false);
                    transform.Find("Empty Border").gameObject.SetActive(false);
                    transform.Find("Full Border").gameObject.SetActive(false);

                    // hide item view if item contained inside
                    slotView.ContainedItemView?.gameObject.SetActive(false);
                }
            }
        }
    }
}
