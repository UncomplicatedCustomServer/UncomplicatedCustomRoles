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
using LabApi.Features.Wrappers;
using UncomplicatedCustomRoles.Manager;

namespace UncomplicatedCustomRoles.Integrations;

internal static class UCT
{
    private const string PluginName = "UncomplicatedCustomTeams";

    public static bool TryGetCustomTeamId(Player player, out uint teamId)
    {
        teamId = 0;
        try
        {
            var summonedTeam = DynamicInvoke.GetMethod(PluginName,
                    "UncomplicatedCustomTeams.API.TeamExtensions.GetCustomTeam", true)?
                .Invoke(null, [player]);

            if (summonedTeam is null)
                return false;

            var definition = DynamicInvoke.GetMethod(PluginName,
                    "UncomplicatedCustomTeams.API.Features.Runtime.SummonedTeam.Definition_get", true)?
                .Invoke(summonedTeam, null);

            if (definition is null)
                return false;

            teamId = Convert.ToUInt32(DynamicInvoke.GetMethod(PluginName,
                    "UncomplicatedCustomTeams.API.Features.Definitions.Team.Id_get", true)?
                .Invoke(definition, null));

            return true;
        }
        catch (Exception e)
        {
            LogManager.Error(e.ToString());
            return false;
        }
    }
}