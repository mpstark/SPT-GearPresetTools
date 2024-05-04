using System.Reflection;
using Aki.Reflection.Patching;
using HarmonyLib;

namespace GearPresetTools.Patches
{
    internal class LoadBuildPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(InteractionsHandlerClass), "TransferContent");
        }

        [PatchPrefix]
        public static void PatchPrefix(ref LootItemClass presetItem, LootItemClass toItem, bool createFakeItems)
        {
            // only call when trying to actually equip items
            if (createFakeItems)
            {
                return;
            }
            
            // copy the preset so that we don't actually modify the client version of the preset
            var copied = presetItem.CloneItem();
            presetItem = copied;

            Plugin.TryPreventEmptyOverwrite(copied, toItem);
        }
    }
}
