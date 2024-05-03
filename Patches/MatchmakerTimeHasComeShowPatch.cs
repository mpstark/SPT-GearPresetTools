using Aki.Reflection.Patching;
using EFT;
using EFT.UI.Matchmaker;
using HarmonyLib;
using System.Reflection;

namespace AutoSaveGearPreset.Patches
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
            Plugin.TrySaveCurrentGear();
        }
    }
}
