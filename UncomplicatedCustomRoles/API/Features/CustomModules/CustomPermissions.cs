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
using LabApi.Features.Permissions;
using UncomplicatedCustomRoles.Manager;

namespace UncomplicatedCustomRoles.API.Features.CustomModules
{
    public class CustomPermissions : CustomModule
    {
        public override List<string> RequiredArgs => new()
        {
            "permissions"
        };

        private string[] Permissions => StringArgs.TryGetValue("permissions", out string permissions) ? permissions.Replace(" ", string.Empty).Split(',') : new string[] { };
        
        public override void OnAdded()
        {
            var player = CustomRole.Player;
            foreach (var permission in Permissions)
            {
                player?.AddPermissions(permission);
            }
            base.OnAdded();
        }

        public override void OnRemoved()
        {
            var player = CustomRole.Player;
            foreach (var permission in Permissions)
            {
                player?.RemovePermissions(permission);
            }
            base.OnRemoved();
        }
    }
}