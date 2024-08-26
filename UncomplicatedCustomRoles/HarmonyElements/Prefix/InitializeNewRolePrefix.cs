using HarmonyLib;
using PlayerRoles;
using UncomplicatedCustomRoles.Events;
using UncomplicatedCustomRoles.Events.Args;
using UncomplicatedCustomRoles.Events.Enums;

namespace UncomplicatedCustomRoles.HarmonyElements.Prefix
{
    [HarmonyPatch(typeof(PlayerRoleManager), nameof(PlayerRoleManager.InitializeNewRole))]
    internal class InitializeNewRolePrefix
    {
        static bool Prefix(ref RoleTypeId targetId, ref RoleChangeReason reason, ref RoleSpawnFlags spawnFlags, PlayerRoleManager __instance)
        {
            ChangingRoleEventArgs eventArgs = new(__instance.Hub, __instance.CurrentRole?.RoleTypeId ?? RoleTypeId.None, targetId, reason, spawnFlags);

            EventManager.InvokeEventClear(EventName.ChangingRole, eventArgs);

            targetId = eventArgs.NewRole;
            reason = eventArgs.RoleChangeReason;
            spawnFlags = eventArgs.RoleSpawnFlags;

            return eventArgs.IsAllowed;
        }
    }
}
