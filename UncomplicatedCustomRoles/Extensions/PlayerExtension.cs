using Exiled.API.Features;
using MEC;
using UncomplicatedCustomRoles.Interfaces;
using UncomplicatedCustomRoles.Manager;

namespace UncomplicatedCustomRoles.Extensions
{
    public static class PlayerExtension
    {
        public static bool HasCustomRole(this Player player)
        {
            return Plugin.PlayerRegistry.ContainsKey(player.Id);
        }

        public static void SetCustomRoleSync(this Player player, ICustomRole role)
        {
            SpawnManager.SummonCustomSubclass(player, role.Id, true);
        }

        public static void SetCustomRoleSync(this Player player, int role)
        {
            SpawnManager.SummonCustomSubclass(player, role, true);
        }

        public static void SetCustomRole(this Player player, int role)
        {
            Timing.RunCoroutine(Events.EventHandler.DoSpawnPlayer(player, role));
        }

        public static void SetCustomRole(this Player player, ICustomRole role)
        {
            Timing.RunCoroutine(Events.EventHandler.DoSpawnPlayer(player, role.Id));
        }

        public static bool TryGetCustomRole(this Player player, out ICustomRole role)
        {
            if (player.HasCustomRole())
            {
                role = Plugin.CustomRoles[Plugin.PlayerRegistry[player.Id]];
                return true;
            }

            role = null;
            return false;
        }

        public static ICustomRole GetCustomRole(this Player player)
        {
            player.TryGetCustomRole(out ICustomRole role);
            return role;
        }
    }
}
