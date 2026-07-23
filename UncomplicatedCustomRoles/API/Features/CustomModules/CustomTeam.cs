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

namespace UncomplicatedCustomRoles.API.Features.CustomModules;

public class CustomTeam : CustomModule
{
    public override List<string> RequiredArgs => ["team"];
    internal string Team => TryGetStringValue("team", string.Empty);
    
    internal bool IsSameTeam(CustomTeam other)
    {
        return other is not null && !string.IsNullOrWhiteSpace(Team) &&
               string.Equals(Team, other.Team, StringComparison.OrdinalIgnoreCase);
    }
    
    internal static bool SameTeam(ReferenceHub first, ReferenceHub second)
    {
        return first is not null && second is not null && first != second &&
               SummonedCustomRole.TryGet(first, out var firstRole) && firstRole.TryGetModule(out CustomTeam firstTeam) &&
               SummonedCustomRole.TryGet(second, out var secondRole) &&
               secondRole.TryGetModule(out CustomTeam secondTeam) &&
               firstTeam.IsSameTeam(secondTeam);
    }

    public override bool Validate(out string error)
    {
        if (string.IsNullOrWhiteSpace(Team))
        {
            error = "'team' must be a non-empty team name (e.g. 'SerpentsHand').";
            return false;
        }

        error = null;
        return true;
    }
}
