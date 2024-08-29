using HarmonyLib;

namespace UncomplicatedCustomRoles.Patches
{
    [HarmonyPatch(typeof(CharacterClassManager), nameof(CharacterClassManager.Start))]
    internal class ServerLoaded
    {
        private static bool _alreadyLoaded = false;
        static void Prefix()
        {
            if (!_alreadyLoaded)
                Plugin.Instance.OnFinishedLoadingPlugins();

            _alreadyLoaded = true;
        }
    }
}
