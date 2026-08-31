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

namespace UncomplicatedCustomRoles.API.Features.CustomModules;

public class LifeStealer : CustomModule
{
    public override List<string> RequiredArgs => ["percentage"];

    public int Percentage => StringArgs.TryGetValue("percentage", out string perc) && int.TryParse(perc, out int numPerc) ? numPerc : 0;

    public override bool Validate(out string error)
    {
        string raw = TryGetStringValue("percentage");
        if (!int.TryParse(raw, out int perc))
        {
            error = $"'percentage' must be a whole number between 0 and 100 (e.g. 75 for 75%), got '{raw}'.";
            return false;
        }

        if (perc is < 0 or > 100)
        {
            error = $"'percentage' must be between 0 and 100, got {perc}.";
            return false;
        }

        error = null;
        return true;
    }
}