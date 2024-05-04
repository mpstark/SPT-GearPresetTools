using System;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using EFT;
using Aki.Reflection.Utils;
using GearPresetTools.Patches;
using GearPresetTools.Utils;
using HarmonyLib;
using System.Linq;

namespace GearPresetTools
{
    // the version number here is generated on build and may have a warning if not yet built
    [BepInPlugin("com.mpstark.GearPresetTools", "GearPresetTools", BuildInfo.Version)]
    public class Plugin : BaseUnityPlugin
    {
        public static Plugin Instance;
        public static ManualLogSource Log => Instance.Logger;

        public static ISession Session => ClientAppUtils.GetMainApp().GetClientBackEndSession();
        public static Profile PlayerProfile => _sessionProfileProperty.GetValue(Session) as Profile;

        private static Type _profileInterface= typeof(ISession).GetInterfaces().First(x => x.GetProperties().Length == 2 && AccessTools.Property(x, "Profile") != null);
        private static PropertyInfo _sessionProfileProperty = AccessTools.Property(_profileInterface, "Profile");

        internal void Awake()
        {
            Settings.Init(Config);
            // Config.SettingChanged += (x, y) => PlayerEncumbranceBar.OnSettingChanged();

            Instance = this;
            DontDestroyOnLoad(this);

            // patches
            new MatchMakerTimeHasComeShowPatch().Enable();
            new SaveBuildPatch().Enable();
            new LoadBuildPatch().Enable();
        }

        /// <summary>
        /// If configured, save the current state of the character as a gear preset
        /// </summary>
        public static void TrySaveCurrentGear()
        {
            if (!Settings.ShouldAutoSavePreset.Value)
            {
                return;
            }

            GearPresetUtils.SaveEquipmentAsBuild(Settings.AutoSavePresetName.Value, PlayerProfile.Inventory.Equipment);
        }

        /// <summary>
        /// If configured, remove slots from a build being saved 
        /// </summary>
        public static void TryRemoveSlotsFromSavingBuild(object build)
        {
            foreach (var (slot, setting) in Settings.DontSaveSlots)
            {
                if (!setting.Value) 
                {
                    continue;
                }

                GearPresetUtils.RemoveSlotFromBuild(build, slot);
            }
        }

        /// <summary>
        /// If configured, prevent empty slots in a preset from applying to inventory
        /// </summary>
        internal static void TryPreventEmptyOverwrite(LootItemClass presetItemCopy, LootItemClass inventoryItem)
        {
            // only affect if top level
            if (inventoryItem.Id != PlayerProfile.Inventory.Equipment.Id)
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
