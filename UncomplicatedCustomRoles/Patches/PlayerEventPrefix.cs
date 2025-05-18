/*
 * This file is a part of the UncomplicatedCustomRoles project.
 * 
 * Copyright (c) 2023-present FoxWorn3365 (Federico Cosma) <me@fcosma.it>
 * 
 * This file is licensed under the GNU Affero General Public License v3.0.
 * You should have received a copy of the AGPL license along with this file.
 * If not, see <https://www.gnu.org/licenses/>.
 */

using HarmonyLib;
using LabApi.Events;
using LabApi.Events.Arguments.Interfaces;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;
using LabApi.Features.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UncomplicatedCustomRoles.API.Features;
using UncomplicatedCustomRoles.API.Features.CustomModules;
using UncomplicatedCustomRoles.Extensions;

// REVIEW (CAN BE VERY WRONG!!!)

namespace UncomplicatedCustomRoles.Patches
{
    internal class PlayerEventPrefix
    {
        private static IEnumerable<MethodInfo> PatchedMethods = new List<MethodInfo>();

        private static void Prefix(IPlayerEvent ev)
        {
            Logger.Info($"Patched ev {ev.GetType().FullName}");
        }

        internal static void Patch(Harmony harmony)
        {
            HarmonyMethod prefixMethod = new(typeof(PlayerEventPrefix).GetMethod("Prefix", BindingFlags.Static | BindingFlags.NonPublic));

            PatchedMethods = typeof(PlayerEvents).GetMethods().Where(m => m.Name.StartsWith("On"));

            foreach (MethodInfo method in PatchedMethods)
                harmony.Patch(method, prefix: prefixMethod);
        }

        internal static void Unpatch(Harmony harmony)
        {
            foreach (MethodInfo method in PatchedMethods)
                harmony.Unpatch(method, HarmonyPatchType.All);
        }
    }
}
