using Exiled.API.Features;
using System.Collections.Generic;
using UncomplicatedCustomRoles.Manager;
using UncomplicatedCustomRoles.Structures;
using UncomplicatedCustomRoles.Events;
using System;
using CustomPlayerEffects;
using MEC;
using PluginAPI.Core;

namespace UncomplicatedCustomRoles.API.Features
{
#nullable enable
    public partial class Manager
    {
        /// <summary>
        /// Get the <see cref="System.Version"/> of the APIs
        /// </summary>
        public static Version Version { get; } = new(2, 0, 0);

        internal static Dictionary<uint, Action> Actions { get; set; } = new();

        /// <summary>
        /// Get a <see cref="Dictionary{int, ICustomRole}"/> of every <see cref="ICustomRole"/> registered at the moment.
        /// </summary>
        public static Dictionary<int, ICustomRole> GetList()
        {
            return Plugin.CustomRoles;
        }

        /// <summary>
        /// Check if a <see cref="Player"/> is a custom role.
        /// </summary>
        public static bool HasCustomRole(Player Player)
        {
            return Plugin.PlayerRegistry.ContainsKey(Player.Id);
        }

        /// <summary>
        /// Check if a <see cref="int">PlayerId</see> is a custom role.
        /// </summary>
        public static bool HasCustomRole(int Id)
        {
            return Plugin.PlayerRegistry.ContainsKey(Id);
        }

        /// <summary>
        /// Check if a <see cref="int">CustomRoleId</see> is currently registered.
        /// </summary>
        public static bool IsRegistered(int Id)
        {
            return Plugin.CustomRoles.ContainsKey(Id);
        }

        /// <summary>
        /// Check if a <see cref="ICustomRole"/> is currently registered.
        /// </summary>
        public static bool IsRegistered(ICustomRole Role)
        {
            return Plugin.CustomRoles.ContainsKey(Role.Id);
        }

        /// <summary>
        /// Get the <see cref="ICustomRole"/> istance by it's <see cref="int">Id</see>
        /// </summary>
        public static ICustomRole Get(int Id)
        {
            return Plugin.CustomRoles[Id];
        }

        /// <summary>
        /// Summon a <see cref="Player"/> to a <see cref="int">CustomRole (id)</see>
        /// </summary>
        public static void Summon(Player Player, int Id)
        {
            Timing.RunCoroutine(UncomplicatedCustomRoles.Events.EventHandler.DoSpawnPlayer(Player, Id, true));
        }

        /// <summary>
        /// Summon a <see cref="Player"/> to a <see cref="ICustomRole"/>
        /// </summary>
        public static void Summon(Player Player, ICustomRole Role)
        {
            Summon(Player, Role.Id);
        }

        /// <summary>
        /// Register a new <see cref="ICustomRole"/> istance.
        /// </summary>
        public static void Register(ICustomRole Role)
        {
            SpawnManager.RegisterCustomSubclass(Role);
        }

        /// <summary>
        /// Get the current <see cref="ICustomRole"/> of a <see cref="Player"/>. If it have no custom role a <see cref="null"/> value will be returned.
        /// </summary>
        public static ICustomRole? Get(Player Player)
        {
            if (HasCustomRole(Player))
            {
                return Get(Plugin.PlayerRegistry[Player.Id]);
            }
            return null;
        }

        /// <summary>
        /// Get all <see cref="int">PlayerId</see> alive with a <see cref="int">CustomRoleId</see>
        /// </summary>
        public static Dictionary<int, int> GetAlive()
        {
            return Plugin.PlayerRegistry;
        }

        /// <summary>
        /// Try to get the current <see cref="ICustomRole"/> of a <see cref="Player"/>. If it have no custom role a <see cref="null"/> value will be returned.
        /// </summary>
        public static bool TryGet(Player Player, out ICustomRole? Role)
        {
            Role = Get(Player);
            if (Role is null)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Get the current number <see cref="ICustomRole"/> players in game.
        /// </summary>
        public static int Count(ICustomRole Role)
        {
            return Plugin.RolesCount[Role.Id].Count;
        }

        /// <summary>
        /// Get the current number <see cref="int">CustomRoleId</see> players in game.
        /// </summary>
        public static int Count(int Role)
        {
            return Plugin.RolesCount[Role].Count;
        }

        /// <summary>
        /// Get the current number of all players with a custom role in game.
        /// </summary>
        public static int Count()
        {
            int total = 0;
            foreach (KeyValuePair<int, List<int>> Count in Plugin.RolesCount)
            {
                total += Count.Value.Count;
            }
            return total;
        }

        /// <summary>
        /// Unregister a <see cref="int">CustomRoleId</see>.
        /// </summary>
        public static void Unregister(int Role)
        {
            if (IsRegistered(Role))
            {
                Plugin.CustomRoles.Remove(Role);
            }
        }

        /// <summary>
        /// Unregister a <see cref="ICustomRole"/>.
        /// </summary>
        public static void Unregister(ICustomRole Role)
        {
            Unregister(Role.Id);
        }

        /// <summary>
        /// Register a <see cref="ICustomFirearm"/> to the plugin
        /// </summary>
        public static void RegisterFirearm(ICustomFirearm Firearm)
        {
            RegisterFirearm(Firearm);
        }

        /// <summary>
        /// Register a new <see cref="Action"/> that will be performed every 2 seconds
        /// </summary>
        public static void RegisterAction(Action Action, uint? Id = null)
        {
            if (Action is not null)
            {
                if (Id is not null && !Actions.ContainsKey((uint)Id))
                {
                    Actions.Add((uint)Id, Action);
                }
                else
                {
                    uint RId = (uint)Actions.Count;
                    while (Actions.ContainsKey(RId))
                    {
                        RId++;
                    }
                    Actions.Add(RId, Action);
                }
            }
        }

        /// <summary>
        /// Unregister a new <see cref="Action"/> that will be performed every 2 seconds
        /// </summary>
        public static void UnregisterAction(uint Id)
        {
            if (Actions.ContainsKey(Id))
            {
                Actions.Remove(Id);
            }
        }
    }
}
