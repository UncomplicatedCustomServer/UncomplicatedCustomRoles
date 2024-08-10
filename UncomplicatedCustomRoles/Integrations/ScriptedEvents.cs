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

        public static void RegisterCustomAction(string name, Func<string[], Tuple<bool, string>> action)
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
            RegisterCustomAction("SET_CUSTOM_ROLE", (string[] args) =>
            {
                if (args.Length < 2)
                    return new(false, "Error: the function SET_CUSTOM_ROLE requires 2 args: SET_CUSTOM_ROLE <PlayerId> <RoleId>");

                Player Player = Player.Get(int.Parse(args[0]));

                if (Player is null)
                    return new(false, $"Error: the given Player ({int.Parse(args[0])}) does not exists!");

                if (!CustomRole.CustomRoles.ContainsKey(int.Parse(args[1])))
                    return new(false, $"Error: the given CustomRole ({int.Parse(args[1])}) does not exists!");

                ICustomRole Role = CustomRole.CustomRoles[int.Parse(args[1])];

                Player.SetCustomRoleSync(Role);

                return new(true, string.Empty);
            });

            // Remove custom role
            RegisterCustomAction("REMOVE_CUSTOM_ROLE", (string[] args) =>
            {
                if (args.Length < 1)
                    return new(false, "Error: the function REMOVE_CUSTOM_ROLE requires 1 args: REMOVE_CUSTOM_ROLE <PlayerId>");

                Player Player = Player.Get(int.Parse(args[0]));

                if (Player is null)
                    return new(false, $"Error: the given Player ({int.Parse(args[0])}) does not exists!");

                if (Player.HasCustomRole())
                    Player.TryRemoveCustomRole();

                return new(true, string.Empty);
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
