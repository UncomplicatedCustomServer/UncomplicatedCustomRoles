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
using UnityEngine;

namespace UncomplicatedCustomRoles.API.Features.CustomModules;

internal class Schematic : CustomModule
{
    public override List<string> RequiredArgs => ["name"];

    private string TargetName => TryGetStringValue("name");

    public override bool Validate(out string error)
    {
        if (string.IsNullOrWhiteSpace(TargetName))
        {
            error = "'name' must be the name of a schematic to spawn; it cannot be empty.";
            return false;
        }

        error = null;
        return true;
    }

    public override void OnAdded()
    {
        if (TargetName is null)
        {
            ThrowError("Argument 'name' not found!");
            return;
        }

        var controller = CustomRole.Player.GameObject.AddComponent<SchematicController>();
        controller.Init(TargetName);
    }

    public override void OnRemoved()
    {
        if (TargetName is null)
            return;

        Object.Destroy(CustomRole.Player.GameObject.GetComponent<SchematicController>());
    }
}