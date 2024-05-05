using System.Collections.Generic;
using BepInEx.Configuration;
using EFT.InventoryLogic;
using GearPresetTools.Utils;

// THIS IS HEAVILY BASED ON DRAKIAXYZ'S SPT-QUICKMOVETOCONTAINER
namespace GearPresetTools.Config
{
    internal class Settings
    {
        public static ConfigFile Config;
        private static List<ConfigEntryBase> ConfigEntries = new List<ConfigEntryBase>();

        private const string AutoSaveSectionTitle = "Auto Save Gear Preset";
        public static ConfigEntry<bool> ShouldAutoSavePreset;
        // public static ConfigEntry<bool> ShouldAutoSavePresetPerMap;  // future implementation?
        public static ConfigEntry<string> AutoSavePresetName;

        private const string DontSaveSlotsSectionTitle = "Slots To Not Save";
        public static Dictionary<EquipmentSlot, ConfigEntry<bool>> DontSaveSlots = new Dictionary<EquipmentSlot, ConfigEntry<bool>>();

        private const string IgnoreEmptySlotSectionTitle = "Slots To Ignore If Preset Empty";
        public static Dictionary<EquipmentSlot, ConfigEntry<bool>> DontUnequipSlots = new Dictionary<EquipmentSlot, ConfigEntry<bool>>();


        public static void Init(ConfigFile Config)
        {
            Settings.Config = Config;

            ConfigEntries.Add(ShouldAutoSavePreset = Config.Bind(
                AutoSaveSectionTitle,
                "Auto Save Preset Enabled",
                true,
                new ConfigDescription(
                    "If a gear preset should be saved on starting to enter raid",
                    null,
                    new ConfigurationManagerAttributes { })));

            ConfigEntries.Add(AutoSavePresetName = Config.Bind(
                AutoSaveSectionTitle,
                "Auto Save Preset Name",
                "Last Kit",
                new ConfigDescription(
                    "What name the auto save preset should be under",
                    null,
                    new ConfigurationManagerAttributes { })));

            // add don't save config for each slot
            foreach (var slot in SlotUtils.Slots)
            {
                var slotString = slot.ToString();
                ConfigEntries.Add(DontSaveSlots[slot] = Config.Bind(
                    DontSaveSlotsSectionTitle,
                    $"Do Not Save: {slotString}",
                    false,
                    new ConfigDescription(
                        $"While saving a new preset, don't save this",
                        null,
                    new ConfigurationManagerAttributes { })));
            }

            // add don't overwrite if empty for each slow
            foreach (var slot in SlotUtils.Slots)
            {
                var slotString = slot.ToString();
                ConfigEntries.Add(DontUnequipSlots[slot] = Config.Bind(
                    IgnoreEmptySlotSectionTitle,
                    $"Do Not Unequip: {slotString}",
                    false,
                    new ConfigDescription(
                        $"While loading a saved preset with nothing in this slot, don't unequip the slot",
                        null,
                    new ConfigurationManagerAttributes { })));
            }

            RecalcOrder();
        }

        private static void RecalcOrder()
        {
            // Set the Order field for all settings, to avoid unnecessary changes when adding new settings
            int settingOrder = ConfigEntries.Count;
            foreach (var entry in ConfigEntries)
            {
                var attributes = entry.Description.Tags[0] as ConfigurationManagerAttributes;
                if (attributes != null)
                {
                    attributes.Order = settingOrder;
                }

                settingOrder--;
            }
        }
    }
}
