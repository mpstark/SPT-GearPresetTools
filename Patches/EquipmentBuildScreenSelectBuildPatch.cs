using System.Linq;
using System.Reflection;
using Aki.Reflection.Patching;
using EFT.UI;
using GearPresetTools.Features;
using GearPresetTools.Wrappers;

namespace GearPresetTools.Patches
{
    internal class EquipmentBuildScreenSelectBuildPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(EquipmentBuildsScreen).GetMethods().First(m => 
            {
                if (m.ReturnType != typeof(void))
                {
                    return false;
                }

                var parameters = m.GetParameters();
                return parameters.Count() == 1 && parameters[0].Name == "selectedBuild";
            });
        }

        [PatchPostfix]
        public static void PatchPostfix(object selectedBuild)
        {
            GearPresetIgnoreSlots.SelectedGearPreset = new GearPreset(selectedBuild);
        }
    }
}
