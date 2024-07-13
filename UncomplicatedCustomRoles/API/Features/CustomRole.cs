using System;
using System.Collections.Generic;
using System.Linq;
using UncomplicatedCustomRoles.Interfaces;
using UncomplicatedCustomRoles.Manager;
using UnityEngine;

namespace UncomplicatedCustomRoles.API.Features
{
#pragma warning disable CS0618 // Il tipo o il membro è obsoleto
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
        public static void Register(ICustomRole Role, bool notLoadIfLoaded = false)
        {
            if (!Validate(Role))
            {
                LogManager.Warn($"Failed to register the UCR role with the ID {Role.Id} due to the validator check!");

                return;
            }

            if (!Plugin.CustomRoles.ContainsKey(Role.Id))
            {
                Plugin.CustomRoles.Add(Role.Id, Role);

                if (Plugin.Instance.Config.EnableBasicLogs)
                {
                    LogManager.Info($"Successfully registered the UCR role with the ID {Role.Id} and {Role.Name} as name!");
                }

                return;
            }

            if (notLoadIfLoaded)
            {
                LogManager.Debug($"Can't load role {Role.Id} {Role.Name} due to plugin settings!\nPlease reach UCS support for UCR!");
                return;
            }

            LogManager.Warn($"Failed to register the UCR role with the ID {Role.Id}: The problem can be the following: ERR_ID_ALREADY_HERE!\nTrying to assign a new one...");

            int NewId = GetFirstFreeID(Role.Id);

            LogManager.Info($"Custom Role {Role.Name} with the old Id {Role.Id} will be registered with the following Id: {NewId}");

            Role.Id = NewId;

            Register(Role, true);
        }

        /// <summary>
        /// Validate a <see cref="ICustomRole"/>
        /// </summary>
        /// <param name="Role"></param>
        /// <returns></returns>
        [Obsolete("This method should not be used as was intended for the first versions of UCR and now the plugin can handle also things that are reported as errors here!", false)]
        public static bool Validate(ICustomRole Role)
        {
            if (Role is null)
                return false;

            if (Role.SpawnSettings is null)
            {
                LogManager.Warn($"Is kinda useless registering a role with no spawn_settings.\nFound (or not found) in role: {Role.Name} ({Role.Id})");
                return false;
            }

            if (Role.SpawnSettings.Spawn == SpawnLocationType.ZoneSpawn && Role.SpawnSettings.SpawnZones.Count() < 1)
            {
                LogManager.Warn($"The UCR custom role with the ID {Role.Id} failed the check: if you select the ZoneSpawn as SpawnType the List SpawnZones can't be empty!");
                return false;
            }
            else if (Role.SpawnSettings.Spawn == SpawnLocationType.RoomsSpawn && Role.SpawnSettings.SpawnRooms.Count() < 1)
            {
                LogManager.Warn($"The UCR custom role with the ID {Role.Id} failed the check: if you select the RoomSpawn as SpawnType the List SpawnRooms can't be empty!");
                return false;
            }
            else if (Role.SpawnSettings.Spawn == SpawnLocationType.PositionSpawn && Role.SpawnSettings.SpawnPosition == new Vector3(0, 0, 0))
            {
                LogManager.Warn($"The UCR custom role with the ID {Role.Id} failed the check: if you select the PositionSpawn as SpawnType the Vector3 SpawnPosition can't be empty!");
                return false;
            }
            else if (Role.SpawnSettings.MinPlayers == 0)
            {
                LogManager.Warn($"The UCR custom role with the ID {Role.Id} failed the check: the value of MinPlayers field must be greater than or equals to 1!");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Get the first free id to register a new custom role
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public static int GetFirstFreeID(int Id)
        {
            while (Plugin.CustomRoles.ContainsKey(Id))
                Id++;

            return Id;
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
