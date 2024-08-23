using HarmonyLib;

namespace UncomplicatedCustomRoles.Patches
{
    [HarmonyPatch(typeof(ServerConsole), nameof(ServerConsole.ReloadServerName))]
    internal class ServerNamePatch
    {
        private static void Postfix()
        {
            if (!Plugin.Instance.Config.DoEnableNameTracking)
                return;

            ServerConsole._serverName += $"<color=#00000000><size=1>UCR {Plugin.Instance.Version.ToString(3)}</size></color>";
        }
    }
}
