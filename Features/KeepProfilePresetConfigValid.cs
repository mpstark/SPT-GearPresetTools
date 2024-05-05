using GearPresetTools.Config;
using GearPresetTools.Patches;
using GearPresetTools.Utils;
using GearPresetTools.Wrappers;

namespace GearPresetTools.Features
{
    public static class KeepProfilePresetConfigValid
    {
        public static void Enable()
        {
            new EquipmentBuildScreenClosePatch().Enable();
        }

        public static void SaveProfilePresetConfig()
        {
            var presetIds = GearPresetStorage.Instance.GetAllGearPresetIds();
            ProfilePresetConfig.Instance.RemoveInvalidPresetIds(ClientUtils.ProfileId, presetIds);
            ProfilePresetConfig.Instance.SaveToFile();
        }
    }
}