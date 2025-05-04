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
    public class CustomPermissions : CustomModule
    {
        public override List<string> RequiredArgs => new()
        {
            "permissions"
        };

        internal string[] Permissions => Args.TryGetValue("permissions", out string permissions) ? permissions.Replace(" ", string.Empty).Split(',') : new string[] { };
    }
}
