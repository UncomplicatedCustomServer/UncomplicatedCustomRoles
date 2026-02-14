/*
 * This file is a part of the UncomplicatedCustomRoles project.
 * 
 * Copyright (c) 2023-present FoxWorn3365 (Federico Cosma) <me@fcosma.it>
 * 
 * This file is licensed under the GNU Affero General Public License v3.0.
 * You should have received a copy of the AGPL license along with this file.
 * If not, see <https://www.gnu.org/licenses/>.
 */

using PlayerRoles;
using System;
using System.Collections.Generic;

namespace UncomplicatedCustomRoles.API.Features.CustomModules
{
    internal class ChangeAppearanceOnKill : CustomModule
    {
        public override List<string> RequiredArgs => new()
        {
            "new_appearance",
            "duration",
            "forever"
        };

        public RoleTypeId NewAppearance => Enum.TryParse(TryGetStringValue("new_appearance", "None"), out RoleTypeId role) ? role : RoleTypeId.None;

        public uint Duration => Convert.ToUInt32(TryGetValue("duration", 0));

        public bool Forever => Convert.ToBoolean(TryGetValue("forever", false));

        public bool AlreadyChanged { get; internal set; } = false;
    }
}
