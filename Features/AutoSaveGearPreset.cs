using GearPresetTools.Config;
using GearPresetTools.Patches;
using GearPresetTools.Utils;
using GearPresetTools.Wrappers;

namespace GearPresetTools.Features
{
    public static class AutoSaveGearPreset
    {
        public static void Enable()
        {
            new MatchMakerTimeHasComeShowPatch().Enable();
        }

        /// <summary>
        /// If configured, save the current state of the character as a gear preset
        /// </summary>
        public static void TrySave()
        {
            if (!Settings.ShouldAutoSavePreset.Value)
            {
                return;
            }

            GearPresetStorage.Instance.SaveEquipmentAsBuild(
                Settings.AutoSavePresetName.Value,
                ClientUtils.SessionProfile.Inventory.Equipment);
        }
    }
}