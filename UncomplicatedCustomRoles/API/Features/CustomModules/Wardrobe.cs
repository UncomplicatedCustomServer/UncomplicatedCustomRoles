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
using UncomplicatedCustomRoles.Integrations;

namespace UncomplicatedCustomRoles.API.Features.CustomModules
{
    internal class Wardrobe : CustomModule
    {
        public override List<string> RequiredArgs => new()
        {
            "name"
        };

        private string TargetName => TryGetStringValue("name");

        public override void OnAdded()
        {
            if (TargetName is null)
            {
                ThrowError("Argument 'name' not found!");
                return;
            }

            if (SLWardobe.PluginInstance is null)
                ThrowError("Plugin 'SLWardrobe' not found!\nMake sure it's installed and enabled to use that flag!");

            SLWardobe.ApplySuit(CustomRole.Player, TargetName);
        }

        public override void OnRemoved()
        {
            if (TargetName is null)
                return;
            
            SLWardobe.RemoveSuit(CustomRole.Player);
        }
    }
}