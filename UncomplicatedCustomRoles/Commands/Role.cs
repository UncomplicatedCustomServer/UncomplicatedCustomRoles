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
using LabApi.Features.Wrappers;
using System.Collections.Generic;
using System.Linq;
using UncomplicatedCustomRoles.API.Features;
using UncomplicatedCustomRoles.API.Interfaces;
using UncomplicatedCustomRoles.Extensions;

namespace UncomplicatedCustomRoles.Commands
{
    public class Role : IUCRCommand
    {
        public string Name { get; } = "role";

        public string Description { get; } = "List all players with a custom role or see a player's custom role";

        public string RequiredPermission { get; } = "ucr.role";

        public bool Executor(List<string> arguments, ICommandSender sender, out string response)
        {
            if (arguments.Count > 1)
            {
                response = "Usage: ucr role (Player ID or Name)";
                return false;
            }

            if (arguments.Count == 1)
            {
                Player Player = Player.Get(arguments[0]);

                if (Player is null)
                {
                    response = $"Sorry but the player {arguments[0]} does not exists!";
                    return false;
                }

                if (Player.TryGetSummonedInstance(out SummonedCustomRole summoned))
                {
                    response = $"Player {Player.Nickname} {Player.UserId} [{Player.PlayerId}] is the custom role {summoned.Role.Name} [{summoned.Role.Id}]";
                    return true;
                }

                response = $"Player {Player.Nickname} {Player.UserId} [{Player.PlayerId}] is not a custom role!";
                return true;
            }
            else
            {
                response = "Custom roles of every player:";
                foreach (Player Player in Player.List.Where(p => !p.IsHost))
                    if (Player.TryGetSummonedInstance(out SummonedCustomRole summoned))
                        response += $"\n - Player {Player.Nickname} {Player.UserId} [{Player.PlayerId}] is the custom role {summoned.Role.Name} [{summoned.Role.Id}]";
                    else
                        response += $"\n - Player {Player.Nickname} {Player.UserId} [{Player.PlayerId}] is not a custom role!";
                return true;
            }
        }
    }
}