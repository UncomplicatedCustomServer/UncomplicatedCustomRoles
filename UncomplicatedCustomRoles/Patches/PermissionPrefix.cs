using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using HarmonyLib;
using System;
using UncomplicatedCustomRoles.API.Features;
using UncomplicatedCustomRoles.API.Features.CustomModules;
using UncomplicatedCustomRoles.Extensions;

namespace UncomplicatedCustomRoles.Patches
{
    [HarmonyPatch(typeof(Permissions), nameof(Permissions.CheckPermission), new Type[] { typeof(Player), typeof(string) })]
    public class PermissionPrefix
    {
        static bool Prefix(Player player, string permission, ref bool __result)
        {
            if (player.TryGetSummonedInstance(out SummonedCustomRole role) && role.GetModule(out CustomPermissions module) && module.Permissions.Contains(permission))
            {
                __result = true;
                return false;
            }

            return true;
        }
    }
}
