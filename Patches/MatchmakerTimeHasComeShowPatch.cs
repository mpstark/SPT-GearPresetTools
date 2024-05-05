using System.Reflection;
using Aki.Reflection.Patching;
using EFT;
using EFT.UI.Matchmaker;
using HarmonyLib;

namespace GearPresetTools.Patches
{
    internal class MatchMakerTimeHasComeShowPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(MatchmakerTimeHasCome), nameof(MatchmakerTimeHasCome.Show), new[] {typeof(ISession), typeof(RaidSettings)});
        }

        [PatchPostfix]
        public static void PatchPostfix()
        {
            Plugin.TrySaveCurrentGearToPreset();
        }
    }
}
