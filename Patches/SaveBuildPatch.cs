using System.Reflection;
using Aki.Reflection.Patching;
using GearPresetTools.Features;
using GearPresetTools.Wrappers;

namespace GearPresetTools.Patches
{
    internal class SaveBuildPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return GearPresetStorage.SaveBuildMethod;
        }

        [PatchPrefix]
        public static void PatchPrefix(object build)
        {
            var preset = new GearPreset(build);
            PartialPresets.TryRemoveSlotsFromSavingBuild(preset);
        }
    }
}
