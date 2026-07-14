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
using System.Reflection;
using HarmonyLib;
using LabApi.Events.Arguments.Interfaces;
using LabApi.Events.Handlers;
using UncomplicatedCustomRoles.API.Features;
using UncomplicatedCustomRoles.Extensions;
using UncomplicatedCustomRoles.Manager;

namespace UncomplicatedCustomRoles.Patches;

internal class PlayerEventPrefix
{
    private static List<MethodInfo> _patchedMethods = [];

    private static readonly Dictionary<Type, string> EventNameCache = new();

    private static void Prefix(IPlayerEvent ev)
    {
        try
        {
            CustomRoleEventHandler.InvokeAll(ev);

            if (SummonedCustomRole.EventTriggeredModuleTotal > 0
                && ev.Player is not null && ev.Player.TryGetSummonedInstance(out var customRole))
            {
                var eventType = ev.GetType();
                if (!EventNameCache.TryGetValue(eventType, out var name))
                {
                    name = eventType.Name.Replace("EventArgs", string.Empty).Replace("Player", string.Empty);
                    EventNameCache[eventType] = name;
                }

                foreach (var module in customRole.CustomModules)
                    if (module.TriggerOnEvents.Contains(name))
                        if (!module.OnEvent(name, ev) && ev is ICancellableEvent deniableEvent)
                            deniableEvent.IsAllowed = false;
            }
        }
        catch (Exception ex)
        {
            LogManager.Error(ex.ToString());
        }
    }

    internal static void Patch(Harmony harmony)
    {
        HarmonyMethod prefixMethod =
            new(typeof(PlayerEventPrefix).GetMethod("Prefix", BindingFlags.Static | BindingFlags.NonPublic));

        _patchedMethods = typeof(PlayerEvents).GetMethods().Where(m =>
            m.Name.StartsWith("On") && m.GetParameters().Length > 0 &&
            typeof(IPlayerEvent).IsAssignableFrom(m.GetParameters()[0].ParameterType)).ToList();

        foreach (var method in _patchedMethods)
            harmony.Patch(method, prefixMethod);
    }

    internal static void Unpatch(Harmony harmony)
    {
        foreach (var method in _patchedMethods)
            harmony.Unpatch(method, HarmonyPatchType.All);
    }
}