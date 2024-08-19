using Exiled.Loader;
using HarmonyLib;

namespace UncomplicatedCustomRoles.Patches
{
    [HarmonyPatch(typeof(Loader), nameof(Loader.LoadPlugins))]
    internal class PluginsLoaded
    {
        static void Postfix() => Plugin.Instance.OnFinishedLoadingPlugins();
    }
}
