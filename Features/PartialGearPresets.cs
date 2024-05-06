using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using EFT;
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
        private static MethodInfo _equipmentBuildsScreenShowBuildMethod = typeof(EquipmentBuildsScreen).GetMethods().First(m =>
        {
            var parameters = m.GetParameters();
            return m.ReturnType == typeof(void) &&
                   parameters.Count() == 1 &&
                   parameters[0].ParameterType == GearPreset.WrappedType &&
                   parameters[0].Name == "equipmentBuild";
        });

        // FIXME: find these field more generically?
        private static FieldInfo _slotViewHeaderSelectActionField = AccessTools.Field(typeof(SlotViewHeader), "action_0");
        private static FieldInfo _containersPanelSlotViewsField = AccessTools.Field(typeof(ContainersPanel), "dictionary_0");

        private static Color _ignoreColor = new Color(0, 0.5f, 0.5f, 1f);

        public static void Enable()
        {
            new SaveBuildPatch().Enable();
            new InteractionsHandlerClassTransferContentPatch().Enable();
            new EquipmentBuildsScreenSelectBuildPatch().Enable();
            new EquipmentBuildsGearShowPatch().Enable();
            new ContainersPanelShowPatch().Enable();
            new EquipmentBuildScreenShowRepPatch().Enable();
        }

        /// <summary>
        /// If configured, set default value of ignore slot to config
        /// </summary>
        public static void TrySetDefaultIgnoreOnSavingBuild(GearPreset preset)
        {
            // can't use automatic (key, value) syntax here, since bsg has an extension method that adds a reference
            foreach (var pair in Settings.IgnoreSlotsByDefault)
            {
                var slot = pair.Key;
                var setting = pair.Value;

                var hasIgnoreValue = ProfilePresetConfig.Instance.SlotHasIgnoreValue(ClientUtils.ProfileId, preset.Id, slot);
                if (!hasIgnoreValue)
                {
                    ProfilePresetConfig.Instance.SetIgnoreSlot(ClientUtils.ProfileId, preset.Id, slot, setting.Value);
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
                                                                            SelectedGearPreset.Id);

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
                presetSlots[slot].ContainedItem = inventorySlots[slot].ContainedItem.ReflectedCloneItem();
            }
        }

        /// <summary>
        /// Adjusts the UI of the equipment build gear panel to reflect partial gear presets
        /// </summary>
        internal static void TryAdjustBuildGearPanel(EquipmentBuildGear panel)
        {
            var buildsScreen = panel.gameObject.GetComponentInParent<EquipmentBuildsScreen>();
            var slotViews = _equipmentTabSlotViewsField.GetValue(panel) as Dictionary<EquipmentSlot, SlotView>;
            var ignoredSlots = ProfilePresetConfig.Instance.GetIgnoredSlots(ClientUtils.ProfileId, SelectedGearPreset.Id);

            // go through all slots on buildGear and add our click handler and setup our ignored slots
            // can't use automatic (key, value) syntax here, since bsg has an extension method that adds a reference
            foreach (var pair in slotViews)
            {
                var slot = pair.Key;
                var slotView = pair.Value;
                var slotHeader = _slotViewSlotHeaderField.GetValue(slotView) as SlotViewHeader;

                // clear selection action from header
                _slotViewHeaderSelectActionField.SetValue(slotHeader, null);

                // set interactable, so we can click it, add our click handler
                slotHeader.Interactable = true;
                slotHeader.SetSelected(false, false);
                slotHeader.OnSelected += (selected) =>
                {
                    ProfilePresetConfig.Instance.SetIgnoreSlot(ClientUtils.ProfileId, SelectedGearPreset.Id, slot, selected);

                    // reselect the build, so it gets rendered again, so it is adjusted for new ignore value
                    _equipmentBuildsScreenShowBuildMethod.Invoke(buildsScreen, new [] { SelectedGearPreset.Value });
                };

                // set the colors for the slot header to be more recognizable
                var transform = slotView.gameObject.transform;
                transform.Find("SlotViewHeader/BackgroundSelected/Left").GetComponent<Image>().color = _ignoreColor;
                transform.Find("SlotViewHeader/BackgroundSelected/Mid").GetComponent<Image>().color = _ignoreColor;
                transform.Find("SlotViewHeader/BackgroundSelected/Right").GetComponent<Image>().color = _ignoreColor;

                // if this slot is ignored, change the ui to reflect that
                if (ignoredSlots.Contains(slot))
                {
                    slotHeader.SetSelected(true, false);

                    // hide background and other pieces of the slot view
                    transform.Find("BackImage").gameObject.SetActive(false);
                    transform.Find("Background").gameObject.SetActive(false);
                    transform.Find("Empty Border").gameObject.SetActive(false);
                    transform.Find("Full Border").gameObject.SetActive(false);

                    // hide item view if item contained inside
                    slotView.ContainedItemView?.gameObject.SetActive(false);
                }
            }
        }

        /// <summary>
        /// Adjusts the UI of the pouches panel of the equipment builds screen to reflect ignored slots
        /// </summary>
        internal static void TryAdjustBuildPouchesPanel(ContainersPanel panel)
        {
            var ignoredSlots = ProfilePresetConfig.Instance.GetIgnoredSlots(ClientUtils.ProfileId, SelectedGearPreset.Id);
            var slotViews = _containersPanelSlotViewsField.GetValue(panel) as Dictionary<EquipmentSlot, SlotView>;

            // hide ignored slots
            foreach (var pair in slotViews)
            {
                var slot = pair.Key;
                var slotView = pair.Value;

                if (ignoredSlots.Contains(slot))
                {
                    slotView.gameObject.SetActive(false);
                }
            }
        }

        /// <summary>
        /// Adjusts the visual representation of the selected build to remove ignored slots
        /// </summary>
        internal static void TryAdjustPlayerVisualRepresentation(PlayerVisualRepresentation visualRepresentation)
        {
            var ignoredSlots = ProfilePresetConfig.Instance.GetIgnoredSlots(ClientUtils.ProfileId, SelectedGearPreset.Id);
            foreach (var ignoredSlot in ignoredSlots)
            {
                SlotUtils.RemoveItemFromSlot(visualRepresentation.Equipment.GetSlot(ignoredSlot));
            }
        }
    }
}
