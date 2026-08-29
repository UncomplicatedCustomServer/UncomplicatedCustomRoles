/*
 * This file is a part of the UncomplicatedCustomRoles project.
 *
 * Copyright (c) 2023-present FoxWorn3365 (Federico Cosma) <me@fcosma.it>
 *
 * This file is licensed under the GNU Affero General Public License v3.0.
 * You should have received a copy of the AGPL license along with this file.
 * If not, see <https://www.gnu.org/licenses/>.
 */

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using CommandSystem;
using LabApi.Features.Wrappers;
using UncomplicatedCustomRoles.API.Features;
using UncomplicatedCustomRoles.API.Interfaces;
using UncomplicatedCustomRoles.Compatibility;
using UncomplicatedCustomRoles.Extensions;
using UncomplicatedCustomRoles.Manager;

namespace UncomplicatedCustomRoles.Commands;

public class Reload : IUCRCommand
{
    public string Name { get; } = "reload";

    public string Description { get; } = "Reload every custom role loaded and search for new";

    public string RequiredPermission { get; } = "ucr.reload";

    public bool Executor(List<string> arguments, ICommandSender sender, out string response)
    {
        ConcurrentDictionary<int, ICustomRole> oldRoles = CustomRole.CustomRoles.Clone();

        CustomRole.CustomRoles = new ConcurrentDictionary<int, ICustomRole>();
        CustomRole.NotLoadedRoles.Clear();
        CustomRole.OutdatedRoles.Clear();
        FlagMigrator.Migrated.Clear();
        ImportManager.Unload();

        FileConfigs.LoadAll();
        FileConfigs.LoadAll(Server.Port.ToString());
        ImportManager.Reload();

        foreach (KeyValuePair<int, ICustomRole> oldRole in oldRoles)
        {
            if (!CustomRole.CustomRoles.ContainsKey(oldRole.Key) &&
                !CompatibilityManager.RolePaths.ContainsKey(oldRole.Value))
                CustomRole.Register(oldRole.Value);
        }

        List<int> removedRoles = oldRoles.Keys.Except(CustomRole.CustomRoles.Keys).ToList();

        foreach (int role in removedRoles)
            SummonedCustomRole.RemoveSpecificRole(role);

        int added = CustomRole.CustomRoles.Keys.Except(oldRoles.Keys).Count();

        response = $"\nSuccessfully reloaded UncomplicatedCustomRoles\n<color=#5db30c>➕</color> Added <b>{added}</b> Custom Roles\n<color=#c23636>➖</color> Removed <b>{removedRoles.Count}</b> Custom Roles\n<color=#00ffff>🔢</color> Loaded a total of <b>{CustomRole.CustomRoles.Count}</b> Custom Roles\n<color=#ffff00>⚠️</color> If you have changed some stats of the Custom Roles such as health and inventory the changes won't take place on already spawned players with these custom roles!";
        return true;
    }
}