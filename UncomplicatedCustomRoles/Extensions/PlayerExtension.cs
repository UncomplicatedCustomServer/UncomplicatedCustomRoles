using Exiled.API.Features;
using MEC;
using System;
using UncomplicatedCustomRoles.API.Features;
using UncomplicatedCustomRoles.Interfaces;
using UncomplicatedCustomRoles.Manager;
using UnityEngine;

/*
 * > 05/06/2024 - A really really good day :)
*/

namespace UncomplicatedCustomRoles.Extensions
{
    public static class PlayerExtension
    {
        /// <summary>
        /// Check if a <see cref="Player"/> is currently a <see cref="ICustomRole"/>.
        /// </summary>
        /// <param name="player"></param>
        /// <returns><see cref="true"/> if the player is a custom role.</returns>
        public static bool HasCustomRole(this Player player)
        {
            return Plugin.PlayerRegistry.ContainsKey(player.Id);
        }

        /// <summary>
        /// Set a <see cref="ICustomRole"/> to a <see cref="Player"/> without a coroutine.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="role"></param>
        public static void SetCustomRoleSync(this Player player, ICustomRole role)
        {
            SpawnManager.SummonCustomSubclass(player, role.Id, true);
        }

        /// <summary>
        /// Set a <see cref="ICustomRole"/> (via it's Id) to a <see cref="Player"/> without a coroutine.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="role"></param>
        public static void SetCustomRoleSync(this Player player, int role)
        {
            SpawnManager.SummonCustomSubclass(player, role, true);
        }

        /// <summary>
        /// Set a <see cref="ICustomRole"/> (via it's Id) to a <see cref="Player"/>.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="role"></param>
        public static void SetCustomRole(this Player player, int role)
        {
            Timing.RunCoroutine(Events.EventHandler.DoSpawnPlayer(player, role));
        }

        /// <summary>
        /// Set a <see cref="ICustomRole"/> to a <see cref="Player"/>.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="role"></param>
        public static void SetCustomRole(this Player player, ICustomRole role)
        {
            Timing.RunCoroutine(Events.EventHandler.DoSpawnPlayer(player, role.Id));
        }

        /// <summary>
        /// Set every attribute of a given <see cref="ICustomRole"/> to a <see cref="Player"/> without considering the spawn settings.
        /// Use this only at your own risk and only if you know what you are doing!
        /// </summary>
        /// <param name="player"></param>
        /// <param name="role"></param>
        [Obsolete("You should not use this function unless you want to handle the role spawn by yourself!", false)]
        public static void SetCustomRoleAttributes(this Player player, ICustomRole role)
        {
            SpawnManager.SummonSubclassApplier(player, role);
        }

        /// <summary>
        /// Try to get the current <see cref="ICustomRole"/> of a <see cref="Player"/> if it's one.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="role"></param>
        /// <returns>true if the player is currently <see cref="ICustomRole"/></returns>
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

        /// <summary>
        /// Get the current <see cref="ICustomRole"/> of a <see cref="Player"/> if it's one.
        /// </summary>
        /// <param name="player"></param>
        /// <returns>The current <see cref="ICustomRole"/> if the player has one, otherwise <see cref="null"/></returns>
        public static ICustomRole GetCustomRole(this Player player)
        {
            player.TryGetCustomRole(out ICustomRole role);
            return role;
        }

        /// <summary>
        /// Try to remove a <see cref="ICustomRole"/> from a <see cref="Player"/> if it has one.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="doResetRole">If true the role will be resetted => modified stats like health and other things will be lost</param>
        /// <returns>True if success</returns>
        public static bool TryRemoveCustomRole(this Player player, bool doResetRole = false)
        {
            if (player.TryGetCustomRole(out ICustomRole role))
            {
                InfiniteEffect.Remove(player);

                if (doResetRole)
                {
                    Vector3 OriginalPosition = player.Position;

                    player.Role.Set(role.Role, PlayerRoles.RoleSpawnFlags.AssignInventory);

                    player.Position = OriginalPosition;

                    return true;
                }

                Plugin.PlayerRegistry.Remove(player.Id);
                Plugin.RolesCount[role.Id].Remove(player.Id);

                return true;
            }
            return false;
        }
    }
}
