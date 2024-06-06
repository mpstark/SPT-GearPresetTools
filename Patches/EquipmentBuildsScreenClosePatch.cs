using System.Reflection;
using Aki.Reflection.Patching;
using EFT.UI;
using GearPresetTools.Features;
using HarmonyLib;
using UnityEngine;

namespace GearPresetTools.Patches
{
    internal class EquipmentBuildScreenClosePatch : ModulePatch
    {
        private static readonly float _throttleLength = 1f;
        private static float _lastCall = 0;

        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(EquipmentBuildsScreen), nameof(EquipmentBuildsScreen.Close));
        }

        [PatchPostfix]
        public static void PatchPostfix()
        {
            // throttle this, since it's called several times in succession
            var timeDiff = Time.time - _lastCall;
            if (timeDiff <= _throttleLength)
            {
                return;
            }
            _lastCall = Time.time;

            KeepProfilePresetConfigValid.SaveProfilePresetConfig();
        }
    }
}

