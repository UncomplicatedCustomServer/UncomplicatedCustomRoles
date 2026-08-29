/*
 * This file is a part of the UncomplicatedCustomRoles project.
 *
 * Copyright (c) 2023-present FoxWorn3365 (Federico Cosma) <me@fcosma.it>
 *
 * This file is licensed under the GNU Affero General Public License v3.0.
 * You should have received a copy of the AGPL license along with this file.
 * If not, see <https://www.gnu.org/licenses/>.
 */

using System.Collections.Generic;
using HarmonyLib;
using PlayerRoles;
using Respawning.NamingRules;
using UnityEngine;

namespace UncomplicatedCustomRoles.Patches;

internal static class PendingUnitNames
{
    private static readonly Dictionary<Team, GeneratedName> Generated = new();

    internal static void Clear()
    {
        Generated.Clear();
    }

    internal static bool IsInFlight(Team team, int knownNames)
    {
        if (!Generated.TryGetValue(team, out GeneratedName generated))
            return false;

        if (generated.Frame != Time.frameCount || knownNames > generated.KnownNames)
        {
            Generated.Remove(team);
            return false;
        }

        return true;
    }

    internal static void Reserve(Team team)
    {
        Generated[team] = new GeneratedName(Time.frameCount, NamingRulesManager.GeneratedNames.TryGetValue(team, out List<string> names) ? names.Count : 0);
    }

    private readonly struct GeneratedName
    {
        internal GeneratedName(int frame, int knownNames)
        {
            Frame = frame;
            KnownNames = knownNames;
        }

        internal int Frame { get; }

        internal int KnownNames { get; }
    }
}

[HarmonyPatch(typeof(NamingRulesManager), nameof(NamingRulesManager.ServerGenerateName))]
internal class UnitNameGenerationPatch
{
    private static void Postfix(Team team)
    {
        PendingUnitNames.Reserve(team);
    }
}