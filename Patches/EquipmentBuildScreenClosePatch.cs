using System;
using System.Reflection;
using Aki.Reflection.Patching;
using EFT.UI;
using GearPresetTools.Features;
using HarmonyLib;

namespace GearPresetTools.Patches
{
    internal class EquipmentBuildScreenClosePatch : ModulePatch
    {
        private static readonly TimeSpan _throttleLength = new TimeSpan(0, 0, 1); // 1 second
        private static DateTime _lastCall= DateTime.MinValue;

        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(EquipmentBuildsScreen), nameof(EquipmentBuildsScreen.Close));
        }

        [PatchPostfix]
        public static void PatchPostfix()
        {
            // throttle this, since it's called several times in succession
            var timeDiff = DateTime.UtcNow - _lastCall;
            if (timeDiff <= _throttleLength)
            {
                return;
            }
            _lastCall = DateTime.UtcNow;

            KeepProfilePresetConfigValid.SaveProfilePresetConfig();
        }
    }
}

