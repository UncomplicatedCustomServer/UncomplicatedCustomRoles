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
using PlayerRoles;
using System;
using System.Collections.Generic;
using System.Linq;
using UncomplicatedCustomRoles.API.Features;
using UncomplicatedCustomRoles.API.Interfaces;
using UncomplicatedCustomRoles.Extensions;

namespace UncomplicatedCustomRoles.Commands
{
    public class Percentages : IUCRCommand
    {
        public string Name { get; } = "percentages";

        public string Description { get; } = "See every spawn percentage of any role";

        public string RequiredPermission { get; } = "ucr.percentages";

        public bool Executor(List<string> args, ICommandSender sender, out string response)
        {
            bool detailed = args.Any() && args[0] is "details";
            response = "Spawn percentages for each base Role:";

            foreach (RoleTypeId role in Enum.GetValues(typeof(RoleTypeId)))
            {
                IEnumerable<ICustomRole> manualRoles = CustomRole.List.Where(r => r.SpawnSettings?.CanReplaceRoles == null || !r.SpawnSettings.CanReplaceRoles.Any());
                if (manualRoles.Any())
                {
                    response += $"\n\n<color=#00ffff>ℹ️</color> <b>Roles without a linked vanilla role</b> ({manualRoles.Count()}) - spawned manually or by another plugin:";
                    foreach (ICustomRole customRole in manualRoles)
                        response += customRole.SpawnSettings is not null && customRole.SpawnSettings.SpawnChance > 0
                            ? $"\n  ∟ {customRole} - {customRole.SpawnSettings.SpawnChance}%"
                            : $"\n  ∟ {customRole}";
                }
            }
            
            response += "\n<size=1>OwO</size>"; // We want to render everything

            return true;
        }
    }
}
