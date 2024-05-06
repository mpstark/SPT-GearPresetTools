using System.Reflection;
using Aki.Reflection.Patching;
using EFT.UI;
using GearPresetTools.Features;
using HarmonyLib;

namespace GearPresetTools.Patches
{
    internal class ContainersPanelShowPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(ContainersPanel), nameof(ContainersPanel.Show));
        }

        [PatchPostfix]
        public static void PatchPostfix(ContainersPanel __instance)
        {
            // only call if this is in equipment builds screen
            var screen = __instance.gameObject.GetComponentInParent<EquipmentBuildsScreen>();
            if (screen == null)
            {
                return;
            }

            PartialGearPresets.TryAdjustBuildPouchesPanel(__instance);
        }
    }
}
