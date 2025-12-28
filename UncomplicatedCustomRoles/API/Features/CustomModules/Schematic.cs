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
using UncomplicatedCustomRoles.API.Features.Controllers;

namespace UncomplicatedCustomRoles.API.Features.CustomModules
{
    internal class Schematic : CustomModule
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

            SchematicController controller = CustomRole.Player.GameObject.AddComponent<SchematicController>();
            controller.Init(TargetName);
        }

        public override void OnRemoved()
        {
            if (TargetName is null)
                return;

            UnityEngine.Object.Destroy(CustomRole.Player.GameObject.GetComponent<SchematicController>());
        }
    }
}