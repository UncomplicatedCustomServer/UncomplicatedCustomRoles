using Exiled.API.Features;
using Exiled.API.Interfaces;
using Exiled.Loader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UncomplicatedCustomRoles.API.Features;
using UncomplicatedCustomRoles.Extensions;
using UncomplicatedCustomRoles.Interfaces;
using UncomplicatedCustomRoles.Manager;
using Unity.Collections.LowLevel.Unsafe;

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
        internal static bool CanInvoke => AddMethod is not null && RemoveMethod is not null;

        /// <summary>
        /// Gets a list of every CustomAction registered by UCR
        /// </summary>
        internal static List<string> CustomActions { get; } = new();

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
            // Set custom role
            RegisterCustomAction("SET_CUSTOM_ROLE", (Tuple<string[], object> args) =>
            {
                if (args.Item1.Length < 2)
                    return new(false, "Error: the function SET_CUSTOM_ROLE requires 2 args: SET_CUSTOM_ROLE <PlayerId> <RoleId>", null);

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
            RegisterCustomAction("REMOVE_CUSTOM_ROLE", (Tuple<string[], object> args) =>
            {
                if (args.Item1.Length < 1)
                    return new(false, "Error: the function REMOVE_CUSTOM_ROLE requires 1 args: REMOVE_CUSTOM_ROLE <PlayerId>", null);

                Player Player = GetPlayerFromArgs(args);

                if (Player is null)
                    return new(false, $"Error: the given Player ({args.Item1.ElementAt(0)}) does not exists!", null);

                if (Player.HasCustomRole())
                    Player.TryRemoveCustomRole();

                return new(true, string.Empty, null);
            });

            RegisterCustomAction("GET_CUSTOM_ROLE", (Tuple<string[], object> args) =>
            {
                if (args.Item1.Length < 1)
                    return new(false, "Error: the function HAS_CUSTOM_ROLE requires 1 args: HAS_CUSTOM_ROLE <PlayerId>", null);

                Player Player = GetPlayerFromArgs(args);

                if (Player is null)
                    return new(false, $"Error: the given Player ({args.Item1.ElementAt(0)}) does not exists!", null);

                if (Player.TryGetSummonedInstance(out SummonedCustomRole role))
                    return new(true, string.Empty, new object[] { role.Role.Id.ToString() });

                return new(true, string.Empty, null);
            });
        }

        /// <summary>
        /// Unregister every registered CustomAction registered in <see cref="CustomActions"/>
        /// </summary>
        public static void UnregisterCustomActions()
        {
            if (CanInvoke)
                foreach (string Name in CustomActions)
                    RemoveMethod.Invoke(null, new object[] { Name });
        }

        /// <summary>
        /// Try to get a Player from the given input
        /// </summary>
        /// <param name="input"></param>
        /// <param name="script"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        internal static Player GetPlayer(string input, object script)
        {
            return ((Player[])GetPlayerMethod.Invoke(null, new[] { input, script, 1 })).FirstOrDefault();
        }

        /// <summary>
        /// Try to get a Player from the given input, supposing the player is the first argument
        /// </summary>
        /// <param name="input"></param>
        /// <param name="script"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        internal static Player GetPlayerFromArgs(Tuple<string[], object> args, int index = 0)
        {
            return GetPlayer(args.Item1.ElementAt(index), args.Item2);
        }
    }
}
