using System.Reflection;
using Aki.Reflection.Patching;
using EFT.UI;
using GearPresetTools.Features;
using HarmonyLib;

namespace GearPresetTools.Patches
{
    internal class EquipmentBuildsGearShowPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(EquipmentTab), nameof(EquipmentTab.Show));
        }

        [PatchPostfix]
        public static void PatchPostfix(EquipmentTab __instance)
        {
            // this patch will catch equipment tab shows as well
            if (__instance.GetType() != typeof(EquipmentBuildGear))
            {
                return;
            }

            PartialGearPresets.TryAdjustBuildGearPanel((EquipmentBuildGear)__instance);
        }
    }
}