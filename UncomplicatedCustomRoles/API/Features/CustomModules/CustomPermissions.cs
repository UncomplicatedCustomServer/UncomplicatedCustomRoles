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
using LabApi.Features.Permissions;

namespace UncomplicatedCustomRoles.API.Features.CustomModules;

public class CustomPermissions : CustomModule
{
    public override List<string> RequiredArgs => ["permissions"];

    private string[] Permissions => StringArgs.TryGetValue("permissions", out var permissions)
        ? permissions.Replace(" ", string.Empty).Split([','], StringSplitOptions.RemoveEmptyEntries)
        : [];

    public override bool Validate(out string error)
    {
        if (Permissions.Length == 0)
        {
            error = "'permissions' must list at least one permission node, e.g. 'myplugin.command' or 'a.b, c.d'.";
            return false;
        }

        error = null;
        return true;
    }

    public override void OnAdded()
    {
        var player = CustomRole.Player;
        foreach (var permission in Permissions) player?.AddPermissions(permission);
        base.OnAdded();
    }

    public override void OnRemoved()
    {
        var player = CustomRole.Player;
        foreach (var permission in Permissions) player?.RemovePermissions(permission);
        base.OnRemoved();
    }
}