using HarmonyLib;

namespace UncomplicatedCustomRoles.Patches
{
    [HarmonyPatch(typeof(CharacterClassManager), nameof(CharacterClassManager.Start))]
    internal class PluginsLoaded
    {
        static void Prefix() => Plugin.Instance.OnFinishedLoadingPlugins();
    }
}
