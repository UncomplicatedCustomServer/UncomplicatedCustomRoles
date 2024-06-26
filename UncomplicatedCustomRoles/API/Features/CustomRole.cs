using System.Collections.Generic;
using System.Linq;
using UncomplicatedCustomRoles.Interfaces;
using UncomplicatedCustomRoles.Manager;

namespace UncomplicatedCustomRoles.API.Features
{
    public class CustomRole
    {
        /// <summary>
        /// Get a list of every <see cref="ICustomRole"/> registered.
        /// </summary>
        public static List<ICustomRole> List { get; } = new(Plugin.CustomRoles.Values);

        /// <summary>
        /// Handle the Respawn Queue for waves handled by UCR.
        /// </summary>
        public List<int> RespawnQueue = Plugin.RoleSpawnQueue;

        /// <summary>
        /// Get a list of every alive player with a custom role and the custom role Id.
        /// </summary>
        public static Dictionary<int, int> Alive { get; } = Plugin.PlayerRegistry;

        /// <summary>
        /// Try to get a registered <see cref="ICustomRole"/> by it's Id.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="customRole"></param>
        /// <returns><see cref="true"/> if the operation was successfull.</returns>
        public static bool TryGet(int id, out ICustomRole customRole)
        {
            if (Plugin.CustomRoles.ContainsKey(id))
            {
                customRole = Plugin.CustomRoles[id];
                return true;
            }

            customRole = null;
            return false;
        }

        /// <summary>
        /// Get a registered <see cref="ICustomRole"/> by it's Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns>The <see cref="ICustomRole"/> with the given Id or <see cref="null"/> if not found.</returns>
        public static ICustomRole Get(int id)
        {
            if (TryGet(id, out ICustomRole customRole)) 
            { 
                return customRole; 
            }

            return null;
        }

        /// <summary>
        /// Register a new <see cref="ICustomRole"/>.
        /// Required only if you want the custom role to be evaluated from UCR.
        /// </summary>
        /// <param name="customRole"></param>
        public static void Register(ICustomRole customRole)
        {
            SpawnManager.RegisterCustomSubclass(customRole);
        }

        /// <summary>
        /// Unregister a registered <see cref="ICustomRole"/>.
        /// </summary>
        /// <param name="customRole"></param>
        public static void Unregister(ICustomRole customRole)
        {
            if (Plugin.CustomRoles.ContainsKey(customRole.Id))
            {
                Plugin.CustomRoles.Remove(customRole.Id);
            }
        }

        /// <summary>
        /// Get the number of <see cref="Exiled.API.Features.Player"/>s with a specific <see cref="ICustomRole"/>.
        /// </summary>
        /// <param name="Role"></param>
        /// <returns>An int with the required number.</returns>
        public static int Count(ICustomRole Role)
        {
            return Plugin.RolesCount[Role.Id].Count;
        }


        /// <summary>
        /// Get the current number of all <see cref="Exiled.API.Features.Player"/>s with a <see cref="ICustomRole"/> in game.
        /// </summary>
        /// <returns>An int with the required number.</returns>
        public static int Count()
        {
            int total = 0;
            foreach (KeyValuePair<int, List<int>> Count in Plugin.RolesCount)
            {
                total += Count.Value.Count;
            }
            return total;
        }
    }
}
