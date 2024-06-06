using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EFT.InventoryLogic;
using Newtonsoft.Json;

namespace GearPresetTools.Config
{
    public class ProfilePresetConfig
    {
        private static string _configPath = Path.Combine(Plugin.Path, "ProfilePresetConfig.json");

        // lazy singleton
        private static readonly Lazy<ProfilePresetConfig> lazy = new Lazy<ProfilePresetConfig>(() => new ProfilePresetConfig());
        public static ProfilePresetConfig Instance { get { return lazy.Value; } }

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
            if (!File.Exists(_configPath))
            {
                return;
            }

            // try to load from path
            try
            {
                _profiles = JsonConvert.DeserializeObject<Dictionary<string, PerProfileConfig>>(File.ReadAllText(_configPath));
                return;
            }
            catch (Exception e)
            {
                Plugin.Log.LogError($"Loading ProfilePresetConfig failed from json at path: {_configPath}");
                Plugin.Log.LogError($"Exception given was: {e.Message}");
                Plugin.Log.LogError($"{e.StackTrace}");
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

        public bool SlotHasIgnoreValue(string profileId, string presetId, EquipmentSlot slot)
        {
            if (!_profiles.ContainsKey(profileId) ||
                !_profiles[profileId].Presets.ContainsKey(presetId))
            {
                return false;
            }

            return _profiles[profileId].Presets[presetId].SlotsToIgnore.ContainsKey(slot);
        }

        public HashSet<EquipmentSlot> GetIgnoredSlots(string profileId, string presetId)
        {
            var slots = new HashSet<EquipmentSlot>();
            if (!_profiles.ContainsKey(profileId) || !_profiles[profileId].Presets.ContainsKey(presetId))
            {
                return slots;
            }

            // get all slots that should be ignored
            // can't use automatic (key, value) syntax here, since bsg has an extension method that adds a reference
            foreach (var pair in _profiles[profileId].Presets[presetId].SlotsToIgnore)
            {
                var slot = pair.Key;
                var shouldIgnore = pair.Value;

                if (shouldIgnore)
                {
                    slots.Add(slot);
                }
            }

            return slots;
        }

        public void RemoveInvalidPresetIds(string profileId, IEnumerable<string> validPresetIds)
        {
            if (!_profiles.ContainsKey(profileId))
            {
                return;
            }

            var validIds = new HashSet<string>(validPresetIds);
            var invalidIds = _profiles[profileId].Presets.Where(p => !validIds.Contains(p.Key))
                                                         .Select(p => p.Key)
                                                         .ToList();

            foreach (var invalidId in invalidIds)
            {
                _profiles[profileId].Presets.Remove(invalidId);
            }
        }

        public void SaveToFile()
        {
            File.WriteAllText(_configPath, JsonConvert.SerializeObject(_profiles, Formatting.Indented));
        }
    }
}
