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

namespace UncomplicatedCustomRoles.API.Features.CustomModules
{
    public class LifeStealer : CustomModule
    {
        public override List<string> RequiredArgs => new()
        {
            "percentage"
        };

        public int Percentage => Args.TryGetValue("percentage", out string perc) && int.TryParse(perc, out int numPerc) ? numPerc : 0; // NOTE: Percentage MUST be an int so like 75 is 75% (0.75)
    }
}
