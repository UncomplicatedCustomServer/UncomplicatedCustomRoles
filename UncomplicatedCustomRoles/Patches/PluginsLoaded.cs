using HarmonyLib;

namespace UncomplicatedCustomRoles.Patches
{
    [HarmonyPatch(typeof(CharacterClassManager), nameof(CharacterClassManager.Start))]
    internal class PluginsLoaded
    {
        static void Postfix() => Plugin.Instance.OnFinishedLoadingPlugins();
    }
}
