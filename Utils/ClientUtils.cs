using System;
using System.Linq;
using System.Reflection;
using Aki.Reflection.Utils;
using EFT;
using HarmonyLib;

namespace GearPresetTools.Utils
{
    public static class ClientUtils
    {
        private static Type _profileInterface = typeof(ISession).GetInterfaces().First(i =>
            {
                var properties = i.GetProperties();
                return properties.Length == 2 &&
                       properties.Any(p => p.Name == "Profile");
            });
        private static PropertyInfo _sessionProfileProperty = AccessTools.Property(_profileInterface, "Profile");

        public static ISession Session => ClientAppUtils.GetMainApp().GetClientBackEndSession();
        public static Profile SessionProfile => _sessionProfileProperty.GetValue(Session) as Profile;
        public static string ProfileId => SessionProfile.ProfileId;
    }
}