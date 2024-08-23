using HarmonyLib;
using Mirror;
using PlayerRoles;
using UncomplicatedCustomRoles.API.Features;

namespace UncomplicatedCustomRoles.Patches
{
#pragma warning disable IDE0060
    [HarmonyPatch(typeof(PlayerRoleManager), nameof(PlayerRoleManager.InitializeNewRole))]
    internal class SetRolePatch
    {
        static void Prefix(PlayerRoleManager __instance, RoleTypeId targetId, RoleChangeReason reason, RoleSpawnFlags spawnFlags = RoleSpawnFlags.All, NetworkReader data = null)
        {
            if (SummonedCustomRole.TryGet(__instance.Hub, out SummonedCustomRole role))
                role.Destroy();
        }
    }
}
