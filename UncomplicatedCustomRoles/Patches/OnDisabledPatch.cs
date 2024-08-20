using Exiled.Loader;
using HarmonyLib;

namespace UncomplicatedCustomRoles.Patches
{
    [HarmonyPatch(typeof(Loader), nameof(Loader.DisablePlugins))]
    internal class OnDisabledPatch
    {
        static void Prefix() => Plugin.Instance.OnStartingUnloadingPlugins();
    }
}
