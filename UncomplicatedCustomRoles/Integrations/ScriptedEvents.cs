﻿/*
 * This file is a part of the UncomplicatedCustomRoles project.
 * 
 * Copyright (c) 2023-present FoxWorn3365 (Federico Cosma) <me@fcosma.it>
 * 
 * This file is licensed under the GNU Affero General Public License v3.0.
 * You should have received a copy of the AGPL license along with this file.
 * If not, see <https://www.gnu.org/licenses/>.
 */

using Exiled.API.Features;
using Exiled.Loader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UncomplicatedCustomRoles.API.Features;
using UncomplicatedCustomRoles.API.Interfaces;
using UncomplicatedCustomRoles.Extensions;
using UncomplicatedCustomRoles.Manager;

namespace UncomplicatedCustomRoles.Integrations
{
    internal class ScriptedEvents
    {
        /// <summary>
        /// Gets the <see cref="System.Reflection.Assembly"/> of ScriptedEvents
        /// </summary>
        internal static Assembly Assembly => Loader.GetPlugin("ScriptedEvents")?.Assembly;

        /// <summary>
        /// Gets the <see cref="Type"/> of the API class
        /// </summary>
        internal static Type Class => Assembly?.GetType("ScriptedEvents.API.Features.ApiHelper");

        /// <summary>
        /// Gets the main class of Scripted Events
        /// </summary>
        internal static object MainClass => Assembly?.GetType("ScriptedEvents.MainPlugin")?.GetProperty("Singleton").GetValue(null, null);

        /// <summary>
        /// Gets the current version of ScriptedEvents
        /// </summary>
        internal static Version Version => (Version)(Assembly?.GetType("ScriptedEvents.MainPlugin")?.GetProperty("Version")?.GetValue(MainClass, null) ?? new Version(0, 0, 0));

        /// <summary>
        /// Gets the RegisterCustomAction method
        /// </summary>
        internal static MethodInfo AddMethod => Class?.GetMethod("RegisterCustomAction");

        /// <summary>
        /// Gets the UnregisterCustomAction method
        /// </summary>
        internal static MethodInfo RemoveMethod => Class?.GetMethod("UnregisterCustomAction");

        /// <summary>
        /// Gets the GetPlayers method
        /// </summary>
        internal static MethodInfo GetPlayerMethod => Class?.GetMethod("GetPlayers");

        /// <summary>
        /// Gets whether the integration can be invoked
        /// </summary>
        internal static bool CanInvoke => AddMethod is not null && RemoveMethod is not null && IsRightVersion;

        /// <summary>
        /// Gets whether the version is correct or not
        /// </summary>
        internal static bool IsRightVersion { get; private set; } = true;

        /// <summary>
        /// Gets a list of every CustomAction registered by UCR
        /// </summary>
        internal static List<string> CustomActions { get; } = new();

        private static bool _alreadyLoaded = false;

        /// <summary>
        /// Register a new CustomAction
        /// </summary>
        /// <param name="name"></param>
        /// <param name="action"></param>
        public static void RegisterCustomAction(string name, Func<Tuple<string[], object>, Tuple<bool, string, object[]>> action)
        {
            if (CanInvoke)
            {
                try
                {
                    AddMethod.Invoke(null, new object[] { name, action });
                    CustomActions.Add(name);
                    LogManager.Debug($"Successfully registered the ScriptedEvents CustomAction for UCR with the name '{name}'");
                }
                catch (Exception e)
                {
                    LogManager.Error($"{e.Source} - {e.GetType().FullName} error: {e.Message}");
                }
            }
        }

        /// <summary>
        /// Register every expected CustomAction native of UCR
        /// </summary>
        public static void RegisterCustomActions()
        {
            if (_alreadyLoaded)
                return;

            if (CanInvoke && Version.CompareTo(new(3, 1, 6)) < 0)
            {
                IsRightVersion = false;
                LogManager.Warn("The ScriptedEvents integration of UCR can't be enabled as your version of ScriptedEvents is OUTDATED!\nRequired: >= 3.1.6 - Found: " + Version);
                return;
            }

            // Set custom role
            RegisterCustomAction("SET_UCR_ROLE", (Tuple<string[], object> args) =>
            {
                if (args.Item1.Length < 2)
                    return new(false, "Error: the function SET_UCR_ROLE requires 2 args: SET_UCR_ROLE <PlayerId> <RoleId>", null);

                Player Player = GetPlayerFromArgs(args);

                if (Player is null)
                    return new(false, $"Error: the given Player ({args.Item1.ElementAt(0)}) does not exists!", null);

                if (!CustomRole.CustomRoles.ContainsKey(int.Parse(args.Item1[1])))
                    return new(false, $"Error: the given CustomRole ({int.Parse(args.Item1[1])}) does not exists!", null);

                ICustomRole Role = CustomRole.CustomRoles[int.Parse(args.Item1[1])];

                Player.SetCustomRoleSync(Role);

                return new(true, string.Empty, null);
            });

            // Remove custom role
            RegisterCustomAction("REMOVE_UCR_ROLE", (Tuple<string[], object> args) =>
            {
                if (args.Item1.Length < 1)
                    return new(false, "Error: the function REMOVE_UCR_ROLE requires 1 args: REMOVE_UCR_ROLE <PlayerId>", null);

                Player Player = GetPlayerFromArgs(args);

                if (Player is null)
                    return new(false, $"Error: the given Player ({args.Item1.ElementAt(0)}) does not exists!", null);

                if (Player.HasCustomRole())
                    Player.TryRemoveCustomRole();

                return new(true, string.Empty, null);
            });

            RegisterCustomAction("GET_UCR_ROLE", (Tuple<string[], object> args) =>
            {
                if (args.Item1.Length < 1)
                    return new(false, "Error: the function GET_UCR_ROLE requires 1 args: GET_UCR_ROLE <PlayerId>", null);

                Player Player = GetPlayerFromArgs(args);

                if (Player is null)
                    return new(false, $"Error: the given Player ({args.Item1.ElementAt(0)}) does not exists!", null);

                if (Player.TryGetSummonedInstance(out SummonedCustomRole role))
                    return new(true, string.Empty, new object[] { role.Role.Id.ToString() });

                return new(true, string.Empty, null);
            });

            _alreadyLoaded = true;
        }

        /// <summary>
        /// Unregister every registered CustomAction registered in <see cref="CustomActions"/>
        /// </summary>
        public static void UnregisterCustomActions()
        {
            if (CanInvoke)
                foreach (string Name in CustomActions)
                    RemoveMethod.Invoke(null, new object[] { Name });

            CustomActions.Clear();
        }

        /// <summary>
        /// Try to get a Player from the given input
        /// </summary>
        /// <param name="input"></param>
        /// <param name="script"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        internal static Player GetPlayer(string input, object script) => ((Player[])GetPlayerMethod.Invoke(null, new[] { input, script, 1 })).FirstOrDefault();

        /// <summary>
        /// Try to get a Player from the given input, supposing the player is the first argument
        /// </summary>
        /// <param name="input"></param>
        /// <param name="script"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        internal static Player GetPlayerFromArgs(Tuple<string[], object> args, int index = 0) => GetPlayer(args.Item1.ElementAt(index), args.Item2);
    }
}
