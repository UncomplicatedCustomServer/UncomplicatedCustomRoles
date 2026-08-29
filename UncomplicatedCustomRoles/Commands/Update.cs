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
using System.IO;
using System.Linq;
using CommandSystem;
using LabApi.Loader.Features.Yaml;
using UncomplicatedCustomRoles.API.Features;
using UncomplicatedCustomRoles.API.Interfaces;
using UncomplicatedCustomRoles.Compatibility;
using UncomplicatedCustomRoles.Manager;

namespace UncomplicatedCustomRoles.Commands;

public class Update : IUCRCommand
{
    public string Name { get; } = "update";

    public string Description { get; } = "Rewrite one or more loaded CustomRole config files to the latest format (outdated roles and deprecated flags)";

    public string RequiredPermission { get; } = "ucr.update";

    public bool Executor(List<string> arguments, ICommandSender sender, out string response)
    {
        response = null;
        if (arguments.Count is 0)
        {
            response = "Usage: ucr update <all | CustomRole Id>";
            return false;
        }

        int updated = 0;

        if (arguments[0].ToLower() is "all")
        {
            foreach (OutdatedCustomRole role in CustomRole.OutdatedRoles.ToList())
            {
                if (UpdateRole(role))
                    updated++;
            }

            foreach (ICustomRole role in FlagMigrator.Migrated.ToList())
            {
                if (PersistMigrated(role))
                    updated++;
            }
        }
        else if (int.TryParse(arguments[0], out int id))
        {
            OutdatedCustomRole outdated = CustomRole.OutdatedRoles.FirstOrDefault(r => r.CustomRole.Id == id);
            ICustomRole migrated = FlagMigrator.Migrated.FirstOrDefault(r => r.Id == id);

            if (outdated is not null && UpdateRole(outdated))
                updated++;
            if (migrated is not null && PersistMigrated(migrated))
                updated++;

            if (outdated is null && migrated is null)
                response = $"CustomRole {arguments[0]} is not outdated / doesn't need a config update!";
        }
        else
        {
            response = $"CustomRole {arguments[0]} not found!";
        }

        response ??= updated > 0
            ? $"Successfully updated {updated} CustomRole config file(s)!"
            : "Nothing to update.";
        return true;
    }

    private static bool UpdateRole(OutdatedCustomRole role)
    {
        if (string.IsNullOrEmpty(role.Path))
            return false;

        File.WriteAllText(role.Path, YamlConfigParser.Serializer.Serialize(role.CustomRole));
        return true;
    }

    private static bool PersistMigrated(ICustomRole role)
    {
        if (!CompatibilityManager.RolePaths.TryGetValue(role, out string path) || string.IsNullOrEmpty(path))
            return false;

        File.WriteAllText(path, YamlConfigParser.Serializer.Serialize(role));
        FlagMigrator.Migrated.Remove(role);
        return true;
    }
}