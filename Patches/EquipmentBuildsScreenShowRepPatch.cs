using System.Linq;
using System.Reflection;
using Aki.Reflection.Patching;
using EFT;
using EFT.UI;
using GearPresetTools.Features;

namespace GearPresetTools.Patches
{
    internal class EquipmentBuildScreenShowRepPatch : ModulePatch
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
                    return parameters.Count() == 1 && parameters[0].ParameterType == typeof(PlayerVisualRepresentation);
                });
        }

        [PatchPrefix]
        public static void PatchPrefix(PlayerVisualRepresentation visualRepresentation)
        {
            PartialGearPresets.TryAdjustPlayerVisualRepresentation(visualRepresentation);
        }
    }
}

