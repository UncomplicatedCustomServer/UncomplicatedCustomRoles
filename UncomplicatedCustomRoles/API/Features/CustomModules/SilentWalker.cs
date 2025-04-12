/*
 * This file is a part of the UncomplicatedCustomRoles project.
 * 
 * Copyright (c) 2023-present FoxWorn3365 (Federico Cosma) <me@fcosma.it>
 * 
 * This file is licensed under the GNU Affero General Public License v3.0.
 * You should have received a copy of the AGPL license along with this file.
 * If not, see <https://www.gnu.org/licenses/>.
 */

using Exiled.Events.EventArgs.Interfaces;
using System.Collections.Generic;

namespace UncomplicatedCustomRoles.API.Features.CustomModules
{
    public class SilentWalker : CustomModule
    {
        public override List<string> TriggerOnEvents => new()
        {
            "MakingNoise"
        };

        public override bool OnEvent(string name, IPlayerEvent ev) => false;
    }
}
