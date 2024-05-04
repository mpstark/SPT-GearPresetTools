using System.Reflection;
using Aki.Reflection.Patching;
using GearPresetTools.Utils;

namespace GearPresetTools.Patches
{
    internal class SaveBuildPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return GearPresetUtils.BuildStorageSaveBuildMethod;
        }

        [PatchPrefix]
        public static void PatchPrefix(object build)
        {
            Plugin.TryRemoveSlotsFromSavingBuild(build);
        }
    }
}
