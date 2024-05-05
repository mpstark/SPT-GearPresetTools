using System.Collections.Generic;
using System.IO;
using EFT.InventoryLogic;
using Newtonsoft.Json;

namespace GearPresetTools.Config
{
    public class ProfilePresetConfig
    {
        private class PerPresetConfig
        {
            // this class is in case we want more per-preset config added on
            public Dictionary<EquipmentSlot, bool> SlotsToIgnore = new Dictionary<EquipmentSlot, bool>();
        }

        private class PerProfileConfig
        {
            // this class is in case we want more per-profile config added on
            public Dictionary<string, PerPresetConfig> Presets = new Dictionary<string, PerPresetConfig>();
        }

        // [profile id] => PerProfileConfig.Presets[preset id] => PerPresetConfig
        private Dictionary<string, PerProfileConfig> _profiles = new Dictionary<string, PerProfileConfig>();

        public ProfilePresetConfig()
        {
        }

        public ProfilePresetConfig(string jsonPath)
        {
            try
            {
                _profiles = JsonConvert.DeserializeObject<Dictionary<string, PerProfileConfig>>(File.ReadAllText(jsonPath));
            }
            catch
            {
                Plugin.Log.LogWarning($"Loading ProfilePresetConfig failed from json at path: {jsonPath}");
            }
        }

        public void SetIgnoreSlot(string profileId, string presetId, EquipmentSlot slot, bool ignoreSlot)
        {
            if (!_profiles.ContainsKey(profileId))
                _profiles[profileId] = new PerProfileConfig();

            if (!_profiles[profileId].Presets.ContainsKey(presetId))
                _profiles[profileId].Presets[presetId] = new PerPresetConfig();

            _profiles[profileId].Presets[presetId].SlotsToIgnore[slot] = ignoreSlot;
        }

        public bool ShouldIgnoreSlot(string profileId, string presetId, EquipmentSlot slot)
        {
            if (!_profiles.ContainsKey(profileId) ||
                !_profiles[profileId].Presets.ContainsKey(presetId) ||
                !_profiles[profileId].Presets[presetId].SlotsToIgnore.ContainsKey(slot))
            {
                return false;
            }

            return _profiles[profileId].Presets[presetId].SlotsToIgnore[slot];
        }

        public void SaveToFile(string filePath)
        {
            File.WriteAllText(filePath, JsonConvert.SerializeObject(_profiles, Formatting.Indented));
        }
    }
}
