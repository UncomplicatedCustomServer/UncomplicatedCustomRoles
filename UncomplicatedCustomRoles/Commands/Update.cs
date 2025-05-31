/*
 * This file is a part of the UncomplicatedCustomRoles project.
 * 
 * Copyright (c) 2023-present FoxWorn3365 (Federico Cosma) <me@fcosma.it>
 * 
 * This file is licensed under the GNU Affero General Public License v3.0.
 * You should have received a copy of the AGPL license along with this file.
 * If not, see <https://www.gnu.org/licenses/>.
 */

using CommandSystem;
using LabApi.Loader.Features.Yaml;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UncomplicatedCustomRoles.API.Features;
using UncomplicatedCustomRoles.API.Interfaces;
using UncomplicatedCustomRoles.Compatibility;

namespace UncomplicatedCustomRoles.Commands
{
    public class Update : IUCRCommand
    {
        public string Name { get; } = "update";

        public string Description { get; } = "Update one or more outdated (but loaded) CustomRole(s)";

        public string RequiredPermission { get; } = "ucr.update";

        public bool Executor(List<string> arguments, ICommandSender sender, out string response)
        {
            response = null;
            if (arguments.Count is 0)
            {
                response = "Usage: ucr update <all | CustomRole Id>";
                return false;
            }

            if (arguments[0].ToLower() is "all")
                foreach (OutdatedCustomRole role in CustomRole.OutdatedRoles)
                    UpdateRole(role);
            else
            {
                if (int.TryParse(arguments[0], out int id))
                {
                    OutdatedCustomRole role = CustomRole.OutdatedRoles.FirstOrDefault(r => r.CustomRole.Id == id);
                    if (role is not null)
                        UpdateRole(role);
                    else
                        response = $"CustomRole {arguments[0]} not found!";
                }
                else
                    response = $"CustomRole {arguments[0]} not found!";
            }

            response ??= "Successfully updated CustomRole(s)!";
            return true;
        }

        private static void UpdateRole(OutdatedCustomRole role) => File.WriteAllText(role.Path, YamlConfigParser.Serializer.Serialize(role.CustomRole));
    }
}
