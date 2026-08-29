/*
 * This file is a part of the UncomplicatedCustomRoles project.
 *
 * Copyright (c) 2023-present FoxWorn3365 (Federico Cosma) <me@fcosma.it>
 *
 * This file is licensed under the GNU Affero General Public License v3.0.
 * You should have received a copy of the AGPL license along with this file.
 * If not, see <https://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using LabApi.Features.Wrappers;
using UncomplicatedCustomRoles.API.Features;
using UncomplicatedCustomRoles.API.Interfaces;
using UncomplicatedCustomRoles.Extensions;
using UncomplicatedCustomRoles.Manager;

namespace UncomplicatedCustomRoles.Integrations;

internal static class ScriptedEvents
{
    private static object _mainPlugin;

    private static bool _alreadyLoaded;

    /// <summary>
    ///     Gets the main class of Scripted Events
    /// </summary>
    internal static object MainPlugin => _mainPlugin ??= DynamicInvoke.GetMethod("ScriptedEvents", "ScriptedEvents.MainPlugin.Singleton_get");

    /// <summary>
    ///     Gets the current version of ScriptedEvents
    /// </summary>
    internal static Version Version
    {
        get
        {
            if (field is not null)
                return field;
            try
            {
                field = (Version)(DynamicInvoke.GetMethod("ScriptedEvents", "ScriptedEvents.MainPlugin.Version_get")?.Invoke(MainPlugin, []) ?? new Version(0, 0, 0));
            }
            catch
            {
                field = new Version(0, 0, 0);
            }

            return field;
        }
    }

    /// <summary>
    ///     Gets whether the version is correct or not
    /// </summary>
    internal static bool IsRightVersion => Version.CompareTo(new Version(3, 1, 6)) > 0;

    /// <summary>
    ///     Gets a list of every CustomAction registered by UCR
    /// </summary>
    internal static List<string> CustomActions { get; } = [];

    /// <summary>
    ///     Register a new CustomAction
    /// </summary>
    /// <param name="name"></param>
    /// <param name="action"></param>
    public static void RegisterCustomAction(string name, Func<Tuple<string[], object>, Tuple<bool, string, object[]>> action)
    {
        try
        {
            DynamicInvoke.GetMethod("ScriptedEvents", "ScriptedEvents.API.Features.ApiHelper.RegisterCustomAction")?.Invoke(null, [name, action]);
            CustomActions.Add(name);
            LogManager.Debug($"Successfully registered the ScriptedEvents CustomAction for UCR with the name '{name}'");
        }
        catch (Exception e)
        {
            LogManager.Error($"{e.Source} - {e.GetType().FullName} error: {e.Message}");
        }
    }

    /// <summary>
    ///     Register every expected CustomAction native of UCR
    /// </summary>
    public static void RegisterCustomActions()
    {
        if (_alreadyLoaded)
            return;

        if (!IsRightVersion)
        {
            if (Version == new Version(0, 0, 0))
                return;

            LogManager.Warn("The ScriptedEvents integration of UCR can't be enabled as your version of ScriptedEvents is OUTDATED!\nRequired: >= 3.1.6 - Found: " + Version);
            return;
        }

        // Set custom role
        RegisterCustomAction("SET_UCR_ROLE", args =>
        {
            if (args.Item1.Length < 2)
                return new Tuple<bool, string, object[]>(false, "Error: the function SET_UCR_ROLE requires 2 args: SET_UCR_ROLE <PlayerId> <RoleId>", null);

            Player Player = GetPlayerFromArgs(args);

            if (Player is null)
                return new Tuple<bool, string, object[]>(false, $"Error: the given Player ({args.Item1.ElementAt(0)}) does not exists!", null);

            if (!int.TryParse(args.Item1[1], out int roleId))
                return new Tuple<bool, string, object[]>(false, $"Error: the given CustomRole Id ({args.Item1[1]}) is not a number!", null);

            if (!CustomRole.CustomRoles.TryGetValue(roleId, out ICustomRole Role))
                return new Tuple<bool, string, object[]>(false, $"Error: the given CustomRole ({roleId}) does not exists!", null);

            Player.SetCustomRoleSync(Role);

            return new Tuple<bool, string, object[]>(true, string.Empty, null);
        });

        // Remove custom role
        RegisterCustomAction("REMOVE_UCR_ROLE", args =>
        {
            if (args.Item1.Length < 1)
                return new Tuple<bool, string, object[]>(false, "Error: the function REMOVE_UCR_ROLE requires 1 args: REMOVE_UCR_ROLE <PlayerId>", null);

            Player Player = GetPlayerFromArgs(args);

            if (Player is null)
                return new Tuple<bool, string, object[]>(false, $"Error: the given Player ({args.Item1.ElementAt(0)}) does not exists!", null);

            if (Player.HasCustomRole())
                Player.TryRemoveCustomRole();

            return new Tuple<bool, string, object[]>(true, string.Empty, null);
        });

        RegisterCustomAction("GET_UCR_ROLE", args =>
        {
            if (args.Item1.Length < 1)
                return new Tuple<bool, string, object[]>(false, "Error: the function GET_UCR_ROLE requires 1 args: GET_UCR_ROLE <PlayerId>", null);

            Player Player = GetPlayerFromArgs(args);

            if (Player is null)
                return new Tuple<bool, string, object[]>(false, $"Error: the given Player ({args.Item1.ElementAt(0)}) does not exists!", null);

            if (Player.TryGetSummonedInstance(out SummonedCustomRole role))
                return new Tuple<bool, string, object[]>(true, string.Empty, [role.Role.Id.ToString()]);

            return new Tuple<bool, string, object[]>(true, string.Empty, null);
        });

        _alreadyLoaded = true;
    }

    /// <summary>
    ///     Unregister every registered CustomAction registered in <see cref="CustomActions" />
    /// </summary>
    public static void UnregisterCustomActions()
    {
        foreach (string Name in CustomActions)
            DynamicInvoke.GetMethod("ScriptedEvents", "ScriptedEvents.API.Features.ApiHelper.UnregisterCustomAction")?.Invoke(null, [Name]);

        CustomActions.Clear();
    }

    /// <summary>
    ///     Try to get a Player from the given input
    /// </summary>
    /// <param name="input"></param>
    /// <param name="script"></param>
    /// <param name="max"></param>
    /// <returns></returns>
    internal static Player GetPlayer(string input, object script)
    {
        return ((Player[])DynamicInvoke.GetMethod("ScriptedEvents", "ScriptedEvents.API.Features.ApiHelper.GetPlayers")?.Invoke(null, [input, script, 1])).FirstOrDefault();
    }

    /// <summary>
    ///     Try to get a Player from the given input, supposing the player is the first argument
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