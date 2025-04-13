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
using Exiled.API.Extensions;
using System.Collections.Generic;
using System.Linq;
using UncomplicatedCustomRoles.API.Features;
using UncomplicatedCustomRoles.API.Interfaces;
using UncomplicatedCustomRoles.Compatibility;

namespace UncomplicatedCustomRoles.Commands
{
    public class List : IUCRCommand
    {
        public string Name { get; } = "list";

        public string Description { get; } = "List all registered custom roles";

        public string RequiredPermission { get; } = "ucr.list";

        public bool Executor(List<string> arguments, ICommandSender sender, out string response)
        {
            List<KeyValuePair<int, ICustomRole>> list = CustomRole.CustomRoles.ToList();
            if (arguments.Count > 0 && arguments[0].Length > 1)
                list = list.Where(r => r.Value.Name.ToLower().Contains(arguments[0].ToLower())).ToList();

            response = "List of all registered CustomRoles:";

            foreach (KeyValuePair<int, ICustomRole> kvp in list)
                if (kvp.Value is not null)
                    if (CustomRole.OutdatedRoles.FirstOrDefault(r => r.CustomRole.Id == kvp.Key) is not null)
                        response += $"\n<color=#ed9609>✔</color> [{kvp.Key}] <color={kvp.Value.Role.GetColor().ToHex()}>{kvp.Value?.Name}</color>";
                    else
                        response += $"\n<color=#00ff00>✔</color> [{kvp.Key}] <color={kvp.Value.Role.GetColor().ToHex()}>{kvp.Value?.Name}</color>";

            foreach (ErrorCustomRole errorCustomRole in CustomRole.NotLoadedRoles)
                response += $"\n<color=#ff0000>❌</color> [{errorCustomRole?.Id}] <u><color={errorCustomRole?.Role.GetColor().ToHex() ?? "white"}>{errorCustomRole?.Name}</color></u>";

            response += $"\n\n<color=#00ffff>🔢</color> Showing <b>{list.Count}</b> of {CustomRole.CustomRoles.Count} CustomRoles";

            if (CustomRole.OutdatedRoles.Count > 0)
                response += $"\n<color=#ffff00>⚠️</color> There {(CustomRole.OutdatedRoles.Count > 1 ? "are" : "is")} <b>{CustomRole.OutdatedRoles.Count}</b> CustomRole{(CustomRole.OutdatedRoles.Count > 1 ? "s" : string.Empty)} that are made for a previous version of the plugin!";

            if (CustomRole.NotLoadedRoles.Count > 0)
                response += $"\n<color=#ff0000>❗</color> There {(CustomRole.NotLoadedRoles.Count > 1 ? "are" : "is")} <b>{CustomRole.NotLoadedRoles.Count}</b> CustomRole{(CustomRole.NotLoadedRoles.Count > 1 ? "s" : string.Empty)} not loaded!";

            response += "\n<size=1>OwO</size>";

            return true;
        }
    }
}