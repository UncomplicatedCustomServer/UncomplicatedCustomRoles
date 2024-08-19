using Exiled.API.Features;
using Exiled.API.Interfaces;
using Exiled.Loader;
using System;
using System.Collections.Generic;
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
        internal static IPlugin<IConfig> ImportedPlugin => Loader.GetPlugin("ScriptedEvents");

        internal static Assembly Assembly => ImportedPlugin?.Assembly;

        internal static Type Class => Assembly?.GetType("ScriptedEvents.API.Features.ApiHelper");

        internal static MethodInfo AddMethod => Class?.GetMethod("RegisterCustomAction");

        internal static MethodInfo RemoveMethod => Class?.GetMethod("UnregisterCustomAction");

        internal static bool CanInvoke => AddMethod is not null && RemoveMethod is not null;

        internal static List<string> CustomActions { get; } = new();

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

        public static void RegisterCustomActions()
        {
            // Set custom role
            RegisterCustomAction("SET_CUSTOM_ROLE", (Tuple<string[], object> args) =>
            {
                if (args.Item1.Length < 2)
                    return new(false, "Error: the function SET_CUSTOM_ROLE requires 2 args: SET_CUSTOM_ROLE <PlayerId> <RoleId>", new object[] { });

                Player Player = Player.Get(int.Parse(args.Item1[0]));

                if (Player is null)
                    return new(false, $"Error: the given Player ({int.Parse(args.Item1[0])}) does not exists!", new object[] { });

                if (!CustomRole.CustomRoles.ContainsKey(int.Parse(args.Item1[1])))
                    return new(false, $"Error: the given CustomRole ({int.Parse(args.Item1[1])}) does not exists!", new object[] { });

                ICustomRole Role = CustomRole.CustomRoles[int.Parse(args.Item1[1])];

                Player.SetCustomRoleSync(Role);

                return new(true, string.Empty, new object[] { });
            });

            // Remove custom role
            RegisterCustomAction("REMOVE_CUSTOM_ROLE", (Tuple<string[], object> args) =>
            {
                if (args.Item1.Length < 1)
                    return new(false, "Error: the function REMOVE_CUSTOM_ROLE requires 1 args: REMOVE_CUSTOM_ROLE <PlayerId>", new object[] { });

                Player Player = Player.Get(int.Parse(args.Item1[0]));

                if (Player is null)
                    return new(false, $"Error: the given Player ({int.Parse(args.Item1[0])}) does not exists!", new object[] { });

                if (Player.HasCustomRole())
                    Player.TryRemoveCustomRole();

                return new(true, string.Empty, new object[] { });
            });

            RegisterCustomAction("GET_CUSTOM_ROLE", (Tuple<string[], object> args) =>
            {
                if (args.Item1.Length < 1)
                    return new(false, "Error: the function HAS_CUSTOM_ROLE requires 1 args: HAS_CUSTOM_ROLE <PlayerId>", new object[] { });

                Player Player = Player.Get(int.Parse(args.Item1[0]));

                if (Player is null)
                    return new(false, $"Error: the given Player ({int.Parse(args.Item1[0])}) does not exists!", new object[] { });

                if (Player.TryGetSummonedInstance(out SummonedCustomRole role))
                    return new(true, string.Empty, new object[] { role.Role.Id.ToString() });

                return new(true, string.Empty, new object[] { string.Empty });
            });
        }

        public static void UnregisterCustomActions()
        {
            if (CanInvoke)
                foreach (string Name in CustomActions)
                    RemoveMethod.Invoke(null, new object[] { Name });
        }
    }
}
