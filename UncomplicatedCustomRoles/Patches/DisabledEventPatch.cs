using Exiled.Loader;
using HarmonyLib;

namespace UncomplicatedCustomRoles.Patches
{
    [HarmonyPatch(typeof(Loader), nameof(Loader.DisablePlugins))]
    internal class DisabledEventPatch
    {
        static void Prefix() => Plugin.Instance.OnStartingUnloadingPlugins();
    }
}
