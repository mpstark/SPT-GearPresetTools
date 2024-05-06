
using System;
using System.Linq;
using System.Reflection;
using Aki.Reflection.Utils;
using EFT.InventoryLogic;
using HarmonyLib;

namespace GearPresetTools.Utils
{
    public static class ItemExtensionMethods
    {
        private static Type _extensionMethodType = PatchConstants.EftTypes.First(t =>
        {
            return t.IsAbstract && t.GetMethods().Any(m => m.Name == "CloneItem");
        });

        private static MethodInfo _cloneItemMethod = AccessTools.Method(_extensionMethodType, "CloneItem");


        /// <summary>
        /// Clone the item without making a reference to GClassXXXX
        /// </summary>
        public static T ReflectedCloneItem<T>(this T originalItem, IIdGenerator idGenerator = null) where T : Item
        {
            return _cloneItemMethod.MakeGenericMethod(typeof(T)).Invoke(null, new object[] { originalItem, idGenerator }) as T;
        }
    }
}
